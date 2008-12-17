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


# DEFINES
!define svn_ROOT "..\..\.."
!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"

#**********************************************************************************************************#
#
#   For building the installer on your own you need:
#       1. Lastest NSIS version from http://nsis.sourceforge.net/Download
#       2. The xml-plugin from http://nsis.sourceforge.net/XML_plug-in
#
#**********************************************************************************************************#
Name "MediaPortal TV Server / Client"
SetCompressor /SOLID lzma

!define TVSERVER.BASE "..\TVServer.Base"
!define MEDIAPORTAL.FILTERBIN "..\..\..\DirectShowFilters\bin\Release"
!define MEDIAPORTAL.BASE "..\..\..\mediaportal\MediaPortal.Base"

!define BUILD_TYPE "Release"
;!define BUILD_TYPE "Debug"

#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
Var StartMenuGroup  ; Holds the Startmenu\Programs folder
Var InstallPath
; variables for commandline parameters for Installer
Var noClient
Var noServer
Var noDesktopSC
Var noStartMenuSC
Var DeployMode
; variables for commandline parameters for UnInstaller
Var RemoveAll       ; Set, when the user decided to uninstall everything

#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"

!define WEB_REQUIREMENTS "http://wiki.team-mediaportal.com/GeneralRequirements/OperatingSystems"


!define REG_UNINSTALL         "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
!define MP_REG_UNINSTALL      "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!define MEMENTO_REGISTRY_ROOT HKLM
!define MEMENTO_REGISTRY_KEY  "${REG_UNINSTALL}"
!define COMMON_APPDATA        "$APPDATA\Team MediaPortal\MediaPortal TV Server"

!define VER_MAJOR       1
!define VER_MINOR       0
!define VER_REVISION    0
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif
!if ${VER_BUILD} == 0       # it's a stable release
    !define VERSION "1.0"
!else                       # it's an svn reöease
    !define VERSION "1.0 SVN build ${VER_BUILD} for TESTING ONLY"
!endif
BrandingText "$(^Name) ${VERSION} by ${COMPANY}"

!define INSTALL_LOG

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include LogicLib.nsh
!include Library.nsh
!include FileFunc.nsh
;!include WinVer.nsh
!define WinVer++
!include Memento.nsh

!include "${svn_InstallScripts}\include-AddRemovePage.nsh"
!include "${svn_InstallScripts}\include-CommonMPMacros.nsh"
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
!define MUI_ICON    "Resources\install.ico"
!define MUI_UNICON  "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP              "Resources\header.bmp"
!if ${VER_BUILD} == 0       # it's a stable release
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "Resources\wizard.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "Resources\wizard.bmp"
!else                       # it's an svn reöease
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "Resources\wizard-svn.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "Resources\wizard-svn.bmp"
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
#!define MUI_FINISHPAGE_SHOWREADME $INSTDIR\readme.txt
#!define MUI_FINISHPAGE_SHOWREADME_TEXT "View Readme"
#!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_LINK "Donate to MediaPortal"
!define MUI_FINISHPAGE_LINK_LOCATION "http://www.team-mediaportal.com/donate.html"

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
#!define MUI_PAGE_CUSTOMFUNCTION_LEAVE WelcomeLeave
!insertmacro MUI_PAGE_WELCOME
Page custom PageReinstall PageLeaveReinstall

!ifndef SVN_BUILD
#!insertmacro MUI_PAGE_LICENSE "..\Docs\license.rtf"
!else
#!insertmacro MUI_PAGE_LICENSE "..\Docs\svn-info.rtf"
!endif

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
!if ${VER_BUILD} == 0
  OutFile "Release\package-tvengine.exe"
!else
  OutFile "Release\setup-tve3.exe"
!endif
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal TV Server"
InstallDirRegKey HKLM "${REG_UNINSTALL}" InstallPath
CRCCheck on
XPStyle on
RequestExecutionLevel admin
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
  ${LOG_TEXT} "DEBUG" "MACRO SectionList ${MacroName}"
  ; This macro used to perform operation on multiple sections.
  ; List all of your components in following manner here.
  !insertmacro "${MacroName}" "SecServer"
  !insertmacro "${MacroName}" "SecClient"
