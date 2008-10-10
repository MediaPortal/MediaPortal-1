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

#**********************************************************************************************************#
#
#   For building the installer on your own you need:
#       1. Lastest NSIS version from http://nsis.sourceforge.net/Download
#       2. The xml-plugin from http://nsis.sourceforge.net/XML_plug-in
#
#**********************************************************************************************************#
Name "MediaPortal"
SetCompressor /SOLID lzma

!ifdef SVN_BUILD
  !define MEDIAPORTAL.BASE "E:\compile\compare_mp1_test"
!else
  !define MEDIAPORTAL.BASE "..\MediaPortal.Base"
!endif
!define MEDIAPORTAL.FILTERBIN "..\..\DirectShowFilters\bin\Release"
!define MEDIAPORTAL.XBMCBIN "..\xbmc\bin\Release"

!define BUILD_TYPE "Release"
;!define BUILD_TYPE "Debug"

#---------------------------------------------------------------------------
# SPECIAL BUILDS
#---------------------------------------------------------------------------
; Uncomment the following line to create a special installer for "Heise Verlag" / ct' magazine
;!define HEISE_BUILD

#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
Var StartMenuGroup  ; Holds the Startmenu\Programs folder
; variables for commandline parameters for Installer
!ifndef HEISE_BUILD
Var noGabest
!endif
Var noDesktopSC
Var noStartMenuSC
Var DeployMode
; variables for commandline parameters for UnInstaller
Var RemoveAll       ; Set, when the user decided to uninstall everything

#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"

!define WEB_REQUIREMENTS "http://wiki.team-mediaportal.com/GeneralRequirements/OperatingSystems"


!define REG_UNINSTALL         "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!define MEMENTO_REGISTRY_ROOT HKLM
!define MEMENTO_REGISTRY_KEY  "${REG_UNINSTALL}"
!define COMMON_APPDATA        "$APPDATA\Team MediaPortal\MediaPortal"

!define VER_MAJOR       0
!define VER_MINOR       9
!define VER_REVISION    3
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif
!if ${VER_BUILD} == 0       # it's a stable release
    !define VERSION "1.0 RC3"
!else                       # it's an svn reöease
    !define VERSION "1.0 RC2 SVN build ${VER_BUILD} for TESTING ONLY"
!endif
BrandingText "$(^Name) ${VERSION} by ${COMPANY}"

!define INSTALL_LOG

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include LogicLib.nsh
!include Library.nsh
!include FileFunc.nsh
;!include WinVer.nsh
!define WinVer++
!include Memento.nsh

!include "${svn_InstallScripts}\include-AddRemovePage.nsh"
!include "${svn_InstallScripts}\include-CommonMPMacros.nsh"
!include setup-languages.nsh

; FileFunc macros
!insertmacro GetParameters
!insertmacro GetOptions
!insertmacro un.GetParameters
!insertmacro un.GetOptions
!insertmacro GetParent
!insertmacro RefreshShellIcons
!insertmacro un.RefreshShellIcons

#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ABORTWARNING
!define MUI_ICON    "Resources\install.ico"
!define MUI_UNICON  "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP              "Resources\header.bmp"
!if ${VER_BUILD} == 0       # it's a stable release
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "Resources\wizard.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "Resources\wizard.bmp"
!else                       # it's an svn reöease
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "Resources\wizard-svn.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "Resources\wizard-svn.bmp"
!endif
!define MUI_HEADERIMAGE_RIGHT

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER       "Team MediaPortal\MediaPortal"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT       HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY        "${REG_UNINSTALL}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN      "$MPdir.Base\Configuration.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run MediaPortal Configuration"
#!define MUI_FINISHPAGE_SHOWREADME $INSTDIR\readme.txt
#!define MUI_FINISHPAGE_SHOWREADME_TEXT "View Readme"
#!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_LINK "Donate to MediaPortal"
!define MUI_FINISHPAGE_LINK_LOCATION "http://www.team-mediaportal.com/donate.html"

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
#!define MUI_PAGE_CUSTOMFUNCTION_LEAVE WelcomeLeave
!insertmacro MUI_PAGE_WELCOME

!ifndef SVN_BUILD
Page custom PageReinstall PageLeaveReinstall
!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
!insertmacro MUI_PAGE_LICENSE "..\Docs\BASS License.txt"
!else
#!insertmacro MUI_PAGE_LICENSE "..\Docs\svn-info.rtf"
!endif

!ifndef HEISE_BUILD
!insertmacro MUI_PAGE_COMPONENTS
!endif
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup


# OBSOLETE - old code to rename existing dirs
#!define MUI_PAGE_CUSTOMFUNCTION_PRE InstFilePre
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
; UnInstaller Interface
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE un.WelcomeLeave
!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

#---------------------------------------------------------------------------
# INSTALLER LANGUAGES
#---------------------------------------------------------------------------
!insertmacro MUI_LANGUAGE English

#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
!if ${VER_BUILD} == 0
  OutFile "Release\package-mediaportal.exe"
!else
  OutFile "Release\MediaPortal-svn-.exe"
!endif
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal"
InstallDirRegKey HKLM "${REG_UNINSTALL}" InstallPath
CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName       "${NAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductVersion    "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyName       "${COMPANY}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyWebsite    "${URL}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileVersion       "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription   ""
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright    ""
ShowUninstDetails show

#---------------------------------------------------------------------------
# USEFUL MACROS
#--------------------------------------------------------------------------- 
!macro SectionList MacroName
  ${LOG_TEXT} "DEBUG" "MACRO SectionList ${MacroName}"
  ; This macro used to perform operation on multiple sections.
  ; List all of your components in following manner here.
