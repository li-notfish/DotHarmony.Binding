using HarmonyOS.Bindings.Api;
using HarmonyOS.Bindings.Runtime;

namespace ApiDemo;

public partial class AsyncDemoPage : ContentPage
{
    public AsyncDemoPage()
    {
        InitializeComponent();
    }

    private async void OnTestPromiseString(object? sender, EventArgs e)
    {
        try
        {
            // .NET 标准化后：Promise<Array<Display>> → Task<DisplayObject[]>，实例属性直接可读
            var display = await Display.GetDefaultDisplayAsync();
            ResultLabel.Text = $"Task<string> result: {display.Name}";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private async void OnTestPromiseDouble(object? sender, EventArgs e)
    {
        try
        {
            var dpi = (await Display.GetDefaultDisplayAsync()).DensityDpi;
            ResultLabel.Text = $"Task<double> result: {dpi}";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private async void OnTestPromiseBool(object? sender, EventArgs e)
    {
        try
        {
            var isFoldable = Display.IsFoldable();
            ResultLabel.Text = $"bool result: {isFoldable}";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private async void OnTestPromiseInt(object? sender, EventArgs e)
    {
        try
        {
            var apiVersion = (int)HarmonyOS.Bindings.Api.DeviceInfo.SdkApiVersion;
            ResultLabel.Text = $"int result: {apiVersion}";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private async void OnTestPromiseUInt(object? sender, EventArgs e)
    {
        try
        {
            // 原始句柄路线：经 NodeApi.CallMethodAsync<T> + globalThis（GetGlobal 已在运行时可用）
            var jsObject = NodeApi.GetGlobal();
            var result = await NodeApi.CallMethodAsync<double>(jsObject, "somePromiseApi");
            ResultLabel.Text = $"Task<double> result: {(uint)result}";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private async void OnTestPromiseLong(object? sender, EventArgs e)
    {
        try
        {
            // 需要连接真实 @ohos.* API（Task<long> 映射已就绪）
            ResultLabel.Text = "Task<long> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private async void OnTestPromiseByte(object? sender, EventArgs e)
    {
        try
        {
            // 需要连接真实 @ohos.* API（Task<byte> 映射已就绪）
            ResultLabel.Text = "Task<byte> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private async void OnTestPromiseVoid(object? sender, EventArgs e)
    {
        try
        {
            // 需要连接真实 @ohos.* API（Promise<void> → Task 已就绪）
            ResultLabel.Text = "Task (Promise<void>) - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }

    private async void OnTestPromiseReject(object? sender, EventArgs e)
    {
        try
        {
            // 调用一个会 reject 的 Promise 验证 ArkTSException 通路
            var jsObject = NodeApi.GetGlobal();
            var result = await NodeApi.CallMethodAsync<string>(jsObject, "willFail");
            ResultLabel.Text = $"Should not reach here: {result}";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error caught: {ex.Reason}\nFull message: {ex.Message}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.GetType().Name}: {ex.Message}";
        }
    }
}
