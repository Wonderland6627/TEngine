#!/usr/bin/env python3
"""
根据相对发布基准的 Git 变更，给微信小游戏打包流程提供版本号更新建议。

基准优先级（auto）：
  A) last_publish 账本中的 commit
  B) 最近的 publish/* git tag
  C) 仅当前工作区（相对 HEAD）
"""

from __future__ import annotations

import argparse
import json
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

DEFAULT_LEDGER_RELATIVE = "Tools/last_publish.json"
RESULT_MARKER = "@@RESULT"


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


def load_ledger(ledger_path: Path) -> dict | None:
    if not ledger_path.is_file():
        return None

    try:
        data = json.loads(ledger_path.read_text(encoding="utf-8-sig"))
    except (OSError, json.JSONDecodeError):
        return None

    if not isinstance(data, dict):
        return None

    last_publish = data.get("lastPublish")
    if not isinstance(last_publish, dict):
        return None

    commit = str(last_publish.get("commit") or "").strip()
    if not commit:
        return None

    return last_publish


def resolve_commit(project_root: Path, ref: str) -> tuple[bool, str, str]:
    ok, stdout, error = run_git(project_root, "rev-parse", "--verify", f"{ref}^{{commit}}")
    if not ok:
        return False, "", error
    return True, stdout.strip(), ""


def find_latest_publish_tag(project_root: Path) -> tuple[bool, str, str]:
    ok, stdout, error = run_git(
        project_root,
        "tag",
        "-l",
        "publish/*",
        "--sort=-creatordate",
    )
    if not ok:
        return False, "", error

    tags = [line.strip() for line in stdout.splitlines() if line.strip()]
    if not tags:
        return False, "", "未找到 publish/* tag。"
    return True, tags[0], ""


def resolve_baseline(
    project_root: Path,
    mode: str,
    custom_ref: str,
    ledger_path: Path,
) -> tuple[str, str, str, dict | None, str]:
    """
    返回: (source, ref_label, commit, ledger_or_none, warning)
    source: ledger | tag | commit | workdir
    """
    mode = (mode or "auto").strip().lower()
    custom_ref = (custom_ref or "").strip()
    warning = ""

    if mode == "workdir":
        return "workdir", "HEAD", "", None, "仅对比当前工作区，跨提交累计变更不会被计入。"

    if mode == "commit":
        if not custom_ref:
            return "workdir", "HEAD", "", None, "未填写自定义基准，已回退到仅工作区。"
        ok, commit, error = resolve_commit(project_root, custom_ref)
        if not ok:
            return "workdir", "HEAD", "", None, f"自定义基准无效（{error}），已回退到仅工作区。"
        return "commit", custom_ref, commit, None, ""

    if mode in ("ledger", "auto"):
        ledger = load_ledger(ledger_path)
        if ledger is not None:
            commit_ref = str(ledger.get("commit") or "").strip()
            ok, commit, error = resolve_commit(project_root, commit_ref)
            if ok:
                label = commit_ref[:12]
                app_ver = str(ledger.get("appVersion") or "").strip()
                res_ver = str(ledger.get("resourceVersion") or "").strip()
                if app_ver or res_ver:
                    label = f"{commit_ref[:12]} (App {app_ver or '?'} / Res {res_ver or '?'})"
                return "ledger", label, commit, ledger, ""
            warning = f"账本 commit 无效（{error}）。"
        else:
            warning = "未找到有效 lastPublish 账本。"

        if mode == "ledger":
            return "workdir", "HEAD", "", None, f"{warning} 已回退到仅工作区。"

    if mode in ("tag", "auto"):
        ok, tag, error = find_latest_publish_tag(project_root)
        if ok:
            ok_commit, commit, commit_error = resolve_commit(project_root, tag)
            if ok_commit:
                warn = warning
                if warn:
                    warn = f"{warn} 已改用最近 publish tag。"
                return "tag", tag, commit, None, warn
            warning = f"{warning} tag `{tag}` 无法解析（{commit_error}）。".strip()
        else:
            warning = f"{warning} {error}".strip()

        if mode == "tag":
            return "workdir", "HEAD", "", None, f"{warning} 已回退到仅工作区。"

    final_warning = warning or "未找到账本或 publish tag，已回退到仅工作区（基准不可靠）。"
    return "workdir", "HEAD", "", None, final_warning


def collect_changed_files(project_root: Path, baseline_commit: str) -> tuple[bool, list[str], str]:
    outputs: list[str] = []

    if baseline_commit:
        ok, since_baseline, error = run_git(
            project_root,
            "diff",
            "--name-only",
            f"{baseline_commit}...HEAD",
        )
        if not ok:
            return False, [], f"读取相对基准变更失败。\n{error}"
        outputs.append(since_baseline)

    ok, workdir_files, error = run_git(project_root, "diff", "--name-only", "HEAD")
    if not ok:
        return False, [], f"读取工作区变更失败。\n{error}"
    outputs.append(workdir_files)

    ok, untracked_files, error = run_git(
        project_root,
        "ls-files",
        "--others",
        "--exclude-standard",
    )
    if not ok:
        return False, [], f"读取未跟踪文件失败。\n{error}"
    outputs.append(untracked_files)

    changed_files = parse_changed_files(outputs, project_root.name)
    return True, changed_files, ""


