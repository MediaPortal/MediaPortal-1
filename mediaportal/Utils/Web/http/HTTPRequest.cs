#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Xml.Serialization;

namespace MediaPortal.Utils.Web
{
  public class HTTPRequest
  {
    private string _host = string.Empty;
    private string _getQuery = string.Empty;
    private string _postQuery = string.Empty;
    private string _cookies = string.Empty;
    private string _scheme = string.Empty;
    private bool _externalBrowser = false;
    private string _encoding = string.Empty;
    private int _delay = 0;

    public HTTPRequest()
    {
    }

    public HTTPRequest(string baseUrl, string getQuery)
    {
      Uri baseUri = new Uri(baseUrl);
      Uri request = new Uri(baseUri, getQuery);
      BuildRequest(request);
    }

    public HTTPRequest(string baseUrl, string getQuery, string postQuery)
      : this(baseUrl, getQuery)
    {
      _postQuery = postQuery;
    }

    public HTTPRequest(string baseUrl, string getQuery, string postQuery, string encoding)
      : this(baseUrl, getQuery, postQuery)
    {
      _encoding = encoding;
    }

    public HTTPRequest(HTTPRequest request)
    {
      _scheme = request._scheme;
      _host = request._host;
      _getQuery = request._getQuery;
      _postQuery = request._postQuery;
      _cookies = request._cookies;
      _externalBrowser = request._externalBrowser;
      _encoding = request._encoding;
    }

    public HTTPRequest(Uri request)
    {
      BuildRequest(request);
    }

    public HTTPRequest(string uri)
    {
      Uri request = new Uri(uri);
      BuildRequest(request);
    }

    private void BuildRequest(Uri request)
    {
      _host = request.Authority;
      _scheme = request.Scheme;
      _getQuery = request.PathAndQuery;
      _getQuery = _getQuery.Replace("%5B", "[");
      _getQuery = _getQuery.Replace("%5D", "]");
    }

    public string Host
    {
      get { return _host; }
    }

    public string GetQuery
    {
      get { return _getQuery; }
    }

    [XmlAttribute("url")]
    public string Url
    {
      set { BuildRequest(new Uri(value)); }
      get { return _scheme + Uri.SchemeDelimiter + _host + _getQuery; }
    }

    [XmlAttribute("post")]
    public string PostQuery
    {
      get { return _postQuery; }
      set { _postQuery = value; }
    }

    [XmlAttribute("external")]
    public bool External
    {
      get { return _externalBrowser; }
      set { _externalBrowser = value; }
    }

    [XmlAttribute("cookies")]
    public string Cookies
    {
      get { return _cookies; }
      set { _cookies = value; }
    }

    [XmlAttribute("encoding")]
    public string Encoding
    {
      get { return _encoding; }
      set { _encoding = value; }
    }

    [XmlAttribute("delay")]
    public int Delay
    {
      get { return _delay; }
      set { _delay = value; }
    }

    public Uri Uri
    {
      get { return new Uri(Url); }
    }

    public Uri BaseUri
    {
      get { return new Uri(_scheme + Uri.SchemeDelimiter + _host); }
    }

    // Add relative or absolute url
    public HTTPRequest Add(string relativeUri)
    {
      if (relativeUri.StartsWith("?"))
        relativeUri = Uri.LocalPath + relativeUri;
      Uri newUri = new Uri(Uri, relativeUri);
      HTTPRequest newHTTPRequest = new HTTPRequest(newUri);
      newHTTPRequest._encoding = this._encoding;
      return newHTTPRequest;
    }

    public void ReplaceTag(string tag, string value)
    {
      _getQuery = _getQuery.Replace(tag, value);
      _postQuery = _postQuery.Replace(tag, value);
      _cookies = _cookies.Replace(tag, value);
    }

    public bool HasTag(string tag)
    {
      if (_getQuery.IndexOf(tag) != -1)
        return true;

      if (_postQuery.IndexOf(tag) != -1)
        return true;

      return false;
    }

    public override string ToString()
    {
      return Url + " POST: " + _postQuery;
    }

    #region Operator Members

    public static bool operator ==(HTTPRequest r1, HTTPRequest r2)
    {
      if ((object)r1 == null || (object)r2 == null)
      {
        if ((object)r1 == null && (object)r2 == null)
          return true;
        return false;
      }
      return r1.Equals(r2);
    }

    public static bool operator !=(HTTPRequest r1, HTTPRequest r2)
    {
      return !(r1 == r2);
    }

    public override bool Equals(object obj)
    {
      HTTPRequest req = obj as HTTPRequest;
      if (req == null)
        return false;
      if (_scheme == req._scheme &&
          _host == req._host &&
          _getQuery == req._getQuery &&
          _postQuery == req._postQuery)
        return true;

      return false;
    }

    public override int GetHashCode()
    {
      return (_host + _getQuery + _scheme + _postQuery).GetHashCode();
    }
    #endregion
  }
}
