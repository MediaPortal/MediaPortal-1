#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.Net;

namespace MediaPortal.Utils.Web
{
  public class HTTPRequest
  {
    private string _scheme = string.Empty;
    private string _host = string.Empty;
    private string _getQuery = string.Empty;
    private string _postQuery = string.Empty;

    public HTTPRequest()
    {
    }

    public HTTPRequest(string baseUrl, string getQuery, string postQuery)
    {
      Uri baseUri = new Uri(baseUrl);
      Uri request = new Uri(baseUri, getQuery);
      BuildRequest(request);
      _postQuery = postQuery;
    }

    public HTTPRequest(HTTPRequest request)
    {
      _scheme = request._scheme;
      _host = request._host;
      _getQuery = request._getQuery;
      _postQuery = request._postQuery;
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

    public HTTPRequest(string host, string getQuery)
    {
      _host = host;
      _getQuery = getQuery;
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

    public string PostQuery
    {
      get { return _postQuery; }
      set { _postQuery = value; }
    }

    public string Url
    {
      get { return _scheme + Uri.SchemeDelimiter + _host + _getQuery; }
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
      Uri newUri = new Uri(Uri, relativeUri);
      return new HTTPRequest(newUri);
    }

    public void ReplaceTag(string tag, string value)
    {
      _getQuery = _getQuery.Replace(tag, value);
      _postQuery = _postQuery.Replace(tag, value);
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
      object o1 = (object)r1;
      object o2 = (object)r2;
      if (o1 == null || o2 == null)
      {
        if (o1 == null && o2 == null)
          return true;
        return false;
      }

      if (r1._scheme == r2._scheme &&
          r1._host == r2._host &&
          r1._getQuery == r2._getQuery &&
          r1._postQuery == r2._postQuery)
        return true;

      return false;
    }

    public static bool operator !=(HTTPRequest r1, HTTPRequest r2)
    {
      return !(r1 == r2);
    }

    #endregion
  }
}
