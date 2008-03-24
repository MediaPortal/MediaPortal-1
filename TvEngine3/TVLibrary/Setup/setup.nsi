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
Name "MediaPortal TV Server / Client"
SetCompressor /SOLID lzma
RequestExecutionLevel admin

#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
Var StartMenuGroup  ; Holds the Startmenu\Programs folder
Var MPBaseDir
Var InstallPath
; variables for commandline parameters for Installer
Var noClient
Var noServer
Var noDesktopSC
Var noStartMenuSC
; variables for commandline parameters for UnInstaller
Var RemoveAll       ; Set, when the user decided to uninstall everything

#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"

!define REG_UNINSTALL         "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
!define MP_REG_UNINSTALL      "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!define MEMENTO_REGISTRY_ROOT HKLM
!define MEMENTO_REGISTRY_KEY  "${REG_UNINSTALL}"
# needs to be changed if we want to edit the appdata path
#!define COMMON_APPDATA        "$APPDATA\Team MediaPortal\TV Server"
!define COMMON_APPDATA        "$APPDATA\MediaPortal TV Server"

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
BrandingText "TV Server ${VERSION} by Team MediaPortal"

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
!include setup-RememberSections.nsh
!include setup-languages.nsh

!insertmacro GetParameters
!insertmacro GetOptions
!insertmacro un.GetParameters
!insertmacro un.GetOptions
!insertmacro GetParent

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
!define MUI_STARTMENUPAGE_DEFAULTFOLDER       "Team MediaPortal\TV Server"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT       HKLM
!define MUI_STARTMENUPAGE_REGISTRY_KEY        "${REG_UNINSTALL}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN      "$INSTDIR\SetupTV.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run TV-Server Configuration"

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
#!define MUI_PAGE_CUSTOMFUNCTION_LEAVE WelcomeLeave
!insertmacro MUI_PAGE_WELCOME
Page custom PageReinstall PageLeaveReinstall
#!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
!define MUI_PAGE_CUSTOMFUNCTION_PRE ComponentsPre       #check, if MediaPortal is installed, if not uncheck and disable the ClientPluginSection
!insertmacro MUI_PAGE_COMPONENTS
!define MUI_PAGE_CUSTOMFUNCTION_PRE DirectoryPre        # Check, if the Server Component has been selected. Only display the directory page in this vase
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!define MUI_PAGE_CUSTOMFUNCTION_SHOW FinishShow           # Check, if the Server Component has been selected. Only display the Startmenu page in this vase
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
OutFile "Release\setup-tve3.exe"
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal TV Server"
InstallDirRegKey HKLM "${REG_UNINSTALL}" InstallPath
CRCCheck on
XPStyle on
ShowInstDetails show
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName       "MediaPortal TV Server"
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
    !insertmacro "${MacroName}" "SecServer"
    !insertmacro "${MacroName}" "SecClient"
!macroend

