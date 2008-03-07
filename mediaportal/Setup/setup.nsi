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
!define APP_NAME "MediaPortal 0.2.3.0"

Name "${APP_NAME}"
SetCompressor lzma
#SetCompressor /SOLID lzma  ; disabled solid, because of performance reasons


;..................................................................................................
;Following two definitions required. Uninstall log will use these definitions.
;You may use these definitions also, when you want to set up the InstallDirRagKey,
;store the language selection, store Start Menu folder etc.
;Enter the windows uninstall reg sub key to add uninstall information to Add/Remove Programs also.

!define REG_UNINSTALL "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"

!define VER_MAJOR       0
!define VER_MINOR       2
!define VER_REVISION    3
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif
;..................................................................................................

# Defines
!define VERSION 0.2.3.0
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"


# General Definitions for the Interface
;..................................................................................................
!define MUI_ICON    "images\install.ico"
!define MUI_UNICON  "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP          "images\header.bmp"
!define MUI_HEADERIMAGE_RIGHT
!define MUI_WELCOMEFINISHPAGE_BITMAP    "images\wizard.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP  "images\wizard.bmp"

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_DEFAULTFOLDER         "MediaPortal"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT         HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY          "${REG_UNINSTALL}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME    StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_TEXT     "Run MediaPortal Configuration"
!define MUI_FINISHPAGE_RUN_FUNCTION RunConfig

!define MUI_UNFINISHPAGE_NOAUTOCLOSE
;..................................................................................................

# Included files
;..................................................................................................
!include Sections.nsh
!include MUI2.nsh
!include LogicLib.nsh
!include InstallOptions.nsh
!include Library.nsh

!include setup-dotnet.nsh
!include setup-winversion.nsh
;..................................................................................................

# Variables used within the Script
;..................................................................................................
Var StartMenuGroup  ; Holds the Startup Group
Var WindowsVersion  ; The Windows Version
Var CommonAppData   ; The Common Application Folder
Var DSCALER         ; Should we install Dscaler Filter
Var GABEST          ; Should we install Gabest Filter
Var FilterDir       ; The Directory, where the filters have been installed
Var LibInstall      ; Needed for Library Installation
Var TmpDir          ; Needed for the Uninstaller
Var UninstAll       ; Set, when the user decided to uninstall everything
;..................................................................................................

# Installer pages
; These instructions define the sequence of the pages shown by the installer
;..................................................................................................
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
!insertmacro MUI_PAGE_LICENSE "..\Docs\BASS License.txt"
Page custom FilterSelection
!insertmacro MUI_PAGE_COMPONENTS

!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH


!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
UnInstPage custom un.UninstallOpionsSelection
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH
;..................................................................................................

# Installer languages
; We might include other languages
;..................................................................................................
!insertmacro MUI_LANGUAGE English
;..................................................................................................

# Installer attributes
; Set the output file name
;..................................................................................................
OutFile "Release\${APP_NAME}_setup.exe"
BrandingText "MediaPortal Installer by Team MediaPortal"
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

# Custom Page for Filter Selection
; This shows the Filter selection page
;..................................................................................................
LangString TEXT_IO_TITLE ${LANG_ENGLISH} "Install MPEG-2 decoder filters"
LangString TEXT_IO_SUBTITLE ${LANG_ENGLISH} "If there is no commercial MPEG-2 decoder installed on your system, you can install the decoders bundled with MediaPortal. They allow you to watch DVDs, TV and MPEG-2 videos without purchasing a third-party decoder. Furthermore those below decoders are required for DVR-MS conversion. It is recommended to leave these options enabled."

Function FilterSelection ;Function name defined with Page command
  !insertmacro MUI_HEADER_TEXT "$(TEXT_IO_TITLE)" "$(TEXT_IO_SUBTITLE)"
  !insertmacro INSTALLOPTIONS_DISPLAY "FilterSelect.ini"

  ; Get the values selected in the Check Boxes
  !insertmacro INSTALLOPTIONS_READ $DSCALER "FilterSelect.ini" "Field 1" "State"
  !insertmacro INSTALLOPTIONS_READ $GABEST "FilterSelect.ini" "Field 2" "State"
FunctionEnd
;..................................................................................................

