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
Name "MediaPortal Update"
;SetCompressor /SOLID lzma

#---------------------------------------------------------------------------
# DEVELOPMENT ENVIRONMENT
#---------------------------------------------------------------------------
# path definitions
!define svn_ROOT "..\.."
!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"
!define svn_DeployVersionSVN "${svn_ROOT}\Tools\Script & Batch tools\DeployVersionSVN"


!define MIN_FRA_MAJOR "2"
!define MIN_FRA_MINOR "0"
!define MIN_FRA_BUILD "*"

# INCLUDE
!include "include-DotNetFramework.nsh"

#---------------------------------------------------------------------------
# BUILD sources
#---------------------------------------------------------------------------
; comment one of the following lines to disable the preBuild
!define BUILD_MediaPortal
!define BUILD_TVServer
#!define BUILD_DeployTool      #     <---- not needed for the updater
#!define BUILD_Installer         #<---- not needed for the updater, because it is building the installer with special parameters, uncomment it for testing please

!include "include-MP-PreBuild.nsh"

!system '"${NSISDIR}\makensis.exe" /DUPDATE_BUILD "${svn_MP}\Setup\setup.nsi"' = 0  # <----  comment for testing purposes these two line please
!system '"${NSISDIR}\makensis.exe" /DUPDATE_BUILD "${svn_TVServer}\Setup\setup.nsi"' = 0

#---------------------------------------------------------------------------
# UNPACKER script
#---------------------------------------------------------------------------
!define NAME    "MediaPortal"
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"
!define VER_MAJOR       1
!define VER_MINOR       0
!define VER_REVISION    1
!define VER_BUILD       0
!define VERSION "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}"

BrandingText  "${NAME} ${VERSION} Update by ${COMPANY}"

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh

!include "${svn_InstallScripts}\include-CommonMPMacros.nsh"

#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ABORTWARNING
!define MUI_ICON    "Resources\install.ico"
#!define MUI_UNICON  "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP              "Resources\header.bmp"
#!if ${VER_BUILD} == 0       # it's a stable release
    !define MUI_WELCOMEFINISHPAGE_BITMAP    "Resources\wizard.bmp"
#    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "Resources\wizard.bmp"
#!else                       # it's an svn reöease
#    !define MUI_WELCOMEFINISHPAGE_BITMAP    "Resources\wizard-svn.bmp"
#    !define MUI_UNWELCOMEFINISHPAGE_BITMAP  "Resources\wizard-svn.bmp"
#!endif
!define MUI_HEADERIMAGE_RIGHT

#!define MUI_COMPONENTSPAGE_SMALLDESC
#!define MUI_STARTMENUPAGE_NODISABLE
#!define MUI_STARTMENUPAGE_DEFAULTFOLDER       "Team MediaPortal\MediaPortal"
#!define MUI_STARTMENUPAGE_REGISTRY_ROOT       HKLM
#!define MUI_STARTMENUPAGE_REGISTRY_KEY        "${REG_UNINSTALL}"
#!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
#!define MUI_FINISHPAGE_RUN            "$MPdir.Base\Configuration.exe"
#!define MUI_FINISHPAGE_RUN_TEXT       "Run MediaPortal Configuration"
#!define MUI_FINISHPAGE_RUN_PARAMETERS "/avoidVersionCheck"
#!define MUI_FINISHPAGE_SHOWREADME $INSTDIR\readme.txt
#!define MUI_FINISHPAGE_SHOWREADME_TEXT "View Readme"
#!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_LINK           "Donate to MediaPortal"
!define MUI_FINISHPAGE_LINK_LOCATION  "http://www.team-mediaportal.com/donate.html"

#!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
#!define MUI_PAGE_CUSTOMFUNCTION_LEAVE WelcomeLeave
!insertmacro MUI_PAGE_WELCOME

#!ifndef SVN_BUILD
#Page custom PageReinstall PageLeaveReinstall
#!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
#!insertmacro MUI_PAGE_LICENSE "..\Docs\BASS License.txt"
#!else
#!insertmacro MUI_PAGE_LICENSE "..\Docs\svn-info.rtf"
#!endif

#!ifndef HEISE_BUILD
!insertmacro MUI_PAGE_COMPONENTS
#!endif
#!insertmacro MUI_PAGE_DIRECTORY
#!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

