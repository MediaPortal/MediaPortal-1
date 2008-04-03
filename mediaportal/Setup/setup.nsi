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
#   For building the installer on your own you need:
#       1. Lastest NSIS version from http://nsis.sourceforge.net/Download
#       2. The xml-plugin from http://nsis.sourceforge.net/XML_plug-in
#
#**********************************************************************************************************#
Name "MediaPortal"
#SetCompressor lzma
SetCompressor /SOLID lzma  ; disabled solid, because of performance reasons

!ifdef HIGH_BUILD
  !define MEDIAPORTAL.BASE "E:\compile\compare_mp1_test"
  !define MEDIAPORTAL.FILTERBIN "..\MediaPortal.Base"
  !define MEDIAPORTAL.XBMCBIN "..\xbmc\bin\Release"
!else
  !define MEDIAPORTAL.BASE "..\MediaPortal.Base"
  !define MEDIAPORTAL.FILTERBIN "..\MediaPortal.Base"
  !define MEDIAPORTAL.XBMCBIN "..\xbmc\bin\Release"
!endif
!define BUILD_TYPE "Release"
;!define BUILD_TYPE "Debug"

#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
Var StartMenuGroup  ; Holds the Startmenu\Programs folder
; variables for commandline parameters for Installer
Var noDscaler
Var noGabest
Var noDesktopSC
Var noStartMenuSC
; variables for commandline parameters for UnInstaller
Var RemoveAll       ; Set, when the user decided to uninstall everything

#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"

!define WEB_REQUIREMENTS "http://wiki.team-mediaportal.com/MediaPortalRequirements"


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
    !define VERSION "1.0 RC1 internal"
!else                       # it's an svn reöease
    !define VERSION "pre-release build ${VER_BUILD}"
!endif
BrandingText "MediaPortal ${VERSION} by Team MediaPortal"

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include LogicLib.nsh
!include Library.nsh
!include FileFunc.nsh
!include WinVer.nsh
!include Memento.nsh

!include setup-AddRemovePage.nsh
!include setup-CommonMPMacros.nsh
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
!define MUI_ICON    "images\install.ico"
!define MUI_UNICON  "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!if ${VER_BUILD} == 0       # it's a stable release
    !define MUI_HEADERIMAGE_BITMAP          "images\header.bmp"
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "images\wizard.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "images\wizard.bmp"
!else                       # it's an svn reöease
    !define MUI_HEADERIMAGE_BITMAP          "images\header-svn.bmp"
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "images\wizard-svn.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "images\wizard-svn.bmp"
!endif
!define MUI_HEADERIMAGE_RIGHT

!define MUI_COMPONENTSPAGE_SMALLDESC
!ifndef HIGH_BUILD
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER       "Team MediaPortal\MediaPortal"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT       HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY        "${REG_UNINSTALL}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  StartMenuGroup
!endif
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN      "$MPdir.Base\Configuration.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run MediaPortal Configuration"

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
#!define MUI_PAGE_CUSTOMFUNCTION_LEAVE WelcomeLeave
!insertmacro MUI_PAGE_WELCOME

!ifndef HIGH_BUILD
Page custom PageReinstall PageLeaveReinstall
!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
!insertmacro MUI_PAGE_LICENSE "..\Docs\BASS License.txt"
!endif

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY

!ifndef HIGH_BUILD
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!endif

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
!ifdef HIGH_BUILD
  OutFile "Release\setup-mp-high.exe"
!else
  OutFile "Release\setup-mediaportal.exe"
!endif
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal"
!ifndef HIGH_BUILD
InstallDirRegKey HKLM "${REG_UNINSTALL}" InstallPath
!endif
CRCCheck on
XPStyle on
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
    ; This macro used to perform operation on multiple sections.
    ; List all of your components in following manner here.
    !insertmacro "${MacroName}" "SecDscaler"
    !insertmacro "${MacroName}" "SecGabest"
!macroend

