using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using DirectX.Capture;
namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// 
	/// </summary>
  public class Recorder
  {
    enum State
    {
      Idle,
      Running,
      Stopping
    };

    static Thread        workerThread =null;
    static State         m_eState=State.Idle;         // thread state
    static bool          m_bRecordingsChanged=false;  // flag indicating that recordings have been added/changed/removed
    static ArrayList     m_tvcards=new ArrayList();
    static string        m_strPreviewChannel;
    static bool          m_bPreviewing=false;
    static bool          m_bPreviewChanged=false;
    static bool          m_bStopRecording=false;

    public Recorder()
    {
    }

    /// <summary>
    /// Start record thread. The recorder thread will take care of all scheduled recordings
    /// </summary>
    static public void Start()
    {
      if (m_eState!=State.Idle) Stop();
      TVDatabase.OnRecordingsChanged += new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
      workerThread =new Thread( new ThreadStart(ThreadFunctionRecord)); 
      workerThread.Start();
    }

    /// <summary>
    /// Stop the record thread
    /// </summary>
    static public void Stop()
    {
      if (m_eState != State.Running) return;
      TVDatabase.OnRecordingsChanged -= new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
      m_eState =State.Stopping;
      while(m_eState ==State.Stopping) System.Threading.Thread.Sleep(100);
    }
    
    /// <summary>
    /// This function gets called by the TVDatabase when a recording has been
    /// added,changed or deleted. It forces the recorder to get the new
    /// recordings.
    /// </summary>
    static public void OnRecordingsChanged()
    {
      m_bRecordingsChanged=true;
    }
		
    /// <summary>
    /// The recorder worker thread
    /// </summary>
    static void ThreadFunctionRecord()
    {
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
        m_eState=State.Idle;
        return;
      }
      for (int i=0; i < m_tvcards.Count;i++)
      {
        TVCaptureDevice card=(TVCaptureDevice)m_tvcards[i];
        card.ID=(i+1);
      }

      DateTime dtTime=DateTime.Now;

      Log.Write("Recorder: thread started. Found:{0} capture cards", m_tvcards.Count);
      m_eState =State.Running;
      m_bRecordingsChanged=true;
      System.Threading.Thread.CurrentThread.Priority=System.Threading.ThreadPriority.Normal;
      

      // get all TV-channels
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      
      ArrayList recordings = new ArrayList();
      ArrayList runningPrograms = new ArrayList();

      int iPreRecordInterval =0;
      int iPostRecordInterval=0;
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        iPreRecordInterval =xmlreader.GetValueAsInt("capture","prerecord", 5);
        iPostRecordInterval=xmlreader.GetValueAsInt("capture","postrecord", 5);
      }

      while (m_eState ==State.Running)
      {
				
        // If the recording schedules have been changed since last time
        if (m_bRecordingsChanged)
        {
          // then get all recordings from the database
          recordings.Clear();
          channels.Clear();
          TVDatabase.GetRecordings(ref recordings);
          TVDatabase.GetChannels(ref channels);
          m_bRecordingsChanged=false;
        }

        HandlePreview();
        
        int iRec=0;

        // for each tv-channel
        foreach (TVChannel chan in channels)
        {
          if (m_eState !=State.Running) break;
          if (m_bPreviewChanged) break;
          if (m_bRecordingsChanged) break;
          if (m_bStopRecording) break;
          // get all programs running for this TV channel
          // between  (now-iPreRecordInterval) - (now+iPostRecordInterval+3 hours)

          DateTime dtStart=DateTime.Now.AddHours(-4);
          DateTime dtEnd=DateTime.Now.AddMinutes(iPostRecordInterval+3*60);
          long iStartTime=Utils.datetolong(dtStart);
          long iEndTime=Utils.datetolong(dtEnd);
          
          runningPrograms.Clear();
          TVDatabase.GetPrograms(chan.Name,iStartTime,iEndTime,ref runningPrograms);

          // for each TV recording scheduled
          iRec=0;
          foreach (TVRecording rec in recordings)
          {
            if (m_eState !=State.Running) break;
            if (m_bPreviewChanged) break;
            if (m_bRecordingsChanged) break;
            if (m_bStopRecording) break;
            bool bRecorded=false;
            
            // if not, then check each check for each tv program
            foreach (TVProgram currentProgram in runningPrograms)
            {
              if (m_eState !=State.Running) break;
              // if the recording should record the tv program
              if ( rec.ShouldRecord(DateTime.Now,currentProgram,iPreRecordInterval, iPostRecordInterval) )
              {
                // yes, then record it
                if (Record(rec,currentProgram, iPreRecordInterval, iPostRecordInterval))
                {
                  bRecorded=true;
                  recordings.RemoveAt(iRec);
                  break;
                }
              }
            }
            iRec++;
            if (bRecorded) break;
          }
        }
 

        iRec=0;
        if (!m_bRecordingsChanged)
        {
          foreach (TVRecording rec in recordings)
          {
            if (m_bPreviewChanged) break;
            if (m_bRecordingsChanged) break;
            if (m_bStopRecording) break;
            // 1st check if the recording itself should b recorded
            if ( rec.ShouldRecord(DateTime.Now,null,iPreRecordInterval, iPostRecordInterval) )
            {
              // yes, then record it
              if ( Record(rec,null,iPreRecordInterval, iPostRecordInterval))
              {
                recordings.RemoveAt(iRec);
                break;
              }
            }
            iRec++;
          }
        }

        Process();
        
        // wait for the next minute
        TimeSpan ts=DateTime.Now-dtTime;
        while (ts.Minutes==0 && DateTime.Now.Minute==dtTime.Minute)
        {
          if (m_eState !=State.Running) break;
          if (m_bPreviewChanged) break;
          if (m_bRecordingsChanged) break;
          if (m_bStopRecording) break;
          System.Threading.Thread.Sleep(500);
          ts=DateTime.Now-dtTime;
        }
        dtTime=DateTime.Now;
      }

      foreach (TVCaptureDevice cap in m_tvcards)
      {
        if (cap.Previewing)
        {
          cap.Previewing=false;
        }
        if (cap.IsRecording)
        {
          cap.StopRecording();
        }
        cap.Process();cap.Process();cap.Process();
      }
      m_eState=State.Idle;
      Log.Write("Recorder: thread stopped");
    }
    /// <summary>
    /// When called this method starts an immediate recording on the channel specified
    /// It will record the next 2 hours.
    /// </summary>
    /// <param name="strChannel"></param>
    static public void RecordNow(string strChannel)
    {
      Log.Write("Recorder: record now:"+strChannel);
      // create a new recording which records the next 2 hours...
      TVRecording tmpRec = new TVRecording();
      tmpRec.Start=Utils.datetolong(DateTime.Now);
      tmpRec.End=Utils.datetolong(DateTime.Now.AddMinutes(2*60) );
      tmpRec.Channel=strChannel;
      tmpRec.Title="Manual";
      tmpRec.RecType=TVRecording.RecordingType.Once;
      
      TVDatabase.AddRecording(ref tmpRec);
    }

    static void Process()
    {
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        dev.Process();
      }
      m_bStopRecording=false;
    }

    static bool Record(TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)
    {
      // check if we're already recording this...
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.IsRecording)
        {
          if (dev.ScheduleRecording.ID==rec.ID) return true;
        }
      }

      Log.Write("Recorder: time to record a program on channel:"+rec.Channel);
      Log.Write("Recorder: find free capture card");

      // find free device
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.UseForRecording)
        {
          if (!dev.Previewing && !dev.IsRecording)
          {
            Log.Write("Recorder: found capture card:{0} {1}", dev.ID, dev.VideoDevice);
            dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
            return true;
          }
        }
      }

      //hrmmm no device free, check if we can stop a preview
      Log.Write("Recorder: All capture cards busy, stop any previewing...");
      
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.UseForRecording)
        {
          if (dev.Previewing && !dev.IsRecording)
          {
            Log.Write("Recorder: capture card:{0} {1} was previewing. Now use it for recording", dev.ID,dev.VideoDevice);
            dev.Previewing=false;
            dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
            return true;
          }
        }
      }
      //no device free...
      Log.Write("No free capture cards found to record program");
      System.Threading.Thread.Sleep(1000);
      return false;
    }

    static public void StopRecording(string m_strChannel)
    {
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        if (dev.IsRecording) 
        {
          if ( String.Compare(dev.ScheduleRecording.Channel,m_strChannel,true)==0)
          {
            Log.Write("Recorder: Stop recording on channel:{0} capture card:{1}", m_strChannel,dev.ID);
            dev.StopRecording();
            m_bStopRecording=true;
          }
        }
      }
    }

    static public bool IsRecording
    {
      get
      {
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return true;
        }
        return false;
      }
    }
    static public TVProgram ProgramRecording
    {
      get
      {
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return dev.ProgramRecording;
        }
        return null;
      }
    }
    static public TVRecording ScheduleRecording
    {
      get
      {
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.IsRecording) return dev.ScheduleRecording;
        }
        return null;
      }
    }

    static void HandlePreview()
    {
      //check if we are already previewing
      if (!m_bPreviewChanged) return;
      m_bPreviewChanged=false;

      if (m_bPreviewing)
      {
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.Previewing) 
          {
            dev.PreviewChannel=m_strPreviewChannel;
            return;
          }
        }
        Log.Write("Recorder: Start preview, find free tuner");
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (!dev.IsRecording && dev.UseForTV) 
          {
            Log.Write("Recorder: use capture card:{0} for previewing:{1}", dev.ID,m_strPreviewChannel);
            dev.Previewing=true;
            dev.PreviewChannel=m_strPreviewChannel;
          }
        }
        return;
      }
      else
      {
        Log.Write("Recorder: stop preview");
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.Previewing) 
          {
            dev.Previewing=false;
          }
        }
        m_strPreviewChannel="";
      }
    }

    static public bool Previewing
    {
      get{
        foreach (TVCaptureDevice dev in m_tvcards)
        {
          if (dev.Previewing) return true;
        }
        return false;
      }
      set
      {
        if (m_bPreviewing!=value)
        {
          m_bPreviewing=value;
          m_bPreviewChanged=true;
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
        if (m_strPreviewChannel!=value) 
        {
          m_strPreviewChannel=value;
          m_bPreviewChanged=true;
        }
      }
    }
  }
}
