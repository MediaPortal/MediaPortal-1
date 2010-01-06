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
using System.Text;

namespace TvDatabase
{
  public class EpisodeFormatter
  {
    #region vars

    #endregion

    public static string GetEpisodeNumber(string seriesNum, string episodeNum, string episodePart)
    {
      string episodeInfo = String.Empty;
      if (!String.IsNullOrEmpty(seriesNum))
      {
        episodeInfo = seriesNum.Trim();
      }
      if (!String.IsNullOrEmpty(episodeNum))
      {
        if (episodeInfo.Length != 0)
        {
          episodeInfo += "." + episodeNum.Trim();
        }
        else
        {
          episodeInfo = episodeNum.Trim();
        }
      }
      if (!String.IsNullOrEmpty(episodePart))
      {
        if (!String.IsNullOrEmpty(episodeInfo))
        {
          episodeInfo += "." + episodePart.Trim();
        }
        else
        {
          episodeInfo = episodePart.Trim();
        }
      }
      return episodeInfo;
    }
  }
}