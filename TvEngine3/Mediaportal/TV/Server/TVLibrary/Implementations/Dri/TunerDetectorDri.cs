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
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.SSDP;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorUpnp"/> which detects DRI tuners.
  /// </summary>
  internal class TunerDetectorDri : ITunerDetectorUpnp
  {
    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "DRI";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a UPnP device.
    /// </summary>
    /// <param name="descriptor">The UPnP device's root descriptor.</param>
    /// <param name="controlPoint">The control point that the device is attached to.</param>
    /// <returns>the compatible tuners exposed by the device</returns>
    public ICollection<ITVCard> DetectTuners(DeviceDescriptor descriptor, UPnPControlPoint controlPoint)
    {
      List<ITVCard> tuners = new List<ITVCard>();

      IEnumerator<DeviceEntry> childDeviceEn = descriptor.RootDescriptor.SSDPRootEntry.Devices.Values.GetEnumerator();
      while (childDeviceEn.MoveNext())
      {
        // Have to check for the tuner service to avoid Ceton tuning adaptor devices.
        foreach (string serviceUrn in childDeviceEn.Current.Services)
        {
          if (serviceUrn.Equals("urn:schemas-opencable-com:service:Tuner:1"))
          {
            string uuid = childDeviceEn.Current.UUID;
            tuners.Add(new TunerDri(descriptor.FindDevice(uuid), controlPoint, new TunerStream(string.Format("MediaPortal DRI {0} Stream Source", uuid), 1)));
            break;
          }
        }
      }
      if (tuners.Count > 0)
      {
        this.LogInfo("DRI detector: tuner added");
      }
      return tuners;
    }
  }
}