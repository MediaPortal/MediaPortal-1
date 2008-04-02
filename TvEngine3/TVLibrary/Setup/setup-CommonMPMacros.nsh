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


!include FileFunc.nsh
!include "XML.nsh"

!include FileFunc.nsh
!insertmacro GetRoot

!include WordFunc.nsh
!insertmacro WordReplace

#**********************************************************************************************************#
#
# different useful macros
#
#**********************************************************************************************************#

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

!ifndef MP_REG_UNINSTALL
  !define MP_REG_UNINSTALL      "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!endif
!ifndef TV3_REG_UNINSTALL
  !define TV3_REG_UNINSTALL     "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
!endif

#**********************************************************************************************************#
# LOGICLIB EXPRESSIONS

;======================================   OLD MP INSTALLATION TESTs

!macro _MP022IsInstalled _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{87819CFA-1786-484D-B0DE-10B5FBF2625D}" "UninstallString"
  IfErrors `${_f}` `${_t}`
!macroend
!define MP022IsInstalled `"" MP022IsInstalled ""`

!macro _MP023IsInstalled _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal 0.2.3.0" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MP023IsInstalled `"" MP023IsInstalled ""`

;======================================   OLD TVServer/TVClient INSTALLATION TESTs

!macro _MSI_TVServerIsInstalled _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{4B738773-EE07-413D-AFB7-BB0AB04A5488}" "UninstallString"
  IfErrors `${_f}` `${_t}`
!macroend
!define MSI_TVServerIsInstalled `"" MSI_TVServerIsInstalled ""`

