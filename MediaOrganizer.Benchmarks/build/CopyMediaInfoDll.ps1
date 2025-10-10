param(
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release"
)

# MediaInfo.dll copy script for MediaOrganizer.Benchmarks
# This script copies MediaInfo.dll from common install locations to the output folder
# if it doesn't already exist there.

Write-Host "MediaInfo.dll Copy Script" -ForegroundColor Green
Write-Host "Output Path: $OutputPath" -ForegroundColor Gray
Write-Host "Configuration: $Configuration" -ForegroundColor Gray

# Common MediaInfo installation paths
$MediaInfoPaths = @(
    "C:\Program Files\MediaInfo\MediaInfo.dll",
    "C:\Program Files (x86)\MediaInfo\MediaInfo.dll"
)

# Target locations
$OutputDll = Join-Path $OutputPath "MediaInfo.dll"
$RuntimeX64 = Join-Path $OutputPath "runtimes\win-x64\native\MediaInfo.dll"
$RuntimeX86 = Join-Path $OutputPath "runtimes\win-x86\native\MediaInfo.dll"

# Find MediaInfo.dll on the system
$SourceDll = $null
foreach ($Path in $MediaInfoPaths) {
    if (Test-Path $Path) {
        $SourceDll = $Path
        Write-Host "Found MediaInfo.dll at: $SourceDll" -ForegroundColor Green
        break
    }
}

if (-not $SourceDll) {
    Write-Warning "MediaInfo.dll not found in common installation paths. MediaInfo benchmarks may fail."
    Write-Host "Searched paths:" -ForegroundColor Yellow
    foreach ($Path in $MediaInfoPaths) {
        Write-Host "  - $Path" -ForegroundColor Yellow
    }
    exit 0
}

# Function to copy DLL if it doesn't exist
function Copy-IfMissing {
    param($Source, $Destination)
    
    if (-not (Test-Path $Destination)) {
        $DestDir = Split-Path $Destination -Parent
        if (-not (Test-Path $DestDir)) {
            New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
            Write-Host "Created directory: $DestDir" -ForegroundColor Cyan
        }
        
        Copy-Item $Source $Destination -Force
        Write-Host "Copied MediaInfo.dll to: $Destination" -ForegroundColor Green
    } else {
        Write-Host "MediaInfo.dll already exists at: $Destination" -ForegroundColor Gray
    }
}

# Copy to main output folder
Copy-IfMissing $SourceDll $OutputDll

# Copy to runtime native folders (for BenchmarkDotNet separate processes)
Copy-IfMissing $SourceDll $RuntimeX64
Copy-IfMissing $SourceDll $RuntimeX86

Write-Host "MediaInfo.dll copy process completed successfully!" -ForegroundColor Green