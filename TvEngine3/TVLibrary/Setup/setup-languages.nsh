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
# This header contains multilanguage strings for the setup routine
#
#**********************************************************************************************************#
!define TITLE_SECServer "MediaPortal TV Server"
!define TITLE_SECClient "MediaPortal TV Plugin/Client"

# ENGLISH
LangString DESC_SECServer                   ${LANG_ENGLISH} "Installs the ${TITLE_SECServer}"
LangString DESC_SECClient                   ${LANG_ENGLISH} "Installs the ${TITLE_SECClient}"
LangString ^UninstallLink                   ${LANG_ENGLISH} "Uninstall $(^Name)"

#!ifdef MUI_ADDREMOVEPAGE
    LangString TEXT_ADDREMOVE_HEADER            ${LANG_ENGLISH} "Already Installed"
    LangString TEXT_ADDREMOVE_HEADER2_REPAIR    ${LANG_ENGLISH} "Choose the maintenance option to perform."
    LangString TEXT_ADDREMOVE_HEADER2_UPDOWN    ${LANG_ENGLISH} "Choose how you want to install $(^Name)."
    LangString TEXT_ADDREMOVE_INFO_REPAIR       ${LANG_ENGLISH} "$(^Name) ${VERSION} is already installed. Select the operation you want to perform and click Next to continue."
    LangString TEXT_ADDREMOVE_INFO_UPGRADE      ${LANG_ENGLISH} "An older version of $(^Name) is installed on your system. It's recommended that you uninstall the current version before installing. Select the operation you want to perform and click Next to continue."
    LangString TEXT_ADDREMOVE_INFO_DOWNGRADE    ${LANG_ENGLISH} "A newer version of $(^Name) is already installed! It is not recommended that you install an older version. If you really want to install this older version, it's better to uninstall the current version first. Select the operation you want to perform and click Next to continue."
    LangString TEXT_ADDREMOVE_REPAIR_OPT1       ${LANG_ENGLISH} "Add/Remove/Reinstall components"
    LangString TEXT_ADDREMOVE_REPAIR_OPT2       ${LANG_ENGLISH} "Uninstall $(^Name)"
    LangString TEXT_ADDREMOVE_UPDOWN_OPT1       ${LANG_ENGLISH} "Uninstall before installing"
    LangString TEXT_ADDREMOVE_UPDOWN_OPT2       ${LANG_ENGLISH} "Do not uninstall"
#!endif

LangString TEXT_MP_NOT_INSTALLED            ${LANG_ENGLISH} "MediaPortal not installed"
LangString TEXT_MSGBOX_ERROR_ON_UNINSTALL   ${LANG_ENGLISH} "An error occured while trying to uninstall old version!$\r$\nDo you still want to continue the installation?"
LangString TEXT_MSGBOX_PARAMETER_ERROR      ${LANG_ENGLISH} "You have done something wrong!$\r$\nIt is not allowed to use 'noClient' & 'noServer' at the same time."
LangString TEXT_MSGBOX_COMPLETE_CLEANUP     ${LANG_ENGLISH} "Do you want to make a complete cleanup?$\r$\nRemove all settings, files and folders?"
