-- Create Evaluation table for tracking student progress evaluations
CREATE TABLE IF NOT EXISTS "evaluations" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_evaluations" PRIMARY KEY AUTOINCREMENT,
    "student_id" INTEGER NOT NULL,
    "activity_id" INTEGER NOT NULL,
    "evaluation_date" TEXT NOT NULL,
    "score" INTEGER NULL,
    "speed_metric" REAL NULL,
    "accuracy_metric" REAL NULL,
    "notes" TEXT NULL,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    "modified_at" TEXT NULL,
    CONSTRAINT "FK_evaluations_students_student_id" FOREIGN KEY ("student_id") REFERENCES "students" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_evaluations_activities_activity_id" FOREIGN KEY ("activity_id") REFERENCES "activities" ("id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_evaluations_student_id" ON "evaluations" ("student_id");
CREATE INDEX IF NOT EXISTS "IX_evaluations_activity_id" ON "evaluations" ("activity_id");
CREATE INDEX IF NOT EXISTS "IX_evaluations_evaluation_date" ON "evaluations" ("evaluation_date");
CREATE INDEX IF NOT EXISTS "IX_evaluations_student_activity" ON "evaluations" ("student_id", "activity_id");
