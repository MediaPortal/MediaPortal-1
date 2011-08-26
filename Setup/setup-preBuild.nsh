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
#       This file is used by:
#                        - DeployToolUnPacker.nsi
#                        - MediaPortalUpdater.nsi
#
#**********************************************************************************************************#

# The following commands needs to be defined by the parent script (the one, which includes this file).
;!define BUILD_MediaPortal
;!define BUILD_TVServer
;!define BUILD_DeployTool
;!define BUILD_Installer

!define svn_UninstallFilelist "${svn_ROOT}\Tools\Script & Batch tools\UninstallFilelist"

# At first build UninstallFilelist.exe
!system '"$%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=Release "${svn_UninstallFilelist}\UninstallFilelist.sln"' = 0
# execute UninstallFilelist.exe, it will create the heasder file with uninstall commands
!system '"${svn_UninstallFilelist}\UninstallFilelist\bin\Release\UninstallFilelist.exe" /dir="${svn_MP}\MediaPortal.Base" /ignore="${svn_MP}\Setup\uninstall-ignore.txt" /output="${svn_MP}\Setup\uninstall.nsh"' = 0

!undef svn_UninstallFilelist
