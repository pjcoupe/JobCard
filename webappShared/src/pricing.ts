/**
 * Job type catalogues for both businesses.
 *
 * The desktop app (JobTypePopup.cs) lays these out as WinForms GroupBoxes full
 * of Buttons and CheckBoxes; the *prices and captions* live in the `pricing`
 * collection of whichever database is open, keyed by control name and flagged
 * `isWheel` to match the running mode. This file preserves the grouping and
 * ordering the popup used, so the web UI can render the same choices with live
 * prices from the database.
 *
 * Which groups each mode shows comes straight from JobTypePopup.Form_Shown:
 *
 * - Wheel: "Wheel repair" (loose buttons) and its nested "Repair and Finishing",
 *   "Wheel Tyre Service", "Rear skirt damage repair" and "Front skirt damage
 *   repair" groups. The plating checkbox groups are hidden.
 * - Plating: "Repair and Finishing", "Plating", "Galv" and "Other" — all
 *   checkboxes. The whole "Wheel repair" box, and therefore everything nested
 *   inside it, is hidden.
 *
 * The two modes also differ in what a pick writes onto the job line, which is
 * why groups carry `writesDetail` — see buildJobTypeCatalogue.
 */

import type { JobDatabase } from './databases.js';

export interface PricingGroup {
  /** Group caption, shown as the heading in the picker. */
  detail: string;
  /** Control names in display order — resolved to captions/prices from Mongo. */
  controls: string[];
}

/** Captions equal to this are placeholder buttons and are never shown. */
export const UNUSED_CAPTION = 'Unused';

/** Control names that are UI actions in the desktop popup, not job types. */
export const NON_JOB_TYPE_CONTROLS = ['clearButton', 'smiley'];

export const WHEEL_PRICING_GROUPS: PricingGroup[] = [
  {
    detail: 'Wheel repair',
    controls: [
      'setUpOnLathe',
      'button41',
      'button42',
      'button43',
      'button44',
      'button45',
      'button15',
      'button16',
      'button1',
      'button40',
      'button11',
      'button13',
      'button12',
      'button14',
      'button17',
      'button18',
      'button19',
      'button20',
      'button21',
      'button22',
      'button23',
      'button24',
      'button25',
      'button26',
      'button27',
      'button28',
      'button29',
      'button47',
      'button48',
      'button49',
      'button53',
      'button54',
      'button55',
      'button56',
      'button57',
      'button58',
      'button59',
      'button60',
      'button61',
    ],
  },
  {
    detail: 'Repair and Finishing',
    controls: [
      'strip',
      'polish',
      'bentSpoke',
      'wheelBalance',
      'button30',
      'button62',
      'button63',
      'button64',
      'button65',
    ],
  },
  {
    detail: 'Wheel Tyre Service',
    controls: ['removeTyre', 'fitTyre', 'button46', 'button50', 'button51', 'button52'],
  },
  {
    detail: 'Rear skirt damage repair',
    controls: [
      'button2',
      'button3',
      'button4',
      'button5',
      'button9',
      'button8',
      'button7',
      'button6',
      'button10',
      'button66',
      'button67',
      'button68',
      'button69',
      'button70',
      'button71',
      'button72',
    ],
  },
  {
    detail: 'Front skirt damage repair',
    controls: [
      'button39',
      'button38',
      'button37',
      'button36',
      'button35',
      'button34',
      'button33',
      'button32',
      'button31',
      'button73',
      'button74',
      'button75',
      'button76',
      'button77',
      'button78',
      'button79',
    ],
  },
];

/**
 * Plating mode's groups (JobTypePopup groupBox1, groupBox2, groupBox4,
 * groupBox5). Every entry is a checkbox on the desktop form, and every one of
 * them is priced at $0 in the shipped data — plating work is priced by hand on
 * the line, unlike wheel repair where the button carries the price.
 */
export const PLATING_PRICING_GROUPS: PricingGroup[] = [
  {
    detail: 'Repair and Finishing',
    controls: ['checkBox1', 'checkBox2', 'checkBox3', 'checkBox4'],
  },
  {
    detail: 'Plating',
    controls: [
      'checkBox5',
      'checkBox6',
      'checkBox7',
      'checkBox8',
      'checkBox9',
      'checkBox10',
      'checkBox11',
      'checkBox12',
      'checkBox13',
    ],
  },
  {
    detail: 'Galv',
    controls: ['checkBox20', 'checkBox21'],
  },
  {
    detail: 'Other',
    controls: ['checkBox22'],
  },
];

/** A pricing record as stored in the open database's `pricing` collection. */
export interface PricingDoc {
  _id?: string;
  isWheel: boolean;
  controlName: string;
  controlText: string;
  /** Stored as a string, usually with a leading '$' (e.g. "$35"). */
  stringPrice: string;
}

/** One selectable job type presented to the user. */
export interface JobTypeOption {
  controlName: string;
  /** Button caption — written to the job line's type field. */
  label: string;
  /**
   * Group caption, written to the job line's detail field — or null to leave
   * whatever detail the line already has. Wheel mode overwrites the detail
   * (doCheckChange sets jobDetail to the parent group's caption); plating mode
   * deliberately does not touch it.
   */
  detail: string | null;
  /** Numeric price (GST exclusive, as stored). */
  price: number;
}

