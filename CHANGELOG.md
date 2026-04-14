# Changelog

## 5.8.0 - 2026-04-14

### Package `ViciOne.Ui.Blazor.Components`

- `Button`, changed `ButtonSize.Small` from `26px` to `24px`
- `TextBox`, fixed missing update of inner input element on forced update

### Package `ViciOne.Ui.Blazor.Components.TestingHelpers`

- `Resizing`, added extension method `SetupForResizeObserver()` for `BunitJSInterop` to unify setup of JS interop in bUnit tests

## 5.7.0 - 2026-03-31

- `bunit` package, updated to version `2.6.2`
- `ComboBox` and `SpinEdit`, height set to `24px` to avoid scaling issues
- `ContextMenu`, renamed `HideAsync()` to `CloseAsync()`
- `ResizeObserver`, fixed unhandled `JSDisconnectedException` and reduced number of `JSDisconnectedExceptions` on dispose
- `Popup`
  - Added `MinimumWidth`, `PreventBrowserContextMenu`, `OnShowing`, `OnClosing` and `ShowAsync()`
  - `VisibleChanged` is now invoked after render to align with the actual visibility on screen
- `PopupHeaderBodyLayout`, added scrollbar support in body
- `Toolbar`, added hover styling / improved spacing
- `ToolbarButton`, title attribute falls back to `Text` when empty or no `Tooltip` is passed
- `ToolbarGroup`, separator is displayed only if the group is not the only child element
- `ToolbarItemBase`, added `CssClass` parameter
- Added `Dialog`

## 5.6.0 - 2026-03-18

- `.NET` packages, updated to version `10.0.5`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `4.5.0`
- `GridFilterControl`, removed `ApplyFilter`
- `SearchBox`
  - Show reset icon instead of search or configured icon when some text is entered
  - Removed `ExecuteSearch`
  - Changed icon size from 16px to 18px
- `Section`, no reset of CSS animation state when hidden
- `TextBox`
  - Input change is now detected properly when something is entered via mouse in Windows emoji panel
  - Height set to `24px` to avoid scaling issues

## 5.5.0 - 2026-03-10

- Added `Toolbar`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `4.3.0`

## 5.4.0 - 2026-03-05

- `ComboBox`, added `Name` parameter and scrollbar styling
- `TextBox`, added `Name` parameter
- `Tooltip`, made `TooltipService` internal as it is not used anywhere

## 5.3.0 - 2026-02-16

- Added `Breadcrumb` and `ResizeObserver`
- `ComboBox`, improved value handling to avoid possible `ArgumentException` (An element with the same key already exists)

## 5.2.0 - 2026-02-10

- `Grid`, added optional sorting of `PropertyColumn` and `TemplateColumn`
- Added `ViciOne.Ui.Blazor.Components.TestingHelpers`
- `PointerCapture`, added `SnapToGridPointerCaptureBehavior`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `4.2.0`
- `.NET` packages, updated to version `10.0.2`

## 5.1.1 - 2026-02-06

- `Grid`, forced re-render of `NavigationColumn` to fix missing navigation columns on use of this component

## 5.1.0 - 2026-01-30

- `ExpandableMenu`, fixed jumping on initial render / improved accessibility
- `Moveable`, `user-select: none` is now applied to moveable element on move automatically
- Updated package project URL to reflect the migration to gitlab.com

## 5.0.1 - 2026-01-09

- Changed license to MIT
- `ViciOne.Ui.Localization` package, updated to version `3.0.1`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `4.0.1`

## 5.0.0 - 2026-01-08

- Added license
- `.NET` packages, updated to version `10.0.1`
- `ViciOne.Ui.Localization` package, updated to version `3.0.0`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `4.0.1`

## 4.3.0 - 2026-01-07

- Added `Moveable`
- `Grid`, fixed multiple event handler registrations in `SelectionFooter`
- `Popup`, added `Popup`, `PopupHeaderBodyLayout` and `PopupHeaderCloseActionButton` component

## 4.2.0 - 2025-11-27

- `AdvancedErrorBoundary`, added parameter `PrepareRecover`
- `.NET` packages, updated to version `9.0.11`
- `SearchBox`, added `TextChanging` and `TextChanged` will now only be invoked when pressing Enter or clicking search icon
- `SectionRail`, added `SectionActionButton` and `SectionLayout`

## 4.1.2 - 2025-10-24

- `Grid`, centered navigation arrow

