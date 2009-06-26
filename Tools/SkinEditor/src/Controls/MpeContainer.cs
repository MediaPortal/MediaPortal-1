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
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using Mpe.Controls.Design;
using Mpe.Controls.Properties;

namespace Mpe.Controls
{
  /// <summary>
  /// Summary description for MpeContainer.
  /// </summary>
  public abstract class MpeContainer : MpeControl
  {
    #region Variables

    protected bool showGrid;
    protected bool showBorder;
    protected bool paintGrid;
    protected Size gridSize;
    protected bool snapToGrid;
    protected MpeLayoutStyle layoutStyle;
    protected int spacing;
    protected bool spring;
    protected MpeImage backImage;

    #endregion

    #region Constructors

    public MpeContainer() : base()
    {
      MpeLog.Debug("MpeContainer()");
      backImage = new MpeImage();
      backImage.Embedded = true;
      backImage.AutoSize = false;
      backImage.Id = 1;
      layoutStyle = MpeLayoutStyle.Grid;
      gridSize = new Size(8, 8);
      showGrid = true;
      showBorder = true;
      snapToGrid = true;
      spacing = 0;
      spring = false;
    }

    public MpeContainer(MpeContainer container) : base(container)
    {
      MpeLog.Debug("MpeContainer(container)");
      backImage = new MpeImage(container.backImage);
      layoutStyle = container.layoutStyle;
      gridSize = container.gridSize;
      showGrid = container.showGrid;
      showBorder = container.showBorder;
      snapToGrid = container.snapToGrid;
      spring = container.spring;
      spacing = container.spacing;
      for (int i = 0; i < container.Controls.Count; i++)
      {
        if (container.Controls[i] is MpeControl)
        {
          MpeControl c = (MpeControl) container.Controls[i];
          if (c.Embedded == false)
          {
            Controls.Add(c.Copy());
          }
        }
      }
    }

    #endregion

    #region Properties

    public override sealed bool Masked
    {
      get { return base.Masked; }
      set
      {
        if (base.Masked != value)
        {
          for (int i = 0; i < Controls.Count; i++)
          {
            if (Controls[i] is MpeControl)
            {
              MpeControl c = (MpeControl) Controls[i];
              if (c.Embedded)
              {
                c.Masked = value;
              }
            }
          }
          base.Masked = value;
        }
      }
    }

    public override sealed MpeScreen MpeScreen
    {
      get { return base.MpeScreen; }
      set
      {
        if (base.MpeScreen != value)
        {
          for (int i = 0; i < Controls.Count; i++)
          {
            if (Controls[i] is MpeControl)
            {
              MpeControl c = (MpeControl) Controls[i];
              if (c.Embedded)
              {
                c.MpeScreen = value;
              }
            }
          }
          base.MpeScreen = value;
        }
      }
    }

    [Category("Layout")]
    [RefreshPropertiesAttribute(RefreshProperties.All)]
    [Description("Determines how controls will be positioned inside the container.")]
    public virtual MpeLayoutStyle LayoutStyle
    {
      get { return layoutStyle; }
      set
      {
        if (layoutStyle != value)
        {
          layoutStyle = value;
          if (value == MpeLayoutStyle.Grid || Spring)
          {
            AutoSize = false;
          }
          else
          {
            AutoSize = true;
          }
          Prepare();
          Modified = true;
          FirePropertyValueChanged("LayoutStyle");
        }
      }
    }

    [Category("Textures")]
    [Browsable(true)]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    public virtual FileInfo TextureBack
    {
      get { return backImage.Texture; }
      set
      {
        backImage.Texture = value;
        Modified = true;
        Invalidate(true);
      }
    }

    [Browsable(false)]
    public virtual MpeImage TextureBackImage
    {
      get { return backImage; }
    }

    [Browsable(true)]
    [ReadOnly(false)]
    public override bool AutoSize
    {
      get { return autoSize; }
      set
      {
        if (autoSize != value)
        {
          if (LayoutStyle != MpeLayoutStyle.Grid && Spring == false)
          {
            MpeLog.Warn("AutoSize must be set to true when using a flow layout.");
            autoSize = true;
            controlLock.Size = true;
            return;
          }
          autoSize = value;
          controlLock.Size = value;
          if (value && TextureBack != null)
          {
            Size = TextureBackImage.TextureImage.Size;
            Prepare();
          }
          MpeLog.Info("AutoSize enabled");
        }
      }
    }

    [Category("Layout")]
    [RefreshPropertiesAttribute(RefreshProperties.All)]
    public virtual bool Spring
    {
      get { return spring; }
      set
      {
        if (spring != value)
        {
          if (LayoutStyle != MpeLayoutStyle.Grid)
          {
            spring = value;
            AutoSize = !value;
            Modified = true;
            Prepare();
            FirePropertyValueChanged("Spring");
          }
          else
          {
            MpeLog.Warn("You can only enable the Spring property when using a Flow Layout.");
            spring = false;
            return;
          }
        }
      }
    }

