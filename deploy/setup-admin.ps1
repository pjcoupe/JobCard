<#
    Job Card web app -- one-time deployment setup. MUST run elevated.

    Open an ADMINISTRATOR PowerShell and run:

        powershell -ExecutionPolicy Bypass -File C:\jobcard\deploy\setup-admin.ps1

    What it does, in order:
      1. Removes the leftover, broken "caddy" service that would fight for 80/443
      2. Opens ports 80 and 443 in Windows Firewall, and closes port 3000
      3. Obtains the Let's Encrypt certificate with win-acme (HTTP-01)
      4. Enables the nginx HTTPS site now that a certificate exists
      5. Installs JobCardAPI and JobCardNginx as auto-starting services via NSSM

    Safe to run more than once: every step checks the current state first, and
    nothing is destroyed on a re-run. Use -SkipCert to re-run without touching a
    certificate you already have.

    BEFORE RUNNING: ports 80 and 443 must be forwarded from the router to
    192.168.1.9, or step 3 fails -- Let's Encrypt has to reach this machine over
    port 80 from the internet to verify you control the name. The script checks
    this for you before it calls Let's Encrypt, and stops if it cannot.
#>

[CmdletBinding()]
param(
    # Re-run service/firewall setup without re-issuing the certificate.
    [switch]$SkipCert,

    # Use Let's Encrypt's staging environment instead of the real one. Staging has
    # far looser rate limits, so use it if a first attempt failed and you need to
    # retry several times. The resulting certificate is NOT browser-trusted -- it
    # only proves the plumbing works.
    [switch]$Staging
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------- configuration
$Hostname     = 'jobcard.duckdns.org'
$ContactEmail = 'peter@willowsoftware.com'

$NginxHome    = 'C:\jobcard\nginx-1.30.4\nginx-1.30.4'
$NginxPrefix  = 'C:/jobcard/nginx-1.30.4/nginx-1.30.4'
$NginxExe     = "$NginxHome\nginx.exe"
$ApiHome      = 'C:\jobcard\webappNode'
$NodeExe      = 'C:\Program Files\nodejs\node.exe'

$CertDir      = 'C:\certs'
$AcmeWebroot  = 'C:\certs\acme-webroot'
$Tools        = 'C:\jobcard\deploy-tools'
$Nssm         = "$Tools\nssm.exe"
$Wacs         = "$Tools\win-acme\wacs.exe"
$ReloadScript = 'C:\jobcard\deploy\reload-nginx.cmd'

$LogDir       = 'C:\jobcard\deploy\logs'

$ApiService   = 'JobCardAPI'
$NginxService = 'JobCardNginx'

# --------------------------------------------------------------------- helpers
$script:StepNumber = 0
function Step([string]$Title) {
    $script:StepNumber++
    Write-Host ''
    Write-Host ('=' * 74) -ForegroundColor Cyan
    Write-Host ("STEP $($script:StepNumber): $Title") -ForegroundColor Cyan
    Write-Host ('=' * 74) -ForegroundColor Cyan
}
function Ok([string]$m)   { Write-Host "  [ok]   $m" -ForegroundColor Green }
function Info([string]$m) { Write-Host "  [info] $m" -ForegroundColor Gray }
function Warn([string]$m) { Write-Host "  [warn] $m" -ForegroundColor Yellow }
function Fail([string]$m) { Write-Host "  [FAIL] $m" -ForegroundColor Red; throw $m }

<#
    Run a console program and collect its output without tripping over two
    Windows PowerShell 5.1 quirks:

      * Merging a native program's stderr with "2>&1" wraps each stderr line in an
        ErrorRecord. Under $ErrorActionPreference = 'Stop' that becomes a
        terminating error, so a program that merely prints a warning -- nginx -t
        writes its success message to stderr -- would abort this script.
      * $? and the error stream are unreliable for native programs generally. The
        exit code is the only trustworthy signal, so that is what we return.
#>
function Invoke-Native {
    param(
        [Parameter(Mandatory)][string]$Exe,
        [string[]]$Arguments = @()
    )
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $lines = & $Exe @Arguments 2>&1 | ForEach-Object { $_.ToString() }
        $code  = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previous
    }
    return [pscustomobject]@{
        Output   = @($lines)
        ExitCode = $code
    }
}

