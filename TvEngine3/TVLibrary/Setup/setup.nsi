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
# Used code for Add/Remove page from official NSIS installation file.
#
#
#
#
#**********************************************************************************************************#
Name "MediaPortal TV Server / Client"
SetCompressor /SOLID lzma
RequestExecutionLevel admin

#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
Var StartMenuGroup
Var LibInstall
Var LibInstall2
Var CommonAppData
Var MPBaseDir
Var InstallPath
;   variables for commandline parameters for Installer
Var noClient
Var noServer
Var noDesktopSC
Var noStartMenuSC
;   variables for commandline parameters for UnInstaller
Var CompleteCleanup

#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"

!define REGKEY          "SOFTWARE\Team MediaPortal\MediaPortal TV Server / Client"
!define REG_UNINSTALL   "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server / Client"

!define VER_MAJOR       0
!define VER_MINOR       9
!define VER_REVISION    0
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include LogicLib.nsh
!include Library.nsh
!include WordFunc.nsh
!include FileFunc.nsh

!include setup-addremove.nsh
!include setup-languages.nsh

!insertmacro GetParameters
!insertmacro GetOptions
!insertmacro un.GetParameters
!insertmacro un.GetOptions

#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ICON "images\install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!if ${VER_BUILD} == 0       # it's a stable release
    !define MUI_HEADERIMAGE_BITMAP "images\header.bmp"
    !define MUI_WELCOMEFINISHPAGE_BITMAP "images\wizard.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP "images\wizard.bmp"

    !define VERSION "1.0"
    BrandingText "MediaPortal TVE3 Installer by Team MediaPortal"
!else                       # it's an svn reöease
    !define MUI_HEADERIMAGE_BITMAP "images\header-svn.bmp"
    !define MUI_WELCOMEFINISHPAGE_BITMAP "images\wizard-svn.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP "images\wizard-svn.bmp"
    
    !define VERSION "pre-release build ${VER_BUILD}"
    BrandingText "${VERSION}"
!endif
!define MUI_HEADERIMAGE_RIGHT

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "MediaPortal\MediaPortal TV Server"
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_REGISTRY_ROOT HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY "${REGKEY}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_TEXT "Run MediaPortal TV Server Setup"
!define MUI_FINISHPAGE_RUN_FUNCTION RunSetup

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
!insertmacro MUI_PAGE_WELCOME
!ifdef VER_MAJOR & VER_MINOR & VER_REVISION & VER_BUILD
    Page custom PageReinstall PageLeaveReinstall
!endif
!define MUI_PAGE_CUSTOMFUNCTION_PRE DisableClientIfNoMP #check, if MediaPortal is installed, if not uncheck and disable the ClientPluginSection
!insertmacro MUI_PAGE_COMPONENTS
!define MUI_PAGE_CUSTOMFUNCTION_PRE dir_pre             # Check, if the Server Component has been selected. Only display the directory page in this vase
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!define MUI_PAGE_CUSTOMFUNCTION_PRE finish_pre          # Check, if the Server Component has been selected. Only display the Startmenu page in this vase
!insertmacro MUI_PAGE_FINISH
; UnInstaller Interface
;[OBSOLETE]         !insertmacro MUI_UNPAGE_COMPONENTS
;[OBSOLETE]         !define MUI_PAGE_CUSTOMFUNCTION_PRE un.dir_pre        # Check, if the Server Component has been selected. Only display the directory page in this vase
!insertmacro MUI_UNPAGE_WELCOME
!define MUI_PAGE_CUSTOMFUNCTION_PRE un.completeClenupQuestion       # ask the user if he wants to do a complete cleanup
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
OutFile Release\setup-tve3.exe
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal TV Server"
InstallDirRegKey HKLM "${REGKEY}" InstallPath
CRCCheck on
XPStyle on
ShowInstDetails show
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName "MediaPortal TV Server"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductVersion "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyName "${COMPANY}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyWebsite "${URL}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileVersion "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription ""
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright ""
ShowUninstDetails show

