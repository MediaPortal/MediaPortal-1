#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

                       NSIS logging system
_____________________________________________________________________________

  These macros for writing a log file during the installation process,
  which can be used for debugging the installation process.
*/


!ifndef LoggingMacros_INCLUDED
!define LoggingMacros_INCLUDED

#---------------------------------------------------------------------------
#           enable it by defining         USE_INSTALL_LOG      in parent script
#---------------------------------------------------------------------------
!define prefixERROR "[ERROR     !!!]   "
!define prefixDEBUG "[    DEBUG    ]   "
!define prefixINFO  "[         INFO]   "

!ifndef NO_INSTALL_LOG

!include FileFunc.nsh

Var LogFile
Var TempInstallLog


!define LOG_OPEN `!insertmacro LOG_OPEN ""`
!define un.LOG_OPEN `!insertmacro LOG_OPEN "un."`
!macro LOG_OPEN UNINSTALL_PREFIX
  GetTempFileName $TempInstallLog
  FileOpen $LogFile "$TempInstallLog" w

  ${${UNINSTALL_PREFIX}GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  ${LOG_TEXT} "INFO" "$(^Name) ${UNINSTALL_PREFIX}installation"
  ${LOG_TEXT} "INFO" "Logging started: $0.$1.$2 $4:$5:$6"
  ${LOG_TEXT} "INFO" "${UNINSTALL_PREFIX}installer version: ${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
  ${LOG_TEXT} "INFO" "============================================================================================"
!macroend


!define LOG_CLOSE `!insertmacro LOG_CLOSE ""`
!define un.LOG_CLOSE `!insertmacro LOG_CLOSE "un."`
!macro LOG_CLOSE UNINSTALL_PREFIX
  SetShellVarContext all

  ${${UNINSTALL_PREFIX}GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  ${LOG_TEXT} "INFO" "============================================================================================"
  ${LOG_TEXT} "INFO" "Logging stopped: $0.$1.$2 $4:$5:$6"
  ${LOG_TEXT} "INFO" "${UNINSTALL_PREFIX}installer version: ${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
  ${LOG_TEXT} "INFO" "$(^Name) ${UNINSTALL_PREFIX}installation"

  FileClose $LogFile

!ifdef INSTALL_LOG_FILE
  CopyFiles "$TempInstallLog" "${INSTALL_LOG_FILE}"
!else
  !ifndef COMMON_APPDATA
    !error "$\r$\n$\r$\nCOMMON_APPDATA is not defined!$\r$\n$\r$\n"
  !endif

  ${${UNINSTALL_PREFIX}GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  CopyFiles "$TempInstallLog" "${COMMON_APPDATA}\log\${UNINSTALL_PREFIX}install_${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}_$2-$1-$0_$4-$5-$6.log"

  Delete "$TempInstallLog"

!endif
!macroend


!define LOG_TEXT `!insertmacro LOG_TEXT`
!macro LOG_TEXT LEVEL TEXT

!if     "${LEVEL}" != "DEBUG"
  !if   "${LEVEL}" != "ERROR"
    !if "${LEVEL}" != "INFO"
      !error "$\r$\n$\r$\nYou call macro LOG_TEXT with wrong LogLevel. Only 'DEBUG', 'ERROR' and 'INFO' are valid!$\r$\n$\r$\n"
    !else
      DetailPrint "${prefix${LEVEL}}${TEXT}"
    !endif
  !else
    DetailPrint "${prefix${LEVEL}}${TEXT}"
  !endif
!endif

  FileWrite $LogFile "${prefix${LEVEL}}${TEXT}$\r$\n"

!macroend

!else #NO_INSTALL_LOG

!define LOG_OPEN `!insertmacro LOG_OPEN`
!macro LOG_OPEN
!macroend

!define LOG_CLOSE `!insertmacro LOG_CLOSE`
!macro LOG_CLOSE
!macroend

!define LOG_TEXT `!insertmacro LOG_TEXT`
!macro LOG_TEXT LEVEL TEXT
      DetailPrint "${prefix${LEVEL}}${TEXT}"
!macroend

!endif #NO_INSTALL_LOG

!endif # !LoggingMacros_INCLUDED