#---------------------------------------------------------------------------
# SECTIONS and REMOVEMACROS
#---------------------------------------------------------------------------
!ifdef HIGH_BUILD     # optional Section which could create a backup
Section "Backup current installation status" SecBackup

  !insertmacro GET_BACKUP_POSTFIX $R0

  DetailPrint "Creating backup of installation dir, this might take some minutes."
  CreateDirectory "$MPdir.Base_$R0"
  CopyFiles /SILENT "$MPdir.Base\*.*" "$MPdir.Base_$R0"

  DetailPrint "Creating backup of configuration dir, this might take some minutes."
  CreateDirectory "$MPdir.Config_$R0"
  CopyFiles /SILENT "$MPdir.Config\*.*" "$MPdir.Config_$R0"

SectionEnd
!else                 # Required invisible Section which renames the INSTDIR to get a real clean installation
Section "-backup" SecBackup

  !insertmacro GET_BACKUP_POSTFIX $R0

  ; CHECK FOR OLD FILES and DIRECTORY
  ${If} ${FileExists} "$MPdir.Base\*.*"

    #  MAYBE WE should always rename the instdir if it exists
    ReadRegDWORD $R1 HKLM "${REG_UNINSTALL}" "VersionMajor"
    ReadRegDWORD $R2 HKLM "${REG_UNINSTALL}" "VersionMinor"

    ${If} $R1 != ${VER_MAJOR}
    ${OrIf} $R2 != ${VER_MINOR}
      Rename "$MPdir.Base" "$MPdir.Base_$R0"
    ${EndIf}

  ${EndIf}

  #${If} ${FileExists} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml"
  #  Rename "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml_$R0"
  #${EndIf}

SectionEnd
!endif

Section "MediaPortal core files (required)" SecCore
  SectionIn RO
  DetailPrint "Installing MediaPortal core files..."

  DetailPrint "Terminating processes ..."
  ExecWait '"taskkill" /F /IM MediaPortal.exe'
  ExecWait '"taskkill" /F /IM configuration.exe'

  ExecWait '"taskkill" /F /IM MPInstaller.exe'
  ExecWait '"taskkill" /F /IM MPTestTool2.exe'
  ExecWait '"taskkill" /F /IM MusicShareWatcher.exe'
  ExecWait '"taskkill" /F /IM TVGuideScheduler.exe'
  ExecWait '"taskkill" /F /IM WebEPG.exe'
  ExecWait '"taskkill" /F /IM WebEPG-conf.exe'

  SetOverwrite on

  !define EXCLUDED_FOLDERS "\
    /x '${MEDIAPORTAL.BASE}\database\' \
    /x '${MEDIAPORTAL.BASE}\InputDeviceMappings' \
    /x '${MEDIAPORTAL.BASE}\language' \
    /x '${MEDIAPORTAL.BASE}\plugins' \
    /x '${MEDIAPORTAL.BASE}\skin' \
    /x '${MEDIAPORTAL.BASE}\thumbs' \
    /x '${MEDIAPORTAL.BASE}\weather' \
    "

  #filters are installed seperatly and are always include in SVN and FINAL releases
  !define EXCLUDED_FILTERS "\
    /x cdxareader.ax \
    /x CLDump.ax \
    /x MPSA.ax \
    /x PDMpgMux.ax \
    /x shoutcastsource.ax \
    /x TsReader.ax \
    /x TTPremiumSource.ax \
    /x GenDMOProp.dll \
    /x MpegAudio.dll \
    /x MpegVideo.dll \
    /x MpaDecFilter.ax \
    /x Mpeg2DecFilter.ax \
    "

  #CONFIG FILES ARE ALWAYS INSTALLED by SVN and FINAL releases, BECAUSE of the config dir location
  !define EXCLUDED_CONFIG_FILES "\
    /x MediaPortalDirs.xml \
    /x CaptureCardDefinitions.xml \
    /x 'eHome Infrared Transceiver List XP.xml' \
    /x ISDNCodes.xml \
    /x keymap.xml \
    /x MusicVideoSettings.xml \
    /x wikipedia.xml \
    /x yac-area-codes.xml \
    "

  # Files which were diffed before including in installer
  # means all of them are in full installer, but only the changed and new ones are in svn installer 
  #We can not use the complete mediaportal.base dir recoursivly , because the plugins, thumbs, weather need to be extracted to their special MPdir location
  # exluding only the folders does not work because /x plugins won't extract the \plugins AND musicplayer\plugins directory
  SetOutPath "$MPdir.Base"
  File /nonfatal /x .svn ${EXCLUDED_FILTERS} ${EXCLUDED_CONFIG_FILES}  "${MEDIAPORTAL.BASE}\*"
  SetOutPath "$MPdir.Base\MusicPlayer"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\MusicPlayer\*"
  SetOutPath "$MPdir.Base\osdskin-media"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\osdskin-media\*"
  SetOutPath "$MPdir.Base\Profiles"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Profiles\*"
  SetOutPath "$MPdir.Base\scripts"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\scripts\*"
  SetOutPath "$MPdir.Base\Tuningparameters"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Tuningparameters\*"
  SetOutPath "$MPdir.Base\WebEPG"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\WebEPG\*"
  SetOutPath "$MPdir.Base\Wizards"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Wizards\*"
  SetOutPath "$MPdir.Base\xmltv"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\xmltv\*"
  ; Doc
  SetOutPath "$MPdir.Base\Docs"
  File "..\Docs\BASS License.txt"
  File "..\Docs\MediaPortal License.rtf"
  #File "..\Docs\LICENSE.rtf"
  #File "..\Docs\SQLite Database Browser.exe"

  # COMMON CONFIG files for SVN and FINAL RELEASES
  SetOutPath "$MPdir.Config"
  File /nonfatal "${MEDIAPORTAL.BASE}\CaptureCardDefinitions.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\eHome Infrared Transceiver List XP.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\ISDNCodes.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\keymap.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\MusicVideoSettings.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\wikipedia.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\yac-area-codes.xml"

  SetOutPath "$MPdir.Database"  
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\database\*"
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
!ifdef HIGH_BUILD
  SetOverwrite off
  File MediaPortalDirs.xml
  SetOverwrite on
