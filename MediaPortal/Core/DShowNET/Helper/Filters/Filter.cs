#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;

#pragma warning disable 618

namespace DShowNET.Helper
{
  /// <summary>
  ///  Represents a DirectShow filter (e.g. video capture device, 
  ///  compression codec).
  /// </summary>
  /// <remarks>
  ///  To save a chosen filer for later recall
  ///  save the MonikerString property on the filter: 
  ///  <code><div style="background-color:whitesmoke;">
  ///   string savedMonikerString = myFilter.MonikerString;
  ///  </div></code>
  ///  
  ///  To recall the filter create a new Filter class and pass the 
  ///  string to the constructor: 
  ///  <code><div style="background-color:whitesmoke;">
  ///   Filter mySelectedFilter = new Filter( savedMonikerString );
  ///  </div></code>
  /// </remarks>
  public class Filter : IComparable
  {
    /// <summary> Human-readable name of the filter </summary>
    private string _name = string.Empty;

    private bool _nameResolved = false;

    /// <summary> Unique string referencing this filter. This string can be used to recreate this filter. </summary>
    public string MonikerString;

    /// <summary> getAnyMoniker take very long time, so use a cached value </summary>
    private static IMoniker[] mon = null;

    /// <summary> Create a new filter from its moniker string. </summary>
    public Filter(string monikerString)
    {
      MonikerString = monikerString;
    }

    /// <summary> Create a new filter from its moniker </summary>
    internal Filter(IMoniker moniker)
    {
      MonikerString = getMonikerString(moniker);
    }

    public string Name
    {
      get
      {
        if (_nameResolved)
        {
          return _name;
        }
        _name = getName(MonikerString);
        return _name;
      }
    }

    public void ResolveName()
    {
      if (_nameResolved)
      {
        return;
      }
      _name = getName(MonikerString);
    }

    /// <summary> Retrieve the a moniker's display name (i.e. it's unique string) </summary>
    protected string getMonikerString(IMoniker moniker)
    {
      string s;
      moniker.GetDisplayName(null, null, out s);
      return (s);
    }

    /// <summary> Retrieve the human-readable name of the filter </summary>
    protected string getName(IMoniker moniker)
    {
      object bagObj = null;
      IPropertyBag bag = null;
      try
      {
        Guid bagId = typeof (IPropertyBag).GUID;
        moniker.BindToStorage(null, null, ref bagId, out bagObj);
        bag = (IPropertyBag) bagObj;
        object val = "";
        int hr = bag.Read("FriendlyName", out val, null);
        if (hr != 0)
        {
          Marshal.ThrowExceptionForHR(hr);
        }
        string ret = val as string;
        if ((ret == null) || (ret.Length < 1))
        {
          throw new NotImplementedException("Device FriendlyName");
        }
        return (ret);
      }
      catch (Exception)
      {
        return ("");
      }
      finally
      {
        bag = null;
        if (bagObj != null)
        {
          DirectShowUtil.ReleaseComObject(bagObj);
        }
        bagObj = null;

        _nameResolved = true;
      }
    }

    /// <summary> Get a moniker's human-readable name based on a moniker string. </summary>
    protected string getName(string monikerString)
    {
      IMoniker parser = null;
      IMoniker moniker = null;
      try
      {
        parser = getAnyMoniker();
        int eaten;
        parser.ParseDisplayName(null, null, monikerString, out eaten, out moniker);
        return (getName(parser));
      }
      finally
      {
        if (moniker != null)
        {
          DirectShowUtil.ReleaseComObject(moniker);
        }
        moniker = null;
        _nameResolved = true;
      }
    }

    /// <summary>
    ///  This method gets a System.Runtime.InteropServices.ComTypes.IMoniker object.
    /// 
    ///  HACK: The only way to create a System.Runtime.InteropServices.ComTypes.IMoniker from a moniker 
    ///  string is to use System.Runtime.InteropServices.ComTypes.IMoniker.ParseDisplayName(). So I 
    ///  need ANY System.Runtime.InteropServices.ComTypes.IMoniker object so that I can call 
    ///  ParseDisplayName(). Does anyone have a better solution?
    /// 
    ///  This assumes there is at least one video compressor filter
    ///  installed on the system.
    /// </summary>
    protected IMoniker getAnyMoniker()
    {
      Guid category = FilterCategory.VideoCompressorCategory;
      int hr;
      object comObj = null;
      ICreateDevEnum enumDev = null;
      IEnumMoniker enumMon = null;

      if (mon != null)
      {
        return mon[0];
      }

      mon = new IMoniker[1];

      try
      {
        // Get the system device enumerator
        Type srvType = Type.GetTypeFromCLSID(ClassId.SystemDeviceEnum);
        if (srvType == null)
        {
          throw new NotImplementedException("System Device Enumerator");
        }
        comObj = Activator.CreateInstance(srvType);
        enumDev = (ICreateDevEnum) comObj;

        // Create an enumerator to find filters in category
        hr = enumDev.CreateClassEnumerator(category, out enumMon, 0);
        if (hr != 0)
        {
          throw new NotSupportedException("No devices of the category");
        }

        // Get first filter
        IntPtr f = IntPtr.Zero;
        hr = enumMon.Next(1, mon, f);
        if ((hr != 0))
        {
          mon[0] = null;
        }

        return (mon[0]);
      }
      finally
      {
        enumDev = null;
        if (enumMon != null)
        {
          DirectShowUtil.ReleaseComObject(enumMon);
        }
        enumMon = null;
        if (comObj != null)
        {
          DirectShowUtil.ReleaseComObject(comObj);
        }
        comObj = null;
      }
    }

    /// <summary>
    ///  Compares the current instance with another object of 
    ///  the same type.
    /// </summary>
    public int CompareTo(object obj)
    {
      if (obj == null)
      {
        return (1);
      }
      Filter f = (Filter) obj;
      return (this.Name.CompareTo(f.Name));
    }
  }
}