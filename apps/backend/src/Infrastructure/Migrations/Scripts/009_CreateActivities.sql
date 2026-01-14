-- Create Activities table
CREATE TABLE IF NOT EXISTS "activities" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_activities" PRIMARY KEY,
    "code" TEXT NULL COLLATE NOCASE,
    "name" TEXT NULL COLLATE NOCASE,
    "description" TEXT NULL COLLATE NOCASE,
    "folder" TEXT NULL COLLATE NOCASE,
    "grade_level" TEXT NULL COLLATE NOCASE,
    "icon" TEXT NULL,
    "is_active" INTEGER NOT NULL DEFAULT 1,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    "updated_at" TEXT NULL
);

CREATE INDEX IF NOT EXISTS "IX_activities_code" ON "activities" ("code");
