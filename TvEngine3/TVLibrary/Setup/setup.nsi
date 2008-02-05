#**********************************************************************************************************#
#
# For the MediaPortal Installer to work you need:
# 1. Lastest NSIS version from http://nsis.sourceforge.net/Download
# 
# Editing is much more easier, if you install HM NSIS Edit from http://hmne.sourceforge.net
#
# ATTENTION: You need to have the vcredist_x86.exe package in the setup folder.
#            Haven't uploaded it, to save 2.5 MB in SVN
#**********************************************************************************************************#

Name "MediaPortal TV Server"

SetCompressor /SOLID lzma
RequestExecutionLevel admin
BrandingText "MediaPortal TVE3 Installer by Team MediaPortal"

# Defines
!define REGKEY "SOFTWARE\Team MediaPortal\$(^Name)"
!define VERSION 1.0
!define COMPANY "Team MediaPortal"
!define URL www.team-mediaportal.com

# MUI defines
!define MUI_ICON "images\install.ico"
!define MUI_HEADERIMAGE_BITMAP "images\header.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "images\wizard.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "images\wizard.bmp"
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_STARTMENUPAGE_REGISTRY_ROOT HKLM
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_REGISTRY_KEY "${REGKEY}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME StartMenuGroup
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "MediaPortal\MediaPortal TV Server"
!define MUI_FINISHPAGE_RUN_TEXT "Run MediaPortal TV Server Setup"
!define MUI_FINISHPAGE_RUN $INSTDIR\SetupTV.exe
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
!define MUI_UNFINISHPAGE_NOAUTOCLOSE

# Included files
!include Sections.nsh
!include MUI2.nsh
!include LogicLib.nsh
!include Library.nsh

# Variables
Var StartMenuGroup
Var LibInstall
Var LibInstall2
Var CommonAppData
Var MPBaseDir


# Installer pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!define MUI_PAGE_CUSTOMFUNCTION_PRE dir_pre             # Check, if the Server Component has been selected. Only display the directory page in this vase
!insertmacro MUI_PAGE_DIRECTORY
!define MUI_PAGE_CUSTOMFUNCTION_PRE startmenu_pre       # Check, if the Server Component has been selected. Only display the Startmenu page in this vase
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!define MUI_PAGE_CUSTOMFUNCTION_PRE finish_pre       # Check, if the Server Component has been selected. Only display the Startmenu page in this vase
!insertmacro MUI_PAGE_FINISH

# Uninstall Pages
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_COMPONENTS
!insertmacro MUI_UNPAGE_INSTFILES

# Installer languages
!insertmacro MUI_LANGUAGE English

# Installer attributes
OutFile Release\setup-tve3.exe
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal TV Server"
CRCCheck on
XPStyle on
ShowInstDetails show
VIProductVersion 1.0.0.0
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName "MediaPortal TV Server"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductVersion "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyName "${COMPANY}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyWebsite "${URL}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileVersion "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription ""
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright ""
InstallDirRegKey HKLM "${REGKEY}" InstallPath
ShowUninstDetails show

