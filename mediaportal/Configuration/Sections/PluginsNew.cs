#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
  public partial class PluginsNew : MediaPortal.Configuration.SectionSettings
  {
    public PluginsNew()
      : this("PluginsNew")
    {
    }

    public PluginsNew(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      //ListViewItem item = new ListViewItem();
      //ImageList imageList = new ImageList();
      //imageList.Images.Add(Image.FromFile("mplogo.gif"));
      ////item.ImageList.Images = imageList;
      //item.Text = "Test";
      //item.Tag = null;
      //listViewPlugins.Items.Add(item);
      try
      {
        
      }
      catch (Exception ex)
      {
        Log.Write("Exception: ex.Data           - {0}", ex.Data);
        Log.Write("Exception: ex.HelpLink       - {0}", ex.HelpLink);
        Log.Write("Exception: ex.InnerException - {0}", ex.InnerException);
        Log.Write("Exception: ex.Message        - {0}", ex.Message);
        Log.Write("Exception: ex.Source         - {0}", ex.Source);
        Log.Write("Exception: ex.StackTrace     - {0}", ex.StackTrace);
        Log.Write("Exception: ex.TargetSite     - {0}", ex.TargetSite);
        Log.Write("Exception: ex                - {0}", ex.ToString());
      }

      CreateMyListView();
      
    }

    private void CreateMyListView()
    {
      // Create a new ListView control.
      //ListView listViewPlugins = new ListView();
      //listViewPlugins.Bounds = new Rectangle(new Point(10, 10), new Size(300, 200));

      // Set the view to show details.
      listViewPlugins.View = View.LargeIcon;
      // Allow the user to edit item text.
      //listViewPlugins.LabelEdit = true;
      // Allow the user to rearrange columns.
      //listViewPlugins.AllowColumnReorder = true;
      // Display check boxes.
      listViewPlugins.CheckBoxes = false;
      // Select the item and subitems when selection is made.
      //listViewPlugins.FullRowSelect = true;
      // Display grid lines.
      //listViewPlugins.GridLines = true;
      // Sort the items in the list in ascending order.
      //listViewPlugins.Sorting = SortOrder.Ascending;

      // Create three items and three sets of subitems for each item.
      ListViewItem item1 = new ListViewItem("item1", 0);
      // Place a check mark next to the item.
      //item1.Checked = true;
      //item1.SubItems.Add("1");
      //item1.SubItems.Add("2");
      //item1.SubItems.Add("3");
      ListViewItem item2 = new ListViewItem("item2", 1);
      //item2.SubItems.Add("4");
      //item2.SubItems.Add("5");
      //item2.SubItems.Add("6");
      ListViewItem item3 = new ListViewItem("item3", 0);
      // Place a check mark next to the item.
      //item3.Checked = true;
      //item3.SubItems.Add("7");
      //item3.SubItems.Add("8");
      //item3.SubItems.Add("9");

      // Create columns for the items and subitems.
      //listViewPlugins.Columns.Add("Item Column", -2, HorizontalAlignment.Left);
      //listViewPlugins.Columns.Add("Column 2", -2, HorizontalAlignment.Left);
      //listViewPlugins.Columns.Add("Column 3", -2, HorizontalAlignment.Left);
      //listViewPlugins.Columns.Add("Column 4", -2, HorizontalAlignment.Center);

      //Add the items to the ListView.
      listViewPlugins.Items.AddRange(new ListViewItem[] { item1, item2, item3 });

      // Create two ImageList objects.
      //ImageList imageListSmall = new ImageList();
      ImageList imageListLarge = new ImageList();

      // Initialize the ImageList objects with bitmaps.
      //imageListSmall.Images.Add(Bitmap.FromFile("plugin_raw.png"));
      //imageListSmall.Images.Add(Bitmap.FromFile("plugin_raw.png"));
      imageListLarge.Images.Add(Bitmap.FromFile("plugin_raw.png"));
      imageListLarge.Images.Add(Bitmap.FromFile("plugin_raw.png"));

      imageListLarge.ImageSize = new Size(64, 64);

      //Assign the ImageList objects to the ListView.
      listViewPlugins.LargeImageList = imageListLarge;

      //listViewPlugins.SmallImageList = imageListSmall;
      listViewPlugins.StateImageList = imageListLarge;

      // Add the ListView to the control collection.
      this.Controls.Add(listViewPlugins);
    }
  }
}
