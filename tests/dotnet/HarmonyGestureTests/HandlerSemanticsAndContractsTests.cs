#nullable enable
using System.Collections.Generic;
using HarmonyOS.ArkUI;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

public class HandlerSemanticsAndContractsTests
{
    [Theory]
    [InlineData(
        ArkUI_XComponentType.ARKUI_XCOMPONENT_TYPE_SURFACE,
        ArkUI_NodeType.ARKUI_NODE_XCOMPONENT)]
    [InlineData(
        ArkUI_XComponentType.ARKUI_XCOMPONENT_TYPE_TEXTURE,
        ArkUI_NodeType.ARKUI_NODE_XCOMPONENT_TEXTURE)]
    public void XComponent_Type_Maps_To_Native_Node_Type(
        ArkUI_XComponentType type,
        ArkUI_NodeType expected)
    {
        Assert.Equal(expected, XComponent.ToNodeType(type));
    }

    [Fact]
    public void Semantic_Mapper_Writes_Description_And_Hint()
    {
        var view = new Label();
        SemanticProperties.SetDescription(view, "description");
        SemanticProperties.SetHint(view, "hint");
        var node = new FakeSemanticNode();

        HarmonySemanticMapper.Apply(view, node);

        Assert.Equal("description", node.Text);
        Assert.Equal("hint", node.Description);
    }

    [Fact]
    public void Semantic_Mapper_Falls_Back_To_AutomationId()
    {
        var view = new Label { AutomationId = "automation-id" };
        var node = new FakeSemanticNode();

        HarmonySemanticMapper.Apply(view, node);

        Assert.Equal("automation-id", node.Text);
        Assert.Null(node.Description);
    }

    [Fact]
    public void Core_Handlers_Implement_Official_Interfaces()
    {
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.IButtonHandler>(new HarmonyButtonHandler());
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.ILabelHandler>(new HarmonyLabelHandler());
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.IEditorHandler>(new HarmonyEditorHandler());
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.IImageHandler>(new HarmonyImageHandler());
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.IProgressBarHandler>(new HarmonyProgressBarHandler());
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.ISliderHandler>(new HarmonySliderHandler());
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.IDatePickerHandler>(new HarmonyDatePickerHandler());
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.ITimePickerHandler>(new HarmonyTimePickerHandler());
        Assert.IsAssignableFrom<Microsoft.Maui.Handlers.IPickerHandler>(new HarmonyPickerHandler());
    }

    private sealed class FakeSemanticNode : IHarmonySemanticNode
    {
        public string? Text { get; private set; }
        public string? Description { get; private set; }

        public void SetSemanticText(string text) => Text = text;

        public void SetSemanticDescription(string description) => Description = description;
    }
}
