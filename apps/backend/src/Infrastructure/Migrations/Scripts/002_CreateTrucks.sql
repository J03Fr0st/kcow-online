-- Create Trucks table
CREATE TABLE IF NOT EXISTS "trucks" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_trucks" PRIMARY KEY AUTOINCREMENT,
    "name" TEXT NOT NULL COLLATE NOCASE,
    "registration_number" TEXT NOT NULL COLLATE NOCASE,
    "status" TEXT NOT NULL COLLATE NOCASE DEFAULT 'Active',
    "notes" TEXT NULL COLLATE NOCASE,
    "is_active" INTEGER NOT NULL DEFAULT 1,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    "updated_at" TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_trucks_registration_number" ON "trucks" ("registration_number");
CREATE INDEX IF NOT EXISTS "IX_trucks_is_active" ON "trucks" ("is_active");
