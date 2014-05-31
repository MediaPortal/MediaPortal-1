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
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;


namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class LnbTypes : SectionSettings
  {


    public LnbTypes(string name)
      : base(name)
    {
     
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      RefreshAll();
    }

    private void RefreshAll()
    {
      mpListViewLnbTypes.Items.Clear();
      IList<LnbType> lnbTypes = ServiceAgents.Instance.CardServiceAgent.ListAllLnbTypes();

      foreach (var lnbType in lnbTypes)
      {
        ListViewItem item = CreateListViewItem(lnbType);

        mpListViewLnbTypes.Items.Add(item);
      }
    }

    private static ListViewItem CreateListViewItem(LnbType lnbType)
    {
      ListViewItem item = new ListViewItem(lnbType.Name);
      item.Tag = lnbType;

      item.SubItems.Add(lnbType.LowBandFrequency.ToString());
      item.SubItems.Add(lnbType.HighBandFrequency.ToString());
      item.SubItems.Add(lnbType.IsBandStacked.ToString());
      item.SubItems.Add(lnbType.IsToroidal.ToString());
      return item;
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
    }

    private void mpButtonAdd_Click(object sender, EventArgs e)
    {
      FormLNBType dlg = new FormLNBType();
      dlg.LnbType = null;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {        
        mpListViewLnbTypes.BeginUpdate();
        try
        {
          RefreshAll();
        }
        finally
        {
          mpListViewLnbTypes.EndUpdate();          
        }
      }
    }

    private bool _ignoreItemCheckedEvent = false;
    private void mpButtonEdit_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListViewLnbTypes.SelectedIndices;
      if (indexes.Count == 0)
        return;
      LnbType lnbType = (LnbType)mpListViewLnbTypes.Items[indexes[0]].Tag;
      FormLNBType dlg = new FormLNBType {LnbType = lnbType};
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        lnbType = dlg.LnbType;
        mpListViewLnbTypes.Items[indexes[0]].Tag = lnbType;
        
        mpListViewLnbTypes.BeginUpdate();
        try
        {
          _ignoreItemCheckedEvent = true;
          ListViewItem item = CreateListViewItem(lnbType);
          mpListViewLnbTypes.Items[indexes[0]] = item;
          mpListViewLnbTypes.Sort();
          ReOrder();
        }
        finally
        {
          mpListViewLnbTypes.EndUpdate();
          RefreshAll();
          _ignoreItemCheckedEvent = false;
        }
      }
    }

    private void ReOrder()
    {
    }

    private void mpButtonDel_Click(object sender, EventArgs e)
    {
      mpListViewLnbTypes.BeginUpdate();
      try
      {

        foreach (ListViewItem item in mpListViewLnbTypes.SelectedItems)
        {
          LnbType lnbType = (LnbType)item.Tag;

          ServiceAgents.Instance.CardServiceAgent.DeleteLnbType(lnbType.IdLnbType);
          mpListViewLnbTypes.Items.Remove(item);
        }
        
        ReOrder();
        mpListViewLnbTypes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      finally
      {
        mpListViewLnbTypes.EndUpdate();
        RefreshAll();
      }
    }

   
  }
}