#region Copyright (C) 2005-2010 Team MediaPortal
/*
// Copyright (C) 2005-2010 Team MediaPortal
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
#   include-MP-PreBuild.nsh
#
#       This is a NSIS header file, containing the commands to compile the MediaPortal source files.
#
#**********************************************************************************************************#

# set different paths, whether env is x32 or x64
!ifdef x64Environment
  !define ALToolPath "%WINDOWS_SDK%\Bin\x64"
!else
  !define ALToolPath "%WINDOWS_SDK%\Bin"
!endif

# The following commands needs to be defined by the parent script (the one, which includes this file).
;!define BUILD_MediaPortal
;!define BUILD_TVServer
;!define BUILD_DeployTool
;!define BUILD_Installer

# GetVersion by exeuting DeployVersionSVN.exe /GetVersion
;!system '"$%ProgramFiles%\TortoiseSVN\bin\SubWCRev.exe" "${svn_ROOT}" RevisionInfoTemplate.nsh version.txt' = 0
!system 'include-MP-PreBuild.bat'
;!define SVN_REVISION "$WCREV$"    ; that's the string in version txt, after SubWCRev has been launched
!include "version.txt"
!delfile "version.txt"
!if ${SVN_REVISION} == 0
  !warning "It seems there was an error, reading the svn revision. 0 will be used."
!endif

!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_ROOT}\Common-MP-TVE3"' = 0
!ifdef BUILD_MediaPortal
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_MP}"' = 0
!system '"$%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=Release;Platform=x86 "${svn_MP}\MediaPortal.sln"' = 0
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_MP}"  /revert' = 0
!endif

!ifdef BUILD_TVServer
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_TVServer}"' = 0
!system '"$%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=Release;Platform=x86 "${svn_TVServer}\TvLibrary.sln"' = 0
!system '"$%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=Release;Platform=x86 "${svn_TVServer}\TvPlugin\TvPlugin.sln"' = 0
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_TVServer}"  /revert' = 0
!endif
!system '"${svn_DeployVersionSVN}\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_ROOT}\Common-MP-TVE3"  /revert' = 0

!ifdef BUILD_DeployTool
!system '"$%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /p:ALToolPath="${ALToolPath}" /target:Rebuild /property:Configuration=Release;Platform=x86 "${svn_DeployTool}\MediaPortal.DeployTool.sln"' = 0
!endif

!ifdef BUILD_Installer
!system '"${NSISDIR}\makensis.exe" "${svn_MP}\Setup\setup.nsi"' = 0
!system '"${NSISDIR}\makensis.exe" "${svn_TVServer}\Setup\setup.nsi"' = 0
!endif
