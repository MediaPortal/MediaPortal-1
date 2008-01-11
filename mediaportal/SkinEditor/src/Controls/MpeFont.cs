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
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml;
using System.Xml.XPath;

namespace Mpe.Controls
{
  public class MpeFont : MpeResource
  {
    #region Variables

    private string name;
    private string textureWorkDir;
    private FileInfo textureFile;
    private FileInfo textureDataFile;
    private Image texture;
    private float[,] textureCoordinates;
    private Rectangle[] textureData;

    private Font systemFont;

    private int startChar;
    private int endChar;
    private int spacingPerChar;

    #endregion

    #region Constructor

    public MpeFont()
    {
      textureWorkDir = null;
      name = "";
      systemFont = new Font("Arial", 10f);
      spacingPerChar = 0;
      startChar = 32;
      endChar = 256;
    }

    public MpeFont(string name) : this()
    {
      this.name = name;
    }

    public MpeFont(string name, bool generate) : this(name)
    {
      if (generate)
      {
        GenerateTexture();
      }
    }

    public MpeFont(MpeFont font)
    {
      textureWorkDir = font.textureWorkDir;
      name = font.name;
      systemFont = new Font(font.systemFont.FontFamily, font.systemFont.Size, font.systemFont.Style);
      spacingPerChar = font.spacingPerChar;
      startChar = font.startChar;
      endChar = font.endChar;
      textureFile = font.textureFile;
      textureDataFile = font.TextureDataFile;
      if (font.texture != null)
      {
        try
        {
          texture = new Bitmap(font.texture, font.texture.Width, font.texture.Height);
        }
        catch (Exception e)
        {
          MpeLog.Warn(e);
        }
      }
      if (font.textureCoordinates != null)
      {
        textureCoordinates = new float[font.textureCoordinates.GetLength(0),font.textureCoordinates.GetLength(1)];
        for (int i = 0; i < textureCoordinates.GetLength(0); i++)
        {
          for (int j = 0; j < textureCoordinates.GetLength(1); j++)
          {
            textureCoordinates[i, j] = font.textureCoordinates[i, j];
          }
        }
        ConvertTextureData();
      }
    }

    #endregion

    #region Properties

    public string Name
    {
      get { return name; }
      set
      {
        if (value != null && value.Equals(name) == false)
        {
          name = value;
        }
      }
    }

    public int Id
    {
      get { return Name.GetHashCode(); }
      set
      {
        //
      }
    }

    public string Description
    {
      get { return systemFont.ToString(); }
    }

    public int StartCharacter
    {
      get { return startChar; }
      set
      {
        startChar = value;
        GenerateTexture();
      }
    }

    public int EndCharacter
    {
      get { return endChar; }
      set
      {
        endChar = value;
        GenerateTexture();
      }
    }

    public string Family
    {
      get { return systemFont.FontFamily.Name; }
      set { SystemFont = new Font(value, (float) Size); }
    }

    public int Size
    {
      get { return (int) systemFont.Size; }
      set
      {
        if (value != (int) systemFont.Size)
        {
          SystemFont = new Font(Family, (float) value);
        }
      }
    }

    public string Style
    {
      get { return systemFont.Style.ToString(); }
    }

    public Font SystemFont
    {
      get { return systemFont; }
      set
      {
        systemFont = value;
        GenerateTexture();
      }
    }

    public FileInfo TextureFile
    {
      get { return textureFile; }
    }

    public Image Texture
    {
      get { return texture; }
    }

    public FileInfo TextureDataFile
    {
      get { return textureDataFile; }
      set { textureDataFile = value; }
    }

    public Rectangle[] TextureData
    {
      get { return textureData; }
    }

    public float[,] TextureCoordinates
    {
      get { return textureCoordinates; }
    }

    public bool Masked
    {
      get { return false; }
      set
      {
        //
      }
    }

    public bool Modified
    {
      get { return false; }
      set
      {
        //
      }
    }

    #endregion

    #region Methods

