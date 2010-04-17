#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gentle.Framework;
using TvDatabase;
using TvLibrary.Log;

namespace SetupTv.Dialogs
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
      this.ShowDialog();
      if (this.DialogResult == DialogResult.OK)
      {
        if (listViewChannels.SelectedItems.Count == 1)
        {
          Channel selectedChannel = listViewChannels.SelectedItems[0].Tag as Channel;
          if (selectedChannel.IdChannel > -1)
          {
            Log.Debug("SelectListChannel: Channel '{0}' has been selected. ID = {1}", selectedChannel.DisplayName,
                      selectedChannel.IdChannel);
            return selectedChannel.IdChannel;
          }
        }
        else
        {
          Log.Debug("SelectListChannel: Invalid channel selection.");
        }
      }

      // return unknown channel
      return -1;
    }

    private void InitChannels()
    {
      listViewChannels.Clear();
      listViewChannels.BeginUpdate();
      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof (Channel));
      if (checkBoxGuideChannels.Checked)
      {
        sb.AddConstraint(Operator.Equals, "visibleInGuide", 1);
      }
      if (checkBoxFTA.Checked)
      {
        sb.AddConstraint(Operator.Equals, "freetoair", 1);
      }
      sb.AddConstraint(Operator.Equals, "isTv", 1);
      sb.AddOrderByField(true, "sortOrder");
      sb.AddOrderByField(true, "displayName");
      SqlStatement stmt = sb.GetStatement(true);
      IList<Channel> channels = ObjectFactory.GetCollection<Channel>(stmt.Execute());

      for (int i = 0; i < channels.Count; i++)
      {
        // TODO: add imagelist with channel logos from MP :)
        ListViewItem curItem = new ListViewItem(channels[i].DisplayName);
        curItem.Tag = channels[i];
        listViewChannels.Items.Add(curItem);
      }
      listViewChannels.EndUpdate();
      mpButtonOk.Enabled = (listViewChannels.Items.Count > 0);
    }

    #region button events

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void mpButtonOk_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close();
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