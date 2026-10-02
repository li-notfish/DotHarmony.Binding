namespace HarmonyOS.Maui.Permissions;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class HarmonyPermissionsAttribute : Attribute
{
    public HarmonyPermissionsAttribute(string permissions)
    {
        Permissions = permissions;
    }

    public string Permissions { get; }
}
