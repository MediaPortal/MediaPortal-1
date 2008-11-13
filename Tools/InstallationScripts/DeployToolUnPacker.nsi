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

# BUILD sources
; comment one of the following lines to disable the preBuild
!define BUILD_MediaPortal
!define BUILD_TVServer
!define BUILD_DeployTool
!define BUILD_Installer

!system '"$%ProgramFiles%\TortoiseSVN\bin\SubWCRev.exe" "${svn_ROOT}" RevisionInfoTemplate.nsh version.txt' = 0
;!define SVN_REVISION "$WCREV$"    ; that's the string in version txt, after SubWCRev has been launched
!include "version.txt"

!system '"$%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild "${svn_DeployVersionSVN}\DeployVersionSVN.sln"' = 0

!ifdef BUILD_MediaPortal
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_MP}"' = 0
!system '"$%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=Release;Platform=x86 "${svn_MP}\MediaPortal.sln"' = 0
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_MP}"  /revert' = 0
!endif

!ifdef BUILD_TVServer
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_TVServer}"' = 0
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" "${svn_TVServer}\TvLibrary.sln"' = 0
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" "${svn_TVServer}\TvPlugin\TvPlugin.sln"' = 0
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_TVServer}"  /revert' = 0
!endif

!ifdef BUILD_DeployTool
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release" "${svn_DeployTool}\MediaPortal.DeployTool.sln"' = 0
!endif

!ifdef BUILD_Installer
!system '"${NSISDIR}\makensis.exe" "${svn_MP}\Setup\setup.nsi"' = 0
!system '"${NSISDIR}\makensis.exe" "${svn_TVServer}\Setup\setup.nsi"' = 0
!endif


# UNPACKER script
Name "MediaPortal Unpacker"
;SetCompressor /SOLID lzma
Icon "${svn_DeployTool}\Install.ico"

OutFile "MediaPortalSetup_1.0preRC4_SVN${SVN_REVISION}.exe"
InstallDir "$TEMP\MediaPortal Installation"

;Page directory
Page instfiles

CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
AutoCloseWindow true

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
  Call AbortIfBadFramework
FunctionEnd

Function .onInstSuccess
  Exec "$INSTDIR\MediaPortal.DeployTool.exe"
FunctionEnd