function Test-ServiceExists([string]$Name) {
    return ($null -ne (Get-Service -Name $Name -ErrorAction SilentlyContinue))
}

function Stop-NginxCompletely {
    if (Test-Path "$NginxHome\logs\nginx.pid") {
        Invoke-Native -Exe $NginxExe -Arguments @('-p', $NginxPrefix, '-s', 'stop') | Out-Null
        Start-Sleep -Seconds 2
    }
    Get-Process -Name nginx -ErrorAction SilentlyContinue |
        Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
}

# ------------------------------------------------------------- pre-flight checks
Step 'Pre-flight checks'

$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host ''
    Write-Host 'This script must run as Administrator.' -ForegroundColor Red
    Write-Host 'Right-click PowerShell -> Run as administrator, then run:' -ForegroundColor Red
    Write-Host '  powershell -ExecutionPolicy Bypass -File C:\jobcard\deploy\setup-admin.ps1' -ForegroundColor Red
    exit 1
}
Ok 'running elevated'

foreach ($required in @($Nssm, $Wacs, $NodeExe, $ReloadScript, $NginxExe)) {
    if (-not (Test-Path $required)) { Fail "missing required file: $required" }
}
Ok 'nssm, wacs, node, nginx and the reload script are all present'

if (-not (Test-Path "$ApiHome\dist\server.js")) {
    Fail "the API is not built -- run 'npm run build' in $ApiHome first"
}
if (-not (Test-Path 'C:\jobcard\webappUI\dist\webapp-ui\browser\index.html')) {
    Fail "the Angular app is not built -- run 'npm run build' in C:\jobcard\webappUI first"
}
Ok 'both the API and the Angular app are built'

New-Item -ItemType Directory -Force -Path $CertDir, $AcmeWebroot, $LogDir | Out-Null
Ok "log directory ready at $LogDir"

# --------------------------------------------------- 1. remove the caddy service
Step 'Remove the leftover caddy service'

if (Test-ServiceExists 'caddy') {
    Info 'found a "caddy" service set to auto-start'
    Info 'it is already broken (C:\caddy has no caddy.exe and its config is misnamed),'
    Info 'but while registered it will try for ports 80 and 443 on every boot.'
    try { Stop-Service -Name 'caddy' -Force -ErrorAction Stop } catch { Info 'it was not running' }
    Invoke-Native -Exe 'sc.exe' -Arguments @('delete', 'caddy') | Out-Null
    Start-Sleep -Seconds 2
    if (Test-ServiceExists 'caddy') {
        Warn 'still listed -- a reboot will finish removing it'
    } else {
        Ok 'caddy service deleted'
    }
    Info 'C:\caddy\Caddyfile.txt is left on disk for reference; delete it whenever you like.'
} else {
    Ok 'no caddy service registered -- nothing to do'
}

# ------------------------------------------------------------- 2. Windows Firewall
Step 'Windows Firewall rules'

# netsh rather than the New-NetFirewallRule cmdlets on purpose: the NetSecurity and
# NetTCPIP modules are not present on this machine, and netsh always is.
function Set-FirewallRule {
    param([string]$Name, [string[]]$Spec)

    # Delete first so a re-run updates the rule instead of stacking duplicates.
    # A missing rule makes netsh exit non-zero, which is fine and expected here.
    Invoke-Native -Exe 'netsh' -Arguments (@('advfirewall', 'firewall', 'delete', 'rule', "name=$Name")) | Out-Null

    $result = Invoke-Native -Exe 'netsh' -Arguments (@('advfirewall', 'firewall', 'add', 'rule', "name=$Name") + $Spec)
    if ($result.ExitCode -ne 0) {
        $result.Output | ForEach-Object { Warn $_ }
        Fail "could not add firewall rule '$Name'"
    }
    Ok "rule set: $Name"
}

Set-FirewallRule 'Job Card web app - HTTP 80' `
    @('dir=in', 'action=allow', 'protocol=TCP', 'localport=80', 'profile=any')

Set-FirewallRule 'Job Card web app - HTTPS 443' `
    @('dir=in', 'action=allow', 'protocol=TCP', 'localport=443', 'profile=any')

