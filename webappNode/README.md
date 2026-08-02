# webappNode — backend API

Express + MongoDB API for the wheel job card web app. It reads and writes the
**same `wheel` database the desktop `GeorgesWheel.exe` uses**, with the same
field names and BSON types, so both applications can run against one dataset.

## Run

```bash
npm install
cp .env.example .env    # optional — every value has a working default
npm run dev             # tsx watch, http://localhost:3000
npm run build && npm start
```

Requires a MongoDB reachable at `MONGO_URL` (default `mongodb://localhost:27017`).
The server checks the connection on startup and exits with a clear message rather
than failing on the first request.

## Configuration

| Variable                 | Default                     | Purpose                                        |
| ------------------------ | --------------------------- | ---------------------------------------------- |
| `MONGO_URL`              | `mongodb://localhost:27017` | Mongo connection string                        |
| `MONGO_DB`               | `wheel`                     | Job database — wheel mode always uses `wheel`  |
| `MONGO_SETTINGS_DB`      | `settings`                  | Shared settings / sent Xero invoices           |
| `PORT`                   | `3000`                      | Listen port                                    |
| `CORS_ORIGIN`            | `http://localhost:4200`     | Comma-separated allowed browser origins        |
| `AUTH_SECRET`†           | `change-me-in-production`   | Signs session tokens — **set this in prod**    |
| `AUTH_USERNAME`†         | `george`                    | The single allowed operator account            |
| `AUTH_PASSWORD_SHA256`†  | (baked-in digest)           | Salted SHA-256 of the password                 |
| `LOGIN_FAILURE_DELAY_MS` | `3000`                      | Delay before answering a failed login          |
| `SESSION_TTL_MS`         | 12 hours                    | Token lifetime                                 |
| `PHOTO_ROOT`†            | `K:\` on Windows, else off  | Root of the shared photo drive                 |
| `MAX_PHOTO_BYTES`        | 25 MB                       | Largest accepted upload                        |
| `PHOTO_BACKUP_ENABLED`   | `true`                      | Also back up new photos to MongoDB             |
| `PHOTO_BACKUP_COMPRESS_THRESHOLD_BYTES` | 14 MB       | Compress only if the base64 form would exceed this |

† can also be stored in `settings.settings` — see below.

To change the password, hash the new one and set `AUTH_PASSWORD_SHA256`:

```bash
node -e "const c=require('crypto');console.log(c.createHash('sha256').update('jobcard-wheel:'+process.argv[1]).digest('hex'))" 'the-new-password'
```

### Settings that can live in MongoDB instead

`PHOTO_ROOT`, `AUTH_SECRET`, `AUTH_PASSWORD_SHA256` and `AUTH_USERNAME` are also
read from the shared `settings.settings` document, using the same field names in
upper case. That keeps the whole configuration in one place instead of a `.env`
file on the host, and the desktop app maps the same fields (it never reads them).

Precedence is **environment → MongoDB → built-in default**, so an environment
variable always wins. That ordering is deliberate: it is the escape hatch when
Mongo is unreachable, or when a bad value in the database would otherwise lock
everyone out.

Values are read once at startup, so changing one means restarting the server.

```js
// in mongosh, against the settings database
db.settings.updateOne({}, { $set: {
  AUTH_SECRET: '<a long random string>',
  AUTH_PASSWORD_SHA256: '<digest from the command above>'
}})
```

> Writing new fields to `settings.settings` requires the **rebuilt desktop app**
> (the one with `[BsonIgnoreExtraElements]` on its document classes). An older
> build throws on any field it does not declare, which breaks its email settings,
> pricing and Xero all at once. Deploy the new `.exe` first.

The server prints a warning at startup if `AUTH_SECRET` or `AUTH_PASSWORD_SHA256`
is still the built-in default. Both defaults are in the repository, so a server
reachable from the internet with those values can be logged into by anyone, and
its session tokens can be forged outright. Set them before exposing the app.

## Authentication

There is one account. `POST /api/auth/login` compares the username and a salted
SHA-256 of the password in constant time; the plaintext password is not stored in
the repository. A correct login returns an HMAC-signed token immediately. A wrong
one waits `LOGIN_FAILURE_DELAY_MS` and then answers `401 {"error":"Access denied"}`,
which slows credential guessing. Every route except `/api/health` and
`/api/auth/login` requires `Authorization: Bearer <token>`.

## Endpoints

| Method   | Path                        | Notes                                              |
| -------- | --------------------------- | -------------------------------------------------- |
| `GET`    | `/api/health`               | Liveness + which database is in use (public)        |
| `POST`   | `/api/auth/login`           | Sign in (public)                                    |
| `GET`    | `/api/auth/me`              | Confirm a stored token is still valid               |
| `GET`    | `/api/jobs`                 | `view`, `field`, `q`, `page`, `pageSize`             |
| `GET`    | `/api/jobs/latest`          | Highest job number                                  |
| `GET`    | `/api/jobs/:id`             | One job                                             |
| `GET`    | `/api/jobs/:id/neighbours`  | Previous / next job numbers                         |
| `POST`   | `/api/jobs`                 | New job — next number, today, disclaimer in notes   |
| `POST`   | `/api/jobs/:id/duplicate`   | Copy the customer details onto a new job            |
| `PUT`    | `/api/jobs/:id`             | Save; totals are recomputed server-side             |
| `DELETE` | `/api/jobs/:id`             | Delete a job                                        |
| `GET`    | `/api/job-types`            | Wheel job type catalogue with live prices           |
| `PUT`    | `/api/job-types/:control`   | Override a price and/or caption                     |
| `GET`    | `/api/customers/fussy`      | Is this phone/email flagged?                        |
| `POST`   | `/api/customers/fussy`      | Flag a customer                                     |
| `DELETE` | `/api/customers/fussy`      | Clear the flag                                      |
| `GET`    | `/api/photos/status`        | Is the photo share reachable?                       |
| `GET`    | `/api/photos/jobs/:id`      | List a job's photos (also self-heals missing backups) |
| `GET`    | `/api/photos/jobs/:id/:name`| Fetch one photo; `?variant=thumbnail` for the 250px preview, otherwise full |
| `POST`   | `/api/photos/jobs/:id`      | Upload (raw binary body, server names the file)     |
| `DELETE` | `/api/photos/jobs/:id/:name`| Delete one photo                                    |
| `GET`    | `/api/xero/status`          | Connection state — never includes secrets or tokens |
| `POST`   | `/api/xero/mode`            | Persist `Draft` or `AuthoriseAndEmail`              |
| `POST`   | `/api/xero/connect/start`   | Begin OAuth; returns the consent URL                |
| `GET`    | `/api/xero/callback`        | Where Xero redirects the browser (**public**)       |
| `POST`   | `/api/xero/connect/complete`| Finish from a pasted redirect URL                   |
| `POST`   | `/api/xero/disconnect`      | Forget the stored tokens                            |
| `GET`    | `/api/xero/tenants`         | Xero organisations on this connection               |
| `POST`   | `/api/xero/tenant`          | Choose the organisation to invoice from             |
| `GET`    | `/api/xero/contacts`        | Candidate customers for `?businessName=`            |
| `GET`    | `/api/xero/jobs/:id/invoice`| Local invoice record + whether the job can be sent  |
| `POST`   | `/api/xero/jobs/:id/invoice`| Create the invoice (body is only `{ contactId }`)   |
| `DELETE` | `/api/xero/jobs/:id/invoice`| Delete or void it, per its live Xero status         |
| `POST`   | `/api/xero/jobs/:id/refresh`| Poll this job's paid status                          |
| `POST`   | `/api/xero/sync-unpaid`     | Re-check every unpaid invoice for the organisation   |

## Xero

Ports the desktop app's `XeroService.cs` and `XeroManagementForm.cs`. Both apps
share one Xero connection through `settings.settings`, and invoice records live in
`settings.sentInvoices` with the exact field names `SentInvoiceDoc` declares.

Configuration comes from the settings document, as it does for the desktop app:
`xeroClientId`, `xeroClientSecret`, `xeroRedirectUri`,
`xeroDefaultSalesAccountCode` (default `200`) and `xeroDefaultTaxType` (default
`OUTPUT2`).

**Nothing secret is ever sent to the browser.** The client secret and the
access/refresh tokens stay on the server; `GET /api/xero/status` returns booleans,
the organisation name, and the *names* of any fields still missing. Sending an
invoice is server-authoritative too — the browser supplies only the chosen
contact, and the order number, line items and total are derived from the stored
job document, the same way `PUT /api/jobs/:id` recomputes totals.

### Sharing the connection with the desktop app

Xero rotates the refresh token on every use and retires the old one, so two apps
refreshing at the same moment would lock one of them out. Both apps therefore take
a short lease (`xeroTokenLockUntilUtc`) before refreshing: the winner refreshes and
writes the new token to `activeXeroToken`, and the loser waits, re-reads, and uses
what it finds. If a refresh still comes back `invalid_grant`, the server re-reads
once in case the desktop app rotated the token underneath it and retries with the
new value before giving up.

### The OAuth callback

Xero requires redirect URIs to be `https`, with an exception only for
`http://localhost` and `http://127.0.0.1`. That leaves two ways to connect:

