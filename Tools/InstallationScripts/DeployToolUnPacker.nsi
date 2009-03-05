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
Name "MediaPortal Unpacker"
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


#---------------------------------------------------------------------------
# BUILD sources
#---------------------------------------------------------------------------
; comment one of the following lines to disable the preBuild
#!define BUILD_MediaPortal
#!define BUILD_TVServer
#!define BUILD_DeployTool
#!define BUILD_Installer

!include "include-MP-PreBuild.nsh"

#---------------------------------------------------------------------------
# UNPACKER script
#---------------------------------------------------------------------------
!define NAME    "MediaPortal"
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"
!define VER_MAJOR       1
!define VER_MINOR       0
!define VER_REVISION    0
!ifdef VER_BUILD
  !undef VER_BUILD
!endif
!define VER_BUILD       0

!define VERSION "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}"

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include "${svn_InstallScripts}\include-CommonMPMacros.nsh"


#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
Icon "${svn_DeployTool}\Install.ico"
OutFile "MediaPortalSetup_1.0_SVN${SVN_REVISION}.exe"
InstallDir "$TEMP\MediaPortal Installation"

;Page directory
Page instfiles

CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
AutoCloseWindow true
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey ProductName       "${NAME}"
VIAddVersionKey ProductVersion    "${VERSION}"
VIAddVersionKey CompanyName       "${COMPANY}"
VIAddVersionKey CompanyWebsite    "${URL}"
VIAddVersionKey FileVersion       "${VERSION}"
VIAddVersionKey FileDescription   "${NAME} installation ${VERSION}"
VIAddVersionKey LegalCopyright    "Copyright © 2005-2009 ${COMPANY}"

;if we want to make it fully silent we can uncomment this
;SilentInstall silent

Section
  IfFileExists "$INSTDIR\*.*" 0 +2
    RMDir "$INSTDIR"

  SetOutPath $INSTDIR
  File /r /x .svn /x *.pdb /x *.vshost.exe "${svn_DeployTool}\bin\Release\*"

  SetOutPath $INSTDIR\deploy
  File "${svn_MP}\Setup\Release\package-mediaportal.exe"
  File "${svn_TVServer}\Setup\Release\package-tvengine.exe"

  SetOutPath $INSTDIR\HelpContent\SetupGuide
  File /r /x .svn "${svn_DeployTool}\HelpContent\SetupGuide\*"
  
  SetOutPath $INSTDIR\HelpContent\DeployToolGuide
  File /r /x .svn "${svn_DeployTool}\HelpContent\DeployToolGuide\*"

SectionEnd

Function .onInit
  !insertmacro MediaPortalNetFrameworkCheck 0
FunctionEnd

Function .onInstSuccess
  Exec "$INSTDIR\MediaPortal.DeployTool.exe"
FunctionEnd