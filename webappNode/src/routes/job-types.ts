import { Router, type Request, type Response } from 'express';
import { buildJobTypeCatalogue, parseStringPrice, type PricingDoc } from 'webapp-shared';
import { pricing } from '../db.js';

export const jobTypesRouter = Router();

/**
 * GET /api/job-types — the wheel-mode job type catalogue with live prices.
 * Prices live in wheel.pricing keyed by the desktop control name; the grouping
 * and ordering come from webappShared so both apps agree on what is on offer.
 */
jobTypesRouter.get('/', async (_req: Request, res: Response) => {
  const docs = (await pricing().find({ isWheel: true }).toArray()) as unknown as PricingDoc[];
  res.json({ groups: buildJobTypeCatalogue(docs) });
});

/**
 * PUT /api/job-types/:controlName — override a price and/or caption.
 * Equivalent to Ctrl-clicking a button in the desktop popup with the override
 * fields filled in (DataAccess.findOrUpdatePrice).
 */
jobTypesRouter.put('/:controlName', async (req: Request, res: Response) => {
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

  await pricing().updateOne(
    { controlName, isWheel: true },
    { $set: update, $setOnInsert: { controlName, isWheel: true } },
    { upsert: true }
  );
  const saved = await pricing().findOne({ controlName, isWheel: true });
  res.json({ pricing: saved });
});
