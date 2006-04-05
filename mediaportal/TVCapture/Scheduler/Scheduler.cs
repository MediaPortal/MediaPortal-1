/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#region usings
using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
#endregion

namespace MediaPortal.TV.Recording
{
  public class Scheduler
  {
    // flag indicating that recordings have been added/changed/removed
    bool _recordingsListChanged ;

    //list of all scheduled recordings
    List<TVRecording> _recordingsList;
    //list of all tv channels present in tv database
    List<TVChannel> _tvChannelsList ;


    //specifies the number of minutes the notify should be send before a program starts
    int _preRecordingWarningTime = 2;
    // number of minutes we should start recording before the program starts
    int _preRecordInterval = 0;
    // number of minutes we keeprecording after the program starts
    int _postRecordInterval = 0;
    DateTime _scheduleTimer;

    public Scheduler()
    {
      _recordingsListChanged = false;
      _recordingsList = new List<TVRecording>();
      _recordingsList.Clear();
      TVDatabase.GetRecordings(ref _recordingsList);
      TVDatabase.OnRecordingsChanged += new TVDatabase.OnRecordingChangedHandler(OnRecordingsChanged);

      _tvChannelsList = new List<TVChannel>();
      TVDatabase.GetChannels(ref _tvChannelsList);

      _preRecordInterval = 0;
      _postRecordInterval = 0;
      //m_bAlwaysTimeshift=false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        _preRecordInterval = xmlreader.GetValueAsInt("capture", "prerecord", 5);
        _postRecordInterval = xmlreader.GetValueAsInt("capture", "postrecord", 5);
        _preRecordingWarningTime = xmlreader.GetValueAsInt("mytv", "recordwarningtime", 2);
      }
      ResetTimer();

    }
    private void OnRecordingsChanged(TVDatabase.RecordingChange change)
    {
      Log.Write("Scheduler:Recordings changed");
      _recordingsListChanged = true;
    }
    public void ResetTimer()
    {
      _scheduleTimer = DateTime.MinValue;
    }
    public void UpdateTimer()
    {
      _scheduleTimer = DateTime.Now;
    }
    void ReloadRecordingList(CommandProcessor handler)
    {
      // then get (refresh) all recordings from the database
      List<TVRecording> oldRecs = new List<TVRecording>();
      oldRecs = _recordingsList;
      _recordingsList = new List<TVRecording>();
      _tvChannelsList.Clear();
      TVDatabase.GetRecordings(ref _recordingsList);
      TVDatabase.GetChannels(ref _tvChannelsList);

      //remember if we already send a notification for this recording
      foreach (TVRecording recording in _recordingsList)
      {
        foreach (TVRecording oldrec in oldRecs)
        {
          if (oldrec.ID == recording.ID)
          {
            recording.IsAnnouncementSend = oldrec.IsAnnouncementSend;
            break;
          }
        }

        //if any card is busy recording a changed recording, then let the card know
        //about the changes
        for (int i = 0; i < handler.TVCards.Count; ++i)
        {
          TVCaptureDevice dev = handler.TVCards[i];
          if (dev.IsRecording)
          {
            if (dev.CurrentTVRecording.ID == recording.ID)
            {
              dev.CurrentTVRecording = recording;
            }//if (dev.CurrentTVRecording.ID==recording.ID)
          }//if (dev.IsRecording)
        }//for (int i=0; i < handler.TVCards.Count;++i)
      }//foreach (TVRecording recording in _recordingsList)
      oldRecs = null;
    }