    [Category("Designer")]
    public virtual bool ShowBorder
    {
      get { return showBorder; }
      set
      {
        if (showBorder != value)
        {
          showBorder = value;
          Invalidate(true);
        }
      }
    }

    [Category("Layout")]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public virtual int Spacing
    {
      get { return spacing; }
      set
      {
        if (spacing != value)
        {
          if (value < 0)
          {
            MpeLog.Warn("Spacing value must be greater than 0.");
            return;
          }
          spacing = value;
          Prepare();
          Modified = true;
          FirePropertyValueChanged("Spacing");
        }
      }
    }

    [ReadOnly(false)]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public override MpeControlPadding Padding
    {
      get { return base.Padding; }
      set
      {
        if (value != null)
        {
          base.Padding.Top = value.Top;
          base.Padding.Bottom = value.Bottom;
          base.Padding.Left = value.Left;
          base.Padding.Right = value.Right;
        }
      }
    }

    [Category("Designer")]
    public virtual bool ShowGrid
    {
      get { return showGrid; }
      set { showGrid = value; }
    }

    [Browsable(false)]
    public bool PaintGrid
    {
      get { return paintGrid; }
      set
      {
        if (showGrid && value)
        {
          paintGrid = true;
        }
        else
        {
          paintGrid = false;
        }
        Invalidate(false);
      }
    }

    [Category("Designer")]
    [Description("Sets the width and height of the grid.")]
    public virtual Size GridSize
    {
      get { return gridSize; }
      set
      {
        gridSize = value;
        Invalidate(false);
      }
    }

    [Category("Designer")]
    [DefaultValue(true)]
    [Description("If true, this container's controls will snap to the grid when moved and resized.")]
    public virtual bool SnapToGrid
    {
      get { return snapToGrid; }
      set { snapToGrid = value; }
    }

    #endregion

    #region Properties - Hidden

    [Browsable(false)]
    public override bool Enabled
    {
      get { return base.Enabled; }
      set { base.Enabled = value; }
    }

    [Browsable(false)]
    public override bool Focused
    {
      get { return base.Focused; }
      set { base.Focused = value; }
    }

    [Browsable(false)]
    public override int OnLeft
    {
      get { return base.OnLeft; }
      set { base.OnLeft = value; }
    }

    [Browsable(false)]
    public override int OnRight
    {
      get { return base.OnRight; }
      set { base.OnRight = value; }
    }

    [Browsable(false)]
    public override int OnUp
    {
      get { return base.OnUp; }
      set { base.OnUp = value; }
    }

    [Browsable(false)]
    public override int OnDown
    {
      get { return base.OnDown; }
      set { base.OnDown = value; }
    }

    [Browsable(false)]
    protected int MpeControlCount
    {
      get
      {
        int c = 0;
        for (int i = 0; i < Controls.Count; i++)
        {
          if (Controls[i] is MpeControl)
          {
            c++;
          }
        }
        return c;
      }
    }

    #endregion

    #region Methods

    public override void Destroy()
    {
      base.Destroy();
      for (int i = 0; i < Controls.Count; i++)
      {
        if (Controls[i] is MpeControl)
        {
          MpeControl c = (MpeControl) Controls[i];
          c.Destroy();
        }
      }
    }

