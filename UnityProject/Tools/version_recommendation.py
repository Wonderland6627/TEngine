#!/usr/bin/env python3
"""
根据当前 Git 变更，给微信小游戏打包流程提供版本号更新建议。
"""

from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path


RESOURCE_PREFIXES = (
    "Assets/AssetRaw/",
    "Assets/Scenes/",
    "Assets/StreamingAssets/",
)

APP_HINT_FILES = {
    "ProjectSettings/ProjectSettings.asset",
    "Assets/TEngine/ResRaw/Resources/TEngineGlobalSettings.asset",
    "Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset",
}

CODE_PREFIXES = (
    "Assets/GameScripts/",
    "Assets/Editor/",
    "Packages/",
)

IGNORE_PREFIXES = (
    "CDN_Backup/",
    "WXExport/",
    "Library/",
    "Temp/",
)

RESOURCE_EXTENSIONS = {
    ".prefab",
    ".unity",
    ".asset",
    ".mat",
    ".anim",
    ".controller",
    ".overridecontroller",
    ".spriteatlas",
    ".png",
    ".jpg",
    ".jpeg",
    ".tga",
    ".psd",
    ".fbx",
    ".obj",
    ".wav",
    ".mp3",
    ".ogg",
    ".ttf",
    ".otf",
    ".shader",
    ".shadervariants",
}

CODE_EXTENSIONS = {
    ".cs",
    ".asmdef",
    ".asmref",
    ".js",
    ".ts",
    ".lua",
    ".json",
    ".xml",
    ".md",
    ".txt",
    ".yaml",
    ".yml",
}


def configure_stdio_utf8() -> None:
    if hasattr(sys.stdout, "reconfigure"):
        try:
            sys.stdout.reconfigure(encoding="utf-8")
            sys.stderr.reconfigure(encoding="utf-8")
        except Exception:
            pass


def run_git(project_root: Path, *args: str) -> tuple[bool, str, str]:
    process = subprocess.run(
        ["git", *args],
        cwd=str(project_root),
        text=True,
        capture_output=True,
    )
    stdout = process.stdout or ""
    stderr = process.stderr or ""

    if process.returncode == 0:
        return True, stdout, stderr

    message = stderr.strip() or stdout.strip() or f"git {' '.join(args)} 执行失败，退出码 {process.returncode}"
    return False, "", message


def normalize_path(raw_path: str, project_folder: str) -> str:
    normalized = raw_path.strip().replace("\\", "/")
    if not normalized:
        return ""

    if normalized.lower().startswith("warning:"):
        return ""

    if normalized.startswith("./"):
        normalized = normalized[2:]

    prefix = f"{project_folder}/"
    if normalized.lower().startswith(prefix.lower()):
        normalized = normalized[len(prefix) :]

    return normalized


def parse_changed_files(outputs: list[str], project_folder: str) -> list[str]:
    changed_set: set[str] = set()
    for output in outputs:
        if not output.strip():
            continue

        for line in output.splitlines():
            normalized = normalize_path(line, project_folder)
            if not normalized:
                continue
            changed_set.add(normalized)

    return sorted(changed_set, key=str.lower)


def is_ignored_path(path: str) -> bool:
    lower_path = path.lower()
    return any(lower_path.startswith(prefix.lower()) for prefix in IGNORE_PREFIXES)


def is_app_hint_file(path: str) -> bool:
    return path in APP_HINT_FILES


def is_resource_file(path: str) -> bool:
    lower_path = path.lower()
    if any(lower_path.startswith(prefix.lower()) for prefix in RESOURCE_PREFIXES):
        return True

    if path in (
        "Assets/TEngine/AssetSetting/AssetBundleCollectorConfig.xml",
        "Assets/TEngine/AssetSetting/AssetBundleCollectorSetting.asset",
    ):
        return True

    if not lower_path.startswith("assets/"):
        return False

    if any(lower_path.startswith(prefix.lower()) for prefix in CODE_PREFIXES):
        return False

    extension = Path(path).suffix.lower()
    return extension in RESOURCE_EXTENSIONS


