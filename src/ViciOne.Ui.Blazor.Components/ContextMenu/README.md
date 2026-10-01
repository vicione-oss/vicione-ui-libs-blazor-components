# ContextMenu

[TOC]

## Introduction

A context menu is a UI element that is providing some clickable actions in a pop-up element according to the given context.

The context is implemented via `IContextMenuContext` and the context menu itself is requested via `IContextMenuRequest<>`.
It can have an optional state implemented via `IContextMenuState<>`.

Folders and class names used in the following sections are examples.

## Steps

### 1. Provide context

- Add a class implementing `IContextMenuContext` to folder `Models`

  ```csharp
  public class MyContextMenuContext : IContextMenuContext
  {
      public ContextMenuItemFilter? ItemFilter { get; init; }
      public required MouseEventArgs MouseEventArgs { get; init; }

      // enrich context
      public object Caller { get; init; }
  }
  ```

### 2. Provide state (optional)

- Add a class implementing `IContextMenuState<>` to folder `Services`

  ```csharp
  public class MyContextMenuState : IContextMenuState<MyContextMenuContext>
  {
      public string HeaderText { get; private set; } = string.Empty;

      public void Update(MyContextMenuContext context)
      {
          // use enriched context to build state
          HeaderText = context.Caller.ToString();
      }
  }
  ```

### 3. Register with Dependency Injection container

- Register request, context and optionally the state class

  ```csharp
  services.AddContextMenuRequest<MyContextMenuContext>();
  services.AddContextMenuState<MyContextMenuContext, MyContextMenuState>();
  ```

### 4. Implement context menu

- Add `MyContextMenu.razor` in folder `Components`

  ```csharp
  @inherits SpecializedContextMenuWithStateBase<MyContextMenuContext, MyContextMenuState>

  <ContextMenu @ref="ContextMenuReference">
      <ContextMenuHeader Text="@State.HeaderText"></ContextMenuHeader>
      <ContextMenuItem Text="My item" OnClick="@ItemClicked" />
  </ContextMenu>
  ```

- Add code behind file `MyContextMenu.razor.cs`

  ```csharp
  public sealed partial class MyContextMenu : SpecializedContextMenuWithStateBase<MyContextMenuContext, MyContextMenuState>
  {
      private void ItemClicked()
      {
          // logic
      }
  }
  ```

  > Use base class `SpecializedContextMenu` when the context menu does not need a state.

### 5. Request context menu

- Inject `IContextMenuRequest<>` and call `SendAsync()` to request the context menu to being displayed

  ```csharp
  [Inject] private IContextMenuRequest<MyContextMenuContext> MyContextMenuRequest { get; set; } = default!;

  private async Task OnContextMenuRequestedAsync(MouseEventArgs e)
  {
      await MyContextMenuRequest.SendAsync(new() { MouseEventArgs = e, Caller = this });
  }
  ```
