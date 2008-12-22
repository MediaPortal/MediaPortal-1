/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Threading;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.ChannelLinkage;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;

namespace TvService
{
  public class TimeShifter
  {
    ITvCardHandler _cardHandler;
    bool _linkageScannerEnabled;
    bool _timeshiftingEpgGrabberEnabled;
		int _waitForTimeshifting = 15;			
		int _waitForUnscrambled = 5;

    ManualResetEvent _eventAudio; // gets signaled when audio PID is seen
    ManualResetEvent _eventVideo; // gets signaled when video PID is seen
    bool _eventsReady = false;

    ChannelLinkageGrabber _linkageGrabber = null;
    /// <summary>
    /// Initializes a new instance of the <see cref="TimerShifter"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public TimeShifter(ITvCardHandler cardHandler)
    {
      _eventAudio = new ManualResetEvent(false);
      _eventVideo = new ManualResetEvent(false);
      _eventsReady = true;

      _cardHandler = cardHandler;
      TvBusinessLayer layer = new TvBusinessLayer();
      _linkageScannerEnabled = (layer.GetSetting("linkageScannerEnabled", "no").Value == "yes");

      _linkageGrabber = new ChannelLinkageGrabber(cardHandler.Card);
      _timeshiftingEpgGrabberEnabled = (layer.GetSetting("timeshiftingEpgGrabberEnabled", "no").Value == "yes");

			_waitForTimeshifting = Int32.Parse(layer.GetSetting("timeshiftWaitForTimeshifting", "15").Value);
			_waitForUnscrambled = Int32.Parse(layer.GetSetting("timeshiftWaitForUnscrambled", "5").Value);
    }


    /// <summary>
    /// Gets the name of the time shift file.
    /// </summary>
    /// <value>The name of the time shift file.</value>
    public string FileName(ref User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return "";
  
				try
				{
					RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
					if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return "";
					if (_cardHandler.IsLocal == false)
					{
						return RemoteControl.Instance.TimeShiftFileName(ref user);
					}
				}
				catch (Exception)
				{
					Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
					return "";
				}

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null) return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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
    /// Gets a value indicating whether this card is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this card is recording; otherwise, <c>false</c>.
    /// </value>
    public bool IsAnySubChannelTimeshifting
    {
      get
      {
        User[] users = _cardHandler.Users.GetUsers();
        if (users == null) return false;
        if (users.Length == 0) return false;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (IsTimeShifting(ref user)) return true;
        }
        return false;
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
        if (_cardHandler.DataBaseCard.Enabled == false) return false;

				try
				{
					RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
					if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return false;
					if (_cardHandler.IsLocal == false)
					{
						return RemoteControl.Instance.IsTimeShifting(ref user);
					}
				}
				catch (Exception)
				{
					Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
					return false;
				} 
        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null) return false;
        bool userExists = false;
        context.GetUser(ref user, out userExists);
        if (!userExists) return false;
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return false;
        return subchannel.IsTimeShifting;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
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
        if (_cardHandler.DataBaseCard.Enabled == false) return DateTime.MinValue;

