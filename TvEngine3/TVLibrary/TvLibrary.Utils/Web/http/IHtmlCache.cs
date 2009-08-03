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

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Interface to enable HTML caching
  /// </summary>
  public interface IHtmlCache
  {
    #region Properties

    /// <summary>
    /// Gets a value indicating whether this <see cref="IHtmlCache"/> is initialised.
    /// </summary>
    /// <value><c>true</c> if initialised; otherwise, <c>false</c>.</value>
    bool Initialised { get; }

    /// <summary>
    /// Gets or sets the cache mode.
    /// </summary>
    /// <value>The cache mode.</value>
    HTMLCache.Mode CacheMode { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Deletes a cached page.
    /// </summary>
    /// <param name="pageUri">The page URI.</param>
    void DeleteCachePage(HTTPRequest page);

    /// <summary>
    /// Loads a page from cache.
    /// </summary>
    /// <param name="pageUri">The page URI.</param>
    /// <returns>bool - true if the page is in the cache</returns>
    bool LoadPage(HTTPRequest page);

    /// <summary>
    /// Saves a page to the cache.
    /// </summary>
    /// <param name="pageUri">The page URI.</param>
    /// <param name="strSource">The HTML source.</param>
    void SavePage(HTTPRequest page, string strSource);

    /// <summary>
    /// Gets the page source of the current loaded page.
    /// </summary>
    /// <returns>HTML source as a string</returns>
    string GetPage();

    #endregion
  }
}