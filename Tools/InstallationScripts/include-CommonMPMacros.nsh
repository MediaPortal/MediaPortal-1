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


!ifndef ___COMMON_MP_MACROS__NSH___
!define ___COMMON_MP_MACROS__NSH___


!include LogicLib.nsh
!include x64.nsh
!include "${svn_InstallScripts}\include-WinVerEx.nsh"


!ifndef COMPANY
  !define COMPANY "Team MediaPortal"
!endif
!ifndef URL
  !define URL "www.team-mediaportal.com"
!endif
!ifndef WEB_REQUIREMENTS
  !define WEB_REQUIREMENTS "http://wiki.team-mediaportal.com/GeneralRequirements"
!endif


!ifndef MP_REG_UNINSTALL
  !define MP_REG_UNINSTALL  "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!endif
!ifndef TV3_REG_UNINSTALL
  !define TV3_REG_UNINSTALL "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
!endif




# references to additional plugins, if not used, these won't be included
!AddPluginDir "${svn_InstallScripts}\GetVersion-plugin\Plugins"
!AddPluginDir "${svn_InstallScripts}\XML-plugin\Plugin"
!include "${svn_InstallScripts}\XML-plugin\Include\XML.nsh"


#---------------------------------------------------------------------------
#   NSIS logging system
#
#           enable it by defining         USE_INSTALL_LOG      in parent script
#---------------------------------------------------------------------------
!define prefixERROR "[ERROR     !!!]   "
!define prefixDEBUG "[    DEBUG    ]   "
!define prefixINFO  "[         INFO]   "

!ifdef USE_INSTALL_LOG

!include FileFunc.nsh
!insertmacro GetTime
!insertmacro un.GetTime

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

!else #!USE_INSTALL_LOG

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

!endif

#---------------------------------------------------------------------------
#   KILL Process       macro for common usage
#---------------------------------------------------------------------------
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



#Var AR_SecFlags
#Var AR_RegFlags

# registry
# ${MEMENTO_REGISTRY_ROOT}
# ${MEMENTO_REGISTRY_KEY}
# ${MEMENTO_REGISTRY_KEY}
#ReadRegDWORD $AR_RegFlags ${MEMENTO_REGISTRY_ROOT} `${MEMENTO_REGISTRY_KEY}` `MementoSection_${__MementoSectionLastSectionId}`

 /*   not needed anymore ----- done by MementoSectionRestore
!macro InitSection SecName
    ;This macro reads component installed flag from the registry and
    ;changes checked state of the section on the components page.
    ;Input: section index constant name specified in Section command.

    ClearErrors
    ;Reading component status from registry
    ReadRegDWORD $AR_RegFlags "${MEMENTO_REGISTRY_ROOT}" "${MEMENTO_REGISTRY_KEY}" "${SecName}"
    IfErrors "default_${SecName}"
    
    ;Status will stay default if registry value not found
    ;(component was never installed)
    IntOp $AR_RegFlags $AR_RegFlags & 0x0001  ;Turn off all other bits
    SectionGetFlags ${${SecName}} $AR_SecFlags  ;Reading default section flags
    IntOp $AR_SecFlags $AR_SecFlags & 0xFFFE  ;Turn lowest (enabled) bit off
    IntOp $AR_SecFlags $AR_RegFlags | $AR_SecFlags      ;Change lowest bit

    ;Writing modified flags
    SectionSetFlags ${${SecName}} $AR_SecFlags

  "default_${SecName}:"
!macroend
*/

!macro FinishSection SecName
  ;This macro reads section flag set by user and removes the section
  ;if it is not selected.
  ;Then it writes component installed flag to registry
  ;Input: section index constant name specified in Section command.

  ${IfNot} ${SectionIsSelected} "${${SecName}}"
    ClearErrors
    ReadRegDWORD $R0 ${MEMENTO_REGISTRY_ROOT} '${MEMENTO_REGISTRY_KEY}' 'MementoSection_${SecName}'

    ${If} $R0 = 1
      !insertmacro "Remove_${${SecName}}"
    ${EndIf}
  ${EndIf}
!macroend

!macro RemoveSection SecName
  ;This macro is used to call section's Remove_... macro
  ;from the uninstaller.
  ;Input: section index constant name specified in Section command.

  !insertmacro "Remove_${${SecName}}"
!macroend