# The API binds 0.0.0.0:3000, not localhost -- so without this rule anything on the
# office LAN can reach it directly over plain HTTP, bypassing nginx and HTTPS
# altogether. nginx is unaffected: it connects over 127.0.0.1, and loopback traffic
# never passes through Windows Firewall.
Set-FirewallRule 'Job Card API - block external 3000' `
    @('dir=in', 'action=block', 'protocol=TCP', 'localport=3000', 'profile=any')

Info 'port 27017 is deliberately left alone -- the desktop app needs MongoDB over the LAN.'

# ------------------------------------------------- 3. stop anything on the ports
Step 'Stop anything already using the ports'

# Stop the services BEFORE hunting stray processes. Order matters on a re-run: the
# API service's own node process matches the stray pattern below, so killing it
# first would just provoke NSSM into restarting it (AppExit Restart, 5s delay), and
# the port check further down would then find port 3000 taken by that fresh
# instance and fail for no good reason.
foreach ($svc in @($NginxService, $ApiService)) {
    if (Test-ServiceExists $svc) {
        $state = (Get-Service $svc).Status
        if ($state -ne 'Stopped') {
            Info "stopping $svc (was $state)"
            # nssm stop rather than Stop-Service: it also handles the Paused state
            # NSSM parks a service in when it is throttling restarts, which
            # Stop-Service refuses to act on.
            Invoke-Native -Exe $Nssm -Arguments @('stop', $svc) | Out-Null
            Start-Sleep -Seconds 3
        }
    }
}
Ok 'existing services stopped'

Stop-NginxCompletely
Ok 'no nginx running'

# Only stop node processes running THIS app, so an unrelated node stays up.
#
# Matched with a regex accepting either slash, because both spellings occur in the
# wild: "npm start" produces dist\server.js, while anything launched from a shell
# or a script usually produces dist/server.js, and Windows honours both. An earlier
# version of this matched only the backslash form and so silently failed to stop a
# forward-slash process -- which then kept port 3000 and made the API service fail
# to start with EADDRINUSE, several steps later and with no obvious connection.
$apiProcs = @(Get-CimInstance Win32_Process -Filter "Name = 'node.exe'" -ErrorAction SilentlyContinue |
              Where-Object { $_.CommandLine -and $_.CommandLine -match 'dist[\\/]server\.js' })
foreach ($p in $apiProcs) {
    Info "stopping API process $($p.ProcessId)"
    Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2
Ok 'no stray API process running'

# Confirm the port is genuinely free rather than assuming the loop above caught
# everything. Anything still holding 3000 would make the API service fail with
# EADDRINUSE at step 8, which reads as "the service is broken" rather than "another
# process has the port" -- so it is worth naming the culprit here instead.
#
# netstat because Get-NetTCPListener needs the NetTCPIP module, absent on this box.
$holders = @(netstat -ano |
             Select-String -Pattern ':3000\s' |
             Select-String -Pattern 'LISTENING' |
             ForEach-Object { ($_.Line.Trim() -split '\s+')[-1] } |
             Sort-Object -Unique)
if ($holders.Count -gt 0) {
    foreach ($pid3000 in $holders) {
        $owner = Get-CimInstance Win32_Process -Filter "ProcessId = $pid3000" -ErrorAction SilentlyContinue
        if ($owner) { Warn "PID $pid3000 still holds port 3000: $($owner.Name) -- $($owner.CommandLine)" }
        else        { Warn "PID $pid3000 still holds port 3000 (process details unavailable)" }
    }
    Fail ('Port 3000 is still in use, so the API service could not start. Stop the ' +
          'process listed above (Stop-Process -Id <pid> -Force) and re-run with -SkipCert.')
}
Ok 'port 3000 is free'

# ---------------------------------------------------------- 4. the certificate
if ($SkipCert) {
    Step 'Certificate (skipped by -SkipCert)'
    Info 'leaving the existing certificate alone'
} else {
    Step 'Obtain the Let''s Encrypt certificate (win-acme, HTTP-01)'

    # nginx must be up to answer the challenge over port 80, but the HTTPS site
    # cannot load yet with no certificate on disk. jobcard-http.conf on its own
    # serves the challenge path, which is exactly what validation needs.
    Info 'starting nginx to serve the challenge'
    Start-Process -FilePath $NginxExe -ArgumentList '-p', $NginxPrefix `
                  -WorkingDirectory $NginxHome -WindowStyle Hidden
    Start-Sleep -Seconds 3
    if (-not (Get-Process -Name nginx -ErrorAction SilentlyContinue)) {
        Fail "nginx would not start -- check $NginxHome\logs\error.log"
    }
    Ok 'nginx is up on port 80'

    # Prove the challenge path works locally before involving Let's Encrypt, whose
    # rate limit on failed validations is not generous.
    #
    # No Host header is needed: the port 80 server is the default_server, so it
    # answers whatever hostname the request arrives with. (Just as well -- 5.1's
    # Invoke-WebRequest refuses to set a Host header at all.)
    $probeName = 'setup-probe'
    $probeDir  = Join-Path $AcmeWebroot '.well-known\acme-challenge'
    $probePath = Join-Path $probeDir $probeName
    New-Item -ItemType Directory -Force -Path $probeDir | Out-Null
    Set-Content -Path $probePath -Value 'probe' -Encoding ascii

    try {
        $probe = Invoke-WebRequest -Uri "http://127.0.0.1/.well-known/acme-challenge/$probeName" `
                                   -UseBasicParsing -TimeoutSec 15
        if ($probe.Content.Trim() -ne 'probe') { Fail 'the challenge path served unexpected content' }
        Ok 'challenge path works locally'
    } catch {
        Fail "nginx is not serving the challenge path: $($_.Exception.Message)"
    }

    # Now try the same file over the hostname, the way Let's Encrypt will fetch it.
    #
    # This normally succeeds: the router does loop a LAN-side request back to nginx.
    # It is still only advisory, because the request never actually leaves the
    # network -- a router that stopped looping back would report a failure here even
    # with the forwards perfectly intact. Before the forwards existed this returned
    # the router's own admin page (nginx/1.17.7) instead, which is why a non-matching
    # response is reported as "intercepted" rather than treated as fatal.
    Info 'checking the hostname from this machine (advisory only -- see comment)'
    $externalState = 'failed'
    try {
        $ext = Invoke-WebRequest -Uri "http://$Hostname/.well-known/acme-challenge/$probeName" `
                                 -UseBasicParsing -TimeoutSec 25
        if ($ext.Content.Trim() -eq 'probe') { $externalState = 'confirmed' }
        else { $externalState = 'intercepted' }
    } catch {
        Info "fetch failed: $($_.Exception.Message)"
    }

    # The probe file is deliberately left in place until after the decision below:
    # if the automatic check failed, the next thing to try is opening that same URL
    # on a phone, and that only works while the file still exists.
    if ($externalState -eq 'confirmed') {
        Ok 'the challenge file came back over the hostname -- the forward works'
    } else {
        Write-Host ''
        if ($externalState -eq 'intercepted') {
            Info 'Something answered, but not with our file -- most likely the router''s'
            Info 'own admin page, which is what it serves when the port 80 forward is'
            Info 'missing. Worth checking the forward before continuing.'
        } else {
            Info 'No answer over the hostname from this machine. That may just mean the'
            Info 'router will not loop a request back to itself, which says nothing'
            Info 'about real traffic from outside -- so it is not conclusive either way.'
        }
        Write-Host ''
        Info 'To settle it, leave this prompt open and load this on a phone with WiFi OFF:'
        Write-Host "    http://$Hostname/.well-known/acme-challenge/$probeName" -ForegroundColor White
        Info 'It should show the word "probe". The file is deleted once you answer, so'
        Info 'check BEFORE typing anything here.'
        Write-Host ''
        Info 'Answer n only if the phone shows a router page or nothing: Let''s Encrypt'
        Info 'would then fail the same way, and repeated failures hit a rate limit.'
        Write-Host ''
        $answer = Read-Host 'Request the certificate? (Y/n)'
        if ($answer -eq 'n') {
            Info 'stopping here. Re-run once the forward is confirmed.'
            Remove-Item $probePath -Force -ErrorAction SilentlyContinue
            Stop-NginxCompletely
            exit 1
        }
    }

    Remove-Item $probePath -Force -ErrorAction SilentlyContinue

    # Argument names verified against wacs.exe --help for 2.2.9.1701. Note it is
    # --source, not --target: --target was the 2.1.x name and no longer exists.
    # --pemfilesname is set explicitly so the output filenames are guaranteed to
    # match the ssl_certificate paths in jobcard-ssl.conf rather than depending on
    # how win-acme derives a name from the common name.
    $wacsArgs = @(
        '--source', 'manual',
        '--host', $Hostname,
        '--friendlyname', $Hostname,
        '--validation', 'filesystem',
        '--webroot', $AcmeWebroot,
        '--store', 'pemfiles',
        '--pemfilespath', $CertDir,
        '--pemfilesname', $Hostname,
        '--installation', 'script',
        '--script', $ReloadScript,
        '--accepttos',
        '--emailaddress', $ContactEmail
    )
    if ($Staging) {
        $wacsArgs += @('--baseuri', 'https://acme-staging-v02.api.letsencrypt.org/')
        Warn 'using the STAGING environment -- the certificate will not be browser-trusted'
    }

    Info "running: wacs.exe $($wacsArgs -join ' ')"
    Write-Host ''
    # Deliberately NOT via Invoke-Native: win-acme's output is worth watching live,
    # and if anything does need a keystroke this leaves it able to ask.
    & $Wacs @wacsArgs
    $wacsExit = $LASTEXITCODE
    Write-Host ''
    if ($wacsExit -ne 0) { Fail "win-acme exited with code $wacsExit -- see its output above" }

    $chain = Join-Path $CertDir "$Hostname-chain.pem"
    $key   = Join-Path $CertDir "$Hostname-key.pem"
    if ((-not (Test-Path $chain)) -or (-not (Test-Path $key))) {
        Warn 'win-acme finished, but not with the filenames the nginx config expects.'
        Warn 'These are the .pem files it actually wrote:'
        Get-ChildItem $CertDir -Filter '*.pem' -ErrorAction SilentlyContinue |
            ForEach-Object { Warn "    $($_.Name)" }
        Fail ("expected '$Hostname-chain.pem' and '$Hostname-key.pem'. Point " +
              "ssl_certificate / ssl_certificate_key in conf\sites\jobcard-ssl.conf.disabled " +
              "at the real names, then re-run with -SkipCert.")
    }
    Ok "certificate written: $chain"
    Ok "private key written: $key"
}

