# webappUI — front end

Angular 19 single-page app for the job card system. Responsive by design:
the same screens are meant to be usable on a phone in the workshop and on a
desktop at the counter.

## Wheel or plating

The sign-in screen carries radio buttons for the two businesses, because one
deployment serves both databases where the desktop app needs a separate
executable per business. The choice:

- is sent with the credentials and **signed into the session token** by the
  server, which is what every later request is read against — see
  "Wheel and plating" in `webappNode/README.md`;
- is remembered in `localStorage` and pre-selected next time, including after
  signing out, since a workshop signs in to the same business every day;
- shows as a chip beside the title in the app bar on every screen. A job number
  exists in both databases, so nothing else on the page would tell them apart;
- cannot be changed without signing in again. There is no in-app switcher on
  purpose.

The job type picker follows the same choice, in two quite different ways —
both ported from `doCheckChange` in `JobTypePopup.cs`:

- **Wheel** — one type per line. Picking writes the group name into the line's
  detail and the caption into its type, steps the quantity up by one and prices
  the line. The sheet closes on the pick.
- **Plating** — one line holds the whole sequence of processes, so picking
  *adds* to the line's type field: `Strip, Polish, (2x)Nickle, Chrome`. Picking
  something already there bumps its count. Detail, quantity and price are left
  alone — they describe the items being plated, not the processes, and are
  filled in by hand. The sheet stays open, showing what is on the line as chips,
  with **Clear line** (the popup's CLEAR, which empties the whole line) and
  **Done**.

The parsing and formatting of that combined field lives in `webappShared`
(`parsePlatingTypes` / `addPlatingType`) so the string is byte-identical to what
the desktop writes, `(12x)` counts and all.

## Running the whole stack

Three projects, started in this order (`webappShared` must be built first because
the other two import it as a local file dependency):

```bash
# 1. shared models — build once, rebuild after changing anything in webappShared
cd webappShared && npm install && npm run build

# 2. API — needs MongoDB on localhost:27017 with the "wheel" and/or "plating" databases
cd ../webappNode && npm install && npm run dev      # http://localhost:3000

# 3. this app
cd ../webappUI && npm install && npm start          # http://localhost:4200
```

Then sign in at http://localhost:4200.

## Run

```bash
npm install
npm start           # http://localhost:4200
```

`npm start` proxies `/api` to `http://localhost:3000` (see `proxy.conf.json`), so
start `webappNode` first. The dev server binds `0.0.0.0`, so you can open it from
a phone on the same network using the "Network" URL that `ng serve` prints.

```bash
npm run build       # production bundle into dist/webapp-ui
```

### Why `webapp-shared` is excluded from prebundling

`angular.json` sets `serve.options.prebundle.exclude: ["webapp-shared"]`.

Vite pre-bundles dependencies once and caches them under `.angular/cache`, on the
assumption that anything in `node_modules` is a fixed release. `webapp-shared` is
a symlink to `../webappShared`, so it breaks that assumption: adding an export to
`webappShared/src` left the dev server serving a months-old copy of the package,
and the first chunk to import the new name failed with
`does not provide an export named …`. A failed lazy chunk aborts the router
navigation silently, so clicking a job row simply did nothing.

Excluding it bundles the shared code with the app itself, so it is rebuilt with
the rest of the source. Production builds were never affected — only `ng serve`.
If the dev server ever serves stale shared code again, delete `.angular/cache`
and restart it.

## Screens

| Route                | Purpose                                                         |
| -------------------- | --------------------------------------------------------------- |
| `/login`             | Sign in. The backend is the only thing that checks credentials.  |
| `/jobs`              | Saved views (incomplete / completed / unpaid / all) and search.  |
| `/jobs/:jobId`       | The job card — customer, dates, work lines, totals, notes.       |
| `/jobs/:jobId/print` | Printable customer or workshop copy.                             |

`authGuard` keeps signed-out visitors on `/login` and remembers where they were
heading; `guestGuard` sends an already-signed-in operator straight to `/jobs`.

## How the responsive layout works

There are no fixed pixel positions anywhere — the desktop app's absolute
coordinates are replaced with:

- **Labels above inputs**, so a long label never squeezes its field.
- **`.field-grid`** — `repeat(auto-fit, minmax(220px, 1fr))`, which collapses to
  one column on a phone without needing a breakpoint.
- **`.button-row`** — flex with `wrap`, so button clusters reflow instead of
  overflowing. `.stack-mobile` makes them full-width below 600px.
- **Work lines** — one row per line on a wide screen
  (work | detail | qty | unit | total); on a phone the three short numeric
  fields share a row while work and detail span the full width.
- **Job type picker** — a bottom sheet on a phone, a centred dialog above 700px.
- **Photo grid** — `auto-fill` square tiles, two or three across on a phone; the
  full-screen viewer sizes its image with `100dvh` so it never runs off screen.
- **Sticky save bar** — Save stays reachable on a long page, and it respects
  `env(safe-area-inset-bottom)` so it clears the iPhone home indicator.
- **44px minimum tap targets** and 16px input text, which stops iOS Safari from
  zooming when a field is focused.
- Wide content scrolls inside `.scroll-x` containers so the page body itself
  never scrolls sideways.

Verified with no horizontal overflow at 320px, 390px and 768px wide.

## Photos

The job card's photo panel replaces the desktop form's top-right picture box.
It needs `PHOTO_ROOT` set on the API host (see `webappNode/README.md`); without it
the panel explains that the share is unreachable and everything else still works.

- **Add photo** is a `<input type="file" accept="image/*,video/*" capture="environment" multiple>`
  wrapped in a styled label — on a phone it opens the camera, on a desktop the
  file picker. Multiple files upload in sequence.
- **Web photo** drives an attached webcam through `getUserMedia`, for the desk
  PCs whose file picker has no camera behind it: a live preview, "Take photo" to
  freeze the frame, then "Use photo" or "Retake". The still is drawn to a canvas
  at the camera's own resolution and uploaded as JPEG down the same path as a
  picked file, so naming, the Mongo backup and duplicate detection are identical.
  A camera dropdown appears only when the machine reports more than one.
  Browsers only grant camera access in a secure context, so on a plain
  `http://host:4200` address the dialog says so and points at **Add photo** —
  see "HTTPS and internet access" in `webappNode/README.md`.
- Tapping a tile opens a full-screen viewer with prev/next and delete, the
  equivalent of the desktop `PictureViewer` dialog (which also deleted on
  right-click).
- Photo bytes are behind the session token, so `PhotoService` fetches them as
  blobs through `HttpClient` and renders object URLs. An `<img src>` pointed
  straight at the API would fail, because the browser cannot attach an
  `Authorization` header to an image request. Components release those URLs on
  destroy.
- The **workshop** print copy embeds the job's first still image, as the desktop
  printout does; the customer copy does not.

## Notes

- Money is calculated by `calculateTotals()` from `webappShared`, so the browser
  and the server agree; the server recomputes on save regardless.
- Edited fields get a light-yellow tint, echoing the desktop app's cue for
  unsaved changes.
