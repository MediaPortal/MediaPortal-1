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
#                         MediaPortal's Current Version
#
#    This file is included in:
#       - MediaPortal setup
#       - TVServer setup
#       - DeployTool unpacker
#
#**********************************************************************************************************#


!define VER_MAJOR       1
!define VER_MINOR       6
!define VER_REVISION    100

#**********************************************************************************************************#

!if ${VER_REVISION} < 100
	!define VER_TYPE ""
	!define SPC ""
	!define VER_MINOR_DISP		${VER_MINOR}
	!define VER_REVISION_DISP	${VER_REVISION}

!else 
	!define SPC " "
	!define /math VER_MINOR_DISP	${VER_MINOR} + 1
	!define VER_REVISION_DISP		0

	!if ${VER_REVISION} == 100
		!define VER_TYPE "Pre Release"
	!else if ${VER_REVISION} < 200
		!define /math ALPHA ${VER_REVISION} - 99
		!define VER_TYPE "Pre Release{$ALPHA}"
		!undef ALPHA
	!else if ${VER_REVISION} == 200
		!define VER_TYPE "Beta"
	!else if ${VER_REVISION} < 300
		!define /math BETA ${VER_REVISION} - 199
		!define VER_TYPE "Beta{$BETA}"
		!undef BETA
	!else if ${VER_REVISION} == 300
		!define VER_TYPE "RC"
	!else
		!define /math RC ${VER_REVISION} - 299
		!define VER_TYPE "RC{$RC}"
		!undef RC
	!endif
!endif
#!define VER_TYPE        "Pre-Release"                 # can be "RC", "Beta". Please comment if Final release


!ifndef VER_BUILD
    !define VER_BUILD   0
!endif

!if ${BUILD_TYPE} == "Debug"               # it's a debug release
    !define VER_DEBUG " >>DEBUG<<"
!else
	!define VER_DEBUG ""
!endif

!if ${VER_BUILD} != 0                      # it's a snapshot release
    !ifdef BRANCH
      !if "${BRANCH}" != ""
        !define VER_BRANCH  " (${BRANCH} branch)"
      !endif
    !endif

    !ifndef VER_BRANCH
      !define VER_BRANCH  ""
    !endif

    !ifndef COMMITTISH
      !define VER_COMMITTISH  ""
    !else
      !define VER_COMMITTISH  "-g${COMMITTISH}"
    !endif

    !define VER_GIT "-${VER_BUILD}${VER_COMMITTISH}${VER_BRANCH}${VER_DEBUG} for TESTING ONLY"
!else
	!define VER_GIT ""
!endif


!define VERSION			"${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}${VER_GIT}"
!define VERSION_DISP	"${VER_MAJOR}.${VER_MINOR_DISP}.${VER_REVISION_DISP}${SPC}${VER_TYPE}${VER_GIT}"
