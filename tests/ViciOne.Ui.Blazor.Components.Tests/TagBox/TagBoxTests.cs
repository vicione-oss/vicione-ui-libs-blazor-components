using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using TagBoxComponent = ViciOne.Ui.Blazor.Components.TagBox.TagBox;

namespace ViciOne.Ui.Blazor.Components.Tests.TagBox;

public sealed class TagBoxTests : IDisposable
{
    private readonly BunitContext _testContext;

    public TagBoxTests()
    {
        _testContext = new BunitContext();

        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }
    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Should_render_without_parameters()
    {
        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>();

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.Find(".tag-box").Should().NotBeNull();
    }

    [Fact]
    public void Should_render_tags()
    {
        // Arrange
        string[] tags = ["Tag1", "Tag2", "Tag3"];

        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, tags));

        // Assert
        var tagLabels = renderedComponent.FindAll(".tag-label");
        tagLabels.Should().HaveCount(3);
        tagLabels[0].TextContent.Should().Be("Tag1");
        tagLabels[1].TextContent.Should().Be("Tag2");
        tagLabels[2].TextContent.Should().Be("Tag3");
    }

    [Fact]
    public void Should_render_empty_when_no_tags()
    {
        // Arrange
        string[] emptyTags = [];

        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags));

        // Assert
        renderedComponent.FindAll(".tag-element").Should().BeEmpty();
    }

    [Fact]
    public void Should_render_custom_css_class()
    {
        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.CssClass, "custom"));

        // Assert
        var tagBox = renderedComponent.Find(".tag-box");
        tagBox.GetAttribute("class").Should().Contain("custom");
    }

    [Fact]
    public void Should_render_multiple_css_classes()
    {
        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.CssClass, "test another-test"));

        // Assert
        var tagBox = renderedComponent.Find(".tag-box");
        var classes = tagBox.GetAttribute("class")!;
        classes.Should().Contain("test");
        classes.Should().Contain("another-test");
    }

    [Fact]
    public void Should_render_disabled_state()
    {
        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Enabled, false));

        // Assert
        var tagBox = renderedComponent.Find(".tag-box");
        tagBox.GetAttribute("class").Should().Contain("disabled");

        var input = renderedComponent.Find(".tag-input");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Should_render_readonly_state()
    {
        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.ReadOnly, true));

        // Assert
        var input = renderedComponent.Find(".tag-input");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void Should_disable_delete_buttons_when_disabled()
    {
        // Arrange
        string[] tags = ["Tag1"];

        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, tags)
            .Add(p => p.Enabled, false));

        // Assert
        var deleteButton = renderedComponent.Find(".delete-button");
        deleteButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Should_disable_delete_buttons_when_readonly()
    {
        // Arrange
        string[] tags = ["Tag1"];

        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, tags)
            .Add(p => p.ReadOnly, true));

        // Assert
        var deleteButton = renderedComponent.Find(".delete-button");
        deleteButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Should_render_available_tags_in_drop_down()
    {
        // Arrange
        string[] availableTags = ["Tag1", "Tag2", "Tag3"];

        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.AvailableTags, availableTags));

        // Assert
        var dropDownItems = renderedComponent.FindAll(".drop-down-item");
        dropDownItems.Should().HaveCount(3);
    }

    [Fact]
    public void Should_mark_selected_tags_in_drop_down()
    {
        // Arrange
        string[] tags = ["Tag1"];
        string[] availableTags = ["Tag1", "Tag2"];

        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, tags)
            .Add(p => p.AvailableTags, availableTags));

        // Assert
        renderedComponent.FindAll(".drop-down-item.selected").Should().HaveCount(1);
    }

    [Fact]
    public void Should_hide_selected_items_in_drop_down_when_hide_selected_items_is_true()
    {
        // Arrange
        string[] tags = ["Tag1"];
        string[] availableTags = ["Tag1", "Tag2"];

        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, tags)
            .Add(p => p.AvailableTags, availableTags)
            .Add(p => p.HideSelectedItems, true));

        // Assert
        var dropDownItems = renderedComponent.FindAll(".drop-down-item");
        dropDownItems.Should().HaveCount(1);
        dropDownItems[0].TextContent.Trim().Should().Be("Tag2");
    }

    [Fact]
    public void Should_remove_tag_on_delete_button_click()
    {
        // Arrange
        string[] tags = ["Tag1", "Tag2"];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, tags)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        // Act
        renderedComponent.FindAll(".delete-button")[0].Click();

        // Assert
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEquivalentTo(["Tag2"]);
    }

    [Fact]
    public void Should_add_tag_on_enter_key()
    {
        // Arrange
        string[] availableTags = ["Tag"];
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AvailableTags, availableTags)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("Tag");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEquivalentTo(["Tag"]);
    }

    [Fact]
    public void Should_not_add_tag_when_input_is_empty_on_enter()
    {
        // Arrange
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        updatedTags.Should().BeNull();
    }

    [Fact]
    public void Should_not_add_duplicate_tag()
    {
        // Arrange
        string[] tags = ["Existing"];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, tags)
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("Existing");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        updatedTags.Should().BeNull();
    }

    [Fact]
    public void Should_add_custom_tag_when_allow_custom_tags_is_true()
    {
        // Arrange
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("Tag");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEquivalentTo(["Tag"]);
    }

    [Fact]
    public void Should_not_add_custom_tag_when_allow_custom_tags_is_false()
    {
        // Arrange
        string[] emptyTags = [];
        string[] emptyAvailableTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AllowCustomTags, false)
            .Add(p => p.AvailableTags, emptyAvailableTags)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("Tag");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        updatedTags.Should().BeNull();
    }

    [Fact]
    public void Should_add_tag_on_focus_out()
    {
        // Arrange
        string[] availableTags = ["Tag"];
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AvailableTags, availableTags)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("Tag");
        input.TriggerEvent("onfocusout", new FocusEventArgs());

        // Assert
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEquivalentTo(["Tag"]);
    }

    [Fact]
    public void Should_not_add_tag_on_focus_out_when_input_is_whitespace()
    {
        // Arrange
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input(" ");
        input.TriggerEvent("onfocusout", new FocusEventArgs());

        // Assert
        updatedTags.Should().BeNull();
    }

    [Fact]
    public void Should_add_tag_from_drop_down_click()
    {
        // Arrange
        string[] availableTags = ["Tag"];
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AvailableTags, availableTags)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        // Act
        renderedComponent.Find(".drop-down-item").Click();

        // Assert
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEquivalentTo(["Tag"]);
    }

    [Fact]
    public void Should_remove_tag_from_drop_down_click_when_tag_is_selected()
    {
        // Arrange
        string[] tags = ["Tag"];
        string[] availableTags = ["Tag"];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, tags)
            .Add(p => p.AvailableTags, availableTags)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        // Act
        renderedComponent.Find(".drop-down-item.selected").Click();

        // Assert
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEmpty();
    }

    [Fact]
    public void Should_preserve_custom_tag_when_toggling_another_tag_from_drop_down()
    {
        // Arrange
        string[] availableTags = ["Tag1", "Tag2"];
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AvailableTags, availableTags)
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Add a custom tag that is not part of the available tags
        input.Input("Custom");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Act - select an available tag from the drop-down
        renderedComponent.FindAll(".drop-down-item")[0].Click();

        // Assert - the previously added custom tag must still be present
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEquivalentTo(["Custom", "Tag1"]);
    }

    [Fact]
    public void Should_render_enabled_by_default()
    {
        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>();

        // Assert
        var tagBox = renderedComponent.Find(".tag-box");
        tagBox.GetAttribute("class").Should().NotContain("disabled");

        var input = renderedComponent.Find(".tag-input");
        input.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Should_filter_available_tags_by_input_value()
    {
        // Arrange
        string[] availableTags = ["Tag", "MQTT", "Tag1"];

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.AvailableTags, availableTags));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("Ta");

        // Assert
        var dropDownItems = renderedComponent.FindAll(".drop-down-item");
        dropDownItems.Should().HaveCount(2);
    }

    [Fact]
    public void Should_filter_available_tags_case_insensitively()
    {
        // Arrange
        string[] availableTags = ["Tag", "MQTT"];

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.AvailableTags, availableTags));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("tag");

        // Assert
        var dropDownItems = renderedComponent.FindAll(".drop-down-item");
        dropDownItems.Should().HaveCount(1);
        dropDownItems[0].TextContent.Trim().Should().Be("Tag");
    }

    [Fact]
    public void Should_clear_input_on_escape_key()
    {
        // Arrange
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.AllowCustomTags, true));

        var input = renderedComponent.Find(".tag-input");
        input.Input("something");

        // Act
        input.KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        input.GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    public void Should_not_invoke_tags_changed_on_escape_key()
    {
        // Arrange
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");
        input.Input("something");

        // Act
        input.KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        updatedTags.Should().BeNull();
    }

    [Fact]
    public void Should_trim_whitespace_from_input_when_adding_tag()
    {
        // Arrange
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("  Tag  ");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEquivalentTo(["Tag"]);
    }

    [Fact]
    public void Should_not_add_tag_when_input_is_only_whitespace_on_enter()
    {
        // Arrange
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("   ");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        updatedTags.Should().BeNull();
    }

    [Fact]
    public void Should_show_all_available_tags_when_input_is_empty()
    {
        // Arrange
        string[] availableTags = ["Tag1", "Tag2", "Tag3"];

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.AvailableTags, availableTags));

        // Assert
        var dropDownItems = renderedComponent.FindAll(".drop-down-item");
        dropDownItems.Should().HaveCount(3);
    }

    [Fact]
    public void Should_render_empty_drop_down_when_no_available_tags()
    {
        // Act
        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.AvailableTags, []));

        // Assert
        renderedComponent.FindAll(".drop-down-item:not(.drop-down-empty)").Should().BeEmpty();
    }

    [Fact]
    public void Should_add_tag_on_focus_out_and_trim_whitespace()
    {
        // Arrange
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("  Tag  ");
        input.TriggerEvent("onfocusout", new FocusEventArgs());

        // Assert
        updatedTags.Should().NotBeNull();
        updatedTags.Should().BeEquivalentTo(["Tag"]);
    }

    [Fact]
    public void Should_not_add_tag_on_focus_out_when_input_is_empty()
    {
        // Arrange
        string[] emptyTags = [];
        IEnumerable<string>? updatedTags = null;

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.Tags, emptyTags)
            .Add(p => p.AllowCustomTags, true)
            .Add(p => p.TagsChanged, t => updatedTags = t));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.TriggerEvent("onfocusout", new FocusEventArgs());

        // Assert
        updatedTags.Should().BeNull();
    }

    [Fact]
    public void Should_show_no_drop_down_items_when_filter_matches_nothing()
    {
        // Arrange
        string[] availableTags = ["Tag1", "Tag2"];

        var renderedComponent = _testContext.Render<TagBoxComponent>(b => b
            .Add(p => p.AvailableTags, availableTags));

        var input = renderedComponent.Find(".tag-input");

        // Act
        input.Input("xyz");

        // Assert
        renderedComponent.FindAll(".drop-down-item:not(.drop-down-empty)").Should().BeEmpty();
    }
}
