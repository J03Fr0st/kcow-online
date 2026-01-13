-- Rename columns to match English field names
ALTER TABLE "activities" RENAME COLUMN "program" TO "code";
ALTER TABLE "activities" RENAME COLUMN "program_name" TO "name";
ALTER TABLE "activities" RENAME COLUMN "educational_focus" TO "description";
ALTER TABLE "activities" RENAME COLUMN "grade" TO "grade_level";

-- SQLite does not support altering column types directly.
-- We need to recreate the table or just treat the BLOB as TEXT (SQLite is flexible with types).
-- Since existing data might be BLOB, we'll leave the type definition flexible but conceptually treat it as TEXT.
-- For a strict migration, we would create a new table, copy data, drop old, rename new.
-- Given this is dev/test data mostly, we will assume no binary data exists yet or it can be cast.

-- Add new columns
ALTER TABLE "activities" ADD COLUMN "is_active" INTEGER NOT NULL DEFAULT 1;
ALTER TABLE "activities" ADD COLUMN "created_at" TEXT NOT NULL DEFAULT (datetime('now'));
ALTER TABLE "activities" ADD COLUMN "updated_at" TEXT NULL;

-- Create index on Code
CREATE INDEX IF NOT EXISTS "ix_activities_code" ON "activities" ("code");