def is_code_file(path: str) -> bool:
    lower_path = path.lower()
    if any(lower_path.startswith(prefix.lower()) for prefix in CODE_PREFIXES):
        return True

    if lower_path.startswith("projectsettings/"):
        return True

    extension = Path(path).suffix.lower()
    return extension in CODE_EXTENSIONS


def analyze_changed_files(changed_files: list[str]) -> dict[str, list[str]]:
    result = {
        "resource_files": [],
        "app_hint_files": [],
        "code_files": [],
        "unknown_files": [],
    }

    for file_path in changed_files:
        if is_ignored_path(file_path):
            continue

        if is_app_hint_file(file_path):
            result["app_hint_files"].append(file_path)
            continue

        if is_resource_file(file_path):
            result["resource_files"].append(file_path)
            continue

        if is_code_file(file_path):
            result["code_files"].append(file_path)
            continue

        result["unknown_files"].append(file_path)

    return result


def append_preview(lines: list[str], title: str, items: list[str], max_count: int = 6) -> None:
    if not items:
        return

    lines.append("")
    lines.append(f"{title}（最多显示 {max_count} 条）：")
    for item in items[:max_count]:
        lines.append(f"  - {item}")

    rest = len(items) - max_count
    if rest > 0:
        lines.append(f"  - ... 另 {rest} 条")


def build_recommendation(changed_files: list[str]) -> str:
    analysis = analyze_changed_files(changed_files)
    lines: list[str] = []
    lines.append("自动分析结果（基于 git diff + 未跟踪文件）")

    if not changed_files:
        lines.append("- 当前没有检测到变更文件。")
        lines.append("- 资源版本号：建议不更新。")
        lines.append("- App版本号：建议不更新。")
        return "\n".join(lines)

    lines.append(f"- 变更文件总数：{len(changed_files)}")
    lines.append(f"- 资源相关：{len(analysis['resource_files'])}")
    lines.append(f"- 代码相关：{len(analysis['code_files'])}")
    lines.append(f"- App配置相关：{len(analysis['app_hint_files'])}")
    lines.append(f"- 其他待确认：{len(analysis['unknown_files'])}")
    lines.append("")
    lines.append("推荐：")

    if analysis["resource_files"]:
        lines.append("- 资源版本号：建议更新（检测到资源文件变更）。")
    else:
        lines.append("- 资源版本号：建议不更新（未检测到资源文件变更）。")

    if analysis["app_hint_files"]:
        lines.append("- App版本号：检测到基础配置变更，请按发布计划确认是否升级；若仍发布到当前 CDN 目录，可保持不变。")
    else:
        lines.append("- App版本号：建议保持不变（默认沿用当前 CDN 目录）。")

    lines.append("- 说明：仅代码改动通常只需要重新导出并替换 bin.txt。")
    append_preview(lines, "资源文件样例", analysis["resource_files"])
    append_preview(lines, "App配置变更样例", analysis["app_hint_files"])
    append_preview(lines, "待确认文件样例", analysis["unknown_files"])
    return "\n".join(lines)


def main() -> int:
    configure_stdio_utf8()

    parser = argparse.ArgumentParser(description="为微信小游戏打包提供版本更新建议。")
    parser.add_argument(
        "--project-root",
        default=".",
        help="UnityProject 根目录（默认当前目录）",
    )
    args = parser.parse_args()

    project_root = Path(args.project_root).resolve()
    if not project_root.exists():
        print(f"目录不存在：{project_root}", file=sys.stderr)
        return 2

    ok, _, error = run_git(project_root, "rev-parse", "--is-inside-work-tree")
    if not ok:
        print(f"当前目录不是 Git 仓库或无法访问 Git。\n{error}", file=sys.stderr)
        return 2

    ok, tracked_changed_files, error = run_git(project_root, "diff", "--name-only", "HEAD")
    if not ok:
        print(f"读取 Git 变更失败。\n{error}", file=sys.stderr)
        return 2

    ok, untracked_files, error = run_git(project_root, "ls-files", "--others", "--exclude-standard")
    if not ok:
        print(f"读取未跟踪文件失败。\n{error}", file=sys.stderr)
        return 2

    changed_files = parse_changed_files([tracked_changed_files, untracked_files], project_root.name)
    recommendation = build_recommendation(changed_files)
    print(recommendation)
    return 0


if __name__ == "__main__":
    sys.exit(main())