def build_recommendation(
    changed_files: list[str],
    baseline_source: str,
    baseline_label: str,
    baseline_commit: str,
    warning: str,
    ledger: dict | None,
) -> str:
    analysis = analyze_changed_files(changed_files)
    lines: list[str] = []
    lines.append("自动分析结果（相对发布基准累计变更）")
    lines.append(f"- 基准来源：{baseline_source}")
    lines.append(f"- 基准：{baseline_label}")
    if baseline_commit:
        lines.append(f"- 对比区间：{baseline_commit[:12]}...HEAD + 工作区")
    else:
        lines.append("- 对比区间：仅工作区（相对 HEAD）")

    if ledger is not None:
        wechat_version = str(ledger.get("wechatVersion") or "").strip()
        published_at = str(ledger.get("publishedAt") or "").strip()
        mode = str(ledger.get("mode") or "").strip()
        if wechat_version:
            lines.append(f"- 账本微信代码包版本：{wechat_version}")
        if published_at:
            lines.append(f"- 账本发布时间：{published_at}")
        if mode:
            lines.append(f"- 账本发布模式：{mode}")

    if warning:
        lines.append(f"- 警告：{warning}")

    resource_changed = 1 if analysis["resource_files"] else 0
    app_hint = 1 if analysis["app_hint_files"] else 0
    code_changed = 1 if analysis["code_files"] else 0

    if not changed_files:
        lines.append("- 当前没有检测到变更文件。")
        lines.append("- 资源版本号：建议不更新。")
        lines.append("- App版本号：建议不更新。")
        lines.append("")
        lines.append(
            f"{RESULT_MARKER} resource={resource_changed} code={code_changed} "
            f"app_hint={app_hint} baseline={baseline_source}"
        )
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
        lines.append("- 流程：升资源版本 → 构建 Bundle → 导出微信 → 全量复制到 CDN_Backup。")
    else:
        lines.append("- 资源版本号：建议不更新（未检测到资源文件变更）。")
        if analysis["code_files"]:
            lines.append("- 流程：导出微信 → 仅复制 bin.txt 到 CDN_Backup。")

    if analysis["app_hint_files"]:
        lines.append("- App版本号：检测到基础配置变更，请按发布计划确认是否升级；若仍发布到当前 CDN 目录，可保持不变。")
    else:
        lines.append("- App版本号：建议保持不变（默认沿用当前 CDN 目录）。")

    lines.append("- 说明：仅代码改动通常只需要重新导出并替换 bin.txt。")
    append_preview(lines, "资源文件样例", analysis["resource_files"])
    append_preview(lines, "App配置变更样例", analysis["app_hint_files"])
    append_preview(lines, "待确认文件样例", analysis["unknown_files"])
    lines.append("")
    lines.append(
        f"{RESULT_MARKER} resource={resource_changed} code={code_changed} "
        f"app_hint={app_hint} baseline={baseline_source}"
    )
    return "\n".join(lines)


def main() -> int:
    configure_stdio_utf8()

    parser = argparse.ArgumentParser(description="为微信小游戏打包提供版本更新建议。")
    parser.add_argument(
        "--project-root",
        default=".",
        help="UnityProject 根目录（默认当前目录）",
    )
    parser.add_argument(
        "--baseline-mode",
        default="auto",
        choices=("auto", "ledger", "tag", "commit", "workdir"),
        help="基准模式：auto=账本→tag→工作区",
    )
    parser.add_argument(
        "--baseline-ref",
        default="",
        help="自定义基准 commit/tag（baseline-mode=commit 时使用）",
    )
    parser.add_argument(
        "--ledger-path",
        default="",
        help="last_publish.json 路径（默认 Tools/last_publish.json）",
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

    ledger_path = Path(args.ledger_path).resolve() if args.ledger_path else (project_root / DEFAULT_LEDGER_RELATIVE)
    baseline_source, baseline_label, baseline_commit, ledger, warning = resolve_baseline(
        project_root,
        args.baseline_mode,
        args.baseline_ref,
        ledger_path,
    )

    ok, changed_files, error = collect_changed_files(project_root, baseline_commit)
    if not ok:
        print(error, file=sys.stderr)
        return 2

    recommendation = build_recommendation(
        changed_files,
        baseline_source,
        baseline_label,
        baseline_commit,
        warning,
        ledger,
    )
    print(recommendation)
    return 0


if __name__ == "__main__":
    sys.exit(main())
