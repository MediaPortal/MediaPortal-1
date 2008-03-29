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
# For the MediaPortal Installer to work you need:
# 1. Lastest NSIS version from http://nsis.sourceforge.net/Download
#
# Editing is much more easier, if you install HM NSIS Edit from http://hmne.sourceforge.net
#
#**********************************************************************************************************#
Name "MediaPortal"
#SetCompressor lzma
SetCompressor /SOLID lzma  ; disabled solid, because of performance reasons

!ifdef HIGH_BUILD
  !define MEDIAPORTAL.BASE "E:\compile\compare_mp1_test"
  !define MEDIAPORTAL.FILTERBIN "..\xbmc\bin\Release"
  !define MEDIAPORTAL.XBMCBIN "..\xbmc\bin\Release"
!else
  !define MEDIAPORTAL.BASE "..\MediaPortal.Base"
  !define MEDIAPORTAL.FILTERBIN "..\xbmc\bin\Release"
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
!insertmacro GetTime
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
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER       "Team MediaPortal\MediaPortal"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT       HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY        "${REG_UNINSTALL}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN      "$INSTDIR\Configuration.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run MediaPortal Configuration"

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
#!define MUI_PAGE_CUSTOMFUNCTION_LEAVE WelcomeLeave
!insertmacro MUI_PAGE_WELCOME
!ifdef HIGH_BUILD
!else
Page custom PageReinstall PageLeaveReinstall
!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
!insertmacro MUI_PAGE_LICENSE "..\Docs\BASS License.txt"
!endif
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
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
InstallDirRegKey HKLM "${REG_UNINSTALL}" InstallPath
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

  ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  ; $0="01"      day
  ; $1="04"      month
  ; $2="2005"    year
  ; $3="Friday"  day of week name
  ; $4="16"      hour
  ; $5="05"      minute
  ; $6="50"      seconds

  DetailPrint "Creating backup of installation dir, this might take some minutes."
  CreateDirectory "$INSTDIR_BACKUP_$1$0-$4$5"
  CopyFiles /SILENT "$INSTDIR\*.*" "$INSTDIR_BACKUP_$1$0-$4$5"

  DetailPrint "Creating backup of configuration dir, this might take some minutes."
  CreateDirectory "$INSTDIR_BACKUP_$1$0-$4$5"
  CopyFiles /SILENT "$INSTDIR\*.*" "$INSTDIR_BACKUP_$1$0-$4$5"

SectionEnd
!else                 # Required invisible Section which renames the INSTDIR to get a real clean installation
Section "-backup" SecBackup

  ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  ; $0="01"      day
  ; $1="04"      month
  ; $2="2005"    year
  ; $3="Friday"  day of week name
  ; $4="16"      hour
  ; $5="05"      minute
  ; $6="50"      seconds

  ; CHECK FOR OLD FILES and DIRECTORY
  ${If} ${FileExists} "$INSTDIR\*.*"

    #  MAYBE WE should always rename the instdir if it exists
    ReadRegDWORD $R0 HKLM "${REG_UNINSTALL}" "VersionMajor"
    ReadRegDWORD $R1 HKLM "${REG_UNINSTALL}" "VersionMinor"

    ${If} $R0 != ${VER_MAJOR}
    ${OrIf} $R1 != ${VER_MINOR}
      Rename "$INSTDIR" "$INSTDIR_BACKUP_$1$0-$4$5"
    ${EndIf}

  ${EndIf}

  ${If} ${FileExists} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml"
    Rename "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml_BACKUP_$1$0-$4$5"
  ${EndIf}

SectionEnd
!endif