!ifndef HEISE_BUILD
  !insertmacro "${MacroName}" "SecGabest"
!endif
!macroend

#---------------------------------------------------------------------------
# SECTIONS and REMOVEMACROS
#---------------------------------------------------------------------------
Section "-prepare" SecPrepare
  ${LOG_TEXT} "DEBUG" "SECTION SecPrepare"
  ${LOG_TEXT} "INFO" "Prepare installation..."
  ${ReadMediaPortalDirs} "$INSTDIR"

  ${LOG_TEXT} "INFO" "Terminating processes..."
  ${KILLPROCESS} "MediaPortal.exe"
  ${KILLPROCESS} "configuration.exe"

  ${KILLPROCESS} "MPInstaller.exe"
  ${KILLPROCESS} "WatchDog.exe"
  ${KILLPROCESS} "MusicShareWatcher.exe"
  ${KILLPROCESS} "TVGuideScheduler.exe"
  ${KILLPROCESS} "WebEPG.exe"
  ${KILLPROCESS} "WebEPG-conf.exe"

  ${LOG_TEXT} "INFO" "Deleting SkinCache..."
  RMDir /r "$MPdir.Cache"
SectionEnd

!if ${VER_BUILD} == 0       # it's an official release (stable or release candidate)
Section "-rename existing dirs" SecBackup
  ${LOG_TEXT} "DEBUG" "SECTION SecBackup"

  !insertmacro GET_BACKUP_POSTFIX $R0

  ${If} ${FileExists} "$MPdir.Base\*.*"
    ${LOG_TEXT} "INFO" "Installation dir already exists. It will be renamed."
    Rename "$MPdir.Base" "$MPdir.Base_$R0"
  ${EndIf}

  ${If} ${FileExists} "$MPdir.Config\*.*"
    ${LOG_TEXT} "INFO" "Configuration dir already exists. It will be renamed."
    Rename "$MPdir.Config" "$MPdir.Config_$R0"
  ${EndIf}

  ${If} ${FileExists} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml"
    ${LOG_TEXT} "INFO" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml already exists. It will be renamed."
    Rename "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml_$R0"
  ${EndIf}

SectionEnd
!else                       # it's a svn reöease
Section "Backup current installation status" SecBackup
  ${LOG_TEXT} "DEBUG" "SECTION SecBackup"

  !insertmacro GET_BACKUP_POSTFIX $R0

  ${LOG_TEXT} "INFO" "Creating backup of installation dir, this might take some minutes."
  CreateDirectory "$MPdir.Base_$R0"
  CopyFiles /SILENT "$MPdir.Base\*.*" "$MPdir.Base_$R0"

  ${LOG_TEXT} "INFO" "Creating backup of configuration dir, this might take some minutes."
  CreateDirectory "$MPdir.Config_$R0"
  CopyFiles /SILENT "$MPdir.Config\*.*" "$MPdir.Config_$R0"

SectionEnd
!endif