				try
				{
					RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
					if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return DateTime.MinValue;
					if (_cardHandler.IsLocal == false)
					{
						return RemoteControl.Instance.TimeShiftStarted(user);
					}
				}
				catch (Exception)
				{
					Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
					return DateTime.MinValue;
				}

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null) return DateTime.MinValue;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null) return DateTime.MinValue;
        return subchannel.StartOfTimeShift;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return DateTime.MinValue;
      }
    }

    private void AudioVideoEventHandler(PidType pidType)
		{
      Log.Info("audioVideoEventHandler {0}", pidType);
      // we are only interested in video and audio PIDs
      if( pidType == PidType.Audio )
      {
        _eventAudio.Set();
      }

      if (pidType == PidType.Video)
      {
        _eventVideo.Set();
      }
		}    

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult Start(ref User user, ref  string fileName)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return TvResult.CardIsDisabled;
                
        lock (this)
        {          
					try
					{
						RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
						if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return TvResult.CardIsDisabled;

						Log.Write("card: StartTimeShifting {0} {1} ", _cardHandler.DataBaseCard.IdCard, fileName);

						if (_cardHandler.IsLocal == false)
						{
							return RemoteControl.Instance.StartTimeShifting(ref user, ref fileName);
						}
					}
					catch (Exception)
					{
						Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
						return TvResult.UnknownError;
					}

          TvCardContext context = _cardHandler.Card.Context as TvCardContext;
          if (context == null) return TvResult.UnknownChannel;
          context.GetUser(ref user);
          ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);

          Log.Write("card: CAM enabled : {0}", _cardHandler.HasCA);

          if (subchannel is TvDvbChannel)
          {
            if (!((TvDvbChannel)subchannel).PMTreceived)
            {
              Log.Info("start subch:{0} No PMT received. Timeshifting failed", subchannel.SubChannelId);
              this.Stop(ref user);
              _cardHandler.Users.RemoveUser(user);
              return TvResult.UnableToStartGraph;
            }
          }

					if (subchannel is BaseSubChannel)
					{                        
            ((BaseSubChannel)subchannel).AudioVideoEvent += new BaseSubChannel.AudioVideoObserverEvent(this.AudioVideoEventHandler);
					}

          if (!_eventsReady)
          {
            _eventAudio = new ManualResetEvent(false);
            _eventVideo = new ManualResetEvent(false);
            _eventsReady = true;
          }          

          if (subchannel == null) return TvResult.UnknownChannel;
          bool isScrambled = false;
          if (subchannel.IsTimeShifting)
          {                        
            if (!WaitForTimeShiftFile(ref user, out isScrambled))
						{
              this.Stop(ref user);
							_cardHandler.Users.RemoveUser(user);
              if (isScrambled)
							{								
								return TvResult.ChannelIsScrambled;
							}							
							return TvResult.NoVideoAudioDetected;
						}

            context.OnZap(user);
            if (_linkageScannerEnabled)
              _cardHandler.Card.StartLinkageScanner(_linkageGrabber);
            if (_timeshiftingEpgGrabberEnabled)
            {
              Channel channel = Channel.Retrieve(user.IdChannel);
              if (channel.GrabEpg)
                _cardHandler.Card.GrabEpg();
              else
                Log.Info("TimeshiftingEPG: channel {0} is not configured for grabbing epg", channel.DisplayName);
            }
            return TvResult.Succeeded;
          }

          bool result = subchannel.StartTimeShifting(fileName);
          if (result == false)
          {
            this.Stop(ref user);
            _cardHandler.Users.RemoveUser(user);						
            return TvResult.UnableToStartGraph;
          }
					fileName += ".tsbuffer";
          isScrambled = false;
          if (!WaitForTimeShiftFile(ref user, out isScrambled))
          {
            this.Stop(ref user);
						_cardHandler.Users.RemoveUser(user);
            if (isScrambled)
            {              
              return TvResult.ChannelIsScrambled;
            }            
            return TvResult.NoVideoAudioDetected;
          }
          context.OnZap(user);
          if (_linkageScannerEnabled)
            _cardHandler.Card.StartLinkageScanner(_linkageGrabber);
          if (_timeshiftingEpgGrabberEnabled)
          {
            Channel channel = Channel.Retrieve(user.IdChannel);
            if (channel.GrabEpg)
              _cardHandler.Card.GrabEpg();
            else
              Log.Info("TimeshiftingEPG: channel {0} is not configured for grabbing epg", channel.DisplayName);
          }
          return TvResult.Succeeded;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }

			this.Stop(ref user);
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool Stop(ref User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return true;        
				if (false == IsTimeShifting(ref user)) return true;
        if (_cardHandler.Recorder.IsRecording(ref user)) return true;

        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
				if (subchannel is BaseSubChannel)
				{          
          ((BaseSubChannel)subchannel).AudioVideoEvent -= new BaseSubChannel.AudioVideoObserverEvent(this.AudioVideoEventHandler);
				}
        
        _eventVideo.Close();
        _eventAudio.Close();
        _eventsReady = false;

        Log.Write("card {2}: StopTimeShifting user:{0} sub:{1}", user.Name, user.SubChannel, _cardHandler.Card.Name);
         
        lock (this)
        {
          bool result = false;          					

					try
					{
						RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
						if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return true;

						Log.Write("card: StopTimeShifting user:{0} sub:{1}", user.Name, user.SubChannel);

						if (_cardHandler.IsLocal == false)
						{
							result = RemoteControl.Instance.StopTimeShifting(ref user);
							return result;
						}
					}
					catch (Exception)
					{
						Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
						return false;
					}

          TvCardContext context = _cardHandler.Card.Context as TvCardContext;
          if (context == null) return true;
          if (_linkageScannerEnabled)
            _cardHandler.Card.ResetLinkageScanner();
          
          if (_cardHandler.IsIdle)
          {
            _cardHandler.StopCard(user);
          }
          else
          {
            if (subchannel != null)
            {
              Log.Debug("card not IDLE - freeing subch: {0}", subchannel.SubChannelId);
              subchannel.StopTimeShifting();
              _cardHandler.Card.FreeSubChannel(subchannel.SubChannelId); 
              /*
              if (subchannel is BaseSubChannel)
              {
                BaseSubChannel baseSubCh = (BaseSubChannel)subchannel;

                Log.Debug("card not IDLE - freeing subch: {0}", subchannel.SubChannelId);
                baseSubCh.Decompose();
                //subchannel.StopTimeShifting();              
                _cardHandler.Card.FreeSubChannel(subchannel.SubChannelId);              
              }
              */
            }
          }
          context.Remove(user);
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
    /// Start timeshifting on the card
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTimeShift(ref User user, ref string fileName)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false) return TvResult.CardIsDisabled;        

				try
				{
					RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
					if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return TvResult.CardIsDisabled;										
				}
				catch (Exception)
				{
					Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
					return TvResult.UnknownError;
				}

        Log.WriteFile("card: CardTimeShift {0} {1}", _cardHandler.DataBaseCard.IdCard, fileName);
        if (IsTimeShifting(ref user)) return TvResult.Succeeded;
        return Start(ref user, ref fileName);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }    

    /// <summary>
    /// Waits for time shift file to be at leat 300kb.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>true when timeshift files is at least of 300kb, else timeshift file is less then 300kb</returns>
    public bool WaitForTimeShiftFile(ref User user, out bool scrambled)
    {
      scrambled = false;
      if (_cardHandler.DataBaseCard.Enabled == false) return false;
			try
			{
				RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
				if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard)) return false;
			}
			catch (Exception)
			{
				Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
				return false;
			}

      // no need to wait for unscrambled signal, since the wait audio video pids are internally raised in tswriter when the packets are unscrambled.
      // instead we should query tswriter in the event that no audio/video events was received, wether or not the current state of the stream is scrambled or not.
      /*
      if (!WaitForUnScrambledSignal(ref user))
      {
        scrambled = true;
        return false;
      }
      */

      //lets check if stream is initially scrambled, if it is and the card has no CA, then we are unable to decrypt stream.
      if (_cardHandler.IsScrambled(ref user))
      {
        if (!_cardHandler.HasCA)
        {
          Log.Write("card: WaitForTimeShiftFile - return scrambled, since card has no CAM.");
          scrambled = true;
          return false;
        }
      }

      int waitForEvent = _waitForTimeshifting * 1000; // in ms           

      DateTime timeStart = DateTime.Now;

      if (_cardHandler.Card.SubChannels.Length <= 0) return false;
      IChannel channel = _cardHandler.Card.SubChannels[0].CurrentChannel;
      bool isRadio = channel.IsRadio;            

      if (isRadio)
      {
        Log.Write("card: WaitForTimeShiftFile - waiting _eventAudio");
        // wait for audio PID to be seen
        if (_eventAudio.WaitOne(waitForEvent, true))
        {
          // start of the video & audio is seen
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForTimeShiftFile - audio is seen after {0} seconds", ts.TotalSeconds);
          _eventVideo.Reset();
          _eventAudio.Reset();

          // give some breathing room for TsReader
          Thread.Sleep(200);
          return true;
        }
        else
        {
          _eventVideo.Reset();
          _eventAudio.Reset();
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForTimeShiftFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(ref user))
          {            
            Log.Write("card: WaitForTimeShiftFile - audio stream is scrambled");
            scrambled = true;            
          }
        }
      }
      else
      {
        Log.Write("card: WaitForTimeShiftFile - waiting _eventAudio & _eventVideo");
        // block until video & audio PIDs are seen or the timeout is reached
        if (_eventAudio.WaitOne(waitForEvent, true))
        {
          _eventAudio.Reset();
          if (_eventVideo.WaitOne(waitForEvent, true))
          {
            // start of the video & audio is seen
            TimeSpan ts = DateTime.Now - timeStart;
            Log.Write("card: WaitForTimeShiftFile - video and audio are seen after {0} seconds", ts.TotalSeconds);
            _eventVideo.Reset();

            // give some breathing room for TsReader
            Thread.Sleep(200);
            return true;
          }
          else
          {
            _eventVideo.Reset();
            _eventAudio.Reset();
            TimeSpan ts = DateTime.Now - timeStart;
            Log.Write("card: WaitForTimeShiftFile - video was found, but audio was not found after {0} seconds", ts.TotalSeconds);
            if (_cardHandler.IsScrambled(ref user))
            {
              Log.Write("card: WaitForTimeShiftFile - audio stream is scrambled");
              scrambled = true;
            }
          }
        }
        else
        {
          _eventVideo.Reset();
          _eventAudio.Reset();
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForTimeShiftFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(ref user))
          {
            Log.Write("card: WaitForTimeShiftFile - audio and video stream is scrambled");
            scrambled = true;
          }
        }
      }      
      return false;
    }
  }
}