- **`xeroRedirectUri` points at this server** — e.g.
  `https://<hostname>/api/xero/callback` once HTTPS is set up, or
  `http://localhost:3000/api/xero/callback` when browsing from the server machine
  itself. `GET /api/xero/callback` completes the connection and shows a
  "you can close this tab" page. Nothing to paste.
- **It points at a localhost URL and you are on another device** (a phone). The
  tab lands on a page that cannot load, but the code is in the address bar. Paste
  that whole URL into the panel and `POST /api/xero/connect/complete` finishes the
  job. This is the desktop app's manual fallback, except the `state` is actually
  verified.

Either way it is a one-time action: the requested scopes include `offline_access`,
so the refresh token keeps the connection alive indefinitely. Switching to the
HTTPS URL later is one field edit in `settings.settings` plus registering the URI
in the Xero developer portal — Xero allows several, so the desktop app's
`http://localhost:8888/callback` keeps working alongside it.

The callback route is the only endpoint besides health and login that is not
behind the session token, because it is a top-level navigation from Xero and
cannot carry an `Authorization` header. It does nothing without a `state` this
server generated in the last ten minutes, and the code alone is useless without
the client secret.

### Paid status

`FullyPaidOnDate` on the Xero invoice is the only paid signal, as in the desktop
app. When it appears, `datePaidUtc` is set on the invoice record and the job card
gets `jobDatePaid` plus `jobPaymentBy: "Xero"` — both already existing, writable
fields. It is never cleared back to unpaid, so payment history is not silently
erased, and re-running a sync is idempotent.

