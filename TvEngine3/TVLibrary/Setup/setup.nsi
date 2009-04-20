#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
#   For building the installer on your own you need:
#       1. Latest NSIS version from http://nsis.sourceforge.net/Download
#
#**********************************************************************************************************#

#---------------------------------------------------------------------------
# SPECIAL BUILDS
#---------------------------------------------------------------------------
##### SVN_BUILD
# This build will be created by svn bot only.
# Creating such a build, will only include the changed and new files since latest stable release to the installer.

##### HEISE_BUILD
# Uncomment the following line to create a setup for "Heise Verlag" / ct' magazine  (without MPC-HC/Gabest Filters)
;!define HEISE_BUILD
# parameter for command line execution: /DHEISE_BUILD

##### BUILD_TYPE
# Uncomment the following line to create a setup in debug mode
;!define BUILD_TYPE "Debug"
# parameter for command line execution: /DBUILD_TYPE=Debug
# by default BUILD_TYPE is set to "Release"
!ifndef BUILD_TYPE
  !define BUILD_TYPE "Release"
!endif

#---------------------------------------------------------------------------
# DEVELOPMENT ENVIRONMENT
#---------------------------------------------------------------------------
# path definitions
!define svn_ROOT "..\..\.."
!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_DirectShowFilters "${svn_ROOT}\DirectShowFilters"
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"

# additional path definitions
!define TVSERVER.BASE "${svn_TVServer}\TVServer.Base"
#!define MEDIAPORTAL.BASE "${svn_MP}\MediaPortal.Base"
!ifdef SVN_BUILD
  !define MEDIAPORTAL.BASE "E:\compile\compare_mp1_test"
!else
  !define MEDIAPORTAL.BASE "${svn_MP}\MediaPortal.Base"
!endif

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
Var UpdateMode
; variables for commandline parameters for UnInstaller

#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define NAME    "MediaPortal TV Server / Client"
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"


!define REG_UNINSTALL         "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
!define MP_REG_UNINSTALL      "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!define MEMENTO_REGISTRY_ROOT HKLM
!define MEMENTO_REGISTRY_KEY  "${REG_UNINSTALL}"
!define COMMON_APPDATA        "$APPDATA\Team MediaPortal\MediaPortal TV Server"

!define VER_MAJOR       1
!define VER_MINOR       0
!define VER_REVISION    1
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif


!if ${BUILD_TYPE} == "Debug"
  !define VERSION "1.0 >>DEBUG<< build ${VER_BUILD} for TESTING ONLY"
!else
!if ${VER_BUILD} == 0       # it's an official release
  !define VERSION "1.0.2"
!else                       # it's a svn release
  !define VERSION "1.0.1 SVN build ${VER_BUILD} for TESTING ONLY"
!endif
!endif
Name          "${NAME}"
SetCompressor /SOLID lzma
BrandingText  "${NAME} ${VERSION} by ${COMPANY}"

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include Library.nsh
!include FileFunc.nsh
!include Memento.nsh
!include "${svn_InstallScripts}\include-WinVerEx.nsh"


!include "${svn_InstallScripts}\include\*"


!include "${svn_InstallScripts}\pages\AddRemovePage.nsh"
!insertmacro AddRemovePage "${REG_UNINSTALL}"
!include "${svn_InstallScripts}\pages\UninstallModePage.nsh"


#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ABORTWARNING
!define MUI_ICON    "Resources\install.ico"
!define MUI_UNICON  "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP              "Resources\header.bmp"
!if ${VER_BUILD} == 0       # it's an official release
  !define MUI_WELCOMEFINISHPAGE_BITMAP      "Resources\wizard.bmp"
!else                       # it's a svn release
  !define MUI_WELCOMEFINISHPAGE_BITMAP      "Resources\wizard-svn.bmp"
!endif
!define MUI_UNWELCOMEFINISHPAGE_BITMAP      "Resources\wizard.bmp"
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
!define MUI_FINISHPAGE_LINK          "Donate to MediaPortal"
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
!insertmacro MUI_UNPAGE_WELCOME
UninstPage custom un.UninstallModePage un.UninstallModePageLeave
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

