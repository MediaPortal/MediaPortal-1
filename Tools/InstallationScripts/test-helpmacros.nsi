; TEST APP FOR MP STUFF

; The name of the installer
Name "test-helpmacros"

; The file to write
OutFile "test-helpmacros.exe"

; The default installation directory
InstallDir $DESKTOP\Example1

; Request application privileges for Windows Vista
RequestExecutionLevel user

;--------------------------------

Page license

;--------------------------------

!define INSTALL_LOG_FILE "$DESKTOP\install_$(^Name).log"

#!include "x64.nsh"
#!include Sections.nsh
#!include LogicLib.nsh
#!include Library.nsh
#!include FileFunc.nsh
;!include WinVer.nsh
!define WinVer++
#!include Memento.nsh

!include LogicLib.nsh

!include setup-CommonMPMacros.nsh

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important
  
SectionEnd ; end the section

;--------------------------------

!macro DoInstallChecks

  ${If} ${MP023IsInstalled}
    !insertmacro MP_GET_INSTALL_DIR $R0
    MessageBox MB_ICONINFORMATION|MB_OK "MP_GET_INSTALL_DIR: $R0"
  ${Else}
    MessageBox MB_ICONINFORMATION|MB_OK "no MP023IsInstalled"
  ${EndIf}

  ${If} ${MPIsInstalled}
    !insertmacro MP_GET_INSTALL_DIR $R0
    MessageBox MB_ICONINFORMATION|MB_OK "MP_GET_INSTALL_DIR: $R0"
  ${Else}
    MessageBox MB_ICONINFORMATION|MB_OK "no MPIsInstalled"
  ${EndIf}

  ${If} ${TVServerIsInstalled}
    !insertmacro TVSERVER_GET_INSTALL_DIR $R0
    MessageBox MB_ICONINFORMATION|MB_OK "TVSERVER_GET_INSTALL_DIR: $R0"
  ${Else}
    MessageBox MB_ICONINFORMATION|MB_OK "no TVServerIsInstalled"
  ${EndIf}


  ${If} ${MSI_TVServerIsInstalled}
    MessageBox MB_ICONINFORMATION|MB_OK "MSI_TVServerIsInstalled"
  ${Else}
    MessageBox MB_ICONINFORMATION|MB_OK "no MSI_TVServerIsInstalled"
  ${EndIf}

  ${If} ${MSI_TVClientIsInstalled}
    MessageBox MB_ICONINFORMATION|MB_OK "MSI_TVClientIsInstalled"
  ${Else}
    MessageBox MB_ICONINFORMATION|MB_OK "no MSI_TVClientIsInstalled"
  ${EndIf}

!macroend

!macro OperationSystemChecks

  ; show error that the OS is not supported and abort the installation
  ${If} ${AtMostWin2000Srv}
    MessageBox MB_ICONINFORMATION|MB_OK "AtMostWin2000Srv"
    StrCpy $0 "OSabort"
  ${ElseIf} ${IsWinXP}
    MessageBox MB_ICONINFORMATION|MB_OK "IsWinXP"
    !insertmacro GetServicePack $R1 $R2
    MessageBox MB_ICONINFORMATION|MB_OK "SP major: $R1"
    MessageBox MB_ICONINFORMATION|MB_OK "SP minor: $R2"
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 2
      StrCpy $0 "OSabort"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWinXP64}
    MessageBox MB_ICONINFORMATION|MB_OK "IsWinXP64"
    StrCpy $0 "OSabort"

  ${ElseIf} ${IsWin2003}
    MessageBox MB_ICONINFORMATION|MB_OK "IsWin2003"
    StrCpy $0 "OSwarn"

  ${ElseIf} ${IsWinVISTA}
    MessageBox MB_ICONINFORMATION|MB_OK "IsWinVISTA"
    !insertmacro GetServicePack $R1 $R2
    MessageBox MB_ICONINFORMATION|MB_OK "SP major: $R1"
    MessageBox MB_ICONINFORMATION|MB_OK "SP minor: $R2"
    ${If} $R2 > 0
      StrCpy $0 "OSwarnBetaSP"
    ${ElseIf} $R1 < 1
      StrCpy $0 "OSwarn"
    ${Else}
      StrCpy $0 "OSok"
    ${EndIf}

  ${ElseIf} ${IsWin2008}
    MessageBox MB_ICONINFORMATION|MB_OK "IsWin2008"
    StrCpy $0 "OSwarn"

  ${Else}
    MessageBox MB_ICONINFORMATION|MB_OK "unknown OS"
    StrCpy $0 "OSabort"
  ${EndIf}

  ; show warnings for some OS
  ${If} $0 == "OSabort"
    MessageBox MB_YESNO|MB_ICONSTOP "$(TEXT_MSGBOX_ERROR_WIN)" IDNO +2
    ExecShell open "${WEB_REQUIREMENTS}"
    Abort
  ${ElseIf} $0 == "OSwarn"
    ${If} $DeployMode == 0
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "$(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
    ${EndIf}
  ${ElseIf} $0 == "OSwarnBetaSP"
    ${If} $DeployMode == 0
      MessageBox MB_YESNO|MB_ICONEXCLAMATION "You are using a beta Service Pack! $(TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED)" IDNO +2
      ExecShell open "${WEB_REQUIREMENTS}"
    ${EndIf}
  ${Else}
    ; do nothing
  ${EndIf}