# ------------------------------------------------------ 5. enable the HTTPS site
Step 'Enable the nginx HTTPS site'

$sslDisabled = "$NginxHome\conf\sites\jobcard-ssl.conf.disabled"
$sslEnabled  = "$NginxHome\conf\sites\jobcard-ssl.conf"

if (Test-Path $sslEnabled) {
    Ok 'HTTPS site is already enabled'
    if (Test-Path $sslDisabled) { Remove-Item $sslDisabled -Force }
} elseif (Test-Path $sslDisabled) {
    Move-Item $sslDisabled $sslEnabled -Force
    Ok 'renamed jobcard-ssl.conf.disabled -> jobcard-ssl.conf'
} else {
    Fail "neither $sslEnabled nor $sslDisabled exists"
}

Info 'validating the full configuration, now including TLS'
$test = Invoke-Native -Exe $NginxExe -Arguments @('-t', '-p', $NginxPrefix)
$test.Output | ForEach-Object { Info $_ }
if ($test.ExitCode -ne 0) { Fail 'nginx configuration is invalid -- see above' }
Ok 'configuration is valid'

# Stop the foreground nginx; from here on the service owns it.
Stop-NginxCompletely
Ok 'foreground nginx stopped, ready to hand over to the service'

# ---------------------------------------------------------------- 6. services
Step 'Install the Windows services'

