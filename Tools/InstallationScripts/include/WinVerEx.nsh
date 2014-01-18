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
#                         WinVer Extended
#
#**********************************************************************************************************#


!verbose push
!verbose 3

!ifndef ___WINVER_EX__NSH___
!define ___WINVER_EX__NSH___

!include WinVer.nsh

!define WINVER_8_NT      0x86020000 ;6.02.9200
!define WINVER_8         0x06020000 ;6.02.9200
!define WINVER_81        0x06030000 ;6.03.9600
!define WINVER_2012_NT   0x86020001 ;6.02.9200
!define WINVER_2012      0x06020001 ;6.02.9200
 
!macro __WinVer_DefineOSTestsEx Test Suffix
!insertmacro __WinVer_DefineOSTest ${Test} 8      '${Suffix}'
!insertmacro __WinVer_DefineOSTest ${Test} 81     '${Suffix}'
!insertmacro __WinVer_DefineOSTest ${Test} 2012   '${Suffix}'
!macroend
 
!insertmacro __WinVer_DefineOSTestsEx AtLeast ""
!insertmacro __WinVer_DefineOSTestsEx Is _NT
!insertmacro __WinVer_DefineOSTestsEx AtMost ""

!macro GetServicePack _major _minor

  Push $0
  Push $1
  Push $2
  Push $3
  System::Call '*(i 148,i,i,i,i,&t128,&i2,&i2,&i2,&i1,&i1)i.r1' ;BUGBUG: no error handling for mem alloc failure!
  System::Call 'kernel32::GetVersionEx(i r1)'

  ; using the service pack major number
  ;System::Call '*$1(i,i,i,i,i,&t128,&i2.r0)'

  ; result is:
  ; "Service Pack 3"         for final Service Packs
  ; "Service Pack 3, v.3311" for beta  Service Packs
  System::Call '*$1(i,i,i,i,i,&t128.r0)'

  ;uncomment for testing
  ;StrCpy $0 "Service Pack 3"
  ;StrCpy $0 "Service Pack 3, v.3311"

  ; split the string by "." and save the word count in $2
  ; if no . is found in $2 the input string (was $0) is saved
  ${WordFind} "$0" "." "#" $2

  ; if $0 = $2 -> no "." was found -> no beta
  ${If} "$2" == "$0"
    StrCpy ${_major} $0 1 -1   ;  "Service Pack 3"
    StrCpy ${_minor} 0
  ${Else}
    StrCpy ${_major} 0
    ;split again, and use the second word as minorVer
    ${WordFind} "$0" "." "+2" ${_minor}  ;  "Service Pack 3, v.3311"
  ${EndIf}

  ;MessageBox MB_OK|MB_ICONEXCLAMATION "Service Pack: >${_major}< >${_minor}<"

  System::Free $1
  pop $3
  pop $2
  pop $1
  pop $0

!macroend

!endif # !___WINVER_EX__NSH___

!verbose pop