# Installer sections
Section "MediaPortal TV Server" SecServer
    SetOverwrite on
    DetailPrint "Installing MediaPortal TV Server"
    #---------------------------- File Copy ----------------------
    # The Plugin Directory
    SetOutPath $INSTDIR\Plugins
    File ..\Plugins\ComSkipLauncher\bin\Release\ComSkipLauncher.dll
    File ..\Plugins\ConflictsManager\bin\Release\ConflictsManager.dll
    File ..\Plugins\PersonalTVGuide\bin\Release\PersonalTVGuide.dll
    File ..\Plugins\PluginBase\bin\Release\PluginBase.dll
    File ..\Plugins\PowerScheduler\bin\Release\PowerScheduler.dll
    File ..\Plugins\PowerScheduler\bin\Release\PowerScheduler.Interfaces.dll
    File ..\Plugins\ServerBlaster\ServerBlaster\bin\Release\ServerBlaster.dll
    File ..\Plugins\TvMovie\bin\Release\TvMovie.dll
    File ..\Plugins\XmlTvImport\obj\Release\XmlTvImport.dll
    
    # Tuning Parameter Directory
    SetOutPath $INSTDIR
    File /r /x .svn ..\TvService\bin\Release\TuningParameters
    
    # Rest of Files
    File ..\DirectShowLib\bin\Release\DirectShowLib.dll
    File ..\dvblib.dll
    File ..\Plugins\PluginBase\bin\Release\PluginBase.dll
    File ..\Plugins\XmlTvImport\bin\Release\PowerScheduler.Interfaces.DLL
    File "..\Plugins\ServerBlaster\ServerBlaster (Learn)\bin\Release\Blaster.exe"
    File ..\Setup\mp.ico
    File ..\SetupTv\bin\Release\SetupTv.exe
    File ..\SetupTv\bin\Release\SetupTv.exe.config
    File ..\TvControl\bin\Release\TvControl.dll
    File ..\TVDatabase\bin\Release\TVDatabase.dll
    File ..\TVDatabase\references\Gentle.Common.DLL
    File ..\TVDatabase\references\Gentle.Framework.DLL
    File ..\TVDatabase\references\Gentle.Provider.MySQL.dll
    File ..\TVDatabase\references\Gentle.Provider.SQLServer.dll
    File ..\TVDatabase\references\log4net.dll
    File ..\TVDatabase\references\MySql.Data.dll
    File ..\TVDatabase\TvBusinessLayer\bin\Release\TvBusinessLayer.dll
    File ..\TvLibrary.Interfaces\bin\Release\TvLibrary.Interfaces.dll
    File ..\TVLibrary\bin\Release\TVLibrary.dll
    File ..\TvService\bin\Release\TuningParameters\Germany_Unitymedia_NRW.dvbc
    File ..\TvService\Gentle.config
    File ..\TvService\bin\Release\TvService.exe
    File ..\TvService\bin\Release\TvService.exe.config
    File ..\SetupControls\bin\Release\SetupControls.dll
        
    # Filters
    File ..\..\Filters\bin\dxerr9.dll
    File ..\..\Filters\bin\hauppauge.dll
    File ..\..\Filters\bin\KNCBDACTRL.dll
    File ..\..\Filters\bin\ttBdaDrvApi_Dll.dll
    File ..\..\Filters\bin\ttdvbacc.dll
    File ..\..\Filters\sources\StreamingServer\release\StreamingServer.dll

    # Following Filters are registered
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\..\Filters\bin\mpFileWriter.ax $InstDir\mpFileWriter.ax $InstDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\..\Filters\bin\MpgMux.ax $InstDir\MpgMux.ax $InstDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\..\Filters\bin\PDMpgMux.ax $InstDir\PDMpgMux.ax $InstDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\..\Filters\bin\RTPSource.ax $InstDir\RTPSource.ax $InstDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\..\Filters\bin\RtspSource.ax $InstDir\RtspSource.ax $InstDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\..\Filters\bin\TSFileSource.ax $InstDir\TSFileSource.ax $InstDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\..\Filters\bin\TsReader.ax $InstDir\TsReader.ax $InstDir
    !insertmacro InstallLib REGDLL $LibInstall REBOOT_NOTPROTECTED ..\..\Filters\bin\TsWriter.ax $InstDir\TsWriter.ax $InstDir
    
    # Common App Data Files
    SetOutPath "$CommonAppData"
    CreateDirectory "$CommonAppData\log"
    File ..\TvService\Gentle.config
    #---------------------------- End Of File Copy ----------------------  
    
    # Installing the TVService 
    DetailPrint "Installing TVService"
    ExecWait '"$INSTDIR\TVService.exe" /install'
    DetailPrint "Finished Installing TVService"
    
    #---------------------------- Post Installation Tasks ----------------------
    WriteRegStr HKLM "${REGKEY}\Components" SecServer 1
    WriteRegStr HKLM "${REGKEY}" InstallPath $INSTDIR
    
    # Create Short Cuts
    SetOutPath $INSTDIR
    WriteUninstaller $INSTDIR\uninstall-tve3.exe
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    SetOutPath $SMPROGRAMS\$StartMenuGroup
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MCE Blaster Learn.lnk" "$INSTDIR\Blaster.exe" "" "$INSTDIR\Blaster.exe" 0 "" "" "MCE Blaster Learn"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TV Server.lnk" "$INSTDIR\SetupTV.exe" "" "$INSTDIR\SetupTV.exe" 0 "" "" "MediaPortal TV Server"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TV Server Logs.lnk" "$CommonAppData\log" "" "$CommonAppData\log" 0 "" "" "TV Server Log Files"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk" $INSTDIR\uninstall-tve3.exe
    !insertmacro MUI_STARTMENU_WRITE_END
    
    # Write Uninstall Information
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayName "$(^Name)"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayVersion "${VERSION}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" Publisher "${COMPANY}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" URLInfoAbout "${URL}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayIcon $INSTDIR\uninstall-tve3.exe
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" UninstallString $INSTDIR\uninstall-tve3.exe
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" NoModify 1
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" NoRepair 1
    