!else
  File MediaPortalDirs.xml
!endif

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
  File "..\core\directshowhelper\directshowhelper\Release\dshowhelper.dll"
  File "..\core\DXUtil\Release\DXUtil.dll"
  File "..\core\fontengine\fontengine\${BUILD_TYPE}\fontengine.*"
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
  ; MPTestTool2
  File "..\MPTestTool2\bin\${BUILD_TYPE}\MPTestTool2.exe"
  File "..\MPTestTool2\bin\${BUILD_TYPE}\DaggerLib.dll"
  File "..\MPTestTool2\bin\${BUILD_TYPE}\DaggerLib.DSGraphEdit.dll"
  File "..\MPTestTool2\bin\${BUILD_TYPE}\DirectShowLib-2005.dll"
  File "..\MPTestTool2\bin\${BUILD_TYPE}\MediaFoundation.dll"
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
  SetOutPath "$MPdir.Plugins\subtitle"
  File "..\SubtitlePlugins\bin\${BUILD_TYPE}\SubtitlePlugins.*"
  SetOutPath "$MPdir.Plugins\Windows"
  File "..\Dialogs\bin\${BUILD_TYPE}\Dialogs.*"
  File "..\WindowPlugins\bin\${BUILD_TYPE}\WindowPlugins.*"

  ; MyBurner plugin dependencies
  #xcopy /y %1\WindowPlugins\GUIBurner\madlldlib.dll .
  #xcopy /y %1\XPImapiBurner\bin\%2\XPBurnComponent.dll .
  #REM xcopy /y %1\WindowPlugins\GUIBurner\XPBurnComponent.dll .
  SetOutPath "$MPdir.Base"
  File "..\WindowPlugins\GUIBurner\madlldlib.dll"
  File "..\XPImapiBurner\bin\${BUILD_TYPE}\XPBurnComponent.dll"
  #File "..\WindowPlugins\GUIBurner\XPBurnComponent.dll"

  ; ========================================
  ; Wizards
  SetOutPath "$MPdir.Base\Wizards"
  File "..\Configuration\Wizards\*.*"

  /*
REM TTPremiumBoot
xcopy /y %1\TTPremiumBoot\*.* TTPremiumBoot\
xcopy /y %1\TTPremiumBoot\21\*.* TTPremiumBoot\21\
xcopy /y %1\TTPremiumBoot\24\*.* TTPremiumBoot\24\
xcopy /y %1\TTPremiumBoot\24Data\*.* \
  ; TTPremiumBoot
  SetOutPath "$MPdir.Base\TTPremiumBoot"
  File "..\TTPremiumBoot\*.*"
  SetOutPath "$INSTDIR\TTPremiumBoot\21"
  File "..\TTPremiumBoot\21\*.*"
  SetOutPath "$INSTDIR\TTPremiumBoot\24"
  File "..\TTPremiumBoot\24\*.*"
  SetOutPath "$INSTDIR\TTPremiumBoot\24Data"
  File "..\TTPremiumBoot\24Data\*.*"
*/
/*
REM C#scripts
xcopy /y %1\scripts\*.* scripts\
xcopy /y %1\scripts\imdb\*.* scripts\imdb\
  ; C#scripts
  SetOutPath "$INSTDIR\scripts"
  File "..\scripts\*.*"
  SetOutPath "$INSTDIR\scripts\imdb"
  File "..\scripts\imdb\*.*"

  #SetOutPath "$MPdir.Cache"
  #SetOutPath "$MPdir.BurnerSupport"
      $\r$\nConfig:  $MPdir.Config \
      $\r$\nPlugins: $MPdir.Plugins \
      $\r$\nLog: $MPdir.Log \
      $\r$\nCustomInputDevice: $MPdir.CustomInputDevice \
      $\r$\nDatabase: $MPdir.Database \
*/


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
  ; used for digital tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TsReader.ax"         "$MPdir.Base\TsReader.ax" "$MPdir.Base"
  ##### not sure for what this is used
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TTPremiumSource.ax"  "$MPdir.Base\TTPremiumSource.ax" "$MPdir.Base"
SectionEnd
!macro Remove_${SecCore}
  DetailPrint "Uninstalling MediaPortal core files..."

  DetailPrint "Terminating processes ..."
  ExecWait '"taskkill" /F /IM MediaPortal.exe'
  ExecWait '"taskkill" /F /IM configuration.exe'

  ExecWait '"taskkill" /F /IM MPInstaller.exe'
  ExecWait '"taskkill" /F /IM MPTestTool2.exe'
  ExecWait '"taskkill" /F /IM MusicShareWatcher.exe'
  ExecWait '"taskkill" /F /IM TVGuideScheduler.exe'
  ExecWait '"taskkill" /F /IM WebEPG.exe'
  ExecWait '"taskkill" /F /IM WebEPG-conf.exe'

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
    ; used for digital tv
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\TsReader.ax"
    ##### not sure for what this is used
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\TTPremiumSource.ax"

    ; Config Files
    Delete /REBOOTOK "$MPdir.Config\CaptureCardDefinitions.xml"
    Delete /REBOOTOK "$MPdir.Config\eHome Infrared Transceiver List XP.xml"
    Delete /REBOOTOK "$MPdir.Config\ISDNCodes.xml"
    Delete /REBOOTOK "$MPdir.Config\keymap.xml"
    Delete /REBOOTOK "$MPdir.Config\MusicVideoSettings.xml"
    Delete /REBOOTOK "$MPdir.Config\wikipedia.xml"
    Delete /REBOOTOK "$MPdir.Config\yac-area-codes.xml"

    ; Remove the Folders
    RmDir /r /REBOOTOK "$MPdir.Base\MusicPlayer"
    RmDir /r /REBOOTOK "$MPdir.Base\osdskin-media"
    RmDir /r /REBOOTOK "$MPdir.Base\Profiles"
    RmDir /r /REBOOTOK "$MPdir.Base\scripts"
    #RmDir /r /REBOOTOK "$MPdir.Base\TTPremiumBoot"
    RmDir /r /REBOOTOK "$MPdir.Base\Tuningparameters"
    RmDir /r /REBOOTOK "$MPdir.Base\WebEPG"
    RmDir /r /REBOOTOK "$MPdir.Base\Wizards"

    RmDir /r /REBOOTOK "$MPdir.BurnerSupport"
    RmDir /r /REBOOTOK "$MPdir.Cache"
    RmDir /r /REBOOTOK "$MPdir.CustomInputDefault"
    RmDir /r /REBOOTOK "$MPdir.Language"
    RmDir /r /REBOOTOK "$MPdir.Weather"

    ; Doc
    Delete /REBOOTOK "$MPdir.Base\Docs\BASS License.txt"
    Delete /REBOOTOK "$MPdir.Base\Docs\MediaPortal License.rtf"
    #Delete /REBOOTOK "$INSTDIR\Docs\LICENSE.rtf"
    #Delete /REBOOTOK "$INSTDIR\Docs\SQLite Database Browser.exe"
    RmDir "$MPdir.Base\Docs"

    ; WebEPG
    RmDir /r /REBOOTOK "$MPdir.Base\WebEPG\channels"
    RmDir /r /REBOOTOK "$MPdir.Base\WebEPG\grabbers"
    RmDir "$MPdir.Base\WebEPG"

    ; xmltv
    Delete /REBOOTOK "$MPdir.Base\xmltv\ReadMe.txt"
    Delete /REBOOTOK "$MPdir.Base\xmltv\xmltv.dtd"
    RmDir "$MPdir.Base\xmltv"

    ; database
    RmDir /r /REBOOTOK "$MPdir.Database\convert"
    RmDir "$MPdir.Database"

    ; plugins
    Delete /REBOOTOK "$MPdir.Plugins\ExternalPlayers\ExternalPlayers.dll"
    RmDir "$MPdir.Plugins\ExternalPlayers"

    RmDir /r /REBOOTOK "$MPdir.Plugins\process\LCDDrivers"
    Delete /REBOOTOK "$MPdir.Plugins\process\ProcessPlugins.dll"
    Delete /REBOOTOK "$MPdir.Plugins\process\PowerSchedulerClientPlugin.dll"
    RmDir "$MPdir.Plugins\process"

    Delete /REBOOTOK "$MPdir.Plugins\subtitle\SubtitlePlugins.dll"
    RmDir "$MPdir.Plugins\subtitle"

    Delete /REBOOTOK "$MPdir.Plugins\Windows\Dialogs.dll"
    Delete /REBOOTOK "$MPdir.Plugins\Windows\WindowPlugins.dll"
    Delete /REBOOTOK "$MPdir.Plugins\Windows\XihSolutions.DotMSN.dll"
    Delete /REBOOTOK "$MPdir.Plugins\Windows\TvPlugin.dll"
    RmDir "$MPdir.Plugins\Windows"

    RmDir "$MPdir.Plugins"

    ; skins
    RmDir /r /REBOOTOK "$MPdir.Skin\BlueTwo"
    RmDir /r /REBOOTOK "$MPdir.Skin\BlueTwo wide"
    RmDir "$MPdir.Skin"

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
    Delete /REBOOTOK "$MPdir.Base\HelpReferences.xml"
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
    Delete /REBOOTOK "$MPdir.Base\MPTestTool2.exe"
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

