using System;
using System.Drawing;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
//using DirectX.Capture;
using MediaPortal.Player;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Class which handles recording, viewing & timeshifting for a single tvcapture card
  /// </summary>
  [Serializable]
  public class TVCaptureDevice
  {
    string        m_strVideoDevice="";
    string        m_strAudioDevice="";
    string        m_strVideoCompressor="";
    string        m_strAudioCompressor="";
    bool          m_bUseForRecording;
    bool          m_bUseForTV;			
    bool          m_bSupportsMPEG2;
    Size          m_FrameSize;
    double        m_FrameRate;
    
    [NonSerialized]
    int           m_iID;

    enum State
    {
      None,
      Initialized,
      Timeshifting,
      PreRecording,
      Recording,
      PostRecording,
      Viewing
    }

    [NonSerialized]
    State         m_eState=State.None;
    
    [NonSerialized]
    TVRecording   m_CurrentTVRecording=null;
    
    [NonSerialized]
    TVProgram     m_CurrentProgramRecording=null;
    
    [NonSerialized]
    string        m_strTVChannel="";
    
    [NonSerialized]
    int           m_iPreRecordInterval=0;
    
    [NonSerialized]
    int           m_iPostRecordInterval=0;
    
    [NonSerialized]
    IGraph      m_graph=null;

    [NonSerialized]
    TVRecorded    m_newRecordedTV = null;

    [NonSerialized]
    bool					m_bAllocated=false;

    [NonSerialized]
    DateTime      m_dtRecordingStartTime;
    [NonSerialized]
    DateTime      m_dtTimeShiftingStarted;

    /// <summary>
    /// Default constructor
    /// </summary>
    public TVCaptureDevice()
    {
    }

    /// <summary>
    /// Will return the filtername of the capture device
    /// </summary>
    /// <returns>filtername of the capture device</returns>
    public override string ToString()
    {
      return m_strVideoDevice;
    }

    /// <summary>
    /// Property which indicates if this card has an onboard mpeg2 encoder or not
    /// </summary>
    public bool SupportsMPEG2
    {
      get { return m_bSupportsMPEG2;}
      set { m_bSupportsMPEG2=value;}
    }

    /// <summary>
    /// Property to set the frame size
    /// </summary>
    public Size FrameSize
    {
      get { return m_FrameSize;}
      set { m_FrameSize=value;}
    }

    /// <summary>
    /// Property to set the frame size
    /// </summary>
    public double FrameRate
    {
      get { return m_FrameRate;}
      set { m_FrameRate=value;}
    }

    /// <summary>
    /// property which returns the date&time when recording was started
    /// </summary>
    public DateTime TimeRecordingStarted
    {
      get { return m_dtRecordingStartTime;}
    }

    /// <summary>
    /// property which returns the date&time when timeshifting was started
    /// </summary>
    public DateTime TimeShiftingStarted
    {
      get { return m_dtTimeShiftingStarted;}
    }

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the TV capture card 
    /// </summary>
    public string VideoDevice
    {
      get { return m_strVideoDevice;}
      set { m_strVideoDevice=value;}
    }

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the audio capture device 
    /// </summary>
    public string AudioDevice
    {
      get { return m_strAudioDevice;}
      set { m_strAudioDevice=value;}
    }

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the video compressor
    /// </summary>
    public string VideoCompressor
    {
      get { return m_strVideoCompressor;}
      set { m_strVideoCompressor=value;}
    }

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the audio compressor
    /// </summary>
    public string AudioCompressor
    {
      get { return m_strAudioCompressor;}
      set { m_strAudioCompressor=value;}
    }


    /// <summary>
    /// Property to specify if this card can be used for TV viewing or not
    /// </summary>
    public bool UseForTV
    {
      get { 
				if (Allocated) return false;
				return m_bUseForTV;
			}
      set { m_bUseForTV=value;}
    }

    /// <summary>
    /// Property to specify if this card is allocated by other processes
    /// like MyRadio or not.
    /// </summary>
		public bool Allocated
		{
			get { return m_bAllocated;}
			set 
			{
				m_bAllocated=value;
			}
		}
    
    /// <summary>
    /// Property to specify if this card can be used for recording or not
    /// </summary>
    public bool UseForRecording
    {
      get { 
				if (Allocated) return false;
				return m_bUseForRecording;
			}
      set { m_bUseForRecording=value;}
    }
    
    /// <summary>
    /// Property to specify the ID of this card
    /// </summary>
    public int ID
    {
      get { return m_iID;}
      set 
      {
        m_iID=value;
        m_eState=State.Initialized;
      }
    }

    /// <summary>
    /// Property which returns true if this card is currently recording
    /// </summary>
    public bool IsRecording
    {
      get { 
        if (m_eState==State.PreRecording) return true;
        if (m_eState==State.Recording) return true;
        if (m_eState==State.PostRecording) return true;
        return false;
      }
    }


    /// <summary>
    /// Property which returns true if this card is currently timeshifting
    /// </summary>
    public bool IsTimeShifting
    {
      get 
      {
        if (IsRecording) return true;
        if (m_eState==State.Timeshifting) return true;
        return false;
      }
    }

    /// <summary>
    /// Property which returns the current TVRecording schedule when its recording
    /// otherwise it returns null
    /// </summary>
    /// <seealso>MediaPortal.TV.Database.TVRecording</seealso>
    public TVRecording CurrentTVRecording
    {
      get 
      { 
        if (!IsRecording) return null;
        return m_CurrentTVRecording;
      }
    }

    /// <summary>
    /// Property which returns the current TVProgram when its recording
    /// otherwise it returns null
    /// </summary>
    /// <seealso>MediaPortal.TV.Database.TVProgram</seealso>
    public TVProgram CurrentProgramRecording
    {
      get { 
        if (IsRecording) return null;
        return m_CurrentProgramRecording;
      }
    }

    /// <summary>
    /// Property which returns true when we're in the post-processing stage of a recording
    /// if we're not recording it returns false;
    /// if we're recording but are NOT in the post-processing stage it returns false;
    /// </summary>
    public bool IsPostRecording
    {
      get { return m_eState==State.PostRecording;}
    }

    /// <summary>
    /// Propery to get/set the name of the current TV channel. 
    /// If the TV channelname is changed and the card is timeshifting then it will
    /// tune to the newly specified tv channel
    /// </summary>
    public string TVChannel
    {
      get { return m_strTVChannel;}
      set
      {
        if (value.Equals(m_strTVChannel)) return;

        if (!IsRecording)
        {
          m_strTVChannel=value;
          if (m_graph!= null)
          {
            m_graph.TuneChannel( GetChannelNr(m_strTVChannel) );
            if (IsTimeShifting && !View)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_FILE_PERCENTAGE,0,0,0,0,0,null);
              msg.Param1=99;
              GUIWindowManager.SendThreadMessage(msg);
            }
          }
        }
      }
    }

    /// <summary>
    /// This method can be used to stop the current recording.
    /// After recording is stopped the card will return to timeshifting mode
    /// </summary>
    public void StopRecording()
    {
      if (!IsRecording) return;

      Log.Write("Card:{0} stop recording",ID);
      // todo : stop recorder
      m_graph.StopRecording();

			m_newRecordedTV.End=Utils.datetolong(DateTime.Now);
			TVDatabase.AddRecordedTV(m_newRecordedTV);
			m_newRecordedTV=null;

      // cleanup...
      m_CurrentProgramRecording = null;
      m_CurrentTVRecording      = null;
      m_iPreRecordInterval      = 0;
      m_iPostRecordInterval     = 0;

      // back to timeshifting state
      m_eState=State.Timeshifting;
    }

    /// <summary>
    /// This method can be used to start a new recording
    /// </summary>
    /// <param name="recording">TVRecording schedule to record</param>
    /// <param name="currentProgram">TVProgram to record</param>
    /// <param name="iPreRecordInterval">Pre record interval</param>
    /// <param name="iPostRecordInterval">Post record interval</param>
    /// <remarks>
    /// The card will start recording live tv to the harddisk and create a new
    /// <see>MediaPortal.TV.Database.TVRecorded</see> record in the TVDatabase 
    /// which contains all details about the new recording like
    /// start-end time, filename, title,description,channel
    /// </remarks>
    /// <seealso>MediaPortal.TV.Database.TVRecorded</seealso>
    /// <seealso>MediaPortal.TV.Database.TVProgram</seealso>
    public void Record(TVRecording recording,TVProgram currentProgram,int iPreRecordInterval,int iPostRecordInterval)
    {
      if (m_eState!=State.Initialized && m_eState!=State.Timeshifting)
      {
        DeleteGraph();
      }
      if (!UseForRecording) return;

			if (currentProgram!=null)
				m_CurrentProgramRecording = currentProgram.Clone();
      m_CurrentTVRecording      = recording;
      m_iPreRecordInterval      = iPreRecordInterval;
      m_iPostRecordInterval     = iPostRecordInterval;
      m_strTVChannel            = recording.Channel;

      Log.Write("Card:{0} record new program on {1}",ID,m_strTVChannel);		
      // create sink graph
      if (CreateGraph())
      {
        bool bContinue=false;
        if (m_graph.SupportsTimeshifting() )
        {
          if (StartTimeShifting())
          {
            bContinue=true;
          }
        }
        else 
        {
          bContinue=true;
        }
        
        if (bContinue)
        {
          // start sink graph
          if (StartRecording(/*recording.IsContentRecording*/true))
          {
          }
        }
      }
      //todo handle errors....
    }

    /// <summary>
    /// Process() method gets called on a regular basis by the Recorder
    /// Here we check if we're currently recording and 
    /// ifso if the recording should be stopped.
    /// </summary>
    public void Process()
    {
      // set postrecording status
      if (IsRecording) 
      {
        if (m_CurrentTVRecording!=null) 
        {
          if ( m_CurrentTVRecording.IsRecordingAtTime(DateTime.Now,m_CurrentProgramRecording,m_iPreRecordInterval, m_iPostRecordInterval) )
          {
            m_eState=State.Recording;

            if ( !m_CurrentTVRecording.IsRecordingAtTime(DateTime.Now,m_CurrentProgramRecording,m_iPreRecordInterval, 0) )
            {
              m_eState=State.PostRecording;
            }
            if ( !m_CurrentTVRecording.IsRecordingAtTime(DateTime.Now,m_CurrentProgramRecording,0, m_iPostRecordInterval) )
            {
              m_eState=State.PreRecording;
            }
          }
          else
          {
            //recording ended
            StopRecording();
          }
        }
      }
    }

    /// <summary>
    /// Method to cleanup any resources and free the card. 
    /// Used by the recorder when its stopping or when external assemblies
    /// like MyRadio want access to the capture card
    /// </summary>
    public void Stop()
    {
      Log.Write("Card:{0} stop",ID);	
      StopRecording();
      StopTimeShifting();
      DeleteGraph();
    }

    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    bool CreateGraph()
    {
      if (Allocated) return false;
      if (m_graph==null)
      {
        Log.Write("Card:{0} CreateGraph",ID);		
        m_graph=GraphFactory.CreateGraph(this);
        if (m_graph==null) return false;
        return m_graph.CreateGraph();
      }
      return true;
    }

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    bool DeleteGraph()
    {
      if (m_graph!=null)
      {
        Log.Write("Card:{0} DeleteGraph",ID);		
        m_graph.DeleteGraph();
        m_graph=null;
				GC.Collect();
				GC.Collect();
				GC.Collect();
      }
      m_eState=State.Initialized;
      return true;
    }

    /// <summary>
    /// Starts timeshifting 
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool StartTimeShifting()
    {
      if (IsRecording) return false;

      Log.Write("Card:{0} start timeshifting :{1}",ID, m_strTVChannel);
			int iChannelNr=GetChannelNr(m_strTVChannel);

			if (m_eState==State.Timeshifting) 
			{
				if (m_graph.GetChannelNumber()!=iChannelNr)
				{
          m_dtTimeShiftingStarted=DateTime.Now;
					m_graph.TuneChannel(iChannelNr);
				}
				return true;
			}

      if (m_eState!=State.Initialized) 
      {
        DeleteGraph();
      }
      CreateGraph();
      string strRecPath;
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
        strRecPath=Utils.RemoveTrailingSlash(strRecPath);
        if (strRecPath==null||strRecPath.Length==0) 
        {
          strRecPath=System.IO.Directory.GetCurrentDirectory();
          strRecPath=Utils.RemoveTrailingSlash(strRecPath);
        }
      }
      string strFileName=String.Format(@"{0}\live.tv",strRecPath);

      // it could be that another card is already timeshifting and therefore has
      // created a live.tv file. ifso, then we create a new one called live[id].tv 
      if (System.IO.File.Exists(strFileName))
      {
        strFileName=String.Format(@"{0}\live{1}.tv",strRecPath,ID);
      }
      
      Log.Write("Card:{0} timeshift to file:{1}",ID, strFileName);
      bool bResult=m_graph.StartTimeShifting(iChannelNr, strFileName);
      m_dtTimeShiftingStarted=DateTime.Now;
      m_eState=State.Timeshifting;
      return bResult;
    }

    /// <summary>
    /// Stops timeshifting and cleans up the timeshifting files
    /// </summary>
    /// <returns>boolean indicating if timeshifting is stopped or not</returns>
    /// <remarks>
    /// Graph should be timeshifting 
    /// </remarks>
    public bool StopTimeShifting()
    {
      if (!IsTimeShifting) return false;

      //stopping timeshifting will also remove the live.tv file 
      Log.Write("Card:{0} stop timeshifting",ID);
      m_graph.StopTimeShifting();
      m_eState=State.Initialized;
      return true;
    }

    /// <summary>
    /// Starts recording live TV to a file
    /// <param name="bContentRecording">Specifies whether a content or reference recording should be made</param>
    /// </summary>
    /// <returns>boolean indicating if recorded is started or not</returns> 
    /// <remarks>
    /// Graph should be timeshifting. When Recording is started the graph is still 
    /// timeshifting
    /// 
    /// A content recording will start recording from the moment this method is called
    /// and ignores any data left/present in the timeshifting buffer files
    /// 
    /// A reference recording will start recording from the moment this method is called
    /// It will examine the timeshifting files and try to record as much data as is available
    /// from the start of the current tv program till the moment recording is stopped again
    /// </remarks>
     bool StartRecording(bool bContentRecording)
     {
      Log.Write("Card:{0} start recording content:{1}",ID, bContentRecording);
      string strRecPath;
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
        strRecPath=Utils.RemoveTrailingSlash(strRecPath);
        if (strRecPath==null||strRecPath.Length==0) 
        {
          strRecPath=System.IO.Directory.GetCurrentDirectory();
          strRecPath=Utils.RemoveTrailingSlash(strRecPath);
        }
      }

      DateTime dtNow = DateTime.Now.AddMinutes(m_iPreRecordInterval);
      TVUtil util=new TVUtil();
      TVProgram currentRunningProgram=null;
      TVProgram prog=util.GetProgramAt(m_strTVChannel, dtNow);
      if (prog!=null) currentRunningProgram=prog.Clone();
      util=null;

			DateTime timeProgStart=new DateTime(1971,11,6,20,0,0,0);
      string strName;
      if (currentRunningProgram!=null)
      {
        DateTime dt=currentRunningProgram.StartTime;
        strName=String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}{9}", 
                                currentRunningProgram.Channel,currentRunningProgram.Title,
                                dt.Year,dt.Month,dt.Day,
                                dt.Hour,
                                dt.Minute,
                                DateTime.Now.Minute,DateTime.Now.Second,
                                ".dvr-ms");
				timeProgStart=currentRunningProgram.StartTime.AddMinutes(-m_iPreRecordInterval);
      }
      else
      {
        DateTime dt=DateTime.Now;
        strName=String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}{9}", 
                                m_strTVChannel,m_CurrentTVRecording.Title,
                                dt.Year,dt.Month,dt.Day,
                                dt.Hour,
                                dt.Minute,
                                DateTime.Now.Minute,DateTime.Now.Second,
                                ".dvr-ms");
      }
      

      string strFileName=String.Format(@"{0}\{1}",strRecPath, Utils.MakeFileName(strName) );
      Log.Write("Card:{0} recording to file:{1}",ID, strFileName);
      bool bResult=m_graph.StartRecording(ref strFileName, bContentRecording,timeProgStart);

			m_newRecordedTV = new TVRecorded();        
			m_newRecordedTV.Start=Utils.datetolong(DateTime.Now);
			m_newRecordedTV.Channel=m_strTVChannel;
			m_newRecordedTV.FileName=strFileName;
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

      m_dtRecordingStartTime=DateTime.Now;
      m_eState=State.Recording;
      return bResult;
    }

    /// <summary>
    /// Returns the channel number for a channel name
    /// </summary>
    /// <param name="strChannelName">Channel Name</param>
    /// <returns>Channel number (or 0 if channelname is unknown)</returns>
    /// <remarks>
    /// Channel names and numbers are stored in the TVDatabase
    /// </remarks>
    int GetChannelNr(string strChannelName)
    { 
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (String.Compare(strChannelName,chan.Name,true)==0)
        {
          if (chan.Number<=0)
          {
            Log.Write("error TV Channel:{0} has an invalid channel number:{1} (freq:{2})", 
              strChannelName, chan.Number,chan.Frequency); 
          }
          return chan.Number;
        }
      }
      return 0;
    }


    /// <summary>
    /// Property indiciating if the card supports timeshifting
    /// </summary>
    /// <returns>boolean indiciating if the graph supports timeshifting</returns>
    public bool SupportsTimeShifting
    {
      get
      {
        if (CreateGraph())
        {
          return (m_graph.SupportsTimeshifting() );
        }
        return false;
      }
    }

    /// <summary>
    /// Property to turn on/off tv viewing
    /// </summary>
    public bool View
    {
      get
      {
        return (m_eState==State.Viewing);
      }
      set
      {
        if (value==false)
        {
          if (View)
          {
            Log.Write("Card:{0} stop viewing :{1}",ID, m_strTVChannel);
            m_graph.StopViewing();
            DeleteGraph();
          }
        }
        else
        {
          if (View) return;
          if (IsRecording) return;
          DeleteGraph();
          if ( CreateGraph() )
          {
            Log.Write("Card:{0} start viewing :{1}",ID, m_strTVChannel);
            int iChannelNr=GetChannelNr(m_strTVChannel);
            m_graph.StartViewing(iChannelNr);
            m_eState=State.Viewing;
          }
        }
      }
    }
  }
}  