!macro DisableComponent SectionName AddText
  !insertmacro UnselectSection "${SectionName}"
  ; Make the unselected section read only
  !insertmacro SetSectionFlag "${SectionName}" 16
  SectionGetText ${SectionName} $R0
  SectionSetText ${SectionName} "$R0${AddText}"
!macroend



#**********************************************************************************************************#
#
# Useful macros for MediaPortal and addtional Software which can be used like other LogicLib expressions.
#
#**********************************************************************************************************#


#**********************************************************************************************************#
# LOGICLIB EXPRESSIONS

;======================================   OLD MP INSTALLATION TESTs

# old installations < 0.2.3.0 RC 3
!macro _MP022IsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{87819CFA-1786-484D-B0DE-10B5FBF2625D}" "UninstallString"
  IfErrors `${_f}` `${_t}`
!macroend
!define MP022IsInstalled `"" MP022IsInstalled ""`

!macro _MP023RC3IsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal 0.2.3.0 RC3" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MP023RC3IsInstalled `"" MP023RC3IsInstalled ""`

!macro _MP023IsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal 0.2.3.0" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MP023IsInstalled `"" MP023IsInstalled ""`

;======================================   OLD TVServer/TVClient INSTALLATION TESTs

!macro _MSI_TVServerIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{4B738773-EE07-413D-AFB7-BB0AB04A5488}" "UninstallString"
  IfErrors `${_f}` `${_t}`
!macroend
!define MSI_TVServerIsInstalled `"" MSI_TVServerIsInstalled ""`

!macro _MSI_TVClientIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{F7444E89-5BC0-497E-9650-E50539860DE0}" "UninstallString"
  IfErrors 0 `${_t}`
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}" "UninstallString"
  IfErrors `${_f}` `${_t}`
!macroend
!define MSI_TVClientIsInstalled `"" MSI_TVClientIsInstalled ""`

;======================================

