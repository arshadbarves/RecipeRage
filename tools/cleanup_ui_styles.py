import os
import re
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Set, Dict, List, Tuple

def find_files(directory: str, extension: str) -> List[Path]:
    return list(Path(directory).rglob(f"*{extension}"))

def get_uxml_styles_and_classes(uxml_path: Path) -> Tuple[Set[str], Set[str]]:
    try:
        # Read the file as text first to handle potential namespace issues or just use a more permissive parser
        content = uxml_path.read_text()
        
        # We can use regex to find classes and styles if ET is too strict with custom tags
        # but ET is generally better if we handle namespaces.
        # Let's try ET with a custom parser that ignores unknown namespaces if possible, 
        # or just use regex for a more robust "extraction" phase.
        
        # Extract classes: class="([^"]+)"
        classes = set()
        for match in re.finditer(r'class="([^"]+)"', content):
            classes.update(match.group(1).split())
            
        # Extract styles: <Style src="([^"]+)" or <ui:Style src="([^"]+)"
        styles = set()
        for match in re.finditer(r'<(?:\w+:)?Style\s+[^>]*src="([^"]+)"', content):
            styles.add(match.group(1))
            
        return classes, styles
    except Exception as e:
        print(f"Error parsing UXML {uxml_path}: {e}")
        return set(), set()

def get_uss_classes_and_imports(uss_path: Path) -> Tuple[Set[str], Set[str]]:
    try:
        content = uss_path.read_text()
        
        # Improved regex for class definitions: .class-name, .another-class {
        # This matches anything starting with . followed by alphanumeric/dash/underscore
        # until a comma, curly brace, or other selector character.
        # We look for the block start '{' to ensure it's a rule.
        
        classes = set()
        # Find all blocks and their selectors
        blocks = re.findall(r'([^{]+)\{[^}]*\}', content)
        for selectors in blocks:
            # For each block, find all class names (starting with .)
            # We filter out pseudo-classes like :hover
            found = re.findall(r'\.([a-zA-Z0-9_-]+)(?::[a-z-]+)?', selectors)
            classes.update(found)
        
        # Regex for @import url("..."); or @import "..."
        imports = set(re.findall(r'@import\s+(?:url\("([^"]+)"\)|"([^"]+)")\s*;', content))
        # Flatten the imports since there are two groups
        flattened_imports = set()
        for imp in imports:
            for group in imp:
                if group:
                    flattened_imports.add(group)
        
        return classes, flattened_imports
    except Exception as e:
        print(f"Error reading USS {uss_path}: {e}")
        return set(), set()

def resolve_path(source_file: Path, path_str: str) -> Path:
    # Strip Unity query strings and fragments (?fileID=... or #Fragment)
    path_str = path_str.split('?')[0].split('#')[0]
    
    # Handle Unity-style paths or relative paths
    if path_str.startswith("project://database/"):
        cleaned = path_str.replace("project://database/", "")
        # Get the path relative to the project root
        return Path(os.getcwd()) / cleaned
    
    # Handle absolute paths if they occur (rare in Unity)
    if path_str.startswith("/"):
        return Path(path_str)
        
    return (source_file.parent / path_str).resolve()

def analyze_styles(base_dir: str):
    uxml_files = find_files(base_dir, ".uxml")
    uss_files = find_files(base_dir, ".uss")
    
    uss_map = {f.absolute(): get_uss_classes_and_imports(f) for f in uss_files}
    uxml_map = {f.absolute(): get_uxml_styles_and_classes(f) for f in uxml_files}
    
    issues = []
    
    # Check for classes in UXML not defined in any referenced USS
    for uxml_path, (classes, styles) in uxml_map.items():
        referenced_uss_classes = set()
        broken_styles = []
        
        def collect_classes(uss_rel_path: str, source_path: Path, visited: Set[Path]):
            abs_uss = resolve_path(source_path, uss_rel_path)
            if not abs_uss.exists():
                broken_styles.append(uss_rel_path)
                return
            
            if abs_uss.absolute() in visited:
                return
            
            visited.add(abs_uss.absolute())
            if abs_uss.absolute() in uss_map:
                cls, imps = uss_map[abs_uss.absolute()]
                referenced_uss_classes.update(cls)
                for imp in imps:
                    collect_classes(imp, abs_uss, visited)

        visited_uss = set()
        for style_src in styles:
            collect_classes(style_src, uxml_path, visited_uss)
            
        missing_classes = classes - referenced_uss_classes
        if broken_styles:
            issues.append({
                'type': 'UXML_BROKEN_STYLE',
                'file': uxml_path,
                'styles': broken_styles
            })
            
        if missing_classes:
            issues.append({
                'type': 'UXML_MISSING_CLASS',
                'file': uxml_path,
                'classes': missing_classes
            })

    # Check for unused classes in USS
    # A class is unused if it's not in any UXML that (directly or indirectly) imports this USS
    # This is more complex because we need to know all UXMLs that reach a USS
    uss_usage = {uss_path: set() for uss_path in uss_map}
    
    for uxml_path, (_, styles) in uxml_map.items():
        visited_uss = set()
        def mark_used(uss_rel_path: str, source_path: Path):
            abs_uss = resolve_path(source_path, uss_rel_path)
            if not abs_uss.exists() or abs_uss.absolute() in visited_uss:
                return
            visited_uss.add(abs_uss.absolute())
            if abs_uss.absolute() in uss_usage:
                uss_usage[abs_uss.absolute()].add(uxml_path)
                _, imps = uss_map[abs_uss.absolute()]
                for imp in imps:
                    mark_used(imp, abs_uss)
        
        for style_src in styles:
            mark_used(style_src, uxml_path)

    for uss_path, (classes, _) in uss_map.items():
        if not classes: continue
        
        # Filter out Unity built-in classes (starting with unity-)
        # These are used by Unity internally even if not in UXML
        own_classes = {c for c in classes if not c.startswith("unity-")}
        if not own_classes: continue
        
        # Find all classes used by UXMLs that reference this USS
        all_relevant_used_classes = set()
        for uxml_path in uss_usage[uss_path]:
            all_relevant_used_classes.update(uxml_map[uxml_path][0])
            
        unused_in_uss = own_classes - all_relevant_used_classes
        if unused_in_uss:
            issues.append({
                'type': 'USS_UNUSED_CLASS',
                'file': uss_path,
                'classes': unused_in_uss
            })
            
    return issues

