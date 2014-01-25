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
  public sealed class TunerModulation
  {
    private readonly string _name;
    private static readonly IDictionary<string, TunerModulation> _values = new Dictionary<string, TunerModulation>();

    public static readonly TunerModulation Qam64 = new TunerModulation("QAM64");
    public static readonly TunerModulation Qam64_2 = new TunerModulation("QAM-64");
    public static readonly TunerModulation Qam256 = new TunerModulation("QAM256");
    public static readonly TunerModulation Qam256_2 = new TunerModulation("QAM-256");
    public static readonly TunerModulation Ntsc = new TunerModulation("NTSC");
    public static readonly TunerModulation NtscM = new TunerModulation("NTSC-M");
    public static readonly TunerModulation Vsb8 = new TunerModulation("8VSB");
    public static readonly TunerModulation Vsb8_2 = new TunerModulation("8-VSB");
    public static readonly TunerModulation All = new TunerModulation("ALL");

    private TunerModulation(string name)
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
      TunerModulation modulation = obj as TunerModulation;
      if (modulation != null && this == modulation)
      {
        return true;
      }
      return false;
    }

    public static ICollection<TunerModulation> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator TunerModulation(string name)
    {
      TunerModulation value = null;
      if (name == null || !_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(TunerModulation modulation)
    {
      return modulation._name;
    }
  }
}