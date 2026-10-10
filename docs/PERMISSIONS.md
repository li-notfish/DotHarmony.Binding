# HarmonyOS permission inference

The permission generator keeps `module.json5` in sync with the MAUI APIs an app
actually uses. It supports three sources:

1. **Built-in permission types**  
   `HarmonyOS.Maui.Permissions` maps MAUI's `Permissions.RequestAsync<T>()`
   permission types to HarmonyOS permissions.
   A permission type may map to multiple permissions.

2. **Library capability maps**  
   A library can ship `harmony-permissions.capabilities.json` next to its
   project file. It maps that library's MAUI methods, properties, and events
   to HarmonyOS permissions, and is automatically discovered through
   `ProjectReference` or a package's `buildTransitive` props.

3. **Explicit project declarations**  
   APIs that cannot be inferred safely are declared in the app project:

   ```xml
   <ItemGroup>
     <HarmonyPermission Include="ohos.permission.READ_PASTEBOARD" When="inuse" />
   </ItemGroup>
   ```

The final manifest is generated from the union of these sources. The build
then verifies that `permissions.json`, `module.json5`, and
`permissions.report.md` describe the same permission set.

## Capability map schema

```json
{
  "version": 3,
  "mauiMethods": [
    {
      "containingType": "Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard",
      "methodName": "GetTextAsync",
      "permission": "ohos.permission.READ_PASTEBOARD",
      "when": "inuse"
    }
  ],
  "mauiMembers": [
    {
      "containingType": "Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard",
      "memberName": "HasText",
      "memberKind": "property",
      "permission": "ohos.permission.READ_PASTEBOARD",
      "when": "inuse"
    }
  ]
}
```

`memberKind` is `property` or `event`. Method calls use `mauiMethods`.
Methods, members, and permission types may map to multiple permissions.

## Build outputs

- `obj/harmony/permissions.json` — machine-readable manifest
- `obj/harmony/permissions.report.md` — human-readable audit trail
- `obj/harmony/host/entry/src/main/module.json5` — generated HAP manifest

The report shows the source file and line for each inferred permission, plus
the origin of explicit and project-reference permissions.
