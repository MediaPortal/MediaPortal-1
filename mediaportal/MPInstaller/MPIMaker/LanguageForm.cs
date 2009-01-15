#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;

namespace MediaPortal.MPInstaller
{
  public partial class Form2 : MPInstallerForm
  {
    public string baseDir;
    public int sortColumn;
    public List<LanguageString> Language;
    public MPLanguageHelper deflang = new MPLanguageHelper();
    public MPLanguageHelper lang = new MPLanguageHelper();
    private string LanguageDirectory = Config.GetFolder(Config.Dir.Language);

    public Form2()
    {
      InitializeComponent();
      deflang.Load("English");
      lang.Load("English");
    }

    private void LoadLanguages()
    {
      if (File.Exists(LanguageDirectory + @"\strings_en.xml"))
      {
        //_availableLanguages = new Dictionary<string, CultureInfo>();

        DirectoryInfo dir = new DirectoryInfo(LanguageDirectory);
        foreach (FileInfo file in dir.GetFiles("strings_*.xml"))
        {
          int pos = file.Name.IndexOf('_') + 1;
          string cultName = file.Name.Substring(pos, file.Name.Length - file.Extension.Length - pos);

          try
          {
            CultureInfo cultInfo = new CultureInfo(cultName);
            languageComboBox.Items.Add(cultInfo.EnglishName);
            languageComboBox2.Items.Add(cultInfo.EnglishName);
          }
          catch (ArgumentException)
          {
          }
        }
      }
      else
      {
        // Get system language
        string strLongLanguage = CultureInfo.CurrentCulture.EnglishName;
        int iTrimIndex = strLongLanguage.IndexOf(" ", 0, strLongLanguage.Length);
        string strShortLanguage = strLongLanguage.Substring(0, iTrimIndex);

        bool bExactLanguageFound = false;
        if (Directory.Exists(LanguageDirectory))
        {
          string[] folders = Directory.GetDirectories(LanguageDirectory, "*.*");


          foreach (string folder in folders)
          {
            string fileName = folder.Substring(folder.LastIndexOf(@"\") + 1);

            //
            // Exclude cvs folder
            //
            if (fileName.ToLower() != "cvs")
            {
              if (fileName.Length > 0)
              {
                fileName = fileName.Substring(0, 1).ToUpper() + fileName.Substring(1);
                languageComboBox.Items.Add(fileName);
                languageComboBox2.Items.Add(fileName);

                // Check language file to user region language
                if (fileName.ToLower() == strLongLanguage.ToLower())
                {
                  languageComboBox2.Text = fileName;
                  bExactLanguageFound = true;
                }
                else if (!bExactLanguageFound && (fileName.ToLower() == strShortLanguage.ToLower()))
                {
                  languageComboBox2.Text = fileName;
                }
              }
            }
          }
        }
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (textBox1.Text.Length == 0)
      {
        MessageBox.Show("ID is mandatory !", "Stop");
        textBox1.Focus();
        return;
      }
      update_listview1(textBox1.Text, textBox2.Text, languageComboBox.Text);
    }

    private void button2_Click(object sender, EventArgs e)
    {
      foreach (int index in listView1.SelectedIndices)
      {
        listView1.Items.RemoveAt(index);
      }
    }

    private void Form2_Load(object sender, EventArgs e)
    {
      LoadLanguages();
      foreach (LanguageString lg in Language)
      {
        ListViewItem item1 = new ListViewItem(lg.dwCode, 0);
        item1.SubItems.Add(lg.mapSting);
        item1.SubItems.Add(lg.language);
        listView1.Items.AddRange(new ListViewItem[] {item1});
      }
      //comboBox1.Items.Clear();
      //foreach (LanguageString lg in deflang.Language)
      //{
      //    comboBox1.Items.Add(lg.dwCode+" - " + lg.mapSting);
      //}
      load_lang();
    }

    private void load_lang()
    {
      listView2.Items.Clear();
      foreach (LanguageString lg in lang.Language)
      {
        ListViewItem item1 = new ListViewItem(lg.dwCode, 0);
        item1.SubItems.Add(lg.mapSting);
        listView2.Items.AddRange(new ListViewItem[] {item1});
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      Language.Clear();
      for (int i = 0; i < listView1.Items.Count; i++)
      {
        Language.Add(new LanguageString(listView1.Items[i].SubItems[2].Text, listView1.Items[i].Text,
                                        listView1.Items[i].SubItems[1].Text));
      }
    }

    private void button4_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void languageComboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
      lang.Load(languageComboBox2.Text);
      load_lang();
    }

    private void button5_Click(object sender, EventArgs e)
    {
      add_reference_value();
    }

    private void add_reference_value()
    {
      foreach (ListViewItem it in listView2.SelectedItems)
      {
        ListViewItem item1 = new ListViewItem(it.SubItems[0].Text, 0);
        item1.SubItems.Add(it.SubItems[1].Text);
        item1.SubItems.Add(languageComboBox2.Text);
        listView1.Items.AddRange(new ListViewItem[] {item1});
      }
    }

    private void button7_Click(object sender, EventArgs e)
    {
      LoadLanguages();
      listView1.Items.Clear();
      foreach (LanguageString lg in Language)
      {
        ListViewItem item1 = new ListViewItem(lg.dwCode, 0);
        item1.SubItems.Add(lg.mapSting);
        item1.SubItems.Add(lg.language);
        listView1.Items.AddRange(new ListViewItem[] {item1});
      }
    }

    private void button6_Click(object sender, EventArgs e)
    {
      if (textBox1.Text.Length == 0)
      {
        MessageBox.Show("ID is mandatory ! You have to give the refence value first !", "Stop");
        textBox1.Focus();
        return;
      }
      string message = "The value '" + textBox2.Text + "' will be used for ID " + textBox1.Text + " as default value.";
      if (!(languageComboBox.Text == "English"))
      {
        message = message + " Caution : an English value should be better as default value ! Do you confirm ?";
      }
      else
      {
        message = message + " Do you confirm ?";
      }
      string caption = "Confirmation";
      MessageBoxButtons buttons = MessageBoxButtons.YesNo;
      DialogResult result;

      // Displays the MessageBox.

      result = MessageBox.Show(message, caption, buttons);

      if (result == DialogResult.No)
      {
        return;
      }
      MPLanguageHelper mplh = new MPLanguageHelper();
      foreach (string lg in languageComboBox.Items)
      {
        mplh.Load(lg);
        if (mplh.isLoaded)
        {
          int idx = -1; // this.Language.BinarySearch(ls, new LanguageStringComparer());
          for (int i = 0; i < mplh.Language.Count; i++)
          {
            if (mplh.Language[i].dwCode.Trim() == textBox1.Text.Trim())
            {
              idx = i;
              break;
            }
          }
          if (idx > -1)
          {
            update_listview1(mplh.Language[idx].dwCode, mplh.Language[idx].mapSting, lg);
          }
          else
          {
            update_listview1(textBox1.Text, textBox2.Text, lg);
          }
        }
      }
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        textBox1.Text = listView1.SelectedItems[0].SubItems[0].Text;
        textBox2.Text = listView1.SelectedItems[0].SubItems[1].Text;
        languageComboBox.Text = listView1.SelectedItems[0].SubItems[2].Text;
        //                listView1.Items.Remove(listView1.SelectedItems[0]);
      }
    }

    private void listView1_MouseClick(object sender, MouseEventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        textBox1.Text = listView1.SelectedItems[0].SubItems[0].Text;
        textBox2.Text = listView1.SelectedItems[0].SubItems[1].Text;
        languageComboBox.Text = listView1.SelectedItems[0].SubItems[2].Text;
        //                listView1.Items.Remove(listView1.SelectedItems[0]);
      }
    }

    private void button8_Click(object sender, EventArgs e)
    {
      listView1.Items.Clear();
    }

    private void update_listview1(string wid, string wval, string wlang)
    {
      for (int i = 0; i < listView1.Items.Count; i++)
      {
        if ((wlang == listView1.Items[i].SubItems[2].Text) && (wid == listView1.Items[i].SubItems[0].Text))
        {
          listView1.Items.RemoveAt(i);
          break;
        }
      }
      //           if (!modify)
      //           {
      ListViewItem item1 = new ListViewItem(wid, 0);
      item1.SubItems.Add(wval);
      item1.SubItems.Add(wlang);
      listView1.Items.AddRange(new ListViewItem[] {item1});
      listView1.Sort();
      //           }
    }

    private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      add_reference_value();
    }

