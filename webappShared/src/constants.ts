/** Business constants carried over from the desktop wheel app. */

/** JobCard.getBusinessName() — NZ (non-Canada) branch. */
export const BUSINESS_NAME = 'Advanced Chrome Platers Ltd';

/** JobCard.GSTText() — NZ branch. */
export const TAX_LABEL = 'GST';

export const GST_NUMBER = '83-712-147';

export const BANK_ACCOUNT_NAME = 'Advanced Plating and Polishing Services Limited';
export const BANK_ACCOUNT_NUMBER = '03-15570138976-00';

/** jobReceivedFrom dropdown options. */
export const RECEIVED_FROM_OPTIONS = ['', 'Customer', 'Courier'] as const;

/** jobPaymentBy dropdown options. */
export const PAYMENT_BY_OPTIONS = ['', 'Cash', 'Eftpos', 'VISA', 'MasterCard', 'Xero'] as const;

/** Canned delivery instruction inserted by the "Collect" button. */
export const COLLECT_TEXT = 'Customer to collect';

/** Rural delivery surcharge added by the "RD" button (btnRDAddressSurcharge). */
export const RD_SURCHARGE = 7;

/** Wheel-mode disclaimer auto-appended to notes on a new job (DisclaimerNoteAsync). */
export const WHEEL_DISCLAIMER_NOTE =
  'DISCLAIMER NOTICE:\n' +
  'When Aluminium wheels have cracks or are damaged in any way the stresses caused by the impact cannot be truly identified without getting the wheel tested.' +
  `We at ${BUSINESS_NAME} weld the cracks and push out dents with a specific wheel repair machine designed and built in Europe.` +
  'This does not in any way certify the wheel for further use on a Vehicle.' +
  `We do not test wheels at ${BUSINESS_NAME}, and take no responsibility if the wheel is used on a vehicle without the wheel being certified.` +
  'It is up to the owner or customer to get the wheel certified and tested for air leaks at their own cost if they feel it is necessary.' +
  'We do not paint wheels.\nCUSTOMER SIGNATURE:   x\n';

/** Printed footer disclaimer (JobCard static constructor). */
export const PRINT_DISCLAIMER =
  'All work not collected within 3 months of completion will be sold to defer costs. ' +
  `At ${BUSINESS_NAME} we have a combined electroplating and polishing history of over 60 years. ` +
  'Advanced Chrome Platers Ltd treat all jobs with the utmost care and attention, however we take no ' +
  'responsibility for any adverse changes in the condition of items during stripping, polishing and/or ' +
  'plating processes. Please also note that items held at our premises are not covered by our insurance ' +
  'for theft, fire etc, and you may wish to contact your insurance agent regarding cover for any valuable ' +
  'items during the time they are held on our premises.';

/** Fields offered as search targets, mirroring the desktop query lists. */
export const SEARCH_FIELDS = [
  { field: 'jobID', label: 'Job number', type: 'number' },
  { field: 'jobCustomer', label: 'Customer name', type: 'string' },
  { field: 'jobBusinessName', label: 'Business name', type: 'string' },
  { field: 'jobPhone', label: 'Phone', type: 'string' },
  { field: 'jobEmail', label: 'Email', type: 'string' },
  { field: 'jobOrderNumber', label: 'Order number', type: 'string' },
  { field: 'jobDetail00', label: 'Job detail', type: 'string' },
  { field: 'jobNotes', label: 'Notes', type: 'string' },
  { field: 'jobDelivery', label: 'Delivery instructions', type: 'string' },
] as const;

export type SearchFieldName = (typeof SEARCH_FIELDS)[number]['field'];

/** Saved list views, equivalent to the desktop's top-row buttons. */
export const JOB_LIST_VIEWS = [
  { id: 'incomplete', label: 'Incomplete jobs' },
  { id: 'completed', label: 'Completed jobs' },
  { id: 'unpaid', label: 'Unpaid customers' },
  { id: 'all', label: 'All jobs' },
] as const;

export type JobListViewId = (typeof JOB_LIST_VIEWS)[number]['id'];
