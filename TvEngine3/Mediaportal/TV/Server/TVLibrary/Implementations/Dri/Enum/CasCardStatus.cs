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
  public sealed class CasCardStatus
  {
    private readonly string _name;
    private static readonly IDictionary<string, CasCardStatus> _values = new Dictionary<string, CasCardStatus>();

    /// <summary>
    /// If a Card is inserted with no error and no firmware upgrade condition.
    /// </summary>
    public static readonly CasCardStatus Inserted = new CasCardStatus("Inserted");
    /// <summary>
    /// If there is no Card inserted.
    /// </summary>
    public static readonly CasCardStatus Removed = new CasCardStatus("Removed");
    /// <summary>
    /// If a Card is inserted and there is an error detected as defined in Appendix E of [CCIF].
    /// </summary>
    public static readonly CasCardStatus Error = new CasCardStatus("Error");
    /// <summary>
    /// If a Card is inserted with no error, but there is a pending
    /// firmware_upgrade() APDU. This can also be used to inform the DRIR the
    /// DRIT is upgrading after receiving a CVT message passed from the
    /// CableCARD Device.
    /// </summary>
    public static readonly CasCardStatus FirmwareUpgrade = new CasCardStatus("Firmware Upgrade");

    private CasCardStatus(string name)
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
      CasCardStatus cardStatus = obj as CasCardStatus;
      if (cardStatus != null && this == cardStatus)
      {
        return true;
      }
      return false;
    }

    public static ICollection<CasCardStatus> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator CasCardStatus(string name)
    {
      CasCardStatus value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(CasCardStatus cardStatus)
    {
      return cardStatus._name;
    }
  }
}