Section "MediaPortal core files (required)" SecCore
  SectionIn RO
  DetailPrint "Installing MediaPortal core files..."

  SetOverwrite on

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
    /x CaptureCardDefinitions.xml \
    /x 'eHome Infrared Transceiver List XP.xml' \
    /x FileDetailContents.xml \
    /x ISDNCodes.xml \
    /x keymap.xml \
    /x MusicVideoSettings.xml \
    /x wikipedia.xml \
    /x yac-area-codes.xml \
    /x thumbs \
    "
    #/x grabber_AllGame_com.xml \
    #/x ProgramSettingProfiles.xml \

  SetOutPath $INSTDIR
  File /nonfatal /r /x svn ${EXCLUDED_FILTERS} ${EXCLUDED_CONFIG_FILES}  "${MEDIAPORTAL.BASE}\*"



  ; ========================================
  ; MediaPortalEXE
  File "..\Configuration\bin\${BUILD_TYPE}\MediaPortal.*"
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

  SetOutPath "$INSTDIR\plugins\ExternalPlayers"
  File "..\ExternalPlayers\bin\${BUILD_TYPE}\ExternalPlayers.*"
  SetOutPath "$INSTDIR\plugins\process"
  File "..\ProcessPlugins\bin\${BUILD_TYPE}\ProcessPlugins.*"
  SetOutPath "$INSTDIR\plugins\subtitle"
  File "..\SubtitlePlugins\bin\${BUILD_TYPE}\SubtitlePlugins.*"
  SetOutPath "$INSTDIR\plugins\Windows"
  File "..\Dialogs\bin\${BUILD_TYPE}\Dialogs.*"
  File "..\WindowPlugins\bin\${BUILD_TYPE}\WindowPlugins.*"

  ; MyBurner plugin dependencies
