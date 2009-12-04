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

!if "${NSIS_VERSION}" != "v2.45"
  !error "$\r$\n$\r$\nPlease update your NSIS installation to latest version. http://nsis.sourceforge.net$\r$\n$\r$\n"
!endif

!ifndef ___COMMON_MP_MACROS__NSH___
!define ___COMMON_MP_MACROS__NSH___


#---------------------------------------------------------------------------
# import other header files
#---------------------------------------------------------------------------
!include LogicLib.nsh
!include x64.nsh

!ifndef NO_OS_DETECTION
  !include "${svn_InstallScripts}\include\WinVerEx.nsh"
  # references to additional plugins, if not used, these won't be included
  !AddPluginDir "${svn_InstallScripts}\GetVersion-plugin\Plugins"
!endif

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


#---------------------------------------------------------------------------
# Default Definitions
#---------------------------------------------------------------------------
!ifndef WEB_REQUIREMENTS
  !define WEB_REQUIREMENTS "http://wiki.team-mediaportal.com/GeneralRequirements"
!endif

!ifndef MP_REG_UNINSTALL
  !define MP_REG_UNINSTALL  "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!endif
!ifndef TV3_REG_UNINSTALL
  !define TV3_REG_UNINSTALL "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
!endif

; modify your registry and uncomment the following line to test if the svn version check is working
;!define SVN_BUILD
!define MIN_INSTALLED_MP_VERSION      "1.0.4.0"
!define MIN_INSTALLED_MP_VERSION_TEXT "v1.1.0 beta1"
!define WEB_DOWNLOAD_MIN_MP_VERSION   "http://www.team-mediaportal.com/news/global/mediaportal_1.1.0_beta_1_-_released!.html"


#---------------------------------------------------------------------------
# SECTION MACROS
#---------------------------------------------------------------------------
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


!macro EnableSection SectionName SectionTitle
/*
    ; check the component
    ;!insertmacro SelectSection "${SectionName}"
    ; remove the read only flag to the section, see Sections.nsh of official NSIS header files
    ;!insertmacro ClearSectionFlag "${SectionName}" "${SF_RO}"
    ${If} ${SectionIsSectionGroup} "${SectionName}"
      !insertmacro SetSectionFlag "${SectionName}" "${SF_EXPAND}"
    ${EndIf}
    ; set new text for the component
    SectionSetText "${SectionName}" "${SectionTitle}"
*/
!macroend

!macro DisableSection SectionName SectionTitle AddText
    ; uncheck the component, so that it won't be installed
    !insertmacro UnselectSection "${SectionName}"
    ; add the read only flag to the section, see Sections.nsh of official NSIS header files
    !insertmacro SetSectionFlag "${SectionName}" ${SF_RO}
    ${If} ${SectionIsSectionGroup} "${SectionName}"
      !insertmacro ClearSectionFlag "${SectionName}" ${SF_EXPAND}
    ${EndIf}
    ; set new text for the component
    SectionSetText "${SectionName}" "${SectionTitle}${AddText}"
!macroend


#---------------------------------------------------------------------------
# COMMANDLINE PARAMETERS
#---------------------------------------------------------------------------
; gets comandline parameter
!macro InitCommandlineParameterCall UNINSTALL
  ${${UNINSTALL}GetParameters} $R0
  ${LOG_TEXT} "DEBUG" "commandline parameters: $R0"
!macroend
!define InitCommandlineParameter `!insertmacro InitCommandlineParameterCall ""`
!define un.InitCommandlineParameter `!insertmacro InitCommandlineParameterCall "un."`

; check for special parameter and set the their variables, need InitCommandlineParameter first
!macro ReadCommandlineParameterCall UNINSTALL Parameter
  ClearErrors
  ${${UNINSTALL}GetOptions} $R0 "/${Parameter}" $R1
  ${IfNot} ${Errors}
    StrCpy $${Parameter} 1
  ${EndUnless}
!macroend
!define ReadCommandlineParameter `!insertmacro ReadCommandlineParameterCall ""`
!define un.ReadCommandlineParameter `!insertmacro ReadCommandlineParameterCall "un."`


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

  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{87819CFA-1786-484D-B0DE-10B5FBF2625D}" "UninstallString"
  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
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

  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{4B738773-EE07-413D-AFB7-BB0AB04A5488}" "UninstallString"
  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MSI_TVServerIsInstalled `"" MSI_TVServerIsInstalled ""`