SectionEnd

Section "MediaPortal TV Plugin/Client" SecClient
    SetOverwrite on
    
    ReadRegSTR $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    
    DetailPrint "Installing MediaPortal TVPlugin"
    
    ${If} $MPBaseDir == ""
        DetailPrint "No MediaPortal Installation found. Skipping installation"
        Return
    ${EndIf}
    
    DetailPrint "MediaPortal Installed at: $MpBaseDir"
        
    #---------------------------- File Copy ----------------------
    # The Plugins
    SetOutPath $MPBaseDir\Plugins\Process
    File ..\Plugins\PowerScheduler\ClientPlugin\bin\Release\PowerSchedulerClientPlugin.dll
    
    SetOutPath $MPBaseDir\Plugins\Windows
    File ..\TvPlugin\TvPlugin\bin\Release\TvPlugin.dll
    
    # Common Files
    SetOutPath $MPBaseDir
    File ..\TvControl\bin\Release\TvControl.dll
    File ..\TVDatabase\bin\Release\TVDatabase.dll
    File ..\TVDatabase\references\Gentle.Common.DLL
    File ..\TVDatabase\references\Gentle.Framework.DLL
    File ..\TVDatabase\references\Gentle.Provider.MySQL.dll
    File ..\TVDatabase\references\Gentle.Provider.SQLServer.dll
    File ..\TVDatabase\references\log4net.dll
    File ..\TVDatabase\references\MySql.Data.dll
    File ..\TVDatabase\TvBusinessLayer\bin\Release\TvBusinessLayer.dll
    File ..\TvLibrary.Interfaces\bin\Release\TvLibrary.Interfaces.dll
    File ..\TvPlugin\TvPlugin\Gentle.config
        
    !insertmacro InstallLib REGDLL $LibInstall2 REBOOT_NOTPROTECTED ..\..\Filters\bin\DVBSub2.ax $MPBaseDir\DVBSub2.ax $MPBaseDir
    !insertmacro InstallLib REGDLL $LibInstall2 REBOOT_NOTPROTECTED ..\..\Filters\bin\RtspSource.ax $MPBaseDir\RtspSource.ax $MPBaseDir
    !insertmacro InstallLib REGDLL $LibInstall2 REBOOT_NOTPROTECTED ..\..\Filters\bin\TSFileSource.ax $MPBaseDir\TSFileSource.ax $MPBaseDir
    !insertmacro InstallLib REGDLL $LibInstall2 REBOOT_NOTPROTECTED ..\..\Filters\bin\TsReader.ax $MPBaseDir\TsReader.ax $MPBaseDir
    
    WriteRegStr HKLM "${REGKEY}\Components" SecClient 1