    public Rectangle GetStringRectangle(string text, Size maxSize)
    {
      if (Texture != null)
      {
        try
        {
          int x1 = int.MaxValue;
          int x2 = int.MinValue;
          Color c = Color.FromArgb(255, 255, 255, 255);
          Bitmap b = new Bitmap(maxSize.Width, maxSize.Height);
          Graphics g = Graphics.FromImage(b);
          g.Clear(c);
          g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
          g.SmoothingMode = SmoothingMode.HighQuality;
          g.TextContrast = 0;
          g.DrawString(text, SystemFont, Brushes.Black, 0, 0);
          Size size = g.MeasureString(text, SystemFont).ToSize();
          //for (int y = 0; y < size.Height; y++)
          //{
          //  for (int x = 0; x < b.Width; x++)
          //  {
          //    if (b.GetPixel(x, y) != c)
          //    {
          //      if (x < x1)
          //      {
          //        x1 = x;
          //      }
          //      if (x > x2)
          //      {
          //        x2 = x;
          //      }
          //    }
          //  }
          //}
          x1 = 0;
          x2 = b.Width;
          return new Rectangle(x1, 0, x2 - x1 + 1, textureData[0].Height);
        }
        catch (Exception ee)
        {
          MpeLog.Debug(ee);
          MpeLog.Error(ee);
        }
      }
      return Rectangle.Empty;
    }

    public void Prepare()
    {
      //
    }

    public void Destroy()
    {
      MpeLog.Debug("MpeFont.Destroy()");
      if (texture != null)
      {
        texture.Dispose();
      }
    }

    public void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeFont.Load()");
      textureWorkDir = parser.FontDir.FullName;
      name = parser.GetString(iterator, "name", name);
      string family = parser.GetString(iterator, "filename", Family);
      int height = parser.GetInt(iterator, "height", Size);
      string style = parser.GetString(iterator, "style", "");
      FontStyle fs = FontStyle.Regular;
      if (style.IndexOf("Bold") >= 0)
      {
        fs |= FontStyle.Bold;
      }
      if (style.IndexOf("Italic") >= 0)
      {
        fs |= FontStyle.Italic;
      }
      if (style.IndexOf("Underline") >= 0)
      {
        fs |= FontStyle.Underline;
      }
      if (style.IndexOf("Strikeout") >= 0)
      {
        fs |= FontStyle.Strikeout;
      }
      systemFont = new Font(family, (float) height, fs);
      startChar = parser.GetInt(iterator, "start", startChar);
      if (startChar < 32)
      {
        startChar = 32;
      }
      endChar = parser.GetInt(iterator, "end", endChar) + 1;
      if (endChar > 256)
      {
        endChar = 256;
      }
      // Setup Texture Files
      if (textureWorkDir != null)
      {
        textureFile = new FileInfo(textureWorkDir + "\\" + name + "_" + height + ".png");
        textureDataFile = new FileInfo(textureFile.FullName + ".xml");
        bool loaded = false;
        if (textureFile.Exists)
        {
          Bitmap b = null;
          try
          {
            b = new Bitmap(textureFile.FullName);
            texture = new Bitmap(b);
            loaded = true;
          }
          catch (Exception e)
          {
            MpeLog.Warn(e);
          }
          finally
          {
            if (b != null)
            {
              b.Dispose();
            }
          }
        }
        if (loaded && textureDataFile.Exists)
        {
          loaded = false;
          Stream r = null;
          try
          {
            r = File.Open(textureDataFile.FullName, FileMode.Open, FileAccess.Read);
            textureCoordinates = (float[,]) new SoapFormatter().Deserialize(r);
            spacingPerChar = (int) textureCoordinates[endChar - startChar, 0];
            ConvertTextureData();
            loaded = true;
          }
          catch (Exception e)
          {
            MpeLog.Warn(e);
          }
          finally
          {
            if (r != null)
            {
              r.Close();
            }
          }
        }
        if (loaded)
        {
          MpeLog.Info("Loaded font texture and data [" + name + "]");
        }
        else
        {
          GenerateTexture();
        }
      }
    }

