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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
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
  public class MpeLabel : MpeControl
  {
    #region Variables

    protected MpeFont font;
    protected string text;
    protected string textValue;
    protected SolidBrush disabledBrush;
    protected Size textSize;
    protected Point textOffset;
    protected bool lookup;

    #endregion

    #region Constructors

    public MpeLabel() : base()
    {
      MpeLog.Debug("MpeLabel()");
      Type = MpeControlType.Label;
      disabledBrush = new SolidBrush(Color.Gray);
      lookup = true;
      font = new MpeFont();
      text = "";
      textValue = "";
      textSize = Size.Empty;
      textOffset = Point.Empty;
      onLeft = 0;
      onRight = 0;
      onUp = 0;
      onDown = 0;
    }

    public MpeLabel(MpeLabel label) : base(label)
    {
      MpeLog.Debug("MpeLabel(label)");
      font = label.font;
      text = label.text;
      textValue = label.textValue;
      disabledBrush = (SolidBrush) label.disabledBrush.Clone();
      textSize = label.textSize;
      textOffset = label.textOffset;
      lookup = label.lookup;
      onLeft = 0;
      onRight = 0;
      onUp = 0;
      onDown = 0;
    }

    #endregion

    #region Properties

    [Category("Layout")]
    [Description("The Label will be automatically sized to the width and height of the text.")]
    [RefreshProperties(RefreshProperties.Repaint)]
    [ReadOnly(true)]
    public override bool AutoSize
    {
      get { return base.AutoSize; }
      set { base.AutoSize = value; }
    }

    [Category("Layout")]
    [Browsable(true)]
    [ReadOnly(true)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public override MpeControlPadding Padding
    {
      get { return base.Padding; }
      set { base.Padding = value; }
    }

    [Category("Labels")]
    [ReadOnly(false)]
    public override MpeControlAlignment Alignment
    {
      get { return base.Alignment; }
      set { base.Alignment = value; }
    }

    [Category("Labels")]
    [Browsable(true)]
    [RefreshProperties(RefreshProperties.Repaint)]
    [Editor(typeof(MpeStringEditor), typeof(UITypeEditor))]
    public override string Text
    {
      get { return text; }
      set
      {
        if (text != null || text.Equals(value) == false)
        {
          text = value;
          if (Lookup)
          {
            try
            {
              int i = int.Parse(value);
              textValue = Parser.GetString("English", int.Parse(text));
            }
            catch
            {
              textValue = value;
            }
          }
          else
          {
            textValue = value;
          }
          Prepare();
          Invalidate(false);
          Modified = true;
          FirePropertyValueChanged("Text");
        }
      }
    }

    [Browsable(false)]
    public bool Lookup
    {
      get { return lookup; }
      set { lookup = value; }
    }

    [Category("Labels")]
    public virtual Color TextColor
    {
      get { return textBrush.Color; }
      set
      {
        if (textBrush.Color != value)
        {
          textBrush.Color = value;
          Invalidate(false);
          Modified = true;
          FirePropertyValueChanged("TextColor");
        }
      }
    }

    [Category("Labels")]
    [Description("")]
    public virtual Color DisabledColor
    {
      get { return disabledBrush.Color; }
      set
      {
        if (disabledBrush.Color != value)
        {
          disabledBrush.Color = value;
          Invalidate(false);
          Modified = true;
          FirePropertyValueChanged("DisabledColor");
        }
      }
    }

    [TypeConverter(typeof(MpeFontConverter))]
    [Category("Labels")]
    [Description("The font that will be used to render the button label.")]
    [RefreshProperties(RefreshProperties.Repaint)]
    public new virtual MpeFont Font
    {
      get { return font; }
      set
      {
        font = value;
        Prepare();
        Modified = true;
        Invalidate(false);
        FirePropertyValueChanged("Font");
      }
    }

    [Browsable(false)]
    public override bool Focused
    {
      get { return base.Focused; }
      set { base.Focused = value; }
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

    #endregion

    #region Methods

    protected override void PrepareControl()
    {
      MpeLog.Debug("MpeLabel.Prepare()");
      if (font != null)
      {
        Rectangle r0 = Rectangle.Empty;
        if (screen != null)
        {
          r0 = font.GetStringRectangle(textValue, screen.Size);
        }
        else
        {
          r0 = font.GetStringRectangle(textValue, new Size(640, 480));
        }
        textSize = r0.Size;
        if (Alignment == MpeControlAlignment.Left)
        {
          textOffset = new Point(Padding.Left - r0.X, Padding.Top - r0.Y);
        }
        else
        {
          textOffset = new Point(Width - Padding.Right - textSize.Width - r0.X, Padding.Top - r0.Y);
        }
        if (AutoSize && (Type == MpeControlType.Label || Type == MpeControlType.FadeLabel))
        {
          int r = Right;
          Size = new Size(textSize.Width + Padding.Left + Padding.Right, textSize.Height + Padding.Top + Padding.Bottom);
          if (alignment == MpeControlAlignment.Right)
          {
            Left = r - Width;
          }
        }
      }
      MpeLog.Debug("MpeLabel.Prepare().end");
    }

    public override MpeControl Copy()
    {
      return new MpeLabel(this);
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeLabel.Load()");
      base.Load(iterator, parser);
      this.parser = parser;
      Text = parser.GetString(iterator, "label", "label");
      tags.Remove("label");
      Alignment = parser.GetAlignment(iterator, "align", Alignment);
      tags.Remove("align");
      if (Alignment == MpeControlAlignment.Right)
      {
        Left = Left - Width;
      }
      TextColor = parser.GetColor(iterator, "textcolor", TextColor);
      tags.Remove("textcolor");
      DisabledColor = parser.GetColor(iterator, "disabledcolor", DisabledColor);
      tags.Remove("disabledcolor");
      Font = parser.GetFont(iterator, "font", Font);
      tags.Remove("font");
      if (Type == MpeControlType.Label || Type == MpeControlType.FadeLabel)
      {
        AutoSize = true;
      }
      Prepare();
      Modified = false;
    }

    public override void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      base.Save(doc, node, parser, reference);

      MpeLabel label = null;
      if (reference != null && reference is MpeLabel)
      {
        label = (MpeLabel) reference;
      }

      // Fix the Left Position
      if (Alignment == MpeControlAlignment.Right)
      {
        parser.SetInt(doc, node, "posX", Right);
      }
      // Fix the Width and Height
      parser.RemoveNode(node, "width");
      parser.RemoveNode(node, "height");
      // Text
      parser.SetValue(doc, node, "label", Text);
      // Font
      if (label == null || !label.Font.Name.Equals(Font.Name))
      {
        parser.SetValue(doc, node, "font", Font.Name);
      }
      // Alignment
      if (label == null || label.Alignment != Alignment)
      {
        parser.SetValue(doc, node, "align", Alignment.ToString().ToLower());
      }
      // Color
      if (label == null || label.TextColor != TextColor)
      {
        parser.SetColor(doc, node, "textcolor", TextColor);
      }
      // DisabledColor
      if (label == null || label.DisabledColor != DisabledColor)
      {
        parser.SetColor(doc, node, "disabledcolor", DisabledColor);
      }
    }

    #endregion

    #region Event Handlers

    protected override void OnPaint(PaintEventArgs e)
    {
      if (Masked)
      {
        if (Type == MpeControlType.Label)
        {
          e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }
        if (Padding.None == false)
        {
          e.Graphics.DrawRectangle(borderPen, Padding.Left, Padding.Top, Width - Padding.Left - Padding.Right - 1,
                                   Height - Padding.Top - Padding.Bottom - 1);
        }
      }
      if (textValue != null && textValue.Length > 0)
      {
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        if (Enabled)
        {
          e.Graphics.DrawString(textValue, font.SystemFont, textBrush, textOffset);
        }
        else
        {
          e.Graphics.DrawString(textValue, font.SystemFont, disabledBrush, textOffset);
        }
      }
    }

    #endregion
  }
}