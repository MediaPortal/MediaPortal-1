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

/*
_____________________________________________________________________________

                       File Association
_____________________________________________________________________________

 Based on code taken from http://nsis.sourceforge.net/File_Association 

 Usage in script:
 1. !include "FileFunc.nsh"
 2. [Section|Function]
      ${FileAssociationFunction} "Param1" "Param2" "..." $var
    [SectionEnd|FunctionEnd]

 FileAssociationFunction=[RegisterExtension|UnRegisterExtension]

_____________________________________________________________________________

 ${RegisterExtension} "[executable]" "[extension]" "[description]"

"[executable]"     ; executable which opens the file format
                   ;
"[extension]"      ; extension, which represents the file format to open
                   ;
"[description]"    ; description for the extension. This will be display in Windows Explorer.
                   ;


 ${UnRegisterExtension} "[extension]" "[description]"

"[extension]"      ; extension, which represents the file format to open
                   ;
"[description]"    ; description for the extension. This will be display in Windows Explorer.
                   ;

_____________________________________________________________________________

                         Macros
_____________________________________________________________________________

 Change log window verbosity (default: 3=no script)

 Example:
 !include "FileAssociation.nsh"
 !insertmacro RegisterExtension
 ${FileAssociation_VERBOSE} 4   # all verbosity
 !insertmacro UnRegisterExtension
 ${FileAssociation_VERBOSE} 3   # no script
*/


!ifndef FileAssociation_INCLUDED
!define FileAssociation_INCLUDED

!include Util.nsh

!verbose push
!verbose 3
!ifndef _FileAssociation_VERBOSE
	!define _FileAssociation_VERBOSE 3
!endif
!verbose ${_FileAssociation_VERBOSE}
!define FileAssociation_VERBOSE `!insertmacro FileAssociation_VERBOSE`
!verbose pop

!macro FileAssociation_VERBOSE _VERBOSE
	!verbose push
	!verbose 3
	!undef _FileAssociation_VERBOSE
	!define _FileAssociation_VERBOSE ${_VERBOSE}
	!verbose pop
!macroend



!macro RegisterExtensionCall _EXECUTABLE _EXTENSION _DESCRIPTION
	!verbose push
	!verbose ${_FileAssociation_VERBOSE}
	Push `${_EXECUTABLE}`
	Push `${_EXTENSION}`
	Push `${_DESCRIPTION}`
	${CallArtificialFunction} RegisterExtension_
	!verbose pop
!macroend

!macro UnRegisterExtensionCall _EXTENSION _DESCRIPTION
	!verbose push
	!verbose ${_FileAssociation_VERBOSE}
	Push `${_EXTENSION}`
	Push `${_DESCRIPTION}`
	${CallArtificialFunction} UnRegisterExtension_
	!verbose pop
!macroend



!define RegisterExtension `!insertmacro RegisterExtensionCall`
!define un.RegisterExtension `!insertmacro RegisterExtensionCall`

!macro RegisterExtension
!macroend

!macro un.RegisterExtension
!macroend

!macro RegisterExtension_
  !verbose push
  !verbose ${_FileAssociation_VERBOSE}

  Exch $R2 ;desc
  Exch
  Exch $R1 ;ext
  Exch
  Exch 2
  Exch $R0 ;exe
  Exch 2
  Push $0
  Push $1

!define Index "Line${__LINE__}"
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
!undef Index

  Pop $1
  Pop $0
  Pop $R2
  Pop $R1
  Pop $R0

  !verbose pop
!macroend



!define UnRegisterExtension `!insertmacro UnRegisterExtensionCall`
!define un.UnRegisterExtension `!insertmacro UnRegisterExtensionCall`

!macro UnRegisterExtension
!macroend

!macro un.UnRegisterExtension
!macroend

!macro UnRegisterExtension_
  !verbose push
  !verbose ${_FileAssociation_VERBOSE}

  Exch $R1 ;desc
  Exch
  Exch $R0 ;ext
  Exch
  Push $0
  Push $1

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

  Pop $1
  Pop $0
  Pop $R1
  Pop $R0

  !verbose pop
!macroend

!endif # !FileAssociation_INCLUDED
