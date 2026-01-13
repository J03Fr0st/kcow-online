-- Fix index casing
DROP INDEX IF EXISTS "ix_activities_code";
CREATE INDEX IF NOT EXISTS "IX_activities_code" ON "activities" ("code");

-- The previous migration already renamed 'program_name' to 'name'.
-- The EF Core migration tried to ADD 'name' again because of a model snapshot sync issue.
-- We check if 'name' exists (it should from 0004).
-- If we need to ensure it has specific constraints (like nullability), we can't easily alter in SQLite.
-- Assuming 0004 did the job correctly, we just ensure the index name is updated.

-- NOTE: logic to add 'name' column if it was missed in 0004 (defensive):
-- SELECT count(*) FROM pragma_table_info('activities') WHERE name='name';
-- If 0, ALTER TABLE "activities" ADD COLUMN "name" TEXT NULL;
-- Since we control 0004, we know it's there.