The browser polls `POST /api/xero/jobs/:id/refresh` every five minutes while a job
is open, matching the desktop's `xeroSyncTimer`, and skips the tick when the tab is
not visible. `POST /api/xero/sync-unpaid` is the manual catch-up for every unpaid
invoice at once.

### Deliberate differences from the desktop app

- The requested scopes are `accounting.transactions` and `accounting.contacts`.
  The C# asks for `accounting.invoices` and `accounting.payments`, which are not
  Xero scope names, and omits `offline_access` — which is why the refresh token
  currently stored in Mongo is an unusable 8 characters.
- The freight line is described as `Freight` and included only when the amount is
  non-zero. The desktop reads its description from `txtFreightText`, a UI-only
  control with no BSON field, so the text is not persisted and cannot be ported.
- When a line has no unit price, the fallback divides the line total by the
  quantity. The desktop assigns the extended total to `UnitAmount` without
  dividing, so a qty-2 line carrying only a $100 total invoices the customer $200.
- Failures report what Xero actually said. The C# token calls discard the response
  body entirely.

## Photos

Photos are served from the **same shared drive the desktop app uses**, so both
programs see the same files. Point `PHOTO_ROOT` at that share — a drive letter on
Windows (`K:\`), or the mount point of the same share on macOS/Linux
(`/Volumes/KodakPictures`). The API process itself needs read and write access.

Nothing about photos is stored in MongoDB. The desktop app locates them purely by
path and filename, and this keeps that convention exactly
(`GetJobPictureFiles` / `SaveUniquePhoto`):

```
{PHOTO_ROOT}/{year}/{year} {MonthName}/{jobID} {business}-{customer} {phone} {details} {NNN}.jpg
                                        ^ a file belongs to the job whose ID leads its name
```

Two details worth knowing:

- **The folder comes from the job's own date, not today.** A photo for a job
  dated 1 May 2026 lives in `2026/2026 May`, which is where the desktop app looks
  (`UpdatePhotos` uses the job's date). A job with no date falls back to today.
- **Uploads are de-duplicated.** As the desktop app does, an upload whose bytes
  match a photo already filed against the job is not written again; the response
  returns `duplicateOf` naming the existing file.

The sequence number is `1 + the existing photo count`, matching `SaveUniquePhoto`,
and videos count toward it just as they do there.

Photo endpoints are behind the session token like everything else, so the browser
fetches image bytes through the API and displays them from object URLs rather than
pointing an `<img src>` at the server.

If `PHOTO_ROOT` is unset or the share is not mounted, the API says so on startup
and every photo request answers `available: false` with a reason. The rest of the
app keeps working — photos are optional, not load-bearing.

**Path safety.** A requested filename must be a bare name (no directory part),
must have a recognised photo extension, and must lead with the job's own ID; the
resolved path is then re-checked to be inside that job's folder. Traversal
attempts, other jobs' photos, and non-photo files in the same folder are all
rejected. (Verified against Windows path semantics as well as POSIX, including
UNC roots.)

### MongoDB backup copy (`jobPictures`)

The network share is a real single point of failure — see the mapped-drive trap
below, plus the hosting PC and the local network both need to be up. So every
new still photo also gets an independent, compressed copy stored directly in
MongoDB, in its own `jobPictures` collection (**not** a field on `jobCard` — the
desktop app's MongoDB driver throws on a document with a field it doesn't
declare, so a new collection it never queries keeps this completely invisible
and safe to it).

Each document: `{ jobId: ObjectId, name: string, contentHash: string, isThumbnail: boolean, base64Image: string }`.
`jobId` points at the job's own `jobCard._id` (not the human job number). `name`
is the share filename this photo derives from — the key that makes serving and
repairing fast: without it, fetching a thumbnail would still require reading
and hashing the full file from the (possibly slow) share first, defeating the
entire point of having one. `contentHash` is the same SHA-256 already computed
for share-side duplicate detection, shared by a photo's full and thumbnail
docs since both derive from the same original — deleting a photo removes both
(matched by `{jobId, name}`), so delete really does mean delete.

**Every photo gets two documents**: the full backup described below
(`isThumbnail: false`) and a 250px-wide preview (`isThumbnail: true`, scaled by
width only so the aspect ratio is preserved exactly, quality 80, generated
alongside the full version in `photo-backup.ts`). The web UI's inline photo
strip only ever fetches thumbnails — fast, a few KB each — and only fetches the
full version when a photo is actually opened in the full-screen viewer.
Content-Type on the way out is sniffed from the actual bytes (JPEG/PNG/GIF/BMP
magic numbers), not assumed to be JPEG or trusted from the filename — an
untouched full backup can be any of those formats, only a thumbnail or a
compressed full backup is guaranteed to actually be JPEG.

**Older jobs self-heal.** If a job has photos on the share but no matching
`jobPictures` docs at all — Peter's own photos added directly to the share are
a real example, and anything the desktop app captures is another, since it
never touches this collection — the very next `GET /api/photos/jobs/:id` call
creates the missing full + thumbnail docs for every still image before
responding (`photo-backup-sync.ts`). The check is per-file and idempotent, so
this costs nothing on jobs already backed up: the first view of an old job is
slower (each photo read from the share once), every view after that is a fast
Mongo-only no-op.

**Most photos are stored completely untouched** — original bytes, original
format, no quality loss — because a typical phone photo (a few MB) easily fits
under `PHOTO_BACKUP_COMPRESS_THRESHOLD_BYTES` (14 MB, checked against the
base64-encoded size). Only a genuinely oversized image gets compressed, and
even then at **full original resolution** — a wheel-damage close-up needs its
detail, and quality alone is normally enough (verified against a 9000×6248 test
image, which quality 85 alone brought to ~2.5MB). Two tiers via `sharp`:
quality 85 first, then quality 50 if that still doesn't fit (verified necessary
against a genuine worst case — pure random noise at 24 megapixels, real camera
sensor noise never comes close). Whatever quality 50 produces is used
regardless of whether it still exceeds the threshold; if the Mongo insert then
fails, it's logged as a backup miss, same as any other failure here — the share
copy, the critical path, is unaffected either way. Re-encoding auto-orients from
EXIF and drops EXIF entirely (including GPS tags) from any photo that needed
compression.

Videos are not backed up (compressing them to a small target needs
transcoding, out of scope). Backing up is best-effort throughout: the share
write is the critical, desktop-app-compatible path, and a Mongo hiccup never
fails an upload or a delete — it just logs a warning server-side, and the
upload response's `backedUp: false` says so.

Deleting a whole job (`DELETE /api/jobs/:jobID`) cascades to remove all of its
`jobPictures` documents, so nothing is orphaned. Nothing currently reads this
collection back for display — it is a write-side safety net for now.

### Windows deployment and the mapped-drive trap

If `K:` is a **mapped network drive**, read this before deploying.

A drive mapping belongs to the logon session that created it. It is not machine
wide. That gives two very different outcomes:

| How the API is started                                   | Does `K:\` resolve? |
| -------------------------------------------------------- | ------------------- |
| Manually, or Task Scheduler **"run only when user is logged on"** | Yes — it inherits the user's session |
| A **Windows Service** (nssm, node-windows, pm2-service), or Task Scheduler as `SYSTEM` | **No** — session 0 has no drive mappings |

The desktop `GeorgesWheel.exe` never hits this because it always runs
interactively as the signed-in user. A service does not, and the failure is quiet:
the path simply does not exist.

**Use the UNC path instead of the drive letter.** It does not depend on a mapping,
so it works in both cases:

```ini
PHOTO_ROOT=\\WORKSHOPPC\Kodak Pictures
```

Find the right value by running `net use` on the machine that has `K:` mapped —
the "Remote" column is the UNC path you want. (`net share` on the PC hosting the
files lists them from the other end.) Backslashes need no escaping in `.env`, and
quoting is optional; both forms parse correctly.

The server prints a warning at startup if `PHOTO_ROOT` is a drive letter on
Windows, so this does not go unnoticed.

**If you do run it as a service, also sort out credentials.** A service running as
`LocalSystem` authenticates to the network as the *computer* account
(`WORKGROUP\PCNAME$`). On a workgroup network — which a small business almost
certainly is, rather than a domain — the file-hosting PC will not recognise that
account and will refuse the share. Fix it by configuring the service to log on as
a specific local user that exists with the **same username and password on both
machines**, and grant that user read/write on the share.

Checklist for the deployment:

1. Both machines on, share reachable, and the hosting PC's firewall allows file
   sharing on the local network.
2. `PHOTO_ROOT` set to the UNC path, not `K:\`.
3. The account the API runs under has **read *and* write** on the share (uploads
   create the `{year}\{year} {Month}` folder if it is missing).
4. Startup log says `photo share ready at …` rather than `photos unavailable: …`.
5. `GET /api/photos/status` confirms the same thing over HTTP.

Note the desktop app and the web app must point at the *same physical location*.
If `K:` on the front-desk PC and `PHOTO_ROOT` on the API host resolve to different
shares, each app will only see its own photos.

### HTTPS and internet access

`ng serve` on port 4200 is a **development** server. Exposing it to the internet
over plain HTTP means the password and the session token cross the open network in
clear text on every sign-in, readable by anyone on the path — the user's home
WiFi, either ISP. Put Caddy in front instead: it serves the built Angular app,
proxies the API, and gets a real certificate by itself.

You do not need to buy a certificate, and you do not need to configure your own
domain.

**1. A hostname.** Let's Encrypt issues certificates for names, not bare IP
addresses, so the public IP alone will not do. Two free options that avoid touching
your own DNS:

- **DuckDNS** — register `something.duckdns.org` and point it at the office IP.
  One form, no DNS knowledge needed.
- **sslip.io** — no registration at all. `1-2-3-4.sslip.io` resolves to `1.2.3.4`;
  substitute the office address and it just works.

Either is fine. DuckDNS is the more established of the two, and it survives the IP
changing; both depend on a third-party DNS service staying up.

**2. Router ports.** Forward **80** and **443** to the machine running Node and
Mongo. Port 80 is needed for the certificate challenge, not just redirects.
**Remove the 4200 forward** — that is the whole point.

**3. Build the front end.** From the repo root:

```bash
cd webappShared && npm run build
cd ../webappUI && npm run build      # -> dist/webapp-ui/browser
```

**4. Caddy.** Copy `Caddyfile.example` to `Caddyfile`, set the hostname and the
path to `dist/webapp-ui/browser`, and run `caddy run`. Once it works, install it as
a service with `caddy service install` so it survives a reboot. Unlike
`PHOTO_ROOT`, Caddy needs no drive mappings, so the session-0 trap above does not
apply to it.

**5. Point the app at itself.** Set `CORS_ORIGIN=https://<hostname>`. Behind Caddy
the browser and API share an origin so CORS never comes into play, but the value
should still be right. Node keeps listening on `localhost:3000` and no longer needs
to be reachable from the network directly.

**6. Xero.** Register `https://<hostname>/api/xero/callback` in the Xero developer
portal and set `xeroRedirectUri` in `settings.settings` to match. Xero accepts
several redirect URIs, so the desktop app's `http://localhost:8888/callback` keeps
working. After this the Connect button completes on any device with no paste step.

**7. Update the bookmark** from `x.x.x.x:4200` to `https://<hostname>`.

A note on one thing that can go wrong: some routers block "DNS rebinding", where a
public hostname resolves to a private address. If the site loads from outside the
office but not from inside it, that is the cause — whitelist the hostname in the
router's DNS settings.

Worth doing at the same time: **MongoDB has no authentication enabled**, and it
listens on the network so the desktop app can reach it. Anyone on the office LAN
can read every job, and now the settings document too. That is unrelated to HTTPS
and needs its own fix (`security.authorization: enabled` plus a user for each app),
but it is the next thing on the list after this.

## Data safety

- **Field whitelist.** `job-fields.ts` lists every writable field and its stored
  type. Anything else in a request body is ignored, and values are coerced to the
  types the desktop app writes (blank becomes `null`, as the desktop does when
  clearing a field).
- **Totals are authoritative on the server.** `PUT` discards any client-supplied
  `jobTOTAL` / `jobGST` / `jobSubTotal` and recomputes them from the saved lines,
  so the stored money can never disagree with the line items.
- **Regex input is escaped** before being used in a search filter.
- **The job list never loads the whole collection.** `GET /api/jobs` always applies
  `.skip().limit()`, `pageSize` is capped at 200 server-side however large a value
  the client asks for, and the rows are projected down to just the fields the list
  renders. Verified against 10,000 seeded jobs: every view examines ~25–49
  documents per request, and that holds on the last page as much as the first,
  because `skip` walks the index rather than the documents. Dropping `pageSize` and
  fetching everything would be about 22MB; one page is under 200KB.

  All four views sort off an index, which is what keeps that true — `jobDate` and
  `jobID` were already indexed by the desktop app, and `db.ts` adds
  `{ jobDateCompleted: -1 }` and `{ jobDatePaid: 1, jobDateCompleted: -1 }` for the
  "Completed jobs" and "Unpaid customers" views, which otherwise scanned the whole
  collection on every request. Check with `explain('executionStats')` and look for
  `IXSCAN`/`SORT_MERGE` rather than `SORT <- COLLSCAN` if a new view is added.
- **Job numbers.** The desktop app takes the highest `jobID` and adds one, and
  `insertWithNextJobId` does the same, retrying on a duplicate-key error rather
  than handing out a wrong number.

  **That retry only actually protects anything if the `jobID` index is unique, and
  on the databases checked so far it is not.** `db.ts` ensures a plain
  (non-unique) `{ jobID: 1 }` index at startup — that keeps job lookups off a
  collection scan even if the desktop app's compound indexes are ever dropped, but
  a non-unique index never raises the duplicate-key error the retry is waiting for.
  Two simultaneous "New job" clicks could therefore both take the same number.

  Making it unique is the real fix, but the index will fail to build if the
  collection already contains duplicates — quite possible in years of production
  data. Check first:

  ```js
  db.jobCard.aggregate([
    { $group: { _id: '$jobID', n: { $sum: 1 } } },
    { $match: { n: { $gt: 1 } } }
  ])
  ```

  If that returns nothing, `db.jobCard.createIndex({ jobID: 1 }, { unique: true })`
  after dropping the non-unique one closes the race. Startup tolerates finding a
  unique index already there and stays quiet about it.

## GST convention

Matching `JobCard.UpdateAllTotals()` and the existing stored documents: line
prices are **GST exclusive**, and

- `jobTOTAL` = sum of line prices (excluding GST)
- `jobGST` = 15% of that
- `jobSubTotal` = the grand total including GST — the amount to pay

Rounding is half-away-from-zero to 2dp, as .NET's `MidpointRounding.AwayFromZero`.

## Not carried over

These desktop features are still absent: Word/RTF printing (the web app uses the
browser's print dialog with an A4 stylesheet) and SMTP email. Xero **is** ported
now — see the Xero section above.

Photos *are* supported — see above — but capture works differently: instead of
driving a DirectShow webcam, the browser's file input with `capture="environment"`
opens the phone camera or a desktop file picker.
