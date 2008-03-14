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
SetCompressor lzma
#SetCompressor /SOLID lzma  ; disabled solid, because of performance reasons

#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
Var StartMenuGroup  ; Holds the Startmenu\Programs folder
# [OBSOLETE] Var WindowsVersion  ; The Windows Version
Var CommonAppData   ; The Common Application Folder
# [OBSOLETE] Var DSCALER         ; Should we install Dscaler Filter
# [OBSOLETE] Var GABEST          ; Should we install Gabest Filter
# [OBSOLETE] Var FilterDir       ; The Directory, where the filters have been installed  
# [OBSOLETE] Var LibInstall      ; Needed for Library Installation
# [OBSOLETE] Var TmpDir          ; Needed for the Uninstaller
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

!define REG_UNINSTALL "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"

!define VER_MAJOR       0
!define VER_MINOR       9
!define VER_REVISION    2
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif
!if ${VER_BUILD} == 0       # it's a stable release
    !define VERSION "1.0 RC2 internal"
!else                       # it's an svn reöease
    !define VERSION "pre-release build ${VER_BUILD}"
!endif
BrandingText "MediaPortal ${VERSION} by Team MediaPortal"


!define FILTER_REBOOT_FLAG "NOREBOOT_NOTPROTECTED"
#!define FILTER_REBOOT_FLAG "REBOOT_NOTPROTECTED"

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include LogicLib.nsh
!include Library.nsh
!include FileFunc.nsh
!include WinVer.nsh

!include setup-RememberSections.nsh
!include setup-languages.nsh

!include setup-dotnet.nsh

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
!define MUI_HEADERIMAGE_BITMAP          "images\header.bmp"
!define MUI_HEADERIMAGE_RIGHT
!define MUI_WELCOMEFINISHPAGE_BITMAP    "images\wizard.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP  "images\wizard.bmp"

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER         "Team MediaPortal\MediaPortal"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT         HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY          "${REG_UNINSTALL}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME    StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_TEXT     "Run MediaPortal Configuration"
!define MUI_FINISHPAGE_RUN_FUNCTION RunConfig

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE WelcomeLeave
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
!insertmacro MUI_PAGE_LICENSE "..\Docs\BASS License.txt"
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
OutFile "Release\setup-mediaportal.exe"
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
!macro SetCommonAppData
    ; Get the Common Application Data Folder
    ; Set the Context to alll, so that we get the All Users folder
    SetShellVarContext all
    StrCpy $CommonAppData "$APPDATA\Team MediaPortal\MediaPortal"
    ; Context back to current user
    SetShellVarContext current
!macroend

