-- Operational command receipts; legacy XSD entities are unchanged.
-- Keys have no automatic expiry: deleting them can make an old retry execute again.
CREATE TABLE billing_commands (
    actor TEXT NOT NULL,
    operation TEXT NOT NULL,
    command_key TEXT NOT NULL,
    request_hash TEXT NOT NULL,
    result_json TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    PRIMARY KEY (actor, operation, command_key)
);
