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
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Mpe.Controls.Design;
using Mpe.Controls.Properties;

namespace Mpe.Controls
{
  /// <summary>
  ///
  /// </summary>
  public class MpeTextArea : MpeContainer
  {
    #region Variables

    private MpeLabel label;
    private MpeSpinButton spinButton;

    #endregion

    #region Constructors

    public MpeTextArea() : base()
    {
      MpeLog.Debug("MpeTextArea()");
      Type = MpeControlType.TextArea;
      layoutStyle = MpeLayoutStyle.Grid;
      label = new MpeLabel();
      label.Embedded = true;
      label.AutoSize = false;
      label.Text = "MpeTextArea";
      label.Lookup = true;
      spinButton = new MpeSpinButton();
      spinButton.Embedded = true;
      spinButton.ShowRange = true;
      spinButton.Orientation = MpeControlOrientation.Vertical;
      Controls.Add(label);
      Controls.Add(spinButton);
    }

    public MpeTextArea(MpeTextArea textarea) : base(textarea)
    {
      MpeLog.Debug("MpeTextArea(textarea)");
      label = new MpeLabel(textarea.label);
      spinButton = new MpeSpinButton(textarea.spinButton);
      Controls.Add(label);
      Controls.Add(spinButton);
    }

    #endregion

    #region Properties - Actions

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

    #region Properties - TextArea

    [Category("Control")]
    public Color Color
    {
      get { return label.TextColor; }
      set
      {
        label.TextColor = value;
        Modified = true;
        FirePropertyValueChanged("Color");
      }
    }

    [Category("Control")]
    public Color DisabledColor
    {
      get { return label.DisabledColor; }
      set
      {
        label.DisabledColor = value;
        Modified = true;
        FirePropertyValueChanged("DisabledColor");
      }
    }