${MementoSection} "DScaler Decoder" SecDscaler
    DetailPrint "Installing DScaler Decoder..."

    SetOutPath "$MPdir.Base"
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\GenDMOProp.dll"  "$MPdir.Base\GenDMOProp.dll" "$MPdir.Base"
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MpegAudio.dll"   "$MPdir.Base\MpegAudio.dll" "$MPdir.Base"
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MpegVideo.dll"   "$MPdir.Base\MpegVideo.dll" "$MPdir.Base"

    ; Write Default Values for Filter into the registry
    WriteRegStr HKCU "Software\DScaler5\Mpeg Audio Filter" "Dynamic Range Control" 1
    WriteRegStr HKCU "Software\DScaler5\Mpeg Audio Filter" "MPEG Audio over SPDIF" 0
    WriteRegStr HKCU "Software\DScaler5\Mpeg Audio Filter" "SPDIF Audio Time Offset" 0
    WriteRegStr HKCU "Software\DScaler5\Mpeg Audio Filter" "Speaker Config" 1
    WriteRegStr HKCU "Software\DScaler5\Mpeg Audio Filter" "Use SPDIF for AC3 & DTS" 0

    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "3:2 playback smoothing" 1
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "Colour space to output" 1
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "Deinterlace Mode" 2
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "Display Forced Subtitles" 1
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "Do Analog Blanking" 1
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "DVB Aspect Preferences" 0
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "Hardcode for PAL with ffdshow" 0
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "IDCT to Use" 2
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "Use accurate aspect ratios" 1
    WriteRegStr HKCU "Software\DScaler5\Mpeg Video Filter" "Video Delay" 0
