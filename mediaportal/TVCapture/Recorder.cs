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
//using DirectX.Capture;
namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// todo : 
	///  -viewing & timeshifting live tv, 
	///  -allocate/free card for my radio
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
    static bool          m_bPreviewing=false;       //todo
    static bool          m_bAlwaysTimeshift=false;  //todo
    static int           m_iPreRecordInterval =0;
    static int           m_iPostRecordInterval=0;
    static TVUtil        m_TVUtil=null;
    static string        m_strPreviewChannel="";
    static State         m_eState=State.None;
    static ArrayList     m_tvcards    = new ArrayList();
    static ArrayList     m_TVChannels = new ArrayList();
    static ArrayList     m_Recordings = new ArrayList();


    // singleton. Dont allow any instance of this class
    private Recorder()
    {
    }

    /// <summary>
    /// Start. Loads the capture cards, recordings, channels and sets everything ready 
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
        m_strPreviewChannel  = xmlreader.GetValueAsString("mytv","channel","");
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
    /// Stop the record thread
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

    static void HandleTimeShifting()
    {
      bool bShouldTimeshift=(m_bAlwaysTimeshift || m_bPreviewing);
      if (!bShouldTimeshift)
      {
        // stop timeshifting
        // check each card to see if it is timeshifting
        foreach (TVCaptureDevice dev in m_tvcards)
        {
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
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsTimeShifting)
          {
            dev.TVChannel=m_strPreviewChannel;
            // yep, then we're done
            return ;
          }
        }

        // no cards timeshifting? then check if we can start one
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          // can we use this card?
          if (dev.UseForTV && !dev.IsRecording)
          {
            // yep. then start timeshifting
            dev.TVChannel=m_strPreviewChannel;
            dev.StartTimeShifting();
            return;
          }
        }
      }
    }

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

      foreach (TVChannel chan in m_TVChannels)
      {
        // get all programs running for this TV channel
        // between  (now-4 hours) - (now+iPostRecordInterval+3 hours)
        DateTime dtStart=dtCurrentTime.AddHours(-4);
        DateTime dtEnd=dtCurrentTime.AddMinutes(m_iPostRecordInterval+3*60);
        long iStartTime=Utils.datetolong(dtStart);
        long iEndTime=Utils.datetolong(dtEnd);
            
        // for each TV recording scheduled
        foreach (TVRecording rec in m_Recordings)
        {
          // check which program is running 
          TVProgram prog=m_TVUtil.GetProgramAt(chan.Name,dtCurrentTime.AddMinutes(m_iPreRecordInterval) );

          // if the recording should record the tv program
          if ( rec.ShouldRecord(dtCurrentTime,prog,m_iPreRecordInterval, m_iPostRecordInterval) )
          {
            // yes, then record it
            if (Record(dtCurrentTime,rec,prog, m_iPreRecordInterval, m_iPostRecordInterval))
            {
              break;
            }
          }
        }
      }
   

      foreach (TVRecording rec in m_Recordings)
      {
        // 1st check if the recording itself should b recorded
        if ( rec.ShouldRecord(DateTime.Now,null,m_iPreRecordInterval, m_iPostRecordInterval) )
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
    /// When called this method starts an immediate recording on the channel specified
    /// It will record the next 2 hours.
    /// </summary>
    /// <param name="strChannel"></param>
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
      if ( rec.ShouldRecord(currentTime,currentProgram,0,0) )
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

    static public bool IsRecording
    {
      get
      {
        if (m_eState!= State.Initialized) return false;
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return true;
        }
        return false;
      }
    }

    static public bool IsRecordingSchedule(TVRecording rec)
    {
      if (m_eState!= State.Initialized) return false;
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.IsRecording && dev.CurrentTVRecording!=null&&dev.CurrentTVRecording.ID==rec.ID) return true;
      }
      return false;
    }
    
    static public TVProgram ProgramRecording
    {
      get
      {
        if (m_eState!= State.Initialized) return null;
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return dev.CurrentProgramRecording;
        }
        return null;
      }
    }

    static public TVRecording CurrentTVRecording
    {
      get
      {
        if (m_eState!= State.Initialized) return null;
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return dev.CurrentTVRecording;
        }
        return null;
      }
    }
    
    static void OnPreviewChannelChanged()
    {
      if (m_eState!= State.Initialized) return ;
      string strLogo=Utils.GetLogo(m_strPreviewChannel);
      if (!System.IO.File.Exists(strLogo))
      {
        strLogo="defaultVideoBig.png";
      }
      GUIPropertyManager.Properties["#TV.View.channel"]=m_strPreviewChannel;
      GUIPropertyManager.Properties["#TV.View.thumb"]  =strLogo;
    }

    static public bool Previewing
    {
      get
      {
        if (m_eState!=State.Initialized) return false;
        return m_bPreviewing;
      }
      set
      {
        if (m_eState!=State.Initialized) return ;
        if (m_bPreviewing!=value)
        {
          if (false==value) Log.Write("Recorder:previewing stop requested");
          else Log.Write("Recorder:previewing start requested");
          m_bPreviewing=value;
					Recorder.Process();
        }
      }
    }

    static public string PreviewChannel
    {
      get 
      {
        return m_strPreviewChannel;
      }
      set
      {
        m_strPreviewChannel=value;
      }
    }
    

    /// <summary>
    /// 
    /// </summary>
    static public void Process()
    {
      if (m_eState!=State.Initialized) return;
      Recorder.HandleTimeShifting();
      Recorder.HandleRecordings();
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        dev.Process();
      }
      Recorder.SetProperties();
    }

    static void SetProperties()
    {
      // for each tv-channel
      if (m_eState!=State.Initialized) return;
      foreach (TVChannel chan in m_TVChannels)
      {
        if (chan.Name.Equals(m_strPreviewChannel))
        {
          TVProgram prog=m_TVUtil.GetCurrentProgram(chan.Name);
          if (prog!=null)
          {
            if (!GUIPropertyManager.Properties["#TV.View.channel"].Equals(m_strPreviewChannel))
            {
              OnPreviewChannelChanged();
            }
            GUIPropertyManager.Properties["#TV.View.start"]=prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
            GUIPropertyManager.Properties["#TV.View.stop"] =prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
            GUIPropertyManager.Properties["#TV.View.genre"]=prog.Genre;
            GUIPropertyManager.Properties["#TV.View.title"]=prog.Title;
            GUIPropertyManager.Properties["#TV.View.description"]=prog.Description;
          }
          else
          {
            if (!GUIPropertyManager.Properties["#TV.View.channel"].Equals(m_strPreviewChannel))
            {
              OnPreviewChannelChanged();
            }
            GUIPropertyManager.Properties["#TV.View.start"]="";
            GUIPropertyManager.Properties["#TV.View.stop"] ="";
            GUIPropertyManager.Properties["#TV.View.genre"]="";
            GUIPropertyManager.Properties["#TV.View.title"]="";
            GUIPropertyManager.Properties["#TV.View.description"]="";
          }
        }
      }

      // handle properties...
      if (IsRecording)
      {
        TVRecording recording = CurrentTVRecording;
        TVProgram program     = ProgramRecording;
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
    }
    
    /// <summary>
    /// This function gets called by the TVDatabase when a recording has been
    /// added,changed or deleted. It forces the recorder to get the new
    /// recordings.
    /// </summary>
    static private void OnRecordingsChanged()
    { 
      if (m_eState!=State.Initialized) return;
      m_bRecordingsChanged=true;
    }
		
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
			}
		}
  }
}
