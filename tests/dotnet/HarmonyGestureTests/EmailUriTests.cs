// HarmonyEmail 纯逻辑单测：mailto: URI 构造（不依赖真机）。
using HarmonyOS.Essentials;
using Microsoft.Maui.ApplicationModel.Communication;
using Xunit;

namespace HarmonyGestureTests;

public class EmailUriTests
{
    [Fact]
    public void Mailto_NullMessage_IsBare()
        => Assert.Equal("mailto:", HarmonyEmail.BuildMailtoUri(null));

    [Fact]
    public void Mailto_EmptyMessage_IsBare()
        => Assert.Equal("mailto:", HarmonyEmail.BuildMailtoUri(new EmailMessage()));

    [Fact]
    public void Mailto_ToSubjectBody_EncodesQuery()
    {
        var uri = HarmonyEmail.BuildMailtoUri(new EmailMessage
        {
            To = ["a@b.c"],
            Subject = "hi there",
            Body = "line1&line2",
        });
        Assert.Equal("mailto:a@b.c?to=a%40b.c&subject=hi%20there&body=line1%26line2", uri);
    }

    [Fact]
    public void Mailto_CcBcc_Included()
    {
        var uri = HarmonyEmail.BuildMailtoUri(new EmailMessage
        {
            To = ["a@b.c"],
            Cc = ["cc@b.c"],
            Bcc = ["bcc@b.c"],
        });
        Assert.Contains("cc=cc%40b.c", uri);
        Assert.Contains("bcc=bcc%40b.c", uri);
    }

    [Fact]
    public void Mailto_NoTo_UsesBareRecipient()
    {
        var uri = HarmonyEmail.BuildMailtoUri(new EmailMessage { Subject = "s" });
        Assert.StartsWith("mailto:?", uri);
        Assert.Contains("subject=s", uri);
    }

    [Fact]
    public void Mailto_MultipleTo_JoinsWithComma()
    {
        var uri = HarmonyEmail.BuildMailtoUri(new EmailMessage
        {
            To = ["a@b.c", "d@e.f"],
        });
        Assert.Contains("a@b.c,d@e.f", uri);
    }
}
