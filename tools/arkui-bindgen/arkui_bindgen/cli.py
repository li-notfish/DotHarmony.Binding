"""arkui-bindgen CLI：gen / check / diff。

gen   —— 重出生成物（枚举 .g.cs + 元数据 JSON + A2 代码目标）。
check —— 内存中重出并与盘上产物逐字节比对，漂移即退出码 1（CI 门禁用）；
         code target 声明 snapshot 时同时做逐签名比对（A2 验收）。
diff  —— 只对 code target 做逐签名比对（--target 指定，--against 覆盖快照路径）。
"""
from __future__ import annotations

import argparse
import io
import sys
from pathlib import Path

from .diff import diff_texts
from .emit_csharp import emit_enum_file, emit_enum_json
from .emit_target import emit_code_target
from .parse import parse_headers
from .sdk import find_include_dir
from .semantics import Semantics


def _read_text(path: Path) -> str | None:
    if not path.is_file():
        return None
    return path.read_bytes().decode("utf-8")


def _write(path: Path, text: str) -> bool:
    """与盘上不同才写入；返回是否发生了写入。"""
    path.parent.mkdir(parents=True, exist_ok=True)
    if _read_text(path) == text:
        return False
    with io.open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    return True


def run(argv: list[str]) -> int:
    ap = argparse.ArgumentParser(prog="arkui-bindgen")
    ap.add_argument("command", choices=("gen", "check", "diff"))
    ap.add_argument("--sdk", default=None, help="SDK 根或 include 根；省略时按环境探测")
    ap.add_argument("--config", default=str(Path(__file__).resolve().parent.parent / "config" / "semantics.yaml"))
    ap.add_argument("--out-dir", default="src/HarmonyOS.Bindings/NativeNode",
                    help="生成物输出目录（相对 CWD）")
    ap.add_argument("--target", default=None, help="diff 时指定 code target 名（缺省为全部声明 snapshot 的目标）")
    ap.add_argument("--against", default=None, help="diff 比对的手写文件（缺省用 target.snapshot）")
    args = ap.parse_args(argv)

    sem = Semantics.load(Path(args.config))
    headers_cfg = sem.headers
    include_root = find_include_dir(args.sdk)
    header_dir = include_root / headers_cfg.get("dir", "arkui")

    header_files = [header_dir / h for h in headers_cfg.get("files", [])]
    glob_dirs = headers_cfg.get("extra_glob_dirs", [])
    attr_files: list[Path] = []
    for g in glob_dirs:
        attr_files.extend(sorted(header_dir.glob(g + "/*.h")))
    all_headers = header_files + attr_files
    missing = [str(h) for h in all_headers if not h.is_file()]
    if missing:
        print("error: 头文件缺失:\n  " + "\n  ".join(missing), file=sys.stderr)
        return 1

    # 包含根：include 根 + arkui 子目录 + 各 glob 目录（node_attributes 内部互相 include "button.h" 等）
    include_roots = [include_root, header_dir] + [header_dir / g for g in glob_dirs]
    model = parse_headers(all_headers, include_roots)

    out_dir = Path(args.out_dir)
    drift: list[str] = []

    def _snapshot_path(snap: str) -> Path:
        tool_root = Path(args.config).resolve().parent.parent
        for cand in (tool_root / snap, Path(snap)):
            if cand.is_file():
                return cand
        return tool_root / snap

    sig_problems: list[str] = []

    for _target_name, tgt in sem.enum_targets.items():
        wanted = list(tgt.get("include_enums", []))
        attr_enum_names: set[str] = set()
        for f in attr_files:
            attr_enum_names.update(parse_headers([f], include_roots).enums.keys())
        wanted += sorted(attr_enum_names)
        src_desc = ", ".join(headers_cfg.get("files", []) + [g + "/*.h" for g in glob_dirs])
        text = emit_enum_file(model.enums, wanted, src_desc)

        out_path = out_dir / tgt["file"]
        if args.command == "gen":
            changed = _write(out_path, text)
            emitted = sum(1 for w in wanted if w in model.enums)
            print(f"{'wrote' if changed else 'up-to-date'}: {out_path} "
                  f"({len(wanted)} wanted, {emitted} emitted)")
        elif args.command == "check" and _read_text(out_path) != text:
            drift.append(str(out_path))

        # JSON 与旧版保持一致：全量已解析枚举，落点仍在 NativeNode/ 根部（jest 测试按此路径读）
        if tgt.get("json"):
            json_path = out_dir / tgt["json"]
            jtext = emit_enum_json(model.enums)
            if args.command == "gen":
                _write(json_path, jtext)
                print(f"wrote: {json_path}")
            elif args.command == "check" and _read_text(json_path) != jtext:
                drift.append(str(json_path))

    # A2：代码目标（结构体/函数表/PInvoke）
    for tname, tgt in sem.code_targets.items():
        try:
            text = emit_code_target(tname, tgt, model, sem)
        except Exception as ex:
            print(f"error: 目标 {tname} 发射失败: {ex}", file=sys.stderr)
            return 1
        out_path = out_dir / tgt["file"]
        if args.command == "gen":
            changed = _write(out_path, text)
            print(f"{'wrote' if changed else 'up-to-date'}: {out_path}")
        elif args.command == "check" and _read_text(out_path) != text:
            drift.append(str(out_path))
        # 逐签名比对：diff 显式触发，或 check 时目标声明了 snapshot
        if args.command == "diff" and args.target and args.target != tname:
            continue
        snap = args.against or tgt.get("snapshot")
        want_diff = (args.command == "diff" and snap) or (args.command == "check" and tgt.get("snapshot"))
        if want_diff:
            if not snap:
                print(f"error: 目标 {tname} 未声明 snapshot 且未给 --against", file=sys.stderr)
                return 1
            spath = _snapshot_path(snap)
            legacy = _read_text(spath)
            if legacy is None:
                print(f"error: 快照不存在: {spath}", file=sys.stderr)
                return 1
            probs = diff_texts(text, legacy, ignore_scopes=set(tgt.get("ignore_scopes", [])))
            if probs:
                for p in probs:
                    sig_problems.append(f"{tname}: {p}")
            elif args.command == "diff":
                print(f"diff OK: {tname} 与 {spath} 逐签名一致")

    if args.command == "check":
        if drift:
            print("drift detected:", file=sys.stderr)
            for d in drift:
                print(f"  {d}", file=sys.stderr)
        if sig_problems:
            print("signature diff detected:", file=sys.stderr)
            for p in sig_problems:
                print(f"  {p}", file=sys.stderr)
        if drift or sig_problems:
            return 1
        print("check OK: generated artifacts are up-to-date")
    elif args.command == "diff":
        if sig_problems:
            print("signature diff detected:", file=sys.stderr)
            for p in sig_problems:
                print(f"  {p}", file=sys.stderr)
            return 1
    return 0


def main() -> None:
    sys.exit(run(sys.argv[1:]))


if __name__ == "__main__":
    main()
