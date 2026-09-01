using System.Globalization;
using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;

namespace Server.Tests.ComputedBackgroundColor;

[Collection<ServerTestCollection>]
public class ComputedBackgroundColorTests(ServerFixture fixture)
{
    // Allow a small per-channel deviation, because the resolved color depends on browser
    // hit-testing and sub-pixel blending which can differ by a unit between runs.
    private const int ChannelTolerance = 2;

    [Theory]
    [InlineData(0, 85, 34, 34)]     // 1 Layer
    [InlineData(1, 68, 78, 27)]     // 2 Layers
    [InlineData(2, 54, 62, 73)]     // 3 Layers
    [InlineData(3, 100, 100, 100)]  // Opaque layer in the middle
    [InlineData(4, 85, 34, 34)]     // Fully transparent layer
    [InlineData(5, 95, 32, 32)]     // Opacity multiplied with alpha
    [InlineData(6, 78, 27, 68)]     // Non-uniform background (red over blue, half-width probe)
    [InlineData(7, 85, 34, 34)]     // Split background with a half-width probe (red only)
    public async Task Should_resolve_expected_background_color_for_demo(
        int demoIndex, int expectedRed, int expectedGreen, int expectedBlue)
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/computed-background-color");

            var demo = page.Locator(".demo").Nth(demoIndex);

            // Scroll the demo into view so the IntersectionObserver triggers the resolution.
            await demo.ScrollIntoViewIfNeededAsync();

            var (red, green, blue) = await WaitForResolvedColorAsync(demo);

            var withinTolerance =
                Math.Abs(red - expectedRed) <= ChannelTolerance &&
                Math.Abs(green - expectedGreen) <= ChannelTolerance &&
                Math.Abs(blue - expectedBlue) <= ChannelTolerance;

            Assert.True(withinTolerance,
                $"Expected rgb({expectedRed}, {expectedGreen}, {expectedBlue}) (+/-{ChannelTolerance}), " +
                $"but was rgb({red}, {green}, {blue}).");
        });
    }

    [Fact]
    public async Task Should_resolve_no_color_when_probe_region_is_entirely_off_screen()
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/computed-background-color");

            // A fixed element positioned completely above the viewport has no probe points on-screen, so
            // document.elementsFromPoint can never hit it and resolve() clips it away to no color.
            var color = await ResolveColorForNewElementAsync(page,
                "position: fixed; inset: -500px 0 auto 0; height: 100px; " +
                "background-color: rgb(12, 34, 56);");

            Assert.Equal(string.Empty, color);
        });
    }

    [Fact]
    public async Task Should_resolve_color_from_visible_part_when_probe_region_is_partially_off_screen()
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/computed-background-color");

            // A fixed opaque bar straddling the top edge is half off-screen. resolve() clips the probe
            // region to the viewport and resolves from the visible part, returning the bar's own color.
            var color = await ResolveColorForNewElementAsync(page,
                "position: fixed; inset: -50px 0 auto 0; height: 100px; z-index: 99999; " +
                "background-color: rgb(12, 34, 56);");

            Assert.True(TryParseRgb(color, out var rgb), $"Expected an rgb(...) color, but was '{color}'.");

            var withinTolerance =
                Math.Abs(rgb.Red - 12) <= ChannelTolerance &&
                Math.Abs(rgb.Green - 34) <= ChannelTolerance &&
                Math.Abs(rgb.Blue - 56) <= ChannelTolerance;

            Assert.True(withinTolerance, $"Expected rgb(12, 34, 56) (+/-{ChannelTolerance}), but was '{color}'.");
        });
    }

    // Creates a throwaway element with the given inline style, asks ComputedBackgroundColor to resolve
    // its background at its own bounding rectangle, then removes it. Returns the resolved rgb(...) value,
    // or an empty string when resolve() returns no color (for example a fully off-screen probe region).
    private static async Task<string> ResolveColorForNewElementAsync(IPage page, string elementCss) => await page.EvaluateAsync<string>(
        """
        async css => {
            const element = document.createElement('div');
            element.style.cssText = css;
            document.body.appendChild(element);

            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));

            try {
                const module = await import('/_content/ViciOne.Ui.Blazor.Components/js/computed-background-color.js');
                return new module.ComputedBackgroundColor().resolve(element) ?? '';
            } finally {
                element.remove();
            }
        }
        """,
        elementCss);

    private static async Task<(int Red, int Green, int Blue)> WaitForResolvedColorAsync(ILocator demo)
    {
        // The color is applied asynchronously once the element becomes visible, so poll until the
        // '--overflow-background' custom property is set to an rgb(...) value.
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var value = await GetResolvedColorAsync(demo);

            if (TryParseRgb(value, out var color))
                return color;

            await Task.Delay(100);
        }

        var lastValue = await GetResolvedColorAsync(demo);
        throw new Xunit.Sdk.XunitException(
            $"Timed out waiting for a resolved '--overflow-background' color. Last value was '{lastValue}'.");
    }

    private static async Task<string> GetResolvedColorAsync(ILocator demo) => await demo.EvaluateAsync<string>(
        "element => getComputedStyle(element).getPropertyValue('--overflow-background').trim()");

    private static bool TryParseRgb(string value, out (int Red, int Green, int Blue) color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(value) || value == "transparent")
            return false;

        var start = value.IndexOf('(', StringComparison.Ordinal);
        var end = value.IndexOf(')', StringComparison.Ordinal);
        if (start < 0 || end <= start)
            return false;

        var channels = value[(start + 1)..end].Split(',');
        if (channels.Length < 3)
            return false;

        if (int.TryParse(channels[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var red) &&
            int.TryParse(channels[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var green) &&
            int.TryParse(channels[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var blue))
        {
            color = (red, green, blue);
            return true;
        }

        return false;
    }
}
