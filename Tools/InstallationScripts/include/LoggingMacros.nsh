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
  
  Disable Logging to file by defining NO_INSTALL_LOG in host script.
    !define NO_INSTALL_LOG
*/


!ifndef LoggingMacros_INCLUDED
!define LoggingMacros_INCLUDED


!define prefixERROR "[ERROR     !!!]   "
!define prefixDEBUG "[    DEBUG    ]   "
!define prefixINFO  "[         INFO]   "

!include FileFunc.nsh

Var LogFile
Var TempInstallLog


!define LOG_OPEN `!insertmacro LOG_OPEN`
!define un.LOG_OPEN `!insertmacro LOG_OPEN`
!macro LOG_OPEN
  !echo "LOG_OPEN"
  !verbose push
  !verbose 3

  !ifndef __UNINSTALL__
    !define UNINSTALL_PREFIX ""
  !else
    !define UNINSTALL_PREFIX "un"
  !endif


  GetTempFileName $TempInstallLog
  FileOpen $LogFile "$TempInstallLog" w

  ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  ${LOG_TEXT} "INFO" "${PRODUCT_NAME} ${UNINSTALL_PREFIX}installation"
  ${LOG_TEXT} "INFO" "Logging started: $0.$1.$2 $4:$5:$6"
  ${LOG_TEXT} "INFO" "${UNINSTALL_PREFIX}installer version: ${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
  ${LOG_TEXT} "INFO" "============================================================================================"


  !undef UNINSTALL_PREFIX

  !verbose pop
!macroend


!define LOG_CLOSE `!insertmacro LOG_CLOSE`
!define un.LOG_CLOSE `!insertmacro LOG_CLOSE`
!macro LOG_CLOSE
  !echo "LOG_CLOSE"
  !verbose push
  !verbose 3

  !ifndef __UNINSTALL__
    !define UNINSTALL_PREFIX ""
  !else
    !define UNINSTALL_PREFIX "un"
  !endif


  SetShellVarContext all

  ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  ${LOG_TEXT} "INFO" "============================================================================================"
  ${LOG_TEXT} "INFO" "Logging stopped: $0.$1.$2 $4:$5:$6"
  ${LOG_TEXT} "INFO" "${UNINSTALL_PREFIX}installer version: ${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
  ${LOG_TEXT} "INFO" "${PRODUCT_NAME} ${UNINSTALL_PREFIX}installation"

  FileClose $LogFile

!ifdef INSTALL_LOG_FILE
  CopyFiles "$TempInstallLog" "${INSTALL_LOG_FILE}"
!else

  !ifdef INSTALL_LOG_DIR

    ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
    CopyFiles "$TempInstallLog" "${INSTALL_LOG_DIR}\${UNINSTALL_PREFIX}install_${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}_$2-$1-$0_$4-$5-$6.log"

  !else

    !ifndef COMMON_APPDATA
      !error "$\r$\n$\r$\nCOMMON_APPDATA is not defined!$\r$\n$\r$\n"
    !endif

    ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
    CopyFiles "$TempInstallLog" "${COMMON_APPDATA}\log\${UNINSTALL_PREFIX}install_${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}_$2-$1-$0_$4-$5-$6.log"

  !endif

!endif
  Delete "$TempInstallLog"

  !undef UNINSTALL_PREFIX

  !verbose pop
!macroend


!define LOG_TEXT `!insertmacro LOG_TEXT`
!macro LOG_TEXT LEVEL TEXT
  !verbose push
  !verbose 3

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

  !verbose pop
!macroend

!endif # !LoggingMacros_INCLUDED
