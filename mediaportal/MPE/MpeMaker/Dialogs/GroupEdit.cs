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
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
  public partial class GroupEdit : Form
  {
    public GroupItem group = new GroupItem();

    public GroupEdit()
    {
      InitializeComponent();
    }

    private void GroupEdit_Load(object sender, EventArgs e) {}

    public void Set(GroupItem item)
    {
      group = item;
      txt_name.Text = item.Name;
      txt_displayname.Text = item.DisplayName;
    }

    public GroupItem Get()
    {
      group.Name = txt_name.Text;
      group.DisplayName = txt_displayname.Text;
      return group;
    }

    private void txt_name_TextChanged(object sender, EventArgs e) {}
  }
}