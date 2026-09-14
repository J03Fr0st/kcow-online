"""Create a consistent, integrity-checked SQLite backup without replacing any file."""
import argparse
from contextlib import closing
from pathlib import Path
import sqlite3


def backup(source: Path, destination: Path) -> None:
    source = source.resolve(strict=True)
    destination = destination.resolve()
    if source == destination:
        raise ValueError('Backup must use a different file')
    # Exclusive creation protects existing backups and provides clear failure on collisions.
    with destination.open('xb'):
        pass
    try:
        with closing(sqlite3.connect(source.as_uri() + '?mode=ro', uri=True)) as original:
            with closing(sqlite3.connect(destination)) as copy:
                original.backup(copy)
                if copy.execute('PRAGMA integrity_check').fetchone() != ('ok',):
                    raise RuntimeError('Backup failed integrity check')
    except Exception:
        # Only the newly created destination belongs to this invocation.
        destination.unlink(missing_ok=True)
        raise


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('destination', type=Path)
    args = parser.parse_args()
    backup(args.source, args.destination)
    print(f'Backup verified: {args.destination.resolve()}')
