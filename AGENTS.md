# AGENTS.md

## Project Context
- 这是一个给 HarmonyOS 做 .NET MAUI API 绑定与互操作的底层开源项目。项目采用分层架构：Maui/Essentials -> Bindings (Api/Nodes) -> Interop (NAPI/PInvoke/NativeAOT)。

## Deployment Rules
- `samples/HarmonyHost` 是共享宿主核心，不是可部署的示例应用。
- 部署 `samples/dotnet/*` 时，直接运行对应 sample 的 `-t:HarmonyRun` 目标；宿主会自动 stage 到该 sample 的 `obj/harmony/host`，无需另行部署或强调 `HarmonyHost`。
- 如果工具必须传 `HOST_DIR`，指向当前 sample 的 `obj/harmony/host`，不要指向 `samples/HarmonyHost`。

## Layout Inspection Rules
- **MUST**: 获取界面结构时，优先使用 `uitest dumpLayout`；它是布局事实来源。
- **FORBIDDEN**: 不要用截图、图像识别或肉眼观察推断布局。
- 只有用户明确要求视觉效果或截图验证时，才使用截图工具，且不得用截图替代布局 JSON。

## Temporary Files
- **MUST**: 本地调试产物、布局 JSON、截图、日志、生成脚本输出等临时文件，统一放到仓库根目录的 `tmp/` 文件夹。
- **FORBIDDEN**: 不要把临时文件散落在仓库根目录；任务结束后及时清理。

### 正确做法
通过 `hdc shell` 执行以下命令，将布局信息导出为 JSON 文件：
```bash
hdc shell uitest dumpLayout -p /data/local/tmp/layout_$(date +%s).json
hdc file recv /data/local/tmp/layout_*.json ./tmp/layout.json
