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
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using Crownwood.Magic.Menus;
using Crownwood.Magic.Win32;
using Mpe.Controls;

namespace Mpe.Forms
{
  /// <summary>
  /// Summary description for SkinPropertiesControl.
  /// </summary>
  public class MpePropertyManager : UserControl
  {
    #region Variables

    private PropertyGrid propertyGrid;
    //private MpePropertyGrid propertyGrid;
    private IContainer components;
    private ComboBox resourceList;
    private MediaPortalEditor mpe;
    private ImageList menuImageList;
    private PopupMenu contextMenu;

    #endregion

    #region Events and Delegates

    public delegate void ResourceListSelectionChangedHandler(MpeResource selectedControl);


    public event ResourceListSelectionChangedHandler ResourceListSelectionChanged;

    #endregion

    #region Contructors

    public MpePropertyManager(MediaPortalEditor mpe)
    {
      SetStyle(ControlStyles.EnableNotifyMessage, true);
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();
      this.mpe = mpe;
      contextMenu = new PopupMenu();
      MenuCommand category =
        new MenuCommand("Sort By Category", menuImageList, 0, new EventHandler(OnMenuCategoryClicked));
      MenuCommand name = new MenuCommand("Sort By Name", menuImageList, 1, new EventHandler(OnMenuNameClicked));
      contextMenu.MenuCommands.AddRange(new MenuCommand[] {category, name});
      propertyGrid.CommandsVisibleIfAvailable = true;
    }

    #endregion

    #region Methods

    public override void Refresh()
    {
      base.Refresh();
      propertyGrid.Refresh();
    }

    public void EnableResourceList()
    {
      resourceList.Enabled = true;
    }

    public void DisableResourceList()
    {
      resourceList.Enabled = false;
    }

    public void HideResourceList()
    {
      resourceList.Visible = false;
      OnResize(null, null);
    }

    public void ShowResourceList()
    {
      resourceList.Visible = true;
      OnResize(null, null);
    }

    public override string ToString()
    {
      string s = "MpePropertyManager";
      s += "(ResourceListCount=[" + resourceList.Items.Count + "]";
      s += ",Selected=[";
      if (SelectedResource != null)
      {
        s += SelectedResource.Id;
      }
      else
      {
        s += null;
      }
      s += "])";
      return s;
    }

    #endregion

    #region Properties

    protected MpeStatusBar StatusBar
    {
      get { return mpe.StatusBar; }
    }

    public MpeResource SelectedResource
    {
      get
      {
        if (propertyGrid.SelectedObject == null)
        {
          return null;
        }
        return (MpeResource) propertyGrid.SelectedObject;
      }
      set
      {
        if (SelectedResource != value)
        {
          if (SelectedResource != null)
          {
            // Reset currently selected resource
            SelectedResource.Masked = false;
          }
          propertyGrid.SelectedObject = value;
          if (value != null && resourceList != null && resourceList.DataSource != null)
          {
            value.Masked = true;
            try
            {
              for (int i = 0; i < resourceList.Items.Count; i++)
              {
                MpeResource resource = (MpeResource) resourceList.Items[i];
                if (resource.Id == value.Id)
                {
                  MpeLog.Debug("Setting resource list index to " + i.ToString());
                  resourceList.SelectedIndex = i;
                  return;
                }
              }
              resourceList.SelectedIndex = -1;
            }
            catch (Exception ee)
            {
              MpeLog.Debug(ee);
              //MpeLog.Warn(ee);
            }
          }
          else
          {
            resourceList.SelectedIndex = -1;
          }
        }
        else
        {
          propertyGrid.Refresh();
        }
      }
    }

    public MpeResourceCollection ResourceList
    {
      set
      {
        resourceList.DataSource = null;
        if (value != null)
        {
          resourceList.DataSource = value.DataSource;
          resourceList.DisplayMember = "DisplayName";
        }
      }
    }

    #endregion

    #region Event Handlers

