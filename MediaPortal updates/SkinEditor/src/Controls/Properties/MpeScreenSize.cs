using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Mpe.Controls.Properties
{
  public class MpeScreenSize
  {
    private string name;
    private bool widescreen;
    private Size size;

    public MpeScreenSize(string name, bool widescreen, int width, int height)
    {
      size = new Size();
      Name = name;
      WideScreen = widescreen;
      Width = width;
      Height = height;
    }

    public static MpeScreenSize PAL = new MpeScreenSize("PAL", false, 720, 576);
    public static MpeScreenSize NTSC = new MpeScreenSize("NTSC", false, 704, 480);
    public static MpeScreenSize SDTV = new MpeScreenSize("SDTV", false, 720, 480);
    public static MpeScreenSize EDTV = new MpeScreenSize("EDTV", true, 852, 480);
    public static MpeScreenSize HDTV = new MpeScreenSize("HDTV", true, 1280, 720);

    public static MpeScreenSize FromResolution(int horizontal, int vertical)
    {
      Size size = new Size(horizontal, vertical);
      if (size == NTSC.Size)
      {
        return NTSC;
      }
      else if (size == SDTV.Size)
      {
        return SDTV;
      }
      else if (size == EDTV.Size)
      {
        return EDTV;
      }
      else if (size == HDTV.Size)
      {
        return HDTV;
      }
      else
      {
        return PAL;
      }
    }

    public int Width
    {
      get { return size.Width; }
      set { size.Width = value; }
    }

    public int Height
    {
      get { return size.Height; }
      set { size.Height = value; }
    }

    public Size Size
    {
      get { return size; }
      set { size = value; }
    }

    public string Name
    {
      get { return name; }
      set { name = value; }
    }

    public bool WideScreen
    {
      get { return widescreen; }
      set { widescreen = value; }
    }

    public override string ToString()
    {
      string s = Name;
      if (widescreen)
      {
        s += " - 16:9 - ";
      }
      else
      {
        s += " - 4:3 - ";
      }
      s += Width + "x" + Height;
      return s;
    }
  }


  internal class MpeScreenSizeConverter : TypeConverter
  {
    private ArrayList values;

    public MpeScreenSizeConverter()
    {
      values = new ArrayList();
      values.Add(MpeScreenSize.PAL);
      values.Add(MpeScreenSize.NTSC);
      values.Add(MpeScreenSize.SDTV);
      values.Add(MpeScreenSize.EDTV);
      values.Add(MpeScreenSize.HDTV);
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
      return new StandardValuesCollection(values);
    }

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
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
        string s = (string) value;
        if (s.StartsWith("NTSC"))
        {
          return MpeScreenSize.NTSC;
        }
        else if (s.StartsWith("SDTV"))
        {
          return MpeScreenSize.SDTV;
        }
        else if (s.StartsWith("EDTV"))
        {
          return MpeScreenSize.EDTV;
        }
        else if (s.StartsWith("HDTV"))
        {
          return MpeScreenSize.HDTV;
        }
        else
        {
          return MpeScreenSize.PAL;
        }
      }
      return base.ConvertFrom(context, culture, value);
    }
  }
}