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
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
  public partial class GroupForm : MPInstallerForm
  {
    public MPinstallerStruct _struct;
    public int sortColumn;

    public GroupForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      _struct.SetupGroups.Clear();
      for (int i = 0; i < group_listView.Items.Count; i++)
      {
        _struct.SetupGroups.Add(new GroupString(group_listView.Items[i].SubItems[0].Text,
                                                group_listView.Items[i].SubItems[1].Text));
      }
      this.Close();
    }

    private void button4_Click(object sender, EventArgs e)
    {
      update_listview1(textBox1.Text, textBox2.Text);
    }

    private void update_listview1(string wid, string wval)
    {
      for (int i = 0; i < group_listView.Items.Count; i++)
      {
        if (wid == group_listView.Items[i].SubItems[0].Text)
        {
          group_listView.Items.RemoveAt(i);
          break;
        }
      }
      ListViewItem item1 = new ListViewItem(wid, 0);
      item1.SubItems.Add(wval);
      group_listView.Items.AddRange(new ListViewItem[] {item1});
      group_listView.Sort();
    }

    private void listView1_MouseClick(object sender, MouseEventArgs e)
    {
      if (group_listView.SelectedItems.Count > 0)
      {
        textBox1.Text = group_listView.SelectedItems[0].SubItems[0].Text;
        textBox2.Text = group_listView.SelectedItems[0].SubItems[1].Text;
        button_delete_group.Enabled = true;
      }
      else
      {
        button_delete_group.Enabled = false;
      }
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (group_listView.SelectedItems.Count > 0)
      {
        textBox1.Text = group_listView.SelectedItems[0].SubItems[0].Text;
        textBox2.Text = group_listView.SelectedItems[0].SubItems[1].Text;
        group_listView.Items.Remove(group_listView.SelectedItems[0]);
      }
    }

    private void GroupForm_Load(object sender, EventArgs e)
    {
      group_listView.Items.Clear();
      foreach (GroupString gs in _struct.SetupGroups)
      {
        update_listview1(gs.Id, gs.Name);
      }
    }

    private void tabPage2_Enter(object sender, EventArgs e)
    {
      comboBox1.Items.Clear();
      listView2.Items.Clear();
      listView3.Items.Clear();
      for (int i = 0; i < group_listView.Items.Count; i++)
      {
        comboBox1.Items.Add(new GroupString(group_listView.Items[i].SubItems[0].Text,
                                            group_listView.Items[i].SubItems[1].Text));
      }
      foreach (MPIFileList fl in _struct.FileList)
      {
        ListViewItem item1 = new ListViewItem(fl.FileName, 0);
        //item1.SubItems.Add(fl.FileName);
        listView2.Items.AddRange(new ListViewItem[] {item1});
      }
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      listView2.Items.Clear();
      listView3.Items.Clear();
      foreach (MPIFileList fl in _struct.FileList)
      {
        if (_struct.FindFileInGroup(((GroupString)comboBox1.SelectedItem).Id, fl.FileName))
        {
          ListViewItem item1 = new ListViewItem(fl.FileName, 0);
          //item1.SubItems.Add(fl.FileName);
          listView3.Items.AddRange(new ListViewItem[] {item1});
        }
        else
        {
          ListViewItem item1 = new ListViewItem(fl.FileName, 0);
          //item1.SubItems.Add(fl.FileName);
          listView2.Items.AddRange(new ListViewItem[] {item1});
        }
      }
    }

    /// <summary>
    ///  Add files to group
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void button2_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem li in listView2.SelectedItems)
      {
        listView3.Items.Add(li.Text);
        listView2.Items.Remove(li);
      }
      //for (int i=0; i < listView2.SelectedItems.Count; i++)
      //{
      //  listView3.Items.Add(listView2.SelectedItems[i].Text);
      //  listView2.Items.Remove(listView2.SelectedItems[i]);
      //}
      update_data();
    }

    /// <summary>
    /// Remove files from group
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void button3_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem li in listView3.SelectedItems)
      {
        listView2.Items.Add(li.Text);
        listView3.Items.Remove(li);
      }
      //for (int i = 0; i < listView3.SelectedItems.Count; i++)
      //{
      //  listView2.Items.Add(listView3.SelectedItems[i].Text);
      //  listView3.Items.Remove(listView3.SelectedItems[i]);
      //}
      update_data();
    }

    /// <summary>
    /// Remove a groups the specified id.
    /// </summary>
    /// <param name="id">The id.</param>
    private void remove_group(string id)
    {
      //for (int i = 0; i < _struct.SetupGroupsMappig.Count; i++)
      //{
      //  if (_struct.SetupGroupsMappig[i].Id == id)
      //     _struct.SetupGroupsMappig.RemoveAt(i);
      //}
      List<GroupStringMapping> tmpl = new List<GroupStringMapping>();
      foreach (GroupStringMapping mp in _struct.SetupGroupsMappig)
      {
        if (mp.Id == id)
        {
          tmpl.Add(mp);
        }
      }

      foreach (GroupStringMapping mp in tmpl)
      {
        _struct.SetupGroupsMappig.Remove(mp);
      }
    }

    private void update_data()
    {
      for (int i = 0; i < _struct.SetupGroupsMappig.Count; i++)
      {
        if (_struct.SetupGroupsMappig[i].Id == ((GroupString)comboBox1.SelectedItem).Id)
        {
          _struct.SetupGroupsMappig.RemoveAt(i);
        }
      }

      for (int i = 0; i < listView3.Items.Count; i++)
      {
        _struct.SetupGroupsMappig.Add(new GroupStringMapping(((GroupString)comboBox1.SelectedItem).Id,
                                                             listView3.Items[i].Text));
      }
    }

    private void button5_Click(object sender, EventArgs e) {}

    private void button_delete_group_Click(object sender, EventArgs e)
    {
      if (group_listView.SelectedItems.Count > 0)
      {
        remove_group(group_listView.SelectedItems[0].SubItems[0].Text);
        group_listView.Items.Remove(group_listView.SelectedItems[0]);
      }
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (group_listView.SelectedItems.Count > 0)
      {
        button_delete_group.Enabled = true;
      }
      else
      {
        button_delete_group.Enabled = false;
      }
    }

    private void listView1_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        button_delete_group_Click(null, null);
      }
    }

    private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Determine whether the column is the same as the last column clicked.
      if (e.Column != sortColumn)
      {
        // Set the sort column to the new column.
        sortColumn = e.Column;
        // Set the sort order to ascending by default.
        group_listView.Sorting = SortOrder.Ascending;
      }
      else
      {
        // Determine what the last sort order was and change it.
        if (group_listView.Sorting == SortOrder.Ascending)
        {
          group_listView.Sorting = SortOrder.Descending;
        }
        else
        {
          group_listView.Sorting = SortOrder.Ascending;
        }
      }

      // Call the sort method to manually sort.
      group_listView.Sort();
      // Set the ListViewItemSorter property to a new ListViewItemComparer
      // object.
      this.group_listView.ListViewItemSorter = new ListViewItemComparer(e.Column, group_listView.Sorting, true);
    }
  }
}