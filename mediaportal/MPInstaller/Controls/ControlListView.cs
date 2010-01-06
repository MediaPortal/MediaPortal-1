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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace MediaPortal.MPInstaller.Controls
{
  /// <summary>
  /// Summary description for ControlListView.
  /// </summary>
  public class ControlListView : System.Windows.Forms.UserControl
  {
    private System.ComponentModel.IContainer components;
    private System.Windows.Forms.ImageList imageListLargeIcons;
    private System.Windows.Forms.ImageList imageListItemBackground;
    public new event MouseEventHandler MouseUp = null;
    public new event MouseEventHandler MouseDown = null;
    public new event EventHandler DoubleClick = null;
    public System.Collections.ArrayList controlList;
    public event SelectedIndexChangedEventHandler SelectedIndexChanged = null;

    public ControlListView()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();
      controlList = new ArrayList();
      // TODO: Add any initialization after the InitializeComponent call
      InitializeMyComponent();
    }

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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof (ControlListView));
      this.imageListLargeIcons = new System.Windows.Forms.ImageList(this.components);
      this.imageListItemBackground = new System.Windows.Forms.ImageList(this.components);
      // 
      // imageListLargeIcons
      // 
      this.imageListLargeIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
      this.imageListLargeIcons.ImageSize = new System.Drawing.Size(32, 32);
      this.imageListLargeIcons.ImageStream =
        ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLargeIcons.ImageStream")));
      this.imageListLargeIcons.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // imageListItemBackground
      // 
      this.imageListItemBackground.ImageSize = new System.Drawing.Size(250, 72);
      this.imageListItemBackground.ImageStream =
        ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListItemBackground.ImageStream")));
      this.imageListItemBackground.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // ControlListView
      // 
      this.AutoScroll = true;
      this.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.Name = "ControlListView";
      this.Click += new System.EventHandler(this.ControlListView_Click);
      this.SizeChanged += new System.EventHandler(this.ControlListView_SizeChanged);
    }

    #endregion

    private void InitializeMyComponent()
    {
      base.MouseDown += new MouseEventHandler(ControlListView_MouseDown);
      base.MouseUp += new MouseEventHandler(ControlListView_MouseUp);
      base.DoubleClick += new EventHandler(ControlListView_DoubleClick);
    }

    private void ControlListView_MouseDown(object sender, MouseEventArgs e)
    {
      if (this.Selected == false)
      {
        this.Selected = true;
      }
      if (MouseDown != null)
      {
        MouseDown(sender, e);
      }
    }

    private void ControlListView_MouseUp(object sender, MouseEventArgs e)
    {
      if (MouseUp != null)
      {
        MouseUp(this, e);
      }
    }

    private void ControlListView_DoubleClick(object sender, EventArgs e)
    {
      if (DoubleClick != null)
      {
        DoubleClick(sender, e);
      }
    }

    private int itemWidth = 640;

    public int ItemWidth
    {
      get { return itemWidth; }
      set { itemWidth = value; }
    }

    private int itemHeight = 100;

    public int ItemHeight
    {
      get { return itemHeight; }
      set { itemHeight = value; }
    }

    public void ReCalculateItems()
    {
      //this.SuspendLayout();
      TileListItem lItem;
      for (int i = 0; i < controlList.Count; i++)
      {
        lItem = (TileListItem)controlList[i];
        lItem.Location = GetItemLocation(i);
      }
      AdjustHeight();
      this.ResumeLayout();
    }

    private System.Drawing.Color itemHorverColor = Color.Snow;

    public System.Drawing.Color ItemHorverColor
    {
      get { return itemHorverColor; }
      set { itemHorverColor = value; }
    }

    private System.Drawing.Color itemSelectionColor = Color.WhiteSmoke;

    public System.Drawing.Color ItemSelectionColor
    {
      get { return itemSelectionColor; }
      set { itemSelectionColor = value; }
    }

    private System.Drawing.Color itemNormalColor = Color.White;

    public System.Drawing.Color ItemNormalColor
    {
      get { return itemNormalColor; }
      set { itemNormalColor = value; }
    }

    private System.Drawing.Point GetItemLocation(int index)
    {
      int ItemPerRow = (this.Width - 20) / ItemWidth;
      if (ItemPerRow == 0)
      {
        ItemPerRow = 1;
      }
      int rowIndex = index / ItemPerRow;
      int colIndex = index - rowIndex * ItemPerRow;
      //Point p = new Point(colIndex * (ItemWidth + verticleSpacing) + 10, rowIndex * (ItemHeight + horizontalSpacing) + 50);
      Point p = new Point(colIndex * (ItemWidth + verticleSpacing) + verticleSpacing,
                          rowIndex * (ItemHeight + horizontalSpacing) + horizontalSpacing);
      return p;
    }

    public int HeightToShowAll
    {
      get
      {
        System.Drawing.Point p = GetItemLocation(controlList.Count - 1);
        return p.Y + ItemHeight + horizontalSpacing;
      }
    }

    private int horizontalSpacing = 5;

    public int HorizontalSpacing
    {
      get { return horizontalSpacing; }
      set { horizontalSpacing = value; }
    }

    private int verticleSpacing = 5;

    public int VerticleSpacing
    {
      get { return verticleSpacing; }
      set { verticleSpacing = value; }
    }

    private System.Drawing.Color normalColor = Color.FromArgb(0xE6, 0xE6, 0xE6);

    public System.Drawing.Color NormalColor
    {
      get { return normalColor; }
      set { normalColor = value; }
    }

    private System.Drawing.Color selectedColor = Color.FromArgb(0xD8, 0xD8, 0xD8);

    public System.Drawing.Color SelectedColor
    {
      get { return selectedColor; }
      set { selectedColor = value; }
    }

    private bool selected = false;

    public bool Selected
    {
      get { return selected; }
      set
      {
        selected = value;
        if (selected == true)
        {
          this.BackColor = selectedColor;
        }
        else
        {
          this.BackColor = normalColor;
          if (previousSelectedItem != null)
          {
            previousSelectedItem.Selected = false;
          }
        }
      }
    }

    private TileListItem previousSelectedItem = null;

    public TileListItem SelectedItem
    {
      get { return previousSelectedItem; }
    }

    private TileListItem previousHorverItem = null;

    public TileListItem HorverItem
    {
      get { return previousHorverItem; }
    }

    private void lItem_ItemSelected(object sender, EventArgs e)
    {
      TileListItem lItem = (TileListItem)sender;

      if (previousSelectedItem != null)
      {
        if (lItem != previousSelectedItem)
        {
          previousSelectedItem.Selected = false;
        }
      }
      previousSelectedItem = lItem;
      if (SelectedIndexChanged != null)
      {
        this.Selected = true;
        SelectedIndexChanged(this, lItem);
      }
    }

    private bool autoResize = false;

    public bool AutoResize
    {
      get { return autoResize; }
      set { autoResize = value; }
    }

    public System.Windows.Forms.ImageList ItemIconImageList
    {
      get { return this.imageListLargeIcons; }
    }

    public System.Windows.Forms.ImageList ItemBackgroundImage
    {
      get { return this.imageListItemBackground; }
    }

    private void Item_MouseEnter(object sender, EventArgs e)
    {
      if (previousHorverItem != null)
      {
        if (previousHorverItem.Selected == false)
        {
          //previousHorverItem.BackgroundImage=imageListItemBackground.Images[0];
        }
      }
      previousHorverItem = (TileListItem)sender;
    }

    private Size oldSize;
    private bool autoAdjustHeight = false;

    public bool AutoAdjustHeight
    {
      get { return autoAdjustHeight; }
      set { autoAdjustHeight = value; }
    }

    private void AdjustHeight()
    {
      if (AutoAdjustHeight == false)
        return;
      if (this.Count == 0)
      {
        this.Height = 50;
      }
      else
      {
        int height = 0;
        height = this.HeightToShowAll;
        if (this.Height != height + 10)
          this.Height = height + 10;
      }
    }

    private bool IsReCalculateNeeded
    {
      get
      {
        int ItemPerRow = (this.Width - 20) / ItemWidth;
        int oldItemPerRow = (oldSize.Width - 20) / ItemWidth;
        if (ItemPerRow == oldItemPerRow)
        {
          return false;
        }
        else
          return true;
      }
    }

    private void Item_DoubleClick(object sender, EventArgs e)
    {
      if (DoubleClick != null)
      {
        DoubleClick(sender, e);
      }
    }

    private void ControlListView_Click(object sender, System.EventArgs e)
    {
      if (SelectedIndexChanged != null)
      {
        this.Selected = true;
        if (previousSelectedItem != null)
        {
          previousSelectedItem.Selected = false;
          previousSelectedItem = null;
        }
        SelectedIndexChanged(this, null);
      }
    }

    private void ControlListView_SizeChanged(object sender, System.EventArgs e)
    {
      if (AutoResize == true && IsReCalculateNeeded == true)
      {
        ReCalculateItems();
      }
      oldSize = this.Size;
    }

    #region IList Members

    public bool IsReadOnly
    {
      get
      {
        // TODO:  Add ControlListView.IsReadOnly getter implementation
        return false;
      }
    }

    public TileListItem this[int index]
    {
      get
      {
        // TODO:  Add ControlListView.this getter implementation
        return (TileListItem)controlList[index];
      }
      set
      {
        // TODO:  Add ControlListView.this setter implementation
        controlList[index] = value;
      }
    }

    public void RemoveAt(int index)
    {
      // TODO:  Add ControlListView.RemoveAt implementation
      Remove((TileListItem)this.controlList[index]);
    }

    public void Insert(int index, TileListItem value)
    {
      // TODO:  Add ControlListView.Insert implementation
      TileListItem lItem = (TileListItem)value;
      PrepareItemToAdd(lItem);
      this.Controls.Add(lItem);
      controlList.Insert(index, value);
      ReCalculateItems();
    }

    public void Remove(TileListItem value)
    {
      // TODO:  Add ControlListView.Remove implementation
      controlList.Remove(value);
      this.Controls.Remove(value);
      ReCalculateItems();
    }

    public bool Contains(TileListItem value)
    {
      // TODO:  Add ControlListView.Contains implementation
      if (this.SelectedItem == value)
      {
        value.Selected = false;
      }
      return controlList.Contains(value);
    }

    public void Clear()
    {
      // TODO:  Add ControlListView.Clear implementation
      controlList.Clear();
      this.Controls.Clear();
      ReCalculateItems();
    }

    public int IndexOf(TileListItem value)
    {
      // TODO:  Add ControlListView.IndexOf implementation
      return controlList.IndexOf(value);
    }

    private void PrepareItemToAdd(TileListItem lItem)
    {
      lItem.ItemSelected += new EventHandler(lItem_ItemSelected);
      lItem.MouseEnter += new EventHandler(Item_MouseEnter);
      lItem.MouseLeave += new EventHandler(Item_MouseLeave);
      lItem.MouseUp += new MouseEventHandler(ControlListView_MouseUp);
      lItem.MouseDown += new MouseEventHandler(ControlListView_MouseDown);
      lItem.DoubleClick += new EventHandler(Item_DoubleClick);
      lItem.Image = imageListLargeIcons.Images[lItem.ImageIndex];
      lItem.Size = new System.Drawing.Size(ItemWidth, ItemHeight);
      lItem.HorverColor = ItemHorverColor;
      lItem.SelectionColor = ItemSelectionColor;
      lItem.NormalColor = ItemNormalColor;
      lItem.BackColor = lItem.NormalColor;
      //lItem.NormalImage = imageListItemBackground.Images[0];
      //lItem.HorverImage=imageListItemBackground.Images[1];
      //lItem.SelectionImage=imageListItemBackground.Images[2];
      lItem.Selected = false;
      lItem.ShowToolTips = true;
    }

    private void Item_MouseLeave(object sender, EventArgs e) {}

    public int Add(TileListItem value)
    {
      // TODO:  Add ControlListView.Add implementation
      TileListItem lItem = (TileListItem)value;
      PrepareItemToAdd(lItem);
      this.Controls.Add(lItem);
      int i = controlList.Add(lItem);
      ReCalculateItems();
      return i;
    }

    public bool IsFixedSize
    {
      get
      {
        // TODO:  Add ControlListView.IsFixedSize getter implementation
        return controlList.IsFixedSize;
      }
    }

    #endregion

    #region ICollection Members

    public bool IsSynchronized
    {
      get
      {
        // TODO:  Add ControlListView.IsSynchronized getter implementation
        return controlList.IsSynchronized;
      }
    }

    public int Count
    {
      get
      {
        // TODO:  Add ControlListView.Count getter implementation
        return controlList.Count;
      }
    }

    public void CopyTo(Array array, int index)
    {
      // TODO:  Add ControlListView.CopyTo implementation
      for (int i = 0; i < array.Length; i++)
      {
        if (!(array.GetValue(i) is TileListItem))
        {
          throw new Exception("Only TileListItem class is expected");
        }
      }
      this.Controls.CopyTo(array, index);
      controlList.CopyTo(array, index);
    }

    public object SyncRoot
    {
      get
      {
        // TODO:  Add ControlListView.SyncRoot getter implementation
        return controlList.SyncRoot;
      }
    }

    #endregion

    #region IEnumerable Members

    public IEnumerator GetEnumerator()
    {
      // TODO:  Add ControlListView.GetEnumerator implementation
      return controlList.GetEnumerator();
    }

    #endregion
  }

  public delegate void SelectedIndexChangedEventHandler(object sender, TileListItem item);
}