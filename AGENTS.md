# AGENTS.md

## Project Context
- 这是一个给 HarmonyOS 做 .NET MAUI API 绑定与互操作的底层开源项目。项目采用分层架构：Maui/Essentials -> Bindings (Api/Nodes) -> Interop (NAPI/PInvoke/NativeAOT)。

## Layout Inspection Rules
- **MUST**: 获取应用布局信息时，**只允许**使用 `dumpLayout` 命令。
- **FORBIDDEN**: **禁止**使用任何图像查看、截图分析或视觉识别工具来获取布局信息。

### 正确做法
通过 `hdc shell` 执行以下命令，将布局信息导出为 JSON 文件：
```bash
hdc shell uitest dumpLayout -p /data/local/tmp/layout_$(date +%s).json
hdc file recv /data/local/tmp/layout_*.json ./layout.json