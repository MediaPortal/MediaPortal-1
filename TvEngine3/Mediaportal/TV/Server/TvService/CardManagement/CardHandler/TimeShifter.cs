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
using System.IO;
using System.Linq;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.ChannelLinkage;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.CardManagement.CardHandler
{
  public class TimeShifter : TimeShifterBase, ITimeShifter
  {
    private readonly bool _linkageScannerEnabled;
    private readonly ChannelLinkageGrabber _linkageGrabber;
    private bool _tuneInProgress;    
    private DateTime _timeAudioEvent;
    private DateTime _timeVideoEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeShifter"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public TimeShifter(ITvCardHandler cardHandler) : base(cardHandler)
    {


      _cardHandler = cardHandler;
      
      _linkageScannerEnabled = (SettingsManagement.GetSetting("linkageScannerEnabled", "no").value == "yes");

      _linkageGrabber = new ChannelLinkageGrabber(cardHandler.Card);
      _timeshiftingEpgGrabberEnabled = (SettingsManagement.GetSetting("timeshiftingEpgGrabberEnabled", "no").value == "yes");

      _timeAudioEvent = DateTime.MinValue;
      _timeVideoEvent = DateTime.MinValue;
    }

    /// <summary>
    /// Gets the name of the time shift file.
    /// </summary>
    /// <value>The name of the time shift file.</value>
    public string FileName(ref IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return "";
        }
      
        var context = _cardHandler.Card.Context as ITvCardContext;
        if (context == null)
          return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return null;
        return subchannel.TimeShiftFileName + ".tsbuffer";
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public bool GetCurrentFilePosition(ref IUser user, ref Int64 position, ref long bufferId)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return false;
        }
       
        var context = _cardHandler.Card.Context as ITvCardContext;
        if (context == null)
          return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return false;
        subchannel.TimeShiftGetCurrentFilePosition(ref position, ref bufferId);
        return (position != -1);
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
    public bool IsAnySubChannelTimeshifting
    {
      get
      {
        IDictionary<string, IUser> users = _cardHandler.UserManagement.Users;        
        if (users.Values.Select(user => (IUser) user.Clone()).Any(userCopy => IsTimeShifting(ref userCopy)))
        {
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(ref IUser user)
    {
      bool isTimeShifting = false;
      try
      {
        var subchannel = GetSubChannel(ref user);
        if (subchannel != null)
        {
          isTimeShifting = subchannel.IsTimeShifting;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return isTimeShifting;
    }


    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(IUser user)
    {
      DateTime timeShiftStarted = DateTime.MinValue;
      try
      {
        ITvSubChannel subchannel = GetSubChannel(ref user);
        if (subchannel != null)
        {
          timeShiftStarted = subchannel.StartOfTimeShift;
          }
        }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return timeShiftStarted;
    }

    protected override void AudioVideoEventHandler(PidType pidType)
    {
      if (_tuneInProgress)
      {
        Log.Info("audioVideoEventHandler - tune in progress");
        return;
      }

      // we are only interested in video and audio PIDs
      if (pidType == PidType.Audio)
      {
        TimeSpan ts = DateTime.Now - _timeAudioEvent;
        if (ts.TotalMilliseconds > 1000)
        {
          // Avoid repetitive events that are kept for next channel change, so trig only once.
          Log.Info("audioVideoEventHandler {0}", pidType);
          _eventAudio.Set();
        }
        else
        {
          Log.Info("audio last seen at {0}", _timeAudioEvent);
        }
        _timeAudioEvent = DateTime.Now;
      }

      if (pidType == PidType.Video)
      {
        TimeSpan ts = DateTime.Now - _timeVideoEvent;
        if (ts.TotalMilliseconds > 1000)
        {
          // Avoid repetitive events that are kept for next channel change, so trig only once.
          Log.Info("audioVideoEventHandler {0}", pidType);
          _eventVideo.Set();
        }
        else
        {
          Log.Info("video last seen at {0}", _timeVideoEvent);
        }
        _timeVideoEvent = DateTime.Now;
      }
    }

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult Start(ref IUser user, ref string fileName)
    {
      TvResult result = TvResult.UnknownError;
      try
      {
#if DEBUG

        if (File.Exists(@"\failts_" + _cardHandler.DataBaseCard.idCard))
        {
          throw new Exception("failed ts on purpose");
        }
#endif
        if (IsTuneCancelled())
        {
          result = TvResult.TuneCancelled;
          return result;
        }
        _eventTimeshift.Reset();
        if (_cardHandler.DataBaseCard.enabled)
          {
            // Let's verify if hard disk drive has enough free space before we start time shifting. The function automatically handles both local and UNC paths
          if (!IsTimeShifting(ref user) && !HasFreeDiskSpace(fileName))
            {
            result = TvResult.NoFreeDiskSpace;            
              }
          else
          {
            Log.Write("card: StartTimeShifting {0} {1} ", _cardHandler.DataBaseCard.idCard, fileName);
            var context = _cardHandler.Card.Context as ITvCardContext;
            if (context != null)
            {
          context.GetUser(ref user);
              ITvSubChannel subchannel = GetSubChannel(user.SubChannel);

              if (subchannel != null)
          {
          _subchannel = subchannel;
          Log.Write("card: CAM enabled : {0}", _cardHandler.HasCA);
                bool pmtReceived = IsPMTreceived(subchannel);
                if (pmtReceived)
          {
                  AttachAudioVideoEventHandler(subchannel);
          bool isScrambled;
          if (subchannel.IsTimeShifting)
          {
                    result = GetTvResultFromTimeshiftingSubchannel(ref user, context);                    
                  }
                  else
            {
                    bool tsStarted = subchannel.StartTimeShifting(fileName);
                    if (tsStarted)
              {
                      fileName += ".tsbuffer";
                      result = GetTvResultFromTimeshiftingSubchannel(ref user, context);
              }
              else
                    {                      
                      result = TvResult.UnableToStartGraph; 
            }
          }
          }
                else
          {
                  Log.Info("start subch:{0} No PMT received. Timeshifting failed", subchannel.SubChannelId);
                  result = TvResult.UnableToStartGraph;
            }
          }
          }
        }
      }
        else
      {
          result = TvResult.CardIsDisabled;
      }
    }
      catch (Exception ex)
    {
        Log.Write(ex);
        result = TvResult.UnknownError;        
        }
      finally
        {
        _eventTimeshift.Set();
        _cancelled = false;
        if (result != TvResult.Succeeded)
          {
          Stop(ref user);
            }
          }
      return result;
          }

    private TvResult GetTvResultFromTimeshiftingSubchannel(ref IUser user, ITvCardContext context)
          {
      TvResult result;
      bool isScrambled;
      if (WaitForFile(ref user, out isScrambled))
      {
        context.OnZap(user);
        StartLinkageScanner();
        StartTimeShiftingEPGgrabber(user);
        result = TvResult.Succeeded;
          }
          else
          {
        result = GetFailedTvResult(isScrambled);
          }
      return result;
        }

    private void StartLinkageScanner()
      {
      if (_linkageScannerEnabled)
      {
        _cardHandler.Card.StartLinkageScanner(_linkageGrabber);
      }
    }

    private static bool IsPMTreceived(ITvSubChannel subchannel)
    {
      var tvDvbChannel = subchannel as TvDvbChannel;
      bool pmtReceived = (tvDvbChannel != null && tvDvbChannel.PMTreceived);
      return pmtReceived;
    }

    private static bool HasFreeDiskSpace(string fileName)
        {
      ulong freeDiskSpace = Utils.GetFreeDiskSpace(fileName);

      
      UInt32 maximumFileSize = UInt32.Parse(SettingsManagement.GetSetting("timeshiftMaxFileSize", "256").value); // in MB
      ulong diskSpaceNeeded = Convert.ToUInt64(maximumFileSize);
      diskSpaceNeeded *= 1000000 * 2; // Convert to bytes; 2 times of timeshiftMaxFileSize
      bool hasFreeDiskSpace = freeDiskSpace > diskSpaceNeeded;
      return hasFreeDiskSpace;
      }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns></returns>    
    public bool Stop(ref IUser user)
    {
      bool stop = false;
      try
      {
        if (_cardHandler.DataBaseCard.enabled)
      {
          ITvSubChannel subchannel = GetSubChannel(user.SubChannel);
          DetachAudioVideoEventHandler(subchannel);          
          Log.Write("card {2}: StopTimeShifting user:{0} sub:{1}", user.Name, user.SubChannel,
                    _cardHandler.Card.Name);
          var context = _cardHandler.Card.Context as ITvCardContext;

          if (context != null)
{
            ResetLinkageScanner();
            if (_cardHandler.IsIdle)
      {
              _cardHandler.PauseCard(user);
        }
        else
        {
              Log.Debug("card not IDLE - removing user: {0}", user.Name);
              _cardHandler.UserManagement.RemoveUser(user);
          }
            context.Remove(user);
            stop = true;        
        }
      }
          }
      catch (Exception ex)
          {
        Log.Write(ex);
            }
      return stop;
          }

    private void ResetLinkageScanner()
        {
      if (_linkageScannerEnabled)
          {
        _cardHandler.Card.ResetLinkageScanner();
          }
        }

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>   
    /// <param name="user">user</param>    
    /// <param name="totalTSpackets">Amount of packets processed</param>    
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>
    public void GetStreamQualityCounters(IUser user, out int totalTSpackets, out int discontinuityCounter)
    {
      totalTSpackets = 0;
      discontinuityCounter = 0;

      var context = _cardHandler.Card.Context as ITvCardContext;
      if (context != null)
      {
      bool userExists;
      context.GetUser(ref user, out userExists);
      }
      ITvSubChannel subchannel = GetSubChannel(user.SubChannel);

      var dvbSubchannel = subchannel as TvDvbChannel;
      if (dvbSubchannel != null)
      {
        dvbSubchannel.GetStreamQualityCounters(out totalTSpackets, out discontinuityCounter);
      }
    }

    public void OnBeforeTune()
    {
      Log.Debug("TimeShifter.OnBeforeTune: resetting audio/video events");
      _tuneInProgress = true;
      _eventAudio.Reset();
      _eventVideo.Reset();
    }

    public void OnAfterTune()
    {
      Log.Debug("TimeShifter.OnAfterTune: resetting audio/video time");
      _timeAudioEvent = DateTime.MinValue;
      _timeVideoEvent = DateTime.MinValue;

      _tuneInProgress = false;
    }
  }
}