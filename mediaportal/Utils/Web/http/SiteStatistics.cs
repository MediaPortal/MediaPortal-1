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
  /// Holds the page and data byte statistics for a web site.
  /// </summary>
  public class SiteStatistics
  {
    #region Variables

    private string _site;
    private int _pages;
    private int _bytes;
    private TimeSpan _totalTime;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteStats"/> class.
    /// </summary>
    /// <param name="site">The site name.</param>
    public SiteStatistics(string site)
    {
      _site = site;
      Clear();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the number of pages transfered since collecting statistics was started.
    /// </summary>
    /// <value>The pages.</value>
    public int Pages
    {
      get { return _pages; }
      set { _pages = value; }
    }

    /// <summary>
    /// Gets or sets the number of bytes transfered since collecting statistics was started.
    /// </summary>
    /// <value>The bytes.</value>
    public int Bytes
    {
      get { return _bytes; }
      set { _bytes = value; }
    }

    /// <summary>
    /// Gets the total time.
    /// </summary>
    /// <value>The total time.</value>
    public TimeSpan TotalTime
    {
      get { return _totalTime; }
    }

    /// <summary>
    /// Gets the site.
    /// </summary>
    /// <value>The site.</value>
    public string Site
    {
      get { return _site; }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds the specified pages and bytes to site statistics.
    /// </summary>
    /// <param name="pages">The pages.</param>
    /// <param name="bytes">The bytes.</param>
    public void Add(int pages, int bytes, TimeSpan time)
    {
      _pages += pages;
      _bytes += bytes;
      _totalTime = _totalTime.Add(time);
    }

    /// <summary>
    /// Clears the statistics.
    /// </summary>
    public void Clear()
    {
      _pages = 0;
      _bytes = 0;
      _totalTime = new TimeSpan();
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public new string ToString()
    {
      float rate = _bytes/(float) _totalTime.TotalSeconds;
      if (rate > 1000)
      {
        return String.Format("Site {0} : Pages {1} : Bytes {2} : Total Time {3} : Av. Rate {4} KBps", _site, _pages,
                             _bytes, _totalTime.ToString(), rate/1000);
      }
      else
      {
        return String.Format("Site {0} : Pages {1} : Bytes {2} : Total Time {3} : Av. Rate {4} Bps", _site, _pages,
                             _bytes, _totalTime.ToString(), rate);
      }
    }

    #endregion
  }
}