!macro _MSI_TVClientIsInstalled _a _b _t _f
  SetRegView 32
  !insertmacro _LOGICLIB_TEMP

  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{F7444E89-5BC0-497E-9650-E50539860DE0}" "UninstallString"
  IfFileExists $_LOGICLIB_TEMP `${_t}`

  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}" "UninstallString"
  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
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

!macro _VCRedist2008IsInstalled _a _b _t _f

  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc90.atl_1fc8b3b9a1e18e3b_9.0.30729.4148_none_51ca66a2bbe76806.manifest" 0 +2
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc90.crt_1fc8b3b9a1e18e3b_9.0.30729.4148_none_5090ab56bcba71c2.manifest" 0 +4
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc90.mfc_1fc8b3b9a1e18e3b_9.0.30729.4148_none_4bf5400abf9d60b7.manifest" 0 +3
  Goto `${_t}`

  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC90.ATL_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_353599c2.manifest" 0 +4
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC90.CRT_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_d495ac4e.manifest" 0 +3
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC90.MFC_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_a57c1f53.manifest" 0 +2
  Goto `${_t}`

  Goto `${_f}`
!macroend
!define VCRedist2008IsInstalled `"" VCRedist2008IsInstalled ""`

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


!macro ReadPreviousVersion
  Push $R0
  Push $R1
  Push $R2
  Push $R3

  ReadRegDWORD $R0 HKLM "${REG_UNINSTALL}" "VersionMajor"
  ReadRegDWORD $R1 HKLM "${REG_UNINSTALL}" "VersionMinor"
  ReadRegDWORD $R2 HKLM "${REG_UNINSTALL}" "VersionRevision"
  ReadRegDWORD $R3 HKLM "${REG_UNINSTALL}" "VersionBuild"
  ${If} $R0 == ""
  ${OrIf} $R1 == ""
  ${OrIf} $R2 == ""
  ${OrIf} $R3 == ""
    StrCpy $PREVIOUS_VERSION ""
  ${Else}
    StrCpy $PREVIOUS_VERSION $R0.$R1.$R2.$R3
  ${EndIf}

  ${VersionCompare} ${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD} $PREVIOUS_VERSION $R0
  ${If} $R0 == 0
    StrCpy $PREVIOUS_VERSION_STATE "same"
  ${ElseIf} $R0 == 1
    StrCpy $PREVIOUS_VERSION_STATE "newer"
  ${ElseIf} $R0 == 2
    StrCpy $PREVIOUS_VERSION_STATE "older"
  ${Else}
    StrCpy $PREVIOUS_VERSION_STATE ""
  ${EndIf}

  Pop $R3
  Pop $R2
  Pop $R1
  Pop $R0
!macroend


#**********************************************************************************************************#
# other MP helper
!macro SetRights
  ${LOG_TEXT} "INFO" "Setting AccessRights to ProgramData dir and reg keys"

  SetOverwrite on
  SetOutPath "$PLUGINSDIR"
  File "${svn_ROOT}\Tools\Script & Batch tools\SetRights\bin\Release\SetRights.exe"

  SetShellVarContext all
  nsExec::ExecToLog '"$PLUGINSDIR\SetRights.exe" FOLDER "$APPDATA\Team MediaPortal"'
  nsExec::ExecToLog '"$PLUGINSDIR\SetRights.exe" HKLM "Software\Team MediaPortal"'
  ;nsExec::ExecToLog '"$PLUGINSDIR\SetRights.exe" HKCU "Software\Team MediaPortal"'
!macroend


!macro UpdateBackupSections
  ${If} ${FileExists} "$MPdir.Base\*.*"
    !insertmacro EnableSection "${SecBackupInstDir}" "Installation directory"
  ${Else}
    !insertmacro DisableSection "${SecBackupInstDir}" "Installation directory" " "
  ${EndIf}

  ${If} ${FileExists} "$MPdir.Config\*.*"
    !insertmacro EnableSection "${SecBackupConfig}" "Configuration directory"
  ${Else}
    !insertmacro DisableSection "${SecBackupConfig}" "Configuration directory" " "
  ${EndIf}

  ${If} ${FileExists} "$MPdir.Thumbs\*.*"
    !insertmacro EnableSection "${SecBackupThumbs}" "Thumbs directory"
  ${Else}
    !insertmacro DisableSection "${SecBackupThumbs}" "Thumbs directory" " "
  ${EndIf}

  ${If} ${SectionIsReadOnly} ${SecBackupInstDir}
  ${AndIf} ${SectionIsReadOnly} ${SecBackupConfig}
  ${AndIf} ${SectionIsReadOnly} ${SecBackupThumbs}
    !insertmacro DisableSection "${SecBackup}" "Backup" " "
  ${Else}
    !insertmacro EnableSection "${SecBackup}" "Backup"
  ${EndIf}