${MementoSectionEnd}
!macro Remove_${SecDscaler}
    DetailPrint "Uninstalling DScaler Decoder..."

    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\GenDMOProp.dll"
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MpegAudio.dll"
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MpegVideo.dll"
!macroend

${MementoSection} "Gabest MPA/MPV decoder" SecGabest
    DetailPrint "Installing Gabest MPA/MPV decoder..."

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
${MementoSectionEnd}
!macro Remove_${SecGabest}
    DetailPrint "Uninstalling Gabest MPA/MPV decoder..."

    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MpaDecFilter.ax"
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\Mpeg2DecFilter.ax"
!macroend

${MementoSectionDone}

#---------------------------------------------------------------------------
# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
Section -Post
    DetailPrint "Doing post installation stuff..."

    ;Removes unselected components
    !insertmacro SectionList "FinishSection"
!ifndef HIGH_BUILD
    ;writes component status to registry
    ${MementoSectionSave}

    SetOverwrite on
    SetOutPath "$MPdir.Base"

    ${If} $noDesktopSC != 1
        CreateShortcut "$DESKTOP\MediaPortal.lnk"               "$MPdir.Base\MediaPortal.exe"      "" "$MPdir.Base\MediaPortal.exe"   0 "" "" "MediaPortal"
        CreateShortcut "$DESKTOP\MediaPortal Configuration.lnk" "$MPdir.Base\Configuration.exe"    "" "$MPdir.Base\Configuration.exe" 0 "" "" "MediaPortal Configuration"
    ${EndIf}

    ${If} $noStartMenuSC != 1
      !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
        ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
        CreateDirectory "$SMPROGRAMS\$StartMenuGroup"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk"                            "$MPdir.Base\MediaPortal.exe"   ""      "$MPdir.Base\MediaPortal.exe"   0 "" "" "MediaPortal"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk"              "$MPdir.Base\Configuration.exe" ""      "$MPdir.Base\Configuration.exe" 0 "" "" "MediaPortal Configuration"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug-Mode.lnk"                 "$MPdir.Base\MPTestTool2.exe"   "-auto" "$MPdir.Base\MPTestTool2.exe"   0 "" "" "MediaPortal Debug-Mode"
        CreateDirectory "$MPdir.Log"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Log-Files.lnk"                  "$MPdir.Log"                    ""      "$MPdir.Log"                    0 "" "" "MediaPortal Log-Files"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Plugins-Skins Installer.lnk"    "$MPdir.Base\MPInstaller.exe"   ""      "$MPdir.Base\MPInstaller.exe"   0 "" "" "MediaPortal Plugins-Skins Installer"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TestTool.lnk"                   "$MPdir.Base\MPTestTool2.exe"   ""      "$MPdir.Base\MPTestTool2.exe"   0 "" "" "MediaPortal TestTool"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\uninstall MediaPortal.lnk"                  "$MPdir.Base\uninstall-mp.exe"
        WriteINIStr "$SMPROGRAMS\$StartMenuGroup\web site.url" "InternetShortcut" "URL" "${URL}"
      !insertmacro MUI_STARTMENU_WRITE_END
    ${EndIf}

    WriteRegDword HKLM "${REG_UNINSTALL}" "VersionMajor"    "${VER_MAJOR}"
    WriteRegDword HKLM "${REG_UNINSTALL}" "VersionMinor"    "${VER_MINOR}"
    WriteRegDword HKLM "${REG_UNINSTALL}" "VersionRevision" "${VER_REVISION}"
    WriteRegDword HKLM "${REG_UNINSTALL}" "VersionBuild"    "${VER_BUILD}"

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
!endif

    ; Associate .mpi files with MPInstaller
    !define Index "Line${__LINE__}"
    ; backup the association, if it already exsists
    ReadRegStr $1 HKCR ".mpi" ""
    StrCmp $1 "" "${Index}-NoBackup"
    StrCmp $1 "MediaPortal.Installer" "${Index}-NoBackup"
    WriteRegStr HKCR ".mpi" "backup_val" $1

    "${Index}-NoBackup:"
    WriteRegStr HKCR ".mpi" "" "MediaPortal.Installer"
    WriteRegStr HKCR "MediaPortal.Installer" "" "MediaPortal Installer"
    WriteRegStr HKCR "MediaPortal.Installer\shell" "" "open"
    WriteRegStr HKCR "MediaPortal.Installer\DefaultIcon" "" "$MPdir.Base\MPInstaller.exe,0"
    WriteRegStr HKCR "MediaPortal.Installer\shell\open\command" "" '$MPdir.Base\MPInstaller.exe "%1"'

    ${RefreshShellIcons}
    # [OBSOLETE] System::Call 'Shell32::SHChangeNotify(i 0x8000000, i 0, i 0, i 0)'
    !undef Index
