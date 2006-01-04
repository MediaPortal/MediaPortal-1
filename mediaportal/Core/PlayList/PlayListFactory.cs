/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.IO;

namespace MediaPortal.Playlists
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayListFactory
	{
		public PlayListFactory()
		{
		}
		static public PlayList Create(string fileName)
		{
			string extension=Path.GetExtension(fileName);
			extension.ToLower();
			if (extension==".m3u")
			{
				return new PlayListM3U();
			}
			if (extension==".pls")
			{
				return new PlayListPLS();
			}
			if (extension==".b4s")
			{
				return new PlayListB4S();
			}
			if (extension==".wpl")
			{
				return new PlayListWPL();
			}
			return null;
		}

		static public bool IsPlayList(string fileName)
		{
			string extension=Path.GetExtension(fileName);
			extension.ToLower();
			if (extension==".m3u") return true;
			if (extension==".pls") return true;
			if (extension==".b4s") return true;
			if (extension==".wpl") return true;
			return false;
		}
	}
}
