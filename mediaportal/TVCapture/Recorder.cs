using System;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Player;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// This class is a singleton which implements the
	/// -task scheduler to schedule, (start,stop) all tv recordings on time
	/// -a front end to other classes to control the tv capture cards
	/// </summary>
  public class Recorder
  {
    enum State
    {
      None,
      Initializing,
      Initialized,
      Deinitializing
    }
    static bool          m_bRecordingsChanged=false;  // flag indicating that recordings have been added/changed/removed
    static bool          m_bTimeshifting =false;       //todo
    static bool          m_bAlwaysTimeshift=false;  //todo
    static bool          m_bViewing=false;  //todo
    static int           m_iPreRecordInterval =0;
    static int           m_iPostRecordInterval=0;
    static TVUtil        m_TVUtil=null;
    static string        m_strTVChannel="";
    static State         m_eState=State.None;
    static ArrayList     m_tvcards    = new ArrayList();
    static ArrayList     m_TVChannels = new ArrayList();
    static ArrayList     m_Recordings = new ArrayList();
    static TVProgram     m_PrevProg=null;
    static bool          m_bWasRecording=false;
    static int           m_RecPrevRecordingId=-1;

    /// <summary>
    /// singleton. Dont allow any instance of this class so make the constructor private
    /// </summary>
    private Recorder()
    {
    }

    /// <summary>
    /// This method will Start the scheduler. It
    /// Loads the capture cards from capturecards.xml (made by the setup)
    /// Loads the recordings (programs scheduled to record) from the tvdatabase
    /// Loads the TVchannels from the tvdatabase
    /// </summary>
    static public void Start()
    {
      if (m_eState!=State.None) return;
      m_eState=State.Initializing;
      CleanProperties();
      m_bRecordingsChanged=false;
    

      m_tvcards.Clear();
      try
      {
        using (Stream r = File.Open(@"capturecards.xml", FileMode.Open, FileAccess.Read))
        {
          SoapFormatter c = new SoapFormatter();
          m_tvcards = (ArrayList)c.Deserialize(r);
          r.Close();
        } 
      }
      catch(Exception)
      {
      }
      if (m_tvcards.Count==0) 
      {
        Log.Write("Recorder: no capture cards found. Use file->setup to setup tvcapture!");
      }
      for (int i=0; i < m_tvcards.Count;i++)
      {
        TVCaptureDevice card=(TVCaptureDevice)m_tvcards[i];
        card.ID=(i+1);
        Log.Write(" card:{0} video device:{1} TV:{2}  record:{3}",
                    card.ID,card.VideoDevice,card.UseForTV,card.UseForRecording);
      }

      m_iPreRecordInterval =0;
      m_iPostRecordInterval=0;
      m_bAlwaysTimeshift=false;
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        m_iPreRecordInterval = xmlreader.GetValueAsInt("capture","prerecord", 5);
        m_iPostRecordInterval= xmlreader.GetValueAsInt("capture","postrecord", 5);
        m_bAlwaysTimeshift   = xmlreader.GetValueAsBool("mytv","alwaystimeshift",false);
        m_strTVChannel  = xmlreader.GetValueAsString("mytv","channel","");
      }

      m_TVChannels.Clear();
      TVDatabase.GetChannels(ref m_TVChannels);

      
      m_Recordings.Clear();
      TVDatabase.GetRecordings(ref m_Recordings);

      m_TVUtil= new TVUtil();

      TVDatabase.OnRecordingsChanged += new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
      //TODO
      GUIWindowManager.Receivers += new SendMessageHandler(Recorder.OnMessage);
      m_eState=State.Initialized;
    }

    /// <summary>
    /// Stops the scheduler. It will cleanup all resources allocated and free
    /// the capture cards
    /// </summary>
    static public void Stop()
    {
      if (m_eState != State.Initialized) return;
      m_eState=State.Deinitializing;
      TVDatabase.OnRecordingsChanged -= new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
      //TODO
      GUIWindowManager.Receivers -= new SendMessageHandler(Recorder.OnMessage);

      foreach (TVCaptureDevice dev in m_tvcards)
      {
        dev.Stop();
      }
      CleanProperties();
      m_bRecordingsChanged=false;
      m_eState=State.None;
    }

    /// <summary>
    /// Checks for each tv capture card whether it should start or stop timeshifting
    /// and if needed stops/or starts the timeshifting.
    /// Timeshifting should be started when
    ///   - no card is timeshifting and Timeshifting=true
    ///   - no card is timeshifting and Allways Timeshifting is turned on in the setup
    /// </summary>
    /// <remarks>
    /// This function gets called on a regular basis by the scheduler and 
    /// it makes sure that just 1 capture card is timeshifting. Since we
    /// only need timeshifting for watching live tv. 
    /// </remarks>
    static void HandleTimeShifting()
    {
      bool bShouldTimeshift=(m_bAlwaysTimeshift || m_bTimeshifting );
      if (!bShouldTimeshift)
      {
        // stop timeshifting
        // check each card to see if it is timeshifting
        for (int i=0; i < m_tvcards.Count;++i)
        {
          TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
          if (dev.IsTimeShifting)
          {
            // yes it is, but make sure its not recording
            if (!dev.IsRecording)
            {
              // not recording. then just stop timeshifting
              dev.StopTimeShifting();
            }
          }
        }
        return;
      }
      else
      {
        // start timeshifting
        // check if a card is already timeshifting
        for (int i=0; i < m_tvcards.Count;++i)
        {
          TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
          if (dev.IsTimeShifting)
          {
            dev.TVChannel=m_strTVChannel;
            // yep, then we're done
            return ;
          }
        }

        // no cards timeshifting? then check if we can start one
        for (int i=0; i < m_tvcards.Count;++i)
        {
          TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
          // can we use this card?
          if (dev.UseForTV && !dev.IsRecording && dev.SupportsTimeShifting)
          {
            // yep. then start timeshifting
            dev.TVChannel=m_strTVChannel;
            dev.StartTimeShifting();
            return;
          }
        }
      }
    }

    /// <summary>
    /// Checks if a recording should be started and ifso starts the recording
    /// This function gets called on a regular basis by the scheduler. It will
    /// look if any of the recordings needs to be started. Ifso it will
    /// find a free tvcapture card and start the recording
    /// </summary>
    static void HandleRecordings()
    { 
      if (m_eState!= State.Initialized) return;
      DateTime dtCurrentTime=DateTime.Now;

      // If the recording schedules have been changed since last time
      if (m_bRecordingsChanged)
      {
        // then get (refresh) all recordings from the database
        m_Recordings.Clear();
        m_TVChannels.Clear();
        TVDatabase.GetRecordings(ref m_Recordings);
        TVDatabase.GetChannels(ref m_TVChannels);
        m_bRecordingsChanged=false;
      }

      // no TV cards? then we cannot record anything, so just return
      if (m_tvcards.Count==0)  return;

      for (int i=0; i < m_TVChannels.Count;++i)
      {
        TVChannel chan =(TVChannel)m_TVChannels[i];
        // get all programs running for this TV channel
        // between  (now-4 hours) - (now+iPostRecordInterval+3 hours)
        DateTime dtStart=dtCurrentTime.AddHours(-4);
        DateTime dtEnd=dtCurrentTime.AddMinutes(m_iPostRecordInterval+3*60);
        long iStartTime=Utils.datetolong(dtStart);
        long iEndTime=Utils.datetolong(dtEnd);
            
        // for each TV recording scheduled
        for (int j=0; j < m_Recordings.Count;++j)
        {
          TVRecording rec =(TVRecording)m_Recordings[j];
          // check which program is running 
          TVProgram prog=m_TVUtil.GetProgramAt(chan.Name,dtCurrentTime.AddMinutes(m_iPreRecordInterval) );

          // if the recording should record the tv program
          if ( rec.IsRecordingProgramAtTime(dtCurrentTime,prog,m_iPreRecordInterval, m_iPostRecordInterval) )
          {
            // yes, then record it
            if (Record(dtCurrentTime,rec,prog, m_iPreRecordInterval, m_iPostRecordInterval))
            {
              break;
            }
          }
        }
      }
   

      for (int j=0; j < m_Recordings.Count;++j)
      {
        TVRecording rec =(TVRecording)m_Recordings[j];
        // 1st check if the recording itself should b recorded
        if ( rec.IsRecordingProgramAtTime(DateTime.Now,null,m_iPreRecordInterval, m_iPostRecordInterval) )
        {
          // yes, then record it
          if ( Record(dtCurrentTime,rec,null,m_iPreRecordInterval, m_iPostRecordInterval))
          {
            // recording it
          }
        }
      }
    }


    /// <summary>
    /// Starts recording the specified tv channel immediately using a reference recording
    /// When called this method starts an erference  recording on the channel specified
    /// It will record the next 2 hours.
    /// </summary>
    /// <param name="strChannel">TVchannel to record</param>
    static public void RecordNow(string strChannel)
    {
      if (m_eState!= State.Initialized) return;
      Log.Write("Recorder: record now:"+strChannel);
      // create a new recording which records the next 2 hours...
      TVRecording tmpRec = new TVRecording();
      tmpRec.Start=Utils.datetolong(DateTime.Now);
      tmpRec.End=Utils.datetolong(DateTime.Now.AddMinutes(2*60) );
      tmpRec.Channel=strChannel;
      tmpRec.Title="Manual";
      tmpRec.RecType=TVRecording.RecordingType.Once;
			tmpRec.IsContentRecording=false;//make a reference recording!
      TVDatabase.AddRecording(ref tmpRec);
    }

    /// <summary>
    /// Finds a free capture card and if found tell it to record the specified program
    /// </summary>
    /// <param name="currentTime"></param>
    /// <param name="rec">TVRecording to record <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
    /// <param name="currentProgram">TVprogram to record <seealso cref="MediaPortal.TV.Database.TVProgram"/> (can be null)</param>
    /// <param name="iPreRecordInterval">Pre record interval in minutes</param>
    /// <param name="iPostRecordInterval">Post record interval in minutes</param>
    /// <returns>true if recording has been started</returns>
    static bool Record(DateTime currentTime,TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)
    {
      if (m_eState!= State.Initialized) return false;
      // check if we're already recording this...
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.IsRecording)
        {
          if (dev.CurrentTVRecording.ID==rec.ID) return false;
        }
      }

      // not recording this yet
      Log.Write("Recorder: time to record a program on channel:"+rec.Channel);
      Log.Write("Recorder: find free capture card");

      // find free device for recording
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.UseForRecording)
        {
          if (!dev.IsRecording)
          {
            Log.Write("Recorder: found capture card:{0} {1}", dev.ID, dev.VideoDevice);
            dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
            return true;
          }
        }
      }

      // still no device found. 
      // if we skip the pre-record interval should the new recording still be started then?
      if ( rec.IsRecordingProgramAtTime(currentTime,currentProgram,0,0) )
      {
        // yes, then find & stop any capture running which is busy post-recording
        Log.Write("check if a capture card is post-recording");
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.UseForRecording)
          {
            if (dev.IsPostRecording)
            {
              Log.Write("Recorder: capture card:{0} {1} was post-recording. Now use it for recording new program", dev.ID,dev.VideoDevice);
              dev.StopRecording();
              dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
              return true;
            }
          }
        }
      }

      //no device free...
      Log.Write("no capture cards are available right now for recording");
      return false;
    }

    /// <summary>
    /// Stops all current recordings. 
    /// </summary>
    static public void StopRecording()
    {
      if (m_eState!= State.Initialized) return;
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.IsRecording) 
        {
          Log.Write("Recorder: Stop recording on channel:{0} capture card:{1}", dev.TVChannel,dev.ID);
          int ID=dev.CurrentTVRecording.ID;
					for (int i=0; i < m_Recordings.Count;++i)
					{
						TVRecording rec =(TVRecording )m_Recordings[i];
						if (rec.ID==ID)
						{
							
							rec.Canceled=Utils.datetolong(DateTime.Now);
							TVDatabase.ChangeRecording(ref rec);
							break;
						}
					}
					dev.StopRecording();
					
        }
      }
    }

    /// <summary>
    /// Property which returns whether one or more capture cards are currently recording
    /// </summary>
    static public bool IsRecording
    {
      get
      {
        if (m_eState!= State.Initialized) return false;
        for (int i=0; i < m_tvcards.Count;++i)
        {
          TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
          if (dev.IsRecording) return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Checks if a tvcapture card is recording the TVRecording specified
    /// </summary>
    /// <param name="rec">TVRecording <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
    /// <returns>true if a card is recording the specified TVRecording, else false</returns>
    static public bool IsRecordingSchedule(TVRecording rec)
    {
      if (m_eState!= State.Initialized) return false;
      for (int i=0; i < m_tvcards.Count;++i)
      {
        TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
        if (dev.IsRecording && dev.CurrentTVRecording!=null&&dev.CurrentTVRecording.ID==rec.ID) return true;
      }
      return false;
    }
    
    /// <summary>
    /// Property which returns the current program being recorded. If no programs are being recorded at the moment
    /// it will return null;
    /// </summary>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    static public TVProgram ProgramRecording
    {
      get
      {
        if (m_eState!= State.Initialized) return null;
        for (int i=0; i < m_tvcards.Count;++i)
        {
          TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
          if (dev.IsRecording) return dev.CurrentProgramRecording;
        }
        return null;
      }
    }
    /// <summary>
    /// Property which returns the current TVRecording being recorded. 
    /// If no recordings are being recorded at the moment
    /// it will return null;
    /// </summary>
    /// <seealso cref="MediaPortal.TV.Database.TVRecording"/>
    static public TVRecording CurrentTVRecording
    {
      get
      {
        if (m_eState!= State.Initialized) return null;
        for (int i=0; i < m_tvcards.Count;++i)
        {
          TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
          if (dev.IsRecording) return dev.CurrentTVRecording;
        }
        return null;
      }
    }
    
    /// <summary>
    /// Sets the current tv channel tags. This function gets called when the current
    /// tv channel gets changed. It will update the corresponding skin tags 
    /// </summary>
    /// <remarks>
    /// Sets the current tags:
    /// #TV.View.channel,  #TV.View.thumb
    /// </remarks>
    static void OnTVChannelChanged()
    {
      if (m_eState!= State.Initialized) return ;
      string strLogo=Utils.GetLogo(m_strTVChannel);
      if (!System.IO.File.Exists(strLogo))
      {
        strLogo="defaultVideoBig.png";
      }
      GUIPropertyManager.Properties["#TV.View.channel"]=m_strTVChannel;
      GUIPropertyManager.Properties["#TV.View.thumb"]  =strLogo;
    }

    /// <summary>
    /// Property which get/set Timeshifting mode.
    /// if Timeshifting mode is turned on then timeshifting will be started
    /// if Timeshifting mode is turned off then timeshifting will be stopped
    /// </summary>
    static public bool Timeshifting
    {
      get
      {
        if (m_eState!=State.Initialized) return false;
        return m_bTimeshifting ;
      }
      set
      {
        if (m_eState!=State.Initialized) return ;
        if (m_bTimeshifting !=value)
        {
          if (false==value) 
          {
            Log.Write("Recorder:Timeshifting stop requested");
            m_bTimeshifting =value;
            Recorder.Process();
            return;
          }
          else 
          {
            //check if timeshifting is supported
            for (int i=0; i < m_tvcards.Count;++i)
            {
              TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
              if (dev.SupportsTimeShifting) 
              {
                // yes, then turn timeshifting on
                Log.Write("Recorder:Timeshifting start requested");
                m_bTimeshifting =value;
                Recorder.Process();
                return;
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Property which get/set TV Viewing mode.
    /// if TV Viewing  mode is turned on then live tv will be shown
    /// if TV Viewing  mode is turned off thenlive tv will be hidden
    /// </summary>
    static public bool View
    {
      get 
      {
        if (!m_bViewing) return false;
        if (Timeshifting)
        {
          if (g_Player.Playing && g_Player.IsTV) return true;
          m_bViewing=false;
        }
        return m_bViewing;
      }
      set
      {
        if (View==value) return;//nothing changed

        if (value==false)
        {
          //turn off tv viewing
          m_bViewing=false;

          // stop any playing..
          if (g_Player.Playing && g_Player.IsTV) 
          {
            g_Player.Stop();
          }

          // stop any card viewing..
          for (int i=0; i < m_tvcards.Count;++i)
          {
            TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
            if (dev.View) 
            {
              dev.View=false;
            }
          }
          m_bViewing=false;
        }
        else
        {
          //turn on TV
          if (Timeshifting)
          {
            //if we're timeshifting, then just play live.tv
            string strRecPath;
            using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
            {
              strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
              strRecPath=Utils.RemoveTrailingSlash(strRecPath);
            }
            string strFileName=String.Format(@"{0}\live.tv",strRecPath);
            g_Player.Play(strFileName);
            m_bViewing=true;
            return;
          }

          //not timeshifting, then ask card to start viewing
          for (int i=0; i < m_tvcards.Count;++i)
          {
            TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
            if (!dev.IsRecording) 
            {
              if (g_Player.Playing && g_Player.IsTV)
              {
                g_Player.Stop();
              }
              dev.TVChannel = m_strTVChannel;
              dev.View=true;
              m_bViewing=true;
              return;
            }
          }
        }
      }
    }

    /// <summary>
    /// Property to get/set the current TV channel. If the current tv channel gets changed
    /// then the scheduler will tune to the new tv channel
    /// </summary>
    static public string TVChannelName
    {
      get 
      {
        return m_strTVChannel;
      }
      set
      {
        m_strTVChannel=value;
      }
    }
    

    /// <summary>
    /// Scheduler main loop. This function needs to get called on a regular basis.
    /// It will handle all scheduler tasks
    /// </summary>
    static public void Process()
    {
      if (m_eState!=State.Initialized) return;
      Recorder.HandleTimeShifting();
      Recorder.HandleRecordings();
      for (int i=0; i < m_tvcards.Count;++i)
      {
        TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
        dev.Process();
      }
      Recorder.SetProperties();
    }

    /// <summary>
    /// Updates the TV tags for the skin bases on the current tv channel, recording...
    /// </summary>
    /// <remarks>
    /// Tags updated are:
    /// #TV.View.channel, #TV.View.start,#TV.View.stop, #TV.View.genre, #TV.View.title, #TV.View.description
    /// #TV.Record.channel, #TV.Record.start,#TV.Record.stop, #TV.Record.genre, #TV.Record.title, #TV.Record.description, #TV.Record.thumb
    /// </remarks>
    static void SetProperties()
    {
      // for each tv-channel
      if (m_eState!=State.Initialized) return;

      for (int i=0; i < m_TVChannels.Count;++i)
      {
        TVChannel chan =(TVChannel)m_TVChannels[i];
        if (chan.Name.Equals(m_strTVChannel))
        {
          TVProgram prog=m_TVUtil.GetCurrentProgram(chan.Name);
          if (m_PrevProg!=prog)
          {
            m_PrevProg=prog;
            if (prog!=null)
            {
              if (!GUIPropertyManager.Properties["#TV.View.channel"].Equals(m_strTVChannel))
              {
                OnTVChannelChanged();
              }
              GUIPropertyManager.Properties["#TV.View.start"]=prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
              GUIPropertyManager.Properties["#TV.View.stop"] =prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
              GUIPropertyManager.Properties["#TV.View.genre"]=prog.Genre;
              GUIPropertyManager.Properties["#TV.View.title"]=prog.Title;
              GUIPropertyManager.Properties["#TV.View.description"]=prog.Description;
            }
            else
            {
              if (!GUIPropertyManager.Properties["#TV.View.channel"].Equals(m_strTVChannel))
              {
                OnTVChannelChanged();
              }
              GUIPropertyManager.Properties["#TV.View.start"]="";
              GUIPropertyManager.Properties["#TV.View.stop"] ="";
              GUIPropertyManager.Properties["#TV.View.genre"]="";
              GUIPropertyManager.Properties["#TV.View.title"]="";
              GUIPropertyManager.Properties["#TV.View.description"]="";
            }
          }//if (m_PrevProg!=prog)
          break;
        }//if (chan.Name.Equals(m_strTVChannel))
      }//for (int i=0; i < m_TVChannels.Count;++i)

      // handle properties...
      if (IsRecording != m_bWasRecording)
      {
        m_bWasRecording=IsRecording;
        if (IsRecording)
        {
          TVRecording recording = CurrentTVRecording;
          TVProgram   program   = ProgramRecording;
          if (m_RecPrevRecordingId != recording.ID)
          {
            m_RecPrevRecordingId=recording.ID;

            if (program==null)
            {
              if (!GUIPropertyManager.Properties["#TV.Record.channel"].Equals(recording.Channel))
              {
                string strLogo=Utils.GetLogo(recording.Channel);
                if (!System.IO.File.Exists(strLogo))
                {
                  strLogo="defaultVideoBig.png";
                }
                GUIPropertyManager.Properties["#TV.Record.thumb"]=strLogo;
              }
              GUIPropertyManager.Properties["#TV.Record.start"]=recording.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
              GUIPropertyManager.Properties["#TV.Record.stop"] =recording.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
              GUIPropertyManager.Properties["#TV.Record.genre"]="";
              GUIPropertyManager.Properties["#TV.Record.title"]=recording.Title;
              GUIPropertyManager.Properties["#TV.Record.description"]="";
            }
            else
            {
              if (!GUIPropertyManager.Properties["#TV.Record.channel"].Equals(program.Channel))
              {
                string strLogo=Utils.GetLogo(program.Channel);
                if (!System.IO.File.Exists(strLogo))
                {
                  strLogo="defaultVideoBig.png";
                }
                GUIPropertyManager.Properties["#TV.Record.thumb"]=strLogo;
              }
              GUIPropertyManager.Properties["#TV.Record.channel"]=program.Channel;
              GUIPropertyManager.Properties["#TV.Record.start"]=program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
              GUIPropertyManager.Properties["#TV.Record.stop"] =program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
              GUIPropertyManager.Properties["#TV.Record.genre"]=program.Genre;
              GUIPropertyManager.Properties["#TV.Record.title"]=program.Title;
              GUIPropertyManager.Properties["#TV.Record.description"]=program.Description;
            }
          }//if (m_RecPrevRecordingId != recording.ID)
        }
        else
        {
          GUIPropertyManager.Properties["#TV.Record.channel"]="";
          GUIPropertyManager.Properties["#TV.Record.start"]="";
          GUIPropertyManager.Properties["#TV.Record.stop"] ="";
          GUIPropertyManager.Properties["#TV.Record.genre"]="";
          GUIPropertyManager.Properties["#TV.Record.title"]="";
          GUIPropertyManager.Properties["#TV.Record.description"]="";
          GUIPropertyManager.Properties["#TV.Record.thumb"]  ="";
        }
      }//if (IsRecording != m_bWasRecording)
    }
    
    /// <summary>
    /// This function gets called by the TVDatabase when a recording has been
    /// added,changed or deleted. It forces the Scheduler to get update its list of
    /// recordings.
    /// </summary>
    static private void OnRecordingsChanged()
    { 
      if (m_eState!=State.Initialized) return;
      m_bRecordingsChanged=true;
    }
		
    /// <summary>
    /// Empties/clears all tv related skin tags. Gets called during startup en shutdown of
    /// the scheduler
    /// </summary>
    static void CleanProperties()
    {
      GUIPropertyManager.Properties["#TV.View.channel"]="";
      GUIPropertyManager.Properties["#TV.View.thumb"]  ="";
      GUIPropertyManager.Properties["#TV.View.start"]="";
      GUIPropertyManager.Properties["#TV.View.stop"] ="";
      GUIPropertyManager.Properties["#TV.View.genre"]="";
      GUIPropertyManager.Properties["#TV.View.title"]="";
      GUIPropertyManager.Properties["#TV.View.description"]="";

      GUIPropertyManager.Properties["#TV.Record.channel"]="";
      GUIPropertyManager.Properties["#TV.Record.start"]="";
      GUIPropertyManager.Properties["#TV.Record.stop"] ="";
      GUIPropertyManager.Properties["#TV.Record.genre"]="";
      GUIPropertyManager.Properties["#TV.Record.title"]="";
      GUIPropertyManager.Properties["#TV.Record.description"]="";
      GUIPropertyManager.Properties["#TV.Record.thumb"]  ="";
    }
		
    /// <summary>
    /// Handles incoming messages from other modules
    /// </summary>
    /// <param name="message">message received</param>
    /// <remarks>
    /// Supports the following messages:
    ///  GUI_MSG_RECORDER_ALLOC_CARD 
    ///  When received the scheduler will release/free all resources for the
    ///  card specified so other assemblies can use it
    ///  
    ///  GUI_MSG_RECORDER_FREE_CARD
    ///  When received the scheduler will alloc the resources for the
    ///  card specified. Its send when other assemblies dont need the card anymore
    ///  
    ///  GUI_MSG_RECORDER_STOP_TIMESHIFT
    ///  When received the scheduler will stop timeshifting.
    /// </remarks>
		static public void OnMessage(GUIMessage message)
		{
			switch(message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_RECORDER_ALLOC_CARD:
					// somebody wants to allocate a capture card
					// if possible, lets release it
					foreach (TVCaptureDevice card in m_tvcards)
					{
						if (card.VideoDevice.Equals(message.Label))
						{
							if (!card.IsRecording)
							{
								card.Stop();
								card.Allocated=true;
								return;
							}
						}
					}
					break;

					
				case GUIMessage.MessageType.GUI_MSG_RECORDER_FREE_CARD:
					// somebody wants to allocate a capture card
					// if possible, lets release it
					foreach (TVCaptureDevice card in m_tvcards)
					{
						if (card.VideoDevice.Equals(message.Label))
						{
							if (card.Allocated)
							{
								card.Allocated=false;
								return;
							}
						}
					}
					break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT:
          foreach (TVCaptureDevice card in m_tvcards)
          {
            if (!card.IsRecording)
            {
              card.Stop();
            }
          }
          break;
			}
		}
  }
}
