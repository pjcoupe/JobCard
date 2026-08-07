import { Router, type Request, type Response } from 'express';
import type { Document, Filter } from 'mongodb';
import {
  FIXED_ROWS,
  NUMBERED_LINE_COUNT,
  WHEEL_DISCLAIMER_NOTE,
  applyTotals,
  fixedRowFields,
  numberedLineFields,
  toLines,
  type JobCardDoc,
  type JobDatabase,
  type JobListItem,
} from 'webapp-shared';
import { sessionDb } from '../auth.js';
import { jobCards, jobPictures } from '../db.js';
import { buildUpdate, COMPUTED_FIELDS } from '../job-fields.js';

export const jobsRouter = Router();

/** Escape user input before using it inside a regular expression. */
function escapeRegex(input: string): string {
  return input.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function parseIntParam(value: unknown, fallback: number, max = Number.MAX_SAFE_INTEGER): number {
  const n = Number(value);
  if (!Number.isFinite(n)) return fallback;
  return Math.min(Math.max(Math.trunc(n), 0), max);
}

/**
 * Saved views matching the desktop app's top-row buttons. "unpaid" means
 * completed but with no payment date — the intent of its Unpaid Customers list.
 */
function filterForView(view: string): { filter: Filter<Document>; sort: Record<string, 1 | -1> } {
  switch (view) {
    case 'completed':
      return { filter: { jobDateCompleted: { $ne: null } }, sort: { jobDateCompleted: -1 } };
    case 'unpaid':
      return {
        filter: { jobDateCompleted: { $ne: null }, jobDatePaid: null },
        sort: { jobDateCompleted: -1 },
      };
    case 'all':
      return { filter: {}, sort: { jobID: -1 } };
    case 'incomplete':
    default:
      return { filter: { jobDateCompleted: null }, sort: { jobDate: -1 } };
  }
}

/**
 * The only fields toListItem actually reads. The list query projects to these so
 * a page of rows does not drag whole job documents out of MongoDB — a job averages
 * about 3KB, most of it fields the list never shows (jobNotes alone carries the
 * ~900-character wheel disclaimer on every job).
 *
 * Built from the shared field helpers so it cannot drift out of step with
 * toLines(): firstDetail only looks at each line's detail and type.
 */
const LIST_PROJECTION: Record<string, 1> = (() => {
  const projection: Record<string, 1> = {
    jobID: 1,
    jobDate: 1,
    jobCustomer: 1,
    jobBusinessName: 1,
    jobPhone: 1,
    jobDateRequired: 1,
    jobDateCompleted: 1,
    jobDatePaid: 1,
    jobQuotation: 1,
    jobGoodReserved: 1,
    jobSubTotal: 1,
  };
  for (let i = 0; i < NUMBERED_LINE_COUNT; i++) {
    const f = numberedLineFields(i);
    projection[f.detail] = 1;
    projection[f.type] = 1;
  }
  for (const name of FIXED_ROWS) {
    const f = fixedRowFields(name);
    projection[f.detail] = 1;
    projection[f.type] = 1;
  }
  return projection;
})();

function firstDetail(doc: JobCardDoc): string | null {
  for (const line of toLines(doc)) {
    if (line.type && line.type.trim()) {
      const detail = line.detail?.trim();
      return detail ? `${detail} — ${line.type.trim()}` : line.type.trim();
    }
    if (line.detail && line.detail.trim()) return line.detail.trim();
  }
  return null;
}

function toIso(value: unknown): string | null {
  if (!value) return null;
  const d = value instanceof Date ? value : new Date(String(value));
  return Number.isNaN(d.getTime()) ? null : d.toISOString();
}

function toListItem(doc: JobCardDoc): JobListItem {
  return {
    _id: String(doc._id),
    jobID: doc.jobID,
    jobDate: toIso(doc.jobDate),
    jobCustomer: doc.jobCustomer ?? null,
    jobBusinessName: doc.jobBusinessName ?? null,
    jobPhone: doc.jobPhone ?? null,
    jobDateRequired: toIso(doc.jobDateRequired),
    jobDateCompleted: toIso(doc.jobDateCompleted),
    jobDatePaid: toIso(doc.jobDatePaid),
    jobQuotation: doc.jobQuotation ?? null,
    jobGoodReserved: doc.jobGoodReserved ?? null,
    summary: firstDetail(doc),
    total: doc.jobSubTotal ?? null,
  };
}

/** GET /api/jobs — saved views plus single-field search, paginated. */
jobsRouter.get('/', async (req: Request, res: Response) => {
  const database = sessionDb(req);
  const view = String(req.query.view ?? 'incomplete');
  const field = req.query.field ? String(req.query.field) : '';
  const q = req.query.q ? String(req.query.q).trim() : '';
  const page = parseIntParam(req.query.page, 0);
  const pageSize = parseIntParam(req.query.pageSize, 25, 200) || 25;

  let { filter, sort } = filterForView(view);

  if (q) {
    if (field === 'jobID') {
      const n = Number(q);
      if (!Number.isFinite(n)) {
        res.status(400).json({ error: 'Job number must be numeric' });
        return;
      }
      // An explicit job number lookup ignores the view filter.
      filter = { jobID: Math.trunc(n) };
      sort = { jobID: -1 };
    } else if (field) {
      filter = { ...filter, [field]: { $regex: escapeRegex(q), $options: 'i' } };
    } else {
      // No field chosen: search the fields an operator is most likely to know,
      // within whatever view is selected — that is what makes "unpaid customers
      // named Ali" work.
      const rx = { $regex: escapeRegex(q), $options: 'i' };
      const textMatch: Filter<Document> = {
        ...filter,
        $or: [
          { jobCustomer: rx },
          { jobBusinessName: rx },
          { jobPhone: rx },
          { jobEmail: rx },
          { jobOrderNumber: rx },
          { jobDetail00: rx },
        ],
      };

      // jobID is a number, so the regex above can never match it: an all-digit
      // search needs an exact term of its own. That term sits OUTSIDE the view
      // filter, alongside the text match rather than within it, so typing a
      // completed job's number while browsing Incomplete still finds it — the
      // same escape the explicit "Job Number" search makes. Without it the
      // dropdown changed what the same digits meant, which is how job 10427
      // could be found one way and not the other.
      const asJobId = /^\d+$/.test(q) ? Number(q) : NaN;
      filter = Number.isSafeInteger(asJobId)
        ? { $or: [{ jobID: asJobId }, textMatch] }
        : textMatch;
    }
  }

  const collection = jobCards(database);
  const [docs, total] = await Promise.all([
    collection
      .find(filter)
      .project(LIST_PROJECTION)
      .sort(sort)
      .skip(page * pageSize)
      .limit(pageSize)
      .toArray(),
    collection.countDocuments(filter),
  ]);

  res.json({
    items: (docs as unknown as JobCardDoc[]).map(toListItem),
    total,
    page,
    pageSize,
  });
});

/** GET /api/jobs/latest — the highest job number, as the desktop opens on. */
jobsRouter.get('/latest', async (req: Request, res: Response) => {
  const doc = await jobCards(sessionDb(req)).find({}).sort({ jobID: -1 }).limit(1).next();
  if (!doc) {
    res.status(404).json({ error: 'No jobs found' });
    return;
  }
  res.json({ job: doc });
});

/** GET /api/jobs/:jobID */
jobsRouter.get('/:jobID', async (req: Request, res: Response) => {
  const jobID = Number(req.params.jobID);
  if (!Number.isFinite(jobID)) {
    res.status(400).json({ error: 'Invalid job number' });
    return;
  }
  const doc = await jobCards(sessionDb(req)).findOne({ jobID: Math.trunc(jobID) });
  if (!doc) {
    res.status(404).json({ error: `Job ${jobID} not found` });
    return;
  }
  res.json({ job: doc });
});

/** GET /api/jobs/:jobID/neighbours — previous/next job numbers for navigation. */
jobsRouter.get('/:jobID/neighbours', async (req: Request, res: Response) => {
  const jobID = Math.trunc(Number(req.params.jobID));
  if (!Number.isFinite(jobID)) {
    res.status(400).json({ error: 'Invalid job number' });
    return;
  }
  const collection = jobCards(sessionDb(req));
  const [prev, next] = await Promise.all([
    collection.find({ jobID: { $lt: jobID } }).sort({ jobID: -1 }).limit(1).next(),
    collection.find({ jobID: { $gt: jobID } }).sort({ jobID: 1 }).limit(1).next(),
  ]);
  res.json({
    previous: prev ? (prev as unknown as JobCardDoc).jobID : null,
    next: next ? (next as unknown as JobCardDoc).jobID : null,
  });
});

/**
 * Allocate the next job number. The desktop app reads the highest jobID and
 * adds one; a unique index on jobID means a racing insert fails, so retry.
 */
async function insertWithNextJobId(
  database: JobDatabase,
  base: Partial<JobCardDoc>
): Promise<JobCardDoc> {
  const collection = jobCards(database);
  let lastError: unknown = null;
  for (let attempt = 0; attempt < 5; attempt++) {
    const highest = await collection.find({}).sort({ jobID: -1 }).limit(1).next();
    const nextId = ((highest as unknown as JobCardDoc | null)?.jobID ?? 0) + 1;
    const doc = { ...base, jobID: nextId } as JobCardDoc;
    try {
      const result = await collection.insertOne(doc as never);
      return { ...doc, _id: String(result.insertedId) };
    } catch (err) {
      const code = (err as { code?: number }).code;
      if (code === 11000) {
        lastError = err;
        continue; // another client took this number — try again
      }
      throw err;
    }
  }
  throw lastError ?? new Error('Could not allocate a job number');
}

/**
 * The notes a new job starts with. DisclaimerNoteAsync only appends the wheel
 * disclaimer when isWheelApp() is true, so a plating job starts with empty
 * notes — its disclaimer is printed on the docket instead of stored per job.
 */
function startingNotes(database: JobDatabase): string | null {
  return database === 'wheel' ? WHEEL_DISCLAIMER_NOTE : null;
}

/**
 * POST /api/jobs — new job.
 * Mirrors btnNewJob_Click: next job number, today's date, and (wheel mode only)
 * the wheel disclaimer pre-filled into notes.
 */
jobsRouter.post('/', async (req: Request, res: Response) => {
  const database = sessionDb(req);
  const job = await insertWithNextJobId(database, {
    jobDate: new Date(),
    jobNotes: startingNotes(database),
  });
  res.status(201).json({ job });
});

/**
 * POST /api/jobs/:jobID/duplicate — copy the customer details onto a new job,
 * exactly the fields btnDuplicate_Click carries over.
 */
jobsRouter.post('/:jobID/duplicate', async (req: Request, res: Response) => {
  const jobID = Math.trunc(Number(req.params.jobID));
  const database = sessionDb(req);
  const source = (await jobCards(database).findOne({ jobID })) as unknown as JobCardDoc | null;
  if (!source) {
    res.status(404).json({ error: `Job ${jobID} not found` });
    return;
  }
  const job = await insertWithNextJobId(database, {
    jobDate: new Date(),
    jobOrderNumber: source.jobOrderNumber ?? null,
    jobCustomer: source.jobCustomer ?? null,
    jobBusinessName: source.jobBusinessName ?? null,
    jobPhone: source.jobPhone ?? null,
    jobAddress: source.jobAddress ?? null,
    jobEmail: source.jobEmail ?? null,
    jobDelivery: source.jobDelivery ?? null,
    jobReceivedFrom: source.jobReceivedFrom ?? null,
    jobNotes: startingNotes(database),
  });
  res.status(201).json({ job });
});

/**
 * PUT /api/jobs/:jobID — save. Only whitelisted fields are written and totals
 * are always recomputed server-side so they cannot drift from the line items.
 */
jobsRouter.put('/:jobID', async (req: Request, res: Response) => {
  const jobID = Math.trunc(Number(req.params.jobID));
  if (!Number.isFinite(jobID)) {
    res.status(400).json({ error: 'Invalid job number' });
    return;
  }
  const database = sessionDb(req);
  const collection = jobCards(database);
  const existing = (await collection.findOne({ jobID })) as unknown as JobCardDoc | null;
  if (!existing) {
    res.status(404).json({ error: `Job ${jobID} not found` });
    return;
  }

  const update = buildUpdate(req.body);
  for (const field of COMPUTED_FIELDS) {
    delete update[field];
  }

  // Recompute totals from the merged document so they match the saved lines.
  const merged = { ...existing, ...update } as JobCardDoc;
  const totals = applyTotals(merged);
  update.jobTOTAL = totals.totalExcludingGst;
  update.jobGST = totals.gst;
  update.jobSubTotal = totals.totalIncludingGst;

  await collection.updateOne({ jobID }, { $set: update });
  const saved = await collection.findOne({ jobID });
  res.json({ job: saved });
});

/** DELETE /api/jobs/:jobID */
jobsRouter.delete('/:jobID', async (req: Request, res: Response) => {
  const jobID = Math.trunc(Number(req.params.jobID));
  if (!Number.isFinite(jobID)) {
    res.status(400).json({ error: 'Invalid job number' });
    return;
  }
  const database = sessionDb(req);
  // includeResultMetadata: false returns the deleted document itself rather than
  // driver 5.x's { value, ok, lastErrorObject } wrapper — which is always truthy,
  // so without this the 404 below could never fire and the cleanup that follows
  // would run with an undefined jobId. Driver 6 made this the default; passing it
  // explicitly means this reads the same under either.
  const deleted = await jobCards(database).findOneAndDelete(
    { jobID },
    { includeResultMetadata: false }
  );
  if (!deleted) {
    res.status(404).json({ error: `Job ${jobID} not found` });
    return;
  }

  // Clean up any photo backups so deleting a job doesn't leave them orphaned.
  // Best effort: the job itself is already gone, which is what matters most.
  try {
    await jobPictures(database).deleteMany({ jobId: deleted._id });
  } catch (err) {
    console.warn(`[jobs] photo backup cleanup failed for job ${jobID}:`, err);
  }

  res.json({ deleted: jobID });
});

/**
 * Find the first empty numbered line, so the UI can append a picked job type
 * the way the desktop popup fills the row that was clicked.
 */
export function firstEmptyNumberedLine(doc: JobCardDoc): number | null {
  for (let i = 0; i < NUMBERED_LINE_COUNT; i++) {
    const f = numberedLineFields(i);
    const hasData =
      (doc[f.detail] && String(doc[f.detail]).trim()) ||
      (doc[f.type] && String(doc[f.type]).trim()) ||
      doc[f.price] != null;
    if (!hasData) return i;
  }
  return null;
}
