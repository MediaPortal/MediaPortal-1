/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Windows.Forms;

namespace SetupTv.Sections
{
  public partial class GroupNameForm : Form
  {
    string _groupName="new group";

    public GroupNameForm()
    {
      InitializeComponent();
      InitNew();
    }

    private void InitNew()
    {
      Text = "Enter name for new group";
      mpLabel1.Text = "Please enter the name for the new group";
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
    }

    private void GroupName_Load(object sender, EventArgs e)
    {
      mpTextBox1.Text = _groupName;
    }

    private void mpButton1_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      _groupName = mpTextBox1.Text;
      Close();
    }

    private void mpButton2_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
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
