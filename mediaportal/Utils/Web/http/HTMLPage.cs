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
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Web;
//using MediaPortal.Webepg.GUI.Library;

namespace MediaPortal.Utils.Web
{
  public class HTMLPage
  {
		HTTPTransaction Page = new HTTPTransaction();
		string _strPageHead = string.Empty;
    string _strPageSource = string.Empty;
		string defaultEncode = "iso-8859-1";
		string _Encoding = string.Empty;
		string _Error;

    public HTMLPage()
    {
    }

    public HTMLPage(string strURL)
    {
      LoadPage(strURL);
    }

		public HTMLPage(string strURL, string encoding)
		{
			_Encoding = encoding;
			LoadPage(strURL);
		}

		public string Encoding
		{
			get { return _Encoding;}
			set { _Encoding = value;}
		}

		public string GetError()
		{
			return _Error;
		}

		public bool LoadPage(string strURL)
		{
			if(HTMLCache.Caching)
			{
				if(HTMLCache.LoadPage(strURL))
				{
					_strPageSource = HTMLCache.GetPage();
					return true;
				}
			}

			Encoding encode;
			string strEncode = defaultEncode;

			if(Page.HTTPGet(strURL))
			{
				byte[] pageData = Page.GetData();
				int i;

				if(_Encoding != "")
				{
					strEncode = _Encoding;
				}
				else
				{
					encode = System.Text.Encoding.GetEncoding(defaultEncode);
					_strPageSource = encode.GetString(pageData);
                    int headEnd;
                    if ((headEnd = _strPageSource.ToLower().IndexOf("</head")) != -1)
                    {
                        if ((i = _strPageSource.ToLower().IndexOf("charset", 0, headEnd)) != -1)
                        {
                            strEncode = "";
                            i += 8;
                            for (; i < _strPageSource.Length && _strPageSource[i] != '\"'; i++)
                                strEncode += _strPageSource[i];
                            _Encoding = strEncode;
                        }

                        if (strEncode == "")
                            strEncode = defaultEncode;
                    }
				}

				// Encoding: depends on selected page
				if(_strPageSource == "" || strEncode != defaultEncode)
				{
                    try
                    { 
                        encode = System.Text.Encoding.GetEncoding(strEncode);
                        _strPageSource = encode.GetString(pageData);
                    }
                    catch(System.ArgumentException)
                    {
                    }
				}

				if(HTMLCache.Caching)
					HTMLCache.SavePage(strURL, _strPageSource);

				return true;
			}
			_Error = Page.GetError();
			return false;
        }

        public string GetPage()
        {
            return _strPageSource;
        }

        public string GetBody()
        {
            //return _strPageSource.Substring(_startIndex, _endIndex - _startIndex);
            //try
            //{
            //    XmlDocument xmlDoc = new XmlDocument();
            //    xmlDoc.LoadXml(_strPageSource);
            //    XmlNode bodyNode = xmlDoc.DocumentElement.SelectSingleNode("//body");
            //    return bodyNode.InnerText;
            //}
            //catch (System.Xml.XmlException ex)
            //{
            //    _Error = "XML Error finding Body"; 
            //}
            int startIndex = _strPageSource.ToLower().IndexOf("<body", 0);
            if (startIndex == -1)
            {
                // report Error
                _Error = "No body start found"; 
                return null;
            }

            int endIndex = _strPageSource.ToLower().IndexOf("</body", startIndex);

            if (endIndex == -1)
            {
                //report Error
                _Error = "No body end found";
                endIndex = _strPageSource.Length;
            }

            return _strPageSource.Substring(startIndex, endIndex - startIndex);
            
        }
    }
}