#---------------------------------------------------------------------------
# INSTALLER LANGUAGES
#---------------------------------------------------------------------------
!insertmacro LANG_LOAD "English"

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
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription   "${NAME} installation ${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright    "Copyright © 2005-2009 ${COMPANY}"
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
!macro RenameInstallDirectory
  !insertmacro GET_BACKUP_POSTFIX $R0

  ${If} $DeployMode == 1
  ${AndIf} $noServer == 1   ; this means the client is being installed, tv server was installed before, so the instdir shouldn't be renamed again       noClient

    ${LOG_TEXT} "INFO" "!!!!!!!!!!!! the client is being installed, tv server was installed before, so the instdir shouldn't be renamed again"

  ${Else}

    !insertmacro RenameDirectory "$INSTDIR" "$INSTDIR_$R0"
    !insertmacro RenameDirectory "${COMMON_APPDATA}" "${COMMON_APPDATA}_$R0"

  ${EndIf}
!macroend
Section "-prepare" SecPrepare
  ${LOG_TEXT} "DEBUG" "SECTION SecPrepare"
  ${LOG_TEXT} "INFO" "Prepare installation..."
  SetShellVarContext all

  ${If} $UpdateMode = 1
    ${LOG_TEXT} "INFO" "Installer started in UpdateMode."

    ; check current install mode: TVServer / TVClient / SingleSeat (both)
    ; if only TVServer, uninstall current one and go on with installation of new one
    ; if only TVClient, uninstall current one and go on with installation of new one
    ; if SingleSeat, if NoClient (Server installation), uninstall current one (removes both components) and install new server component
    ; if SingleSeat, if NoServer (Client installation), Do NOT uninstall current one (was removed on during server install already), but install new client component
    ${If} $noClient == 1

      ${LOG_TEXT} "INFO" "TVServer component will be installed soon. (/noClient was used)"

      ${LOG_TEXT} "INFO" "InstallMode is: Dedicated TVServer or SingleSeat."
      ${LOG_TEXT} "INFO" "  ==>    Old TVServer/Client will be uninstalled now."
      !insertmacro NsisSilentUinstall "${TV3_REG_UNINSTALL}"

    ${ElseIf} $noServer == 1

      ${LOG_TEXT} "INFO" "TVClient component will be installed soon. (/noServer was used)"

      ${If} ${TVServerIsInstalled}
        ${LOG_TEXT} "INFO" "TVServer component is already installed."
        ${LOG_TEXT} "INFO" "     --> Means it is a SingleSeat installation."
        ${LOG_TEXT} "INFO" "     --> Means TVServer was already updated."
        ${LOG_TEXT} "INFO" "  ==>    TVServer/Client won't be uninstalled."
      ${Else}
        ${LOG_TEXT} "INFO" "TVServer component isn't already installed."
        ${LOG_TEXT} "INFO" "     --> Means it is a TVClient-only installation."
        ${LOG_TEXT} "INFO" "  ==>    Old TVClient will be uninstalled now."
        !insertmacro NsisSilentUinstall "${TV3_REG_UNINSTALL}"
      ${EndIf}

    ${EndIf}

  ${Else}

    ${LOG_TEXT} "DEBUG" "SecPrepare: not in updateMode"
    !if ${VER_BUILD} == 0       # it's an official release
      !insertmacro RenameInstallDirectory
    !endif

  ${EndIf}

SectionEnd

