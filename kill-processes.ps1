<#
  Kill Frontend and Backend Dev Processes
  - Backend: stop any running Kcow.Api process and anything bound to the dev ports
  - Frontend: stop only the Node process bound to the dev port

  This intentionally avoids killing *all* dotnet/node processes on your machine.
#>

Write-Host "Killing KCOW dev processes (safe mode)..." -ForegroundColor Yellow

function Stop-ProcessById([int]$ProcessId, [string]$Reason)
{
  $proc = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue %
  if (-not $proc)
  { return 
  }
  Write-Host "  Killing PID $ProcessId ($($proc.ProcessName)) - $Reason" -ForegroundColor Gray
  Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
}

function Get-PidsOnPort([int]$Port)
{
  try
  {
    return Get-NetTCPConnection -LocalPort $Port -ErrorAction Stop | Select-Object -ExpandProperty OwningProcess -Unique
  } catch
  {
    return @()
  }
}

# Backend: stop Kcow.Api if it exists (this is what was locking the DLLs)
Write-Host "`n[Backend] Searching for Kcow.Api process..." -ForegroundColor Cyan
$kcowApi = Get-Process -Name "Kcow.Api" -ErrorAction SilentlyContinue
if ($kcowApi)
{
  foreach ($proc in $kcowApi)
  {
    Stop-ProcessById -ProcessId $proc.Id -Reason "Kcow.Api"
  }
} else
{
  Write-Host "  No Kcow.Api process found." -ForegroundColor Gray
}

# Backend: stop anything bound to the KCOW dev ports
$backendPorts = @(5039, 7278)
Write-Host "`n[Backend] Searching for processes on ports $($backendPorts -join ', ')..." -ForegroundColor Cyan
foreach ($port in $backendPorts)
{
  $pids = Get-PidsOnPort -Port $port
  foreach ($processId in $pids)
  {
    Stop-ProcessById -ProcessId $processId -Reason "port $port"
  }
}

# Frontend: stop only node bound to port 4200
Write-Host "`n[Frontend] Searching for Node process on port 4200..." -ForegroundColor Cyan
$frontendPids = Get-PidsOnPort -Port 4200
foreach ($processId in $frontendPids)
{
  $proc = Get-Process -Id $processId -ErrorAction SilentlyContinue
  if ($proc -and $proc.ProcessName -eq "node")
  {
    Stop-ProcessById -ProcessId $processId -Reason "frontend dev port 4200"
  }
}

Write-Host "`nDone! Waiting 2 seconds for processes to fully terminate..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Write-Host "`nProcess cleanup complete!" -ForegroundColor Green
