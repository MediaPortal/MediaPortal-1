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
using System.ServiceModel;
using Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Service
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  [ServiceKnownType(typeof(SetTopBoxProfile))]
  [ServiceKnownType(typeof(TunerSetTopBoxConfig))]
  [ServiceKnownType(typeof(UsbUirtDetail))]
  internal interface IUsbUirtConfigService
  {
    [OperationContract]
    TunerSetTopBoxConfig GetSetTopBoxConfigurationForTuner(string tunerExternalId);

    [OperationContract]
    void SaveSetTopBoxConfiguration(ICollection<TunerSetTopBoxConfig> config);

    [OperationContract]
    ICollection<UsbUirtDetail> GetAllUsbUirtDetails();

    [OperationContract]
    IList<string> GetAllSetTopBoxProfileNames();

    [OperationContract]
    bool SaveSetTopBoxProfile(SetTopBoxProfile profile);

    [OperationContract]
    LearnResult Learn(uint usbUirtIndex, TimeSpan timeLimit, out string command);

    [OperationContract]
    TransmitResult Transmit(uint usbUirtIndex, TransmitZone zone, string setTopBoxProfileName, string channelNumber);
  }
}