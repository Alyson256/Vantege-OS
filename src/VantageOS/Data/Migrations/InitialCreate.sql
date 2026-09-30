CREATE TABLE IF NOT EXISTS profiles (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    name        TEXT NOT NULL,
    slot        INTEGER,
    tweak_ids   TEXT NOT NULL,
    created_at  TEXT DEFAULT (datetime('now')),
    updated_at  TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS action_history (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    action_type     TEXT NOT NULL,
    tweak_id        TEXT,
    previous_value  TEXT,
    applied_value   TEXT,
    timestamp       TEXT DEFAULT (datetime('now')),
    rolled_back     INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS user_preferences (
    key     TEXT PRIMARY KEY,
    value   TEXT NOT NULL
);
