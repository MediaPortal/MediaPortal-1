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
# This header file is taken original taken from:           http://nsis.sourceforge.net/Add/Remove_Functionality
#     and modified for our needs.
#
#**********************************************************************************************************#

Var AR_SecFlags
Var AR_RegFlags
 
!macro InitSection SecName
    ;This macro reads component installed flag from the registry and
    ;changes checked state of the section on the components page.
    ;Input: section index constant name specified in Section command.

    ClearErrors
    ;Reading component status from registry
    ReadRegDWORD $AR_RegFlags HKLM "${REG_UNINSTALL}\Components" "${SecName}"
    IfErrors "default_${SecName}"
    
    ;Status will stay default if registry value not found
    ;(component was never installed)
    IntOp $AR_RegFlags $AR_RegFlags & 0x0001  ;Turn off all other bits
    SectionGetFlags ${${SecName}} $AR_SecFlags  ;Reading default section flags
    IntOp $AR_SecFlags $AR_SecFlags & 0xFFFE  ;Turn lowest (enabled) bit off
    IntOp $AR_SecFlags $AR_RegFlags | $AR_SecFlags      ;Change lowest bit

    ;Writing modified flags
    SectionSetFlags ${${SecName}} $AR_SecFlags

  "default_${SecName}:"
!macroend
 
!macro FinishSection SecName
    ;This macro reads section flag set by user and removes the section
    ;if it is not selected.
    ;Then it writes component installed flag to registry
    ;Input: section index constant name specified in Section command.

    # reading old component status from registry
    ReadRegDWORD $AR_RegFlags HKLM "${REG_UNINSTALL}\Components" "${SecName}"
    IfErrors 0 +2
    StrCpy $AR_RegFlags 0 ; no reg entry yet, so it was not installed yet

    # checking if section is selected
    SectionGetFlags ${${SecName}} $AR_SecFlags  ;Reading section flags
    ;Checking lowest bit:
    IntOp $AR_SecFlags $AR_SecFlags & 0x0001
    IntCmp $AR_SecFlags 1 +2

    ; is section selected ????
    ${If} $AR_SecFlags == 1 ; is selected
        WriteRegDWORD HKLM "${REG_UNINSTALL}\Components" "${SecName}" 1
    ${Else}                 ; is not selected
        WriteRegDWORD HKLM "${REG_UNINSTALL}\Components" "${SecName}" 0
        ${If} $AR_RegFlags == 1 ; was section installed prevously --> remove it
            !insertmacro "Remove_${${SecName}}"
        ${EndIf}
    ${EndIf}
!macroend
 
!macro RemoveSection SecName
    ;This macro is used to call section's Remove_... macro
    ;from the uninstaller.
    ;Input: section index constant name specified in Section command.

    !insertmacro "Remove_${${SecName}}"
!macroend



!ifdef VER_MAJOR & VER_MINOR & VER_REVISION & VER_BUILD
    !insertmacro VersionCompare
!endif

#####    Add/Remove/Reinstall page
!ifdef VER_MAJOR & VER_MINOR & VER_REVISION & VER_BUILD

Var ReinstallPageCheck

Function PageReinstall
    ReadRegStr $R0 HKLM "${REGKEY}" "InstallPath"

    ${If} $R0 == ""
        Abort
    ${EndIf}

    ReadRegDWORD $R0 HKLM "${REGKEY}" "VersionMajor"
    ReadRegDWORD $R1 HKLM "${REGKEY}" "VersionMinor"
    ReadRegDWORD $R2 HKLM "${REGKEY}" "VersionRevision"
    ReadRegDWORD $R3 HKLM "${REGKEY}" "VersionBuild"
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
    ReadRegStr $R1 HKLM "${REG_UNINSTALL}" "UninstallString"
    IfFileExists '$R1' 0 onError

    ;Run uninstaller
    HideWindow
    ClearErrors
    ExecWait '$R1 _?=$INSTDIR'
    BringToFront
    
    IfErrors onError uninstallDone
    
    onError:
    MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_ERROR_ON_UNINSTALL)" /SD IDNO IDYES finish IDNO 0
    Quit

    uninstallDone:
    IfFileExists '$R1' 0 +2
    Delete $R1
    
    finish:
FunctionEnd

!endif # VER_MAJOR & VER_MINOR & VER_REVISION & VER_BUILD
#####    End of Add/Remove/Reinstall page