Section "MediaPortal core files (required)" SecCore
  SectionIn RO
  ${LOG_TEXT} "DEBUG" "SECTION SecCore"
  ${LOG_TEXT} "INFO" "Installing MediaPortal core files..."

  SetOverwrite on

  #CONFIG FILES ARE ALWAYS INSTALLED by SVN and FINAL releases, BECAUSE of the config dir location
  !define EXCLUDED_CONFIG_FILES "\
    /x CaptureCardDefinitions.xml \
    /x 'eHome Infrared Transceiver List XP.xml' \
    /x HelpReferences.xml \
    /x ISDNCodes.xml \
    /x keymap.xml \
    /x wikipedia.xml \
    /x yac-area-codes.xml \
    "

  # Files which were diffed before including in installer
  # means all of them are in full installer, but only the changed and new ones are in svn installer 
  #We can not use the complete mediaportal.base dir recoursivly , because the plugins, thumbs, weather need to be extracted to their special MPdir location
  # exluding only the folders does not work because /x plugins won't extract the \plugins AND musicplayer\plugins directory
  SetOutPath "$MPdir.Base"
  File /nonfatal /x .svn ${EXCLUDED_CONFIG_FILES}  "${MEDIAPORTAL.BASE}\*"
  SetOutPath "$MPdir.Base\MusicPlayer"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\MusicPlayer\*"
  SetOutPath "$MPdir.Base\osdskin-media"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\osdskin-media\*"
  SetOutPath "$MPdir.Base\Profiles"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Profiles\*"
  SetOutPath "$MPdir.Base\Tuningparameters"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Tuningparameters\*"
  SetOutPath "$MPdir.Base\WebEPG"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\WebEPG\*"
  SetOutPath "$MPdir.Base\Wizards"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Wizards\*"
  ; Doc
  SetOutPath "$MPdir.Base\Docs"
  File "..\Docs\BASS License.txt"
  File "..\Docs\MediaPortal License.rtf"
  #File "..\Docs\LICENSE.rtf"
  #File "..\Docs\SQLite Database Browser.exe"

  SetOutPath "$MPdir.BurnerSupport"
  CreateDirectory "$MPdir.BurnerSupport"
  SetOutPath "$MPdir.Config"
  CreateDirectory "$MPdir.Config"
  SetOutPath "$MPdir.Database"
  CreateDirectory "$MPdir.Database"
  SetOutPath "$MPdir.Log"
  CreateDirectory "$MPdir.Log"

  # COMMON CONFIG files for SVN and FINAL RELEASES
  SetOutPath "$MPdir.Config"
  File /nonfatal "${MEDIAPORTAL.BASE}\CaptureCardDefinitions.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\eHome Infrared Transceiver List XP.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\HelpReferences.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\ISDNCodes.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\keymap.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\wikipedia.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\yac-area-codes.xml"
  SetOutPath "$MPdir.Config\xmltv"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\xmltv\*"

  SetOutPath "$MPdir.Config\scripts\MovieInfo"
  File /nonfatal "${MEDIAPORTAL.BASE}\scripts\MovieInfo\IMDB.csscript"

  SetOutPath "$MPdir.CustomInputDefault"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\InputDeviceMappings\defaults\*"
  SetOutPath "$MPdir.Language"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\language\*"
  SetOutPath "$MPdir.Plugins"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\plugins\*"
  SetOutPath "$MPdir.Skin"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\skin\*"
  SetOutPath "$MPdir.Thumbs"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\thumbs\*"
  SetOutPath "$MPdir.Weather"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\weather\*"


  SetOutPath "$MPdir.Base"
  SetOverwrite on
  ; ========================================
  ; MediaPortalEXE
  ;should be           , but because of postbuild.bat there are too much matching files
  ;File "..\xbmc\bin\Release\${BUILD_TYPE}\MediaPortal.*"
  File "..\xbmc\bin\${BUILD_TYPE}\MediaPortal.exe"
  File "..\xbmc\bin\${BUILD_TYPE}\MediaPortal.exe.config"
  ; Configuration
  File "..\Configuration\bin\${BUILD_TYPE}\Configuration.*"

  ; ========================================
  ; Core
  File "..\core\bin\${BUILD_TYPE}\Core.*"
  File "..\core\bin\${BUILD_TYPE}\DirectShowLib.*"

  #those files are moved to MediaPortal.Base
  #File "..\core\directshowhelper\directshowhelper\Release\dshowhelper.dll"
  #File "..\core\DXUtil\Release\DXUtil.dll"
  #File "..\core\fontengine\fontengine\${BUILD_TYPE}\fontengine.*"

  ; Utils
  File "..\Utils\bin\${BUILD_TYPE}\Utils.dll"
  ; Support
  File "..\MediaPortal.Support\bin\${BUILD_TYPE}\MediaPortal.Support.*"
  ; Databases
  File "..\databases\bin\${BUILD_TYPE}\databases.*"
  ; TvCapture
  File "..\tvcapture\bin\${BUILD_TYPE}\tvcapture.*"
  ; TvGuideScheduler
  File "..\TVGuideScheduler\bin\${BUILD_TYPE}\TVGuideScheduler.*"

  ; ========================================
  ; MusicShareWatcher
  File "..\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\${BUILD_TYPE}\MusicShareWatcherHelper.*"
  File "..\ProcessPlugins\MusicShareWatcher\MusicShareWatcher\bin\${BUILD_TYPE}\MusicShareWatcher.exe"
  ; MPInstaller
  File "..\MPInstaller\bin\${BUILD_TYPE}\MPInstaller.*"
  File "..\MPInstaller\bin\${BUILD_TYPE}\MPInstaller.Library.*"
  ; WatchDog
  File "..\WatchDog\bin\${BUILD_TYPE}\WatchDog.exe"
  File "..\WatchDog\bin\${BUILD_TYPE}\DaggerLib.dll"
  File "..\WatchDog\bin\${BUILD_TYPE}\DaggerLib.DSGraphEdit.dll"
  File "..\WatchDog\bin\${BUILD_TYPE}\DirectShowLib-2005.dll"
  File "..\WatchDog\bin\${BUILD_TYPE}\MediaFoundation.dll"
  ; WebEPG
  File "..\WebEPG\WebEPG\bin\${BUILD_TYPE}\WebEPG.dll"
  File /oname=WebEPG.exe "..\WebEPG\WebEPG-xmltv\bin\${BUILD_TYPE}\WebEPG-xmltv.exe"
  File "..\WebEPG\WebEPG-conf\bin\${BUILD_TYPE}\WebEPG-conf.exe"

  ; ========================================
  ; Plugins
  File "..\RemotePlugins\bin\${BUILD_TYPE}\RemotePlugins.*"
  File "..\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\${BUILD_TYPE}\HCWHelper.*"
  File "..\RemotePlugins\Remotes\X10Remote\Interop.X10.dll"

  SetOutPath "$MPdir.Plugins\ExternalPlayers"
  File "..\ExternalPlayers\bin\${BUILD_TYPE}\ExternalPlayers.*"
  SetOutPath "$MPdir.Plugins\process"
  File "..\ProcessPlugins\bin\${BUILD_TYPE}\ProcessPlugins.*"
  File "..\CybrDisplayPlugin\bin\${BUILD_TYPE}\CybrDisplayPlugin.*"
  SetOutPath "$MPdir.Plugins\subtitle"
  File "..\SubtitlePlugins\bin\${BUILD_TYPE}\SubtitlePlugins.*"
  SetOutPath "$MPdir.Plugins\Windows"
  File "..\Dialogs\bin\${BUILD_TYPE}\Dialogs.*"
  File "..\WindowPlugins\bin\${BUILD_TYPE}\WindowPlugins.*"

  ; MyBurner plugin dependencies
  SetOutPath "$MPdir.Base"
  File "..\XPImapiBurner\bin\${BUILD_TYPE}\XPBurnComponent.dll"

  ; ========================================
  ; Wizards
  SetOutPath "$MPdir.Base\Wizards"
  File "..\Configuration\Wizards\*.*"

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  SetOutPath "$MPdir.Base"
  ;filter used for SVCD and VCD playback
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\cdxareader.ax"       "$MPdir.Base\cdxareader.ax" "$MPdir.Base"
  ##### MAYBE used by VideoEditor
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\CLDump.ax"           "$MPdir.Base\CLDump.ax" "$MPdir.Base"
  ; used for scanning in tve2
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MPSA.ax"             "$MPdir.Base\MPSA.ax" "$MPdir.Base"
  ;filter for analog tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\PDMpgMux.ax"         "$MPdir.Base\PDMpgMux.ax" "$MPdir.Base"
  ; used for shoutcast
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\shoutcastsource.ax"  "$MPdir.Base\shoutcastsource.ax" "$MPdir.Base"
  ; used for channels with two mono languages in one stereo streams
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MPAudioSwitcher.ax"  "$MPdir.Base\MPAudioSwitcher.ax" "$MPdir.Base"
  ; used for digital tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TsReader.ax"         "$MPdir.Base\TsReader.ax" "$MPdir.Base"
  WriteRegStr HKCR "Media Type\Extensions\.ts"        "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
  WriteRegStr HKCR "Media Type\Extensions\.tp"        "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
  WriteRegStr HKCR "Media Type\Extensions\.tsbuffer"  "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
  WriteRegStr HKCR "Media Type\Extensions\.rtsp"      "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"

