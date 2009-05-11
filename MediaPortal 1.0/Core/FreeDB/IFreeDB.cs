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

using System;

namespace MediaPortal.Freedb
{
	/// <summary>
	/// Summary description for IFreeDB.
	/// </summary>
	public interface IFreeDB
	{
		bool connect();
    bool connect(FreeDBSite site);
    bool disconnect();
    //string getDiscID(string tracks, string[] offsets, string time);
    string getServerMessage();
    string[] getListOfGenres();
    string[] getHelp(string topic);
    string[] getLog();
    string[] getStatus();
    string[] getUsers();
    string[] getVersion();
    string[] getMessageOfTheDay();
    bool update();
    CDInfo[] getCDInfo();  // possible ones
    CDInfoDetail getCDInfoDetail(CDInfo info);
    bool sendCDInfoDetail(CDInfoDetail info);  // write it to the FreeDB db...
	}
}
