DROP INDEX IF EXISTS "IX_student_families_family_id";

CREATE INDEX IF NOT EXISTS "ix_student_families_family_id" ON "student_families" ("family_id");
CREATE INDEX IF NOT EXISTS "ix_student_families_student_id" ON "student_families" ("student_id");
