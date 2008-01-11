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
using System.IO;
using System.Resources;
using System.Windows.Forms;
using Crownwood.Magic.Menus;
using Mpe.Controls;
using Mpe.Controls.Properties;
using Mpe.Designers.Mask;
using Mpe.Forms;

namespace Mpe.Designers
{
  /// <summary>
  /// This is the base class that all resource designers should inherit from.
  /// This class will handle all mouse, keyboard and control events.
  /// </summary>
  public class MpeResourceDesigner : UserControl, MpeDesigner
  {
    #region Variables

    private MediaPortalEditor mpe;
    private MpeControlMask mask;
    private MpeResourceCollection resourceList;
    private PopupMenu contextMenu;

    private IContainer components;
    private ImageList menuImageList;

    private const int WM_KEYDOWN = 0x100;
    private const int WM_KEYUP = 0x101;
    private const int WM_SYSKEYDOWN = 0x104;

    private MouseEventArgs activeMouseEvent;

    protected MpeScreen screen;

    #endregion

    #region Constructors

    public MpeResourceDesigner(MediaPortalEditor mpe)
    {
      // Set Painting Styles
      SetStyle(ControlStyles.DoubleBuffer, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.UserPaint, true);

      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      // Initialize variables
      this.mpe = mpe;
      mask = new MpeControlMask(this);
      //this.mask2 = new MpeResourceMask();
      //this.Controls.Add(mask2);
      resourceList = new MpeResourceCollection();

      // Create Popup Context Menu
      contextMenu = new PopupMenu();
      contextMenu.MenuCommands.Add(new MenuCommand("Parent", menuImageList, 4));
      contextMenu.MenuCommands.Add(new MenuCommand("-"));
      contextMenu.MenuCommands.Add(new MenuCommand("Send To Back", menuImageList, 0));
      contextMenu.MenuCommands.Add(new MenuCommand("Bring To Front", menuImageList, 1));
      contextMenu.MenuCommands.Add(new MenuCommand("-"));
      contextMenu.MenuCommands.Add(new MenuCommand("Copy"));
      contextMenu.MenuCommands.Add(new MenuCommand("Paste"));
      contextMenu.MenuCommands.Add(new MenuCommand("Cut"));
      contextMenu.MenuCommands.Add(new MenuCommand("Delete", menuImageList, 2));
      contextMenu.MenuCommands.Add(new MenuCommand("-"));
      contextMenu.MenuCommands.Add(new MenuCommand("Properties", menuImageList, 3));
      contextMenu.MenuCommands.Add(new MenuCommand("Test"));
    }

    #endregion

    #region Properties

    public MpeStatusBar StatusBar
    {
      get { return mpe.StatusBar; }
    }

    public MpePropertyManager PropertyManager
    {
      get { return mpe.PropertyManager; }
    }

    public MpeParser Parser
    {
      get { return mpe.Parser; }
    }

    public MpeControlMask Mask
    {
      get { return mask; }
    }

    public MpeResourceCollection ResourceList
    {
      get { return resourceList; }
    }

    #endregion

    #region Properties - Designer

    public virtual string ResourceName
    {
      get { return "MpeResourceDesigner"; }
    }

    public virtual bool AllowAdditions
    {
      get { return false; }
    }

    public virtual bool AllowDeletions
    {
      get { return false; }
    }

    #endregion

    #region Methods

    protected virtual void UpdatePropertyManager()
    {
      if (ResourceList == null)
      {
        PropertyManager.HideResourceList();
      }
      else
      {
        PropertyManager.ResourceList = ResourceList;
        PropertyManager.ShowResourceList();
      }
      PropertyManager.SelectedResource = mask.SelectedControl;
    }

    protected virtual void UpdateContextMenu(MpeControl c)
    {
      //
    }

