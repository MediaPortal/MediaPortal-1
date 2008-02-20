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

Name "MediaPortal TV Server / Client"

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
!define MUI_FINISHPAGE_RUN 
!define MUI_FINISHPAGE_RUN_FUNCTION RunSetup
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
Var InstallPath


# Installer pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!define MUI_PAGE_CUSTOMFUNCTION_PRE dir_pre          # Check, if the Server Component has been selected. Only display the directory page in this vase
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!define MUI_PAGE_CUSTOMFUNCTION_PRE finish_pre       # Check, if the Server Component has been selected. Only display the Startmenu page in this vase
!insertmacro MUI_PAGE_FINISH

# Uninstall Pages
!insertmacro MUI_UNPAGE_COMPONENTS
!define MUI_PAGE_CUSTOMFUNCTION_PRE un.dir_pre        # Check, if the Server Component has been selected. Only display the directory page in this vase
!insertmacro MUI_UNPAGE_CONFIRM
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
    
    ReadRegStr $InstallPath HKLM "${REGKEY}" InstallPath
    ${If} $InstallPath != ""
        MessageBox MB_OKCANCEL|MB_ICONQUESTION "TV Server is already installed.$\r$\nPress 'OK' to overwrite the existing installation$\r$\nPress 'Cancel' to Abort the installation" IDOK lbl_install IDCANCEL 0
        DetailPrint "User pressed Cancel. Skipping installation"
        Return
      lbl_install:
        # Uninstall / Stop the TV Service before proceeding with the installation 
        DetailPrint "DeInstalling TVService"
        ExecWait '"$InstallPath\TVService.exe" /uninstall'
        DetailPrint "Finished DeInstalling TVService"
    ${EndIf}    
    
    Pop $0
    
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
    File ..\Plugins\XmlTvImport\bin\Release\XmlTvImport.dll
    
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
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    SetOutPath $SMPROGRAMS\$StartMenuGroup
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MCE Blaster Learn.lnk" "$INSTDIR\Blaster.exe" "" "$INSTDIR\Blaster.exe" 0 "" "" "MCE Blaster Learn"
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TV Server Logs.lnk" "$CommonAppData\log" "" "$CommonAppData\log" 0 "" "" "TV Server Log Files"
    # Change outpath back to the install dir, so that the shortcut to SetupTV gets the correct working directory
    SetOutPath $INSTDIR
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TV Server.lnk" "$INSTDIR\SetupTV.exe" "" "mp.ico" 0 "" "" "MediaPortal TV Server"
    
    !insertmacro MUI_STARTMENU_WRITE_END
SectionEnd

Section "MediaPortal TV Plugin/Client" SecClient
    SetOverwrite on
    
    ReadRegSTR $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    
    DetailPrint "Installing MediaPortal TVPlugin"
    
    ${If} $MPBaseDir == ""
        MessageBox MB_OK|MB_ICONEXCLAMATION "Couldn't find an existing MediaPortal Installation.\r\nAborting the TV Plugin installation"       
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
    File ..\Plugins\PowerScheduler\bin\Release\PowerScheduler.Interfaces.dll
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

# This section is executed at the end to do some house keeping
Section -post 
    # Write the Uninstaller
    SetOutPath $INSTDIR
    WriteUninstaller $INSTDIR\uninstall-tve3.exe
    
    # Create Uninstaller Short Cut
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    SetOutPath $SMPROGRAMS\$StartMenuGroup
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
    ReadRegSTR $0 HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    ClearErrors
    StrCmp $0 "" +2
    StrCpy $LibInstall2 1
    Pop $0 
    
FunctionEnd

; Start the Setup after the successfull install
; needed in an extra function to set the working directory
Function RunSetup
SetOutPath $INSTDIR
Exec "$INSTDIR\SetupTV.exe"
FunctionEnd

