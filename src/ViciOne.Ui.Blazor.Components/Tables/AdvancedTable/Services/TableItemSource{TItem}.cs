using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;

/// <summary>
/// The rows a table currently holds and the provider calls that fill them: the loaded window, the total the
/// provider reported, and the cancellation that keeps one fetch from landing on top of another.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
/// <param name="logger">Used to report a provider that fails or contradicts itself.</param>
/// <param name="componentName">Names the reporting component in the log, since a page may hold several tables.</param>
/// <remarks>
/// Knows nothing about rows, selection or rendering. It answers "which items, and how many are there in
/// total" — what the table does in response to a load is the table's own business.
/// </remarks>
internal sealed partial class TableItemSource<TItem>(ILogger logger, string componentName) : IAsyncDisposable
    where TItem : class
{
    // Separate from the per-load source below: a range fetch must survive the next ordinary load cancelling
    // its predecessor, so only disposal cuts it short.
    private readonly CancellationTokenSource _disposalCancellationTokenSource = new();

    private CancellationTokenSource? _loadCancellationTokenSource;

    private ItemRange? _window;

    /// <summary>
    /// The items of the loaded window, or every item when the table is not windowed.
    /// </summary>
    public IReadOnlyCollection<TItem> Items { get; private set; } = [];

    /// <summary>
    /// How many items the provider reported in total, which is what the footer counts and what virtualization
    /// sizes its scrollbar by. Never negative, whatever the provider says.
    /// </summary>
    public int TotalItemCount { get; private set; }

    /// <summary>
    /// Whether a provider response has arrived at all. Lets the table hold its "No data" placeholder back while
    /// the first load is still in flight, instead of flashing it before the rows land.
    /// </summary>
    public bool ItemsProvided { get; private set; }

    /// <summary>
    /// Where <see cref="Items"/> starts in the filtered, sorted set. Zero unless a window was requested.
    /// </summary>
    public int WindowStartIndex { get; private set; }

    /// <summary>
    /// Whether the last load ended in a provider failure. The table reads it to leave the selection alone: an
    /// emptied body means the fetch broke, not that the rows are gone.
    /// </summary>
    public bool LastLoadFailed { get; private set; }

    /// <summary>
    /// Requests the window starting at <paramref name="skip"/> and <paramref name="take"/> items long for the
    /// next load.
    /// </summary>
    public void SetWindow(int skip, int take)
    {
        if (_window?.Skip == skip && _window.Take == take)
            return;

        _window = new ItemRange(skip, take);
    }

    /// <summary>
    /// Fetches from <paramref name="itemsProvider"/> and replaces <see cref="Items"/> with what comes back.
    /// Returns whether the items were replaced at all — a fetch overtaken by a newer one replaces nothing.
    /// </summary>
    /// <param name="itemsProvider">The provider to fetch from.</param>
    /// <param name="filterState">The applied filter the fetch is made under.</param>
    /// <param name="sortingState">The applied sorting the fetch is made under.</param>
    /// <param name="windowed">
    /// Whether to ask for the window set by <see cref="SetWindow"/>. When <see langword="false"/>, the whole set
    /// is requested and the window start returns to zero.
    /// </param>
    /// <remarks>
    /// A provider failure is not rethrown: nothing above the table can present it, and the applied filter and
    /// sorting have already been committed and reported. The source empties out so the table shows its "No data"
    /// placeholder rather than rows the applied state never produced, and the failure is left in the log.
    /// </remarks>
    public async Task<bool> LoadAsync(IItemsProvider<TItem> itemsProvider, FilterState filterState,
        SortingState sortingState, bool windowed)
    {
        try
        {
            if (_loadCancellationTokenSource is not null)
            {
                await _loadCancellationTokenSource.CancelAsync();
                _loadCancellationTokenSource.Dispose();
            }

            _loadCancellationTokenSource = new CancellationTokenSource();

            var itemRange = windowed ? _window : null;

            var context = new ItemsProviderContext(filterState, sortingState, itemRange);

            var response = await itemsProvider.GetItemsAsync(context, _loadCancellationTokenSource.Token);

            if (response.Items is null)
                throw new InvalidOperationException($"{itemsProvider.GetType().FullName} returned a null item set.");

            if (response.TotalItemCount < response.Items.Count)
                ItemsProviderReportedTooFewItems(logger, componentName, response.TotalItemCount, response.Items.Count);

            Items = response.Items;
            TotalItemCount = Math.Max(response.TotalItemCount, 0);
            WindowStartIndex = itemRange?.Skip ?? 0;
            ItemsProvided = true;
            LastLoadFailed = false;

            return true;
        }
        catch (OperationCanceledException)
        {
            // A newer fetch took over, so leaving the previous window standing is what the caller expects.
            return false;
        }
        catch (ObjectDisposedException)
        {
            // CancellationTokenSource already disposed, nothing we can do, return gracefully
            return false;
        }
        catch (Exception exception)
        {
            ItemsProviderFailed(logger, exception, componentName);

            Items = [];
            TotalItemCount = 0;
            WindowStartIndex = 0;
            ItemsProvided = true;
            LastLoadFailed = true;

            return true;
        }
    }

    /// <summary>
    /// Fetches <paramref name="take"/> items from <paramref name="skip"/> without touching the loaded window,
    /// for a selection range reaching outside it. Returns <see langword="null"/> when the fetch could not
    /// complete.
    /// </summary>
    /// <remarks>
    /// Deliberately not the ordinary load path: that one would swap the rendered window for this range and
    /// cancel the fetch in flight.
    /// </remarks>
    public async Task<IReadOnlyList<TItem>?> FetchRangeAsync(IItemsProvider<TItem> itemsProvider,
        FilterState filterState, SortingState sortingState, int skip, int take)
    {
        try
        {
            var context = new ItemsProviderContext(filterState, sortingState, new ItemRange(skip, take));

            var response = await itemsProvider.GetItemsAsync(context, _disposalCancellationTokenSource.Token);

            return response.Items as IReadOnlyList<TItem> ?? [.. response.Items];
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully
            return null;
        }
        catch (ObjectDisposedException)
        {
            // CancellationTokenSource already disposed, nothing we can do, return gracefully
            return null;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _disposalCancellationTokenSource.CancelAsync();
        _disposalCancellationTokenSource.Dispose();

        if (_loadCancellationTokenSource is not null)
        {
            await _loadCancellationTokenSource.CancelAsync();
            _loadCancellationTokenSource.Dispose();
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "{Component} failed to load its items: the supplied " +
        "ItemsProvider threw. The table shows its No data placeholder; the applied filter and sorting stand as " +
        "reported, so the next provide retries them.")]
    private static partial void ItemsProviderFailed(ILogger logger, Exception ex, string component);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Component} received a TotalItemCount of {TotalItemCount} " +
        "below the {ItemCount} items returned with it. The body shows the items, so the footer count disagrees " +
        "with what is rendered.")]
    private static partial void ItemsProviderReportedTooFewItems(ILogger logger, string component, int totalItemCount,
        int itemCount);
}
