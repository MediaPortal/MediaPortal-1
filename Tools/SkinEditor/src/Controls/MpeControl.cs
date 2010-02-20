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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Xml;
using System.Xml.XPath;
using Mpe.Controls.Properties;
using Mpe.Controls.Design;

namespace Mpe.Controls
{
  /// <summary>
  /// Summary description for TestControl.
  /// </summary>
  public class MpeControl : Control, MpeResource
  {
    #region Variables

    private MpeControlType type;
    protected MpeScreen screen;
    protected MpeParser parser;
    protected int id;
    protected bool embedded;
    protected string description;
    protected bool focused;
    protected bool enabled;
    protected string visible;
    protected bool masked;
    private bool modified;
    protected Color diffuseColor;
    protected Color dimColor;
    protected bool autoSize;
    protected MpeControlAlignment alignment;
    protected MpeControlPadding padding;
    protected MpeControlLock controlLock;
    private MpeAnimationType animation;
    protected int onLeft;
    protected int onRight;
    protected int onUp;
    protected int onDown;
    protected SolidBrush textBrush;
    protected Pen borderPen;
    protected MpeTagCollection tags;
    private bool preparing;
    private bool reference;

    #endregion

    #region Events and Delegates

    public delegate void StatusChangedHandler(MpeControl sender, bool modified);


    public event StatusChangedHandler StatusChanged;


    public delegate void PropertyValueChangedHandler(MpeControl sender, string propertyName);


    public event PropertyValueChangedHandler PropertyValueChanged;


    public delegate void IdentityChangedHandler(MpeControl sender, IdentityEventArgs e);


    public event IdentityChangedHandler IdentityChanged;

    #endregion

    #region Constructors

    public MpeControl()
    {
      MpeLog.Debug("MpeControl()");
      preparing = true;
      SetStyle(ControlStyles.SupportsTransparentBackColor, true);
      SetStyle(ControlStyles.DoubleBuffer, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.ResizeRedraw, true);
      Animation = new MpeAnimationType();
      BackColor = Color.Transparent;
      Size = new Size(64, 64);
      Location = new Point(8, 8);
      alignment = MpeControlAlignment.Left;
      autoSize = false;
      borderPen = new Pen(Color.FromArgb(128, 255, 255, 255), 1.0f);
      borderPen.DashStyle = DashStyle.Dash;
      controlLock = new MpeControlLock();
      controlLock.LockChanged += new MpeControlLock.LockChangedHandler(OnLockChanged);
      description = "";
      diffuseColor = Color.FromArgb(255, 255, 255, 255);
      dimColor = Color.FromArgb(0x60ffffff);
      embedded = false;
      enabled = true;
      focused = false;
      id = 0;
      masked = false;
      modified = false;
      onLeft = 0;
      onRight = 0;
      onUp = 0;
      onDown = 0;
      padding = new MpeControlPadding(0);
      padding.PaddingChanged += new MpeControlPadding.PaddingChangedHandler(OnPaddingChanged);
      parser = null;
      textBrush = new SolidBrush(Color.Black);
      type = MpeControlType.Empty;
      visible = "true";
      screen = null;
      tags = new MpeTagCollection();
      tags.TagAdded += new MpeTagCollection.TagAddedHandler(OnTagCollectionChanged);
      tags.TagChanged += new MpeTagCollection.TagChangedHandler(OnTagCollectionChanged);
      tags.TagRemoved += new MpeTagCollection.TagRemovedHandler(OnTagCollectionChanged);
      preparing = false;
      reference = false;
    }