!macroend

#---------------------------------------------------------------------------
# SECTIONS and REMOVEMACROS
#---------------------------------------------------------------------------
!if ${VER_BUILD} == 0       # it's an official release (stable or release candidate)
Section "-prepare" SecPrepare
  ${LOG_TEXT} "DEBUG" "SECTION SecPrepare"
  ${LOG_TEXT} "INFO" "Prepare installation..."
  SetShellVarContext all

  !insertmacro GET_BACKUP_POSTFIX $R0

  ${If} $DeployMode == 1
  ${AndIf} $noServer == 1   ; this means the client is being installed, tv server was installed before, so the instdir shouldn't be renamed again       noClient

    ${LOG_TEXT} "INFO" "!!!!!!!!!!!! the client is being installed, tv server was installed before, so the instdir shouldn't be renamed again"

  ${Else}

    ${If} ${FileExists} "${COMMON_APPDATA}\*.*"
      ${LOG_TEXT} "INFO" "Configuration dir already exists. It will be renamed."
      Rename "${COMMON_APPDATA}" "${COMMON_APPDATA}_$R0"
    ${EndIf}

  ${EndIf}

SectionEnd
!endif

${MementoSection} "MediaPortal TV Server" SecServer
  ${LOG_TEXT} "DEBUG" "MementoSection SecServer"
  ${LOG_TEXT} "INFO" "Installing MediaPortal TV Server..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${KILLPROCESS} "TVService.exe"
  ${KILLPROCESS} "SetupTv.exe"

  SetOverwrite on

  ReadRegStr $InstallPath HKLM "${REG_UNINSTALL}" InstallPath
  ${If} $InstallPath != ""
    ${LOG_TEXT} "INFO" "Uninstalling TVService"
    ExecWait '"$InstallPath\TVService.exe" /uninstall'
    ${LOG_TEXT} "INFO" "Finished uninstalling TVService"
  ${EndIf}

  Pop $0

  #---------------------------- File Copy ----------------------
  ; Tuning Parameter Directory
  SetOutPath $INSTDIR\TuningParameters
  File /r /x .svn "${TVSERVER.BASE}\TuningParameters\*"
  File "${MEDIAPORTAL.BASE}\TuningParameters\*.dvbc"
  File "${MEDIAPORTAL.BASE}\TuningParameters\dvbt.xml"

  ; The Plugin Directory
  SetOutPath $INSTDIR\Plugins
  File ..\Plugins\ComSkipLauncher\bin\Release\ComSkipLauncher.dll
  File ..\Plugins\ConflictsManager\bin\Release\ConflictsManager.dll
  # removed it because it is not working like it should
  #File ..\Plugins\PersonalTVGuide\bin\Release\PersonalTVGuide.dll
  File ..\Plugins\PowerScheduler\bin\Release\PowerScheduler.dll
  File ..\Plugins\ServerBlaster\ServerBlaster\bin\Release\ServerBlaster.dll
  File ..\Plugins\TvMovie\bin\Release\TvMovie.dll
  File ..\Plugins\XmlTvImport\bin\Release\XmlTvImport.dll

  ; Rest of Files
  SetOutPath $INSTDIR
  File ..\DirectShowLib\bin\Release\DirectShowLib.dll
  File ..\Plugins\PluginBase\bin\Release\PluginBase.dll
  File ..\Plugins\PowerScheduler\PowerScheduler.Interfaces\bin\Release\PowerScheduler.Interfaces.dll
  File "..\Plugins\ServerBlaster\ServerBlaster (Learn)\bin\Release\Blaster.exe"
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
  File ..\TvService\bin\Release\TvService.exe
  File ..\TvService\bin\Release\TvService.exe.config
  File ..\SetupControls\bin\Release\SetupControls.dll

  ; 3rd party assemblys
  File "${TVSERVER.BASE}\dvblib.dll"
  File "${TVSERVER.BASE}\dxerr9.dll"
  File "${TVSERVER.BASE}\hauppauge.dll"
  File "${TVSERVER.BASE}\hcwWinTVCI.dll"
  File "${TVSERVER.BASE}\KNCBDACTRL.dll"
  File "${TVSERVER.BASE}\ttBdaDrvApi_Dll.dll"
  File "${TVSERVER.BASE}\ttdvbacc.dll"

  File "${MEDIAPORTAL.FILTERBIN}\StreamingServer.dll"

  ; Common App Data Files
  SetOutPath "${COMMON_APPDATA}"
  File "..\TvService\Gentle.config"
  File "${TVSERVER.BASE}\HelpReferences.xml"

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION   for TVServer
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  ${LOG_TEXT} "INFO" "filter registration..."
  ; filters for digital tv
  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TsReader.ax" "$INSTDIR\TsReader.ax" "$INSTDIR"
  ${EndIf}
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\TsWriter.ax" "$INSTDIR\TsWriter.ax" "$INSTDIR"
  ; filters for analog tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\mpFileWriter.ax" "$INSTDIR\mpFileWriter.ax" "$INSTDIR"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\PDMpgMux.ax" "$INSTDIR\PDMpgMux.ax" "$INSTDIR"

  #---------------------------------------------------------------------------
  # SERVICE INSTALLATION
  #---------------------------------------------------------------------------
  ${LOG_TEXT} "INFO" "Installing TVService"
  ExecWait '"$INSTDIR\TVService.exe" /install'
  ${LOG_TEXT} "INFO" "Finished Installing TVService"

  SetOutPath $INSTDIR
  ${If} $noDesktopSC != 1
    CreateShortCut "$DESKTOP\TV-Server Configuration.lnk" "$INSTDIR\SetupTV.exe" "" "$INSTDIR\SetupTV.exe" 0 "" "" "MediaPortal TV Server"
  ${EndIf}

  ${If} $noStartMenuSC != 1
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
    CreateDirectory "$SMPROGRAMS\$StartMenuGroup"
    CreateShortCut "$SMPROGRAMS\$StartMenuGroup\TV-Server Configuration.lnk" "$INSTDIR\SetupTV.exe"  "" "$INSTDIR\SetupTV.exe"  0 "" "" "TV-Server Configuration"
    CreateDirectory "${COMMON_APPDATA}\log"
    CreateShortCut "$SMPROGRAMS\$StartMenuGroup\TV-Server Log-Files.lnk"     "${COMMON_APPDATA}\log" "" "${COMMON_APPDATA}\log" 0 "" "" "TV-Server Log-Files"
    # [OBSOLETE] CreateShortcut "$SMPROGRAMS\$StartMenuGroup\MCE Blaster Learn.lnk" "$INSTDIR\Blaster.exe" "" "$INSTDIR\Blaster.exe" 0 "" "" "MCE Blaster Learn"
    !insertmacro MUI_STARTMENU_WRITE_END
  ${EndIf}
