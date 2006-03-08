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
using System.Collections;
using System.Net;

namespace MediaPortal.Utils.Web
{
	public class HTTPAuth
	{
		static Hashtable _AuthList;

        static HTTPAuth()
        {
        }

        public static NetworkCredential Get(string site)
        {
            NetworkCredential login = null;

            if (_AuthList != null)
            {
                login = (NetworkCredential)_AuthList[site];
            }

            return login;
        }

        public static void Add(string site, NetworkCredential login)
        {
            if (_AuthList == null)
                _AuthList = new Hashtable();

            _AuthList.Add(site, login);
        }
    }
}
