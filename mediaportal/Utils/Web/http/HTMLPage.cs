/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.IO;
using System.Net;
using System.Web;
//using MediaPortal.Webepg.GUI.Library;

namespace MediaPortal.Utils.Web
{
    public class HTMLPage
    {
		HTTPTransaction Page;
		string m_strPageHead = string.Empty;
        string m_strPageSource = string.Empty;
		string defaultEncode = "iso8859-1";
		string m_Encoding;
		string m_Error;

        public HTMLPage()
        {
			Page = new HTTPTransaction();
        }

        public HTMLPage(string strURL)
        {
            LoadPage(strURL);
        }

		public HTMLPage(string strURL, string encoding)
		{
			m_Encoding = encoding;
			LoadPage(strURL);
		}

		public string Encoding
		{
			get { return m_Encoding;}
			set { m_Encoding = value;}
		}

		public string GetError()
		{
			return m_Error;
		}

		public bool LoadPage(string strURL)
		{
			if(HTMLCache.Caching)
			{
				if(HTMLCache.LoadPage(strURL))
				{
					m_strPageSource = HTMLCache.GetPage();
					return true;
				}
			}

			Encoding encode;
			string strEncode = defaultEncode;

			if(Page.HTTPGet(strURL))
			{
				byte[] pageData = Page.GetData();
				int i;

				if(m_Encoding != null)
				{
					strEncode = m_Encoding;
				}
				else
				{
					encode = System.Text.Encoding.GetEncoding(defaultEncode);
					m_strPageSource = encode.GetString(pageData);
					if((i=m_strPageSource.IndexOf("charset"))!= -1)
					{
						strEncode = "";
						i+=8;
						for(;i<m_strPageSource.Length && m_strPageSource[i]!='\"';i++)
							strEncode+=m_strPageSource[i];
						m_Encoding = strEncode;
					}
				}

				// Encoding: depends on selected page
				if(m_strPageSource == "" || strEncode != defaultEncode)
				{
					encode = System.Text.Encoding.GetEncoding(strEncode);
					m_strPageSource = encode.GetString(pageData);
				}

				if(HTMLCache.Caching)
					HTMLCache.SavePage(strURL, m_strPageSource);

				return true;
			}
			m_Error = Page.GetError();
			return false;
        }

        public string GetPage()
        {
            return m_strPageSource;
        }

//        public string GetBody()
//        {
//            //return m_strPageSource.Substring(m_startIndex, m_endIndex - m_startIndex);
//        }
    }
}
