#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
  public partial class GroupForm : Form
  {
    public MPinstalerStruct _struct;
    public GroupForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      _struct.SetupGroups.Clear();
      for (int i = 0; i < listView1.Items.Count; i++)
      {
        _struct.SetupGroups.Add(new GroupString(listView1.Items[i].SubItems[0].Text, listView1.Items[i].SubItems[1].Text));
      }
      this.Close();
    }

    private void button4_Click(object sender, EventArgs e)
    {
      update_listview1(textBox1.Text, textBox2.Text);
    }
    
    private void update_listview1(string wid, string wval)
    {
      for (int i = 0; i < listView1.Items.Count; i++)
      {
        if (wid == listView1.Items[i].SubItems[0].Text)
        {
          listView1.Items.RemoveAt(i);
          break;
        }
      }
      ListViewItem item1 = new ListViewItem(wid, 0);
      item1.SubItems.Add(wval);
      listView1.Items.AddRange(new ListViewItem[] { item1 });
      listView1.Sort();
    }

    private void listView1_MouseClick(object sender, MouseEventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        textBox1.Text = listView1.SelectedItems[0].SubItems[0].Text;
        textBox2.Text = listView1.SelectedItems[0].SubItems[1].Text;
      }
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        textBox1.Text = listView1.SelectedItems[0].SubItems[0].Text;
        textBox2.Text = listView1.SelectedItems[0].SubItems[1].Text;
        listView1.Items.Remove(listView1.SelectedItems[0]);
      }

    }

    private void GroupForm_Load(object sender, EventArgs e)
    {
      listView1.Items.Clear();
      foreach (GroupString gs in _struct.SetupGroups)
      {
        update_listview1(gs.Id, gs.Name);
      }
    }

    private void tabPage2_Enter(object sender, EventArgs e)
    {
      comboBox1.Items.Clear();
      for (int i = 0; i < listView1.Items.Count; i++)
      {
        comboBox1.Items.Add(listView1.Items[i].SubItems[0].Text);
      }
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      listView2.Items.Clear();
      listView3.Items.Clear();
      foreach (MPIFileList fl in _struct.FileList)
      {
        if (_struct.FindFileInGroup(comboBox1.Text, fl.FileName))
        {
          ListViewItem item1 = new ListViewItem(fl.FileName, 0);
          //item1.SubItems.Add(fl.FileName);
          listView3.Items.AddRange(new ListViewItem[] { item1 });
        }
        else
        {
          ListViewItem item1 = new ListViewItem(fl.FileName, 0);
          //item1.SubItems.Add(fl.FileName);
          listView2.Items.AddRange(new ListViewItem[] { item1 });
        }
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      for (int i=0; i < listView2.SelectedItems.Count; i++)
      {
        listView3.Items.Add(listView2.SelectedItems[i].Text);
        listView2.Items.Remove(listView2.SelectedItems[i]);
      }
      update_data();
    }

    private void button3_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < listView3.SelectedItems.Count; i++)
      {
        listView2.Items.Add(listView3.SelectedItems[i].Text);
        listView3.Items.Remove(listView3.SelectedItems[i]);
      }
      update_data();
    }
    
    private void update_data()
    {
      for (int i = 0; i < _struct.SetupGroupsMappig.Count; i++)
      {
        if (_struct.SetupGroupsMappig[i].Id==comboBox1.Text)
          _struct.SetupGroupsMappig.RemoveAt(i);
      }

      for (int i = 0; i < listView3.Items.Count; i++)
      {
        _struct.SetupGroupsMappig.Add(new GroupStringMapping(comboBox1.Text, listView3.Items[i].Text));
      }
    }
  }
}