#---------------------------------------------------------------------------
# SECTIONS and REMOVEMACROS
#---------------------------------------------------------------------------
Section "MediaPortal core files (required)" SecCore
    SectionIn RO
    DetailPrint "Installing MediaPortal core files..."

    SetOverwrite on

    ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
    ; $0="01"      day
    ; $1="04"      month
    ; $2="2005"    year
    ; $3="Friday"  day of week name
    ; $4="16"      hour
    ; $5="05"      minute
    ; $6="50"      seconds

    ; CHECK FOR OLD FILES and DIRECTORY
    IfFileExists "$INSTDIR\*.*" 0 noInstDirRename

    /*  MAYBE WE should always rename the instdir if it exists
    ReadRegDWORD $R0 HKLM "${REG_UNINSTALL}" "VersionMajor"
    ReadRegDWORD $R1 HKLM "${REG_UNINSTALL}" "VersionMinor"

    ${If} $R0 < ${VER_MAJOR}
        Goto instDirRename
    ${Else}
        ${If} $R1 < ${VER_MINOR}
            Goto instDirRename
        ${Else}
            Goto noInstDirRename
        ${EndIf}
    ${EndIf}

    instDirRename:
    */
        Rename "$INSTDIR" "$INSTDIR_BACKUP_$1$0-$4$5"

    noInstDirRename:

    IfFileExists "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" 0 +2
        Rename "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml_BACKUP_$1$0-$4$5"


    ; Doc
    SetOutPath $INSTDIR\Docs
    File "..\Docs\BASS License.txt"
    File "..\Docs\LICENSE.rtf"
    File "..\Docs\MediaPortal License.rtf"
    File "..\Docs\SQLite Database Browser.exe"

    SetOutPath $INSTDIR

    ; Folder
    File /r ..\xbmc\bin\Release\database
    File /r ..\xbmc\bin\Release\InputDeviceMappings
    File /r ..\xbmc\bin\Release\language
    File /r ..\xbmc\bin\Release\MusicPlayer
    File /r ..\xbmc\bin\Release\osdskin-media
    File /r ..\xbmc\bin\Release\plugins
    File /r ..\xbmc\bin\Release\scripts
    File /r ..\xbmc\bin\Release\skin
    File /r ..\xbmc\bin\Release\TTPremiumBoot
    File /r ..\xbmc\bin\Release\Tuningparameters
    File /r ..\xbmc\bin\Release\weather
    File /r ..\xbmc\bin\Release\WebEPG
    File /r ..\xbmc\bin\Release\Wizards
    File /r ..\xbmc\bin\Release\xmltv

    ; Attention: Don't forget to add a Remove for every file to the UniNstall Section
    ;------------  Common Files and Folders for XP & Vista
    ; Files
    File ..\xbmc\bin\Release\AppStart.exe
    File ..\xbmc\bin\Release\AppStart.exe.config
    File ..\xbmc\bin\Release\AxInterop.WMPLib.dll
    File ..\xbmc\bin\Release\BallonRadio.ico
    File ..\xbmc\bin\Release\bass.dll
    File ..\xbmc\bin\Release\Bass.Net.dll
    File ..\xbmc\bin\Release\bass_fx.dll
    File ..\xbmc\bin\Release\bass_vis.dll
    File ..\xbmc\bin\Release\bass_vst.dll
    File ..\xbmc\bin\Release\bass_wadsp.dll
    File ..\xbmc\bin\Release\bassasio.dll
    File ..\xbmc\bin\Release\bassmix.dll
    File ..\xbmc\bin\Release\BassRegistration.dll
    File ..\xbmc\bin\Release\Configuration.exe
    File ..\xbmc\bin\Release\Configuration.exe.config
    File ..\xbmc\bin\Release\Core.dll
    File ..\xbmc\bin\Release\CSScriptLibrary.dll
    File ..\xbmc\bin\Release\d3dx9_30.dll
    File ..\xbmc\bin\Release\DaggerLib.dll
    File ..\xbmc\bin\Release\DaggerLib.DSGraphEdit.dll
    File ..\xbmc\bin\Release\Databases.dll
    File ..\xbmc\bin\Release\defaultMusicViews.xml
    File ..\xbmc\bin\Release\defaultProgramViews.xml
    File ..\xbmc\bin\Release\defaultVideoViews.xml
    File ..\xbmc\bin\Release\DirectShowLib-2005.dll
    File ..\xbmc\bin\Release\DirectShowLib.dll
    File ..\xbmc\bin\Release\dlportio.dll
    File ..\xbmc\bin\Release\dshowhelper.dll
    File ..\xbmc\bin\Release\dvblib.dll
    File ..\xbmc\bin\Release\dxerr9.dll
    File ..\xbmc\bin\Release\DXUtil.dll
    File ..\xbmc\bin\Release\edtftpnet-1.2.2.dll
    File ..\xbmc\bin\Release\edtftpnet-1.2.5.dll
    File ..\xbmc\bin\Release\FastBitmap.dll
    File ..\xbmc\bin\Release\fontEngine.dll
    File ..\xbmc\bin\Release\FTD2XX.DLL
    File ..\xbmc\bin\Release\hauppauge.dll
    File ..\xbmc\bin\Release\HcwHelper.exe
    File ..\xbmc\bin\Release\ICSharpCode.SharpZipLib.dll
    File ..\xbmc\bin\Release\inpout32.dll
    File ..\xbmc\bin\Release\Interop.GIRDERLib.dll
    File ..\xbmc\bin\Release\Interop.iTunesLib.dll
    File ..\xbmc\bin\Release\Interop.TunerLib.dll
    File ..\xbmc\bin\Release\Interop.WMEncoderLib.dll
    File ..\xbmc\bin\Release\Interop.WMPLib.dll
    File ..\xbmc\bin\Release\Interop.X10.dll
    File ..\xbmc\bin\Release\KCS.Utilities.dll
    File ..\xbmc\bin\Release\lame_enc.dll
    File ..\xbmc\bin\Release\LibDriverCoreClient.dll
    File ..\xbmc\bin\Release\log4net.dll
    File ..\xbmc\bin\Release\madlldlib.dll
    File ..\xbmc\bin\Release\MediaFoundation.dll
    File ..\xbmc\bin\Release\MediaPadLayer.dll
    File ..\xbmc\bin\Release\MediaPortal.exe
    File ..\xbmc\bin\Release\MediaPortal.exe.config
    File ..\xbmc\bin\Release\MediaPortal.Support.dll
    File ..\xbmc\bin\Release\menu.bin
    File ..\xbmc\bin\Release\Microsoft.ApplicationBlocks.ApplicationUpdater.dll
    File ..\xbmc\bin\Release\Microsoft.ApplicationBlocks.ApplicationUpdater.Interfaces.dll
    File ..\xbmc\bin\Release\Microsoft.ApplicationBlocks.ExceptionManagement.dll
    File ..\xbmc\bin\Release\Microsoft.ApplicationBlocks.ExceptionManagement.Interfaces.dll
    File ..\xbmc\bin\Release\Microsoft.DirectX.dll
    File ..\xbmc\bin\Release\Microsoft.DirectX.Direct3D.dll
    File ..\xbmc\bin\Release\Microsoft.DirectX.Direct3DX.dll
    File ..\xbmc\bin\Release\Microsoft.DirectX.DirectDraw.dll
    File ..\xbmc\bin\Release\Microsoft.DirectX.DirectInput.dll
    File ..\xbmc\bin\Release\Microsoft.Office.Interop.Outlook.dll
    File ..\xbmc\bin\Release\MPInstaller.exe
    File ..\xbmc\bin\Release\MPInstaller.Library.dll
    File ..\xbmc\bin\Release\mplogo.gif
    File ..\xbmc\bin\Release\MPTestTool2.exe
    File ..\xbmc\bin\Release\mpviz.dll
    File ..\xbmc\bin\Release\MusicShareWatcher.exe
    File ..\xbmc\bin\Release\MusicShareWatcherHelper.dll
    File ..\xbmc\bin\Release\RemotePlugins.dll
    File ..\xbmc\bin\Release\restart.vbs
    File ..\xbmc\bin\Release\SG_VFD.dll
    File ..\xbmc\bin\Release\SG_VFDv5.dll
    File ..\xbmc\bin\Release\sqlite.dll
    File ..\xbmc\bin\Release\taglib-sharp.dll
    File ..\xbmc\bin\Release\TaskScheduler.dll
    File ..\xbmc\bin\Release\ttBdaDrvApi_Dll.dll
    File ..\xbmc\bin\Release\ttdvbacc.dll
    File ..\xbmc\bin\Release\TVCapture.dll
    File ..\xbmc\bin\Release\TVGuideScheduler.exe
    File ..\xbmc\bin\Release\Utils.dll
    File ..\xbmc\bin\Release\WebEPG.dll
    File ..\xbmc\bin\Release\WebEPG.exe
    File ..\xbmc\bin\Release\WebEPG-conf.exe
    File ..\xbmc\bin\Release\X10Unified.dll
    File ..\xbmc\bin\Release\xAPMessage.dll
    File ..\xbmc\bin\Release\xAPTransport.dll
    File ..\xbmc\bin\Release\XPBurnComponent.dll
    ;------------  End of Common Files and Folders for XP & Vista

    
    /*; [OBSOLETE]  
    ; In Case of Vista some Folders / Files need to be copied to the Appplication Data Folder
    ; Simply Change the output Directory in Case of Vista
    ${if} $WindowsVersion == "Vista"
        ; We have a special MediaPortalDirs.xml
        File MediaPortalDirs.xml

        ; We need to have the custom Inputmapping dir created
        CreateDirectory "$CommonAppData\InputDeviceMappings\custom"

        ;From here on, the Vista specific files should go to the common App Folder
        SetOutPath $CommonAppData
    ${Else}
        File ..\xbmc\bin\Release\MediaPortalDirs.xml
    ${Endif}
    */

    File MediaPortalDirs.xml
    CreateDirectory "$CommonAppData\InputDeviceMappings\custom"
    SetOutPath $CommonAppData

    ; ************************************************************************

    ; Config Files (XML)
    File ..\xbmc\bin\Release\CaptureCardDefinitions.xml
    File "..\xbmc\bin\Release\eHome Infrared Transceiver List XP.xml"
    File ..\xbmc\bin\Release\FileDetailContents.xml
    File ..\xbmc\bin\Release\grabber_AllGame_com.xml
    File ..\xbmc\bin\Release\ISDNCodes.xml
    File ..\xbmc\bin\Release\keymap.xml
    File ..\xbmc\bin\Release\MusicVideoSettings.xml
    File ..\xbmc\bin\Release\ProgramSettingProfiles.xml
    File ..\xbmc\bin\Release\wikipedia.xml
    File ..\xbmc\bin\Release\yac-area-codes.xml

    ; We are not deleting Files and Folders after this point

    ; Folders
    File /r ..\xbmc\bin\Release\thumbs

    # [OBSOLETE] if we decide to install all filters to INSTDIR
    # [OBSOLETE] The Following Filters and Dll need to be copied to \windows\system32 for xp
    # [OBSOLETE] In Vista they stay in the Install Directory
    # [OBSOLETE] !insertmacro SetFilterDir
    # [OBSOLETE] NOTE: The Filters and Common DLLs found below will be deleted and unregistered manually and not via the automatic Uninstall Log
    # [OBSOLETE] Filters (Copy and Register)

    #---------------------------------------------------------------------------
    # FILTER REGISTRATION
    #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
    #---------------------------------------------------------------------------
    SetOutPath $INSTDIR
    ;filter used for SVCD and VCD playback
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\cdxareader.ax $INSTDIR\cdxareader.ax $INSTDIR
    ##### MAYBE used by VideoEditor
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\CLDump.ax $INSTDIR\CLDump.ax $INSTDIR
    ;filter for analog tv
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MpgMux.ax $INSTDIR\MpgMux.ax $INSTDIR
    ; used for scanning in tve2
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MPSA.ax $INSTDIR\MPSA.ax $INSTDIR
    ; used for shoutcast
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\shoutcastsource.ax $INSTDIR\shoutcastsource.ax $INSTDIR
    ; used for digital tv
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\TsReader.ax $INSTDIR\TsReader.ax $INSTDIR
    ##### not sure for what this is used
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\TTPremiumSource.ax $INSTDIR\TTPremiumSource.ax $INSTDIR
    ##### not sure for what this is used
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\WinTVCapWriter.ax $INSTDIR\WinTVCapWriter.ax $INSTDIR

    # [MAYBE OBSOLETE] File ..\xbmc\bin\Release\
    # [OBSOLETE] frodos first try for ts
    # [OBSOLETE] !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MPReader.ax $INSTDIR\MPReader.ax $INSTDIR
    # [OBSOLETE] !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MPTS.ax $INSTDIR\MPTS.ax $INSTDIR
    # [OBSOLETE] !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MPTSWriter.ax $INSTDIR\MPTSWriter.ax $INSTDIR

    # [OBSOLETE] replaced by tsreader
    # [OBSOLETE] !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\TSFileSource.ax $INSTDIR\TSFileSource.ax $INSTDIR

    # [OBSOLETE] Common DLLs
    # [OBSOLETE] Installing the Common dll
    # [OBSOLETE] !insertmacro InstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MFC71.dll $INSTDIR\MFC71.dll $INSTDIR
    # [OBSOLETE] !insertmacro InstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MFC71u.dll $INSTDIR\MFC71u.dll $INSTDIR
    
    ; needed by hauppauge.dll
    !insertmacro InstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\msvcp71.dll $INSTDIR\msvcp71.dll $INSTDIR
    !insertmacro InstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\msvcr71.dll $INSTDIR\msvcr71.dll $INSTDIR
