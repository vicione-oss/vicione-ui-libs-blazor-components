using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Models;

internal record BreadcrumbItemContext
{
    internal required BreadcrumbItem Instance { get; init; }
    internal ElementReference? ElementReference { get; set; }
}