#---------------------------------------------------------------------------
# SECTIONS and REMOVEMACROS
#---------------------------------------------------------------------------
${MementoSection} "MediaPortal TV Server" SecServer
    DetailPrint "Installing MediaPortal TV Server..."

    SetOverwrite on

    ReadRegStr $InstallPath HKLM "${REG_UNINSTALL}" InstallPath
    ${If} $InstallPath != ""
        DetailPrint "Uninstalling TVService"
        ExecWait '"$InstallPath\TVService.exe" /uninstall'
        DetailPrint "Finished uninstalling TVService"
    ${EndIf}

    Pop $0

    #---------------------------- File Copy ----------------------
    ; Tuning Parameter Directory
    SetOutPath $INSTDIR\TuningParameters
    File /r /x .svn ..\TvService\bin\Release\TuningParameters\*

    ; The Plugin Directory
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

    ; Rest of Files
    SetOutPath $INSTDIR
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

    ; 3rd party assemblys
    File ..\..\Filters\bin\dxerr9.dll
    File ..\..\Filters\bin\hauppauge.dll
    File ..\..\Filters\bin\hcwWinTVCI.dll
    File ..\..\Filters\bin\KNCBDACTRL.dll
    File ..\..\Filters\bin\StreamingServer.dll
    File ..\..\Filters\bin\ttBdaDrvApi_Dll.dll
    File ..\..\Filters\bin\ttdvbacc.dll

    ; Common App Data Files
    SetOutPath "${COMMON_APPDATA}"
    File ..\TvService\Gentle.config

    #---------------------------------------------------------------------------
    # FILTER REGISTRATION   for TVServer
    #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
    #---------------------------------------------------------------------------
    DetailPrint "filter registration..."
    ; filters for digital tv


    ReadRegStr $MPBaseDir HKLM "${MP_REG_UNINSTALL}" "InstallPath"

  ${If} $MPBaseDir == ""
    # this fallback should only be enabled until MediaPortal 1.0 is out
    ReadRegStr $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"

    ${If} $MPBaseDir == ""
      !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED ..\..\Filters\bin\TsReader.ax $INSTDIR\TsReader.ax $INSTDIR
    ${EndIf}
  ${EndIf}
    
    
    
    
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED ..\..\Filters\bin\TsWriter.ax $INSTDIR\TsWriter.ax $INSTDIR
    ; filters for analog tv
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED ..\..\Filters\bin\mpFileWriter.ax $INSTDIR\mpFileWriter.ax $INSTDIR
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED ..\..\Filters\bin\PDMpgMux.ax $INSTDIR\PDMpgMux.ax $INSTDIR

    #---------------------------------------------------------------------------
    # SERVICE INSTALLATION
    #---------------------------------------------------------------------------
    DetailPrint "Installing TVService"
    ExecWait '"$INSTDIR\TVService.exe" /install'
    DetailPrint "Finished Installing TVService"

    SetOutPath $INSTDIR

    ${If} $noDesktopSC != 1
        CreateShortcut "$DESKTOP\TV-Server Configuration.lnk" "$INSTDIR\SetupTV.exe" "" "$INSTDIR\SetupTV.exe" 0 "" "" "MediaPortal TV Server"
    ${EndIf}

    ${If} $noStartMenuSC != 1
        !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
        ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
        CreateDirectory "$SMPROGRAMS\$StartMenuGroup"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\TV-Server Configuration.lnk" "$INSTDIR\SetupTV.exe"  "" "$INSTDIR\SetupTV.exe"  0 "" "" "TV-Server Configuration"
        CreateDirectory "${COMMON_APPDATA}\log"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\TV-Server Log-Files.lnk"     "${COMMON_APPDATA}\log" "" "${COMMON_APPDATA}\log" 0 "" "" "TV-Server Log-Files"
    # [OBSOLETE] CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MCE Blaster Learn.lnk" "$INSTDIR\Blaster.exe" "" "$INSTDIR\Blaster.exe" 0 "" "" "MCE Blaster Learn"
        !insertmacro MUI_STARTMENU_WRITE_END
    ${EndIf}
