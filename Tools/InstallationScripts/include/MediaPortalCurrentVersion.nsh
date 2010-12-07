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
!define VER_MINOR       1
!define VER_REVISION    6
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif

!if "${SKRIPT_NAME}" == "MediaPortal Unpacker"
  ;this is for display purposes
  !define VERSION "1.2.0 Beta"
!else

  !if ${BUILD_TYPE} == "Debug"
    !define VERSION "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION} >>DEBUG<< build ${VER_BUILD} for TESTING ONLY"
  !else

    !if ${VER_BUILD} == 0       # it's an official release
      !define VERSION "1.2.0"
    !else                       # it's a svn release
      !define VERSION "1.2.0 SVN build ${VER_BUILD} for TESTING ONLY"
    !endif

  !endif

!endif
