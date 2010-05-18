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
using System.Drawing;
using System.Windows.Forms;
using Mpe.Controls;
using Mpe.Designers.Mask;

namespace Mpe.Designers
{
  /// <summary>
  /// Summary description for MpeControlMask.
  /// </summary>
  public class MpeControlMask
  {
    #region Variables

    private int nodeSize;
    private Pen nodePen;
    private Brush nodeBrush;
    private Brush disabledNodeBrush;
    private Brush barBrush;
    private MpeControl control;
    private MaskComponent[] components;

    private MpeResourceDesigner designer;
    private bool visible;

    public bool MoveDrag;
    public bool MoveDragStart;
    public bool ResizeDrag;
    public int ResizeNodeIndex;
    public Point DragPoint;
    public Rectangle Ghost;

    private Size defaultMinControlSize;
    private Size defaultGridSize;

    #endregion

    #region Constructors

    public MpeControlMask(MpeResourceDesigner designer)
    {
      this.designer = designer;
      // Initialize Nodes
      nodeSize = 6;
      nodePen = new Pen(Color.Black, -1.0f);
      nodeBrush = new SolidBrush(Color.FromArgb(0, 255, 0));
      //disabledNodeBrush = new SolidBrush(Color.FromArgb(128,32,32,32));
      disabledNodeBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control));

      // Initialize Bars
      barBrush = new SolidBrush(Color.FromArgb(96, 255, 255, 255));
      //barBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control));

      // Initialize Mask Components
      components = new MaskComponent[4];
      components[0] = new MaskTop(this);
      components[1] = new MaskRight(this);
      components[2] = new MaskBottom(this);
      components[3] = new MaskLeft(this);

      for (int i = 0; i < components.Length; i++)
      {
        components[i].MouseMove += new MouseEventHandler(designer.OnMouseMove);
        components[i].MouseDown += new MouseEventHandler(designer.OnMouseDown);
        components[i].MouseDown += new MouseEventHandler(designer.OnControlMouseDown);
        components[i].Click += new EventHandler(designer.OnControlClick);
        components[i].MouseUp += new MouseEventHandler(designer.OnMouseUp);
      }

      visible = true;
      Hide();

      defaultGridSize = new Size(8, 8);
      defaultMinControlSize = new Size(16, 16);

      Ghost = new Rectangle(0, 0, 0, 0);
      MoveDrag = false;
      MoveDragStart = false;
      ResizeDrag = false;
      ResizeNodeIndex = 0;
      DragPoint = new Point(0, 0);
    }

    #endregion

    #region Properties

    public MpeControl SelectedControl
    {
      get { return control; }
      set
      {
        if (value == null || (control != null && control.MpeParent != value.MpeParent))
        {
          if (control.MpeParent != null)
          {
            control.MpeParent.PaintGrid = false;
          }
          RemoveComponents();
        }
        if (control != null)
        {
          control.PropertyValueChanged -= new MpeControl.PropertyValueChangedHandler(OnPropertyValueChanged);
        }
        control = value;
        if (control != null)
        {
          if (control.MpeParent != null)
          {
            control.MpeParent.PaintGrid = true;
          }
          control.PropertyValueChanged += new MpeControl.PropertyValueChangedHandler(OnPropertyValueChanged);
          AddComponents();
          Update();
          if (visible == false)
          {
            Show();
          }
        }
      }
    }

    public Brush BarBrush
    {
      get { return barBrush; }
      set
      {
        if (value != null)
        {
          barBrush = value;
        }
      }
    }

    public Brush NodeBrush
    {
      get { return nodeBrush; }
      set
      {
        if (value != null)
        {
          nodeBrush = value;
        }
      }
    }

    public Brush DisabledNodeBrush
    {
      get { return disabledNodeBrush; }
      set
      {
        if (value != null)
        {
          disabledNodeBrush = value;
        }
      }
    }

    public Pen NodePen
    {
      get { return nodePen; }
      set
      {
        if (value != null)
        {
          nodePen = value;
        }
      }
    }

    public int NodeSize
    {
      get { return nodeSize; }
      set
      {
        if (value > 0)
        {
          nodeSize = value;
        }
      }
    }

    public bool SnapToGrid
    {
      get
      {
        if (control != null && control.MpeParent != null)
        {
          return control.MpeParent.SnapToGrid;
        }
        return false;
      }
    }

    public Size GridSize
    {
      get
      {
        if (control != null && control.MpeParent != null)
        {
          return control.MpeParent.GridSize;
        }
        return defaultGridSize;
      }
    }

    public Size MinControlSize
    {
      get { return defaultMinControlSize; }
    }

    public bool LocationLocked
    {
      get
      {
        if (control != null)
        {
          return control.Locked.Location;
        }
        return false;
      }
    }

    public bool SizeLocked
    {
      get
      {
        if (control != null)
        {
          return control.Locked.Size;
        }
        return false;
      }
    }

    #endregion

    #region Methods

    protected void AddComponents()
    {
      if (control != null && control.Parent != null)
      {
        for (int i = 0; i < components.Length; i++)
        {
          if (control.Parent.Controls.Contains(components[i]) == false)
          {
            control.Parent.Controls.Add(components[i]);
            components[i].BringToFront();
          }
        }
      }
    }

    protected void RemoveComponents()
    {
      //if (control != null && control.Parent != null) 
      for (int i = 0; i < components.Length; i++)
      {
        //control.Parent.Controls.Remove(components[i]);
        components[i].Parent.Controls.Remove(components[i]);
      }
    }

    protected void Update()
    {
      if (control != null)
      {
        if (visible)
        {
          Hide();
          Initialize();
          Show();
        }
        else
        {
          Initialize();
        }
      }
      else
      {
        Hide();
      }
    }

    protected void Initialize()
    {
      for (int i = 0; i < components.Length; i++)
      {
        components[i].Initialize();
      }
    }

    public void Hide()
    {
      if (visible)
      {
        visible = false;
        for (int i = 0; i < components.Length; i++)
        {
          components[i].Hide();
        }
      }
    }

    public void Show()
    {
      if (visible == false)
      {
        visible = true;
        for (int i = 0; i < components.Length; i++)
        {
          components[i].Show();
        }
      }
    }

    public void SelectNextControl()
    {
      if (control == null || control.MpeParent == null)
      {
        return;
      }
      bool searching = true;
      Control c = control;
      while (searching)
      {
        c = control.Parent.GetNextControl(c, true);
        if (c is MpeControl)
        {
          SelectedControl = (MpeControl) c;
          searching = false;
        }
      }
    }

    public void SelectPreviousControl()
    {
      if (control == null || control.MpeParent == null)
      {
        return;
      }
      bool searching = true;
      Control c = control;
      while (searching)
      {
        c = control.Parent.GetNextControl(c, false);
        if (c is MpeControl)
        {
          SelectedControl = (MpeControl) c;
          searching = false;
        }
      }
    }

    public Point ConvertPoint(Control c, int x, int y)
    {
      if (SelectedControl != null && SelectedControl.Parent != null)
      {
        return SelectedControl.Parent.PointToClient(c.PointToScreen(new Point(x, y)));
      }
      return new Point(x, y);
    }

    #endregion

    #region Event Handlers

    public void OnPropertyValueChanged(MpeControl sender, string property)
    {
      if (sender != null && sender == SelectedControl)
      {
        if (property != null &&
            (property.Equals("Location") || property.Equals("Size") || property.Equals("LocationLocked") ||
             property.Equals("SizeLocked")))
        {
          Update();
        }
      }
    }

    #endregion
  }
}