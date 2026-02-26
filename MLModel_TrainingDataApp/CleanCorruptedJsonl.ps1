# Script to identify and remove corrupted lines from JSONL file

$inputFile = ".\Data\tv_episode_training_data.jsonl"
$outputFile = ".\Data\tv_episode_training_data_cleaned.jsonl"
$corruptedFile = ".\Data\tv_episode_training_data_corrupted.jsonl"

$lineNum = 0
$goodLines = 0
$badLines = 0
$validLines = @()
$corruptedInfo = @()

Write-Output "Reading file into memory..."

# Read all lines into memory
$allLines = Get-Content $inputFile

Write-Output "Processing $($allLines.Count) lines..."

foreach ($line in $allLines) {
    $lineNum++
    
    try {
        # Try to parse the JSON
        $line | ConvertFrom-Json | Out-Null
        
        # If successful, add to valid lines array
        $validLines += $line
        $goodLines++
        
        if ($goodLines % 1000 -eq 0) {
            Write-Output "  Validated $goodLines lines..."
        }
    }
    catch {
        # If parsing fails, log it
        $badLines++
        $corruptedInfo += "Line $lineNum"
        $corruptedInfo += $line
        $corruptedInfo += "Error: $($_.Exception.Message)"
        $corruptedInfo += "---"
        
        Write-Output "  Found corrupted line $lineNum"
    }
}

Write-Output "`nWriting results to files..."

# Write all valid lines to output file
$validLines | Set-Content -Path $outputFile

# Write corrupted lines information if any
if ($corruptedInfo.Count -gt 0) {
    $corruptedInfo | Set-Content -Path $corruptedFile
}

Write-Output "`n=== Summary ==="
Write-Output "Total lines processed: $lineNum"
Write-Output "Valid lines: $goodLines"
Write-Output "Corrupted lines: $badLines"
Write-Output "`nCleaned file saved to: $outputFile"
Write-Output "Corrupted lines logged to: $corruptedFile"

if ($badLines -gt 0) {
    Write-Output "`n? Found $badLines corrupted lines. Review $corruptedFile for details."
    Write-Output "You can replace the original file with the cleaned version:"
    Write-Output "  Move-Item '$inputFile' '${inputFile}.backup'"
    Write-Output "  Move-Item '$outputFile' '$inputFile'"
}
else {
    Write-Output "`n? No corrupted lines found. File is clean!"
}
