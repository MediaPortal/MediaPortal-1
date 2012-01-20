#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;


/* Code by morpheus_xx to fix handling of multiple KNC1 cards.
 *
 * If there is a better way to get the correct index inside tswriter by querying the tunerfilter, 
 * this solution is obsolete.
 * 
 * The device index counts from 0...n for all KNC1 cards.
 * 
 */

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.ConditionalAccess.KNC
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
      IList<Card> cards = CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
      List<String> deviceids = (from card in cards where card.name.StartsWith("KNC BDA") || card.name.StartsWith("Mystique") select card.devicePath).ToList();            

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
          if (TvCard.DevicePath == curId)
          {
            found = idx;
            break;
          }
        }
      }
      return found;
    }
  }
}