${MementoSectionEnd}
!macro Remove_${SecServer}
  ${LOG_TEXT} "DEBUG" "MACRO Remove_${SecServer}"
  ${LOG_TEXT} "INFO" "Uninstalling MediaPortal TV Server..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${KILLPROCESS} "TVService.exe"
  ${KILLPROCESS} "SetupTv.exe"

  #---------------------------------------------------------------------------
  # CLEARING DATABASE if RemoveAll was selected
  #---------------------------------------------------------------------------
  ${If} $RemoveAll == 1
    ExecWait '"$INSTDIR\SetupTv.exe" --delete-db'
  ${EndIf}

  #---------------------------------------------------------------------------
  # SERVICE UNINSTALLATION
  #---------------------------------------------------------------------------
  ${LOG_TEXT} "INFO" "DeInstalling TVService"
  ExecWait '"$INSTDIR\TVService.exe" /uninstall'
  ${LOG_TEXT} "INFO" "Finished DeInstalling TVService"

  #---------------------------------------------------------------------------
  # FILTER UNREGISTRATION     for TVServer
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  ${LOG_TEXT} "INFO" "Unreg and remove filters..."
  ; filters for digital tv
  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\TsReader.ax
    WriteRegStr HKCR "Media Type\Extensions\.ts"        "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
    WriteRegStr HKCR "Media Type\Extensions\.tp"        "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
    WriteRegStr HKCR "Media Type\Extensions\.tsbuffer"  "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
    WriteRegStr HKCR "Media Type\Extensions\.rtsp"      "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
  ${EndIf}
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\TsWriter.ax
  ; filters for analog tv
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\mpFileWriter.ax
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED $INSTDIR\PDMpgMux.ax

  ${LOG_TEXT} "INFO" "remove files..."
  ; Remove TuningParameters
  RMDir /r /REBOOTOK $INSTDIR\TuningParameters

  ; Remove Plugins
  Delete /REBOOTOK $INSTDIR\Plugins\ComSkipLauncher.dll
  Delete /REBOOTOK $INSTDIR\Plugins\ConflictsManager.dll
  #Delete /REBOOTOK $INSTDIR\Plugins\PersonalTVGuide.dll
  Delete /REBOOTOK $INSTDIR\Plugins\PowerScheduler.dll
  Delete /REBOOTOK $INSTDIR\Plugins\ServerBlaster.dll
  Delete /REBOOTOK $INSTDIR\Plugins\TvMovie.dll
  Delete /REBOOTOK $INSTDIR\Plugins\XmlTvImport.dll
  RMDir "$INSTDIR\Plugins"

  ; And finally remove all the files installed
  ; Leave the directory in place, as it might contain user modified files
  Delete /REBOOTOK $INSTDIR\DirectShowLib.dll
  Delete /REBOOTOK $INSTDIR\dvblib.dll
  Delete /REBOOTOK $INSTDIR\PluginBase.dll
  Delete /REBOOTOK $INSTDIR\PowerScheduler.Interfaces.DLL
  Delete /REBOOTOK $INSTDIR\Blaster.exe
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
  ${LOG_TEXT} "DEBUG" "MementoSection SecClient"
  ${LOG_TEXT} "INFO" "Installing MediaPortal TV Client plugin..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${KILLPROCESS} "MediaPortal.exe"
  ${KILLPROCESS} "configuration.exe"

  SetOverwrite on

  ${LOG_TEXT} "INFO" "MediaPortal Installed at: $MPdir.Base"
  ${LOG_TEXT} "INFO" "MediaPortalPlugins are at: $MPdir.Plugins"
  
  #---------------------------- File Copy ----------------------
  ; Common Files
  SetOutPath "$MPdir.Base"
  File ..\Plugins\PowerScheduler\PowerScheduler.Interfaces\bin\Release\PowerScheduler.Interfaces.dll
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
  
  ;Gentle.Config
  SetOutPath "$MPdir.Config"
  File ..\TvPlugin\TvPlugin\Gentle.config

  ; The Plugins
  SetOutPath "$MPdir.Plugins\Process"
  File ..\Plugins\PowerScheduler\ClientPlugin\bin\Release\PowerSchedulerClientPlugin.dll
  SetOutPath "$MPdir.Plugins\Windows"
  File ..\TvPlugin\TvPlugin\bin\Release\TvPlugin.dll

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION       for TVClient
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\DVBSub2.ax" "$MPdir.Base\DVBSub2.ax" "$MPdir.Base"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${MEDIAPORTAL.FILTERBIN}\mmaacd.ax" "$MPdir.Base\mmaacd.ax" "$MPdir.Base"
${MementoSectionEnd}
!macro Remove_${SecClient}
  ${LOG_TEXT} "DEBUG" "MACRO Remove_${SecClient}"
  ${LOG_TEXT} "INFO" "Uninstalling MediaPortal TV Client plugin..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${KILLPROCESS} "MediaPortal.exe"
  ${KILLPROCESS} "configuration.exe"

  #---------------------------------------------------------------------------
  # FILTER UNREGISTRATION     for TVClient
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\DVBSub2.ax"
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\mmaacd.ax"

  ; The Plugins
  Delete /REBOOTOK "$MPdir.Plugins\Process\PowerSchedulerClientPlugin.dll"
  Delete /REBOOTOK "$MPdir.Plugins\Windows\TvPlugin.dll"

  ; Common Files
  Delete /REBOOTOK "$MPdir.Base\PowerScheduler.Interfaces.dll"
  Delete /REBOOTOK "$MPdir.Base\TvControl.dll"
  Delete /REBOOTOK "$MPdir.Base\TVDatabase.dll"
  Delete /REBOOTOK "$MPdir.Base\Gentle.Common.DLL"
  Delete /REBOOTOK "$MPdir.Base\Gentle.Framework.DLL"
  Delete /REBOOTOK "$MPdir.Base\Gentle.Provider.MySQL.dll"
  Delete /REBOOTOK "$MPdir.Base\Gentle.Provider.SQLServer.dll"
  Delete /REBOOTOK "$MPdir.Base\log4net.dll"
  Delete /REBOOTOK "$MPdir.Base\MySql.Data.dll"
  Delete /REBOOTOK "$MPdir.Base\TvBusinessLayer.dll"
  Delete /REBOOTOK "$MPdir.Base\TvLibrary.Interfaces.dll"