${MementoSectionEnd}
!macro Remove_${SecServer}
    DetailPrint "Uninstalling MediaPortal TV Server..."

    #---------------------------------------------------------------------------
    # SERVICE UNINSTALLATION
    #---------------------------------------------------------------------------
    DetailPrint "DeInstalling TVService"
    ExecWait '"$INSTDIR\TVService.exe" /uninstall'
    DetailPrint "Finished DeInstalling TVService"

    #---------------------------------------------------------------------------
    # FILTER UNREGISTRATION     for TVServer
    #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
    #---------------------------------------------------------------------------
    DetailPrint "Unreg and remove filters..."
    ; filters for digital tv
    ReadRegStr $MPBaseDir HKLM "${MP_REG_UNINSTALL}" "InstallPath"

  ${If} $MPBaseDir == ""
    # this fallback should only be enabled until MediaPortal 1.0 is out
    ReadRegStr $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"

    ${If} $MPBaseDir == ""
      !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\TsReader.ax
    ${EndIf}
  ${EndIf}
  
  
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\TsWriter.ax
    ; filters for analog tv
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\mpFileWriter.ax
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\PDMpgMux.ax

    DetailPrint "remove files..."
    ; Remove TuningParameters
    RmDir /r /REBOOTOK $INSTDIR\TuningParameters

    ; Remove Plugins
    Delete /REBOOTOK $INSTDIR\Plugins\ComSkipLauncher.dll
    Delete /REBOOTOK $INSTDIR\Plugins\ConflictsManager.dll
    Delete /REBOOTOK $INSTDIR\Plugins\PersonalTVGuide.dll
    Delete /REBOOTOK $INSTDIR\Plugins\PluginBase.dll
    Delete /REBOOTOK $INSTDIR\Plugins\PowerScheduler.dll
    Delete /REBOOTOK $INSTDIR\Plugins\PowerScheduler.Interfaces.dll
    Delete /REBOOTOK $INSTDIR\Plugins\ServerBlaster.dll
    Delete /REBOOTOK $INSTDIR\Plugins\TvMovie.dll
    Delete /REBOOTOK $INSTDIR\Plugins\XmlTvImport.dll
    RmDir "$INSTDIR\Plugins"
    
    ; And finally remove all the files installed
    ; Leave the directory in place, as it might contain user modified files
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

    ; 3rd party assemblys
    Delete /REBOOTOK $INSTDIR\dxerr9.dll
    Delete /REBOOTOK $INSTDIR\hauppauge.dll
    Delete /REBOOTOK $INSTDIR\hcwWinTVCI.dll
    Delete /REBOOTOK $INSTDIR\KNCBDACTRL.dll
    Delete /REBOOTOK $INSTDIR\StreamingServer.dll
    Delete /REBOOTOK $INSTDIR\ttBdaDrvApi_Dll.dll
    Delete /REBOOTOK $INSTDIR\ttdvbacc.dll

    ; remove Start Menu shortcuts
    Delete "$SMPROGRAMS\$StartMenuGroup\TV-Server Configuration.lnk"
    Delete "$SMPROGRAMS\$StartMenuGroup\TV-Server Log-Files.lnk"
    # [OBSOLETE] Delete "$SMPROGRAMS\$StartMenuGroup\MCE Blaster Learn.lnk"
    ; remove Desktop shortcuts
    Delete "$DESKTOP\TV-Server Configuration.lnk"
!macroend

${MementoSection} "MediaPortal TV Client plugin" SecClient
    DetailPrint "Installing MediaPortal TV Client plugin..."

    SetOverwrite on

    ReadRegSTR $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    DetailPrint "MediaPortal Installed at: $MpBaseDir"

    #---------------------------- File Copy ----------------------
    ; Common Files
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

    ; The Plugins
    SetOutPath $MPBaseDir\Plugins\Process
    File ..\Plugins\PowerScheduler\ClientPlugin\bin\Release\PowerSchedulerClientPlugin.dll
    SetOutPath $MPBaseDir\Plugins\Windows
    File ..\TvPlugin\TvPlugin\bin\Release\TvPlugin.dll

    #---------------------------------------------------------------------------
    # FILTER REGISTRATION       for TVClient
    #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
    #---------------------------------------------------------------------------
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED ..\..\Filters\bin\DVBSub2.ax $MPBaseDir\DVBSub2.ax $MPBaseDir
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED ..\..\Filters\bin\mmaacd.ax $MPBaseDir\mmaacd.ax $MPBaseDir
${MementoSectionEnd}
!macro Remove_${SecClient}
    DetailPrint "Uninstalling MediaPortal TV Client plugin..."

    #---------------------------------------------------------------------------
    # FILTER UNREGISTRATION     for TVClient
    #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
    #---------------------------------------------------------------------------
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $MPBaseDir\DVBSub2.ax
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $MPBaseDir\mmaacd.ax

    ; The Plugins
    Delete /REBOOTOK $MPBaseDir\Plugins\Process\PowerSchedulerClientPlugin.dll
    Delete /REBOOTOK $MPBaseDir\Plugins\Windows\TvPlugin.dll

    ; Common Files
    Delete /REBOOTOK $MPBaseDir\PowerScheduler.Interfaces.dll
    Delete /REBOOTOK $MPBaseDir\TvControl.dll
    Delete /REBOOTOK $MPBaseDir\TVDatabase.dll
    Delete /REBOOTOK $MPBaseDir\Gentle.Common.DLL
    Delete /REBOOTOK $MPBaseDir\Gentle.Framework.DLL
    Delete /REBOOTOK $MPBaseDir\Gentle.Provider.MySQL.dll
    Delete /REBOOTOK $MPBaseDir\Gentle.Provider.SQLServer.dll
    Delete /REBOOTOK $MPBaseDir\log4net.dll
    Delete /REBOOTOK $MPBaseDir\MySql.Data.dll
    Delete /REBOOTOK $MPBaseDir\TvBusinessLayer.dll
    Delete /REBOOTOK $MPBaseDir\TvLibrary.Interfaces.dll
    Delete /REBOOTOK $MPBaseDir\Gentle.config
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

    ${If} $noStartMenuSC != 1
        !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
        ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
        CreateDirectory "$SMPROGRAMS\$StartMenuGroup"
        CreateShortcut "$SMPROGRAMS\$StartMenuGroup\uninstall TV-Server.lnk" "$INSTDIR\uninstall-tve3.exe"
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
    WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon        "$INSTDIR\mp.ico,0"
    WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString    "$INSTDIR\uninstall-tve3.exe"
    WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 1
    WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 1
 
    WriteUninstaller "$INSTDIR\uninstall-tve3.exe"
