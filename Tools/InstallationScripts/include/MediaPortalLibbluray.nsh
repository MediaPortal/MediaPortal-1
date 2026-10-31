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
#		- DeployToolUnpacker
#       
#       
#
#**********************************************************************************************************#

# Correspond to nuget Package

#**********************************************************************************************************#
!ifndef CompileTimeIfFileExist_loaded 
!include ${git_InstallScripts}\include\CompileTimeIfFileExist.nsh
!endif 

#---------------------------------------------------------------------------
# libbluray comes only from the NuGet package MediaPortal.libbluray (BDReader\packages.config).
# The package is restored by NuGet before the installer is built.
# Defines: GIT_LIBBLURAY_VERSION, Libbluray_nuget_path
#---------------------------------------------------------------------------
!ifndef Libbluray_nuget_path
!insertmacro CompileTimeIfFileExist "${git_DirectShowFilters}\BDReader\packages.config" BDReader_packages_config_is_present
!ifndef BDReader_packages_config_is_present
	!error "BuildError: ${git_DirectShowFilters}\BDReader\packages.config is missing"
!endif

!searchparse /noerrors /file "${git_DirectShowFilters}\BDReader\packages.config" `<package id="MediaPortal.libbluray" version="` GIT_LIBBLURAY_VERSION `"`
!ifndef GIT_LIBBLURAY_VERSION
	!error "BuildError: package MediaPortal.libbluray is not listed in BDReader\packages.config"
!endif

!define Libbluray_nuget_path "${git_NugetPackages}\MediaPortal.libbluray.${GIT_LIBBLURAY_VERSION}"
!insertmacro CompileTimeIfFileExist "${Libbluray_nuget_path}\" libbluray_nuget_is_present
!ifndef libbluray_nuget_is_present
	!error "BuildError: NuGet package MediaPortal.libbluray ${GIT_LIBBLURAY_VERSION} is not restored in ${git_NugetPackages}"
!endif
!echo "BUILD MESSAGE: Libbluray NuGet package ${GIT_LIBBLURAY_VERSION}"
!endif
