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
using System.IO;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public class TimeShifter : TimeShifterBase, ITimeShifter
  {    
    private readonly ChannelLinkageGrabber _linkageGrabber;
    private readonly bool _linkageScannerEnabled;
    private DateTime _timeAudioEvent;
    private DateTime _timeVideoEvent;
    private bool _tuneInProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeShifter"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public TimeShifter(ITvCardHandler cardHandler)
    {
      string timeshiftingFolder = cardHandler.DataBaseCard.TimeshiftingFolder;

      bool hasFolder = TVDatabase.TVBusinessLayer.Common.IsFolderValid(timeshiftingFolder);
      if (!hasFolder)
      {
        timeshiftingFolder = SetDefaultTimeshiftingFolder(cardHandler);
      }

      if (!Directory.Exists(timeshiftingFolder))
      {
        try
        {
          Directory.CreateDirectory(timeshiftingFolder);
        }
        catch (Exception)
        {
          timeshiftingFolder = SetDefaultTimeshiftingFolder(cardHandler);
          Directory.CreateDirectory(timeshiftingFolder); //if it fails, then nothing works reliably.s
        }
      }

      _cardHandler = cardHandler;

      _linkageScannerEnabled = (SettingsManagement.GetSetting("linkageScannerEnabled", "no").Value == "yes");

      _linkageGrabber = new ChannelLinkageGrabber(cardHandler.Card);
      _timeshiftingEpgGrabberEnabled = (SettingsManagement.GetSetting("timeshiftingEpgGrabberEnabled", "no").Value ==
                                        "yes");

      _timeAudioEvent = DateTime.MinValue;
      _timeVideoEvent = DateTime.MinValue;
    }

    #region ITimeShifter Members

    /// <summary>
    /// Gets the name of the time shift file.
    /// </summary>
    /// <value>The name of the time shift file.</value>
    public string FileName(ref IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return "";
        }
                
        ITvSubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
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
    /// <param name="userName"> </param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public bool GetCurrentFilePosition(string userName, ref long position, ref long bufferId)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return false;
        }
                
        ITvSubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingSubChannel(userName));
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
        return _cardHandler.UserManagement.IsAnyUserTimeShifting();        
      }
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(IUser user)
    {
      bool isTimeShifting = false;
      try
      {
        foreach (ISubChannel subch in user.SubChannels.Values)
        {
          ITvSubChannel subchannel = GetSubChannel(user.Name, subch.IdChannel);
          if (subchannel != null)
          {
            isTimeShifting = subchannel.IsTimeShifting;
            if (isTimeShifting)
            {
              break; 
            }            
          } 
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
    public DateTime TimeShiftStarted(string userName, int idChannel)
    {
      DateTime timeShiftStarted = DateTime.MinValue;
      try
      {
        ITvSubChannel subchannel = GetSubChannel(userName, idChannel);
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

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <param name="subChannelId1"> </param>
    /// <param name="subChannelId"> </param>
    /// <param name="idChannel"> </param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult Start(ref IUser user, ref string fileName, int subChannelId, int idChannel)
    {
      TvResult result = TvResult.UnknownError;
      try
      {
#if DEBUG

        if (File.Exists(@"\failts_" + _cardHandler.DataBaseCard.IdCard))
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
        if (_cardHandler.DataBaseCard.Enabled)
        {
          // Let's verify if hard disk drive has enough free space before we start time shifting. The function automatically handles both local and UNC paths
          if (!IsTimeShifting(user) && !HasFreeDiskSpace(fileName))
          {
            result = TvResult.NoFreeDiskSpace;
          }
          else
          {
            Log.Write("card: StartTimeShifting {0} {1} ", _cardHandler.DataBaseCard.IdCard, fileName);            
            
            _cardHandler.UserManagement.RefreshUser(ref user);
            ITvSubChannel subchannel = GetSubChannel(subChannelId);

            if (subchannel != null)
            {
              _subchannel = subchannel;
              Log.Write("card: CAM enabled : {0}", _cardHandler.IsConditionalAccessSupported);
              AttachAudioVideoEventHandler(subchannel);
              if (subchannel.IsTimeShifting)
              {
                result = GetTvResultFromTimeshiftingSubchannel(ref user);
              }
              else
              {
                bool tsStarted = subchannel.StartTimeShifting(fileName);
                if (tsStarted)
                {
                  fileName += ".tsbuffer";
                  result = GetTvResultFromTimeshiftingSubchannel(ref user);
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
          Stop(ref user, idChannel);
        }
      }
      return result;
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns></returns>    
    public bool Stop(ref IUser user, int idChannel)
    {
      bool stop = false;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled)
        {
          ITvSubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel));
          DetachAudioVideoEventHandler(subchannel);
          Log.Write("card {2}: StopTimeShifting user:{0} sub:{1}", user.Name, _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel),
                    _cardHandler.Card.Name);          
          ResetLinkageScanner();          
          _cardHandler.UserManagement.RemoveUser(user, idChannel);
          stop = true;          
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return stop;
    }

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="totalTSpackets">Amount of packets processed</param>
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>
    public void GetStreamQualityCounters(string userName, out int totalTSpackets, out int discontinuityCounter)
    {
      totalTSpackets = 0;
      discontinuityCounter = 0;

      ITvSubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingSubChannel(userName));

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

    #endregion

    private static string SetDefaultTimeshiftingFolder(ITvCardHandler cardHandler)
    {
      string timeshiftingFolder = TVDatabase.TVBusinessLayer.Common.GetDefaultTimeshiftingFolder();
      cardHandler.DataBaseCard.TimeshiftingFolder = timeshiftingFolder;
      TVDatabase.TVBusinessLayer.CardManagement.SaveCard(cardHandler.DataBaseCard);
      return timeshiftingFolder;
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

    private TvResult GetTvResultFromTimeshiftingSubchannel(ref IUser user)
    {
      TvResult result;
      bool isScrambled;
      if (WaitForFile(ref user, out isScrambled))
      {
        _cardHandler.UserManagement.OnZap(user, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
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

    private static bool HasFreeDiskSpace(string fileName)
    {
      ulong freeDiskSpace = Utils.GetFreeDiskSpace(fileName);


      UInt32 maximumFileSize = UInt32.Parse(SettingsManagement.GetSetting("timeshiftMaxFileSize", "256").Value);
        // in MB
      ulong diskSpaceNeeded = Convert.ToUInt64(maximumFileSize);
      diskSpaceNeeded *= 1000000*2; // Convert to bytes; 2 times of timeshiftMaxFileSize
      bool hasFreeDiskSpace = freeDiskSpace > diskSpaceNeeded;
      return hasFreeDiskSpace;
    }

    private void ResetLinkageScanner()
    {
      if (_linkageScannerEnabled)
      {
        _cardHandler.Card.ResetLinkageScanner();
      }
    }
  }
}