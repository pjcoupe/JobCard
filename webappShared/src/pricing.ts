/**
 * Wheel-mode job type catalogue.
 *
 * The desktop app (JobTypePopup.cs) lays these out as WinForms GroupBoxes full
 * of Buttons; the button *prices and captions* live in the `wheel.pricing`
 * collection keyed by control name (isWheel: true). This file preserves the
 * grouping and ordering that the popup used in wheel mode, so the web UI can
 * render the same choices with live prices from the database.
 *
 * Wheel mode shows only: "Wheel repair" (loose buttons) plus its nested
 * "Repair and Finishing", "Wheel Tyre Service", "Rear skirt damage repair" and
 * "Front skirt damage repair" groups. Plating / Galv / Other groups are hidden
 * (JobTypePopup.Form_Shown).
 *
 * IMPORTANT: when a button is picked, the desktop app writes the *group name*
 * into the job's detail column and the button caption into the type column
 * (see doCheckChange), which is why each group carries a `detail` value.
 */

export interface PricingGroup {
  /** Group caption; also the value written to the job line's detail field. */
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

/** A pricing record as stored in wheel.pricing. */
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
  /** Group caption — written to the job line's detail field. */
  detail: string;
  /** Numeric price (GST exclusive, as stored). */
  price: number;
}

/** Parse the stored "$35" style price into a number. */
export function parseStringPrice(stringPrice: string | null | undefined): number {
  if (!stringPrice) return 0;
  const n = Number(String(stringPrice).replace(/[$,\s]/g, ''));
  return Number.isFinite(n) ? n : 0;
}

/**
 * Build the grouped option list shown in the UI, dropping placeholder buttons.
 * Any control missing from the pricing collection is simply omitted.
 */
export function buildJobTypeCatalogue(
  pricing: PricingDoc[]
): Array<{ detail: string; options: JobTypeOption[] }> {
  const byName = new Map<string, PricingDoc>();
  for (const p of pricing) {
    byName.set(p.controlName, p);
  }
  return WHEEL_PRICING_GROUPS.map((group) => ({
    detail: group.detail,
    options: group.controls
      .map((controlName) => {
        const doc = byName.get(controlName);
        if (!doc) return null;
        const label = (doc.controlText || '').trim();
        if (!label || label === UNUSED_CAPTION) return null;
        return {
          controlName,
          label,
          detail: group.detail,
          price: parseStringPrice(doc.stringPrice),
        } satisfies JobTypeOption;
      })
      .filter((o): o is JobTypeOption => o !== null),
  })).filter((g) => g.options.length > 0);
}
