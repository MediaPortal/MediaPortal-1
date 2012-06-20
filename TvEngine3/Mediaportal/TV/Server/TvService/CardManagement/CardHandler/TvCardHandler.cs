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
using System.Linq;
using System.Threading;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.TVService.Services;

namespace Mediaportal.TV.Server.TVService.CardManagement.CardHandler
{
  public class TvCardHandler : ITvCardHandler
  {
    #region variables

    private ITVCard _card;
    private Card _dbsCard;
    private readonly UserManagement _userManagement;
    private readonly IParkedUserManagement _parkedUserManagement;

    private readonly DisEqcManagement _disEqcManagement;
    private readonly TeletextManagement _teletext;
    private readonly ChannelScanning _scanner;
    private readonly EpgGrabbing _epgGrabbing;
    private readonly AudioStreams _audioStreams;
    private readonly Recorder _recorder;
    private readonly TimeShifter _timerShifter;
    private readonly CardTuner _tuner;
    private ICiMenuActions _ciMenu;    

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ITVCard"/> class.
    /// </summary>
    public TvCardHandler(Card dbsCard, ITVCard card)
    {
      _dbsCard = dbsCard;
      Card = card;
      _userManagement = new UserManagement(this);
      _parkedUserManagement = new ParkedUserManagement(this);

      _disEqcManagement = new DisEqcManagement(this);
      _teletext = new TeletextManagement(this);
      _scanner = new ChannelScanning(this);
      _epgGrabbing = new EpgGrabbing(this);
      _audioStreams = new AudioStreams(this);
      _tuner = new CardTuner(this);
      _recorder = new Recorder(this);
      _timerShifter = new TimeShifter(this);      
    }

    #endregion

    #region CI Menu handling

