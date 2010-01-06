/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System.Collections.Generic;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.ChannelLinkage;
using System.Threading;

namespace TvService
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
      Log.Info("OnLinkageReceived()");
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
      TvBusinessLayer layer = new TvBusinessLayer();
      Channel dbPortalChannel = layer.GetChannelByTuningDetail(pChannel.NetworkId, pChannel.TransportId,
                                                               pChannel.ServiceId);
      if (dbPortalChannel == null)
      {
        Log.Info("Portal channel with networkId={0}, transportId={1}, serviceId={2} not found", pChannel.NetworkId,
                 pChannel.TransportId, pChannel.ServiceId);
        return;
      }
      Gentle.Framework.Broker.Execute("delete from ChannelLinkageMap WHERE idPortalChannel=" + dbPortalChannel.IdChannel);
      foreach (LinkedChannel lChannel in pChannel.LinkedChannels)
      {
        Channel dbLinkedChannnel = layer.GetChannelByTuningDetail(lChannel.NetworkId, lChannel.TransportId,
                                                                  lChannel.ServiceId);
        if (dbLinkedChannnel == null)
        {
          Log.Info("Linked channel with name={0}, networkId={1}, transportId={2}, serviceId={3} not found",
                   lChannel.Name, lChannel.NetworkId, lChannel.TransportId, lChannel.ServiceId);
          continue;
        }
        dbLinkedChannnel.DisplayName = lChannel.Name;
        dbLinkedChannnel.Persist();
        ChannelLinkageMap map = new ChannelLinkageMap(dbPortalChannel.IdChannel, dbLinkedChannnel.IdChannel);
        map.Persist();
      }
    }

    private void UpdateDatabaseThread()
    {
      Thread.CurrentThread.Priority = ThreadPriority.Lowest;

      List<PortalChannel> linkages = _card.ChannelLinkages;
      Log.Info("ChannelLinkage received. {0} portal channels read", linkages.Count);
      foreach (PortalChannel pChannel in linkages)
      {
        Log.Info("[Linkage Scanner] New portal channel {0} {1} {2}", pChannel.NetworkId, pChannel.ServiceId,
                 pChannel.TransportId);
        foreach (LinkedChannel lchan in pChannel.LinkedChannels)
          Log.Info("[Linkage Scanner] - {0}", lchan.Name);
        PersistPortalChannel(pChannel);
      }
    }

    #endregion
  }
}