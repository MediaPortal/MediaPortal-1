#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.Util;
using MediaPortal.MPInstaller.Controls;
using Pabo.MozBar;

namespace MediaPortal.MPInstaller
{
  public partial class ControlPanel : MPInstallerForm
  {
    public MPInstallHelper lst = new MPInstallHelper();
    public MPInstallHelper lst_online = new MPInstallHelper();
    private string InstallDir = Config.GetFolder(Config.Dir.Installer);
    private Hashtable[] groupTables;
    int groupColumn = 0;
    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

    public ControlPanel()
    {
      InitializeComponent();

    }

    private void button2_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    /// <summary>
    /// Handles the Load event of the controlp control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void controlp_Load(object sender, EventArgs e)
    {
      listView1.Items.Clear();
      listView1.Sorting = SortOrder.Ascending;
      lst.LoadFromFile();
      InitCategories();
      LoadListFiles();
      //LoadToListview("All");
      comboBox_filter.SelectedIndex = 0;
      comboBox_view.SelectedIndex = 0;
    }

    public void LoadToListview(string strgroup)
    {
      LoadToListview(lst, listView1, strgroup);
    }
    
    public bool TestView(MPpackageStruct pk, int idx )
    {
      if (checkBox_comp.Checked && VersionPharser.CompareVersions(versionInfo.FileVersion, pk.InstallerInfo.ProjectProperties.MPMinVersion) < 0)
        return false;

      switch (idx)
      {
        case 0:
          return true;
        case 1:
          {
            if (!pk.isNew)
              return true;
            break;
          }
        case 2:
          {
            if (pk.isUpdated)
              return true;
            break;
          }
        case 3:
          {
            if (pk.isNew)
              return true;
            break;
          }
      }
      return false;
    }

    /// <summary>
    /// Loads to listview.
    /// </summary>
    /// <param name="mpih">The mpih.</param>
    /// <param name="lv">The lv.</param>
    /// <param name="strgroup">The strgroup.</param>
    public void LoadToListview(MPInstallHelper mpih, ListView lv, string strgroup)
    {
      lv.Items.Clear();
      controlListView1.Clear();
      for (int i = 0; i < mpih.Items.Count; i++)
      {
        MPpackageStruct pk = (MPpackageStruct)mpih.Items[i];
        if ((pk.InstallerInfo.Group == strgroup || strgroup == "All")&&TestView(pk,comboBox_filter.SelectedIndex))
        {
          ListViewItem item1 = new ListViewItem(pk.InstallerInfo.Name, 0);
          if (pk.InstallerInfo.Logo != null)
          {
            imageList1.Images.Add(pk.InstallerInfo.Logo);
            item1.ImageIndex = imageList1.Images.Count - 1;
          }
          if (pk.isNew) item1.ForeColor = Color.Blue;
          if (pk.isUpdated) item1.ForeColor = Color.BlueViolet;
          item1.Tag = pk;
          item1.ToolTipText = pk.InstallerInfo.Description;
          item1.SubItems.Add(pk.InstallerInfo.Author);
          item1.SubItems.Add(pk.InstallerInfo.Version);
          item1.SubItems.Add(Path.GetFileName(pk.FileName));
          item1.SubItems.Add(pk.InstallerInfo.Group);
          lv.Items.AddRange(new ListViewItem[] { item1 });
          //----------------------
          TileListItem item = new TileListItem();
          item.Title = pk.InstallerInfo.Name;
          item.Author = pk.InstallerInfo.Author;
          item.Version = pk.InstallerInfo.Version;
          item.Description = pk.InstallerInfo.Description;
          item.CreateDate = pk.InstallerInfo.ProjectProperties.CreationDate;
          if (!string.IsNullOrEmpty(pk.ScreenUrl))
          {
            item.PictureBox.LoadAsync(pk.ScreenUrl);
          }
          controlListView1.Add(item);
          //--------------------

        }
      }
      //lv.Items.AddRange(itemlist);
      InitGroups(lv);
      SetGroups(0, lv);
      SetButtonState();
    }

    private void InitGroups(ListView myListView)
    {
      groupTables = new Hashtable[myListView.Columns.Count];
      for (int column = 0; column < myListView.Columns.Count; column++)
      {
        // Create a hash table containing all the groups 
        // needed for a single column.
        groupTables[column] = CreateGroupsTable(column, myListView);
      }

      // Start with the groups created for the Title column.
      SetGroups(0, myListView);

    }

