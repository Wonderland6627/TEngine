#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
拆分脚本：将 levels.json 拆分成 level_1.json, level_2.json 等
"""

import json
import os
from pathlib import Path


def split_levels_json(input_file='levels.json', output_dir='.'):
    """
    将 levels.json 拆分成多个单独的 level_X.json 文件
    
    Args:
        input_file: 输入的 levels.json 文件路径
        output_dir: 输出目录，默认为当前目录
    """
    # 获取脚本所在目录
    script_dir = Path(__file__).parent
    input_path = script_dir / input_file
    output_path = Path(output_dir) if output_dir != '.' else script_dir
    
    # 检查输入文件是否存在
    if not input_path.exists():
        print(f"Error: {input_path} not found!")
        return
    
    # 读取 levels.json
    print(f"Reading {input_path}...")
    try:
        with open(input_path, 'r', encoding='utf-8') as f:
            levels = json.load(f)
    except json.JSONDecodeError as e:
        print(f"Error: Failed to parse JSON - {e}")
        return
    except Exception as e:
        print(f"Error: Failed to read file - {e}")
        return
    
    # 验证数据格式
    if not isinstance(levels, list):
        print("Error: levels.json should contain a JSON array!")
        return
    
    # 拆分每个关卡
    print(f"Splitting {len(levels)} levels...")
    split_count = 0
    
    for level in levels:
        if not isinstance(level, dict) or 'levelId' not in level:
            print(f"Warning: Skipping invalid level data: {level}")
            continue
        
        level_id = level['levelId']
        output_file = output_path / f"level_{level_id}.json"
        
        # 写入单个关卡文件
        try:
            with open(output_file, 'w', encoding='utf-8') as f:
                json.dump(level, f, ensure_ascii=False, indent=4)
            print(f"Created: {output_file}")
            split_count += 1
        except Exception as e:
            print(f"Error: Failed to write {output_file} - {e}")
    
    print(f"\nSplit complete! Created {split_count} level files.")


if __name__ == '__main__':
    import sys
    
    # 支持命令行参数
    input_file = sys.argv[1] if len(sys.argv) > 1 else 'levels.json'
    output_dir = sys.argv[2] if len(sys.argv) > 2 else '.'
    
    split_levels_json(input_file, output_dir)

