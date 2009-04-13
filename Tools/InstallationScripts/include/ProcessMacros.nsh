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


!ifndef ProcessMacros_INCLUDED
!define ProcessMacros_INCLUDED


!define KillProcess `!insertmacro KillProcess`
!macro KillProcess Process
  ${LOG_TEXT} "INFO" "KillProcess: ${Process}"

  StrCpy $R1 1  ; set counter to 1
  ${Do}

    nsExec::Exec '"taskkill" /F /IM "${Process}"'

    Pop $0

    ${Select} $0
      ${Case} "0"
        ${LOG_TEXT} "INFO" "KillProcess: ${Process} was killed successfully."
        ${ExitDo}
      ${Case} "128"
        ${LOG_TEXT} "INFO" "KillProcess: ${Process} is not running."
        ${ExitDo}
      ${CaseElse}

        ${LOG_TEXT} "ERROR" "KillProcess: Unknown result: $0"
        IntOp $R1 $R1 + 1  ; increase retry-counter +1
        ${If} $R1 > 5  ; try max. 5 times
          ${ExitDo}
        ${Else}
          ${LOG_TEXT} "INFO" "KillProcess: Trying again. $R1/5"
        ${EndIf}

      ${EndSelect}
  ${Loop}
!macroend

!define StopService `!insertmacro StopService`
!macro StopService Service
  ${LOG_TEXT} "INFO" "StopService: ${Service}"

  StrCpy $R1 1  ; set counter to 1
  ${Do}

    nsExec::Exec 'net stop "${Service}"'

    Pop $0

    ${Select} $0
      ${Case} "0"
        ${LOG_TEXT} "INFO" "StopService: ${Service} was stopped successfully."
        ${ExitDo}
      ${Case} "2"
        ${LOG_TEXT} "INFO" "StopService: ${Service} is not started."
        ${ExitDo}
      ${CaseElse}

        ${LOG_TEXT} "ERROR" "StopService: Unknown result: $0"
        IntOp $R1 $R1 + 1  ; increase retry-counter +1
        ${If} $R1 > 5  ; try max. 5 times
          ${ExitDo}
        ${Else}
          ${LOG_TEXT} "INFO" "StopService: Trying again. $R1/5"
        ${EndIf}

      ${EndSelect}
  ${Loop}
!macroend

!define RenameDirectory `!insertmacro RenameDirectory`
!macro RenameDirectory DirPath NewDirPath
  ${LOG_TEXT} "INFO" "RenameDirectory: Old path: ${DirPath}"
  ${LOG_TEXT} "INFO" "RenameDirectory: New path: ${NewDirPath}"

  ${If} ${FileExists} "${DirPath}\*.*"
    ${LOG_TEXT} "INFO" "RenameDirectory: Directory exists. Trying to rename."

    StrCpy $R1 1  ; set counter to 1
    ${Do}

      ClearErrors
      Rename "${DirPath}" "${NewDirPath}"

      IntOp $R1 $R1 + 1  ; increase retry-counter +1
      ${IfNot} ${Errors}
        ${LOG_TEXT} "INFO" "RenameDirectory: Renamed directory successfully."
        ${ExitDo}
      ${ElseIf} $R1 > 5  ; try max. 5 times
        ${LOG_TEXT} "ERROR" "RenameDirectory: Renaming directory failed for some reason."
        ${ExitDo}
      ${Else}
        ${LOG_TEXT} "INFO" "RenameDirectory: Trying again. $R1/5"
      ${EndIf}

    ${Loop}
  
  ${Else}
    ${LOG_TEXT} "INFO" "RenameDirectory: Directory does not exist. No need to rename: ${DirPath}"
  ${EndIf}
!macroend



!endif # !ProcessMacros_INCLUDED