## 4.1.1 - 2025-10-17

- `PropertyGrid`, removed `sealed` modifier from class `NumericPropertyDescriptor` to allow inheritance

## 4.1.0 - 2025-10-15

- `ContextMenu`, added [`README.md`](src/ViciOne.Ui.Blazor.Components/ContextMenu/README.md)
- Removed `three-dots.scss` from package
- `.NET` packages, updated to version `9.0.10`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `3.7.0`

## 4.0.0 - 2025-10-06

- `Grid`
  - Removed `IHasUpdateLock` as marking it obsolete causes ambiguous calls
  - Removed `ItemCount` parameter from footer components
  - Renamed `Footer` parameter to `Footers`

## 3.12.0 - 2025-10-02

- `PropertyGrid` added (experimental)
- `CheckBox`
  - Added `ValueExpression` and `Valid` parameter
  - Added `FocusAsync()`
- `ComboBox`
  - Added `Enabled`, `NoOptionSelected`, `ReadOnly`, `UpdateKey ` and `ValueExpression` parameter
  - Passing a value that is not selectable results in an empty state
- `ContextMenu`, z-index changed from `1050` to `1060` to ensure context menu is displayed above DX dialogs when requested from said dialogs
- `Grid`, marked `IHasUpdateLock` as obsolete
- `SpinEdit`
  - Added `Placeholder`, `UpdateKey`
  - Added `FocusAsync()`
- `TextBox`
  - Added `EscapePressed`, `SpellCheck`, `UpdateKey` and `ValueExpression` parameter
  - Added `SelectContentAsync()`
  - Pressing `Escape` reverts input back to the value captured when gaining focus
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `3.6.1`

## 3.11.0 - 2025-09-01

- `ContentLoadingIndication` added

## 3.10.0 - 2025-08-27

- Use SASS files from `ViciOne.Ui.Design` package
- `Grid`, `LeftPane`, added scrolling

## 3.9.0 - 2025-08-06

- `Grid`, ensure table header border is displayed on scrolling

## 3.8.9 - 2025-08-05

- `Grid`, added virtual scrolling

## 3.8.8 - 2025-07-22

- `LoadingSpinner`, improved message queue handling when hidden

## 3.8.7 - 2025-07-10

- `Grid`, added footer section

## 3.8.6 - 2025-07-03

- `Switch`, unified disabled styling

## 3.8.5 - 2025-06-12

- `ContextMenu`, fixed initial rendering to correctly apply item filters

## 3.8.4 - 2025-06-11

- `ContextMenu`
  - Disposal of `ContextMenuItem` reworked
  - Rendering of `ContextMenu` improved
- `Sidebar`, adjusted resize handle to not overflow content

## 3.8.3 - 2025-06-02

- `ContextMenu`
  - Fixed position calculation return values to support null values

## 3.8.2 - 2025-05-30

- `AdvancedErrorBoundary`, custom logging

## 3.8.1 - 2025-05-28

- `ContextMenu`
  - Robustness of position calculation improved

## 3.8.0 - 2025-05-27

- `LoadingSpinner`
  - Added `ChildContent` to allow render logic customization
  - Added `LoadingSpinnerMessage` for rendering loading spinner messages in `ChildContent`

## 3.7.1 - 2025-05-16

- Updated call to `DefineStaticWebAssets` to fix broken pickup of generated js files

## 3.7.0 - 2025-05-14

- `Grid`, added `LeftPane` component

## 3.6.0 - 2025-04-28

- `LoadingSpinner`
  - Message queue improved
  - Queuing of messages, skip and direct display added

## 3.5.1 - 2025-04-11

- `TooltipDisplay`, fixed `JSDisconnectedException` on dispose

## 3.5.0 - 2025-04-01

- Added customizable tooltip components and adjacent services

## 3.4.0 - 2025-03-10

- `AdvancedErrorBoundary` added

## 3.3.2 - 2025-03-04

- `TextBox`, added `color-scheme: dark` to provide suitable auto-fill styling

## 3.3.1 - 2025-02-27

- `ViciOne.Ui.Localization` package, updated to version `2.21.0`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `3.3.0`

## 3.3.0 - 2025-02-26

- `ComboBox`
  - Added parameters `Enabled` and `ReadOnly`
  - Passing a value that is not selectable now results in an empty combo box

## 3.2.1 - 2025-02-21

- Fixed `LoadingSpinner` background colour alpha and message text alignment

