using System;
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
	[Serializable]
	public class TVCaptureDevice
  {
    string        m_strAudioCompressor="";
    string        m_strVideoCompressor="";
    string        m_strVideoDevice="";
    string        m_strAudioDevice="";
    string        m_strCaptureFormat="";
    bool          m_bUseForRecording;
    bool          m_bUseForTV;			
    [NonSerialized]
    bool          m_bIsRecording=false;        // flag indicating if we are recording or not
    
    [NonSerialized]
    bool          m_bStopRecording=false;      // flag indicating we should abort current recording
    
    [NonSerialized]
    TVProgram     m_CurrentProgramRecording=null;     // when recording this contains the program we are recording
    
    [NonSerialized]
    TVRecording   m_CurrentTVRecording=null;      // when recording this contains the schedule

    [NonSerialized]
    bool          m_bPreview=false;            // is preview running or not?

    [NonSerialized]
    int           m_iPreviewChannel=28;        // current preview channel

    [NonSerialized]
    string        m_strPreviewChannel="";

    [NonSerialized]
    Capture       capture=null;                // current capture running

    [NonSerialized]
    TVRecorded    m_newRecordedTV = null;

    [NonSerialized]
    string        m_strFilenameTmp="";

    [NonSerialized]
    string        m_strFilenameEnd="";

    [NonSerialized]
    int           m_iPreRecordInterval=0;

    [NonSerialized]
    int           m_iPostRecordInterval=0;

    [NonSerialized]
    int           m_iID=0;

		public TVCaptureDevice()
		{
		}

    public string AudioCompressor
    {
      get { return m_strAudioCompressor;}
      set { m_strAudioCompressor=value;}
    }

    public string VideoCompressor
    {
      get { return m_strVideoCompressor;}
      set { m_strVideoCompressor=value;}
    }

    public string VideoDevice
    {
      get { return m_strVideoDevice;}
      set { m_strVideoDevice=value;}
    }
    public string AudioDevice
    {
      get { return m_strAudioDevice;}
      set { m_strAudioDevice=value;}
    }
    public string CaptureFormat
    {
      get { return m_strCaptureFormat;}
      set { m_strCaptureFormat=value;}
    }
    public bool UseForTV
    {
      get { return m_bUseForTV;}
      set { m_bUseForTV=value;}
    }
    public bool UseForRecording
    {
      get { return m_bUseForRecording;}
      set { m_bUseForRecording=value;}
    }
    public override string ToString()
    {
      return VideoDevice;
    }
    public int ID
    {
      get { return m_iID;}
      set {m_iID=value;}
    }

    void SetBrightnessContrastGamma()
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


    /// <summary>
    /// Start a new capture/preview graph
    /// </summary>
    /// <param name="strFileName">Filename where capture should be stored. If empty no capture is started</param>
    /// <param name="iChannelNr">TV Channel number to capture/preview</param>
    /// <returns></returns>
    bool StartCapture(string strFileName, int iChannelNr)
    {
      StopCapture();
      GUIGraphicsContext.OnGammaContrastBrightnessChanged += new VideoGammaContrastBrightnessHandler(this.OnGammaContrastChanged);

      Log.Write("  CaptureCard:{0} start capture",ID);
      DirectX.Capture.Filter filterVideoDevice=null;
      DirectX.Capture.Filter filterAudioDevice=null;
      Filters filters=new Filters();

      Log.Write("  CaptureCard:{0}   find video capture device:{1}",ID,VideoDevice);
      // find Video capture device
      foreach (Filter filter in filters.VideoInputDevices)
      {
        if (String.Compare(filter.Name,VideoDevice)==0)
        {
          Log.Write("  CaptureCard:{0}     add Video capture device:{1}",ID, VideoDevice);
          filterVideoDevice=filter;
          break;
        }
      }

      Log.Write("  CaptureCard:{0}   find audio capture device:{1}",ID,AudioDevice);
      // find audio capture device
      foreach (Filter filter in filters.AudioInputDevices)
      {
        if (String.Compare(filter.Name,AudioDevice)==0)
        {
          Log.Write("  CaptureCard:{0}     add audio capture device:{1}",ID, AudioDevice);
          filterAudioDevice=filter;
          break;
        }
      }

      if (filterVideoDevice==null) 
      {
        Log.Write("  CaptureCard:{0} video capture device not found",ID);
        return false;
      }

      // create new capture!
      Log.Write("  CaptureCard:{0}   create directshow graph",ID);
      capture = new Capture(filterVideoDevice,filterAudioDevice);

      if (strFileName.Length>0)
      {
        // add audio compressor
        Log.Write("  CaptureCard:{0}   find audio compressor:{1}",ID,AudioCompressor);
        foreach (Filter filter in filters.AudioCompressors)
        {
          if (String.Compare(filter.Name,AudioCompressor)==0)
          {
            Log.Write("  CaptureCard:{0}     add audio compressor:{1}",ID, AudioCompressor);
            capture.AudioCompressor=filter;
            break;
          }
        }

        //add Video compressor
        Log.Write("  CaptureCard:{0}   find video compressor:{1}",ID,VideoCompressor);
        foreach (Filter filter in filters.VideoCompressors)
        {
          if (String.Compare(filter.Name,VideoCompressor)==0)
          {
            Log.Write("  CaptureCard:{0}     add Video compressor:{1}",ID, VideoCompressor);
            capture.VideoCompressor=filter;
            break;
          }
        }
      }

      
      Log.Write("  CaptureCard:{0}   select correct channel",ID);
      SelectChannel(iChannelNr,false);

      // set brightness, contrast, gamma etc...
      Log.Write("  CaptureCard:{0}   Setup brightness, contrast, gamma settings",ID);
      SetBrightnessContrastGamma();

      capture.LoadSettings();

      // set filename for capture
      if (strFileName!=null && strFileName.Length>0)
      {
        capture.Filename=strFileName;
      }
      else capture.Filename="";
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(OnVideoWindowChanged);
      return true;
    }

    /// <summary>
    /// Stop the current preview/capture graph
    /// </summary>
    void StopCapture()
    {
      if (capture!=null)
      {
        Log.Write("  CaptureCard:{0} Stop capture",ID);
        GUIGraphicsContext.OnGammaContrastBrightnessChanged -= new VideoGammaContrastBrightnessHandler(this.OnGammaContrastChanged);
        GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(OnVideoWindowChanged);
        m_iPreviewChannel=0;
        m_strPreviewChannel="";
        capture.PreviewWindow=null;
        if (capture.Running)
          capture.Stop();
        capture.Dispose();
        capture=null;
      }
    }

    void SelectChannel(int iChannelNr, bool bCheckSame)
    {
      Log.Write("  CaptureCard:{0}   select channel:{1}",ID,iChannelNr);
      bool bNeedStop=false;
      bool bWasRunning=capture.Running; 
      bool bPreviewing=Previewing;
      string strFileName=capture.Filename;
      if (iChannelNr==m_iPreviewChannel && bCheckSame) 
      {
        Log.Write("  CaptureCard:{0}   same channel,ignore",ID);
        return;
      }
      if (m_iPreviewChannel>=254 && iChannelNr <254) bNeedStop=true;
      if (m_iPreviewChannel<254 && iChannelNr >=254) bNeedStop=true;
      if (bNeedStop && bWasRunning) 
      {
        Log.Write("  CaptureCard:{0}   Switch channels. Need stop",ID);
        StopCapture();
        StartCapture(strFileName,iChannelNr);
        if (bPreviewing)
        {
          Previewing=true;
        }
        return;
      }
      if (bWasRunning)
        Log.Write("  CaptureCard:{0}   Switch channel {1}->{2}",ID, m_iPreviewChannel,iChannelNr);
      else
        Log.Write("  CaptureCard:{0}   Select channel {1}",ID, iChannelNr);
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
          Log.Write("  CaptureCard:{0}   select composite in",ID);
          // find composite in
          foreach (DirectX.Capture.CrossbarSource source in capture.VideoSources)
          {
            // and if found
            if ( source.IsComposite)
            {
              // set it as the video source
              Log.Write("  CaptureCard:{0}   source=composite in",ID);
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
          Log.Write("  CaptureCard:{0}   select SVHS in",ID);
          foreach (DirectX.Capture.CrossbarSource  source in capture.VideoSources)
          {
            // and if found
            if ( source.IsSVHS)
            {
              //set it as the video source
              Log.Write("  CaptureCard:{0}   source=SVHS in",ID);
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
          Log.Write("  CaptureCard:{0}   source=Tuner country:{1} channel:{2}",ID,iTunerCountry,iChannelNr);
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
      else Log.Write("  CaptureCard:{0} No video sources?",ID);
      
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
            Log.Write("  CaptureCard:{0}   Failed to set tuner to country:{1}",ID, iTunerCountry);
          }
          try
          {
            capture.Tuner.Mode= DShowNET.AMTunerModeType.TV;
          }
          catch (Exception)
          {
            Log.Write("  CaptureCard:{0}   Failed to set tuner tuning mode to tv",ID);
          }
          try
          {
            capture.Tuner.SetTuningSpace(66);
          }
          catch (Exception)
          {
            Log.Write("  CaptureCard:{0}   Failed to set tuner tuningspace",ID);
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
              Log.Write("  CaptureCard:{0}   Failed to set tuner input type to :{1}",ID, strTunerType);
            }

            try
            {
              capture.Tuner.Channel=iChannelNr;
							
              Log.Write("  CaptureCard:{0}   TV tuner set to channel:{1} freq:{2} Hz",ID, capture.Tuner.Channel, capture.Tuner.GetVideoFrequency);
            }
            catch(Exception)
            {
              Log.Write("  CaptureCard:{0}   failed to set tuner channel to:{1}",ID,iChannelNr);
            }
          }
        }
        else Log.Write("  CaptureCard:{0}   No tuner?",ID);
      }
      try
      {
        capture.FixCrossbarRouting();
      }
      catch (Exception)
      {
        Log.Write("  CaptureCard:{0} Failed to set crossbar routing",ID);
      }
    }


    void OnGammaContrastChanged()
    {
      if (Previewing && capture!=null)
      {
        if (capture.VideoAmp!=null)
        {
          if (GUIGraphicsContext.Brightness>0)
          {
            Log.Write("  CaptureCard:{0} set brightness:{1}",ID, GUIGraphicsContext.Brightness);
            capture.VideoAmp.Brightness=GUIGraphicsContext.Brightness;
            GUIGraphicsContext.Save();
          }
          if (GUIGraphicsContext.Contrast>0)
          {
            Log.Write("  CaptureCard:{0} set Contrast:{1}",ID, GUIGraphicsContext.Contrast);
            capture.VideoAmp.Contrast=GUIGraphicsContext.Contrast;
            GUIGraphicsContext.Save();
          }
          if (GUIGraphicsContext.Gamma>0)
          {
            Log.Write("  CaptureCard:{0} set Gamma:{1}",ID, GUIGraphicsContext.Gamma);
            capture.VideoAmp.Gamma=GUIGraphicsContext.Gamma;
            GUIGraphicsContext.Save();
          }
          if (GUIGraphicsContext.Saturation>0)
          {
            Log.Write("  CaptureCard:{0} set Saturation:{1}",ID, GUIGraphicsContext.Saturation);
            capture.VideoAmp.Saturation=GUIGraphicsContext.Saturation;
            GUIGraphicsContext.Save();
          }
          if (GUIGraphicsContext.Sharpness>0)
          {
            Log.Write("  CaptureCard:{0} set Sharpness:{1}",ID, GUIGraphicsContext.Sharpness);
            capture.VideoAmp.Sharpness=GUIGraphicsContext.Sharpness;
            GUIGraphicsContext.Save();
          }

        }
      }
    }

    void OnVideoWindowChanged()
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
    public string PreviewChannel
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
            Log.Write("  CaptureCard:{0} preview channel:{1}={2}",ID, m_strPreviewChannel,iChannel);
            if (Previewing && capture!=null)
            {
              try
              {
                SelectChannel(iChannel,true);
              }
              catch(Exception ex)
              {
                Log.Write("  CaptureCard:{0} SelectChannel() failed:{1} {2}",ID,ex.Message, ex.StackTrace);
              }
            }
            else 
            {
              m_iPreviewChannel=iChannel;
            }
          }
        }
        else Log.Write("  CaptureCard:{0} Unknown channel:{1}",ID, m_strPreviewChannel);
      }
    }

    /// <summary>
    /// Turn on/off previewing. You can only turn on previewing when we're not recording
    /// </summary>
    public bool Previewing
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
    /// Start recording a tv program / recording schedule
    /// </summary>
    /// <param name="rec">The recording schedule for this recording</param>
    /// <param name="currentProgram">the tv program to record (may be null)</param>
    /// <param name="iInterval">interval the recording should start before the starttime and end after the stoptime</param>
    public void Record(TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)
    { 
      m_iPreRecordInterval=iPreRecordInterval;
      m_iPostRecordInterval=iPostRecordInterval;
      // get capture format & recording path
      string strRecPath="";
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
      }

      Log.Write("  CaptureCard:{0} start recording:{1}",ID, rec.ToString());
      m_bStopRecording=false;
      m_CurrentProgramRecording=currentProgram;
      m_CurrentTVRecording=rec;
      m_bIsRecording=true;
      Previewing=false;

      if (strRecPath==null) 
      {
        Log.Write("  CaptureCard:{0} Capture failed. no recording path specified in setup",ID);
        m_bIsRecording=false;
        m_CurrentProgramRecording=null;
        m_CurrentTVRecording=null;
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
                                dt.Minute,CaptureFormat);
        }
        else
        {
          DateTime dt=DateTime.Now;
          strName=String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}{7}", 
                                        rec.Channel,rec.Title,
                                        dt.Year,dt.Month,dt.Day,
                                        dt.Hour,
                                        dt.Minute,CaptureFormat);
        }
        
        m_strFilenameEnd=String.Format(@"{0}\{1}",strRecPath, Utils.MakeFileName(strName) );
        m_strFilenameTmp=String.Format(@"{0}\record{1}{2}",strRecPath, ID,CaptureFormat );
        try
        {
          System.IO.File.Delete(m_strFilenameTmp);
        }
        catch (Exception)
        {
        }

        // create new capture
        Log.Write("  CaptureCard:{0} start capture to {1}",ID, m_strFilenameTmp);
        if ( !StartCapture(m_strFilenameTmp, iChannelNr) )
        {
          // create capture failed
          Log.Write("  CaptureCard:{0} cannot start capture",ID);
          m_bIsRecording=false;
          m_CurrentProgramRecording=null;
          m_CurrentTVRecording=null;
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
        m_newRecordedTV = new TVRecorded();        
        m_newRecordedTV.Start=Utils.datetolong(DateTime.Now);
        m_newRecordedTV.Channel=rec.Channel;
        m_newRecordedTV.FileName=m_strFilenameEnd;
        if (currentRunningProgram!=null)
        {
          m_newRecordedTV.Title=currentRunningProgram.Title;
          m_newRecordedTV.Genre=currentRunningProgram.Genre;
          m_newRecordedTV.Description=currentRunningProgram.Description;
        }
        else
        {
          m_newRecordedTV.Title="";
          m_newRecordedTV.Genre="";
          m_newRecordedTV.Description="";
        }
        return;
      }
      catch(Exception ex)
      {
        Log.Write("  CaptureCard:{0} exception during capture:{1} {2}",ID, ex.Message, ex.StackTrace);
      }
      // and stop the capture
      Log.Write("  CaptureCard:{0} stop recording:{1}",ID, rec.ToString());
      StopCapture();
      m_bIsRecording=false;
      m_CurrentProgramRecording=null;
      m_CurrentTVRecording=null;
    }

    /// <summary>
    /// Returns whether the record is currently recording or not
    /// </summary>
    public bool IsRecording
    {
      get { return m_bIsRecording;}
    }

    /// <summary>
    /// This function will stop the current recording
    /// </summary>
    public void StopRecording()
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
    public TVProgram ProgramRecording
    {
      get { return m_CurrentProgramRecording;}
    }
    
    /// <summary>
    /// Returns the current recording schedule which is being recorded
    /// Is only valid when IsRecording returns true
    /// </summary>
    public TVRecording ScheduleRecording
    {
      get { return m_CurrentTVRecording;}
    }


    public void Process()
    {
      // are we recording?
      if (!IsRecording) return; // no, then just return

      //yes, recording still running?
      if ( m_CurrentTVRecording.ShouldRecord(DateTime.Now,m_CurrentProgramRecording,m_iPreRecordInterval, m_iPostRecordInterval) )
      {
        // yes, if user didnt cancel recording then just return
        if (m_bStopRecording==false) return;
      }

      // did use cancel recording?
      if (m_bStopRecording==true)
      {
        // yep, stop it
        m_CurrentTVRecording.Canceled=Utils.datetolong(DateTime.Now);
        TVDatabase.ChangeRecording(ref m_CurrentTVRecording);
        Log.Write("  CaptureCard:{0} user canceled recording:{1}",ID, m_CurrentTVRecording.ToString());
      }

      // recording ended or stopped
      StopCapture();
      // rename the temp. name-> final name
      Log.Write("  CaptureCard:{0} stop capture to {1}",ID, m_strFilenameTmp);
      try
      {
        Log.Write("  CaptureCard:{0} move {1}->{2}",ID, m_strFilenameTmp,m_strFilenameEnd);
        if (System.IO.File.Exists(m_strFilenameEnd))
        {
          System.IO.File.Delete(m_strFilenameEnd);
        }
        System.IO.File.Move(m_strFilenameTmp, m_strFilenameEnd);
      }
      catch(Exception ex)
      {
        Log.Write("  CaptureCard:{0} unable to move file {1}-{2} {3}",ID, m_strFilenameTmp, m_strFilenameEnd,ex.Message);
      }

      m_newRecordedTV.End=Utils.datetolong(DateTime.Now);
      TVDatabase.AddRecordedTV(m_newRecordedTV);
      m_newRecordedTV=null;
      m_bIsRecording=false;
      m_CurrentProgramRecording=null;
      m_CurrentTVRecording=null;
      m_strFilenameTmp="";
      m_strFilenameEnd="";
    }
    

    /// <summary>
    /// return the channel number for a channel name
    /// </summary>
    /// <param name="strChannelName">Channel Name</param>
    /// <returns>Channel number</returns>
    int GetChannelNr(string strChannelName)
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
	}
}
