using System.Globalization;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Shared.Pages;

namespace Shared.Components;

public sealed partial class Navigation : ComponentBase
{
    private IReadOnlyList<NavigationItemDescriptor> _rootNavigationItemDescriptors = [];

    protected override void OnInitialized()
        => _rootNavigationItemDescriptors = BuildNavigationItemDescriptors(DiscoverHrefs());

    private static IEnumerable<string> DiscoverHrefs()
    {
        var routeAttributeType = typeof(RouteAttribute);

        return typeof(Navigation).Assembly
            .GetTypes()
            .Where(type => typeof(IComponent).IsAssignableFrom(type) && type != typeof(NotFoundPage))
            .Select(type => type
                .GetCustomAttributes(routeAttributeType, inherit: false)
                .Cast<RouteAttribute>()
                .Select(attribute => attribute.Template)
                .FirstOrDefault())
            .Where(template => !string.IsNullOrEmpty(template) && !template.Contains('{', StringComparison.Ordinal))
            .Select(template => template!);
    }

    private static List<NavigationItemDescriptor> BuildNavigationItemDescriptors(IEnumerable<string> hrefs)
    {
        var roots = new List<NavigationItemDescriptor>();

        foreach (var href in hrefs)
        {
            var segments = href.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
                continue;

            var currentLevel = roots;
            var currentHref = string.Empty;

            foreach (var segment in segments)
            {
                currentHref = $"{currentHref}/{segment}";

                var navigationItemDescriptor = currentLevel.Find(candidate => candidate.Href == currentHref);

                if (navigationItemDescriptor is null)
                {
                    navigationItemDescriptor = new NavigationItemDescriptor { Href = currentHref, Text = ToTitleCase(segment) };

                    currentLevel.Add(navigationItemDescriptor);
                }

                currentLevel = navigationItemDescriptor.Children;
            }
        }

        SortRecursive(roots);

        return roots;
    }

    private static void SortRecursive(List<NavigationItemDescriptor> navigationItemDescriptors)
    {
        navigationItemDescriptors.Sort(static (left, right) => string.CompareOrdinal(left.Text, right.Text));

        foreach (var navigationItemDescriptor in navigationItemDescriptors)
            SortRecursive(navigationItemDescriptor.Children);
    }

    private static string ToTitleCase(string segment)
    {
        var words = segment
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(CultureInfo.InvariantCulture.TextInfo.ToTitleCase);

        return string.Join(' ', words);
    }
}
