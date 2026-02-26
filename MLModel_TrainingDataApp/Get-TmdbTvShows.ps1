$tmdbApiKey = ""
$target = 5000
$out = ""

if ([string]::IsNullOrWhiteSpace($out))
{
    throw 'Output path ($out) is empty. Set $out to the output file path'
}

if ([string]::IsNullOrWhiteSpace($tmdbApiKey))
{
    throw 'TMDb API key ($tmdbApiKey) is empty. Set $tmdbApiKey to your TMDb API key'
}

$page = 1
$shows = @{}
$batchSize = 10

# Initialize output file
@() | ConvertTo-Json | Set-Content $out

Write-Output "Fetching $target TV shows from TMDb..."

while ($shows.Count -lt $target) {
  $url = "https://api.themoviedb.org/3/tv/popular?api_key=$tmdbApiKey&page=$page"
  $resp = Invoke-RestMethod -Uri $url -UseBasicParsing
  
  foreach ($s in $resp.results) {
    if (-not $shows.ContainsKey($s.id)) {
      $shows[$s.id] = [pscustomobject]@{
        tmdb_id = $s.id
        name = $s.name
        first_air_date = $s.first_air_date
        popularity = $s.popularity
      }
    }
  }
  
  # Write to file every $batchSize shows
  if ($shows.Count % $batchSize -eq 0) {
    ($shows.Values) | ConvertTo-Json -Depth 3 | Set-Content $out
    Write-Output "Persisted $($shows.Count)/$target shows to $out"
  }
  
  if ($page -ge $resp.total_pages) { break }
  $page++
  Start-Sleep -Milliseconds 10
}

# Final write to ensure all shows are saved
if ($shows.Count % $batchSize -ne 0) {
  ($shows.Values) | ConvertTo-Json -Depth 3 | Set-Content $out
}

Write-Output "✓ Saved $($shows.Count) shows → $out"
