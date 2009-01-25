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

using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Implementations;
using TvDatabase;

/* Code by morpheus_xx to fix handling of multiple KNC1 cards.
 *
 * If there is a better way to get the correct index inside tswriter by querying the tunerfilter, 
 * this solution is obsolete.
 * 
 * The device index counts from 0...n for all KNC1 cards.
 * 
 */
namespace TVLibrary.Implementations.DVB
{
  /// <summary>
  /// Enumerates all KNC cards, sorts by DevicePath and returns matching index.
  /// Needed for extendes IKNC interface which passes the card index to tswriter
  /// </summary>
  public static class KNCDeviceLookup
  {
    /// <summary>
    /// Returns DeviceIndex of KNC1 card
    /// </summary>
    /// <param name="TvCard">TvCard object</param>
    /// <returns>The device index</returns>
    public static int GetDeviceIndex(TvCardBase TvCard)
    {
      // temporary list to hold device paths
      List<String> deviceids = new List<String>();
      IList<Server> dbsServers = Server.ListAll();

      foreach (Server server in dbsServers)
      {
        foreach (Card dbsCard in server.ReferringCard())
        {
          // only count all KNC cards
          if (dbsCard.Name.StartsWith("KNC BDA"))
          {
            deviceids.Add(dbsCard.DevicePath);
          }
        }
      }

      int idx = -1;
      int found = 0;
      // loop through all cards and return found as index
      if (deviceids.Count > 0)
      {
        // use sorted list, should be same order as in Windows hardware manager
        deviceids.Sort();
        foreach (String curId in deviceids)
        {
          idx++;
          if (TvCard.DevicePath == curId) { found = idx; break; }
        }
      }
      return found;
    }
  }
}
