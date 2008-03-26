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

#!include "x64.nsh"
#!include Sections.nsh
#!include LogicLib.nsh
#!include Library.nsh
#!include FileFunc.nsh
#!include WinVer.nsh
#!include Memento.nsh

!include LogicLib.nsh

!include setup-CommonMPMacros.nsh

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important
  
SectionEnd ; end the section

;--------------------------------


Function .onInit

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


  ${If} ${VCRedistIsInstalled}
    MessageBox MB_ICONINFORMATION|MB_OK "VCRedistIsInstalled"
  ${Else}
    MessageBox MB_ICONINFORMATION|MB_OK "no VCRedistIsInstalled"
  ${EndIf}

FunctionEnd