SectionEnd

#---------------------------------------------------------------------------
# This section is called on uninstall and removes all components
Section Uninstall
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
    Delete "$SMPROGRAMS\$StartMenuGroup\uninstall MediaPortal.lnk"
    Delete "$SMPROGRAMS\$StartMenuGroup\web site.url"
    RmDir "$SMPROGRAMS\$StartMenuGroup"

    ; remove Desktop shortcuts
    Delete "$DESKTOP\MediaPortal.lnk"
    Delete "$DESKTOP\MediaPortal Configuration.lnk"

    ; remove last files and instdir
    Delete /REBOOTOK "$MPdir.Base\uninstall-mp.exe"
    RmDir "$MPdir.Base"

    ; do we need to deinstall everything? Then remove also the CommonAppData and InstDir
    ${If} $RemoveAll == 1
        DetailPrint "Removing User Settings"
        DeleteRegKey HKLM "${REG_UNINSTALL}"
        RmDir /r /REBOOTOK "$MPdir.Config"
        RmDir /r /REBOOTOK "$MPdir.Database"
        RmDir /r /REBOOTOK "$MPdir.Plugins"
        RmDir /r /REBOOTOK "$MPdir.Skin"
        RmDir /r /REBOOTOK "$MPdir.Base"
    ${EndIf}

    ; Remove File Association for .mpi files
    !define Index "Line${__LINE__}"
    ReadRegStr $1 HKCR ".mpi" ""
    StrCmp $1 "MediaPortal.Installer" 0 "${Index}-NoOwn" ; only do this if we own it
    ReadRegStr $1 HKCR ".mpi" "backup_val"
    StrCmp $1 "" 0 "${Index}-Restore" ; if backup="" then delete the whole key
    DeleteRegKey HKCR ".mpi"
    Goto "${Index}-NoOwn"

    "${Index}-Restore:"
    WriteRegStr HKCR ".mpi" "" $1
    DeleteRegValue HKCR ".mpi" "backup_val"

    DeleteRegKey HKCR "MediaPortal.Installer" ;Delete key with association settings

    ${un.RefreshShellIcons}

    "${Index}-NoOwn:"
    !undef Index