!macroend

!macro BackupConfigDir
  ${LOG_TEXT} "INFO" "Creating backup of configuration dir"

  ${WordReplace} "$MPdir.Thumbs" "$MPdir.Config\" "" "+" $R1  ; is thumbs a subdir of config?
  ${If} "$R1" == "$MPdir.Thumbs"  ; no replace >> thumbs is not a subdir of config
    ${LOG_TEXT} "INFO" "BackupConfigDir: Thumbs is NOT a subdir of config"

    ${LOG_TEXT} "INFO" "BackupConfigDir: Copying complete config-dir"
    CreateDirectory "$MPdir.Config_$R0"
    CopyFiles /SILENT "$MPdir.Config\*.*" "$MPdir.Config_$R0"
  ${Else}
    ${LOG_TEXT} "INFO" "BackupConfigDir: Thumbs is a subdir of config"

    ${LOG_TEXT} "INFO" "BackupConfigDir: Rename thumbs-dir to get it out of config-dir"
    Rename "$MPdir.Thumbs" "$MPdir.Config$R0$R1"

    ${LOG_TEXT} "INFO" "BackupConfigDir: Copying complete config-dir"
    CreateDirectory "$MPdir.Config_$R0"
    CopyFiles /SILENT "$MPdir.Config\*.*" "$MPdir.Config_$R0"

    ${LOG_TEXT} "INFO" "BackupConfigDir: Rename thumbs-dir to get it back in config-dir"
    Rename "$MPdir.Config$R0$R1" "$MPdir.Thumbs"
  ${EndIf}
!macroend

!macro BackupThumbsDir
  ${LOG_TEXT} "INFO" "Creating backup of thumbs dir"

  ${WordReplace} "$MPdir.Thumbs" "$MPdir.Config\" "" "+" $R1  ; is thumbs a subdir of config?
  ${If} "$R1" == "$MPdir.Thumbs"  ; no replace >> thumbs is not a subdir of config
    ${LOG_TEXT} "INFO" "BackupThumbsDir: Thumbs is NOT a subdir of config"

    ${LOG_TEXT} "INFO" "BackupThumbsDir: Copying complete thumbs-dir"
    CreateDirectory "$MPdir.Thumbs_$R0"
    CopyFiles /SILENT "$MPdir.Thumbs\*.*" "$MPdir.Thumbs_$R0"
  ${Else}
    ${LOG_TEXT} "INFO" "BackupThumbsDir: Thumbs is a subdir of config"

    ${LOG_TEXT} "INFO" "BackupThumbsDir: Copying complete thumbs-dir"
    CreateDirectory "$MPdir.Config_$R0\$R1"  ; create thumbsdir in config-backupdir
    CopyFiles /SILENT "$MPdir.Thumbs\*.*" "$MPdir.Config_$R0\$R1"
  ${EndIf}
!macroend


#---------------------------------------------------------------------------
#   COMPLETE MEDIAPORTAL CLEANUP
#---------------------------------------------------------------------------
!macro CompleteMediaPortalCleanup

# make and uninstallation of the other app, which may be still installed
!if "${PRODUCT_NAME}" == "MediaPortal"
  !insertmacro NSISuninstall "${TV3_REG_UNINSTALL}"
!endif
!if "${PRODUCT_NAME}" == "MediaPortal TV Server / Client"
  !insertmacro NSISuninstall "${MP_REG_UNINSTALL}"
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
DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"

DeleteRegKey HKLM "Software\Team MediaPortal"
DeleteRegKey HKCU "Software\Team MediaPortal"

DeleteRegKey HKLM "Software\MediaPortal"
DeleteRegKey HKCU "Software\MediaPortal"


# And again to be sure
DeleteRegKey HKLM "Software\Team MediaPortal"
DeleteRegKey HKCU "Software\Team MediaPortal"

DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"

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

!macro RunUninstaller SILENT

  ReadRegStr $R0 HKLM "${REG_UNINSTALL}" "UninstallString"
  ${If} ${FileExists} "$R0"

!if "${SILENT}" != "silent"
    ; Run uninstaller nonsilent, hide the installer windows
    HideWindow
!endif

    ; get installation dir from uninstaller.exe path
    ${GetParent} $R0 $R1

    ; clearerrors, to catch if uninstall fails
    ClearErrors
    ; copy uninstaller to temp, to make sure uninstaller.exe in instdir is deleted, too
    CopyFiles $R0 "$TEMP\uninstall-temp.exe"

    ; launch uninstaller
    ${If} $PREVIOUS_VERSION_STATE == "same"
    ${AndIf} $EXPRESS_UPDATE == "1"
      ExecWait '"$TEMP\uninstall-temp.exe" _?=$R1'
    ${Else}