## 3.2.0 - 2025-02-19

- `LoadingSpinner` added

## 3.1.1 - 2025-02-13

- `TextBox`, `ValueChanging` / `ValueChanged` is invoked only when value has actually changed

## 3.1.0 - 2025-02-05

- `SpinEdit`, fixed `ArgumentException` thrown input is cleared and `TValue` and `TLimit` is of type `Enum`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `3.2.1`

## 3.0.0 - 2024-12-03

- `.NET` packages, updated to version `9.0.0`

## 2.0.0 - 2024-11-28

- Reduced CSS bundle size by importing icon styles only once
- Restructure `ButtonSize`, `SidebarMode`, `SidebarPlacement` and `SwitchSize` to type-safe enum pattern
- `PopupRoot`
  - Added remarks to hint at the requirement for using the component only once
  - Unlocked `IServiceCollectionExtensions`

## 1.10.1 - 2024-11-11

- `TextBox`, reintroduced `ValueChanging`

## 1.10.0 - 2024-11-08

- `ButtonSizeExtensions`, added `ToMonochromeIconSize()`
- `ComboBox`
  - Added generic class `ComboBoxItem<TValue, TText>` to model combo box items
  - Hover state / clicks are now correctly recognized when hovering over drop-down arrow
- `TextBox`, `SearchBox` and `SpinEdit` retain entered value until parameter change is recognized

## 1.9.1 - 2024-11-06

- `Grid`
  - `AddGridActionButton`, added `Text` parameter
  - Reworked `IServiceCollectionExtensions` for more flexibility

## 1.9.0 - 2024-11-06

- Added CSS custom properties
  - `--vo-ui-bc-color-accent`, general accent color
  - `--vo-ui-bc-color-accent-hover`, general accent color when hovered
  - `--vo-ui-bc-check-box-color-accent-focus`, used in `CheckBox`, accent color when focused
  - `--vo-ui-bc-check-box-color-accent-hover`, used in `CheckBox`, accent color when hovered
  - `--vo-ui-bc-text-box-background-color`, used in `TextBox`, input background color
  - `--vo-ui-bc-text-box-color`, used in `TextBox`, input text color
- `Grid` added
- `Button`, shows pointer icon on hover
- `SearchBox`
  - Added icon properties
  - Moved icon closer to right border

## 1.8.1 - 2024-10-30

- `SectionRail`, left separator removed

## 1.8.0 - 2024-10-28

- `SpinEdit` added
- `Checkbox` added
- `ComboBox`, forced rerender of `select` element when change in `Items` or `Value` is detected to ensure selected option is displayed in text area
- `SearchBox` added
- `TextBox`
  - Added `Valid` flag
  - Added `Id` parameter


## 1.7.5 - 2024-10-15

- `Button`, height is now the same with or without text / adjusted layout of icon and text for improved display

## 1.7.4 - 2024-10-11

- `.NET` packages, updated to version `8.0.10`
- `ViciOne.Ui.MonochromeIcons` packages, updated to version `2.0.0`

## 1.7.3 - 2024-10-11

- `PopupCell`, does not receive clicks anymore to ensure underlying UI elements are reachable
- `ContextMenu`, adopted z-index of `1050` from `PopupCell`

## 1.7.2 - 2024-10-10

- `ComboBox`, selected item is updated when `Value` changes

## 1.7.1 - 2024-10-10

- Exclude unneccessary files from NuGet-package

## 1.7.0 - 2024-10-09

- `ComboBox` does not span whole horizontal space by default
- `ContextMenu` added

## 1.6.0 - 2024-09-27

- `Switch`, added missing border reset
- `Sidebar`, added parameter `FluidWidth`

## 1.5.0 - 2024-09-04

- `SectionRail` added
- `EditorLayout`, `Center` reduces its dimension when content in `Left`, `Top`, `Right` and `Bottom` needs more space

## 1.4.0 - 2024-08-27

- `ExpandableMenu` added

## 1.3.0 - 2024-08-26

- `Sidebar` added
- `EditorLayout` renders `Left` after `Bottom` to ensure content in `Left` can overlay content in `Top`, `Center` and `Bottom`

## 1.2.0 - 2024-08-14

- `EditorLayout` added

## 1.1.0 - 2024-07-29

- `TextBox`, added parameter `Password`

## 1.0.0 - 2024-07-24

- Initial release, starting with `Button`, `ComboBox`, `Switch` and `TextBox`