SectionEnd
!macro Remove_${SecCore}
    DetailPrint "Uninstalling MediaPortal core files..."

    #---------------------------------------------------------------------------
    # FILTER UNREGISTRATION     for TVClient
    #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
    #---------------------------------------------------------------------------
    ;filter used for SVCD and VCD playback
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\cdxareader.ax
    ##### MAYBE used by VideoEditor
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\CLDump.ax
    ;filter for analog tv
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MpgMux.ax
    ; used for scanning in tve2
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MPSA.ax
    ; used for shoutcast
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\shoutcastsource.ax
    ; used for digital tv
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\TsReader.ax
    ##### not sure for what this is used
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\TTPremiumSource.ax
    ##### not sure for what this is used
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\WinTVCapWriter.ax

    # [OBSOLETE] frodos first try for ts
    # [OBSOLETE] !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MPReader.ax
    # [OBSOLETE] !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MPTS.ax
    # [OBSOLETE] !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MPTSWriter.ax

    # [OBSOLETE] replaced by tsreader
    # [OBSOLETE] !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\TSFileSource.ax

    # [OBSOLETE] Common DLLs will not be removed. Too Dangerous
    # [OBSOLETE] !insertmacro UnInstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MFC71.dll
    # [OBSOLETE] !insertmacro UnInstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MFC71u.dll
    # [OBSOLETE] !insertmacro UnInstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MFC80.dll
    # [OBSOLETE] !insertmacro UnInstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MFC80u.dll

    ; needed by hauppauge.dll
    !insertmacro UnInstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\msvcp71.dll
    !insertmacro UnInstallLib DLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\msvcr71.dll

    ; Config Files
    Delete /REBOOTOK  $CommonAppData\CaptureCardDefinitions.xml
    Delete /REBOOTOK  "$CommonAppData\eHome Infrared Transceiver List XP.xml"
    Delete /REBOOTOK  $CommonAppData\FileDetailContents.xml
    Delete /REBOOTOK  $CommonAppData\grabber_AllGame_com.xml
    Delete /REBOOTOK  $CommonAppData\ISDNCodes.xml
    Delete /REBOOTOK  $CommonAppData\keymap.xml
    Delete /REBOOTOK  $CommonAppData\MusicVideoSettings.xml
    Delete /REBOOTOK  $CommonAppData\ProgramSettingProfiles.xml
    Delete /REBOOTOK  $CommonAppData\wikipedia.xml
    Delete /REBOOTOK  $CommonAppData\yac-area-codes.xml

    ; Remove the Folders
    RmDir /r /REBOOTOK $INSTDIR\Burner
    RmDir /r /REBOOTOK $CommonAppData\Burner
    RmDir /r /REBOOTOK $INSTDIR\Cache
    RmDir /r /REBOOTOK $CommonAppData\Cache
    RmDir /r /REBOOTOK $INSTDIR\Docs
    RmDir /r /REBOOTOK $INSTDIR\database\convert
    RmDir /REBOOTOK $INSTDIR\database
    RmDir /r /REBOOTOK $INSTDIR\InputDeviceMappings\defaults
    RmDir /REBOOTOK $INSTDIR\InputDeviceMappings
    RmDir /r /REBOOTOK $INSTDIR\language
    RmDir /r /REBOOTOK $INSTDIR\MusicPlayer
    RmDir /r /REBOOTOK $INSTDIR\osdskin-media
    RmDir /r /REBOOTOK $INSTDIR\plugins
    RmDir /r /REBOOTOK $INSTDIR\scripts
    RmDir /r /REBOOTOK $INSTDIR\skin\BlueTwo
    RmDir /r /REBOOTOK "$INSTDIR\skin\BlueTwo wide"
    RmDir /REBOOTOK $INSTDIR\skin
    RmDir /r /REBOOTOK $INSTDIR\TTPremiumBoot
    RmDir /r /REBOOTOK $INSTDIR\Tuningparameters
    RmDir /r /REBOOTOK $INSTDIR\weather
    RmDir /r /REBOOTOK $INSTDIR\WebEPG
    RmDir /r /REBOOTOK $INSTDIR\Wizards

   ; Remove Files in MP Root Directory
    Delete /REBOOTOK  $INSTDIR\AppStart.exe
    Delete /REBOOTOK  $INSTDIR\AppStart.exe.config
    Delete /REBOOTOK  $INSTDIR\AxInterop.WMPLib.dll
    Delete /REBOOTOK  $INSTDIR\BallonRadio.ico
    Delete /REBOOTOK  $INSTDIR\bass.dll
    Delete /REBOOTOK  $INSTDIR\Bass.Net.dll
    Delete /REBOOTOK  $INSTDIR\bass_fx.dll
    Delete /REBOOTOK  $INSTDIR\bass_vis.dll
    Delete /REBOOTOK  $INSTDIR\bass_vst.dll
    Delete /REBOOTOK  $INSTDIR\bass_wadsp.dll
    Delete /REBOOTOK  $INSTDIR\bassasio.dll
    Delete /REBOOTOK  $INSTDIR\bassmix.dll
    Delete /REBOOTOK  $INSTDIR\BassRegistration.dll
    Delete /REBOOTOK  $INSTDIR\Configuration.exe
    Delete /REBOOTOK  $INSTDIR\Configuration.exe.config
    Delete /REBOOTOK  $INSTDIR\Core.dll
    Delete /REBOOTOK  $INSTDIR\CSScriptLibrary.dll
    Delete /REBOOTOK  $INSTDIR\d3dx9_30.dll
    Delete /REBOOTOK  $INSTDIR\DaggerLib.dll
    Delete /REBOOTOK  $INSTDIR\DaggerLib.DSGraphEdit.dll
    Delete /REBOOTOK  $INSTDIR\Databases.dll
    Delete /REBOOTOK  $INSTDIR\defaultMusicViews.xml
    Delete /REBOOTOK  $INSTDIR\defaultProgramViews.xml
    Delete /REBOOTOK  $INSTDIR\defaultVideoViews.xml
    Delete /REBOOTOK  $INSTDIR\DirectShowLib-2005.dll
    Delete /REBOOTOK  $INSTDIR\DirectShowLib.dll
    Delete /REBOOTOK  $INSTDIR\dlportio.dll
    Delete /REBOOTOK  $INSTDIR\dshowhelper.dll
    Delete /REBOOTOK  $INSTDIR\dvblib.dll
    Delete /REBOOTOK  $INSTDIR\dxerr9.dll
    Delete /REBOOTOK  $INSTDIR\DXUtil.dll
    Delete /REBOOTOK  $INSTDIR\edtftpnet-1.2.2.dll
    Delete /REBOOTOK  $INSTDIR\edtftpnet-1.2.5.dll
    Delete /REBOOTOK  $INSTDIR\FastBitmap.dll
    Delete /REBOOTOK  $INSTDIR\fontEngine.dll
    Delete /REBOOTOK  $INSTDIR\FTD2XX.DLL
    Delete /REBOOTOK  $INSTDIR\hauppauge.dll
    Delete /REBOOTOK  $INSTDIR\HcwHelper.exe
    Delete /REBOOTOK  $INSTDIR\ICSharpCode.SharpZipLib.dll
    Delete /REBOOTOK  $INSTDIR\inpout32.dll
    Delete /REBOOTOK  $INSTDIR\Interop.GIRDERLib.dll
    Delete /REBOOTOK  $INSTDIR\Interop.iTunesLib.dll
    Delete /REBOOTOK  $INSTDIR\Interop.TunerLib.dll
    Delete /REBOOTOK  $INSTDIR\Interop.WMEncoderLib.dll
    Delete /REBOOTOK  $INSTDIR\Interop.WMPLib.dll
    Delete /REBOOTOK  $INSTDIR\Interop.X10.dll
    Delete /REBOOTOK  $INSTDIR\KCS.Utilities.dll
    Delete /REBOOTOK  $INSTDIR\lame_enc.dll
    Delete /REBOOTOK  $INSTDIR\LibDriverCoreClient.dll
    Delete /REBOOTOK  $INSTDIR\log4net.dll
    Delete /REBOOTOK  $INSTDIR\madlldlib.dll
    Delete /REBOOTOK  $INSTDIR\MediaFoundation.dll
    Delete /REBOOTOK  $INSTDIR\MediaPadLayer.dll
    Delete /REBOOTOK  $INSTDIR\MediaPortalDirs.xml
    Delete /REBOOTOK  $INSTDIR\MediaPortal.exe
    Delete /REBOOTOK  $INSTDIR\MediaPortal.exe.config
    Delete /REBOOTOK  $INSTDIR\MediaPortal.Support.dll
    Delete /REBOOTOK  $INSTDIR\menu.bin
    Delete /REBOOTOK  $INSTDIR\Microsoft.ApplicationBlocks.ApplicationUpdater.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.ApplicationBlocks.ApplicationUpdater.Interfaces.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.ApplicationBlocks.ExceptionManagement.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.ApplicationBlocks.ExceptionManagement.Interfaces.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.DirectX.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.DirectX.Direct3D.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.DirectX.Direct3DX.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.DirectX.DirectDraw.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.DirectX.DirectInput.dll
    Delete /REBOOTOK  $INSTDIR\Microsoft.Office.Interop.Outlook.dll
    Delete /REBOOTOK  $INSTDIR\MPInstaller.exe
    Delete /REBOOTOK  $INSTDIR\MPInstaller.Library.dll
    Delete /REBOOTOK  $INSTDIR\mplogo.gif
    Delete /REBOOTOK  $INSTDIR\MPTestTool2.exe
    Delete /REBOOTOK  $INSTDIR\mpviz.dll
    Delete /REBOOTOK  $INSTDIR\MusicShareWatcher.exe
    Delete /REBOOTOK  $INSTDIR\MusicShareWatcherHelper.dll
    Delete /REBOOTOK  $INSTDIR\RemotePlugins.dll
    Delete /REBOOTOK  $INSTDIR\restart.vbs
    Delete /REBOOTOK  $INSTDIR\SG_VFD.dll
    Delete /REBOOTOK  $INSTDIR\SG_VFDv5.dll
    Delete /REBOOTOK  $INSTDIR\sqlite.dll
    Delete /REBOOTOK  $INSTDIR\taglib-sharp.dll
    Delete /REBOOTOK  $INSTDIR\TaskScheduler.dll
    Delete /REBOOTOK  $INSTDIR\ttBdaDrvApi_Dll.dll
    Delete /REBOOTOK  $INSTDIR\ttdvbacc.dll
    Delete /REBOOTOK  $INSTDIR\TVCapture.dll
    Delete /REBOOTOK  $INSTDIR\TVGuideScheduler.exe
    Delete /REBOOTOK  $INSTDIR\Utils.dll
    Delete /REBOOTOK  $INSTDIR\WebEPG.dll
    Delete /REBOOTOK  $INSTDIR\WebEPG.exe
    Delete /REBOOTOK  $INSTDIR\WebEPG-conf.exe
    Delete /REBOOTOK  $INSTDIR\X10Unified.dll
    Delete /REBOOTOK  $INSTDIR\xAPMessage.dll
    Delete /REBOOTOK  $INSTDIR\xAPTransport.dll
    Delete /REBOOTOK  $INSTDIR\XPBurnComponent.dll
