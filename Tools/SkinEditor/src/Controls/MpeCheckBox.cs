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
  /// <summary>
  ///
  /// </summary>
  public class MpeCheckBox : MpeContainer
  {
    #region Variables

    protected MpeImage image;
    protected MpeLabel label;
    protected FileInfo textureFile;
    protected FileInfo textureCheckedFile;
    protected bool on;

    #endregion

    #region Constructors

    public MpeCheckBox() : base()
    {
      MpeLog.Debug("MpeCheckBox()");
      Type = MpeControlType.CheckBox;
      AllowDrop = false;
      alignment = MpeControlAlignment.Right;
      layoutStyle = MpeLayoutStyle.HorizontalFlow;
      spring = false;
      spacing = 5;
      showBorder = false;
      autoSize = true;
      controlLock.Size = true;
      image = new MpeImage();
      image.Embedded = true;
      image.AutoSize = false;
      label = new MpeLabel();
      label.Embedded = true;
      label.Text = "MpeCheckBox";
      Controls.Add(label);
      Controls.Add(image);
    }

    public MpeCheckBox(MpeCheckBox checkbox) : base(checkbox)
    {
      MpeLog.Debug("MpeCheckBox(checkbox)");
      Type = MpeControlType.CheckBox;
      AllowDrop = false;
      label = new MpeLabel(checkbox.label);
      image = new MpeImage(checkbox.image);
      textureFile = checkbox.textureFile;
      textureCheckedFile = checkbox.textureCheckedFile;
      Controls.Add(label);
      Controls.Add(image);
    }

    #endregion

    #region Properties

    [Browsable(false)]
    public override FileInfo TextureBack
    {
      get { return base.TextureBack; }
      set { base.TextureBack = value; }
    }

    [Category("Labels")]
    [Browsable(true)]
    [RefreshProperties(RefreshProperties.Repaint)]
    [Editor(typeof(MpeStringEditor), typeof(UITypeEditor))]
    public override string Text
    {
      get { return label.Text; }
      set { label.Text = value; }
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
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public virtual Color Color
    {
      get { return label.DisabledColor; }
      set { label.DisabledColor = value; }
    }

    [Category("Labels")]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public virtual Color DisabledColor
    {
      get { return label.DisabledColor; }
      set { label.DisabledColor = value; }
    }

    [Category("Labels")]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public virtual Color FocusedColor
    {
      get { return label.TextColor; }
      set { label.TextColor = value; }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureChecked
    {
      get { return textureCheckedFile; }
      set { textureCheckedFile = value; }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it does not have focus.")]
    public FileInfo Texture
    {
      get { return textureFile; }
      set { textureFile = value; }
    }

    [Category("Textures")]
    public Size TextureSize
    {
      get { return image.Size; }
      set
      {
        if (image.Size != value)
        {
          image.Size = value;
          Modified = true;
          Invalidate(false);
          FirePropertyValueChanged("TextureSize");
        }
      }
    }

    [ReadOnly(true)]
    public override int Spacing
    {
      get { return base.Spacing; }
      set { base.Spacing = value; }
    }

    [ReadOnly(false)]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public override MpeControlAlignment Alignment
    {
      get { return alignment; }
      set
      {
        if (alignment != value)
        {
          alignment = value;
          switch (value)
          {
            case MpeControlAlignment.Right:
              label.BringToFront();
              break;
            case MpeControlAlignment.Left:
              label.SendToBack();
              break;
          }
        }
      }
    }

    [Browsable(false)]
    public override bool Spring
    {
      get { return base.Spring; }
      set { base.Spring = value; }
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

    [Browsable(true)]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public override bool Focused
    {
      get { return base.Focused; }
      set
      {
        base.Focused = value;
        if (value)
        {
          label.Enabled = true;
          Enabled = true;
        }
        else
        {
          label.Enabled = false;
        }
        label.Invalidate(false);
      }
    }

    [Browsable(true)]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public override bool Enabled
    {
      get { return base.Enabled; }
      set
      {
        base.Enabled = value;
        if (value == false)
        {
          Focused = false;
        }
        if (label != null)
        {
          label.Invalidate(false);
        }
      }
    }

    [Category("Control")]
    [DefaultValue(false)]
    public bool Checked
    {
      get { return on; }
      set
      {
        if (on != value)
        {
          on = value;
          Prepare();
        }
      }
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

    #endregion

    #region Methods

    protected override void PrepareControl()
    {
      if (image != null)
      {
        if (on && image.Texture != textureCheckedFile)
        {
          image.Texture = textureCheckedFile;
        }
        else if (on == false && image.Texture != textureFile)
        {
          image.Texture = textureFile;
        }
      }
      base.PrepareControl();
    }

    public override MpeControl Copy()
    {
      return new MpeCheckBox(this);
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      base.Load(iterator, parser);
      MpeLog.Debug("MpeCheckBox.Load()");
      this.parser = parser;
      label.Load(iterator, parser);
      label.Enabled = false;
      tags.Remove("label");
      tags.Remove("align");
      tags.Remove("textcolor");
      tags.Remove("disabledcolor");
      tags.Remove("font");
      // Textures
      Texture = parser.GetImageFile(iterator, "textureCheckmarkNoFocus", Texture);
      tags.Remove("textureCheckmarkNoFocus");
      TextureChecked = parser.GetImageFile(iterator, "textureCheckmark", TextureChecked);
      tags.Remove("textureCheckmark");
      int w = parser.GetInt(iterator, "MarkWidth", TextureSize.Width);
      tags.Remove("MarkWidth");
      int h = parser.GetInt(iterator, "MarkHeight", TextureSize.Height);
      tags.Remove("MarkHeight");
      TextureSize = new Size(w, h);
      Modified = false;
    }

    public override void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      base.Save(doc, node, parser, reference);
      // Remove Width and Height
      parser.RemoveNode(node, "width");
      parser.RemoveNode(node, "height");

      MpeCheckBox checkbox = null;
      if (reference != null && reference is MpeCheckBox)
      {
        checkbox = (MpeCheckBox) reference;
      }

      // Label
      label.Save(doc, node, parser, checkbox != null ? checkbox.label : null);
      parser.SetValue(doc, node, "type", Type.ToString());

      // TextureChecked
      if (checkbox == null || checkbox.TextureChecked == null || checkbox.TextureChecked.Equals(TextureChecked) == false
        )
      {
        if (TextureChecked == null)
        {
          parser.SetValue(doc, node, "textureCheckmark", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureCheckmark", TextureChecked.Name);
        }
      }
      // Texture
      if (checkbox == null || checkbox.Texture == null || checkbox.Texture.Equals(Texture) == false)
      {
        if (Texture == null)
        {
          parser.SetValue(doc, node, "textureCheckmarkNoFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureCheckmarkNoFocus", Texture.Name);
        }
      }
      // TextureSize
      if (checkbox == null || checkbox.TextureSize != TextureSize)
      {
        parser.SetInt(doc, node, "MarkWidth", TextureSize.Width);
        parser.SetInt(doc, node, "MarkHeight", TextureSize.Height);
      }
      // Shadow
      parser.SetValue(doc, node, "shadow", "no");
      // Save Correct Type
      //parser.SetValue(doc, node, "type", parser.ControlTypeToXmlString(Type));
      //parser.SetValue(doc, node, "type", Type.ToString());
    }

    #endregion
  }
}