/** Parse the stored "$35" style price into a number. */
export function parseStringPrice(stringPrice: string | null | undefined): number {
  if (!stringPrice) return 0;
  const n = Number(String(stringPrice).replace(/[$,\s]/g, ''));
  return Number.isFinite(n) ? n : 0;
}

/** The groups one mode shows, and how a pick from them fills a job line. */
export function pricingGroupsFor(database: JobDatabase): PricingGroup[] {
  return database === 'wheel' ? WHEEL_PRICING_GROUPS : PLATING_PRICING_GROUPS;
}

/**
 * The `isWheel` value that mode's pricing documents carry. The flag is stored
 * per document rather than implied by the database, so both must line up:
 * DataAccess.findOrUpdatePrice always filters on Eq("isWheel", isWheelApp()).
 */
export function pricingIsWheelFlag(database: JobDatabase): boolean {
  return database === 'wheel';
}

/**
 * Title-case a plating caption, e.g. "SILVER GALV" -> "Silver Galv". The
 * desktop stores plating captions in capitals but writes them onto the job line
 * title-cased (doCheckChange, via en-NZ TextInfo.ToTitleCase of the lowercased
 * caption), so jobs read the same whichever app created them.
 */
function titleCase(text: string): string {
  return text
    .toLowerCase()
    .replace(/(^|[\s/(-])([a-z])/g, (_match, prefix: string, letter: string) => prefix + letter.toUpperCase());
}

/* -------------------------------------------------------------------------
 * Plating lines: several processes on one line
 * ---------------------------------------------------------------------- */

/**
 * One process on a plating line, e.g. `{ label: 'Nickle', count: 12 }`.
 *
 * A plating job is a sequence of processes applied to the same items, so the
 * desktop puts them all in one line's type field rather than one per line:
 *
 *   "Strip, Polish, (3x)Copper, (12x)Nickle, Chrome"
 *
 * A count above one is written as a `(Nx)` prefix with no space after it, and a
 * count of one is written bare. Quantity, unit price and line total are left
 * alone entirely — they describe the items being plated, not the processes, so
 * that line is priced by hand. Wheel mode is the opposite: one type per line,
 * with the count in the quantity column driving the price.
 */
export interface PlatingType {
  label: string;
  count: number;
}

/**
 * Read a plating line's type field back into its processes.
 *
 * Ports the parsing half of doCheckChange, quirks included: the `(Nx)` prefix is
 * only looked for when "x)" appears somewhere in the entry, and a prefix that
 * does not parse as a number is left as part of the label rather than being
 * silently dropped. Repeats of the same process are summed, and first-seen order
 * is kept so re-saving a line never reshuffles it.
 */
export function parsePlatingTypes(text: string | null | undefined): PlatingType[] {
  const entries: PlatingType[] = [];
  const byLabel = new Map<string, PlatingType>();

  for (const raw of String(text ?? '').split(', ')) {
    let label = raw;
    let count = 1;

    if (raw.includes('x)')) {
      const open = raw.indexOf('(');
      const x = open < 0 ? -1 : raw.indexOf('x', open);
      // At least one character between "(" and "x" — the desktop's endIdx >= idx + 2.
      if (open >= 0 && x >= open + 2) {
        const parsed = Number(raw.slice(open + 1, x).trim());
        if (Number.isInteger(parsed)) {
          count = parsed;
          label = raw.slice(x + 2);
        }
      }
    }

    label = titleCase(label.trim());
    if (!label) continue;

    const existing = byLabel.get(label);
    if (existing) {
      existing.count += count;
      continue;
    }
    const entry: PlatingType = { label, count };
    byLabel.set(label, entry);
    entries.push(entry);
  }

  return entries;
}

/** Write the processes back out in the desktop's format. */
export function formatPlatingTypes(entries: PlatingType[]): string {
  return entries
    .map((entry) => (entry.count > 1 ? `(${entry.count}x)${entry.label}` : entry.label))
    .join(', ');
}

/**
 * Add one more process to a plating line, which is all picking a plating type
 * does. Picking one already on the line bumps its count instead of repeating it.
 *
 * The desktop appends the pick to the split list and re-accumulates the whole
 * lot, which is why an empty line works out the same way here: the empty first
 * entry is dropped by the parser.
 */
export function addPlatingType(existing: string | null | undefined, label: string): string {
  return formatPlatingTypes(parsePlatingTypes(`${existing ?? ''}, ${label}`));
}

/**
 * Build the grouped option list shown in the UI, dropping placeholder buttons.
 * Any control missing from the pricing collection is simply omitted.
 */
export function buildJobTypeCatalogue(
  pricing: PricingDoc[],
  database: JobDatabase
): Array<{ detail: string; options: JobTypeOption[] }> {
  const byName = new Map<string, PricingDoc>();
  for (const p of pricing) {
    byName.set(p.controlName, p);
  }
  const isWheel = database === 'wheel';
  return pricingGroupsFor(database)
    .map((group) => ({
      detail: group.detail,
      options: group.controls
        .map((controlName) => {
          const doc = byName.get(controlName);
          if (!doc) return null;
          const caption = (doc.controlText || '').trim();
          if (!caption || caption === UNUSED_CAPTION) return null;
          return {
            controlName,
            label: isWheel ? caption : titleCase(caption),
            detail: isWheel ? group.detail : null,
            price: parseStringPrice(doc.stringPrice),
          } satisfies JobTypeOption;
        })
        .filter((o): o is JobTypeOption => o !== null),
    }))
    .filter((g) => g.options.length > 0);
}