SectionEnd
!macro Remove_${SecCore}
  ${LOG_TEXT} "DEBUG" "MACRO Remove_${SecCore}"
  ${LOG_TEXT} "INFO" "Uninstalling MediaPortal core files..."

  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${KILLPROCESS} "MediaPortal.exe"
  ${KILLPROCESS} "configuration.exe"

  ${KILLPROCESS} "MPInstaller.exe"
  ${KILLPROCESS} "WatchDog.exe"
  ${KILLPROCESS} "MusicShareWatcher.exe"
  ${KILLPROCESS} "TVGuideScheduler.exe"
  ${KILLPROCESS} "WebEPG.exe"
  ${KILLPROCESS} "WebEPG-conf.exe"

  #---------------------------------------------------------------------------
  # FILTER UNREGISTRATION     for TVClient
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  ;filter used for SVCD and VCD playback
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\cdxareader.ax"
  ##### MAYBE used by VideoEditor
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\CLDump.ax"
  ; used for scanning in tve2
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MPSA.ax"
  ;filter for analog tv
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\PDMpgMux.ax"
  ; used for shoutcast
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\shoutcastsource.ax"
  ; used for channels with two mono languages in one stereo streams
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MPAudioSwitcher.ax"
  ; used for digital tv
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\TsReader.ax"

  ; Config Files
  Delete /REBOOTOK "$MPdir.Config\CaptureCardDefinitions.xml"
  Delete /REBOOTOK "$MPdir.Config\eHome Infrared Transceiver List XP.xml"
  Delete /REBOOTOK "$MPdir.Config\HelpReferences.xml"
  Delete /REBOOTOK "$MPdir.Config\ISDNCodes.xml"
  Delete /REBOOTOK "$MPdir.Config\keymap.xml"
  Delete /REBOOTOK "$MPdir.Config\wikipedia.xml"
  Delete /REBOOTOK "$MPdir.Config\yac-area-codes.xml"

  ; Remove the Folders
  RMDir /r /REBOOTOK "$MPdir.Base\MusicPlayer"
  RMDir /r /REBOOTOK "$MPdir.Base\osdskin-media"
  RMDir /r /REBOOTOK "$MPdir.Base\Profiles"
  RMDir /r /REBOOTOK "$MPdir.Base\Tuningparameters"
  RMDir /r /REBOOTOK "$MPdir.Base\WebEPG"
  RMDir /r /REBOOTOK "$MPdir.Base\Wizards"

  RMDir /r /REBOOTOK "$MPdir.BurnerSupport"
  RMDir /r /REBOOTOK "$MPdir.Cache"
  RMDir /r /REBOOTOK "$MPdir.CustomInputDefault"
  RMDir /r /REBOOTOK "$MPdir.Weather"

  ; Language
  Delete /REBOOTOK "$MPdir.Language\*"
  RMDir "$MPdir.Language"

  ; Doc
  Delete /REBOOTOK "$MPdir.Base\Docs\BASS License.txt"
  Delete /REBOOTOK "$MPdir.Base\Docs\MediaPortal License.rtf"
  #Delete /REBOOTOK "$INSTDIR\Docs\LICENSE.rtf"
  #Delete /REBOOTOK "$INSTDIR\Docs\SQLite Database Browser.exe"
  RMDir "$MPdir.Base\Docs"

  ; WebEPG
  RMDir /r /REBOOTOK "$MPdir.Base\WebEPG\channels"
  RMDir /r /REBOOTOK "$MPdir.Base\WebEPG\grabbers"
  RMDir "$MPdir.Base\WebEPG"

  ; MPinstaller
  Delete /REBOOTOK "$MPdir.Base\Installer\cleanup.xml"
  RMDir "$MPdir.Base\Installer"

  ; xmltv
  Delete /REBOOTOK "$MPdir.Base\xmltv\ReadMe.txt"
  Delete /REBOOTOK "$MPdir.Base\xmltv\xmltv.dtd"
  RMDir "$MPdir.Base\xmltv"

  ; plugins
  Delete /REBOOTOK "$MPdir.Plugins\ExternalPlayers\ExternalPlayers.dll"
  RMDir "$MPdir.Plugins\ExternalPlayers"

  RMDir /r /REBOOTOK "$MPdir.Plugins\process\LCDDrivers"
  Delete /REBOOTOK "$MPdir.Plugins\process\ProcessPlugins.dll"
  Delete /REBOOTOK "$MPdir.Plugins\process\CybrDisplayPlugin.dll"
  Delete /REBOOTOK "$MPdir.Plugins\process\PowerSchedulerClientPlugin.dll"
  RMDir "$MPdir.Plugins\process"

  Delete /REBOOTOK "$MPdir.Plugins\subtitle\SubtitlePlugins.dll"
  RMDir "$MPdir.Plugins\subtitle"

  Delete /REBOOTOK "$MPdir.Plugins\Windows\Dialogs.dll"
  Delete /REBOOTOK "$MPdir.Plugins\Windows\WindowPlugins.dll"
  Delete /REBOOTOK "$MPdir.Plugins\Windows\XihSolutions.DotMSN.dll"
  RMDir "$MPdir.Plugins\Windows"

  RMDir "$MPdir.Plugins"

  ; skins
  RMDir /r /REBOOTOK "$MPdir.Skin\BlueTwo"
  RMDir /r /REBOOTOK "$MPdir.Skin\BlueTwo wide"
  RMDir /r /REBOOTOK "$MPdir.Skin\Blue3"
  RMDir "$MPdir.Skin"

  ; Remove Files in MP Root Directory
  Delete /REBOOTOK "$MPdir.Base\AppStart.exe"
  Delete /REBOOTOK "$MPdir.Base\AppStart.exe.config"
  Delete /REBOOTOK "$MPdir.Base\AxInterop.WMPLib.dll"
  Delete /REBOOTOK "$MPdir.Base\BallonRadio.ico"
  Delete /REBOOTOK "$MPdir.Base\bass.dll"
  Delete /REBOOTOK "$MPdir.Base\Bass.Net.dll"
  Delete /REBOOTOK "$MPdir.Base\bass_fx.dll"
  Delete /REBOOTOK "$MPdir.Base\bass_vis.dll"
  Delete /REBOOTOK "$MPdir.Base\bass_vst.dll"
  Delete /REBOOTOK "$MPdir.Base\bass_wadsp.dll"
  Delete /REBOOTOK "$MPdir.Base\bassasio.dll"
  Delete /REBOOTOK "$MPdir.Base\bassmix.dll"
  Delete /REBOOTOK "$MPdir.Base\BassRegistration.dll"
  Delete /REBOOTOK "$MPdir.Base\Configuration.exe"
  Delete /REBOOTOK "$MPdir.Base\Configuration.exe.config"
  Delete /REBOOTOK "$MPdir.Base\Core.dll"
  Delete /REBOOTOK "$MPdir.Base\CSScriptLibrary.dll"
  Delete /REBOOTOK "$MPdir.Base\d3dx9_30.dll"
  Delete /REBOOTOK "$MPdir.Base\DaggerLib.dll"
  Delete /REBOOTOK "$MPdir.Base\DaggerLib.DSGraphEdit.dll"
  Delete /REBOOTOK "$MPdir.Base\Databases.dll"
  Delete /REBOOTOK "$MPdir.Base\defaultMusicViews.xml"
  Delete /REBOOTOK "$MPdir.Base\defaultVideoViews.xml"
  Delete /REBOOTOK "$MPdir.Base\DirectShowLib-2005.dll"
  Delete /REBOOTOK "$MPdir.Base\DirectShowLib.dll"
  Delete /REBOOTOK "$MPdir.Base\dlportio.dll"
  Delete /REBOOTOK "$MPdir.Base\dshowhelper.dll"
  Delete /REBOOTOK "$MPdir.Base\dvblib.dll"
  Delete /REBOOTOK "$MPdir.Base\dxerr9.dll"
  Delete /REBOOTOK "$MPdir.Base\DXUtil.dll"
  Delete /REBOOTOK "$MPdir.Base\edtftpnet-1.2.2.dll"
  Delete /REBOOTOK "$MPdir.Base\FastBitmap.dll"
  Delete /REBOOTOK "$MPdir.Base\fontEngine.dll"
  Delete /REBOOTOK "$MPdir.Base\FTD2XX.DLL"
  Delete /REBOOTOK "$MPdir.Base\hauppauge.dll"
  Delete /REBOOTOK "$MPdir.Base\HcwHelper.exe"
  Delete /REBOOTOK "$MPdir.Base\ICSharpCode.SharpZipLib.dll"
  Delete /REBOOTOK "$MPdir.Base\inpout32.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.GIRDERLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.iTunesLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.TunerLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.WMEncoderLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.WMPLib.dll"
  Delete /REBOOTOK "$MPdir.Base\Interop.X10.dll"
  Delete /REBOOTOK "$MPdir.Base\KCS.Utilities.dll"
  Delete /REBOOTOK "$MPdir.Base\lame_enc.dll"
  Delete /REBOOTOK "$MPdir.Base\LibDriverCoreClient.dll"
  Delete /REBOOTOK "$MPdir.Base\log4net.dll"
  Delete /REBOOTOK "$MPdir.Base\madlldlib.dll"
  Delete /REBOOTOK "$MPdir.Base\MediaFoundation.dll"
  Delete /REBOOTOK "$MPdir.Base\MediaPadLayer.dll"
  Delete /REBOOTOK "$MPdir.Base\MediaPortalDirs.xml"
  Delete /REBOOTOK "$MPdir.Base\MediaPortal.exe"
  Delete /REBOOTOK "$MPdir.Base\MediaPortal.exe.config"
  Delete /REBOOTOK "$MPdir.Base\MediaPortal.Support.dll"
  Delete /REBOOTOK "$MPdir.Base\menu.bin"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.ApplicationBlocks.ApplicationUpdater.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.ApplicationBlocks.ApplicationUpdater.Interfaces.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.ApplicationBlocks.ExceptionManagement.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.ApplicationBlocks.ExceptionManagement.Interfaces.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.Direct3D.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.Direct3DX.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.DirectDraw.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.DirectX.DirectInput.dll"
  Delete /REBOOTOK "$MPdir.Base\Microsoft.Office.Interop.Outlook.dll"
  Delete /REBOOTOK "$MPdir.Base\MPInstaller.exe"
  Delete /REBOOTOK "$MPdir.Base\MPInstaller.Library.dll"
  Delete /REBOOTOK "$MPdir.Base\mplogo.gif"
  Delete /REBOOTOK "$MPdir.Base\WatchDog.exe"
  Delete /REBOOTOK "$MPdir.Base\mpviz.dll"
  Delete /REBOOTOK "$MPdir.Base\MusicShareWatcher.exe"
  Delete /REBOOTOK "$MPdir.Base\MusicShareWatcherHelper.dll"
  Delete /REBOOTOK "$MPdir.Base\RemotePlugins.dll"
  Delete /REBOOTOK "$MPdir.Base\restart.vbs"
  Delete /REBOOTOK "$MPdir.Base\SG_VFD.dll"
  Delete /REBOOTOK "$MPdir.Base\SG_VFDv5.dll"
  Delete /REBOOTOK "$MPdir.Base\sqlite.dll"
  Delete /REBOOTOK "$MPdir.Base\taglib-sharp.dll"
  Delete /REBOOTOK "$MPdir.Base\TaskScheduler.dll"
  Delete /REBOOTOK "$MPdir.Base\ttBdaDrvApi_Dll.dll"
  Delete /REBOOTOK "$MPdir.Base\ttdvbacc.dll"
  Delete /REBOOTOK "$MPdir.Base\TVCapture.dll"
  Delete /REBOOTOK "$MPdir.Base\TVGuideScheduler.exe"
  Delete /REBOOTOK "$MPdir.Base\Utils.dll"
  Delete /REBOOTOK "$MPdir.Base\WebEPG.dll"
  Delete /REBOOTOK "$MPdir.Base\WebEPG.exe"
  Delete /REBOOTOK "$MPdir.Base\WebEPG-conf.exe"
  Delete /REBOOTOK "$MPdir.Base\X10Unified.dll"
  Delete /REBOOTOK "$MPdir.Base\xAPMessage.dll"
  Delete /REBOOTOK "$MPdir.Base\xAPTransport.dll"
  Delete /REBOOTOK "$MPdir.Base\XPBurnComponent.dll"
