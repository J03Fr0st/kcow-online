-- Create Receipts table for receipt number tracking
CREATE TABLE IF NOT EXISTS "receipts" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_receipts" PRIMARY KEY AUTOINCREMENT,
    "payment_id" INTEGER NOT NULL,
    "receipt_number" TEXT NOT NULL,
    "issued_date" TEXT NOT NULL DEFAULT (datetime('now')),
    CONSTRAINT "FK_receipts_payments_payment_id" FOREIGN KEY ("payment_id") REFERENCES "payments" ("id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_receipts_receipt_number" ON "receipts" ("receipt_number");
CREATE INDEX IF NOT EXISTS "IX_receipts_payment_id" ON "receipts" ("payment_id");
