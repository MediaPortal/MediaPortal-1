/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TvControl;

using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.ChannelLinkage;
using TvEngine.Events;
using System.Threading;

namespace TvService
{
  public class ChannelLinkageGrabber : BaseChannelLinkageScanner
  {
    #region variables
    ITVCard _card;
    List<PortalChannel> _cashedLinkages;
    #endregion

    #region ctor
    public ChannelLinkageGrabber(ITVCard card)
    {
      _card = card;
      _cashedLinkages = new List<PortalChannel>();
      InitCache();
    }
    #endregion

    #region callback
    public override int OnLinkageReceived()
    {
      Log.Info("OnLinkageReceived()");
      List<PortalChannel> linkages=_card.ChannelLinkages;
      Log.Info("ChannelLinkage received. {0} portal channels read", linkages.Count);
      foreach (PortalChannel pChannel in linkages)
      {
        PortalChannel cachedPChannel = GetCachedPortalChannel(pChannel);
        if (cachedPChannel == null)
        {
          _cashedLinkages.Add(pChannel);
          PersistPortalChannel(pChannel);
          continue;
        }
        if (!CompareLinkedChannels(cachedPChannel.LinkedChannels, pChannel.LinkedChannels))
        {
          cachedPChannel.LinkedChannels = pChannel.LinkedChannels;
          PersistPortalChannel(pChannel);
        }
      }
      return 0;
    }
    #endregion

    #region private methods
    private void InitCache()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      IList linkages = ChannelLinkageMap.ListAll();
      PortalChannel pChannel=null;
      int lastPortalChannelId = -1;
      foreach (ChannelLinkageMap map in linkages)
      {
        if (map.IdPortalChannel != lastPortalChannelId)
        {
          if (pChannel != null)
            _cashedLinkages.Add(pChannel);
          Channel chan2 = layer.GetChannel(map.IdPortalChannel);
          IList details2=chan2.ReferringTuningDetail();
          TuningDetail detail2=(TuningDetail)details2[0];
          pChannel = new PortalChannel();
          pChannel.NetworkId = (ushort)detail2.NetworkId;
          pChannel.TransportId = (ushort)detail2.TransportId;
          pChannel.ServiceId = (ushort)detail2.ServiceId;
          lastPortalChannelId=map.IdPortalChannel;
        }
        LinkedChannel lChannel = new LinkedChannel();
        Channel chan = layer.GetChannel(map.IdLinkedChannel);
        IList details=chan.ReferringTuningDetail();
        TuningDetail detail=(TuningDetail)details[0];
        lChannel.Name = chan.Name;
        lChannel.NetworkId = (ushort)detail.NetworkId;
        lChannel.TransportId = (ushort)detail.TransportId;
        lChannel.ServiceId = (ushort)detail.ServiceId;
        pChannel.LinkedChannels.Add(lChannel);
      }
      if (pChannel != null)
        _cashedLinkages.Add(pChannel);
    }
    private void PersistPortalChannel(PortalChannel pChannel)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Channel dbPortalChannel = layer.GetChannelByTuningDetail(pChannel.NetworkId, pChannel.TransportId, pChannel.ServiceId);
      if (dbPortalChannel == null)
      {
        Log.Info("Portal channel with networkId={0}, transportId={1}, serviceId={2} not found", pChannel.NetworkId, pChannel.TransportId, pChannel.ServiceId);
        return;
      }
      layer.DeleteLinkageMapForPortalChannel(dbPortalChannel);
      foreach (LinkedChannel lChannel in pChannel.LinkedChannels)
      {
        Channel dbLinkedChannnel = layer.GetChannelByTuningDetail(lChannel.NetworkId, lChannel.TransportId, lChannel.ServiceId);
        if (dbLinkedChannnel==null)
        {
          Log.Info("Linked channel with name={0}, networkId={1}, transportId={2}, serviceId={3} not found", lChannel.Name, lChannel.NetworkId, lChannel.TransportId, lChannel.ServiceId);
          continue;
        }
        dbLinkedChannnel.Name=lChannel.Name;
        dbLinkedChannnel.Persist();
        ChannelLinkageMap map = new ChannelLinkageMap(dbPortalChannel.IdChannel, dbLinkedChannnel.IdChannel);
        map.Persist();
      }
    }
    private PortalChannel GetCachedPortalChannel(PortalChannel pChannel)
    {
      foreach (PortalChannel pChan in _cashedLinkages)
      {
        if ((pChan.NetworkId == pChannel.NetworkId) && (pChan.ServiceId == pChannel.ServiceId) && (pChan.TransportId == pChannel.TransportId))
          return pChan;
      }
      return null;
    }
    private bool CompareLinkedChannels(List<LinkedChannel> cachedLinks, List<LinkedChannel> links)
    {
      if (links.Count != cachedLinks.Count)
      {
        return false;
      }
      for (int i = 0; i < links.Count; i++)
      {
        if ((cachedLinks[i].Name != links[i].Name) || (cachedLinks[i].NetworkId != links[i].NetworkId) || (cachedLinks[i].ServiceId != links[i].ServiceId) || (cachedLinks[i].TransportId != links[i].TransportId))
          return false;
      }
      return true;
    }
    #endregion
  }
}
