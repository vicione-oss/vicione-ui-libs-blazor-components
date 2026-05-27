using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;

namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Extensions;

internal static class BreadcrumbItemExtensions
{
    public static List<BreadcrumbItem> GetAncestorsIncludingSelf(this BreadcrumbItem item)
    {
        var result = new List<BreadcrumbItem>();

        var ancestor = item;
        while (ancestor is not null)
        {
            result.Insert(0, ancestor);

            ancestor = ancestor.Parent;
        }

        return result;
    }
}