!macroend

!ifndef HEISE_BUILD
${MementoSection} "Gabest MPA/MPV decoder" SecGabest
  ${LOG_TEXT} "DEBUG" "MementoSection SecGabest"
  ${LOG_TEXT} "INFO" "Installing Gabest MPA/MPV decoder..."

  SetOutPath "$MPdir.Base"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MpaDecFilter.ax"   "$MPdir.Base\MpaDecFilter.ax" "$MPdir.Base"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\Mpeg2DecFilter.ax" "$MPdir.Base\Mpeg2DecFilter.ax" "$MPdir.Base"

  ; Write Default Values for Filter into the registry
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AAC Downmix" 1
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AC3 Dynamic Range" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AC3 LFE" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AC3 Speaker Config" 2
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "AC3Decoder" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "Boost" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "DTS Dynamic Range" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "DTS LFE" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "DTS Speaker Config" 2
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "DTSDecoder" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "Normalize" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Audio Filter" "Output Format" 0

  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Brightness" 128
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Contrast" 100
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Deinterlace" 0
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Enable Planar YUV Modes" 1
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Forced Subtitles" 1
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Hue" 180
  WriteRegStr HKCU "Software\MediaPortal\Mpeg Video Filter" "Saturation" 100

  ; adjust the merit of this directshow filter
  SetOutPath "$MPdir.Base"
  File "Resources\SetMerit.exe"

  ${LOG_TEXT} "INFO" "set merit for MPA"
  nsExec::ExecToLog '"$MPdir.Base\SetMerit.exe" {3D446B6F-71DE-4437-BE15-8CE47174340F} 00600000'
  ${LOG_TEXT} "INFO" "set merit for MPV"
  nsExec::ExecToLog '"$MPdir.Base\SetMerit.exe" {39F498AF-1A09-4275-B193-673B0BA3D478} 00600000'