!if "${SILENT}" != "silent"
      ExecWait '"$TEMP\uninstall-temp.exe" /frominstall _?=$R1'
!else
      ExecWait '"$TEMP\uninstall-temp.exe" /S _?=$R1'
!endif

    ${EndIf}

  ${EndIf}

!macroend


#---------------------------------------------------------------------------
#   MediaPortal specific OS SystemCheck
#---------------------------------------------------------------------------
!ifndef NO_OS_DETECTION

!macro ShowMissingComponent MISSING_COMPONENTS

    ExecShell open "${WEB_REQUIREMENTS}"
    StrCpy $0 ""
    StrCpy $0 "$0$(MISSING_COMPONENT_INTRO)$\r$\n"
    StrCpy $0 "$0$(MISSING_COMPONENT_INSTALL)$\r$\n$\r$\n"
    StrCpy $0 "$0${MISSING_COMPONENTS}$\r$\n$\r$\n"
    StrCpy $0 "$0$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n"
    StrCpy $0 "$0$(TEXT_MISSING_COMPONENT_MORE_INFO)"
    MessageBox MB_OK|MB_ICONSTOP "$0"
    Abort

!macroend

!macro MediaPortalOperatingSystemCheck
  ${LOG_TEXT} "INFO" ".: Operating System Check :."


  !insertmacro GetServicePack $R1 $R2
  ${LOG_TEXT} "INFO" "GetServicePack-Macro:: major: $R1"
  ${LOG_TEXT} "INFO" "GetServicePack-Macro:: minor: $R2"

  GetVersion::WindowsName
  Pop $R0
  ${LOG_TEXT} "INFO" "GetVersion-Plugin::WindowsName: $R0"


  ; show error that the OS is not supported and abort the installation
  ${If} ${AtMostWin2000}
    ${LOG_TEXT} "INFO" "OSTest::AtMostWin2000"
    StrCpy $0 "OSabort"

  ${ElseIf} ${IsWinXP}
    ${LOG_TEXT} "INFO" "OSTest::IsWinXP"
    !insertmacro GetServicePack $R1 $R2
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 2
      StrCpy $0 "OSabort"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ;${ElseIf} ${IsWinXP64}
    ${If} ${RunningX64}
      ${LOG_TEXT} "INFO" "OSTest::IsWinXP::x64"
      StrCpy $0 "OSabort"
    ${EndIf}

  ${ElseIf} ${IsWin2003}
    ${LOG_TEXT} "INFO" "OSTest::IsWin2003"
    StrCpy $0 "OSwarn"

  ${ElseIf} ${IsWinVISTA}
    ${LOG_TEXT} "INFO" "OSTest::IsWinVISTA"
    !insertmacro GetServicePack $R1 $R2
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 1
      StrCpy $0 "OSwarn"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWin2008}
    ${LOG_TEXT} "INFO" "OSTest::IsWin2008"
    StrCpy $0 "OSwarn"

  ${ElseIf} ${IsWin7}
    ${LOG_TEXT} "INFO" "OSTest::IsWin7"
    StrCpy $0 "OSok"

  ${ElseIf} ${IsWin2008R2}
    ${LOG_TEXT} "INFO" "OSTest::IsWin2008R2"
    StrCpy $0 "OSwarn"

  ${Else}
    ${LOG_TEXT} "INFO" "OSTest::unknown OS"
    StrCpy $0 "OSabort"
  ${EndIf}

  ; show warnings for some OS
  ${If} $0 == "OSabort"
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${ElseIf} $0 == "OSwarn"
    MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
  ${ElseIf} $0 == "OSwarnBetaSP"
    MessageBox MB_YESNO|MB_ICONEXCLAMATION "You are using a beta Service Pack! $(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
  ${Else}
    ; do nothing
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!macro MediaPortalAdminCheck
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