    private void PrepareControl(MpeControl c)
    {
      if (c != null && c.Embedded == false)
      {
        MpeLog.Debug("Preparing " + c.ToString());
        ResourceList.Add(c);
        c.MpeScreen = screen;
        c.Click += new EventHandler(OnControlClick);
        c.MouseDown += new MouseEventHandler(OnControlMouseDown);
        c.StatusChanged += new MpeControl.StatusChangedHandler(OnControlStatusChanged);
        c.KeyUp += new KeyEventHandler(OnKeyUp);
        c.IdentityChanged += new MpeControl.IdentityChangedHandler(OnControlIdentityChanged);
        c.PropertyValueChanged += new MpeControl.PropertyValueChangedHandler(OnControlPropertyValueChanged);
        if (c is MpeContainer)
        {
          c.ControlAdded += new ControlEventHandler(OnControlAdded);
          c.ControlRemoved += new ControlEventHandler(OnControlRemoved);
          if (AllowAdditions)
          {
            c.DragDrop += new DragEventHandler(OnDragDrop);
            c.DragEnter += new DragEventHandler(OnDragEnter);
            MpeLog.Debug("DragDrop enabled");
          }
          for (int i = 0; i < c.Controls.Count; i++)
          {
            if (c.Controls[i] is MpeControl)
            {
              PrepareControl((MpeControl) c.Controls[i]);
            }
          }
        }
      }
      else if (c != null && c.Embedded == true)
      {
        MpeLog.Debug("Preparing Embedded " + c.ToString());
        c.MpeScreen = screen;
      }
      MpeLog.Debug("Prepared " + c.ToString());
    }

    private void ReleaseControl(MpeControl c)
    {
      MpeLog.Debug("Removing " + c.ToString());
      if (c is MpeContainer)
      {
        for (int i = 0; i < c.Controls.Count; i++)
        {
          if (c.Controls[i] is MpeControl)
          {
            ReleaseControl((MpeControl) c.Controls[i]);
          }
        }
        c.ControlAdded -= new ControlEventHandler(OnControlAdded);
        c.ControlRemoved -= new ControlEventHandler(OnControlRemoved);
        if (AllowAdditions)
        {
          c.DragDrop -= new DragEventHandler(OnDragDrop);
          c.DragEnter -= new DragEventHandler(OnDragEnter);
        }
      }
      c.Click -= new EventHandler(OnControlClick);
      c.MouseDown -= new MouseEventHandler(OnControlMouseDown);
      c.StatusChanged -= new MpeControl.StatusChangedHandler(OnControlStatusChanged);
      c.KeyUp -= new KeyEventHandler(OnKeyUp);
      c.PropertyValueChanged -= new MpeControl.PropertyValueChangedHandler(OnControlPropertyValueChanged);
      c.IdentityChanged -= new MpeControl.IdentityChangedHandler(OnControlIdentityChanged);
      ResourceList.Remove(c);
      MpeLog.Debug("Removed " + c.ToString());
    }

    protected void AddControl(MpeControl c)
    {
      Controls.Add(c);
    }

    protected void RemoveControl(MpeControl c)
    {
      Controls.Remove(c);
    }

    public virtual void CopyControl()
    {
      if (mask.SelectedControl != null)
      {
        mpe.Clipboard = mask.SelectedControl;
        MpeLog.Info("Copied control...");
      }
    }

    public virtual void PasteControl()
    {
      if (mask.SelectedControl is MpeContainer)
      {
        if (mpe.Clipboard != null && mpe.Clipboard is MpeControl)
        {
          try
          {
            MpeControl c = ((MpeControl) mpe.Clipboard).Copy();
            c.Left += mask.GridSize.Width;
            c.Top += mask.GridSize.Height;
            mask.SelectedControl.Controls.Add(c);
            mask.SelectedControl = c;
            UpdatePropertyManager();
            MpeLog.Info("Pasted control...");
          }
          catch (Exception ee)
          {
            MpeLog.Debug(ee);
            MpeLog.Error(ee);
          }
        }
      }
    }

    public virtual void CutControl()
    {
      if (AllowDeletions)
      {
        if (mask.SelectedControl != screen && mask.SelectedControl.MpeParent != null)
        {
          try
          {
            MpeControl control = mask.SelectedControl;
            MpeContainer parent = control.MpeParent;
            parent.Controls.Remove(control);
            mpe.Clipboard = control;
            mask.SelectedControl = null;
            UpdatePropertyManager();
          }
          catch (Exception ee)
          {
            MpeLog.Debug(ee);
            MpeLog.Error(ee);
          }
        }
      }
    }

    #endregion

    #region Methods - Designer

