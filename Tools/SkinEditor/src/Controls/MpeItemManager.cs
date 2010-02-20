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
using System.Globalization;

namespace Mpe.Controls
{
  [TypeConverter(typeof(MpeItemManagerConverter))]
  public class MpeItemManager
  {
    #region Variables

    private string first;
    private string last;
    private string interval;
    private string digits;
    private MpeItemCollection values;

    #endregion

    public MpeItemManager()
    {
      values = new MpeItemCollection();
      values.Type = MpeItemType.Text;
      Type = MpeItemType.Integer;
    }

    public MpeItemManager(MpeItemManager manager) : this()
    {
      values = new MpeItemCollection(manager.values);
      first = manager.first;
      last = manager.last;
      interval = manager.interval;
      digits = manager.digits;
    }


    public delegate void TypeChangingHandler(MpeItemEventArgs e);


    public event TypeChangingHandler TypeChanging;

    [RefreshPropertiesAttribute(RefreshProperties.All)]
    public string First
    {
      get { return first; }
      set
      {
        if (first != value)
        {
          switch (Type)
          {
            case MpeItemType.Integer:
              try
              {
                int i = int.Parse(value);
                first = i.ToString("D" + digits);
                UpdateCollection();
              }
              catch (Exception e)
              {
                MpeLog.Warn(e);
              }
              break;
            case MpeItemType.Float:
              try
              {
                double d = double.Parse(value);
                first = d.ToString("F" + digits);
                UpdateCollection();
              }
              catch (Exception e)
              {
                MpeLog.Warn(e);
              }
              break;
          }
        }
      }
    }

    [RefreshPropertiesAttribute(RefreshProperties.All)]
    public string Last
    {
      get { return last; }
      set
      {
        if (last.Equals(value) == false)
        {
          switch (Type)
          {
            case MpeItemType.Integer:
              try
              {
                int i = int.Parse(value);
                last = i.ToString("D" + digits);
                UpdateCollection();
              }
              catch (Exception e)
              {
                MpeLog.Warn(e);
              }
              break;
            case MpeItemType.Float:
              try
              {
                double d = double.Parse(value);
                last = d.ToString("F" + digits);
                last = value;
                UpdateCollection();
              }
              catch (Exception e)
              {
                MpeLog.Warn(e);
              }
              break;
          }
        }
      }
    }

    [RefreshPropertiesAttribute(RefreshProperties.All)]
    public string Interval
    {
      get { return interval; }
      set
      {
        if (interval != value)
        {
          switch (Type)
          {
            case MpeItemType.Integer:
              try
              {
                int.Parse(value);
                interval = value;
                UpdateCollection();
              }
              catch (Exception e)
              {
                MpeLog.Warn(e);
              }
              break;
            case MpeItemType.Float:
              try
              {
                double.Parse(value);
                interval = value;
                UpdateCollection();
              }
              catch (Exception e)
              {
                MpeLog.Warn(e);
              }
              break;
            case MpeItemType.Text:
              interval = "-";
              break;
          }
        }
      }
    }

    [RefreshPropertiesAttribute(RefreshProperties.All)]
    public string Digits
    {
      get { return digits; }
      set
      {
        if (Type == MpeItemType.Text)
        {
          digits = "-";
          return;
        }
        try
        {
          int.Parse(value);
          digits = value;
          if (Type == MpeItemType.Integer)
          {
            int i = int.Parse(last);
            last = i.ToString("D" + digits);
            i = int.Parse(first);
            first = i.ToString("D" + digits);
          }
          else
          {
            double d = double.Parse(last);
            last = d.ToString("F" + digits);
            d = double.Parse(first);
            first = d.ToString("F" + digits);
          }
          UpdateCollection();
        }
        catch (Exception e)
        {
          MpeLog.Warn(e);
        }
      }
    }

    [RefreshPropertiesAttribute(RefreshProperties.All)]
    public MpeItemType Type
    {
      get { return values.Type; }
      set
      {
        if (values.Type != value)
        {
          if (TypeChanging != null)
          {
            MpeItemEventArgs e = new MpeItemEventArgs(values.Type, value);
            TypeChanging(e);
            if (e.CancelTypeChange)
            {
              return;
            }
          }
          values.Type = value;
          switch (value)
          {
            case MpeItemType.Integer:
              first = "1";
              last = "9";
              interval = "1";
              Digits = "1";
              break;
            case MpeItemType.Float:
              first = "0.0";
              last = "1.0";
              interval = "0.1";
              Digits = "2";
              break;
            case MpeItemType.Text:
              digits = "-";
              first = "-";
              last = "-";
              interval = "-";
              break;
          }
          UpdateCollection();
        }
      }
    }

    public MpeItemCollection Values
    {
      get { return values; }
      set
      {
        values = value;
        MpeLog.Debug("Collection was modified");
      }
    }

    private void UpdateCollection()
    {
      values.Clear();
      switch (Type)
      {
        case MpeItemType.Integer:
          int nFirst = int.Parse(First);
          int nLast = int.Parse(Last);
          int nInterval = int.Parse(Interval);
          if (nFirst <= nLast)
          {
            for (int i = nFirst; i <= nLast; i += nInterval)
            {
              values.Add(Type, i.ToString("D" + Digits));
            }
          }
          else
          {
            for (int i = nFirst; i >= nLast; i -= nInterval)
            {
              values.Add(Type, i.ToString("D" + Digits));
            }
          }
          break;
        case MpeItemType.Float:
          double fFirst = double.Parse(First);
          double fLast = double.Parse(Last);
          double fInterval = double.Parse(Interval);
          double f = fFirst;
          if (fFirst <= fLast)
          {
            while (f <= fLast)
            {
              values.Add(Type, f.ToString("F" + Digits));
              f += fInterval;
            }
            f = double.Parse(values[values.Count - 1].Value);
            if (f != fLast)
            {
              values.Add(Type, fLast.ToString("F" + Digits));
            }
          }
          else
          {
            f = fFirst;
            while (f >= fLast)
            {
              values.Add(Type, f.ToString("F" + Digits));
              f -= fInterval;
            }
            f = double.Parse(values[values.Count - 1].Value);
            if (f != fLast)
            {
              values.Add(Type, fLast.ToString("F" + Digits));
            }
          }
          break;
      }
    }
  }


  internal class MpeItemManagerConverter : ExpandableObjectConverter
  {
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
    {
      if (destType == typeof(string) && value is MpeItemManager)
      {
        MpeItemManager m = (MpeItemManager) value;
        return "(" + m.Type.ToString() + "Collection)";
      }
      return base.ConvertTo(context, culture, value, destType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == typeof(string))
      {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }
  }
}