# Installer sections
;
; This is the Main section, which installs all MediaPortal Files
;
;..................................................................................................
Section "MediaPortal" SecMediaPortal
    SectionIn RO
    DetailPrint "Installing MediaPortal..."
    
    SetOverwrite on

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
    File ..\xbmc\bin\Release\TTPremiumSource.ax
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
    File /r ..\xbmc\bin\Release\xmltv

    ; The Following Filters and Dll need to be copied to \windows\system32 for xp
    ; In Vista they stay in the Install Directory
    ${if} $WindowsVersion == "Vista"
        SetOutPath $INSTDIR
        StrCpy $FilterDir $InstDir
    ${Else}
        SetOutPath $SYSDIR
        StrCpy $FilterDir $SysDir
    ${Endif}


    ; NOTE: The Filters and Common DLLs found below will be deleted and unregistered manually and not via the automatic Uninstall Log

    ; Filters (Copy and Register)
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\cdxareader.ax $FilterDir\cdxareader.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\CLDump.ax $FilterDir\CLDump.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MpgMux.ax $FilterDir\MpgMux.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MPReader.ax $FilterDir\MPReader.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MPSA.ax $FilterDir\MPSA.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MPTS.ax $FilterDir\MPTS.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MPTSWriter.ax $FilterDir\MPTSWriter.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\shoutcastsource.ax $FilterDir\shoutcastsource.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\TSFileSource.ax $FilterDir\TSFileSource.ax $FilterDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\WinTVCapWriter.ax $FilterDir\WinTVCapWriter.ax $FilterDir

    ; Install and Register only when
    WriteRegStr HKLM "${REG_UNINSTALL}" Dscaler 0
    ${If} $DSCALER == 1
        WriteRegStr HKLM "${REG_UNINSTALL}" Dscaler 1
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\GenDMOProp.dll $FilterDir\GenDMOProp.dll $FilterDir
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MpegAudio.dll $FilterDir\MpegAudio.dll $FilterDir
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MpegVideo.dll $FilterDir\MpegVideo.dll $FilterDir
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
    ${EndIf}

    WriteRegStr HKLM "${REG_UNINSTALL}" Gabest 0
    ${If} $GABEST == 1
        WriteRegStr HKLM "${REG_UNINSTALL}" Gabest 1
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MpaDecFilter.ax $FilterDir\MpaDecFilter.ax $FilterDir
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\Mpeg2DecFilter.ax $FilterDir\Mpeg2DecFilter.ax $FilterDir

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
    ${EndIf}

    ; Common DLLs
    ; Installing the Common dll
    !insertmacro InstallLib DLL $LibInstall REBOOT_PROTECTED ..\xbmc\bin\Release\MFC71.dll $FilterDir\MFC71.dll $FilterDir
    !insertmacro InstallLib DLL $LibInstall REBOOT_PROTECTED ..\xbmc\bin\Release\MFC71u.dll $FilterDir\MFC71u.dll $FilterDir
    !insertmacro InstallLib DLL $LibInstall REBOOT_PROTECTED ..\xbmc\bin\Release\msvcp71.dll $FilterDir\msvcp71.dll $FilterDir
    !insertmacro InstallLib DLL $LibInstall REBOOT_PROTECTED ..\xbmc\bin\Release\msvcr71.dll $FilterDir\msvcr71.dll $FilterDir

    WriteRegStr HKLM "${REG_UNINSTALL}\Components" Main 1

    ; Write the Install / Config Dir into the registry for the Public SVN Installer to recognize the environment
    WriteRegStr HKLM "SOFTWARE\Team MediaPortal\MediaPortal" ApplicationDir $INSTDIR

    ${if} $WindowsVersion == "Vista"
       WriteRegStr HKLM "SOFTWARE\Team MediaPortal\MediaPortal" ConfigDir $CommonAppData
    ${Else}
        WriteRegStr HKLM "SOFTWARE\Team MediaPortal\MediaPortal" ConfigDir $INSTDIR
    ${Endif}
SectionEnd

# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
;..................................................................................................
 Section -Post
    WriteRegStr HKLM "${REG_UNINSTALL}" Path $INSTDIR
    WriteRegStr HKLM "${REG_UNINSTALL}" PathFilter $FILTERDIR
    WriteRegStr HKLM "${REG_UNINSTALL}" WindowsVersion $WindowsVersion

    ; Create the Statmenu and the Desktop shortcuts

    ; The OutputPath specifies the Working Directory used for the Shortcuts
    SetOutPath $INSTDIR
    WriteUninstaller $INSTDIR\uninstall.exe
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
    CreateDirectory $SMPROGRAMS\$StartMenuGroup
    SetShellVarContext current
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk" "$INSTDIR\MediaPortal.exe" "" "$INSTDIR\MediaPortal.exe" 0 "" "" "MediaPortal"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug.lnk" "$INSTDIR\MPTestTool2.exe" "-auto" "$INSTDIR\MPTestTool2.exe" 0 "" "" "MediaPortal Debug"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk" "$INSTDIR\Configuration.exe" "" "$INSTDIR\Configuration.exe" 0 "" "" "MediaPortal Configuration"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\License.lnk" "$INSTDIR\Docs\MediaPortal License.rtf" "" "$INSTDIR\Docs\MediaPortal License.rtf" 0 "" "" "License"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MPInstaller.lnk" "$INSTDIR\MPInstaller.exe" "" "$INSTDIR\MPInstaller.exe" 0 "" "" "MediaPortal Extension Installer"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MPTestTool.lnk" "$INSTDIR\MPTestTool2.exe" "" "$INSTDIR\MPTestTool2.exe" 0 "" "" "MediaPortal Test Tool"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk" $INSTDIR\uninstall.exe

    CreateShortcut "$DESKTOP\MediaPortal.lnk" "$INSTDIR\MediaPortal.exe" "" "$INSTDIR\MediaPortal.exe" 0 "" "" "MediaPortal"
    CreateShortcut "$DESKTOP\MediaPortal Configuration.lnk" "$INSTDIR\Configuration.exe" "" "$INSTDIR\Configuration.exe" 0 "" "" "MediaPortal Configuration"
    !insertmacro MUI_STARTMENU_WRITE_END

    !ifdef VER_MAJOR & VER_MINOR & VER_REVISION & VER_BUILD
        WriteRegDword HKLM "${REG_UNINSTALL}" "VersionMajor"    "${VER_MAJOR}"
        WriteRegDword HKLM "${REG_UNINSTALL}" "VersionMinor"    "${VER_MINOR}"
        WriteRegDword HKLM "${REG_UNINSTALL}" "VersionRevision" "${VER_REVISION}"
        WriteRegDword HKLM "${REG_UNINSTALL}" "VersionBuild"    "${VER_BUILD}"
    !endif

    # Write Uninstall Information
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName        "$(^Name)"
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayVersion     "${VERSION}"
    WriteRegStr HKLM "${REG_UNINSTALL}" Publisher          "${COMPANY}"
    WriteRegStr HKLM "${REG_UNINSTALL}" URLInfoAbout       "${URL}"
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon        "$INSTDIR\MediaPortal.exe,0"
    WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString    "$INSTDIR\uninstall.exe"
    #WriteRegStr HKLM "${REG_UNINSTALL}" ModifyPath         "$INSTDIR\add-remove-mp.exe"
    WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 1
    WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 1

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

    System::Call 'Shell32::SHChangeNotify(i 0x8000000, i 0, i 0, i 0)'
    !undef Index
SectionEnd

# Installer functions
Function .onInit
    InitPluginsDir

    !insertmacro INSTALLOPTIONS_EXTRACT "FilterSelect.ini"

    ; Get Windows Version
    Call GetWindowsVersion
    Pop $R0
    StrCpy $WindowsVersion $R0
    ${if} $WindowsVersion == "95"
    ${OrIf} $WindowsVersion == "98"
    ${OrIf} $WindowsVersion == "ME"
    ${OrIf} $WindowsVersion == "NT 4.0"
        MessageBox MB_OK|MB_ICONSTOP "MediaPortal is not support on Windows $WindowsVersion. Installation aborted"
        Abort
    ${EndIf}
    ${If} $WindowsVersion == "2003"
        ; MS Reports also XP 64 as NT 5.2. So we default on XP
        StrCpy $WindowsVersion 'XP'
    ${EndIf}

    ; Check if .Net is installed
    Call IsDotNetInstalled
    Pop $0
    ${If} $0 == 0
        MessageBox MB_OK|MB_ICONSTOP "Microsoft .Net Framework Runtime is a prerequisite. Please install first."
        Abort
    ${EndIf}

    ; Get the Common Application Data Folder to Store Files for Vista
    ; Set the Context to alll, so that we get the All Users folder
    SetShellVarContext all
    StrCpy $CommonAppData "$APPDATA\Team MediaPortal\MediaPortal"
    ; Context back to current user
    SetShellVarContext current

    ; Needed for Library Install
    ; Look if we already have a registry entry for MP. if this is the case we don't need to install anymore the Shared Libraraies
    Push $0
    ReadRegStr $0 HKLM "${REG_UNINSTALL}" Path
    ClearErrors
    StrCmp $0 "" +2
    StrCpy $LibInstall 1
    Pop $0
