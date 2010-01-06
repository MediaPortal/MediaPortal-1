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
using System.Collections.Generic;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Service class to collect statistics on HTTP pages and bytes of data transfered.
  /// </summary>
  public class HttpStatistics : IHttpStatistics
  {
    #region Variables

    private Dictionary<string, SiteStatistics> _siteList;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpStats"/> class.
    /// </summary>
    public HttpStatistics() {}

    #endregion

    #region Properties

    /// <summary>
    /// Gets the number of sites for which statistics are stored.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
      get
      {
        if (_siteList != null)
        {
          return _siteList.Count;
        }
        else
        {
          return 0;
        }
      }
    }

    #endregion

    #region <IHttpStatistics> Implementations

    /// <summary>
    /// Getbies the index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>the site statistics</returns>
    public SiteStatistics GetbyIndex(int index)
    {
      Dictionary<string, SiteStatistics>.Enumerator listEnumerator = _siteList.GetEnumerator();

      listEnumerator.MoveNext();
      for (int i = 0; i < index; i++)
      {
        listEnumerator.MoveNext();
      }

      return listEnumerator.Current.Value;
    }

    /// <summary>
    /// Gets the statistics for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <returns>site statistics</returns>
    public SiteStatistics Get(string site)
    {
      SiteStatistics stats = null;

      if (_siteList != null)
      {
        stats = _siteList[site];
      }

      return stats;
    }

    /// <summary>
    /// Clears the statistics for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    public void Clear(string site)
    {
      if (_siteList.ContainsKey(site))
      {
        //_siteList.Remove(site);
        SiteStatistics stats = _siteList[site];
        stats.Clear();
      }
    }

    /// <summary>
    /// Adds to the statistics for a specified site.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <param name="pages">The pages.</param>
    /// <param name="bytes">The bytes.</param>
    public void Add(string site, int pages, int bytes, TimeSpan time)
    {
      SiteStatistics stats;

      if (_siteList == null)
      {
        _siteList = new Dictionary<string, SiteStatistics>();
      }

      if (_siteList.ContainsKey(site))
      {
        stats = _siteList[site];
        stats.Add(pages, bytes, time);
      }
      else
      {
        stats = new SiteStatistics(site);
        stats.Add(pages, bytes, time);
        _siteList.Add(site, stats);
      }
    }

    #endregion
  }
}