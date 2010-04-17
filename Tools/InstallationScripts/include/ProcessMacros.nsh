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

!ifndef ProcessMacros_INCLUDED
!define ProcessMacros_INCLUDED

!include LogicLib.nsh
!include "${svn_InstallScripts}\include\LoggingMacros.nsh"

#***************************
#***************************

!define KillProcess `!insertmacro KillProcess`
!macro KillProcess Process
  !echo "KillProcess: ${Process}"
  !verbose push
  !verbose 3


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


  !verbose pop
!macroend

!define StopService `!insertmacro StopService`
!macro StopService Service
  !echo "StopService: ${Service}"
  !verbose push
  !verbose 3

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


  !verbose pop
!macroend

!define RenameDirectory `!insertmacro RenameDirectory`
!macro RenameDirectory DirPath NewDirPath
  !echo "RenameDirectory: old ${DirPath} | new: ${NewDirPath}"
  !verbose push
  !verbose 3


  ${LOG_TEXT} "INFO" "RenameDirectory: Old path: ${DirPath}"
  ${LOG_TEXT} "INFO" "RenameDirectory: New path: ${NewDirPath}"

${If} ${FileExists} "${DirPath}\*.*"

  ${LOG_TEXT} "INFO" "RenameDirectory: Directory exists. Trying to remove if empty."
  RMDir "${DirPath}"

  ${If} ${FileExists} "${DirPath}\*.*"
    ${LOG_TEXT} "INFO" "RenameDirectory: Directory still exists, means it is not empty. Trying to rename."

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
    ${LOG_TEXT} "INFO" "RenameDirectory: Directory does not exist anymore. No need to rename: ${DirPath}"
  ${EndIf}
  
${Else}

    ${LOG_TEXT} "INFO" "RenameDirectory: Directory does not exist. No need to rename: ${DirPath}"

${EndIf}


  !verbose pop
!macroend



!endif # !ProcessMacros_INCLUDED
