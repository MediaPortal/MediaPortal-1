/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SetupTv.Sections
{
  public partial class GroupNameForm : Form
  {
    string _groupName="new group";

    public GroupNameForm()
    {
      InitializeComponent();
    }

    private void GroupName_Load(object sender, EventArgs e)
    {
      mpTextBox1.Text = _groupName;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      _groupName = mpTextBox1.Text;
      Close();
    }

    public string GroupName
    {
      get
      {
        return _groupName;
      }
      set
      {
        _groupName = value;
      }
    }
  }
}