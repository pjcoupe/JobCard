# webappShared — shared models and rules

TypeScript package used by both `webappNode` and `webappUI`, so the two sides
cannot drift apart on data shapes, money maths, or the job type catalogue.

It is consumed as a local file dependency (`"webapp-shared": "file:../webappShared"`)
and must be built before the other two projects:

```bash
npm install
npm run build       # emits dist/
```

## Contents

| File                | What it holds                                                        |
| ------------------- | -------------------------------------------------------------------- |
| `job-card.model.ts` | `JobCardDoc` plus the flat ↔ line conversion (`toLines`, `applyLine`) |
| `totals.ts`         | `calculateTotals`, `applyTotals`, `round2`, `GST_RATE`                |
| `pricing.ts`        | Wheel job type groups and `buildJobTypeCatalogue`                    |
| `constants.ts`      | Business name, tax labels, dropdown options, disclaimers, views      |
| `dates.ts`          | `d/M/yy` display and `<input type="date">` conversion                |
| `photos.ts`         | Photo folder/filename conventions on the shared drive                |
| `api.ts`            | Request/response contracts                                           |

## Why the flat ↔ line conversion exists

The desktop app stores each job line as a numbered set of top-level fields rather
than an array — `jobDetail00`, `jobType00`, `jobQty00`, `jobUnitPrice00`,
`jobPrice00`, then `01`, `02` … up to `17`. It also has eleven fixed legacy rows
named after the service (`jobWheelCrackText`, `jobWheelCrackQty`, …) and a
standalone `jobFreight`.

`toLines()` normalises all of that into an ordered `JobLine[]` the UI can loop
over; `applyLine()` writes a line back to its original field names. This keeps the
web app compatible with documents the desktop app wrote, without reshaping the
database.

## The GST convention

Line prices are GST exclusive. `calculateTotals()` sums them, adds 15%, and
returns:

- `totalExcludingGst` → stored in `jobTOTAL`
- `gst` → stored in `jobGST`
- `totalIncludingGst` / `amountToPay` → stored in `jobSubTotal`

The field-name-to-meaning mapping looks inverted, but it is what
`JobCard.UpdateAllTotals()` does and what the existing documents contain, so it is
preserved exactly. `round2()` rounds half-away-from-zero to match .NET's
`MidpointRounding.AwayFromZero`.

## Photo conventions

`photos.ts` encodes how the desktop app finds photos, since nothing about them is
stored in the database — the path *is* the index:

```
{root}/{year}/{year} {MonthName}/{jobID} {business}-{customer} {phone} {details} {NNN}.jpg
```

`photoFolderSegments()` derives the folder from a job's date, `jobIdFromFilename()`
does the leading-number match `GetJobPictureFiles` performs, and
`buildPhotoFilename()` reproduces `SaveUniquePhoto`'s naming, including the
`1 + existing count` sequence number and the Windows-forbidden-character
substitution. Keeping these here means the backend and the UI cannot disagree
about which files belong to a job.

## Pricing catalogue

`WHEEL_PRICING_GROUPS` records the grouping and ordering of the desktop
`JobTypePopup` in wheel mode. Captions and prices are **not** hard-coded — they
are read at runtime from the `wheel.pricing` collection, keyed by the original
WinForms control name (`setUpOnLathe`, `button43`, …), and buttons still captioned
`Unused` are dropped. Editing a price in the web app updates the same document the
desktop app reads.
