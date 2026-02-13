CREATE TABLE IF NOT EXISTS import_audit_log (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    started_at TEXT NOT NULL,
    completed_at TEXT,
    run_by TEXT NOT NULL DEFAULT 'system',
    source_path TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'InProgress',
    schools_created INTEGER NOT NULL DEFAULT 0,
    class_groups_created INTEGER NOT NULL DEFAULT 0,
    activities_created INTEGER NOT NULL DEFAULT 0,
    students_created INTEGER NOT NULL DEFAULT 0,
    total_failed INTEGER NOT NULL DEFAULT 0,
    total_skipped INTEGER NOT NULL DEFAULT 0,
    exceptions_file_path TEXT,
    notes TEXT
);
