@echo off
REM Reload nginx so it picks up a renewed certificate.
REM
REM win-acme runs this after every successful renewal (it is wired in as the
REM "script" installation step, so it is stored in the renewal and repeats
REM automatically). Without it nginx would keep serving the old certificate from
REM memory until something restarted it -- which is the classic way an
REM auto-renewing setup still manages to expire.
REM
REM Runs as SYSTEM from the win-acme scheduled task, so no assumptions about a
REM logged-on user or a mapped drive.

set NGINX_HOME=C:\jobcard\nginx-1.30.4\nginx-1.30.4
set NGINX_PREFIX=C:/jobcard/nginx-1.30.4/nginx-1.30.4

echo [reload-nginx] verifying configuration before reloading
"%NGINX_HOME%\nginx.exe" -t -p "%NGINX_PREFIX%"
if errorlevel 1 (
    echo [reload-nginx] ERROR: nginx config is invalid, refusing to reload.
    echo [reload-nginx] The old certificate stays in use. Fix the config and rerun.
    exit /b 1
)

echo [reload-nginx] reloading
"%NGINX_HOME%\nginx.exe" -s reload -p "%NGINX_PREFIX%"
if errorlevel 1 goto restart_service

echo [reload-nginx] reloaded successfully
exit /b 0

:restart_service
REM A reload needs a running master process with a readable pid file. If that
REM failed, fall back to bouncing the service. Costs a second of downtime, which
REM beats serving an expired certificate.
echo [reload-nginx] reload failed, restarting the service instead
net stop JobCardNginx
net start JobCardNginx
if errorlevel 1 (
    echo [reload-nginx] ERROR: could not restart JobCardNginx. Check it by hand.
    exit /b 1
)
echo [reload-nginx] service restarted
exit /b 0