!macroend

Section "DScaler Decoder" SecDscaler
    DetailPrint "Installing DScaler Decoder..."

    SetOutPath $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\GenDMOProp.dll $INSTDIR\GenDMOProp.dll $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MpegAudio.dll $INSTDIR\MpegAudio.dll $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MpegVideo.dll $INSTDIR\MpegVideo.dll $INSTDIR

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
SectionEnd
!macro Remove_${SecDscaler}
    DetailPrint "Uninstalling DScaler Decoder..."

    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\GenDMOProp.dll
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MpegAudio.dll
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MpegVideo.dll
!macroend

Section "Gabest MPA/MPV decoder" SecGabest
    DetailPrint "Installing Gabest MPA/MPV decoder..."

    SetOutPath $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\MpaDecFilter.ax $INSTDIR\MpaDecFilter.ax $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} ..\xbmc\bin\Release\Mpeg2DecFilter.ax $INSTDIR\Mpeg2DecFilter.ax $INSTDIR

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
SectionEnd
!macro Remove_${SecGabest}
    DetailPrint "Uninstalling Gabest MPA/MPV decoder..."

    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\MpaDecFilter.ax
    !insertmacro UnInstallLib REGDLL NOTSHARED ${FILTER_REBOOT_FLAG} $INSTDIR\Mpeg2DecFilter.ax
