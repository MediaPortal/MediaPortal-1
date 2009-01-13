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

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Interface for HTTP statistics class.
  /// </summary>
  public interface IHttpStatistics
  {
    #region Properties

    /// <summary>
    /// Gets the number of sites for which statistics are stored.
    /// </summary>
    /// <value>The count.</value>
    int Count { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Getbies the index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>the site statistics</returns>
    SiteStatistics GetbyIndex(int index);

    /// <summary>
    /// Gets the statistics for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <returns>the site statistics</returns>
    SiteStatistics Get(string site);

    /// <summary>
    /// Clears the statistics for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    void Clear(string site);

    /// <summary>
    /// Adds to the statistics for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <param name="pages">The pages.</param>
    /// <param name="bytes">The bytes.</param>
    /// <param name="rate">The rate.</param>
    void Add(string site, int pages, int bytes, TimeSpan time);

    #endregion
  }
}