!macro _MPIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${MP_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MPIsInstalled `"" MPIsInstalled ""`

!macro _TVServerIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP 0 `${_f}`

  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "MementoSection_SecServer"
  StrCmp $_LOGICLIB_TEMP 1 `${_t}` `${_f}`
!macroend
!define TVServerIsInstalled `"" TVServerIsInstalled ""`

!macro _TVClientIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP 0 `${_f}`

  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "MementoSection_SecClient"
  StrCmp $_LOGICLIB_TEMP 1 `${_t}` `${_f}`
!macroend
!define TVClientIsInstalled `"" TVClientIsInstalled ""`

;======================================   3rd PARTY APPLICATION TESTs

!macro _VCRedistIsInstalled _a _b _t _f

  ClearErrors
  ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" CurrentVersion
  IfErrors `${_f}`

  StrCpy $R1 $R0 3
 
  StrCmp $R1 '5.1' lbl_winnt_XP
  StrCmp $R1 '5.2' lbl_winnt_2003
  StrCmp $R1 '6.0' lbl_winnt_vista `${_f}`


  lbl_winnt_vista:
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc80.crt_1fc8b3b9a1e18e3b_8.0.50727.762_none_10b2f55f9bffb8f8.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc80.mfc_1fc8b3b9a1e18e3b_8.0.50727.762_none_0c178a139ee2a7ed.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc80.atl_1fc8b3b9a1e18e3b_8.0.50727.762_none_11ecb0ab9b2caf3c.manifest" 0 `${_f}`
  Goto `${_t}`

  lbl_winnt_2003:
  lbl_winnt_XP:
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC80.CRT_1fc8b3b9a1e18e3b_8.0.50727.762_x-ww_6b128700.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC80.MFC_1fc8b3b9a1e18e3b_8.0.50727.762_x-ww_3bf8fa05.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC80.ATL_1fc8b3b9a1e18e3b_8.0.50727.762_x-ww_cbb27474.manifest" 0 `${_f}`
  Goto `${_t}`
!macroend
!define VCRedistIsInstalled `"" VCRedistIsInstalled ""`

!macro _dotNetIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP

  ReadRegStr $4 HKLM "Software\Microsoft\.NETFramework" "InstallRoot"
  # remove trailing back slash
  Push $4
  Exch $EXEDIR
  Exch $EXEDIR
  Pop $4
  # if the root directory doesn't exist .NET is not installed
  IfFileExists $4 0 `${_f}`

  StrCpy $0 0

  EnumStart:

    EnumRegKey $2 HKLM "Software\Microsoft\.NETFramework\Policy"  $0
    IntOp $0 $0 + 1
    StrCmp $2 "" `${_f}`

    StrCpy $1 0

    EnumPolicy:

      EnumRegValue $3 HKLM "Software\Microsoft\.NETFramework\Policy\$2" $1
      IntOp $1 $1 + 1
       StrCmp $3 "" EnumStart
        IfFileExists "$4\$2.$3" `${_t}` EnumPolicy
!macroend
!define dotNetIsInstalled `"" dotNetIsInstalled ""`

#**********************************************************************************************************#
# Get MP infos
!macro MP_GET_INSTALL_DIR _var
  SetRegView 32
  ${LOG_TEXT} "DEBUG" "MACRO:MP_GET_INSTALL_DIR"

  ${If} ${MP023IsInstalled}
    ReadRegStr ${_var} HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
    ${LOG_TEXT} "INFO" "MediaPortal v0.2.3 installation dir found: ${_var}"
  ${ElseIf} ${MPIsInstalled}
    ReadRegStr ${_var} HKLM "${MP_REG_UNINSTALL}" "InstallPath"
    ${LOG_TEXT} "INFO" "MediaPortal installation dir found: ${_var}"
  ${Else}
    StrCpy ${_var} ""
    ${LOG_TEXT} "INFO" "No MediaPortal installation found: _var will be empty"
  ${EndIf}

!macroend

!macro TVSERVER_GET_INSTALL_DIR _var
  SetRegView 32
  ${LOG_TEXT} "DEBUG" "MACRO:TVSERVER_GET_INSTALL_DIR"

  ${If} ${TVServerIsInstalled}
  ${OrIf} ${TVClientIsInstalled}
    ReadRegStr ${_var} HKLM "${TV3_REG_UNINSTALL}" "InstallPath"
  ${Else}
    StrCpy ${_var} ""
  ${EndIf}

!macroend

!macro MP_GET_VERSION _var
  SetRegView 32
  ${LOG_TEXT} "DEBUG" "MACRO:MP_GET_VERSION"

  ${If} ${MPIsInstalled}
    ReadRegDWORD $R0 HKLM "${MP_REG_UNINSTALL}" "VersionMajor"
    ReadRegDWORD $R1 HKLM "${MP_REG_UNINSTALL}" "VersionMinor"
    ReadRegDWORD $R2 HKLM "${MP_REG_UNINSTALL}" "VersionRevision"
    ReadRegDWORD $R3 HKLM "${MP_REG_UNINSTALL}" "VersionBuild"
    StrCpy ${_var} $R0.$R1.$R2.$R3
  ${Else}
    StrCpy ${_var} ""
  ${EndIf}

!macroend

!macro TVSERVER_GET_VERSION _var
  SetRegView 32
  ${LOG_TEXT} "DEBUG" "MACRO:TVSERVER_GET_VERSION"

  ${If} ${TVServerIsInstalled}
  ${OrIf} ${TVClientIsInstalled}
    ReadRegDWORD $R0 HKLM "${TV3_REG_UNINSTALL}" "VersionMajor"
    ReadRegDWORD $R1 HKLM "${TV3_REG_UNINSTALL}" "VersionMinor"
    ReadRegDWORD $R2 HKLM "${TV3_REG_UNINSTALL}" "VersionRevision"
    ReadRegDWORD $R3 HKLM "${TV3_REG_UNINSTALL}" "VersionBuild"
    StrCpy ${_var} $R0.$R1.$R2.$R3
  ${Else}
    StrCpy ${_var} ""
  ${EndIf}

!macroend

!include FileFunc.nsh
!insertmacro GetTime
!macro GET_BACKUP_POSTFIX _var

  ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  ; $0="01"      day
  ; $1="04"      month
  ; $2="2005"    year
  ; $3="Friday"  day of week name
  ; $4="16"      hour
  ; $5="05"      minute
  ; $6="50"      seconds
  
  StrCpy ${_var} "BACKUP_$1-$0_$4-$5"

!macroend



#**********************************************************************************************************#
#
# common language strings
#
#**********************************************************************************************************#
LangString TEXT_MP_NOT_INSTALLED                  ${LANG_ENGLISH} "MediaPortal not installed"
LangString TEXT_TVSERVER_NOT_INSTALLED            ${LANG_ENGLISH} "TVServer not installed"


LangString TEXT_MSGBOX_INSTALLATION_CANCELD       ${LANG_ENGLISH} "Installation will be canceled."
LangString TEXT_MSGBOX_MORE_INFO                  ${LANG_ENGLISH} "Do you want to get more information about it?"

LangString TEXT_MSGBOX_ERROR_WIN                  ${LANG_ENGLISH} "Your operating system is not supported by $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED  ${LANG_ENGLISH} "Your operating system is not recommended by $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_ADMIN                ${LANG_ENGLISH} "You need administration rights to install $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)"
LangString TEXT_MSGBOX_ERROR_VCREDIST             ${LANG_ENGLISH} "Microsoft Visual C++ 2005 SP1 Redistributable Package (x86) is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_DOTNET20             ${LANG_ENGLISH} "Microsoft .Net Framework 2.0 SP2 is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_DOTNET20_SP          ${LANG_ENGLISH} "Microsoft .Net Framework 2.0 is installed.$\r$\nBut Service Pack 2 is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_DOTNET35             ${LANG_ENGLISH} "Microsoft .Net Framework 3.5 SP1 is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_DOTNET35_SP          ${LANG_ENGLISH} "Microsoft .Net Framework 3.5 is installed.$\r$\nBut Service Pack 1 is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"

LangString TEXT_MSGBOX_ERROR_IS_INSTALLED         ${LANG_ENGLISH} "$(^Name) is already installed. You need to uninstall it, before you continue with the installation.$\r$\nUninstall will be lunched when pressing OK."
LangString TEXT_MSGBOX_ERROR_ON_UNINSTALL         ${LANG_ENGLISH} "An error occured while trying to uninstall old version!$\r$\nDo you still want to continue the installation?"
LangString TEXT_MSGBOX_ERROR_REBOOT_REQUIRED      ${LANG_ENGLISH} "A reboot is required after a previous action. Reboot you system and try it again."
LangString TEXT_MSGBOX_ERROR_UPDATE_BUT_NOT_INSTALLED ${LANG_ENGLISH} "$(^Name) is not installed. It is not possible to install this update.$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)"


LangString UPDATE_ERROR_WRONGEXE                  ${LANG_ENGLISH} "updating $(^Name) is only allowed by starting MediaPortalUpdater!"
LangString UPDATE_ERROR_UNKNOWN                   ${LANG_ENGLISH} "strange / unknown error, please use full installer"
LangString UPDATE_ERROR_NOTHING_INSTALLED         ${LANG_ENGLISH} "Nothing to do, nothing installed, please use the full installer"
LangString UPDATE_ERROR_VERSION_MP                ${LANG_ENGLISH} "wrong version of MediaPortal is installed or svn, please use the full installer"
LangString UPDATE_ERROR_VERSION_TVSERVER          ${LANG_ENGLISH} "wrong version or TVServer or Client plugin is installed or svn, please use the full installer"

  /*
; Section flag test
!macro _MPIsInstalled _a _b _t _f
  !insertmacro _LOGICLIB_TEMP

  ReadRegStr $MPBaseDir HKLM "${MP_REG_UNINSTALL}" "UninstallString"
  ${If} $MPBaseDir == ""
    # this fallback should only be enabled until MediaPortal 1.0 is out
    ReadRegStr $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"

#!define MP_REG_UNINSTALL      "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
#!define TV3_REG_UNINSTALL     "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"

    ${If} $MPBaseDir == ""
        !insertmacro UnselectSection "${SecClient}"
        ; Make the unselected section read only
        !insertmacro SetSectionFlag "${SecClient}" 16
        SectionGetText ${SecClient} $R0
        SectionSetText ${SecClient} "$R0 ($(TEXT_MP_NOT_INSTALLED))"
    ${EndIf}
  ${EndIf}
    SectionGetFlags `${_b}` $_LOGICLIB_TEMP
    IntOp $_LOGICLIB_TEMP $_LOGICLIB_TEMP & `${_a}`

    !insertmacro _= $_LOGICLIB_TEMP `${_a}` `${_t}` `${_f}`
  !macroend
  
  #!define MPIsInstalled `${SF_SELECTED} SectionFlagIsSet`
!define MPIsInstalled "!insertmacro _MPIsInstalled"
  */

#---------------------------------------------------------------------------
#   Read      Special MediaPortal directories from  xml
#
#           enable it by defining         USE_READ_MP_DIRS      in parent script
#---------------------------------------------------------------------------
!ifdef USE_READ_MP_DIRS

Var MyDocs
Var UserAppData
Var CommonAppData

Var MPdir.Base

Var MPdir.Config
Var MPdir.Plugins
Var MPdir.Log
Var MPdir.CustomInputDevice
Var MPdir.CustomInputDefault
Var MPdir.Skin
Var MPdir.Language
Var MPdir.Database
Var MPdir.Thumbs
Var MPdir.Weather
Var MPdir.Cache
Var MPdir.BurnerSupport

#***************************
#***************************

!macro GET_PATH_TEXT

  Pop $R0

  ${xml::GotoPath} "/Config" $0
  ${If} $0 != 0
    ${LOG_TEXT} "ERROR" "xml::GotoPath /Config"
    Goto error
  ${EndIf}

  loop:

  ${xml::FindNextElement} "Dir" $0 $1
  ${If} $1 != 0
    ${LOG_TEXT} "ERROR" "xml::FindNextElement >/Dir< >$0<"
    Goto error
  ${EndIf}

  ${xml::ElementPath} $0
  ${xml::GetAttribute} "id" $0 $1
  ${If} $1 != 0
    ${LOG_TEXT} "ERROR" "xml::GetAttribute >id< >$0<"
    Goto error
  ${EndIf}
  ${IfThen} $0 == $R0  ${|} Goto foundDir ${|}

  Goto loop


  foundDir:
  ${xml::ElementPath} $0
  ${xml::GotoPath} "$0/Path" $1
  ${If} $1 != 0
    ${LOG_TEXT} "ERROR" "xml::GotoPath >$0/Path<"
    Goto error
  ${EndIf}

  ${xml::GetText} $0 $1
  ${If} $1 != 0
    ; maybe the path is only empty, which means MPdir.Base
    #MessageBox MB_OK "error: xml::GetText"
    #Goto error
    StrCpy $0 ""
  ${EndIf}

  Push $0
  Goto end

  error:
  Push "-1"

  end:

!macroend
Function GET_PATH_TEXT
  !insertmacro GET_PATH_TEXT
FunctionEnd
Function un.GET_PATH_TEXT
  !insertmacro GET_PATH_TEXT
FunctionEnd

#***************************
#***************************

!include FileFunc.nsh
!insertmacro GetRoot
!insertmacro un.GetRoot
!include WordFunc.nsh
!insertmacro WordReplace
!insertmacro un.WordReplace
!macro ReadMPdir UNINSTALL_PREFIX DIR
  ${LOG_TEXT} "DEBUG" "macro: ReadMPdir | DIR: ${DIR}"

  Push "${DIR}"
  Call ${UNINSTALL_PREFIX}GET_PATH_TEXT
  Pop $0
  ${IfThen} $0 == -1 ${|} Goto error ${|}

  ${LOG_TEXT} "DEBUG" "macro: ReadMPdir | text found in xml: '$0'"
  ${${UNINSTALL_PREFIX}WordReplace} "$0" "%APPDATA%" "$UserAppData" "+" $0
  ${${UNINSTALL_PREFIX}WordReplace} "$0" "%PROGRAMDATA%" "$CommonAppData" "+" $0

  ${${UNINSTALL_PREFIX}GetRoot} "$0" $1

  ${IfThen} $1 == "" ${|} StrCpy $0 "$MPdir.Base\$0" ${|}

  ; TRIM    \    AT THE END
  StrLen $1 "$0"
    #${DEBUG_MSG} "1 $1$\r$\n2 $2$\r$\n3 $3"
  IntOp $2 $1 - 1
    #${DEBUG_MSG} "1 $1$\r$\n2 $2$\r$\n3 $3"
  StrCpy $3 $0 1 $2
    #${DEBUG_MSG} "1 $1$\r$\n2 $2$\r$\n3 $3"

  ${If} $3 == "\"
    StrCpy $MPdir.${DIR} $0 $2
  ${Else}
    StrCpy $MPdir.${DIR} $0
  ${EndIf}

!macroend

#***************************
#***************************

!macro ReadConfig UNINSTALL_PREFIX PATH_TO_XML
  ${LOG_TEXT} "DEBUG" "macro: ReadConfig | UNINSTALL_PREFIX: ${UNINSTALL_PREFIX} | PATH_TO_XML: ${PATH_TO_XML}"

  IfFileExists "${PATH_TO_XML}\MediaPortalDirs.xml" 0 error

  
  #${xml::LoadFile} "$EXEDIR\MediaPortalDirsXP.xml" $0
  ${xml::LoadFile} "$0\MediaPortalDirs.xml" $0
  ${IfThen} $0 != 0 ${|} Goto error ${|}

  #</Dir>  Log CustomInputDevice CustomInputDefault Skin Language Database Thumbs Weather Cache BurnerSupport

  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Config
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Plugins
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Log
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" CustomInputDevice
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" CustomInputDefault
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Skin
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Language
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Database
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Thumbs
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Weather
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Cache
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" BurnerSupport


  Push "0"
  Goto end

  error:
  Push "-1"

  end:

!macroend
Function ReadConfig
  Pop $0

  !insertmacro ReadConfig "" "$0"
FunctionEnd
Function un.ReadConfig
  Pop $0

  !insertmacro ReadConfig "un." "$0"
FunctionEnd

#***************************
#***************************

!macro LoadDefaultDirs

  StrCpy $MPdir.Config              "$CommonAppData\Team MediaPortal\MediaPortal"

  StrCpy $MPdir.Plugins             "$MPdir.Base\plugins"
  StrCpy $MPdir.Log                 "$MPdir.Config\log"
  StrCpy $MPdir.CustomInputDevice   "$MPdir.Config\InputDeviceMappings"
  StrCpy $MPdir.CustomInputDefault  "$MPdir.Base\InputDeviceMappings\defaults"
  StrCpy $MPdir.Skin                "$MPdir.Base\skin"
  StrCpy $MPdir.Language            "$MPdir.Base\language"
  StrCpy $MPdir.Database            "$MPdir.Config\database"
  StrCpy $MPdir.Thumbs              "$MPdir.Config\thumbs"
  StrCpy $MPdir.Weather             "$MPdir.Base\weather"
  StrCpy $MPdir.Cache               "$MPdir.Config\cache"
  StrCpy $MPdir.BurnerSupport       "$MPdir.Base\Burner"

!macroend

!define ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirs ""`
!define un.ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirs "un."`
!macro ReadMediaPortalDirs UNINSTALL_PREFIX INSTDIR
  ${LOG_TEXT} "DEBUG" "macro ReadMediaPortalDirs"

  StrCpy $MPdir.Base "${INSTDIR}"
  SetShellVarContext current
  StrCpy $MyDocs "$DOCUMENTS"
  StrCpy $UserAppData "$APPDATA"
  SetShellVarContext all
  StrCpy $CommonAppData "$APPDATA"


  !insertmacro LoadDefaultDirs

  Push "$MyDocs\Team MediaPortal"
  Call ${UNINSTALL_PREFIX}ReadConfig
  Pop $0
  ${If} $0 != 0   ; an error occured
    ${LOG_TEXT} "ERROR" "could not read '$MyDocs\Team MediaPortal\MediaPortalDirs.xml'"

    Push "$MPdir.Base"
    Call ${UNINSTALL_PREFIX}ReadConfig
    Pop $0
    ${If} $0 != 0   ; an error occured
      ${LOG_TEXT} "ERROR" "could not read '$MPdir.Base\MediaPortalDirs.xml'"

      ${LOG_TEXT} "INFO" "no MediaPortalDirs.xml read. using LoadDefaultDirs"
      !insertmacro LoadDefaultDirs

    ${Else}
      ${LOG_TEXT} "INFO" "read '$MPdir.Base\MediaPortalDirs.xml' successfully"
    ${EndIf}

  ${Else}
    ${LOG_TEXT} "INFO" "read '$MyDocs\Team MediaPortal\MediaPortalDirs.xml' successfully"
  ${EndIf}

  ${LOG_TEXT} "INFO" "Installer will use the following directories:"
  ${LOG_TEXT} "INFO" "          Base:  $MPdir.Base"
  ${LOG_TEXT} "INFO" "          Config:  $MPdir.Config"
  ${LOG_TEXT} "INFO" "          Plugins: $MPdir.Plugins"
  ${LOG_TEXT} "INFO" "          Log: $MPdir.Log"
  ${LOG_TEXT} "INFO" "          CustomInputDevice: $MPdir.CustomInputDevice"
  ${LOG_TEXT} "INFO" "          CustomInputDefault: $MPdir.CustomInputDefault"
  ${LOG_TEXT} "INFO" "          Skin: $MPdir.Skin"
  ${LOG_TEXT} "INFO" "          Language: $MPdir.Language"
  ${LOG_TEXT} "INFO" "          Database: $MPdir.Database"
  ${LOG_TEXT} "INFO" "          Thumbs: $MPdir.Thumbs"
  ${LOG_TEXT} "INFO" "          Weather: $MPdir.Weather"
  ${LOG_TEXT} "INFO" "          Cache: $MPdir.Cache"
  ${LOG_TEXT} "INFO" "          BurnerSupport: $MPdir.BurnerSupport"
!macroend

!endif # !USE_READ_MP_DIRS

#---------------------------------------------------------------------------
#   COMPLETE MEDIAPORTAL CLEANUP
#---------------------------------------------------------------------------
!macro CompleteMediaPortalCleanup

# make and uninstallation of the other app, which may be still installed
!if "${NAME}" == "MediaPortal"
  !insertmacro NSISuninstall "${TV3_REG_UNINSTALL}"
!else
  !if "${NAME}" == "MediaPortal TV Server / Client"
    !insertmacro NSISuninstall "${MP_REG_UNINSTALL}"
  !endif
!endif

SetShellVarContext all
# Delete new MediaPortal ( >= 0.2.3 RC3 ) and TVengine 3 directories
RMDir /r /REBOOTOK "$PROGRAMFILES\Team MediaPortal"
RMDir /r /REBOOTOK "$APPDATA\Team MediaPortal"
RMDir /r /REBOOTOK "$LOCALAPPDATA\VirtualStore\Program Files\Team MediaPortal"
RMDir /r /REBOOTOK "$LOCALAPPDATA\VirtualStore\ProgramData\Team MediaPortal"

# Delete old MediaPortal ( <= 0.2.3 RC2 ) directories 
RMDir /r /REBOOTOK "$PROGRAMFILES\MediaPortal"
RMDir /r /REBOOTOK "$APPDATA\MediaPortal"
RMDir /r /REBOOTOK "$LOCALAPPDATA\VirtualStore\Program Files\MediaPortal"
RMDir /r /REBOOTOK "$LOCALAPPDATA\VirtualStore\ProgramData\MediaPortal"

# Delete old TV3 engine directories
RMDir /r /REBOOTOK "$PROGRAMFILES\MediaPortal TV Engine"
RMDir /r /REBOOTOK "$APPDATA\MediaPortal TV Engine"
RMDir /r /REBOOTOK "$LOCALAPPDATA\VirtualStore\Program Files\MediaPortal TV Engine"
RMDir /r /REBOOTOK "$LOCALAPPDATA\VirtualStore\ProgramData\MediaPortal TV Engine"

# Delete menu shortcut icons
SetShellVarContext all
RMDir /r /REBOOTOK "$APPDATA\Microsoft\Windows\Start Menu\Programs\Team MediaPortal"
RMDir /r /REBOOTOK "$APPDATA\Microsoft\Windows\Start Menu\Programs\MediaPortal"
SetShellVarContext current
RMDir /r /REBOOTOK "$APPDATA\Microsoft\Windows\Start Menu\Programs\Team MediaPortal"
RMDir /r /REBOOTOK "$APPDATA\Microsoft\Windows\Start Menu\Programs\MediaPortal"
RMDir /r /REBOOTOK "$LOCALAPPDATA\Microsoft\Windows\Start Menu\Programs\Team MediaPortal"
RMDir /r /REBOOTOK "$LOCALAPPDATA\Microsoft\Windows\Start Menu\Programs\MediaPortal"

# Remove registry keys
DeleteRegKey HKLM "Software\Team MediaPortal"
DeleteRegKey HKCU "Software\Team MediaPortal"

DeleteRegKey HKLM "Software\MediaPortal"
DeleteRegKey HKCU "Software\MediaPortal"

!macroend

!include FileFunc.nsh
!insertmacro un.GetParent
!macro NSISuninstall REG_KEY

  ReadRegStr $R0 HKLM "${REG_KEY}" UninstallString
  ${If} ${FileExists} "$R0"
    ; get parent folder of uninstallation EXE (RO) and save it to R1
    ${un.GetParent} $R0 $R1
    ; start uninstallation of installed MP, from tmp folder, so it will delete itself
    ;HideWindow
    ClearErrors
    CopyFiles $R0 "$TEMP\uninstall-temp.exe"
    ExecWait '"$TEMP\uninstall-temp.exe" _?=$R1 /RemoveAll'
    ;BringToFront

    /*
    ; if an error occured, ask to cancel installation
    ${If} ${Errors}
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_ON_UNINSTALL)" IDYES +2
      Quit
    ${EndIf}
    */
  ${EndIf}
!macroend


#---------------------------------------------------------------------------
#   MediaPortal specific OS SystemCheck
#---------------------------------------------------------------------------
!macro MediaPortalOperatingSystemCheck HideWarnings
# HideWarnings   is used to disable some Warning MessageBoxes if needed, for example:     if $DeployMode = 1
  ${LOG_TEXT} "INFO" ".: Operating System Check :."

  GetVersion::WindowsName
  Pop $R0
  ${LOG_TEXT} "INFO" "GetVersion::WindowsName: $R0"
  !insertmacro GetServicePack $R1 $R2
  ${LOG_TEXT} "INFO" "GetServicePack major: $R1"
  ${LOG_TEXT} "INFO" "GetServicePack minor: $R2"


  ; show error that the OS is not supported and abort the installation
  ${If} ${AtMostWin2000Srv}
    StrCpy $0 "OSabort"
  ${ElseIf} ${IsWinXP}
    !insertmacro GetServicePack $R1 $R2
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 2
      StrCpy $0 "OSabort"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWinXP64}
    StrCpy $0 "OSabort"

  ${ElseIf} ${IsWin2003}
    StrCpy $0 "OSwarn"

  ${ElseIf} ${IsWinVISTA}
    !insertmacro GetServicePack $R1 $R2
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 1
      StrCpy $0 "OSwarn"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWin2008}
    StrCpy $0 "OSwarn"

  ${Else}
    StrCpy $0 "OSabort"
  ${EndIf}

  ; show warnings for some OS
  ${If} $0 == "OSabort"
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${ElseIf} $0 == "OSwarn"
    ${If} ${HideWarnings} == 0
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
    ${EndIf}
  ${ElseIf} $0 == "OSwarnBetaSP"
    ${If} ${HideWarnings} == 0
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "You are using a beta Service Pack! $(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
    ${EndIf}
  ${Else}
    ; do nothing
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!macro MediaPortalAdminCheck HideWarnings
  ${LOG_TEXT} "INFO" ".: Administration Rights Check :."

  ; check if current user is admin
  UserInfo::GetOriginalAccountType
  Pop $0
  #StrCmp $0 "Admin" 0 +3
  ${IfNot} $0 == "Admin"
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_ADMIN)"
    Abort
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!macro MediaPortalVCRedistCheck HideWarnings
  ${LOG_TEXT} "INFO" ".: Microsoft Visual C++ Redistributable Check :."

  ; check if VC Redist 2005 SP1 is installed
  ${IfNot} ${VCRedistIsInstalled}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_VCREDIST)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!macro MediaPortalNetFrameworkCheck HideWarnings
  ${LOG_TEXT} "INFO" ".: Microsoft .Net Framework Check :."

  ; check if .Net Framework 2.0 is installed
  ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727" "Install"
  ; check if .Net Framework 2.0 SP2 is installed
  ReadRegDWORD $1 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727" "SP"

  ; check if .Net Framework 3.5 is installed
  ReadRegDWORD $2 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5" "Install"
  ; check if .Net Framework 3.5 SP1 is installed
  ReadRegDWORD $3 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5" "SP"

  ${LOG_TEXT} "INFO" ".Net 2.0 installed? $0"
  ${LOG_TEXT} "INFO" ".Net 2.0 ServicePack: $1"
  ${LOG_TEXT} "INFO" ".Net 3.5 installed? $2"
  ${LOG_TEXT} "INFO" ".Net 3.5 ServicePack: $3"

  ${If} ${IsWinVISTA}
  ${AndIf} ${RunningX64} ; 3.5 is installed, check for sp1
    ${LOG_TEXT} "INFO" "OS is Vista64, .Net 3.5 is installed."

    ${If} $3 < 1  ; if 3.5, but no sp1
      MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET35_SP)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
      Abort
    ${EndIf}

  ${ElseIf} $0 != 1  ; if no 2.0

    ${If} $2 != 1  ; if no 3.5
      MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET20)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
      Abort
    ${ElseIf} $3 < 1  ; if 3.5, but no sp1
      MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET35_SP)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
      Abort
    ${EndIf}

  ${ElseIf} $1 < 2  ; if 2.0, but no sp2
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET20_SP)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!endif # !___COMMON_MP_MACROS__NSH___

