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
  public sealed class CasDescramblingStatus
  {
    private readonly string _name;
    private static readonly IDictionary<string, CasDescramblingStatus> _values = new Dictionary<string, CasDescramblingStatus>();

    /// <summary>
    /// NoCard response or ca_pmt_reply() with ca_enable = 0x74 to 0xFF.
    /// </summary>
    public static readonly CasDescramblingStatus Unknown = new CasDescramblingStatus("Unknown");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x01
    /// </summary>
    public static readonly CasDescramblingStatus Possible = new CasDescramblingStatus("Possible");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x02
    /// </summary>
    public static readonly CasDescramblingStatus PossiblePurchaseDialog = new CasDescramblingStatus("Possible (purchase dialogue)");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x03
    /// </summary>
    public static readonly CasDescramblingStatus PossibleTechnicalDialog = new CasDescramblingStatus("Possible (technical dialogue)");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x71
    /// </summary>
    public static readonly CasDescramblingStatus NotPossibleNoEntitlement = new CasDescramblingStatus("Not possible (no entitlement)");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x73
    /// </summary>
    public static readonly CasDescramblingStatus NotPossibleTechnicalReason = new CasDescramblingStatus("Not possible (technical reason)");

    private CasDescramblingStatus(string name)
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
      CasDescramblingStatus descramblingStatus = obj as CasDescramblingStatus;
      if (descramblingStatus != null && this == descramblingStatus)
      {
        return true;
      }
      return false;
    }

    public static ICollection<CasDescramblingStatus> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator CasDescramblingStatus(string name)
    {
      CasDescramblingStatus value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(CasDescramblingStatus descramblingStatus)
    {
      return descramblingStatus._name;
    }
  }
}