    public MpeControl(MpeControl control) : this()
    {
      MpeLog.Debug("MpeControl(control)");
      preparing = true;
      Size = control.Size;
      Location = control.Location;
      alignment = control.alignment;
      autoSize = control.autoSize;
      borderPen = control.borderPen;
      controlLock = new MpeControlLock(control.controlLock);
      controlLock.LockChanged += new MpeControlLock.LockChangedHandler(OnLockChanged);
      description = control.description;
      diffuseColor = control.diffuseColor;
      dimColor = control.dimColor;
      animation = control.animation;
      embedded = control.embedded;
      enabled = control.enabled;
      focused = control.focused;
      id = control.id;
      masked = control.masked;
      modified = control.modified;
      onLeft = control.onLeft;
      onRight = control.onRight;
      onUp = control.onUp;
      onDown = control.onDown;
      padding = new MpeControlPadding(control.padding);
      padding.PaddingChanged += new MpeControlPadding.PaddingChangedHandler(OnPaddingChanged);
      parser = control.parser;
      textBrush = (SolidBrush) control.textBrush.Clone();
      type = control.type;
      visible = control.visible;
      screen = control.screen;
      tags = new MpeTagCollection(control.tags);
      tags.TagAdded += new MpeTagCollection.TagAddedHandler(OnTagCollectionChanged);
      tags.TagChanged += new MpeTagCollection.TagChangedHandler(OnTagCollectionChanged);
      tags.TagRemoved += new MpeTagCollection.TagRemovedHandler(OnTagCollectionChanged);
      preparing = false;
    }

    #endregion

    #region Properties

    [Browsable(false)]
    public virtual MpeParser Parser
    {
      get { return parser; }
      set { parser = value; }
    }

    [Browsable(false)]
    public MpeContainer MpeParent
    {
      get
      {
        if (Parent != null && Parent is MpeContainer)
        {
          return (MpeContainer) Parent;
        }
        return null;
      }
    }

    [Browsable(false)]
    public virtual MpeScreen MpeScreen
    {
      get { return screen; }
      set { screen = value; }
    }

    [Browsable(false)]
    public Point AbsoluteLocation
    {
      get
      {
        if (MpeScreen != null && MpeParent != null)
        {
          Point wp = MpeScreen.PointToScreen(new Point(0, 0));
          Point cp = MpeParent.PointToScreen(Location);
          int x = cp.X - wp.X;
          int y = cp.Y - wp.Y;
          return new Point(x, y);
        }
        return Location;
      }
    }

    [Browsable(false)]
    public virtual bool Embedded
    {
      get { return embedded; }
      set { embedded = value; }
    }

    [Browsable(false)]
    public virtual bool IsReference
    {
      get { return reference; }
      set { reference = value; }
    }

    [Category("Control")]
    [Description("The type of control you want. See the list below for valid types of control")]
    [ReadOnly(true)]
    public virtual MpeControlType Type
    {
      get { return type; }
      set { type = value; }
    }

    [Category("Control")]
    [RefreshProperties(RefreshProperties.Repaint)]
    [Editor(typeof(MpeAnimationEditor), typeof(UITypeEditor))]
    public virtual MpeAnimationType Animation
    {
      get { return animation; }
      set
      {
        if (animation != value)
        {
          animation = value;
          Modified = true;
          FirePropertyValueChanged("Animation");
        }
      }
    }

    [Category("Control")]
    [Description("The id of the control. The id will couple the skin file to the code, so if we later on want to check that a user pressed a button, the id will be required and must be unique. For controls that will never be referenced in the code it is safe to set it to '1'")]
    [DefaultValue(0)]
    public virtual int Id
    {
      get { return id; }
      set
      {
        if (id != value)
        {
          if (IdentityChanged != null)
          {
            IdentityEventArgs e = new IdentityEventArgs(value, id);
            IdentityChanged(this, e);
            if (e.Cancel)
            {
              MpeLog.Warn(e.Message);
              return;
            }
          }
          id = value;
          Modified = true;
          FirePropertyValueChanged("Id");
        }
      }
    }

    [Category("Control")]
    [DefaultValue("")]
    public virtual string Description
    {
      get { return description; }
      set
      {
        if (description.Equals(value) == false)
        {
          description = value;
          Modified = true;
          FirePropertyValueChanged("Description");
        }
      }
    }

    [Browsable(false)]
    public virtual bool Masked
    {
      get { return masked; }
      set { masked = value; }
    }

    [Browsable(false)]
    [ReadOnly(true)]
    [Category("(Advanced)")]
    public bool Modified
    {
      get { return modified; }
      set
      {
        if (modified != value)
        {
          modified = value;
          FireStatusChanged();
        }
      }
    }

    [Category("Custom")]
    public virtual MpeTagCollection Tags
    {
      get { return tags; }
      set { tags = value; }
    }

    [Browsable(false)]
    public override bool AllowDrop
    {
      get { return base.AllowDrop; }
      set { base.AllowDrop = value; }
    }

