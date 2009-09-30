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


!ifndef ___MediaPortalDirectories__NSH___
!define ___MediaPortalDirectories__NSH___

!include LogicLib.nsh

!ifndef NO_INSTALL_LOG
  !include "${svn_InstallScripts}\include\LoggingMacros.nsh"
!else

  !ifndef LOG_TEXT
    !define prefixERROR "[ERROR     !!!]   "
    !define prefixDEBUG "[    DEBUG    ]   "
    !define prefixINFO  "[         INFO]   "

    !define LOG_TEXT `!insertmacro LOG_TEXT`
    !macro LOG_TEXT LEVEL TEXT
        DetailPrint "${prefix${LEVEL}}${TEXT}"
    !macroend
  !endif

!endif


!AddPluginDir "${svn_InstallScripts}\XML-plugin\Plugin"
!include "${svn_InstallScripts}\XML-plugin\Include\XML.nsh"

#---------------------------------------------------------------------------
#   Read      Special MediaPortal directories from  xml
#
#           enable it by defining         USE_READ_MP_DIRS      in parent script
#---------------------------------------------------------------------------

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
  ;${LOG_TEXT} "DEBUG" "macro: ReadMPdir | DIR: ${DIR}"

  Push "${DIR}"
  Call ${UNINSTALL_PREFIX}GET_PATH_TEXT
  Pop $0
  ${IfThen} $0 == -1 ${|} Goto error ${|}

  ;${LOG_TEXT} "DEBUG" "macro: ReadMPdir | text found in xml: '$0'"
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
  ;${LOG_TEXT} "DEBUG" "macro: ReadConfig | UNINSTALL_PREFIX: ${UNINSTALL_PREFIX} | PATH_TO_XML: ${PATH_TO_XML}"

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
  StrCpy $MPdir.Skin                "$MPdir.Config\skin"
  StrCpy $MPdir.Language            "$MPdir.Config\language"
  StrCpy $MPdir.Database            "$MPdir.Config\database"
  StrCpy $MPdir.Thumbs              "$MPdir.Config\thumbs"
  StrCpy $MPdir.Weather             "$MPdir.Base\weather"
  StrCpy $MPdir.Cache               "$MPdir.Config\cache"
  StrCpy $MPdir.BurnerSupport       "$MPdir.Base\Burner"

!macroend

!define ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirs ""`
!define un.ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirs "un."`
!macro ReadMediaPortalDirs UNINSTALL_PREFIX INSTDIR
  ;${LOG_TEXT} "DEBUG" "macro ReadMediaPortalDirs"

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
    ${LOG_TEXT} "ERROR" "Loading MediaPortalDirectories from MyDocs failed. ('$MyDocs\Team MediaPortal\MediaPortalDirs.xml')"
    ${LOG_TEXT} "INFO"  "Trying to load from installation directory now."

    Push "$MPdir.Base"
    Call ${UNINSTALL_PREFIX}ReadConfig
    Pop $0
    ${If} $0 != 0   ; an error occured
      ${LOG_TEXT} "ERROR" "Loading MediaPortalDirectories from InstallDir failed. ('$MPdir.Base\MediaPortalDirs.xml')"
      ${LOG_TEXT} "INFO"  "Using default paths for MediaPortalDirectories now."
      !insertmacro LoadDefaultDirs

    ${Else}
      ${LOG_TEXT} "INFO" "Loaded MediaPortalDirectories from InstallDir successfully. ('$MPdir.Base\MediaPortalDirs.xml')"
    ${EndIf}

  ${Else}
    ${LOG_TEXT} "INFO" "Loaded MediaPortalDirectories from MyDocs successfully. ('$MyDocs\Team MediaPortal\MediaPortalDirs.xml')"
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

!endif # !___MediaPortalDirectories__NSH___

