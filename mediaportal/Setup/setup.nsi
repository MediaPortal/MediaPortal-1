#**********************************************************************************************************#
#
# For the MediaPortal Installer to work you need:
# 1. Lastest NSIS version from http://nsis.sourceforge.net/Download
# 
# Editing is much more easier, if you installe Eclipse from www.eclipse.org and the NSIS Plugin http://nsis.sourceforge.net/EclipseNSIS_-_NSIS_plugin_for_Eclipse
#
#**********************************************************************************************************#

!define APP_NAME "MediaPortal 0.2.3.0 RC3"

Name "${APP_NAME}"

SetCompressor lzma

;..................................................................................................
;Following two definitions required. Uninstall log will use these definitions.
;You may use these definitions also, when you want to set up the InstallDirRagKey,
;store the language selection, store Start Menu folder etc.
;Enter the windows uninstall reg sub key to add uninstall information to Add/Remove Programs also.

!define INSTDIR_REG_ROOT "HKLM"
!define INSTDIR_REG_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"
;..................................................................................................

# Defines
!define VERSION 0.2.3.0
!define COMPANY "Team MediaPortal"
!define URL www.team-mediaportal.com


# General Definitions for the Interface
;..................................................................................................
!define MUI_ICON "images\install.ico"
!define MUI_HEADERIMAGE_BITMAP "images\header.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "images\wizard.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "images\wizard.bmp"
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_STARTMENUPAGE_REGISTRY_ROOT HKLM
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_REGISTRY_KEY $(^INSTDIR_REG_KEY)
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME StartMenuGroup
!define MUI_STARTMENUPAGE_DEFAULTFOLDER MediaPortal
!define MUI_FINISHPAGE_RUN  
!define MUI_FINISHPAGE_RUN_FUNCTION RunConfig
!define MUI_FINISHPAGE_RUN_TEXT "Run MediaPortal Configuration"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
!define MUI_UNFINISHPAGE_NOAUTOCLOSE
;..................................................................................................

# Included files
;..................................................................................................
!include Sections.nsh
!include MUI2.nsh
!include LogicLib.nsh
!include InstallOptions.nsh
!include Library.nsh
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
;..................................................................................................

# Installer pages
; These instructions define the sequence of the pages shown by the installer
;..................................................................................................
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE ..\Docs\LICENSE.rtf
Page custom FilterSelection
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH


