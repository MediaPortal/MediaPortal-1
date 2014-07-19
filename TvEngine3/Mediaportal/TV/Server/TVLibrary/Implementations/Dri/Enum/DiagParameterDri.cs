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
  internal sealed class DiagParameterDri
  {
    private readonly string _name;
    private static readonly IDictionary<string, DiagParameterDri> _values = new Dictionary<string, DiagParameterDri>();

    /// <summary>
    /// Name of the DRIT manufacturer.
    /// </summary>
    public static readonly DiagParameterDri HostManufacturer = new DiagParameterDri("Host Manufacturer");
    /// <summary>
    /// Serial Number of the DRIT.
    /// </summary>
    public static readonly DiagParameterDri HostSerialNumber = new DiagParameterDri("Host Serial Number");
    /// <summary>
    /// Unique ID of the DRIT used for Card/Host binding.
    /// </summary>
    public static readonly DiagParameterDri HostId = new DiagParameterDri("Host ID");
    /// <summary>
    /// Explicit description of the current power status.
    /// </summary>
    public static readonly DiagParameterDri HostPowerStatus = new DiagParameterDri("Host Power Status");
    /// <summary>
    /// Explicit description of the current boot status.
    /// </summary>
    public static readonly DiagParameterDri HostBootStatus = new DiagParameterDri("Host Boot Status");
    /// <summary>
    /// Explicit description of the current memory allocation.
    /// </summary>
    public static readonly DiagParameterDri HostMemoryReport = new DiagParameterDri("Host Memory Report");
    /// <summary>
    /// Explicit description of the DRM application supported by the devices, including name, version number and date.
    /// </summary>
    public static readonly DiagParameterDri HostApplication = new DiagParameterDri("Host Application");
    /// <summary>
    /// Explicit description of the DRIT firmware including name, version number and date.
    /// </summary>
    public static readonly DiagParameterDri HostFirmware = new DiagParameterDri("Host Firmware");

    private DiagParameterDri(string name)
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
      DiagParameterDri diagParam = obj as DiagParameterDri;
      if (diagParam != null && this == diagParam)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static ICollection<DiagParameterDri> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator DiagParameterDri(string name)
    {
      DiagParameterDri value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DiagParameterDri diagParam)
    {
      return diagParam._name;
    }
  }
}