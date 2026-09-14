// IEmail 鸿蒙实现：mailto: URI（to/cc/bcc/subject/body 编码进 query）+ startAbility
// （want 通道）。模拟器无邮件应用时 startAbility 返回 false 通道（ILauncher）语义——
// 直接走 startAbility 拉起，无邮件应用时系统给 toast。
#nullable enable
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyEmail : IEmail
{
    // OHOS 无标准"邮件撰写能力"查询——want 通道存在即尽力（mailto 由系统路由，无邮件应用时系统给 toast）
    public bool IsComposeSupported => true;

    public Task ComposeAsync(EmailMessage? message) =>
        NodeApi.CallMethodAsync<object?>(HarmonyPreferences.Context, "startAbility",
            NativeValue.From(new Dictionary<string, object?>
            {
                ["uri"] = BuildMailtoUri(message),
            }));

    internal static string BuildMailtoUri(EmailMessage? message)
    {
        if (message is null)
            return "mailto:";
        var query = new List<string>();
        void Add(string key, IEnumerable<string>? values)
        {
            var joined = string.Join(",", values ?? []);
            if (joined.Length > 0)
                query.Add($"{key}={Uri.EscapeDataString(joined)}");
        }
        Add("to", message.To);
        Add("cc", message.Cc);
        Add("bcc", message.Bcc);
        if (!string.IsNullOrEmpty(message.Subject))
            query.Add($"subject={Uri.EscapeDataString(message.Subject)}");
        if (!string.IsNullOrEmpty(message.Body))
            query.Add($"body={Uri.EscapeDataString(message.Body)}");

        var to = string.Join(",", message.To ?? []);
        // 收件人整体不转义（逗号分隔多个收件人）；无 query 时不带 ?
        if (query.Count == 0)
            return $"mailto:{to}";
        return to.Length == 0
            ? $"mailto:?{string.Join("&", query)}"
            : $"mailto:{to}?{string.Join("&", query)}";
    }
}