!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
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
CRCCheck on
XPStyle on
ShowInstDetails show
VIProductVersion 0.2.3.0
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName "${NAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductVersion "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyName "${COMPANY}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyWebsite "${URL}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileVersion "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription ""
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright ""
InstallDirRegKey HKLM "${INSTDIR_REG_KEY}" Path
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
Section -Main SEC0000
    SetOverwrite on   

    ; Doc
    SetOutPath $INSTDIR\Docs
    File "..\Docs\BASS License.txt"
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
    File ..\xbmc\bin\Release\Databases.dll
    File ..\xbmc\bin\Release\defaultMusicViews.xml
    File ..\xbmc\bin\Release\defaultProgramViews.xml
    File ..\xbmc\bin\Release\defaultVideoViews.xml
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
    File ..\xbmc\bin\Release\SG_LCD.dll
    File ..\xbmc\bin\Release\SG_VFD.dll
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
    WriteRegStr HKLM "${INSTDIR_REG_KEY}" Dscaler 0 
    ${If} $DSCALER == 1
        WriteRegStr HKLM "${INSTDIR_REG_KEY}" Dscaler 1
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\GenDMOProp.dll $FilterDir\GenDMOProp.dll $FilterDir
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MpegAudio.dll $FilterDir\MpegAudio.dll $FilterDir
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MpegVideo.dll $FilterDir\MpegVideo.dll $FilterDir
    ${EndIf}
  
    WriteRegStr HKLM "${INSTDIR_REG_KEY}" Gabest 0
    ${If} $GABEST == 1
        WriteRegStr HKLM "${INSTDIR_REG_KEY}" Gabest 1
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\MpaDecFilter.ax $FilterDir\MpaDecFilter.ax $FilterDir
        !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\xbmc\bin\Release\Mpeg2DecFilter.ax $FilterDir\Mpeg2DecFilter.ax $FilterDir
    ${EndIf}
    
    ; Common DLLs
    ; Installing the Common dll
    !insertmacro InstallLib DLL $LibInstall REBOOT_PROTECTED ..\xbmc\bin\Release\MFC71.dll $FilterDir\MFC71.dll $FilterDir
    !insertmacro InstallLib DLL $LibInstall REBOOT_PROTECTED ..\xbmc\bin\Release\MFC71u.dll $FilterDir\MFC71u.dll $FilterDir
    !insertmacro InstallLib DLL $LibInstall REBOOT_PROTECTED ..\xbmc\bin\Release\msvcp71.dll $FilterDir\msvcp71.dll $FilterDir
    !insertmacro InstallLib DLL $LibInstall REBOOT_PROTECTED ..\xbmc\bin\Release\msvcr71.dll $FilterDir\msvcr71.dll $FilterDir

    
    ; Create the Statmenu and the Desktop shortcuts
    SetShellVarContext current
    SetOutPath $SMPROGRAMS\$StartMenuGroup
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk" "$INSTDIR\MediaPortal.exe" "" "$INSTDIR\MediaPortal.exe" 0 "" "" "MediaPortal" 
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug.lnk" "$INSTDIR\MediaPortal.exe" "-auto" "$INSTDIR\MediaPortal.exe" 0 "" "" "MediaPortal Debug"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk" "$INSTDIR\Configuration.exe" "" "$INSTDIR\Configuration.exe" 0 "" "" "MediaPortal Configuration" 
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\License.lnk" "$INSTDIR\Docs\MediaPortal License.rtf" "" "$INSTDIR\Docs\MediaPortal License.rtf" 0 "" "" "License"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MPInstaller.lnk" "$INSTDIR\MPInstaller.exe" "" "$INSTDIR\MPInstaller.exe" 0 "" "" "MediaPortal Extension Installer"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MPTestTool.lnk" "$INSTDIR\MPTestTool2.exe" "" "$INSTDIR\MPTestTool2.exe" 0 "" "" "MediaPortal Test Tool"

    CreateShortcut "$DESKTOP\MediaPortal.lnk" "$INSTDIR\MediaPortal.exe" "" "$INSTDIR\MediaPortal.exe" 0 "" "" "MediaPortal" 
    CreateShortcut "$DESKTOP\MediaPortal Configuration.lnk" "$INSTDIR\Configuration.exe" "" "$INSTDIR\Configuration.exe" 0 "" "" "MediaPortal Configuration" 
    WriteRegStr HKLM "${INSTDIR_REG_KEY}\Components" Main 1
SectionEnd

# THis Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
;..................................................................................................
Section -post SEC0001
    WriteRegStr HKLM "${INSTDIR_REG_KEY}" Path $INSTDIR
    WriteRegStr HKLM "${INSTDIR_REG_KEY}" PathFilter $FILTERDIR
    WriteRegStr HKLM "${INSTDIR_REG_KEY}" WindowsVersion $WindowsVersion
    SetOutPath $INSTDIR
    WriteUninstaller $INSTDIR\uninstall.exe
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    SetOutPath $SMPROGRAMS\$StartMenuGroup
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk" $INSTDIR\uninstall.exe
    !insertmacro MUI_STARTMENU_WRITE_END
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayName "$(^Name)"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayVersion "${VERSION}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" Publisher "${COMPANY}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" URLInfoAbout "${URL}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayIcon $INSTDIR\uninstall.exe
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" UninstallString $INSTDIR\uninstall.exe
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" NoModify 1
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" NoRepair 1
SectionEnd


