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
using System.Reflection;
using System.Text;
using System.Threading;
using MediaPortal.Services;
using mshtml;
using SHDocVw;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Gets HTML web Page.
  /// </summary>
  public class HTMLPage
  {
    #region Variables

    private string _strPageHead = string.Empty;
    private string _strPageSource = string.Empty;
    private string _defaultEncode = "iso-8859-1";
    private string _pageEncodingMessage = string.Empty;
    private string _encoding = string.Empty;
    private string _error;
    private IHtmlCache _cache;
    private InternetExplorer _IE;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HTMLPage"/> class.
    /// </summary>
    public HTMLPage()
    {
      _cache = GlobalServiceProvider.Get<IHtmlCache>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HTMLPage"/> class.
    /// </summary>
    /// <param name="page">The page request.</param>
    public HTMLPage(HTTPRequest page) : this()
    {
      _encoding = page.Encoding;
      LoadPage(page);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HTMLPage"/> class.
    /// </summary>
    /// <param name="page">The page request.</param>
    /// <param name="encoding">The encoding.</param>
    public HTMLPage(HTTPRequest page, string encoding) : this()
    {
      _encoding = encoding;
      LoadPage(page);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the encoding.
    /// </summary>
    /// <value>The encoding.</value>
    public string Encoding
    {
      get { return _encoding; }
      set { _encoding = value; }
    }

    /// <summary>
    /// Gets the page encoding message.
    /// </summary>
    /// <value>The page encoding message.</value>
    public string PageEncodingMessage
    {
      get { return _pageEncodingMessage; }
    }

    /// <summary>
    /// Gets the error.
    /// </summary>
    /// <value>The error.</value>
    public string Error
    {
      get { return _error; }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Loads the page.
    /// </summary>
    /// <param name="page">The page request.</param>
    /// <returns>true if sucessful</returns>
    public bool LoadPage(HTTPRequest page)
    {
      if (_cache != null && _cache.Initialised)
      {
        if (_cache.LoadPage(page))
        {
          _strPageSource = _cache.GetPage();
          return true;
        }
      }

      // Delay before getting page
      if (page.Delay > 0)
        Thread.Sleep(page.Delay);

      bool success;

      if (page.External)
      {
        success = GetExternal(page);
      }
      else
      {
        success = GetInternal(page);
      }

      if (success)
      {
        if (_cache != null && _cache.Initialised)
        {
          _cache.SavePage(page, _strPageSource);
        }

        return true;
      }
      return false;
    }

    /// <summary>
    /// Gets the page.
    /// </summary>
    /// <returns>page HTML source</returns>
    public string GetPage()
    {
      return _strPageSource;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the page using external com browser IE.
    /// </summary>
    /// <param name="page">The page request.</param>
    /// <returns>true if successful</returns>
    private bool GetExternal(HTTPRequest page)
    {
      // Use External Browser (IE) to get HTML page
      // IE downloads all linked graphics ads, etc
      // IE will run Javascript source if required to renderthe page
      if (_IE == null)
      {
        _IE = new InternetExplorer();
      }

      IWebBrowser2 webBrowser = (IWebBrowser2) _IE;

      object empty = Missing.Value;

      // check if request is POST or GET
      if (page.PostQuery != null)
      {
        ASCIIEncoding encoding = new ASCIIEncoding();
        object postData = (object) encoding.GetBytes(page.PostQuery);
        object header = (object) "Content-Type: application/x-www-form-urlencoded\n\r";
        webBrowser.Navigate(page.Url, ref empty, ref empty, ref postData, ref header);
      }
      else
      {
        webBrowser.Navigate(page.Url, ref empty, ref empty, ref empty, ref empty);
      }

      while (webBrowser.Busy == true)
      {
        Thread.Sleep(500);
      }
      HTMLDocumentClass doc = (HTMLDocumentClass) webBrowser.Document;

      _strPageSource = doc.body.innerHTML;

      return true;
    }

    /// <summary>
    /// Gets the page using internal .NET
    /// </summary>
    /// <param name="page">The page request.</param>
    /// <returns>true if sucessful</returns>
    private bool GetInternal(HTTPRequest page)
    {
      // Use internal code to get HTML page
      HTTPTransaction Page = new HTTPTransaction();
      Encoding encode;
      string strEncode = _defaultEncode;

      if (Page.HTTPGet(page))
      {
        byte[] pageData = Page.GetData();
        int i;

        if (_encoding != "")
        {
          strEncode = _encoding;
          _pageEncodingMessage = "Forced: " + _encoding;
        }
        else
        {
          encode = System.Text.Encoding.GetEncoding(_defaultEncode);
          _strPageSource = encode.GetString(pageData);
          int headEnd;
          if ((headEnd = _strPageSource.ToLower().IndexOf("</head")) != -1)
          {
            if ((i = _strPageSource.ToLower().IndexOf("charset", 0, headEnd)) != -1)
            {
              strEncode = "";
              i += 8;
              for (; i < _strPageSource.Length && _strPageSource[i] != '\"'; i++)
              {
                strEncode += _strPageSource[i];
              }
              _encoding = strEncode;
            }

            if (strEncode == "")
            {
              strEncode = _defaultEncode;
              _pageEncodingMessage = "Default: " + _defaultEncode;
            }
            else
            {
              _pageEncodingMessage = strEncode;
            }
          }
        }

        // Encoding: depends on selected page
        if (_strPageSource == "" || strEncode.ToLower() != _defaultEncode)
        {
          try
          {
            encode = System.Text.Encoding.GetEncoding(strEncode);
            _strPageSource = encode.GetString(pageData);
          }
          catch (ArgumentException)
          {
          }
        }
        return true;
      }
      _error = Page.GetError();
      return false;
    }

    #endregion
  }
}