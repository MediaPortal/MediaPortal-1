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
using System.Collections;
using System.Net;

namespace MediaPortal.Utils.Web
{
  public class HTTPRequest : IComparable
  {
    private string _host = string.Empty;
    private string _getQuery = string.Empty;
    private string _postQuery = string.Empty;

    public HTTPRequest()
    {
    }

    public HTTPRequest(HTTPRequest request)
    {
      _host = request._host;
      _getQuery = request._getQuery;
      _postQuery = request._postQuery;
    }

    public HTTPRequest(string uri)
    {
      Uri request = new Uri(uri);
      _host = request.Scheme + Uri.SchemeDelimiter + request.Authority;
      _getQuery = request.PathAndQuery;
    }

    public HTTPRequest(string host, string getQuery)
    {
      _host = host;
      _getQuery = getQuery;
    }

    public string Host
    {
      get { return _host;}
      set { _host = value; }
    }

    public string GetQuery
    {
      get { return _getQuery;}
      set { _getQuery = value; }
    }

    public string PostQuery
    {
      get { return _postQuery;}
      set { _postQuery = value; }
    }

    public Uri GetUri()
    {
      return new Uri(_host + _getQuery);
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

    public string ToString()
    {
      return _host + _getQuery + " POST: " + _postQuery;
    }

    #region IComparable Members

    public int CompareTo(object obj)
    {
      HTTPRequest compareObj = (HTTPRequest)obj;
      string local = _host + _getQuery + _postQuery;
      string compare = compareObj.Host + compareObj.GetQuery + compareObj.PostQuery;
      return local.CompareTo(compare);
    }

    #endregion
  }
}
