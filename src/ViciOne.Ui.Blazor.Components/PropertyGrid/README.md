# PropertyGrid (experimental)

[[_TOC_]]

## Introduction

The property grid is composed of individual components and services as shown in the following diagram.

**It is in an experimental stage and must be considered subject to change or removal in future updates
until the experimental label is removed.**

```mermaid
flowchart TD

subgraph Client
  subgraph ClientServices["Services"]
    ExamplePropertyDescriptorProvider
  end
end

subgraph Package["ViciOne.Ui.Blazor.Components"]
  subgraph Components
    PropertyGrid
    PropertyGroup
    PropertyEntrySet
    PropertyEntry
    PropertyInput
  end
  
  subgraph Models
    subgraph Descriptors
        IPropertyDescriptor

        PropertyDescriptor
    end

    subgraph Items
      IPropertyGridItem
    end
  end
  
  subgraph PackageServices["Services"]
    IPropertyValueEqualityComparer
    IPropertyGridController
    IPropertyGridEvents
    IPropertyGridMessageStore
    IPropertyGridItemCollectionBuilder
    IPropertyGridState
    IPropertyDescriptorProvider
  end
end

PropertyDescriptor-. implements .- IPropertyDescriptor
PropertyDescriptor-. instantiated in .- ExamplePropertyDescriptorProvider
ExamplePropertyDescriptorProvider-. implements .->IPropertyDescriptorProvider

IPropertyDescriptorProvider-. injected into .->IPropertyGridItemCollectionBuilder
IPropertyGridItem-.->IPropertyGridItemCollectionBuilder

IPropertyGridEvents-. injected into .->IPropertyGridController
IPropertyGridMessageStore-. injected into .->IPropertyGridController
IPropertyGridItemCollectionBuilder-. injected into .->IPropertyGridController
IPropertyGridState-. injected into .->IPropertyGridController

IPropertyGridController-. passed as parameter to .->PropertyGrid
PropertyGrid-. listens to events of .->IPropertyGridState

IPropertyValueEqualityComparer-. injected into .->IPropertyGridItemCollectionBuilder

PropertyGrid-- renders -->PropertyGroup
PropertyGroup-- renders -->PropertyEntrySet
PropertyEntrySet-- renders -->PropertyEntry
PropertyEntry-- renders -->PropertyInput

classDef component stroke:green
classDef service stroke:blue

class PropertyGrid, component
class IPropertyValueEqualityComparer, service
class IPropertyGridController, service
class IPropertyGridEvents, service
class IPropertyGridMessageStore, service
class IPropertyGridItemCollectionBuilder, service
class IPropertyGridState, service
class IPropertyDescriptorProvider, service
```

## Get started

### 1. Add context class

``` csharp
public sealed class ExamplePropertyGridContext
{
    public required string Subject { get; init; } // exemplary property to describe context
}
```

> Alternatively, you can use any existing class that represents the property context.

### 2. Describe properties

``` csharp
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class ExampleInstancePropertyDescriptorProvider
    : IPropertyDescriptorProvider<ExamplePropertyGridContext, ExampleInstance>
{
    public IEnumerable<IPropertyDescriptor<ExampleInstance>> GetPropertyDescriptors(ExamplePropertyGridContext context)
    {
        yield return new PropertyDescriptor<ExampleInstance, string>()
        {
            Category = "String properties",
            Name = nameof(ExampleInstance.Description),
            DisplayName = $"{context.Subject} {nameof(ExampleInstance.Description)}",
            GetValue = (instance) => ExampleInstance.Description,
            SetValue = (instance, value) => instance.Description = value
        };

        ...
    }
}
```

> This provider describe the property `Description` of an imaginary class `ExampleInstance`.

### 3. Register minimal services

``` csharp
services.AddPropertyGrid<ExamplePropertyGridContext>()
  .WithPropertyDescriptorProvider<ExampleInstancePropertyDescriptorProvider>();
```

### 3. Render property grid

``` html
@* AnyComponent.razor *@

@using ViciOne.Ui.Blazor.Components.PropertyGrid.Services

@inject IPropertyGridController<ExamplePropertyGridContext> PropertyGridController

<PropertyGrid Controller="PropertyGridController" />
```

### 4. Set instances

``` csharp
ExampleInstance _instance1 = new();
ExampleInstance _instance2 = new();

PropertyGridController.SetInstance([_instance1, _instance2], new ExamplePropertyGridContext { Subject = "Foo" });
```

### 5. Add TooltipDisplay

- Add `TooltipDisplay` component to your application at a central place (e.g. in `MainLayout.razor`)

  ``` html
  @using ViciOne.Ui.Blazor.Components.Tooltip.Components

  <TooltipDisplay />
  ```