function Remove-ExistingService([string]$Name) {
    if (Test-ServiceExists $Name) {
        Info "$Name already exists -- removing it so settings apply cleanly"
        Invoke-Native -Exe $Nssm -Arguments @('stop', $Name) | Out-Null
        Start-Sleep -Seconds 2
        Invoke-Native -Exe $Nssm -Arguments @('remove', $Name, 'confirm') | Out-Null
        Start-Sleep -Seconds 2
    }
}

function Set-NssmValue {
    param([string]$Service, [string[]]$Setting)
    $r = Invoke-Native -Exe $Nssm -Arguments (@('set', $Service) + $Setting)
    if ($r.ExitCode -ne 0) {
        $r.Output | ForEach-Object { Warn $_ }
        Fail "nssm set $Service $($Setting -join ' ') failed"
    }
}

# --- the API -----------------------------------------------------------------
Remove-ExistingService $ApiService

$r = Invoke-Native -Exe $Nssm -Arguments @('install', $ApiService, $NodeExe, 'dist\server.js')
if ($r.ExitCode -ne 0) { $r.Output | ForEach-Object { Warn $_ }; Fail "could not install $ApiService" }

Set-NssmValue $ApiService @('DisplayName', 'Job Card API (Node)')
Set-NssmValue $ApiService @('Description', 'Backend API for the Job Card web app. Serves /api behind nginx and reads MongoDB.')
Set-NssmValue $ApiService @('AppDirectory', $ApiHome)
Set-NssmValue $ApiService @('Start', 'SERVICE_AUTO_START')

