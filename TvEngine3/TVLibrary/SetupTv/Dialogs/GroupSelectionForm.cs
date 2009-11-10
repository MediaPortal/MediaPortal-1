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
using System.Windows.Forms;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;

namespace SetupTv.Sections
{
  public partial class GroupSelectionForm : Form
  {
    string _preselectedGroupName = string.Empty;
    List<object> _groups = new List<object>();

    private SelectionType _SelectionType = SelectionType.ForDeleting;

    public enum SelectionType
    {
      ForDeleting,
      ForRenaming
    }

    public SelectionType Selection 
    {
      get { return _SelectionType; }
      set { _SelectionType = value; }
    }

    public GroupSelectionForm()
    {
      InitializeComponent();
    }

    public GroupSelectionForm(string preselectedGroupName)
    {
      InitializeComponent();

      _preselectedGroupName = preselectedGroupName;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void mpButton2_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    public DialogResult ShowDialog(Type groupType)
    {
      this.LoadGroups(groupType);

      return base.ShowDialog();
    }

    public DialogResult ShowDialog(Type groupType, IWin32Window owner)
    {
      this.LoadGroups(groupType);

      return base.ShowDialog(owner);
    }

    private void LoadGroups(Type groupType)
    {
      if (groupType == typeof(ChannelGroup))
      {
        IList<ChannelGroup> tmp = ChannelGroup.ListAll();
        foreach (ChannelGroup group in tmp)
        {
          bool isFixedGroupName = (
                group.GroupName == TvConstants.TvGroupNames.AllChannels ||
                (
                  _SelectionType == SelectionType.ForRenaming && 
                  (
                    group.GroupName == TvConstants.TvGroupNames.Analog ||
                    group.GroupName == TvConstants.TvGroupNames.DVBC ||
                    group.GroupName == TvConstants.TvGroupNames.DVBS ||
                    group.GroupName == TvConstants.TvGroupNames.DVBT)
                  )
                );

          if (!isFixedGroupName)
          {
            _groups.Add(group);
          }
        }
      }
      else if (groupType == typeof(RadioChannelGroup))
      {
        IList<RadioChannelGroup> tmp = RadioChannelGroup.ListAll();
        foreach (RadioChannelGroup group in tmp)
        {
          bool isFixedGroupName = (
                group.GroupName == TvConstants.RadioGroupNames.AllChannels ||
                (
                  _SelectionType == SelectionType.ForRenaming &&
                  (
                    group.GroupName == TvConstants.RadioGroupNames.Analog ||
                    group.GroupName == TvConstants.RadioGroupNames.DVBC ||
                    group.GroupName == TvConstants.RadioGroupNames.DVBS ||
                    group.GroupName == TvConstants.RadioGroupNames.DVBT)
                  )
                );

          if (!isFixedGroupName)
          {
            _groups.Add(group);
          }
        }
      }
      else
      {
        return;
      }

      try
      {
        foreach (object group in _groups)
        {
          string name = group.GetType().InvokeMember("GroupName", System.Reflection.BindingFlags.GetProperty, null, group, null).ToString();

          listBox1.Items.Add(name);

          if (name == _preselectedGroupName)
          {
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
          }
        }
      }
      catch (Exception exp)
      {
        Log.Error("LoadGroups error: {0}", exp.Message);
      }

      if (listBox1.SelectedIndex <= -1 && listBox1.Items.Count > 0)
      {
        listBox1.SelectedIndex = 0;
      }
    }

    public object Group
    {
      get
      {
        if (_groups == null || listBox1.SelectedIndex <= -1)
        {
          return null;
        }

        return _groups[listBox1.SelectedIndex];
      }
    }
  }
}
