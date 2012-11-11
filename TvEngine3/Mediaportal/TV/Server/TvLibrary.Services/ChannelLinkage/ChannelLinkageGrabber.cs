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
using System.Threading;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.ChannelLinkage
{
  public class ChannelLinkageGrabber : BaseChannelLinkageScanner
  {
  

    #region variables

    private readonly ITVCard _card;

    #endregion

    #region ctor

    public ChannelLinkageGrabber(ITVCard card)
    {
      _card = card;
    }

    #endregion

    #region callback

    public override int OnLinkageReceived()
    {
      this.LogInfo("OnLinkageReceived()");
      Thread workerThread = new Thread(UpdateDatabaseThread);
      workerThread.IsBackground = true;
      workerThread.Name = "Channel linkage update thread";
      workerThread.Start();
      return 0;
    }

    #endregion

    #region private methods

    private static void PersistPortalChannel(PortalChannel pChannel)
    {      
      Channel dbPortalChannel = ChannelManagement.GetChannelByTuningDetail(pChannel.NetworkId, pChannel.TransportId,
                                                               pChannel.ServiceId);
      if (dbPortalChannel == null)
      {
        Log.Info("Portal channel with networkId={0}, transportId={1}, serviceId={2} not found", pChannel.NetworkId,
                 pChannel.TransportId, pChannel.ServiceId);
        return;
      }      
      ChannelManagement.DeleteAllChannelLinkageMaps(dbPortalChannel.IdChannel);
      foreach (LinkedChannel lChannel in pChannel.LinkedChannels)
      {
        Channel dbLinkedChannnel = ChannelManagement.GetChannelByTuningDetail(lChannel.NetworkId, lChannel.TransportId,
                                                                  lChannel.ServiceId);        
        if (dbLinkedChannnel == null)
        {
          Log.Info("Linked channel with name={0}, networkId={1}, transportId={2}, serviceId={3} not found",
                   lChannel.Name, lChannel.NetworkId, lChannel.TransportId, lChannel.ServiceId);
          continue;
        }
        dbLinkedChannnel.DisplayName = lChannel.Name;        
        ChannelManagement.SaveChannel(dbLinkedChannnel);

        var map = new ChannelLinkageMap
                                  {
                                    IdLinkedChannel = dbLinkedChannnel.IdChannel,
                                    IdPortalChannel = dbPortalChannel.IdChannel,
                                    DisplayName = lChannel.Name
                                  };

        ChannelManagement.SaveChannelLinkageMap(map);
      }
    }

    private void UpdateDatabaseThread()
    {
      Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      List<PortalChannel> linkages = _card.ChannelLinkages;
      this.LogInfo("ChannelLinkage received. {0} portal channels read", linkages.Count);
      foreach (PortalChannel pChannel in linkages)
      {
        this.LogInfo("[Linkage Scanner] New portal channel {0} {1} {2}", pChannel.NetworkId, pChannel.ServiceId,
                 pChannel.TransportId);
        foreach (LinkedChannel lchan in pChannel.LinkedChannels)
        {
          this.LogInfo("[Linkage Scanner] - {0} nid={1},tid={2} sid={3}", lchan.Name, lchan.NetworkId, lchan.TransportId,
                   lchan.ServiceId);
        }
        PersistPortalChannel(pChannel);
      }
    }

    #endregion
  }
}