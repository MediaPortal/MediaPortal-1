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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Mpe.Controls.Design;
using Mpe.Controls.Properties;

namespace Mpe.Controls
{
  public class MpeSpinButton : MpeContainer
  {
    #region Variables

    protected MpeControlOrientation orientation;
    protected bool reverse;
    protected bool showRange;
    protected MpeItemManager items;
    protected MpeLabel label;
    protected MpeImage imageUp;
    protected MpeImage imageDown;
    protected MpeGroup imageGroup;
    //protected MpeImage imageUpFocus;
    //protected MpeImage imageDownFocus;
    protected FileInfo textureUp;
    protected FileInfo textureUpFocus;
    protected FileInfo textureDown;
    protected FileInfo textureDownFocus;

    #endregion

    #region Constructors

    public MpeSpinButton() : base()
    {
      MpeLog.Debug("MpeSpinButton()");
      Type = MpeControlType.SpinButton;
      orientation = MpeControlOrientation.Horizontal;
      alignment = MpeControlAlignment.Right;
      reverse = false;
      spacing = 5;
      layoutStyle = MpeLayoutStyle.HorizontalFlow;
      alignment = MpeControlAlignment.Left;
      autoSize = true;
      controlLock.Size = true;
      // Label
      label = new MpeLabel();
      label.Embedded = true;
      // Images
      imageGroup = new MpeGroup();
      imageGroup.Embedded = true;
      imageGroup.ShowBorder = false;
      imageGroup.LayoutStyle = MpeLayoutStyle.VerticalFlow;
      imageUp = new MpeImage();
      imageUp.Embedded = true;
      imageUp.Size = new Size(16, 16);
      imageDown = new MpeImage();
      imageDown.Embedded = true;
      imageDown.Size = new Size(16, 16);
      // Items
      items = new MpeItemManager();
      items.Type = MpeItemType.Integer;
      items.TypeChanging += new MpeItemManager.TypeChangingHandler(OnItemTypeChanging);
      items.Values.ItemInserted += new MpeItemCollection.ItemInsertedHandler(OnItemInserted);
      items.Values.ItemRemoved += new MpeItemCollection.ItemRemovedHandler(OnItemRemoved);
      items.Values.ItemSet += new MpeItemCollection.ItemSetHandler(OnItemSet);
      items.Values.ItemsCleared += new MpeItemCollection.ItemsClearedHandler(OnItemsCleared);
      SetLabel();
      Prepare();
    }

    public MpeSpinButton(MpeSpinButton spinner) : base(spinner)
    {
      MpeLog.Debug("MpeSpinButton(spinner)");
      orientation = spinner.orientation;
      reverse = spinner.reverse;
      showRange = spinner.showRange;
      textureUp = spinner.textureUp;
      textureUpFocus = spinner.textureUpFocus;
      textureDown = spinner.textureDown;
      textureDownFocus = spinner.textureDownFocus;
      label = new MpeLabel(spinner.label);
      imageUp = new MpeImage(spinner.imageUp);
      imageDown = new MpeImage(spinner.imageDown);
      imageGroup = new MpeGroup(spinner.imageGroup);
      items = new MpeItemManager(spinner.items);
      items.TypeChanging += new MpeItemManager.TypeChangingHandler(OnItemTypeChanging);
      items.Values.ItemInserted += new MpeItemCollection.ItemInsertedHandler(OnItemInserted);
      items.Values.ItemRemoved += new MpeItemCollection.ItemRemovedHandler(OnItemRemoved);
      items.Values.ItemSet += new MpeItemCollection.ItemSetHandler(OnItemSet);
      items.Values.ItemsCleared += new MpeItemCollection.ItemsClearedHandler(OnItemsCleared);
      SetLabel();
      Prepare();
    }

    #endregion

