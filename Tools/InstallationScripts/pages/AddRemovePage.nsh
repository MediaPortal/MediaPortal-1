#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

/*
_____________________________________________________________________________

                       AddRemovePage
_____________________________________________________________________________

  The original header file is taken from:
              http://nsis.sourceforge.net/Add/Remove_Functionality
  and modified for our needs.
*/

!ifndef ___ADD_REMOVE_PAGE__NSH___
!define ___ADD_REMOVE_PAGE__NSH___

!include WordFunc.nsh
!include FileFunc.nsh

#####    Add/Remove/Reinstall page
Var ReinstallMode
Var ReinstallModePage.optBtn1
Var ReinstallModePage.optBtn1.state
Var ReinstallModePage.optBtn2
Var ReinstallModePage.optBtn2.state

Function PageReinstallMode
  ; check if software is already installed
  ${If} $PREVIOUS_VERSION == ""
    Abort
  ${EndIf}


  ; save current values to stack
  Push $R0
  Push $R1
  Push $R2
  Push $R3


  ; set string for control texts
  ${If} $PREVIOUS_VERSION_STATE == "newer"
    StrCpy $R1 "$(TEXT_ADDREMOVE_INFO_UPGRADE)"
    StrCpy $R2 "$(TEXT_ADDREMOVE_UPDOWN_OPT1)"
    StrCpy $R3 "$(TEXT_ADDREMOVE_UPDOWN_OPT2)"
    !insertmacro MUI_HEADER_TEXT "$(TEXT_ADDREMOVE_HEADER)" "$(TEXT_ADDREMOVE_HEADER2_UPDOWN)"

  ${ElseIf} $PREVIOUS_VERSION_STATE == "older"
    StrCpy $R1 "$(TEXT_ADDREMOVE_INFO_DOWNGRADE)"
    StrCpy $R2 "$(TEXT_ADDREMOVE_UPDOWN_OPT1)"
    StrCpy $R3 "$(TEXT_ADDREMOVE_UPDOWN_OPT2)"
    !insertmacro MUI_HEADER_TEXT "$(TEXT_ADDREMOVE_HEADER)" "$(TEXT_ADDREMOVE_HEADER2_UPDOWN)"

  ${ElseIf} $PREVIOUS_VERSION_STATE == "same"
    StrCpy $R1 "$(TEXT_ADDREMOVE_INFO_REPAIR)"
    StrCpy $R2 "$(TEXT_ADDREMOVE_REPAIR_OPT1)"
    StrCpy $R3 "$(TEXT_ADDREMOVE_REPAIR_OPT2)"
    !insertmacro MUI_HEADER_TEXT "$(TEXT_ADDREMOVE_HEADER)" "$(TEXT_ADDREMOVE_HEADER2_REPAIR)"

  ${Else}
    MessageBox MB_ICONSTOP "Unknown value of PREVIOUS_VERSION_STATE, aborting" /SD IDOK
    Abort
  ${EndIf}


  ; create controls
  nsDialogs::Create /NOUNLOAD 1018
  Pop $R0

  ${NSD_CreateLabel} 0 0 300u 24u $R1
  Pop $R1

  ${NSD_CreateRadioButton} 30u 50u -30u 8u $R2
  Pop $ReinstallModePage.optBtn1
  ${NSD_OnClick} $ReinstallModePage.optBtn1 PageReinstallModeUpdateSelection

  ${NSD_CreateRadioButton} 30u 70u -30u 8u $R3
  Pop $ReinstallModePage.optBtn2
  ${NSD_OnClick} $ReinstallModePage.optBtn2 PageReinstallModeUpdateSelection


  ; set current ReinstallMode to option buttons
  ${If} $ReinstallMode == 2
    ${NSD_Check} $ReinstallModePage.optBtn2
  ${Else}
    ; if not 2, set to 1
    ${NSD_Check} $ReinstallModePage.optBtn1
    ; set reinstallmode to 1, if reinstallmode = ""
    StrCpy $ReinstallMode 1
  ${EndIf}


  nsDialogs::Show

  ; restore values from stack
  Pop $R3
  Pop $R2
  Pop $R1
  Pop $R0
FunctionEnd

Function PageReinstallModeUpdateSelection

  ${NSD_GetState} $ReinstallModePage.optBtn1 $ReinstallModePage.optBtn1.state
  ${NSD_GetState} $ReinstallModePage.optBtn2 $ReinstallModePage.optBtn2.state

  ${If} $ReinstallModePage.optBtn2.state == ${BST_CHECKED}
    StrCpy $ReinstallMode 2
  ${Else}
    StrCpy $ReinstallMode 1
  ${EndIf}

FunctionEnd

Function PageLeaveReinstallMode

  StrCpy $EXPRESS_UPDATE 0

  ; Uninstall is selected
  ${If} $PREVIOUS_VERSION_STATE == "same"
  ${AndIf} $ReinstallMode == 2

    StrCpy $EXPRESS_UPDATE 1
    Call RunUninstaller
    Quit

  ${EndIf}


  ; ExpressUpdate is selected
  ${If} $PREVIOUS_VERSION_STATE != "same"
  ${AndIf} $ReinstallMode == 1

    StrCpy $EXPRESS_UPDATE 1
    Call LoadPreviousSettings

  ${EndIf}

FunctionEnd

/*
  ${NSD_GetState} $R2 $R1

  StrCmp $R0 "1" 0 +2
    StrCmp $R1 "1" doUninstall finish
  StrCmp $R0 "2" 0 +3
    StrCmp $R1 "1" finish doUninstall

  doUninstall:
  ; check if MP is already installed
  ReadRegStr $R0 HKLM "${RegKey}" UninstallString
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
*/

!endif # !___ADD_REMOVE_PAGE__NSH___
