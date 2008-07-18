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
using System.Globalization;

namespace Mpe.Controls.Properties
{
  [TypeConverter(typeof(MpeControlPaddingConverter))]
  [
    Description(
      "Set the padding around the control.\n\nCustom: Left, Right, Top, Bottom\n[Example] 2, 3, 4, 5\n\nSimple: Left and Right, Top and Bottom\n[Example] 2, 4\n\nAll: Left and Right and Top and Bottom\n[Example] 2"
      )]
  public class MpeControlPadding
  {
    #region Variables

    private int top;
    private int left;
    private int right;
    private int bottom;
    private bool readOnly;

    #endregion

    #region Events and Delegates

    public delegate void PaddingChangedHandler();


    public event PaddingChangedHandler PaddingChanged;

    #endregion

    #region Constructors

    public MpeControlPadding()
    {
      Clear();
    }

    public MpeControlPadding(MpeControlPadding padding)
    {
      Set(padding.Left, padding.Right, padding.Top, padding.Bottom);
    }

    public MpeControlPadding(int padding)
    {
      Set(padding);
    }

    public MpeControlPadding(int lr, int tb)
    {
      Set(lr, tb);
    }

    public MpeControlPadding(int l, int r, int t, int b)
    {
      Set(l, r, t, b);
    }

    public MpeControlPadding(Point p)
    {
      Set(p);
    }

    #endregion

    #region Properties

    [RefreshProperties(RefreshProperties.Repaint)]
    public int Top
    {
      get { return top; }
      set
      {
        if (readOnly == false && top != value)
        {
          //top = value > 0 ? value : 0;
          top = value;
          if (PaddingChanged != null)
          {
            PaddingChanged();
          }
        }
      }
    }

    [RefreshProperties(RefreshProperties.Repaint)]
    public int Right
    {
      get { return right; }
      set
      {
        if (readOnly == false && right != value)
        {
          right = value > 0 ? value : 0;
          if (PaddingChanged != null)
          {
            PaddingChanged();
          }
        }
      }
    }

    [RefreshProperties(RefreshProperties.Repaint)]
    public int Bottom
    {
      get { return bottom; }
      set
      {
        if (readOnly == false && bottom != value)
        {
          //bottom = value > 0 ? value : 0;
          bottom = value;
          if (PaddingChanged != null)
          {
            PaddingChanged();
          }
        }
      }
    }

    [RefreshProperties(RefreshProperties.Repaint)]
    public int Left
    {
      get { return left; }
      set
      {
        if (readOnly == false && left != value)
        {
          left = value > 0 ? value : 0;
          if (PaddingChanged != null)
          {
            PaddingChanged();
          }
        }
      }
    }

    [Browsable(false)]
    public bool All
    {
      get
      {
        if (left == right && left == top && left == bottom)
        {
          return true;
        }
        return false;
      }
    }

    [Browsable(false)]
    public bool Simple
    {
      get
      {
        if (left == right && top == bottom)
        {
          return true;
        }
        return false;
      }
    }

    [Browsable(false)]
    public bool ReadOnly
    {
      get { return readOnly; }
      set { readOnly = value; }
    }

    [Browsable(false)]
    public bool None
    {
      get
      {
        if (left == 0 && right == 0 && bottom == 0 && top == 0)
        {
          return true;
        }
        return false;
      }
    }

    [Browsable(false)]
    public int Width
    {
      get { return left + right; }
    }

    [Browsable(false)]
    public int Height
    {
      get { return top + bottom; }
    }

    #endregion

    #region Methods

    public void Clear()
    {
      top = 0;
      right = 0;
      bottom = 0;
      left = 0;
    }

    public void Set(int padding)
    {
      Top = padding;
      Right = padding;
      Bottom = padding;
      Left = padding;
    }

    public void Set(int lr, int tb)
    {
      Top = tb;
      Bottom = tb;
      Left = lr;
      Right = lr;
    }

    public void Set(int l, int r, int t, int b)
    {
      Top = t;
      Right = r;
      Bottom = b;
      Left = l;
    }

    public void Set(Point p)
    {
      Top = p.Y;
      Bottom = p.Y;
      Left = p.X;
      Right = p.X;
    }

    public override bool Equals(object obj)
    {
      if (obj != null && obj is MpeControlPadding)
      {
        MpeControlPadding p = (MpeControlPadding) obj;
        if (p.Left == Left && p.Right == Right && p.Top == Top && p.Bottom == Bottom)
        {
          return true;
        }
      }
      return false;
    }

    public override int GetHashCode()
    {
      string s = left.ToString() + right.ToString() + bottom.ToString() + top.ToString();
      return s.GetHashCode();
    }

    public Point ToPoint()
    {
      return new Point(Left, Top);
    }

    #endregion
  }


  //internal class MpeControlPaddingConverter : ExpandableObjectConverter {
  internal class MpeControlPaddingConverter : StringConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == typeof(MpeControlPadding) || destinationType == typeof(string))
      {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                     Type destinationType)
    {
      if (destinationType == typeof(String) && value is MpeControlPadding)
      {
        MpeControlPadding padding = (MpeControlPadding) value;
        if (padding.All)
        {
          return padding.Left.ToString();
        }
        else if (padding.Simple)
        {
          return padding.Left + ", " + padding.Top;
        }
        else
        {
          return padding.Left + ", " + padding.Right + ", " + padding.Top + ", " + padding.Bottom;
        }
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(String) || sourceType == typeof(string))
      {
        return true;
      }
      return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value is String)
      {
        string s = (string) value;
        if (s.IndexOf(",") < 0)
        {
          try
          {
            int i = int.Parse(s);
            return new MpeControlPadding(i);
          }
          catch
          {
            MpeLog.Warn("Invalid format for padding property.");
            throw new ArgumentException("Invalid format for padding property [" + s + "]");
          }
        }
        else
        {
          MpeControlPadding padding = new MpeControlPadding();
          try
          {
            string[] ss = s.Split(',');
            if (ss == null || (ss.Length != 4 && ss.Length != 2))
            {
              MpeLog.Warn("Invalid format for padding property [" + s + "]");
              throw new ArgumentException("Invalid format for padding property [" + s + "]");
            }
            if (ss.Length == 4)
            {
              padding.Left = int.Parse(ss[0].Trim());
              padding.Right = int.Parse(ss[1].Trim());
              padding.Top = int.Parse(ss[2].Trim());
              padding.Bottom = int.Parse(ss[3].Trim());
            }
            else
            {
              padding.Left = int.Parse(ss[0].Trim());
              padding.Right = padding.Left;
              padding.Top = int.Parse(ss[1].Trim());
              padding.Bottom = padding.Top;
            }
            return padding;
          }
          catch (Exception ee)
          {
            MpeLog.Debug(ee);
            MpeLog.Warn("Invalid format for padding property [" + s + "]");
            throw new ArgumentException("Invalid format for padding property [" + s + "]");
          }
        }
      }
      return base.ConvertFrom(context, culture, value);
    }
  }
}