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

# ENGLISH
LangString DESC_SecDscaler                        ${LANG_ENGLISH} "Installs the DScaler Decoder"
LangString DESC_SecGabest                         ${LANG_ENGLISH} "Installs the Gabest MPA/MPV decoder"
LangString ^UninstallLink                         ${LANG_ENGLISH} "Uninstall $(^Name)"

LangString TEXT_MSGBOX_REMOVE_ALL                 ${LANG_ENGLISH} "!!! ATTENTION !!!$\r$\nDo you want to make a complete cleanup?$\r$\nThis removes completly the registry keys, the installation and the common app data directory, inclusive thumbs, databases, skins and plugins!"
LangString TEXT_MSGBOX_REMOVE_ALL_STUPID          ${LANG_ENGLISH} "!!! ATTENTION !!!$\r$\nAgain for those who slept the msgBox before. :( $(TEXT_MSGBOX_REMOVE_ALL)"

LangString TEXT_MSGBOX_ERROR_DOTNET               ${LANG_ENGLISH} "Microsoft .Net Framework Runtime is a prerequisite. Please install first."
LangString TEXT_MSGBOX_ERROR_WIN                  ${LANG_ENGLISH} "MediaPortal requires at least Windows XP. Your Windows is not supported. Installation aborted"
LangString TEXT_MSGBOX_ERROR_IS_INSTALLED         ${LANG_ENGLISH} "MediaPortal is already installed. You need to uninstall it, before you continue with the installation.$\r$\nUninstall will be lunched when pressing OK."
LangString TEXT_MSGBOX_ERROR_ON_UNINSTALL         ${LANG_ENGLISH} "An error occured while trying to uninstall old version!$\r$\nDo you still want to continue the installation?"
LangString TEXT_MSGBOX_ERROR_REBOOT_REQUIRED      ${LANG_ENGLISH} "A reboot is required after a previous action. Reboot you system and try it again."