    #region Properties

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public FileInfo TextureUp
    {
      get { return textureUp; }
      set
      {
        textureUp = value;
        if (Focused == false)
        {
          imageUp.Texture = value;
          Invalidate(true);
        }
        Modified = true;
        FirePropertyValueChanged("TextureUp");
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    public FileInfo TextureUpFocus
    {
      get { return textureUpFocus; }
      set
      {
        textureUpFocus = value;
        if (Focused)
        {
          imageUp.Texture = value;
          Invalidate(true);
        }
        Modified = true;
        FirePropertyValueChanged("TextureUpFocus");
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    public FileInfo TextureDown
    {
      get { return textureDown; }
      set
      {
        textureDown = value;
        if (Focused == false)
        {
          imageDown.Texture = value;
          Invalidate(true);
        }
        Modified = true;
        FirePropertyValueChanged("TextureDown");
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    public FileInfo TextureDownFocus
    {
      get { return textureDownFocus; }
      set
      {
        textureDownFocus = value;
        if (Focused)
        {
          imageDown.Texture = value;
          Invalidate(true);
        }
        Modified = true;
        FirePropertyValueChanged("TextureDownFocus");
      }
    }

    [Category("Textures")]
    public Size TextureSize
    {
      get { return imageUp.Size; }
      set
      {
        if (imageUp.Size != value)
        {
          imageUp.Size = value;
          imageDown.Size = value;
          Modified = true;
          FirePropertyValueChanged("TextureSize");
        }
      }
    }

    [Category("Layout")]
    public MpeControlOrientation Orientation
    {
      get { return orientation; }
      set
      {
        if (orientation != value)
        {
          orientation = value;
          Prepare();
          Modified = true;
          FirePropertyValueChanged("Orientation");
        }
      }
    }

    [ReadOnly(false)]
    public override MpeControlAlignment Alignment
    {
      get { return alignment; }
      set
      {
        if (alignment != value)
        {
          alignment = value;
          Prepare();
          Modified = true;
          FirePropertyValueChanged("Orientation");
        }
      }
    }

    [Category("Control")]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public bool ShowRange
    {
      get { return showRange; }
      set
      {
        if (showRange != value)
        {
          showRange = value;
          SetLabel();
          Modified = true;
          FirePropertyValueChanged("ShowRange");
        }
      }
    }

    [Category("Control")]
    public bool Reverse
    {
      get { return reverse; }
      set
      {
        if (reverse != value)
        {
          reverse = value;
          Modified = true;
          FirePropertyValueChanged("Reverse");
        }
      }
    }

    [Browsable(true)]
    public override bool Focused
    {
      get { return base.Focused; }
      set
      {
        if (base.Focused != value)
        {
          base.Focused = value;
          if (value)
          {
            imageUp.Texture = textureUpFocus;
            imageDown.Texture = textureDownFocus;
          }
          else
          {
            imageUp.Texture = textureUp;
            imageDown.Texture = textureDown;
          }
          Invalidate(true);
        }
      }
    }

    [Browsable(true)]
    public override bool Enabled
    {
      get { return base.Enabled; }
      set
      {
        if (base.Enabled != value)
        {
          base.Enabled = value;
          if (label != null)
          {
            label.Enabled = value;
          }
          Invalidate(true);
        }
      }
    }

    [Category("Items")]
    public MpeItemManager Items
    {
      get { return items; }
      set
      {
        items = value;
        Invalidate(false);
      }
    }

    [Category("Labels")]
    [Browsable(true)]
    [ReadOnly(true)]
    public override string Text
    {
      get { return label.Text; }
    }

    [TypeConverter(typeof(MpeFontConverter))]
    [Category("Labels")]
    [Description("The font that will be used to render the button label.")]
    [RefreshProperties(RefreshProperties.Repaint)]
    public new MpeFont Font
    {
      get { return label.Font; }
      set { label.Font = value; }
    }

    [Category("Labels")]
    public virtual Color Color
    {
      get { return label.TextColor; }
      set { label.TextColor = value; }
    }

    [Category("Labels")]
    public virtual Color DisabledColor
    {
      get { return label.DisabledColor; }
      set { label.DisabledColor = value; }
    }

    [Browsable(true)]
    public override int OnLeft
    {
      get { return base.OnLeft; }
      set { base.OnLeft = value; }
    }

    [Browsable(true)]
    public override int OnRight
    {
      get { return base.OnRight; }
      set { base.OnRight = value; }
    }

    [Browsable(true)]
    public override int OnUp
    {
      get { return base.OnUp; }
      set { base.OnUp = value; }
    }

    [Browsable(true)]
    public override int OnDown
    {
      get { return base.OnDown; }
      set { base.OnDown = value; }
    }

    [Browsable(false)]
    public override bool Spring
    {
      get { return base.Spring; }
      set { base.Spring = value; }
    }

    [ReadOnly(true)]
    public override int Spacing
    {
      get { return base.Spacing; }
      set { base.Spacing = value; }
    }

    [Browsable(false)]
    public override MpeControlPadding Padding
    {
      get { return base.Padding; }
      set { base.Padding = value; }
    }

    [Browsable(false)]
    public override MpeLayoutStyle LayoutStyle
    {
      get { return base.LayoutStyle; }
      set { base.LayoutStyle = value; }
    }

    [Browsable(false)]
    public override bool ShowGrid
    {
      get { return base.ShowGrid; }
      set { base.ShowGrid = value; }
    }

    [Browsable(false)]
    public override bool SnapToGrid
    {
      get { return base.SnapToGrid; }
      set { base.SnapToGrid = value; }
    }

    [Browsable(false)]
    public override Size GridSize
    {
      get { return base.GridSize; }
      set { base.GridSize = value; }
    }

    [Browsable(false)]
    public override FileInfo TextureBack
    {
      get { return base.TextureBack; }
      set { base.TextureBack = value; }
    }

    #endregion

    #region Methods

    private void SetLabel()
    {
      if (items.Values.Count > 0)
      {
        label.Lookup = items.Type != MpeItemType.Text ? false : true;
        string s = items.Values[0].Value;
        if (showRange)
        {
          s += "/" + items.Last;
        }
        label.Text = s;
      }
      else
      {
        if (showRange)
        {
          label.Text = "0/0";
        }
        else
        {
          label.Text = "0";
        }
      }
    }

    public override MpeControl Copy()
    {
      return new MpeSpinButton(this);
    }

    protected override void PrepareControl()
    {
      if (label != null && imageGroup != null && imageUp != null && imageDown != null)
      {
        MpeLog.Debug("MpeSpinButton.Prepare()");
        Controls.Clear();
        imageGroup.Controls.Clear();
        if (Orientation == MpeControlOrientation.Horizontal)
        {
          imageGroup.LayoutStyle = MpeLayoutStyle.HorizontalFlow;
          switch (Alignment)
          {
            case MpeControlAlignment.Left:
              imageGroup.Controls.Add(imageDown);
              imageGroup.Controls.Add(imageUp);
              Controls.Add(label);
              Controls.Add(imageGroup);
              break;
            case MpeControlAlignment.Center:
              Controls.Add(imageDown);
              Controls.Add(label);
              Controls.Add(imageUp);
              break;
            case MpeControlAlignment.Right:
              imageGroup.Controls.Add(imageDown);
              imageGroup.Controls.Add(imageUp);
              Controls.Add(imageGroup);
              Controls.Add(label);
              break;
          }
        }
        else
        {
          imageGroup.LayoutStyle = MpeLayoutStyle.VerticalFlow;
          imageGroup.Controls.Add(imageUp);
          imageGroup.Controls.Add(imageDown);
          switch (Alignment)
          {
            case MpeControlAlignment.Left:
              Controls.Add(label);
              Controls.Add(imageGroup);
              break;
            default:
              Controls.Add(imageGroup);
              Controls.Add(label);
              break;
          }
        }
        base.PrepareControl();
      }
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeSpinButton.Load()");
      base.Load(iterator, parser);
      this.parser = parser;
      label.Load(iterator, parser);
      TextureUp = parser.GetImageFile(iterator, "textureUp", TextureUp);
      TextureUpFocus = parser.GetImageFile(iterator, "textureUpFocus", TextureUpFocus);
      TextureDown = parser.GetImageFile(iterator, "textureDown", TextureDown);
      TextureDownFocus = parser.GetImageFile(iterator, "textureDownFocus", TextureDownFocus);
      int w = parser.GetInt(iterator, "width", TextureSize.Width);
      int h = parser.GetInt(iterator, "height", TextureSize.Height);
      TextureSize = new Size(w, h);
      string s = parser.GetString(iterator, "orientation", "");
      if (s.Equals("vertical"))
      {
        Orientation = MpeControlOrientation.Vertical;
      }
      else
      {
        Orientation = MpeControlOrientation.Horizontal;
      }
      Reverse = parser.GetBoolean(iterator, "reverse", Reverse);
      ShowRange = parser.GetBoolean(iterator, "showrange", ShowRange);
      // Load SubItems
      s = parser.GetString(iterator, "subtype", "");
      if (s.Equals("integer"))
      {
        items.Type = MpeItemType.Integer;
        items.First = parser.GetString(iterator, "subitems/first", items.First);
        items.Last = parser.GetString(iterator, "subitems/last", items.Last);
        items.Digits = parser.GetString(iterator, "subitems/digits", items.Digits);
        items.Interval = parser.GetString(iterator, "subitems/interval", items.Interval);
      }
      else if (s.Equals("float"))
      {
        items.Type = MpeItemType.Float;
        items.First = parser.GetString(iterator, "subitems/first", items.First);
        items.Last = parser.GetString(iterator, "subitems/last", items.Last);
        items.Digits = parser.GetString(iterator, "subitems/digits", items.Digits);
        items.Interval = parser.GetString(iterator, "subitems/interval", items.Interval);
      }
      // Remove known tags
      tags.Remove("align");
      tags.Remove("font");
      tags.Remove("textcolor");
      tags.Remove("disabledcolor");
      tags.Remove("label");
      tags.Remove("orientation");
      tags.Remove("reverse");
      tags.Remove("showrange");
      tags.Remove("subtype");
      tags.Remove("textureDown");
      tags.Remove("textureDownFocus");
      tags.Remove("textureUp");
      tags.Remove("textureUpFocus");
      SetLabel();
      Prepare();
      Modified = false;
    }

    public override void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      base.Save(doc, node, parser, reference);
      MpeSpinButton spin = null;
      if (reference != null && reference is MpeSpinButton)
      {
        spin = (MpeSpinButton) reference;
      }
      // Fix the Width and Height to be the texture size
      parser.SetInt(doc, node, "width", TextureSize.Width);
      parser.SetInt(doc, node, "height", TextureSize.Height);
      // Font
      if (spin == null || !spin.Font.Name.Equals(Font.Name))
      {
        parser.SetValue(doc, node, "font", Font.Name);
      }
      // Alignment
      if (spin == null || spin.Alignment != Alignment)
      {
        parser.SetValue(doc, node, "align", Alignment.ToString().ToLower());
      }
      // Color
      if (spin == null || spin.Color != Color)
      {
        parser.SetColor(doc, node, "textcolor", Color);
      }
      // DisabledColor
      if (spin == null || spin.DisabledColor != DisabledColor)
      {
        parser.SetColor(doc, node, "disabledcolor", DisabledColor);
      }
      // Orientation
      if (spin == null || spin.Orientation != Orientation)
      {
        parser.SetValue(doc, node, "orientation", Orientation.ToString().ToLower());
      }
      // Reverse
      if (spin == null || spin.Reverse != Reverse)
      {
        parser.SetValue(doc, node, "reverse", Reverse ? "yes" : "no");
      }
      // ShowRange
      if (spin == null || spin.ShowRange != ShowRange)
      {
        parser.SetValue(doc, node, "showrange", ShowRange ? "yes" : "no");
      }
      // Textures
      if (spin == null || spin.TextureUp == null || spin.TextureUp.Equals(TextureUp) == false)
      {
        if (TextureUp == null)
        {
          parser.SetValue(doc, node, "textureUp", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureUp", TextureUp.Name);
        }
      }
      if (spin == null || spin.TextureUpFocus == null || spin.TextureUpFocus.Equals(TextureUpFocus) == false)
      {
        if (TextureUpFocus == null)
        {
          parser.SetValue(doc, node, "textureUpFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureUpFocus", TextureUpFocus.Name);
        }
      }
      if (spin == null || spin.TextureDown == null || spin.TextureDown.Equals(TextureDown) == false)
      {
        if (TextureDown == null)
        {
          parser.SetValue(doc, node, "textureDown", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureDown", TextureDown.Name);
        }
      }
      if (spin == null || spin.TextureDownFocus == null || spin.TextureDownFocus.Equals(TextureDownFocus) == false)
      {
        if (TextureDownFocus == null)
        {
          parser.SetValue(doc, node, "textureDownFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureDownFocus", TextureDownFocus.Name);
        }
      }
      // Items
      parser.SetValue(doc, node, "subtype", items.Type.ToString().ToLower());
      XmlElement subitems = doc.CreateElement("subitems");
      parser.SetValue(doc, subitems, "type", items.Type.ToString().ToLower());
      parser.SetValue(doc, subitems, "first", items.First);
      parser.SetValue(doc, subitems, "last", items.Last);
      parser.SetValue(doc, subitems, "interval", items.Interval);
      parser.SetValue(doc, subitems, "digits", items.Digits);
      node.AppendChild(subitems);
    }

    #endregion

    #region Event Handlers

    private void OnItemsCleared()
    {
      MpeLog.Debug("OnItemsCleared()");
      SetLabel();
    }

    private void OnItemInserted(int index, MpeItem item)
    {
      MpeLog.Debug("OnItemInserted()");
      SetLabel();
    }

    private void OnItemSet(int index, MpeItem oldItem, MpeItem newItem)
    {
      MpeLog.Debug("OnItemSet()");
      SetLabel();
    }

    private void OnItemRemoved(int index, MpeItem item)
    {
      MpeLog.Debug("OnItemRemoved()");
      SetLabel();
    }

    private void OnItemTypeChanging(MpeItemEventArgs e)
    {
      if (e.NewType == MpeItemType.Text)
      {
        MpeLog.Warn("SpinButton items must be of type Integer or Float");
        e.CancelTypeChange = true;
      }
    }

    #endregion
  }
}