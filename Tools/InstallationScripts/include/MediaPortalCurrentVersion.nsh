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
!define VER_MINOR       2
!define VER_REVISION    0
!define VER_TYPE        ""                 # can be "RC", "Beta", and similar....


!ifndef VER_BUILD
    !define VER_BUILD   0
!endif

!define VERSION "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION} ${VER_TYPE}"

!if ${BUILD_TYPE} == "Debug"               # it's a debug release
    !define VER_DEBUG = ">>DEBUG<<"
!endif

!if ${VER_BUILD} != 0                      # it's a svn release
      !define VERSION "${VERSION} ${VER_DEBUG} build ${VER_BUILD} for TESTING ONLY"
!endif
