<#
.SYNOPSIS
  Verifies that the native filters in DirectShowFilters do not use MFC or ATL (ADR-0007).

.DESCRIPTION
  Fails (exit code 1) when a source file below -Path, outside comments, includes afx*.h / atl*.h or uses
  CString, CComPtr, CRect, CPoint, CSize or the CAtl* containers, or when a *.vcxproj turns on
  UseOfMfc / UseOfAtl.
  Not scanned: build output (build, vcpkg_installed) and third-party StdString.h with its MFC branch under
  #ifdef _MFC_VER.

.EXAMPLE
  .\Check-NoMfcAtl.ps1 -Path ..\DirectShowFilters
#>
param(
  [Parameter(Mandatory = $true)][string]$Path
)

$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath $Path).Path
$skipDirs = @('build', 'vcpkg_installed', 'obj', 'bin')
$skipFiles = @('DirectShowHelper\source\StdString.h')

$codeRule = '#\s*include\s*[<"](afx|atl)[A-Za-z0-9_]*\.h[>"]|\b(CString[AW]?|CComPtr|CComQIPtr|CRect|CPoint|CSize|CAtl[A-Za-z]+)\b'
$projRule = '<UseOf(Mfc|Atl)>\s*(Static|Dynamic)\s*</UseOf'
$comments = '(?s)/\*.*?\*/|//[^\r\n]*'

$problems = New-Object System.Collections.Generic.List[string]

foreach ($file in Get-ChildItem -LiteralPath $root -Recurse -File -Include *.cpp, *.c, *.h, *.hpp, *.inl, *.vcxproj, *.props, *.targets) {
  $rel = $file.FullName.Substring($root.Length + 1)
  $parts = $rel.Split('\')
  if ($parts | Where-Object { $skipDirs -contains $_ }) { continue }
  if ($skipFiles -contains $rel) { continue }

  $text = [System.IO.File]::ReadAllText($file.FullName)
  if ($file.Extension -in '.vcxproj', '.props', '.targets') {
    $rule = $projRule
  } else {
    $text = [regex]::Replace($text, $comments, { param($m) "`n" * ($m.Value.Split("`n").Count - 1) })
    $rule = $codeRule
  }

  $number = 0
  foreach ($line in $text.Split("`n")) {
    $number++
    if ($line -cmatch $rule) {
      $problems.Add("${rel}:${number}: $($line.Trim())")
    }
  }
}

if ($problems.Count -gt 0) {
  $problems | ForEach-Object { Write-Host "MFC/ATL: $_" }
  Write-Error "$($problems.Count) uses of MFC or ATL found (see ADR-0007)"
  exit 1
}

Write-Host 'No MFC or ATL in DirectShowFilters.'