SectionEnd

#---------------------------------------------------------------------------
# This section is called on uninstall and removes all components
Section Uninstall
    ;First removes all optional components
    !insertmacro SectionList "RemoveSection"

    ; remove registry key
    DeleteRegKey HKLM "${REG_UNINSTALL}"

    ; remove Start Menu shortcuts
    Delete "$SMPROGRAMS\$StartMenuGroup\uninstall TV-Server.lnk"
    Delete "$SMPROGRAMS\$StartMenuGroup\web site.url"
    RmDir "$SMPROGRAMS\$StartMenuGroup"

    ; remove last files and instdir
    RmDir /REBOOTOK "$INSTDIR\pmt"
    Delete /REBOOTOK "$INSTDIR\uninstall-tve3.exe"
    RmDir "$INSTDIR"

    ${If} $RemoveAll == 1
        DetailPrint "Removing User Settings"
        RmDir /r /REBOOTOK "${COMMON_APPDATA}"
        RmDir /r /REBOOTOK $INSTDIR
    ${EndIf}
SectionEnd

#---------------------------------------------------------------------------
# FUNCTIONS
#---------------------------------------------------------------------------
Function .onInit
    #### check and parse cmdline parameter
    ; set default values for parameters ........
    StrCpy $noClient 0
    StrCpy $noServer 0
    StrCpy $noDesktopSC 0
    StrCpy $noStartMenuSC 0

    ; gets comandline parameter
    ${GetParameters} $R0

    ; check for special parameter and set the their variables
    ClearErrors
    ${GetOptions} $R0 "/noClient" $R1
    IfErrors +2
    StrCpy $noClient 1

    ClearErrors
    ${GetOptions} $R0 "/noServer" $R1
    IfErrors +2
    StrCpy $noServer 1

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
    ${If} $noClient = 1
    ${AndIf} $noServer = 1
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_PARAMETER_ERROR)" IDOK 0
        Abort
    ${ElseIf} $noClient = 1
        !insertmacro SelectSection ${SecServer}
        !insertmacro UnselectSection ${SecClient}
    ${ElseIf} $noServer = 1
        !insertmacro SelectSection ${SecClient}
        !insertmacro UnselectSection ${SecServer}
    ${EndIf}

    ; check if minimum Windows version is XP
    ${If} ${AtMostWin2000}
        MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)"
        Abort
    ${EndIf}

    ; check if old msi based client plugin is installed.
    ClearErrors
    ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}" "UninstallString"
    ${IfNot} ${Errors}
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_MSI_CLIENT)" IDOK 0
        Abort
    ${EndIf}

    ; check if old msi based server is installed.
    ClearErrors
    ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{4B738773-EE07-413D-AFB7-BB0AB04A5488}" "UninstallString"
    ${IfNot} ${Errors}
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_MSI_SERVER)" IDOK 0
        Abort
    ${EndIf}

    ; check if reboot is required
    ${If} ${FileExists} "$INSTDIR\rebootflag"
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)" IDOK 0
        Abort
    ${EndIf}

    ; if silent and tve3 is already installed, remove it first, the continue with installation
    ${If} ${Silent}
        ReadRegStr $R0 HKLM "${REG_UNINSTALL}" "UninstallString"
        ${If} ${FileExists} '$R0'
            ${GetParent} $R0 $R1
            ; start uninstallation of installed MP, from tmp folder, so it will delete itself
            ClearErrors
            CopyFiles $R0 "$TEMP\uninstall-tve3.exe"
            ExecWait '"$TEMP\uninstall-tve3.exe" /S _?=$R1'

            ; if an error occured, ask to cancel installation
            #${If} ${Errors}
            #    MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_ON_UNINSTALL)" /SD IDNO IDYES +2 IDNO 0
            #    Quit
            #${EndIf}
        ${EndIf}
    ${EndIf}

    SetShellVarContext all
