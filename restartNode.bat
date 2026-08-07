powershell -Command "Restart-Service -Name JobCardAPI -Force"
powershell -Command "Get-Service JobCardAPI | Select-Object Name, Status"
