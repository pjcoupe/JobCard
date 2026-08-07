# Job Card web app — deployment

How `https://jobcard.duckdns.org` is served from the workshop PC.

Everything runs on one machine, `192.168.1.9`:

```
        internet
           │
           │  ports 80 + 443 forwarded by the router
           ▼
    ┌──────────────────┐
    │ nginx  (443/80)  │  JobCardNginx service
    │                  │
    │  /api/*  ────────┼──► Node API on 127.0.0.1:3000   JobCardAPI service
    │  everything else │        │
    │  ── static ──────┼──► webappUI/dist/webapp-ui/browser
    └──────────────────┘        │
                                ▼
                         MongoDB on 27017          MongoDB service
                         (wheel, plating, settings)
                                │
                                ▼
                         D:\Kodak Pictures  (photos)
```

nginx is the only thing exposed to the internet. Node listens on port 3000 and
Mongo on 27017, neither of which is forwarded at the router.

## Layout

| Path | What |
| --- | --- |
| `nginx-1.30.4/nginx-1.30.4/` | nginx. Config in `conf/`, logs in `logs/` |
| `nginx-1.30.4/nginx-1.30.4/conf/nginx.conf` | Top-level config; loads `sites/*.conf` |
| `…/conf/sites/jobcard-http.conf` | Port 80: ACME challenge, then redirect to HTTPS |
| `…/conf/sites/jobcard-ssl.conf` | Port 443: the real site |
| `…/conf/sites/localhost-diagnostic.conf` | `http://127.0.0.1:8080` copy, loopback only |
| `…/conf/snippets/` | Shared header and proxy fragments |
| `deploy/setup-admin.ps1` | One-time elevated setup |
| `deploy/reload-nginx.cmd` | Run by win-acme after each renewal |
| `deploy/logs/` | Service stdout/stderr, rotated at 10 MB |
| `deploy-tools/` | Downloaded nssm.exe and win-acme (not in git) |
| `C:\certs\` | Certificate and private key |
| `C:\certs\acme-webroot\` | Where win-acme writes challenge files |

The config under `conf/` is tracked in git. The nginx binary, logs, temp and stock
`html/` `docs/` `contrib/` directories are not — see `.gitignore`.

## First-time setup

### 1. Router — already done

Ports 80 and 443 are **already forwarded** to `192.168.1.9`, and remote/WAN
management is off. Port 80 was confirmed reaching nginx on 2026-08-04 by loading
a marker file from a phone on mobile data. Nothing to do here.

Two things to keep in mind for the future:

- **Port 80 must stay forwarded permanently**, not just for the first
  certificate. win-acme re-validates the same way on every renewal, so removing
  that rule would break renewal about 60 days later — long after the change, which
  makes it an unpleasant thing to debug.
- Give this PC a **static LAN address or DHCP reservation** for `192.168.1.9` if it
  does not have one. If its address changes, both forwards point at nothing.

**Loopback from inside the LAN works.** A request to `jobcard.duckdns.org` from a
machine on the network is looped back by the router and reaches nginx normally, so
office PCs can use the same URL as everyone else and no hosts-file entry is needed.

Worth knowing for future debugging: before the forwards existed, that same request
was answered by the router's own admin service (nginx/1.17.7) instead. So if the
site ever stops working from inside the office and you see an unexpected
nginx/1.17.7 page or a router login, suspect the port forwards have been lost
rather than anything on this PC.

### 2. Run the setup script

In an **Administrator** PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File C:\jobcard\deploy\setup-admin.ps1
```

It removes the stale `caddy` service, sets the firewall rules, obtains the
certificate, enables the HTTPS site, and installs both services. It is safe to
re-run; it checks the current state at every step.

Before calling Let's Encrypt it writes a probe file and fetches it *from the
internet*. If that fails it stops rather than burning failed-validation attempts
against the rate limit — which is the signal that step 1 is not finished.

Useful switches:

- `-SkipCert` — re-run without touching an existing certificate.
- `-Staging` — use Let's Encrypt staging, which has much looser rate limits. The
  certificate is not browser-trusted; it only proves the plumbing works. Worth
  using if a first real attempt failed and you need several tries.

