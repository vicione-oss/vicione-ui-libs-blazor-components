namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <inheritdoc/>
public interface IPropertyGridController<TContext> : IPropertyGridController
{
    /// <summary>
    /// Sets the instances whose properties should be rendered in the property grid
    /// </summary>
    /// <param name="instances">Instances whose properties should be rendered in the property grid</param>
    /// <param name="context">Context in which properties should be rendered in the property grid</param>
    void SetInstances(IEnumerable<object> instances, TContext context);
}
