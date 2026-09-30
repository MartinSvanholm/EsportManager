#Requires -RunAsAdministrator

$user = '{{UserName}}'
$shares = @{
    '{{GameFilesShare}}' = 'C:\ProgramFiles\EpicGames\Fortnite'
    '{{ManifestsShare}}' = 'C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests'
}

if ($user.Contains('{{')) {
    Write-Error 'This is a template. Download the script from Settings in EsportManager instead.'
    exit 1
}

Write-Host 'Removing non-system shares...'
Get-SmbShare | Where-Object { -not $_.Special } | ForEach-Object {
    Write-Host "Deleting share: $($_.Name)"
    Remove-SmbShare -Name $_.Name -Force
}

Write-Host ''
$password = Read-Host -AsSecureString "Password for '$user' (use the same on every club PC)"

if (Get-LocalUser -Name $user -ErrorAction SilentlyContinue) {
    Write-Host "Updating user: $user"
    Set-LocalUser -Name $user -Password $password -PasswordNeverExpires $true -UserMayChangePassword $false
} else {
    Write-Host "Creating user: $user"
    New-LocalUser -Name $user -Password $password -PasswordNeverExpires -UserMayNotChangePassword | Out-Null
}

Write-Host ''
Write-Host 'Creating shares...'
foreach ($name in $shares.Keys) {
    $path = $shares[$name]
    New-Item -ItemType Directory -Force -Path $path | Out-Null
    icacls $path /grant "${user}:(OI)(CI)F" | Out-Null
    New-SmbShare -Name $name -Path $path -FullAccess $user | Out-Null
    Write-Host "Shared $path as \\$env:COMPUTERNAME\$name"
}

Write-Host ''
Write-Host 'Done.'
