<#
.SYNOPSIS
  Verifies that native binaries built from DirectShowFilters carry the expected
  exploit mitigations (see DirectShowFilters\Directory.Build.props).

.DESCRIPTION
  Reads the PE header of every *.dll / *.ax below -Path that is not tracked by git
  (i.e. produced by the build, not a committed third-party binary) and checks:
    - /DYNAMICBASE  (ASLR)
    - /NXCOMPAT     (DEP)
    - /guard:cf     (Control Flow Guard)
    - /HIGHENTROPYVA (64-bit images only)
  Exits with code 1 if any binary misses a mitigation.

.EXAMPLE
  .\Check-NativeHardening.ps1 -Path ..\DirectShowFilters
#>
param(
  [Parameter(Mandatory = $true)][string]$Path,
  # File name patterns of binaries we do not build ourselves
  [string[]]$Exclude = @('libbluray*.dll', 'freetype*.dll', 'd3dx9*.dll', 'msvc*.dll', 'vcruntime*.dll', 'unrar.dll')
)

$ErrorActionPreference = 'Stop'

$IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA = 0x0020
$IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE    = 0x0040
$IMAGE_DLLCHARACTERISTICS_NX_COMPAT       = 0x0100
$IMAGE_DLLCHARACTERISTICS_GUARD_CF        = 0x4000

function Get-PeInfo([string]$file) {
  $bytes = [IO.File]::ReadAllBytes($file)
  if ($bytes.Length -lt 0x40 -or $bytes[0] -ne 0x4D -or $bytes[1] -ne 0x5A) { return $null } # 'MZ'
  $pe = [BitConverter]::ToInt32($bytes, 0x3C)
  if ([BitConverter]::ToUInt32($bytes, $pe) -ne 0x00004550) { return $null }               # 'PE\0\0'
  $optional = $pe + 24
  $magic = [BitConverter]::ToUInt16($bytes, $optional)                                       # 0x10B PE32, 0x20B PE32+
  [pscustomobject]@{
    Is64            = ($magic -eq 0x20B)
    Characteristics = [BitConverter]::ToUInt16($bytes, $optional + 70)
  }
}

$root = (Resolve-Path $Path).Path
$tracked = @{}
Push-Location $root
try {
  git ls-files -- '*.dll' '*.ax' 2>$null | ForEach-Object { $tracked[(Join-Path $root $_).ToLowerInvariant()] = $true }
} finally {
  Pop-Location
}

$binaries = Get-ChildItem -Path $root -Recurse -File -Include *.dll, *.ax |
  Where-Object { $_.FullName -notmatch '\\obj\\|\\vcpkg_installed\\' -and -not $tracked.ContainsKey($_.FullName.ToLowerInvariant()) } |
  Where-Object { $name = $_.Name; -not ($Exclude | Where-Object { $name -like $_ }) }

if (-not $binaries) {
  Write-Error "No built binaries found below $root"
}

$failures = 0
foreach ($bin in $binaries) {
  $info = Get-PeInfo $bin.FullName
  if (-not $info) { continue }

  $missing = @()
  if (-not ($info.Characteristics -band $IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE)) { $missing += 'DYNAMICBASE' }
  if (-not ($info.Characteristics -band $IMAGE_DLLCHARACTERISTICS_NX_COMPAT))    { $missing += 'NXCOMPAT' }
  if (-not ($info.Characteristics -band $IMAGE_DLLCHARACTERISTICS_GUARD_CF))     { $missing += 'GUARD:CF' }
  if ($info.Is64 -and -not ($info.Characteristics -band $IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA)) { $missing += 'HIGHENTROPYVA' }

  $relative = $bin.FullName.Substring($root.Length + 1)
  if ($missing) {
    $failures++
    Write-Host ("FAIL  {0}  missing: {1}" -f $relative, ($missing -join ', ')) -ForegroundColor Red
  } else {
    Write-Host ("OK    {0}" -f $relative)
  }
}

if ($failures) {
  Write-Host "$failures binary(ies) without the required mitigations." -ForegroundColor Red
  exit 1
}
Write-Host 'All native binaries are hardened.'
