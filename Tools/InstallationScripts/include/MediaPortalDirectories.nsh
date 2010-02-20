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

!ifndef ___MediaPortalDirectories__NSH___
!define ___MediaPortalDirectories__NSH___

!include LogicLib.nsh
!include FileFunc.nsh
!include WordFunc.nsh

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

!define ReadMPdir `!insertmacro ReadMPdir`
!macro ReadMPdir DIR

  ${xml::RootElement} $0 $1
  IntCmp $1 -1 ${DIR}_fail
  ${xml::XPathNode} "//Config/Dir[@id='${DIR}']/Path" $1
  IntCmp $1 -1 ${DIR}_fail
  ${xml::GetText} $0 $1
  IntCmp $1 -1 ${DIR}_fail

  ${WordReplace} "$0" "%APPDATA%" "$UserAppData" "+" $0
  ${WordReplace} "$0" "%PROGRAMDATA%" "$CommonAppData" "+" $0

  ; if there is no root, it is relative to MediaPortal's base dir
  ${GetRoot} "$0" $1
  ${If} $1 == ""
    StrCpy $0 "$MPdir.Base\$0"
  ${EndIf}

  # trim   \   at the end of the path
  ; path length
  StrLen $1 "$0"
  IntOp $2 $1 - 1
  ; get last char from path
  StrCpy $3 $0 1 $2

  ${If} $3 == "\"
    StrCpy $MPdir.${DIR} $0 $2
  ${Else}
    StrCpy $MPdir.${DIR} $0
  ${EndIf}

  Goto ${DIR}_done
  ${DIR}_fail:
    ${LOG_TEXT} "ERROR" "Reading ${DIR}-dir from MediaPortalDirs.xml failed."
    ${LOG_TEXT} "INFO" "  Using default: $MPdir.${DIR}"
  ${DIR}_done:

!macroend

#***************************
#***************************

!define LoadDefaultDirs `!insertmacro LoadDefaultDirs`
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

#***************************
#***************************

!define ReadConfig `!insertmacro ReadConfigCall`
!macro ReadConfigCall _PATH_TO_XML _RESULT
  !verbose push
  !verbose ${_MediaPortalDirectories_VERBOSE}
  Push `${_PATH_TO_XML}`
  ${CallArtificialFunction2} ReadConfig_
  Pop ${_RESULT}
  !verbose pop
!macroend

!macro ReadConfig_
  !verbose push
  !verbose 3

  Exch $0 ;_PATH_TO_XML
  Push $1
  Push $2
  Push $3
  Push $4


  IfFileExists "$0\MediaPortalDirs.xml" 0 ReadConfig_fail

  ${xml::LoadFile} "$0\MediaPortalDirs.xml" $1
  IntCmp $1 -1 ReadConfig_fail

  ${LoadDefaultDirs}
  ${ReadMPdir} Config
  ${ReadMPdir} Plugins
  ${ReadMPdir} Log
  ${ReadMPdir} CustomInputDevice
  ${ReadMPdir} CustomInputDefault
  ${ReadMPdir} Skin
  ${ReadMPdir} Language
  ${ReadMPdir} Database
  ${ReadMPdir} Thumbs
  ${ReadMPdir} Weather
  ${ReadMPdir} Cache
  ${ReadMPdir} BurnerSupport


  StrCpy $0 "0"
  Goto ReadConfig_done

  ReadConfig_fail:
  StrCpy $0 "-1"
  ReadConfig_done:

  Pop $4
  Pop $3
  Pop $2
  Pop $1
  Exch $0

  !verbose pop
!macroend

#***************************
#***************************

!define ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirsCall`
!define un.ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirsCall`
!macro ReadMediaPortalDirsCall INSTDIR
  !verbose push
  !verbose 3

  Push $0


  StrCpy $MPdir.Base "${INSTDIR}"
  SetShellVarContext current
  StrCpy $MyDocs "$DOCUMENTS"
  StrCpy $UserAppData "$APPDATA"
  SetShellVarContext all
  StrCpy $CommonAppData "$APPDATA"

  ${LoadDefaultDirs}

  ${ReadConfig} "$MyDocs\Team MediaPortal" $0
  ${If} $0 != 0   ; an error occured
    ${LOG_TEXT} "ERROR" "Loading MediaPortalDirectories from MyDocs failed. ('$MyDocs\Team MediaPortal\MediaPortalDirs.xml')"
    ${LOG_TEXT} "INFO"  "Trying to load from installation directory now."

    ${ReadConfig} "$MPdir.Base" $0
    ${If} $0 != 0   ; an error occured
      ${LOG_TEXT} "ERROR" "Loading MediaPortalDirectories from InstallDir failed. ('$MPdir.Base\MediaPortalDirs.xml')"
      ${LOG_TEXT} "INFO"  "Using default paths for MediaPortalDirectories now."

      ${LoadDefaultDirs}

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


  Pop $0

  !verbose pop
!macroend

!endif # !___MediaPortalDirectories__NSH___

