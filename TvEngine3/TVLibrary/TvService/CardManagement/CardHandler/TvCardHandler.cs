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
using System.Net;
using System.Threading;
using TvLibrary;
using TvLibrary.Channels;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;
using TvLibrary.Log;
using TvControl;
using TvDatabase;

namespace TvService
{
  public class TvCardHandler : ITvCardHandler
  {
    #region variables

    private bool _isLocal;
    private ITVCard _card;
    private Card _dbsCard;
    private readonly UserManagement _userManagement;
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
      IsLocal = _card != null;
      _userManagement = new UserManagement(this);
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

    /// <summary>
    /// Gets the users.
    /// </summary>
    /// <value>The users.</value>
    public UserManagement Users
    {
      get { return _userManagement; }
    }

    /// <summary>
    /// Gets the diseqc handler.
    /// </summary>
    /// <value>The dis eq C.</value>
    public DisEqcManagement DisEqC
    {
      get { return _disEqcManagement; }
    }

    public TeletextManagement Teletext
    {
      get { return _teletext; }
    }

    public ChannelScanning Scanner
    {
      get { return _scanner; }
    }

    public EpgGrabbing Epg
    {
      get { return _epgGrabbing; }
    }

    public AudioStreams Audio
    {
      get { return _audioStreams; }
    }

    public Recorder Recorder
    {
      get { return _recorder; }
    }

    public TimeShifter TimeShifter
    {
      get { return _timerShifter; }
    }

    public CardTuner Tuner
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
      get { return _dbsCard; }
      set { _dbsCard = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this card is in the local pc or remote.
    /// </summary>
    /// <value><c>true</c> if this card is local; otherwise, <c>false</c>.</value>
    public bool IsLocal
    {
      get { return _isLocal; }
      set { _isLocal = value; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is idle.
    /// </summary>
    /// <value><c>true</c> if this instance is idle; otherwise, <c>false</c>.</value>
    public bool IsIdle
    {
      get
      {
        IUser[] users = Users.GetUsers();
        if (users == null)
          return true;
        if (users.Length == 0)
          return true;
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
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.HasCA(_dbsCard.IdCard);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return false;
          }
        }
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
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            return RemoteControl.Instance.NumberOfChannelsDecrypting(_dbsCard.IdCard);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return 0;
          }
        }
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
        return _dbsCard.DevicePath;
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
          if (_dbsCard.Enabled == false)
            return false;
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
          if (_dbsCard.Enabled == false)
            return 0;
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
          if (_dbsCard.Enabled == false)
            return 0;
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
        if (_dbsCard.Enabled == false)
          return;
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
          if (_dbsCard.Enabled == false)
            return 0;
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
          if (_dbsCard.Enabled == false)
            return 0;
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
      if (IsLocal)
      {
        _card.Dispose();
      }
    }

    public void SetParameters()
    {
      if (!IsLocal)
        return;
      if (_card == null)
        return;
      ScanParameters settings = new ScanParameters();
      TvBusinessLayer layer = new TvBusinessLayer();
      settings.TimeOutTune = Int32.Parse(layer.GetSetting("timeoutTune", "2").Value);
      settings.TimeOutPAT = Int32.Parse(layer.GetSetting("timeoutPAT", "5").Value);
      settings.TimeOutCAT = Int32.Parse(layer.GetSetting("timeoutCAT", "5").Value);
      settings.TimeOutPMT = Int32.Parse(layer.GetSetting("timeoutPMT", "10").Value);
      settings.TimeOutSDT = Int32.Parse(layer.GetSetting("timeoutSDT", "20").Value);
      settings.TimeOutAnalog = Int32.Parse(layer.GetSetting("timeoutAnalog", "20").Value);
      settings.UseDefaultLnbFrequencies = (layer.GetSetting("lnbDefault", "true").Value == "true");
      settings.LnbLowFrequency = Int32.Parse(layer.GetSetting("LnbLowFrequency", "0").Value);
      settings.LnbHighFrequency = Int32.Parse(layer.GetSetting("LnbHighFrequency", "0").Value);
      settings.LnbSwitchFrequency = Int32.Parse(layer.GetSetting("LnbSwitchFrequency", "0").Value);
      settings.MinimumFiles = Int32.Parse(layer.GetSetting("timeshiftMinFiles", "6").Value);
      settings.MaximumFiles = Int32.Parse(layer.GetSetting("timeshiftMaxFiles", "20").Value);
      settings.MaximumFileSize = UInt32.Parse(layer.GetSetting("timeshiftMaxFileSize", "256").Value);
      settings.MaximumFileSize *= 1000;
      settings.MaximumFileSize *= 1000;
      _card.Parameters = settings;
    }

    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(ref IUser user)
    {
      try
      {
        if (_dbsCard.Enabled == false)
          return null;
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
        var context = _card.Context as ITvCardContext;
        if (context == null)
          return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return null;
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
        if (_dbsCard.Enabled == false)
          return -1;
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
        ITvCardContext context = _card.Context as ITvCardContext;
        context.GetUser(ref user);
        return user.IdChannel;
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

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>channel</returns>
    public string CurrentChannelName(ref IUser user)
    {
      try
      {
        if (_dbsCard.Enabled == false)
          return "";
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

        ITvCardContext context = _card.Context as ITvCardContext;
        if (context == null)
          return "";
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
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
        if (_dbsCard.Enabled == false)
          return true;
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
        ITvCardContext context = _card.Context as ITvCardContext;
        if (context == null)
          return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
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
    public void PauseCard(IUser user)
    {
      try
      {
        if (_dbsCard.Enabled == false)
          return;
        if (IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
            RemoteControl.Instance.PauseCard(user);
            return;
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
            return;
          }
        }
        Log.Info("Pausecard");

        //remove all subchannels, except for this user...
        ITvSubChannel[] channels = _card.SubChannels;
        for (int i = 0; i < channels.Length; ++i)
        {
          _card.FreeSubChannel(channels[i].SubChannelId);
        }

        ITvCardContext context = _card.Context as ITvCardContext;
        if (context != null)
        {
          context.Clear();
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
    public void StopCard(IUser user)
    {
      try
      {
        if (_dbsCard.Enabled == false)
          return;
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

        ITvCardContext context = _card.Context as ITvCardContext;
        if (context != null)
        {
          context.Clear();
        }
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

    /// <summary>
    /// Gets the current video stream.
    /// </summary>
    /// <returns></returns>
    public IVideoStream GetCurrentVideoStream(IUser user)
    {
      if (_dbsCard.Enabled == false)
        return null;
      if (IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _dbsCard.ReferencedServer().HostName;
          return RemoteControl.Instance.GetCurrentVideoStream(user);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _dbsCard.ReferencedServer().HostName);
          return null;
        }
      }
      ITvCardContext context = _card.Context as ITvCardContext;
      if (context == null)
        return null;
      context.GetUser(ref user);
      ITvSubChannel subchannel = _card.GetSubChannel(user.SubChannel);
      if (subchannel == null)
        return null;
      return subchannel.GetCurrentVideoStream;
    }


    /// <summary>
    /// returns a virtual card for this tvcard
    /// </summary>
    /// <returns></returns>
    public VirtualCard GetVirtualCard(IUser user)
    {
      VirtualCard card = new VirtualCard(user);
      card.RecordingFormat = _dbsCard.RecordingFormat;
      card.RecordingFolder = _dbsCard.RecordingFolder;
      card.TimeshiftFolder = _dbsCard.TimeShiftFolder;
      card.RemoteServer = Dns.GetHostName();
      return card;
    }
  }
}