# Installer functions
Function .onInit
    InitPluginsDir
       
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
    ${if} $WindowsVersion == "2000" 
    ${OrIf} $WindowsVersion == "2003"
        MessageBox MB_OK|MB_ICONSTOP "MediaPortal is not support on Windows $WindowsVersion. Use at your own risk"
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
    ReadRegStr $0 HKLM "${INSTDIR_REG_KEY}" Path
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
    ReadRegStr $R0 HKLM "${INSTDIR_REG_KEY}\Components" "${SECTION_NAME}"
    StrCmp $R0 1 0 next${UNSECTION_ID}
    !insertmacro SelectSection "${UNSECTION_ID}"
    GoTo done${UNSECTION_ID}
next${UNSECTION_ID}:
    !insertmacro UnselectSection "${UNSECTION_ID}"
done${UNSECTION_ID}:
    Pop $R0
!macroend

# Uninstaller sections

LangString ^UninstallLink ${LANG_ENGLISH} "Uninstall $(^Name)"

Section /o -un.Main UNSEC0000    
    ; Remove the Folders
    ; Don't touch the Database, InputMappings
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
    RmDir /r /REBOOTOK $INSTDIR\skin
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
    Delete /REBOOTOK  $INSTDIR\Databases.dll
    Delete /REBOOTOK  $INSTDIR\defaultMusicViews.xml
    Delete /REBOOTOK  $INSTDIR\defaultProgramViews.xml
    Delete /REBOOTOK  $INSTDIR\defaultVideoViews.xml
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
    Delete /REBOOTOK  $INSTDIR\SG_LCD.dll
    Delete /REBOOTOK  $INSTDIR\SG_VFD.dll
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
    
    ; Delete StartMenu- , Desktop ShortCuts and Registry Entry
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug.lnk"
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\License.lnk
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\MPInstaller.lnk
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\MPTestTool.lnk
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\MediaPortal Configuration.lnk"
    Delete /REBOOTOK "$DESKTOP\MediaPortal Configuration.lnk"
    Delete /REBOOTOK $DESKTOP\MediaPortal.lnk
    DeleteRegValue HKLM "${INSTDIR_REG_KEY}\Components" Main
SectionEnd

Section -un.post UNSEC0001
    DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk"
    Delete /REBOOTOK $INSTDIR\uninstall.exe
    DeleteRegValue HKLM "${INSTDIR_REG_KEY}" StartMenuGroup
    DeleteRegValue HKLM "${INSTDIR_REG_KEY}" Path
    DeleteRegValue HKLM "${INSTDIR_REG_KEY}" PathFilter
    DeleteRegKey /IfEmpty HKLM "${INSTDIR_REG_KEY}\Components"
    DeleteRegKey /IfEmpty HKLM "${INSTDIR_REG_KEY}"
    RmDir /REBOOTOK $SMPROGRAMS\$StartMenuGroup
SectionEnd

# Uninstaller functions
Function un.onInit
    ReadRegStr $INSTDIR HKLM "${INSTDIR_REG_KEY}" Path
    ReadRegStr $FILTERDIR HKLM "${INSTDIR_REG_KEY}" PathFilter
    ReadRegStr $GABEST HKLM "${INSTDIR_REG_KEY}" Gabest
    ReadRegStr $DSCALER HKLM "${INSTDIR_REG_KEY}" Dscaler
    ReadRegStr $WindowsVersion HKLM "${INSTDIR_REG_KEY}" WindowsVersion
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup
    !insertmacro SELECT_UNSECTION Main ${UNSEC0000}
    
    ; Get the Common Application Data Folder to Store Files for Vista
    ; Set the Context to alll, so that we get the All Users folder
    SetShellVarContext all
    StrCpy $CommonAppData "$APPDATA\Team MediaPortal\MediaPortal"
    ; Context back to current user
    SetShellVarContext current
FunctionEnd

