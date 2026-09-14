"""Check production liveness/readiness, migration, backup and restore on disposable data."""
from contextlib import closing
from pathlib import Path
import os
import socket
import sqlite3
import subprocess
import tempfile
import time
import urllib.error
import urllib.request
from backup_sqlite import backup

repo = Path(__file__).resolve().parents[1]
api = repo / 'apps/backend/src/Api/bin/Debug/net10.0/Kcow.Api.dll'


def status(url):
    try:
        with urllib.request.urlopen(url, timeout=2) as response:
            return response.status
    except urllib.error.HTTPError as error:
        return error.code


def probe(directory, env, expected):
    with socket.socket() as port_socket:
        port_socket.bind(('127.0.0.1', 0))
        port = port_socket.getsockname()[1]
    url = f'http://127.0.0.1:{port}'
    with (directory / 'host.log').open('w') as log:
        process = subprocess.Popen(['dotnet', str(api), '--urls', url], cwd=directory,
                                   env=env, stdout=log, stderr=subprocess.STDOUT,
                                   creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
        try:
            for _ in range(150):
                if process.poll() is not None:
                    raise RuntimeError((directory / 'host.log').read_text())
                try:
                    if status(url + '/health') == 200:
                        break
                except (urllib.error.URLError, TimeoutError):
                    time.sleep(0.1)
            else:
                raise RuntimeError('Host did not become live')
            assert status(url + '/health/ready') == expected
        finally:
            process.terminate()
            try:
                process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait(timeout=5)


with tempfile.TemporaryDirectory(prefix='kcow-operations-') as temporary:
    directory = Path(temporary)
    db = directory / 'application.db'
    env = dict(os.environ, ConnectionStrings__DefaultConnection=f'Data Source={db};Pooling=False',
               ASPNETCORE_ENVIRONMENT='Production', DOTNET_ENVIRONMENT='Production',
               Jwt__Key='isolated-test-key-' + 'x' * 48, Jwt__Issuer='kcow-api', Jwt__Audience='kcow-frontend')
    probe(directory, env, 503)
    assert not db.exists(), 'Production startup/readiness must not create a database'
    for _ in range(2):
        result = subprocess.run(['dotnet', str(api), 'database', 'migrate'], cwd=directory,
                                env=env, capture_output=True, text=True, timeout=30)
        assert result.returncode == 0, result.stdout + result.stderr
    with closing(sqlite3.connect(db)) as connection:
        assert connection.execute('SELECT COUNT(*) FROM users').fetchone()[0] == 0
        assert connection.execute('SELECT COUNT(*) FROM SchemaVersions').fetchone()[0] >= 19
    probe(directory, env, 200)
    snapshot = directory / 'backup.db'
    backup(db, snapshot)
    try:
        backup(db, snapshot)
        raise AssertionError('Existing backup was overwritten')
    except FileExistsError:
        pass
    restored = directory / 'restored.db'
    backup(snapshot, restored)
    restored_env = dict(env, ConnectionStrings__DefaultConnection=f'Data Source={restored};Pooling=False')
    probe(directory, restored_env, 200)
print('Production probes, migration rerun, no auth seed, backup and restore verified')
