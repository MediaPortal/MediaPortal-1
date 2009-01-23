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

#**********************************************************************************************************#
#
# POST INSTALLATION UPDATE FILE
#
#       add here all changes, needed after applying an 1.0.1 update
#
#**********************************************************************************************************#

# these are only examples
#Delete "$SMPROGRAMS\$StartMenuGroup\MediaPortal Log-Files.lnk"
#RMDir "$SMPROGRAMS\$StartMenuGroup"


# unreg the old mpi extension, is replaced by mpe1
${UnRegisterExtension} "$MPdir.Base\MPInstaller.exe" ".mpi" "MediaPortal extension package"
${RefreshShellIcons}