# Mongo must be up first, or the API exits deliberately (server.ts calls
# process.exit(1) when it cannot connect). NSSM would keep restarting it until
# Mongo appeared, but the dependency makes boot orderly rather than noisy.
Set-NssmValue $ApiService @('DependOnService', 'MongoDB')

# Capture the API's own diagnostics. Its startup lines -- which databases it
# connected to, whether the photo share resolved -- are the first thing worth
# reading when something is wrong, and a service has no console to print them to.
Set-NssmValue $ApiService @('AppStdout', "$LogDir\api.log")
Set-NssmValue $ApiService @('AppStderr', "$LogDir\api.log")
Set-NssmValue $ApiService @('AppRotateFiles', '1')
Set-NssmValue $ApiService @('AppRotateOnline', '1')
Set-NssmValue $ApiService @('AppRotateBytes', '10485760')

# Restart on crash, with a delay so a persistent failure does not spin the CPU.
Set-NssmValue $ApiService @('AppExit', 'Default', 'Restart')
Set-NssmValue $ApiService @('AppRestartDelay', '5000')
Ok "$ApiService installed"

# --- nginx -------------------------------------------------------------------
Remove-ExistingService $NginxService

$r = Invoke-Native -Exe $Nssm -Arguments @('install', $NginxService, $NginxExe, '-p', $NginxPrefix)
if ($r.ExitCode -ne 0) { $r.Output | ForEach-Object { Warn $_ }; Fail "could not install $NginxService" }

Set-NssmValue $NginxService @('DisplayName', 'Job Card nginx (HTTPS front end)')
Set-NssmValue $NginxService @('Description', 'Serves the Job Card Angular app over HTTPS and proxies /api to the Node API.')
Set-NssmValue $NginxService @('AppDirectory', $NginxHome)
Set-NssmValue $NginxService @('Start', 'SERVICE_AUTO_START')

# Deliberately NOT dependent on JobCardAPI. nginx serves the Angular app perfectly
# well without the API -- only /api would return 502 -- and a dependency would mean
# every "Restart-Service JobCardAPI" after a rebuild dragged the whole site down
# with it, and refused to run at all without -Force.

Set-NssmValue $NginxService @('AppStdout', "$LogDir\nginx-service.log")
Set-NssmValue $NginxService @('AppStderr', "$LogDir\nginx-service.log")
Set-NssmValue $NginxService @('AppRotateFiles', '1')
Set-NssmValue $NginxService @('AppRotateOnline', '1')
Set-NssmValue $NginxService @('AppRotateBytes', '10485760')

Set-NssmValue $NginxService @('AppExit', 'Default', 'Restart')
Set-NssmValue $NginxService @('AppRestartDelay', '5000')

# nginx runs a master plus a worker, and stopping it has to take the worker with it
# -- a worker left holding port 80 would make the next start fail to bind.
#
# 0 means "use every stop method in order": Ctrl+C, then WM_CLOSE, then WM_QUIT,
# then terminate. The first one is the one that matters, because nginx treats
# Ctrl+C as a fast shutdown and brings its worker down itself.
#
# NSSM 2.24 has no pre-stop hook to run "nginx -s stop" with (AppEvents arrived in
# 2.25, and AppKillProcessTree likewise), so the Ctrl+C path is the mechanism here
# rather than a nicety.
Set-NssmValue $NginxService @('AppStopMethodSkip', '0')
Ok "$NginxService installed"

