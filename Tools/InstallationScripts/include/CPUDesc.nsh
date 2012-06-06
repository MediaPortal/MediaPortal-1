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

!ifndef CPUDesc_INCLUDED
!define CPUDesc_INCLUDED

!AddPluginDir "${git_InstallScripts}\CPUDesc-plugin\Plugin"
!verbose 3

# To be used with LogicLib
!macro _SSE2Supported _a _b _t _f
  cpudesc::tell
  Pop $0

  StrCpy $1 $0 1 41
  StrCmp $1 "1" `${_t}` `${_f}`
!macroend

!define SSE2Supported `"" SSE2Supported ""`

!endif # !CPUDesc_INCLUDED