#---------------------------------------------------------------------------
# SECTIONS and MACROS
#---------------------------------------------------------------------------
Section "MediaPortal TV Server" SecServer
    SetOverwrite on
    DetailPrint "Installing MediaPortal TV Server"
    
    ReadRegStr $InstallPath HKLM "${REGKEY}" InstallPath
    ${If} $InstallPath != ""
        #MessageBox MB_OKCANCEL|MB_ICONQUESTION "TV Server is already installed.$\r$\nPress 'OK' to overwrite the existing installation$\r$\nPress 'Cancel' to Abort the installation" /SD IDOK IDOK lbl_install IDCANCEL 0
        #DetailPrint "User pressed Cancel. Skipping installation"
        #Return
      #lbl_install:
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
    File ..\..\Filters\bin\hcwWinTVCI.dll
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
    SetOverwrite off
    SetOutPath "$CommonAppData"
    CreateDirectory "$CommonAppData\log"
    File ..\TvService\Gentle.config
    SetOverwrite on
    #---------------------------- End Of File Copy ----------------------  
    
    # Installing the TVService 
    DetailPrint "Installing TVService"
    ExecWait '"$INSTDIR\TVService.exe" /install'
    #!insertmacro InstallService
    DetailPrint "Finished Installing TVService"
    
    #---------------------------- Post Installation Tasks ----------------------
    WriteRegStr HKLM "${REGKEY}" InstallPath $INSTDIR

    SetOutPath $INSTDIR

    ${If} $noDesktopSC != 1
        CreateShortcut "$DESKTOP\MediaPortal TV Server.lnk" "$INSTDIR\SetupTV.exe" "" "$INSTDIR\SetupTV.exe" 0 "" "" "MediaPortal TV Server"
    ${EndIf}
    ${If} $noStartMenuSC != 1
        !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TV Server Logs.lnk" "$CommonAppData\log" "" "$CommonAppData\log" 0 "" "" "TV Server Log Files"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MediaPortal TV Server.lnk" "$INSTDIR\SetupTV.exe" "" "$INSTDIR\SetupTV.exe" 0 "" "" "MediaPortal TV Server"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MCE Blaster Learn.lnk" "$INSTDIR\Blaster.exe" "" "$INSTDIR\Blaster.exe" 0 "" "" "MCE Blaster Learn"
        !insertmacro MUI_STARTMENU_WRITE_END
    ${EndIf}
SectionEnd
!macro Remove_${SecServer}
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
    Delete /REBOOTOK $INSTDIR\hcwWinTVCI.dll
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
    #DeleteRegValue HKLM "${REGKEY}\Components" SecServer
    DeleteRegValue HKLM "${REGKEY}" InstallPath
    
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\MCE Blaster Learn.lnk"
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\MediaPortal TV Server.lnk"
    Delete /REBOOTOK "$SMPROGRAMS\$StartMenuGroup\MediaPortal TV Server Logs.lnk"
!macroend
 
Section "MediaPortal TV Plugin/Client" SecClient
    SetOverwrite on
    
    ReadRegSTR $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    
    DetailPrint "Installing MediaPortal TVPlugin"
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
    !insertmacro InstallLib REGDLL $LibInstall2 REBOOT_NOTPROTECTED ..\..\Filters\bin\mmaacd.ax $MPBaseDir\mmaacd.ax $MPBaseDir
SectionEnd
!macro Remove_${SecClient}
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
    !insertmacro UnInstallLib REGDLL SHARED REBOOT_NOTPROTECTED $MPBaseDir\mmaacd.ax
    
    Delete /REBOOTOK  $MPBaseDir\DVBSub2.ax
    Delete /REBOOTOK  $MPBaseDir\RtspSource.ax
    Delete /REBOOTOK  $MPBaseDir\TSFileSource.ax
    Delete /REBOOTOK  $MPBaseDir\TsReader.ax
    Delete /REBOOTOK  $MPBaseDir\mmaacd.ax
!macroend

!macro CompleteCleanup
    ;Place all commands in here to do a real cleanup and delete all files / data of tvserver

    #doing this is a high risk, imagine the user installs the application to Program Files, the uninstaller would try to remove the complete folder
    #RmDir /r /REBOOTOK $INSTDIR
    RmDir /r /REBOOTOK $CommonAppData

    DeleteRegKey HKLM "${REGKEY}"
    DeleteRegKey HKLM "${REG_UNINSTALL}"
!macroend
#####    End of Sections and macros

#####    Add/Remove callback functions
!macro SectionList MacroName
  ;This macro used to perform operation on multiple sections.
  ;List all of your components in following manner here.
 
  !insertmacro "${MacroName}" "SecServer"
  !insertmacro "${MacroName}" "SecClient"