!macroend

${MementoSectionDone}

#---------------------------------------------------------------------------
# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
Section -Post
  ${LOG_TEXT} "DEBUG" "SECTION Post"
  ${LOG_TEXT} "INFO" "Doing post installation stuff..."

  ${If} $DeployMode == 1

    #MessageBox MB_OK|MB_ICONEXCLAMATION "DeployMode == 1"
    ReadRegDWORD $R0 ${MEMENTO_REGISTRY_ROOT} '${MEMENTO_REGISTRY_KEY}' 'MementoSection_SecServer'
    ReadRegDWORD $R1 ${MEMENTO_REGISTRY_ROOT} '${MEMENTO_REGISTRY_KEY}' 'MementoSection_SecClient'

    ;writes component status to registry
    ${MementoSectionSave}

    ${If} $noClient == 1
    ${AndIf} $R1 != ""
      WriteRegDWORD ${MEMENTO_REGISTRY_ROOT} "${MEMENTO_REGISTRY_KEY}" 'MementoSection_SecClient' $R1
    ${ElseIf} $noServer == 1
    ${AndIf} $R0 != ""
      WriteRegDWORD ${MEMENTO_REGISTRY_ROOT} "${MEMENTO_REGISTRY_KEY}" 'MementoSection_SecServer' $R0
    ${EndIf}

  ${Else}

    ;Removes unselected components
    !insertmacro SectionList "FinishSection"

    ;writes component status to registry
    ${MementoSectionSave}

  ${EndIf}

  SetOverwrite on
  SetOutPath $INSTDIR

  ${If} $noStartMenuSC != 1
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
    CreateDirectory "$SMPROGRAMS\$StartMenuGroup"
    CreateShortCut "$SMPROGRAMS\$StartMenuGroup\uninstall TV-Server.lnk" "$INSTDIR\uninstall-tve3.exe"
    WriteINIStr "$SMPROGRAMS\$StartMenuGroup\Help.url"      "InternetShortcut" "URL" "http://wiki.team-mediaportal.com/"
    WriteINIStr "$SMPROGRAMS\$StartMenuGroup\web site.url"  "InternetShortcut" "URL" "${URL}"
    !insertmacro MUI_STARTMENU_WRITE_END
  ${EndIf}

  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMajor"    "${VER_MAJOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMinor"    "${VER_MINOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionRevision" "${VER_REVISION}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionBuild"    "${VER_BUILD}"

  ; Write Uninstall Information
  WriteRegStr HKLM "${REG_UNINSTALL}" InstallPath        $INSTDIR
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName        "$(^Name)"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayVersion     "${VERSION}"
  WriteRegStr HKLM "${REG_UNINSTALL}" Publisher          "${COMPANY}"
  WriteRegStr HKLM "${REG_UNINSTALL}" URLInfoAbout       "${URL}"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon        "$INSTDIR\SetupTv.exe,0"
  WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString    "$INSTDIR\uninstall-tve3.exe"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 1
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 1

  WriteUninstaller "$INSTDIR\uninstall-tve3.exe"
SectionEnd

#---------------------------------------------------------------------------
# This section is called on uninstall and removes all components
Section Uninstall
  ${LOG_TEXT} "DEBUG" "SECTION Uninstall"
  ;First removes all optional components
  !insertmacro SectionList "RemoveSection"

  ; remove registry key
  DeleteRegKey HKLM "${REG_UNINSTALL}"

  ; remove Start Menu shortcuts
  Delete "$SMPROGRAMS\$StartMenuGroup\uninstall TV-Server.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\Help.url"
  Delete "$SMPROGRAMS\$StartMenuGroup\web site.url"
  RMDir "$SMPROGRAMS\$StartMenuGroup"

  ; remove last files and instdir
  RMDir /REBOOTOK "$INSTDIR\pmt"
  Delete /REBOOTOK "$INSTDIR\uninstall-tve3.exe"
  RMDir "$INSTDIR"

  ${If} $RemoveAll == 1
    ${LOG_TEXT} "INFO" "Removing User Settings"
    RMDir /r /REBOOTOK "${COMMON_APPDATA}"
    RMDir /r /REBOOTOK $INSTDIR
  ${EndIf}
SectionEnd

#---------------------------------------------------------------------------
# FUNCTIONS
#---------------------------------------------------------------------------
Function .onInit
  ${LOG_OPEN}
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInit"

  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $noClient 0
  StrCpy $noServer 0
  StrCpy $noDesktopSC 0
  StrCpy $noStartMenuSC 0
  StrCpy $DeployMode 0

  ; gets comandline parameter
  ${GetParameters} $R0
  ${LOG_TEXT} "DEBUG" "commandline parameters: $R0"

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

  ClearErrors
  ${GetOptions} $R0 "/DeployMode" $R1
  IfErrors +2
  StrCpy $DeployMode 1
  #### END of check and parse cmdline parameter

  ; reads components status for registry
  ${MementoSectionRestore}

  ; update the component status -> commandline parameters have higher priority than registry values
  ${If} $noClient = 1
  ${AndIf} $noServer = 1
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_PARAMETER_ERROR)"
    Abort
  ${ElseIf} $noClient = 1
    !insertmacro SelectSection ${SecServer}
    !insertmacro UnselectSection ${SecClient}
  ${ElseIf} $noServer = 1
    !insertmacro SelectSection ${SecClient}
    !insertmacro UnselectSection ${SecServer}
  ${EndIf}

  ; check if old msi based client plugin is installed.
  ${If} ${MSI_TVClientIsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MSI_CLIENT)"
    Abort
  ${EndIf}

  ; check if old msi based server is installed.
  ${If} ${MSI_TVServerIsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MSI_SERVER)"
    Abort
  ${EndIf}

  ; show error that the OS is not supported and abort the installation
  ${If} ${AtMostWin2000Srv}
    StrCpy $0 "OSabort"
  ${ElseIf} ${IsWinXP}
    !insertmacro GetServicePack $R1 $R2
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 2
      StrCpy $0 "OSabort"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWinXP64}
    StrCpy $0 "OSabort"

  ${ElseIf} ${IsWin2003}
    StrCpy $0 "OSwarn"

  ${ElseIf} ${IsWinVISTA}
    !insertmacro GetServicePack $R1 $R2
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 1
      StrCpy $0 "OSwarn"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWin2008}
    StrCpy $0 "OSwarn"

  ${Else}
    StrCpy $0 "OSabort"
  ${EndIf}

  ; show warnings for some OS
  ${If} $0 == "OSabort"
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${ElseIf} $0 == "OSwarn"
    ${If} $DeployMode == 0
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
    ${EndIf}
  ${ElseIf} $0 == "OSwarnBetaSP"
    ${If} $DeployMode == 0
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "You are using a beta Service Pack! $(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
    ${EndIf}
  ${Else}
    ; do nothing
  ${EndIf}

  ; check if current user is admin
  UserInfo::GetOriginalAccountType
  Pop $0
  #StrCmp $0 "Admin" 0 +3
  ${IfNot} $0 == "Admin"
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_ADMIN)"
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
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
    Abort
  ${EndIf}

  ${IfNot} ${MPIsInstalled}
    !insertmacro DisableComponent "${SecClient}" " ($(TEXT_MP_NOT_INSTALLED))"
  ${else}
    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${ReadMediaPortalDirs} $MPdir.Base
  ${EndIf}

  SetShellVarContext all