    [Category("Designer")]
    public virtual MpeControlLock Locked
    {
      get { return controlLock; }
      set
      {
        if (value != null)
        {
          controlLock.Location = value.Location;
          controlLock.Size = value.Size;
        }
      }
    }

    [Category("Layout")]
    [ReadOnly(true)]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public virtual bool AutoSize
    {
      get { return autoSize; }
      set
      {
        if (autoSize != value)
        {
          autoSize = value;
          controlLock.Size = value;
          Prepare();
        }
      }
    }

    [Category("Layout")]
    [ReadOnly(true)]
    public virtual MpeControlPadding Padding
    {
      get { return padding; }
      set
      {
        if (padding.Equals(value) == false)
        {
          padding = value;
          Prepare();
          Modified = true;
          Invalidate(false);
          FirePropertyValueChanged("Padding");
        }
      }
    }

    [Category("Layout")]
    [ReadOnly(true)]
    public virtual MpeControlAlignment Alignment
    {
      get { return alignment; }
      set
      {
        if (alignment != value)
        {
          alignment = value;
          Prepare();
          Modified = true;
          Invalidate(false);
          FirePropertyValueChanged("Alignment");
        }
      }
    }

    [Category("Control")]
    [DefaultValue(false)]
    [Browsable(true)]
    public new virtual bool Focused
    {
      get { return focused; }
      set
      {
        if (focused != value)
        {
          focused = value;
          Invalidate(false);
        }
      }
    }

    [Category("Control")]
    [DefaultValue(true)]
    public new virtual bool Enabled
    {
      get { return enabled; }
      set
      {
        if (enabled != value)
        {
          enabled = value;
          Invalidate(false);
        }
      }
    }

    [Category("Control")]
    [DefaultValue(true)]
    public new virtual string Visible
    {
      get { return visible; }
      set
      {
        if (visible != value)
        {
          visible = value;
          Modified = true;
          FirePropertyValueChanged("Visible");
        }
      }
    }

    [Category("Control")]
    [Description("Allows you to mix a color & a graphics texture. E.g. If you have a graphics texture like a blue button you can mix it with a yellow color diffuse and the end result will be green. Defaults to 0xFFFFFFFF")]
    public virtual Color DiffuseColor
    {
      get { return diffuseColor; }
      set
      {
        if (diffuseColor != value)
        {
          diffuseColor = value;
          Modified = true;
          FirePropertyValueChanged("DiffuseColor");
        }
      }
    }

    [Category("Control")]
    [Description("Color for a control when it is not focused. Defaults to half transparent (0x60ffffff)")]
    public virtual Color DimColor
    {
      get { return dimColor; }
      set
      {
        if (dimColor != value)
        {
          dimColor = value;
          Modified = true;
          FirePropertyValueChanged("DimColor");
        }
      }
    }

    [Category("Actions")]
    [Description("The control id to move the focus to when the user moves left. If not specified (or zero) MediaPortal will find the closest control in that direction to move to")]
    public virtual int OnLeft
    {
      get { return onLeft; }
      set
      {
        if (onLeft != value)
        {
          onLeft = value;
          Modified = true;
          FirePropertyValueChanged("OnLeft");
        }
      }
    }

    [Category("Actions")]
    [Description("The control id to move the focus to when the user moves right. If not specified (or zero) MediaPortal will find the closest control in that direction to move to")]
    public virtual int OnRight
    {
      get { return onRight; }
      set
      {
        if (onRight != value)
        {
          onRight = value;
          Modified = true;
          FirePropertyValueChanged("OnRight");
        }
      }
    }

    [Category("Actions")]
    [Description("The control id to move the focus to when the user moves up. If not specified (or zero) MediaPortal will find the closest control in that direction to move to")]
    public virtual int OnUp
    {
      get { return onUp; }
      set
      {
        if (onUp != value)
        {
          onUp = value;
          Modified = true;
          FirePropertyValueChanged("OnUp");
        }
      }
    }

    [Category("Actions")]
    [Description("The control id to move the focus to when the user moves down. If not specified (or zero) MediaPortal will find the closest control in that direction to move to")]
    public virtual int OnDown
    {
      get { return onDown; }
      set
      {
        if (onDown != value)
        {
          onDown = value;
          Modified = true;
          FirePropertyValueChanged("OnDown");
        }
      }
    }

