#region Copyright (C) 2005-2010 Team MediaPortal
/*
// Copyright (C) 2005-2010 Team MediaPortal
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

# DEFINES
!define svn_ROOT "..\.."
!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"




; TEST APP FOR MP STUFF

; The name of the installer
Name "test-helpmacros"

; The file to write
OutFile "${__FILE__}.exe"

; The default installation directory
InstallDir $DESKTOP\Example1

; Request application privileges for Windows Vista
RequestExecutionLevel user
ShowInstDetails show

;--------------------------------

#!define INSTALL_LOG_FILE "$DESKTOP\install_$(^Name).log"

!include MUI2.nsh
#!include "x64.nsh"
#!include Sections.nsh
#!include LogicLib.nsh
#!include Library.nsh
#!include FileFunc.nsh
#!include Memento.nsh

Var frominstall



!include "${svn_InstallScripts}\include\*"

;--------------------------------

!define MUI_FINISHPAGE_NOAUTOCLOSE

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES

!define MUI_PAGE_CUSTOMFUNCTION_PRE un.WelcomePagePre
!insertmacro MUI_UNPAGE_WELCOME
!define MUI_PAGE_CUSTOMFUNCTION_PRE un.ConfirmPagePre
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!define MUI_PAGE_CUSTOMFUNCTION_PRE un.FinishPagePre
!insertmacro MUI_UNPAGE_FINISH
;--------------------------------

; The stuff to install
Section "test"
  WriteUninstaller "$EXEDIR\uninstall.exe"
  
  IfSilent _silent
    DetailPrint "install testapp2"
    Goto _end
  
  _silent:
    DetailPrint "silent install testapp2"
  
  _end:
SectionEnd

Section "uninstall"
  
  IfSilent _silent
    DetailPrint "uninstall testapp2"
    Goto _end
  
  _silent:
    DetailPrint "silent uninstall testapp2"
  
  _end:

  ; parse parameters
  ${If} $frominstall == 1
    Quit
  ${EndIf}

SectionEnd





Function un.onInit

  ; parse parameters
  ClearErrors
  ${un.GetParameters} $R0

  ${un.GetOptions} $R0 "/frominstall" $R1
  ${Unless} ${Errors}
    StrCpy $frominstall 1
  ${EndUnless}
FunctionEnd

Function un.WelcomePagePre

  ${If} $frominstall == 1
    Abort
  ${EndIf}

FunctionEnd

Function un.ConfirmPagePre

  ${If} $frominstall == 1
    Abort
  ${EndIf}

FunctionEnd

Function un.FinishPagePre

  ${If} $frominstall == 1
    SetRebootFlag false
    Abort
  ${EndIf}

FunctionEnd
