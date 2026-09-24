"""arkui-bindgen：HarmonyOS NDK 头文件 → C# 绑定生成器（M3 工程化）。

取代 src/nativeBinding/extract_arkui_types.py（仅枚举）与 scripts/check-abi-mirror.ps1
（正则级结构校验）；以 libclang 解析为唯一事实源，语义修正全部走 config/semantics.yaml。
"""

__version__ = "1.0.0"
