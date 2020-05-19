#region Copyright (C) 2005-2020 Team MediaPortal
/*
// Copyright (C) 2005-2020 Team MediaPortal
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
#                         MediaPortal's Current Version
#
#    This file is included in:
#       - MediaPortal setup
#       
#       
#
#**********************************************************************************************************#

# Correspond to nuget Package

#**********************************************************************************************************#
!include ${git_InstallScripts}\include\CompileTimeIfFileExist.nsh


!macro macro_build_libbluray_jar

!ifndef $%ANT_HOME%
!define ANT_HOME "${git_NugetPackages}\ANT.1.10.7\tools\bin\"
!echo "BUILD MESSAGE: ant_home variable system not found - Using ANT Nuget Package"
!else
!echo "local ANT_HOME system variable found to $%ANT_HOME% and use it"
!define ANT_HOME ""
!endif
!system '${ANT_HOME}ant -f ${LibblurayJAR} -Dsrc_awt=:java-j2se' = 0

!macroend 

Section libbluray
	; Order of detection 
	; 1: Forced nuget package to use with MediaPortal.libbluray_to_use.txt in root nuget packages folder (Value set to 0.0.0 will force build)
	; 2: Use Nuget version set in BDReader project
	; 3: Build libbluray from git submodule directly
	
	;Force Mode
	;using FORCE_BUILD_Libbluray_JAR from nsi file
	

# check if files exists 
!insertmacro CompileTimeIfFileExist "${git_NugetPackages}\MediaPortal.libbluray_to_use.txt" force_libbluray_version_is_present
!insertmacro CompileTimeIfFileExist "${git_DirectShowFilters}\BDReader\packages.config" BDReader_packages_config_is_present
!insertmacro CompileTimeIfFileExist "${git_Libbluray}\src\libbluray\bluray-version.h" bluray-version_h_is_present
!insertmacro CompileTimeIfFileExist "${git_Libbluray}\libbluray.vcxproj" libbluray_vcxproj_is_present
	
!ifdef force_libbluray_version_is_present
	# file MediaPortal.libbluray_to_use.txt must contain text as example: #define BLURAY_VERSION_STRING "1.1.2" if set to 0.0.0 it will use git libbluray
	!searchparse /noerrors /file "${git_NugetPackages}\MediaPortal.libbluray_to_use.txt" `#define BLURAY_VERSION_STRING "` GIT_LIBBLURAY_VERSION `"`
	!echo "BUILD MESSAGE: MediaPortal.libbluray_to_use.txt point to ${GIT_LIBBLURAY_VERSION} and found in ${git_NugetPackages}"
		!if ${GIT_LIBBLURAY_VERSION} != "0.0.0"
			!define _Libbluray_NugetCheck
			!echo "BUILD MESSAGE: MediaPortal.libbluray_to_use.txt defined to ${GIT_LIBBLURAY_VERSION}"
		!else
			!echo "BUILD MESSAGE: MediaPortal.libbluray_to_use.txt set as forced build with 0.0.0 value"
		!endif
!else
	# Give libbluray Nuget package from package.config BDReader project
	!ifdef BDReader_packages_config_is_present
		!searchparse /noerrors /file "${git_DirectShowFilters}\BDReader\packages.config" `<package id="MediaPortal.libbluray" version="` GIT_LIBBLURAY_VERSION `"`
		!echo "BUILD MESSAGE: Libbluray version read from BDReader project : ${GIT_LIBBLURAY_VERSION}"
		!ifdef GIT_LIBBLURAY_VERSION
		!define _Libbluray_NugetCheck
		!else 
		!echo "BUILD MESSAGE: Libbluray version read from BDReader project is wrong, use build libbluray"
		!endif
	!else
		# Give libbluray version from bluray-version file available in libbluray submodule
		!ifdef bluray-version_h_is_present
			!searchparse /noerrors /file "${git_Libbluray}\src\libbluray\bluray-version.h" `#define BLURAY_VERSION_STRING "` GIT_LIBBLURAY_VERSION `"`
			!echo "BUILD MESSAGE: Local Git Libbluray Version : ${GIT_LIBBLURAY_VERSION}"
			!define _Libbluray_NugetCheck
		!else 
			!echo "BUILD MESSAGE: Bluray-version.h is missing, build will stop here"
		!endif
	!endif
!endif

# Check if Nuget package is the same version than Git Libbluray submodule otherwise build lib with MP
!ifdef _Libbluray_NugetCheck
!insertmacro CompileTimeIfFileExist "${git_NugetPackages}\MediaPortal.libbluray.${GIT_LIBBLURAY_VERSION}\" libbluray_nuget_is_present
!endif

!ifdef FORCE_BUILD_Libbluray_JAR
!ifdef libbluray_nuget_is_present
!undef libbluray_nuget_is_present
!endif
!echo "BUILD MESSAGE : Flag Force Build detected"
!endif

# If nuget present and not forced, use it, else build with installer
!ifdef libbluray_nuget_is_present
	!echo "BUILD MESSAGE: Libbluray Nuget Package is present and match to wanted version : ${GIT_LIBBLURAY_VERSION} "
	!define Libbluray_use_Nuget_JAR
	!ifndef libbluray_vcxproj_is_present || BUILD_Libbluray_DLL
	!define Libbluray_use_Nuget_DLL
	!else 
	!define Libbluray_use_Build
	!endif
	!define Libbluray_nuget_path "${git_NugetPackages}\MediaPortal.libbluray.${GIT_LIBBLURAY_VERSION}"
!else 
	!echo "BUILD MESSAGE: Libbluray build forced by user or missing nuget package > only using local git submodule"
	!insertmacro macro_Build_libbluray_jar	
	!searchparse /noerrors /file "${git_Libbluray}\src\libbluray\bluray-version.h" `#define BLURAY_VERSION_STRING "` GIT_LIBBLURAY_VERSION `"`
	!echo "BUILD MESSAGE: Local Git Libbluray Version : ${GIT_LIBBLURAY_VERSION}"
    !define Libbluray_use_Build
!endif
SectionEnd