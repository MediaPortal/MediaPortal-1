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

!define svn_OUT "${svn_ROOT}\Release"

!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_Common_MP_TVE3 "${svn_ROOT}\Common-MP-TVE3"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_DirectShowFilters "${svn_ROOT}\DirectShowFilters"

!define svn_TvEngine2 "${svn_ROOT}\TvEngine2"

!define svn_DeployVersionSVN "${svn_ROOT}\Tools\Script & Batch tools\DeployVersionSVN"

!system 'mkdir "${svn_OUT}"' = 0
