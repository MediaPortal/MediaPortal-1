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
using System.Windows.Forms;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class GroupNameForm : Form
  {
    private string _groupName = "new group";
    private List<string> _groupNames;

    private MediaTypeEnum _mediaType = MediaTypeEnum.TV;

   

    public GroupNameForm()
    {
      InitializeComponent();
      InitNew();
    }

    public GroupNameForm(MediaTypeEnum mediaType)
    {
      _mediaType = mediaType;
      InitializeComponent();
      InitNew();
    }

    private void InitNew()
    {
      Text = "Enter name for new group";
      mpLabel1.Text = "Please enter the name for the new group";
      GetGroupNames();
    }

    public GroupNameForm(string groupName)
    {
      InitializeComponent();
      InitChange(groupName);
    }

    private void InitChange(string groupName)
    {
      _groupName = groupName;
      Text = "Change name for group";
      mpLabel1.Text = "Please enter the new name for the group";
      GetGroupNames();
    }

    private void GetGroupNames()
    {
      IList<ChannelGroup> groups = ServiceAgents.Instance.ChannelGroupServiceAgent.ListAllChannelGroupsByMediaType(_mediaType);
      _groupNames = new List<string>();
      foreach (ChannelGroup group in groups)
      {
        _groupNames.Add(group.GroupName);
      }
    }

    private void GroupName_Load(object sender, EventArgs e)
    {
      mpTextBox1.Text = _groupName;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      if (_groupNames.Contains(mpTextBox1.Text))
      {
        MessageBox.Show("Group name already exists.");
      }
      else
      {
        DialogResult = DialogResult.OK;
        _groupName = mpTextBox1.Text;
        Close();
      }
    }

    private void mpButton2_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    public string GroupName
    {
      get { return _groupName; }
      set { _groupName = value; }
    }

    public MediaTypeEnum MediaType
    {
      get { return _mediaType; }
      set { _mediaType = value; }
    }

    private void mpTextBox1_KeyUp(object sender, KeyEventArgs e)
    {
      // is done with form property: AcceptButton = mpbutton1
      //if (e.KeyCode == Keys.Enter)
      //{
      //  DialogResult = DialogResult.OK;
      //  _groupName = mpTextBox1.Text;
      //  Close();
      //}
    }
  }
}