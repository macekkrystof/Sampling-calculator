param(
  [int]$Max = 10
)

Write-Host "Starting Ralph - Max $MAX iterations"
Write-Host ""

for ($i = 1; $i -le $MAX; $i++) {
  Write-Host "====================================================="
  Write-Host " Iteration $i of $MAX"
  Write-Host "====================================================="

  $result = ""
  claude --dangerously-skip-permissions -p "@prompt.md @progress.txt" 2>&1 | ForEach-Object {
    Write-Host $_
    $result += "$_`n"
  }

  if ($LASTEXITCODE -ne 0) {
    Write-Warning "Claude CLI crashed with exit code $LASTEXITCODE. Retrying in 5 seconds..."
    Start-Sleep -Seconds 5
    continue
  }
  if ($result -match "<promise>COMPLETE</promise>") {
    Write-Host "Ralph has finished all tasks."
    break
  }
}