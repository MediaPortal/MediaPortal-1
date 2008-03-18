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

#**********************************************************************************************************#
#
# For the MediaPortal Installer to work you need:
# 1. Lastest NSIS version from http://nsis.sourceforge.net/Download
#
# Editing is much more easier, if you install HM NSIS Edit from http://hmne.sourceforge.net
#
#**********************************************************************************************************#
Name "MediaPortal"
SetCompressor zlib
#SetCompressor /SOLID lzma  ; disabled solid, because of performance reasons

!define APP_NAME "MediaPortal 0.2.3.0"
RequestExecutionLevel admin

#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define PRODUCT_NAME "MediaPortal SVN Snapshot"
!define PRODUCT_VERSION "0.2.3.0 final"
!define PRODUCT_PUBLISHER "high"
!define PRODUCT_WEB_SITE "http://www.team-mediaportal.com"
!define MUI_WELCOMEPAGE_TITLE_3LINES
!define MUI_FINISHPAGE_TITLE_3LINES
!define COMPANY "Team MediaPortal"
!define URL     "www.team-mediaportal.com"

!define REG_UNINSTALL         "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"

!define VER_MAJOR       0
!define VER_MINOR       9
!define VER_REVISION    2
!ifndef VER_BUILD
    !define VER_BUILD   0
!endif
!if ${VER_BUILD} == 0       # it's a stable release
    !define VERSION "1.0 RC2 internal"
!else                       # it's an svn reöease
    !define VERSION "pre-release build ${VER_BUILD}"
!endif
BrandingText "MediaPortal ${VERSION} by Team MediaPortal"

!define PATH_TO_SNAPSHOT_ZIP "E:\compile\pub_mp1\final\snapshot.zip"

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include "MUI2.nsh"
!include "c:\Programme\NSIS\Plugins\ZipDLL.nsh"
!include "dialogs.nsh"

#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ABORTWARNING
!define MUI_ICON    "images\install.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP          "images\header.bmp"
!define MUI_HEADERIMAGE_RIGHT
!define MUI_WELCOMEFINISHPAGE_BITMAP    "images\wizard-svn.bmp"

!define MUI_LANGDLL_REGISTRY_ROOT "${PRODUCT_UNINST_ROOT_KEY}"
!define MUI_LANGDLL_REGISTRY_KEY "${PRODUCT_UNINST_KEY}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "NSIS:Language"

!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN      "$INSTDIR\Configuration.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run Configuration.exe to (re-)configure MediaPortal"



!define MUI_WELCOMEPAGE_TITLE "Welcome to the MediaPortal SVN-snapshot installer!!"
!define MUI_WELCOMEPAGE_TEXT "This wizard will guide you through the installation of $(^NameDA). If you'd like the bleeding edge code and don't mind the inherent risks, upgrade to the snapshot releases as they become available. If stability is important to you, you might want to stay with the fully-tested releases. $_CLICK"
!define MUI_CUSTOMFUNCTION_ABORT OnUserAbort
!define MUI_LICENSEPAGE_TEXT_TOP "Please read before you install a SVN-Snapshot!"
!define MUI_LICENSEPAGE_TEXT_BOTTOM ""
!define MUI_FINISHPAGE_TITLE "MediaPortal SVN-snapshot successfully installed!"
!define MUI_LICENSEPAGE_BUTTON "continue"

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "gpl2.txt"
#!insertmacro MUI_PAGE_LICENSE "..\Docs\MediaPortal License.rtf"
#!insertmacro MUI_PAGE_LICENSE "..\Docs\BASS License.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
Page instfiles
#!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

#---------------------------------------------------------------------------
# INSTALLER LANGUAGES
#---------------------------------------------------------------------------
!insertmacro MUI_LANGUAGE "English"

#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
#!if ${VER_BUILD} == 0
#    OutFile "Release\setup-mediaportal-svn-.exe"
#!else
#    OutFile "Release\setup-mediaportal-svn-${VER_BUILD}.exe"
#!endif
OutFile "MediaPortal-svn-.exe"

#InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal"
#InstallDirRegKey HKLM "${REG_UNINSTALL}" InstallPath
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal"
InstallDirRegKey HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
CRCCheck on
XPStyle on
#ShowInstDetails show
ShowInstDetails nevershow
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName       "${NAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductVersion    "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyName       "${COMPANY}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyWebsite    "${URL}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileVersion       "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription   ""
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright    ""

#---------------------------------------------------------------------------
# SECTIONS
#---------------------------------------------------------------------------
Section /o "Backup MediaPortal" Section1
DetailPrint "Waiting for backup-folder to proceed"
Call func
# See if the user selects a folder:
  ${if} $R0 == "${NULL}"
  DetailPrint "Operation was canceled!"
  MessageBox MB_OK "Backup aborted. The installer will now continue."
  Goto next
  ${else}
  DetailPrint "You choose: $R0"
  ${endif}
  CreateDirectory "$R0"
  DetailPrint "Creating backup, this might take some minutes."
  
