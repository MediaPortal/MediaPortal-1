#region Copyright (C) 2005-2009 Team MediaPortal

/*
 *  Copyright (C) 2005-2009 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#   For building the installer on your own you need:
#       1. Latest NSIS version from http://nsis.sourceforge.net/Download
#
#**********************************************************************************************************#

#---------------------------------------------------------------------------
# SPECIAL BUILDS
#---------------------------------------------------------------------------
##### SVN_BUILD
# This build will be created by svn bot only.
# Creating such a build, will only include the changed and new files since latest stable release to the installer.

##### HEISE_BUILD
# Uncomment the following line to create a setup for "Heise Verlag" / ct' magazine  (without MPC-HC/Gabest Filters)
;!define HEISE_BUILD
# parameter for command line execution: /DHEISE_BUILD

##### BUILD_TYPE
# Uncomment the following line to create a setup in debug mode
;!define BUILD_TYPE "Debug"
# parameter for command line execution: /DBUILD_TYPE=Debug
# by default BUILD_TYPE is set to "Release"
!ifndef BUILD_TYPE
  !define BUILD_TYPE "Release"
!endif


#---------------------------------------------------------------------------
# DEVELOPMENT ENVIRONMENT
#---------------------------------------------------------------------------
# path definitions
!define svn_ROOT "..\..\.."
!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_Common_MP_TVE3 "${svn_ROOT}\Common-MP-TVE3"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_DirectShowFilters "${svn_ROOT}\DirectShowFilters"
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"

# additional path definitions
!define TVSERVER.BASE "${svn_TVServer}\TVServer.Base"
#!define MEDIAPORTAL.BASE "${svn_MP}\MediaPortal.Base"
!ifdef SVN_BUILD
  !define MEDIAPORTAL.BASE "E:\compile\compare_mp1_test"
!else
  !define MEDIAPORTAL.BASE "${svn_MP}\MediaPortal.Base"
!endif


#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define PRODUCT_NAME          "MediaPortal TV Server / Client"
!define PRODUCT_PUBLISHER     "Team MediaPortal"
!define PRODUCT_WEB_SITE      "www.team-mediaportal.com"

!define REG_UNINSTALL         "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
!define MP_REG_UNINSTALL      "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!define MEMENTO_REGISTRY_ROOT HKLM
!define MEMENTO_REGISTRY_KEY  "${REG_UNINSTALL}"
!define COMMON_APPDATA        "$APPDATA\Team MediaPortal\MediaPortal TV Server"
!define STARTMENU_GROUP       "$SMPROGRAMS\Team MediaPortal\MediaPortal TV Server"

!define VER_MAJOR       1
!define VER_MINOR       0
!define VER_REVISION    4
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif


!if ${BUILD_TYPE} == "Debug"
  !define VERSION "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION} >>DEBUG<< build ${VER_BUILD} for TESTING ONLY"
!else
!if ${VER_BUILD} == 0       # it's an official release
  ;!define VERSION "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}"
  ;this is for display purposes
  !define VERSION "1.1.0 Beta1"
!else                       # it's a svn release
  ;!define VERSION "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION} SVN build ${VER_BUILD} for TESTING ONLY"
  ;this is for display purposes
  !define VERSION "1.1.0 Beta1 SVN build ${VER_BUILD} for TESTING ONLY"
!endif
!endif

SetCompressor /SOLID lzma


#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
;Var StartMenuGroup  ; Holds the Startmenu\Programs folder
Var InstallPath
Var noClient
Var noServer
Var noDesktopSC
;Var noStartMenuSC
Var DeployMode
Var UpdateMode

Var PREVIOUS_INSTALLDIR
Var PREVIOUS_VERSION
Var PREVIOUS_VERSION_STATE
Var EXPRESS_UPDATE

Var PREVIOUS_GENTLE_CONFIG
Var PREVIOUS_GENTLE_CONFIG_PLUGIN

Var frominstall


#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include Library.nsh
!include FileFunc.nsh
!include Memento.nsh

!include "${svn_InstallScripts}\include\FileAssociation.nsh"
!include "${svn_InstallScripts}\include\LanguageMacros.nsh"
!include "${svn_InstallScripts}\include\LoggingMacros.nsh"
!include "${svn_InstallScripts}\include\MediaPortalDirectories.nsh"
!include "${svn_InstallScripts}\include\MediaPortalMacros.nsh"
!include "${svn_InstallScripts}\include\ProcessMacros.nsh"
!include "${svn_InstallScripts}\include\WinVerEx.nsh"

!include "${svn_InstallScripts}\pages\AddRemovePage.nsh"
!include "${svn_InstallScripts}\pages\UninstallModePage.nsh"


#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ABORTWARNING
!define MUI_ICON    "Resources\install.ico"
!define MUI_UNICON  "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP              "Resources\header.bmp"
!if ${VER_BUILD} == 0       # it's an official release
  !define MUI_WELCOMEFINISHPAGE_BITMAP      "Resources\wizard.bmp"
!else                       # it's a svn release
  !define MUI_WELCOMEFINISHPAGE_BITMAP      "Resources\wizard-svn.bmp"
!endif
!define MUI_UNWELCOMEFINISHPAGE_BITMAP      "Resources\wizard.bmp"
!define MUI_HEADERIMAGE_RIGHT

!define MUI_COMPONENTSPAGE_SMALLDESC
;!define MUI_STARTMENUPAGE_NODISABLE
;!define MUI_STARTMENUPAGE_DEFAULTFOLDER       "Team MediaPortal\TV Server"
;!define MUI_STARTMENUPAGE_REGISTRY_ROOT       HKLM
;!define MUI_STARTMENUPAGE_REGISTRY_KEY        "${REG_UNINSTALL}"
;!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN      "$INSTDIR\SetupTV.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run TV-Server Configuration"
#!define MUI_FINISHPAGE_SHOWREADME $INSTDIR\readme.txt
#!define MUI_FINISHPAGE_SHOWREADME_TEXT "View Readme"
#!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_LINK          "Donate to MediaPortal"
!define MUI_FINISHPAGE_LINK_LOCATION "http://www.team-mediaportal.com/donate.html"

!define MUI_UNFINISHPAGE_NOAUTOCLOSE


#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${svn_MP}\Docs\MediaPortal License.rtf"

Page custom PageReinstallMode PageLeaveReinstallMode

!define MUI_PAGE_CUSTOMFUNCTION_PRE PageComponentsPre
!insertmacro MUI_PAGE_COMPONENTS

!define MUI_PAGE_CUSTOMFUNCTION_PRE PageDirectoryPre
!insertmacro MUI_PAGE_DIRECTORY

;!define MUI_PAGE_CUSTOMFUNCTION_PRE PageStartmenuPre
;!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup

!insertmacro MUI_PAGE_INSTFILES

!define MUI_PAGE_CUSTOMFUNCTION_SHOW PageFinishShow
!insertmacro MUI_PAGE_FINISH


; UnInstaller Interface
!define MUI_PAGE_CUSTOMFUNCTION_PRE un.WelcomePagePre
!insertmacro MUI_UNPAGE_WELCOME
UninstPage custom un.UninstallModePage un.UninstallModePageLeave
;!define MUI_PAGE_CUSTOMFUNCTION_PRE un.ConfirmPagePre
;!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!define MUI_PAGE_CUSTOMFUNCTION_PRE un.FinishPagePre
!insertmacro MUI_UNPAGE_FINISH


#---------------------------------------------------------------------------
# INSTALLER LANGUAGES
#---------------------------------------------------------------------------
!insertmacro LANG_LOAD "English"


#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
Name          "${PRODUCT_NAME}"
BrandingText  "${PRODUCT_NAME} ${VERSION} by ${PRODUCT_PUBLISHER}"
!if ${VER_BUILD} == 0
  OutFile "Release\package-tvengine.exe"
!else
  OutFile "Release\Setup-TvEngine-svn-${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}.exe"
!endif
InstallDir ""
CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName       "MediaPortal TV Server"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductVersion    "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyName       "${PRODUCT_PUBLISHER}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyWebsite    "${PRODUCT_WEB_SITE}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileVersion       "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription   "${PRODUCT_NAME} installation ${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright    "Copyright © 2005-2009 ${PRODUCT_PUBLISHER}"
ShowUninstDetails show


#---------------------------------------------------------------------------
# USEFUL MACROS
#---------------------------------------------------------------------------
!macro SectionList MacroName
  ${LOG_TEXT} "DEBUG" "MACRO SectionList ${MacroName}"
  ; This macro used to perform operation on multiple sections.
  ; List all of your components in following manner here.
  !insertmacro "${MacroName}" "SecServer"
  !insertmacro "${MacroName}" "SecClient"
!macroend

!macro RenameInstallDirectory
  !insertmacro GET_BACKUP_POSTFIX $R0

  ${If} $DeployMode == 1
  ${AndIf} $noServer == 1   ; this means the client is being installed, tv server was installed before, so the instdir shouldn't be renamed again       noClient

    ${LOG_TEXT} "INFO" "!!!!!!!!!!!! the client is being installed, tv server was installed before, so the instdir shouldn't be renamed again"

  ${Else}

    !insertmacro RenameDirectory "$INSTDIR" "$INSTDIR_$R0"
    !insertmacro RenameDirectory "${COMMON_APPDATA}" "${COMMON_APPDATA}_$R0"

  ${EndIf}
!macroend

!macro BackupGentleConfig
  GetTempFileName $PREVIOUS_GENTLE_CONFIG
  ${If} ${FileExists} "${COMMON_APPDATA}\Gentle.config"
    ${LOG_TEXT} "INFO" "Backup Gentle.Config (${COMMON_APPDATA}\Gentle.config)"
    CopyFiles /SILENT /FILESONLY "${COMMON_APPDATA}\Gentle.config" "$PREVIOUS_GENTLE_CONFIG"
  ${EndIf}

  GetTempFileName $PREVIOUS_GENTLE_CONFIG_PLUGIN
  ${If} ${FileExists} "$MPdir.Config\Gentle.config"
    ${LOG_TEXT} "INFO" "Backup Gentle.Config ($MPdir.Config\Gentle.config)"
    CopyFiles /SILENT /FILESONLY "$MPdir.Config\Gentle.config" "$PREVIOUS_GENTLE_CONFIG_PLUGIN"
  ${EndIf}
!macroend

!macro RestoreGentleConfig
  ${If} ${FileExists} "$PREVIOUS_GENTLE_CONFIG"
    ${LOG_TEXT} "INFO" "Restore Gentle.Config (${COMMON_APPDATA}\Gentle.config)"
    CopyFiles /SILENT /FILESONLY "$PREVIOUS_GENTLE_CONFIG" "${COMMON_APPDATA}\Gentle.config" 
  ${EndIf}

  ${If} ${FileExists} "$PREVIOUS_GENTLE_CONFIG_PLUGIN"
    ${LOG_TEXT} "INFO" "Restore Gentle.Config ($MPdir.Config\Gentle.config)"
    CopyFiles /SILENT /FILESONLY "$PREVIOUS_GENTLE_CONFIG_PLUGIN" "$MPdir.Config\Gentle.config"
  ${EndIf}
!macroend

Function RunUninstaller

  ${VersionCompare} 1.0.2.22779 $PREVIOUS_VERSION $R0
  ; old (un)installers should be called silently
  ${If} $R0 == 2 ;previous is higher than 22780
    !insertmacro RunUninstaller "NoSilent"
  ${Else}
    !insertmacro RunUninstaller "silent"
  ${EndIf}

FunctionEnd


#---------------------------------------------------------------------------
# SECTIONS and REMOVEMACROS
#---------------------------------------------------------------------------
Section "-prepare" SecPrepare
  ${LOG_TEXT} "DEBUG" "SECTION SecPrepare"
  ${LOG_TEXT} "INFO" "Prepare installation..."
  SetShellVarContext all

  ; uninstall old version if necessary
  ${If} $UpdateMode = 1
    ${LOG_TEXT} "DEBUG" "SecPrepare: DeployMode = 1 | UpdateMode = 1"

    ; check current install mode: TVServer / TVClient / SingleSeat (both)
    ; if only TVServer, uninstall current one and go on with installation of new one
    ; if only TVClient, uninstall current one and go on with installation of new one
    ; if SingleSeat, if NoClient (Server installation), uninstall current one (removes both components) and install new server component
    ; if SingleSeat, if NoServer (Client installation), Do NOT uninstall current one (was removed on during server install already), but install new client component
    ${If} $noClient == 1

      ${LOG_TEXT} "INFO" "TVServer component will be installed soon. (/noClient was used)"

      ${LOG_TEXT} "INFO" "InstallMode is: Dedicated TVServer or SingleSeat."
      ${LOG_TEXT} "INFO" "  ==>    Old TVServer/Client will be uninstalled now."
      !insertmacro NsisSilentUinstall "${TV3_REG_UNINSTALL}"

    ${ElseIf} $noServer == 1

      ${LOG_TEXT} "INFO" "TVClient component will be installed soon. (/noServer was used)"

      ${If} ${TVServerIsInstalled}
        ${LOG_TEXT} "INFO" "TVServer component is already installed."
        ${LOG_TEXT} "INFO" "     --> Means it is a SingleSeat installation."
        ${LOG_TEXT} "INFO" "     --> Means TVServer was already updated."
        ${LOG_TEXT} "INFO" "  ==>    TVServer/Client won't be uninstalled."
      ${Else}
        ${LOG_TEXT} "INFO" "TVServer component isn't already installed."
        ${LOG_TEXT} "INFO" "     --> Means it is a TVClient-only installation."
        ${LOG_TEXT} "INFO" "  ==>    Old TVClient will be uninstalled now."
        !insertmacro NsisSilentUinstall "${TV3_REG_UNINSTALL}"
      ${EndIf}

    ${EndIf}

  ${ElseIf} $DeployMode == 1
    ${LOG_TEXT} "DEBUG" "SecPrepare: DeployMode = 1 | UpdateMode = 0"

    !insertmacro RenameInstallDirectory

  ${Else}
    ${LOG_TEXT} "DEBUG" "SecPrepare: DeployMode = 0 | UpdateMode = 0"

    !insertmacro BackupGentleConfig

    ${LOG_TEXT} "INFO" "Uninstalling old version ..."
    ${If} ${Silent}
      !insertmacro RunUninstaller "silent"
    ${ElseIf} $EXPRESS_UPDATE != ""
      Call RunUninstaller
      BringToFront
    ${EndIf}

  ${EndIf}

SectionEnd

${MementoSection} "MediaPortal TV Server" SecServer
  ${LOG_TEXT} "DEBUG" "MementoSection SecServer"
  ${LOG_TEXT} "INFO" "Installing MediaPortal TV Server..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${StopService} "TVservice"
  ${KillProcess} "SetupTv.exe"

  SetOverwrite on

  ReadRegStr $InstallPath HKLM "${REG_UNINSTALL}" InstallPath
  ${If} $InstallPath != ""
    ${LOG_TEXT} "INFO" "Uninstalling TVService"
    ExecWait '"$InstallPath\TVService.exe" /uninstall'
    ${LOG_TEXT} "INFO" "Finished uninstalling TVService"
  ${EndIf}

  Pop $0

  #---------------------------- File Copy ----------------------
  ; Tuning Parameter Directory
  SetOutPath "${COMMON_APPDATA}\TuningParameters"
  File /r /x .svn "${TVSERVER.BASE}\TuningParameters\*"
  ; WebEPG Grabbers Directory
  SetOutPath "${COMMON_APPDATA}\WebEPG"
  File /r /x .svn "${TVSERVER.BASE}\WebEPG\*"

  ; The Plugin Directory
  SetOutPath "$INSTDIR\Plugins"
  File "${svn_TVServer}\Plugins\ComSkipLauncher\bin\${BUILD_TYPE}\ComSkipLauncher.dll"
  File "${svn_TVServer}\Plugins\ConflictsManager\bin\${BUILD_TYPE}\ConflictsManager.dll"
  File "${svn_TVServer}\Plugins\PowerScheduler\bin\${BUILD_TYPE}\PowerScheduler.dll"
  File "${svn_TVServer}\Plugins\ServerBlaster\ServerBlaster\bin\${BUILD_TYPE}\ServerBlaster.dll"
  File "${svn_TVServer}\Plugins\TvMovie\bin\${BUILD_TYPE}\TvMovie.dll"
  File "${svn_TVServer}\Plugins\XmlTvImport\bin\${BUILD_TYPE}\XmlTvImport.dll"
  File "${svn_TVServer}\Plugins\WebEPG\WebEPG\bin\${BUILD_TYPE}\WebEPG.dll"
  File "${svn_TVServer}\Plugins\WebEPG\WebEPGPlugin\bin\${BUILD_TYPE}\WebEPGImport.dll"

  ; Rest of Files
  SetOutPath "$INSTDIR"
  File "${svn_TVServer}\DirectShowLib\bin\${BUILD_TYPE}\DirectShowLib.dll"
  File "${svn_TVServer}\Plugins\PluginBase\bin\${BUILD_TYPE}\PluginBase.dll"
  File "${svn_Common_MP_TVE3}\PowerScheduler.Interfaces\bin\${BUILD_TYPE}\PowerScheduler.Interfaces.dll"
  File "${svn_TVServer}\Plugins\ServerBlaster\ServerBlaster (Learn)\bin\${BUILD_TYPE}\Blaster.exe"
  File "${svn_TVServer}\SetupTv\bin\${BUILD_TYPE}\SetupTv.exe"
  File "${svn_TVServer}\SetupTv\bin\${BUILD_TYPE}\SetupTv.exe.config"
  File "${svn_TVServer}\TvControl\bin\${BUILD_TYPE}\TvControl.dll"
  File "${svn_TVServer}\TVDatabase\bin\${BUILD_TYPE}\TVDatabase.dll"
  File "${svn_TVServer}\TVDatabase\references\Gentle.Common.DLL"
  File "${svn_TVServer}\TVDatabase\references\Gentle.Framework.DLL"
  File "${svn_TVServer}\TVDatabase\references\Gentle.Provider.MySQL.dll"
  File "${svn_TVServer}\TVDatabase\references\Gentle.Provider.SQLServer.dll"
  File "${svn_TVServer}\TVDatabase\references\log4net.dll"
  File "${svn_TVServer}\TVDatabase\references\MySql.Data.dll"
  File "${svn_TVServer}\TVDatabase\TvBusinessLayer\bin\${BUILD_TYPE}\TvBusinessLayer.dll"
  File "${svn_TVServer}\TvLibrary.Interfaces\bin\${BUILD_TYPE}\TvLibrary.Interfaces.dll"
  File "${svn_TVServer}\TVLibrary\bin\${BUILD_TYPE}\TVLibrary.dll"
  File "${svn_TVServer}\TvService\bin\${BUILD_TYPE}\TvService.exe"
  File "${svn_TVServer}\TvService\bin\${BUILD_TYPE}\TvService.exe.config"
  File "${svn_TVServer}\SetupControls\bin\${BUILD_TYPE}\SetupControls.dll"
  File "${svn_TVServer}\TVLibrary.Utils\bin\${BUILD_TYPE}\TVLibrary.Utils.dll"
  File "${svn_TVServer}\TVLibrary.Utils\bin\${BUILD_TYPE}\Interop.SHDocVw.dll"

  ; 3rd party assemblys
  File "${TVSERVER.BASE}\hauppauge.dll"
  File "${TVSERVER.BASE}\hcwWinTVCI.dll"
  File "${TVSERVER.BASE}\KNCBDACTRL.dll"
  File "${TVSERVER.BASE}\ttBdaDrvApi_Dll.dll"
  File "${TVSERVER.BASE}\ttdvbacc.dll"
  File "${TVSERVER.BASE}\Ionic.Zip.dll"

  File "${svn_DirectShowFilters}\StreamingServer\bin\${BUILD_TYPE}\StreamingServer.dll"
  
  File "${svn_DirectShowFilters}\DXErr9\bin\${BUILD_TYPE}\dxerr9.dll"
  ; binary used for skystar2 support
  File "${svn_DirectShowFilters}\dvblib\bin\${BUILD_TYPE}\dvblib.dll"

  ; Common App Data Files
  SetOutPath "${COMMON_APPDATA}"
  File "${svn_TVServer}\TvService\Gentle.config"
  File "${TVSERVER.BASE}\HelpReferences.xml"

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION   for TVServer
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  ${LOG_TEXT} "INFO" "filter registration..."
  ; filters for digital tv
  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\TsReader\bin\${BUILD_TYPE}\TsReader.ax" "$INSTDIR\TsReader.ax" "$INSTDIR"
  ${EndIf}
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\TsWriter\bin\${BUILD_TYPE}\TsWriter.ax" "$INSTDIR\TsWriter.ax" "$INSTDIR"
  ; filters for analog tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\MPWriter\bin\${BUILD_TYPE}\mpFileWriter.ax" "$INSTDIR\mpFileWriter.ax" "$INSTDIR"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\bin\Release\PDMpgMux.ax" "$INSTDIR\PDMpgMux.ax" "$INSTDIR"
  ; filter for IPTV support
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\MPIPTVSource\bin\${BUILD_TYPE}\MPIPTVSource.ax" "$INSTDIR\MPIPTVSource.ax" "$INSTDIR"

  #---------------------------------------------------------------------------
  # SERVICE INSTALLATION
  #---------------------------------------------------------------------------
  ${LOG_TEXT} "INFO" "Installing TVService"
  ExecWait '"$INSTDIR\TVService.exe" /install'
  ${LOG_TEXT} "INFO" "Finished Installing TVService"

  SetOutPath $INSTDIR
  ${If} $noDesktopSC != 1
    CreateShortCut "$DESKTOP\TV-Server Configuration.lnk" "$INSTDIR\SetupTV.exe" "" "$INSTDIR\SetupTV.exe" 0 "" "" "MediaPortal TV Server"
  ${EndIf}

  ;${If} $noStartMenuSC != 1
    ;!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
    CreateDirectory "${STARTMENU_GROUP}"
    CreateShortCut "${STARTMENU_GROUP}\TV-Server Configuration.lnk" "$INSTDIR\SetupTV.exe"  "" "$INSTDIR\SetupTV.exe"  0 "" "" "TV-Server Configuration"
    CreateDirectory "${COMMON_APPDATA}\log"
    CreateShortCut "${STARTMENU_GROUP}\TV-Server Log-Files.lnk"     "${COMMON_APPDATA}\log" "" "${COMMON_APPDATA}\log" 0 "" "" "TV-Server Log-Files"
    # [OBSOLETE] CreateShortcut "${STARTMENU_GROUP}\MCE Blaster Learn.lnk" "$INSTDIR\Blaster.exe" "" "$INSTDIR\Blaster.exe" 0 "" "" "MCE Blaster Learn"
    ;!insertmacro MUI_STARTMENU_WRITE_END
  ;${EndIf}
${MementoSectionEnd}
!macro Remove_${SecServer}
  ${LOG_TEXT} "DEBUG" "MACRO Remove_${SecServer}"
  ${LOG_TEXT} "INFO" "Uninstalling MediaPortal TV Server..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${StopService} "TVservice"
  ${KillProcess} "SetupTv.exe"

  #---------------------------------------------------------------------------
  # CLEARING DATABASE if RemoveAll was selected
  #---------------------------------------------------------------------------
  ${If} $UnInstallMode == 1
  ${OrIf} $UnInstallMode == 2
    ExecWait '"$INSTDIR\SetupTv.exe" --delete-db'
  ${EndIf}

  #---------------------------------------------------------------------------
  # SERVICE UNINSTALLATION
  #---------------------------------------------------------------------------
  ${LOG_TEXT} "INFO" "DeInstalling TVService"
  ExecWait '"$INSTDIR\TVService.exe" /uninstall'
  ${LOG_TEXT} "INFO" "Finished DeInstalling TVService"

  #---------------------------------------------------------------------------
  # FILTER UNREGISTRATION     for TVServer
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  ${LOG_TEXT} "INFO" "Unreg and remove filters..."
  ; filters for digital tv
  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$INSTDIR\TsReader.ax"
    WriteRegStr HKCR "Media Type\Extensions\.ts"        "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
    WriteRegStr HKCR "Media Type\Extensions\.tp"        "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
    WriteRegStr HKCR "Media Type\Extensions\.tsbuffer"  "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
    WriteRegStr HKCR "Media Type\Extensions\.rtsp"      "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
  ${EndIf}
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$INSTDIR\TsWriter.ax"
  ; filters for analog tv
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$INSTDIR\mpFileWriter.ax"
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$INSTDIR\PDMpgMux.ax"
  ; filter for IPTV support
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$INSTDIR\MPIPTVSource.ax"

  ${LOG_TEXT} "INFO" "remove files..."
  ; Remove TuningParameters
  RMDir /r "${COMMON_APPDATA}\TuningParameters"
  ; Remove WebEPG Grabbers
  RMDir /r "${COMMON_APPDATA}\WebEPG"

  ; Remove Plugins
  Delete "$INSTDIR\Plugins\ComSkipLauncher.dll"
  Delete "$INSTDIR\Plugins\ConflictsManager.dll"
  Delete "$INSTDIR\Plugins\PowerScheduler.dll"
  Delete "$INSTDIR\Plugins\ServerBlaster.dll"
  Delete "$INSTDIR\Plugins\TvMovie.dll"
  Delete "$INSTDIR\Plugins\WebEPG.dll"
  Delete "$INSTDIR\Plugins\WebEPGImport.dll"
  Delete "$INSTDIR\Plugins\XmlTvImport.dll"
  RMDir "$INSTDIR\Plugins"

  ; And finally remove all the files installed
  ; Leave the directory in place, as it might contain user modified files
  Delete "$INSTDIR\DirectShowLib.dll"
  ; binary used for skystar2 support
  Delete "$INSTDIR\dvblib.dll"
  Delete "$INSTDIR\PluginBase.dll"
  Delete "$INSTDIR\PowerScheduler.Interfaces.DLL"
  Delete "$INSTDIR\Blaster.exe"
  Delete "$INSTDIR\SetupTv.exe"
  Delete "$INSTDIR\SetupTv.exe.config"
  Delete "$INSTDIR\TvControl.dll"
  Delete "$INSTDIR\TVDatabase.dll"
  Delete "$INSTDIR\Gentle.Common.DLL"
  Delete "$INSTDIR\Gentle.Framework.DLL"
  Delete "$INSTDIR\Gentle.Provider.MySQL.dll"
  Delete "$INSTDIR\Gentle.Provider.SQLServer.dll"
  Delete "$INSTDIR\log4net.dll"
  Delete "$INSTDIR\MySql.Data.dll"
  Delete "$INSTDIR\TvBusinessLayer.dll"
  Delete "$INSTDIR\TvLibrary.Interfaces.dll"
  Delete "$INSTDIR\TVLibrary.dll"
  Delete "$INSTDIR\Germany_Unitymedia_NRW.dvbc"
  Delete "$INSTDIR\TvService.exe"
  Delete "$INSTDIR\TvService.exe.config"
  Delete "$INSTDIR\SetupControls.dll"

  ; 3rd party assemblys
  Delete "$INSTDIR\dxerr9.dll"
  Delete "$INSTDIR\hauppauge.dll"
  Delete "$INSTDIR\hcwWinTVCI.dll"
  Delete "$INSTDIR\KNCBDACTRL.dll"
  Delete "$INSTDIR\StreamingServer.dll"
  Delete "$INSTDIR\ttBdaDrvApi_Dll.dll"
  Delete "$INSTDIR\ttdvbacc.dll"
  Delete "$INSTDIR\Ionic.Zip.dll"

  ; remove Start Menu shortcuts
  Delete "${STARTMENU_GROUP}\TV-Server Configuration.lnk"
  Delete "${STARTMENU_GROUP}\TV-Server Log-Files.lnk"
  ; remove Desktop shortcuts
  Delete "$DESKTOP\TV-Server Configuration.lnk"
!macroend

${MementoSection} "MediaPortal TV Client plugin" SecClient
  ${LOG_TEXT} "DEBUG" "MementoSection SecClient"
  ${LOG_TEXT} "INFO" "Installing MediaPortal TV Client plugin..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${KillProcess} "MediaPortal.exe"
  ${KillProcess} "configuration.exe"

  SetOverwrite on

  ${LOG_TEXT} "INFO" "MediaPortal Installed at: $MPdir.Base"
  ${LOG_TEXT} "INFO" "MediaPortalPlugins are at: $MPdir.Plugins"
  
  #---------------------------- File Copy ----------------------
  ; Common Files
  SetOutPath "$MPdir.Base"
  File "${svn_TVServer}\TvControl\bin\${BUILD_TYPE}\TvControl.dll"
  File "${svn_TVServer}\TvLibrary.Interfaces\bin\${BUILD_TYPE}\TvLibrary.Interfaces.dll"
  File "${svn_TVServer}\TVDatabase\bin\${BUILD_TYPE}\TVDatabase.dll"
  File "${svn_TVServer}\TVDatabase\references\Gentle.Common.DLL"
  File "${svn_TVServer}\TVDatabase\references\Gentle.Framework.DLL"
  File "${svn_TVServer}\TVDatabase\references\Gentle.Provider.MySQL.dll"
  File "${svn_TVServer}\TVDatabase\references\Gentle.Provider.SQLServer.dll"
  File "${svn_TVServer}\TVDatabase\references\log4net.dll"
  File "${svn_TVServer}\TVDatabase\references\MySql.Data.dll"
  File "${svn_TVServer}\TVDatabase\TvBusinessLayer\bin\${BUILD_TYPE}\TvBusinessLayer.dll"

  ;Gentle.Config
  SetOutPath "$MPdir.Config"
  File "${svn_TVServer}\TvPlugin\TvPlugin\Gentle.config"

  ; The Plugins
  SetOutPath "$MPdir.Plugins\Windows"
  File "${svn_TVServer}\TvPlugin\TvPlugin\bin\${BUILD_TYPE}\TvPlugin.dll"

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION       for TVClient
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\DVBSubtitle2\bin\${BUILD_TYPE}\DVBSub2.ax" "$MPdir.Base\DVBSub2.ax"  "$MPdir.Base"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\bin\Release\mmaacd.ax"                     "$MPdir.Base\mmaacd.ax"   "$MPdir.Base"
${MementoSectionEnd}
!macro Remove_${SecClient}
  ${LOG_TEXT} "DEBUG" "MACRO Remove_${SecClient}"
  ${LOG_TEXT} "INFO" "Uninstalling MediaPortal TV Client plugin..."

  ${If} ${TVClientIsInstalled}
    ${LOG_TEXT} "INFO" "TV Client plugin is installed"

    ${If} $MPdir.Base = ""
      ${LOG_TEXT} "ERROR" "MediaPortal Directory not found, TVClient plugin uninstallation will fail!!"
    ${Else}
      ${LOG_TEXT} "INFO" "Removing TV Client plugin in: $MPdir.Base"
    ${EndIf}

    ${Else}
    ${LOG_TEXT} "INFO" "TV Client plugin is -- NOT -- installed"
  ${EndIf}

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${KillProcess} "MediaPortal.exe"
  ${KillProcess} "configuration.exe"

  #---------------------------------------------------------------------------
  # FILTER UNREGISTRATION     for TVClient
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\DVBSub2.ax"
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\mmaacd.ax"

  ; The Plugins
  Delete "$MPdir.Plugins\Windows\TvPlugin.dll"

  ; Common Files
  Delete "$MPdir.Base\TVDatabase.dll"
  Delete "$MPdir.Base\Gentle.Common.DLL"
  Delete "$MPdir.Base\Gentle.Framework.DLL"
  Delete "$MPdir.Base\Gentle.Provider.MySQL.dll"
  Delete "$MPdir.Base\Gentle.Provider.SQLServer.dll"
  Delete "$MPdir.Base\log4net.dll"
  Delete "$MPdir.Base\MySql.Data.dll"
  Delete "$MPdir.Base\TvBusinessLayer.dll"
  Delete "$MPdir.Base\TvControl.dll"
  Delete "$MPdir.Base\TvLibrary.Interfaces.dll"
!macroend

${MementoSectionDone}

#---------------------------------------------------------------------------
# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
Section -Post
  ${LOG_TEXT} "DEBUG" "SECTION Post"
  ${LOG_TEXT} "INFO" "Doing post installation stuff..."

  ${If} $DeployMode == 1

    #MessageBox MB_OK|MB_ICONEXCLAMATION "DeployMode == 1"
    ReadRegDWORD $R0 ${MEMENTO_REGISTRY_ROOT} '${MEMENTO_REGISTRY_KEY}' 'MementoSection_SecServer'
    ReadRegDWORD $R1 ${MEMENTO_REGISTRY_ROOT} '${MEMENTO_REGISTRY_KEY}' 'MementoSection_SecClient'

    ;writes component status to registry
    ${MementoSectionSave}

    ${If} $noClient == 1
    ${AndIf} $R1 != ""
      WriteRegDWORD ${MEMENTO_REGISTRY_ROOT} "${MEMENTO_REGISTRY_KEY}" 'MementoSection_SecClient' $R1
    ${ElseIf} $noServer == 1
    ${AndIf} $R0 != ""
      WriteRegDWORD ${MEMENTO_REGISTRY_ROOT} "${MEMENTO_REGISTRY_KEY}" 'MementoSection_SecServer' $R0
    ${EndIf}

  ${Else}

    ;Removes unselected components
    !insertmacro SectionList "FinishSection"

    ;writes component status to registry
    ${MementoSectionSave}

    !insertmacro RestoreGentleConfig

  ${EndIf}


  SetOverwrite on
  SetOutPath $INSTDIR

  ; cleaning/renaming log dir - requested by chemelli
  RMDir /r "${COMMON_APPDATA}\log\OldLogs"
  CreateDirectory "${COMMON_APPDATA}\log\OldLogs"
  CopyFiles /SILENT /FILESONLY "${COMMON_APPDATA}\log\*" "${COMMON_APPDATA}\log\OldLogs"
  Delete "${COMMON_APPDATA}\log\*"

  ; if TVplugin is enabled, save MP installation path to uninstall it even if mp is already uninstalled
  ${If} ${TVClientIsInstalled}
    WriteRegDWORD HKLM "${REG_UNINSTALL}" "MediaPortalInstallationDir" "$MPdir.Base"
  ${EndIf}

  ;${If} $noStartMenuSC != 1
    ;!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
    CreateDirectory "${STARTMENU_GROUP}"
    CreateShortCut "${STARTMENU_GROUP}\uninstall TV-Server.lnk" "$INSTDIR\uninstall-tve3.exe"
    WriteINIStr "${STARTMENU_GROUP}\Help.url"      "InternetShortcut" "URL" "http://wiki.team-mediaportal.com/"
    WriteINIStr "${STARTMENU_GROUP}\web site.url"  "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
    ;!insertmacro MUI_STARTMENU_WRITE_END
  ;${EndIf}

  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMajor"    "${VER_MAJOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMinor"    "${VER_MINOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionRevision" "${VER_REVISION}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionBuild"    "${VER_BUILD}"

  ; Write Uninstall Information
  WriteRegStr HKLM "${REG_UNINSTALL}" InstallPath        $INSTDIR
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName        "${PRODUCT_NAME}"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayVersion     "${VERSION}"
  WriteRegStr HKLM "${REG_UNINSTALL}" Publisher          "${PRODUCT_PUBLISHER}"
  WriteRegStr HKLM "${REG_UNINSTALL}" URLInfoAbout       "${PRODUCT_WEB_SITE}"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon        "$INSTDIR\SetupTv.exe,0"
  WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString    "$INSTDIR\uninstall-tve3.exe"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 1
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 1

  WriteUninstaller "$INSTDIR\uninstall-tve3.exe"

  ; set rights to programmdata directory and reg keys
  !insertmacro SetRights
SectionEnd

#---------------------------------------------------------------------------
# This section is called on uninstall and removes all components
Section Uninstall
  ${LOG_TEXT} "DEBUG" "SECTION Uninstall"
  ;First removes all optional components
  !insertmacro SectionList "RemoveSection"

  ; remove registry key
  DeleteRegKey HKLM "${REG_UNINSTALL}"

  ; remove Start Menu shortcuts
  ; $StartMenuGroup (default): "Team MediaPortal\TV Server"
  Delete "${STARTMENU_GROUP}\uninstall TV-Server.lnk"
  Delete "${STARTMENU_GROUP}\Help.url"
  Delete "${STARTMENU_GROUP}\web site.url"
  RMDir "${STARTMENU_GROUP}"
  RMDir "$SMPROGRAMS\Team MediaPortal"

  ; remove last files and instdir
  RMDir "$INSTDIR\pmt"
  Delete "$INSTDIR\uninstall-tve3.exe"
  RMDir "$INSTDIR"



  ${If} $UnInstallMode == 1

    ${LOG_TEXT} "INFO" "Removing User Settings"
    RMDir /r "${COMMON_APPDATA}"
    RMDir /r $INSTDIR

    RMDir /r "$LOCALAPPDATA\VirtualStore\ProgramData\Team MediaPortal\MediaPortal TV Server"
    RMDir /r "$LOCALAPPDATA\VirtualStore\Program Files\Team MediaPortal\MediaPortal TV Server"
    RMDir "$LOCALAPPDATA\VirtualStore\ProgramData\Team MediaPortal"
    RMDir "$LOCALAPPDATA\VirtualStore\Program Files\Team MediaPortal"

  ${ElseIf} $UnInstallMode == 2

    !insertmacro CompleteMediaPortalCleanup

  ${EndIf}


  ${If} $frominstall == 1
    Quit
  ${EndIf}
SectionEnd


#---------------------------------------------------------------------------
# SOME MACROS AND FUNCTIONS
#---------------------------------------------------------------------------
Function ReadPreviousSettings
  ; read and analyze previous version
  !insertmacro ReadPreviousVersion

  ; read previous used directories
  !insertmacro TVSERVER_GET_INSTALL_DIR $PREVIOUS_INSTALLDIR
FunctionEnd

Function LoadPreviousSettings
  ; reset INSTDIR
  ${If} "$PREVIOUS_INSTALLDIR" != ""
    StrCpy $INSTDIR "$PREVIOUS_INSTALLDIR"
  ${ElseIf} "$INSTDIR" == ""
    StrCpy $INSTDIR "$PROGRAMFILES\Team MediaPortal\MediaPortal TV Server"
  ${EndIf}


  ; reset previous component selection from registry
  ${MementoSectionRestore}

  ; set sections, according to possible selections
  ${IfNot} ${MPIsInstalled}
    !insertmacro DisableSection "${SecClient}" "MediaPortal TV Client plugin" " ($(TEXT_MP_NOT_INSTALLED))"
  ${else}
    !insertmacro EnableSection "${SecClient}" "MediaPortal TV Client plugin"

    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${ReadMediaPortalDirs} $MPdir.Base
  ${EndIf}

  ; update component selection
  Call .onSelChange
FunctionEnd


#---------------------------------------------------------------------------
# INSTALLER CALLBACKS
#---------------------------------------------------------------------------
Function .onInit
  ${LOG_OPEN}
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInit"


  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $noClient 0
  StrCpy $noServer 0
  StrCpy $noDesktopSC 0
  ;StrCpy $noStartMenuSC 0
  StrCpy $DeployMode 0
  StrCpy $UpdateMode 0

  ${InitCommandlineParameter}
  ${ReadCommandlineParameter} "noClient"
  ${ReadCommandlineParameter} "noServer"
  ${ReadCommandlineParameter} "noDesktopSC"
  ;${ReadCommandlineParameter} "noStartMenuSC"
  ${ReadCommandlineParameter} "DeployMode"

  ClearErrors
  ${GetOptions} $R0 "/UpdateMode" $R1
  IfErrors +2
  IntOp $UpdateMode $DeployMode & 1
  #### END of check and parse cmdline parameter


  ${If} $DeployMode != 1
    !insertmacro DoPreInstallChecks
  ${EndIf}


  Call ReadPreviousSettings
  Call LoadPreviousSettings


  ; update the component status -> commandline parameters have higher priority than registry values
  ${If} $noClient = 1
  ${AndIf} $noServer = 1
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_PARAMETER_ERROR)"
    Abort
  ${ElseIf} $noClient = 1
    !insertmacro SelectSection ${SecServer}
    !insertmacro UnselectSection ${SecClient}
  ${ElseIf} $noServer = 1
    !insertmacro SelectSection ${SecClient}
    !insertmacro UnselectSection ${SecServer}
  ${EndIf}

  SetShellVarContext all
FunctionEnd

Function .onInstFailed
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInstFailed"
  ${LOG_CLOSE}
FunctionEnd

Function .onInstSuccess
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInstSuccess"
  ${LOG_CLOSE}
FunctionEnd

Function .onSelChange
  ${LOG_TEXT} "DEBUG" "FUNCTION .onSelChange"

  ; disable the next button if nothing is selected
  ${IfNot} ${SectionIsSelected} ${SecServer}
  ${AndIfNot} ${SectionIsSelected} ${SecClient}
    EnableWindow $mui.Button.Next 0
  ${Else}
    EnableWindow $mui.Button.Next 1
  ${EndIf}
FunctionEnd

;======================================

Function PageComponentsPre
  ; skip page if previous settings are used for update
  ${If} $EXPRESS_UPDATE == 1
    Abort
  ${EndIf}
FunctionEnd

Function PageDirectoryPre
  ; skip page if previous settings are used for update
  ${If} $EXPRESS_UPDATE == 1
    Abort
  ${EndIf}

  ; It checks, if the Server has been selected and only displays the Directory page in this case
  ${IfNot} ${SectionIsSelected} SecServer
    Abort
  ${EndIf}
FunctionEnd

Function PageFinishShow
  ; This function is called, after the Finish Page creation is finished

  ; It checks, if the Server has been selected and only displays the run checkbox in this case
  ${IfNot} ${TVServerIsInstalled}
    SendMessage $mui.FinishPage.Run ${BM_CLICK} 0 0
    ShowWindow  $mui.FinishPage.Run ${SW_HIDE}
  ${Else}
    EnableWindow $mui.FinishPage.Run 0 # start out disabled
  ${EndIf}
FunctionEnd






#---------------------------------------------------------------------------
# UNINSTALLER CALLBACKS
#---------------------------------------------------------------------------
Function un.onInit
  ${un.LOG_OPEN}
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onInit"


  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $UnInstallMode 0

  ${un.InitCommandlineParameter}
  ${un.ReadCommandlineParameter} "frominstall"

  ; check for special parameter and set the their variables
  ClearErrors
  ${un.GetOptions} $R0 "/RemoveAll" $R1
  IfErrors +2
  StrCpy $UnInstallMode 1
  #### END of check and parse cmdline parameter


  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    Sleep 1
  ${else}
    ReadRegStr $MPdir.Base HKLM "${REG_UNINSTALL}" "MediaPortalInstallationDir"

    ${If} $MPdir.Base = ""
      !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${EndIf}

    ${un.ReadMediaPortalDirs} $MPdir.Base
  ${EndIf}

  !insertmacro TVSERVER_GET_INSTALL_DIR $INSTDIR
  ;!insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

  SetShellVarContext all
FunctionEnd

Function un.onUninstFailed
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onUninstFailed"
  ${un.LOG_CLOSE}
FunctionEnd

Function un.onUninstSuccess
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onUninstSuccess"

  ; write a reboot flag, if reboot is needed, so the installer won't continue until reboot is done
  ${If} ${RebootFlag}
    ${LOG_TEXT} "INFO" "!!! Some files were not able to uninstall. To finish uninstallation completly a REBOOT is needed."
    FileOpen $0 $INSTDIR\rebootflag w
    Delete $INSTDIR\rebootflag ; this will not be deleted until the reboot because it is currently opened
    RmDir $INSTDIR
    FileClose $0
  ${EndIf}

  ${un.LOG_CLOSE}
FunctionEnd

;======================================

Function un.WelcomePagePre

  ${If} $frominstall == 1
    Abort
  ${EndIf}

FunctionEnd
/*
Function un.ConfirmPagePre

  ${If} $frominstall == 1
    Abort
  ${EndIf}

FunctionEnd
*/
Function un.FinishPagePre

  ${If} $frominstall == 1
    SetRebootFlag false
    Abort
  ${EndIf}

FunctionEnd


#---------------------------------------------------------------------------
# SECTION DECRIPTIONS     must be at the end
#---------------------------------------------------------------------------
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} $(DESC_SecClient)
  !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} $(DESC_SecServer)
!insertmacro MUI_FUNCTION_DESCRIPTION_END
