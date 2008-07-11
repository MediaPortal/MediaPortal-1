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
#                         .NET Framework Detection
#
# code was taken from:
#                         http://nsis.sourceforge.net/How_to_Detect_any_.NET_Framework
#
#**********************************************************************************************************#

# USAGE:

;!define MIN_FRA_MAJOR "3"
;!define MIN_FRA_MINOR "0"
;!define MIN_FRA_BUILD "*"
;;NB Use an asterisk to match anything.
;Call AbortIfBadFramework
;; No pops. It just aborts inside the function, or returns if all is well.
;; Change this if you like.

#**********************************************************************************************************#

Function AbortIfBadFramework
 
  ;Save the variables in case something else is using them
  Push $0
  Push $1
  Push $2
  Push $3
  Push $4
  Push $R1
  Push $R2
  Push $R3
  Push $R4
  Push $R5
  Push $R6
  Push $R7
  Push $R8
 
  StrCpy $R5 "0"
  StrCpy $R6 "0"
  StrCpy $R7 "0"
  StrCpy $R8 "0.0.0"
  StrCpy $0 0
 
  loop:
 
  ;Get each sub key under "SOFTWARE\Microsoft\NET Framework Setup\NDP"
  EnumRegKey $1 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP" $0
  StrCmp $1 "" done ;jump to end if no more registry keys
  IntOp $0 $0 + 1
  StrCpy $2 $1 1 ;Cut off the first character
  StrCpy $3 $1 "" 1 ;Remainder of string
 
  ;Loop if first character is not a 'v'
  StrCmpS $2 "v" start_parse loop
 
  ;Parse the string
  start_parse:
  StrCpy $R1 ""
  StrCpy $R2 ""
  StrCpy $R3 ""
  StrCpy $R4 $3
 
  StrCpy $4 1
 
  parse:
  StrCmp $3 "" parse_done ;If string is empty, we are finished
  StrCpy $2 $3 1 ;Cut off the first character
  StrCpy $3 $3 "" 1 ;Remainder of string
  StrCmp $2 "." is_dot not_dot ;Move to next part if it's a dot
 
  is_dot:
  IntOp $4 $4 + 1 ; Move to the next section
  goto parse ;Carry on parsing
 
  not_dot:
  IntCmp $4 1 major_ver
  IntCmp $4 2 minor_ver
  IntCmp $4 3 build_ver
  IntCmp $4 4 parse_done
 
  major_ver:
  StrCpy $R1 $R1$2
  goto parse ;Carry on parsing
 
  minor_ver:
  StrCpy $R2 $R2$2
  goto parse ;Carry on parsing
 
  build_ver:
  StrCpy $R3 $R3$2
  goto parse ;Carry on parsing
 
  parse_done:
 
  IntCmp $R1 $R5 this_major_same loop this_major_more
  this_major_more:
  StrCpy $R5 $R1
  StrCpy $R6 $R2
  StrCpy $R7 $R3
  StrCpy $R8 $R4
 
  goto loop
 
  this_major_same:
  IntCmp $R2 $R6 this_minor_same loop this_minor_more
  this_minor_more:
  StrCpy $R6 $R2
  StrCpy $R7 R3
  StrCpy $R8 $R4
  goto loop
 
  this_minor_same:
  IntCmp R3 $R7 loop loop this_build_more
  this_build_more:
  StrCpy $R7 $R3
  StrCpy $R8 $R4
  goto loop
 
  done:
 
  ;Have we got the framework we need?
  IntCmp $R5 ${MIN_FRA_MAJOR} max_major_same fail end
  max_major_same:
  IntCmp $R6 ${MIN_FRA_MINOR} max_minor_same fail end
  max_minor_same:
  IntCmp $R7 ${MIN_FRA_BUILD} end fail end
 
  fail:
  StrCmp $R8 "0.0.0" no_framework
  goto wrong_framework
 
  no_framework:
  MessageBox MB_OK|MB_ICONSTOP "Installation failed.$\n$\n\
         This software requires Windows Framework version \
         ${MIN_FRA_MAJOR}.${MIN_FRA_MINOR}.${MIN_FRA_BUILD} or higher.$\n$\n\
         No version of Windows Framework is installed.$\n$\n\
         Please update your computer at http://windowsupdate.microsoft.com/."
  abort
 
  wrong_framework:
  MessageBox MB_OK|MB_ICONSTOP "Installation failed!$\n$\n\
         This software requires Windows Framework version \
         ${MIN_FRA_MAJOR}.${MIN_FRA_MINOR}.${MIN_FRA_BUILD} or higher.$\n$\n\
         The highest version on this computer is $R8.$\n$\n\
         Please update your computer at http://windowsupdate.microsoft.com/."
  abort
 
  end:
 
  ;Pop the variables we pushed earlier
  Pop $R8
  Pop $R7
  Pop $R6
  Pop $R5
  Pop $R4
  Pop $R3
  Pop $R2
  Pop $R1
  Pop $4
  Pop $3
  Pop $2
  Pop $1
  Pop $0
 
FunctionEnd