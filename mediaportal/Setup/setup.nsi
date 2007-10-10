#**********************************************************************************************************#
#
# For the MediaPortal Installer to work you need:
# 1. Lastest NSIS version from http://nsis.sourceforge.net/Download
# 2. Advanced Uninstall Log NSIS Header from http://nsis.sourceforge.net/Advanced_Uninstall_Log_NSIS_Header
# 3. Editing is much more easier, if you installe Eclipse from www.eclipse.org and the NSIS Plugin http://nsis.sourceforge.net/EclipseNSIS_-_NSIS_plugin_for_Eclipse
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
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_STARTMENUPAGE_REGISTRY_ROOT HKLM
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_REGISTRY_KEY $(^INSTDIR_REG_KEY)
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME StartMenuGroup
!define MUI_STARTMENUPAGE_DEFAULTFOLDER MediaPortal
!define MUI_FINISHPAGE_RUN $INSTDIR\Configuration.exe
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
!include AdvUninstLog.nsh
!include Library.nsh
;..................................................................................................

# Variables used within the Script
;..................................................................................................
Var StartMenuGroup  ; Holds the Startup Group
Var WindowsVersion  ; The Windows Version
Var CommonAppData   ; The Common Application Folder
VAR DSCALER         ; Should we install Dscaler Filter
VAR GABEST          ; Should we install Gabest Filter
VAR FilterDir       ; The Directory, where the filters have been installed
Var LibInstall      ; Needed for Library Installation
;..................................................................................................

# Uninstaller Options
;..................................................................................................
;Specify the preferred uninstaller operation mode, either unattended or interactive.
;Be aware only one of the following two macros has to be inserted, neither both, neither none.

!insertmacro UNATTENDED_UNINSTALL

;!insertmacro INTERACTIVE_UNINSTALL
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
    SetOutPath $INSTDIR
    SetOverwrite on   

    ;After set the output path open the uninstall log macros block and add files/dirs with File /r
    ;This should be repeated every time the parent output path is changed either within the same
    ;section, or if there are more sections including optional components.
    !insertmacro UNINSTALL.LOG_OPEN_INSTALL
        
    ;------------  Common Files and Folders for XP & Vista
    ; Doc
    File "..\Docs\BASS License.txt"
    File "..\Docs\MediaPortal License.rtf"
    File "..\Docs\SQLite Database Browser.exe"

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
    File ..\xbmc\bin\Release\mpviz.dll
    File ..\xbmc\bin\Release\MusicShareWatcher.exe
    File ..\xbmc\bin\Release\MusicShareWatcherHelper.dll
    File ..\xbmc\bin\Release\RemotePlugins.dll
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
    
    ;Before changing the parent output directory we need to close the opened previously uninstall log macros block.
    !insertmacro UNINSTALL.LOG_CLOSE_INSTALL
    
    ; And we open it again
    !insertmacro UNINSTALL.LOG_OPEN_INSTALL
    
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

    ; Folders 
    File /r ..\xbmc\bin\Release\thumbs
    File /r ..\xbmc\bin\Release\xmltv
    
    ; Close before we change the directory
    !insertmacro UNINSTALL.LOG_CLOSE_INSTALL
    
    ; And we open it again
    
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
    File ..\xbmc\bin\Release\cdxareader.ax
    File ..\xbmc\bin\Release\CLDump.ax
    File ..\xbmc\bin\Release\MpgMux.ax
    File ..\xbmc\bin\Release\MPReader.ax
    File ..\xbmc\bin\Release\MPSA.ax
    File ..\xbmc\bin\Release\MPTS.ax
    File ..\xbmc\bin\Release\MPTSWriter.ax
    File ..\xbmc\bin\Release\shoutcastsource.ax
    File ..\xbmc\bin\Release\TSFileSource.ax
    File ..\xbmc\bin\Release\WinTVCapWriter.ax

    RegDll $FilterDir\cdxareader.ax
    RegDll $FilterDir\CLDump.ax
    RegDll $FilterDir\MpgMux.ax
    RegDll $FilterDir\MPReader.ax
    RegDll $FilterDir\MPSA.ax
    RegDll $FilterDir\MPTS.ax
    RegDll $FilterDir\MPTSWriter.ax
    RegDll $FilterDir\shoutcastsource.ax
    RegDll $FilterDir\TSFileSource.ax
    RegDll $FilterDir\WinTVCapWriter.ax

    ; Install and Register only when 
    ${If} $DSCALER == 1
        File ..\xbmc\bin\Release\GenDMOProp.dll
        File ..\xbmc\bin\Release\MpegAudio.dll
        File ..\xbmc\bin\Release\MpegVideo.dll
        
        RegDll $FilterDir\GenDMOProp.dll
        RegDll $FilterDir\MpegAudio.dll
        RegDll $FilterDir\MpegVideo.dll
    ${EndIf}
  
    ${If} $GABEST == 1
        File ..\xbmc\bin\Release\MpaDecFilter.ax
        File ..\xbmc\bin\Release\Mpeg2DecFilter.ax
        
        RegDll $FilterDir\MpaDecFilter.ax
        RegDll $FilterDir\Mpeg2DecFilter.ax        
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

    CreateShortcut "$DESKTOP\MediaPortal.lnk" "$INSTDIR\MediaPortal.exe" "" "$INSTDIR\MediaPortal.exe" 0 "" "" "MediaPortal" 
    CreateShortcut "$DESKTOP\MediaPortal Configuration.lnk" "$INSTDIR\Configuration.exe" "" "$INSTDIR\Configuration.exe" 0 "" "" "MediaPortal Configuration" 
    WriteRegStr HKLM "${INSTDIR_REG_KEY}\Components" Main 1