    [Category("Control")]
    [ReadOnly(false)]
    [TypeConverter(typeof(MpeFontConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    public new MpeFont Font
    {
      get { return label.Font; }
      set
      {
        label.Font = value;
        Modified = true;
        FirePropertyValueChanged("Font");
      }
    }

    [Category("Control")]
    [Browsable(true)]
    [Editor(typeof(MpeTextAreaEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public new string Text
    {
      get { return label.Text; }
      set
      {
        label.Text = value;
        Modified = true;
        FirePropertyValueChanged("Text");
      }
    }

    [Category("Control")]
    [ReadOnly(true)]
    public override MpeControlAlignment Alignment
    {
      get { return label.Alignment; }
      set
      {
        if (label.Alignment != value)
        {
          label.Alignment = value;
          Modified = true;
          FirePropertyValueChanged("Alignment");
        }
      }
    }

    #endregion

    #region Properties - SpinButton

    [Category("SpinButton")]
    [ReadOnly(false)]
    public MpeControlAlignment SpinAlign
    {
      get { return base.Alignment; }
      set
      {
        if (value == MpeControlAlignment.Center)
        {
          MpeLog.Warn("SpinButton alignment can only be set to Left or Right");
          return;
        }
        if (base.Alignment != value)
        {
          base.Alignment = value;
          Modified = true;
          FirePropertyValueChanged("SpinAlign");
        }
      }
    }

    [Category("SpinButton")]
    public Size SpinSize
    {
      get { return spinButton.TextureSize; }
      set
      {
        spinButton.TextureSize = value;
        Modified = true;
        FirePropertyValueChanged("SpinSize");
      }
    }

    [Category("SpinButton")]
    public Color SpinColor
    {
      get { return spinButton.Color; }
      set
      {
        spinButton.Color = value;
        Modified = true;
        FirePropertyValueChanged("SpinColor");
      }
    }

    [Category("SpinButton")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public FileInfo TextureUp
    {
      get { return spinButton.TextureUp; }
      set
      {
        spinButton.TextureUp = value;
        Modified = true;
        FirePropertyValueChanged("TextureUp");
      }
    }

    [Category("SpinButton")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    public FileInfo TextureUpFocus
    {
      get { return spinButton.TextureUpFocus; }
      set
      {
        spinButton.TextureUpFocus = value;
        Modified = true;
        FirePropertyValueChanged("TextureUpFocus");
      }
    }

    [Category("SpinButton")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public FileInfo TextureDown
    {
      get { return spinButton.TextureDown; }
      set
      {
        spinButton.TextureDown = value;
        Modified = true;
        FirePropertyValueChanged("TextureDown");
      }
    }

    [Category("SpinButton")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    public FileInfo TextureDownFocus
    {
      get { return spinButton.TextureDownFocus; }
      set
      {
        spinButton.TextureDownFocus = value;
        Modified = true;
        FirePropertyValueChanged("TextureDownFocus");
      }
    }

    #endregion

    #region Properties - Hidden

    [Browsable(false)]
    public override bool Spring
    {
      get { return base.Spring; }
      set { base.Spring = value; }
    }

    [Browsable(false)]
    public override FileInfo TextureBack
    {
      get { return base.TextureBack; }
      set { base.TextureBack = value; }
    }

    [Browsable(false)]
    public override bool AutoSize
    {
      get { return base.AutoSize; }
      set { base.AutoSize = value; }
    }

    [Browsable(false)]
    public override Size GridSize
    {
      get { return base.GridSize; }
      set { base.GridSize = value; }
    }

    [Browsable(false)]
    public override bool ShowBorder
    {
      get { return base.ShowBorder; }
      set { base.ShowBorder = value; }
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
    public override MpeLayoutStyle LayoutStyle
    {
      get { return base.LayoutStyle; }
      set { base.LayoutStyle = value; }
    }

    #endregion

    #region Methods

    public override MpeControl Copy()
    {
      return new MpeTextArea(this);
    }

    protected override void PrepareControl()
    {
      if (label != null && spinButton != null)
      {
        label.Left = Padding.Left;
        label.Top = Padding.Top;
        label.Width = Width - Padding.Width;
        label.Height = Height - Padding.Height - spinButton.Height - Spacing;
        spinButton.Top = Padding.Top + label.Height + Spacing;
        switch (SpinAlign)
        {
          case MpeControlAlignment.Right:
            spinButton.Left = Width - spinButton.Width - Padding.Right;
            break;
          case MpeControlAlignment.Left:
            spinButton.Left = Padding.Left;
            break;
        }
      }
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeTextArea.Load()");
      base.Load(iterator, parser);
      this.parser = parser;
      Controls.Clear();
      Padding = parser.GetPadding(iterator, "mpe/padding", Padding);
      Spacing = parser.GetInt(iterator, "mpe/spacing", Spacing);
      Left -= Padding.Left;
      Top -= Padding.Top;
      label = (MpeLabel) parser.CreateControl(MpeControlType.Label);
      if (label != null)
      {
        label.Lookup = false;
        label.Embedded = true;
        label.AutoSize = false;
        label.Font = parser.GetFont(iterator, "font", label.Font);
        label.DisabledColor = parser.GetColor(iterator, "disabledcolor", label.DisabledColor);
        label.TextColor = parser.GetColor(iterator, "textcolor", label.TextColor);
        label.Text = parser.GetString(iterator, "text", label.Text);
        Controls.Add(label);
      }
      spinButton = (MpeSpinButton) parser.CreateControl(MpeControlType.SpinButton);
      if (spinButton != null)
      {
        spinButton.Embedded = true;
        spinButton.Color = parser.GetColor(iterator, "SpinColor", spinButton.Color);
        int w = parser.GetInt(iterator, "SpinWidth", spinButton.TextureSize.Width);
        int h = parser.GetInt(iterator, "SpinHeight", spinButton.TextureSize.Height);
        spinButton.TextureSize = new Size(w, h);
        spinButton.TextureUp = parser.GetImageFile(iterator, "textureUp", spinButton.TextureUp);
        spinButton.TextureUpFocus = parser.GetImageFile(iterator, "textureUpFocus", spinButton.TextureUpFocus);
        spinButton.TextureDown = parser.GetImageFile(iterator, "textureDown", spinButton.TextureDown);
        spinButton.TextureDownFocus = parser.GetImageFile(iterator, "textureDownFocus", spinButton.TextureDownFocus);
        SpinAlign = parser.GetAlignment(iterator, "SpinAlign", SpinAlign);
        Controls.Add(spinButton);
      }
      Width += Padding.Width;
      Height += Padding.Height + Spacing + spinButton.Height;
      // Remove known tags
      tags.Remove("align");
      tags.Remove("font");
      tags.Remove("textcolor");
      tags.Remove("disabledcolor");
      tags.Remove("text");
      tags.Remove("textureDown");
      tags.Remove("textureDownFocus");
      tags.Remove("textureUp");
      tags.Remove("textureUpFocus");
      tags.Remove("spinAlign");
      tags.Remove("spinColor");
      tags.Remove("spinWidth");
      tags.Remove("spinHeight");
      tags.Remove("spinPosX");
      tags.Remove("spinPosY");
      tags.Remove("SpinAlign");
      tags.Remove("SpinColor");
      tags.Remove("SpinWidth");
      tags.Remove("SpinHeight");
      tags.Remove("SpinPosX");
      tags.Remove("SpinPosY");
      Modified = false;
    }

    public override void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      base.Save(doc, node, parser, reference);
      MpeTextArea textarea = null;
      if (reference != null && reference is MpeTextArea)
      {
        textarea = (MpeTextArea) reference;
      }
      // Label Properties
      Point p = label.AbsoluteLocation;
      parser.SetInt(doc, node, "posX", p.X);
      parser.SetInt(doc, node, "posY", p.Y);
      parser.SetInt(doc, node, "width", label.Width);
      parser.SetInt(doc, node, "height", label.Height);
      parser.SetValue(doc, node, "text", Text);
      if (textarea == null || !textarea.Font.Name.Equals(Font.Name))
      {
        parser.SetValue(doc, node, "font", Font.Name);
      }
      if (textarea == null || textarea.Color != Color)
      {
        parser.SetColor(doc, node, "textcolor", Color);
      }
      if (textarea == null || textarea.DisabledColor != DisabledColor)
      {
        parser.SetColor(doc, node, "disabledcolor", DisabledColor);
      }
      // SpinButton Properties
      p = spinButton.AbsoluteLocation;
      parser.SetInt(doc, node, "SpinPosX", p.X);
      parser.SetInt(doc, node, "SpinPosY", p.Y);
      if (textarea == null || textarea.SpinAlign != SpinAlign)
      {
        parser.SetValue(doc, node, "SpinAlign", SpinAlign.ToString().ToLower());
      }
      if (textarea == null || textarea.SpinSize != SpinSize)
      {
        parser.SetInt(doc, node, "SpinWidth", SpinSize.Width);
        parser.SetInt(doc, node, "SpinHeight", SpinSize.Height);
      }
      if (textarea == null || textarea.SpinColor != SpinColor)
      {
        parser.SetColor(doc, node, "SpinColor", SpinColor);
      }
      if (textarea == null || textarea.TextureUp == null || textarea.TextureUp.Equals(TextureUp) == false)
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
      if (textarea == null || textarea.TextureUpFocus == null || textarea.TextureUpFocus.Equals(TextureUpFocus) == false
        )
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
      if (textarea == null || textarea.TextureDown == null || textarea.TextureDown.Equals(TextureDown) == false)
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
      if (textarea == null || textarea.TextureDownFocus == null ||
          textarea.TextureDownFocus.Equals(TextureDownFocus) == false)
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
      // Mpe
      if (Spacing != 0 || Padding.None == false)
      {
        XmlElement mpenode = doc.CreateElement("mpe");
        node.AppendChild(mpenode);
        if (Spacing != 0)
        {
          parser.SetInt(doc, mpenode, "spacing", Spacing);
        }
        if (Padding.None == false)
        {
          parser.SetPadding(doc, mpenode, "padding", Padding);
        }
      }
    }

    #endregion

    #region Event Handlers

    protected override void OnPaint(PaintEventArgs e)
    {
      e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
    }

    protected override void OnPaddingChanged()
    {
      Prepare();
    }

    #endregion
  }
}