#xcopy /y %1\WindowPlugins\GUIBurner\madlldlib.dll .
#xcopy /y %1\XPImapiBurner\bin\%2\XPBurnComponent.dll .
#REM xcopy /y %1\WindowPlugins\GUIBurner\XPBurnComponent.dll .
  SetOutPath "$INSTDIR"
  File "..\WindowPlugins\GUIBurner\madlldlib.dll"
  File "..\XPImapiBurner\bin\${BUILD_TYPE}\XPBurnComponent.dll"
  #File "..\WindowPlugins\GUIBurner\XPBurnComponent.dll"

  ; ========================================
  ; Wizards
  SetOutPath "$INSTDIR\Wizards"
  File "..\Configuration\Wizards\*.*"

  /*
REM TTPremiumBoot
xcopy /y %1\TTPremiumBoot\*.* TTPremiumBoot\
xcopy /y %1\TTPremiumBoot\21\*.* TTPremiumBoot\21\
xcopy /y %1\TTPremiumBoot\24\*.* TTPremiumBoot\24\
xcopy /y %1\TTPremiumBoot\24Data\*.* \
REM TTPremiumBoot
xcopy /y %1\scripts\*.* scripts\
xcopy /y %1\scripts\imdb\*.* scripts\imdb\
*/
/*
  ; TTPremiumBoot
  SetOutPath "$INSTDIR\TTPremiumBoot"
  File "..\TTPremiumBoot\*.*"
  SetOutPath "$INSTDIR\TTPremiumBoot\21"
  File "..\TTPremiumBoot\21\*.*"
  SetOutPath "$INSTDIR\TTPremiumBoot\24"
  File "..\TTPremiumBoot\24\*.*"
  SetOutPath "$INSTDIR\TTPremiumBoot\24Data"
  File "..\TTPremiumBoot\24Data\*.*"
  ; scripts
  SetOutPath "$INSTDIR\scripts"
  File "..\scripts\*.*"
  SetOutPath "$INSTDIR\scripts\imdb"
  File "..\scripts\imdb\*.*"
*/
  /*
  ; Doc
  SetOutPath $INSTDIR\Docs
  File "..\Docs\BASS License.txt"
  File "..\Docs\LICENSE.rtf"
  File "..\Docs\MediaPortal License.rtf"
  File "..\Docs\SQLite Database Browser.exe"
*/

  # COMMON CONFIG files for SVN and FINAL RELEASES
  SetOutPath "${COMMON_APPDATA}"
  CreateDirectory "${COMMON_APPDATA}\InputDeviceMappings\custom"
  ; Config Files (XML)
  File "${MEDIAPORTAL.BASE}\CaptureCardDefinitions.xml"
  File "${MEDIAPORTAL.BASE}\eHome Infrared Transceiver List XP.xml"
  File "${MEDIAPORTAL.BASE}\FileDetailContents.xml"
  File "${MEDIAPORTAL.BASE}\ISDNCodes.xml"
  File "${MEDIAPORTAL.BASE}\keymap.xml"
  File "${MEDIAPORTAL.BASE}\MusicVideoSettings.xml"
  File "${MEDIAPORTAL.BASE}\wikipedia.xml"
  File "${MEDIAPORTAL.BASE}\yac-area-codes.xml"
  #File "${MEDIAPORTAL.BASE}\grabber_AllGame_com.xml"
  #File "${MEDIAPORTAL.BASE}\ProgramSettingProfiles.xml"
  ; Folders
  File /r "${MEDIAPORTAL.BASE}\thumbs"

  File MediaPortalDirs.xml

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  SetOutPath $INSTDIR
  ;filter used for SVCD and VCD playback
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\cdxareader.ax"       $INSTDIR\cdxareader.ax $INSTDIR
  ##### MAYBE used by VideoEditor
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\CLDump.ax"           $INSTDIR\CLDump.ax $INSTDIR
  ; used for scanning in tve2
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MPSA.ax"             $INSTDIR\MPSA.ax $INSTDIR
  ;filter for analog tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\PDMpgMux.ax"         $INSTDIR\PDMpgMux.ax $INSTDIR
  ; used for shoutcast
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\shoutcastsource.ax"  $INSTDIR\shoutcastsource.ax $INSTDIR
  ; used for digital tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TsReader.ax"         $INSTDIR\TsReader.ax $INSTDIR
  ##### not sure for what this is used
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TTPremiumSource.ax"  $INSTDIR\TTPremiumSource.ax $INSTDIR
SectionEnd
!macro Remove_${SecCore}
    DetailPrint "Uninstalling MediaPortal core files..."

    #---------------------------------------------------------------------------
    # FILTER UNREGISTRATION     for TVClient
    #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
    #---------------------------------------------------------------------------
    ;filter used for SVCD and VCD playback
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\cdxareader.ax
    ##### MAYBE used by VideoEditor
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\CLDump.ax
    ; used for scanning in tve2
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\MPSA.ax
    ;filter for analog tv
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\PDMpgMux.ax
    ; used for shoutcast
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\shoutcastsource.ax
    ; used for digital tv
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\TsReader.ax
    ##### not sure for what this is used
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\TTPremiumSource.ax

    ; Config Files
    Delete /REBOOTOK "${COMMON_APPDATA}\CaptureCardDefinitions.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\eHome Infrared Transceiver List XP.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\FileDetailContents.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\ISDNCodes.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\keymap.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\MusicVideoSettings.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\wikipedia.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\yac-area-codes.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\grabber_AllGame_com.xml"
    Delete /REBOOTOK "${COMMON_APPDATA}\ProgramSettingProfiles.xml"

    ; Remove the Folders
    RmDir /r /REBOOTOK $INSTDIR\Burner
    RmDir /r /REBOOTOK "${COMMON_APPDATA}\Burner"
    RmDir /r /REBOOTOK $INSTDIR\Cache
    RmDir /r /REBOOTOK "${COMMON_APPDATA}\Cache"
    RmDir /r /REBOOTOK $INSTDIR\language
    RmDir /r /REBOOTOK $INSTDIR\MusicPlayer
    RmDir /r /REBOOTOK $INSTDIR\osdskin-media
    RmDir /r /REBOOTOK $INSTDIR\plugins
    RmDir /r /REBOOTOK $INSTDIR\Profiles
    RmDir /r /REBOOTOK $INSTDIR\scripts
    RmDir /r /REBOOTOK $INSTDIR\TTPremiumBoot
    RmDir /r /REBOOTOK $INSTDIR\Tuningparameters
    RmDir /r /REBOOTOK $INSTDIR\weather
    RmDir /r /REBOOTOK $INSTDIR\WebEPG
    RmDir /r /REBOOTOK $INSTDIR\Wizards

    ; database
    RmDir /r /REBOOTOK $INSTDIR\database\convert
    RmDir $INSTDIR\database

    ; Doc
    Delete /REBOOTOK "$INSTDIR\Docs\BASS License.txt"
    Delete /REBOOTOK "$INSTDIR\Docs\LICENSE.rtf"
    Delete /REBOOTOK "$INSTDIR\Docs\MediaPortal License.rtf"
    Delete /REBOOTOK "$INSTDIR\Docs\SQLite Database Browser.exe"
    RmDir $INSTDIR\Docs

    ; InputDeviceMappings
    RmDir /r /REBOOTOK $INSTDIR\InputDeviceMappings\defaults
    RmDir $INSTDIR\InputDeviceMappings

    ; skins
    RmDir /r /REBOOTOK "$INSTDIR\skin\BlueTwo"
    RmDir /r /REBOOTOK "$INSTDIR\skin\BlueTwo wide"
    RmDir $INSTDIR\skin

    ; WebEPG
    RmDir /r /REBOOTOK "$INSTDIR\WebEPG\channels"
    RmDir /r /REBOOTOK "$INSTDIR\WebEPG\grabbers"
    RmDir $INSTDIR\WebEPG

    ; xmltv
    Delete /REBOOTOK $INSTDIR\xmltv\ReadMe.txt
    Delete /REBOOTOK $INSTDIR\xmltv\xmltv.dtd
    RmDir $INSTDIR\xmltv

    ; Remove Files in MP Root Directory
    Delete /REBOOTOK $INSTDIR\AppStart.exe
    Delete /REBOOTOK $INSTDIR\AppStart.exe.config
    Delete /REBOOTOK $INSTDIR\AxInterop.WMPLib.dll
    Delete /REBOOTOK $INSTDIR\BallonRadio.ico
    Delete /REBOOTOK $INSTDIR\bass.dll
    Delete /REBOOTOK $INSTDIR\Bass.Net.dll
    Delete /REBOOTOK $INSTDIR\bass_fx.dll
    Delete /REBOOTOK $INSTDIR\bass_vis.dll
    Delete /REBOOTOK $INSTDIR\bass_vst.dll
    Delete /REBOOTOK $INSTDIR\bass_wadsp.dll
    Delete /REBOOTOK $INSTDIR\bassasio.dll
    Delete /REBOOTOK $INSTDIR\bassmix.dll
    Delete /REBOOTOK $INSTDIR\BassRegistration.dll
    Delete /REBOOTOK $INSTDIR\Configuration.exe
    Delete /REBOOTOK $INSTDIR\Configuration.exe.config
    Delete /REBOOTOK $INSTDIR\Core.dll
    Delete /REBOOTOK $INSTDIR\CSScriptLibrary.dll
    Delete /REBOOTOK $INSTDIR\d3dx9_30.dll
    Delete /REBOOTOK $INSTDIR\DaggerLib.dll
    Delete /REBOOTOK $INSTDIR\DaggerLib.DSGraphEdit.dll
    Delete /REBOOTOK $INSTDIR\Databases.dll
    Delete /REBOOTOK $INSTDIR\defaultMusicViews.xml
    Delete /REBOOTOK $INSTDIR\defaultProgramViews.xml
    Delete /REBOOTOK $INSTDIR\defaultVideoViews.xml
    Delete /REBOOTOK $INSTDIR\DirectShowLib-2005.dll
    Delete /REBOOTOK $INSTDIR\DirectShowLib.dll
    Delete /REBOOTOK $INSTDIR\dlportio.dll
    Delete /REBOOTOK $INSTDIR\dshowhelper.dll
    Delete /REBOOTOK $INSTDIR\dvblib.dll
    Delete /REBOOTOK $INSTDIR\dxerr9.dll
    Delete /REBOOTOK $INSTDIR\DXUtil.dll
    Delete /REBOOTOK $INSTDIR\edtftpnet-1.2.2.dll
    Delete /REBOOTOK $INSTDIR\FastBitmap.dll
    Delete /REBOOTOK $INSTDIR\fontEngine.dll
    Delete /REBOOTOK $INSTDIR\FTD2XX.DLL
    Delete /REBOOTOK $INSTDIR\hauppauge.dll
    Delete /REBOOTOK $INSTDIR\HcwHelper.exe
    Delete /REBOOTOK $INSTDIR\ICSharpCode.SharpZipLib.dll
    Delete /REBOOTOK $INSTDIR\inpout32.dll
    Delete /REBOOTOK $INSTDIR\Interop.GIRDERLib.dll
    Delete /REBOOTOK $INSTDIR\Interop.iTunesLib.dll
    Delete /REBOOTOK $INSTDIR\Interop.TunerLib.dll
    Delete /REBOOTOK $INSTDIR\Interop.WMEncoderLib.dll
    Delete /REBOOTOK $INSTDIR\Interop.WMPLib.dll
    Delete /REBOOTOK $INSTDIR\Interop.X10.dll
    Delete /REBOOTOK $INSTDIR\KCS.Utilities.dll
    Delete /REBOOTOK $INSTDIR\lame_enc.dll
    Delete /REBOOTOK $INSTDIR\LibDriverCoreClient.dll
    Delete /REBOOTOK $INSTDIR\log4net.dll
    Delete /REBOOTOK $INSTDIR\madlldlib.dll
    Delete /REBOOTOK $INSTDIR\MediaFoundation.dll
    Delete /REBOOTOK $INSTDIR\MediaPadLayer.dll
    Delete /REBOOTOK $INSTDIR\MediaPortalDirs.xml
    Delete /REBOOTOK $INSTDIR\MediaPortal.exe
    Delete /REBOOTOK $INSTDIR\MediaPortal.exe.config
    Delete /REBOOTOK $INSTDIR\MediaPortal.Support.dll
    Delete /REBOOTOK $INSTDIR\menu.bin
    Delete /REBOOTOK $INSTDIR\Microsoft.ApplicationBlocks.ApplicationUpdater.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.ApplicationBlocks.ApplicationUpdater.Interfaces.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.ApplicationBlocks.ExceptionManagement.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.ApplicationBlocks.ExceptionManagement.Interfaces.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.DirectX.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.DirectX.Direct3D.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.DirectX.Direct3DX.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.DirectX.DirectDraw.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.DirectX.DirectInput.dll
    Delete /REBOOTOK $INSTDIR\Microsoft.Office.Interop.Outlook.dll
    Delete /REBOOTOK $INSTDIR\MPInstaller.exe
    Delete /REBOOTOK $INSTDIR\MPInstaller.Library.dll
    Delete /REBOOTOK $INSTDIR\mplogo.gif
    Delete /REBOOTOK $INSTDIR\MPTestTool2.exe
    Delete /REBOOTOK $INSTDIR\mpviz.dll
    Delete /REBOOTOK $INSTDIR\MusicShareWatcher.exe
    Delete /REBOOTOK $INSTDIR\MusicShareWatcherHelper.dll
    Delete /REBOOTOK $INSTDIR\RemotePlugins.dll
    Delete /REBOOTOK $INSTDIR\restart.vbs
    Delete /REBOOTOK $INSTDIR\SG_VFD.dll
    Delete /REBOOTOK $INSTDIR\SG_VFDv5.dll
    Delete /REBOOTOK $INSTDIR\sqlite.dll
    Delete /REBOOTOK $INSTDIR\taglib-sharp.dll
    Delete /REBOOTOK $INSTDIR\TaskScheduler.dll
    Delete /REBOOTOK $INSTDIR\ttBdaDrvApi_Dll.dll
    Delete /REBOOTOK $INSTDIR\ttdvbacc.dll
    Delete /REBOOTOK $INSTDIR\TVCapture.dll
    Delete /REBOOTOK $INSTDIR\TVGuideScheduler.exe
    Delete /REBOOTOK $INSTDIR\Utils.dll
    Delete /REBOOTOK $INSTDIR\WebEPG.dll
    Delete /REBOOTOK $INSTDIR\WebEPG.exe
    Delete /REBOOTOK $INSTDIR\WebEPG-conf.exe
    Delete /REBOOTOK $INSTDIR\X10Unified.dll
    Delete /REBOOTOK $INSTDIR\xAPMessage.dll
    Delete /REBOOTOK $INSTDIR\xAPTransport.dll
    Delete /REBOOTOK $INSTDIR\XPBurnComponent.dll