# check if Vista is installed and config-backup is needed  
  ClearErrors
  ReadRegSTR $5 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" "CurrentVersion"
  StrCmp $5 '6.0' lbl_vista_config lbl_vista_error

  lbl_vista_config:
  ClearErrors
  ReadRegSTR $2 HKEY_LOCAL_MACHINE "SOFTWARE\Team MediaPortal\MediaPortal" "ConfigDir"
  CreateDirectory "$R0\vista_config"
  CopyFiles /SILENT "$2\*.*" "$R0\vista_config"
  Goto lbl_vista_error
  
  lbl_vista_error:  
  Rename $INSTDIR\start.bat $INSTDIR\restore.bat
  CopyFiles /SILENT "$INSTDIR\*.*" "$R0"
  MessageBox MB_OK "Done! To restore exchange your backup folder with your install directory AND run restore.bat!"
  Goto next
  
next:
IfFileExists $INSTDIR\MediaPortal.exe +3 0
  MessageBox MB_OK  "No MediaPortal installation found in $INSTDIR. The updater will now exit."
  QUIT
SectionEnd

Section "Install Snapshot" Section2
  SetOutPath "$INSTDIR\"
  File /r "${PATH_TO_SNAPSHOT_ZIP}"
  ClearErrors
  CreateDirectory "$INSTDIR\temp\"
  ZipDLL::extractall "$INSTDIR\snapshot.zip" "$INSTDIR\temp"
  SetOutPath "$INSTDIR\temp\"
  
  ReadRegSTR $4 HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ConfigDir"
  Rename $INSTDIR\temp\CaptureCardDefinitions.xml $INSTDIR\temp\CaptureCardDefinitions.old
#  Rename $INSTDIR\temp\MediaPortalDirs.xml $INSTDIR\temp\MediaPortalDirs.old
  CopyFiles /SILENT "$INSTDIR\temp\*.xml" "$4\"
  CopyFiles /SILENT "$INSTDIR\temp\Thumbs" "$4\"
  CopyFiles /SILENT "$INSTDIR\temp\XMLTV" "$4\"
  Rename $INSTDIR\temp\CaptureCardDefinitions.old $INSTDIR\temp\CaptureCardDefinitions.xml
#  Rename $INSTDIR\temp\MediaPortalDirs.old $INSTDIR\temp\MediaPortalDirs.xml 
  ClearErrors
  
  CopyFiles /SILENT "$INSTDIR\temp\*.*" "$INSTDIR"
  SetOutPath "$INSTDIR\"
   IfErrors 0 +6
  MessageBox MB_OK  "The Updater could not copy all files. Be sure that none of them is in use and TV-Service is stopped (only when TV-EngineV3 is used).  The updater will now exit."
  Delete $INSTDIR\snapshot.zip
  RMDir /r $INSTDIR\temp
  QUIT
  
  ClearErrors
  ReadRegSTR $5 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" "CurrentVersion"
  StrCmp $5 '6.0' lbl_winnt_vista lbl_error

  lbl_winnt_vista:
  ClearErrors
  ReadRegDWORD $0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "Dscaler"
  IfErrors InstallMSDE1 NoInstallMSDE1
  
  InstallMSDE1:
  execwait "$INSTDIR\start4.bat"
  Delete $INSTDIR\start.bat
  Rename $INSTDIR\start4.bat $INSTDIR\start.bat
  ClearErrors
  IfErrors fertig fertig
  
  NOInstallMSDE1:
  execwait "$INSTDIR\start3.bat"
  Delete $INSTDIR\start.bat
  Rename $INSTDIR\start3.bat $INSTDIR\start.bat
  ClearErrors
  IfErrors fertig fertig
  
  
  lbl_error:  
  ClearErrors
  ReadRegDWORD $0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "Dscaler"
  IfErrors InstallMSDE NoInstallMSDE
  
  InstallMSDE:
  execwait "$INSTDIR\start2.bat"
  Delete $INSTDIR\start.bat
  Rename $INSTDIR\start2.bat $INSTDIR\start.bat
  ClearErrors
  IfErrors fertig fertig
  
  NOInstallMSDE:
  execwait "$INSTDIR\start.bat"
  Delete $INSTDIR\start2.bat
  ClearErrors
  IfErrors fertig fertig
  
  
  fertig:
  Delete $INSTDIR\snapshot.zip
  RMDir /r $INSTDIR\temp
  IfFileExists $INSTDIR\restore.bat +3 0
  CopyFiles /SILENT "$INSTDIR\start.bat" "$R0"
  Rename $R0\start.bat $R0\restore.bat
  Delete $INSTDIR\start.bat
  Delete $INSTDIR\start2.bat
  Delete $R0\start.bat
  Delete $R0\start2.bat
  SectionEnd

Section -AdditionalIcons
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "$SMPROGRAMS\MediaPortal\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
SectionEnd

Section -Post
SectionEnd

LangString DESC_Section1 ${LANG_ENGLISH} "Creates a backup of your former mediaPortal installation"
LangString DESC_Section2 ${LANG_ENGLISH} "Installs the latest MediaPortal Snapshot"

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${Section1} $(DESC_Section1)
!insertmacro MUI_DESCRIPTION_TEXT ${Section2} $(DESC_Section2)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

Function func
# Params:
# 1) Title: "Looking for something"
# 2) Caption: none
# 3) InitDir: "$EXEDIR"
# 4) Return: $R0
${ModernFolderBox} "Please specify a backup-folder!" "" "$EXEDIR" ${VAR_R0}
FunctionEnd

Function OnUserAbort
	;If user press Cancel button when count not finish.
	;Case installer unload the plugins after this command
	Delay::Free
FunctionEnd