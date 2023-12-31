#region Copyright (C) 2005-2023 Team MediaPortal
/*
// Copyright (C) 2005-2023 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

!include LogicLib.nsh
!include WinMessages.nsh

!AddPluginDir "${git_InstallScripts}\FontInfo-plugin\Plugins\x86-unicode"

!macro FontInstallHelper FontFileSrc FontFileDst FontInternalName Resource RegSuffix RegRoot
  ClearErrors
  ${IfNot} ${FileExists} "${FontFileDst}"
    File "/oname=${FontFileDst}" "${FontFileSrc}"
  ${EndIf}
  ${IfNot} ${Errors}
    Push $0
    Push "${Resource}"
    Exch $1
    Push "${FontInternalName}${RegSuffix}"
    Exch $2
    Push $9
    StrCpy $9 "Software\Microsoft\Windows NT\CurrentVersion\Fonts"
    !if "${NSIS_CHAR_SIZE}" < 2
    ReadRegStr $0 ${RegRoot} "SOFTWARE\Microsoft\Windows NT\CurrentVersion" "CurrentVersion"
    ${IfThen} $0 == "" ${|} StrCpy $9 "Software\Microsoft\Windows\CurrentVersion\Fonts" ${|}
    !endif
    System::Call 'GDI32::AddFontResource(tr1)i.r0'
    ${If} $0 <> 0
        WriteRegStr ${RegRoot} "$9" "$2" "$1"
    ${Else}
        SetErrors
    ${EndIf}
    Pop $9
    Pop $2
    Pop $1
    Pop $0
  ${Else}
    SetErrors
  ${EndIf}
!macroend

!macro FontInstallTTF FontFileSrc FontFileName FontInternalName
  !insertmacro FontInstallHelper "${FontFileSrc}" "$Fonts\${FontFileName}" "${FontInternalName}" "${FontFileName}" " (TrueType)" HKLM
!macroend
 
!macro FontUninstallHelper FontFileDst FontInternalName Resource RegSuffix RegRoot
  System::Call 'GDI32::RemoveFontResource(t"${Resource}")'
  DeleteRegValue ${RegRoot} "Software\Microsoft\Windows NT\CurrentVersion\Fonts" "${FontInternalName}${RegSuffix}"
  !if "${NSIS_CHAR_SIZE}" < 2
  DeleteRegValue ${RegRoot} "Software\Microsoft\Windows\CurrentVersion\Fonts" "${FontInternalName}${RegSuffix}"
  !endif
  ClearErrors
  Delete "${FontFileDst}"
!macroend

!macro FontUninstallTTF FontFileName FontInternalName
  !insertmacro FontUninstallHelper "$Fonts\${FontFileName}" "${FontInternalName}" "${FontFileName}" " (TrueType)" HKLM
!macroend
