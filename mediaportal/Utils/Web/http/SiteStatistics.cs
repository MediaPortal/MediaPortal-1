#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Net;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Holds the page and data byte statistics for a web site.
  /// </summary>
  public class SiteStatistics
  {
    #region Variables
    private string _site;
    private int _pages = 0;
    private int _bytes = 0;
    private float _rate = 0; // Average transfer rate
    #endregion

    #region Constructors/Destructors
    /// <summary>
    /// Initializes a new instance of the <see cref="SiteStats"/> class.
    /// </summary>
    /// <param name="site">The site name.</param>
    public SiteStatistics(string site)
    {
      _site = site;
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
    #endregion

    #region Public Methods
    /// <summary>
    /// Adds the specified pages and bytes to site statistics.
    /// </summary>
    /// <param name="pages">The pages.</param>
    /// <param name="bytes">The bytes.</param>
    public void Add(int pages, int bytes, float rate)
    {
      _pages += pages;
      _bytes += bytes;
      if (_rate == 0)
        _rate = rate;
      else
        _rate = (_rate + rate) / 2;
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public string ToString()
    {
      if(_rate > 1000)
      return String.Format("Site {0} : Pages {1} : Bytes {2} : Av. Rate {3} KBps", _site, _pages, _bytes, _rate/1000);
      else
      return String.Format("Site {0} : Pages {1} : Bytes {2} : Av. Rate {3} Bps", _site, _pages, _bytes, _rate);

    }
    #endregion
  }
}