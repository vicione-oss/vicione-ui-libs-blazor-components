using System.Diagnostics;
using Microsoft.Playwright;

namespace Server.Tests.Infrastructure;

// A CSS custom property only shows in real layout; bUnit renders markup without applying any CSS.
internal static class FooterPaddingBottom
{
    // Sets up the root and returns the bottom padding of its footer and, if nested, of the nested root's footer.
    // Setup runs on whatever root is attached at the time, as the start of interactivity replaces the prerendered
    // elements. The clone keeps the scope identifiers, so the component's CSS applies to it as to a nested component;
    // it is made before setting the property, so it does not copy the property along.
    private const string MeasureScript = """
        ([rootSelector, property, setProperty, nest]) => {
            const root = document.querySelector(`${rootSelector}:not(.nested-root)`);
            const footer = root?.querySelector('.footer');
            if (!footer)
                return null;

            let nested = root.querySelector('.nested-root');
            if (nest && !nested) {
                nested = root.cloneNode(true);
                nested.classList.add('nested-root');
                footer.appendChild(nested);
            }

            if (setProperty)
                root.style.setProperty(property, '0px');

            const paddingBottom = element => getComputedStyle(element.querySelector('.footer')).paddingBottom;
            return nest ? `${paddingBottom(root)} ${paddingBottom(nested)}` : paddingBottom(root);
        }
        """;

    private static readonly TimeSpan s_measureTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Repeats setup and measurement until the result equals <paramref name="expected"/> or a timeout passes,
    /// and returns the last result for the caller to assert.
    /// </summary>
    public static async Task<string?> MeasureAsync(
        IPage page, string rootSelector, string property, bool setProperty, bool nest, string expected)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            var actual = await page.EvaluateAsync<string?>(MeasureScript, new object[] { rootSelector, property, setProperty, nest });
            if (actual == expected || stopwatch.Elapsed > s_measureTimeout)
                return actual;

            await Task.Delay(100);
        }
    }
}
