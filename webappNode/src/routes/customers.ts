import { Router, type Request, type Response } from 'express';
import { sessionDb } from '../auth.js';
import { fussyCustomers } from '../db.js';

export const customersRouter = Router();

/**
 * Extract the 9+ digit phone numbers from a free-text phone field, the same way
 * DataAccess.StripPhoneAndEmailToSqlSuitable does, so the fussy-customer list
 * keys match those written by the desktop app.
 */
export function extractPhoneNumbers(phone: string): string[] {
  const found: string[] = [];
  let current = '';
  let nonDigitRun = 0;
  for (const ch of phone.trim()) {
    if (ch >= '0' && ch <= '9') {
      current += ch;
      nonDigitRun = 0;
      continue;
    }
    if (ch === ' ') {
      nonDigitRun++;
      if (current.length >= 9) {
        found.push(current);
        current = '';
      }
    } else {
      nonDigitRun++;
    }
    // Two consecutive non-digits end the current run, as in the desktop parser.
    if (nonDigitRun >= 2) {
      current = '';
    }
  }
  if (current.length >= 9) {
    found.push(current);
  }
  return found;
}

/**
 * GET /api/customers/fussy?phone=&email=
 * The desktop app tints the whole form salmon for these customers; the web UI
 * shows a warning banner instead.
 */
customersRouter.get('/fussy', async (req: Request, res: Response) => {
  const phone = String(req.query.phone ?? '');
  const email = String(req.query.email ?? '').trim();
  const keys = extractPhoneNumbers(phone);
  if (email) keys.push(email);
  if (keys.length === 0) {
    res.json({ isFussy: false });
    return;
  }
  const count = await fussyCustomers(sessionDb(req)).countDocuments({ phoneOrEmail: { $in: keys } });
  res.json({ isFussy: count > 0 });
});

/** POST /api/customers/fussy — flag a customer (the desktop's "!" button). */
customersRouter.post('/fussy', async (req: Request, res: Response) => {
  const body = (req.body ?? {}) as { phone?: unknown; email?: unknown };
  const phone = String(body.phone ?? '');
  const email = String(body.email ?? '').trim();
  const keys = extractPhoneNumbers(phone);
  if (email) keys.push(email);
  if (keys.length === 0) {
    res.status(400).json({ error: 'Phone must contain at least a 9 digit number' });
    return;
  }
  const database = sessionDb(req);
  for (const key of keys) {
    await fussyCustomers(database).updateOne(
      { phoneOrEmail: key },
      { $setOnInsert: { phoneOrEmail: key } },
      { upsert: true }
    );
  }
  res.json({ added: keys });
});

/** DELETE /api/customers/fussy — clear the flag again. */
customersRouter.delete('/fussy', async (req: Request, res: Response) => {
  const body = (req.body ?? {}) as { phone?: unknown; email?: unknown };
  const keys = extractPhoneNumbers(String(body.phone ?? ''));
  const email = String(body.email ?? '').trim();
  if (email) keys.push(email);
  if (keys.length === 0) {
    res.status(400).json({ error: 'Nothing to clear' });
    return;
  }
  const result = await fussyCustomers(sessionDb(req)).deleteMany({ phoneOrEmail: { $in: keys } });
  res.json({ removed: result.deletedCount });
});
