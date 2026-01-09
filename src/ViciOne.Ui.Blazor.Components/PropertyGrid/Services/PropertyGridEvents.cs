using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class PropertyGridEvents<TContext> : IPropertyGridEvents<TContext>
{
    public event Action<PropertyGridPropertyChangedEventArgs>? PropertyChanged;
    public event Action<PropertyGridContextMenuVisibilityChangedEventArgs>? ContextMenuVisibilityChanged;

    public void NotifyPropertyChanged(IPropertyGridItem item)
        => PropertyChanged?.Invoke(new PropertyGridPropertyChangedEventArgs { Sender = this, Item = item });

    public void NotifyContextMenuVisibilityChanged(bool visible)
        => ContextMenuVisibilityChanged?.Invoke(new PropertyGridContextMenuVisibilityChangedEventArgs { Sender = this, Visible = visible });
}
