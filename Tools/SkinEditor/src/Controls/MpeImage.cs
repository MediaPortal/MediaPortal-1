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
  public class MpeImage : MpeControl
  {
    #region Variables

    protected Image textureImage;
    protected FileInfo textureFile;
    protected bool keepAspectRatio;
    protected int colorKey;
    protected bool filtered;
    protected bool centered;

    #endregion

    #region Constructors

    public MpeImage() : base()
    {
      MpeLog.Debug("MpeImage()");
      Type = MpeControlType.Image;
      textureFile = null;
      textureImage = null;
      keepAspectRatio = false;
      colorKey = 0;
      filtered = true;
      centered = false;
    }

    public MpeImage(MpeImage image) : base(image)
    {
      MpeLog.Debug("MpeImage(image)");
      textureFile = image.textureFile;
      textureImage = image.textureImage;
      keepAspectRatio = image.keepAspectRatio;
      centered = image.centered;
      filtered = image.filtered;
      colorKey = image.colorKey;
      diffuseColor = image.diffuseColor;
    }

    #endregion

    #region Properties

    public override bool IsReference
    {
      get { return base.IsReference; }
      set
      {
        Texture = null;
        Size = new Size(64, 64);
        Location = new Point(8, 8);
        base.IsReference = value;
      }
    }

    [Category("Textures")]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [Description("This property defines the image.")]
    public virtual FileInfo Texture
    {
      get { return textureFile; }
      set
      {
        if (IsReference)
        {
          MpeLog.Warn("You cannot set a texture value for the reference image.");
          return;
        }
        if (value != null && value.Exists)
        {
          textureFile = value;
          textureImage = new Bitmap(textureFile.FullName);
          Prepare();
          Invalidate(false);
          Modified = true;
          FirePropertyValueChanged("Texture");
        }
        else
        {
          textureFile = null;
          textureImage = null;
          AutoSize = false;
          Invalidate(false);
          Modified = true;
          FirePropertyValueChanged("Texture");
        }
      }
    }

    [Category("Textures")]
    [Browsable(false)]
    public virtual Image TextureImage
    {
      get { return textureImage; }
    }

    [Category("Layout")]
    [DefaultValue(false)]
    [ReadOnly(false)]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property will set the control size equal to the size of the image.")]
    public override bool AutoSize
    {
      get { return base.AutoSize; }
      set { base.AutoSize = value; }
    }

    [Category("Layout")]
    public virtual bool KeepAspectRatio
    {
      get { return keepAspectRatio; }
      set
      {
        if (keepAspectRatio != value)
        {
          keepAspectRatio = value;
          if (value)
          {
            Prepare();
          }
          Modified = true;
          Invalidate(false);
          FirePropertyValueChanged("KeepAspectRatio");
        }
      }
    }

    [Category("Control")]
    public virtual bool Filtered
    {
      get { return filtered; }
      set
      {
        if (filtered != value)
        {
          filtered = value;
          Modified = true;
          FirePropertyValueChanged("Filtered");
        }
      }
    }

    [Category("Layout")]
    public virtual bool Centered
    {
      get { return centered; }
      set
      {
        if (centered != value)
        {
          centered = value;
          Modified = true;
          FirePropertyValueChanged("Centered");
        }
      }
    }

    [Category("Control")]
    public virtual int ColorKey
    {
      get { return colorKey; }
      set
      {
        if (colorKey != value)
        {
          colorKey = value;
          Modified = true;
          FirePropertyValueChanged("ColorKey");
        }
      }
    }

    [ReadOnly(false)]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public override MpeControlPadding Padding
    {
      get { return base.Padding; }
      set { base.Padding = value; }
    }

    [Browsable(false)]
    public override MpeControlAlignment Alignment
    {
      get { return base.Alignment; }
      set { base.Alignment = value; }
    }

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

    #endregion

    #region Methods

    public override MpeControl Copy()
    {
      return new MpeImage(this);
    }

    public override void Destroy()
    {
      if (textureImage != null)
      {
        textureImage.Dispose();
      }
      base.Destroy();
    }

    protected override void PrepareControl()
    {
      if (AutoSize && TextureImage != null)
      {
        Size =
          new Size(TextureImage.Width + Padding.Left + Padding.Right, TextureImage.Height + Padding.Top + Padding.Bottom);
        return;
      }
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeImage.Load()");
      this.parser = parser;
      AutoSize = false;
      base.Load(iterator, parser);
      Texture = parser.GetImageFile(iterator, "texture", Texture);
      tags.Remove("texture");
      if (parser.GetInt(iterator, "width", -1) < 0 || parser.GetInt(iterator, "height", -1) < 0)
      {
        if (Texture == null)
        {
          AutoSize = false;
          Size = new Size(64, 64);
        }
        else
        {
          AutoSize = true;
        }
      }
      Centered = parser.GetBoolean(iterator, "centered", Centered);
      tags.Remove("centered");
      Filtered = parser.GetBoolean(iterator, "filtered", Filtered);
      tags.Remove("filtered");
      KeepAspectRatio = parser.GetBoolean(iterator, "keepaspectratio", KeepAspectRatio);
      tags.Remove("keepaspectratio");
      ColorKey = parser.GetInt(iterator, "colorkey", ColorKey);
      tags.Remove("colorkey");

      Modified = false;
    }

    public override void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      if (doc != null && node != null)
      {
        base.Save(doc, node, parser, reference);
        if (AutoSize)
        {
          parser.RemoveNode(node, "width");
          parser.RemoveNode(node, "height");
        }
        MpeImage image = null;
        if (reference != null && reference is MpeImage)
        {
          image = (MpeImage) reference;
        }
        // For the reference image, the texture should be set to the default screen background
        if (IsReference)
        {
          parser.SetValue(doc, node, "texture", (MpeScreen.TextureBack != null ? MpeScreen.TextureBack.Name : "-"));
        }
        else
        {
          parser.SetValue(doc, node, "texture", (Texture != null ? Texture.Name : "-"));
        }
        if (image == null || image.Centered != Centered)
        {
          parser.SetValue(doc, node, "centered", Centered ? "yes" : "no");
        }
        if (image == null || image.Filtered != Filtered)
        {
          parser.SetValue(doc, node, "filtered", Filtered ? "yes" : "no");
        }
        if (image == null || image.KeepAspectRatio != KeepAspectRatio)
        {
          parser.SetValue(doc, node, "keepaspectratio", KeepAspectRatio ? "yes" : "no");
        }
        if (image == null || image.ColorKey != ColorKey)
        {
          parser.SetInt(doc, node, "colorkey", ColorKey);
        }
      }
    }

    #endregion

    #region Event Handlers

    protected override void OnPaint(PaintEventArgs e)
    {
      if (textureImage == null)
      {
        e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        if (Padding.None == false)
        {
          e.Graphics.DrawRectangle(borderPen, Padding.Left, Padding.Top, Width - Padding.Left - Padding.Right - 1,
                                   Height - Padding.Top - Padding.Bottom - 1);
        }
      }
      else
      {
        if (Masked)
        {
          e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
          if (Padding.None == false)
          {
            e.Graphics.DrawRectangle(borderPen, Padding.Left, Padding.Top, Width - Padding.Left - Padding.Right - 1,
                                     Height - Padding.Top - Padding.Bottom - 1);
          }
        }
        e.Graphics.DrawImage(textureImage, Padding.Left, Padding.Top, Width - Padding.Left - Padding.Right,
                             Height - Padding.Top - Padding.Bottom);
      }
    }

    #endregion
  }
}