    public virtual void Destroy()
    {
      try
      {
        PropertyManager.ResourceListSelectionChanged -=
          new MpePropertyManager.ResourceListSelectionChangedHandler(OnResourceListSelectionChanged);
        PropertyManager.ResourceList = null;
        PropertyManager.SelectedResource = null;
      }
      catch (Exception e)
      {
        throw new DesignerException(e.Message);
      }
    }

    public virtual void Pause()
    {
      try
      {
        PropertyManager.ResourceListSelectionChanged -=
          new MpePropertyManager.ResourceListSelectionChangedHandler(OnResourceListSelectionChanged);
        PropertyManager.ResourceList = null;
        PropertyManager.SelectedResource = null;
      }
      catch (Exception e)
      {
        throw new DesignerException(e.Message);
      }
    }

    public virtual void Resume()
    {
      try
      {
        PropertyManager.ResourceListSelectionChanged +=
          new MpePropertyManager.ResourceListSelectionChangedHandler(OnResourceListSelectionChanged);
        UpdatePropertyManager();
      }
      catch (Exception e)
      {
        throw new DesignerException(e.Message);
      }
    }

    public virtual void Initialize()
    {
      //
    }

    public virtual void Save()
    {
      //
    }

    public virtual void Cancel()
    {
      //
    }

    #endregion

    #region Event Handlers

    public virtual void OnControlAdded(object sender, ControlEventArgs e)
    {
      if (e.Control != null && e.Control is MpeControl)
      {
        PrepareControl((MpeControl) e.Control);
      }
    }

    public virtual void OnControlRemoved(object sender, ControlEventArgs e)
    {
      if (e.Control != null && e.Control is MpeControl)
      {
        ReleaseControl((MpeControl) e.Control);
      }
    }

    public virtual void OnControlStatusChanged(MpeControl sender, bool modified)
    {
      mpe.ToggleDesignerStatus(ResourceName, modified);
    }

    public virtual void OnControlPropertyValueChanged(MpeControl sender, string property)
    {
      switch (property)
      {
        case "Id":
          UpdatePropertyManager();
          break;
      }
    }

    public virtual void OnControlIdentityChanged(MpeControl sender, IdentityEventArgs e)
    {
      if (ResourceList != null && ResourceList.IsUniqueId(e.New) == false)
      {
        e.Cancel = true;
        e.Message = "The new Id is not unique and will therefore be reset.";
      }
      else
      {
        e.Cancel = false;
        MpeLog.Info("Control Id set to " + e.New);
      }
    }

    public void OnResourceListSelectionChanged(MpeResource resource)
    {
      if (resource is MpeControl)
      {
        mask.SelectedControl = (MpeControl) resource;
      }
    }

    public void OnControlClick(object sender, EventArgs e)
    {
      if (activeMouseEvent != null)
      {
        if (mask.SelectedControl != sender && (sender is MaskComponent) == false)
        {
          try
          {
            mask.SelectedControl = (MpeControl) sender;
            Focus();
            UpdatePropertyManager();
          }
          catch (Exception ee)
          {
            MpeLog.Debug(ee);
            MpeLog.Error(ee);
          }
        }
        if (activeMouseEvent.Button == MouseButtons.Right)
        {
          if (sender != null && sender is Control)
          {
            Control c = (Control) sender;
            UpdateContextMenu(mask.SelectedControl);
            MenuCommand mc =
              contextMenu.TrackPopup(c.PointToScreen(new Point(activeMouseEvent.X, activeMouseEvent.Y)), false);
            OnMenuSelection(mc);
          }
        }
        activeMouseEvent = null;
      }
    }

    public void OnControlMouseDown(object sender, MouseEventArgs e)
    {
      activeMouseEvent = new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta);
    }

    public void OnDragEnter(object sender, DragEventArgs e)
    {
      if (e.AllowedEffect == DragDropEffects.Copy)
      {
        e.Effect = DragDropEffects.Copy;
      }
    }