### 3. Xero redirect URI

Register `https://jobcard.duckdns.org/api/xero/callback` in the Xero developer
portal, and set `xeroRedirectUri` in the `settings.settings` document to match.
Xero accepts several redirect URIs, so the desktop app's
`http://localhost:8888/callback` keeps working alongside it.

### 4. Update the bookmark

From `192.168.1.9:4200` to `https://jobcard.duckdns.org`.

## Day-to-day

### Services

Both start automatically at boot. `JobCardAPI` declares a dependency on
`MongoDB`, because it exits deliberately if it cannot connect. `JobCardNginx`
depends on nothing: it serves the Angular app happily without the API, and only
`/api` would return 502 while the API is down. That is on purpose — a dependency
would mean restarting the API after a rebuild took the whole site down with it.

```powershell
Get-Service JobCardAPI, JobCardNginx

Restart-Service JobCardAPI            # after changing .env or rebuilding the API
Restart-Service JobCardNginx

# nginx config change: test first, then reload with no dropped connections
C:\jobcard\nginx-1.30.4\nginx-1.30.4\nginx.exe -t -p C:/jobcard/nginx-1.30.4/nginx-1.30.4
C:\jobcard\nginx-1.30.4\nginx-1.30.4\nginx.exe -s reload -p C:/jobcard/nginx-1.30.4/nginx-1.30.4
```

NSSM restarts either service if it crashes, waiting 5 seconds between attempts.

### Deploying new code

```powershell
cd C:\jobcard\webappShared ; npm run build
cd C:\jobcard\webappUI     ; npm run build      # front end: no restart needed
cd C:\jobcard\webappNode   ; npm run build
Restart-Service JobCardAPI                       # back end: restart required
```

The Angular build is static files that nginx reads per request, so a front-end
deploy needs no restart. Because `index.html` is served `no-store` while the
hashed bundles are cached for a year, browsers pick up a new build on next load
without a stale-cache problem.

### Logs

| File | What |
| --- | --- |
| `deploy/logs/api.log` | API stdout and stderr, including its startup diagnostics |
| `nginx-1.30.4/nginx-1.30.4/logs/access.log` | Every request to the site |
| `nginx-1.30.4/nginx-1.30.4/logs/error.log` | nginx errors — first stop if it will not start |
| `nginx-1.30.4/nginx-1.30.4/logs/acme.log` | Certificate challenge requests |
| `deploy/logs/nginx-service.log` | Anything nginx wrote before opening its own logs |

The API's startup lines are worth reading after any change — they report which
databases it connected to and whether the photo share resolved.

### Certificate renewal

win-acme installed a scheduled task that renews at around 55 days and runs
`deploy/reload-nginx.cmd` afterwards, so nginx picks the new certificate up
without being restarted. Nothing to do by hand.

To check or force it:

```powershell
C:\jobcard\deploy-tools\win-acme\wacs.exe --list
C:\jobcard\deploy-tools\win-acme\wacs.exe --renew --force
```

Let's Encrypt emails `peter@willowsoftware.com` if a certificate is close to
expiring, which is the backstop if the task ever stops running.

## Troubleshooting

**Is it nginx, the app, or TLS?** Browse `http://127.0.0.1:8080` on the PC
itself. That is the same app and API without TLS, bound to loopback only. If it
works and the public URL does not, the fault is the certificate, the port
forward, or DNS — not nginx, Node, or the Angular build.

```powershell
Invoke-WebRequest http://127.0.0.1:8080/api/health -UseBasicParsing   # API + Mongo
Invoke-WebRequest http://127.0.0.1:8080/ -UseBasicParsing             # static files
```

**Works from outside the office but not inside.** Not the current behaviour —
loopback works — so if this starts happening, something has changed. Two causes,
in order of likelihood:

1. **The port forwards have been lost** (router reset, firmware update, or the PC's
   LAN address changed so the rules point at nothing). Check that `192.168.1.9` is
   still this machine's address and that both rules still name it.
