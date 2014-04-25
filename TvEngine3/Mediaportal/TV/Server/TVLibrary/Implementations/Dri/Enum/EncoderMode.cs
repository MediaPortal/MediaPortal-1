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
  internal sealed class EncoderMode
  {
    private readonly string _name;
    private static readonly IDictionary<string, EncoderMode> _values = new Dictionary<string, EncoderMode>();

    public static readonly EncoderMode ConstantBitRate = new EncoderMode("CBR");
    public static readonly EncoderMode AverageBitRate = new EncoderMode("AVR");
    public static readonly EncoderMode VariableBitRate = new EncoderMode("VBR");

    private EncoderMode(string name)
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
      EncoderMode mode = obj as EncoderMode;
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

    public static ICollection<EncoderMode> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator EncoderMode(string name)
    {
      EncoderMode value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(EncoderMode mode)
    {
      return mode._name;
    }
  }
}