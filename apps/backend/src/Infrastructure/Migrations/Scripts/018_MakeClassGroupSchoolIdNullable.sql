-- Make school_id nullable on class_groups for legacy records with missing/invalid school references.
-- SQLite doesn't support ALTER COLUMN, so we recreate the table.

PRAGMA foreign_keys = OFF;

CREATE TABLE IF NOT EXISTS "class_groups_new" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_class_groups" PRIMARY KEY AUTOINCREMENT,
    "name" TEXT NOT NULL COLLATE NOCASE,
    "day_truck" TEXT NULL COLLATE NOCASE,
    "description" TEXT NULL COLLATE NOCASE,
    "school_id" INTEGER NULL,
    "truck_id" INTEGER NULL,
    "day_of_week" INTEGER NOT NULL,
    "start_time" TEXT NOT NULL,
    "end_time" TEXT NOT NULL,
    "sequence" INTEGER NOT NULL DEFAULT 1,
    "evaluate" INTEGER NOT NULL DEFAULT 0,
    "notes" TEXT NULL COLLATE NOCASE,
    "import_flag" INTEGER NOT NULL DEFAULT 0,
    "group_message" TEXT NULL COLLATE NOCASE,
    "send_certificates" TEXT NULL COLLATE NOCASE,
    "money_message" TEXT NULL COLLATE NOCASE,
    "ixl" TEXT NULL COLLATE NOCASE,
    "is_active" INTEGER NOT NULL DEFAULT 1,
    "legacy_id" TEXT NULL COLLATE NOCASE,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    "updated_at" TEXT NULL,
    CONSTRAINT "FK_class_groups_schools_school_id" FOREIGN KEY ("school_id") REFERENCES "schools" ("id") ON DELETE SET NULL,
    CONSTRAINT "FK_class_groups_trucks_truck_id" FOREIGN KEY ("truck_id") REFERENCES "trucks" ("id") ON DELETE SET NULL
);

INSERT INTO "class_groups_new" SELECT * FROM "class_groups";

DROP TABLE "class_groups";

ALTER TABLE "class_groups_new" RENAME TO "class_groups";

CREATE INDEX IF NOT EXISTS "ix_class_groups_school_day_time" ON "class_groups" ("school_id", "day_of_week", "start_time");
CREATE INDEX IF NOT EXISTS "IX_class_groups_truck_id" ON "class_groups" ("truck_id");
CREATE INDEX IF NOT EXISTS "IX_class_groups_legacy_id" ON "class_groups" ("legacy_id");

PRAGMA foreign_keys = ON;