SectionEnd

# This section installs the VC++ Redist Library
Section -Redist SecRedist
    SetOutPath $INSTDIR
    SetOverwrite on
    
    # Now Copy the VC Redist File, which will be executed as part of the install
    File vcredist_x86.exe

    # Installing VC++ Redist Package
    DetailPrint "Installing VC++ Redist Package"
    ExecWait '"$INSTDIR\vcredist_x86.exe" /q:a /c:"VCREDI~3.EXE /q:a /c:""msiexec /i vcredist.msi /qb!"" "'
    DetailPrint "Finished Installing VC++ Redist Package"
    Delete /REBOOTOK  $INSTDIR\vcredist_x86.exe
SectionEnd

LangString DESC_SECClient ${LANG_ENGLISH} "Installs the MediaPortal TVServer Client Plugin"
LangString DESC_SECServer ${LANG_ENGLISH} "Installs the MediaPortal TVServer"

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} $(DESC_SECClient)
  !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} $(DESC_SECServer)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

# Macro for selecting uninstaller sections
!macro SELECT_UNSECTION SECTION_NAME UNSECTION_ID
    Push $R0
    ReadRegStr $R0 HKLM "${REGKEY}\Components" "${SECTION_NAME}"
    StrCmp $R0 1 0 next${UNSECTION_ID}
    !insertmacro SelectSection "${UNSECTION_ID}"
    GoTo done${UNSECTION_ID}
next${UNSECTION_ID}:
    !insertmacro UnselectSection "${UNSECTION_ID}"
    # Make the unselected section read only
    !insertmacro SetSectionFlag "${UNSECTION_ID}" 16
done${UNSECTION_ID}:
    Pop $R0
!macroend

# Uninstaller sections
Section /o un.SecServer UNSecServer
    
    # Remove Folders
    RmDir /r /REBOOTOK $INSTDIR\Plugins
    RmDir /r /REBOOTOK $INSTDIR\TuningParameters
    
    # De-instell the service
        # Installing the TVService 
    DetailPrint "DeInstalling TVService"
    ExecWait '"$INSTDIR\TVService.exe" /uninstall'
    DetailPrint "Finished Installing TVService"
       
    # Unregister the Filters
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\mpFileWriter.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\MpgMux.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\PDMpgMux.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\RTPSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\RtspSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\TSFileSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\TsReader.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\TsWriter.ax
    
    # And finally remove the complete install dir
    RmDir /r $InstDir
    
    # Remove Registry Keys and Start Menu
    DeleteRegValue HKLM "${REGKEY}\Components" SecServer
    DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk"
    Delete /REBOOTOK $INSTDIR\uninstall-tve3.exe
    DeleteRegValue HKLM "${REGKEY}" StartMenuGroup
    DeleteRegValue HKLM "${REGKEY}" InstallPath
    DeleteRegKey /IfEmpty HKLM "${REGKEY}\Components"
    DeleteRegKey /IfEmpty HKLM "${REGKEY}"
    RmDir /REBOOTOK $SMPROGRAMS\$StartMenuGroup
    RmDir /REBOOTOK $INSTDIR
SectionEnd

