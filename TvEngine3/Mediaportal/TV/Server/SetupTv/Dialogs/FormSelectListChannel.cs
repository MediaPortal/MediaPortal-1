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
using System.Windows.Forms;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormSelectListChannel : Form
  {

    public FormSelectListChannel()
    {
      InitializeComponent();
      InitChannels();
    }

    public int ShowFormModal()
    {
      ShowDialog();
      if (DialogResult == DialogResult.OK)
      {
        if (listViewChannels.SelectedItems.Count == 1)
        {
          Channel selectedChannel = listViewChannels.SelectedItems[0].Tag as Channel;
          if (selectedChannel.IdChannel > -1)
          {
            this.LogDebug("SelectListChannel: Channel '{0}' has been selected. ID = {1}", selectedChannel.DisplayName,
                      selectedChannel.IdChannel);
            return selectedChannel.IdChannel;
          }
        }
        else
        {
          this.LogDebug("SelectListChannel: Invalid channel selection.");
        }
      }

      // return unknown channel
      return -1;
    }

    private void InitChannels()
    {
      listViewChannels.Clear();
      listViewChannels.BeginUpdate();
      try
      {
        IList<Channel> channels = new List<Channel>();
        
        if (checkBoxGuideChannels.Checked)
        {
          channels = ServiceAgents.Instance.ChannelServiceAgent.ListAllVisibleChannelsByMediaType(MediaTypeEnum.TV);
        }
        else
        {
          channels =
            ServiceAgents.Instance.ChannelServiceAgent.ListAllChannelsByMediaType(MediaTypeEnum.TV).OrderBy(c => c.SortOrder).
              OrderBy(c => c.DisplayName).ToList();
        }                

        foreach (Channel t in channels)
        {
          // TODO: add imagelist with channel logos from MP :)
          ListViewItem curItem = new ListViewItem(t.DisplayName) {Tag = t};
          listViewChannels.Items.Add(curItem);
        }
      }
      finally
      {
        listViewChannels.EndUpdate();
      }
      mpButtonOk.Enabled = (listViewChannels.Items.Count > 0);
    }

    #region button events

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void checkBoxGuideChannels_CheckedChanged(object sender, EventArgs e)
    {
      InitChannels();
    }

    private void checkBoxFTA_CheckedChanged(object sender, EventArgs e)
    {
      InitChannels();
    }

    #endregion

    #region internal events

    private void listViewChannels_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
      try
      {
        if (e.Item != null)
        {
          e.Item.Checked = e.IsSelected;
        }
      }
      catch (Exception) {}
    }

    private void listViewChannels_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      try
      {
        if (e.Item != null)
        {
          e.Item.Selected = e.Item.Checked;
        }
      }
      catch (Exception) {}
    }

    #endregion
  }
}