!macroend

Function .onInit
  ${LOG_OPEN}


  MessageBox MB_ICONINFORMATION|MB_YESNO "Do OS detection checks?" IDNO noOperationSystemChecks
    !insertmacro OperationSystemChecks

  noOperationSystemChecks:



  MessageBox MB_ICONINFORMATION|MB_YESNO "Do vcr InstallCheck?" IDNO novcrInstallCheck

  ${If} ${VCRedistIsInstalled}
    MessageBox MB_ICONINFORMATION|MB_OK "vcr IsInstalled"
  ${Else}
    MessageBox MB_ICONINFORMATION|MB_OK "no vcr IsInstalled"
  ${EndIf}

  novcrInstallCheck:



  MessageBox MB_ICONINFORMATION|MB_YESNO "Do MP InstallChecks?" IDNO noInstallChecks
    !insertmacro DoInstallChecks

  noInstallChecks:



  MessageBox MB_ICONINFORMATION|MB_YESNO "Do kill process test?" IDNO noKillProcess

  ${KILLPROCESS} "MPInstaller.exe"
  ${KILLPROCESS} "makensisw.exe"
  ${KILLPROCESS} "Input Service Configuration.exe"

  MessageBox MB_ICONINFORMATION|MB_OK "KillProcess FINISHED"

  noKillProcess:



  MessageBox MB_ICONINFORMATION|MB_YESNO "DoXmlTests?" IDNO noXmlTests

  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    MessageBox MB_ICONINFORMATION|MB_OK "no MPIsInstalled"
  ${else}
    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${ReadMediaPortalDirs} $MPdir.Base
  ${EndIf}

      MessageBox MB_ICONINFORMATION|MB_OK "Found the following Entries: \
      $\r$\nBase:  $MPdir.Base$\r$\n \
      $\r$\nConfig:  $MPdir.Config \
      $\r$\nPlugins: $MPdir.Plugins \
      $\r$\nLog: $MPdir.Log \
      $\r$\nCustomInputDevice: $MPdir.CustomInputDevice \
      $\r$\nCustomInputDefault: $MPdir.CustomInputDefault \
      $\r$\nSkin: $MPdir.Skin \
      $\r$\nLanguage: $MPdir.Language \
      $\r$\nDatabase: $MPdir.Database \
      $\r$\nThumbs: $MPdir.Thumbs \
      $\r$\nWeather: $MPdir.Weather \
      $\r$\nCache: $MPdir.Cache \
      $\r$\nBurnerSupport: $MPdir.BurnerSupport \
      "

  noXmlTests:



  Abort

FunctionEnd

Function .onInstFailed
  ${LOG_CLOSE}
FunctionEnd

Function .onInstSuccess
  ${LOG_CLOSE}
FunctionEnd