SectionEnd

#---------------------------------------------------------------------------
# FUNCTIONS
#---------------------------------------------------------------------------
Function .onInit
    #### check and parse cmdline parameter
    ; set default values for parameters ........
    StrCpy $noDscaler 0
    StrCpy $noGabest 0
    StrCpy $noDesktopSC 0
    StrCpy $noStartMenuSC 0

    ; gets comandline parameter
    ${GetParameters} $R0

    ; check for special parameter and set the their variables
    ClearErrors
    ${GetOptions} $R0 "/noDscaler" $R1
    IfErrors +2
    StrCpy $noDscaler 1

    ClearErrors
    ${GetOptions} $R0 "/noGabest" $R1
    IfErrors +2
    StrCpy $noGabest 1

    ClearErrors
    ${GetOptions} $R0 "/noDesktopSC" $R1
    IfErrors +2
    StrCpy $noDesktopSC 1

    ClearErrors
    ${GetOptions} $R0 "/noStartMenuSC" $R1
    IfErrors +2
    StrCpy $noStartMenuSC 1
    #### END of check and parse cmdline parameter

    ; reads components status for registry
    ${MementoSectionRestore}

    ; update the component status -> commandline parameters have higher priority than registry values
    ${If} $noDscaler = 1
        !insertmacro UnselectSection ${SecDscaler}
    ${EndIf}
    ${If} $noGabest = 1
        !insertmacro UnselectSection ${SecGabest}
    ${EndIf}

    ; check if old mp 0.2.2 is installed
    ${If} ${MP022IsInstalled}
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_MP022)" IDOK 0
        Abort
    ${EndIf}