FunctionEnd

Function .onInstSuccess

FunctionEnd

; Start the Configuration after the successfull install
; needed in an extra function to set the working directory
Function RunConfig
SetOutPath $INSTDIR
Exec "$INSTDIR\Configuration.exe"
FunctionEnd

# Macro for selecting uninstaller sections
!macro SELECT_UNSECTION SECTION_NAME UNSECTION_ID
    Push $R0
    ReadRegStr $R0 HKLM "${REG_UNINSTALL}\Components" "${SECTION_NAME}"
    StrCmp $R0 1 0 next${UNSECTION_ID}
    !insertmacro SelectSection "${UNSECTION_ID}"
    GoTo done${UNSECTION_ID}
next${UNSECTION_ID}:
    !insertmacro UnselectSection "${UNSECTION_ID}"
done${UNSECTION_ID}:
    Pop $R0
!macroend

# Uninstaller sections

# Custom Page for Uninstall User settings
; This shows the Uninstall User Serrings Page
;..................................................................................................
LangString UNINSTALL_SETTINGS_TITLE ${LANG_ENGLISH} "Uninstall User settings"
LangString UNINSTALL_SETTINGS_SUBTITLE ${LANG_ENGLISH} "Attention: This will remove all your customised settings including Skins and Databases."

Function un.UninstallOpionsSelection ;Function name defined with Page command
  !insertmacro MUI_HEADER_TEXT "$(UNINSTALL_SETTINGS_TITLE)" "$(UNINSTALL_SETTINGS_SUBTITLE)"
  !insertmacro INSTALLOPTIONS_DISPLAY "UnInstallOptions.ini"

  ; Get the values selected in the Check Boxes
  !insertmacro INSTALLOPTIONS_READ $UninstAll "UninstallOptions.ini" "Field 1" "State"
FunctionEnd

LangString ^UninstallLink ${LANG_ENGLISH} "Uninstall $(^Name)"

