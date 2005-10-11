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
using MediaPortal.Webepg.GUI.Library;

namespace MediaPortal.Util
{
    public class HTMLPage
    {
		string m_strPageHead = string.Empty;
        string m_strPageSource = string.Empty;
		string defaultEncode="iso8859-1";
        int m_startIndex;
        int m_endIndex;

        public HTMLPage()
        {
        }

		public HTMLPage(string strURL)
		{
			LoadPage(strURL, true, "");
		}

        public HTMLPage(string strURL, bool isHTML)
        {
            LoadPage(strURL, isHTML, "");
        }

		public HTMLPage(string strURL, bool isHTML, string encoding)
		{
			LoadPage(strURL, isHTML, encoding);
		}

        private void LoadPage(string strURL, bool isHTML, string encoding)
        {
			Encoding encode;
			string strEncode = defaultEncode;
			byte[] buffer;
			byte[] Block;
			byte[] LastBlock = new byte[]{};
			int size;
			int i;

            try
            {
                // Make the Webrequest
                HttpWebRequest req = (HttpWebRequest) WebRequest.Create(strURL);
				req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0;  WindowsNT 5.0; .NET CLR 1 .1.4322)";
                WebResponse result = req.GetResponse();
                Stream ReceiveStream = result.GetResponseStream();

				Block = new byte[32000];

				while((size = ReceiveStream.Read(Block,0,32000)) > 0)
				{
					buffer = new byte[LastBlock.Length+size];
					i=0;
					foreach(byte b in LastBlock)
						buffer[i++]=b;
					for(int b=0; b < size; b++)
						buffer[i++]=Block[b];
					LastBlock=buffer;
				}
			
				ReceiveStream.Close();
				result.Close();

				buffer = new byte[LastBlock.Length+size];
				i=0;
				foreach(byte b in LastBlock)
					buffer[i++]=b;
				for(int b=0; b < size; b++)
					buffer[i++]=Block[b];

				if(encoding != "")
				{
					strEncode = encoding;
					Log.WriteFile(Log.LogType.Log, false, "WebPage encoding forced: {0}", strEncode);
				}
				else
				{
					if(isHTML)
					{
						encode = System.Text.Encoding.GetEncoding(defaultEncode);
						m_strPageSource = encode.GetString(buffer);
						if((i=m_strPageSource.IndexOf("charset"))!= -1)
						{
							strEncode = "";
							i+=8;
							for(;i<m_strPageSource.Length && m_strPageSource[i]!='\"';i++)
								strEncode+=m_strPageSource[i];
							Log.WriteFile(Log.LogType.Log, false, "WebPage encoding: {0}", strEncode);
						}
					}
				}

                // Encoding: depends on selected page
				if(m_strPageSource == "" || strEncode != defaultEncode)
				{
					encode = System.Text.Encoding.GetEncoding(strEncode); //strEncode);
					//sr = new StreamReader(ReceiveStream, encode);
					m_strPageSource = encode.GetString(buffer);
				}
				m_startIndex=0;
				m_endIndex=m_strPageSource.Length;
            }
            catch (WebException ex)
            {
                Log.WriteFile(Log.LogType.Log, true, "Error retreiving WebPage: {0} Encoding:{1} err:{2} stack:{3}", strURL, strEncode, ex.Message, ex.StackTrace);
            }
        }

        public string GetPage() //string strURL, string strEncode)
        {
            return m_strPageSource;
        }

		public bool SetStart(string strStart)
		{
			int startIndex = m_strPageSource.IndexOf(strStart, 0);

			if(startIndex != -1)
			{
				m_startIndex = startIndex;
				return true;
			}
			return false;
		}

		public bool SetEnd(string strEnd)
		{
			int endIndex = m_strPageSource.IndexOf(strEnd, 0);

			if(endIndex != -1)
			{
				m_endIndex = endIndex;
				return true;
			}
			return false;
		}

        public string SubPage()
        {
            return m_strPageSource.Substring(m_startIndex, m_endIndex - m_startIndex);
        }
    }
}