    public bool TimeToProcessRecordings
    {
      get
      {
        TimeSpan ts = DateTime.Now - _scheduleTimer;
        if (ts.TotalSeconds < 60) return false;
        return true;
      }
    }
    public void Process(CommandProcessor handler)
    {
      if (!TimeToProcessRecordings) return;
      UpdateTimer();

      DateTime dtCurrentTime = DateTime.Now;
      // If the scheduled recordings have been changed,deleted or added since last time
      // then we need to re-load the recordings from the database
      if (_recordingsListChanged)
      {
        Log.Write("Scheduler:reload recordings");
        ReloadRecordingList(handler);
        _recordingsListChanged = false;
      }//if (_recordingsListChanged)

      //for each tv channel
      int card;
      for (int i = 0; i < _tvChannelsList.Count; ++i)
      {
        TVChannel chan = _tvChannelsList[i];

        // get all programs running for this TV channel
        // between  (now-4 hours) - (now+iPostRecordInterval+3 hours)
        DateTime dtStart = dtCurrentTime.AddHours(-4);
        DateTime dtEnd = dtCurrentTime.AddMinutes(_postRecordInterval + 3 * 60);
        long iStartTime = Utils.datetolong(dtStart);
        long iEndTime = Utils.datetolong(dtEnd);

        // for each TV recording scheduled
        for (int j = 0; j < _recordingsList.Count; ++j)
        {
          TVRecording rec = _recordingsList[j];
          //if recording is not canceled
          if (rec.Canceled > 0) continue;

          //and recording has not finished yet (already recorded)
          if (rec.IsDone()) continue;

          //is this the correct channel for the recording
          if (rec.RecType == TVRecording.RecordingType.EveryTimeOnEveryChannel || chan.Name == rec.Channel)
          {
            //Are we already recording this recording?
            if (!handler.IsRecordingSchedule(rec, out card))
            {
              //no, then check if its time to record it
              int paddingFront = rec.PaddingFront;
              int paddingEnd = rec.PaddingEnd;
              if (paddingFront < 0) paddingFront = _preRecordInterval;
              if (paddingEnd < 0) paddingEnd = _postRecordInterval;

              // check which program is current running on this channel
              TVProgram prog = chan.GetProgramAt(dtCurrentTime.AddMinutes(1 + paddingFront));

              // if the recording should record the tv program
              if (rec.IsRecordingProgramAtTime(dtCurrentTime, prog, paddingFront, paddingEnd))
              {
                // yes, then record it
                if (Record(handler,dtCurrentTime, rec, prog, paddingFront, paddingEnd))
                {
                  break;
                }
              }
              else
              {
                //not necessary to record, but maybe its time to send a notification
                //first check if we didnt already send it
                if (!rec.IsAnnouncementSend)
                {
                  //no, then check if a notification needs to be send
                  DateTime dtTime = DateTime.Now.AddMinutes(_preRecordingWarningTime);
                  TVProgram prog2Min = chan.GetProgramAt(dtTime.AddMinutes(1 + paddingFront));

                  // if the recording should record the tv program
                  if (rec.IsRecordingProgramAtTime(dtTime, prog2Min, paddingFront, paddingEnd))
                  {
                    //then send the announcement that we are about to record this recording in 2 minutes from now
                    Log.WriteFile(Log.LogType.Recorder, "Recorder: Send announcement for recording:{0}", rec.ToString());
                    rec.IsAnnouncementSend = true;
                    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING, 0, 0, 0, 0, 0, null);
                    msg.Object = rec;
                    msg.Object2 = prog2Min;
                    GUIGraphicsContext.SendMessage(msg);
                  }
                }
              }
            }//if (!IsRecordingSchedule(rec, out card)) 
          }//if (rec.RecType==TVRecording.RecordingType.EveryTimeOnEveryChannel || chan.Name==rec.Channel)
        } //for (int j=0; j < _recordingsList.Count;++j)
      }//for (int i=0; i < _tvChannelsList.Count;++i)


