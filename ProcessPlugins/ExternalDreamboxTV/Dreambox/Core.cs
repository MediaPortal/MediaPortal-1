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



namespace DreamBox
{
    public class Core
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";

        public DreamBox.Data Data = null;
        public DreamBox.Remote Remote = null;
        private DreamBox.XML _XML = null;

        public Core(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;

            this.Data = new Data(url, username, password);
            this.Remote = new Remote(url, username, password);
            this._XML = new XML(url, username, password);
        }




        public XML XML
        {
            get
            {
                return _XML;
            }
        }

        public string Url
        {
            get { return _Url; }
            set { _Url = value; }
        }

        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }

        public string UserName
        {
            get { return _UserName; }
            set { _UserName = value; }
        }


        private string _SelectedBouquet;
        public string SelectedBouquet
        {
            get { return _SelectedBouquet; }
            set { _SelectedBouquet = value; }
        }
        private string _SelectedChannel;

        public string SelectedChannel
        {
            get { return _SelectedChannel; }
            set { _SelectedChannel = value; }
        }


        public ChannelInfo CurrentChannel
        {
            get
            {
                return new ChannelInfo(_Url, _UserName, _Password);
            }
        }
        public Remote RemoteControl
        {
            get
            {
                return new Remote(_Url, _UserName, _Password);
            }
        }

        public void Reboot()
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData("/cgi-bin/admin?command=reboot");
        }




    }
}
