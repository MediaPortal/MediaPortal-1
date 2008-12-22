#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Text;
using System.IO;
using System.Net;
using System.Web;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Class provides HTML caching
  /// </summary>
  public class HTMLCache : IHtmlCache
  {
    #region Enums
    public enum Mode
    {
      Disabled = 0,
      Enabled = 1,
      Replace = 2
    }
    #endregion

    #region Variables
    const string CACHE_DIR = "WebCache";
    static bool _initialised = false;
    static Mode _cacheMode = Mode.Disabled;
    static string _strPageSource;
    #endregion

    #region Constructors/Destructors
    /// <summary>
    /// Initializes a new instance of the <see cref="HTMLCache"/> class.
    /// </summary>
    public HTMLCache()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets a value indicating whether this <see cref="IHtmlCache"/> is initialised.
    /// </summary>
    /// <value><c>true</c> if initialised; otherwise, <c>false</c>.</value>
    public bool Initialised
    {
      get { return _initialised; }
    }

    public Mode CacheMode
    {
      get { return _cacheMode; }
      set { _cacheMode = value; }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initialises the WebCache.
    /// </summary>
    public void WebCacheInitialise()
    {
      if (!System.IO.Directory.Exists(CACHE_DIR))
        System.IO.Directory.CreateDirectory(CACHE_DIR);

      _initialised = true;
    }

    /// <summary>
    /// Deletes a cached page.
    /// </summary>
    /// <param name="pageUri">The page URI.</param>
    public void DeleteCachePage(HTTPRequest page)
    {
      string file = GetCacheFileName(page);

      if (System.IO.File.Exists(file))
        System.IO.File.Delete(file);
    }

    /// <summary>
    /// Loads a page from cache.
    /// </summary>
    /// <param name="pageUri">The page URI.</param>
    /// <returns>bool - true if the page is in the cache</returns>
    public bool LoadPage(HTTPRequest page)
    {
      if (_cacheMode == Mode.Enabled)
      {
        if (LoadCacheFile(GetCacheFileName(page)))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Saves a page to the cache.
    /// </summary>
    /// <param name="pageUri">The page URI.</param>
    /// <param name="strSource">The HTML source.</param>
    public void SavePage(HTTPRequest page, string strSource)
    {
      if (_cacheMode != Mode.Disabled)
        SaveCacheFile(GetCacheFileName(page), strSource);
    }

    /// <summary>
    /// Gets the page source of the current loaded page.
    /// </summary>
    /// <returns>HTML source as a string</returns>
    public string GetPage() //string strURL, string strEncode)
    {
      return _strPageSource;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Loads the cache file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns></returns>
    private bool LoadCacheFile(string file)
    {
      if (System.IO.File.Exists(file))
      {
        TextReader CacheFile = new StreamReader(file);
        _strPageSource = CacheFile.ReadToEnd();
        CacheFile.Close();

        return true;
      }

      return false;
    }

    /// <summary>
    /// Saves the cache file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="source">The source.</param>
    private void SaveCacheFile(string file, string source)
    {
      if (System.IO.File.Exists(file))
        System.IO.File.Delete(file);

      TextWriter CacheFile = new StreamWriter(file);
      CacheFile.Write(source);
      CacheFile.Close();
    }

    /// <summary>
    /// Gets the name of the cache file.
    /// </summary>
    /// <param name="Page">The page.</param>
    /// <returns>filename</returns>
    private static string GetCacheFileName(HTTPRequest Page)
    {
      uint gethash = (uint)Page.Uri.GetHashCode();

      if (Page.PostQuery == null || Page.PostQuery == string.Empty)
        return CACHE_DIR + "/" + Page.Host + "_" + gethash.ToString() + ".html";

      uint posthash = (uint)Page.PostQuery.GetHashCode();

      return CACHE_DIR + "/" + Page.Host + "_" + gethash.ToString() + "_" + posthash.ToString() + ".html";
    }
    #endregion
  }
}