!macroend

${MementoSection} "DScaler Decoder" SecDscaler
    DetailPrint "Installing DScaler Decoder..."

    SetOutPath $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\GenDMOProp.dll"  $INSTDIR\GenDMOProp.dll $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MpegAudio.dll"   $INSTDIR\MpegAudio.dll $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MpegVideo.dll"   $INSTDIR\MpegVideo.dll $INSTDIR

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

    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\GenDMOProp.dll
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\MpegAudio.dll
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\MpegVideo.dll
!macroend

${MementoSection} "Gabest MPA/MPV decoder" SecGabest
    DetailPrint "Installing Gabest MPA/MPV decoder..."

    SetOutPath $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\MpaDecFilter.ax"   $INSTDIR\MpaDecFilter.ax $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\Mpeg2DecFilter.ax" $INSTDIR\Mpeg2DecFilter.ax $INSTDIR

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

    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\MpaDecFilter.ax
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\Mpeg2DecFilter.ax
!macroend

${MementoSectionDone}

#---------------------------------------------------------------------------
# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
Section -Post
    DetailPrint "Doing post installation stuff..."

    ;Removes unselected components
    !insertmacro SectionList "FinishSection"
    ;writes component status to registry
    ${MementoSectionSave}

    SetOverwrite on
    SetOutPath $INSTDIR

    ${If} $noDesktopSC != 1
        CreateShortcut "$DESKTOP\MediaPortal.lnk"               "$INSTDIR\MediaPortal.exe"      "" "$INSTDIR\MediaPortal.exe"   0 "" "" "MediaPortal"
        CreateShortcut "$DESKTOP\MediaPortal Configuration.lnk" "$INSTDIR\Configuration.exe"    "" "$INSTDIR\Configuration.exe" 0 "" "" "MediaPortal Configuration"
    ${EndIf}

    ${If} $noStartMenuSC != 1
      !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
        ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
        CreateDirectory "$SMPROGRAMS\$StartMenuGroup"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk"                            "$INSTDIR\MediaPortal.exe"      ""      "$INSTDIR\MediaPortal.exe"   0 "" "" "MediaPortal"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk"              "$INSTDIR\Configuration.exe"    ""      "$INSTDIR\Configuration.exe" 0 "" "" "MediaPortal Configuration"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug-Mode.lnk"                 "$INSTDIR\MPTestTool2.exe"      "-auto" "$INSTDIR\MPTestTool2.exe"   0 "" "" "MediaPortal Debug-Mode"
        CreateDirectory "${COMMON_APPDATA}\log"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Log-Files.lnk"                  "${COMMON_APPDATA}\log"         ""      "${COMMON_APPDATA}\log"      0 "" "" "MediaPortal Log-Files"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Plugins-Skins Installer.lnk"    "$INSTDIR\MPInstaller.exe"      ""      "$INSTDIR\MPInstaller.exe"   0 "" "" "MediaPortal Plugins-Skins Installer"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TestTool.lnk"                   "$INSTDIR\MPTestTool2.exe"      ""      "$INSTDIR\MPTestTool2.exe"   0 "" "" "MediaPortal TestTool"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\uninstall MediaPortal.lnk"                  "$INSTDIR\uninstall-mp.exe"
        WriteINIStr "$SMPROGRAMS\$StartMenuGroup\web site.url" "InternetShortcut" "URL" "${URL}"
      !insertmacro MUI_STARTMENU_WRITE_END
    ${EndIf}

    WriteRegDword HKLM "${REG_UNINSTALL}" "VersionMajor"    "${VER_MAJOR}"
    WriteRegDword HKLM "${REG_UNINSTALL}" "VersionMinor"    "${VER_MINOR}"
    WriteRegDword HKLM "${REG_UNINSTALL}" "VersionRevision" "${VER_REVISION}"
    WriteRegDword HKLM "${REG_UNINSTALL}" "VersionBuild"    "${VER_BUILD}"

    ; Write Uninstall Information
    WriteRegStr HKLM "${REG_UNINSTALL}" InstallPath        $INSTDIR
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName        "$(^Name)"
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayVersion     "${VERSION}"
    WriteRegStr HKLM "${REG_UNINSTALL}" Publisher          "${COMPANY}"
    WriteRegStr HKLM "${REG_UNINSTALL}" URLInfoAbout       "${URL}"
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon        "$INSTDIR\MediaPortal.exe,0"
    WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString    "$INSTDIR\uninstall-mp.exe"
    WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 1
    WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 1
 
    WriteUninstaller "$INSTDIR\uninstall-mp.exe"

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
    WriteRegStr HKCR "MediaPortal.Installer\DefaultIcon" "" "$INSTDIR\MPInstaller.exe,0"
    WriteRegStr HKCR "MediaPortal.Installer\shell\open\command" "" '$INSTDIR\MPInstaller.exe "%1"'

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
    Delete /REBOOTOK "$INSTDIR\uninstall-mp.exe"
    RmDir "$INSTDIR"

    ; do we need to deinstall everything? Then remove also the CommonAppData and InstDir
    ${If} $RemoveAll == 1
        DetailPrint "Removing User Settings"
        DeleteRegKey HKLM "${REG_UNINSTALL}"
        RmDir /r /REBOOTOK "${COMMON_APPDATA}"
        RmDir /r /REBOOTOK "$INSTDIR"
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
    ${If} ${FileExists} "$INSTDIR\rebootflag"
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
        Abort
    ${EndIf}
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
    
    ReadRegStr $INSTDIR HKLM "${REG_UNINSTALL}" "InstallPath"
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

    SetShellVarContext all
FunctionEnd

Function un.onUninstSuccess
    ; write a reboot flag, if reboot is needed, so the installer won't continue until reboot is done
    ${If} ${RebootFlag}
        FileOpen $0 $INSTDIR\rebootflag w
        Delete /REBOOTOK $INSTDIR\rebootflag ; this will not be deleted until the reboot because it is currently opened
        RmDir /REBOOTOK $INSTDIR
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