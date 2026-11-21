param([string]$Docx, [string]$Pdf)
# Exports with Word; stops only Word processes this script started if it hangs.
$before = @(Get-Process WINWORD -ErrorAction SilentlyContinue | ForEach-Object { $_.Id })
$job = Start-Job -ArgumentList $Docx, $Pdf -ScriptBlock {
    param($d, $p)
    $w = New-Object -ComObject Word.Application
    $w.Visible = $false; $w.DisplayAlerts = 0
    $doc = $w.Documents.Open($d, $false, $true)
    $doc.ExportAsFixedFormat($p, 17)
    $doc.Close($false); $w.Quit()
}
if (-not (Wait-Job $job -Timeout 120)) {
    Get-Process WINWORD -ErrorAction SilentlyContinue | Where-Object { $before -notcontains $_.Id } | Stop-Process -Force
    "timed out"
} else { Receive-Job $job; "done" }
Remove-Job $job -Force
