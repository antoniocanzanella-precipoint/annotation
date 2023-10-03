param(
    [string]$HostSlideDirectory = "C:\Slides"
)

$ErrorActionPreference = "Stop"
$StartTime = Get-Date

& .\tests_plan\20-annota_application.ps1
& .\tests_plan\30-annota_http.ps1 -HostSlideDirectory $HostSlideDirectory

$EndTime = Get-Date
$ElapsedTime = New-TimeSpan -Start $StartTime -End $EndTime
$TimeString = '{0:hh\:mm\:ss}' -f $ElapsedTime

Write-Host "It took $timeString to complete database and integration tests." -ForegroundColor Green