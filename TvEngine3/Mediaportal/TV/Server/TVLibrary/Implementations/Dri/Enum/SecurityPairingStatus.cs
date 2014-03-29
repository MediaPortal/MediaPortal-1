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
  public sealed class SecurityPairingStatus
  {
    private readonly string _name;
    private static readonly IDictionary<string, SecurityPairingStatus> _values = new Dictionary<string, SecurityPairingStatus>();

    /// <summary>
    /// The DRIT tuner is registered with a DRIR, controlled content
    /// can be released under DRM protection.
    /// </summary>
    public static readonly SecurityPairingStatus Green = new SecurityPairingStatus("Green");
    /// <summary>
    /// The DRIT tuner is registered with a DRIR, but this pairing is
    /// going to time out. The DRIR is expected to refresh its pairing in
    /// background. Controlled content can be released under DRM
    /// protection.
    /// </summary>
    public static readonly SecurityPairingStatus Orange = new SecurityPairingStatus("Orange");
    /// <summary>
    /// The DRIT tuner is not registered with a DRIR. No controlled
    /// content can be released.
    /// </summary>
    public static readonly SecurityPairingStatus Red = new SecurityPairingStatus("Red");

    private SecurityPairingStatus(string name)
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
      SecurityPairingStatus pairingStatus = obj as SecurityPairingStatus;
      if (pairingStatus != null && this == pairingStatus)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static ICollection<SecurityPairingStatus> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator SecurityPairingStatus(string name)
    {
      SecurityPairingStatus value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(SecurityPairingStatus pairingStatus)
    {
      return pairingStatus._name;
    }
  }
}