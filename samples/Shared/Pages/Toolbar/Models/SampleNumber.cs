namespace Shared.Pages.Toolbar.Models;

public sealed class SampleNumber
{
    public int Value { get; private set; } = 1;

    public event Action? Changed;

    public void SetValue(int value)
    {
        if (value == Value)
            return;

        Value = value;

        Changed?.Invoke();
    }
}