    public void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      MpeLog.Debug("MpeFont.Save()");
      textureWorkDir = parser.FontDir.FullName;
      parser.SetValue(doc, node, "name", Name);
      parser.SetValue(doc, node, "filename", Family);
      parser.SetInt(doc, node, "height", Size);
      parser.SetValue(doc, node, "style", Style);
      parser.SetInt(doc, node, "start", StartCharacter);
      parser.SetInt(doc, node, "end", EndCharacter - 1);
      // Setup Texture Files
      if (textureWorkDir != null)
      {
        int size = Size;
        string sTexture = textureWorkDir + "\\" + name + "_" + size + ".png";
        string sData = sTexture + ".xml";
        if (textureFile != null && textureFile.Exists && textureFile.FullName.Equals(sTexture) == false)
        {
          MpeLog.Info("Moving texture file...");
          textureFile.MoveTo(sTexture);
          textureFile.Refresh();
        }
        else
        {
          textureFile = new FileInfo(sTexture);
        }
        if (textureDataFile != null && textureDataFile.Exists && textureDataFile.FullName.Equals(sData) == false)
        {
          MpeLog.Info("Moving texture data file...");
          textureDataFile.MoveTo(sData);
          textureDataFile.Refresh();
        }
        else
        {
          textureDataFile = new FileInfo(sData);
        }
      }
      // Save Texture
      try
      {
        textureFile.Delete();
        texture.Save(textureFile.FullName);
        textureFile.Refresh();
      }
      catch (Exception e)
      {
        MpeLog.Error(e);
        throw new MpeParserException("Error saving font. Could not write to texture file.");
      }
      // Save Texture Data	
      Stream s = null;
      try
      {
        textureDataFile.Delete();
        s = File.Open(textureDataFile.FullName, FileMode.CreateNew, FileAccess.ReadWrite);
        SoapFormatter b = new SoapFormatter();
        b.Serialize(s, (object) textureCoordinates);
        textureDataFile.Refresh();
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        throw new MpeParserException("Error saving font. Could not write to texture data file.");
      }
      finally
      {
        if (s != null)
        {
          s.Close();
        }
      }
    }

    private void GenerateTexture()
    {
      //Create an array to store character dimensions
      textureCoordinates = new float[(10 + endChar - startChar),4];

      // Create a bitmap on which to measure the alphabet
      Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

      Graphics g = Graphics.FromImage(bmp);
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TextRenderingHint = TextRenderingHint.AntiAlias;
      g.TextContrast = 0;

      // Establish the font and texture size
      float textureScale = 1.0f; // Draw fonts into texture without scaling

      // Calculate the dimensions for the smallest power-of-two texture which
      // can hold all the printable characters
      int textureWidth;
      int textureHeight;
      textureWidth = textureHeight = 64;
      for (;;)
      {
        try
        {
          // Measure the alphabet
          MpeLog.Debug("Calculating texture size. Scale=" + textureScale + " Width=" + textureWidth + " Height=" +
                       textureHeight);
          PaintTexture(g, true, textureScale, textureWidth, textureHeight);
        }
        catch (InvalidOperationException)
        {
          // Scale up the texture size and try again
          textureWidth *= 2;
          textureHeight *= 2;
          continue;
        }

        break;
      }
      MpeLog.Debug("Calculated texture size. Scale = [" + textureScale + "] Width = [" + textureWidth + "] Height = [" +
                   textureHeight + "]");
      bmp.Dispose();

      // Release the bitmap used for measuring and create one for drawing      
      bmp = new Bitmap(textureWidth, textureHeight, PixelFormat.Format32bppArgb);
      g = Graphics.FromImage(bmp);
      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TextRenderingHint = TextRenderingHint.AntiAlias;
      g.TextContrast = 0;

      // Draw the alphabet
      PaintTexture(g, false, textureScale, textureWidth, textureHeight);
      textureCoordinates[endChar - startChar, 0] = spacingPerChar;

      // Create a new texture and data for the font from the bitmap we just created
      try
      {
        if (texture != null)
        {
          texture.Dispose();
        }
        texture = new Bitmap(bmp);
        ConvertTextureData();
        MpeLog.Info("Generated font texture and data [" + name + "]");
      }
      catch (Exception e)
      {
        MpeLog.Error(e);
      }
      finally
      {
        if (bmp != null)
        {
          bmp.Dispose();
        }
        if (g != null)
        {
          g.Dispose();
        }
      }
    }

    private void PaintTexture(Graphics g, bool measureOnly, float textureScale, int textureWidth, int textureHeight)
    {
      string str;
      float x = 0;
      float y = 0;
      Point p = Point.Empty;
      Size size = new Size(p);

      // Calculate the spacing between characters based on line height
      size = g.MeasureString(" ", systemFont).ToSize();
      x = spacingPerChar = (int) Math.Ceiling(size.Height*0.3);

      for (char c = (char) startChar; c < (char) endChar; c++)
      {
        str = c.ToString();
        // We need to do some things here to get the right sizes.  The default implemententation of MeasureString
        // will return a resolution independant size.  For our height, this is what we want.  However, for our width, we 
        // want a resolution dependant size.
        Size resSize = g.MeasureString(str, systemFont).ToSize();
        size.Height = resSize.Height + 1;

        // Now the Resolution independent width
        if (c != ' ')
        {
          // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
          resSize = g.MeasureString(str, systemFont, p, StringFormat.GenericTypographic).ToSize();
          size.Width = resSize.Width;
        }
        else
        {
          size.Width = resSize.Width;
        }

        if ((x + size.Width + spacingPerChar) > textureWidth)
        {
          x = spacingPerChar;
          y += size.Height;
        }

        // Make sure we have room for the current character
        if ((y + size.Height) > textureHeight)
        {
          throw new InvalidOperationException("Texture too small for alphabet");
        }

        if (measureOnly == false)
        {
          if (c != ' ')
          {
            // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
            g.DrawString(str, systemFont, Brushes.White, new Point((int) x, (int) y), StringFormat.GenericTypographic);
          }
          else
          {
            g.DrawString(str, systemFont, Brushes.White, new Point((int) x, (int) y));
          }
          textureCoordinates[c - startChar, 0] = ((float) (x + 0 - spacingPerChar))/textureWidth;
          textureCoordinates[c - startChar, 1] = ((float) (y + 0 + 0))/textureHeight;
          textureCoordinates[c - startChar, 2] = ((float) (x + size.Width + spacingPerChar))/textureWidth;
          textureCoordinates[c - startChar, 3] = ((float) (y + size.Height + 0))/textureHeight;
        }

        x += size.Width + (2*spacingPerChar);
      }
    }

    private void ConvertTextureData()
    {
      textureData = new Rectangle[textureCoordinates.GetLength(0)];
      for (int i = 0; i < textureData.Length; i++)
      {
        textureData[i] = new Rectangle(
          (int) (texture.Width*textureCoordinates[i, 0]),
          (int) (texture.Height*textureCoordinates[i, 1]),
          (int) (texture.Width*(textureCoordinates[i, 2] - textureCoordinates[i, 0])),
          (int) (texture.Height*(textureCoordinates[i, 3] - textureCoordinates[i, 1]))
          );
      }
    }

    public override string ToString()
    {
      return Name;
    }

    #endregion
  }


  public class MpeFontConverter : TypeConverter
  {
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
      if (MediaPortalEditor.Global.Parser != null)
      {
        return new StandardValuesCollection(MediaPortalEditor.Global.Parser.FontNames);
      }
      return base.GetStandardValues(context);
    }

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
      return true;
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
      {
        return true;
      }
      return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value.GetType() == typeof(string))
      {
        if (MediaPortalEditor.Global.Parser != null)
        {
          return MediaPortalEditor.Global.Parser.GetFont((string) value);
        }
      }
      return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      //MpeLog.Debug("CanConvertTo(" + destinationType.FullName + ")");
      if (destinationType == typeof(string))
      {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                     Type destinationType)
    {
      //MpeLog.Debug("ConvertTo(" + destinationType.FullName + "," + value.GetType().FullName + ")");
      if (destinationType == typeof(string))
      {
        if (value.GetType() == typeof(MpeFont))
        {
          return ((MpeFont) value).Name;
        }
        else if (value.GetType() == typeof(string))
        {
          return (string) value;
        }
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
}