# ------------------------------------------------------------------ 7. start up
Step 'Start the services'

Start-Service $ApiService
Start-Sleep -Seconds 8
$apiState = (Get-Service $ApiService).Status
if ($apiState -ne 'Running') { Fail "$ApiService is $apiState -- check $LogDir\api.log" }
Ok "$ApiService is running"

Start-Service $NginxService
Start-Sleep -Seconds 4
$nginxState = (Get-Service $NginxService).Status
if ($nginxState -ne 'Running') { Fail "$NginxService is $nginxState -- check $NginxHome\logs\error.log" }
Ok "$NginxService is running"

# ------------------------------------------------------------------ 8. verify
Step 'Verify'

try {
    $h = Invoke-WebRequest -Uri 'http://127.0.0.1:8080/api/health' -UseBasicParsing -TimeoutSec 20
    Ok "API through nginx: $($h.Content)"
} catch {
    Warn "API health check failed: $($_.Exception.Message)"
}

try {
    $s = Invoke-WebRequest -Uri 'http://127.0.0.1:8080/' -UseBasicParsing -TimeoutSec 20
    Ok "Angular app through nginx: HTTP $($s.StatusCode), $($s.RawContentLength) bytes"
} catch {
    Warn "static app check failed: $($_.Exception.Message)"
}

if (Test-Path (Join-Path $CertDir "$Hostname-chain.pem")) {
    try {
        # 5.1 has no -SkipCertificateCheck, so trust everything for this one check.
        # The point is that TLS negotiates and the app is served; whether the chain
        # validates is what the browser test from off-site is for. (Connecting to
        # 127.0.0.1 would fail a name check regardless.)
        Add-Type -TypeDefinition @'
using System.Net;
public static class JobCardTempCertPolicy {
    public static void Allow() {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }
}
'@ -ErrorAction SilentlyContinue
        [JobCardTempCertPolicy]::Allow()
        $t = Invoke-WebRequest -Uri 'https://127.0.0.1/' -UseBasicParsing -TimeoutSec 20
        Ok "HTTPS responds locally: HTTP $($t.StatusCode), $($t.RawContentLength) bytes"
    } catch {
        Warn "local HTTPS check failed: $($_.Exception.Message)"
    } finally {
        [Net.ServicePointManager]::ServerCertificateValidationCallback = $null
    }

    try {
        $cert = New-Object Security.Cryptography.X509Certificates.X509Certificate2((Join-Path $CertDir "$Hostname-chain.pem"))
        Ok "certificate subject: $($cert.Subject), expires $($cert.NotAfter.ToString('yyyy-MM-dd'))"
    } catch {
        Info 'could not parse the certificate for an expiry date (harmless)'
    }
}

Write-Host ''
Write-Host 'Renewal task registered by win-acme:' -ForegroundColor Cyan
$tasks = Invoke-Native -Exe 'schtasks' -Arguments @('/query', '/fo', 'LIST')
$found = $tasks.Output | Select-String -Pattern 'win-acme' -SimpleMatch
if ($found) { $found | ForEach-Object { Info $_.ToString().Trim() } }
else { Warn 'no win-acme task found -- run "wacs.exe" once interactively and choose the scheduled-task option' }

Write-Host ''
Write-Host ('=' * 74) -ForegroundColor Green
Write-Host 'SETUP COMPLETE' -ForegroundColor Green
Write-Host ('=' * 74) -ForegroundColor Green
Write-Host ''
Write-Host 'Services (both auto-start at boot):'
Get-Service $ApiService, $NginxService | Select-Object Name, Status, StartType | Format-Table -AutoSize
Write-Host 'Now test from a phone on mobile data, NOT office WiFi:'
Write-Host "    https://$Hostname" -ForegroundColor White
Write-Host ''
Write-Host 'Then reboot to confirm both services come back on their own.'
Write-Host 'Remaining manual steps are in C:\jobcard\deploy\DEPLOYMENT.md.'
Write-Host ''
