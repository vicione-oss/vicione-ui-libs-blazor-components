using System.Globalization;
using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Tables.Shared;

// Keyboard movement is owned by the module and never crosses to .NET, so this is the only tier that can see
// it: bUnit dispatches no key events into TypeScript, has no layout to scroll, no virtualization window to
// refill and no browser to decide `:focus-visible`. What the C# side does with a gesture once it arrives is
// covered by AdvancedTableTests.Keyboard.
//
// The focused cell is asserted through the `data-row-index` and `data-column-id` of the cell carrying the tab
// stop, plus the fact that it holds the document's focus: those are the contract movement is defined in, and
// reading the cell's geometry instead would only restate the browser's own scrolling arithmetic.
[Collection<ServerTestCollection>]
public class KeyboardNavigationTests(ServerFixture fixture)
{
    private const string KeyboardPageUrl = "/tables/advanced-table/keyboard";

    // The SimpleTable page mirrors four of this page's samples, so the same selectors address both.
    private const string SimpleTableKeyboardPageUrl = "/tables/simple-table/keyboard";

    // Multiple selection, a select column pinned left, a column pinned right, a container narrower than the
    // columns add up to, and a veto on the third row.
    private const string MultipleSampleSelector = ".keyboard-container.multi";

    // A hundred rows under full loading, so every row is rendered and the bottom edge is reachable.
    private const string SingleSampleSelector = ".keyboard-container.single";

    // Sortable and filterable columns, plus a button that hides the middle one: everything the header row
    // owns as a gesture rather than as a tab stop.
    private const string HeaderRowSampleSelector = ".keyboard-container.header-row";

    // No rows at all, so the no-data placeholder is what carries the stop.
    private const string EmptySampleSelector = ".keyboard-container.empty";

    // Row-click selection off, one column wired to CellActivated and one deliberately left unwired.
    private const string ActivationSampleSelector = ".keyboard-container.activation";

    // Nine hundred rows under virtualization, so only a slice of them is ever rendered.
    private const string VirtualizedSampleSelector = ".keyboard-container.virtualized";

    // The one cell of the table that Tab can land on. .NET renders `-1` on every cell and the module promotes
    // exactly one of them, so this selector names the position without knowing where it is. It is one compound
    // selector rather than a list, so the sample selector prefixed to it scopes the header cells as well.
    private const string FocusedCellSelector = ":is(td, th)[tabindex='0']";

    // Scroll offsets and cell edges are both fractional, so a focused cell brought fully into view still
    // measures a sliver past the edge it was scrolled to.
    private const double OverhangTolerancePixels = 1;

    // Ordinary page content either side of the single-selection sample. The tab-order tests need something to
    // arrive from and something to leave for; a table with nothing around it can only be asked which of its
    // elements carry a `tabindex`, which is what the bUnit tier already does.
    private const string BeforeTableButtonSelector = "#before-single-table";

    private const string AfterTableButtonSelector = "#after-single-table";

    private const string ToggleValueColumnButtonSelector = "#toggle-value-column";

