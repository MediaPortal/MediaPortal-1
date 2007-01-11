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
    public TvCard()
    {
      _card = null;
      _dbsCard = null;
    }
    #endregion

    #region properties
    public void Lock(User user)
    {
      if (Card != null)
      {
        TvCardContext context = (TvCardContext)Card.Context;
        context.Lock(user);
      }
    }

    public void Unlock()
    {
      if (Card != null)
      {
        TvCardContext context = (TvCardContext)Card.Context;
        context.Unlock();
      }
    }

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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
    /// Gets the current channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return null;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.CurrentChannel(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return null;
            }
          }
          return _card.Channel;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return null;
        }
      }
    }
    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>id of database channel</returns>
    public int CurrentDbChannel
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return -1;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.CurrentDbChannel(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return -1;
            }
          }
          TvCardContext context = (TvCardContext)_card.Context;
          return context.IdChannel;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return -1;
        }
      }
    }

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>channel</returns>
    public string CurrentChannelName
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return "";
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.CurrentChannelName(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return "";
            }
          }
          if (_card.Channel == null) return "";
          return _card.Channel.Name;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return "";
        }
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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>filename or null when not recording</returns>
    public string FileName
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return "";
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.FileName(_dbsCard.IdCard);

            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return "";
            }
          }
          return _card.FileName;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return "";
        }
      }
    }

    public string TimeShiftFileName
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return "";
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.TimeShiftFileName(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return "";
            }
          }
          return _card.TimeShiftFileName + ".tsbuffer";
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return "";
        }
      }
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting
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
              return RemoteControl.Instance.IsTimeShifting(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          return _card.IsTimeShifting || _card.IsRecording;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
      }
    }
    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording
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
              return RemoteControl.Instance.IsRecording(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          return _card.IsRecording;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
    public bool IsGrabbingTeletext
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
              return RemoteControl.Instance.IsGrabbingTeletext(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          return _card.GrabTeletext;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
      }
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    public bool HasTeletext
    {
      get
      {
        if (_dbsCard.Enabled == false) return false;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.HasTeletext(_dbsCard.IdCard);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return false;
          }
        }
        return _card.HasTeletext;
      }
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(int pageNumber)
    {
      try
      {
        if (_dbsCard.Enabled == false) return new TimeSpan(0, 0, 0, 15);
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.TeletextRotation(_dbsCard.IdCard, pageNumber);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return new TimeSpan(0, 0, 0, 15);
          }
        }
        return _card.TeletextDecoder.RotationTime(pageNumber);
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
    public DateTime TimeShiftStarted
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return DateTime.MinValue;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.TimeShiftStarted(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return DateTime.MinValue;
            }
          }
          return _card.StartOfTimeShift;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return DateTime.MinValue;
        }
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return DateTime.MinValue;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.RecordingStarted(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return DateTime.MinValue;
            }
          }
          return _card.RecordingStarted;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return DateTime.MinValue;
        }
      }
    }


    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    public bool IsScrambled
    {
      get
      {
        try
        {
          if (_dbsCard.Enabled == false) return true;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.IsScrambled(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          return (false == _card.IsReceivingAudioVideo);
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
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
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public bool Tune(IChannel channel, int idChannel)
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        Log.Write("Controller:Tune {0} to {1}", _dbsCard.IdCard, channel.Name);
        lock (this)
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.Tune(_dbsCard.IdCard, channel, idChannel);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          if (CurrentDbChannel == idChannel)
          {
            return true;
          }
          _card.CamType = (CamType)_dbsCard.CamType;
          bool result = _card.Tune(channel);
          TvCardContext context = (TvCardContext)_card.Context;
          context.IdChannel= idChannel;
          Log.Write("Controller: Tuner locked:{0} signal strength:{1} signal quality:{2}",
             _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);

          return result;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    public bool TuneScan(IChannel channel, int idChannel)
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        Log.Write("Controller:TuneScan {0} to {1}", _dbsCard.IdCard, channel.Name);
        lock (this)
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.TuneScan(_dbsCard.IdCard, channel, idChannel);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          _card.CamType = (CamType)_dbsCard.CamType;
          bool result = _card.TuneScan(channel);
          TvCardContext context = (TvCardContext)_card.Context;
          context.IdChannel = idChannel;
          Log.Write("Controller: Tuner locked:{0} signal strength:{1} signal quality:{2}",
             _card.IsTunerLocked, _card.SignalLevel, _card.SignalQuality);
          return result;
        }
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
    public void GrabTeletext(bool onOff)
    {
      try
      {
        if (_dbsCard.Enabled == false) return;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            RemoteControl.Instance.GrabTeletext(_dbsCard.IdCard, onOff);
            return;
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return;
          }
        }
        _card.GrabTeletext = onOff;
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
    public byte[] GetTeletextPage(int pageNumber, int subPageNumber)
    {
      try
      {
        if (_dbsCard.Enabled == false) return new byte[] { 1 };
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetTeletextPage(_dbsCard.IdCard, pageNumber, subPageNumber);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return new byte[] { 1 };
          }
        }
        if (_card.TeletextDecoder == null) return new byte[1] { 1 };
        return _card.TeletextDecoder.GetRawPage(pageNumber, subPageNumber);
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
    public int SubPageCount(int pageNumber)
    {
      try
      {
        if (_dbsCard.Enabled == false) return -1;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.SubPageCount(_dbsCard.IdCard, pageNumber);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return -1;
          }
        }
        if (_card.TeletextDecoder == null) return 0;
        return _card.TeletextDecoder.NumberOfSubpages(pageNumber) + 1;
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
    public TvResult StartTimeShifting(string fileName)
    {
      try
      {
        if (_dbsCard.Enabled == false) return TvResult.CardIsDisabled;
        Log.Write("Controller: StartTimeShifting {0} {1} ", _dbsCard.IdCard, fileName);
        lock (this)
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.StartTimeShifting(_dbsCard.IdCard, fileName);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return TvResult.UnknownError;
            }
          }

          if (_card.IsTimeShifting)
          {
            return TvResult.Succeeded;
          }

          if (WaitForUnScrambledSignal() == false)
          {
            Log.Write("Controller: channel is scrambled");
            _card.StopGraph();

            return TvResult.ChannelIsScrambled;
          }

          bool result = _card.StartTimeShifting(fileName);
          if (result == false)
          {
            _card.StopTimeShifting();
            _card.StopGraph();
            return TvResult.UnableToStartGraph;
          }
          fileName += ".tsbuffer";
          if (!WaitForTimeShiftFile(fileName))
          {
            if (IsScrambled)
            {
              _card.StopTimeShifting();
              _card.StopGraph();
              return TvResult.ChannelIsScrambled;
            }
            _card.StopTimeShifting();
            _card.StopGraph();
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

    public void StopCard()
    {
      try
      {
        if (_dbsCard.Enabled == false) return;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            RemoteControl.Instance.StopCard(_dbsCard.IdCard);
            return;
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return;
          }
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
    public bool StopTimeShifting(User user)
    {
      try
      {
        if (_dbsCard.Enabled == false) return true;
        if (false == IsTimeShifting) return true;
        if (IsRecording) return true;

        Log.Write("Controller: StopTimeShifting {0}", _dbsCard.IdCard);
        lock (this)
        {
          bool result;
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              result = RemoteControl.Instance.StopTimeShifting(_dbsCard.IdCard, user);
              return result;
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          result = _card.StopTimeShifting();
          if (result == true)
          {
            Log.Write("Controller:Timeshifting stopped on card:{0}", _dbsCard.IdCard);
          }
          Unlock();
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
    public bool StartRecording(ref string fileName, bool contentRecording, long startTime)
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
              return RemoteControl.Instance.StartRecording(_dbsCard.IdCard, ref fileName, contentRecording, startTime);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }

          if (_card.IsRecordingTransportStream || (_dbsCard.RecordingFormat == 1))
          {
            fileName = System.IO.Path.ChangeExtension(fileName, ".ts");
          }
          else
          {
            fileName = System.IO.Path.ChangeExtension(fileName, ".mpg");
          }
          Log.Write("Controller: StartRecording {0} {1}", _dbsCard.IdCard, fileName);
          return _card.StartRecording((_dbsCard.RecordingFormat == 1), fileName);
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
    public bool StopRecording()
    {
      try
      {
        if (_dbsCard.Enabled == false) return false;
        Log.Write("Controller: StopRecording {0}", _dbsCard.IdCard);
        lock (this)
        {
          if (IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
              return RemoteControl.Instance.StopRecording(_dbsCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
              return false;
            }
          }
          Log.Write("Controller: StopRecording for card:{0}", _dbsCard.IdCard);
          if (IsRecording)
          {
            _card.StopRecording();
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

    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="cardId">IChannel containing the transponder tuning details.</param>
    /// <returns>list of channels found</returns>
    public IChannel[] Scan(IChannel channel)
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
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return null;
          }
        }
        ITVScanning scanner = _card.ScanningInterface;
        if (scanner == null) return null;
        scanner.Reset();
        List<IChannel> channelsFound = scanner.Scan(channel);
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


    #region audio streams
    public IAudioStream[] AvailableAudioStreams
    {
      get
      {
        if (_dbsCard.Enabled == false) return new List<IAudioStream>().ToArray();
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.AvailableAudioStreams(_dbsCard.IdCard);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return null;
          }
        }
        List<IAudioStream> streams = _card.AvailableAudioStreams;
        return streams.ToArray();
      }
    }

    public IAudioStream GetCurrentAudioStream()
    {
      if (_dbsCard.Enabled == false) return null;
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          return RemoteControl.Instance.GetCurrentAudioStream(_dbsCard.IdCard);
        }
        catch (Exception)
        {
          Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return null;
        }
      }
      return _card.CurrentAudioStream;
    }

    public void SetCurrentAudioStream(IAudioStream stream)
    {
      if (_dbsCard.Enabled == false) return;
      Log.WriteFile("Controller: setaudiostream:{0} {1}", _dbsCard.IdCard, stream);
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          RemoteControl.Instance.SetCurrentAudioStream(_dbsCard.IdCard, stream);
          return;
        }
        catch (Exception)
        {
          Log.Error("Controller: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return;
        }
      }
      _card.CurrentAudioStream = stream;
    }


    #region DiSEqC

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
    public void StopGrabbingEpg()
    {
      if (IsLocal == false)
      {
        // RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
        // RemoteControl.Instance.StopGrabbingEpg();
        return;
      }

      _card.IsEpgGrabbing = false;
    }

    #endregion

    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTune(IChannel channel, Channel dbChannel)
    {
      try
      {
        if (_dbsCard.Enabled == false) return TvResult.CardIsDisabled;
        bool result;
        Log.WriteFile("Controller: CardTune {0} {1}", _dbsCard.IdCard, channel.Name);
        if (IsScrambled)
        {
          result = TuneScan(channel, dbChannel.IdChannel);
          if (result == false) return TvResult.UnableToStartGraph;
          return TvResult.Succeeded;
        }
        if (CurrentDbChannel == dbChannel.IdChannel)
        {
          return TvResult.Succeeded;
        }
        result = TuneScan(channel, dbChannel.IdChannel);
        if (result == false) return TvResult.UnableToStartGraph;
        return TvResult.Succeeded;
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
    public TvResult CardTimeShift(string fileName)
    {
      try
      {
        if (_dbsCard.Enabled == false) return TvResult.CardIsDisabled;
        Log.WriteFile("Controller: CardTimeShift {0} {1}", _dbsCard.IdCard, fileName);
        if (IsTimeShifting) return TvResult.Succeeded;
        return StartTimeShifting(fileName);
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
    public bool WaitForUnScrambledSignal()
    {
      if (_dbsCard.Enabled == false) return false;
      Log.Write("Controller: WaitForUnScrambledSignal");
      DateTime timeStart = DateTime.Now;
      while (true)
      {
        if (IsScrambled)
        {
          Log.Write("Controller:   scrambled, sleep 100");
          System.Threading.Thread.Sleep(100);
          TimeSpan timeOut = DateTime.Now - timeStart;
          if (timeOut.TotalMilliseconds >= 5000)
          {
            Log.Write("Controller:   return scrambled");
            return false;
          }
        }
        else
        {
          Log.Write("Controller:   return not scrambled");
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
    public bool WaitForTimeShiftFile(string fileName)
    {
      if (_dbsCard.Enabled == false) return false;
      Log.Write("Controller: WaitForTimeShiftFile");
      if (!WaitForUnScrambledSignal()) return false;
      DateTime timeStart = DateTime.Now;
      ulong fileSize = 0;

      IChannel channel = _card.Channel;
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
                    Log.Write("Controller: timeshifting fileSize:{0}", fileSize);
                  }
                  fileSize = newfileSize;
                  if (fileSize >= minTimeShiftFile)
                  {
                    TimeSpan ts = DateTime.Now - timeStart;
                    Log.Write("Controller: timeshifting fileSize:{0} {1}", fileSize, ts.TotalMilliseconds);
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
            Log.Write("Controller: timeshifting fileSize:{0} TIMEOUT", fileSize);
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

    public VirtualCard GetVirtualCard()
    {
      VirtualCard card = new VirtualCard(_dbsCard.IdCard);
      card.RecordingFormat = _dbsCard.RecordingFormat;
      card.RecordingFolder = _dbsCard.RecordingFolder;
      card.TimeshiftFolder = _dbsCard.TimeShiftFolder;
      card.RemoteServer = Dns.GetHostName();
      return card;
    }

    public void Dispose()
    {
      if (IsLocal)
      {
        _card.Dispose();
      }
    }
    #endregion
  }
}