${MementoSection} "MediaPortal TV Server" SecServer
  ${LOG_TEXT} "DEBUG" "MementoSection SecServer"
  ${LOG_TEXT} "INFO" "Installing MediaPortal TV Server..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${StopService} "TVservice"
  ${KillProcess} "SetupTv.exe"

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
  File ..\Plugins\ComSkipLauncher\bin\${BUILD_TYPE}\ComSkipLauncher.dll
  File ..\Plugins\ConflictsManager\bin\${BUILD_TYPE}\ConflictsManager.dll
  # removed it because it is not working like it should
  #File ..\Plugins\PersonalTVGuide\bin\${BUILD_TYPE}\PersonalTVGuide.dll
  File ..\Plugins\PowerScheduler\bin\${BUILD_TYPE}\PowerScheduler.dll
  File ..\Plugins\ServerBlaster\ServerBlaster\bin\${BUILD_TYPE}\ServerBlaster.dll
  File ..\Plugins\TvMovie\bin\${BUILD_TYPE}\TvMovie.dll
  File ..\Plugins\XmlTvImport\bin\${BUILD_TYPE}\XmlTvImport.dll

  ; Rest of Files
  SetOutPath $INSTDIR
  File ..\DirectShowLib\bin\${BUILD_TYPE}\DirectShowLib.dll
  File ..\Plugins\PluginBase\bin\${BUILD_TYPE}\PluginBase.dll
  File ..\Plugins\PowerScheduler\PowerScheduler.Interfaces\bin\${BUILD_TYPE}\PowerScheduler.Interfaces.dll
  File "..\Plugins\ServerBlaster\ServerBlaster (Learn)\bin\${BUILD_TYPE}\Blaster.exe"
  File ..\SetupTv\bin\${BUILD_TYPE}\SetupTv.exe
  File ..\SetupTv\bin\${BUILD_TYPE}\SetupTv.exe.config
  File ..\TvControl\bin\${BUILD_TYPE}\TvControl.dll
  File ..\TVDatabase\bin\${BUILD_TYPE}\TVDatabase.dll
  File ..\TVDatabase\references\Gentle.Common.DLL
  File ..\TVDatabase\references\Gentle.Framework.DLL
  File ..\TVDatabase\references\Gentle.Provider.MySQL.dll
  File ..\TVDatabase\references\Gentle.Provider.SQLServer.dll
  File ..\TVDatabase\references\log4net.dll
  File ..\TVDatabase\references\MySql.Data.dll
  File ..\TVDatabase\TvBusinessLayer\bin\${BUILD_TYPE}\TvBusinessLayer.dll
  File ..\TvLibrary.Interfaces\bin\${BUILD_TYPE}\TvLibrary.Interfaces.dll
  File ..\TVLibrary\bin\${BUILD_TYPE}\TVLibrary.dll
  File ..\TvService\bin\${BUILD_TYPE}\TvService.exe
  File ..\TvService\bin\${BUILD_TYPE}\TvService.exe.config
  File ..\SetupControls\bin\${BUILD_TYPE}\SetupControls.dll

  ; 3rd party assemblys
  File "${TVSERVER.BASE}\dvblib.dll"
  File "${TVSERVER.BASE}\dxerr9.dll"
  File "${TVSERVER.BASE}\hauppauge.dll"
  File "${TVSERVER.BASE}\hcwWinTVCI.dll"
  File "${TVSERVER.BASE}\KNCBDACTRL.dll"
  File "${TVSERVER.BASE}\ttBdaDrvApi_Dll.dll"
  File "${TVSERVER.BASE}\ttdvbacc.dll"
  File "${TVSERVER.BASE}\ICSharpCode.SharpZipLib.dll"

  File "${svn_DirectShowFilters}\StreamingServer\bin\${BUILD_TYPE}\StreamingServer.dll"

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
    !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\TsReader\bin\${BUILD_TYPE}\TsReader.ax" "$INSTDIR\TsReader.ax" "$INSTDIR"
  ${EndIf}
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\TsWriter\bin\${BUILD_TYPE}\TsWriter.ax" "$INSTDIR\TsWriter.ax" "$INSTDIR"
  ; filters for analog tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\MPWriter\bin\${BUILD_TYPE}\mpFileWriter.ax" "$INSTDIR\mpFileWriter.ax" "$INSTDIR"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\bin\Release\PDMpgMux.ax" "$INSTDIR\PDMpgMux.ax" "$INSTDIR"

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
  ${StopService} "TVservice"
  ${KillProcess} "SetupTv.exe"

  #---------------------------------------------------------------------------
  # CLEARING DATABASE if RemoveAll was selected
  #---------------------------------------------------------------------------
  ${If} $UnInstallMode == 1
  ${OrIf} $UnInstallMode == 2
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
  ${KillProcess} "MediaPortal.exe"
  ${KillProcess} "configuration.exe"

  SetOverwrite on

  ${LOG_TEXT} "INFO" "MediaPortal Installed at: $MPdir.Base"
  ${LOG_TEXT} "INFO" "MediaPortalPlugins are at: $MPdir.Plugins"
  
  #---------------------------- File Copy ----------------------
  ; Common Files
  SetOutPath "$MPdir.Base"
  File ..\Plugins\PowerScheduler\PowerScheduler.Interfaces\bin\${BUILD_TYPE}\PowerScheduler.Interfaces.dll
  File ..\TvControl\bin\${BUILD_TYPE}\TvControl.dll
  File ..\TVDatabase\bin\${BUILD_TYPE}\TVDatabase.dll
  File ..\TVDatabase\references\Gentle.Common.DLL
  File ..\TVDatabase\references\Gentle.Framework.DLL
  File ..\TVDatabase\references\Gentle.Provider.MySQL.dll
  File ..\TVDatabase\references\Gentle.Provider.SQLServer.dll
  File ..\TVDatabase\references\log4net.dll
  File ..\TVDatabase\references\MySql.Data.dll
  File ..\TVDatabase\TvBusinessLayer\bin\${BUILD_TYPE}\TvBusinessLayer.dll
  File ..\TvLibrary.Interfaces\bin\${BUILD_TYPE}\TvLibrary.Interfaces.dll
  
  ;Gentle.Config
  SetOutPath "$MPdir.Config"
  File ..\TvPlugin\TvPlugin\Gentle.config

  ; The Plugins
  SetOutPath "$MPdir.Plugins\Process"
  File ..\Plugins\PowerScheduler\ClientPlugin\bin\${BUILD_TYPE}\PowerSchedulerClientPlugin.dll
  SetOutPath "$MPdir.Plugins\Windows"
  File ..\TvPlugin\TvPlugin\bin\${BUILD_TYPE}\TvPlugin.dll

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION       for TVClient
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\DVBSubtitle2\bin\${BUILD_TYPE}\DVBSub2.ax" "$MPdir.Base\DVBSub2.ax"  "$MPdir.Base"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\bin\Release\mmaacd.ax"                     "$MPdir.Base\mmaacd.ax"   "$MPdir.Base"
${MementoSectionEnd}
!macro Remove_${SecClient}
  ${LOG_TEXT} "DEBUG" "MACRO Remove_${SecClient}"
  ${LOG_TEXT} "INFO" "Uninstalling MediaPortal TV Client plugin..."

  ; Kill running Programs
  ${LOG_TEXT} "INFO" "Terminating processes ..."
  ${KillProcess} "MediaPortal.exe"
  ${KillProcess} "configuration.exe"

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

  ; cleaning/renaming log dir - requested by chemelli
  RMDir /r "${COMMON_APPDATA}\log\OldLogs"
  CreateDirectory "${COMMON_APPDATA}\log\OldLogs"
  CopyFiles /SILENT /FILESONLY "${COMMON_APPDATA}\log\*" "${COMMON_APPDATA}\log\OldLogs"
  Delete "${COMMON_APPDATA}\log\*"

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
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName        "${NAME}"
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
  ; $StartMenuGroup (default): "Team MediaPortal\TV Server"
  Delete "$SMPROGRAMS\$StartMenuGroup\uninstall TV-Server.lnk"
  Delete "$SMPROGRAMS\$StartMenuGroup\Help.url"
  Delete "$SMPROGRAMS\$StartMenuGroup\web site.url"
  RMDir "$SMPROGRAMS\$StartMenuGroup"
  RMDir "$SMPROGRAMS\Team MediaPortal"

  ; remove last files and instdir
  RMDir /REBOOTOK "$INSTDIR\pmt"
  Delete /REBOOTOK "$INSTDIR\uninstall-tve3.exe"
  RMDir "$INSTDIR"



  ${If} $UnInstallMode == 1

    ${LOG_TEXT} "INFO" "Removing User Settings"
    RMDir /r /REBOOTOK "${COMMON_APPDATA}"
    RMDir /r /REBOOTOK $INSTDIR

    RMDir /r /REBOOTOK "$LOCALAPPDATA\VirtualStore\ProgramData\Team MediaPortal\MediaPortal TV Server"
    RMDir /r /REBOOTOK "$LOCALAPPDATA\VirtualStore\Program Files\Team MediaPortal\MediaPortal TV Server"
    RMDir /REBOOTOK "$LOCALAPPDATA\VirtualStore\ProgramData\Team MediaPortal"
    RMDir /REBOOTOK "$LOCALAPPDATA\VirtualStore\Program Files\Team MediaPortal"

  ${ElseIf} $UnInstallMode == 2

    !insertmacro CompleteMediaPortalCleanup

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
  StrCpy $UpdateMode 0

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

  ClearErrors
  ${GetOptions} $R0 "/UpdateMode" $R1
  IfErrors +2
  IntOp $UpdateMode $DeployMode & 1
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


