import { Router, type Request, type Response } from 'express';
import {
  buildJobTypeCatalogue,
  parseStringPrice,
  pricingIsWheelFlag,
  type PricingDoc,
} from 'webapp-shared';
import { sessionDb } from '../auth.js';
import { pricing } from '../db.js';

export const jobTypesRouter = Router();

/**
 * GET /api/job-types — the job type catalogue with live prices, for whichever
 * business the session signed in to. Prices live in that database's `pricing`
 * collection keyed by the desktop control name and flagged `isWheel` to match
 * the mode; the grouping and ordering come from webappShared so both apps agree
 * on what is on offer.
 */
jobTypesRouter.get('/', async (req: Request, res: Response) => {
  const database = sessionDb(req);
  const docs = (await pricing(database)
    .find({ isWheel: pricingIsWheelFlag(database) })
    .toArray()) as unknown as PricingDoc[];
  res.json({ groups: buildJobTypeCatalogue(docs, database) });
});

/**
 * PUT /api/job-types/:controlName — override a price and/or caption.
 * Equivalent to Ctrl-clicking a button in the desktop popup with the override
 * fields filled in (DataAccess.findOrUpdatePrice).
 */
jobTypesRouter.put('/:controlName', async (req: Request, res: Response) => {
  const database = sessionDb(req);
  const isWheel = pricingIsWheelFlag(database);
  const controlName = String(req.params.controlName);
  const body = (req.body ?? {}) as { price?: unknown; label?: unknown };

  const update: Record<string, unknown> = {};
  if (body.price !== undefined && body.price !== null && String(body.price).trim() !== '') {
    const price = parseStringPrice(String(body.price));
    if (!Number.isFinite(price) || price < 0) {
      res.status(400).json({ error: 'Price must be a non-negative number' });
      return;
    }
    // Stored the same way the desktop writes it, e.g. "$35".
    update.stringPrice = `$${price}`;
  }
  if (typeof body.label === 'string' && body.label.trim() !== '') {
    update.controlText = body.label.trim();
  }
  if (Object.keys(update).length === 0) {
    res.status(400).json({ error: 'Provide a price and/or a label to change' });
    return;
  }

  await pricing(database).updateOne(
    { controlName, isWheel },
    { $set: update, $setOnInsert: { controlName, isWheel } },
    { upsert: true }
  );
  const saved = await pricing(database).findOne({ controlName, isWheel });
  res.json({ pricing: saved });
});