# Various Functions that helps us during the installation
;...............................................................................
; IsDotNETInstalled
;
; Usage:
;   Call IsDotNETInstalled
;   Pop $0
;   StrCmp $0 1 found.NETFramework no.NETFramework
Function IsDotNETInstalled
  Push $0
  Push $1
  Push $2
  Push $3
  Push $4
 
  ReadRegStr $4 HKEY_LOCAL_MACHINE \
    "Software\Microsoft\.NETFramework" "InstallRoot"
  # remove trailing back slash
  Push $4
  Exch $EXEDIR
  Exch $EXEDIR
  Pop $4
  # if the root directory doesn't exist .NET is not installed
  IfFileExists $4 0 noDotNET
 
  StrCpy $0 0
 
  EnumStart:
 
    EnumRegKey $2 HKEY_LOCAL_MACHINE \
      "Software\Microsoft\.NETFramework\Policy"  $0
    IntOp $0 $0 + 1
    StrCmp $2 "" noDotNET
 
    StrCpy $1 0
 
    EnumPolicy:
 
      EnumRegValue $3 HKEY_LOCAL_MACHINE \
        "Software\Microsoft\.NETFramework\Policy\$2" $1
      IntOp $1 $1 + 1
       StrCmp $3 "" EnumStart
        IfFileExists "$4\$2.$3" foundDotNET EnumPolicy
 
  noDotNET:
    StrCpy $0 0
    Goto done
 
  foundDotNET:
    StrCpy $0 1
 
  done:
    Pop $4
    Pop $3
    Pop $2
    Pop $1
    Exch $0
FunctionEnd

; GetWindowsVersion
;
; Based on Yazno's function, http://yazno.tripod.com/powerpimpit/
; Updated by Joost Verburg
;
; Returns on top of stack
;
; Windows Version (95, 98, ME, NT x.x, 2000, XP, 2003, Vista)
; or
; '' (Unknown Windows Version)
;
; Usage:
;   Call GetWindowsVersion
;   Pop $R0
;   ; at this point $R0 is "NT 4.0" or whatnot
 
Function GetWindowsVersion
 
  Push $R0
  Push $R1
 
  ClearErrors
 
  ReadRegStr $R0 HKLM \
  "SOFTWARE\Microsoft\Windows NT\CurrentVersion" CurrentVersion
 
  IfErrors 0 lbl_winnt
  
  ; we are not NT
  ReadRegStr $R0 HKLM \
  "SOFTWARE\Microsoft\Windows\CurrentVersion" VersionNumber
 
  StrCpy $R1 $R0 1
  StrCmp $R1 '4' 0 lbl_error
 
  StrCpy $R1 $R0 3
 
  StrCmp $R1 '4.0' lbl_win32_95
  StrCmp $R1 '4.9' lbl_win32_ME lbl_win32_98
 
  lbl_win32_95:
    StrCpy $R0 '95'
  Goto lbl_done
 
  lbl_win32_98:
    StrCpy $R0 '98'
  Goto lbl_done
 
  lbl_win32_ME:
    StrCpy $R0 'ME'
  Goto lbl_done
 
  lbl_winnt:
 
  StrCpy $R1 $R0 1
 
  StrCmp $R1 '3' lbl_winnt_x
  StrCmp $R1 '4' lbl_winnt_x
 
  StrCpy $R1 $R0 3
 
  StrCmp $R1 '5.0' lbl_winnt_2000
  StrCmp $R1 '5.1' lbl_winnt_XP
  StrCmp $R1 '5.2' lbl_winnt_2003
  StrCmp $R1 '6.0' lbl_winnt_vista lbl_error
 
  lbl_winnt_x:
    StrCpy $R0 "NT $R0" 6
  Goto lbl_done
 
  lbl_winnt_2000:
    Strcpy $R0 '2000'
  Goto lbl_done
 
  lbl_winnt_XP:
    Strcpy $R0 'XP'
  Goto lbl_done
 
  lbl_winnt_2003:
    Strcpy $R0 '2003'
  Goto lbl_done
 
  lbl_winnt_vista:
    Strcpy $R0 'Vista'
  Goto lbl_done
 
  lbl_error:
    Strcpy $R0 ''
  lbl_done:
 
  Pop $R1
  Exch $R0
 
FunctionEnd