; UnInstaller Interface
#!insertmacro MUI_UNPAGE_WELCOME
#UninstPage custom un.UninstallModePage un.UninstallModePageLeave
#!insertmacro MUI_UNPAGE_INSTFILES
#!insertmacro MUI_UNPAGE_FINISH

#---------------------------------------------------------------------------
# INSTALLER LANGUAGES
#---------------------------------------------------------------------------
!insertmacro MUI_LANGUAGE English

#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
#Icon "${svn_DeployTool}\Install.ico"
OutFile "MediaPortalUpdater_1.0.1_SVN${SVN_REVISION}.exe"
InstallDir "$TEMP\MediaPortal Installation"

;Page directory
#Page instfiles
/*   TODO
  - add additional installer pages
*/


CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey ProductName       "${NAME}"
VIAddVersionKey ProductVersion    "${VERSION}"
VIAddVersionKey CompanyName       "${COMPANY}"
VIAddVersionKey CompanyWebsite    "${URL}"
VIAddVersionKey FileVersion       "${VERSION}"
VIAddVersionKey FileDescription   "${NAME} Updater ${VERSION}"
VIAddVersionKey LegalCopyright    "Copyright © 2005-2009 ${COMPANY}"

;if we want to make it fully silent we can uncomment this
;SilentInstall silent

Section "MediaPortal" SecMP
SectionEnd
Section "TV Client plugin" SecClient
SectionEnd
Section "TV Server" SecServer
SectionEnd

Section
  IfFileExists "$INSTDIR\*.*" 0 +2
    RMDir "$INSTDIR"

  # extract the installers to temp
  SetOutPath $INSTDIR
  File "${svn_MP}\Setup\Release\package-mediaportal.exe"
  File "${svn_TVServer}\Setup\Release\package-tvengine.exe"


  # install MP
  ${If} ${MPIsInstalled}
    Exec "'$INSTDIR\package-mediaportal.exe' /S /DeployMode"
  ${EndIf}

  # install TVserver or plugin  
  ${If} ${TVServerIsInstalled}
  ${AndIf} ${TVClientIsInstalled}
    Exec "'$INSTDIR\package-mediaportal.exe' /S /DeployMode"
  ${Else}
    ${If} ${TVServerIsInstalled}
      Exec "'$INSTDIR\package-mediaportal.exe' /S /noClient /DeployMode"
    ${ElseIf} ${TVClientIsInstalled}
      Exec "'$INSTDIR\package-mediaportal.exe' /S /noServer /DeployMode"
    ${EndIf}
  ${EndIf}


SectionEnd

Function .onInit
  Call AbortIfBadFramework


  ; OS and other common initialization checks are done in the following NSIS header file
  !insertmacro MediaPortalOperatingSystemCheck 0

  # installation checks
  ${IfNot} ${MPIsInstalled}
  ${AndIfNot} ${TVServerIsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(UPDATE_ERROR_NOTHING_INSTALLED)"
    Abort
  ${EndIf}

  ${If} ${TVClientIsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(UPDATE_ERROR_UNKNOWN)"
    Abort
  ${EndIf}


  ${If} ${MPIsInstalled}
    !insertmacro MP_GET_VERSION $0
    ${IfNot} $0 = "1.0.0.0"
      MessageBox MB_OK|MB_ICONSTOP "$(UPDATE_ERROR_VERSION_MP)"
      Abort
    ${EndIf}
  ${EndIf}
  
  ${If} ${TVServerIsInstalled}
  ${OrIf} ${TVClientIsInstalled}
    !insertmacro TVSERVER_GET_VERSION $0
    ${IfNot} $0 = "1.0.0.0"
      MessageBox MB_OK|MB_ICONSTOP "$(UPDATE_ERROR_VERSION_TVSERVER)"
      Abort
    ${EndIf}
  ${EndIf}


  # sections for components page  (only for visual reason)
  #first all will be unselected and read only
  !insertmacro DisableComponent "${SecMP}"     ""
  !insertmacro DisableComponent "${SecServer}" ""
  !insertmacro DisableComponent "${SecClient}" ""
  # the ones which will be updated, needs to be enabled again
  ${If} ${MPIsInstalled}
    !insertmacro SelectSection ${SecMP}
  ${EndIf}
  ${If} ${TVServerIsInstalled}
    !insertmacro SelectSection ${SecServer}
  ${EndIf}
  ${If} ${TVClientIsInstalled}
    !insertmacro SelectSection ${SecClient}
  ${EndIf}
FunctionEnd