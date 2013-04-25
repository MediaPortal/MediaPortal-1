#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
  public class VersionInfo : IComparable
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionInfo"/> class.
    /// Version number components are stored in string format
    /// </summary>
    public VersionInfo()
    {
      Major = "0";
      Minor = "0";
      Build = "0";
      Revision = "0";
    }

    public VersionInfo(Version version)
    {
      Major = version.Major.ToString();
      Minor = version.Minor.ToString();
      Build = version.Build.ToString();
      Revision = version.Revision.ToString();
    }

    private string _major;

    public string Major
    {
      get { return string.IsNullOrEmpty(_major) ? "0" : _major; }
      set { _major = value; }
    }

    private string _minor;

    public string Minor
    {
      get { return string.IsNullOrEmpty(_minor) ? "0" : _minor; }
      set { _minor = value; }
    }

    private string _build;

    public string Build
    {
      get { return string.IsNullOrEmpty(_build) ? "0" : _build; }
      set { _build = value; }
    }

    private string _revision;

    public string Revision
    {
      get { return string.IsNullOrEmpty(_revision) ? "0" : _revision; }
      set { _revision = value; }
    }

    public static VersionInfo Parse(string s)
    {
      VersionInfo ver = new VersionInfo();
      string[] vers = s.Split('.');
      if (vers.Length > 3)
      {
        ver.Major = vers[0];
        ver.Minor = vers[1];
        ver.Build = vers[2];
        ver.Revision = vers[3];
      }
      return ver;
    }

    public override string ToString()
    {
      return Major + "." + Minor + "." + Build + "." + Revision;
    }

    /// <summary>
    /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: 
    ///                     Value 
    ///                     Meaning 
    ///                     Less than zero 
    ///                     This instance is less than <paramref name="obj"/>. 
    ///                     Zero 
    ///                     This instance is equal to <paramref name="obj"/>. 
    ///                     Greater than zero 
    ///                     This instance is greater than <paramref name="obj"/>. 
    /// </returns>
    /// <param name="obj">An object to compare with this instance. 
    ///                 </param><exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. 
    ///                 </exception><filterpriority>2</filterpriority>
    public int CompareTo(object obj)
    {
      return this.ToString().CompareTo(obj.ToString());
    }

    public int CompareTo(VersionInfo obj)
    {
      int i1 = CompareNumber(Major, obj.Major);
      int i2 = CompareNumber(Minor, obj.Minor);
      int i3 = CompareNumber(Build, obj.Build);
      int i4 = CompareNumber(Revision, obj.Revision);
      if (i1 != 0)
        return i1;
      if (i2 != 0)
        return i2;
      if (i3 != 0)
        return i3;
      return i4;
    }

    private static int CompareNumber(string s1, string s2)
    {
      if (s1 == "*")
        return 0;
      if (s2 == "*")
        return 0;
      int i = s1.CompareTo(s2);
      int v1 = -1;
      int v2 = -1;
      try
      {
        int.TryParse(s1, out v1);
        int.TryParse(s2, out v2);
      }
      catch (Exception) {}
      if (v1 > -1 && v2 > -1)
        return v1.CompareTo(v2);
      return s1.CompareTo(s2);
    }
  }
}