    private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Determine whether the column is the same as the last column clicked.
      if (e.Column != sortColumn)
      {
        // Set the sort column to the new column.
        sortColumn = e.Column;
        // Set the sort order to ascending by default.
        listView1.Sorting = SortOrder.Ascending;
      }
      else
      {
        // Determine what the last sort order was and change it.
        if (listView1.Sorting == SortOrder.Ascending)
        {
          listView1.Sorting = SortOrder.Descending;
        }
        else
        {
          listView1.Sorting = SortOrder.Ascending;
        }
      }

      // Call the sort method to manually sort.
      listView1.Sort();
      // Set the ListViewItemSorter property to a new ListViewItemComparer
      // object.
      this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, listView1.Sorting, true);
    }

    private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Determine whether the column is the same as the last column clicked.
      if (e.Column != sortColumn)
      {
        // Set the sort column to the new column.
        sortColumn = e.Column;
        // Set the sort order to ascending by default.
        listView2.Sorting = SortOrder.Ascending;
      }
      else
      {
        // Determine what the last sort order was and change it.
        if (listView2.Sorting == SortOrder.Ascending)
        {
          listView2.Sorting = SortOrder.Descending;
        }
        else
        {
          listView2.Sorting = SortOrder.Ascending;
        }
      }

      // Call the sort method to manually sort.
      listView2.Sort();
      // Set the ListViewItemSorter property to a new ListViewItemComparer
      // object.
      if (e.Column == 0)
      {
        this.listView2.ListViewItemSorter = new ListViewItemComparer(e.Column, listView2.Sorting, false);
      }
      else
      {
        this.listView2.ListViewItemSorter = new ListViewItemComparer(e.Column, listView2.Sorting, true);
      }
    }
  }

  // Implements the manual sorting of items by columns.
  internal class ListViewItemComparer : IComparer
  {
    private int col;
    private SortOrder order;

    public ListViewItemComparer()
    {
      col = 0;
      order = SortOrder.Ascending;
    }

    // Is the sort alphabetic or number?
    public readonly bool Alphabetic;

    public ListViewItemComparer(int column, SortOrder order, bool alphabetic)
    {
      this.col = column;
      this.order = order;
      this.Alphabetic = alphabetic;
    }

    public int Compare(object x, object y)
    {
      // Convert the items that must be compared into ListViewItem objects.
      string listX = ((ListViewItem) x).SubItems[this.col].Text;
      string listY = ((ListViewItem) y).SubItems[this.col].Text;
      int val;
      // Sort using the specified column and specified sorting type.
      if (Alphabetic)
      {
        val = listX.CompareTo(listY);
      }
      else
      {
        if (int.Parse(listX) > int.Parse(listY))
        {
          val = 1;
        }
        else if (int.Parse(listX) == int.Parse(listY))
        {
          val = 0;
        }
        else
        {
          val = -1;
        }
      }
      if (this.order == SortOrder.Ascending)
      {
        return val;
      }
      else
      {
        return val*-1;
      }
    }
  }
}