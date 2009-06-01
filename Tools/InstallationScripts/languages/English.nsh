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

/*
_____________________________________________________________________________

     This header contains language strings for the NSIS setup routine.
_____________________________________________________________________________
*/
!define LANG "ENGLISH" ; Must be the lang name define my NSIS


!insertmacro LANG_STRING ^UninstallLink             "Uninstall $(^Name)"


# Descriptions for components (sections)
!insertmacro LANG_STRING DESC_SecGabest             "Installs the MPC-HC audio/video decoders"

!insertmacro LANG_STRING DESC_SecServer             "Installs the MediaPortal TV Server"
!insertmacro LANG_STRING DESC_SecClient             "Installs the MediaPortal TV Client plugin"

!insertmacro LANG_STRING TEXT_MP_NOT_INSTALLED        "MediaPortal not installed"
!insertmacro LANG_STRING TEXT_TVSERVER_NOT_INSTALLED  "TVServer not installed"


# Texts for message boxes
!if "${NAME}" == "MediaPortal"
  !insertmacro LANG_STRING TEXT_MSGBOX_REMOVE_ALL             "!!! ATTENTION !!!$\r$\nDo you want to make a complete cleanup?$\r$\nThis removes completly the registry keys, the installation and the common app data directory, inclusive thumbs, databases, skins and plugins!"
!else
  !if "${NAME}" == "MediaPortal TV Server / Client"
    !insertmacro LANG_STRING TEXT_MSGBOX_REMOVE_ALL           "!!! ATTENTION !!!$\r$\nDo you want to make a complete cleanup?$\r$\nThis removes completly the registry keys, the installation and the common app data directory AND THE ---TVDATABASE---!!!"
  !endif
!endif
!insertmacro LANG_STRING TEXT_MSGBOX_REMOVE_ALL_STUPID      "!!! ATTENTION !!!$\r$\nAgain for those who slept the msgBox before. :($\r$\n$\r$\n$(TEXT_MSGBOX_REMOVE_ALL)"

!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_SVN_NOMP         "MediaPortal is not installed. You need to install this before you can install an svn snapshot."
!insertmacro LANG_STRING TEXT_MSGBOX_PARAMETER_ERROR        "You have done something wrong!$\r$\nIt is not allowed to use 'noClient' & 'noServer' at the same time."

!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_MP022            "Old MSI-based MediaPortal 0.2.2.0 is still installed. Why didn't you follow the instructions and didn't remove it first? Do that and restart this setup."
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_MP023RC3         "Old MediaPortal 0.2.3.0 RC3 is still installed. Why didn't you follow the instructions and didn't remove it first? Do that and restart this setup."
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_MP023            "Old MediaPortal 0.2.3.0 is still installed. Why didn't you follow the instructions and didn't remove it first? Do that and restart this setup."

!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_MSI_CLIENT       "Old MSI-based TV Client plugin is still installed. Why didn't you follow the instructions and didn't remove it first? Do that and restart this setup."
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_MSI_SERVER       "Old MSI-based TV Server is still installed. Why didn't you follow the instructions and didn't remove it first? Do that and restart this setup."


# Texts for requirement checks
!insertmacro LANG_STRING TEXT_MSGBOX_INSTALLATION_CANCELD         "Installation will be canceled."
!insertmacro LANG_STRING TEXT_MSGBOX_MORE_INFO                    "Do you want to get more information about it?"

!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_WIN                    "Your operating system is not supported by $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED    "Your operating system is not recommended by $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_ADMIN                  "You need administration rights to install $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_VCREDIST_2005          "Microsoft Visual C++ 2005 SP1 Redistributable Package (x86) is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_VCREDIST_2008          "Microsoft Visual C++ 2008 SP1 Redistributable Package (x86) is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_DOTNET20               "Microsoft .Net Framework 2.0 SP2 is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_DOTNET20_SP            "Microsoft .Net Framework 2.0 is installed.$\r$\nBut Service Pack 2 is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_DOTNET35               "Microsoft .Net Framework 3.5 SP1 is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_DOTNET35_SP            "Microsoft .Net Framework 3.5 is installed.$\r$\nBut Service Pack 1 for .Net 3.5 is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"

