;======================================
; Xface Skin.nsi
;
; (C) Copyright Harley, 2009
;======================================


#---------------------------------------------------------------------------
# SPECIAL BUILDS
#---------------------------------------------------------------------------
##### BUILD_TYPE
# Uncomment the following line to create a setup in debug mode
;!define BUILD_TYPE "Debug"
# parameter for command line execution: /DBUILD_TYPE=Debug
# by default BUILD_TYPE is set to "Release"


#---------------------------------------------------------------------------
# DEVELOPMENT ENVIRONMENT
#---------------------------------------------------------------------------
# path definitions
!define svn_ROOT "..\..\.."
!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_DirectShowFilters "${svn_ROOT}\DirectShowFilters"
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"

!define svn_xface "${svn_ROOT}\MediaPortal Skins\Xface"


#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define PRODUCT_NAME          "Xface"
!define PRODUCT_PUBLISHER     "Harley"
!define PRODUCT_WEB_SITE      "http://www.team-mediaportal.com/files/Download/Skins/16:9/XfaceSkin/"

; VER_BUILD is set to zero for Release builds
!define VER_MAJOR       1
!define VER_MINOR       9
!define VER_REVISION    0
!define VER_BUILD       0

!define VERSION "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}"

BrandingText "${PRODUCT_NAME} - ${VERSION} by ${PRODUCT_PUBLISHER}"
SetCompressor /SOLID /FINAL lzma

; enable logging
!define INSTALL_LOG

; to use default path to logfile, COMMON_APPDATA has to be defined
; default logfile is: "${COMMON_APPDATA}\Logs\install_${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}.log"
; if you want to set custom path to logfile, uncomment the following line
#!define INSTALL_LOG_FILE "$DESKTOP\install_$(^Name).log"

;======================================

!include x64.nsh
!include MUI2.nsh
!include Sections.nsh
!include LogicLib.nsh


!include include\LanguageMacros.nsh

!define NO_INSTALL_LOG
!include "${svn_InstallScripts}\include\LoggingMacros.nsh"
!include "${svn_InstallScripts}\include\MediaPortalDirectories.nsh"
!include "${svn_InstallScripts}\include\MediaPortalMacros.nsh"




!include pages\SelectOptionPage.nsh
!macro SelectOptionPageList MacroName
  ; This macro used to define the SelectOption pages.
  ; List all the option pages which should be displayed here.
  !insertmacro "${MacroName}" "EPG"               "11rows"            "9rows"
  !insertmacro "${MacroName}" "Basichome"        "Video" "MovingPictures"
  !insertmacro "${MacroName}" "Topbar"            "withShortcuts"  "withoutShortcuts"
!macroend

;======================================

Name "${PRODUCT_NAME}"
OutFile "..\${PRODUCT_NAME} - ${VERSION}.exe"
InstallDir ""

ShowInstDetails show
CRCCheck On


#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ABORTWARNING
!define MUI_ICON                        "images\XFACE.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP          "images\header.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP    "images\wizard.bmp"
!define MUI_HEADERIMAGE_RIGHT

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_FINISHPAGE_NOAUTOCLOSE
;!define MUI_FINISHPAGE_RUN_NOTCHECKED
;!define MUI_FINISHPAGE_RUN      "$DIR_INSTALL\Input Service Configuration\Input Service Configuration.exe"
;!define MUI_FINISHPAGE_RUN_TEXT "Run Input Service Configuration"


#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
!insertmacro MUI_PAGE_WELCOME

; MediaPortal install path
!define MUI_PAGE_HEADER_TEXT "$(TEXT_MPDIR_HEADER)"
!define MUI_PAGE_HEADER_SUBTEXT "$(TEXT_MPDIR_SUBTEXT)"
!define MUI_DIRECTORYPAGE_TEXT_TOP "$(TEXT_MPDIR_TOP)"
!define MUI_DIRECTORYPAGE_TEXT_DESTINATION "$(TEXT_MPDIR_DESTINATION)"
!define MUI_DIRECTORYPAGE_VARIABLE "$MPdir.Base"
!define MUI_PAGE_CUSTOMFUNCTION_PRE DirectoryPreMP
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE DirectoryLeaveMP
!insertmacro MUI_PAGE_DIRECTORY

!insertmacro SelectOptionPageList SelectOptionPage
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

# Installer languages
!insertmacro LANG_LOAD "English"



Section "-Core"

  SetOverwrite on
  SetOutPath "$MPdir.Skin\Xface"

  DetailPrint "core files ..."
  File /r /x .svn "${svn_xface}\Xface\*.*"
  
  !insertmacro SelectOptionPageList SelectOptionInstall

SectionEnd


;======================================

Function .onInit

  !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
  ${ReadMediaPortalDirs} $MPdir.Base

  InitPluginsDir
  !insertmacro SelectOptionPageList SelectOptionOnInit

FunctionEnd

;======================================

Function DirectoryPreMP
  !insertmacro MP_GET_INSTALL_DIR $0
  ${IfNot} $0 == ""
    Abort
  ${EndIf}
FunctionEnd

Function DirectoryLeaveMP
  ; refresh MP subdirs, if it user has changed the path again
  ${ReadMediaPortalDirs} $MPdir.Base
FunctionEnd