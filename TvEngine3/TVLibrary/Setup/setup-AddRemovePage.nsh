#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

#**********************************************************************************************************#
#
# This original header file is taken from:           http://nsis.sourceforge.net/Add/Remove_Functionality
#     and modified for our needs.
#
#**********************************************************************************************************#

!include WordFunc.nsh
!include FileFunc.nsh

!insertmacro VersionCompare
!insertmacro GetParent

#####    Add/Remove/Reinstall page
Var ReinstallPageCheck

Function PageReinstall
    ReadRegStr $R0 HKLM "${REG_UNINSTALL}" "InstallPath"

    ${If} $R0 == ""
        Abort
    ${EndIf}

    ReadRegDWORD $R0 HKLM "${REG_UNINSTALL}" "VersionMajor"
    ReadRegDWORD $R1 HKLM "${REG_UNINSTALL}" "VersionMinor"
    ReadRegDWORD $R2 HKLM "${REG_UNINSTALL}" "VersionRevision"
    ReadRegDWORD $R3 HKLM "${REG_UNINSTALL}" "VersionBuild"
    StrCpy $R0 $R0.$R1.$R2.$R3

    ${VersionCompare} ${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD} $R0 $R0
    ${If} $R0 == 0
        StrCpy $R1 "$(TEXT_ADDREMOVE_INFO_REPAIR)"
        StrCpy $R2 "$(TEXT_ADDREMOVE_REPAIR_OPT1)"
        StrCpy $R3 "$(TEXT_ADDREMOVE_REPAIR_OPT2)"
        !insertmacro MUI_HEADER_TEXT "$(TEXT_ADDREMOVE_HEADER)" "$(TEXT_ADDREMOVE_HEADER2_REPAIR)"
        StrCpy $R0 "2"
    ${ElseIf} $R0 == 1
        StrCpy $R1 "$(TEXT_ADDREMOVE_INFO_UPGRADE)"
        StrCpy $R2 "$(TEXT_ADDREMOVE_UPDOWN_OPT1)"
        StrCpy $R3 "$(TEXT_ADDREMOVE_UPDOWN_OPT2)"
        !insertmacro MUI_HEADER_TEXT "$(TEXT_ADDREMOVE_HEADER)" "$(TEXT_ADDREMOVE_HEADER2_UPDOWN)"
        StrCpy $R0 "1"
    ${ElseIf} $R0 == 2
        StrCpy $R1 "$(TEXT_ADDREMOVE_INFO_DOWNGRADE)"
        StrCpy $R2 "$(TEXT_ADDREMOVE_UPDOWN_OPT1)"
        StrCpy $R3 "$(TEXT_ADDREMOVE_UPDOWN_OPT2)"
        !insertmacro MUI_HEADER_TEXT "$(TEXT_ADDREMOVE_HEADER)" "$(TEXT_ADDREMOVE_HEADER2_UPDOWN)"
        StrCpy $R0 "1"
    ${Else}
        Abort
    ${EndIf}

    nsDialogs::Create /NOUNLOAD 1018

    ${NSD_CreateLabel} 0 0 100% 24u $R1
    Pop $R1

    ${NSD_CreateRadioButton} 30u 50u -30u 8u $R2
    Pop $R2
    ${NSD_OnClick} $R2 PageReinstallUpdateSelection

    ${NSD_CreateRadioButton} 30u 70u -30u 8u $R3
    Pop $R3
    ${NSD_OnClick} $R3 PageReinstallUpdateSelection

    ${If} $ReinstallPageCheck != 2
        SendMessage $R2 ${BM_SETCHECK} ${BST_CHECKED} 0
    ${Else}
        SendMessage $R3 ${BM_SETCHECK} ${BST_CHECKED} 0
    ${EndIf}

    nsDialogs::Show
FunctionEnd

Function PageReinstallUpdateSelection
    Pop $R1

    ${NSD_GetState} $R2 $R1

    ${If} $R1 == ${BST_CHECKED}
        StrCpy $ReinstallPageCheck 1
    ${Else}
        StrCpy $ReinstallPageCheck 2
    ${EndIf}

FunctionEnd

Function PageLeaveReinstall
    ${NSD_GetState} $R2 $R1

    StrCmp $R0 "1" 0 +2
        StrCmp $R1 "1" doUninstall finish
    StrCmp $R0 "2" 0 +3
        StrCmp $R1 "1" finish doUninstall

    doUninstall:
    ; check if MP is already installed
    ReadRegStr $R0 HKLM "${REG_UNINSTALL}" UninstallString
    ${If} ${FileExists} "$R0"
        ; get parent folder of uninstallation EXE (RO) and save it to R1
        ${GetParent} $R0 $R1
        ; start uninstallation of installed MP, from tmp folder, so it will delete itself
        HideWindow
        ClearErrors
        CopyFiles $R0 "$TEMP\uninstall-temp.exe"
        ExecWait '"$TEMP\uninstall-temp.exe" _?=$R1'
        BringToFront

        ; if an error occured, ask to cancel installation
        ${If} ${Errors}
            MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_ON_UNINSTALL)" IDYES +2
            Quit
        ${EndIf}
    ${EndIf}

    ; if reboot flag is set, abort the installation, and continue the installer on next startup
    ${If} ${FileExists} "$INSTDIR\rebootflag"
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
        WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" "$(^Name)" $EXEPATH
        Quit
    ${EndIf}

    finish:
FunctionEnd



LangString TEXT_ADDREMOVE_HEADER            ${LANG_ENGLISH} "Already Installed"
LangString TEXT_ADDREMOVE_HEADER2_REPAIR    ${LANG_ENGLISH} "Choose the maintenance option to perform."
LangString TEXT_ADDREMOVE_HEADER2_UPDOWN    ${LANG_ENGLISH} "Choose how you want to install $(^Name)."
LangString TEXT_ADDREMOVE_INFO_REPAIR       ${LANG_ENGLISH} "$(^Name) ${VERSION} is already installed. Select the operation you want to perform and click Next to continue."
LangString TEXT_ADDREMOVE_INFO_UPGRADE      ${LANG_ENGLISH} "An older version of $(^Name) is installed on your system. It is recommended that you uninstall the current version before installing. Select the operation you want to perform and click Next to continue."
LangString TEXT_ADDREMOVE_INFO_DOWNGRADE    ${LANG_ENGLISH} "A newer version of $(^Name) is already installed! It is not recommended that you install an older version. If you really want to install this older version, it's better to uninstall the current version first. Select the operation you want to perform and click Next to continue."
LangString TEXT_ADDREMOVE_REPAIR_OPT1       ${LANG_ENGLISH} "Add/Remove/Reinstall components"
LangString TEXT_ADDREMOVE_REPAIR_OPT2       ${LANG_ENGLISH} "Uninstall $(^Name)"
LangString TEXT_ADDREMOVE_UPDOWN_OPT1       ${LANG_ENGLISH} "Uninstall before installing"
LangString TEXT_ADDREMOVE_UPDOWN_OPT2       ${LANG_ENGLISH} "Do not uninstall"