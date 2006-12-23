#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace DreamBox
{
    public class Request
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";

        public Request(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;
        }

        public Stream GetStream(string command)
        {
            Uri uri = new Uri(_Url + command);
            WebRequest request = WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(_UserName, _Password);

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            return stream;
        }

        public string PostData(string command)
        {
            Uri uri = new Uri(_Url + command);
            WebRequest request = WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(_UserName, _Password);

            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());

            string str = reader.ReadToEnd();
            return str;
        }
    }
}