FunctionEnd

Function .onInstFailed
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInstFailed"
  ${LOG_CLOSE}
FunctionEnd

Function .onInstSuccess
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInstSuccess"
  ${LOG_CLOSE}
FunctionEnd

Function un.onInit
  ${un.LOG_OPEN}
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onInit"
  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $RemoveAll 0

  ; gets comandline parameter
  ${un.GetParameters} $R0
  ${LOG_TEXT} "DEBUG" "commandline parameters: $R0"

  ; check for special parameter and set the their variables
  ClearErrors
  ${un.GetOptions} $R0 "/RemoveAll" $R1
  IfErrors +2
  StrCpy $RemoveAll 1
  #### END of check and parse cmdline parameter

  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    Sleep 1
  ${else}
    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${un.ReadMediaPortalDirs} $MPdir.Base
  ${EndIf}

  !insertmacro TVSERVER_GET_INSTALL_DIR $INSTDIR
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

  SetShellVarContext all
FunctionEnd

Function un.onUninstFailed
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onUninstFailed"
  ${un.LOG_CLOSE}
FunctionEnd

Function un.onUninstSuccess
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onUninstSuccess"

  ; write a reboot flag, if reboot is needed, so the installer won't continue until reboot is done
  ${If} ${RebootFlag}
    ${LOG_TEXT} "INFO" "!!! Some files were not able to uninstall. To finish uninstallation completly a REBOOT is needed."
    FileOpen $0 $INSTDIR\rebootflag w
    Delete /REBOOTOK $INSTDIR\rebootflag ; this will not be deleted until the reboot because it is currently opened
    RmDir /REBOOTOK $INSTDIR
    FileClose $0
  ${EndIf}

  ${un.LOG_CLOSE}
FunctionEnd


Function .onSelChange
  ${LOG_TEXT} "DEBUG" "FUNCTION .onSelChange"

  ; disable the next button if nothing is selected
  ${IfNot} ${SectionIsSelected} ${SecServer}
  ${AndIfNot} ${SectionIsSelected} ${SecClient}
    EnableWindow $mui.Button.Next 0
  ${Else}
    EnableWindow $mui.Button.Next 1
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
  #${IfNot} ${MP023IsInstalled}
  #${AndIfNot} ${MPIsInstalled}
  #  !insertmacro DisableComponent "${SecClient}" " ($(TEXT_MP_NOT_INSTALLED))"
  #${EndIf}
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
  MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "$(TEXT_MSGBOX_REMOVE_ALL)" IDNO +3
  MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "$(TEXT_MSGBOX_REMOVE_ALL_STUPID)" IDNO +2
  StrCpy $RemoveAll 1
FunctionEnd

#---------------------------------------------------------------------------
# SECTION DECRIPTIONS     must be at the end
#---------------------------------------------------------------------------
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} $(DESC_SecClient)
  !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} $(DESC_SecServer)
!insertmacro MUI_FUNCTION_DESCRIPTION_END