# C-Sharp Code Style

This document contains rules and recommendations specific to this repository on how to write C-Sharp code.

These rules and recommendations override or add to [`common C# code conventions`](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

## General rules

General rules are ensured by Code Analyzers running in the background based on the configuration applied in [`.globalconfig`](/.globalconfig).

## Comparing references

Prefer `==` or `!=` over [`Object.ReferenceEquals()`](https://learn.microsoft.com/en-us/dotnet/api/system.object.referenceequals), unless the latter is absolutely necessary, as a [translation](https://sharplab.io/#v2:CYLg1APgAgTAjAWAFDKgZgASwwYQwb2Q2K0ygBYMBZACgEoCiTmA3AQwCcM2MBeDAHYBTAO656AbibNi7LgCM+g0eLpSkMktJkBLAGYYaAQgBKQvUI5CBAYyEBRAI4BXNgBsAzjTYAaDPLo6bU1iRX42dWYAX2QooA==) to the former takes place anyway.

Also, do not use `EqualityComparer<T>.Default.Equals()` if not absolutely needed, because it is less efficient than `==` for reference types and does not provide any additional benefits for value types.

## Expression body in constructors, methods, and destructors

When using an expression body (`=>`) in a **constructor**, **method**, or **destructor**, the expression should be put on a new line.

``` csharp
// correct
public Foo(int value)
    => Value = value;

public int GetValue()
    => Value;

~Foo()
    => Cleanup();

// incorrect
public Foo(int value) => Value = value;
public int GetValue() => Value;
~Foo() => Cleanup();
```

## Is it allowed to initialize a non-nullable field with `default!`?

Using `default!` should be **avoided whenever possible** because it assigns `null` to a member that should never be null.

These assignments could result in possible access violations later on because it disables [Null-state analysis](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references#null-state-analysis) making code compile that should never compile.

``` csharp
public class Foo;

public class Bar
{
    private Foo _this = default!; // initializes non-nullable field with null, not recommended

    private Foo? _that; // prefer nullable type
}
```

## Conditional expressions

Prefer early returns or conventional `if` statements over the conditional (ternary) `? :` operator when the branches involve complex expressions, such as `await` calls, method invocations with multiple arguments, or logic that is hard to read on a single line.

Never use a conditional (ternary) `? :` operator directly as the whole body of a `return` (or an expression-bodied member acting as a return). Such expressions are hard to debug because you cannot set a breakpoint on an individual branch. Use `if` statements with early returns instead.

``` csharp
// avoid: ternary used directly as the return value
public static Type MakeNonNullableType(this Type type)
    => type.IsNullableValueType() ? new NullableConverter(type).UnderlyingType : type;

// prefer: early return
public static Type MakeNonNullableType(this Type type)
{
    if (type.IsNullableValueType())
        return new NullableConverter(type).UnderlyingType;

    return type;
}
```

Reserve the conditional operator for simple, short value selections assigned to a local variable, where readability is not compromised.

``` csharp
// avoid: complex ternary
var boundingClientRects = elementReferences is null
    ? null
    : await HtmlElementHelper.GetBoundingClientRectsAsync(elementReferences);

// prefer: early return
if (elementReferences is null)
    return;

var boundingClientRects = await HtmlElementHelper.GetBoundingClientRectsAsync(elementReferences);

// still fine: simple value selection
var label = isEnabled ? "On" : "Off";
```

### Test methods

- Test method names should use [snake case](https://en.wikipedia.org/wiki/Snake_case) pattern.

  > The Test Explorer replaces underlines with spaces because of [`our configuration`](https://gitlab.i40.ifm-datalink.net/acx/vo-ui/vo-blazor-components/-/blob/master/xunit.runner.json#L4) of [`methodDisplayOptions`](https://xunit.net/docs/runsettings#MethodDisplayOptions). As a result, test names like `Should_return_access_level_requirement` are displayed as formulated sentences like `Should return access level requirement`.
