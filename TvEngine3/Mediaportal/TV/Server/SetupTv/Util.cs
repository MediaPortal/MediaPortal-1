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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV
{
  public class Utils
  {
    // singleton. Dont allow any instance of this class
    private Utils() {}

    public static void UpdateCardStatus(MPListView mpListView1)
    {
      if (!ServiceHelper.IsAvailable)
      {
        return;
      }
      
      try
      {
        IList<CardPresentation> cards = ServiceAgents.Instance.ControllerServiceAgent.ListAllCards();

        int i = 0;

        while (mpListView1.Items.Count > cards.Count)
        {
          mpListView1.Items.RemoveAt(mpListView1.Items.Count - 1);
        }

        IDictionary<int, int> cardIds = new Dictionary<int, int>();

        foreach (CardPresentation card in cards)
        {
          ListViewItem item;
          if (mpListView1.Items.Count < cards.Count)
          {
            item = mpListView1.Items.Add(""); // 0 card id
            item.SubItems.Add("");//1 card type
            item.SubItems.Add("");//2 state
            item.SubItems.Add("");//3 channelname
            item.SubItems.Add("");//4 scrambled
            item.SubItems.Add("");//5 user
            item.SubItems.Add("");//6 cardname
            item.SubItems.Add("");//7 subchannels 
            item.SubItems.Add("");//7 owner
          }
          else
          {
            item = mpListView1.Items[i];
          }

          string cardId = "n/a";
          if (card.CardId.HasValue)
          {
            cardId = card.CardId.Value.ToString(CultureInfo.InvariantCulture);
          }

          item.SubItems[0].Text = cardId;
          item.SubItems[0].Tag = cardId;
          item.SubItems[1].Text = card.CardType;
          item.SubItems[2].Text = card.State;
          item.SubItems[3].Text = card.ChannelName;
          item.SubItems[4].Text = card.IsScrambled;
          item.SubItems[5].Text = card.UserName;
          item.SubItems[6].Text = card.CardName;
          item.SubItems[7].Text = card.SubChannels.ToString(CultureInfo.InvariantCulture);
          item.SubItems[8].Text = card.IsOwner;

          /*if (!card.Idle)
          {
            int nrOfusers = 0;
            bool hasCardId = cardIds.TryGetValue(card.CardId.GetValueOrDefault(), out nrOfusers);
            if (hasCardId)
            {
              nrOfusers++;
              cardIds[card.CardId.GetValueOrDefault()] = nrOfusers;
            }
            else
            {
              nrOfusers = 1;
              cardIds.Add(card.CardId.GetValueOrDefault(), nrOfusers);
            }
          } */

          if (card.SubChannelsCountOk)
          {
            ColorLine(Color.White, item);
          }
          else
          {
            ColorLine(Color.Red, item);
          }
          i++;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    private static void ColorLine(Color lineColor, ListViewItem item)
    {
      item.UseItemStyleForSubItems = false;
      item.BackColor = lineColor;

      foreach (ListViewItem.ListViewSubItem lvi in item.SubItems)
      {
        lvi.BackColor = lineColor;
      }
    }
  }
}