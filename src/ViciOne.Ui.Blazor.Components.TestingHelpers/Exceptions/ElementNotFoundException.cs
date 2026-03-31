namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Exceptions;

/// <summary>
/// Represents a failure to find an element in the searched target.
/// </summary>
public sealed class ElementNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementNotFoundException"/> class with a specified error message.
    /// </summary>
    public ElementNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementNotFoundException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    public ElementNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private ElementNotFoundException()
    {
    }
}