${MementoSectionEnd}
!macro Remove_${SecGabest}
  ${LOG_TEXT} "DEBUG" "MACRO Remove_${SecGabest}"
  ${LOG_TEXT} "INFO" "Uninstalling Gabest MPA/MPV decoder..."

  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MpaDecFilter.ax"
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\Mpeg2DecFilter.ax"

  ; remove the tool to adjust the merit
  Delete /REBOOTOK "$MPdir.Base\SetMerit.exe"
!macroend
!endif

${MementoSectionDone}

#---------------------------------------------------------------------------
# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
Section -Post
  ${LOG_TEXT} "DEBUG" "SECTION Post"
  ${LOG_TEXT} "INFO" "Doing post installation stuff..."

  ;Removes unselected components
  !insertmacro SectionList "FinishSection"

  ;writes component status to registry
  ${MementoSectionSave}

  SetOverwrite on
  SetOutPath "$MPdir.Base"

  ${If} $noDesktopSC != 1
    CreateShortCut "$DESKTOP\MediaPortal.lnk"               "$MPdir.Base\MediaPortal.exe"      "" "$MPdir.Base\MediaPortal.exe"   0 "" "" "MediaPortal"
    CreateShortCut "$DESKTOP\MediaPortal Configuration.lnk" "$MPdir.Base\Configuration.exe"    "" "$MPdir.Base\Configuration.exe" 0 "" "" "MediaPortal Configuration"
  ${EndIf}

  ${If} $noStartMenuSC != 1
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
      ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
      CreateDirectory "$SMPROGRAMS\$StartMenuGroup"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk"                            "$MPdir.Base\MediaPortal.exe"   ""      "$MPdir.Base\MediaPortal.exe"   0 "" "" "MediaPortal"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk"              "$MPdir.Base\Configuration.exe" ""      "$MPdir.Base\Configuration.exe" 0 "" "" "MediaPortal Configuration"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug-Mode.lnk"                 "$MPdir.Base\WatchDog.exe"   "-auto"    "$MPdir.Base\WatchDog.exe"   0 "" "" "MediaPortal Debug-Mode"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Plugins-Skins Installer.lnk"    "$MPdir.Base\MPInstaller.exe"   ""      "$MPdir.Base\MPInstaller.exe"   0 "" "" "MediaPortal Plugins-Skins Installer"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Logs Collector.lnk"             "$MPdir.Base\WatchDog.exe"   ""         "$MPdir.Base\WatchDog.exe"   0 "" "" "MediaPortal WatchDog"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\uninstall MediaPortal.lnk"                  "$MPdir.Base\uninstall-mp.exe"
      CreateShortCut "$SMPROGRAMS\$StartMenuGroup\User Files.lnk"                             "$MPdir.Config"                 ""      "$MPdir.Config"                 0 "" "" "Browse you config files, databases, thumbs, logs, ..."
      WriteINIStr "$SMPROGRAMS\$StartMenuGroup\Help.url"      "InternetShortcut" "URL" "http://wiki.team-mediaportal.com/"
      WriteINIStr "$SMPROGRAMS\$StartMenuGroup\web site.url"  "InternetShortcut" "URL" "${URL}"
    !insertmacro MUI_STARTMENU_WRITE_END
  ${EndIf}

  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMajor"    "${VER_MAJOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMinor"    "${VER_MINOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionRevision" "${VER_REVISION}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionBuild"    "${VER_BUILD}"

  ; Write Uninstall Information
  WriteRegStr HKLM "${REG_UNINSTALL}" InstallPath        "$MPdir.Base"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName        "$(^Name)"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayVersion     "${VERSION}"
  WriteRegStr HKLM "${REG_UNINSTALL}" Publisher          "${COMPANY}"
  WriteRegStr HKLM "${REG_UNINSTALL}" URLInfoAbout       "${URL}"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon        "$MPdir.Base\MediaPortal.exe,0"
  WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString    "$MPdir.Base\uninstall-mp.exe"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 1
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 1

  WriteUninstaller "$MPdir.Base\uninstall-mp.exe"

  ${registerExtension} "$MPdir.Base\MPInstaller.exe" ".mpi" "MediaPortal extension package"
  ${registerExtension} "$MPdir.Base\MPInstaller.exe" ".xmp" "MediaPortal extension project"

  ${RefreshShellIcons}
SectionEnd

#---------------------------------------------------------------------------
# This section is called on uninstall and removes all components
Section Uninstall
  ${LOG_TEXT} "DEBUG" "SECTION Uninstall"
  ;First removes all optional components
  !insertmacro SectionList "RemoveSection"
  ;now also remove core component
  !insertmacro Remove_${SecCore}

  ; remove registry key
  DeleteRegValue HKLM "${REG_UNINSTALL}" "UninstallString"

  ; remove Start Menu shortcuts
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug-Mode.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Log-Files.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Plugins-Skins Installer.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal TestTool.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Logs Collector.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\uninstall MediaPortal.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\User Files.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\Help.url"
  Delete "$SMPROGRAMS\$StartMenuGroup\web site.url"
  RMDir "$SMPROGRAMS\$StartMenuGroup"

  ; remove Desktop shortcuts
  Delete "$DESKTOP\MediaPortal.lnk"
  Delete "$DESKTOP\MediaPortal Configuration.lnk"

  ; remove last files and instdir
  Delete /REBOOTOK "$MPdir.Base\uninstall-mp.exe"
  RMDir "$MPdir.Base"

  ; do we need to deinstall everything? Then remove also the CommonAppData and InstDir
  ${If} $RemoveAll == 1
    ${LOG_TEXT} "INFO" "Removing User Settings"
    DeleteRegKey HKLM "${REG_UNINSTALL}"
    RMDir /r /REBOOTOK "$MPdir.Config"
    RMDir /r /REBOOTOK "$MPdir.Database"
    RMDir /r /REBOOTOK "$MPdir.Language"
    RMDir /r /REBOOTOK "$MPdir.Plugins"
    RMDir /r /REBOOTOK "$MPdir.Skin"
    RMDir /r /REBOOTOK "$MPdir.Base"

    SetShellVarContext all
    RMDir /r /REBOOTOK "$APPDATA\VirtualStore\ProgramData\Team MediaPortal\MediaPortal"
    RMDir /r /REBOOTOK "$APPDATA\VirtualStore\Program Files\Team MediaPortal\MediaPortal"
  ${EndIf}

  ${unregisterExtension} ".mpi" "MediaPortal extension package"
  ${unregisterExtension} ".xmp" "MediaPortal extension project"

  ${un.RefreshShellIcons}
SectionEnd

#---------------------------------------------------------------------------
# FUNCTIONS
#---------------------------------------------------------------------------
Function .onInit
  ${LOG_OPEN}
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInit"

  #### check and parse cmdline parameter
  ; set default values for parameters ........
!ifndef HEISE_BUILD
  StrCpy $noGabest 0
!endif
  StrCpy $noDesktopSC 0
  StrCpy $noStartMenuSC 0
  StrCpy $DeployMode 0

  ; gets comandline parameter
  ${GetParameters} $R0
  ${LOG_TEXT} "DEBUG" "commandline parameters: $R0"

!ifndef HEISE_BUILD
  ClearErrors
  ${GetOptions} $R0 "/noGabest" $R1
  IfErrors +2
  StrCpy $noGabest 1
!endif

  ClearErrors
  ${GetOptions} $R0 "/noDesktopSC" $R1
  IfErrors +2
  StrCpy $noDesktopSC 1

  ClearErrors
  ${GetOptions} $R0 "/noStartMenuSC" $R1
  IfErrors +2
  StrCpy $noStartMenuSC 1

  ClearErrors
  ${GetOptions} $R0 "/DeployMode" $R1
  IfErrors +2
  StrCpy $DeployMode 1
  #### END of check and parse cmdline parameter

  ; reads components status for registry
  ${MementoSectionRestore}

!ifndef HEISE_BUILD
  ; update the component status -> commandline parameters have higher priority than registry values
  ${If} $noGabest = 1
    !insertmacro UnselectSection ${SecGabest}
  ${EndIf}
!endif

  ; check if old mp 0.2.2 is installed
  ${If} ${MP022IsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MP022)"
    Abort
  ${EndIf}

  ; check if old mp 0.2.3 RC3 is installed
  ${If} ${MP023RC3IsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MP023RC3)"
    Abort
  ${EndIf}

  ; check if old mp 0.2.3 is installed.
  ${If} ${MP023IsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MP023)"
    Abort
  ${EndIf}

  ; show error that the OS is not supported and abort the installation
  ${If} ${AtMostWin2000Srv}
    StrCpy $0 "OSabort"
  ${ElseIf} ${IsWinXP}
    !insertmacro GetServicePack $R1 $R2
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 2
      StrCpy $0 "OSabort"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWinXP64}
    StrCpy $0 "OSabort"

  ${ElseIf} ${IsWin2003}
    StrCpy $0 "OSwarn"

  ${ElseIf} ${IsWinVISTA}
    !insertmacro GetServicePack $R1 $R2
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 1
      StrCpy $0 "OSwarn"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWin2008}
    StrCpy $0 "OSwarn"

  ${Else}
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

  ; check if current user is admin
  UserInfo::GetOriginalAccountType
  Pop $0
  #StrCmp $0 "Admin" 0 +3
  ${IfNot} $0 == "Admin"
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_ADMIN)"
    Abort
  ${EndIf}

  ; check if .Net is installed
  ${IfNot} ${dotNetIsInstalled}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ; check if VC Redist 2005 SP1 is installed
  ${IfNot} ${VCRedistIsInstalled}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_VCREDIST)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ; check if reboot is required
  ${If} ${FileExists} "$MPdir.Base\rebootflag"
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
    Abort
  ${EndIf}

