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
#                         WinVer Extended
#
# code was taken from:
#                         https://sourceforge.net/tracker/?func=detail&atid=373088&aid=1949260&group_id=22049
#
#**********************************************************************************************************#

; LogicLib extensions for handling Windows versions.
;
; IsNT checks if the installer is running on Windows NT family (NT4, 2000, XP, etc.)
;
;   ${If} ${IsNT}
;     DetailPrint "Running on NT. Installing Unicode enabled application."
;   ${Else}
;     DetailPrint "Not running on NT. Installing ANSI application."
;   ${EndIf}
;
; AtLeastWin<version> checks if the installer is running on Windows version at least as specified.
; IsWin<version> checks if the installer is running on Windows version exactly as specified.
; AtMostWin<version> checks if the installer is running on Windows version at most as specified.
;
; <version> can be replaced with the following values:
;
;   95
;   98
;   ME
;
;   NT4
;   2000
;   2000Srv
;   XP
;   XP64
;   2003
;   Vista
;   2008
;
; Usage examples:
;
;   ${If} ${IsNT}
;   DetailPrint "Running on NT family."
;   DetailPrint "Surely not running on 95, 98 or ME."
;   ${AndIf} ${AtLeastWinNT4}
;     DetailPrint "Running on NT4 or better. Could even be 2003."
;   ${EndIf}
;
;   ${If} ${AtLeastWinXP}
;     DetailPrint "Running on XP or better."
;   ${EndIf}
;
;   ${If} ${IsWin2000}
;     DetailPrint "Running on 2000."
;   ${EndIf}
;
;   ${If} ${AtMostWinXP}
;     DetailPrint "Running on XP or older. Surely not running on Vista. Maybe 98, or even 95."
;   ${EndIf}
;
; Warning:
;
;   Windows 95 and NT both use the same version number. To avoid getting NT4 misidentified
;   as Windows 95 and vice-versa or 98 as a version higher than NT4, always use IsNT to
;   check if running on the NT family.
;
;     ${If} ${AtLeastWin95}
;     ${And} ${AtMostWinME}
;       DetailPrint "Running 95, 98 or ME."
;       DetailPrint "Actually, maybe it's NT4?"
;       ${If} ${IsNT}
;         DetailPrint "Yes, it's NT4! oops..."
;       ${Else}
;         DetailPrint "Nope, not NT4. phew..."
;       ${EndIf}
;     ${EndIf}

#**********************************************************************************************************#

!verbose push
!verbose 3

!ifndef ___WINVER__NSH___
!define ___WINVER__NSH___

!include LogicLib.nsh

!include WordFunc.nsh
!insertmacro WordFind
!insertmacro un.WordFind

!define WINVER_95      0x4000
!define WINVER_98      0x40A0 ;4.10
!define WINVER_ME      0x45A0 ;4.90

!define WINVER_NT4     0x4000
!define WINVER_2000    0x5000
!define WINVER_2000Srv 0x5001
!define WINVER_XP      0x5010
!define WINVER_XP64    0x5020
!define WINVER_2003    0x5021
!define WINVER_VISTA   0x6000
!define WINVER_2008    0x6001


!macro __ParseWinVer
	!insertmacro _LOGICLIB_TEMP
	Push $0
	Push $1
	System::Call '*(i 148,i,i,i,i,&t128,&i2,&i2,&i2,&i1,&i1)i.r1' ;BUGBUG: no error handling for mem alloc failure!
	System::Call 'kernel32::GetVersionEx(i r1)'
	System::Call '*$1(i,i.r0)'
	!define _WINVER_PARSEVER_OLDSYS _WINVER${__LINE__}
	IntCmpU $0 5 0 ${_WINVER_PARSEVER_OLDSYS} ;OSVERSIONINFOEX can be used on NT4SP6 and later, but we only use it on NT5+
	System::Call '*$1(i 156)'
	System::Call 'kernel32::GetVersionEx(i r1)'
	${_WINVER_PARSEVER_OLDSYS}:
	!undef _WINVER_PARSEVER_OLDSYS
	IntOp $_LOGICLIB_TEMP $0 << 12 ;we already have the major version in r0
	System::Call '*$1(i,i,i.r0)'
	IntOp $0 $0 << 4
	IntOp $_LOGICLIB_TEMP $_LOGICLIB_TEMP | $0
	System::Call '*$1(i,i,i,i,i,&t128,&i2,&i2,&i2,&i1.r0,&i1)'
	!define _WINVER_PARSEVER_NOTNTSERVER _WINVER${__LINE__}
	IntCmp $0 1 ${_WINVER_PARSEVER_NOTNTSERVER} ${_WINVER_PARSEVER_NOTNTSERVER} ;oviex.wProductType > VER_NT_WORKSTATION
	IntOp $_LOGICLIB_TEMP $_LOGICLIB_TEMP | 1
	${_WINVER_PARSEVER_NOTNTSERVER}:
	!undef _WINVER_PARSEVER_NOTNTSERVER
	System::Free $1
	pop $1
	pop $0
!macroend

!macro _IsNT _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  System::Call kernel32::GetVersion()i.s
  Pop $_LOGICLIB_TEMP
  IntOp $_LOGICLIB_TEMP $_LOGICLIB_TEMP & 0x80000000
  !insertmacro _== $_LOGICLIB_TEMP 0 `${_t}` `${_f}`
!macroend
!define IsNT `"" IsNT ""`

!macro __WinVer_DefineOSTest Test OS

  !define ${Test}Win${OS} `"" WinVer${Test} ${WINVER_${OS}}`

!macroend

!macro __WinVer_DefineOSTests Test

  !insertmacro __WinVer_DefineOSTest ${Test} 95
  !insertmacro __WinVer_DefineOSTest ${Test} 98
  !insertmacro __WinVer_DefineOSTest ${Test} ME
  !insertmacro __WinVer_DefineOSTest ${Test} NT4
  !insertmacro __WinVer_DefineOSTest ${Test} 2000
  !insertmacro __WinVer_DefineOSTest ${Test} 2000Srv
  !insertmacro __WinVer_DefineOSTest ${Test} XP
  !insertmacro __WinVer_DefineOSTest ${Test} XP64
  !insertmacro __WinVer_DefineOSTest ${Test} 2003
  !insertmacro __WinVer_DefineOSTest ${Test} VISTA
  !insertmacro __WinVer_DefineOSTest ${Test} 2008

!macroend

!macro _WinVerAtLeast _a _b _t _f
  !insertmacro __ParseWinVer
  !insertmacro _>= $_LOGICLIB_TEMP `${_b}` `${_t}` `${_f}`
!macroend

!macro _WinVerIs _a _b _t _f
  !insertmacro __ParseWinVer
  !insertmacro _= $_LOGICLIB_TEMP `${_b}` `${_t}` `${_f}`
!macroend

!macro _WinVerAtMost _a _b _t _f
  !insertmacro __ParseWinVer
  !insertmacro _<= $_LOGICLIB_TEMP `${_b}` `${_t}` `${_f}`
!macroend

!insertmacro __WinVer_DefineOSTests AtLeast
!insertmacro __WinVer_DefineOSTests Is
!insertmacro __WinVer_DefineOSTests AtMost



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

!endif # !___WINVER__NSH___

!verbose pop