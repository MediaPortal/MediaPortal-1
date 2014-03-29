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

using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Enum
{
  public sealed class AvTransportRecordQualityMode
  {
    private readonly string _name;
    private static readonly IDictionary<string, AvTransportRecordQualityMode> _values = new Dictionary<string, AvTransportRecordQualityMode>();

    public static readonly AvTransportRecordQualityMode Ep0 = new AvTransportRecordQualityMode("0:EP");
    public static readonly AvTransportRecordQualityMode Lp1 = new AvTransportRecordQualityMode("1:LP");
    public static readonly AvTransportRecordQualityMode Sp2 = new AvTransportRecordQualityMode("2:SP");
    public static readonly AvTransportRecordQualityMode Basic0 = new AvTransportRecordQualityMode("0:BASIC");
    public static readonly AvTransportRecordQualityMode Medium1 = new AvTransportRecordQualityMode("1:MEDIUM");
    public static readonly AvTransportRecordQualityMode High2 = new AvTransportRecordQualityMode("2:HIGH");
    public static readonly AvTransportRecordQualityMode NotImplemented = new AvTransportRecordQualityMode("NOT_IMPLEMENTED");

    private AvTransportRecordQualityMode(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      AvTransportRecordQualityMode mode = obj as AvTransportRecordQualityMode;
      if (mode != null && this == mode)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static ICollection<AvTransportRecordQualityMode> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator AvTransportRecordQualityMode(string name)
    {
      AvTransportRecordQualityMode value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(AvTransportRecordQualityMode mode)
    {
      return mode._name;
    }
  }
}