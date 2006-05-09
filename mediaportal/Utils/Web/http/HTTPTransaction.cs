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
    string _agent = "Mozilla/4.0 (compatible; MSIE 6.0;  WindowsNT 5.0; .NET CLR 1 .1.4322)";
    string _postType = "application/x-www-form-urlencoded";
    CookieCollection _cookies;
    HttpWebResponse _response;
    string _error = string.Empty;
    int blockSize = 8196;
    byte[] _data;

    public HTTPTransaction()
    {
    }

    public HTTPTransaction(HTTPRequest request)
    {
      Transaction(request);
    }

    public bool HTTPGet(HTTPRequest request)
    {
      return Transaction(request);
    }

    public void SetAgent(string newAgent)
    {
      _agent = newAgent;
    }

    public string GetError()
    {
      return _error;
    }

    public CookieCollection Cookies
    {
      get { return _cookies;}
      set { _cookies=value;}
    }

    public byte[] GetData() //string strURL, string strEncode)
    {
      return _data;
    }

    private bool Transaction(HTTPRequest pageRequest)
    {
      ArrayList Blocks = new ArrayList();
      byte[] Block;
      byte[] readBlock;
      int size;
      int totalSize;

      try
      {
        // Make the Webrequest
        // Create the request header
        Uri pageUri = pageRequest.Uri;
        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(pageUri);
        request.UserAgent = _agent;
        if (pageRequest.PostQuery == string.Empty)
        {
          // GET request
          request.Credentials = HTTPAuth.Get(pageUri.Host);
          request.CookieContainer = new CookieContainer();
          if (_cookies != null)
            request.CookieContainer.Add(_cookies);
        }
        else
        {
          // POST request
          request.ContentType = _postType;
          request.ContentLength = pageRequest.PostQuery.Length;
          request.Method = "POST";

          // Write post message 
          try
          {
            Stream OutputStream = request.GetRequestStream();
            StreamWriter WriteStream = new StreamWriter(OutputStream);
            WriteStream.Write(pageRequest.PostQuery);
            WriteStream.Flush();
          }
          catch (WebException ex)
          {
            _error = ex.Message;
            return false;
          }
        }

        _response = (HttpWebResponse) request.GetResponse();
        if (request.CookieContainer != null)
        {
          _response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
          _cookies = _response.Cookies;
        }

        Stream ReceiveStream = _response.GetResponseStream();

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
        _response.Close();

        int pos=0;
        _data = new byte[ totalSize ];

        for(int i = 0; i< Blocks.Count; i++)
        {
          Block = (byte[]) Blocks[i];
          Block.CopyTo(_data, pos);
          pos += Block.Length;
        }

      }
      catch (WebException ex)
      {
        _error = ex.Message;
        return false;
      }
      return true;
    }
  }
}
