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


!ifndef ___COMMON_MP_MACROS__NSH___
!define ___COMMON_MP_MACROS__NSH___


!include LogicLib.nsh
!include x64.nsh
!include "${svn_InstallScripts}\include-WinVerEx.nsh"
!include "${svn_InstallScripts}\include\LanguageMacros.nsh"
!include "${svn_InstallScripts}\include\LoggingMacros.nsh"


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

!macro _VCRedist2005IsInstalled _a _b _t _f

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
!define VCRedist2005IsInstalled `"" VCRedist2005IsInstalled ""`

!macro _VCRedist2008IsInstalled _a _b _t _f

  ClearErrors
  ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" CurrentVersion
  IfErrors `${_f}`

  StrCpy $R1 $R0 3
 
  StrCmp $R1 '5.1' lbl_winnt_XP
  StrCmp $R1 '5.2' lbl_winnt_2003
  StrCmp $R1 '6.0' lbl_winnt_vista `${_f}`


  lbl_winnt_vista:
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc90.crt_1fc8b3b9a1e18e3b_9.0.30729.1_none_e163563597edeada.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc90.mfc_1fc8b3b9a1e18e3b_9.0.30729.1_none_dcc7eae99ad0d9cf.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc90.atl_1fc8b3b9a1e18e3b_9.0.30729.1_none_e29d1181971ae11e.manifest" 0 `${_f}`
  Goto `${_t}`

  lbl_winnt_2003:
  lbl_winnt_XP:
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC90.CRT_1fc8b3b9a1e18e3b_9.0.30729.1_x-ww_6f74963e.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC90.MFC_1fc8b3b9a1e18e3b_9.0.30729.1_x-ww_405b0943.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC90.ATL_1fc8b3b9a1e18e3b_9.0.30729.1_x-ww_d01483b2.manifest" 0 `${_f}`
  Goto `${_t}`
!macroend
!define VCRedist2008IsInstalled `"" VCRedist2008IsInstalled ""`

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
  ;${LOG_TEXT} "DEBUG" "MACRO:MP_GET_INSTALL_DIR"

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
  ;${LOG_TEXT} "DEBUG" "MACRO:TVSERVER_GET_INSTALL_DIR"

  ${If} ${TVServerIsInstalled}
  ${OrIf} ${TVClientIsInstalled}
    ReadRegStr ${_var} HKLM "${TV3_REG_UNINSTALL}" "InstallPath"
    ${LOG_TEXT} "INFO" "TVServer/Client installation dir found: ${_var}"
  ${Else}
    StrCpy ${_var} ""
    ${LOG_TEXT} "INFO" "No TVServer/Client installation found: _var will be empty"
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
DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"

DeleteRegKey HKLM "Software\Team MediaPortal"
DeleteRegKey HKCU "Software\Team MediaPortal"

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
!macro NsisSilentUinstall REG_KEY
!if "${REG_KEY}" == "${TV3_REG_UNINSTALL}"
  ${StopService} "TVservice"
!endif

  ReadRegStr $R0 HKLM "${REG_KEY}" UninstallString
  ${If} ${FileExists} "$R0"
    ; get parent folder of uninstallation EXE (RO) and save it to R1
    ${un.GetParent} $R0 $R1
    ; start uninstallation of installed MP, from tmp folder, so it will delete itself
    ;HideWindow
    ClearErrors
    CopyFiles $R0 "$TEMP\uninstall-temp.exe"
    ExecWait '"$TEMP\uninstall-temp.exe" /S _?=$R1'
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

  ; check if VC Redist 2008 SP1 is installed
  ${IfNot} ${VCRedist2008IsInstalled}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_VCREDIST_2008)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!macro MediaPortalNetFrameworkCheck HideWarnings
  ${LOG_TEXT} "INFO" ".: Microsoft .Net Framework Check :."

  ; check if .Net Framework 3.5 is installed
  ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5" "Install"
  ; check if .Net Framework 3.5 SP1 is installed
  ReadRegDWORD $1 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5" "SP"

  ${LOG_TEXT} "INFO" ".Net 3.5 installed? $2"
  ${LOG_TEXT} "INFO" ".Net 3.5 ServicePack: $3"

  ${If} $0 != 1  ; if no 3.5
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET35)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${ElseIf} $1 < 1  ; if 3.5, but no sp1
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_DOTNET35_SP)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!endif # !___COMMON_MP_MACROS__NSH___

