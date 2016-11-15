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
#
#                         MediaPortal's NSIS initialization
#
#    This file is included in:
#       - MediaPortal setup
#       - TVServer setup
#       - DeployTool unpacker
#
#     Here are commonly used special builds and paths defined.
#
#**********************************************************************************************************#
!echo "%COMPUTERNAME% = $%COMPUTERNAME%"


##### BUILD_TYPE
# Uncomment the following line to create a setup in debug mode
;!define BUILD_TYPE "Debug"
# parameter for command line execution: /DBUILD_TYPE=Debug
# by default BUILD_TYPE is set to "Release"
!ifndef BUILD_TYPE
  !define BUILD_TYPE "Release"
!endif


##### path definitions

!define git_MP "${git_ROOT}\mediaportal"
!define git_TVServer "${git_ROOT}\TvEngine3\TVLibrary"
!define git_Common_MP_TVE3 "${git_ROOT}\Common-MP-TVE3"
!define git_DeployTool "${git_ROOT}\Tools\MediaPortal.DeployTool"
!define git_DirectShowFilters "${git_ROOT}\DirectShowFilters"
!define git_Libbluray "${git_ROOT}\libbluray"
!define LibblurayJAR "${git_ROOT}\libbluray\src\libbluray\bdj"

!define git_TvEngine2 "${git_ROOT}\TvEngine2"

!define git_DeployVersionGIT "${git_ROOT}\Tools\Script & Batch tools\DeployVersionGIT"

#code after build scripts are fixed
!if "$%COMPUTERNAME%" != "S15341228"
!define git_OUT "${git_ROOT}\Release"
!else

#code before build scripts are fixed
!if "${SKRIPT_NAME}" == "MediaPortal"
  !define git_OUT "${git_MP}\Setup\Release"
!else

  !if "${SKRIPT_NAME}" == "MediaPortal TV Server / Client"
    !define git_OUT "${git_TVServer}\Setup\Release"
  !else

    !if "${SKRIPT_NAME}" == "MediaPortal Unpacker"
      !define git_OUT "${git_InstallScripts}"
    !endif

  !endif
!endif
#end of workaound code
!endif

!system 'mkdir "${git_OUT}"'
