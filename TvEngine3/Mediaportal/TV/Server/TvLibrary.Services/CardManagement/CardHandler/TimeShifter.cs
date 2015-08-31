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
using System.Text.RegularExpressions;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Enum;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using IServiceSubChannel = Mediaportal.TV.Server.TVService.Interfaces.Services.ISubChannel;
using ITvLibrarySubChannel = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ISubChannel;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public class TimeShifter : TimeShifterBase, ITimeShifter
  {
    private readonly ChannelLinkageGrabber _linkageGrabber;
    private DateTime _timeAudioEvent;
    private DateTime _timeVideoEvent;
    private bool _tuneInProgress;

    /// <summary>
    /// The minimum number of buffer files to use for time shifting.
    /// </summary>
    protected int _fileCount = 6;

    /// <summary>
    /// The maximum number of buffer files to use for time shifting.
    /// </summary>
    protected int _fileCountMaximum = 20;

    /// <summary>
    /// The size in bytes of each time shift buffer file.
    /// </summary>
    protected ulong _fileSize = 256000000;    // 256 MB

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeShifter"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public TimeShifter(ITvCardHandler cardHandler)
      : base(cardHandler, "timeShiftBufferFolder")
    {
      _linkageGrabber = new ChannelLinkageGrabber(cardHandler.Card);
      _timeAudioEvent = DateTime.MinValue;
      _timeVideoEvent = DateTime.MinValue;
    }

    #region ITimeShifter Members

    public override void ReloadConfiguration()
    {
      this.LogDebug("time-shifter: reload configuration");

      string currentFolder = _folder;
      base.ReloadConfiguration();
      if (!string.Equals(currentFolder, _folder) && Directory.Exists(_folder))
      {
        // Remove any old timeshift buffer files.
        try
        {
          Regex r = new Regex(@"^live\d+-\d+\.ts\.tsbuffer");
          string[] files = Directory.GetFiles(_folder);
          foreach (string file in files)
          {
            try
            {
              // TODO Ideally we should avoid making assumptions about the
              // format of the buffer file names, because they're not in our
              // control.
              if (r.Match(Path.GetFileName(file)).Success)
              {
                File.Delete(file);
              }
            }
            catch (Exception ex)
            {
              this.LogWarn(ex, "time-shifter: failed to delete old file, file = {0}", file);
            }
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "time-shifter: failed to delete old files");
        }
      }

      _fileSize = (ulong)SettingsManagement.GetValue("timeShiftBufferFileSize", 256) * 1000 * 1000;
      _fileCount = SettingsManagement.GetValue("timeShiftBufferFileCount", 6);
      _fileCountMaximum = SettingsManagement.GetValue("timeShiftBufferFileCountMaximum", 20);
      this.LogDebug("  buffer file size          = {0} bytes", _fileSize);
      this.LogDebug("  buffer file count         = {0}", _fileCount);
      this.LogDebug("  buffer file count max     = {0}", _fileCountMaximum);
    }

    /// <summary>
    /// Gets the name of the time shift file.
    /// </summary>
    /// <value>The name of the time shift file.</value>
    public string FileName(ref IUser user)
    {
      try
      {
        if (_cardHandler.Card.IsEnabled == false)
        {
          return "";
        }
                
        ITvLibrarySubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        if (subchannel == null)
          return null;
        return subchannel.TimeShiftFileName;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        return "";
      }
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public bool GetCurrentFilePosition(string userName, out long position, out long bufferId)
    {
      position = 0;
      bufferId = 0;
      try
      {
        if (_cardHandler.Card.IsEnabled == false)
        {
          return false;
        }
                
        ITvLibrarySubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingSubChannel(userName));
        if (subchannel == null)
          return false;
        subchannel.TimeShiftGetCurrentFilePosition(out position, out bufferId);
        return (position != -1);
      }
      catch (Exception ex)
      {
        this.LogError(ex);
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
      if (user == null || user.SubChannels == null)
        return false;

      try
      {
        foreach (IServiceSubChannel subch in user.SubChannels.Values)
        {
          ITvLibrarySubChannel subchannel = GetSubChannel(user.Name, subch.IdChannel);
          if (subchannel != null && subchannel.IsTimeShifting)
            return true;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      return false;
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
        ITvLibrarySubChannel subchannel = GetSubChannel(userName, idChannel);
        if (subchannel != null)
        {
          timeShiftStarted = subchannel.StartOfTimeShift;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      return timeShiftStarted;
    }

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <param name="subChannelId"> </param>
    /// <param name="idChannel"> </param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult Start(ref IUser user, out string fileName, int subChannelId, int idChannel)
    {
      TvResult result = TvResult.UnknownError;
      fileName = string.Empty;
      try
      {
#if DEBUG

        if (File.Exists(@"\failts_" + _cardHandler.Card.TunerId))
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
        if (_cardHandler.Card.IsEnabled)
        {
          var subChannelByChannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel);
          fileName = Path.Combine(_folder, string.Format("live{0}-{1}.ts.tsbuffer", user.CardId, subChannelByChannelId));
          if (!IsTimeShifting(user))
          {
            CleanTimeShiftFiles(_folder, fileName);
          }

          // Let's verify if hard disk drive has enough free space before we start time shifting. The function automatically handles both local and UNC paths
          if (!IsTimeShifting(user) && Utils.GetFreeDiskSpace(fileName) < 2 * _fileSize)
          {
            result = TvResult.NoFreeDiskSpace;
          }
          else
          {
            this.LogDebug("card: StartTimeShifting {0} {1} ", _cardHandler.Card.TunerId, fileName);            
            
            _cardHandler.UserManagement.RefreshUser(ref user);
            ITvLibrarySubChannel subchannel = GetSubChannel(subChannelId);

            if (subchannel != null)
            {
              _subchannel = subchannel;
              AttachAudioVideoEventHandler(subchannel);
              if (subchannel.IsTimeShifting)
              {
                result = GetTvResultFromTimeshiftingSubchannel(ref user);
              }
              else
              {
                if (subchannel.StartTimeShifting(fileName, _fileCount, _fileCountMaximum, _fileSize))
                {
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
              this.LogError("start subch:subchannel is null");
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
        this.LogError(ex);
        result = TvResult.UnknownError;
      }
      finally
      {
        _eventTimeshift.Set();
        _cancelled = false;
        if (result != TvResult.Succeeded)
        {
          Stop(ref user, idChannel);
          fileName = string.Empty;
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
        if (_cardHandler.Card.IsEnabled)
        {
          ITvLibrarySubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel));
          DetachAudioVideoEventHandler(subchannel);
          this.LogDebug("card {2}: StopTimeShifting user:{0} sub:{1}", user.Name, _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel),
                    _cardHandler.Card.Name);          
          ResetLinkageScanner();
          _cardHandler.UserManagement.RemoveUser(user, idChannel);
          if (_cardHandler.IsIdle)
          {
            StopTimeShiftingEpgGrabber();
          }
          stop = true;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
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

      ITvLibrarySubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingSubChannel(userName));
      if (subchannel != null)
      {
        subchannel.GetStreamQualityCounters(out totalTSpackets, out discontinuityCounter);
      }
    }

    public void OnBeforeTune()
    {
      this.LogDebug("TimeShifter.OnBeforeTune: resetting audio/video events");
      _tuneInProgress = true;
      _eventAudio.Reset();
      _eventVideo.Reset();
    }

    public void OnAfterTune()
    {
      this.LogDebug("TimeShifter.OnAfterTune: resetting audio/video time");
      _timeAudioEvent = DateTime.MinValue;
      _timeVideoEvent = DateTime.MinValue;

      _tuneInProgress = false;
    }

    public void CopyBuffer(string userName, long position1, long bufferId1, long position2, long bufferId2, string destination)
    {
      this.LogInfo("time-shifter: copy buffer, user = {0}, start = {1}.{2}, end = {3}.{4}, destination = {5}", userName, bufferId1, position1, bufferId2, position2, destination);
      try
      {
        ITvLibrarySubChannel subchannel = GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingSubChannel(userName));
        if (subchannel == null || string.IsNullOrEmpty(subchannel.TimeShiftFileName))
        {
          return;
        }

        string baseFileName = subchannel.TimeShiftFileName;
        this.LogDebug("time-shifter: time-shift file name = {0}", baseFileName);

        // We assume it is okay to merge the files. In theory this might not be
        // a valid assumption. However, for now we only support MPEG 2 TS, and
        // the buffer files should be aligned to TS packet boundaries.
        // Therefore the assumption should hold.
        using (var writer = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
        {
          long id = bufferId1;
          const int BUFFER_SIZE = 940;   // multiple of TS packet size (188)
          byte[] buffer = new byte[BUFFER_SIZE];
          while (true)
          {
            // TODO Ideally we should avoid making assumptions about the format
            // of the buffer file names, because they're not in our control.
            string source = baseFileName + id.ToString() + ".ts";
            if (!File.Exists(source))
            {
              if (bufferId1 <= bufferId2 || id <= bufferId2)
              {
                break;
              }
              id = 1;
              source = baseFileName + "1.ts";
              if (!File.Exists(source))
              {
                break;
              }
            }

            this.LogDebug("  copy {0}", source);
            using (var reader = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
              long sourcePosition = 0;
              if (id == bufferId1)
              {
                sourcePosition = position1;
              }
              sourcePosition = reader.Seek(sourcePosition, SeekOrigin.Begin);

              do
              {
                int readSize = BUFFER_SIZE;
                if (id == bufferId2)
                {
                  readSize = (int)Math.Min(position2 - sourcePosition, (long)BUFFER_SIZE);
                  if (readSize == 0)
                  {
                    break;
                  }
                }
                int bytesRead = reader.Read(buffer, 0, readSize);
                if (bytesRead == 0)
                {
                  break;
                }

                sourcePosition += bytesRead;
                writer.Write(buffer, 0, bytesRead);
              }
              while (true);
              reader.Close();
              reader.Dispose();
            }

            if (id == bufferId2)
            {
              break;
            }

            id++;
          }

          writer.Flush();
          writer.Close();
          writer.Dispose();
        }
        this.LogInfo("time-shifter: copy complete");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "time-shifter: failed to copy buffer");
      }
    }

    #endregion

    protected override string GetDefaultFolder()
    {
      return Path.Combine(PathManager.GetDataPath, "Time-shift Buffer");
    }

    protected override void AudioVideoEventHandler(PidType pidType)
    {
      if (_tuneInProgress)
      {
        this.LogInfo("audioVideoEventHandler - tune in progress");
        return;
      }

      // we are only interested in video and audio PIDs
      if (pidType == PidType.Audio)
      {
        TimeSpan ts = DateTime.Now - _timeAudioEvent;
        if (ts.TotalMilliseconds > 1000)
        {
          // Avoid repetitive events that are kept for next channel change, so trig only once.
          this.LogInfo("audioVideoEventHandler {0}", pidType);
          _eventAudio.Set();
        }
        else
        {
          this.LogInfo("audio last seen at {0}", _timeAudioEvent);
        }
        _timeAudioEvent = DateTime.Now;
      }

      if (pidType == PidType.Video)
      {
        TimeSpan ts = DateTime.Now - _timeVideoEvent;
        if (ts.TotalMilliseconds > 1000)
        {
          // Avoid repetitive events that are kept for next channel change, so trig only once.
          this.LogInfo("audioVideoEventHandler {0}", pidType);
          _eventVideo.Set();
        }
        else
        {
          this.LogInfo("video last seen at {0}", _timeVideoEvent);
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
        StartTimeShiftingEpgGrabber(user);
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
      IChannelLinkageScanner scanner = _cardHandler.Card.ChannelLinkageScanningInterface;
      if (scanner != null)
      {
        scanner.Start(_linkageGrabber);
      }
    }

    private void ResetLinkageScanner()
    {
      IChannelLinkageScanner scanner = _cardHandler.Card.ChannelLinkageScanningInterface;
      if (scanner != null)
      {
        scanner.Reset();
      }
    }

    /// <summary>
    /// deletes time shifting files left in the specified folder.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="fileName">Name of the file.</param>
    private static void CleanTimeShiftFiles(string folder, string fileName)
    {
      try
      {
        Log.Debug(@"time-shifter: delete timeshift files {0}\{1}", folder, fileName);
        string[] files = Directory.GetFiles(folder);
        foreach (string f in files)
        {
          if (f.StartsWith(fileName))
          {
            try
            {
              Log.Debug("time-shifter:   delete {0}", f);
              File.Delete(f);
            }
            catch (Exception e)
            {
              Log.Debug("time-shifter: Error \"{0}\" on delete in CleanTimeshiftFiles", e.Message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }
  }
}