FunctionEnd

Function .onSelChange
  ; disable the next button if nothing is selected
  ${IfNot} ${SectionIsSelected} ${SecServer}
  ${AndIfNot} ${SectionIsSelected} ${SecClient}
    EnableWindow $mui.Button.Next 0
  ${Else}
    EnableWindow $mui.Button.Next 1
  ${EndIf}
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

  ReadRegStr $MPBaseDir HKLM "${MP_REG_UNINSTALL}" "InstallPath"
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
        CopyFiles $R0 "$TEMP\uninstall-tve3.exe"
        ExecWait '"$TEMP\uninstall-tve3.exe" _?=$R1'
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
Function ComponentsPre
    ReadRegStr $MPBaseDir HKLM "${MP_REG_UNINSTALL}" "InstallPath"

  ${If} $MPBaseDir == ""
    # this fallback should only be enabled until MediaPortal 1.0 is out
    ReadRegStr $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"

    ${If} $MPBaseDir == ""
        !insertmacro UnselectSection "${SecClient}"
        ; Make the unselected section read only
        !insertmacro SetSectionFlag "${SecClient}" 16
        SectionGetText ${SecClient} $R0
        SectionSetText ${SecClient} "$R0 ($(TEXT_MP_NOT_INSTALLED))"
    ${EndIf}
  ${EndIf}
FunctionEnd

Function DirectoryPre
    ; This function is called, before the Directory Page is displayed

    ; It checks, if the Server has been selected and only displays the Directory page in this case
    ${IfNot} ${SectionIsSelected} SecServer
        Abort
    ${EndIf}
FunctionEnd

Function FinishShow
    ; This function is called, after the Finish Page creation is finished

    ; It checks, if the Server has been selected and only displays the run checkbox in this case
    ${IfNot} ${SectionIsSelected} SecServer
        SendMessage $mui.FinishPage.Run ${BM_CLICK} 0 0
        ShowWindow  $mui.FinishPage.Run ${SW_HIDE}
    ${EndIf}
FunctionEnd

Function un.WelcomeLeave
    ; This function is called, before the uninstallation process is startet

    ; It asks the user, if he wants to remove all files and settings
    StrCpy $RemoveAll 0
    MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "$(TEXT_MSGBOX_REMOVE_ALL)" IDYES 0 IDNO noRemoveAll
    MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "$(TEXT_MSGBOX_REMOVE_ALL_STUPID)" IDYES 0 IDNO noRemoveAll
    StrCpy $RemoveAll 1

    noRemoveAll:

FunctionEnd

#---------------------------------------------------------------------------
# SECTION DECRIPTIONS     must be at the end
#---------------------------------------------------------------------------
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} $(DESC_SecClient)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} $(DESC_SecServer)
!insertmacro MUI_FUNCTION_DESCRIPTION_END