/* OBSOLETE, not sure why i added this in the past
!ifdef SVN_BUILD
  ${IfNot} ${MPIsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_SVN_NOMP)"
    Abort
  ${EndIf}
!endif
*/

/* OBSOLETE - old code to rename existing dirs
  ${If} ${Silent}
    Call InstFilePre
  ${EndIf}
*/

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

Function un.onInit
  ${un.LOG_OPEN}
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onInit"
  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $RemoveAll 0

  ; gets comandline parameter
  ${un.GetParameters} $R0
  ${LOG_TEXT} "DEBUG" "commandline parameters: $R0"

  ; check for special parameter and set the their variables
  ClearErrors
  ${un.GetOptions} $R0 "/RemoveAll" $R1
  IfErrors +2
  StrCpy $RemoveAll 1
  #### END of check and parse cmdline parameter

  ReadRegStr $INSTDIR HKLM "${REG_UNINSTALL}" "InstallPath"
  ${un.ReadMediaPortalDirs} "$INSTDIR"
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

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
    FileOpen $0 "$MPdir.Base\rebootflag" w
    Delete /REBOOTOK "$MPdir.Base\rebootflag" ; this will not be deleted until the reboot because it is currently opened
    RMDir /REBOOTOK "$MPdir.Base"
    FileClose $0
  ${EndIf}

  ${un.LOG_CLOSE}
