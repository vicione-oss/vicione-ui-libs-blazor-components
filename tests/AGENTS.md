# AGENTS.md

## Code style
- Use `Arrange`, `Act` and `Assert` comments to structure tests
- Use `AwesomeAssertions` in unit tests to implement assertions using `Should()`
- Use `snake_case` naming for test methods with only the first letter of the method name capitalized

## Line breaks in bUnit parameter builders

A **single** `.Add()` stays on the same line as the builder lambda. Break into one line per
`.Add()` only when the builder sets **two or more** parameters.

``` csharp
// correct
popup.Render(b => b.Add(p => p.Visible, true));

var popup = _testContext.Render<PopupComponent>(b => b
    .Add(p => p.CssClass, "test-popup")
    .Add(p => p.Visible, true));

// incorrect
popup.Render(b => b
    .Add(p => p.Visible, true));
```

The one exception is a single `.Add()` that would push the line past the 140 character limit
(`roslynator_max_line_length` in [`.globalconfig`](/.globalconfig)) — break it like the
multi-parameter form.
