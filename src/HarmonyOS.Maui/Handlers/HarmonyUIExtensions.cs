// C# 14 extension members：MAUI Color → ArkUI 节点着色的桥接。
// HarmonyOS.Bindings 不引用 Microsoft.Maui（分层约束），Color 桥接只能放 Handler 层；
// 生成的节点类不能手改，故对 Color 入参的便利重载统一收在本扩展块。
// Color.ToUint() / (byte)(x*255) 截断语义与旧手工元组一致（MAUI Graphics 源码核对）。
#nullable enable
using Microsoft.Maui.Graphics;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public static class HarmonyUIExtensions
{
    extension(ArkUINode node)
    {
        /// <summary>以 MAUI Color 设背景色（等价 c.ToUint() 直传）</summary>
        public void SetBackgroundColor(Color color) => node.SetBackgroundColor(color.ToUint());
    }

    extension(HarmonyOS.ArkUI.Text text)
    {
        public void SetFontColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            text.SetFontColor(r, g, b, a);
        }
    }

    extension(HarmonyOS.ArkUI.TextInput input)
    {
        public void SetFontColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            input.SetFontColor(r, g, b, a);
        }

        public void SetPlaceholderColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            input.SetPlaceholderColor(r, g, b, a);
        }
    }

    extension(HarmonyOS.ArkUI.TextArea area)
    {
        public void SetFontColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            area.SetFontColor(r, g, b, a);
        }

        public void SetPlaceholderColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            area.SetPlaceholderColor(r, g, b, a);
        }
    }

    extension(HarmonyOS.ArkUI.Button button)
    {
        public void SetFontColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            button.SetFontColor(r, g, b, a);
        }
    }

    extension(HarmonyOS.ArkUI.Slider slider)
    {
        public void SetSelectedColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            slider.SetSelectedColor(r, g, b, a);
        }

        public void SetTrackColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            slider.SetTrackColor(r, g, b, a);
        }

        public void SetBlockColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            slider.SetBlockColor(r, g, b, a);
        }
    }

    extension(HarmonyOS.ArkUI.Progress progress)
    {
        public void SetColor(Color color)
        {
            var (r, g, b, a) = Argb(color);
            progress.SetColor(r, g, b, a);
        }
    }

    private static (byte r, byte g, byte b, byte a) Argb(Color c)
        => ((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
}
