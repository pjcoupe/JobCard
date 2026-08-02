import type { WithId, Document } from 'mongodb';
import {
  XERO_DEFAULT_ACCOUNT_CODE,
  XERO_DEFAULT_TAX_TYPE,
  normalizeInvoiceMode,
  type XeroInvoiceMode,
} from 'webapp-shared';
import { settings } from './db.js';

/**
 * The Xero half of the shared settings.settings document. Ports the settings
 * accessors in DataAccess.cs (findSettings, UpdateSettingsFieldsAsync) so both
 * apps read and write the same fields with the same semantics.
 *
 * Every value here is sensitive to some degree and none of it may be returned to
 * the browser as-is — see the XeroStatusResponse builder in routes/xero.ts.
 */
export interface XeroSettings {
  _id: unknown;
  xeroClientId: string;
  xeroClientSecret: string;
  xeroRedirectUri: string;
  /**
   * The access token in use. activeXeroToken is the field shared with the desktop
   * app; xeroAccessToken is the older field, still written for builds that predate
   * the change (DataAccess.SettingsSettingsDoc / XeroService.ActiveToken).
   */
  activeXeroToken: string;
  xeroAccessToken: string;
  xeroRefreshToken: string;
  xeroTokenExpiresAtUtc: Date | null;
  xeroTokenLockUntilUtc: Date | null;
  xeroTenantId: string;
  xeroTenantName: string;
  xeroLastTenant: string;
  xeroInvoiceMode: XeroInvoiceMode;
  xeroDefaultSalesAccountCode: string;
  xeroDefaultTaxType: string;
}

function str(value: unknown): string {
  return typeof value === 'string' ? value.trim() : '';
}

function date(value: unknown): Date | null {
  if (value instanceof Date) return Number.isNaN(value.getTime()) ? null : value;
  if (typeof value === 'string' && value.trim()) {
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }
  return null;
}

function shape(doc: WithId<Document>): XeroSettings {
  return {
    _id: doc._id,
    xeroClientId: str(doc.xeroClientId),
    xeroClientSecret: str(doc.xeroClientSecret),
    xeroRedirectUri: str(doc.xeroRedirectUri),
    activeXeroToken: str(doc.activeXeroToken) || str(doc.xeroAccessToken),
    xeroAccessToken: str(doc.xeroAccessToken),
    xeroRefreshToken: str(doc.xeroRefreshToken),
    xeroTokenExpiresAtUtc: date(doc.xeroTokenExpiresAtUtc),
    xeroTokenLockUntilUtc: date(doc.xeroTokenLockUntilUtc),
    xeroTenantId: str(doc.xeroTenantId),
    xeroTenantName: str(doc.xeroTenantName),
    xeroLastTenant: str(doc.xeroLastTenant),
    xeroInvoiceMode: normalizeInvoiceMode(str(doc.xeroInvoiceMode)),
    xeroDefaultSalesAccountCode: str(doc.xeroDefaultSalesAccountCode) || XERO_DEFAULT_ACCOUNT_CODE,
    xeroDefaultTaxType: str(doc.xeroDefaultTaxType) || XERO_DEFAULT_TAX_TYPE,
  };
}

/**
 * Load the settings document, mirroring DataAccess.findSettings(): if there is
 * more than one, prefer the one with a complete Xero client config, else take the
 * first. Returns null when the collection is empty.
 */
export async function findXeroSettings(): Promise<XeroSettings | null> {
  const docs = await settings().find({}).toArray();
  if (docs.length === 0) return null;
  for (const doc of docs) {
    if (str(doc.xeroClientId) && str(doc.xeroClientSecret) && str(doc.xeroRedirectUri)) {
      return shape(doc);
    }
  }
  return shape(docs[0]!);
}

/**
 * Write specific fields of the settings document, mirroring
 * DataAccess.UpdateSettingsFieldsAsync.
 *
 * Always a per-field $set on the document's own _id, never a replace: this
 * document also holds the email credentials and the whole pricing sub-document,
 * and the desktop app would lose them.
 */
export async function updateXeroSettingsFields(
  id: unknown,
  fields: Record<string, unknown>
): Promise<void> {
  if (Object.keys(fields).length === 0) return;
  await settings().updateOne({ _id: id as never }, { $set: fields }, { upsert: true });
}

/** Which of the three client-config fields are still blank. */
export function missingClientConfig(config: XeroSettings | null): string[] {
  if (!config) return ['xeroClientId', 'xeroClientSecret', 'xeroRedirectUri'];
  const missing: string[] = [];
  if (!config.xeroClientId) missing.push('xeroClientId');
  if (!config.xeroClientSecret) missing.push('xeroClientSecret');
  if (!config.xeroRedirectUri) missing.push('xeroRedirectUri');
  return missing;
}
