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

!ifndef IrssSystemRegistry_INCLUDED
!define IrssSystemRegistry_INCLUDED

!define AutoRunPath 'HKCU "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"'

!macro SetAutoRun name executablePath
  WriteRegStr ${AutoRunPath} '${name}' '${executablePath}'
!macroend

!macro RemoveAutoRun name
  WriteRegStr ${AutoRunPath} '${name}' ''
!macroend

!macro GetAutoRun name variable
  ReadRegStr ${variable} ${AutoRunPath} '${name}'
!macroend

!macro _IsAutoRun _a _b _t _f
  !insertmacro _LOGICLIB_TEMP

  !insertmacro GetAutoRun `${_b}` $_LOGICLIB_TEMP
  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define IsAutoRun `"" IsAutoRun`

!endif # !IrssSystemRegistry_INCLUDED
