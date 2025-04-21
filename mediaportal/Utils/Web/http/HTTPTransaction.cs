#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Services;

// for Service Framework

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Creates an HTTP request and gets response data from web site.
  /// 
  /// Supports both GET and POST opterations
  /// </summary>
  /// <remarks>
  /// Will use the following Services if they are reqistered:
  /// 
  /// - IHttpAuth for NetworkCredentials (site authenication)
  /// - IHttpStats for Site statistics.
  /// </remarks>
  public class HTTPTransaction : IDisposable
  {
    #region Variables

    private string _agent = "Mozilla/4.0 (compatible; MSIE 6.0;  WindowsNT 5.0; .NET CLR 1 .1.4322)";
    private string _postType = "application/x-www-form-urlencoded";
    private CookieCollection _cookies;
    private HttpWebResponse _response;
    private string _error = string.Empty;
    private int blockSize = 8196;
    private byte[] _data;
    private IHttpAuthentication _auth;
    private IHttpStatistics _stats;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HTTPTransaction"/> class.
    /// </summary>
    public HTTPTransaction()
    {
      _auth = GlobalServiceProvider.Get<IHttpAuthentication>();
      _stats = GlobalServiceProvider.Get<IHttpStatistics>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HTTPTransaction"/> class and performs HTTPRequest.
    /// </summary>
    /// <param name="request">The request.</param>
    public HTTPTransaction(HTTPRequest request)
    {
      Transaction(request);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Performs HTTPRequest
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    public bool HTTPGet(HTTPRequest request)
    {
      return Transaction(request);
    }

    public bool HTTPGet(HTTPRequest request, string strFilePath)
    {
      return Transaction(request, strFilePath);
    }

    /// <summary>
    /// Sets the agent used for HTTP requests.
    /// </summary>
    /// <param name="newAgent">The new agent.</param>
    public void SetAgent(string newAgent)
    {
      _agent = newAgent;
    }

    /// <summary>
    /// Gets the error.
    /// </summary>
    /// <returns>error string</returns>
    public string GetError()
    {
      return _error;
    }

    /// <summary>
    /// Gets or sets the cookies.
    /// </summary>
    /// <value>The cookies.</value>
    public CookieCollection Cookies
    {
      get { return _cookies; }
      set { _cookies = value; }
    }

    public HttpStatusCode StatusCode
    {
      get { return this._response != null ? _response.StatusCode : 0; }
    }

    public WebHeaderCollection ResponseHeaders
    {
      get { return this._response != null ? _response.Headers : null; }
    }

    /// <summary>
    /// Gets the data transfered from the web site.
    /// </summary>
    /// <returns>the data</returns>
    public byte[] GetData() //string strURL, string strEncode)
    {
      return _data;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Performs HTTP transactions for the specified page request.
    /// </summary>
    /// <param name="pageRequest">The page request.</param>
    /// <returns>bool - Success/fail</returns>
    private bool Transaction(HTTPRequest pageRequest, string strFullPath = null)
    {
      ArrayList Blocks = new ArrayList();
      byte[] Block;
      byte[] readBlock;
      int size;
      int totalSize;
      DateTime startTime = DateTime.Now;

      if (pageRequest.Delay > 0)
      {
        Thread.Sleep(pageRequest.Delay);
      }

      Uri pageUri = pageRequest.Uri;
      try
      {
        if (!string.IsNullOrWhiteSpace(strFullPath) && !Directory.Exists(Path.GetDirectoryName(strFullPath)))
          return false; //destination directory doesn't exist

        // Make the Webrequest
        // Create the request header
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pageUri);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // request.Proxy = WebProxy.GetDefaultProxy();
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) {}
        request.UserAgent = _agent;
        request.AllowAutoRedirect = false;
        if (pageRequest.Cookies != string.Empty)
        {
          string[] cookiesArray = pageRequest.Cookies.Split(new Char[] {';'});
          foreach (string cookie in cookiesArray)
          {
            string[] cookieParts = cookie.Split(new Char[] {'='});
            if (cookieParts.Length >= 2)
            {
              if (_cookies == null)
                _cookies = new CookieCollection();
              _cookies.Add(new Cookie(cookieParts[0], cookieParts[1], "/", request.RequestUri.Host));
            }
          }
        }

        if (pageRequest.Headers != string.Empty)
        {
          string[] headersArray = pageRequest.Headers.Split(new Char[] { ';' });
          foreach (string header in headersArray)
          {
            string[] headersParts = header.Split(new Char[] { '=' });
            if (headersParts.Length >= 2 && !string.IsNullOrWhiteSpace(headersParts[0]))
              request.Headers[headersParts[0]] = headersParts[1];
          }
        }

        if (pageRequest.ModifiedSince > DateTime.MinValue && File.Exists(strFullPath))
          request.IfModifiedSince = pageRequest.ModifiedSince;

        if (pageRequest.PostQuery == string.Empty)
        {
          // GET request
          if (_auth != null)
          {
            request.Credentials = _auth.Get(pageUri.Host);
          }
          request.CookieContainer = new CookieContainer();
          if (_cookies != null)
          {
            request.CookieContainer.Add(_cookies);
          }
        }
        else
        {
          // POST request
          request.ContentType = _postType;
          request.ContentLength = pageRequest.PostQuery.Length;
          request.Method = "POST";

          request.CookieContainer = new CookieContainer();
          if (_cookies != null)
            request.CookieContainer.Add(_cookies);

          // Write post message 
          try
          {
            Stream OutputStream = request.GetRequestStream();
            using (StreamWriter WriteStream = new StreamWriter(OutputStream))
            {
              WriteStream.Write(pageRequest.PostQuery);
              WriteStream.Flush();
            }
          }
          catch (WebException ex)
          {
            _error = ex.Message;
            return false;
          }
        }

        _response = (HttpWebResponse)request.GetResponse();

        //Check for not modified
        if (_response.StatusCode == HttpStatusCode.NotModified)
        {
          totalSize = 0;
          goto close;
        }
        // Check for redirection
        else if ((_response.StatusCode == HttpStatusCode.Found) ||
            (_response.StatusCode == HttpStatusCode.Redirect) ||
            (_response.StatusCode == HttpStatusCode.Moved) ||
            (_response.StatusCode == HttpStatusCode.MovedPermanently))
        {
          Uri uri = new Uri(_response.Headers["Location"]);
          HttpWebRequest redirect = (HttpWebRequest)WebRequest.Create(uri);
          try
          {
            // Use the current user in case an NTLM Proxy or similar is used.
            // request.Proxy = WebProxy.GetDefaultProxy();
            redirect.Proxy.Credentials = CredentialCache.DefaultCredentials;
          }
          catch (Exception) {}
          redirect.UserAgent = _agent;
          redirect.AllowAutoRedirect = false;
          redirect.Referer = _response.ResponseUri.ToString();
          redirect.Headers.Add(request.Headers);

          redirect.CookieContainer = new CookieContainer();
          if (_response.Headers["Set-Cookie"] != null)
          {
            string cookieStr = _response.Headers["Set-Cookie"];
            Regex cookieParser = new Regex("(?<name>[^=]+)=(?<value>[^;]+)(;)");
            Match result = cookieParser.Match(cookieStr);
            if (result.Success)
            {
              Cookie reply = new Cookie(result.Groups["name"].ToString(), result.Groups["value"].ToString());
              reply.Domain = uri.Host;
              redirect.CookieContainer.Add(reply);
            }
          }
          //redirect.ContentType = "text/html"; 
          _response = (HttpWebResponse)redirect.GetResponse();
        }

        if (request.CookieContainer != null)
        {
          _response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
          _cookies = _response.Cookies;
        }

        Stream ReceiveStream = _response.GetResponseStream();

        Block = new byte[blockSize];
        totalSize = 0;

        if (!string.IsNullOrWhiteSpace(strFullPath))
        {
          //Save to file
          using (FileStream fs = new FileStream(strFullPath, FileMode.Create))
          {
            while ((size = ReceiveStream.Read(Block, 0, blockSize)) > 0)
            {
              fs.Write(Block, 0, size);
              totalSize += size;
            }
          }
        }
        else
        {
          while ((size = ReceiveStream.Read(Block, 0, blockSize)) > 0)
          {
            readBlock = new byte[size];
            Array.Copy(Block, readBlock, size);
            Blocks.Add(readBlock);
            totalSize += size;
          }
        }

        ReceiveStream.Close();

close:
        _response.Close();

        if (Blocks.Count > 0)
        {
          int pos = 0;
          _data = new byte[totalSize];

          for (int i = 0; i < Blocks.Count; i++)
          {
            Block = (byte[])Blocks[i];
            Block.CopyTo(_data, pos);
            pos += Block.Length;
          }
        }
      }
      catch (WebException ex)
      {
        _error = ex.Message;
        return false;
      }

      // Collect sits statistics
      if (_stats != null)
      {
        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime - startTime;
        _stats.Add(pageUri.Host, 1, totalSize, duration);
      }

      return true;
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      if (_response != null)
      {
        _response.Close();
      }
    }

    #endregion
  }
}