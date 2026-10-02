"""semantics.yaml 加载与访问。

原则（ROADMAP M3）：生成器不猜。所有命名归一、bool 表达、单位勘误、
类型取舍（opaque / 句柄别名 / IntPtr）都必须以声明式配置落在这里，
配置缺口在提取阶段报错，而不是静默产出错的东西。
"""
from __future__ import annotations

from pathlib import Path

import yaml


class Semantics:
    def __init__(self, raw: dict) -> None:
        self._raw = raw or {}

    @classmethod
    def load(cls, path: Path) -> "Semantics":
        with open(path, "r", encoding="utf-8") as f:
            return cls(yaml.safe_load(f))

    @property
    def headers(self) -> dict:
        return self._raw.get("headers", {})

    @property
    def enum_targets(self) -> dict:
        return self._raw.get("enum_targets", {})

    @property
    def bool_policy(self) -> dict:
        return self._raw.get("bool", {"unmanaged_fnptr": "byte", "pinvoke": "marshal_u1"})

    @property
    def code_targets(self) -> dict:
        return self._raw.get("code_targets", {})

    def get(self, key: str, default=None):
        return self._raw.get(key, default)
