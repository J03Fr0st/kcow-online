-- Create ClassGroups table
CREATE TABLE IF NOT EXISTS "class_groups" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_class_groups" PRIMARY KEY AUTOINCREMENT,
    "name" TEXT NOT NULL COLLATE NOCASE,
    "day_truck" TEXT NULL COLLATE NOCASE,
    "description" TEXT NULL COLLATE NOCASE,
    "school_id" INTEGER NOT NULL,
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
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    "updated_at" TEXT NULL,
    CONSTRAINT "FK_class_groups_schools_school_id" FOREIGN KEY ("school_id") REFERENCES "schools" ("id") ON DELETE RESTRICT,
    CONSTRAINT "FK_class_groups_trucks_truck_id" FOREIGN KEY ("truck_id") REFERENCES "trucks" ("id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "ix_class_groups_school_day_time" ON "class_groups" ("school_id", "day_of_week", "start_time");
CREATE INDEX IF NOT EXISTS "IX_class_groups_truck_id" ON "class_groups" ("truck_id");
