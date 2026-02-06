-- Create Attendance table for tracking student attendance per session
CREATE TABLE IF NOT EXISTS "attendance" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_attendance" PRIMARY KEY AUTOINCREMENT,
    "student_id" INTEGER NOT NULL,
    "class_group_id" INTEGER NOT NULL,
    "session_date" TEXT NOT NULL,
    "status" INTEGER NOT NULL DEFAULT 0,
    "notes" TEXT NULL,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    "modified_at" TEXT NULL,
    CONSTRAINT "FK_attendance_students_student_id" FOREIGN KEY ("student_id") REFERENCES "students" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_attendance_class_groups_class_group_id" FOREIGN KEY ("class_group_id") REFERENCES "class_groups" ("id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_attendance_student_id" ON "attendance" ("student_id");
CREATE INDEX IF NOT EXISTS "IX_attendance_session_date" ON "attendance" ("session_date");
CREATE INDEX IF NOT EXISTS "IX_attendance_class_group_id" ON "attendance" ("class_group_id");
CREATE INDEX IF NOT EXISTS "IX_attendance_student_date" ON "attendance" ("student_id", "session_date");