def report_issues(issues):
    if not issues:
        print("No issues found!")
        return

    print("\n--- Cleanup Report ---")
    for issue in issues:
        rel_path = os.path.relpath(issue['file'], os.getcwd())
        if issue['type'] == 'UXML_MISSING_CLASS':
            print(f"[UXML] {rel_path}: Classes not found in any USS: {', '.join(issue['classes'])}")
        elif issue['type'] == 'USS_UNUSED_CLASS':
            print(f"[USS] {rel_path}: Classes not used in any UXML: {', '.join(issue['classes'])}")
        elif issue['type'] == 'UXML_BROKEN_STYLE':
            print(f"[UXML] {rel_path}: Broken style references: {', '.join(issue['styles'])}")

def perform_cleanup(issues):
    for issue in issues:
        if issue['type'] == 'UXML_MISSING_CLASS':
            cleanup_uxml(issue['file'], issue['classes'])
        elif issue['type'] == 'USS_UNUSED_CLASS':
            cleanup_uss(issue['file'], issue['classes'])

def cleanup_uxml(file_path: Path, classes_to_remove: Set[str]):
    print(f"Cleaning UXML: {file_path}")
    tree = ET.parse(file_path)
    root = tree.getroot()
    
    modified = False
    for elem in root.iter():
        class_attr = elem.get('class')
        if class_attr:
            current_classes = class_attr.split()
            new_classes = [c for c in current_classes if c not in classes_to_remove]
            if len(new_classes) != len(current_classes):
                if new_classes:
                    elem.set('class', ' '.join(new_classes))
                else:
                    del elem.attrib['class']
                modified = True
                
    if modified:
        tree.write(file_path, encoding='utf-8', xml_declaration=True)

def cleanup_uss(file_path: Path, classes_to_remove: Set[str]):
    print(f"Cleaning USS: {file_path}")
    content = file_path.read_text()
    
    for cls in classes_to_remove:
        # Match .classname { ... }
        # - Must start with .
        # - Must match exactly (word boundaries for class names)
        # - Must be followed by { or comma (for multiple selectors)
        
        # This is hard with just regex for multiple selectors.
        # Let's use a slightly more complex regex that handles the block removal.
        # We only remove the block IF it only contains the unused class.
        
        # 1. Remove blocks where the ONLY selector is the unused class
        pattern_only = rf'(?<![a-zA-Z0-9_-])\.{cls}\s*\{{[^}}]*\}}'
        content = re.sub(pattern_only, '', content)
        
        # 2. Remove the class from comma-separated selectors
        # Case: .unused, .used { ... } -> .used { ... }
        content = re.sub(rf'\.{cls}\s*,\s*', '', content)
        # Case: .used, .unused { ... } -> .used { ... }
        content = re.sub(rf',\s*\.{cls}(?=\s*\{{)', '', content)
        
    file_path.write_text(content)

if __name__ == "__main__":
    import sys
    import argparse
    import json
    
    parser = argparse.ArgumentParser(description="UXML/USS Cleanup Tool")
    parser.add_argument("--json", action="store_true", help="Output results in JSON format")
    parser.add_argument("--cleanup", action="store_true", help="Perform cleanup automatically")
    parser.add_argument("--file", type=str, help="Limit cleanup to a specific file path")
    args = parser.parse_args()
    
    base_assets = "Assets"
    if not os.path.exists(base_assets):
        print("Please run this script from the project root.")
        sys.exit(1)
        
    found_issues = analyze_styles(base_assets)
    
    # Filter issues if a specific file is requested
    if args.file:
        target_abs = Path(args.file).absolute()
        found_issues = [i for i in found_issues if Path(i['file']).absolute() == target_abs]
    
    if args.json:
        # Convert issues to JSON serializable (Path to str)
        json_issues = []
        for issue in found_issues:
            json_issue = issue.copy()
            json_issue['file'] = str(issue['file'])
            if 'classes' in issue: json_issue['classes'] = list(issue['classes'])
            if 'styles' in issue: json_issue['styles'] = list(issue['styles'])
            json_issues.append(json_issue)
        print(json.dumps(json_issues))
        
        if args.cleanup:
            perform_cleanup(found_issues)
    else:
        print("Analyzing UI styles...")
        report_issues(found_issues)
        
        if found_issues:
            if args.cleanup:
                perform_cleanup(found_issues)
                print("Cleanup complete.")
            else:
                confirm = input("\nDo you want to perform cleanup? (y/n): ")
                if confirm.lower() == 'y':
                    perform_cleanup(found_issues)
                    print("Cleanup complete.")
                else:
                    print("Cleanup cancelled.")
