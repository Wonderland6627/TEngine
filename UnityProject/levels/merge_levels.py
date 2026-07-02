#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
合并脚本：将所有 level_X.json 文件合并成 levels.json
"""

import json
import os
import re
from pathlib import Path


def merge_levels_json(input_dir='.', output_file='levels.json'):
    """
    将所有 level_X.json 文件合并成 levels.json
    
    Args:
        input_dir: 输入目录，默认为当前目录
        output_file: 输出的 levels.json 文件路径
    """
    # 获取脚本所在目录
    script_dir = Path(__file__).parent
    input_path = Path(input_dir) if input_dir != '.' else script_dir
    output_path = script_dir / output_file
    
    # 查找所有 level_X.json 文件
    pattern = re.compile(r'^level_(\d+)\.json$')
    level_files = []
    
    print(f"Scanning {input_path} for level files...")
    for file in input_path.glob('level_*.json'):
        match = pattern.match(file.name)
        if match:
            level_id = int(match.group(1))
            level_files.append((level_id, file))
    
    if not level_files:
        print(f"Error: No level_X.json files found in {input_path}!")
        return
    
    # 按 levelId 排序
    level_files.sort(key=lambda x: x[0])
    print(f"Found {len(level_files)} level files")
    
    # 读取并合并所有关卡
    levels = []
    merge_count = 0
    
    for level_id, file_path in level_files:
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                level = json.load(f)
            
            # 验证 levelId 是否匹配文件名
            if isinstance(level, dict) and level.get('levelId') == level_id:
                levels.append(level)
                print(f"Loaded: {file_path.name} (levelId: {level_id})")
                merge_count += 1
            else:
                print(f"Warning: {file_path.name} has mismatched levelId, skipping...")
        except json.JSONDecodeError as e:
            print(f"Error: Failed to parse {file_path.name} - {e}")
        except Exception as e:
            print(f"Error: Failed to read {file_path.name} - {e}")
    
    if not levels:
        print("Error: No valid levels to merge!")
        return
    
    # 写入合并后的 levels.json（人类可读格式，方便版本管理 diff 和人工检查）
    try:
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(levels, f, ensure_ascii=False, indent=4)
        print(f"\nMerge complete! Created {output_path} with {merge_count} levels.")
    except Exception as e:
        print(f"Error: Failed to write {output_path} - {e}")


if __name__ == '__main__':
    import sys
    
    # 支持命令行参数
    input_dir = sys.argv[1] if len(sys.argv) > 1 else '.'
    output_file = sys.argv[2] if len(sys.argv) > 2 else 'levels.json'
    
    merge_levels_json(input_dir, output_file)