    public void OnDragDrop(object sender, DragEventArgs e)
    {
      MpeLog.Debug("OnDragDrop()");
      if (sender == null || !(sender is MpeContainer))
      {
        MpeLog.Warn("Could not locate parent MpeContainer... Cancelling DragDrop operation.");
        return;
      }
      MpeContainer mpc = (MpeContainer) sender;
      if (e.Data.GetDataPresent(typeof(MpeControlType)))
      {
        MpeControlType type = (MpeControlType) e.Data.GetData(typeof(MpeControlType));
        MpeLog.Debug("DragDrop: " + type.ToString());
        MpeControl c = Parser.CreateControl(type);
        c.Id = ResourceList.GenerateUniqueId();
        c.Location = mpc.PointToClient(new Point(e.X, e.Y));
        mpc.Controls.Add(c);
        c.BringToFront();
        Mask.SelectedControl = c;
        UpdatePropertyManager();
      }
      else if (e.Data.GetDataPresent(typeof(FileInfo)))
      {
        FileInfo image = (FileInfo) e.Data.GetData(typeof(FileInfo));
        MpeImage mpi = (MpeImage) Parser.CreateControl(MpeControlType.Image);
        mpi.Id = ResourceList.GenerateUniqueId();
        mpc.Controls.Add(mpi);
        mpi.Texture = image;
        mpi.AutoSize = true;
        mpi.Location = mpc.PointToClient(new Point(e.X, e.Y));
        mpi.BringToFront();
        Mask.SelectedControl = mpi;
        UpdatePropertyManager();
      }
      else
      {
        MpeLog.Debug("Unknown DataType... Cancelling DragDrop");
        return;
      }
      Focus();
    }