    [Fact]
    public async Task Should_move_the_focused_cell_in_both_directions_and_stop_at_every_edge()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // The stop is seeded on the default cell, so the table is already somewhere before the first press
            await FocusTableAsync(page, SingleSampleSelector);
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 0, columnId: "Key");

            // Left at the first column is a no-op
            await page.Keyboard.PressAsync("ArrowLeft");
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 0, columnId: "Key");

            // Up from the first data row reaches the header of that column, and no further
            await page.Keyboard.PressAsync("ArrowUp");
            await ExpectFocusedHeaderAsync(page, SingleSampleSelector, columnId: "Key");

            await page.Keyboard.PressAsync("ArrowUp");
            await ExpectFocusedHeaderAsync(page, SingleSampleSelector, columnId: "Key");

            // And back down into the body it came from
            await page.Keyboard.PressAsync("ArrowDown");
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 0, columnId: "Key");

            // Right walks the columns and stops at the last one
            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 0, columnId: "Value");

            await page.Keyboard.PressAsync("ArrowRight");
            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 0, columnId: "Quantity");

            // Down to the last of the hundred rows, and no further
            for (var step = 0; step < 120; step++)
                await page.Keyboard.PressAsync("ArrowDown");

            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 99, columnId: "Quantity");
        });
    }

    [Fact]
    public async Task Should_walk_the_focused_cell_across_the_pin_groups_in_the_order_they_are_shown()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, MultipleSampleSelector);

            // The select column is pinned left and renders first, so it is the default cell. Its id is
            // generated, so it is identified by not being one of the declared ones.
            var selectColumnId = await GetFocusedColumnIdAsync(page, MultipleSampleSelector);
            Assert.DoesNotContain(selectColumnId, new[] { "Key", "Value", "Timestamp", "Quantity" });

            // Assert — the unpinned columns in the middle, then the right-pinned one at the end
            foreach (var columnId in new[] { "Key", "Value", "Timestamp", "Quantity" })
            {
                await page.Keyboard.PressAsync("ArrowRight");
                await ExpectFocusedCellAsync(page, MultipleSampleSelector, rowIndex: 0, columnId: columnId);
            }

            // And back again, ending on the left-pinned column it started from
            for (var step = 0; step < 4; step++)
                await page.Keyboard.PressAsync("ArrowLeft");

            await ExpectFocusedCellAsync(page, MultipleSampleSelector, rowIndex: 0, columnId: selectColumnId);
        });
    }

    [Fact]
    public async Task Should_move_the_focused_cell_through_a_row_the_selection_veto_covers()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // The vetoed row is the one whose checkbox is disabled
            var vetoedCheckbox = page.Locator($"{MultipleSampleSelector} tbody tr:nth-child(3) input[type='checkbox']");
            await Expect(vetoedCheckbox).ToBeDisabledAsync();

            await FocusTableAsync(page, MultipleSampleSelector);

            // Act
            for (var step = 0; step < 2; step++)
                await page.Keyboard.PressAsync("ArrowDown");

            // Assert — movement and selection are different concepts, so the veto does not block the keyboard
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 2);

            await page.Keyboard.PressAsync("ArrowDown");
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 3);
        });
    }

    [Fact]
    public async Task Should_show_the_indicator_on_a_keyboard_arrival_and_never_on_a_click()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // Act — a mouse-only user, all the way to selecting a row
            await page.Locator($"{MultipleSampleSelector} tbody tr").First.ClickAsync();
            await Expect(page.Locator($"{MultipleSampleSelector} tbody tr.selected")).ToHaveCountAsync(1);

            // Assert — the click moved the position to the cell it landed on, and painted nothing:
            // `:focus-visible` does not match a pointer press. Asserted on the rendered shadow rather than on
            // a class, because the shadow is what a user can see.
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 0);
            Assert.Equal("none", await GetFocusedCellBoxShadowAsync(page, MultipleSampleSelector));

            // Act — an arrow press is keyboard use, and the same cell paints from then on
            await page.Keyboard.PressAsync("ArrowDown");

            // Assert
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 1);
            Assert.NotEqual("none", await GetFocusedCellBoxShadowAsync(page, MultipleSampleSelector));
        });
    }

    [Fact]
    public async Task Should_paint_the_indicator_on_a_tab_arrival_before_any_movement()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // Arrange — the stop before the table, and no click anywhere inside it
            await page.Locator(BeforeTableButtonSelector).FocusAsync();

            // Act
            await page.Keyboard.PressAsync("Tab");

            // Assert — arriving by key is what `:focus-visible` matches, so the marker is there before the
            // first arrow rather than after it
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 0, columnId: "Key");
            Assert.NotEqual("none", await GetFocusedCellBoxShadowAsync(page, SingleSampleSelector));
        });
    }

    [Fact]
    public async Task Should_scroll_the_focused_cell_into_view_on_both_axes()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, MultipleSampleSelector);

            var container = page.Locator($"{MultipleSampleSelector} .inner-table-container");

            Assert.Equal(0, await GetScrollOffsetAsync(container, "scrollLeft"));
            Assert.Equal(0, await GetScrollOffsetAsync(container, "scrollTop"));

            // Act — the container is narrower than the columns add up to, so the last unpinned column is
            // off to the right until the keyboard is brought to it
            for (var step = 0; step < 3; step++)
                await page.Keyboard.PressAsync("ArrowRight");

            await ExpectFocusedCellAsync(page, MultipleSampleSelector, rowIndex: 0, columnId: "Timestamp");

            // Assert
            Assert.True(await GetScrollOffsetAsync(container, "scrollLeft") > 0,
                "the focused cell was not brought out from behind the right-hand pinned column");

            // Act — and down past the bottom of the three-hundred-pixel viewport
            for (var step = 0; step < 20; step++)
                await page.Keyboard.PressAsync("ArrowDown");

            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 20);

            // Assert
            Assert.True(await GetScrollOffsetAsync(container, "scrollTop") > 0,
                "the focused cell was not scrolled down into view");
        });
    }

    // The browser scrolls a newly focused element into view by itself now, and it knows nothing of a header
    // that sticks to the top of the scroll box — so it will happily leave a cell underneath one.
    [Fact]
    public async Task Should_keep_the_focused_cell_clear_of_the_sticky_header()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, SingleSampleSelector);

            // Arrange — well down the table, so coming back up has somewhere to scroll from
            for (var step = 0; step < 20; step++)
                await page.Keyboard.PressAsync("ArrowDown");

            await ExpectFocusedRowAsync(page, SingleSampleSelector, rowIndex: 20);

            // Act
            for (var step = 0; step < 20; step++)
                await page.Keyboard.PressAsync("ArrowUp");

            await ExpectFocusedRowAsync(page, SingleSampleSelector, rowIndex: 0);

            // Assert
            var headerBottom = await GetElementEdgeAsync(page, $"{SingleSampleSelector} thead", "bottom");
            var focusedCellTop = await GetFocusedCellEdgeAsync(page, SingleSampleSelector, "top");

            Assert.True(focusedCellTop >= headerBottom - 1,
                $"the focused cell's top edge is at {focusedCellTop}, under a header ending at {headerBottom}");
        });
    }

    // The scrollbars sit inside the container's edges, so a focused cell scrolled only until it touches one is
    // left partly under a scrollbar with room still to scroll.
    [Fact]
    public async Task Should_scroll_the_focused_cell_fully_clear_of_the_container_edges()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, MultipleSampleSelector);

            // Act — out to the last unpinned column and down past the bottom of the viewport, the two moves
            // that scroll the container as far as a focused cell can take it
            await PressRepeatedlyAsync(page, 3, "ArrowRight");
            await ExpectFocusedCellAsync(page, MultipleSampleSelector, rowIndex: 0, columnId: "Timestamp");

            await PressRepeatedlyAsync(page, 20, "ArrowDown");
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 20);

            // Assert
            var rightOverhang = await GetFocusedCellRightOverhangAsync(page, MultipleSampleSelector);
            var bottomOverhang = await GetFocusedCellBottomOverhangAsync(page, MultipleSampleSelector);

            Assert.True(rightOverhang <= OverhangTolerancePixels,
                $"{rightOverhang} pixels of the focused cell were left hidden past the right edge");

            Assert.True(bottomOverhang <= OverhangTolerancePixels,
                $"{bottomOverhang} pixels of the focused cell were left hidden past the bottom edge");
        });
    }

    [Fact]
    public async Task Should_leave_the_body_where_it_is_when_the_focused_cell_reaches_a_pinned_column()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, MultipleSampleSelector);

            var container = page.Locator($"{MultipleSampleSelector} .inner-table-container");

            // Arrange — out to the last unpinned column, which is what scrolls the body right to begin with
            await PressRepeatedlyAsync(page, 3, "ArrowRight");
            await ExpectFocusedCellAsync(page, MultipleSampleSelector, rowIndex: 0, columnId: "Timestamp");

            var scrollLeft = await GetScrollOffsetAsync(container, "scrollLeft");
            Assert.True(scrollLeft > 0, "the unpinned column was not brought out from behind the right pin");

            // Act — on to the right-pinned column, which is held against the container's edge already
            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedCellAsync(page, MultipleSampleSelector, rowIndex: 0, columnId: "Quantity");

            // Assert — a pinned cell is visible wherever the body sits, so nothing may move to reach it
            Assert.Equal(scrollLeft, await GetScrollOffsetAsync(container, "scrollLeft"));
        });
    }

    // Both repeat tests below dispatch the event rather than holding the key, so what they assert is the
    // `repeat` flag itself and not the operating system's repeat rate.
    [Fact]
    public async Task Should_leave_the_selection_alone_on_a_repeated_space()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, MultipleSampleSelector);

            var selectedRows = page.Locator($"{MultipleSampleSelector} tbody tr.selected");

            // Arrange — one real press, confirmed before the repeat goes in
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 0);
            await page.Keyboard.PressAsync("Control+ ");
            await Expect(selectedRows).ToHaveCountAsync(1);

            // Act
            await DispatchRepeatedKeyDownAsync(page, MultipleSampleSelector, key: " ", ctrlKey: true);

            // Assert — a second toggle would take the row back out again
            await Expect(selectedRows).ToHaveCountAsync(1);
        });
    }

    [Fact]
    public async Task Should_raise_no_activation_on_a_repeated_enter()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, ActivationSampleSelector);

            var lastActivated = page.Locator("p", new PageLocatorOptions { HasText = "Last activated:" });

            // Arrange — the keyboard on the column that wired the event, so only the repeat is left to reject
            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedCellAsync(page, ActivationSampleSelector, rowIndex: 0, columnId: "Value");

            // Act
            await DispatchRepeatedKeyDownAsync(page, ActivationSampleSelector, key: "Enter", ctrlKey: false);

            // Assert
            await Expect(lastActivated).ToContainTextAsync("nothing yet");
        });
    }

    [Fact]
    public async Task Should_activate_only_a_column_that_wired_the_event()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, ActivationSampleSelector);

            var lastActivated = page.Locator("p", new PageLocatorOptions { HasText = "Last activated:" });

            // Act — the first column is the one deliberately left unwired
            await ExpectFocusedCellAsync(page, ActivationSampleSelector, rowIndex: 0, columnId: "Key");
            await page.Keyboard.PressAsync("Enter");

            // Assert
            await Expect(lastActivated).ToContainTextAsync("nothing yet");

            // Act — the column beside it wired it
            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedCellAsync(page, ActivationSampleSelector, rowIndex: 0, columnId: "Value");
            await page.Keyboard.PressAsync("Enter");

            // Assert
            await Expect(lastActivated).Not.ToContainTextAsync("nothing yet");
        });
    }

    // Entering is the only route into a control inside a cell, so it is what has to work for the select
    // checkboxes, for a consumer's button, and for an editor a handler swaps in.
    [Fact]
    public async Task Should_enter_a_cell_on_enter_and_return_to_it_on_tab_and_escape()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, ActivationSampleSelector);

            var buttonSelector = $"{ActivationSampleSelector} tbody tr:first-child td[data-column-id='Value'] button";

            // Act — Enter on the cell holding the button
            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedCellAsync(page, ActivationSampleSelector, rowIndex: 0, columnId: "Value");
            await page.Keyboard.PressAsync("Enter");

            // Assert
            await Expect(page.Locator(buttonSelector)).ToBeFocusedAsync();

            // Act — the cell holds one control, so Tab off it is Tab off the last one
            await page.Keyboard.PressAsync("Tab");

            // Assert
            await ExpectFocusedCellAsync(page, ActivationSampleSelector, rowIndex: 0, columnId: "Value");

            // Act — and Escape gets back out of it just the same
            await page.Keyboard.PressAsync("Enter");
            await Expect(page.Locator(buttonSelector)).ToBeFocusedAsync();
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await ExpectFocusedCellAsync(page, ActivationSampleSelector, rowIndex: 0, columnId: "Value");
        });
    }

    // The guarantee the boundary exists for, on the one sample whose cells hold a real tab stop of their own:
    // the browser would walk straight into that button, and the table has to get focus past it instead.
    [Fact]
    public async Task Should_step_over_a_focusable_control_in_a_cell_on_the_way_out()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, ActivationSampleSelector);

            var buttonSelector = $"{ActivationSampleSelector} tbody tr:first-child td[data-column-id='Value'] button";

            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedCellAsync(page, ActivationSampleSelector, rowIndex: 0, columnId: "Value");

            // Act — the cell holds a button the consumer left in the tab sequence, and Tab must not find it
            await page.Keyboard.PressAsync("Tab");

            // Assert
            Assert.False(await IsFocusedAsync(page, buttonSelector),
                "Tab stepped into the button inside the cell instead of leaving the table");

            // And it did not come to rest on a boundary on the way, either
            Assert.DoesNotContain("tab-boundary", await GetFocusedElementDescriptionAsync(page),
                StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Should_reach_a_select_checkbox_only_by_entering_its_cell()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            var rows = page.Locator($"{MultipleSampleSelector} tbody tr");

            // Arrange — the select column is the first, so the default cell is a select cell already
            await FocusTableAsync(page, MultipleSampleSelector);

            // Act — Enter enters the cell and lands on the checkbox, and Space is then the checkbox's own
            await page.Keyboard.PressAsync("Enter");
            await Expect(rows.First.Locator("input[type='checkbox']")).ToBeFocusedAsync();

            await page.Keyboard.PressAsync(" ");

            // Assert — the checkbox toggled its row rather than the table replacing the selection
            await Expect(page.Locator($"{MultipleSampleSelector} tbody tr.selected")).ToHaveCountAsync(1);

            // And Tab is not what got there: the checkbox is out of the sequence, so Tab off the cell leaves
            await page.Keyboard.PressAsync("Escape");
            await page.Keyboard.PressAsync("Tab");

            var checkboxFocused = await IsFocusedAsync(page, $"{MultipleSampleSelector} tbody input[type='checkbox']");
            var focusedDescription = await GetFocusedElementDescriptionAsync(page);

            Assert.False(checkboxFocused, $"Tab reached {focusedDescription} instead of leaving the table");
        });
    }

    [Fact]
    public async Task Should_take_the_focused_cell_to_the_cell_a_click_landed_on()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // Arrange — the keyboard somewhere else first, so the click has a position to move
            await FocusTableAsync(page, SingleSampleSelector);
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 0, columnId: "Key");

            // Act
            await page.Locator($"{SingleSampleSelector} tbody tr").Nth(4)
                .Locator("td[data-column-id='Value']").ClickAsync();

            // Assert — the stop followed the click rather than staying where the keyboard left it, which is
            // also what stops the selection and the keyboard from ending up on two different rows
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 4, columnId: "Value");

            // And the next press carries on from there instead of jumping back to the top
            await page.Keyboard.PressAsync("ArrowDown");
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 5, columnId: "Value");
        });
    }

    [Fact]
    public async Task Should_come_back_to_the_same_cell_on_a_shift_tab_return()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // Arrange — a position three rows down, set by key
            await FocusTableAsync(page, SingleSampleSelector);

            for (var step = 0; step < 3; step++)
                await page.Keyboard.PressAsync("ArrowDown");

            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 3, columnId: "Key");

            // Act — out of the table and back in the other way
            await page.Keyboard.PressAsync("Tab");
            await Expect(page.Locator(AfterTableButtonSelector)).ToBeFocusedAsync();

            await page.Keyboard.PressAsync("Shift+Tab");

            // Assert — the same cell, not the default one
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 3, columnId: "Key");

            // And the next press steps on from it rather than starting over at the first row
            await page.Keyboard.PressAsync("ArrowDown");
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 4, columnId: "Key");
        });
    }

    [Fact]
    public async Task Should_claim_space_even_where_the_table_cannot_select()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, ActivationSampleSelector);

            var container = page.Locator($"{ActivationSampleSelector} .inner-table-container");

            await ExpectFocusedRowAsync(page, ActivationSampleSelector, rowIndex: 0);

            Assert.Equal(0, await GetScrollOffsetAsync(container, "scrollTop"));

            // Act
            await page.Keyboard.PressAsync(" ");

            // Assert — Space is the table's key whether or not anything comes of it, so the container is not
            // paged down out from under the keyboard; with the row-click channel off, nothing is selected
            await ExpectFocusedRowAsync(page, ActivationSampleSelector, rowIndex: 0);
            Assert.Equal(0, await GetScrollOffsetAsync(container, "scrollTop"));
            await Expect(page.Locator($"{ActivationSampleSelector} tbody tr.selected")).ToHaveCountAsync(0);
        });
    }

    [Fact]
    public async Task Should_go_on_moving_past_the_edge_of_the_rendered_window()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, VirtualizedSampleSelector);

            await ExpectFocusedRowAsync(page, VirtualizedSampleSelector, rowIndex: 0);

            // Act — well past the rows the first window and its overscan hold, so the moves only continue
            // because the scroll each one performs makes virtualization render the next batch
            await StepDownAsync(page, VirtualizedSampleSelector, fromRowIndex: 0, steps: 40);

            // Assert
            await ExpectFocusedRowAsync(page, VirtualizedSampleSelector, rowIndex: 40);
        });
    }

    [Fact]
    public async Task Should_bring_the_focus_back_with_a_row_a_scroll_took_out_of_the_rendered_window()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, VirtualizedSampleSelector);

            await StepDownAsync(page, VirtualizedSampleSelector, fromRowIndex: 0, steps: 5);

            var container = page.Locator($"{VirtualizedSampleSelector} .inner-table-container");

            // Act — a wheel-scroll far enough down that the row the keyboard is on is derendered. The stop
            // parks on a rendered cell so keys keep arriving; the remembered row is what the next move goes
            // back to.
            await container.EvaluateAsync("element => { element.scrollTop = 20000; }");
            await Expect(page.Locator($"{VirtualizedSampleSelector} tbody td[data-row-index='5']"))
                .ToHaveCountAsync(0);

            // Assert — the next press brings the position the user set back rather than starting over, and
            // the focus comes back with the row instead of being left on the document
            await page.Keyboard.PressAsync("ArrowDown");
            await ExpectFocusedRowAsync(page, VirtualizedSampleSelector, rowIndex: 5);

            // And stepping continues from there
            await page.Keyboard.PressAsync("ArrowDown");
            await ExpectFocusedRowAsync(page, VirtualizedSampleSelector, rowIndex: 6);
        });
    }

    [Fact]
    public async Task Should_leave_the_scroll_to_the_mouse_wheel_after_a_row_was_clicked()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            var container = page.Locator($"{VirtualizedSampleSelector} .inner-table-container");

            await page.Locator($"{VirtualizedSampleSelector} tbody td[data-row-index='2']").First.ClickAsync();

            await container.HoverAsync();

            // Act — wheel down, then back up, in steps small enough that the clicked row and every cell the
            // focus falls back to are derendered one after another
            await WheelAndExpectScrollToFollowAsync(page, container, deltaY: 200, steps: 30);
            await WheelAndExpectScrollToFollowAsync(page, container, deltaY: -200, steps: 15);
        });
    }

    [Fact]
    public async Task Should_navigate_a_simple_table_the_same_way()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            // SimpleTable is a thin wrapper over AdvancedTable, so what is checked here is that a table it
            // declares reaches the module at all — not the gesture set a second time.
            await GotoSimpleTableKeyboardPageAsync(page);

            await FocusTableAsync(page, MultipleSampleSelector);

            // Act
            await page.Keyboard.PressAsync("ArrowRight");

            // Assert
            await ExpectFocusedCellAsync(page, MultipleSampleSelector, rowIndex: 0, columnId: "Key");
        });
    }

    // The header row is part of the arrow matrix, not a set of tab stops, and the gestures it carries have to
    // keep working now that its cells are focused rather than its content.
    [Fact]
    public async Task Should_sort_from_the_header_row_without_leaving_it()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, HeaderRowSampleSelector);

            // Arrange — up out of the body and onto the first column's header
            await page.Keyboard.PressAsync("ArrowUp");
            await ExpectFocusedHeaderAsync(page, HeaderRowSampleSelector, columnId: "Key");

            // Act
            await page.Keyboard.PressAsync("Enter");

            // Assert — the sort applied, and the reshape it caused left the position where it was: a reset
            // into the body would make Shift+Enter unusable
            await Expect(page.Locator($"{HeaderRowSampleSelector} th[data-column-id='Key']"))
                .ToHaveAttributeAsync("aria-sort", "ascending");
            await ExpectFocusedHeaderAsync(page, HeaderRowSampleSelector, columnId: "Key");

            // Act — a second level on the column beside it
            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedHeaderAsync(page, HeaderRowSampleSelector, columnId: "Value");
            await page.Keyboard.PressAsync("Shift+Enter");

            // Assert — both columns sorted, so the first level survived the second being added
            await Expect(page.Locator($"{HeaderRowSampleSelector} .sorting-indicator")).ToHaveCountAsync(2);
            await ExpectFocusedHeaderAsync(page, HeaderRowSampleSelector, columnId: "Value");
        });
    }

    [Fact]
    public async Task Should_open_a_column_filter_from_its_header_on_alt_arrow_down()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, HeaderRowSampleSelector);

            await page.Keyboard.PressAsync("ArrowUp");
            await ExpectFocusedHeaderAsync(page, HeaderRowSampleSelector, columnId: "Key");

            // Act
            await page.Keyboard.PressAsync("Alt+ArrowDown");

            // Assert — the same panel the filter icon opens, reached without a pointer
            await Expect(page.Locator(".filter-editor-frame")).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Should_reset_to_the_first_cell_when_a_sort_reshapes_the_body()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, HeaderRowSampleSelector);

            // Arrange — a position well down the body, and a sort applied from somewhere that is not a header
            for (var step = 0; step < 5; step++)
                await page.Keyboard.PressAsync("ArrowDown");

            await ExpectFocusedRowAsync(page, HeaderRowSampleSelector, rowIndex: 5);

            // Act — dispatched rather than clicked: a real click focuses the header cell, and a position in the
            // header row is the one a reshape leaves where it is
            await page.Locator($"{HeaderRowSampleSelector} th[data-column-id='Key'] .header-cell")
                .DispatchEventAsync("click");
            await Expect(page.Locator($"{HeaderRowSampleSelector} th[data-column-id='Key']"))
                .ToHaveAttributeAsync("aria-sort", "ascending");

            // Assert — every row changed place, so the row the position named means nothing any more
            await ExpectFocusedCellAsync(page, HeaderRowSampleSelector, rowIndex: 0, columnId: "Key");
        });
    }

    [Fact]
    public async Task Should_move_the_focused_header_to_its_neighbor_when_its_column_is_hidden()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, HeaderRowSampleSelector);

            // Arrange — the header of the middle column, which is the one the button hides
            await page.Keyboard.PressAsync("ArrowUp");
            await page.Keyboard.PressAsync("ArrowRight");
            await ExpectFocusedHeaderAsync(page, HeaderRowSampleSelector, columnId: "Value");

            // Act
            await page.Locator(ToggleValueColumnButtonSelector).ClickAsync();
            await Expect(page.Locator($"{HeaderRowSampleSelector} th[data-column-id='Value']"))
                .ToHaveCountAsync(0);

            // Assert — the stop is on the column to the right of the hidden one, and not in the body. Only the
            // stop: clicking the button took the focus, and the table never pulls it back from the user.
            var focusedCell = page.Locator($"{HeaderRowSampleSelector} {FocusedCellSelector}");

            await Expect(focusedCell).ToHaveCountAsync(1);
            await Expect(page.Locator($"{HeaderRowSampleSelector} th[data-column-id='Quantity']"))
                .ToHaveAttributeAsync("tabindex", "0");
        });
    }

    [Fact]
    public async Task Should_hold_the_tab_stop_on_the_no_data_placeholder_of_an_empty_table()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            var placeholderCell = page.Locator($"{EmptySampleSelector} tbody tr.no-data td");

            // Act
            await placeholderCell.FocusAsync();

            // Assert — the stop is there, and the gestures that need a row do nothing on it
            await Expect(placeholderCell).ToHaveAttributeAsync("tabindex", "0");

            await page.Keyboard.PressAsync("ArrowDown");
            await page.Keyboard.PressAsync("ArrowRight");
            await page.Keyboard.PressAsync(" ");
            await page.Keyboard.PressAsync("Enter");

            await Expect(placeholderCell).ToBeFocusedAsync();

            // Act — but the header row is still reachable, so an empty table can be sorted and filtered
            await page.Keyboard.PressAsync("ArrowUp");

            // Assert
            await ExpectFocusedHeaderAsync(page, EmptySampleSelector, columnId: "Key");
        });
    }

    // The four tests below cover the tab order, which every other test in this file arrives past: they use
    // `FocusAsync`, so focus never had to travel by key to get there.
    [Fact]
    public async Task Should_reach_a_cell_by_tab_from_before_the_table()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // Arrange — the button immediately before the table, so the table's own stop is the next one
            await page.Locator(BeforeTableButtonSelector).FocusAsync();

            // Act
            await page.Keyboard.PressAsync("Tab");

            // Assert — the default cell, and not the scroll container, a row or a header
            var defaultCellSelector = $"{SingleSampleSelector} tbody tr:first-child td[data-column-id='Key']";

            var defaultCellFocused = await IsFocusedAsync(page, defaultCellSelector);
            var focusedDescription = await GetFocusedElementDescriptionAsync(page);

            Assert.True(defaultCellFocused, $"Tab reached {focusedDescription} instead of the default cell");
        });
    }

    // The gesture a consumer reports as a dead keyboard, minus the dialog it is reported from: arrive by Tab,
    // having never pressed a pointer anywhere inside the table, and press an arrow.
    [Fact]
    public async Task Should_move_the_focused_cell_on_the_first_arrow_after_a_tab_arrival()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // Arrange — the stop before the table, and no click anywhere inside it. A click would focus a
            // cell, which is the state every other movement test starts from and the one that would hide this.
            await page.Locator(BeforeTableButtonSelector).FocusAsync();

            // Act
            await page.Keyboard.PressAsync("Tab");

            // The arrival is asserted before the arrow so a failure says which half broke: Tab not landing on
            // a cell and the cell ignoring the key are different defects with the same symptom.
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 0, columnId: "Key");

            await page.Keyboard.PressAsync("ArrowDown");

            // Assert
            await ExpectFocusedCellAsync(page, SingleSampleSelector, rowIndex: 1, columnId: "Key");
        });
    }

    [Fact]
    public async Task Should_leave_the_table_by_tab_rather_than_trapping_focus()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            await page.Locator(BeforeTableButtonSelector).FocusAsync();

            // Act — the table contributes exactly one stop, so the button after it is two presses away. A
            // bound rather than an open loop: focus that never leaves is the failure being looked for, and an
            // unbounded walk would hang instead of reporting it.
            var pressesToLeave = await TabUntilFocusedAsync(page, AfterTableButtonSelector, maximumPresses: 20);

            // Assert
            Assert.True(pressesToLeave >= 0,
                "focus never reached the button after the table — the table is trapping it");
        });
    }

    [Fact]
    public async Task Should_make_the_table_body_a_single_tab_stop()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);

            // Act — walk from before the table to after it, keeping what each press focused
            var stops = await TabThroughTheTableAsync(page, maximumPresses: 20);

            // Assert — the walk really did cross the table, or the claims below are vacuous
            Assert.Contains(AfterTableButtonSelector.TrimStart('#'), stops[^1], StringComparison.Ordinal);

            // One cell, and only one: not one per row, not one per column, and not the scroll container too
            Assert.Equal(1, stops.Count(stop => stop.StartsWith("td", StringComparison.Ordinal)));
            Assert.DoesNotContain(stops, stop => stop.StartsWith("th", StringComparison.Ordinal));
            Assert.DoesNotContain(stops,
                stop => stop.Contains("inner-table-container", StringComparison.Ordinal));

            // The boundaries are crossed, never rested on: each hands focus straight on, so a press never
            // ends on one. Seeing one here would mean a user pressing Tab and watching focus vanish.
            Assert.DoesNotContain(stops, stop => stop.Contains("tab-boundary", StringComparison.Ordinal));
        });
    }

    // Both modifier tests below exist because `AdvancedTable.razor.ts` reading `event.ctrlKey` and
    // `event.shiftKey` is the one step no other test covers: the bUnit tier calls `SelectRowAsync` with the
    // two flags already set, so swapping them at the call site leaves every one of its tests green.
    [Fact]
    public async Task Should_add_to_the_selection_on_ctrl_space_and_replace_on_space()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, MultipleSampleSelector);

            var selectedRows = page.Locator($"{MultipleSampleSelector} tbody tr.selected");

            // Act — Ctrl+Space accumulates. Every step is confirmed before the next key goes in: a
            // selection crosses to .NET and back, so pressing on without waiting races the round trip and
            // the second press can be interpreted against the state the first one has not delivered yet.
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 0);

            await page.Keyboard.PressAsync("Control+ ");
            await Expect(selectedRows).ToHaveCountAsync(1);

            await page.Keyboard.PressAsync("ArrowDown");
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 1);

            await page.Keyboard.PressAsync("Control+ ");

            // Assert
            await Expect(selectedRows).ToHaveCountAsync(2);

            // Act — and a press without the modifier replaces the whole selection with the focused row
            await page.Keyboard.PressAsync(" ");

            // Assert
            await Expect(selectedRows).ToHaveCountAsync(1);

            // And it is the focused cell's row that survived, not whichever row was selected first
            await Expect(page.Locator($"{MultipleSampleSelector} tbody tr:nth-child(2).selected"))
                .ToHaveCountAsync(1);
        });
    }

    [Fact]
    public async Task Should_range_select_from_the_anchor_on_shift_space()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, MultipleSampleSelector);

            var selectedRows = page.Locator($"{MultipleSampleSelector} tbody tr.selected");

            // Arrange — a plain Space sets the anchor the range is measured from
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 0);
            await page.Keyboard.PressAsync(" ");
            await Expect(selectedRows).ToHaveCountAsync(1);

            // Act — extend to the fourth row. This sample refuses to select its third one, so the range
            // covers rows 0 to 3 and delivers three of them.
            for (var step = 0; step < 3; step++)
                await page.Keyboard.PressAsync("ArrowDown");

            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 3);
            await page.Keyboard.PressAsync("Shift+ ");

            // Assert
            await Expect(selectedRows).ToHaveCountAsync(3);

            // Act — back toward the anchor. The range shrinks rather than accumulating, which is what
            // proves the anchor survived both presses instead of following the keyboard.
            await page.Keyboard.PressAsync("ArrowUp");
            await page.Keyboard.PressAsync("ArrowUp");
            await ExpectFocusedRowAsync(page, MultipleSampleSelector, rowIndex: 1);
            await page.Keyboard.PressAsync("Shift+ ");

            // Assert
            await Expect(selectedRows).ToHaveCountAsync(2);
        });
    }

    // Focus now travels in both directions — the arrival sets the position and a move sets the focus — so the
    // two could keep handing the position back and forth. A loop would stop answering key presses long before
    // it reported anything, which is what the bound below catches.
    [Fact]
    public async Task Should_not_chase_its_own_focus_between_the_position_and_the_arrival()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoKeyboardPageAsync(page);
            await FocusTableAsync(page, SingleSampleSelector);

            await StartCountingFocusArrivalsAsync(page, SingleSampleSelector);

            // Act — the same two cells, over and over, so every press moves the focus off one and onto
            // another. Bounded by a timeout rather than by Playwright: two directions chasing each other
            // starve the renderer, and `PressAsync` has no timeout to hit.
            const int Presses = 20;

            var pressing = PressRepeatedlyAsync(page, Presses, "ArrowDown", "ArrowUp");
            var pressingFinished = await Task.WhenAny(pressing, Task.Delay(TimeSpan.FromSeconds(20)));

            Assert.True(pressingFinished == pressing,
                "the page stopped answering key presses — the focus and the position are chasing each other");

            await pressing;

            // Assert — one arrival per press is the whole budget. The bound is generous because Blazor may
            // re-render a row for its own reasons; what it catches is the unbounded case.
            var arrivalCount = await GetFocusArrivalCountAsync(page);

            Assert.True(arrivalCount <= Presses * 2,
                $"the table took focus {arrivalCount} times over {Presses} presses");
        });
    }

    private async Task GotoKeyboardPageAsync(IPage page)
        => await GotoAsync(page, KeyboardPageUrl, [
            MultipleSampleSelector, SingleSampleSelector, HeaderRowSampleSelector, EmptySampleSelector,
            ActivationSampleSelector, VirtualizedSampleSelector
        ]);

    private async Task GotoSimpleTableKeyboardPageAsync(IPage page)
        => await GotoAsync(page, SimpleTableKeyboardPageUrl, [
            MultipleSampleSelector, SingleSampleSelector, ActivationSampleSelector, VirtualizedSampleSelector
        ]);

    private async Task GotoAsync(IPage page, string url, IReadOnlyCollection<string> sampleSelectors)
    {
        await page.GotoAsync($"{fixture.ServerAddress}{url}");

        // Waits for every sample on the page, not just the one under test. Each table loads its rows through
        // a provider, and a table still settling reaches the reload path — which resets the keyboard position
        // on purpose, because after a reload it would name whichever item ended up in that row. Pressing a
        // key before the whole page has its rows races that, and the press is the thing that loses.
        foreach (var sampleSelector in sampleSelectors)
            await Expect(page.Locator($"{sampleSelector} tbody tr").First).ToBeVisibleAsync();
    }

    // Focus lands on the cell carrying the tab stop, which is what a Tab into the table reaches: the table has
    // exactly one stop of its own, and the seed puts it on the default cell.
    private static async Task FocusTableAsync(IPage page, string sampleSelector)
        => await page.Locator($"{sampleSelector} {FocusedCellSelector}").FocusAsync();

    // Steps down one row at a time. A press that outruns the rows virtualization has produced is dropped
    // rather than queued — the accepted limitation of holding the arrow down — so each step is pressed again
    // until it lands. What the retry waits for is the refill the previous step's scroll already triggered, so
    // it converges; a step that never lands is the failure this reports.
    private static async Task StepDownAsync(IPage page, string sampleSelector, int fromRowIndex, int steps)
    {
        for (var step = 1; step <= steps; step++)
        {
            var targetRowIndex = fromRowIndex + step;

            for (var attempt = 0; ; attempt++)
            {
                await page.Keyboard.PressAsync("ArrowDown");

                if (await GetFocusedRowIndexAsync(page, sampleSelector) == targetRowIndex)
                    break;

                Assert.True(attempt < 20, $"the keyboard never reached row {targetRowIndex}");

                await Task.Delay(100);
            }
        }
    }

    // Each step waits long enough for virtualization to answer the scroll, because the table only pulls the
    // scroll back once the rows it renders in response have replaced the focused one.
    private static async Task WheelAndExpectScrollToFollowAsync(IPage page, ILocator container, int deltaY,
        int steps)
    {
        var previous = await container.EvaluateAsync<double>("element => element.scrollTop");

        for (var step = 1; step <= steps; step++)
        {
            await page.Mouse.WheelAsync(0, deltaY);
            await Task.Delay(150);

            var current = await container.EvaluateAsync<double>("element => element.scrollTop");

            Assert.True(deltaY > 0 ? current > previous : current < previous,
                $"wheel step {step} of {deltaY}px moved the scroll from {previous} to {current}");

            previous = current;
        }
    }

    private static async Task ExpectFocusedCellAsync(IPage page, string sampleSelector, int rowIndex,
        string columnId)
    {
        await ExpectFocusedRowAsync(page, sampleSelector, rowIndex);

        await Expect(page.Locator($"{sampleSelector} {FocusedCellSelector}"))
            .ToHaveAttributeAsync("data-column-id", columnId);
    }

    private static async Task ExpectFocusedRowAsync(IPage page, string sampleSelector, int rowIndex)
    {
        var focusedCell = page.Locator($"{sampleSelector} {FocusedCellSelector}");

        // Exactly one cell carries the stop, whatever the table has re-rendered in the meantime, and it is
        // the browser's focus — the position and the focus being one thing is the whole design
        await Expect(focusedCell).ToHaveCountAsync(1);
        await Expect(focusedCell).ToBeFocusedAsync();
        await Expect(focusedCell).ToHaveAttributeAsync("data-row-index",
            rowIndex.ToString(CultureInfo.InvariantCulture));
    }

    private static async Task ExpectFocusedHeaderAsync(IPage page, string sampleSelector, string columnId)
    {
        var focusedCell = page.Locator($"{sampleSelector} {FocusedCellSelector}");

        await Expect(focusedCell).ToHaveCountAsync(1);
        await Expect(focusedCell).ToBeFocusedAsync();
        await Expect(page.Locator($"{sampleSelector} th[data-column-id='{columnId}']")).ToBeFocusedAsync();
    }

    // Reads the focused row without waiting for one to exist, so a press that was dropped reports "nowhere"
    // instead of holding the retry loop up for an actionability timeout.
    private static async Task<int> GetFocusedRowIndexAsync(IPage page, string sampleSelector)
        => await page.EvaluateAsync<int>(
            """
            selector => {
                const cell = document.querySelector(selector + " td[tabindex='0']");

                return cell ? Number(cell.dataset.rowIndex) : -1;
            }
            """,
            sampleSelector);

    private static async Task<string> GetFocusedColumnIdAsync(IPage page, string sampleSelector)
        => await page.Locator($"{sampleSelector} {FocusedCellSelector}")
            .EvaluateAsync<string>("element => element.dataset.columnId");

    // The indicator as a user meets it: the shadow the focused cell actually paints, which is `none` unless
    // the browser has decided the focus is worth showing.
    private static async Task<string> GetFocusedCellBoxShadowAsync(IPage page, string sampleSelector)
        => await page.Locator($"{sampleSelector} {FocusedCellSelector}")
            .EvaluateAsync<string>("element => getComputedStyle(element).boxShadow");

    private static async Task<double> GetFocusedCellEdgeAsync(IPage page, string sampleSelector, string edge)
        => await page.Locator($"{sampleSelector} {FocusedCellSelector}")
            .EvaluateAsync<double>($"element => element.getBoundingClientRect().{edge}");

    private static async Task<double> GetElementEdgeAsync(IPage page, string selector, string edge)
        => await page.Locator(selector).EvaluateAsync<double>(
            $"element => element.getBoundingClientRect().{edge}");

    // How far the focused cell reaches past the right-hand edge of the strip its column can be seen in: the
    // width the container scrolls its content through, less the columns pinned against that edge. Taken from
    // the scroll range rather than from `clientWidth`, because the headless browsers this suite runs in scroll
    // their content across a width their own `clientWidth` does not report.
    private static async Task<double> GetFocusedCellRightOverhangAsync(IPage page, string sampleSelector)
        => await page.EvaluateAsync<double>(
            """
            selector => {
                const container = document.querySelector(selector + ' .inner-table-container');
                const cell = container.querySelector("td[tabindex='0']");

                let pinnedWidth = 0;

                for (const th of container.querySelectorAll('thead th')) {
                    if (th.style.right !== '')
                        pinnedWidth += th.offsetWidth;
                }

                const scrolledTo = container.scrollLeft;
                container.scrollLeft = 1e6;
                const scrollportWidth = container.scrollWidth - container.scrollLeft;
                container.scrollLeft = scrolledTo;

                const visibleRight = container.getBoundingClientRect().left + scrollportWidth - pinnedWidth;

                return cell.getBoundingClientRect().right - visibleRight;
            }
            """,
            sampleSelector);

    private static async Task<double> GetFocusedCellBottomOverhangAsync(IPage page, string sampleSelector)
        => await page.EvaluateAsync<double>(
            """
            selector => {
                const container = document.querySelector(selector + ' .inner-table-container');
                const cell = container.querySelector("td[tabindex='0']");

                const scrolledTo = container.scrollTop;
                container.scrollTop = 1e6;
                const scrollportHeight = container.scrollHeight - container.scrollTop;
                container.scrollTop = scrolledTo;

                const visibleBottom = container.getBoundingClientRect().top + scrollportHeight;

                return cell.getBoundingClientRect().bottom - visibleBottom;
            }
            """,
            sampleSelector);

    private static async Task<int> GetScrollOffsetAsync(ILocator container, string offsetName)
    {
        var offset = await container.EvaluateAsync<double>($"element => element.{offsetName}");

        return (int)Math.Round(offset);
    }

    // Presses Tab until the element the selector names has the focus, and answers with the number of presses
    // it took — or `-1` if the bound ran out first, which is what a trapped or unreachable stop looks like.
    private static async Task<int> TabUntilFocusedAsync(IPage page, string selector, int maximumPresses)
    {
        for (var press = 1; press <= maximumPresses; press++)
        {
            await page.Keyboard.PressAsync("Tab");

            if (await IsFocusedAsync(page, selector))
                return press;
        }

        return -1;
    }

    // Walks Tab from the button before the table and describes what each press focused, so one walk can be
    // asked several questions about the sequence rather than only about its end.
    //
    // Stops at the button after the table. The page carries several samples, so a walk that runs on reaches
    // the next table's stop and the next one's again — and every question about "the table's stops" then has
    // to be asked of a sequence covering all of them.
    private static async Task<List<string>> TabThroughTheTableAsync(IPage page, int maximumPresses)
    {
        await page.Locator(BeforeTableButtonSelector).FocusAsync();

        var stops = new List<string>();

        for (var press = 0; press < maximumPresses; press++)
        {
            await page.Keyboard.PressAsync("Tab");
            stops.Add(await GetFocusedElementDescriptionAsync(page));

            if (await IsFocusedAsync(page, AfterTableButtonSelector))
                break;
        }

        return stops;
    }

    private static async Task<bool> IsFocusedAsync(IPage page, string selector)
        => await page.EvaluateAsync<bool>(
            "selector => document.activeElement === document.querySelector(selector)", selector);

    // Describes the focused element the way a failure needs to read it: what it is, plus enough of its
    // identity to tell two stops of the same kind apart.
    private static async Task<string> GetFocusedElementDescriptionAsync(IPage page)
        => await page.EvaluateAsync<string>(
            @"() => {
                  const element = document.activeElement;

                  if (!element || element === document.body)
                      return 'nothing';

                  const elementId = element.id ? '#' + element.id : '';
                  const cssClasses = typeof element.className === 'string' && element.className.trim()
                      ? '.' + element.className.trim().split(/\s+/).join('.')
                      : '';

                  return element.tagName.toLowerCase() + elementId + cssClasses;
              }");

    private static async Task PressRepeatedlyAsync(IPage page, int presses, params string[] keys)
    {
        for (var press = 0; press < presses; press++)
            await page.Keyboard.PressAsync(keys[press % keys.Length]);
    }

    // The keydown the operating system sends while a key is held down. Dispatched on the focused cell because
    // that is the only target the module's listener accepts, and built by hand because holding a key through
    // Playwright would assert the repeat rate rather than the flag.
    private static async Task DispatchRepeatedKeyDownAsync(IPage page, string sampleSelector, string key,
        bool ctrlKey)
            => await page.EvaluateAsync(
                """
                ({ selector, key, ctrlKey }) => {
                    const cell = document.querySelector(selector + " td[tabindex='0']");

                    cell.dispatchEvent(new KeyboardEvent('keydown',
                        { key, ctrlKey, repeat: true, bubbles: true, cancelable: true }));
                }
                """,
                new { selector = sampleSelector, key, ctrlKey });

    // Counts every focus arrival inside the sample's table. Installed from the test rather than read off the
    // component, so it counts what the DOM delivered and not what the module believes it asked for.
    private static async Task StartCountingFocusArrivalsAsync(IPage page, string sampleSelector)
        => await page.EvaluateAsync(
            @"selector => {
                  window.focusArrivalCount = 0;

                  document.querySelector(selector + ' .inner-table-container')
                      .addEventListener('focusin', () => { window.focusArrivalCount++; });
              }",
            sampleSelector);

    private static async Task<int> GetFocusArrivalCountAsync(IPage page)
        => await page.EvaluateAsync<int>("() => window.focusArrivalCount");
}
