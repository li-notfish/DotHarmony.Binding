using HarmonyOS.Bindings.Runtime;

namespace HelloApp;

public partial class AsyncDemoPage : ContentPage
{
    public AsyncDemoPage()
    {
        InitializeComponent();
    }

    private async void OnTestPromiseString(object sender, EventArgs e)
    {
        try
        {
            // 示例：调用真实的 @ohos.* API（如 ability.getBundleName()）
            // var jsObject = NodeApi.GetGlobal();
            // var result = await NodeApi.CallMethodAsync<string>(jsObject, "getDeviceName");
            // ResultLabel.Text = $"Promise<string> result: {result}";
            
            ResultLabel.Text = "Promise<string> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
    }

    private async void OnTestPromiseDouble(object sender, EventArgs e)
    {
        try
        {
            // var result = await NodeApi.CallMethodAsync<double>(jsObject, "getScreenDensity");
            // ResultLabel.Text = $"Promise<double> result: {result}";
            
            ResultLabel.Text = "Promise<double> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
    }

    private async void OnTestPromiseBool(object sender, EventArgs e)
    {
        try
        {
            // var result = await NodeApi.CallMethodAsync<bool>(jsObject, "isInDarkMode");
            // ResultLabel.Text = $"Promise<bool> result: {result}";
            
            ResultLabel.Text = "Promise<bool> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
    }

    private async void OnTestPromiseInt(object sender, EventArgs e)
    {
        try
        {
            // var result = await NodeApi.CallMethodAsync<int>(jsObject, "getDeviceType");
            // ResultLabel.Text = $"Promise<int> result: {result}";
            
            ResultLabel.Text = "Promise<int> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
    }

    private async void OnTestPromiseUInt(object sender, EventArgs e)
    {
        try
        {
            // var result = await NodeApi.CallMethodAsync<uint>(jsObject, "getApiVersion");
            // ResultLabel.Text = $"Promise<uint> result: {result}";
            
            ResultLabel.Text = "Promise<uint> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
    }

    private async void OnTestPromiseLong(object sender, EventArgs e)
    {
        try
        {
            // var result = await NodeApi.CallMethodAsync<long>(jsObject, "getCurrentTime");
            // ResultLabel.Text = $"Promise<long> result: {result}";
            
            ResultLabel.Text = "Promise<long> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
    }

    private async void OnTestPromiseByte(object sender, EventArgs e)
    {
        try
        {
            // var result = await NodeApi.CallMethodAsync<byte>(jsObject, "getBatteryLevel");
            // ResultLabel.Text = $"Promise<byte> result: {result}";
            
            ResultLabel.Text = "Promise<byte> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
    }

    private async void OnTestPromiseVoid(object sender, EventArgs e)
    {
        try
        {
            // await NodeApi.CallMethodAsyncVoid(jsObject, "vibrateShort");
            // ResultLabel.Text = "Promise<void> completed";
            
            ResultLabel.Text = "Promise<void> - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error: {ex.Reason}";
        }
    }

    private async void OnTestPromiseReject(object sender, EventArgs e)
    {
        try
        {
            // 这是一个会 reject 的 Promise
            // var result = await NodeApi.CallMethodAsync<string>(jsObject, "willFail");
            // ResultLabel.Text = $"Should not reach here: {result}";
            
            ResultLabel.Text = "Promise Reject - 需要连接真实 @ohos.* API";
        }
        catch (ArkTSException ex)
        {
            ResultLabel.Text = $"ArkTS Error caught: {ex.Reason}\nFull message: {ex.Message}";
        }
    }
}
