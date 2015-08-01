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
using System.Collections;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormOrderItems : Form
  {
    private MPListViewStringColumnSorter _listViewColumnSorter = null;
    private SortOrder _lastSortOrder = SortOrder.None;

    public FormOrderItems(string caption, string explanation, ListViewItem[] items)
    {
      InitializeComponent();
      Text = caption;
      labelItems.Text = explanation;

      _listViewColumnSorter = new MPListViewStringColumnSorter();
      _listViewColumnSorter.Order = SortOrder.None;
      listViewItems.BeginUpdate();
      listViewItems.Items.AddRange(items);
      listViewItems.EndUpdate();
      listViewItems.ListViewItemSorter = _listViewColumnSorter;
      listViewItems_SelectedIndexChanged(null, null);
    }

    public IList Items
    {
      get
      {
        return listViewItems.Items;
      }
    }

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void buttonOrderByName_Click(object sender, EventArgs e)
    {
      _listViewColumnSorter.OrderType = MPListViewStringColumnSorter.OrderTypes.AsString;

      // Reverse the current sort direction.
      _listViewColumnSorter.Order = _lastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
      _lastSortOrder = _listViewColumnSorter.Order;

      // Perform the sort with these new sort options.
      listViewItems.Sort();

      switch (_listViewColumnSorter.Order)
      {
        case SortOrder.Ascending:
          buttonOrderByName.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_asc;
          break;
        case SortOrder.Descending:
          buttonOrderByName.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_dsc;
          break;
        case SortOrder.None:
          buttonOrderByName.Image = global::Mediaportal.TV.Server.SetupTV.Properties.Resources.icon_sort_none;
          break;
      }

      // Reset sort order to enable manual re-ordering.
      _listViewColumnSorter.Order = SortOrder.None;
    }

    private void buttonOrderUp_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewItems.SelectedItems;
      if (items == null || listViewItems.Items.Count < 2)
      {
        return;
      }
      listViewItems.BeginUpdate();
      try
      {
        foreach (ListViewItem item in items)
        {
          int index = item.Index;
          if (index > 0)
          {
            listViewItems.Items.RemoveAt(index);
            listViewItems.Items.Insert(index - 1, item);
          }
        }
      }
      finally
      {
        listViewItems.EndUpdate();
      }
    }

    private void buttonOrderDown_Click(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = listViewItems.SelectedItems;
      if (items == null || listViewItems.Items.Count < 2)
      {
        return;
      }
      listViewItems.BeginUpdate();
      try
      {
        for (int i = items.Count - 1; i >= 0; i--)
        {
          ListViewItem item = items[i];
          int index = item.Index;
          if (index + 1 < listViewItems.Items.Count)
          {
            listViewItems.Items.RemoveAt(index);
            item = listViewItems.Items.Insert(index + 1, item);
            item.Selected = true;
          }
        }
      }
      finally
      {
        listViewItems.EndUpdate();
      }
    }

    private void listViewItems_SizeChanged(object sender, EventArgs e)
    {
      columnHeaderItem.Width = listViewItems.Width - 6;
    }

    private void listViewItems_SelectedIndexChanged(object sender, EventArgs e)
    {
      buttonOrderByName.Enabled = listViewItems.Items.Count > 1;
      ListView.SelectedListViewItemCollection items = listViewItems.SelectedItems;
      buttonOrderUp.Enabled = items.Count > 1 || (items.Count == 1 && items[0].Index != 0);
      buttonOrderDown.Enabled = items.Count > 1 || (items.Count == 1 && items[0].Index != listViewItems.Items.Count - 1);
    }
  }
}