!macroend

#---------------------------------------------------------------------------
# This macro used to perform operation on multiple sections.
# List all of your components in following manner here.
!macro SectionList MacroName
    ;This macro used to perform operation on multiple sections.
    ;List all of your components in following manner here.

    !insertmacro "${MacroName}" "SecDscaler"
    !insertmacro "${MacroName}" "SecGabest"
!macroend

#---------------------------------------------------------------------------
# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
Section -Post
    ;Removes unselected components and writes component status to registry
    !insertmacro SectionList "FinishSection"

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
        CreateDirectory "$CommonAppData\log"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Log-Files.lnk"                  "$CommonAppData\log"            ""      "$CommonAppData\log"         0 "" "" "MediaPortal Log-Files"
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

    ; [OBSOLETE]  should be obsolete sooner or later
    /*WriteRegStr HKLM "${REG_UNINSTALL}" Path $INSTDIR
    WriteRegStr HKLM "${REG_UNINSTALL}" PathFilter $FILTERDIR
    WriteRegStr HKLM "${REG_UNINSTALL}" WindowsVersion $WindowsVersion*/

    ; [OBSOLETE]   WE SHOULD USER THE OTHER REGKEY IN FUTURE
    ; Write the Install / Config Dir into the registry for the Public SVN Installer to recognize the environment
    WriteRegStr HKLM "SOFTWARE\Team MediaPortal\MediaPortal" ApplicationDir $INSTDIR

    ; [OBSOLETE]   WE DONT NEED THIS REGKEY IN FUTURE
    /*${if} $WindowsVersion == "Vista"
        WriteRegStr HKLM "SOFTWARE\Team MediaPortal\MediaPortal" ConfigDir $CommonAppData
    ${Else}
        WriteRegStr HKLM "SOFTWARE\Team MediaPortal\MediaPortal" ConfigDir $INSTDIR
    ${Endif}*/

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
    ; first removes all optional components
    !insertmacro SectionList "RemoveSection"
    !insertmacro Remove_${SecCore}

    ; remove registry key
    DeleteRegKey HKLM "${REG_UNINSTALL}"

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
        RmDir /r /REBOOTOK $CommonAppData
        RmDir /r /REBOOTOK $INSTDIR
    ${EndIf}

    # [OBSOLETE] DeleteRegValue HKLM "${REG_UNINSTALL}" StartMenuGroup
    # [OBSOLETE] DeleteRegValue HKLM "${REG_UNINSTALL}" Path
    # [OBSOLETE] DeleteRegValue HKLM "${REG_UNINSTALL}" PathFilter

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
    ; [OBSOLETE] System::Call 'Shell32::SHChangeNotify(i 0x8000000, i 0, i 0, i 0)'

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
    ${GetOptions} $R0 "/noDscaler" $R1
    IfErrors +2
    StrCpy $noDscaler 1
    ${GetOptions} $R0 "/noGabest" $R1
    IfErrors +2
    StrCpy $noGabest 1
    ${GetOptions} $R0 "/noDesktopSC" $R1
    IfErrors +2
    StrCpy $noDesktopSC 1
    ${GetOptions} $R0 "/noStartMenuSC" $R1
    IfErrors +2
    StrCpy $noStartMenuSC 1
    #### END of check and parse cmdline parameter

    ; reads components status for registry
    !insertmacro SectionList "InitSection"

    ; update the component status -> commandline parameters have higher priority than registry values
    ${If} $noDscaler = 1
        !insertmacro UnselectSection ${SecDscaler}
    ${EndIf}
    ${If} $noGabest = 1
        !insertmacro UnselectSection ${SecGabest}
    ${EndIf}

    ; check if minimum Windows version is XP
    ${If} ${AtMostWin2000}
        MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)"
        Abort
    ${EndIf}

    ; check if .Net is installed
    Call IsDotNetInstalled
    Pop $0
    ${If} $0 == 0
        MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET)"
        Abort
    ${EndIf}

    !insertmacro SetCommonAppData

    # [OBSOLETE] Needed for Library Install
    # [OBSOLETE] Look if we already have a registry entry for MP. if this is the case we don't need to install anymore the Shared Libraraies
    # [OBSOLETE] Push $0
    # [OBSOLETE] ReadRegStr $0 HKLM "${REG_UNINSTALL}" Path
    # [OBSOLETE] ClearErrors
    # [OBSOLETE] StrCmp $0 "" +2
    # [OBSOLETE] StrCpy $LibInstall 1
    # [OBSOLETE] Pop $0
