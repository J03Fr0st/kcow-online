-- Create Families table
CREATE TABLE IF NOT EXISTS "families" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_families" PRIMARY KEY AUTOINCREMENT,
    "family_name" TEXT NOT NULL COLLATE NOCASE,
    "primary_contact_name" TEXT NOT NULL COLLATE NOCASE,
    "phone" TEXT NULL COLLATE NOCASE,
    "email" TEXT NULL COLLATE NOCASE,
    "address" TEXT NULL COLLATE NOCASE,
    "notes" TEXT NULL COLLATE NOCASE,
    "is_active" INTEGER NOT NULL DEFAULT 1,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    "updated_at" TEXT NULL
);
