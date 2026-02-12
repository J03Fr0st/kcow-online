-- Create Payments table
CREATE TABLE IF NOT EXISTS "payments" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_payments" PRIMARY KEY AUTOINCREMENT,
    "student_id" INTEGER NOT NULL,
    "invoice_id" INTEGER NULL,
    "payment_date" TEXT NOT NULL,
    "amount" TEXT NOT NULL,
    "payment_method" INTEGER NOT NULL DEFAULT 0,
    "receipt_number" TEXT NOT NULL,
    "notes" TEXT NULL,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_payments_students_student_id" FOREIGN KEY ("student_id") REFERENCES "students" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_payments_invoices_invoice_id" FOREIGN KEY ("invoice_id") REFERENCES "invoices" ("id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_payments_student_id" ON "payments" ("student_id");
CREATE INDEX IF NOT EXISTS "IX_payments_invoice_id" ON "payments" ("invoice_id");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_payments_receipt_number" ON "payments" ("receipt_number");