FunctionEnd

Function un.onInit
    #### check and parse cmdline parameter
    ; set default values for parameters ........
    strcpy $RemoveAll 0

    ; gets comandline parameter
    ${un.GetParameters} $R0

    ; check for special parameter and set the their variables
    ${un.GetOptions} $R0 "/RemoveAll" $R1
    IfErrors +2
    strcpy $RemoveAll 1
    #### END of check and parse cmdline parameter
    
    ReadRegStr $INSTDIR HKLM "${REG_UNINSTALL}" "InstallPath"
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

    !insertmacro SetCommonAppData
FunctionEnd

# Start the Configuration after the successfull install
# needed in an extra function to set the working directory
Function RunConfig
    SetOutPath $INSTDIR
    Exec "$INSTDIR\Configuration.exe"
FunctionEnd

Function WelcomeLeave
    ; check if MP is already installed
    ReadRegStr $R0 HKLM "${REG_UNINSTALL}" UninstallString
    IfFileExists $R0 0 unInstallDone
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
        IfErrors 0 unInstallDone
            MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_ON_UNINSTALL)" /SD IDNO IDYES unInstallDone IDNO 0
            Quit
    unInstallDone:

FunctionEnd

Function un.WelcomeLeave
    ; This function is called, before the uninstallation process is startet
    ; It asks the user, if he wants to remove all files and settings
    MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_REMOVE_ALL)" IDYES 0 IDNO noRemoveAll
        strcpy $RemoveAll 1
    noRemoveAll:

FunctionEnd

#---------------------------------------------------------------------------
# SECTION DECRIPTIONS     must be at the end
#---------------------------------------------------------------------------
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDscaler} $(DESC_SecDscaler)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecGabest}  $(DESC_SecGabest)
!insertmacro MUI_FUNCTION_DESCRIPTION_END