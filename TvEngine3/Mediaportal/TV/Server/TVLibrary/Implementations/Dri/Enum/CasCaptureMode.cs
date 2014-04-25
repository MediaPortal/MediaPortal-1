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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum
{
  internal sealed class CasCaptureMode
  {
    private readonly string _name;
    private static readonly IDictionary<string, CasCaptureMode> _values = new Dictionary<string, CasCaptureMode>();

    public static readonly CasCaptureMode Live = new CasCaptureMode("Live");
    public static readonly CasCaptureMode Buffer = new CasCaptureMode("Buffer");
    public static readonly CasCaptureMode Record = new CasCaptureMode("Record");

    private CasCaptureMode(string name)
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
      CasCaptureMode captureMode = obj as CasCaptureMode;
      if (captureMode != null && this == captureMode)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static ICollection<CasCaptureMode> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator CasCaptureMode(string name)
    {
      CasCaptureMode value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(CasCaptureMode captureMode)
    {
      return captureMode._name;
    }
  }
}