    public void OnMouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        if (mask == null || mask.SelectedControl == null)
        {
          return;
        }
        Point p = mask.ConvertPoint((Control) sender, e.X, e.Y);
        if (mask.MoveDrag)
        {
          mask.DragPoint = new Point(p.X - mask.SelectedControl.Left, p.Y - mask.SelectedControl.Top);
          mask.Ghost.Size = mask.SelectedControl.Size;
          mask.Ghost.Location = mask.SelectedControl.Parent.PointToScreen(mask.SelectedControl.Location);
          mask.MoveDragStart = true;
          return;
        }
        if (mask.ResizeDrag)
        {
          switch (mask.ResizeNodeIndex)
          {
            case 0:
              mask.DragPoint.X = p.X - mask.SelectedControl.Left;
              mask.DragPoint.Y = p.Y - mask.SelectedControl.Top;
              break;
            case 1:
              mask.DragPoint.Y = p.Y - mask.SelectedControl.Top;
              break;
            case 2:
              mask.DragPoint.X = p.X - mask.SelectedControl.Left - mask.SelectedControl.Width;
              mask.DragPoint.Y = p.Y - mask.SelectedControl.Top;
              break;
            case 3:
              mask.DragPoint.X = p.X - mask.SelectedControl.Left;
              break;
            case 4:
              mask.DragPoint.X = p.X - mask.SelectedControl.Left - mask.SelectedControl.Width;
              break;
            case 5:
              mask.DragPoint.X = p.X - mask.SelectedControl.Left;
              mask.DragPoint.Y = p.Y - mask.SelectedControl.Top - mask.SelectedControl.Height;
              break;
            case 6:
              mask.DragPoint.Y = p.Y - mask.SelectedControl.Top - mask.SelectedControl.Height;
              break;
            case 7:
              mask.DragPoint.X = p.X - mask.SelectedControl.Left - mask.SelectedControl.Width;
              mask.DragPoint.Y = p.Y - mask.SelectedControl.Top - mask.SelectedControl.Height;
              break;
          }
        }
      }
    }

    public void OnMouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        if (mask == null || mask.SelectedControl == null)
        {
          return;
        }
        if (mask.MoveDrag)
        {
          if (mask.MoveDragStart == false)
          {
            ControlPaint.DrawReversibleFrame(mask.Ghost, Color.Transparent, FrameStyle.Thick);
          }
          mask.SelectedControl.Location = mask.SelectedControl.Parent.PointToClient(mask.Ghost.Location);
          StatusBar.LocationStatus();
          mask.MoveDrag = false;
          UpdatePropertyManager();
          Cursor = Cursors.Default;
        }
        else if (mask.ResizeDrag)
        {
          StatusBar.LocationStatus();
          StatusBar.SizeStatus();
          mask.Show();
          mask.ResizeDrag = false;
          UpdatePropertyManager();
          Cursor = Cursors.Default;
        }
      }
    }

    public void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        if (mask == null || mask.SelectedControl == null)
        {
          return;
        }
        Point p = mask.ConvertPoint((Control) sender, e.X, e.Y);
        if (mask.MoveDrag)
        {
          if (Cursor != Cursors.SizeAll)
          {
            Cursor = Cursors.SizeAll;
          }
          if (mask.MoveDragStart)
          {
            mask.MoveDragStart = false;
          }
          else
          {
            ControlPaint.DrawReversibleFrame(mask.Ghost, Color.Transparent, FrameStyle.Thick);
          }
          p.X = p.X - mask.DragPoint.X;
          p.Y = p.Y - mask.DragPoint.Y;
          p.X = p.X < 0 ? 0 : p.X;
          p.Y = p.Y < 0 ? 0 : p.Y;
          p.X = p.X + mask.SelectedControl.Width > mask.SelectedControl.Parent.Width
                  ? mask.SelectedControl.Parent.Width - mask.SelectedControl.Width
                  : p.X;
          p.Y = p.Y + mask.SelectedControl.Height > mask.SelectedControl.Parent.Height
                  ? mask.SelectedControl.Parent.Height - mask.SelectedControl.Height
                  : p.Y;
          p.X = mask.SnapToGrid ? p.X - (p.X%mask.GridSize.Width) : p.X;
          p.Y = mask.SnapToGrid ? p.Y - (p.Y%mask.GridSize.Height) : p.Y;
          mask.Ghost.Location = mask.SelectedControl.Parent.PointToScreen(p);
          StatusBar.LocationStatus(p);
          ControlPaint.DrawReversibleFrame(mask.Ghost, Color.Transparent, FrameStyle.Thick);
        }
        else if (mask.ResizeDrag)
        {
          int x, y, w, h;
          mask.Hide();
          switch (mask.ResizeNodeIndex)
          {
            case 0:
              x = p.X - mask.DragPoint.X;
              x = mask.SnapToGrid ? x - (x%mask.GridSize.Width) : x;
              w = mask.SelectedControl.Right - x;
              if (w >= mask.MinControlSize.Width)
              {
                mask.SelectedControl.Width = w;
                mask.SelectedControl.Left = x;
              }
              y = p.Y - mask.DragPoint.Y;
              y = mask.SnapToGrid ? y - (y%mask.GridSize.Height) : y;
              h = mask.SelectedControl.Bottom - y;
              if (h >= mask.MinControlSize.Height)
              {
                mask.SelectedControl.Height = h;
                mask.SelectedControl.Top = y;
              }
              if (Cursor != Cursors.SizeNWSE)
              {
                Cursor = Cursors.SizeNWSE;
              }
              StatusBar.LocationStatus(mask.SelectedControl.Location);
              StatusBar.SizeStatus(mask.SelectedControl.Size);
              break;
            case 1:
              y = p.Y - mask.DragPoint.Y;
              y = mask.SnapToGrid ? y - (y%mask.GridSize.Height) : y;
              h = mask.SelectedControl.Bottom - y;
              if (h >= mask.MinControlSize.Height)
              {
                mask.SelectedControl.Height = h;
                mask.SelectedControl.Top = y;
              }
              if (Cursor != Cursors.SizeNS)
              {
                Cursor = Cursors.SizeNS;
              }
              StatusBar.LocationStatus(mask.SelectedControl.Location);
              StatusBar.SizeStatus(mask.SelectedControl.Size);
              break;
            case 2:
              w = p.X - mask.DragPoint.X - mask.SelectedControl.Left;
              w = mask.SnapToGrid ? w - (w%mask.GridSize.Width) : w;
              if (w >= mask.MinControlSize.Width)
              {
                mask.SelectedControl.Width = w;
              }
              y = p.Y - mask.DragPoint.Y;
              y = mask.SnapToGrid ? y - (y%mask.GridSize.Height) : y;
              h = mask.SelectedControl.Bottom - y;
              if (h >= mask.MinControlSize.Height)
              {
                mask.SelectedControl.Height = h;
                mask.SelectedControl.Top = y;
              }
              if (Cursor != Cursors.SizeNESW)
              {
                Cursor = Cursors.SizeNESW;
              }
              StatusBar.LocationStatus(mask.SelectedControl.Location);
              StatusBar.SizeStatus(mask.SelectedControl.Size);
              break;
            case 3:
              x = p.X - mask.DragPoint.X;
              x = mask.SnapToGrid ? x - (x%mask.GridSize.Width) : x;
              w = mask.SelectedControl.Right - x;
              if (w >= mask.MinControlSize.Width)
              {
                mask.SelectedControl.Width = w;
                mask.SelectedControl.Left = x;
              }
              if (Cursor != Cursors.SizeWE)
              {
                Cursor = Cursors.SizeWE;
              }
              StatusBar.LocationStatus(mask.SelectedControl.Location);
              StatusBar.SizeStatus(mask.SelectedControl.Size);
              break;
            case 4:
              w = p.X - mask.DragPoint.X - mask.SelectedControl.Left;
              w = mask.SnapToGrid ? w - (w%mask.GridSize.Width) : w;
              if (w >= mask.MinControlSize.Width)
              {
                mask.SelectedControl.Width = w;
              }
              if (Cursor != Cursors.SizeWE)
              {
                Cursor = Cursors.SizeWE;
              }
              StatusBar.SizeStatus(mask.SelectedControl.Size);
              break;
            case 5:
              x = p.X - mask.DragPoint.X;
              x = mask.SnapToGrid ? x - (x%mask.GridSize.Width) : x;
              w = mask.SelectedControl.Right - x;
              if (w >= mask.MinControlSize.Width)
              {
                mask.SelectedControl.Width = w;
                mask.SelectedControl.Left = x;
              }
              h = p.Y - mask.DragPoint.Y - mask.SelectedControl.Top;
              h = mask.SnapToGrid ? h - (h%mask.GridSize.Height) : h;
              if (h >= mask.MinControlSize.Height)
              {
                mask.SelectedControl.Height = h;
              }
              if (Cursor != Cursors.SizeNESW)
              {
                Cursor = Cursors.SizeNESW;
              }
              StatusBar.LocationStatus(mask.SelectedControl.Location);
              StatusBar.SizeStatus(mask.SelectedControl.Size);
              break;
            case 6:
              h = p.Y - mask.DragPoint.Y - mask.SelectedControl.Top;
              h = mask.SnapToGrid ? h - (h%mask.GridSize.Height) : h;
              if (h >= mask.MinControlSize.Height)
              {
                mask.SelectedControl.Height = h;
              }
              if (Cursor != Cursors.SizeNS)
              {
                Cursor = Cursors.SizeNS;
              }
              StatusBar.SizeStatus(mask.SelectedControl.Size);
              break;
            case 7:
              w = p.X - mask.DragPoint.X - mask.SelectedControl.Left;
              w = mask.SnapToGrid ? w - (w%mask.GridSize.Width) : w;
              if (w >= mask.MinControlSize.Width)
              {
                mask.SelectedControl.Width = w;
              }
              h = p.Y - mask.DragPoint.Y - mask.SelectedControl.Top;
              h = mask.SnapToGrid ? h - (h%mask.GridSize.Height) : h;
              if (h >= mask.MinControlSize.Height)
              {
                mask.SelectedControl.Height = h;
              }
              if (Cursor != Cursors.SizeNWSE)
              {
                Cursor = Cursors.SizeNWSE;
              }
              StatusBar.SizeStatus(mask.SelectedControl.Size);
              break;
          }
        }
      }
    }

    public void OnMouseWheel(object sender, MouseEventArgs e)
    {
      if (e.Delta > 0)
      {
        mask.SelectNextControl();
        UpdatePropertyManager();
      }
      else if (e.Delta < 0)
      {
        mask.SelectPreviousControl();
        UpdatePropertyManager();
      }
    }

    public void OnKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Escape:
          mask.SelectedControl = null;
          UpdatePropertyManager();
          break;
        case Keys.Delete:
          MenuCommand c = new MenuCommand("Delete");
          OnMenuSelection(c);
          break;
      }
      mask.Show();
      UpdatePropertyManager();
    }

    public void OnMenuSelection(MenuCommand command)
    {
      if (command == null || command.Enabled == false || mask.SelectedControl == null)
      {
        return;
      }
      switch (command.Text)
      {
        case "Parent":
          mask.SelectedControl = mask.SelectedControl.MpeParent;
          UpdatePropertyManager();
          break;
        case "Send To Back":
          mask.SelectedControl.SendToBack();
          break;
        case "Bring To Front":
          mask.SelectedControl.BringToFront();
          break;
        case "Cut":
          CutControl();
          break;
        case "Copy":
          CopyControl();
          break;
        case "Paste":
          PasteControl();
          break;
        case "Delete":
          if (AllowDeletions)
          {
            if (mask.SelectedControl.MpeParent != null)
            {
              try
              {
                MpeControl control = mask.SelectedControl;
                MpeContainer parent = control.MpeParent;
                parent.Controls.Remove(control);
                mask.SelectedControl = null;
                UpdatePropertyManager();
              }
              catch (Exception ee)
              {
                MpeLog.Debug(ee);
                MpeLog.Error(ee);
              }
            }
          }
          break;
        case "Properties":
          mpe.FocusPropertyManager();
          break;
        case "Test":
          //mask2.SelectedControl = mask.SelectedControl;
          break;
      }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      if (mask.SelectedControl == null)
      {
        return false;
      }
      if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
      {
        MpeContainer parent = mask.SelectedControl.MpeParent;
        switch (keyData)
        {
          case Keys.Right:
            if (parent.LayoutStyle == MpeLayoutStyle.Grid)
            {
              if (mask.LocationLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Left += (mask.GridSize.Width - (mask.SelectedControl.Left%mask.GridSize.Width));
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is LocationLocked and cannot be moved using the keyboard or mouse");
              }
            }
            else if (parent.LayoutStyle == MpeLayoutStyle.HorizontalFlow)
            {
              mask.SelectedControl.SendBack();
            }
            break;
          case Keys.Right | Keys.Shift:
            if (parent.LayoutStyle == MpeLayoutStyle.Grid)
            {
              if (mask.LocationLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Left ++;
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is LocationLocked and cannot be moved using the keyboard or mouse");
              }
            }
            break;
          case Keys.Right | Keys.Control:
            if (parent.Spring == false)
            {
              if (mask.SizeLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Width += (mask.GridSize.Width - (mask.SelectedControl.Width%mask.GridSize.Width));
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is SizeLocked and cannot be resized using the keyboard or mouse");
              }
            }
            break;
          case Keys.Right | Keys.Control | Keys.Shift:
            if (parent.Spring == false)
            {
              if (mask.SizeLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Width ++;
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is SizeLocked and cannot be resized using the keyboard or mouse");
              }
            }
            break;
          case Keys.Left:
            if (parent.LayoutStyle == MpeLayoutStyle.Grid)
            {
              if (mask.LocationLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Left -= (mask.GridSize.Width - (mask.SelectedControl.Left%mask.GridSize.Width));
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is LocationLocked and cannot be moved using the keyboard or mouse");
              }
            }
            else if (parent.LayoutStyle == MpeLayoutStyle.HorizontalFlow)
            {
              mask.SelectedControl.BringForward();
            }
            break;
          case Keys.Left | Keys.Shift:
            if (parent.LayoutStyle == MpeLayoutStyle.Grid)
            {
              if (mask.LocationLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Left --;
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is LocationLocked and cannot be moved using the keyboard or mouse");
              }
            }
            break;
          case Keys.Left | Keys.Control:
            if (parent.Spring == false)
            {
              if (mask.SizeLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Width -= (mask.GridSize.Width - (mask.SelectedControl.Width%mask.GridSize.Width));
                if (mask.SelectedControl.Width < mask.MinControlSize.Width)
                {
                  mask.SelectedControl.Width = mask.MinControlSize.Width;
                }
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is SizeLocked and cannot be resized using the keyboard or mouse");
              }
            }
            break;
          case Keys.Left | Keys.Control | Keys.Shift:
            if (parent.Spring == false)
            {
              if (mask.SizeLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Width --;
                if (mask.SelectedControl.Width < mask.MinControlSize.Width)
                {
                  mask.SelectedControl.Width = mask.MinControlSize.Width;
                }
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is SizeLocked and cannot be resized using the keyboard or mouse");
              }
            }
            break;
          case Keys.Up:
            if (parent.LayoutStyle == MpeLayoutStyle.Grid)
            {
              if (mask.LocationLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Top -= (mask.GridSize.Height - (mask.SelectedControl.Top%mask.GridSize.Height));
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is LocationLocked and cannot be moved using the keyboard or mouse");
              }
            }
            else if (parent.LayoutStyle == MpeLayoutStyle.VerticalFlow)
            {
              mask.SelectedControl.BringForward();
            }
            break;
          case Keys.Up | Keys.Shift:
            if (parent.LayoutStyle == MpeLayoutStyle.Grid)
            {
              if (mask.LocationLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Top--;
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is LocationLocked and cannot be moved using the keyboard or mouse");
              }
            }
            break;
          case Keys.Up | Keys.Control:
            if (parent.Spring == false)
            {
              if (mask.SizeLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Height -= (mask.GridSize.Height -
                                                (mask.SelectedControl.Height%mask.GridSize.Height));
                if (mask.SelectedControl.Height < mask.MinControlSize.Height)
                {
                  mask.SelectedControl.Height = mask.MinControlSize.Height;
                }
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is SizeLocked and cannot be resized using the keyboard or mouse");
              }
            }
            break;
          case Keys.Up | Keys.Control | Keys.Shift:
            if (parent.Spring == false)
            {
              if (mask.SizeLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Height --;
                if (mask.SelectedControl.Height < mask.MinControlSize.Height)
                {
                  mask.SelectedControl.Height = mask.MinControlSize.Height;
                }
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is SizeLocked and cannot be resized using the keyboard or mouse");
              }
            }
            break;
          case Keys.Down:
            if (parent.LayoutStyle == MpeLayoutStyle.Grid)
            {
              if (mask.LocationLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Top += (mask.GridSize.Height - (mask.SelectedControl.Top%mask.GridSize.Height));
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is LocationLocked and cannot be moved using the keyboard or mouse");
              }
            }
            else if (parent.LayoutStyle == MpeLayoutStyle.VerticalFlow)
            {
              mask.SelectedControl.SendBack();
            }
            break;
          case Keys.Down | Keys.Shift:
            if (parent.LayoutStyle == MpeLayoutStyle.Grid)
            {
              if (mask.LocationLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Top++;
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is LocationLocked and cannot be moved using the keyboard or mouse");
              }
            }
            break;
          case Keys.Down | Keys.Control:
            if (parent.Spring == false)
            {
              if (mask.SizeLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Height += (mask.GridSize.Height -
                                                (mask.SelectedControl.Height%mask.GridSize.Height));
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is SizeLocked and cannot be resized using the keyboard or mouse");
              }
            }
            break;
          case Keys.Down | Keys.Control | Keys.Shift:
            if (parent.Spring == false)
            {
              if (mask.SizeLocked == false)
              {
                mask.Hide();
                mask.SelectedControl.Height ++;
              }
              else
              {
                mpe.StatusBar.Warn(
                  "The selected control is SizeLocked and cannot be resized using the keyboard or mouse");
              }
            }
            break;
        }
      }
      return true;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
      OnMouseWheel(this, e);
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
      ResourceManager resources = new ResourceManager(typeof(MpeResourceDesigner));
      menuImageList = new ImageList(components);
      // 
      // menuImageList
      // 
      menuImageList.ColorDepth = ColorDepth.Depth24Bit;
      menuImageList.ImageSize = new Size(16, 16);
      menuImageList.ImageStream = ((ImageListStreamer) (resources.GetObject("menuImageList.ImageStream")));
      menuImageList.TransparentColor = Color.Magenta;
      // 
      // MpeResourceDesigner
      // 
      AutoScroll = true;
      BackColor = SystemColors.Control;
      Name = "MpeResourceDesigner";
      Size = new Size(320, 240);
      MouseUp += new MouseEventHandler(OnMouseUp);
      KeyUp += new KeyEventHandler(OnKeyUp);
      MouseMove += new MouseEventHandler(OnMouseMove);
      MouseWheel += new MouseEventHandler(OnMouseWheel);
      MouseDown += new MouseEventHandler(OnMouseDown);
      ControlAdded += new ControlEventHandler(OnControlAdded);
    }

    #endregion
  }
}