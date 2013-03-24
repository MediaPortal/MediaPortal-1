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

!define ALToolPath "%WINDOWS_SDK%\Bin"

# The following commands needs to be defined by the parent script (the one, which includes this file).
;!define BUILD_MediaPortal
;!define BUILD_TVServer
;!define BUILD_DeployTool
;!define BUILD_Installer


!if ${VER_BUILD} != 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_MP}"' = 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_TVServer}"' = 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_ROOT}\Common-MP-TVE3"' = 0
!endif

!ifdef BUILD_MediaPortal
!system '"$%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:rebuild /property:Configuration=Release ${git_DirectShowFilters}\Filters.sln' = 0
!system '"$%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /tv:3.5 /p:TargetFrameworkVersion=v3.5 /target:Rebuild /property:Configuration=Release;Platform=x86 "${git_MP}\MediaPortal.sln"' = 0
!endif

!ifdef BUILD_TVServer
!system '"$%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /tv:3.5 /p:TargetFrameworkVersion=v3.5 /target:Rebuild /property:Configuration=Release;Platform=x86 "${git_TVServer}\TvLibrary.sln"' = 0
!system '"$%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /tv:3.5 /p:TargetFrameworkVersion=v3.5 /target:Rebuild /property:Configuration=Release;Platform=x86 "${git_TVServer}\TvPlugin\TvPlugin.sln"' = 0
!endif

!if ${VER_BUILD} != 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_MP}"  /revert' = 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_TVServer}"  /revert' = 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_ROOT}\Common-MP-TVE3"  /revert' = 0
!endif

!ifdef BUILD_DeployTool
!system '"$%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /tv:3.5 /p:TargetFrameworkVersion=v3.5 /p:ALToolPath="${ALToolPath}" /target:Rebuild /property:Configuration=Release;Platform=x86 "${git_DeployTool}\MediaPortal.DeployTool.sln"' = 0
!endif

!ifdef BUILD_Installer
!system '"${NSISDIR}\makensis.exe" "${git_MP}\Setup\setup.nsi"' = 0
!system '"${NSISDIR}\makensis.exe" "${git_TVServer}\Setup\setup.nsi"' = 0
!endif