    public bool CiMenuSupported
    {
      get
      {
        // is card a dvb card? then expose it's ConditionalAccess here
        TvCardDvbBase dvbCard = _card as TvCardDvbBase;
        if (dvbCard != null)
        {
          if (dvbCard.HasCA && dvbCard.ConditionalAccess.CiMenu != null && dvbCard.ConditionalAccess.IsCamReady())
            // only if cam is ready
          {
            _ciMenu = dvbCard.ConditionalAccess.CiMenu;
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Gets the ConditionalAccess handler.
    /// </summary>
    /// <value>ConditionalAccess</value>
    public ICiMenuActions CiMenuActions
    {
      get { return _ciMenu; }
    }

    #endregion


    public IUserManagement UserManagement
    {
      get { return _userManagement; }
    }


    public IParkedUserManagement ParkedUserManagement
    {
      get { return _parkedUserManagement; }
    }    

    /// <summary>
    /// Gets the diseqc handler.
    /// </summary>
    /// <value>The dis eq C.</value>
    public IDisEqcManagement DisEqC
    {
      get { return _disEqcManagement; }
    }

    public ITeletextManagement Teletext
    {
      get { return _teletext; }
    }

    public IChannelScanning Scanner
    {
      get { return _scanner; }
    }

    public IEpgGrabbing Epg
    {
      get { return _epgGrabbing; }
    }

    public IAudioStreams Audio
    {
      get { return _audioStreams; }
    }

    public IRecorder Recorder
    {
      get { return _recorder; }
    }

    public ITimeShifter TimeShifter
    {
      get { return _timerShifter; }
    }

    public ICardTuner Tuner
    {
      get { return _tuner; }
    }

    /// <summary>
    /// Gets or sets the reference to the ITVCard interface
    /// </summary>
    /// <value>The card.</value>
    public ITVCard Card
    {
      get { return _card; }
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
        return _card.SupportsSubChannels;
      }
    }

    /// <summary>
    /// Gets or sets the reference the Card database record 
    /// </summary>
    /// <value>The card record from the database.</value>
    public Card DataBaseCard
    {
      get { return _dbsCard; }
      set { _dbsCard = value; }
    }



    /// <summary>
    /// Gets a value indicating whether this instance is idle.
    /// </summary>
    /// <value><c>true</c> if this instance is idle; otherwise, <c>false</c>.</value>
    public bool IsIdle
    {
      get
      {
        IDictionary<string, IUser> users = UserManagement.Users;
        if (users.Count == 0)
        {
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Does the card have a CA module.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public bool HasCA
    {
      get
      {       
        return _card.HasCA;
      }
    }


    /// <summary>
    /// Returns the number of channels the card is currently decrypting
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {       
        return _card.NumberOfChannelsDecrypting;
      }
    }


    /// <summary>
    /// Gets the type of card.
    /// </summary>
    /// <value>cardtype (Analog,DvbS,DvbT,DvbC,Atsc,WebStream)</value>
    public CardType Type
    {
      get
      {
        try
        {         
          return _card.CardType;
        }
        catch (ThreadAbortException)
        {
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
    /// <returns>name of card</returns>
    public string CardName
    {
      get
      {
        try
        {          
          return _card.Name;
        }
        catch (ThreadAbortException)
        {
          return "";
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return "";
        }
      }
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <returns>device of card</returns>
    public string CardDevice()
    {
      try
      {      
        return _dbsCard.devicePath;
      }
      catch (ThreadAbortException)
      {
        return "";
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
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    public bool TunerLocked
    {
      get
      {
        try
        {
          if (_dbsCard.enabled == false)
          {
            return false;
          }
          return _card.IsTunerLocked;
        }
        catch (ThreadAbortException)
        {
          return false;
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
    /// <returns>signal quality (0-100)</returns>
    public int SignalQuality
    {
      get
      {
        try
        {
          if (_dbsCard.enabled == false)
          {
            return 0;
          }
          return _card.SignalQuality;
        }
        catch (ThreadAbortException)
        {
          return 0;
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
    /// <returns>signal level (0-100)</returns>
    public int SignalLevel
    {
      get
      {
        try
        {
          if (_dbsCard.enabled == false)
          {
            return 0;
          }         
          return _card.SignalLevel;
        }
        catch (ThreadAbortException)
        {
          return 0;
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
    public void UpdateSignalSate()
    {
      try
      {
        if (_dbsCard.enabled == false)
        {
          return;
        }       
        _card.ResetSignalUpdate();
      }
      catch (ThreadAbortException)
      {       
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
          if (_dbsCard.enabled == false)
          {
            return 0;
          }
          
          return _card.MinChannel;
        }
        catch (ThreadAbortException)
        {
          return 0;
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
          if (_dbsCard.enabled == false)
          {
            return 0;
          }
          return _card.MaxChannel;
        }
        catch (ThreadAbortException)
        {
          return 0;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return 0;
        }
      }
    }


    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      _card.Dispose();
    }    

    

    public void SetParameters()
    {
      if (_card == null)
      {
        return;
      }
      ScanParameters settings = new ScanParameters();
      
      settings.TimeOutTune = Int32.Parse(SettingsManagement.GetSetting("timeoutTune", "2").value);
      settings.TimeOutPAT = Int32.Parse(SettingsManagement.GetSetting("timeoutPAT", "5").value);
      settings.TimeOutCAT = Int32.Parse(SettingsManagement.GetSetting("timeoutCAT", "5").value);
      settings.TimeOutPMT = Int32.Parse(SettingsManagement.GetSetting("timeoutPMT", "10").value);
      settings.TimeOutSDT = Int32.Parse(SettingsManagement.GetSetting("timeoutSDT", "20").value);
      settings.TimeOutAnalog = Int32.Parse(SettingsManagement.GetSetting("timeoutAnalog", "20").value);
      settings.UseDefaultLnbFrequencies = (SettingsManagement.GetSetting("lnbDefault", "true").value == "true");
      settings.LnbLowFrequency = Int32.Parse(SettingsManagement.GetSetting("LnbLowFrequency", "0").value);
      settings.LnbHighFrequency = Int32.Parse(SettingsManagement.GetSetting("LnbHighFrequency", "0").value);
      settings.LnbSwitchFrequency = Int32.Parse(SettingsManagement.GetSetting("LnbSwitchFrequency", "0").value);
      settings.MinimumFiles = Int32.Parse(SettingsManagement.GetSetting("timeshiftMinFiles", "6").value);
      settings.MaximumFiles = Int32.Parse(SettingsManagement.GetSetting("timeshiftMaxFiles", "20").value);
      settings.MaximumFileSize = UInt32.Parse(SettingsManagement.GetSetting("timeshiftMaxFileSize", "256").value);
      settings.MaximumFileSize *= 1000;
      settings.MaximumFileSize *= 1000;
      _card.Parameters = settings;
    }


    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="idChannel"> </param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(ref IUser user, int idChannel)
    {
      try
      {
        if (_dbsCard.enabled == false)
        {
          return null;
        }
               
        if (Context == null)
        {
          return null;
        }
        _userManagement.RefreshUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(_userManagement.GetSubChannelIdByChannelId(user.Name, idChannel));
        if (subchannel == null)
        {
          return null;
        }
        return subchannel.CurrentChannel;
      }
      catch(ThreadAbortException)
      {
        return null;
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
    public int CurrentDbChannel(ref IUser user)
    {
      try
      {
        if (_dbsCard.enabled == false)
          return -1;

        _userManagement.RefreshUser(ref user);
        return _userManagement.GetTimeshiftingChannelId(user.Name);
      }
      catch (ThreadAbortException)
      {
        return -1;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    private ITvCardContext Context
    {
      get { return _card.Context as ITvCardContext; }
    }

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="idChannel"> </param>
    /// <returns>channel</returns>
    public string CurrentChannelName(ref IUser user, int idChannel)
    {
      try
      {
        if (_dbsCard.enabled == false)
        {
          return "";
        }
        
        if (Context == null)
          return "";
        _userManagement.RefreshUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(_userManagement.GetSubChannelIdByChannelId(user.Name, idChannel));
        if (subchannel == null)
          return "";
        if (subchannel.CurrentChannel == null)
          return "";
        return subchannel.CurrentChannel.Name;
      }
      catch (ThreadAbortException)
      {
        return "";
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    public bool IsScrambled(ref IUser user)
    {
      try
      {
        if (_dbsCard.enabled == false)
        {
          return true;
        }
               
        if (Context == null)
          return false;
        _userManagement.RefreshUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(_userManagement.GetTimeshiftingSubChannel(user.Name));
        if (subchannel == null)
          return false;
        return (false == subchannel.IsReceivingAudioVideo);
      }
      catch (ThreadAbortException)
      {
        return false;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    public bool IsScrambled(int subchannelId)
    {
      try
      {
        if (_dbsCard.enabled == false)
        {
          return true;
        }
        
        if (Context == null)
          return false;

        ITvSubChannel subchannel = _card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return false;
        return (false == subchannel.IsReceivingAudioVideo);
      }
      catch (ThreadAbortException)
      {
        return false;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }


    /// <summary>
    /// Pauses the card.
    /// </summary>
    public void PauseCard()
    {
      try
      {
        if (_dbsCard.enabled == false)
        {
          return;
        }

        if (_parkedUserManagement.HasAnyParkedUsers())
        {
          Log.Info("unable to Pausecard since there are parked channels");
          return;
        }

        Log.Info("Pausecard");
        //remove all subchannels, except for this user...
        FreeAllSubChannels();
        
        if (Context != null)
        {
          _userManagement.Clear();
        }

        if (_card.SupportsPauseGraph)
        {
          _card.PauseGraph();
        }
        else
        {
          _card.StopGraph();
        }
      }
      catch (ThreadAbortException)
      {        
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

   

    /// <summary>
    /// Stops the card.
    /// </summary>
    public void StopCard()
    {
      try
      {
        if (!_dbsCard.enabled)
        {
          return;
        }

        if (_parkedUserManagement.HasAnyParkedUsers())
        {
          Log.Info("unable to Stopcard since there are parked channels");
          return;
        }
      
        Log.Info("Stopcard");

        //remove all subchannels, except for this user...
        FreeAllSubChannels();        

        
        
        _userManagement.Clear();
        
        _card.StopGraph();
      }
      catch (ThreadAbortException)
      {       
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    private void FreeAllSubChannels()
    {
      ITvSubChannel[] channels = _card.SubChannels;
      foreach (ITvSubChannel tvSubChannel in channels)
      {
        _card.FreeSubChannel(tvSubChannel.SubChannelId);
      }

    }

    /// <summary>
    /// Gets the current video stream.
    /// </summary>
    /// <returns></returns>
    public IVideoStream GetCurrentVideoStream(IUser user)
    {
      if (_dbsCard.enabled == false)
      {
        return null;
      }
            
      if (Context == null)
      {
        return null;
      }
      _userManagement.RefreshUser(ref user);
      ITvSubChannel subchannel = _card.GetSubChannel(_userManagement.GetTimeshiftingSubChannel(user.Name));
      if (subchannel == null)
        return null;
      return subchannel.GetCurrentVideoStream;
    }
  }
}