    #endregion

    #region Methods

    protected void FireStatusChanged()
    {
      if (StatusChanged != null)
      {
        MpeLog.Debug("MpeControl.FireStatusChanged(" + modified + ")");
        StatusChanged(this, modified);
      }
    }

    protected void FirePropertyValueChanged(string propertyName)
    {
      if (PropertyValueChanged != null)
      {
        MpeLog.Debug("MpeControl.FirePropertyValueChanged(" + propertyName + ", " + modified + ")");
        PropertyValueChanged(this, propertyName);
      }
    }

    public void SendBack()
    {
      if (MpeParent != null)
      {
        try
        {
          int i = MpeParent.Controls.GetChildIndex(this, true);
          MpeParent.Controls.SetChildIndex(this, (i + 1));
          MpeParent.Prepare();
        }
        catch (Exception ee)
        {
          MpeLog.Warn(ee);
        }
      }
    }

    public new void SendToBack()
    {
      base.SendToBack();
      if (MpeParent != null)
      {
        MpeParent.Prepare();
      }
    }

    public new void BringToFront()
    {
      base.BringToFront();
      if (MpeParent != null)
      {
        MpeParent.Prepare();
      }
    }

    public void BringForward()
    {
      if (MpeParent != null)
      {
        try
        {
          int i = MpeParent.Controls.GetChildIndex(this, true);
          if (i > 0)
          {
            MpeParent.Controls.SetChildIndex(this, (i - 1));
            MpeParent.Prepare();
          }
        }
        catch (Exception ee)
        {
          MpeLog.Warn(ee);
        }
      }
    }

    public override sealed string ToString()
    {
      return id.ToString() + " - " + type.DisplayName;
    }

    protected override sealed void Dispose(bool disposing)
    {
      if (disposing)
      {
        Destroy();
      }
      base.Dispose(disposing);
    }

    public void Prepare()
    {
      if (preparing == false)
      {
        preparing = true;
        try
        {
          PrepareControl();
        }
        catch (Exception e)
        {
          MpeLog.Debug(e);
        }
        finally
        {
          preparing = false;
        }
      }
    }

    public virtual MpeControl Copy()
    {
      return new MpeControl(this);
    }

    protected virtual void PrepareControl()
    {
      //
    }

    public virtual void Destroy()
    {
      textBrush.Dispose();
      borderPen.Dispose();
    }

