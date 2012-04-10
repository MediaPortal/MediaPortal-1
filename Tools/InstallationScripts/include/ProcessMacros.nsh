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
!include "${git_InstallScripts}\include\LoggingMacros.nsh"

!AddPluginDir "${git_InstallScripts}\nsSCM-plugin\Plugin"
!AddPluginDir "${git_InstallScripts}\KillProc-plugin\Plugin"

#***************************
#***************************

!define KillProcess `!insertmacro KillProcess`
!macro KillProcess Process
  !echo "KillProcess: ${Process}"
  !verbose push
  !verbose 3


  ${LOG_TEXT} "INFO" "KillProcess: ${Process}"

  KillProcDLL::KillProc "${Process}"

  ${If} $R0 == "0"
    ${LOG_TEXT} "INFO" "KillProcess: ${Process} was killed successfully."
  ${ElseIf} $R0 == "603"
    ${LOG_TEXT} "INFO" "KillProcess: ${Process} is not running."
  ${Else}
    ${LOG_TEXT} "INFO" "KillProcess: Unable to kill ${Process} (error $R0)."
    Abort "Unable to kill ${Process} (error $R0). Installation aborted."
  ${EndIF}

  !verbose pop
!macroend

!define StopService `!insertmacro StopService`
!macro StopService Service
  !echo "StopService: ${Service}"
  !verbose push
  !verbose 3

  ${LOG_TEXT} "INFO" "StopService: ${Service}"

  nsSCM::QueryStatus /NOUNLOAD "${Service}"
  Pop $0
  Pop $1

  ${IfNot} $1 = 1

    ${LOG_TEXT} "INFO" "StopService: Trying to stop ${Service}..."

    nsSCM::Stop /NOUNLOAD "${Service}"
    Pop $0

    ${If} $0 == "error"
      ${LOG_TEXT} "INFO" "StopService: Unable to stop ${Service} (error $1)."
      Abort "Unable to stop ${Service} (error $1). Installation aborted."
    ${Else}

      StrCpy $R0 0
      ${Do}
        ${If} $R0 > 0
          ${LOG_TEXT} "INFO" "StopService: sleeping 20ms and rechecking service status..."
          Sleep 20
        ${EndIF}

        IntOp $R0 $R0 + 1
        nsSCM::QueryStatus /NOUNLOAD "${Service}"
        Pop $0
        Pop $1
      ${LoopUntil} $1 = 1

      ${LOG_TEXT} "INFO" "StopService: ${Service} was stopped successfully."
    ${EndIF}

  ${Else}
    ${LOG_TEXT} "INFO" "StopService: ${Service} is already stopped."
  ${EndIf}

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
