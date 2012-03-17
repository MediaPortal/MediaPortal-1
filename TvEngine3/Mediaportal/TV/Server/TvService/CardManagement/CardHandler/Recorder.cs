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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.CardManagement.CardHandler
{
  public class Recorder : TimeShifterBase, IRecorder
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Recording"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public Recorder(ITvCardHandler cardHandler) : base(cardHandler)
    {
      string recordingFolder = cardHandler.DataBaseCard.recordingFolder;

      bool hasFolder = TVDatabase.TVBusinessLayer.Common.IsFolderValid(recordingFolder);

      if (!hasFolder)
      {
        recordingFolder = SetDefaultRecordingFolder(cardHandler);
      }

      if (!Directory.Exists(recordingFolder))
      {
        try
        {
          Directory.CreateDirectory(recordingFolder);
        }
        catch (Exception)
        {
          recordingFolder = SetDefaultRecordingFolder(cardHandler);
          Directory.CreateDirectory(recordingFolder); //if it fails, then nothing works reliably.
        }        
      }

      _cardHandler = cardHandler;
      _timeshiftingEpgGrabberEnabled = (SettingsManagement.GetSetting("timeshiftingEpgGrabberEnabled", "no").value == "yes");
    }

    private static string SetDefaultRecordingFolder(ITvCardHandler cardHandler)
    {
      string recordingFolder;
      recordingFolder = TVDatabase.TVBusinessLayer.Common.GetDefaultRecordingFolder();
      cardHandler.DataBaseCard.recordingFolder = recordingFolder;
      TVDatabase.TVBusinessLayer.CardManagement.SaveCard(cardHandler.DataBaseCard);
      return recordingFolder;
    }


    protected override void AudioVideoEventHandler(PidType pidType)
    {
      Log.Debug("Recorder audioVideoEventHandler {0}", pidType);

      // we are only interested in video and audio PIDs
      if (pidType == PidType.Audio)
      {
        _eventAudio.Set();
      }

      if (pidType == PidType.Video)
      {
        _eventVideo.Set();
      }
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns></returns>
    public TvResult Start(ref IUser user, ref string fileName)
    {
      TvResult result = TvResult.UnknownError;
      try
      {
#if DEBUG
        if (File.Exists(@"\failrec_" + _cardHandler.DataBaseCard.idCard))
        {
          throw new Exception("failed rec. on purpose");
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
          var context = _cardHandler.Card.Context as TvCardContext;
          if (context != null)
          {
          context.GetUser(ref user);
            ITvSubChannel subchannel = GetSubChannel(user.SubChannel);
            if (subchannel != null)
            {
          _subchannel = subchannel;

          fileName = fileName.Replace("\r\n", " ");
              fileName = Path.ChangeExtension(fileName, ".ts");

              bool useErrorDetection = true;
          if (useErrorDetection)
          {
            // fix mantis 0002807: A/V detection for recordings is not working correctly 
            // reset the events ONLY before attaching the observer, at a later position it can already miss the a/v callback.
                if (IsTuneCancelled())
                {
                  result = TvResult.TuneCancelled;
                  return result;
                }
            _eventVideo.Reset();
            _eventAudio.Reset();
            Log.Debug("Recorder.start add audioVideoEventHandler");
                AttachAudioVideoEventHandler(subchannel);                
          }

          Log.Write("card: StartRecording {0} {1}", _cardHandler.DataBaseCard.idCard, fileName);
              bool recStarted = subchannel.StartRecording(fileName);
              if (recStarted)
          {
            fileName = subchannel.RecordingFileName;
            context.Owner = user;
            if (useErrorDetection)
            {
                  bool isScrambled;
                  if (WaitForFile(ref user, out isScrambled))
              {
                    result = TvResult.Succeeded;
                }
                else
                {
                    DetachAudioVideoEventHandler(subchannel);
                    result = GetFailedTvResult(isScrambled);
                }
                }
              }
            }
          }
        }
            else
        {
          result = TvResult.CardIsDisabled;
          }
        if (result == TvResult.Succeeded)
        {
          StartTimeShiftingEPGgrabber(user);
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
          HandleFailedRecording(ref user, fileName);                    
    }
      }
      return result;
    }

    private void HandleFailedRecording(ref IUser user, string fileName)
    {
      Log.Write("card: Recording failed! {0} {1}", _cardHandler.DataBaseCard.idCard, fileName);
      string cardRecordingFolderName = _cardHandler.DataBaseCard.recordingFolder;
      Stop(ref user);
      _cardHandler.UserManagement.RemoveUser(user);

      string recordingfolderName = System.IO.Path.GetDirectoryName(fileName);
      if (recordingfolderName == cardRecordingFolderName)
      {
        Utils.FileDelete(fileName);
      }
      else
      {
        // delete 0-byte file in case of error
        Utils.DeleteFileAndEmptyDirectory(fileName);
      }
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    public bool Stop(ref IUser user)
    {
      bool stop = false;
      try
      {
        if (_cardHandler.DataBaseCard.enabled)
        {
        Log.Write("card: StopRecording card={0}, user={1}", _cardHandler.DataBaseCard.idCard, user.Name);
          var context = _cardHandler.Card.Context as TvCardContext;
          if (context != null)
        {
            if (user.IsAdmin)
          {
              stop = StopRecording(ref user, context);
              if (stop)
            {
                SetContextOwnerToNextRecUser(context);
            }
          }
          }
          else
          {
            Log.Write("card: StopRecording context null");
          }
            }
      }
      catch (Exception ex)
            {
        Log.Write(ex);
            }
      return stop;
          }

    private bool StopRecording(ref IUser user, TvCardContext context)
          {
      bool stop = false;
      context.GetUser(ref user);
      ITvSubChannel subchannel = GetSubChannel(user.SubChannel);
            if (subchannel != null)
            {
        subchannel.StopRecording();
        _cardHandler.Card.FreeSubChannel(user.SubChannel);
        if (subchannel.IsTimeShifting == false || context.Users.Count <= 1)
              {
          _cardHandler.UserManagement.RemoveUser(user);
              }
        stop = true;
            }
      else
      {
        Log.Write("card: StopRecording subchannel null, skipping");        
      }
      return stop;
    }

    private void SetContextOwnerToNextRecUser(ITvCardContext context)
    {
      IDictionary<string, IUser> users = context.Users;
      foreach (KeyValuePair<string, IUser> t in users)
      {
        ITvSubChannel subchannel = GetSubChannel(t.Value.SubChannel);
        if (subchannel != null)
        {
          if (subchannel.IsRecording)
          {
            Log.Write("card: StopRecording setting new context owner on user '{0}'", t.Value.Name);
            context.Owner = t.Value;
            break;
          }
        }
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
        IDictionary<string, IUser> users = _cardHandler.UserManagement.Users;
        if (users.Values.Select(user => (IUser) user.Clone()).Any(userCopy => IsRecording(ref userCopy)))
        {
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording(ref IUser user)
    {
      bool isRecording = false;
      try
      {
        var subchannel = GetSubChannel(ref user);
        if (subchannel != null)
        {
          isRecording = subchannel.IsRecording;
          }
        }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return isRecording;
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>filename or null when not recording</returns>
    public string FileName(ref IUser user)
    {
      string recordingFileName = "";
      try
      {
        ITvSubChannel subchannel = GetSubChannel(ref user);
        if (subchannel != null)
        {
          recordingFileName = subchannel.RecordingFileName; 
          }
        }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return recordingFileName;
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(IUser user)
    {
      DateTime recordingStarted = DateTime.MinValue;
      try
      {
        ITvSubChannel subchannel = GetSubChannel(ref user);
        if (subchannel != null)
        {
          recordingStarted = subchannel.RecordingStarted;
          }
        }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return recordingStarted;
    }
        }
}