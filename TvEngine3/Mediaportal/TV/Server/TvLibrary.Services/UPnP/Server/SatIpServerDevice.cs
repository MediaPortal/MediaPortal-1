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

using System.Xml;
using System.Globalization;
using System.Collections.Generic;
using UPnP.Infrastructure.Dv;
using UPnP.Infrastructure.Dv.DeviceTree;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.UPnP.Server
{
  public class SatIpServerDevice : DvDevice
  {
    public const string LIGHT_SERVER_DEVICE_TYPE = "ses-com:device:SatIPServer";
    public const int LIGHT_SERVER_DEVICE_TYPE_VERSION = 1;

    public SatIpServerDevice(string serverId) : base(LIGHT_SERVER_DEVICE_TYPE, LIGHT_SERVER_DEVICE_TYPE_VERSION, serverId, new SatIpServerDeviceInformation())
    {
      DescriptionGenerateHook += GenerateDescriptionFunc;
    }

    private static void GenerateDescriptionFunc(XmlWriter writer, DvDevice device, GenerationPosition pos,
                                                EndpointConfiguration config, CultureInfo culture)
    {
      if (pos == GenerationPosition.AfterDeviceList)
      {
        writer.WriteElementString("satip", "X_SATIPCAP", "urn:ses-com:satip", getCapabilities());
      }
    }

    private static string getCapabilities()
    {
      string capabilities = string.Empty;
      IList<Card> cards = new List<Card>();
      int DVBS2 = 0;
      int DVBT = 0;
      int DVBC = 0;

      cards = GlobalServiceProvider.Get<ICardService>().ListAllCards(CardIncludeRelationEnum.None);
      foreach (Card card in cards)
      {
        CardType cardType = GlobalServiceProvider.Get<IControllerService>().Type(card.IdCard);
        switch (cardType)
        {
          case CardType.DvbS:
            ++DVBS2;
            break;
          case CardType.DvbT:
            ++DVBT;
            break;
          case CardType.DvbC:
            // currently the specification doesn't support DVB-C, so also increment DVB-T
            ++DVBT;
            break;
        }
      }

      if (DVBS2 > 0)
      {
        capabilities += "DVBS2-" + DVBS2;
      }
      if (DVBT > 0)
      {
        if (capabilities != string.Empty)
          capabilities += ",";
        capabilities += "DVBT-" + DVBT;
      }
      if (DVBC > 0)
      {
        if (capabilities != string.Empty)
          capabilities += ",";
        capabilities += "DVBC-" + DVBC;
      }

      return capabilities;
    }
  }
}