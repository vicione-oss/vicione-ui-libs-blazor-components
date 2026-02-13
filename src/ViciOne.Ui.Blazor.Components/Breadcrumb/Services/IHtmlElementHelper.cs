using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Services;

internal interface IHtmlElementHelper
{
    Task<IEnumerable<DomRect>> GetBoundingClientRectsAsync(IEnumerable<ElementReference> htmlElements);
}
