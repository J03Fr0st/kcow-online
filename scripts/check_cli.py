"""Exercise the real CLI entry point using disposable paths, never the configured DB."""
from pathlib import Path
import os
import subprocess
import tempfile

repo = Path(__file__).resolve().parents[1]
api = repo / 'apps/backend/src/Api/bin/Debug/net10.0/Kcow.Api.dll'
for flags in (['--help'], ['--preview']):
    with tempfile.TemporaryDirectory(prefix='kcow-preview-') as directory:
        db = Path(directory) / 'must-not-exist.db'
        env = dict(os.environ, ConnectionStrings__DefaultConnection=f'Data Source={db};Pooling=False',
                   ASPNETCORE_ENVIRONMENT='Production', DOTNET_ENVIRONMENT='Production')
        result = subprocess.run(['dotnet', str(api), 'import', 'run', *flags, '--input', directory],
                                cwd=directory, env=env, text=True, capture_output=True, timeout=30)
        assert result.returncode == 0, result.stdout + result.stderr
        assert not db.exists(), f'{flags}: preview/help created a database'
print('CLI preview and help: no database created')
