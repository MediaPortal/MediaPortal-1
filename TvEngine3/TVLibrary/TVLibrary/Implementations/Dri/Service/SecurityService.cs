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
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri.Service
{
  public sealed class DriSecurityPairingStatus
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriSecurityPairingStatus> _values = new Dictionary<string, DriSecurityPairingStatus>();

    /// <summary>
    /// The DRIT tuner is registered with a DRIR, controlled content
    /// can be released under DRM protection.
    /// </summary>
    public static readonly DriSecurityPairingStatus Green = new DriSecurityPairingStatus("Green");
    /// <summary>
    /// The DRIT tuner is registered with a DRIR, but this pairing is
    /// going to time out. The DRIR is expected to refresh its pairing in
    /// background. Controlled content can be released under DRM
    /// protection.
    /// </summary>
    public static readonly DriSecurityPairingStatus Orange = new DriSecurityPairingStatus("Orange");
    /// <summary>
    /// The DRIT tuner is not registered with a DRIR. No controlled
    /// content can be released.
    /// </summary>
    public static readonly DriSecurityPairingStatus Red = new DriSecurityPairingStatus("Red");

    private DriSecurityPairingStatus(string name)
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
      DriSecurityPairingStatus pairingStatus = obj as DriSecurityPairingStatus;
      if (pairingStatus != null && this == pairingStatus)
      {
        return true;
      }
      return false;
    }

    public static ICollection<DriSecurityPairingStatus> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator DriSecurityPairingStatus(string name)
    {
      DriSecurityPairingStatus value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriSecurityPairingStatus pairingStatus)
    {
      return pairingStatus._name;
    }
  }

  public class SecurityService : BaseService
  {
    private CpAction _setDrmAction = null;

    public SecurityService(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Security")
    {
      _service.Actions.TryGetValue("SetDRM", out _setDrmAction);
    }

    /// <summary>
    /// Upon receipt of the SetDRM action, the DRIT SHALL set the DrmPairingStatus state variable to “Red” and switch
    /// to the designated DRM systems in less than 5s.
    /// </summary>
    /// <param name="newDrm">This argument sets the DrmUUID state variable.</param>
    public void SetDrm(string newDrm)
    {
      _setDrmAction.InvokeAction(new List<object> { newDrm });
    }
  }
}
