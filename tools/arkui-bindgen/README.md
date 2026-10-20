# arkui-bindgen

HarmonyOS NDK 头文件（ArkUI `native_*.h`）→ C# 绑定生成器。取代已删除的
`src/nativeBinding/extract_arkui_types.py`（正则抽枚举）与 `scripts/check-abi-mirror.ps1`
（正则级函数表结构校验，由 `--check` 吞并）。

## 原则

- **libclang 是唯一事实源**：枚举值、结构体布局、函数表成员顺序全部来自
  `clang.cindex` 的 AST，不用正则解析 C。
- **生成器不猜**：命名归一、C bool 表达、头文件注释单位勘误、opaque/句柄别名
  取舍一律走 [config/semantics.yaml](config/semantics.yaml) 声明式配置；
  配置缺口报错，不会静默产出错的东西。

## 用法

```powershell
python -m pip install -r requirements.txt   # libclang + PyYAML（+pytest 跑单测）

# 生成（默认读取 config/semantics.yaml 的头文件集合；默认 out-dir 已是 NativeNode/）
python -m arkui_bindgen gen --sdk "<OpenHarmony SDK 根>" \
    --out-dir src\HarmonyOS.Bindings\NativeNode

# 漂移检查（本地门禁）：内存重出并与盘上产物逐字节比对；
# 声明了 snapshot 的 code target 同时做逐签名比对
python -m arkui_bindgen check

# 逐签名比对：生成物 vs 手写快照（迁移验收与 SDK 升级差异定位）
python -m arkui_bindgen diff               # 全部声明 snapshot 的目标
python -m arkui_bindgen diff --target animate
```

`--sdk` 省略时按 `OHOS_SDK_BASE` → `OHOS_SDK_HOME` → `OHSDK_HOME` → DevEco 内置
（`D:\Program Files\Huawei\DevEco Studio\sdk\default\openharmony`）→ 历史默认路径
（`D:\Harmony\OpenHarmony\Sdk`）探测，每个候选依次尝试「版本号子目录 / `openharmony` 子目录 / 根」三种形态。
TS 侧 `api-generator` 走同一套解析（`--sdk` 优先），两侧无需分别记忆候选顺序。

> `check` 依赖本机 OpenHarmony SDK 头文件，因此当前 GitHub Actions 最小 CI 不执行该步骤；
> 本地合入前请先运行一次。

## 测试

```powershell
python -m pytest tools/arkui-bindgen/tests -v
```
