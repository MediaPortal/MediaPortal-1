#region Copyright (C) 2005-2024 Team MediaPortal
/*
// Copyright (C) 2005-2024 Team MediaPortal
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

!ifndef Architecture
  !define Architecture x86
!endif

!ifdef x64Environment
  !define Prog_Path '$%ProgramFiles(x86)%'
!else
  !define Prog_Path '$%ProgramFiles%'
!endif

!include ${git_InstallScripts}\include\CompileTimeIfFileExist.nsh

!define ALToolPath "${Prog_Path}\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools"

!insertmacro CompileTimeIfFileExist "${Prog_Path}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" VS2019Community
# !insertmacro CompileTimeIfFileExist "${Prog_Path}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" VS2019Buildtools
!ifdef VS2019Community
  !define MSBuild_Path "${Prog_Path}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
!else
  !define MSBuild_Path "${Prog_Path}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
!endif

# The following commands needs to be defined by the parent script (the one, which includes this file).
;!define BUILD_MediaPortal
;!define BUILD_TVServer
;!define BUILD_DeployTool
;!define BUILD_Installer

!macro PrepareBuildReport Project
  !define BuildReport '${git_ROOT}\Build\BuildReport'
  !define xml '${git_OUT}\Build_Report_${Project}.xml'
  !define html '${git_OUT}\Build_Report_${Project}.html'
  !define logger '/l:XmlFileLogger,"${BuildReport}\MSBuild.ExtensionPack.Loggers.dll";logfile=${xml}'

  !system 'xcopy /I /Y "${BuildReport}\_BuildReport_Files" "${git_OUT}\_BuildReport_Files"'
!macroend

!macro FinalizeBuildReport
  !system '"${BuildReport}\msxsl.exe" "${xml}" "${BuildReport}\_BuildReport_Files\BuildReport.xslt" -o "${html}"' = 0
  !undef BuildReport
  !undef xml
  !undef html
  !undef logger
!macroend

!if ${VER_BUILD} != 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_MP}"' = 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_TVServer}"' = 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_ROOT}\Common-MP-TVE3"' = 0
!endif

!system '"${MSBuild_Path}" "${git_ROOT}\Build\RestorePackages.targets"' = 0

; Build MP
!ifdef BUILD_MediaPortal
!if "${Architecture}" == "x64"
  !define FilterArchitecture x64
!else
  !define FilterArchitecture Win32
!endif
!include "${git_InstallScripts}\include\MediaPortalLibbluray.nsh"
!ifdef libbluray_vcxproj_is_present && Libbluray_use_Build
!insertmacro PrepareBuildReport libbluray
!system '"${MSBuild_Path}"  /p:PlatformToolset=v142 ${logger} /target:rebuild /property:Configuration=Release_libbluray;Platform=${FilterArchitecture} "${git_DirectShowFilters}\Filters.sln"' = 0
!insertmacro FinalizeBuildReport
!endif
!insertmacro PrepareBuildReport DirectShowFilters
!system '"${MSBuild_Path}" ${logger} /target:rebuild /property:Configuration=Release;Platform=${FilterArchitecture} "${git_DirectShowFilters}\Filters.sln"' = 0
!insertmacro FinalizeBuildReport

!insertmacro PrepareBuildReport MediaPortal
!system '"${MSBuild_Path}" ${logger} /target:Rebuild /property:Configuration=Release;Platform=${Architecture} "${git_MP}\MediaPortal.sln"' = 0
!insertmacro FinalizeBuildReport
!insertmacro PrepareBuildReport MPx86Proxy
!system '"${MSBuild_Path}" ${logger} /target:Rebuild /property:Configuration=Release;Platform=x86 "${git_ROOT}\Tools\MPx86Proxy\MPx86Proxy.sln"' = 0
!insertmacro FinalizeBuildReport
!endif

!if ${VER_BUILD} != 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_MP}"  /revert' = 0
!endif

; Build MP installer
!ifdef BUILD_Installer
;!system '${git_ROOT}\Build\MSBUILD_MP_LargeAddressAware.bat Release' = 0
!system '"${NSISDIR}\makensis.exe" /V3 /DBUILD_TYPE=Release /DArchitecture=${Architecture} "${git_MP}\Setup\setup.nsi" > ${git_ROOT}\Build\BuildMediaportal.log' = 0
!endif

; Build TV server
!ifdef BUILD_TVServer
!insertmacro PrepareBuildReport TvPlugin
!system '"${MSBuild_Path}" ${logger} /target:Rebuild /property:Configuration=Release;Platform="Any CPU" "${git_TVServer}\TvPlugin\TvPlugin.sln"' = 0
!insertmacro FinalizeBuildReport
!insertmacro PrepareBuildReport TvLibrary
# Use x86 platform target; x64 configuration is not available for tv server yet
#!system '"${MSBuild_Path}" ${logger} /target:Rebuild /property:Configuration=Release;Platform=x86 "${git_TVServer}\TvLibrary.sln"' = 0
!system '"${MSBuild_Path}" ${logger} /target:Rebuild /property:Configuration=Release;Platform=${Architecture} "${git_TVServer}\TvLibrary.sln"' = 0
!insertmacro FinalizeBuildReport
!endif

!if ${VER_BUILD} != 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_TVServer}"  /revert' = 0
!system '"${git_DeployVersionGIT}\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /git="${git_ROOT}" /path="${git_ROOT}\Common-MP-TVE3"  /revert' = 0
!endif

!ifdef BUILD_DeployTool
!insertmacro PrepareBuildReport DeployTool
;!system '"${MSBuild_Path}" ${logger} /p:ALToolPath="${ALToolPath}" /target:Rebuild /property:Configuration=Release;Platform=${Architecture} "${git_DeployTool}\MediaPortal.DeployTool.sln"' = 0
!system '"${MSBuild_Path}" ${logger} /target:Rebuild /property:Configuration=Release;Platform=${Architecture} "${git_DeployTool}\MediaPortal.DeployTool.sln"' = 0
!insertmacro FinalizeBuildReport
!endif

; Build TV installer
!ifdef BUILD_Installer
!system '"${NSISDIR}\makensis.exe" /V3 /DBUILD_TYPE=Release /DArchitecture=${Architecture} "${git_TVServer}\Setup\setup.nsi" > ${git_ROOT}\Build\BuildTVServer.log' = 0
!endif
