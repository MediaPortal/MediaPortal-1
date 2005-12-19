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
using System.IO;
using System.Net;
using System.Web;
using System.Collections;

namespace MediaPortal.Utils.Web
{
    public class HTTPTransaction
    {
		string m_Agent = "Mozilla/4.0 (compatible; MSIE 6.0;  WindowsNT 5.0; .NET CLR 1 .1.4322)";
		CookieCollection m_Cookies;
		HttpWebResponse m_Response;
		string m_Error = string.Empty;
		int blockSize = 8196;
		byte[] m_Data;

		public HTTPTransaction()
		{
		}

		public HTTPTransaction(string strURL)
		{
			Transaction(strURL);	// defaults to HTTP Get
		}

		public bool HTTPGet(string strURL)
		{
			return Transaction(strURL);
		}

		public void SetAgent(string newAgent)
		{
			m_Agent = newAgent;
		}

		public string GetError()
		{
			return m_Error;
		}

		public CookieCollection Cookies
		{
			get { return m_Cookies;}
			set { m_Cookies=value;}
		}

		public byte[] GetData() //string strURL, string strEncode)
		{
			return m_Data;
		}

        private bool Transaction(string strURL)
        {
			ArrayList Blocks = new ArrayList();
			byte[] Block;
			byte[] readBlock;
			int size;
			int totalSize;

            try
            {
                // Make the Webrequest
                Uri Page = new Uri(strURL);
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(Page);
				request.UserAgent = m_Agent;
                request.Credentials = HTTPAuth.Get(Page.Host);
				request.CookieContainer = new CookieContainer();
				if(m_Cookies != null)
					request.CookieContainer.Add( m_Cookies );
					
                m_Response = (HttpWebResponse) request.GetResponse();
				m_Response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
				m_Cookies = m_Response.Cookies;

                Stream ReceiveStream = m_Response.GetResponseStream();

				Block = new byte[blockSize];
				totalSize = 0;

				while( (size = ReceiveStream.Read(Block, 0, blockSize) ) > 0)
				{
					readBlock = new byte[size];
					Array.Copy(Block, readBlock, size);
					Blocks.Add( readBlock );
					totalSize += size;
				}

				ReceiveStream.Close();
				m_Response.Close();

				int pos=0;
				m_Data = new byte[ totalSize ];
				
				for(int i = 0; i< Blocks.Count; i++)
				{
					Block = (byte[]) Blocks[i];
					Block.CopyTo(m_Data, pos);
					pos += Block.Length;
				}

            }
            catch (WebException ex)
            {
                m_Error = ex.Message;
			 	return false;
            }
			return true;
        }
    }
}