2. **The router has started blocking DNS rebinding** — a public hostname resolving
   to a private address. Whitelist `jobcard.duckdns.org` in the router's DNS
   settings.

As a fix that bypasses the router entirely, add this to
`C:\Windows\System32\drivers\etc\hosts` on the affected PC — Notepad must be run as
administrator to save it:

```
192.168.1.9    jobcard.duckdns.org
```

The certificate is valid for that name, so HTTPS keeps working with no warning.
That is why this is better than browsing `https://192.168.1.9` directly: the
certificate does not cover a bare IP address, so that warns on every visit.

**nginx will not start.** Read `logs/error.log`. Usually either a port already
taken (`netstat -ano | findstr ":443 "`) or a missing certificate file.

**Certificate request failed.** Nearly always the port 80 forward. Confirm from
outside the network that `http://jobcard.duckdns.org/.well-known/acme-challenge/x`
reaches nginx rather than the router — nginx logs every such request to
`logs/acme.log`, so an empty log means the request never arrived. Use `-Staging`
while debugging to avoid the rate limit.

Test this from a phone with WiFi off, not from the PC. A request made from inside
the network has to be looped back by the router (NAT hairpinning) and many routers
simply will not do that, so it can fail while real outside traffic works fine. The
setup script's own internet check has the same limitation, which is why it asks
whether to continue rather than giving up.

**Port 443 times out from outside but 80 works.** Only the 443 forward is
missing; step 1.3.

## Known issues, deliberately left open

These were found while deploying and consciously deferred. None of them is
caused by the nginx setup; all three predate it.

1. **The login password is the public default.** `webappNode/.env` sets
   `AUTH_PASSWORD_SHA256` to `043096f8…d0732`, which is byte-for-byte
   `DEFAULT_AUTH_PASSWORD_SHA256` in `webappNode/src/server-settings.ts` — a
   tracked file in a public GitHub repo. The salt (`jobcard-wheel:`) is public
   too and the hash is a single unstretched SHA-256, so it is cheap to attack
   offline. Anyone who recovers it gets every job, customer and photo, from
   anywhere on the internet.

   To fix, pick a new password and set both:

   ```powershell
   node -e "console.log(require('crypto').createHash('sha256').update('jobcard-wheel:'+process.argv[1]).digest('hex'))" "the new password"
   ```

   Put that hash in `AUTH_PASSWORD_SHA256` in `webappNode/.env`, then
   `Restart-Service JobCardAPI`. The API prints a warning on every start until
   this is done. Note `AUTH_PASSWORD=` in `.env` is dead config — nothing reads
   it; only the hash matters.

   `AUTH_SECRET` is *not* affected: it is a proper random value and `.env` is
   untracked.

2. **MongoDB has no authentication** and listens on `0.0.0.0:27017`, so anyone on
   the office LAN can read and write every job and the settings document. Fixing
   it means `security.authorization: enabled` plus a user for each app, and
   touching the desktop app's connection string. Not internet-exposed — 27017 is
   not forwarded.

3. **The API binds `0.0.0.0:3000`, not localhost.** `webappNode/README.md` says
   it listens on localhost only, but `app.listen(port)` with no host binds every
   interface. The setup script therefore adds a firewall rule blocking inbound
   3000, which is why HTTPS cannot be bypassed from the LAN. The tidier fix is
   `app.listen(config.port, '127.0.0.1')` in `webappNode/src/server.ts`, after
   which the firewall rule is belt-and-braces.

## Reference

- nginx 1.30.4, Windows build, in `nginx-1.30.4/nginx-1.30.4/`. Stock config kept
  as `conf/nginx.conf.default-backup`.
- NSSM 2.24 — <https://nssm.cc/release/nssm-2.24.zip>
- win-acme 2.2.9.1701 x64 trimmed —
  <https://github.com/win-acme/win-acme/releases/download/v2.2.9.1701/win-acme.v2.2.9.1701.x64.trimmed.zip>
- The old `C:\caddy\Caddyfile.txt` is left on disk as a reference for what the
  previous Caddy setup did. The `caddy` service itself is removed.