    public virtual void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeControl.Load()");
      this.parser = parser;
      if (iterator != null)
      {
        // First load tags
        XPathNodeIterator i = iterator.Current.SelectChildren("", "");
        while (i.MoveNext())
        {
          if (i.Current.Name != null && i.Current.Value != null)
          {
            XPathNodeIterator ci = i.Current.SelectChildren("", "");
            if (ci.Count == 0)
            {
              if (i.Current.Name == "animation")
              {
                int pos = -1;
                switch (i.Current.Value)
                {
                  case "WindowOpen":
                    pos = 0;
                    break;
                  case "WindowClose":
                    pos = 1;
                    break;
                  case "Hidden":
                    pos = 2;
                    break;
                  case "Focus":
                    pos = 3;
                    break;
                  case "Unfocus":
                    pos = 4;
                    break;
                  case "VisibleChange":
                    pos = 5;
                    break;
                }
                if (pos >= 0)
                {
                  Animation.Animation[pos].Enabled = true;
                  string efect = i.Current.GetAttribute("effect", String.Empty);
                  switch (efect)
                  {
                    case "fade":
                      Animation.Animation[pos].Efect = MpeAnimationEfect.fade;
                      break;
                    case "slide":
                      Animation.Animation[pos].Efect = MpeAnimationEfect.slide;
                      break;
                    case "rotate":
                      Animation.Animation[pos].Efect = MpeAnimationEfect.rotate;
                      break;
                    case "rotatex":
                      Animation.Animation[pos].Efect = MpeAnimationEfect.rotatex;
                      break;
                    case "rotatey":
                      Animation.Animation[pos].Efect = MpeAnimationEfect.rotatey;
                      break;
                    case "zoom":
                      Animation.Animation[pos].Efect = MpeAnimationEfect.zoom;
                      break;
                  }
                  int parami = 0;
                  bool paramb = false;
                  int.TryParse(i.Current.GetAttribute("time",String.Empty ),out parami);
                  Animation.Animation[pos].Time = parami;
                  parami = 0;
                  int.TryParse(i.Current.GetAttribute("delay", String.Empty), out parami);
                  Animation.Animation[pos].Delay = parami;
                  Animation.Animation[pos].Start = i.Current.GetAttribute("start", String.Empty);
                  Animation.Animation[pos].End = i.Current.GetAttribute("end", String.Empty);
                  parami = 0;
                  int.TryParse(i.Current.GetAttribute("acceleration", String.Empty), out parami);
                  Animation.Animation[pos].Acceleration = parami;
                  string center = i.Current.GetAttribute("center", String.Empty);
                  if (center.Contains(","))
                  {
                    int.TryParse(center.Substring(0, center.IndexOf(',')), out parami);
                    int paramy = 0;
                    int.TryParse(center.Substring(center.IndexOf(',')+1), out paramy);
                    Animation.Animation[pos].Center = new Point(parami, paramy);
                  }
                  Animation.Animation[pos].Condition = i.Current.GetAttribute("condition", String.Empty);
                  bool.TryParse(i.Current.GetAttribute("reversible", String.Empty), out paramb);
                  Animation.Animation[pos].Reversible = paramb;
                  paramb = false;
                  bool.TryParse(i.Current.GetAttribute("pulse", String.Empty), out paramb);
                  Animation.Animation[pos].Pulse = paramb;
                  switch (i.Current.GetAttribute("tween", String.Empty))
                  {
                    case "elastic":
                      Animation.Animation[pos].Tween = MpeAnimationTween.elastic;
                      break;
                    case "bounce":
                      Animation.Animation[pos].Tween = MpeAnimationTween.bounce;
                      break;
                    case "circle":
                      Animation.Animation[pos].Tween = MpeAnimationTween.circle;
                      break;
                    case "back":
                      Animation.Animation[pos].Tween = MpeAnimationTween.back;
                      break;
                    case "sine":
                      Animation.Animation[pos].Tween = MpeAnimationTween.sine;
                      break;
                    case "cubic":
                      Animation.Animation[pos].Tween = MpeAnimationTween.cubic;
                      break;
                    case "quadratic":
                      Animation.Animation[pos].Tween = MpeAnimationTween.quadratic;
                      break;
                    case "linear":
                      Animation.Animation[pos].Tween = MpeAnimationTween.linear;
                      break;
                  }
                  switch (i.Current.GetAttribute("easing", String.Empty))
                  {
                    case "out":
                      Animation.Animation[pos].Easing = MpeAnimationEasing.Out;
                      break;
                    case "in":
                      Animation.Animation[pos].Easing = MpeAnimationEasing.In;
                      break;
                    case "inout":
                      Animation.Animation[pos].Easing = MpeAnimationEasing.inout;
                      break;
                  }
                }
              }
              else
              {
                tags.Add(i.Current.Name, i.Current.Value, false);
              }
            }
          }
        }
        tags.Remove("type");
        Id = parser.GetInt(iterator, "id", Id);
        tags.Remove("id");
        Description = parser.GetString(iterator, "description", Description);
        tags.Remove("description");
        // The position must be converted from absolute to relative
        int x = parser.GetInt(iterator, "posX", Left);
        int y = parser.GetInt(iterator, "posY", Top);
        MpeContainer c = MpeParent;
        while (c != null && c.Type != MpeControlType.Screen)
        {
          x -= c.Left;
          y -= c.Top;
          c = c.MpeParent;
        }
        Left = x;
        Top = y;
        tags.Remove("posX");
        tags.Remove("posY");
        // Load the rest of the properties
        Width = parser.GetInt(iterator, "width", Width);
        tags.Remove("width");
        Height = parser.GetInt(iterator, "height", Height);
        tags.Remove("height");
        Padding = parser.GetPadding(iterator, "padding", Padding);
        Visible = parser.GetString(iterator, "visible", Visible);
        tags.Remove("visible");
        DiffuseColor = parser.GetColor(iterator, "colordiffuse", DiffuseColor);
        tags.Remove("colordiffuse");
        DimColor = parser.GetColor(iterator, "dimColor", DimColor);
        tags.Remove("dimColor");
        OnLeft = parser.GetInt(iterator, "onleft", OnLeft);
        tags.Remove("onleft");
        OnRight = parser.GetInt(iterator, "onright", OnRight);
        tags.Remove("onright");
        OnUp = parser.GetInt(iterator, "onup", OnUp);
        tags.Remove("onup");
        OnDown = parser.GetInt(iterator, "ondown", OnDown);
        tags.Remove("ondown");
      }
      if (reference)
      {
        Id = 1;
      }
    }

