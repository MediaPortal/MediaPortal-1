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
		string _Agent = "Mozilla/4.0 (compatible; MSIE 6.0;  WindowsNT 5.0; .NET CLR 1 .1.4322)";
		CookieCollection _Cookies;
		HttpWebResponse _Response;
		string _Error = string.Empty;
		int blockSize = 8196;
		byte[] _Data;

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
			_Agent = newAgent;
		}

		public string GetError()
		{
			return _Error;
		}

		public CookieCollection Cookies
		{
			get { return _Cookies;}
			set { _Cookies=value;}
		}

		public byte[] GetData() //string strURL, string strEncode)
		{
			return _Data;
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
				request.UserAgent = _Agent;
                request.Credentials = HTTPAuth.Get(Page.Host);
				request.CookieContainer = new CookieContainer();
				if(_Cookies != null)
					request.CookieContainer.Add( _Cookies );
					
                _Response = (HttpWebResponse) request.GetResponse();
				_Response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
				_Cookies = _Response.Cookies;

                Stream ReceiveStream = _Response.GetResponseStream();

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
				_Response.Close();

				int pos=0;
				_Data = new byte[ totalSize ];
				
				for(int i = 0; i< Blocks.Count; i++)
				{
					Block = (byte[]) Blocks[i];
					Block.CopyTo(_Data, pos);
					pos += Block.Length;
				}

            }
            catch (WebException ex)
            {
                _Error = ex.Message;
			 	return false;
            }
			return true;
        }
    }
}