    protected override void OnNotifyMessage(Message m)
    {
      if (m.Msg == (int) Msgs.WM_CONTEXTMENU)
      {
        //short x = (short)(m.LParam.ToInt32());
        //short y = (short)(m.LParam.ToInt32() >> 16);
        contextMenu.TrackPopup(new Point(m.LParam.ToInt32()));
      }
      base.OnNotifyMessage(m);
    }

    private void OnMenuCategoryClicked(object sender, EventArgs e)
    {
      try
      {
        propertyGrid.PropertySort = PropertySort.Categorized;
      }
      catch (Exception ee)
      {
        StatusBar.Debug(ee);
      }
    }

    private void OnMenuNameClicked(object sender, EventArgs e)
    {
      try
      {
        propertyGrid.PropertySort = PropertySort.Alphabetical;
      }
      catch (Exception ee)
      {
        StatusBar.Debug(ee);
      }
    }

    private void OnResourceListSelectionChanged(object sender, EventArgs e)
    {
      SelectedResource = (MpeResource) resourceList.SelectedItem;
      MpeLog.Debug("ResourceListSelectionChange = " +
                   (SelectedResource != null ? SelectedResource.Id.ToString() : "null"));
      if (ResourceListSelectionChanged != null)
      {
        ResourceListSelectionChanged(SelectedResource);
      }
    }

    private void OnResize(object sender, EventArgs e)
    {
      if (resourceList.Visible)
      {
        resourceList.Location = new Point(1, 1);
        resourceList.Width = Width - 3;
        propertyGrid.Location = new Point(1, 24);
        propertyGrid.Width = Width - 3;
        propertyGrid.Height = Height - 24;
      }
      else
      {
        propertyGrid.Location = new Point(1, 1);
        propertyGrid.Width = Width - 3;
        propertyGrid.Height = Height - 1;
      }
    }

    private void OnFocusEnter(object sender, EventArgs e)
    {
      OnResize(sender, e);
    }

    #endregion

    #region Component Designer Generated Code

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new Container();
      ResourceManager resources = new ResourceManager(typeof(MpePropertyManager));
      propertyGrid = new PropertyGrid();
      resourceList = new ComboBox();
      menuImageList = new ImageList(components);
      SuspendLayout();
      // 
      // propertyGrid
      // 
      propertyGrid.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                               | AnchorStyles.Left)
                                              | AnchorStyles.Right)));
      propertyGrid.CommandsVisibleIfAvailable = true;
      propertyGrid.LargeButtons = false;
      propertyGrid.LineColor = SystemColors.ScrollBar;
      propertyGrid.Location = new Point(1, 25);
      propertyGrid.Name = "propertyGrid";
      propertyGrid.Size = new Size(414, 321);
      propertyGrid.TabIndex = 0;
      propertyGrid.Text = "PropertyGrid";
      propertyGrid.ToolbarVisible = false;
      propertyGrid.ViewBackColor = SystemColors.Window;
      propertyGrid.ViewForeColor = SystemColors.WindowText;
      // 
      // resourceList
      // 
      resourceList.DropDownStyle = ComboBoxStyle.DropDownList;
      resourceList.Location = new Point(1, 2);
      resourceList.Name = "resourceList";
      resourceList.Size = new Size(413, 21);
      resourceList.TabIndex = 1;
      resourceList.SelectionChangeCommitted += new EventHandler(OnResourceListSelectionChanged);
      // 
      // menuImageList
      // 
      menuImageList.ColorDepth = ColorDepth.Depth24Bit;
      menuImageList.ImageSize = new Size(20, 20);
      menuImageList.ImageStream = ((ImageListStreamer) (resources.GetObject("menuImageList.ImageStream")));
      menuImageList.TransparentColor = Color.Magenta;
      // 
      // MpePropertyManager
      // 
      Controls.Add(resourceList);
      Controls.Add(propertyGrid);
      DockPadding.All = 1;
      Name = "MpePropertyManager";
      Size = new Size(416, 328);
      Resize += new EventHandler(OnResize);
      Enter += new EventHandler(OnFocusEnter);
      ResumeLayout(false);
    }

    #endregion
  }
}