Section /o -un.Main UNSEC0000
    ; Remove the Folders
    RmDir /r /REBOOTOK $INSTDIR\Burner
    RmDir /r /REBOOTOK $INSTDIR\Cache
    RmDir /r /REBOOTOK $INSTDIR\Docs
    RmDir /r /REBOOTOK $CommonAppData\Burner
    RmDir /r /REBOOTOK $CommonAppData\Cache
    RmDir /r /REBOOTOK $INSTDIR\language
    RmDir /r /REBOOTOK $INSTDIR\MusicPlayer
    RmDir /r /REBOOTOK $INSTDIR\osdskin-media
    RmDir /r /REBOOTOK $INSTDIR\plugins
    RmDir /r /REBOOTOK $INSTDIR\scripts
    RmDir /r /REBOOTOK $INSTDIR\skin\BlueTwo
    RmDir /r /REBOOTOK "$INSTDIR\skin\BlueTwo wide"
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
    Delete /REBOOTOK  $INSTDIR\SG_VFDv3.dll
    Delete /REBOOTOK  $INSTDIR\sqlite.dll
    Delete /REBOOTOK  $INSTDIR\taglib-sharp.dll
    Delete /REBOOTOK  $INSTDIR\TaskScheduler.dll
    Delete /REBOOTOK  $INSTDIR\ttBdaDrvApi_Dll.dll
    Delete /REBOOTOK  $INSTDIR\ttdvbacc.dll
    Delete /REBOOTOK  $INSTDIR\TTPremiumSource.ax
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
    ;------------  End of Files in MP Root Directory --------------

    ; In Case of Vista the Files to Uninstall are in the Appplication Data Folder
    ${if} $WindowsVersion == "Vista"
        StrCpy $TmpDir $CommonAppData
    ${Else}
         StrCpy $TmpDir $INSTDIR
    ${Endif}

    ; Config Files, which are in different location on Vista and XP (XML)
    Delete /REBOOTOK  $TmpDir\CaptureCardDefinitions.xml
    Delete /REBOOTOK  "$TmpDir\eHome Infrared Transceiver List XP.xml"
    Delete /REBOOTOK  $TmpDir\FileDetailContents.xml
    Delete /REBOOTOK  $TmpDir\grabber_AllGame_com.xml
    Delete /REBOOTOK  $TmpDir\ISDNCodes.xml
    Delete /REBOOTOK  $TmpDir\keymap.xml
    Delete /REBOOTOK  $TmpDir\MusicVideoSettings.xml
    Delete /REBOOTOK  $TmpDir\ProgramSettingProfiles.xml
    Delete /REBOOTOK  $TmpDir\wikipedia.xml
    Delete /REBOOTOK  $TmpDir\yac-area-codes.xml

    ; Do we need to deinstall everything? Then remove also the CommonAppData and InstDir
    ${If} $UninstAll == 1
        DetailPrint "Removing User Settings"
        RmDir /r /REBOOTOK $CommonAppData
        RmDir /r /REBOOTOK $INSTDIR
    ${EndIf}

    ; Uninstall the Common DLLs and Filters
    ; They will onl be removed, when the UseCount = 0
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\cdxareader.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\CLDump.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\MpgMux.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\MPReader.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\MPSA.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\MPTS.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\MPTSWriter.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\shoutcastsource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\TSFileSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\WinTVCapWriter.ax

    ${If} $DSCALER == 1
        !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\GenDMOProp.dll
        !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\MpegAudio.dll
        !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\MpegVideo.dll
    ${EndIf}

    ${If} $GABEST == 1
        !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\MpaDecFilter.ax
        !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $FilterDir\Mpeg2DecFilter.ax
    ${EndIf}

    ; Common DLLs will not be removed. Too Dangerous
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\MFC71.dll
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\MFC71u.dll
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\msvcp71.dll
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\msvcr71.dll
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\MFC80.dll
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\MFC80u.dll

    ; Delete StartMenu- , Desktop ShortCuts and Registry Entry
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug.lnk"
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\License.lnk
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\MPInstaller.lnk
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\MPTestTool.lnk
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk"
    Delete /REBOOTOK "$DESKTOP\MediaPortal Configuration.lnk"
    Delete /REBOOTOK $DESKTOP\MediaPortal.lnk
    DeleteRegValue HKLM "${REG_UNINSTALL}\Components" Main
SectionEnd

Section -un.post UNSEC0001
    DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk"
    Delete /REBOOTOK $INSTDIR\uninstall.exe
    DeleteRegValue HKLM "${REG_UNINSTALL}" StartMenuGroup
    DeleteRegValue HKLM "${REG_UNINSTALL}" Path
    DeleteRegValue HKLM "${REG_UNINSTALL}" PathFilter
    DeleteRegKey /IfEmpty HKLM "${REG_UNINSTALL}\Components"
    DeleteRegKey /IfEmpty HKLM "${REG_UNINSTALL}"
    RmDir /REBOOTOK $SMPROGRAMS\$StartMenuGroup

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

    System::Call 'Shell32::SHChangeNotify(i 0x8000000, i 0, i 0, i 0)'

    "${Index}-NoOwn:"
    !undef Index

SectionEnd

# Uninstaller functions
Function un.onInit
    ReadRegStr $INSTDIR HKLM "${REG_UNINSTALL}" Path
    ReadRegStr $FILTERDIR HKLM "${REG_UNINSTALL}" PathFilter
    ReadRegStr $GABEST HKLM "${REG_UNINSTALL}" Gabest
    ReadRegStr $DSCALER HKLM "${REG_UNINSTALL}" Dscaler
    ReadRegStr $WindowsVersion HKLM "${REG_UNINSTALL}" WindowsVersion
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup
    !insertmacro SELECT_UNSECTION Main ${UNSEC0000}

    ; Extract the Uninstall Option Custom Page
    !insertmacro INSTALLOPTIONS_EXTRACT "UnInstallOptions.ini"

    ; Get the Common Application Data Folder to Store Files for Vista
    ; Set the Context to alll, so that we get the All Users folder
    SetShellVarContext all
    StrCpy $CommonAppData "$APPDATA\Team MediaPortal\MediaPortal"
    ; Context back to current user
    SetShellVarContext current
FunctionEnd