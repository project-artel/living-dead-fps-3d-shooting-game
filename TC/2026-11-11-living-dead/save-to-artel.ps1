param([switch]$Publish)
$ErrorActionPreference = 'Stop'
$document = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'test-cases.md') -Raw -Encoding utf8
$common = [regex]::Match($document, '(?s)## 공통 실행 조건\s*(.*?)\r?\n## TC-01').Groups[1].Value.Trim()
$sections = [regex]::Matches($document, '(?ms)^## (TC-\d{2}) ([^\r\n]+)\r?\n(.*?)(?=^## |\z)')
if ($sections.Count -ne 20) { throw 'TC count must be 20.' }
function Read-Field([string]$text, [string]$field) {
    $result = [regex]::Match($text, '(?m)^- ' + [regex]::Escape($field) + ': (.*)$')
    if (!$result.Success) { throw "Missing field: $field" }
    return $result.Groups[1].Value.Trim()
}
$cases = @($sections | ForEach-Object {
    $number = $_.Groups[1].Value
    $title = $_.Groups[2].Value.Trim()
    $body = $_.Groups[3].Value
    $scene = if ($number -in @('TC-01','TC-02')) { 'game' } elseif ($number -eq 'TC-20') { 'Map_v2 / zombie_level' } else { 'Map_v2' }
    $note = [regex]::Match($body, '(?m)^- 결함 후보: (.*)$').Groups[1].Value.Trim()
    $expected = Read-Field $body 'Judge'
    if ($note) { $expected += "`n`n결함 후보: $note" }
    [ordered]@{
        scene = $scene
        step = "[$number] $title`n`n" + (Read-Field $body 'Act')
        precondition = "작성 가능성: " + (Read-Field $body '상태') + "`nQA 결과: 미실행`n우선순위: " + (Read-Field $body '우선순위') + "`n`n시작 조건: " + (Read-Field $body 'Reach') + "`n`n공통 실행 조건:`n$common`n`n코드 근거: " + (Read-Field $body '근거')
        expectedValue = $expected
    }
})
$cases | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'artel-cases.json') -Encoding utf8
if (!$Publish) { Write-Output "Prepared $($cases.Count) cases."; exit 0 }
$credentialPath = if ($env:ARTEL_CONFIG_DIR) { Join-Path $env:ARTEL_CONFIG_DIR 'credentials.json' } else { Join-Path $env:USERPROFILE '.artel\credentials.json' }
$stored = if (Test-Path -LiteralPath $credentialPath) { Get-Content -LiteralPath $credentialPath -Raw | ConvertFrom-Json }
$token = if ($env:ARTEL_TOKEN) { $env:ARTEL_TOKEN } else { $stored.token }
if (!$token) { throw 'ARTEL credential missing.' }
$base = if ($env:ARTEL_API_BASE_URL) { $env:ARTEL_API_BASE_URL } elseif ($stored.apiBaseUrl) { $stored.apiBaseUrl } else { 'https://stage-orch.artel.kr' }
$headers = @{ Authorization = "Bearer $token" }
$project = "$base/api/projects/17"
function Request([string]$method, [string]$path, $body) {
    $args = @{ Uri = "$project/$path"; Method = $method; Headers = $headers }
    if ($null -ne $body) { $args.ContentType = 'application/json; charset=utf-8'; $args.Body = [System.Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json -Depth 12 -Compress)) }
    return Invoke-RestMethod @args
}
$folderName = '2026-11-11 TC 20개'
$folders = Request 'GET' 'test-case-folders' $null
$folder = @($folders.items | Where-Object { $_.name -eq $folderName -and $null -eq $_.parentId })
if ($folder.Count -gt 1) { throw 'Ambiguous target folder.' }
if ($folder.Count -eq 0) { $folder = Request 'POST' 'test-case-folders' @{ name = $folderName; parentId = $null } } else { $folder = $folder[0] }
$existing = Request 'GET' 'test-cases' $null
foreach ($case in $cases) {
    $match = @($existing.items | Where-Object { $_.folderId -eq $folder.id -and $_.step -eq $case.step })
    if ($match.Count -gt 1) { throw 'Duplicate case in target folder.' }
    if ($match.Count -eq 1) { continue }
    $case.folderId = $folder.id
    $created = Request 'POST' 'test-cases' $case
    Write-Output "Saved $($created.id): $($case.step.Split("`n")[0])"
}
$verified = Request 'GET' 'test-cases' $null
$saved = @($verified.items | Where-Object { $_.folderId -eq $folder.id })
if ($saved.Count -ne 20) { throw "Expected 20 saved cases; got $($saved.Count)." }
foreach ($case in $cases) {
    $actual = @($saved | Where-Object { $_.step -eq $case.step })
    if ($actual.Count -ne 1 -or $actual[0].precondition -ne $case.precondition -or $actual[0].expectedValue -ne $case.expectedValue -or $actual[0].scene -ne $case.scene) { throw "Saved content mismatch: $($case.step)" }
    if ($actual[0].verificationStatus -ne 'DRAFT') { throw 'Unexpected verification status.' }
}
$receipt = [ordered]@{ projectId = '17'; apiBaseUrl = $base; folderId = $folder.id; folderName = $folder.name; count = $saved.Count; verifiedAt = (Get-Date).ToUniversalTime().ToString('o'); cases = @($saved | Select-Object id,scene,step,verificationStatus,folderId) }
$receipt | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'artel-saved.json') -Encoding utf8
Write-Output "Verified $($saved.Count) DRAFT cases in folder $($folder.id)."
