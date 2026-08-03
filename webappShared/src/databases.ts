/**
 * Which business a session works against — the MongoDB database holding its
 * jobs, prices, fussy customers and photo backups.
 *
 * The desktop app decides this from its own executable name: anything with
 * "wheel" in it uses the "wheel" database, everything else uses "plating"
 * (JobTypePopup.isWheelApp, read by DataAccess.connectMongoDb). One installed
 * exe is therefore permanently one business.
 *
 * The web app is a single deployment serving both, so there is no filename to
 * key off. The choice moves to the sign-in screen, and the session token carries
 * it from then on — see webappNode/src/auth.ts. It cannot be changed without
 * signing in again, which is deliberate: a stray request must never be able to
 * land in the other business's data.
 *
 * The `settings` database is NOT per-business. Both modes share it, exactly as
 * the desktop app does.
 */

export const JOB_DATABASES = ['wheel', 'plating'] as const;

export type JobDatabase = (typeof JOB_DATABASES)[number];

/** What the sign-in screen starts on, and the safer of the two to default to. */
export const DEFAULT_JOB_DATABASE: JobDatabase = 'wheel';

export function isJobDatabase(value: unknown): value is JobDatabase {
  return typeof value === 'string' && (JOB_DATABASES as readonly string[]).includes(value);
}

/** Display names, so the server's messages and the UI agree on wording. */
export const JOB_DATABASE_LABELS: Record<JobDatabase, string> = {
  wheel: 'Wheel',
  plating: 'Plating',
};

export function jobDatabaseLabel(database: JobDatabase): string {
  return JOB_DATABASE_LABELS[database];
}
