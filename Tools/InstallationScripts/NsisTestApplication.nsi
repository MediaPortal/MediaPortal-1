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
OutFile "test-helpmacros.exe"

; The default installation directory
InstallDir $DESKTOP\Example1

; Request application privileges for Windows Vista
RequestExecutionLevel user
ShowInstDetails show

;--------------------------------

Page license
Page instfiles 

;--------------------------------

!define INSTALL_LOG_FILE "$DESKTOP\install_$(^Name).log"

#!include "x64.nsh"
#!include Sections.nsh
#!include LogicLib.nsh
#!include Library.nsh
#!include FileFunc.nsh
;!include WinVer.nsh
!define WinVer++
#!include Memento.nsh

!include LogicLib.nsh

!include include-CommonMPMacros.nsh

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important
  
SectionEnd ; end the section

;--------------------------------

!macro DoInstallChecks

  ${If} ${MP023IsInstalled}
    !insertmacro MP_GET_INSTALL_DIR $R0
    DetailPrint "MP_GET_INSTALL_DIR: $R0"
  ${Else}
    DetailPrint "no MP023IsInstalled"
  ${EndIf}

  ${If} ${MPIsInstalled}
    !insertmacro MP_GET_INSTALL_DIR $R0
    DetailPrint "MP_GET_INSTALL_DIR: $R0"
  ${Else}
    DetailPrint "no MPIsInstalled"
  ${EndIf}

  ${If} ${TVServerIsInstalled}
    !insertmacro TVSERVER_GET_INSTALL_DIR $R0
    DetailPrint "TVSERVER_GET_INSTALL_DIR: $R0"
  ${Else}
    DetailPrint "no TVServerIsInstalled"
  ${EndIf}


  ${If} ${MSI_TVServerIsInstalled}
    DetailPrint "MSI_TVServerIsInstalled"
  ${Else}
    DetailPrint "no MSI_TVServerIsInstalled"
  ${EndIf}

  ${If} ${MSI_TVClientIsInstalled}
    DetailPrint "MSI_TVClientIsInstalled"
  ${Else}
    DetailPrint "no MSI_TVClientIsInstalled"
  ${EndIf}

!macroend

!macro OperationSystemChecks

  ; show error that the OS is not supported and abort the installation
  ${If} ${AtMostWin2000Srv}
    DetailPrint "AtMostWin2000Srv"
    StrCpy $0 "OSabort"
  ${ElseIf} ${IsWinXP}
    DetailPrint "IsWinXP"
    !insertmacro GetServicePack $R1 $R2
    DetailPrint "SP major: $R1"
    DetailPrint "SP minor: $R2"
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 2
      StrCpy $0 "OSabort"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWinXP64}
    DetailPrint "IsWinXP64"
    StrCpy $0 "OSabort"

  ${ElseIf} ${IsWin2003}
    DetailPrint "IsWin2003"
    StrCpy $0 "OSwarn"

  ${ElseIf} ${IsWinVISTA}
    DetailPrint "IsWinVISTA"
    !insertmacro GetServicePack $R1 $R2
    DetailPrint "SP major: $R1"
    DetailPrint "SP minor: $R2"
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 1
      StrCpy $0 "OSwarn"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWin2008}
    DetailPrint "IsWin2008"
    StrCpy $0 "OSwarn"

  ${Else}
    DetailPrint "unknown OS"
    StrCpy $0 "OSabort"
  ${EndIf}

  ; show warnings for some OS
  ${If} $0 == "OSabort"
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${ElseIf} $0 == "OSwarn"
    ${If} $DeployMode == 0
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
    ${EndIf}
  ${ElseIf} $0 == "OSwarnBetaSP"
    ${If} $DeployMode == 0
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "You are using a beta Service Pack! $(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
    ${EndIf}
  ${Else}
    ; do nothing
  ${EndIf}

!macroend

Section
  ${LOG_OPEN}


  !insertmacro OperationSystemChecks

  ${If} ${VCRedistIsInstalled}
    DetailPrint "vcr IsInstalled"
  ${Else}
    DetailPrint "no vcr IsInstalled"
  ${EndIf}
  
  !insertmacro DoInstallChecks


  
  
  MessageBox MB_ICONINFORMATION|MB_YESNO "Do kill process test?" IDNO noKillProcess

  ${KILLPROCESS} "MPInstaller.exe"
  ${KILLPROCESS} "makensisw.exe"
  ${KILLPROCESS} "Input Service Configuration.exe"

  DetailPrint "KillProcess FINISHED"

  noKillProcess:


  
  
  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    DetailPrint "no MPIsInstalled"
  ${else}
    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${ReadMediaPortalDirs} $MPdir.Base
  ${EndIf}

  DetailPrint "Found the following Entries:"
  DetailPrint "    Base:  $MPdir.Base"
  DetailPrint "    Config:  $MPdir.Config"
  DetailPrint "    Plugins: $MPdir.Plugins"
  DetailPrint "    Log: $MPdir.Log"
  DetailPrint "    CustomInputDevice: $MPdir.CustomInputDevice"
  DetailPrint "    CustomInputDefault: $MPdir.CustomInputDefault"
  DetailPrint "    Skin: $MPdir.Skin"
  DetailPrint "    Language: $MPdir.Language"
  DetailPrint "    Database: $MPdir.Database"
  DetailPrint "    Thumbs: $MPdir.Thumbs"
  DetailPrint "    Weather: $MPdir.Weather"
  DetailPrint "    Cache: $MPdir.Cache"
  DetailPrint "    BurnerSupport: $MPdir.BurnerSupport"

SectionEnd

Function .onInstFailed
  ${LOG_CLOSE}
FunctionEnd

Function .onInstSuccess
  ${LOG_CLOSE}
FunctionEnd