!macroend
 
Section -FinishComponents
  ;Removes unselected components and writes component status to registry
  !insertmacro SectionList "FinishSection"
SectionEnd
 
Section -Post
    SetOverwrite on
    SetOutPath $INSTDIR
    
    ${If} $noStartMenuSC != 1
        !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk" $INSTDIR\uninstall-tve3.exe
        !insertmacro MUI_STARTMENU_WRITE_END
    ${EndIf}

    !ifdef VER_MAJOR & VER_MINOR & VER_REVISION & VER_BUILD
        WriteRegDword HKLM "${REGKEY}" "VersionMajor" "${VER_MAJOR}"
        WriteRegDword HKLM "${REGKEY}" "VersionMinor" "${VER_MINOR}"
        WriteRegDword HKLM "${REGKEY}" "VersionRevision" "${VER_REVISION}"
        WriteRegDword HKLM "${REGKEY}" "VersionBuild" "${VER_BUILD}"
    !endif

    # Write Uninstall Information
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName "$(^Name)"
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayVersion "${VERSION}"
    WriteRegStr HKLM "${REG_UNINSTALL}" Publisher "${COMPANY}"
    WriteRegStr HKLM "${REG_UNINSTALL}" URLInfoAbout "${URL}"
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon "$INSTDIR\mp.ico,0"
    WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString "$INSTDIR\uninstall-tve3.exe"
    WriteRegStr HKLM "${REG_UNINSTALL}" ModifyPath "$INSTDIR\add-remove-tve3.exe"
    WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 0
    WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 0
 
    CopyFiles "$EXEPATH" "$INSTDIR\add-remove-tve3.exe"
    WriteUninstaller $INSTDIR\uninstall-tve3.exe
SectionEnd
#####    End of Add/Remove callback functions

#####    Uninstaller sections
Section Uninstall
    ;First removes all optional components
    !insertmacro SectionList "RemoveSection"

    ;Removes directory and registry key:
    Delete /REBOOTOK "$INSTDIR\add-remove-tve3.exe"

    # Get the uninstall string, so that we can delete the exe
    ReadRegStr $R1 HKLM "${REG_UNINSTALL}" UninstallString
    Delete /REBOOTOK $R1
    DeleteRegKey HKLM "${REG_UNINSTALL}"

    #startmenu
    Delete "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk"
    RmDir "$SMPROGRAMS\$StartMenuGroup"
    DeleteRegValue HKLM "${REGKEY}" StartMenuGroup
    DeleteRegKey /IfEmpty HKLM "${REGKEY}"

    ${If} $CompleteCleanup == 1
        !insertmacro CompleteCleanup
    ${EndIf}
SectionEnd
#####    End of Uninstaller sections
 
Function .onInit
    #### check and parse cmdline parameter
    ; set default values for parameters ........
    strcpy $noClient 0
    strcpy $noServer 0
    strcpy $noDesktopSC 0
    strcpy $noStartMenuSC 0

    ; gets comandline parameter
    ${GetParameters} $R0

    ; check for special parameter and set the their variables
    ${GetOptions} $R0 "/noClient" $R1
    IfErrors +2
    strcpy $noClient 1
    ${GetOptions} $R0 "/noServer" $R1
    IfErrors +2
    strcpy $noServer 1
    ${GetOptions} $R0 "/noDesktopSC" $R1
    IfErrors +2
    strcpy $noDesktopSC 1
    ${GetOptions} $R0 "/noStartMenuSC" $R1
    IfErrors +2
    strcpy $noStartMenuSC 1
    #### END of check and parse cmdline parameter

    ;Reads components status for registry
    !insertmacro SectionList "InitSection"
    
    ;update the component status with infos from commandline parameters
    ${If} $noClient = 1
    ${AndIf} $noServer = 1
        MessageBox MB_OK|MB_ICONEXCLAMATION "You have done something wrong!$\r$\nIt is not allowed to use 'noClient' & 'noServer' at the same time." IDOK 0
        Quit
    ${ElseIf} $noClient = 1
        #MessageBox MB_OK|MB_ICONEXCLAMATION "SecClient IDOK 0"
        !insertmacro SelectSection ${SecServer}
        !insertmacro UnselectSection ${SecClient}
    ${ElseIf} $noServer = 1
        #MessageBox MB_OK|MB_ICONEXCLAMATION "SecServer IDOK 0"
        !insertmacro SelectSection ${SecClient}
        !insertmacro UnselectSection ${SecServer}
    ${EndIf}

    ; if silent and tve3 is already installed, remove it first, the continue with installation
    IfSilent 0 noSilent

        #MessageBox MB_YESNO|MB_ICONEXCLAMATION "xxxxx" IDYES 0 IDNO 0
        ReadRegStr $R1 HKLM "${REG_UNINSTALL}" "UninstallString"
        IfFileExists '$R1' 0 noSilent

        ClearErrors
        #MessageBox MB_YESNO|MB_ICONEXCLAMATION "xxxxx" IDYES 0 IDNO 0
        ExecWait '$R1 /S _?=$INSTDIR'

    noSilent:

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