${If} $DeployMode = 0

  ; OS and other common initialization checks are done in the following NSIS header file
  !insertmacro MediaPortalOperatingSystemCheck $DeployMode
  !insertmacro MediaPortalAdminCheck $DeployMode
  !insertmacro MediaPortalVCRedistCheck $DeployMode

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

  ; check if reboot is required
  ${If} ${FileExists} "$INSTDIR\rebootflag"
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
    Abort
  ${EndIf}

${EndIf}


  ; Read installation dir from registry, ONLY if
  ;   - installer is started in UpdateMode
  ;   - MediaPortal is already installed
  ${If} $UpdateMode = 1
    ${If} ${TVServerIsInstalled}
    ${OrIf} ${TVClientIsInstalled}
      !insertmacro TVSERVER_GET_INSTALL_DIR $INSTDIR
    ${Else}
      MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_UPDATE_BUT_NOT_INSTALLED)"
      Abort
    ${EndIf}
  ${EndIf}

  ; If Silent:   check if MP is installed -> if not disable that component
  ; If notSilent: do nothing    -> MP check is done on ComponentsPagePre, so it will checked dynamically
  ${If} ${Silent}

    ${IfNot} ${MPIsInstalled}
      !insertmacro UnselectSection "${SecClient}"
    ${else}
      !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
      ${ReadMediaPortalDirs} $MPdir.Base
    ${EndIf}

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
  StrCpy $UnInstallMode 0

  ; gets comandline parameter
  ${un.GetParameters} $R0
  ${LOG_TEXT} "DEBUG" "commandline parameters: $R0"

  ; check for special parameter and set the their variables
  ClearErrors
  ${un.GetOptions} $R0 "/RemoveAll" $R1
  IfErrors +2
  StrCpy $UnInstallMode 1
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
        WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" "${NAME}" $EXEPATH
        Quit
    ${EndIf}