      //check if any manually added recording should be recorded
      for (int j = 0; j < _recordingsList.Count; ++j)
      {
        TVRecording rec = _recordingsList[j];
        //if recording has been canceled then skip it
        if (rec.Canceled > 0) continue;

        //if recording has been recorded already, then skip it
        if (rec.IsDone()) continue;

        int paddingFront = rec.PaddingFront;
        int paddingEnd = rec.PaddingEnd;
        if (paddingFront < 0) paddingFront = _preRecordInterval;
        if (paddingEnd < 0) paddingEnd = _postRecordInterval;

        // 1st check if the recording itself should b recorded
        if (rec.IsRecordingProgramAtTime(DateTime.Now, null, paddingFront, paddingEnd))
        {
          //yes, time to record it. Are we already recording it?
          if (!handler.IsRecordingSchedule(rec, out card))
          {
            // no, then start recording it now
            if (Record(handler, dtCurrentTime, rec, null, paddingFront, paddingEnd))
            {
              // recording it
            }
          }
        }
        else
        {
          //no time yet to record this recording, 
          //if we are going to record it within 2 minutes from now, then send an annoucement

          //is announcement already sent?
          if (!rec.IsAnnouncementSend)
          {
            //no, is the recording going to start within 2 mins?
            DateTime dtTime = DateTime.Now.AddMinutes(_preRecordingWarningTime);
            // if the recording should record the tv program
            if (rec.IsRecordingProgramAtTime(dtTime, null, paddingFront, paddingEnd))
            {
              //yes, then send the announcement
              rec.IsAnnouncementSend = true;
              Log.WriteFile(Log.LogType.Recorder, "Recorder: Send announcement for recording:{0}", rec.ToString());
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING, 0, 0, 0, 0, 0, null);
              msg.Object = rec;
              msg.Object2 = null;
              GUIGraphicsContext.SendMessage(msg);
            }
          }
        }
      }//for (int j=0; j < _recordingsList.Count;++j)
    }

    /// <summary>
    /// Find a capture card we can use to start a new recording
    /// </summary>
    /// <param name="recordingChannel">Channel we need to record</param>
    /// <param name="terminatePostRecording">
    /// false: use algoritm 1 ( see below)
    /// true: use algoritm 2 ( see below)
    /// </param>
    /// <returns>
    /// -1 : no card found
    /// else card which can be usfed for recording</returns>
    /// <remarks>
    /// MP will first use the following algorithm to find a card to use for the recording:
    ///		- card must be able to record the selected channel
    ///		- card must be free (or viewing the channel we need to record)
    ///		- of all cards found it will use the one with the highest priority
    ///		
    ///	if no card is found then MP will try to use the following algorithm:
    ///		- card must be able to record the selected channel
    ///		- card must be free  (or viewing the channel we need to record) or postrecording on any channel !!!
    ///		- of all cards found it will use the one with the highest priority
    ///	
    ///	Note. If the determined card is in use and the user is currently watching different channel on it
    ///	then the one we need to record then MP will look if there are other cards available with maybe have a
    ///	lower priority. reason for this is that we want to prevent the situation where the user
    ///	is watching channel A, and then when the recording starts on channel B the user suddenly 
    ///	sees channel B
    /// </remarks>
    private int FindFreeCardForRecording(CommandProcessor handler,string recordingChannel, bool stopRecordingsWithLowerPriority, int recordingPrio)
    {
      // if we are viewing a tv channel, and we want to record the program on this channel
      // then just use the same card 
      if (handler.CurrentCardIndex >= 0 && handler.CurrentCardIndex < handler.TVCards.Count)
      {
        TVCaptureDevice dev = handler.TVCards[handler.CurrentCardIndex];
        //is card viewing?
        if (dev.View && !dev.IsRecording)
        {
          // is it viewing the channel we want to record?
          if (dev.TVChannel == recordingChannel)
          {
            //then just use the current selected card
            if (dev.UseForRecording) return handler.CurrentCardIndex;
          }
        }
      }

      //no, then find another card which is free
      int cardNo = 0;
      int highestPrio = -1;
      int highestCard = -1;
      for (int loop = 0; loop <= 1; loop++)
      {
        highestPrio = -1;
        highestCard = -1;
        cardNo = 0;
        for (int counter=0; counter < handler.TVCards.Count;counter++)
        {
          TVCaptureDevice dev =handler.TVCards[counter];
          //if we may use the  card for recording tv?
          if (dev.UseForRecording)
          {
            // and is it not recording already?
            // or recording a show which has lower priority then the one we need to record?
            if (!dev.IsRecording || (dev.IsRecording && stopRecordingsWithLowerPriority && dev.CurrentTVRecording.Priority < recordingPrio))
            {
              //and can it receive the channel we want to record?
              if (TVDatabase.CanCardViewTVChannel(recordingChannel, dev.ID) || handler.TVCards.Count == 1)
              {
                // does this card has the highest priority?
                if (dev.Priority > highestPrio)
                {
                  //yes then we use this card
                  //but do we want to use it?
                  //if the user is using this card to watch tv on another channel
                  //then we prefer to use another tuner for the recording
                  bool preferCard = false;
                  if (handler.CurrentCardIndex == cardNo)
                  {
                    //user is watching tv on this tuner
                    if (loop >= 1)
                    {
                      //first loop didnt find any other free card,
                      //so no other choice then to use this one.
                      preferCard = true;
                    }
                    else
                    {
                      //is user watching same channel as we wanna record?
                      if (dev.IsTimeShifting && dev.TVChannel == recordingChannel)
                      {
                        //yes, then he wont notice anything, so we can use the card
                        preferCard = true;
                      }
                    }
                  }
                  else
                  {
                    //user is not using this tuner, so we can use this card
                    preferCard = true;
                  }

                  if (preferCard)
                  {
                    highestPrio = dev.Priority;
                    highestCard = cardNo;
                  }
                }//if (dev.Priority>highestPrio)

                //if this card has the same priority and is already watching this channel
                //then we use this card
                if (dev.Priority == highestPrio)
                {
                  if ((dev.IsTimeShifting || dev.View == true) && dev.TVChannel == recordingChannel)
                  {
                    highestPrio = dev.Priority;
                    highestCard = cardNo;
                  }
                }
              }//if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) || handler.TVCards.Count==1 )
            }//if (!dev.IsRecording)
          }//if (dev.UseForRecording)
          cardNo++;
        }//foreach (TVCaptureDevice dev in handler.TVCards)
        if (highestCard >= 0)
        {
          return highestCard;
        }
      }//for (int loop=0; loop <= 1; loop++)
      return -1;
    }

    /// <summary>
    /// Start recording a new program
    /// </summary>
    /// <param name="currentTime"></param>
    /// <param name="rec">TVRecording to record <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
    /// <param name="currentProgram">TVprogram to record <seealso cref="MediaPortal.TV.Database.TVProgram"/> (can be null)</param>
    /// <param name="iPreRecordInterval">Pre record interval in minutes</param>
    /// <param name="iPostRecordInterval">Post record interval in minutes</param>
    /// <returns>true if recording has been started</returns>
    bool Record(CommandProcessor handler,  DateTime currentTime, TVRecording rec, TVProgram currentProgram, int iPreRecordInterval, int iPostRecordInterval)
    {
      if (rec == null) return false;
      if (iPreRecordInterval < 0) iPreRecordInterval = 0;
      if (iPostRecordInterval < 0) iPostRecordInterval = 0;

      // Check if we're already recording this...
      for (int i=0; i < handler.TVCards.Count;++i)
      {
        TVCaptureDevice dev = handler.TVCards[i];
        if (dev.IsRecording)
        {
          if (dev.CurrentTVRecording.ID == rec.ID)
          {
            //we are alreay recording this schedule, so we can return
            return false;
          }
        }
      }

      // not recording this yet
      Log.WriteFile(Log.LogType.Recorder, "Recorder: time to record '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6} {7}", rec.Title, rec.Channel, rec.StartTime.ToLongTimeString(), rec.EndTime.ToLongTimeString(), rec.ID, rec.Priority, rec.Quality.ToString(), rec.RecType.ToString());
      if (currentProgram != null)
      {
        Log.WriteFile(Log.LogType.Recorder, "Recorder: program:{0}-{1}", currentProgram.StartTime.ToLongTimeString(), currentProgram.EndTime.ToLongTimeString());
      }

      // find free card we can use for recording
      int cardNo = FindFreeCardForRecording(handler,rec.Channel, false, rec.Priority);
      if (cardNo < 0)
      {
        // no card found. 
        //check if this recording has a higher priority then any recordings currently busy
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  No card found, check if a card is recording a show which has a lower priority then priority:{0}", rec.Priority);
        cardNo = FindFreeCardForRecording(handler,rec.Channel, true, rec.Priority);
        if (cardNo < 0)
        {
          //no, other recordings have higher priority...
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  no recordings have a lower priority then priority:{0}", rec.Priority);
          return false;
        }
      }

      //did we find a free tv card for this recording?
      if (cardNo < 0)
      {
        //no then show a recording conflict and let the user choose
        //which recording to stop
        GUIDialogMenuBottomRight pDlgOK = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT) as GUIDialogMenuBottomRight;
        pDlgOK.Reset();
        pDlgOK.SetHeading(879);//Recording Conflict
        pDlgOK.SetHeadingRow2(GUILocalizeStrings.Get(880) + " " + rec.Channel);
        pDlgOK.SetHeadingRow3(881);
        int cardWithLowestPriority = -1;
        int lowestPriority = TVRecording.HighestPriority;
        int count = 0;
        for (int i = 0; i < handler.TVCards.Count; i++)
        {
          TVCaptureDevice dev = handler.TVCards[i];
          if (!dev.IsRecording) continue;
          if (dev.CurrentTVRecording.Channel == rec.Channel)
          {
            if (dev.IsPostRecording) return false;
          }

          GUIListItem item = new GUIListItem();
          item.Label = dev.CurrentTVRecording.Title;
          string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, dev.CurrentTVRecording.Channel);
          if (System.IO.File.Exists(strLogo))
          {
            item.IconImage = strLogo;
          }
          pDlgOK.Add(item);
          int prio = dev.CurrentTVRecording.Priority;
          if (prio < lowestPriority)
          {
            cardWithLowestPriority = i;
            lowestPriority = prio;
          }
          count++;
        }

        if (count > 0)
        {
          pDlgOK.TimeOut = 60;
          pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
          if (pDlgOK.TimedOut)
          {
            cardNo = cardWithLowestPriority;
            TVCaptureDevice dev = handler.TVCards[cardNo];
            Log.WriteFile(Log.LogType.Recorder, "Recorder: Canceled recording:{0} priority:{1} on card:{2}",
                           dev.CurrentTVRecording.ToString(),
                           dev.CurrentTVRecording.Priority,
                           dev.CommercialName);
            StopRecording(handler,dev.CurrentTVRecording);
          }
          else
          {
            int selectedIndex = pDlgOK.SelectedLabel;
            if (selectedIndex >= 0)
            {
              for (int i = 0; i < handler.TVCards.Count; ++i)
              {
                TVCaptureDevice dev = handler.TVCards[i];
                if (dev.IsRecording)
                {
                  if (count == selectedIndex)
                  {
                    cardNo = i;
                    Log.WriteFile(Log.LogType.Recorder, "Recorder: User canceled recording:{0} priority:{1} on card:{2}",
                      dev.CurrentTVRecording.ToString(),
                      dev.CurrentTVRecording.Priority,
                      dev.CommercialName);
                    StopRecording(handler,dev.CurrentTVRecording);
                    break;
                  }
                  count++;
                }
              }
            }
          }
        }
        if (cardNo < 0)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder:  no card available for recording");
          return false;//no card free
        }
      }

      //now we have a free card
      TVCaptureDevice card = handler.TVCards[cardNo];
      Log.WriteFile(Log.LogType.Recorder, "Recorder:  using card:{0} prio:{1}", card.CommercialName, card.Priority);
      if (card.IsRecording)
      {
        //if its recording, we cancel it 
        Log.WriteFile(Log.LogType.Recorder, "Recorder:  card:{0} was recording. Now use it for recording new program", card.CommercialName);
        Log.WriteFile(Log.LogType.Recorder, "Recorder: Stop recording card:{0} channel:{1}", card.CommercialName, card.TVChannel);
        if (card.CurrentTVRecording.RecType == TVRecording.RecordingType.Once)
        {
          card.CurrentTVRecording.Canceled = Utils.datetolong(DateTime.Now);
        }
        else
        {
          long datetime = Utils.datetolong(DateTime.Now);
          TVProgram prog = card.CurrentProgramRecording;
          if (prog != null) datetime = Utils.datetolong(prog.StartTime);
          card.CurrentTVRecording.CanceledSeries.Add(datetime);
        }
        //and stop the recording
        TVDatabase.UpdateRecording(card.CurrentTVRecording, TVDatabase.RecordingChange.Canceled);
        card.StopRecording();
      }



      //finally start recording...
      handler.TuneExternalChannel(rec.Channel, false);
      card.Record(rec, currentProgram, iPostRecordInterval, iPostRecordInterval);

      //if the user was using this card to watch tv, then start watching it also
      if (handler.CurrentCardIndex == cardNo)
      {
        handler.TVChannelName = rec.Channel;
        TimeShiftTvCommand cmd = new TimeShiftTvCommand(rec.Channel);
        cmd.Execute(handler);
      }
      handler.LogTunerStatus();
      return true;
    }//bool Record(DateTime currentTime,TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)

    void StopRecording(CommandProcessor handler,TVRecording rec)
    {
      CancelRecordingCommand cmd = new CancelRecordingCommand(rec);
      cmd.Execute(handler);
    }
  }
}
