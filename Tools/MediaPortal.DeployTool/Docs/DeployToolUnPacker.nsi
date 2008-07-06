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
!define ROOT "..\..\.."
!define MPtrunk "${ROOT}\mediaportal"
!define TVServertrunk "${ROOT}\TvEngine3\TVLibrary"

!define DEPLOY.BIN "..\bin\Release"


# BUILD MediaPortal
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release "${MPtrunk}\DeployVersionSVN\DeployVersionSVN.sln"'

!system '"${MPtrunk}\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${MPtrunk}"'
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" "${MPtrunk}\MediaPortal.sln"'
!system '"${MPtrunk}\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${MPtrunk}"  /revert'

# BUILD TVServer
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release "${TVServertrunk}\DeployVersionSVN\DeployVersionSVN.sln"'

!system '"${MPtrunk}\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${TVServertrunk}"'
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" "${TVServertrunk}\TvLibrary.sln"'
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" "${TVServertrunk}\TvPlugin\TvPlugin.sln"'
!system '"${MPtrunk}\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${TVServertrunk}"  /revert'

# BUILD installer
!execute '"${NSISDIR}\makensis.exe" "${ROOT}\mediaportal\Setup\setup.nsi"'
!execute '"${NSISDIR}\makensis.exe" "${ROOT}\TvEngine3\TVLibrary\Setup\setup.nsi"'



Name "MediaPortal Unpacker"
;SetCompressor /SOLID lzma
Icon "${ROOT}\Tools\MediaPortal.DeployTool\Install.ico"

OutFile "MediaPortal Setup 1.0preRC2 (SVN_test).exe"
InstallDir "$TEMP\MediaPortal Installation"

Page directory
Page instfiles

CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
AutoCloseWindow true

Section
  SetOutPath $INSTDIR
  File /r /x .svn /x *.pdb /x *.vshost.exe "${DEPLOY.BIN}\*"

  SetOutPath $INSTDIR\deploy
  File "${ROOT}\mediaportal\Setup\Release\package-mediaportal.exe"
  File "${ROOT}\TvEngine3\TVLibrary\Setup\Release\package-tvengine.exe"

SectionEnd

Function .onInstSuccess
  Exec "$INSTDIR\MediaPortal.DeployTool.exe"
FunctionEnd