    public virtual void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      if (doc != null && node != null)
      {
        // Type
        parser.SetValue(doc, node, "type", type.ToString());
        // Description
        string s1 = Description != null ? Description : Type.ToString();
        string s2 = (reference != null && reference.Description != null) ? reference.Description : Type.ToString();
        if (reference == null || (!s1.Equals(s2)))
        {
          parser.SetValue(doc, node, "description", s1);
        }
        // Id
        parser.SetInt(doc, node, "id", Id);
        // Location - Absolute Positioning
        Point p = AbsoluteLocation;
        parser.SetInt(doc, node, "posX", p.X);
        parser.SetInt(doc, node, "posY", p.Y);

        // Size
        if (reference == null || reference.Width != Width)
        {
          parser.SetInt(doc, node, "width", Width);
        }
        // Height
        if (reference == null || reference.Height != Height)
        {
          parser.SetInt(doc, node, "height", Height);
        }
        // DiffuseColor 
        if (reference == null || reference.DiffuseColor != DiffuseColor)
        {
          parser.SetColor(doc, node, "colordiffuse", DiffuseColor);
        }
        // DimColor 
        if (reference == null || reference.DimColor != DimColor)
        {
          parser.SetColor(doc, node, "dimColor", DimColor);
        }
        // Animation
        Animation.Save(doc, node, parser);
        // Visible
        //if (Visible == false)
        //{
          parser.SetValue(doc, node, "visible", Visible);
        //}
        // Actions
        if (OnLeft > 0)
        {
          parser.SetInt(doc, node, "onleft", OnLeft);
        }
        if (OnRight > 0)
        {
          parser.SetInt(doc, node, "onright", OnRight);
        }
        if (OnUp > 0)
        {
          parser.SetInt(doc, node, "onup", OnUp);
        }
        if (OnDown > 0)
        {
          parser.SetInt(doc, node, "ondown", OnDown);
        }


        string[] keys = tags.Keys;
        for (int i = 0; i < keys.Length; i++)
        {
          parser.SetValue(doc, node, keys[i], tags[keys[i]].Value);
        }
      }
    }

    #endregion

    #region Event Handlers

    protected virtual void OnTagCollectionChanged(MpeTag tag)
    {
      Modified = true;
    }

    protected virtual void OnLockChanged(MpeControlLockType type, bool value)
    {
      if (MpeParent != null)
      {
        switch (type)
        {
          case MpeControlLockType.Location:
            if (MpeParent.Spring)
            {
              if (value == false)
              {
                MpeLog.Warn("Cannot change location lock. The control belongs to a spring layout.");
                controlLock.Location = true;
              }
            }
            else
            {
              if (MpeParent.LayoutStyle != MpeLayoutStyle.Grid)
              {
                if (value == false)
                {
                  MpeLog.Warn("Cannot change location lock. The control belongs to a flow layout.");
                  controlLock.Location = true;
                }
              }
            }
            break;
          case MpeControlLockType.Size:
            if (MpeParent.Spring)
            {
              if (value == false)
              {
                MpeLog.Warn("Cannot change size lock. The control belongs to a spring layout.");
                controlLock.Size = true;
              }
            }
            else
            {
              if (MpeParent.LayoutStyle == MpeLayoutStyle.Grid)
              {
                if (AutoSize)
                {
                  if (value == false)
                  {
                    MpeLog.Warn("Cannot change size lock. The control is autosized.");
                    controlLock.Size = true;
                  }
                }
              }
            }
            break;
        }
      }
      if (type == MpeControlLockType.Location)
      {
        FirePropertyValueChanged("LocationLocked");
      }
      else
      {
        FirePropertyValueChanged("SizeLocked");
      }
    }

    protected virtual void OnPaddingChanged()
    {
      Prepare();
      Modified = true;
      Invalidate(false);
      FirePropertyValueChanged("Padding");
    }

    protected override void OnLocationChanged(EventArgs e)
    {
      base.OnLocationChanged(e);
      Modified = true;
      FirePropertyValueChanged("Location");
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      Prepare();
      Modified = true;
      FirePropertyValueChanged("Size");
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      e.Graphics.DrawString(Name, Font, textBrush, new Rectangle(1, 1, Width - 1, Height - 1));
      e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
    }

    #endregion

    #region Hidden Control Properties

    [Browsable(false)]
    public new string AccessibleDescription
    {
      get { return base.AccessibleDescription; }
    }

    [Browsable(false)]
    public new AccessibleRole AccessibleRole
    {
      get { return base.AccessibleRole; }
    }

    [Browsable(false)]
    public new string AccessibleName
    {
      get { return base.AccessibleName; }
    }

    [Browsable(false)]
    public override BindingContext BindingContext
    {
      get { return base.BindingContext; }
      set { base.BindingContext = value; }
    }

    [Browsable(false)]
    public override ContextMenu ContextMenu
    {
      get { return base.ContextMenu; }
      set { base.ContextMenu = value; }
    }

    [Browsable(false)]
    public override Cursor Cursor
    {
      get { return base.Cursor; }
      set { base.Cursor = value; }
    }

    [Browsable(false)]
    public override RightToLeft RightToLeft
    {
      get { return base.RightToLeft; }
      set { base.RightToLeft = value; }
    }

    [Browsable(false)]
    public override DockStyle Dock
    {
      get { return base.Dock; }
      set { base.Dock = value; }
    }

    [Browsable(false)]
    public override Font Font
    {
      get { return base.Font; }
      set { base.Font = value; }
    }

    [Browsable(false)]
    public override Color ForeColor
    {
      get { return base.ForeColor; }
      set { base.ForeColor = value; }
    }

    [Browsable(false)]
    public override ISite Site
    {
      get { return base.Site; }
      set { base.Site = value; }
    }

    [Browsable(false)]
    public override string Text
    {
      get { return base.Text; }
      set { base.Text = value; }
    }

    [Browsable(false)]
    public override Color BackColor
    {
      get { return base.BackColor; }
      set { base.BackColor = value; }
    }

    [Browsable(false)]
    public override AnchorStyles Anchor
    {
      get { return base.Anchor; }
      set { base.Anchor = value; }
    }

    [Browsable(false)]
    public override Image BackgroundImage
    {
      get { return base.BackgroundImage; }
      set { base.BackgroundImage = value; }
    }

    [Browsable(false)]
    public new ControlBindingsCollection DataBindings
    {
      get { return base.DataBindings; }
    }

    [Browsable(false)]
    public new ImeMode ImeMode
    {
      get { return base.ImeMode; }
      set { base.ImeMode = value; }
    }

    [Browsable(false)]
    public new int TabIndex
    {
      get { return base.TabIndex; }
      set { base.TabIndex = value; }
    }

    [Browsable(false)]
    public new bool TabStop
    {
      get { return base.TabStop; }
      set { base.TabStop = value; }
    }

    [Browsable(false)]
    public new object Tag
    {
      get { return base.Tag; }
      set { base.Tag = value; }
    }

    [Browsable(false)]
    public new bool CausesValidation
    {
      get { return base.CausesValidation; }
      set { base.CausesValidation = value; }
    }

    #endregion
  }

  #region IdentityEventArgs

  public class IdentityEventArgs : EventArgs
  {
    private bool cancel;
    private int newId;
    private int currentId;
    private string message;

    public IdentityEventArgs(int newId, int currentId)
    {
      cancel = false;
      Message = "";
      this.newId = newId;
      this.currentId = currentId;
    }

    public bool Cancel
    {
      get { return cancel; }
      set { cancel = value; }
    }

    public int New
    {
      get { return newId; }
    }

    public int Current
    {
      get { return currentId; }
    }

    public string Message
    {
      get { return message; }
      set { message = value; }
    }
  }

  #endregion
}