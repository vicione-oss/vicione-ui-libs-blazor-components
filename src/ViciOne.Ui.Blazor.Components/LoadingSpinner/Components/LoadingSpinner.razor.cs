using System.Timers;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.LoadingSpinner.Models;

namespace ViciOne.Ui.Blazor.Components.LoadingSpinner.Components;

/// <summary>
/// An overlay component to indicate a loading process. (Keeps the user busy)
/// </summary>
public sealed partial class LoadingSpinner : ComponentBase, IDisposable
{
    private TimedMessage? _currentMessage;
    private bool _currentMessageExpired;
    private bool _fadeOutCurrentMessage;
    private IEnumerable<TimedMessage> _messages = [];
    private readonly Queue<TimedMessage> _messageQueue = [];
    private int _passedTicks;
    private readonly System.Timers.Timer _messageTimer = new()
    {
        AutoReset = true,
        Enabled = false,
        Interval = 100,
    };
    private bool _visible;

    /// <summary>
    /// Gets or sets if the <see cref="LoadingSpinner"/> is visible.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; }

    /// <summary>
    /// The <see cref="TimedMessage"/>s to display when the <see cref="LoadingSpinner"/> is visible.
    /// </summary>
    [Parameter]
    public IEnumerable<TimedMessage> Messages { get; set; } = [];

    /// <summary>
    /// A render fragment to customize render logic based on the given <see cref="RenderContext"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="LoadingSpinnerMessage"/> to render <see cref="RenderContext.Message" />.
    /// </remarks>
    [Parameter]
    public RenderFragment<RenderContext>? ChildContent { get; set; }

    /// <summary>
    /// Clears the message queue.
    /// </summary>
    public void ClearMessageQueue()
    {
        StopTimer();
        _messageQueue.Clear();
    }

    private void DequeueMessage()
    {
        if (_messageQueue.TryDequeue(out var newMessage))
        {
            _currentMessage = newMessage;
            SetCurrentMessageExpired(false);
        }
        else
        {
            _currentMessage = null;
            SetCurrentMessageExpired(true);
        }
    }

    /// <summary>
    /// Displays <paramref name="message"/> instantaniousely and continues with
    /// the remaining queue afterwards.
    /// </summary>
    public void DisplayMessage(TimedMessage message)
    {
        _currentMessage = message;
        SetCurrentMessageExpired(false);

        ResetTimer();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _messageTimer.Elapsed -= OnMessageTimerElapsedAsync;
        _messageTimer.Dispose();
    }

    /// <summary>
    /// Skips the remaining duration of the current message and advances to
    /// the next one in queue.
    /// </summary>
    public void NextMessage()
    {
        DequeueMessage();

        if (!_messageTimer.Enabled)
            ResetTimer();
    }

    private async void OnMessageTimerElapsedAsync(object? s, ElapsedEventArgs e)
    {
        if (_passedTicks >= int.MaxValue / 2)
            _passedTicks = 0;

        _passedTicks++;

        if (_currentMessage is not null && !_currentMessageExpired)
        {
            switch (_currentMessage.RemainingDisplayDuration)
            {
                case < 0: // the message DOES NOT expire
                    break;

                case > 0: // the message can expire but still has time left
                    if (_passedTicks % 10 == 0)
                        _currentMessage.RemainingDisplayDuration--;
                    // set message to expired before actually expiring to play fade out animation correctly
                    else if (_currentMessage.RemainingDisplayDuration == 1 && _passedTicks % 5 == 0)
                        _fadeOutCurrentMessage = true;

                    break;

                case 0: // the message can expire and is expired
                    SetCurrentMessageExpired(true);
                    break;
            }
        }
        else
        {
            DequeueMessage();
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
        => _messageTimer.Elapsed += OnMessageTimerElapsedAsync;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        var requeueMessages = false;

        if (Messages != _messages)
        {
            _messages = Messages;
            requeueMessages = true;
        }

        if (Visible != _visible)
        {
            _visible = Visible;

            if (_visible)
            {
                requeueMessages = true;
            }
            else
            {
                _currentMessage = null;
                SetCurrentMessageExpired(true);
                ClearMessageQueue();
            }
        }

        if (_visible && requeueMessages)
        {
            foreach (var message in _messages)
            {
                message.ResetRemainingDuration();
                _messageQueue.Enqueue(message);
            }

            if (_messageQueue.Count != 0)
                ResetTimer();
            else if (_messageQueue.Count == 0)
                StopTimer();
        }
    }

    /// <summary>
    /// Queues <paramref name="message"/> for display.
    /// </summary>
    public void QueueMessage(TimedMessage message)
    {
        _messageQueue.Enqueue(message);

        if (!_messageTimer.Enabled)
            ResetTimer();
    }

    private void ResetTimer()
    {
        if (!_visible)
            return;

        _passedTicks = 0;
        _messageTimer.Start();
    }

    private void SetCurrentMessageExpired(bool expired)
    {
        _currentMessageExpired = expired;

        if (!_currentMessageExpired)
            _fadeOutCurrentMessage = false;
    }

    private void StopTimer()
    {
        _messageTimer.Stop();
        _passedTicks = 0;
    }
}