!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_IS_INSTALLED               "$(^Name) is already installed. You need to uninstall it, before you continue with the installation.$\r$\nUninstall will be lunched when pressing OK."
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_ON_UNINSTALL               "An error occured while trying to uninstall old version!$\r$\nDo you still want to continue the installation?"
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_REBOOT_REQUIRED            "A reboot is required after a previous action. Reboot you system and try it again."
!insertmacro LANG_STRING TEXT_MSGBOX_ERROR_UPDATE_BUT_NOT_INSTALLED   "$(^Name) is not installed. It is not possible to install this update.$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)"


!insertmacro LANG_STRING UPDATE_ERROR_WRONGEXE                    "updating $(^Name) is only allowed by starting MediaPortalUpdater!"
!insertmacro LANG_STRING UPDATE_ERROR_UNKNOWN                     "strange / unknown error, please use full installer"
!insertmacro LANG_STRING UPDATE_ERROR_NOTHING_INSTALLED           "Nothing to do, nothing installed, please use the full installer"
!insertmacro LANG_STRING UPDATE_ERROR_VERSION_MP                  "wrong version of MediaPortal is installed or svn, please use the full installer"
!insertmacro LANG_STRING UPDATE_ERROR_VERSION_TVSERVER            "wrong version or TVServer or Client plugin is installed or svn, please use the full installer"



# Strings for AddRemove-Page
!insertmacro LANG_STRING TEXT_ADDREMOVE_HEADER          "Already Installed"
!insertmacro LANG_STRING TEXT_ADDREMOVE_HEADER2_REPAIR  "Choose the maintenance option to perform."
!insertmacro LANG_STRING TEXT_ADDREMOVE_HEADER2_UPDOWN  "Choose how you want to install $(^Name)."
!insertmacro LANG_STRING TEXT_ADDREMOVE_INFO_SELECT     "Select the operation you want to perform and click Next to continue."
!insertmacro LANG_STRING TEXT_ADDREMOVE_INFO_REPAIR     "$(^Name) ${VERSION} is already installed. $(TEXT_ADDREMOVE_INFO_SELECT)"
!insertmacro LANG_STRING TEXT_ADDREMOVE_INFO_UPGRADE    "An older version of $(^Name) is installed on your system. $(TEXT_ADDREMOVE_INFO_SELECT)"
!insertmacro LANG_STRING TEXT_ADDREMOVE_INFO_DOWNGRADE  "A newer version of $(^Name) is already installed! It is not recommended that you install an older version. $(TEXT_ADDREMOVE_INFO_SELECT)"
!insertmacro LANG_STRING TEXT_ADDREMOVE_REPAIR_OPT1     "Add/Remove/Reinstall components"
!insertmacro LANG_STRING TEXT_ADDREMOVE_REPAIR_OPT2     "Uninstall $(^Name)"
!insertmacro LANG_STRING TEXT_ADDREMOVE_UPDOWN_OPT1     "Upgrade $(^Name) using previous settings (recommended)"
!insertmacro LANG_STRING TEXT_ADDREMOVE_UPDOWN_OPT2     "Change settings (advanced)"


# Strings for UninstallMode-Page
!insertmacro LANG_STRING TEXT_UNMODE_HEADER          "Uninstallation Mode"
!insertmacro LANG_STRING TEXT_UNMODE_HEADER2          "Please choose the mode, you want to do the uninstallation."
!insertmacro LANG_STRING TEXT_UNMODE_OPT0          "Standard Uninstall (recommended)"
!insertmacro LANG_STRING TEXT_UNMODE_OPT1          "Complete Uninstallation for ${NAME}"
!insertmacro LANG_STRING TEXT_UNMODE_OPT2          "Full MediaPortal Products cleanup"
!insertmacro LANG_STRING TEXT_UNMODE_OPT0_DESC          "Only the main application will be uninstalled, userfiles and databases will not be deleted (recommended)"
!insertmacro LANG_STRING TEXT_UNMODE_OPT1_DESC          "This will uninstall ${NAME}, delete all userfiles and databases"
!insertmacro LANG_STRING TEXT_UNMODE_OPT2_DESC          "This will also remove all files, folders, databases, settings and registry keys which might be leftovers from older MediaPortal versions."
!insertmacro LANG_STRING TEXT_UNMODE_OPT1_MSGBOX          "Are you sure that you want to do a Complete Uninstallation? This can not be undone!"
!insertmacro LANG_STRING TEXT_UNMODE_OPT2_MSGBOX          "Are you sure that you want to do a Full MediaPortal Products cleanup? This can not be undone!"
