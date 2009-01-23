#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

#**********************************************************************************************************#
#
#                         File Association
#
# code was taken from:
#                         http://nsis.sourceforge.net/File_Association
#
#**********************************************************************************************************#

# USAGE:

;!include "registerExtension.nsh"
;...
# later, inside a section:
;${registerExtension} "c:\myplayer.exe" ".mkv" "MKV File"

;${unregisterExtension} ".mkv" "MKV File"

#**********************************************************************************************************#

!ifndef ___FILE_ASSOCIATION__NSH___
!define ___FILE_ASSOCIATION__NSH___


!define RegisterExtension `!insertmacro RegisterExtension ""`
!define un.RegisterExtension `!insertmacro RegisterExtension "un."`
!define UnRegisterExtension `!insertmacro UnRegisterExtension ""`
!define un.UnRegisterExtension `!insertmacro UnRegisterExtension "un."`

!macro ___RegisterExtension___
!define Index "Line${__LINE__}"
  pop $R0 ; ext name
  pop $R1
  pop $R2
  push $1
  push $0
  ReadRegStr $1 HKCR $R1 ""
  StrCmp $1 "" "${Index}-NoBackup"
    StrCmp $1 "OptionsFile" "${Index}-NoBackup"
    WriteRegStr HKCR $R1 "backup_val" $1
"${Index}-NoBackup:"
  WriteRegStr HKCR $R1 "" $R0
  ReadRegStr $0 HKCR $R0 ""
  StrCmp $0 "" 0 "${Index}-Skip"
	WriteRegStr HKCR $R0 "" $R0
	WriteRegStr HKCR "$R0\shell" "" "open"
	WriteRegStr HKCR "$R0\DefaultIcon" "" "$R2,0"
"${Index}-Skip:"
  WriteRegStr HKCR "$R0\shell\open\command" "" '$R2 "%1"'
  WriteRegStr HKCR "$R0\shell\edit" "" "Edit $R0"
  WriteRegStr HKCR "$R0\shell\edit\command" "" '$R2 "%1"'
  pop $0
  pop $1
!undef Index
!macroend

!macro ___UnRegisterExtension___
  pop $R1 ; description
  pop $R0 ; extension
!define Index "Line${__LINE__}"
  ReadRegStr $1 HKCR $R0 ""
  StrCmp $1 $R1 0 "${Index}-NoOwn" ; only do this if we own it
  ReadRegStr $1 HKCR $R0 "backup_val"
  StrCmp $1 "" 0 "${Index}-Restore" ; if backup="" then delete the whole key
  DeleteRegKey HKCR $R0
  Goto "${Index}-NoOwn"
"${Index}-Restore:"
  WriteRegStr HKCR $R0 "" $1
  DeleteRegValue HKCR $R0 "backup_val"
  DeleteRegKey HKCR $R1 ;Delete key with association name settings
"${Index}-NoOwn:"
!undef Index
!macroend
 
 
Function RegisterExtension
  !insertmacro ___RegisterExtension___
FunctionEnd
Function un.RegisterExtension
  !insertmacro ___RegisterExtension___
FunctionEnd

Function UnRegisterExtension
  !insertmacro ___UnRegisterExtension___
FunctionEnd
Function un.UnRegisterExtension
  !insertmacro ___UnRegisterExtension___
FunctionEnd


!macro RegisterExtension UNINSTALL_PREFIX executable extension description
       Push "${executable}"  ; "full path to my.exe"
       Push "${extension}"   ;  ".mkv"
       Push "${description}" ;  "MKV File"
       Call ${UNINSTALL_PREFIX}RegisterExtension
!macroend
  
!macro UnRegisterExtension UNINSTALL_PREFIX extension description
       Push "${extension}"   ;  ".mkv"
       Push "${description}"   ;  "MKV File"
       Call ${UNINSTALL_PREFIX}UnRegisterExtension
!macroend

!endif # !___FILE_ASSOCIATION__NSH___

