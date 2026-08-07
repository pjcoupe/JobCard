---
name: mongo
description: Query the jobcard MongoDB databases (wheel, plating, settings) read-only to inspect real data — job cards, jobPictures photo backups, pricing, settings, sent Xero invoices. Use when diagnosing behaviour that depends on what is actually stored, verifying a schema or field name, or checking whether documents/indexes exist. Triggers on "check the database", "what's in mongo", "look at job #N", "does that collection have…", or any question about live data rather than code.
---

# Querying the jobcard MongoDB

Read-only access to the live Mongo server via `query.mjs` in this directory. It
reuses the driver in `webappNode/node_modules` and the connection details in
`webappNode/.env` (`MONGO_IP` / `MONGO_PORT`), so nothing needs installing and
it always points at the same server the app does.

Run it from the repo root:

```bash
node .claude/skills/mongo/query.mjs --dbs
```

## Databases

| Database   | Contents |
|------------|----------|
| `wheel`    | GeorgesWheel job cards — `jobCard`, `jobPictures`, `pricing`, `fussyCustomer` |
| `plating`  | Advanced Chrome Platers, same collections |
| `settings` | Shared by both — `settings`, `sentInvoices` |

## Commands

```bash
# List databases and their collections
node .claude/skills/mongo/query.mjs --dbs

# Find documents (default limit 5)
node .claude/skills/mongo/query.mjs wheel jobCard '{"jobID":10427}'

# Project just the fields you care about — job docs are ~3KB each
node .claude/skills/mongo/query.mjs wheel jobCard '{"jobID":10427}' \
  --projection '{"jobID":1,"jobDate":1,"jobCustomer":1,"jobDateCompleted":1}'

# Count, sort, limit
node .claude/skills/mongo/query.mjs wheel jobPictures '{}' --count
node .claude/skills/mongo/query.mjs wheel jobCard '{}' --sort '{"jobID":-1}' --limit 3

# Distinct values
node .claude/skills/mongo/query.mjs wheel jobPictures '{}' --distinct isThumbnail

# Anything else — an async expression with `client` and `db(name)` in scope
node .claude/skills/mongo/query.mjs --eval "db('wheel').collection('jobCard').countDocuments({jobDateCompleted:null})"
node .claude/skills/mongo/query.mjs --eval "db('wheel').collection('jobPictures').indexes()"
```

## Output handling

Strings longer than 200 characters are truncated with a `…[N chars total]`
marker — this matters for `jobPictures.base64Image`, which holds whole images
and would otherwise flood the output. Pass `--maxString 0` to disable, but
prefer a projection that excludes the field. `ObjectId`s print as
`ObjectId(hex)`, dates as ISO strings, binaries as a byte count.

## Schema notes worth knowing

- `jobCard.jobID` is a **number**, not a string — a `$regex` filter can never
  match it.
- `jobPictures` documents are `{ jobId, name, contentHash, isThumbnail,
  base64Image }` where `jobId` is the **`_id` ObjectId of the job card**, not
  the numeric `jobID`. Two documents per photo: `isThumbnail: false` (full) and
  `isThumbnail: true` (250px preview). Written only by the web app — the desktop
  app never touches this collection.
- Photo *files* live on the share at `PHOTO_ROOT/{year}/{year} {MonthName}/`,
  named `{jobID} {business}-{customer} {phone} {details} {NNN}{ext}`.
  `jobPictures` is a backup of those, not the source of truth.

## Rules

- **Read-only.** Do not use `--eval` to insert, update, delete, drop, or create
  indexes. This points at live production data for a running business. If a
  write genuinely looks necessary, describe it and let the user run it.
- Always project or limit before reading `jobPictures` or whole job documents.
