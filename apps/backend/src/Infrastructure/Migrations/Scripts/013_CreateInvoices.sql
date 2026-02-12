-- Create Invoices table
CREATE TABLE IF NOT EXISTS "invoices" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_invoices" PRIMARY KEY AUTOINCREMENT,
    "student_id" INTEGER NOT NULL,
    "invoice_date" TEXT NOT NULL,
    "amount" TEXT NOT NULL,
    "due_date" TEXT NOT NULL,
    "status" INTEGER NOT NULL DEFAULT 0,
    "description" TEXT NULL,
    "notes" TEXT NULL,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_invoices_students_student_id" FOREIGN KEY ("student_id") REFERENCES "students" ("id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_invoices_student_id" ON "invoices" ("student_id");
CREATE INDEX IF NOT EXISTS "IX_invoices_status" ON "invoices" ("status");
CREATE INDEX IF NOT EXISTS "IX_invoices_due_date" ON "invoices" ("due_date");
