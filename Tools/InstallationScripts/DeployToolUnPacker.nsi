#region Copyright (C) 2005-2011 Team MediaPortal
/*
// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#**********************************************************************************************************#
#
#   For building the installer on your own you need:
#       1. Latest NSIS version from http://nsis.sourceforge.net/Download
#
#**********************************************************************************************************#

#---------------------------------------------------------------------------
# DEVELOPMENT ENVIRONMENT
#---------------------------------------------------------------------------
# SKRIPT_NAME is needed to diff between the install scripts in imported headers
!define SKRIPT_NAME "MediaPortal Unpacker"
# path definitions, all others are done in MediaPortalScriptInit
!define git_ROOT "..\.."
!define git_InstallScripts "${git_ROOT}\Tools\InstallationScripts"
# common script init
!include "${git_InstallScripts}\include\MediaPortalScriptInit.nsh"


#---------------------------------------------------------------------------
# UNPACKER script
#---------------------------------------------------------------------------
!define PRODUCT_NAME          "MediaPortal"
!define PRODUCT_PUBLISHER     "Team MediaPortal"
!define PRODUCT_WEB_SITE      "www.team-mediaportal.com"

; needs to be done before importing MediaPortalCurrentVersion, because there the VER_BUILD will be set, if not already.
!ifdef VER_BUILD ; means !build_release was used
  !undef VER_BUILD

  ;!system 'include-MP-PreBuild.bat'
  !system '"..\Script & Batch tools\DeployVersionGIT\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /GetVersion=version.template.txt /path=${GIT_ROOT}'

  !include "version.txt"
  !delfile "version.txt"
  !if ${VER_BUILD} == 0
    !warning "It seems there was an error, reading the git revision. 0 will be used."
  !endif
!endif

; import version from shared file
!include "${git_InstallScripts}\include\MediaPortalCurrentVersion.nsh"

#---------------------------------------------------------------------------
# BUILD sources
#---------------------------------------------------------------------------
; comment one of the following lines to disable the preBuild
!define BUILD_MediaPortal
!define BUILD_TVServer
!define BUILD_DeployTool
!define BUILD_Installer

!include "include-MP-PreBuild.nsh"


#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!define NO_INSTALL_LOG
!include "${git_InstallScripts}\include\LanguageMacros.nsh"
!include "${git_InstallScripts}\include\MediaPortalMacros.nsh"


#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
Name          "${SKRIPT_NAME}"
BrandingText  "${PRODUCT_NAME} ${VERSION_DISP} by ${PRODUCT_PUBLISHER}"
Icon "${git_DeployTool}\Install.ico"
!define /date buildTIMESTAMP "%Y-%m-%d-%H-%M"
!if ${VER_BUILD} == 0
  OutFile "${git_OUT}\MediaPortalSetup_${VERSION}_${buildTIMESTAMP}.exe"
!else
  OutFile "${git_OUT}\MediaPortalSetup_${VERSION}_${buildTIMESTAMP}.exe"
!endif
InstallDir "$TEMP\MediaPortal Installation"

CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
AutoCloseWindow true
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey ProductName       "${PRODUCT_NAME}"
VIAddVersionKey ProductVersion    "${VERSION_DISP}"
VIAddVersionKey CompanyName       "${PRODUCT_PUBLISHER}"
VIAddVersionKey CompanyWebsite    "${PRODUCT_WEB_SITE}"
VIAddVersionKey FileVersion       "${VERSION}"
VIAddVersionKey FileDescription   "${PRODUCT_NAME} installation ${VERSION_DISP}"
VIAddVersionKey LegalCopyright    "Copyright © 2005-2011 ${PRODUCT_PUBLISHER}"

;if we want to make it fully silent we can uncomment this
;SilentInstall silent

;Page directory
Page instfiles

!insertmacro LANG_LOAD "English"

;sections for unpacking
Section
  IfFileExists "$INSTDIR\*.*" 0 +2
    RMDir /r "$INSTDIR"

  SetOutPath $INSTDIR
  File /r /x .git /x *.pdb /x *.vshost.exe "${git_DeployTool}\bin\Release\*"

  SetOutPath $INSTDIR\deploy
#code after build scripts are fixed
!if "$%COMPUTERNAME%" != "S15341228"
  File "${git_OUT}\package-mediaportal.exe"
  File "${git_OUT}\package-tvengine.exe"
!else

#code before build scripts are fixed
  File "${git_MP}\Setup\Release\package-mediaportal.exe"
  File "${git_TVServer}\Setup\Release\package-tvengine.exe"
#end of workaound code
!endif
 
  SetOutPath $INSTDIR\HelpContent\DeployToolGuide
  File /r /x .git "${git_DeployTool}\HelpContent\DeployToolGuide\*"

SectionEnd

Function .onInit
  !insertmacro MediaPortalNetFrameworkCheck
FunctionEnd

Function .onInstSuccess
  Exec "$INSTDIR\MediaPortal.DeployTool.exe"
FunctionEnd