# Uninstaller sections
Section /o un.SecClient UNSecClient
    
    # The Plugins
    Delete /REBOOTOK  $MPBaseDir\Plugins\Process\PowerSchedulerClientPlugin.dll
    Delete /REBOOTOK  $MPBaseDir\Plugins\Windows\TvPlugin.dll
    
    # Common Files
    Delete /REBOOTOK  $MPBaseDir\TvControl.dll
    Delete /REBOOTOK  $MPBaseDir\TVDatabase.dll
    Delete /REBOOTOK  $MPBaseDir\Gentle.Common.DLL
    Delete /REBOOTOK  $MPBaseDir\Gentle.Framework.DLL
    Delete /REBOOTOK  $MPBaseDir\Gentle.Provider.MySQL.dll
    Delete /REBOOTOK  $MPBaseDir\Gentle.Provider.SQLServer.dll
    Delete /REBOOTOK  $MPBaseDir\log4net.dll
    Delete /REBOOTOK  $MPBaseDir\MySql.Data.dll
    Delete /REBOOTOK  $MPBaseDir\TvBusinessLayer.dll
    Delete /REBOOTOK  $MPBaseDir\TvLibrary.Interfaces.dll
    Delete /REBOOTOK  $MPBaseDir\Gentle.config
    
    #Unregister the Filters
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $MPBaseDir\DVBSub2.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $MPBaseDir\RtspSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $MPBaseDir\TSFileSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $MPBaseDir\TsReader.ax
   
    DeleteRegValue HKLM "${REGKEY}\Components" SecClient
    DeleteRegKey /IfEmpty HKLM "${REGKEY}\Components"
    DeleteRegKey /IfEmpty HKLM "${REGKEY}"
SectionEnd


# Installer functions
Function .onInit
    InitPluginsDir
    
    ; Get the Common Application Data Folder
    ; Set the Context to alll, so that we get the All Users folder
    SetShellVarContext all
    StrCpy $CommonAppData "$APPDATA\MediaPortal TV Server"
    ; Context back to current user
    SetShellVarContext current
    
    ; Needed for Library Install
    ; Look if we already have a registry entry for TV Server. if this is the case we don't need to install anymore the Shared Libraraies
    Push $0
    ReadRegStr $0 HKLM "${REGKEY}" InstallPath
    ClearErrors
    StrCmp $0 "" +2
    StrCpy $LibInstall 1
    Pop $0

    ; Needed for Library Install
    ; Look if we already have a registry entry for MP. if this is the case we don't need to install anymore the Shared Libraraies
    Push $0
    ReadRegSTR $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    ClearErrors
    StrCmp $0 "" +2
    StrCpy $LibInstall2 1
    Pop $0 
    
FunctionEnd

# Uninstaller functions
Function un.onInit
    ReadRegSTR $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    ReadRegStr $INSTDIR HKLM "${REGKEY}" InstallPath
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup
    !insertmacro SELECT_UNSECTION SecServer ${UNSecServer}
    !insertmacro SELECT_UNSECTION SecClient ${UNSecClient}
    
        ; Get the Common Application Data Folder
    ; Set the Context to alll, so that we get the All Users folder
    SetShellVarContext all
    StrCpy $CommonAppData "$APPDATA\MediaPortal TV Server"
    ; Context back to current user
    SetShellVarContext current
FunctionEnd

# Installer Language Strings
# TODO Update the Language Strings with the appropriate translations.

LangString ^UninstallLink ${LANG_ENGLISH} "Uninstall $(^Name)"

# This function is called, before the Directory Page is displayed
# It checks, if the Server has been selected and only displays the Directory page in this case
Function dir_pre
         ${If} ${SectionIsSelected} SecServer
            strcpy $0 1
         ${Else}
            strcpy $0 2
            abort            
         ${EndIf}
Functionend

# This function is called, before the Startmenu Page is displayed
# It checks, if the Server has been selected and only displays the Directory page in this case
Function startmenu_pre
         ${If} ${SectionIsSelected} SecServer
            strcpy $0 1
         ${Else}
            strcpy $0 2
            abort            
         ${EndIf}
Functionend

# This function is called, before the Finish Page is displayed
# It checks, if the Server has been selected and only displays the Directory page in this case
Function finish_pre
         ${If} ${SectionIsSelected} SecServer
            strcpy $0 1
         ${Else}
             strcpy $0 2
             abort       
         ${EndIf}
Functionend