    private void SetGroups(int column, ListView myListView)
    {
      // Remove the current groups.
      myListView.Groups.Clear();

      // Retrieve the hash table corresponding to the column.
      Hashtable groups = (Hashtable)groupTables[column];

      // Copy the groups for the column to an array.
      ListViewGroup[] groupsArray = new ListViewGroup[groups.Count];
      groups.Values.CopyTo(groupsArray, 0);

      // Sort the groups and add them to myListView.
      Array.Sort(groupsArray, new ListViewGroupSorter(myListView.Sorting));
      myListView.Groups.Clear();
      myListView.Groups.AddRange(groupsArray);

      // Iterate through the items in myListView, assigning each 
      // one to the appropriate group.
      foreach (ListViewItem item in myListView.Items)
      {
        // Retrieve the subitem text corresponding to the column.
        string subItemText = item.SubItems[column].Text;

        // For the Title column, use only the first letter.
        if (column == 0)
        {
          subItemText = subItemText.Substring(0, 1);
        }

        // Assign the item to the matching group.
        item.Group = (ListViewGroup)groups[subItemText];
      }
    }
    private Hashtable CreateGroupsTable(int column, ListView myListView)
    {
      // Create a Hashtable object.
      Hashtable groups = new Hashtable();

      // Iterate through the items in myListView.
      foreach (ListViewItem item in myListView.Items)
      {
        // Retrieve the text value for the column.
        string subItemText = item.SubItems[column].Text;

        // Use the initial letter instead if it is the first column.
        if (column == 0)
        {
          subItemText = subItemText.Substring(0, 1);
        }

        // If the groups table does not already contain a group
        // for the subItemText value, add a new group using the 
        // subItemText value for the group header and Hashtable key.
        if (!groups.Contains(subItemText))
        {
          groups.Add(subItemText, new ListViewGroup(subItemText,
              HorizontalAlignment.Left));
        }
      }

      // Return the Hashtable object.
      return groups;
    }

    private class ListViewGroupSorter : IComparer
    {
      private SortOrder order;

      // Stores the sort order.
      public ListViewGroupSorter(SortOrder theOrder)
      {
        order = theOrder;
      }

      // Compares the groups by header value, using the saved sort
      // order to return the correct value.
      public int Compare(object x, object y)
      {
        int result = String.Compare(
            ((ListViewGroup)x).Header,
            ((ListViewGroup)y).Header
        );
        if (order == SortOrder.Ascending)
        {
          return result;
        }
        else
        {
          return -result;
        }
      }
    }


    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetButtonState();
    }

    private void button4_Click(object sender, EventArgs e)
    {
      InstallWizard wiz = new InstallWizard();
      wiz.package.LoadFromFile(Config.GetFolder(Config.Dir.Installer) + @"\" +((MPpackageStruct)(listView1.SelectedItems[0].Tag)).GetLocalFilename());
      if (wiz.package.isValid)
      {
        wiz.starStep();
      }
      else
        MessageBox.Show("Invalid package !");

    }

     private void button5_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        MPpackageStruct pk = lst.Find(listView1.SelectedItems[0].SubItems[0].Text);
        if (pk == null)
          return;
        string file_name = pk.FileName;
        string temp_file = Path.GetFullPath(Environment.GetEnvironmentVariable("TEMP")) + @"\" + file_name;
        DownloadForm dw = new DownloadForm(pk.InstallerInfo.UpdateURL, temp_file);
        dw.Text = listView1.SelectedItems[0].SubItems[3].Text;
        dw.ShowDialog();
        if (File.Exists(temp_file))
        {
          InstallWizard wiz = new InstallWizard();
          wiz.package.LoadFromFile(temp_file);
          if (wiz.package.isValid)
          {
            wiz.starStep();
            LoadListFiles();
            LoadToListview("All");
          }
          else
            MessageBox.Show("Invalid package !");
        }
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      InstallWizard wiz = new InstallWizard();
      wiz.package.LoadFromFile(Config.GetFolder(Config.Dir.Installer) + @"\" + ((MPpackageStruct)(listView1.SelectedItems[0].Tag)).GetLocalFilename());
      //if (wiz.package.isValid)
      //{
        wiz.uninstall(listView1.SelectedItems[0].Text);
        listView1.Items.Clear();
        LoadListFiles();
        LoadToListview("All");
      //}
      //else
      //  MessageBox.Show("Invalid package !");

    }

    private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Set the sort order to ascending when changing
      // column groups; otherwise, reverse the sort order.
      if (listView1.Sorting == SortOrder.Descending ||
          ((e.Column != groupColumn)))
      {
        listView1.Sorting = SortOrder.Ascending;
      }
      else
      {
        listView1.Sorting = SortOrder.Descending;
      }
      groupColumn = e.Column;

      // Set the groups to those created for the clicked column.
      SetGroups(e.Column, listView1);

    }

