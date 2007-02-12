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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;

namespace TvService
{
  public class TvCard
  {
    #region variables
    bool _isLocal;
    ITVCard _card;
    Card _dbsCard;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvCard"/> class.
    /// </summary>
    public TvCard()
    {
      _card = null;
      _dbsCard = null;
    }
    #endregion

    #region properties
    /// <summary>
    /// Locks the card to the user specified
    /// </summary>
    /// <param name="user">The user.</param>
    public void Lock(User user)
    {
      if (Card != null)
      {
        TvCardContext context = (TvCardContext)Card.Context;
        context.Lock(user);
      }
    }

    /// <summary>
    /// Unlocks this card.
    /// </summary>
    /// <param name="user">The user.</param>
    /// 
    public void Unlock(User user)
    {
      if (Card != null)
      {
        TvCardContext context = (TvCardContext)Card.Context;
        context.Remove(user);
      }
    }

    /// <summary>
    /// Determines whether the specified user is owner of this card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is owner; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOwner(User user)
    {
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          return RemoteControl.Instance.IsOwner(_dbsCard.IdCard, user);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return false;
        }
      }
      else
      {
        TvCardContext context = (TvCardContext)Card.Context;
        return context.IsOwner(user);
      }
    }

    /// <summary>
    /// Removes the user from this card
    /// </summary>
    /// <param name="user">The user.</param>
    public void RemoveUser(User user)
    {
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          RemoteControl.Instance.RemoveUserFromOtherCards(_dbsCard.IdCard, user);
          return;
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return;
        }
      }
      TvCardContext context = _card.Context as TvCardContext;
      if (context == null) return;
      if (!context.DoesExists(user)) return;
      Log.Info("card: remove user:{0} sub:{1}", user.Name, user.SubChannel);
      context.GetUser(ref user);
      context.Remove(user);
      if (context.ContainsUsersForSubchannel(user.SubChannel) == false)
      {
        Log.Info("card: free subchannel sub:{0}", user.SubChannel);
        _card.FreeSubChannel(user.SubChannel);
      }

      if (IsIdle)
      {
        _card.StopGraph();
      }
    }

    /// <summary>
    /// Determines whether the card is locked and ifso returns by which user
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified card is locked; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocked(out User user)
    {
      user = null;
      if (Card != null)
      {
        TvCardContext context = (TvCardContext)Card.Context;
        context.IsLocked(out user);
        return (user != null);
      }
      return false;
    }

    /// <summary>
    /// Gets or sets the reference to the ITVCard interface
    /// </summary>
    /// <value>The card.</value>
    public ITVCard Card
    {
      get
      {
        return _card;
      }
      set
      {
        _card = value;
        if (_card.Context == null)
        {
          _card.Context = new TvCardContext();
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether this card supports sub channels.
    /// </summary>
    /// <value><c>true</c> if card supports sub channels; otherwise, <c>false</c>.</value>
    public bool SupportsSubChannels
    {
      get
      {
        if (Type == CardType.Analog) return false;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.SupportsSubChannels(_dbsCard.IdCard);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return false;
          }
        }
        return _card.SupportsSubChannels;
      }
    }

    /// <summary>
    /// Gets or sets the reference the Card database record 
    /// </summary>
    /// <value>The card record from the database.</value>
    public Card DataBaseCard
    {
      get
      {
        return _dbsCard;
      }
      set
      {
        _dbsCard = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this card is in the local pc or remote.
    /// </summary>
    /// <value><c>true</c> if this card is local; otherwise, <c>false</c>.</value>
    public bool IsLocal
    {
      get
      {
        return _isLocal;
      }
      set
      {
        _isLocal = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is idle.
    /// </summary>
    /// <value><c>true</c> if this instance is idle; otherwise, <c>false</c>.</value>
    public bool IsIdle
    {
      get
      {
        User[] users = GetUsers();
        if (users == null) return true;
        if (users.Length == 0) return true;
        return false;
      }
    }
    #endregion

    #region methods

    /// <summary>
    /// Gets the type of card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <value>cardtype (Analog,DvbS,DvbT,DvbC,Atsc)</value>
    public CardType Type
    {
      get
      {
        try
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.Type(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return CardType.Analog;
            }
          }
          if ((_card as HybridCard) != null)
          {
            HybridCard hybrid = (HybridCard)_card;
            ITVCard card = hybrid.GetById(_dbsCard.IdCard);
            if ((card as TvCardAnalog) != null) return CardType.Analog;
            if ((card as TvCardATSC) != null) return CardType.Atsc;
            if ((card as TvCardDVBC) != null) return CardType.DvbC;
            if ((card as TvCardDVBS) != null) return CardType.DvbS;
            if ((card as TvCardDvbSS2) != null) return (CardType)_card.cardType; //CardType.DvbS;
            if ((card as TvCardDVBT) != null) return CardType.DvbT;
            return CardType.Analog;
          }
          if ((_card as TvCardAnalog) != null) return CardType.Analog;
          if ((_card as TvCardATSC) != null) return CardType.Atsc;
          if ((_card as TvCardDVBC) != null) return CardType.DvbC;
          if ((_card as TvCardDVBS) != null) return CardType.DvbS;
          if ((_card as TvCardDvbSS2) != null) return (CardType)_card.cardType; //CardType.DvbS;
          if ((_card as TvCardDVBT) != null) return CardType.DvbT;
          return CardType.Analog;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return CardType.Analog;
        }
      }
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <returns>name of card</returns>
    public string CardName
    {
      get
      {
        try
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.CardName(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return "";
            }
          }
          return _card.Name;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return "";
        }
      }
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {

      try
      {
        if (_dbsCard.Enabled == false) return false;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.CanTune(_dbsCard.IdCard, channel);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return false;
          }
        }
        return _card.CanTune(channel);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>device of card</returns>
    public string CardDevice()
    {
      try
      {
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.CardDevice(_dbsCard.IdCard);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return "";
          }
        }
        return _card.DevicePath;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }


    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    public bool TunerLocked
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return false;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.TunerLocked(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          return _card.IsTunerLocked;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
      }
    }

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    public int SignalQuality
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return 0;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.SignalQuality(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return 0;
            }
          }
          return _card.SignalQuality;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return 0;
        }
      }
    }

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    public int SignalLevel
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return 0;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.SignalLevel(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return 0;
            }
          }
          return _card.SignalLevel;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return 0;
        }
      }
    }

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void UpdateSignalSate()
    {
      try
      {
        if (_dbsCard.Enabled == false) return;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            RemoteControl.Instance.UpdateSignalSate(_dbsCard.IdCard);
            return;
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return;
          }
        }
        _card.ResetSignalUpdate();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }


    /// <summary>
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MinChannel
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return 0;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.MinChannel(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return 0;
            }
          }
          return _card.MinChannel;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return 0;
        }
      }
    }

    /// <summary>
    /// Gets the max channel to which we can tune.
    /// </summary>
    /// <value>The max channel.</value>
    public int MaxChannel
    {

      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return 0;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.MaxChannel(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return 0;
            }
          }
          return _card.MaxChannel;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return 0;
        }
      }
    }


    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="cardId">IChannel containing the transponder tuning details.</param>
    /// <returns>list of channels found</returns>
    public IChannel[] Scan(IChannel channel, ScanParameters settings)
    {
      try
      {
        if (_dbsCard.Enabled == false) return new List<IChannel>().ToArray();
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.Scan(_dbsCard.IdCard, channel);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return null;
          }
        }
        ITVScanning scanner = _card.ScanningInterface;
        if (scanner == null) return null;
        scanner.Reset();
        List<IChannel> channelsFound = scanner.Scan(channel, settings);
        if (channelsFound == null) return null;
        return channelsFound.ToArray();

      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return null;
      }
    }

    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool GrabEpg(BaseEpgGrabber grabber)
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        if (IsLocal == false)
        {
          //RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          //RemoteControl.Instance.GrabEpg();
          return false;
        }

        _card.GrabEpg(grabber);
        return true;

      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Gets the epg.
    /// </summary>
    /// <value>The epg.</value>
    public List<EpgChannel> Epg
    {
      get
      {
        if (_dbsCard.Enabled == false) return new List<EpgChannel>();
        if (IsLocal == false)
        {
          //RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          //RemoteControl.Instance.GrabEpg();
          return new List<EpgChannel>();
        }

        return _card.Epg;
      }
    }


    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      if (IsLocal)
      {
        _card.Dispose();
      }
    }

    public void SetParameters()
    {
      if (_card == null) return;
      ScanParameters settings = new ScanParameters();
      TvBusinessLayer layer = new TvBusinessLayer();
      settings.TimeOutTune = Int32.Parse(layer.GetSetting("timeoutTune", "2").Value);
      settings.TimeOutPAT = Int32.Parse(layer.GetSetting("timeoutPAT", "5").Value);
      settings.TimeOutCAT = Int32.Parse(layer.GetSetting("timeoutCAT", "5").Value);
      settings.TimeOutPMT = Int32.Parse(layer.GetSetting("timeoutPMT", "10").Value);
      settings.TimeOutSDT = Int32.Parse(layer.GetSetting("timeoutSDT", "20").Value);
      _card.Parameters = settings;
    }

    #endregion

    #region private methods
    /// <summary>
    /// Determines whether card is tuned to the transponder specified by transponder
    /// </summary>
    /// <param name="transponder">The transponder.</param>
    /// <returns>
    /// 	<c>true</c> if card is tuned to the transponder; otherwise, <c>false</c>.
    /// </returns>
    public bool IsTunedToTransponder(IChannel transponder)
    {
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          return RemoteControl.Instance.IsTunedToTransponder(_dbsCard.IdCard, transponder);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return false;
        }
      }
      ITvSubChannel[] subchannels = _card.SubChannels;
      if (subchannels == null) return false;
      if (subchannels.Length == 0) return false;
      if (subchannels[0].CurrentChannel == null) return false;
      return (false == IsDifferentTransponder(subchannels[0].CurrentChannel, transponder));
    }

    /// <summary>
    /// Determines whether transponder 1 is the same as transponder2
    /// </summary>
    /// <param name="transponder1">The transponder1.</param>
    /// <param name="transponder2">The transponder2.</param>
    /// <returns>
    /// 	<c>true</c> if transponder 1 is not equal to transponder 2; otherwise, <c>false</c>.
    /// </returns>
    public bool IsDifferentTransponder(IChannel transponder1, IChannel transponder2)
    {
      DVBCChannel dvbcChannelNew = transponder2 as DVBCChannel;
      if (dvbcChannelNew != null)
      {
        DVBCChannel dvbcChannelCurrent = transponder1 as DVBCChannel;
        if (dvbcChannelNew.Frequency != dvbcChannelCurrent.Frequency) return true;
        return false;
      }

      DVBTChannel dvbtChannelNew = transponder2 as DVBTChannel;
      if (dvbtChannelNew != null)
      {
        DVBTChannel dvbtChannelCurrent = transponder1 as DVBTChannel;
        if (dvbtChannelNew.Frequency != dvbtChannelCurrent.Frequency) return true;
        return false;
      }

      DVBSChannel dvbsChannelNew = transponder2 as DVBSChannel;
      if (dvbsChannelNew != null)
      {
        DVBSChannel dvbsChannelCurrent = transponder1 as DVBSChannel;
        if (dvbsChannelNew.Frequency != dvbsChannelCurrent.Frequency) return true;
        if (dvbsChannelNew.Polarisation != dvbsChannelCurrent.Polarisation) return true;
        if (dvbsChannelNew.ModulationType != dvbsChannelCurrent.ModulationType) return true;
        if (dvbsChannelNew.SatelliteIndex != dvbsChannelCurrent.SatelliteIndex) return true;
        if (dvbsChannelNew.DisEqc != dvbsChannelCurrent.DisEqc) return true;
        return false;
      }

      ATSCChannel atscChannelNew = transponder2 as ATSCChannel;
      if (atscChannelNew != null)
      {
        ATSCChannel atscChannelCurrent = transponder1 as ATSCChannel;
        if (atscChannelNew.MajorChannel != atscChannelCurrent.MajorChannel) return true;
        if (atscChannelNew.MinorChannel != atscChannelCurrent.MinorChannel) return true;
        if (atscChannelNew.PhysicalChannel != atscChannelCurrent.PhysicalChannel) return true;
        return false;
      }
      return false;
    }
    #endregion

    #region subchannels

    /// <summary>
    /// Gets the users for this card.
    /// </summary>
    /// <returns></returns>
    public User[] GetUsers()
    {
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          return RemoteControl.Instance.GetUsersForCard(_dbsCard.IdCard);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return null;
        }
      }
      TvCardContext context = _card.Context as TvCardContext;
      if (context == null) return null;
      return context.Users;
    }


    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return null;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.CurrentChannel(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return null;
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return null;
        return subchannel.CurrentChannel;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return null;
      }
    }

    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>id of database channel</returns>
    public int CurrentDbChannel(ref User user)
    {

      try
      {
        if (_dbsCard.Enabled == false) return -1;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.CurrentDbChannel(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return -1;
          }
        }
        TvCardContext context = (TvCardContext)_card.Context;
        context.GetUser(ref user);
        return user.IdChannel;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>channel</returns>
    public string CurrentChannelName(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return "";
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.CurrentChannelName(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return "";
          }
        }

        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return "";
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return "";
        if (subchannel.CurrentChannel == null) return "";
        return subchannel.CurrentChannel.Name;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>filename or null when not recording</returns>
    public string RecordingFileName(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return "";
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.RecordingFileName(ref user);

          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return "";
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return null;
        return subchannel.RecordingFileName;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Gets the name of the time shift file.
    /// </summary>
    /// <value>The name of the time shift file.</value>
    public string TimeShiftFileName(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return "";
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.TimeShiftFileName(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return "";
          }
        }

        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return null;
        return subchannel.TimeShiftFileName + ".tsbuffer";
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.IsTimeShifting(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return false;
          }
        }

        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return false;
        return subchannel.IsTimeShifting || subchannel.IsRecording;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this card is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this card is recording; otherwise, <c>false</c>.
    /// </value>
    public bool IsAnySubChannelRecording
    {
      get
      {
        User[] users = GetUsers();
        if (users == null) return false;
        if (users.Length == 0) return false;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (IsRecording(ref user)) return true;
        }
        return false;
      }
    }
    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.IsRecording(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return false;
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return false;
        return subchannel.IsRecording;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return false;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.IsScanning(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          return _card.IsScanning;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
      }
    }

    /// <summary>
    /// Returns if the card is grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    public bool IsGrabbingEpg
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return false;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.IsGrabbingEpg(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          return _card.IsEpgGrabbing;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
      }
    }

    /// <summary>
    /// Returns if the card is grabbing teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    public bool IsGrabbingTeletext(User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.IsGrabbingTeletext(user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return false;
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return false;
        return subchannel.GrabTeletext;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    public bool HasTeletext(User user)
    {
      if (_dbsCard.Enabled == false) return false;
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          return RemoteControl.Instance.HasTeletext(user);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return false;
        }
      }
      TvCardContext context = _card.Context as TvCardContext;
      if (context == null) return false;
      context.GetUser(ref user);
      ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
      if (subchannel == null) return false;
      return subchannel.HasTeletext;
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(User user, int pageNumber)
    {
      try
      {
        if (_dbsCard.Enabled == false) return new TimeSpan(0, 0, 0, 15);
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.TeletextRotation(user, pageNumber);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return new TimeSpan(0, 0, 0, 15);
          }
        }

        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return new TimeSpan(0, 0, 0, 15);
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return new TimeSpan(0, 0, 0, 15);
        return subchannel.TeletextDecoder.RotationTime(pageNumber);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return new TimeSpan(0, 0, 0, 15);
      }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return DateTime.MinValue;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.TimeShiftStarted(user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return DateTime.MinValue;
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return DateTime.MinValue;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return DateTime.MinValue;
        return subchannel.StartOfTimeShift;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return DateTime.MinValue;
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return DateTime.MinValue;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.RecordingStarted(user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return DateTime.MinValue;
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return DateTime.MinValue;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return DateTime.MinValue;
        return subchannel.RecordingStarted;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return DateTime.MinValue;
      }
    }

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    public bool IsScrambled(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return true;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.IsScrambled(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return false;
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return false;
        return (false == subchannel.IsReceivingAudioVideo);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void GrabTeletext(User user, bool onOff)
    {
      try
      {
        if (_dbsCard.Enabled == false) return;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            RemoteControl.Instance.GrabTeletext(user, onOff);
            return;
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return;
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return;
        subchannel.GrabTeletext = onOff;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    /// <summary>
    /// Gets the teletext page.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    public byte[] GetTeletextPage(User user, int pageNumber, int subPageNumber)
    {
      try
      {
        if (_dbsCard.Enabled == false) return new byte[] { 1 };
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetTeletextPage(user, pageNumber, subPageNumber);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return new byte[] { 1 };
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return new byte[1] { 1 };
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return new byte[1] { 1 };
        if (subchannel.TeletextDecoder == null) return new byte[1] { 1 };
        return subchannel.TeletextDecoder.GetRawPage(pageNumber, subPageNumber);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return new byte[] { 1 };
      }
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public int SubPageCount(User user, int pageNumber)
    {
      try
      {
        if (_dbsCard.Enabled == false) return -1;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.SubPageCount(user, pageNumber);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return -1;
          }
        }
        TvCardContext context = _card.Context as TvCardContext;
        if (context == null) return -1;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return -1;
        if (subchannel.TeletextDecoder == null) return -1;
        return subchannel.TeletextDecoder.NumberOfSubpages(pageNumber) + 1;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult StartTimeShifting(ref User user, ref  string fileName)
    {
      try
      {
        if (_dbsCard.Enabled == false) return TvResult.CardIsDisabled;
        Log.Write("card: StartTimeShifting {0} {1} ", _dbsCard.IdCard, fileName);
        lock (this)
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.StartTimeShifting(ref user, ref fileName);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return TvResult.UnknownError;
            }
          }

          TvCardContext context = _card.Context as TvCardContext;
          if (context == null) return TvResult.UnknownChannel;
          context.GetUser(ref user);
          ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
          if (subchannel == null) return TvResult.UnknownChannel;

          if (subchannel.IsTimeShifting)
          {
            return TvResult.Succeeded;
          }

          if (WaitForUnScrambledSignal(ref user) == false)
          {
            Log.Write("card: channel is scrambled");
            RemoveUser(user);

            return TvResult.ChannelIsScrambled;
          }

          bool result = subchannel.StartTimeShifting(fileName);
          if (result == false)
          {
            RemoveUser(user);
            return TvResult.UnableToStartGraph;
          }
          fileName += ".tsbuffer";
          if (!WaitForTimeShiftFile(ref user, fileName))
          {
            if (IsScrambled(ref user))
            {
              RemoveUser(user);
              return TvResult.ChannelIsScrambled;
            }
            RemoveUser(user);
            return TvResult.NoVideoAudioDetected;
          }
          return TvResult.Succeeded;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Stops the card.
    /// </summary>
    public void StopCard(User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            RemoteControl.Instance.StopCard(user);
            return;
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return;
          }
        }
        Log.Info("Stopcard");

        //remove all subchannels, except for this user...
        ITvSubChannel[] channels = _card.SubChannels;
        for (int i = 0; i < channels.Length; ++i)
        {
          _card.FreeSubChannel(channels[i].SubChannelId);
        }

        TvCardContext context = _card.Context as TvCardContext;
        if (context != null)
        {
          context.Clear();
        }
        _card.StopGraph();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool StopTimeShifting(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return true;
        if (false == IsTimeShifting(ref user)) return true;
        if (IsRecording(ref user)) return true;

        Log.Write("card: StopTimeShifting user:{0} sub:{1}", user.Name, user.SubChannel);
        lock (this)
        {
          bool result = false;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              result = RemoteControl.Instance.StopTimeShifting(ref user);
              return result;
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          TvCardContext context = _card.Context as TvCardContext;
          if (context == null) return true;
          context.Remove(user);
          if (IsIdle)
          {
            StopCard(user);
          }
          return result;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">if true then create a content recording else a reference recording</param>
    /// <param name="startTime">not used</param>
    /// <returns></returns>
    public bool StartRecording(ref User user, ref string fileName, bool contentRecording, long startTime)
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        lock (this)
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.StartRecording(ref user, ref fileName, contentRecording, startTime);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }

          TvCardContext context = _card.Context as TvCardContext;
          if (context == null) return false;

          context.GetUser(ref user);
          ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
          if (subchannel == null) return false;
          if (subchannel.IsRecordingTransportStream || (_dbsCard.RecordingFormat == 1))
          {
            fileName = System.IO.Path.ChangeExtension(fileName, ".ts");
          }
          else
          {
            fileName = System.IO.Path.ChangeExtension(fileName, ".mpg");
          }

          Log.Write("card: StartRecording {0} {1}", _dbsCard.IdCard, fileName);
          bool result = subchannel.StartRecording((_dbsCard.RecordingFormat == 1), fileName);
          if (result)
          {
            fileName = subchannel.RecordingFileName;
            context.Owner = user;
          }
          return result;

        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool StopRecording(ref User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        Log.Write("card: StopRecording {0}", _dbsCard.IdCard);
        lock (this)
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.StopRecording(ref user);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          Log.Write("card: StopRecording for card:{0}", _dbsCard.IdCard);
          TvCardContext context = _card.Context as TvCardContext;
          if (context == null) return false;
          if (IsRecording(ref user))
          {
            context.GetUser(ref user);
            ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
            if (subchannel == null) return false;
            subchannel.StopRecording();
            if (subchannel.IsTimeShifting == false || context.Users.Length <= 1)
            {
              RemoveUser(user);
            }
          }

          User[] users = context.Users;
          for (int i = 0; i < users.Length; ++i)
          {
            ITvSubChannel subchannel = _card.GetSubChannel(users[i].SubChannel);
            if (subchannel != null)
            {
              if (subchannel.IsRecording)
              {
                context.Owner = users[i];
                break;
              }
            }
          }
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    #region audio streams
    /// <summary>
    /// Gets the available audio streams.
    /// </summary>
    /// <value>The available audio streams.</value>
    public IAudioStream[] AvailableAudioStreams(User user)
    {
      if (_dbsCard.Enabled == false) return new List<IAudioStream>().ToArray();
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          return RemoteControl.Instance.AvailableAudioStreams(user);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return null;
        }
      }

      TvCardContext context = _card.Context as TvCardContext;
      if (context == null) return new List<IAudioStream>().ToArray();
      context.GetUser(ref user);
      ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
      if (subchannel == null) return new List<IAudioStream>().ToArray();
      return subchannel.AvailableAudioStreams.ToArray();
    }

    /// <summary>
    /// Gets the current audio stream.
    /// </summary>
    /// <returns></returns>
    public IAudioStream GetCurrentAudioStream(User user)
    {
      if (_dbsCard.Enabled == false) return null;
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          return RemoteControl.Instance.GetCurrentAudioStream(user);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return null;
        }
      }
      TvCardContext context = _card.Context as TvCardContext;
      if (context == null) return null;
      context.GetUser(ref user);
      ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
      if (subchannel == null) return null;
      return subchannel.CurrentAudioStream;
    }

    /// <summary>
    /// Sets the current audio stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void SetCurrentAudioStream(User user, IAudioStream stream)
    {
      if (_dbsCard.Enabled == false) return;
      Log.WriteFile("card: setaudiostream:{0} {1}", _dbsCard.IdCard, stream);
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          RemoteControl.Instance.SetCurrentAudioStream(user, stream);
          return;
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return;
        }
      }
      TvCardContext context = _card.Context as TvCardContext;
      if (context == null) return;
      context.GetUser(ref user);
      ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
      if (subchannel == null) return;
      subchannel.CurrentAudioStream = stream;
    }


    #region DiSEqC

    /// <summary>
    /// returns the current diseqc motor position
    /// </summary>
    /// <param name="satellitePosition">The satellite position.</param>
    /// <param name="stepsAzimuth">The steps azimuth.</param>
    /// <param name="stepsElevation">The steps elevation.</param>
    public void DiSEqCGetPosition(out int satellitePosition, out int stepsAzimuth, out int stepsElevation)
    {
      satellitePosition = -1;
      stepsAzimuth = 0;
      stepsElevation = 0;
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.GetPosition(out  satellitePosition, out  stepsAzimuth, out  stepsElevation);
    }

    /// <summary>
    /// resets the diseqc motor.
    /// </summary>
    public void DiSEqCReset()
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.Reset();
    }
    /// <summary>
    /// stops the diseqc motor
    /// </summary>
    public void DiSEqCStopMotor()
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.StopMotor();
    }
    /// <summary>
    /// sets the east limit of the diseqc motor
    /// </summary>
    public void DiSEqCSetEastLimit()
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.SetEastLimit();
    }
    /// <summary>
    /// sets the west limit of the diseqc motor
    /// </summary>
    public void DiSEqCSetWestLimit()
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.SetWestLimit();
    }
    /// <summary>
    /// Enables or disables the use of the west/east limits
    /// </summary>
    /// <param name="onOff">if set to <c>true</c> [on off].</param>
    public void DiSEqCForceLimit(bool onOff)
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.ForceLimits = onOff;
    }
    /// <summary>
    /// Drives the diseqc motor in the direction specified by the number of steps
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="numberOfSteps">The number of steps.</param>
    public void DiSEqCDriveMotor(DiSEqCDirection direction, byte numberOfSteps)
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.DriveMotor(direction, numberOfSteps);
    }
    /// <summary>
    /// Stores the current diseqc motor position
    /// </summary>
    /// <param name="position">The position.</param>
    public void DiSEqCStorePosition(byte position)
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.StorePosition(position);
    }
    /// <summary>
    /// Drives the diseqc motor to the reference positition
    /// </summary>
    public void DiSEqCGotoReferencePosition()
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.GotoReferencePosition();
    }
    /// <summary>
    /// Drives the diseqc motor to the specified position
    /// </summary>
    /// <param name="position">The position.</param>
    public void DiSEqCGotoPosition(byte position)
    {
      if (IsLocal == false)
      {
        return;
      }

      IDiSEqCMotor motor = _card.DiSEqCMotor;
      if (motor == null) return;
      motor.GotoPosition(position);
    }
    #endregion

    /// <summary>
    /// Stops the grabbing epg.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    public void StopGrabbingEpg(User user)
    {
      if (IsLocal == false)
      {
        // RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
        // RemoteControl.Instance.StopGrabbingEpg();
        return;
      }
      TvCardContext context = _card.Context as TvCardContext;
      if (context != null)
      {
        context.Remove(user);
        if (context.ContainsUsersForSubchannel(user.SubChannel) == false)
        {
          _card.FreeSubChannel(user.SubChannel);
        }
      }
      _card.IsEpgGrabbing = false;
    }


    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public TvResult Tune(ref User user, IChannel channel, int idChannel)
    {
      try
      {
        if (_dbsCard.Enabled == false) return TvResult.CardIsDisabled;
        Log.Write("card:Tune {0} to {1}", _dbsCard.IdCard, channel.Name);
        lock (this)
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.Tune(ref user, channel, idChannel);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return TvResult.ConnectionToSlaveFailed;
            }
          }

          //@FIX this fails for back-2-back recordings
          //if (CurrentDbChannel(ref user) == idChannel && idChannel >= 0)
          //{
          //  return true;
          //}
          Log.Info("card:user:{0}:{1}:{2} tune {3}", user.Name, user.CardId, user.SubChannel, channel.ToString());
          _card.CamType = (CamType)_dbsCard.CamType;
          SetParameters();

          //check if transponder differs
          TvCardContext context = (TvCardContext)_card.Context;
          if (_card.SubChannels.Length > 0)
          {
            if (IsTunedToTransponder(channel) == false)
            {
              if (context.IsOwner(user))
              {
                Log.Info("card: to different transponder");

                //remove all subchannels, except for this user...
                User[] users = context.Users;
                for (int i = 0; i < users.Length; ++i)
                {
                  if (users[i].Name != user.Name)
                  {
                    Log.Info("  stop subchannel:{0} user:{1}", i, users[i].Name);
                    _card.FreeSubChannel(users[i].SubChannel);
                    context.Remove(users[i]);
                  }
                }
              }
              else
              {
                Log.Info("card: user:{0} is not the card owner. Cannot switch transponder", user.Name);
                return TvResult.NotTheOwner;
              }
            }
          }

          ITvSubChannel result = _card.Tune(user.SubChannel, channel);
          if (result != null)
          {
            Log.Info("card: tuned user:{0} subchannel:{1}", user.Name, result.SubChannelId);
            user.SubChannel = result.SubChannelId;
            user.IdChannel = idChannel;
          }
          context.Add(user);
          Log.Write("card: Tuner locked:{0} signal strength:{1} signal quality:{2}", _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
          if (result == null) return TvResult.AllCardsBusy;
          return TvResult.Succeeded;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }


    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTune(ref User user, IChannel channel, Channel dbChannel)
    {
      try
      {
        if (_dbsCard.Enabled == false) return TvResult.CardIsDisabled;
        TvResult result;
        Log.WriteFile("card: CardTune {0} {1} {2}:{3}:{4}", _dbsCard.IdCard, channel.Name, user.Name, user.CardId, user.SubChannel);
        if (IsScrambled(ref user))
        {
          result = Tune(ref user, channel, dbChannel.IdChannel);
          Log.Info("card2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
          return result;
        }
        if (CurrentDbChannel(ref user) == dbChannel.IdChannel && dbChannel.IdChannel >= 0)
        {
          return TvResult.Succeeded;
        }
        result = Tune(ref user, channel, dbChannel.IdChannel);
        Log.Info("card2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
        return result;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }

    /// <summary>
    /// Start timeshifting on the card
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTimeShift(ref User user, ref string fileName)
    {
      try
      {
        if (_dbsCard.Enabled == false) return TvResult.CardIsDisabled;
        Log.WriteFile("card: CardTimeShift {0} {1}", _dbsCard.IdCard, fileName);
        if (IsTimeShifting(ref user)) return TvResult.Succeeded;
        return StartTimeShifting(ref user, ref fileName);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }

    /// <summary>
    /// Waits for un scrambled signal.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns>true if channel is unscrambled else false</returns>
    public bool WaitForUnScrambledSignal(ref User user)
    {
      if (_dbsCard.Enabled == false) return false;
      Log.Write("card: WaitForUnScrambledSignal");
      DateTime timeStart = DateTime.Now;
      while (true)
      {
        if (IsScrambled(ref user))
        {
          Log.Write("card:   scrambled, sleep 100");
          System.Threading.Thread.Sleep(100);
          TimeSpan timeOut = DateTime.Now - timeStart;
          if (timeOut.TotalMilliseconds >= 5000)
          {
            Log.Write("card:   return scrambled");
            return false;
          }
        }
        else
        {
          Log.Write("card:   return not scrambled");
          return true;
        }
      }
    }

    /// <summary>
    /// Waits for time shift file to be at leat 300kb.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>true when timeshift files is at least of 300kb, else timeshift file is less then 300kb</returns>
    public bool WaitForTimeShiftFile(ref User user, string fileName)
    {
      if (_dbsCard.Enabled == false) return false;
      Log.Write("card: WaitForTimeShiftFile");
      if (!WaitForUnScrambledSignal(ref user)) return false;
      DateTime timeStart = DateTime.Now;
      ulong fileSize = 0;
      if (_card.SubChannels.Length <= 0) return false;
      IChannel channel = _card.SubChannels[0].CurrentChannel;
      bool isRadio = channel.IsRadio;
      ulong minTimeShiftFile = 500 * 1024;//300Kb
      if (isRadio)
        minTimeShiftFile = 200 * 1024;//100Kb

      timeStart = DateTime.Now;
      try
      {
        while (true)
        {
          if (System.IO.File.Exists(fileName))
          {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
              if (stream.Length > 0)
              {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                  stream.Seek(0, SeekOrigin.Begin);

                  ulong newfileSize = reader.ReadUInt64();
                  if (newfileSize != fileSize)
                  {
                    Log.Write("card: timeshifting fileSize:{0}", fileSize);
                  }
                  fileSize = newfileSize;
                  if (fileSize >= minTimeShiftFile)
                  {
                    TimeSpan ts = DateTime.Now - timeStart;
                    Log.Write("card: timeshifting fileSize:{0} {1}", fileSize, ts.TotalMilliseconds);
                    return true;
                  }
                }
              }
            }
          }
          System.Threading.Thread.Sleep(100);
          TimeSpan timeOut = DateTime.Now - timeStart;
          if (timeOut.TotalMilliseconds >= 15000)
          {
            Log.Write("card: timeshifting fileSize:{0} TIMEOUT", fileSize);
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// returns a virtual card for this tvcard
    /// </summary>
    /// <returns></returns>
    public VirtualCard GetVirtualCard(User user)
    {
      VirtualCard card = new VirtualCard(user);
      card.RecordingFormat = _dbsCard.RecordingFormat;
      card.RecordingFolder = _dbsCard.RecordingFolder;
      card.TimeshiftFolder = _dbsCard.TimeShiftFolder;
      card.RemoteServer = Dns.GetHostName();
      return card;
    }
    #endregion

    #endregion
  }
}
