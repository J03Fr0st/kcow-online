"""Lightweight executable architecture rules; run from any working directory."""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
errors = []
application = root / 'apps/backend/src/Application'
for path in application.rglob('*.cs'):
    if {'bin', 'obj'} & set(path.parts):
        continue
    if re.search(r'^\s*using\s+(?:Kcow\.Infrastructure|Dapper|Microsoft\.Data\.Sqlite)\b', path.read_text(encoding='utf-8'), re.MULTILINE):
        errors.append(f'{path.relative_to(root)}: Application must depend on policy-owned ports')
for name, allowed in {'Domain': set(), 'Application': {'Kcow.Domain'}, 'Infrastructure': {'Kcow.Domain', 'Kcow.Application'}, 'Api': {'Kcow.Application', 'Kcow.Infrastructure'}}.items():
    path = root / f'apps/backend/src/{name}/Kcow.{name}.csproj'
    for reference in ET.parse(path).iter('ProjectReference'):
        target = Path(reference.attrib['Include'].replace('\\', '/')).stem
        if target not in allowed:
            errors.append(f'{name}: forbidden project reference {target}')
core = root / 'apps/frontend/src/app/core'
for path in core.rglob('*.ts'):
    for module in re.findall(r"(?:from\s*|import\s*\()['\"]([^'\"]+)", path.read_text(encoding='utf-8')):
        if module.startswith('@features/') or (module.startswith('.') and '/features/' in (path.parent / module).resolve().as_posix()):
            errors.append(f'{path.relative_to(root)}: core must not import feature-owned code')
assert not errors, '\n'.join(errors)
print('Architecture boundaries verified')
