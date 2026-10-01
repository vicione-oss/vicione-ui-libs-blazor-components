# AGENTS.md

## Code style

- Use `Arrange`, `Act` and `Assert` comments to structure tests
- Use `AwesomeAssertions` in unit tests to implement assertions using `Should()`
- Use `snake_case` naming for test methods with only the first letter of the method name capitalized

## bUnit parameter builders

The builder lambda ends its line, and every `.Add()` or `.AddChildContent()` goes on its own line,
indented one level below the builder it belongs to. This applies even when a builder sets only a
single parameter, so the indentation always mirrors the rendered component tree.

Name the builder lambda parameter `b` and the parameter selector `p`, including in nested child
component builders and in methods that receive a builder.

``` csharp
// correct
popup.Render(b => b
    .Add(p => p.Visible, true));

var renderedComponent = _testContext.Render<TabStripComponent>(b => b
    .Add(p => p.ActiveTabIndex, 0)
    .AddChildContent<Tab>(b => b
        .Add(p => p.Text, "A"))
    .AddChildContent<Tab>(b => b
        .Add(p => p.Text, "B")));

// incorrect
popup.Render(b => b.Add(p => p.Visible, true));

var renderedComponent = _testContext.Render<TabStripComponent>(b => b
    .Add(p => p.ActiveTabIndex, 0)
    .AddChildContent<Tab>(b => b.Add(p => p.Text, "A")));
```
