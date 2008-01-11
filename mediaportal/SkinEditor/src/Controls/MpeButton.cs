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
  /// This class implements the behaviour for a MediaPortal button object.  It contains
  /// all of the properties and events required for the screen and control editor
  /// to manipulate button objects.
  /// </summary>
  public class MpeButton : MpeLabel
  {
    #region Variables

    protected FileInfo textureFocus;
    protected Image textureFocusImage;
    protected FileInfo textureNoFocus;
    protected Image textureNoFocusImage;
    protected int hyperlink;

    #endregion

    #region Constructors

    public MpeButton() : base()
    {
      MpeLog.Debug("MpeButton()");
      Type = MpeControlType.Button;
    }

    public MpeButton(MpeButton button) : base(button)
    {
      MpeLog.Debug("MpeButton(button)");
      Type = MpeControlType.Button;
      textureFocus = button.textureFocus;
      textureFocusImage = button.textureFocusImage;
      textureNoFocus = button.textureNoFocus;
      textureNoFocusImage = button.textureNoFocusImage;
      diffuseColor = button.diffuseColor;
    }

    #endregion

    #region Properties

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureFocus
    {
      get { return textureFocus; }
      set
      {
        if (value != textureFocus)
        {
          textureFocus = value;
          if (value != null)
          {
            textureFocusImage = new Bitmap(textureFocus.FullName);
            Prepare();
          }
          else
          {
            textureFocusImage = null;
            AutoSize = false;
          }
          Invalidate(false);
          Modified = true;
          FirePropertyValueChanged("TextureFocus");
        }
      }
    }

    [Browsable(false)]
    public Image TextureFocusImage
    {
      get { return textureFocusImage; }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it does not have focus.")]
    public FileInfo TextureNoFocus
    {
      get { return textureNoFocus; }
      set
      {
        if (value != textureNoFocus)
        {
          textureNoFocus = value;
          if (value != null)
          {
            textureNoFocusImage = new Bitmap(textureNoFocus.FullName);
            Prepare();
          }
          else
          {
            textureNoFocusImage = null;
            AutoSize = false;
          }
          Invalidate(false);
          Modified = true;
          FirePropertyValueChanged("TextureNoFocus");
        }
      }
    }

    [Browsable(false)]
    public Image TextureNoFocusImage
    {
      get { return textureNoFocusImage; }
    }

    [Category("Layout")]
    [DefaultValue(false)]
    [ReadOnly(false)]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [
      Description(
        "This property will set the control size equal to the size of the texture.  The control will remain Location Locked while this property remains true."
        )]
    public override bool AutoSize
    {
      get { return base.AutoSize; }
      set { base.AutoSize = value; }
    }

    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public override bool Focused
    {
      get { return base.Focused; }
      set { base.Focused = value; }
    }

    [Category("Labels")]
    [ReadOnly(false)]
    [Browsable(true)]
    public override MpeControlPadding Padding
    {
      get { return base.Padding; }
      set { base.Padding = value; }
    }

    [Category("Labels")]
    [ReadOnly(false)]
    [Browsable(true)]
    [Description("The color of the text when the button does not have the focus/not selected")]
    public override  Color DisabledColor
    {
      get { return base.DisabledColor; }
      set { base.DisabledColor = value; }
    }
   
    [Category("Labels")]
    [ReadOnly(false)]
    [Browsable(true)]
    [Description("The color of the text when the button has the focus/is selected")]
    public override Color TextColor
    {
      get { return base.TextColor; }
      set { base.TextColor = value; }
    }

    [Category("Actions")]
    [Description("The id of the xml skin page that MP will load when the button is pushed")]
    public virtual int Hyperlink
    {
      get { return hyperlink; }
      set
      {
        if (hyperlink != value)
        {
          hyperlink = value;
          Modified = true;
          FirePropertyValueChanged("Hyperlink");
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

    public override MpeControl Copy()
    {
      return new MpeButton(this);
    }

    protected override void PrepareControl()
    {
      base.PrepareControl();
      if (AutoSize && textureNoFocusImage != null)
      {
        Size = textureNoFocusImage.Size;
      }
      else if (AutoSize && textureFocusImage != null)
      {
        Size = textureFocusImage.Size;
      }
    }

    public override void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      base.Save(doc, node, parser, reference);
      MpeButton button = null;
      if (reference != null)
      {
        button = (MpeButton) reference;
      }
      // Size
      if (button == null || button.Size != Size)
      {
        parser.SetInt(doc, node, "width", Width);
        parser.SetInt(doc, node, "height", Height);
      }
      // hyperlink
      if (button == null || button.Hyperlink != Hyperlink)
      {
        parser.SetInt(doc, node, "hyperlink", Hyperlink);
      }
      // DiffuseColor
      if (button == null || button.DiffuseColor != DiffuseColor)
      {
        parser.SetColor(doc, node, "colordiffuse", DiffuseColor);
      }
      // TextOffsetX
      if (button == null || button.Padding.Left != Padding.Left)
      {
        parser.SetInt(doc, node, "textXOff", Padding.Left);
      }
      // TextOffsetY
      if (button == null || button.Padding.Top != Padding.Top)
      {
        parser.SetInt(doc, node, "textYOff", Padding.Top);
      }
      // TextureFocus
      if (button == null || button.TextureFocus == null || button.TextureFocus.Equals(TextureFocus) == false)
      {
        if (TextureFocus == null)
        {
          parser.SetValue(doc, node, "textureFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureFocus", TextureFocus.Name);
        }
      }
      // TextureNoFocus
      if (button == null || button.TextureNoFocus == null || button.TextureNoFocus.Equals(TextureNoFocus) == false)
      {
        if (TextureNoFocus == null)
        {
          parser.SetValue(doc, node, "textureNoFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureNoFocus", TextureNoFocus.Name);
        }
      }
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeButton.Load()");
      base.Load(iterator, parser);
      this.parser = parser;
      if (Text != null && Text.Equals("-"))
      {
        Text = "";
      }
      AutoSize = false;
      TextureFocus = parser.GetImageFile(iterator, "textureFocus", TextureFocus);
      tags.Remove("textureFocus");
      TextureNoFocus = parser.GetImageFile(iterator, "textureNoFocus", TextureNoFocus);
      tags.Remove("textureNoFocus");
      if (TextureNoFocusImage != null)
      {
        if (TextureNoFocusImage.Width == Width && TextureNoFocusImage.Height == Height)
        {
          MpeLog.Debug("TextureNoFocus = " + TextureNoFocusImage.Size + " AutoSize enabled");
          AutoSize = true;
        }
      }
      else if (TextureNoFocusImage == null && TextureFocusImage != null)
      {
        if (TextureFocusImage.Width == Width && TextureFocusImage.Height == Height)
        {
          MpeLog.Debug("TextureFocus = " + TextureFocusImage.Size + " AutoSize enabled");
          AutoSize = true;
        }
      }
      Padding.Left = Padding.Right = parser.GetInt(iterator, "textXOff", Padding.Left);
      Padding.Top = Padding.Bottom = parser.GetInt(iterator, "textYOff", Padding.Top);
      Hyperlink = parser.GetInt(iterator, "hyperlink", 0);
      tags.Remove("textXOff");
      tags.Remove("textYOff");
      tags.Remove("hyperlink");
      PrepareControl();
      Modified = false;
    }

    #endregion

    #region Event Handlers

    protected override void OnPaint(PaintEventArgs e)
    {
      if (Focused)
      {
        if (textureFocusImage == null)
        {
          e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }
        else
        {
          e.Graphics.DrawImage(textureFocusImage, 0, 0, Width, Height);
        }
      }
      else
      {
        if (textureNoFocusImage == null)
        {
          e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }
        else
        {
          e.Graphics.DrawImage(textureNoFocusImage, 0, 0, Width, Height);
        }
      }
      base.OnPaint(e);
    }

    #endregion
  }
}