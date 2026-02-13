-- Add legacy_id column to entities for re-import matching
ALTER TABLE schools ADD COLUMN legacy_id TEXT;
ALTER TABLE class_groups ADD COLUMN legacy_id TEXT;
ALTER TABLE activities ADD COLUMN legacy_id TEXT;
ALTER TABLE students ADD COLUMN legacy_id TEXT;

-- Create unique indexes for conflict detection (NULL values excluded from uniqueness)
CREATE UNIQUE INDEX IF NOT EXISTS idx_schools_legacy_id ON schools(legacy_id) WHERE legacy_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_class_groups_legacy_id ON class_groups(legacy_id) WHERE legacy_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_activities_legacy_id ON activities(legacy_id) WHERE legacy_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_students_legacy_id ON students(legacy_id) WHERE legacy_id IS NOT NULL;
