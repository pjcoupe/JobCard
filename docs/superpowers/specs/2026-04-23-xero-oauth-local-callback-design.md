# Xero OAuth local callback (HttpListener)

Date: 2026-04-23  
Status: Approved (Peter, 2026-04-23)  
Scope: Replace manual authorization-code paste with automatic capture from `xeroRedirectUri` when it points at a local HTTP callback, with timeout and optional paste fallback.  
Related: `docs/superpowers/specs/2026-04-21-xero-invoicing-design.md`

## 1) Problem

Xero redirects the browser to `xeroRedirectUri` (for example `http://localhost:8888/callback`) after login. Nothing in the app was listening on that URL, so the callback step failed. The UI then relied on the user pasting the `code` from the address bar.

## 2) Goals

- On **Connect to Xero**, start a local **HTTP** listener that matches the configured redirect URI, open the authorize URL, then **read `code` and `state` from the query string** automatically.
- **Validate `state`** against the value sent in the authorize request (CSRF / session binding for this desktop flow).
- Respond to the browser with a **short HTML success page** so the user can close the tab.
- **Remove the manual paste step** on the success path.
- **Optional fallback**: if the listener fails to start, times out, or returns an invalid/missing `code`/`state`, offer the **existing paste-code dialog** (same behavior as today).
- **Chosen lifecycle (approach C)**: start the listener **only when Connect is clicked** (not when the modal opens). Stop the listener and cancel any wait when the OAuth attempt ends (success, failure, timeout) or when the **Xero Management** modal is closed while a connect is in progress.

## 3) Non-goals

- PKCE (optional future hardening; confidential client + secret remains as today).
- HTTPS redirect URI for localhost (keep `http://localhost` as configured in Mongo).
- Changing Xero API usage beyond the authorization step.

## 4) User flow

1. User opens **Xero Management** (no listener yet).
2. User clicks **Connect to Xero**; app validates `xeroClientId`, `xeroClientSecret`, `xeroRedirectUri` as today.
3. App generates **cryptographically random `state`** (e.g. GUID without braces), keeps it in memory for this attempt only.
4. App **starts `HttpListener`** with a prefix derived from `xeroRedirectUri` (see section 6). If start fails → show clear error and optionally open paste fallback (implementation may offer paste immediately or only after user chooses; minimum is error text + ability to retry Connect).
5. App opens the browser with `BuildAuthorizeUrl(settings, state)` (unchanged contract except `state` is always validated on callback).
6. App **waits** for a single incoming GET whose path matches the redirect, or until **timeout** (recommended default **3 minutes**, single constant in code).
7. On request: parse query `code` and `state`; send **200** + minimal HTML body (e.g. “You can close this tab and return to JobCard.”). **Do not** log full query strings (secrets in `code`).
8. **Stop** listener and dispose resources before or immediately after token exchange (port should not stay bound after this attempt).
9. If `state` does not match, or `code` is missing → treat as failure; offer **paste fallback**.
10. If valid → `ExchangeCodeAsync` as today; reload tenants and status as today.
11. If **timeout** or **listener error** → stop listener; offer **paste fallback**; if user cancels paste, show authorization cancelled message.

## 5) Modal close during connect

- Register **FormClosing** (or equivalent) on `XeroManagementForm` while a connect is active.
- **Cancel** the wait (cancellation token or equivalent) and **stop** the listener so `GetContext` / async wait does not hang.
- Do not call token exchange without a validated code.

## 6) Technical constraints (.NET Framework 4.8, WinForms)

### 6.1 Prefix and redirect URI

- Derive `HttpListener.Prefixes` from `settings.xeroRedirectUri` (absolute URI).
- **Normalize** trailing slash: `HttpListener` prefix rules require care; one implementation choice is to normalize to a prefix that matches what Xero will request (document the exact rule in code comments only if needed for maintainers; spec: **must match** browser redirect).
- Host should remain **`localhost`** as stored in settings (avoid `127.0.0.1` mismatch with Xero app config).

### 6.2 Unblocking wait on cancel / close

- Pending `HttpListener.GetContextAsync()` (or sync `GetContext`) should be unblocked by **`HttpListener.Stop()`** from the UI close path (documented .NET pattern when cancellation is not sufficient).

### 6.3 Windows URL ACL

- If start throws **access denied**, surface text that the machine may need a one-time **`netsh http add urlacl`** for the chosen URL (user- or machine-specific); do not silently swallow.

### 6.4 Port conflict / second instance

- Only one listener per port. If bind fails, message should say port in use or permission; then **paste fallback** remains available.

## 7) Code organization (implementation hint)

- Prefer a **small helper** (e.g. static methods or a short class colocated with `XeroService`) that: builds prefix from redirect URI, starts listener, waits for one request, writes response, stops listener. **`XeroManagementForm`** orchestrates: state generation, timeout, fallback dialog, disabling **Connect** while waiting, wiring cancel on form close.
- Not prescriptive on file split as long as behavior matches this spec.

## 8) UX while waiting

- Disable **Connect** (or show inline “Waiting for Xero…”) from listener start until attempt completes, to avoid overlapping `state` values or multiple browsers.

## 9) Self-review (spec quality)

- [x] Placeholders: timeout default stated; exact HTML left to implementation.
- [x] Contradictions: earlier brainstorm considered “listener for whole modal”; **superseded** by approved **approach C** (start/stop around Connect only).
- [x] Ambiguity: “optional” paste after start failure — implementation should at minimum show error; offering paste on the same screen flow is acceptable.
- [x] Scope: authorization step only; invoice and tenant behavior unchanged.

## 10) Verification (after implementation)

- Connect with `xeroRedirectUri` = `http://localhost:8888/callback` (or configured port): browser completes; app receives code; no paste.
- Close modal during wait: no hang; listener released.
- Wrong `state` in callback: no token stored; user can retry or use paste.
- Stop another process using the port: bind fails; user can use paste fallback or free port.
