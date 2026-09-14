"""Build retained tools and exercise their real CLI against isolated SQLite files."""
from contextlib import closing
from pathlib import Path
import hashlib
import os
import sqlite3
import subprocess
import tempfile

repo = Path(__file__).resolve().parents[1]
projects = sorted((repo / 'apps/backend/tools').glob('*/*.csproj'))
for project in projects:
    built = subprocess.run(['dotnet', 'build', str(project), '--verbosity', 'quiet'],
                           capture_output=True, text=True, timeout=120)
    assert built.returncode == 0, built.stdout + built.stderr

with tempfile.TemporaryDirectory(prefix='kcow-legacy-tools-') as temporary:
    directory = Path(temporary)
    db = directory / 'isolated.db'
    env = dict(os.environ, ConnectionStrings__DefaultConnection=f'Data Source={db};Pooling=False',
               ASPNETCORE_ENVIRONMENT='Production', DOTNET_ENVIRONMENT='Production')

    def run(assembly, *args):
        result = subprocess.run(['dotnet', str(assembly), *map(str, args)], cwd=directory,
                                env=env, capture_output=True, text=True, timeout=30)
        assert result.returncode == 0, result.stdout + result.stderr
        return result.stdout

    assemblies = {p.stem: p.parent / f'bin/Debug/net10.0/{p.stem}.dll' for p in projects}
    for assembly in assemblies.values():
        run(assembly, '--help')
        assert not db.exists(), 'Help created the database'
    api = repo / 'apps/backend/src/Api/bin/Debug/net10.0/Kcow.Api.dll'
    run(api, 'database', 'migrate')
    before = hashlib.sha256(db.read_bytes()).digest()
    for assembly in assemblies.values():
        run(assembly, '--count')
        run(assembly, '--sample', '1')
        assert hashlib.sha256(db.read_bytes()).digest() == before, 'Read command changed database'

    xml = directory / 'Activity.xml'
    xml.write_text('<dataroot><Activity><ActivityID>7</ActivityID><Program>CODE</Program>'
                   '<ProgramName>Tool contract</ProgramName></Activity></dataroot>', encoding='utf-8')
    xsd = repo / 'docs/legacy/3_Activity/Activity.xsd'
    activity = assemblies['ActivityImportRunner']
    preview = run(activity, xml, xsd, '--preview')
    assert 'Imported: 1' in preview, preview
    assert hashlib.sha256(db.read_bytes()).digest() == before
    assert not (directory / 'migration-output').exists()
    first = run(activity, xml, xsd)
    assert 'Imported: 1' in first, first
    rerun = run(activity, xml, xsd)
    assert 'Skipped: 1' in rerun, rerun
    with closing(sqlite3.connect(db)) as connection:
        assert connection.execute('SELECT id,code,name FROM activities').fetchall() == [(7, 'CODE', 'Tool contract')]
print('Four legacy tools build; help/read-only commands and activity preview/import/rerun verified')