    private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (comboBox_view.Text == "Icons")
      {
        SetGroups(4, listView1);
        listView1.View = System.Windows.Forms.View.LargeIcon;
      }
      else listView1.View = System.Windows.Forms.View.Details;
    }

    private void tabPage1_Enter(object sender, EventArgs e)
    {
      InitGroups(listView1);
      SetGroups(0, listView1);
    }


    private void button3_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        MPInstallHelper temp_lst = new MPInstallHelper();
        temp_lst.LoadFromFile();
        MPpackageStruct pk = temp_lst.Find(listView1.SelectedItems[0].SubItems[0].Text);
        if (pk != null)
        {
          if (!String.IsNullOrEmpty(pk.InstallerInfo.UpdateURL.Trim()))
          {
            string file_name = "MPExtensionFileList.xml";
            string temp_file = Path.GetFullPath(Environment.GetEnvironmentVariable("TEMP")) + @"\" + file_name;
            DownloadForm dw = new DownloadForm(pk.InstallerInfo.UpdateURL + "/" + file_name, temp_file);
            dw.Text = listView1.SelectedItems[0].SubItems[3].Text;
            dw.ShowDialog();
            if (File.Exists(temp_file))
            {
              MPInstallHelper temp_mpih = new MPInstallHelper();
              temp_mpih.LoadFromFile(temp_file);
              int idx = temp_mpih.IndexOf(pk);
              if (idx > -1)
              {
               //if (((MPpackageStruct)temp_mpih.lst[idx])._intalerStruct.Version.CompareTo(pk._intalerStruct.Version) > 0)
                //{
                  if (MessageBox.Show("New version found!. Do you want download and install ?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                  {
                    MPpackageStruct pk1 = ((MPpackageStruct)temp_mpih.Items[idx]);
                    file_name = pk1.FileName;
                    temp_file = Path.GetFullPath(Environment.GetEnvironmentVariable("TEMP")) + @"\" + file_name;
                    DownloadForm dw1 = new DownloadForm(pk1.InstallerInfo.UpdateURL , temp_file);
                    dw1.Text = pk1.InstallerInfo.UpdateURL + "/" + pk1.FileName + "/" + pk1.InstallerInfo.Version;
                    dw1.ShowDialog();
                    if (File.Exists(temp_file))
                    {
                      InstallWizard wiz = new InstallWizard();
                      wiz.package.LoadFromFile(temp_file);
                      if (wiz.package.isValid)
                      {
                        wiz.starStep();
                        listView1.Items.Clear();
                        lst.LoadFromFile();
                        LoadToListview("All");
                      }
                      else
                        MessageBox.Show("Invalid package !");
                    }
                  }
                //}
                //else
                //{
                //  MessageBox.Show("No updates were found !");
                //}
              }
              else
              {
                MessageBox.Show("Wrong udapte URL !");
              }
            }
          }
          else
          {
            MessageBox.Show("Update information not found !");
          }
        }
        else
        {
          MessageBox.Show("Error processing package !");
        }
      }
    }

    private void InitCategories()
    {
      mozPane1.Items.Clear();
      MozItem it1 = new MozItem();
      it1.Images.Focus = 0;
      it1.Images.FocusImage = null;
      it1.Images.Normal = 0;
      it1.Images.NormalImage = null;
      it1.Images.Selected = 0;
      it1.Images.SelectedImage = null;
      it1.Location = new System.Drawing.Point(2, 2);
      it1.Size = new System.Drawing.Size(151, 57);
      it1.TabIndex = 0;
      it1.Text = "All";
      it1.TextAlign = Pabo.MozBar.MozTextAlign.Right;
      mozPane1.Items.AddRange(new Pabo.MozBar.MozItem[] { it1 });
      int cn = 1;
      foreach (string s in MPinstallerStruct.CategoryListing)
      {
        MozItem it = new MozItem();
        it.Images.Focus = cn;
        it.Images.FocusImage = null;
        it.Images.Normal = cn;
        it.Images.NormalImage = null;
        it.Images.Selected = cn;
        it.Images.SelectedImage = null;
        it.Location = new System.Drawing.Point(2, 2);
        it.Size = new System.Drawing.Size(151, 57);
        it.TabIndex = 0;
        it.Text = s;
        it.TextAlign = Pabo.MozBar.MozTextAlign.Right;
        mozPane1.Items.AddRange(new Pabo.MozBar.MozItem[] { it });
        cn++;
      }
    }

    private void mozPane1_ItemSelected(object sender, MozItemEventArgs e)
    {
      LoadToListview(e.MozItem.Text);
    }

    private void SetButtonState()
    {
      button_uninstall.Enabled = false;
      button3.Enabled = false;
      button4.Enabled = false;
      button5.Enabled = false;
      contextMenuStrip1.Enabled = false;
      if (listView1.SelectedItems.Count > 0)
      {
        MPpackageStruct pk=lst.Find(listView1.SelectedItems[0].Text);
        contextMenuStrip1.Enabled= true; 
        if (!pk.isNew)
        {
          button_uninstall.Enabled = true;
          button4.Enabled = true;
          if (pk.isUpdated)
            button3.Enabled = true;
        }
        else
        {
          button5.Enabled = true;
        }
      }
      else
      {
        button_uninstall.Enabled = false;
        button3.Enabled = false;
        button4.Enabled = false;
        button5.Enabled = false;
      }
    }

    private void button6_Click(object sender, EventArgs e)
    {
      string temp_file = MpiFileList.ONLINE_LISTING;
      if (!Directory.Exists(InstallDir))
      {
        Directory.CreateDirectory(InstallDir);
      }
      DownloadForm dw = new DownloadForm(MPinstallerStruct.DEFAULT_UPDATE_SITE + "/" + "MPExtensionFileList.xml", temp_file);
      dw.Text = "Download online list";
      dw.ShowDialog();
      LoadListFiles();
      mozPane1.SelectItem(0);
      LoadToListview("All");
    }

    private void LoadListFiles()
    {
      lst.LoadFromFile();
      lst.NormalizeNames();
      for (int i = 0; i < lst.Items.Count; i++)
      {
        ((MPpackageStruct)lst.Items[i]).isInstalled = true;
        ((MPpackageStruct)lst.Items[i]).isLocal = true;
      }
      string temp_file = MpiFileList.ONLINE_LISTING;
      if (File.Exists(temp_file))
      {
        lst_online.LoadFromFile(temp_file);
        lst_online.NormalizeNames();
        lst_online.Compare(lst);
        lst.AddRange(lst_online);
      }
      //else MessageBox.Show("File read error");
    }

    private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
    {
      mozPane1.SelectItem(0);
      LoadToListview("All");
    }

    private void informationToolStripMenuItem_Click(object sender, EventArgs e)
    {
      MPpackageStruct pk = lst.Find(listView1.SelectedItems[0].Text);
      Info dlg = new Info(pk);
      dlg.ShowDialog();
    }

    private void button_local_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        this.Hide();
        install_Package(openFileDialog1.FileName);
        this.Show();
      }
    }

    private void install_Package(string fil)
    {
      InstallWizard wiz = new InstallWizard();
      wiz.package.LoadFromFile(fil);
      if (wiz.package.isValid)
      {
        wiz.starStep();
      }
      else
      {
        MessageBox.Show("Invalid package !");
      }
    }

    private void checkBox_comp_CheckedChanged(object sender, EventArgs e)
    {
      mozPane1.SelectItem(0);
      LoadToListview("All");
    }
  }

  public class ListViewGroupSorter : IComparer
  {
    private SortOrder order;

    // Stores the sort order.
    public ListViewGroupSorter(SortOrder theOrder)
    {
      order = theOrder;
    }

    // Compares the groups by header value, using the saved sort
    // order to return the correct value.
    public int Compare(object x, object y)
    {
      int result = String.Compare(
          ((ListViewGroup)x).Header,
          ((ListViewGroup)y).Header
      );
      if (order == SortOrder.Ascending)
      {
        return result;
      }
      else
      {
        return -result;
      }
    }
  }
}