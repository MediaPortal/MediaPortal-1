using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;


namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities
{
  public class CardBLL
  {
    private readonly Card _entity;

    public CardBLL(Card entity)
    {      
      _entity = entity;      
    }

    public Card Entity
    {
      get { return _entity; }
    }

    /// <summary>
    /// Checks if a card can view a specific channel
    /// </summary>
    /// <param name="channelId">Channel id</param>
    /// <returns>true/false</returns>
    public bool CanViewTvChannel(int channelId)
    {
      IList<ChannelMap> _cardChannels = _entity.ChannelMaps;
      foreach (ChannelMap _cmap in _cardChannels)
      {
        if (channelId == _cmap.idChannel && !_cmap.epgOnly)
        {
          return true;
        }
      }
      return false;
    }

  }
}
