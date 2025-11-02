#!/usr/bin/env python3
import re
import sys
from pathlib import Path

def replace_debug_logs(file_path):
    """Replace Debug.Log calls with GameLogger calls in a C# file."""
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    modified = False
    
    # Check if file already has GameLogger import
    has_logging_import = 'using Core.Logging;' in content
    
    # Replace Debug.Log( with GameLogger.Log(
    content = re.sub(r'Debug\.Log\(', 'GameLogger.Log(', content)
    
    # Replace Debug.LogWarning( with GameLogger.LogWarning(
    content = re.sub(r'Debug\.LogWarning\(', 'GameLogger.LogWarning(', content)
    
    # Replace Debug.LogError( with GameLogger.LogError(
    content = re.sub(r'Debug\.LogError\(', 'GameLogger.LogError(', content)
    
    # Replace Debug.LogException( with GameLogger.LogException(
    content = re.sub(r'Debug\.LogException\(', 'GameLogger.LogException(', content)
    
    # Remove [ClassName] prefixes from log messages since category is automatic now
    # Pattern: "[ClassName] Message" -> "Message"
    content = re.sub(r'GameLogger\.(Log|LogWarning|LogError)\(\$?"?\[[\w]+\]\s*', r'GameLogger.\1($"', content)
    content = re.sub(r'GameLogger\.(Log|LogWarning|LogError)\("?\[[\w]+\]\s*', r'GameLogger.\1("', content)
    
    if content != original_content:
        modified = True
        
        # Add using Core.Logging if not present and GameLogger is used
        if not has_logging_import and 'GameLogger' in content:
            # Find the last using statement
            using_pattern = r'(using [^;]+;)'
            matches = list(re.finditer(using_pattern, content))
            if matches:
                last_using = matches[-1]
                insert_pos = last_using.end()
                content = content[:insert_pos] + '\nusing Core.Logging;' + content[insert_pos:]
        
        # Write back
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        
        return True
    
    return False

def main():
    # Get all C# files with Debug calls
    script_dir = Path(__file__).parent
    assets_dir = script_dir / 'Assets' / 'Scripts'
    
    if not assets_dir.exists():
        print(f"Error: {assets_dir} not found")
        return
    
    modified_files = []
    
    # Find all .cs files
    for cs_file in assets_dir.rglob('*.cs'):
        try:
            with open(cs_file, 'r', encoding='utf-8') as f:
                content = f.read()
                if 'Debug.' in content:
                    if replace_debug_logs(cs_file):
                        modified_files.append(cs_file)
                        print(f"✓ Modified: {cs_file.relative_to(script_dir)}")
        except Exception as e:
            print(f"✗ Error processing {cs_file}: {e}")
    
    print(f"\n✓ Modified {len(modified_files)} files")

if __name__ == '__main__':
    main()