!macro _MSI_TVClientIsInstalled _a _b _t _f
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
  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${MP_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MPIsInstalled `"" MPIsInstalled ""`

!macro _TVServerIsInstalled _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP 0 `${_f}`

  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "MementoSection_SecServer"
  StrCmp $_LOGICLIB_TEMP 1 `${_t}` `${_f}`
!macroend
!define TVServerIsInstalled `"" TVServerIsInstalled ""`

!macro _TVClientIsInstalled _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP 0 `${_f}`

  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "MementoSection_SecClient"
  StrCmp $_LOGICLIB_TEMP 1 `${_t}` `${_f}`
!macroend
!define TVClientIsInstalled `"" TVClientIsInstalled ""`

;======================================   3rd PARTY APPLICATION TESTs

!macro _VCRedistIsInstalled _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{7299052b-02a4-4627-81f2-1818da5d550d}" "DisplayVersion"
  IfErrors `${_f}` 0
  StrCmp $_LOGICLIB_TEMP "8.0.56336" `${_t}` `${_f}`
!macroend
!define VCRedistIsInstalled `"" VCRedistIsInstalled ""`

!macro _dotNetIsInstalled _a _b _t _f
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

  ${If} ${MP023IsInstalled}
    ReadRegStr ${_var} HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
  ${ElseIf} ${MPIsInstalled}
    ReadRegStr ${_var} HKLM "${MP_REG_UNINSTALL}" "InstallPath"
  ${Else}
    StrCpy ${_var} ""
  ${EndIf}

!macroend

!macro TVSERVER_GET_INSTALL_DIR _var

  ${If} ${TVServerIsInstalled}
    ReadRegStr ${_var} HKLM "${TV3_REG_UNINSTALL}" "InstallPath"
  ${Else}
    StrCpy ${_var} ""
  ${EndIf}

!macroend

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
LangString TEXT_MSGBOX_ERROR_VCREDIST             ${LANG_ENGLISH} "Microsoft Visual C++ 2005 SP1 Redistributable Package (x86) is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_DOTNET               ${LANG_ENGLISH} "Microsoft .Net Framework 2 is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"

LangString TEXT_MSGBOX_ERROR_IS_INSTALLED         ${LANG_ENGLISH} "$(^Name) is already installed. You need to uninstall it, before you continue with the installation.$\r$\nUninstall will be lunched when pressing OK."
LangString TEXT_MSGBOX_ERROR_ON_UNINSTALL         ${LANG_ENGLISH} "An error occured while trying to uninstall old version!$\r$\nDo you still want to continue the installation?"
LangString TEXT_MSGBOX_ERROR_REBOOT_REQUIRED      ${LANG_ENGLISH} "A reboot is required after a previous action. Reboot you system and try it again."

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



Function GET_PATH_TEXT

  Pop $R0

  ${xml::GotoPath} "/Config" $0
  ${If} $0 != 0
  #  MessageBox MB_OK "error: xml::GotoPath /Config"
    Goto error
  ${EndIf}

  loop:

  ${xml::FindNextElement} "Dir" $0 $1
  ${If} $1 != 0
  #  MessageBox MB_OK "error: xml::FindNextElement >/Dir< >$0<"
    Goto error
  ${EndIf}
  ;MessageBox MB_OK "xml::FindNextElement$\n$$0=$0$\n$$1=$1"

  ${xml::ElementPath} $0
  ${xml::GetAttribute} "id" $0 $1
  ${If} $1 != 0
  #  MessageBox MB_OK "error: xml::GetAttribute >id< >$0<"
    Goto error
  ${EndIf}
  ${IfThen} $0 == $R0  ${|} Goto foundDir ${|}

  Goto loop


  foundDir:
  ${xml::ElementPath} $0
  ${xml::GotoPath} "$0/Path" $1
  ${If} $1 != 0
  #  MessageBox MB_OK "error: xml::GotoPath >$0/Path<"
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

FunctionEnd
  
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

!macro ReadMPdir DIR

  Push "${DIR}"
  Call GET_PATH_TEXT
  Pop $0
  ${IfThen} $0 == -1 ${|} Goto error ${|}

  ${WordReplace} "$0" "%APPDATA%" "$UserAppData" "+" $0
  ${WordReplace} "$0" "%PROGRAMDATA%" "$CommonAppData" "+" $0

  ${GetRoot} "$0" $1

  ${IfThen} $1 == "" ${|} StrCpy $0 "$MPdir.Base\$0" ${|}

  ; TRIM    \    AT THE END
  StrLen $1 "$0"
      #MessageBox MB_OK "1 $1$\r$\n2 $2$\r$\n3 $3"
  IntOp $2 $1 - 1
      #MessageBox MB_OK "1 $1$\r$\n2 $2$\r$\n3 $3"
  StrCpy $3 $0 1 $2
      #MessageBox MB_OK "1 $1$\r$\n2 $2$\r$\n3 $3"
  
  ${If} $3 == "\"
    StrCpy $MPdir.${DIR} $0 $2
  ${Else}
    StrCpy $MPdir.${DIR} $0
  ${EndIf}

!macroend



Function LoadDefaultDirs

  StrCpy $MPdir.Config              "$MPdir.Base\Config"
  StrCpy $MPdir.Plugins             "$MPdir.Base\plugins"
  StrCpy $MPdir.Log                 "$MPdir.Base\log"
  StrCpy $MPdir.CustomInputDevice   "$MPdir.Base\InputDeviceMappings\custom"
  StrCpy $MPdir.CustomInputDefault  "$MPdir.Base\InputDeviceMappings\defaults"
  StrCpy $MPdir.Skin                "$MPdir.Base\skin"
  StrCpy $MPdir.Language            "$MPdir.Base\language"
  StrCpy $MPdir.Database            "$MPdir.Base\database"
  StrCpy $MPdir.Thumbs              "$MPdir.Base\thumbs"
  StrCpy $MPdir.Weather             "$MPdir.Base\weather"
  StrCpy $MPdir.Cache               "$MPdir.Base\cache"
  StrCpy $MPdir.BurnerSupport       "$MPdir.Base\Burner"

FunctionEnd
Function ReadConfig

  Pop $0
  IfFileExists "$0\MediaPortalDirs.xml" 0 error

  
  #${xml::LoadFile} "$EXEDIR\MediaPortalDirsXP.xml" $0
  ${xml::LoadFile} "$0\MediaPortalDirs.xml" $0
  ${IfThen} $0 != 0 ${|} Goto error ${|}

  #</Dir>  Log CustomInputDevice CustomInputDefault Skin Language Database Thumbs Weather Cache BurnerSupport

  !insertmacro ReadMPdir Config
  !insertmacro ReadMPdir Plugins
  !insertmacro ReadMPdir Log
  !insertmacro ReadMPdir CustomInputDevice
  !insertmacro ReadMPdir CustomInputDefault
  !insertmacro ReadMPdir Skin
  !insertmacro ReadMPdir Language
  !insertmacro ReadMPdir Database
  !insertmacro ReadMPdir Thumbs
  !insertmacro ReadMPdir Weather
  !insertmacro ReadMPdir Cache
  !insertmacro ReadMPdir BurnerSupport


  Push "0"
  Goto end

  error:
  Push "-1"

  end:

FunctionEnd



!define ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirs`
!macro ReadMediaPortalDirs

  !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
  SetShellVarContext current
  StrCpy $MyDocs "$DOCUMENTS"
  StrCpy $UserAppData "$APPDATA"
  SetShellVarContext all
  StrCpy $CommonAppData "$APPDATA"


  Call LoadDefaultDirs

  Push "$MyDocs\Team MediaPortal"
  Call ReadConfig
  Pop $0
  ${If} $0 != 0   ; an error occured
    MessageBox MB_OK "error: read mpdirs.xml in $MyDocs\Team MediaPortal"

    Push "$MPdir.Base"
    Call ReadConfig
    Pop $0
    ${If} $0 != 0   ; an error occured
      MessageBox MB_OK "error: read mpdirs.xml in $MPdir.Base"

      Call LoadDefaultDirs

    ${EndIf}

  ${EndIf}

!macroend
  
  
  
  
  
  
  