!macro MediaPortalVCRedistCheck
  ${LOG_TEXT} "INFO" ".: Microsoft Visual C++ Redistributable Check :."

  ; check if VC Redist 2008 SP1 is installed
  ${IfNot} ${VCRedist2008IsInstalled}
    !insertmacro ShowMissingComponent "     - Microsoft Visual C++ 2008 Service Pack 1 Redistributable Package ATL Security Update"
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!macro MediaPortalNetFrameworkCheck
  ${LOG_TEXT} "INFO" ".: Microsoft .Net Framework Check :."

  ; check if .Net Framework 3.5 is installed
  ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5" "Install"
  ; check if .Net Framework 3.5 SP1 is installed
  ReadRegDWORD $1 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5" "SP"

  ${LOG_TEXT} "INFO" ".Net 3.5 installed? $0"
  ${LOG_TEXT} "INFO" ".Net 3.5 ServicePack: $1"

  ${If} $0 != 1  ; if no 3.5
  ${OrIf} $1 < 1  ; if 3.5, but no sp1
    !insertmacro ShowMissingComponent "     - Microsoft .NET Framework 3.5 Service Pack 1"
  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend

!ifdef SVN_BUILD
!macro MinimumVersionForSVNCheck
  ${LOG_TEXT} "INFO" ".: MinimumVersionForSVNCheck: Compare installed and minimum version for this SVN snapshot :."

!if "${PRODUCT_NAME}" == "MediaPortal"
  ${IfNot} ${MPIsInstalled}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_SVN_NOMP)" IDNO +2
    ExecShell open "${WEB_DOWNLOAD_MIN_MP_VERSION}"
    Abort
  ${Else}

    !insertmacro MP_GET_VERSION $R0
!endif

!if "${PRODUCT_NAME}" == "MediaPortal TV Server / Client"
  ${IfNot} ${TVServerIsInstalled}
  ${AndIfNot} ${TVClientIsInstalled}
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_SVN_NOMP)" IDNO +2
    ExecShell open "${WEB_DOWNLOAD_MIN_MP_VERSION}"
    Abort
  ${Else}

    !insertmacro TVSERVER_GET_VERSION $R0
!endif

    ${VersionCompare} $R0 ${MIN_INSTALLED_MP_VERSION} $R0
    ${If} $R0 == 0
      ; installed version is EQUAL to min
    ${ElseIf} $R0 == 1
      ; installed version is NEWER than min
    ${ElseIf} $R0 == 2
      ; installed version is OLDER than min
      MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_SVN_WRONG_VERSION)" IDNO +2
      ExecShell open "${WEB_DOWNLOAD_MIN_MP_VERSION}"
      Abort
    ${Else}
      ; installed version is: not found
    ${EndIf}

  ${EndIf}

  ${LOG_TEXT} "INFO" "============================"
!macroend
!endif


!if "${PRODUCT_NAME}" == "MediaPortal"

!macro DoPreInstallChecks

!ifdef SVN_BUILD
  ; check if correct MP version ist installed, which is required for this svn snapshot
  !insertmacro MinimumVersionForSVNCheck
!endif

  ; OS and other common initialization checks are done in the following NSIS header file
  !insertmacro MediaPortalOperatingSystemCheck
  !insertmacro MediaPortalAdminCheck
  !insertmacro MediaPortalVCRedistCheck
  !insertmacro MediaPortalNetFrameworkCheck

  ; check if old mp 0.2.2 is installed
  ${If} ${MP022IsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MP022)"
    Abort
  ${EndIf}

  ; check if old mp 0.2.3 RC3 is installed
  ${If} ${MP023RC3IsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MP023RC3)"
    Abort
  ${EndIf}

  ; check if old mp 0.2.3 is installed.
  ${If} ${MP023IsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MP023)"
    Abort
  ${EndIf}

  ; check if reboot is required
  ${If} ${FileExists} "$MPdir.Base\rebootflag"
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
    Abort
  ${EndIf}

!macroend

!endif

!if "${PRODUCT_NAME}" == "MediaPortal TV Server / Client"

!macro DoPreInstallChecks

!ifdef SVN_BUILD
  ; check if correct MP version ist installed, which is required for this svn snapshot
  !insertmacro MinimumVersionForSVNCheck
!endif

  ; OS and other common initialization checks are done in the following NSIS header file
  !insertmacro MediaPortalOperatingSystemCheck
  !insertmacro MediaPortalAdminCheck
  !insertmacro MediaPortalVCRedistCheck
  !insertmacro MediaPortalNetFrameworkCheck

  ; check if old msi based client plugin is installed.
  ${If} ${MSI_TVClientIsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MSI_CLIENT)"
    Abort
  ${EndIf}

  ; check if old msi based server is installed.
  ${If} ${MSI_TVServerIsInstalled}
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_MSI_SERVER)"
    Abort
  ${EndIf}

  ; check if reboot is required
  ${If} ${FileExists} "$INSTDIR\rebootflag"
    MessageBox MB_OK|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_REBOOT_REQUIRED)"
    Abort
  ${EndIf}

!macroend

!endif

!endif

!endif # !___COMMON_MP_MACROS__NSH___