Function un.onInit
    #### check and parse cmdline parameter
    ; set default values for parameters ........
    strcpy $CompleteCleanup 0

    ; gets comandline parameter
    ${un.GetParameters} $R0

    ; check for special parameter and set the their variables
    ${un.GetOptions} $R0 "/CompleteCleanup" $R1
    IfErrors +2
    strcpy $CompleteCleanup 1
    #### END of check and parse cmdline parameter


    ReadRegStr $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    ReadRegStr $INSTDIR HKLM "${REGKEY}" InstallPath
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

    ; Get the Common Application Data Folder
    ; Set the Context to alll, so that we get the All Users folder
    SetShellVarContext all
    StrCpy $CommonAppData "$APPDATA\MediaPortal TV Server"
    ; Context back to current user
    SetShellVarContext current
FunctionEnd

#####    other functions
; Start the Setup after the successfull install
; needed in an extra function to set the working directory
Function RunSetup
SetOutPath $INSTDIR
Exec "$INSTDIR\SetupTV.exe"
FunctionEnd

Function .onSelChange
    ; disable the next button if nothing is selected
    Push $0
    Push $1
    SectionGetFlags ${SecServer} $0
    IntOp $0 ${SF_SELECTED} & $0
    SectionGetFlags ${SecClient} $1
    IntOp $1 ${SF_SELECTED} & $1
    IntOp $0 $1 | $0
    GetDlgItem $1 $HWNDPARENT 1
    EnableWindow $1 $0
    Pop $1
    Pop $0
FunctionEnd

Function DisableClientIfNoMP
    ReadRegStr $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    
    ${If} $MPBaseDir == ""
        !insertmacro UnselectSection "${SecClient}"
        # Make the unselected section read only
        !insertmacro SetSectionFlag "${SecClient}" 16
        SectionGetText ${SecClient} $0
        StrCpy $0 "$0 ($(TEXT_MP_NOT_INSTALLED))"
        SectionSetText ${SecClient} $0
    ${EndIf}
FunctionEnd

# This function is called, before the Directory Page is displayed
# It checks, if the Server has been selected and only displays the Directory page in this case
Function dir_pre
         ${If} ${SectionIsSelected} SecServer
            strcpy $0 1
         ${Else}
            strcpy $0 2
            abort
         ${EndIf}
FunctionEnd

# This function is called, before the Uninstall Confirmation Page is displayed
# It checks, if the Server has been selected and only displays the Directory page in this case
Function un.dir_pre
         ${If} ${SectionIsSelected} UNSecServer
            strcpy $0 1
         ${Else}
            strcpy $0 2
            abort
         ${EndIf}
FunctionEnd

# This function is called, before the Finish Page is displayed
# It checks, if the Server has been selected and only displays the Directory page in this case
Function finish_pre
         ${If} ${SectionIsSelected} SecServer
            strcpy $0 1
         ${Else}
            strcpy $0 2
            abort
         ${EndIf}
FunctionEnd

# This function is called, before the uninstallation process is startet
# It asks the user, if he wants to do a complete cleanup
Function un.completeClenupQuestion
    MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_COMPLETE_CLEANUP)" IDYES 0 IDNO end
    strcpy $CompleteCleanup 1
    
    end:
FunctionEnd
#####    End of other functions

#####    Installer Language Strings
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} $(DESC_SECClient)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} $(DESC_SECServer)
!insertmacro MUI_FUNCTION_DESCRIPTION_END