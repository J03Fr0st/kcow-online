-- Create AuditLog table for tracking entity changes
-- Provides audit trail for attendance corrections and other entity modifications (FR14)
CREATE TABLE IF NOT EXISTS "audit_log" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_audit_log" PRIMARY KEY AUTOINCREMENT,
    "entity_type" TEXT NOT NULL,
    "entity_id" INTEGER NOT NULL,
    "field" TEXT NOT NULL,
    "old_value" TEXT NULL,
    "new_value" TEXT NULL,
    "changed_by" TEXT NOT NULL,
    "changed_at" TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS "IX_audit_log_entity" ON "audit_log" ("entity_type", "entity_id");
CREATE INDEX IF NOT EXISTS "IX_audit_log_changed_at" ON "audit_log" ("changed_at");