!ifndef HIGH_BUILD
    ; check if old mp 0.2.3 is installed.
    ${If} ${MP023IsInstalled}
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_MP023)" IDOK 0
        Abort
    ${EndIf}
!endif

    ; check if minimum Windows version is XP
    ${If} ${AtMostWin2000}
        MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)" IDNO +2
        ExecShell open "${WEB_REQUIREMENTS}"
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
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
        Abort
    ${EndIf}

    #${ReadMediaPortalDirs}
/*
    ${If} ${Silent}
        RmDir /r "${COMMON_APPDATA}\Cache"

        ; check if MP is already installed
        ReadRegStr $R0 HKLM "${REG_UNINSTALL}" UninstallString
        ${If} ${FileExists} "$R0"
            ; get parent folder of uninstallation EXE (RO) and save it to R1
            ${GetParent} $R0 $R1
            ; start uninstallation of installed MP, from tmp folder, so it will delete itself
            ClearErrors
            CopyFiles $R0 "$TEMP\uninstall-mp.exe"
            ExecWait '"$TEMP\uninstall-mp.exe" /S _?=$R1'

            ; if an error occured, ask to cancel installation
            IfErrors 0 unInstallDone
                MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_ON_UNINSTALL)" /SD IDNO IDYES unInstallDone IDNO 0
                Quit
            unInstallDone:

            ; if reboot flag is set, abort the installation, and continue the installer on next startup
            ${If} ${FileExists} "$INSTDIR\rebootflag"
                MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)" IDOK 0
                WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" "$(^Name)" $EXEPATH
                Quit
            ${EndIf}
        ${EndIf}
    ${EndIf}
*/
    SetShellVarContext all
FunctionEnd

Function .onVerifyInstDir
  #MessageBox MB_OK "onVerifyInstDir"
  ${ReadMediaPortalDirs} "$INSTDIR"
FunctionEnd

Function un.onInit
  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $RemoveAll 0

  ; gets comandline parameter
  ${un.GetParameters} $R0

  ; check for special parameter and set the their variables
  ClearErrors
  ${un.GetOptions} $R0 "/RemoveAll" $R1
  IfErrors +2
  StrCpy $RemoveAll 1
  #### END of check and parse cmdline parameter

  ${un.ReadMediaPortalDirs} "$INSTDIR"

  ReadRegStr $INSTDIR HKLM "${REG_UNINSTALL}" "InstallPath"
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

  SetShellVarContext all
FunctionEnd

Function un.onUninstSuccess
  ; write a reboot flag, if reboot is needed, so the installer won't continue until reboot is done
  ${If} ${RebootFlag}
    FileOpen $0 "$MPdir.Base\rebootflag" w
    Delete /REBOOTOK "$MPdir.Base\rebootflag" ; this will not be deleted until the reboot because it is currently opened
    RmDir /REBOOTOK "$MPdir.Base"
    FileClose $0
  ${EndIf}
FunctionEnd
/*
Function WelcomeLeave
    ; check if MP is already installed
    ReadRegStr $R0 HKLM "${REG_UNINSTALL}" UninstallString
    ${If} ${FileExists} "$R0"
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_IS_INSTALLED)"

        ; get parent folder of uninstallation EXE (RO) and save it to R1
        ${GetParent} $R0 $R1
        ; start uninstallation of installed MP, from tmp folder, so it will delete itself
        HideWindow
        ClearErrors
        CopyFiles $R0 "$TEMP\uninstall-mp.exe"
        ExecWait '"$TEMP\uninstall-mp.exe" _?=$R1'
        BringToFront

        ; if an error occured, ask to cancel installation
        ${If} ${Errors}
            MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_ON_UNINSTALL)" /SD IDNO IDYES unInstallDone IDNO 0
            Quit
        ${EndIf}
    ${EndIf}

    ; if reboot flag is set, abort the installation, and continue the installer on next startup
    ${If} ${FileExists} "$INSTDIR\rebootflag"
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)" IDOK 0
        WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" "$(^Name)" $EXEPATH
        Quit
    ${EndIf}
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
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDscaler} $(DESC_SecDscaler)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecGabest}  $(DESC_SecGabest)
!insertmacro MUI_FUNCTION_DESCRIPTION_END