SectionEnd

# THis Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
;..................................................................................................
Section -post SEC0001
    WriteRegStr HKLM "${INSTDIR_REG_KEY}" Path $INSTDIR
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
    
    ; Prepare the Uninstall Log
    !insertmacro UNINSTALL.LOG_PREPARE_INSTALL
        
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
    Push $0
    ReadRegStr $0 HKLM "${INSTDIR_REG_KEY}" Path
    ClearErrors
    StrCmp $0 "" +2
    StrCpy $LibInstall 1
    Pop $0
FunctionEnd

Function .onInstSuccess

    ;create/update log always within .onInstSuccess function
    !insertmacro UNINSTALL.LOG_UPDATE_INSTALL

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

    ;uninstall from path, must be repeated for every install logged path individual
    !insertmacro UNINSTALL.LOG_UNINSTALL "$INSTDIR"
    !insertmacro UNINSTALL.LOG_UNINSTALL "$APPDATA\Team MediaPortal\MediaPortal"

    ;end uninstall, after uninstall from all logged paths has been performed
    !insertmacro UNINSTALL.LOG_END_UNINSTALL

    ; Remove the Common DLLs and Filters, which have not been logged with the Automatic Uninstaller
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\MFC71.dll
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\MFC71u.dll
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\msvcp71.dll
    !insertmacro UnInstallLib DLL SHARED NOREMOVE $FilterDir\msvcr71.dll    

    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\MediaPortal.lnk
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\MediaPortal Debug.lnk"
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\License.lnk
    Delete /REBOOTOK $SMPROGRAMS\$StartMenuGroup\MPInstaller.lnk
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
    DeleteRegKey /IfEmpty HKLM "${INSTDIR_REG_KEY}\Components"
    DeleteRegKey /IfEmpty HKLM "${INSTDIR_REG_KEY}"
    RmDir /REBOOTOK $SMPROGRAMS\$StartMenuGroup
SectionEnd

# Uninstaller functions
Function un.onInit
    ReadRegStr $INSTDIR HKLM "${INSTDIR_REG_KEY}" Path
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup
    !insertmacro SELECT_UNSECTION Main ${UNSEC0000}
    !insertmacro UNINSTALL.LOG_BEGIN_UNINSTALL
    
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