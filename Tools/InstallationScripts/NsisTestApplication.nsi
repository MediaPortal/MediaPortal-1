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

#---------------------------------------------------------------------------
# DEVELOPMENT ENVIRONMENT
#---------------------------------------------------------------------------
!define svn_ROOT "..\.."
!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"


#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
#!include "x64.nsh"
#!include Sections.nsh
#!include LogicLib.nsh
#!include Library.nsh
#!include FileFunc.nsh
#!include Memento.nsh

!include "${svn_InstallScripts}\include\*"


#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_FINISHPAGE_NOAUTOCLOSE


#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES


#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
; The name of the installer
Name "test-helpmacros"

; The file to write
OutFile "${__FILE__}.exe"

; The default installation directory
InstallDir $DESKTOP\Example1

; Request application privileges for Windows Vista
RequestExecutionLevel user
ShowInstDetails show


#---------------------------------------------------------------------------
#---------------------------------------------------------------------------
#---------------------------------------------------------------------------


SectionGroup "run nsis installed"

Section /o "Install TestApp 2"
  ExecWait '"$EXEDIR\NsisTestApplication2.nsi.exe"'
SectionEnd
Section /o "Uninstall TestApp 2"
  ExecWait '"$EXEDIR\uninstall.exe"'
SectionEnd
Section /o "SILENT Install TestApp 2"
  ExecWait '"$EXEDIR\NsisTestApplication2.nsi.exe" /S'
SectionEnd
Section /o "SILENT Uninstall TestApp 2"
  ExecWait '"$EXEDIR\uninstall.exe" /S'
SectionEnd


Section /o "SILENT Uninstall TestApp 2 - QUIT"
    HideWindow
    ClearErrors
    ExecWait '"$EXEDIR\uninstall.exe" /frominstall'
    BringToFront

    ; if an error occured, ask to cancel installation
    ${If} ${Errors}
      MessageBox MB_OK|MB_ICONEXCLAMATION "ERROR"
    ${EndIf}
SectionEnd

SectionGroupEnd


SectionGroup "Get infos"

Section /o "MediaPortal Product information"

    DetailPrint ""
    DetailPrint "--------------------------------------"
    DetailPrint "- MediaPortal Installation"
    DetailPrint "--------------------------------------"

  ${If} ${MP023IsInstalled}
    !insertmacro MP_GET_INSTALL_DIR $R0
    DetailPrint "X  MP 0.2.3.0 InstDir: $R0"
  ${Else}
    DetailPrint "!  MP 0.2.3.0 is not installed"
  ${EndIf}

  ${If} ${MPIsInstalled}
    !insertmacro MP_GET_INSTALL_DIR $R0
    DetailPrint "X  MediaPortal InstDir: $R0"
  ${Else}
    DetailPrint "!  MediaPortal is not installed"
  ${EndIf}

  ${If} ${TVServerIsInstalled}
    !insertmacro TVSERVER_GET_INSTALL_DIR $R0
    DetailPrint "X  TVServer InstDir: $R0"
  ${Else}
    DetailPrint "!  TVServer is not installed"
  ${EndIf}


  ${If} ${MSI_TVServerIsInstalled}
    DetailPrint "X  old MSI-based TVServer is installed"
  ${Else}
    DetailPrint "!  old MSI-based TVServer is not installed"
  ${EndIf}

  ${If} ${MSI_TVClientIsInstalled}
    DetailPrint "X  old MSI-based TVClient is installed"
  ${Else}
    DetailPrint "!  old MSI-based TVClient is not installed"
  ${EndIf}

SectionEnd

Section /o "Read MediaPortal directories"

  DetailPrint ""
  DetailPrint "--------------------------------------"
  DetailPrint "- Read MediaPortal directories"
  DetailPrint "--------------------------------------"

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

Section /o "OS, .Net and VCRedist Tests"

  !insertmacro MediaPortalOperatingSystemCheck
  !insertmacro MediaPortalAdminCheck
  !insertmacro MediaPortalVCRedistCheck
  !insertmacro MediaPortalNetFrameworkCheck

SectionEnd

Section /o "MediaPortal Update Procedure Check"

    !insertmacro MP_GET_VERSION $0
    ${If} $0 == 1.0.0.0
      ${LOG_TEXT} "INFO" "Removing 1.0 files..."
    ${ElseIf} $0 == 1.0.1.0
      ${LOG_TEXT} "INFO" "Removing 1.0.1 files..."
    ${ElseIf} $0 == ""
      ${LOG_TEXT} "INFO" "It seems MP is not installed, no update procedure will be done"
    ${ElseIf} $R3 != 0
      ${LOG_TEXT} "INFO" "An SVN version ($0) of MP is installed. Update is not supported."
    ${Else}
      ${LOG_TEXT} "INFO" "MediaPortal $0 is installed."
    ${EndIf}

