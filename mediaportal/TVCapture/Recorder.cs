using System;
using System.Threading;
using System.Collections;
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
    static bool          m_bIsRecording=false;        // flag indicating if we are recording or not
    static bool          m_bStopRecording=false;      // flag indicating we should abort current recording
    static TVProgram     m_programRecording=null;     // when recording this contains the program we are recording
    static TVRecording   m_recordRecording=null;      // when recording this contains the schedule
    static bool          m_bPreview=false;            // is preview running or not?
    static int           m_iPreviewChannel=28;        // current preview channel
    static string        m_strPreviewChannel="";
    static Capture       capture=null;                // current capture running
    
    public Recorder()
    {
    }

    static void OnGammaContrastChanged()
    {
      if (Previewing && capture!=null)
      {
        if (capture.VideoAmp!=null)
        {
          if (GUIGraphicsContext.Brightness>0)
          {
            Log.Write("set brightness:{0}", GUIGraphicsContext.Brightness);
            capture.VideoAmp.Brightness=GUIGraphicsContext.Brightness;
            GUIGraphicsContext.Save();
          }
          if (GUIGraphicsContext.Contrast>0)
          {
            Log.Write("set Contrast:{0}", GUIGraphicsContext.Contrast);
            capture.VideoAmp.Contrast=GUIGraphicsContext.Contrast;
            GUIGraphicsContext.Save();
          }
          if (GUIGraphicsContext.Gamma>0)
          {
            Log.Write("set Gamma:{0}", GUIGraphicsContext.Gamma);
            capture.VideoAmp.Gamma=GUIGraphicsContext.Gamma;
            GUIGraphicsContext.Save();
          }
          if (GUIGraphicsContext.Saturation>0)
          {
            Log.Write("set Saturation:{0}", GUIGraphicsContext.Saturation);
            capture.VideoAmp.Saturation=GUIGraphicsContext.Saturation;
            GUIGraphicsContext.Save();
          }
          if (GUIGraphicsContext.Sharpness>0)
          {
            Log.Write("set Sharpness:{0}", GUIGraphicsContext.Sharpness);
            capture.VideoAmp.Sharpness=GUIGraphicsContext.Sharpness;
            GUIGraphicsContext.Save();
          }

        }
      }
    }

    static void OnVideoWindowChanged()
    {
      if (Previewing && capture!=null)
      {
        capture.SetVideoPosition(GUIGraphicsContext.VideoWindow);
      }
    }

    /// <summary>
    /// select which TV channel should be visible in the tv-preview window
    /// Previewing only works when nothing is recording. 
    /// </summary>
    static public string PreviewChannel
    {
      get 
      {
        return m_strPreviewChannel;
      }
      set 
      { 
        // get the channel number for the tv channel
        m_strPreviewChannel=value;
        int iChannel=GetChannelNr(m_strPreviewChannel);
        if (iChannel>0)
        {
          // if we're already previewing & not recording
          // then just switch channels
          if (!IsRecording )
          {
            Log.Write("preview channel:{0}={1}", m_strPreviewChannel,iChannel);
            if (Previewing && capture!=null)
            {
              try
              {
                SelectChannel(iChannel,true);
              }
              catch(Exception ex)
              {
                Log.Write("SelectChannel() failed:{0} {1}",ex.Message, ex.StackTrace);
              }
            }
            else 
            {
              m_iPreviewChannel=iChannel;
            }
          }
        }
        else Log.Write("Unknown channel:{0}", m_strPreviewChannel);
      }
    }

    /// <summary>
    /// Turn on/off previewing. You can only turn on previewing when we're not recording
    /// </summary>
    static public bool Previewing
    {
      get { return m_bPreview;}
      set 
      {
        // should preview turned off
        if (value==false)
        {
          // yes. Check that capture is running
          m_bPreview=false;
          if (capture!=null)
          {
            // and we're not recording
            if (!IsRecording)
            {
              // then stop capture & thus preview
              StopCapture();
            }
            return;
          }
        }
        else
        {
          // preview should b turned on
          // check if we're not recording
          if (!IsRecording)
          {
            // check if preview isnt running already
            if (!m_bPreview || capture==null)
            {
              // start preview
              if (StartCapture("",m_iPreviewChannel))
              {
                // and set video window position
                capture.PreviewWindow=GUIGraphicsContext.form;
                capture.SetVideoPosition(GUIGraphicsContext.VideoWindow);
                if (!capture.Running)
                {
                  capture.Start();
                }
                m_bPreview=value;
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Start record thread. The recorder thread will take care of all scheduled recordings
    /// </summary>
    static public void Start()
    {
      if (m_eState!=State.Idle) Stop();
      
      TVDatabase.OnRecordingsChanged += new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
      GUIGraphicsContext.OnGammaContrastBrightnessChanged += new VideoGammaContrastBrightnessHandler(Recorder.OnGammaContrastChanged);
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
      
      DateTime dtTime=DateTime.Now;

      Log.Write("Recording thread started");
      m_eState =State.Running;
      m_bRecordingsChanged=true;
      m_bIsRecording=false;
      m_programRecording=null;
      m_recordRecording=null;
    
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


        // for each tv-channel
        foreach (TVChannel chan in channels)
        {
          if (m_eState !=State.Running) break;
          // get all programs running for this TV channel
          // between  (now-iPreRecordInterval) - (now+iPostRecordInterval+3 hours)

          DateTime dtStart=DateTime.Now.AddMinutes(-iPreRecordInterval);
          DateTime dtEnd=DateTime.Now.AddMinutes(iPostRecordInterval+3*60);
          long iStartTime=Utils.datetolong(dtStart);
          long iEndTime=Utils.datetolong(dtEnd);
          
          runningPrograms.Clear();
          TVDatabase.GetPrograms(chan.Name,iStartTime,iEndTime,ref runningPrograms);

          // for each TV recording scheduled
          foreach (TVRecording rec in recordings)
          {
            if (m_eState !=State.Running) break;
           
            // 1st check if the recording itself should b recording
            bool bRecorded=false;
            if ( rec.ShouldRecord(DateTime.Now,null,iPreRecordInterval, iPostRecordInterval) )
            {
              // yes, then record it
              Record(rec,null,iPreRecordInterval, iPostRecordInterval);
              bRecorded=true;
            }

            if (!bRecorded)
            {
              // if not, then check each check for each tv program
              foreach (TVProgram currentProgram in runningPrograms)
              {
                if (m_eState !=State.Running) break;
                // if the recording should record the tv program
                if ( rec.ShouldRecord(DateTime.Now,currentProgram,iPreRecordInterval, iPostRecordInterval) )
                {
                  // yes, then record it
                  Record(rec,currentProgram, iPreRecordInterval, iPostRecordInterval);
                  bRecorded=true;
                  break;
                }
              }
            }
            if (bRecorded) 
            {
              break;
            }
          }
        }
        
        // wait for the next minute
        TimeSpan ts=DateTime.Now-dtTime;
        while (ts.Minutes==0 && DateTime.Now.Minute==dtTime.Minute)
        {
          if (m_eState !=State.Running) break;
          if (m_bRecordingsChanged) break;
          System.Threading.Thread.Sleep(1000);
          ts=DateTime.Now-dtTime;
        }
        dtTime=DateTime.Now;
      }
      m_eState=State.Idle;
      m_bIsRecording=false;
      m_programRecording=null;
      m_recordRecording=null;
      Log.Write("Recording thread stopped");
    }

    /// <summary>
    /// Stop the current preview/capture graph
    /// </summary>
    static void StopCapture()
    {
      if (capture!=null)
      {
        Log.Write("Stop capture");
        GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(OnVideoWindowChanged);
        m_iPreviewChannel=0;
        m_strPreviewChannel="";
        capture.PreviewWindow=null;
        Log.Write("stop capture");
        if (capture.Running)
          capture.Stop();
        capture.Dispose();
        capture=null;
      }
    }

    static void SelectChannel(int iChannelNr, bool bCheckSame)
    {
      Log.Write(" select channel:{0}",iChannelNr);
      bool bNeedStop=false;
      bool bWasRunning=capture.Running; 
      bool bPreviewing=Previewing;
      string strFileName=capture.Filename;
      if (iChannelNr==m_iPreviewChannel && bCheckSame) 
      {
        Log.Write(" same channel,ignore");
        return;
      }
      if (m_iPreviewChannel>=254 && iChannelNr <254) bNeedStop=true;
      if (m_iPreviewChannel<254 && iChannelNr >=254) bNeedStop=true;
      if (bNeedStop && bWasRunning) 
      {
        Log.Write("Switch channels. Need stop");
        StopCapture();
        StartCapture(strFileName,iChannelNr);
        if (bPreviewing)
        {
          Previewing=true;
        }
        return;
      }
      if (bWasRunning)
        Log.Write("Switch channel {0}->{1}", m_iPreviewChannel,iChannelNr);
      else
        Log.Write("Select channel {0}", iChannelNr);
      m_iPreviewChannel=iChannelNr;

      int iTunerCountry=31;
      string strTunerType="Antenna";
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strTunerType=xmlreader.GetValueAsString("capture","tuner","Antenna");
        iTunerCountry=xmlreader.GetValueAsInt("capture","country",31);
      }
      if (capture.VideoSources!=null)
      {
        // now select the video source
        if (iChannelNr==254) // composite in
        {
          Log.Write("select composite in");
          // find composite in
          foreach (DirectX.Capture.CrossbarSource source in capture.VideoSources)
          {
            // and if found
            if ( source.IsComposite)
            {
              // set it as the video source
              Log.Write(" source=composite in");
              try
              {
                capture.VideoSource=source;
              }
              catch(Exception)
              {
              }
              break;
            }
          }
        }
        else if (iChannelNr==255) // SVHS-in
        {
          // find SVHS-Input
          Log.Write("select SVHS in");
          foreach (DirectX.Capture.CrossbarSource  source in capture.VideoSources)
          {
            // and if found
            if ( source.IsSVHS)
            {
              //set it as the video source
              Log.Write(" source=SVHS in");
              try
              {
                capture.VideoSource=source;
              }
              catch(Exception)
              {
              }
              break;
            }
          }
        }
        else // tuner-in
        {
          Log.Write(" source=Tuner country:{0} channel:{1}",iTunerCountry,iChannelNr);
          // find TV Tuner
          foreach (DirectX.Capture.CrossbarSource  source in capture.VideoSources)
          {
            // if found
            if ( source.IsTuner)
            {
              // set it as the video source
              // and set tuner properties like channel,country,...
              try
              {
                capture.VideoSource=source;
              }
              catch(Exception)
              {
              }
              break;
            }
          }
        }
      }
      else Log.Write("No video sources?");
      
      if (iChannelNr<254)
      {
        if (capture.Tuner!=null)
        {
          try
          {
            capture.Tuner.Country=iTunerCountry;
          }
          catch (Exception)
          {
            Log.Write("Failed to set tuner to country:{0}", iTunerCountry);
          }
          try
          {
            capture.Tuner.Mode= DShowNET.AMTunerModeType.TV;
          }
          catch (Exception)
          {
            Log.Write("Failed to set tuner tuning mode to tv");
          }
          try
          {
            capture.Tuner.SetTuningSpace(66);
          }
          catch (Exception)
          {
            Log.Write("Failed to set tuner tuningspace");
          }
          if (iChannelNr>0)
          {
            try
            {
              // set type: antenna or cable?
              if (strTunerType=="Antenna")
              {
                capture.Tuner.InputType=TunerInputType.Antenna;
              }
              else
              {
                capture.Tuner.InputType=TunerInputType.Cable;
              }
            }
            catch (Exception)
            {
              Log.Write("Failed to set tuner input type to :{0}", strTunerType);
            }

            try
            {
              capture.Tuner.Channel=iChannelNr;
							
							Log.Write("TV tuner set to channel:{0} freq:{1} Hz", capture.Tuner.Channel, capture.Tuner.GetVideoFrequency);
            }
            catch(Exception)
            {
              Log.Write("failed to set tuner channel to:{0}",iChannelNr);
            }
          }
        }
        else Log.Write("No tuner?");
      }
      try
      {
        capture.FixCrossbarRouting();
      }
      catch (Exception)
      {
        Log.Write("Failed to set crossbar routing");
      }
    }

    /// <summary>
    /// Start a new capture/preview graph
    /// </summary>
    /// <param name="strFileName">Filename where capture should be stored. If empty no capture is started</param>
    /// <param name="iChannelNr">TV Channel number to capture/preview</param>
    /// <returns></returns>
    static bool StartCapture(string strFileName, int iChannelNr)
    {
      StopCapture();
      Log.Write("start capture channel:{0}", iChannelNr);
      // get profile from MediaPortal.xml
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strVideoDevice=xmlreader.GetValueAsString("capture","videodevice","none");
        string strAudioDevice=xmlreader.GetValueAsString("capture","audiodevice","none");
        string strCompressorAudio=xmlreader.GetValueAsString("capture","audiocompressor","none");
        string strCompressorVideo=xmlreader.GetValueAsString("capture","Videocompressor","none");
        if (strVideoDevice=="none") 
        {
          Log.Write("err:no video capture device selected in setup");
          return false;
        }
        DirectX.Capture.Filter VideoDevice=null;
        DirectX.Capture.Filter audioDevice=null;
        Filters filters=new Filters();

        Log.Write(" find video capture device");
        // find Video capture device
        foreach (Filter filter in filters.VideoInputDevices)
        {
          if (String.Compare(filter.Name,strVideoDevice)==0)
          {
            Log.Write(" found Video capture device:{0}", strVideoDevice);
            VideoDevice=filter;
            break;
          }
        }

        Log.Write(" find audio capture device");
        // find audio capture device
        foreach (Filter filter in filters.AudioInputDevices)
        {
          if (String.Compare(filter.Name,strAudioDevice)==0)
          {
            Log.Write(" found audio capture device:{0}", strAudioDevice);
            audioDevice=filter;
            break;
          }
        }

        if (VideoDevice==null) 
        {
          Log.Write("video capture device not found");
          return false;
        }

        // create new capture!
        Log.Write("create directshow graph");
        capture = new Capture(VideoDevice,audioDevice);

        // add audio compressor
        Log.Write(" add audio compressor");
        foreach (Filter filter in filters.AudioCompressors)
        {
          if (String.Compare(filter.Name,strCompressorAudio)==0)
          {
            Log.Write(" found audio compressor:{0}", strCompressorAudio);
            capture.AudioCompressor=filter;
            break;
          }
        }

        //add Video compressor
        Log.Write(" add video compressor");
        foreach (Filter filter in filters.VideoCompressors)
        {
          if (String.Compare(filter.Name,strCompressorVideo)==0)
          {
            Log.Write(" found Video compressor:{0}", strCompressorVideo);
            capture.VideoCompressor=filter;
            break;
          }
        }

        SelectChannel(iChannelNr,false);

        // set brightness, contrast, gamma etc...
        SetBrightnessContrastGamma();

        capture.LoadSettings();

        // set filename for capture
        if (strFileName!=null && strFileName.Length>0)
        {
          capture.Filename=strFileName;
        }
        else capture.Filename="";
        GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(OnVideoWindowChanged);
      }
      return true;
    }

    /// <summary>
    /// Start recording a tv program / recording schedule
    /// </summary>
    /// <param name="rec">The recording schedule for this recording</param>
    /// <param name="currentProgram">the tv program to record (may be null)</param>
    /// <param name="iInterval">interval the recording should start before the starttime and end after the stoptime</param>
    static void Record(TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)
    { 

      // get capture format & recording path
      string strCaptureFormat="";
      string strRecPath="";
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strCaptureFormat=xmlreader.GetValueAsString("capture","format",".avi");
        strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
      }

      Log.Write("start recording:{0}", rec.ToString());
      m_bStopRecording=false;
      m_programRecording=currentProgram;
      m_recordRecording=rec;
      m_bIsRecording=true;
      Previewing=false;

      if (strRecPath==null) 
      {
        Log.Write("Capture failed. no recording path specified in setup");
        m_bIsRecording=false;
        m_programRecording=null;
        m_recordRecording=null;
        return;
      }

      // get the channel number
      int iChannelNr=GetChannelNr(rec.Channel);
      
      // compose the filename in format [channel][date][time].mpg
      try
      {
        string strName;
        if (currentProgram!=null)
        {
          DateTime dt=currentProgram.StartTime;
          strName=String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}{7}", 
            currentProgram.Channel,currentProgram.Title,
            dt.Year,dt.Month,dt.Day,
            dt.Hour,
            dt.Minute,strCaptureFormat);
        }
        else
        {
          DateTime dt=DateTime.Now;
          strName=String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}{7}", 
            rec.Channel,rec.Title,
            dt.Year,dt.Month,dt.Day,
            dt.Hour,
            dt.Minute,strCaptureFormat);
        }
        string strFilenameEnd=String.Format(@"{0}\{1}",strRecPath, Utils.MakeFileName(strName) );
        string strFilenameTmp=String.Format(@"{0}\record{1}",strRecPath, strCaptureFormat );
        try
        {
          System.IO.File.Delete(strFilenameTmp);
        }
        catch (Exception)
        {
        }

        // create new capture
        Log.Write("start capture to {0}", strFilenameTmp);
        if ( !StartCapture(strFilenameTmp, iChannelNr) )
        {
          // create capture failed
          Log.Write("cannot start capture");
          m_bIsRecording=false;
          m_programRecording=null;
          m_recordRecording=null;
          return;
        }
        // start new capture
        capture.Start();


        // capture is now running
        // now just wait till recording has ended
        // or user has canceled the recording
        TVUtil util=new TVUtil();
        TVProgram currentRunningProgram=util.GetCurrentProgram(rec.Channel);
        util=null;
        TVRecorded newRecordedTV = new TVRecorded();
        newRecordedTV.Start=Utils.datetolong(DateTime.Now);
        newRecordedTV.Channel=rec.Channel;
        newRecordedTV.FileName=strFilenameEnd;
        if (currentRunningProgram!=null)
        {
          newRecordedTV.Title=currentRunningProgram.Title;
          newRecordedTV.Genre=currentRunningProgram.Genre;
          newRecordedTV.Description=currentRunningProgram.Description;
        }
        else
        {
          newRecordedTV.Title="";
          newRecordedTV.Genre="";
          newRecordedTV.Description="";
        }
        while ( rec.ShouldRecord(DateTime.Now,currentProgram,iPreRecordInterval, iPostRecordInterval) )
        {
          // did user cancel the recording?
          if (m_bStopRecording==true)
          {
            // yep, stop it
            rec.Canceled=Utils.datetolong(DateTime.Now);
            TVDatabase.ChangeRecording(ref rec);
            Log.Write("user canceled recording:{0}", rec.ToString());
            break;
          }
          // if program is stopping, then stop capture also
          if (m_eState !=State.Running) break;

          System.Threading.Thread.Sleep(1000);
        }

        // recording ended or stopped
        StopCapture();
        // rename the temp. name-> final name
        Log.Write("stop capture to {0}", strFilenameTmp);
        try
        {
          Log.Write("move {0}->{1}", strFilenameTmp,strFilenameEnd);
          if (System.IO.File.Exists(strFilenameEnd))
          {
            System.IO.File.Delete(strFilenameEnd);
          }
          System.IO.File.Move(strFilenameTmp, strFilenameEnd);
        }
        catch(Exception ex)
        {
          Log.Write("unable to move file {0}-{1} {2}", strFilenameTmp, strFilenameEnd,ex.Message);
        }

        newRecordedTV.End=Utils.datetolong(DateTime.Now);
        TVDatabase.AddRecordedTV(newRecordedTV);
      }
      catch(Exception ex)
      {
        Log.Write("exception during capture:{0} {1}", ex.Message, ex.StackTrace);
      }
      // and stop the capture
      Log.Write("stop recording:{0}", rec.ToString());
      StopCapture();
      m_bIsRecording=false;
      m_programRecording=null;
      m_recordRecording=null;
    }

    /// <summary>
    /// Returns whether the record is currently recording or not
    /// </summary>
    static public bool IsRecording
    {
      get { return m_bIsRecording;}
    }

    /// <summary>
    /// This function will stop the current recording
    /// </summary>
    static public void StopRecording()
    {
      if (IsRecording)
      {
        m_bStopRecording=true;
      }
    }
    /// <summary>
    /// Returns the current TV program which is being recorded (can be null)
    /// Is only valid when IsRecording returns true
    /// </summary>
    static public TVProgram ProgramRecording
    {
      get { return m_programRecording;}
    }
    
    /// <summary>
    /// Returns the current recording schedule which is being recorded
    /// Is only valid when IsRecording returns true
    /// </summary>
    static public TVRecording ScheduleRecording
    {
      get { return m_recordRecording;}
    }

    /// <summary>
    /// When called this method starts an immediate recording on the channel specified
    /// It will record the next 2 hours.
    /// </summary>
    /// <param name="strChannel"></param>
    static public void RecordNow(string strChannel)
    {
      if (!IsRecording)
      {
        // create a new recording which records the next 2 hours...
        TVRecording tmpRec = new TVRecording();
        tmpRec.Start=Utils.datetolong(DateTime.Now);
        tmpRec.End=Utils.datetolong(DateTime.Now.AddMinutes(2*60) );
        tmpRec.Channel=strChannel;
        tmpRec.Title="Manual";
        tmpRec.RecType=TVRecording.RecordingType.Once;
        
        TVDatabase.AddRecording(ref tmpRec);
      }
    }

    /// <summary>
    /// return the channel number for a channel name
    /// </summary>
    /// <param name="strChannelName">Channel Name</param>
    /// <returns>Channel number</returns>
    static int GetChannelNr(string strChannelName)
    {
      int iChannelNr=0;
      
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (String.Compare(strChannelName,chan.Name,true)==0)
        {
          iChannelNr=(int)chan.Number;
          break;
        }
      }
      return iChannelNr;
    }
    static void SetBrightnessContrastGamma()
    {
      if (capture==null) return;
      if (capture.VideoAmp==null) return;

      if (GUIGraphicsContext.Brightness==0) GUIGraphicsContext.Brightness=capture.VideoAmp.BrightnessDefault;
      if (GUIGraphicsContext.Contrast==0) GUIGraphicsContext.Contrast=capture.VideoAmp.ContrastDefault;
      if (GUIGraphicsContext.Gamma==0) GUIGraphicsContext.Gamma=capture.VideoAmp.GammaDefault;
      if (GUIGraphicsContext.Saturation==0) GUIGraphicsContext.Saturation=capture.VideoAmp.SaturationDefault;
      if (GUIGraphicsContext.Sharpness==0) GUIGraphicsContext.Sharpness=capture.VideoAmp.SharpnessDefault;

      capture.VideoAmp.Brightness=GUIGraphicsContext.Brightness;
      capture.VideoAmp.Contrast=GUIGraphicsContext.Contrast;
      capture.VideoAmp.Gamma=GUIGraphicsContext.Gamma;
      capture.VideoAmp.Saturation=GUIGraphicsContext.Saturation;
      capture.VideoAmp.Sharpness=GUIGraphicsContext.Sharpness;


    }
  }
}
