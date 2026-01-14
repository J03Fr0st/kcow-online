-- Create StudentFamilies join table
CREATE TABLE IF NOT EXISTS "student_families" (
    "student_id" INTEGER NOT NULL,
    "family_id" INTEGER NOT NULL,
    "relationship_type" TEXT NOT NULL COLLATE NOCASE,
    CONSTRAINT "PK_student_families" PRIMARY KEY ("student_id", "family_id"),
    CONSTRAINT "FK_student_families_students_student_id" FOREIGN KEY ("student_id") REFERENCES "students" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_student_families_families_family_id" FOREIGN KEY ("family_id") REFERENCES "families" ("id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "ix_student_families_student_id" ON "student_families" ("student_id");
CREATE INDEX IF NOT EXISTS "ix_student_families_family_id" ON "student_families" ("family_id");