SectionEnd

SectionGroupEnd


SectionGroup "cleanup tests"

Section /o "MediaPortal CleanUp: 1.0 for 1.0.1/1.0.2 Update"

;MP secPrepare
  !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
  ${ReadMediaPortalDirs} "$MPdir.Base"

  ${LOG_TEXT} "INFO" "Deleting SkinCache..."
  RMDir /r "$MPdir.Cache"

      ${LOG_TEXT} "INFO" "Removing 1.0 files..."
      !include "${svn_MP}\Setup\update-1.0.1.nsh"

SectionEnd

Section /o "MediaPortal CleanUp: 1.0.1 for 1.0.2 Update"

;MP secPrepare
  !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
  ${ReadMediaPortalDirs} "$MPdir.Base"

  ${LOG_TEXT} "INFO" "Deleting SkinCache..."
  RMDir /r "$MPdir.Cache"

      ${LOG_TEXT} "INFO" "Removing 1.0.1 files..."
      !include "${svn_MP}\Setup\update-1.0.2.nsh"

SectionEnd

SectionGroupEnd


SectionGroup "Backup Tests"

Section /o "Rename MP dir"

  !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
  ${ReadMediaPortalDirs} $MPdir.Base

  !insertmacro GET_BACKUP_POSTFIX $R0

  !insertmacro RenameDirectory "$MPdir.Base" "$MPdir.Base_$R0"

  ClearErrors
  CreateDirectory "C:\MPTestDir"
    ${If} ${Errors}
      ${LOG_TEXT} "ERROR" "CreateDirectory"
    ${EndIf}
  !insertmacro RenameDirectory "C:\MPTestDir" "C:\MPTestDir_$R0"
    ${If} ${Errors}
      ${LOG_TEXT} "ERROR" "RenameDirectory"
    ${EndIf}
  RMDir "C:\MPTestDir_$R0"
    ${If} ${Errors}
      ${LOG_TEXT} "ERROR" "RMDir"
    ${EndIf}

  !insertmacro RenameDirectory "C:\MPTestDir" "C:\MPTestDir_$R0"
SectionEnd

Section /o "Backup ConfigDir"

  !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
  ${ReadMediaPortalDirs} $MPdir.Base

  !insertmacro GET_BACKUP_POSTFIX $R0
  !insertmacro BackupConfigDir

SectionEnd

Section /o "Backup ThumbsDir"

  !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
  ${ReadMediaPortalDirs} $MPdir.Base

  !insertmacro GET_BACKUP_POSTFIX $R0
  !insertmacro BackupThumbsDir

SectionEnd

SectionGroupEnd


SectionGroup "other macro Tests"

Section /o "Stop TVService"

  ${StopService} "TVservice"

SectionEnd

Section /o "Kill Processes"

  ${KillProcess} "MPInstaller.exe"
  ${KillProcess} "makensisw.exe"
  ${KillProcess} "Input Service Configuration.exe"

SectionEnd

Section /o "RegisterExtension"

  ${LOG_TEXT} "INFO" "RegisterExtension"
  ${RegisterExtension} "$MPdir.Base\MPInstaller.exe"  ".mpi"  "MediaPortal extension package"
  ${RegisterExtension} "$MPdir.Base\MPInstaller.exe"  ".mpe1" "MediaPortal extension package"
  ${RegisterExtension} "$MPdir.Base\MPIMaker.exe"     ".xmp"  "MediaPortal extension project"

  ${RefreshShellIcons}
SectionEnd

Section /o "UnRegisterExtension"

  ${LOG_TEXT} "INFO" "UnRegisterExtension"
  ${UnRegisterExtension} ".mpi"   "MediaPortal extension package"
  ${UnRegisterExtension} ".mpe1"  "MediaPortal extension package"
  ${UnRegisterExtension} ".xmp"   "MediaPortal extension project"

  ${RefreshShellIcons}
SectionEnd

Section /o "CleanLogDirectory"

  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    DetailPrint "no MPIsInstalled"
  ${else}
    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${ReadMediaPortalDirs} $MPdir.Base

    RMDir /r "$MPdir.Log\OldLogs"
    CreateDirectory "$MPdir.Log\OldLogs"
    CopyFiles /SILENT /FILESONLY "$MPdir.Log\*" "$MPdir.Log\OldLogs"
    Delete "$MPdir.Log\*"
  ${EndIf}

SectionEnd

SectionGroupEnd