FunctionEnd
*/
Function ComponentsPre

  ${IfNot} ${MPIsInstalled}

    ; uncheck the component, so that it won't be installed
    !insertmacro UnselectSection "${SecClient}"
    ; add the read only flag to the section, see Sections.nsh of official NSIS header files
    !insertmacro SetSectionFlag "${SecClient}" ${SF_RO}
    ; set new text for the component
    SectionSetText ${SecClient} "MediaPortal TV Client plugin ($(TEXT_MP_NOT_INSTALLED))"

  ${else}

    ; check the component
    !insertmacro SelectSection "${SecClient}"
    ; remove the read only flag to the section, see Sections.nsh of official NSIS header files
    !insertmacro ClearSectionFlag "${SecClient}" ${SF_RO}
    ; set new text for the component
    SectionSetText ${SecClient} "MediaPortal TV Client plugin"

    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${ReadMediaPortalDirs} $MPdir.Base

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
  ${IfNot} ${TVServerIsInstalled}
    SendMessage $mui.FinishPage.Run ${BM_CLICK} 0 0
    ShowWindow  $mui.FinishPage.Run ${SW_HIDE}
  ${EndIf}
FunctionEnd

#---------------------------------------------------------------------------
# SECTION DECRIPTIONS     must be at the end
#---------------------------------------------------------------------------
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} $(DESC_SecClient)
  !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} $(DESC_SecServer)
!insertmacro MUI_FUNCTION_DESCRIPTION_END