FunctionEnd


/* OBSOLETE - old code to rename existing dirs
Function InstFilePre
  ReadRegDWORD $R1 HKLM "${REG_UNINSTALL}" "VersionMajor"
  ReadRegDWORD $R2 HKLM "${REG_UNINSTALL}" "VersionMinor"
  ReadRegDWORD $R3 HKLM "${REG_UNINSTALL}" "VersionRevision"

  ${IfNot} ${MPIsInstalled}
    ${If} $R1 != ${VER_MAJOR}
    ${OrIf} $R2 != ${VER_MINOR}
    ${OrIf} $R3 != ${VER_REVISION}

      !insertmacro GET_BACKUP_POSTFIX $R0

      ${If} ${FileExists} "$MPdir.Base\*.*"
        Rename "$MPdir.Base" "$MPdir.Base_$R0"
      ${EndIf}

      ${If} ${FileExists} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml"
        Rename "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml_$R0"
      ${EndIf}

    ${EndIf}
  ${EndIf}

  ${ReadMediaPortalDirs} "$INSTDIR"
FunctionEnd
*/

Function un.WelcomeLeave
    ; This function is called, before the uninstallation process is startet

    ; It asks the user, if he wants to remove all files and settings
    StrCpy $RemoveAll 0
    MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "$(TEXT_MSGBOX_REMOVE_ALL)" IDNO +3
    MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "$(TEXT_MSGBOX_REMOVE_ALL_STUPID)" IDNO +2
    StrCpy $RemoveAll 1

FunctionEnd

#---------------------------------------------------------------------------
# SECTION DECRIPTIONS     must be at the end
#---------------------------------------------------------------------------
!ifndef HEISE_BUILD
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecGabest}  $(DESC_SecGabest)
!insertmacro MUI_FUNCTION_DESCRIPTION_END
!endif