    protected override void PrepareControl()
    {
      MpeLog.Debug("MpeContainer.Prepare(" + Type.ToString() + ")");
      if (LayoutStyle == MpeLayoutStyle.VerticalFlow)
      {
        if (Spring)
        {
          int w = Width - Padding.Width;
          int count = MpeControlCount;
          int h = 64;
          if (count != 0)
          {
            h = Height - Padding.Height - (count)*Spacing;
            h = h/count;
          }
          int y = Padding.Top;
          for (int i = 0; i < Controls.Count; i++)
          {
            if (Controls[i] is MpeControl)
            {
              MpeControl c = (MpeControl) Controls[i];
              c.Left = Padding.Left;
              c.Top = y;
              c.Width = w;
              c.Height = h;
              y += h + Spacing;
              if (c.Locked.Location == false)
              {
                c.Locked.Location = true;
              }
              if (c.Locked.Size == false)
              {
                c.Locked.Size = true;
              }
            }
          }
        }
        else
        {
          int x = Padding.Left;
          int y = Padding.Top;
          int w = x;
          int h = y;
          int cc = 0;
          for (int i = 0; i < Controls.Count; i++)
          {
            if (Controls[i] is MpeControl)
            {
              MpeControl c = (MpeControl) Controls[i];
              c.Left = x;
              c.Top = y;
              int cw = x + c.Width;
              if (w < cw)
              {
                w = cw;
              }
              y += c.Height + Spacing;
              h += c.Height + Spacing;
              cc++;
              if (c.Locked.Location == false)
              {
                c.Locked.Location = true;
              }
              if (c.Locked.Size && c.AutoSize == false)
              {
                c.Locked.Size = false;
              }
            }
          }
          if (cc > 0)
          {
            h -= Spacing;
            w += Padding.Right;
            h += Padding.Bottom;
            Width = w;
            Height = h;
          }
          else
          {
            Size = new Size(64, 64);
          }
        }
      }
      else if (LayoutStyle == MpeLayoutStyle.HorizontalFlow)
      {
        if (Spring)
        {
          int h = Height - Padding.Top - Padding.Bottom;
          int count = MpeControlCount;
          int w = 64;
          if (count != 0)
          {
            w = Width - Padding.Left - Padding.Right - (count)*Spacing;
            w = w/count;
          }
          int x = Padding.Left;
          for (int i = 0; i < Controls.Count; i++)
          {
            if (Controls[i] is MpeControl)
            {
              MpeControl c = (MpeControl) Controls[i];
              c.Left = x;
              c.Top = Padding.Top;
              c.Width = w;
              c.Height = h;
              x += w + Spacing;
              if (c.Locked.Location == false)
              {
                c.Locked.Location = true;
              }
              if (c.Locked.Size == false)
              {
                c.Locked.Size = true;
              }
            }
          }
        }
        else
        {
          int x = Padding.Left;
          int y = Padding.Top;
          int w = x;
          int h = y;
          int cc = 0;
          for (int i = 0; i < Controls.Count; i++)
          {
            if (Controls[i] is MpeControl)
            {
              MpeControl c = (MpeControl) Controls[i];
              c.Left = x;
              c.Top = y;
              int ch = y + c.Height;
              if (h < ch)
              {
                h = ch;
              }
              x += c.Width + Spacing;
              w += c.Width + Spacing;
              cc++;
              if (c.Locked.Location == false)
              {
                c.Locked.Location = true;
              }
              if (c.Locked.Size && c.AutoSize == false)
              {
                c.Locked.Size = false;
              }
            }
          }
          if (cc > 0)
          {
            w -= Spacing;
            w += Padding.Right;
            h += Padding.Bottom;
            Width = w;
            Height = h;
          }
          else
          {
            Size = new Size(64, 64);
          }
        }
      }
      else
      {
        for (int i = 0; i < Controls.Count; i++)
        {
          if (Controls[i] is MpeControl)
          {
            MpeControl c = (MpeControl) Controls[i];
            if (c.Locked.Location)
            {
              c.Locked.Location = false;
            }
            if (c.Locked.Size && c.AutoSize == false)
            {
              c.Locked.Size = false;
            }
          }
        }
      }
      Invalidate(true);
    }

    #endregion

    #region Event Handlers

    protected virtual void OnEmbeddedClick(object sender, EventArgs e)
    {
      OnClick(e);
    }

    protected virtual void OnEmbeddedMouseDown(object sender, MouseEventArgs e)
    {
      OnMouseDown(e);
    }

    protected override sealed void OnControlAdded(ControlEventArgs e)
    {
      if (e.Control != null && e.Control is MpeControl)
      {
        MpeControl c = (MpeControl) e.Control;
        c.SizeChanged += new EventHandler(OnControlSizeChanged);
        if (c.Embedded)
        {
          c.Click += new EventHandler(OnEmbeddedClick);
          c.MouseDown += new MouseEventHandler(OnEmbeddedMouseDown);
        } //else {
        Prepare();
        //}
        Modified = true;
      }
      base.OnControlAdded(e);
    }

    protected override sealed void OnControlRemoved(ControlEventArgs e)
    {
      if (e.Control != null && e.Control is MpeControl)
      {
        MpeControl c = (MpeControl) e.Control;
        c.SizeChanged -= new EventHandler(OnControlSizeChanged);
        if (c.Embedded)
        {
          c.Click -= new EventHandler(OnEmbeddedClick);
          c.MouseDown -= new MouseEventHandler(OnEmbeddedMouseDown);
        } //else {
        Prepare();
        //}
        Modified = true;
      }
      base.OnControlRemoved(e);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
      base.OnPaintBackground(e);
      if (backImage != null && backImage.TextureImage != null)
      {
        e.Graphics.DrawImage(backImage.TextureImage, 0, 0, Width, Height);
      }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      if (PaintGrid)
      {
        ControlPaint.DrawGrid(e.Graphics, ClientRectangle, GridSize, Color.Transparent);
      }
    }

    private void OnControlSizeChanged(object sender, EventArgs e)
    {
      if (Spring == false)
      {
        Prepare();
      }
    }

    protected override void OnPaddingChanged()
    {
      if (LayoutStyle == MpeLayoutStyle.Grid)
      {
        MpeLog.Warn("Padding cannot be changed. The control belongs to a grid layout.");
        Padding = new MpeControlPadding(0);
      }
      else
      {
        Prepare();
      }
    }

    protected override void OnLocationChanged(EventArgs e)
    {
      if (backImage != null)
      {
        backImage.Location = Location;
      }
      base.OnLocationChanged(e);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      if (backImage != null)
      {
        backImage.Size = Size;
      }
      base.OnSizeChanged(e);
    }

    #endregion
  }
}