# Uninstall TV Server
Section /o "un.MediaPortal TV Server" UNSecServer
     
    # De-instell the service
    DetailPrint "DeInstalling TVService"
    ExecWait '"$INSTDIR\TVService.exe" /uninstall'
    DetailPrint "Finished DeInstalling TVService"
       
    # Unregister the Filters
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\mpFileWriter.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\MpgMux.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\PDMpgMux.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\RTPSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\RtspSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\TSFileSource.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\TsReader.ax
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $InstDir\TsWriter.ax

    # Remove Folders
    RmDir /r /REBOOTOK $INSTDIR\Plugins
    RmDir /r /REBOOTOK $INSTDIR\TuningParameters
    
    # And finally remove all the files installed
    # Leave the directory in place, as it might contain user modified files
    Delete /REBOOTOK $INSTDIR\DirectShowLib.dll
    Delete /REBOOTOK $INSTDIR\dvblib.dll
    Delete /REBOOTOK $INSTDIR\PluginBase.dll
    Delete /REBOOTOK $INSTDIR\PowerScheduler.Interfaces.DLL
    Delete /REBOOTOK $INSTDIR\Blaster.exe
    Delete /REBOOTOK $INSTDIR\mp.ico
    Delete /REBOOTOK $INSTDIR\SetupTv.exe
    Delete /REBOOTOK $INSTDIR\SetupTv.exe.config
    Delete /REBOOTOK $INSTDIR\TvControl.dll
    Delete /REBOOTOK $INSTDIR\TVDatabase.dll
    Delete /REBOOTOK $INSTDIR\Gentle.Common.DLL
    Delete /REBOOTOK $INSTDIR\Gentle.Framework.DLL
    Delete /REBOOTOK $INSTDIR\Gentle.Provider.MySQL.dll
    Delete /REBOOTOK $INSTDIR\Gentle.Provider.SQLServer.dll
    Delete /REBOOTOK $INSTDIR\log4net.dll
    Delete /REBOOTOK $INSTDIR\MySql.Data.dll
    Delete /REBOOTOK $INSTDIR\TvBusinessLayer.dll
    Delete /REBOOTOK $INSTDIR\TvLibrary.Interfaces.dll
    Delete /REBOOTOK $INSTDIR\TVLibrary.dll
    Delete /REBOOTOK $INSTDIR\Germany_Unitymedia_NRW.dvbc
    Delete /REBOOTOK $INSTDIR\Gentle.config
    Delete /REBOOTOK $INSTDIR\TvService.exe
    Delete /REBOOTOK $INSTDIR\TvService.exe.config
    Delete /REBOOTOK $INSTDIR\SetupControls.dll
    #Filters
    Delete /REBOOTOK $INSTDIR\dxerr9.dll
    Delete /REBOOTOK $INSTDIR\hauppauge.dll
    Delete /REBOOTOK $INSTDIR\KNCBDACTRL.dll
    Delete /REBOOTOK $INSTDIR\ttBdaDrvApi_Dll.dll
    Delete /REBOOTOK $INSTDIR\ttdvbacc.dll
    Delete /REBOOTOK $INSTDIR\StreamingServer.dll
    Delete /REBOOTOK $INSTDIR\mpFileWriter.ax
    Delete /REBOOTOK $INSTDIR\MpgMux.ax
    Delete /REBOOTOK $INSTDIR\PDMpgMux.ax
    Delete /REBOOTOK $INSTDIR\RTPSource.ax
    Delete /REBOOTOK $INSTDIR\RtspSource.ax
    Delete /REBOOTOK $INSTDIR\TSFileSource.ax
    Delete /REBOOTOK $INSTDIR\TsReader.ax
    Delete /REBOOTOK $INSTDIR\TsWriter.ax
    
    # Remove Registry Keys and Start Menu
    DeleteRegValue HKLM "${REGKEY}\Components" SecServer
    DeleteRegValue HKLM "${REGKEY}" InstallPath
    
    # Check, if we have a Client installed. If Not, we can cleanup the registry and start menu
    Push $R0
    ReadRegStr $R0 HKLM "${REGKEY}\Components" SecClient
    ${If} $R0 == ""
        DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
        Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk"
        Delete /REBOOTOK $INSTDIR\uninstall-tve3.exe
        DeleteRegValue HKLM "${REGKEY}" StartMenuGroup
        RmDir /REBOOTOK $SMPROGRAMS\$StartMenuGroup       
        DeleteRegKey /IfEmpty HKLM "${REGKEY}\Components"
        DeleteRegKey /IfEmpty HKLM "${REGKEY}"           
    ${EndIf}
    Pop $R0
SectionEnd

# Uninstall TV Plugin
Section /o "un.MediaPortal TV Plugin/Client" UNSecClient
    
    # The Plugins
    Delete /REBOOTOK  $MPBaseDir\Plugins\Process\PowerSchedulerClientPlugin.dll
    Delete /REBOOTOK  $MPBaseDir\Plugins\Windows\TvPlugin.dll
    
    # Common Files
    Delete /REBOOTOK  $MPBaseDir\PowerScheduler.Interfaces.dll
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
   
    Delete /REBOOTOK  $MPBaseDir\DVBSub2.ax
    Delete /REBOOTOK  $MPBaseDir\RtspSource.ax
    Delete /REBOOTOK  $MPBaseDir\TSFileSource.ax
    Delete /REBOOTOK  $MPBaseDir\TsReader.ax
   
    DeleteRegValue HKLM "${REGKEY}\Components" SecClient
    
    # Check, if we have a TV Server installed. If Not, we can cleanup the registry and start menu
    Push $R0
    Push $R1
    ReadRegStr $R0 HKLM "${REGKEY}\Components" SecServer
    ${If} $R0 == ""    
        # Get the uninstall string, so that we can delete the exe
        ReadRegStr $R1 HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" UninstallString       
        Delete /REBOOTOK $R1
        Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk"
        DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
        DeleteRegValue HKLM "${REGKEY}" StartMenuGroup
        RmDir /REBOOTOK $SMPROGRAMS\$StartMenuGroup           
        DeleteRegKey /IfEmpty HKLM "${REGKEY}\Components"
        DeleteRegKey /IfEmpty HKLM "${REGKEY}"
    ${EndIf}
    Pop $R0
    Pop $R1
SectionEnd

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
LangString ^UninstallLink ${LANG_ENGLISH} "Uninstall $(^Name)"

LangString DESC_SECClient ${LANG_ENGLISH} "Installs the MediaPortal TVServer Client Plugin"
LangString DESC_SECServer ${LANG_ENGLISH} "Installs the MediaPortal TVServer"

LangString DESC_UnSECClient ${LANG_ENGLISH} "Uninstalls the MediaPortal TVServer Client Plugin"
LangString DESC_UnSECServer ${LANG_ENGLISH} "Uninstalls the MediaPortal TVServer"

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} $(DESC_SECClient)
  !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} $(DESC_SECServer)
  !insertmacro MUI_DESCRIPTION_TEXT ${UnSecClient} $(DESC_UNSECClient)
  !insertmacro MUI_DESCRIPTION_TEXT ${UnSecServer} $(DESC_UnSECServer) 
!insertmacro MUI_FUNCTION_DESCRIPTION_END

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

# This function is called, before the Uninstall Confirmation Page is displayed
# It checks, if the Server has been selected and only displays the Directory page in this case
Function un.dir_pre
         ${If} ${SectionIsSelected} UNSecServer
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
