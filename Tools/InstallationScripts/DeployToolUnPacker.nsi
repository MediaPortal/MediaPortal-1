#region Copyright (C) 2005-2011 Team MediaPortal
/*
// Copyright (C) 2005-2011 Team MediaPortal
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

#**********************************************************************************************************#
#
#   For building the installer on your own you need:
#       1. Latest NSIS version from http://nsis.sourceforge.net/Download
#
#**********************************************************************************************************#

#---------------------------------------------------------------------------
# DEVELOPMENT ENVIRONMENT
#---------------------------------------------------------------------------
# SKRIPT_NAME is needed to diff between the install scripts in imported headers
!define SKRIPT_NAME "MediaPortal Unpacker"
# path definitions, all others are done in MediaPortalScriptInit
!define git_ROOT "..\.."
!define git_InstallScripts "${git_ROOT}\Tools\InstallationScripts"
# common script init
!include "${git_InstallScripts}\include\MediaPortalScriptInit.nsh"


#---------------------------------------------------------------------------
# UNPACKER script
#---------------------------------------------------------------------------
!define PRODUCT_NAME          "MediaPortal"
!define PRODUCT_PUBLISHER     "Team MediaPortal"
!define PRODUCT_WEB_SITE      "www.team-mediaportal.com"

; needs to be done before importing MediaPortalCurrentVersion, because there the VER_BUILD will be set, if not already.
!ifdef VER_BUILD ; means !build_release was used
  !undef VER_BUILD

  ;!system 'include-MP-PreBuild.bat'
  !system '"..\Script & Batch tools\DeployVersionGIT\DeployVersionGIT\bin\Release\DeployVersionGIT.exe" /GetVersion=version.template.txt /path=${GIT_ROOT}'

  !include "version.txt"
  !delfile "version.txt"
  !if ${VER_BUILD} == 0
    !warning "It seems there was an error, reading the git revision. 0 will be used."
  !endif
!endif

; import version from shared file
!include "${git_InstallScripts}\include\MediaPortalCurrentVersion.nsh"

#---------------------------------------------------------------------------
# BUILD sources
#---------------------------------------------------------------------------
; comment one of the following lines to disable the preBuild
!define BUILD_MediaPortal
!define BUILD_TVServer
!define BUILD_DeployTool
!define BUILD_Installer

!include "include-MP-PreBuild.nsh"


#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!define NO_INSTALL_LOG
!include "${git_InstallScripts}\include\LanguageMacros.nsh"
!include "${git_InstallScripts}\include\MediaPortalMacros.nsh"
!include "${git_InstallScripts}\include\DotNetSearch.nsh"


#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
Name          "${SKRIPT_NAME}"
BrandingText  "${PRODUCT_NAME} ${VERSION_DISP} by ${PRODUCT_PUBLISHER}"
Icon "${git_DeployTool}\Install.ico"
!define /date buildTIMESTAMP "%Y-%m-%d-%H-%M"
!if ${VER_BUILD} == 0
  OutFile "${git_OUT}\MediaPortalSetup_${VERSION}_${buildTIMESTAMP}.exe"
!else
  OutFile "${git_OUT}\MediaPortalSetup_${VERSION}_${buildTIMESTAMP}.exe"
!endif
InstallDir "$TEMP\MediaPortal Installation"

CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
AutoCloseWindow true
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey ProductName       "${PRODUCT_NAME}"
VIAddVersionKey ProductVersion    "${VERSION_DISP}"
VIAddVersionKey CompanyName       "${PRODUCT_PUBLISHER}"
VIAddVersionKey CompanyWebsite    "${PRODUCT_WEB_SITE}"
VIAddVersionKey FileVersion       "${VERSION}"
VIAddVersionKey FileDescription   "${PRODUCT_NAME} installation ${VERSION_DISP}"
VIAddVersionKey LegalCopyright    "Copyright © 2005-2013 ${PRODUCT_PUBLISHER}"

;if we want to make it fully silent we can uncomment this
;SilentInstall silent

;Page directory
Page instfiles

!insertmacro LANG_LOAD "English"

;sections for unpacking
Section
  IfFileExists "$INSTDIR\*.*" 0 +2
    RMDir /r "$INSTDIR"

  SetOutPath $INSTDIR
  File /r /x .git /x *.pdb /x *.vshost.exe "${git_DeployTool}\bin\Release\*"

  SetOutPath $INSTDIR\deploy
#code after build scripts are fixed
!if "$%COMPUTERNAME%" != "S15341228"
  File "${git_OUT}\package-mediaportal.exe"
  File "${git_OUT}\package-tvengine.exe"
!else

#code before build scripts are fixed
  File "${git_MP}\Setup\Release\package-mediaportal.exe"
  File "${git_TVServer}\Setup\Release\package-tvengine.exe"
#end of workaound code
!endif
 
  SetOutPath $INSTDIR\HelpContent\DeployToolGuide
  File /r /x .git "${git_DeployTool}\HelpContent\DeployToolGuide\*"

SectionEnd

Function CheckAndDownloadDotNet45
# Let's see if the user has the .NET Framework 4.5 installed on their system or not
# Remember: you need Vista SP2 or 7 SP1.  It is built in to Windows 8, and not needed
# In case you're wondering, running this code on Windows 8 will correctly return is_equal
# or is_greater (maybe Microsoft releases .NET 4.5 SP1 for example)

# Set up our Variables
Var /GLOBAL dotNET45IsThere
Var /GLOBAL dotNET_CMD_LINE
Var /GLOBAL EXIT_CODE

ReadRegDWORD $dotNET45IsThere HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
IntCmp $dotNET45IsThere 378389 is_equal is_less is_greater

is_equal:
    Goto done_compare_not_needed
is_greater:
    # Useful if, for example, Microsoft releases .NET 4.5 SP1
    # We want to be able to simply skip install since it's not
    # needed on this system
    Goto done_compare_not_needed
is_less:
    Goto done_compare_needed

done_compare_needed:
    #.NET Framework 4.5 install is *NEEDED*

    # Microsoft Download Center EXE:
    # Web Bootstrapper: http://go.microsoft.com/fwlink/?LinkId=225704
    # Full Download: http://go.microsoft.com/fwlink/?LinkId=225702

    # Setup looks for components\dotNET45Full.exe relative to the install EXE location
    # This allows the installer to be placed on a USB stick (for computers without internet connections)
    # If the .NET Framework 4.5 installer is *NOT* found, Setup will connect to Microsoft's website
    # and download it for you

    # Reboot Required with these Exit Codes:
    # 1641 or 3010

    # Command Line Switches:
    # /showrmui /passive /norestart

    # Silent Command Line Switches:
    # /q /norestart


    # Let's see if the user is doing a Silent install or not
    IfSilent is_quiet is_not_quiet

    is_quiet:
        StrCpy $dotNET_CMD_LINE "/q /norestart"
        Goto LookForLocalFile
    is_not_quiet:
        StrCpy $dotNET_CMD_LINE "/showrmui /passive /norestart"
        Goto LookForLocalFile

    LookForLocalFile:
        # Let's see if the user stored the Full Installer
        IfFileExists "$EXEPATH\components\dotNET45Full.exe" do_local_install do_network_install

        do_local_install:
            # .NET Framework found on the local disk.  Use this copy

            ExecWait '"$EXEPATH\components\dotNET45Full.exe" $dotNET_CMD_LINE' $EXIT_CODE
            Goto is_reboot_requested

        # Now, let's Download the .NET
        do_network_install:

            Var /GLOBAL dotNetDidDownload
            NSISdl::download "http://go.microsoft.com/fwlink/?LinkId=225704" "$TEMP\dotNET45Web.exe" $dotNetDidDownload

            StrCmp $dotNetDidDownload success fail
            success:
                ExecWait '"$TEMP\dotNET45Web.exe" $dotNET_CMD_LINE' $EXIT_CODE
                Goto is_reboot_requested

            fail:
                MessageBox MB_OK|MB_ICONEXCLAMATION "Unable to download .NET Framework.  ${PRODUCT_NAME} will be installed, but will not function without the Framework!"
                Goto done_dotNET_function

            # $EXIT_CODE contains the return codes.  1641 and 3010 means a Reboot has been requested
            is_reboot_requested:
                ${If} $EXIT_CODE = 1641
                ${OrIf} $EXIT_CODE = 3010
                    SetRebootFlag true
                ${EndIf}

done_compare_not_needed:
    # Done dotNET Install
    Goto done_dotNET_function

#exit the function
done_dotNET_function:

FunctionEnd

Function .onInit
  !insertmacro MediaPortalNetFrameworkCheck
  !insertmacro MediaPortalNet4FrameworkCheck
  # Code disable for NET4.5 checking
  ;call CheckAndDownloadDotNet45
FunctionEnd

Function .onInstSuccess
  Exec "$INSTDIR\MediaPortal.DeployTool.exe"
FunctionEnd