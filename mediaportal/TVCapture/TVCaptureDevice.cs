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
  /// </summary>
  [Serializable]
  public class TVCaptureDevice
  {
		string        m_strVideoDevice="";
    bool          m_bUseForRecording;
    bool          m_bUseForTV;			
    
    [NonSerialized]
    int           m_iID;

    enum State
    {
      None,
      Initialized,
      Timeshifting,
      PreRecording,
      Recording,
      PostRecording
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
    SinkGraph     m_graph=null;

		[NonSerialized]
		TVRecorded    m_newRecordedTV = null;

		[NonSerialized]
		bool					m_bAllocated=false;

    public TVCaptureDevice()
    {
    }

		public override string ToString()
		{
			return m_strVideoDevice;
		}

    /// <summary>
    /// Property to get/set the full name of the TV capture card 
    /// </summary>
    public string VideoDevice
    {
      get { return m_strVideoDevice;}
      set { m_strVideoDevice=value;}
    }


    /// <summary>
    /// Property to specify if this card should be used for TV viewing or not
    /// </summary>
    public bool UseForTV
    {
      get { 
				if (Allocated) return false;
				return m_bUseForTV;
			}
      set { m_bUseForTV=value;}
    }
		public bool Allocated
		{
			get { return m_bAllocated;}
			set 
			{
				m_bAllocated=value;
			}
		}
    
    /// <summary>
    /// Property to specify if this card should be used for recording or not
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
    /// Property which returns true if this card is recording
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
    /// Propery which returns the current TV channel
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
          if (IsTimeShifting)
          {
            m_graph.TuneChannel( GetChannelNr(m_strTVChannel) );
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_FILE_PERCENTAGE,0,0,0,0,0,null);
						msg.Param1=99;
						GUIWindowManager.SendThreadMessage(msg);
          }
        }
      }
    }

    /// <summary>
    /// This method can be used to stop the current recording
    /// </summary>
    public void StopRecording()
    {
      if (!IsRecording) return;

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
    public void Record(TVRecording recording,TVProgram currentProgram,int iPreRecordInterval,int iPostRecordInterval)
    {
      if (m_eState!=State.Initialized && m_eState!=State.Timeshifting) return;
      if (!UseForRecording) return;

			if (currentProgram!=null)
				m_CurrentProgramRecording = currentProgram.Clone();
      m_CurrentTVRecording      = recording;
      m_iPreRecordInterval      = iPreRecordInterval;
      m_iPostRecordInterval     = iPostRecordInterval;
      m_strTVChannel            = recording.Channel;
		
      // create sink graph
      if (CreateGraph())
      {
        if (StartTimeShifting())
        {
          // start sink graph
          if (StartRecording(recording.IsContentRecording))
          {
          }
        }
      }
      //todo handle errors....
    }

    /// <summary>
    /// Process() method gets called on a regular basis by the Recorder
    /// Here we check if we're recording and ifso if the recording should be stopped
    /// </summary>
    public void Process()
    {
      // set postrecording status
      if (IsRecording) 
      {
        if (m_CurrentTVRecording!=null) 
        {
          if ( m_CurrentTVRecording.IsRecording(DateTime.Now,m_CurrentProgramRecording,m_iPreRecordInterval, m_iPostRecordInterval) )
          {
            m_eState=State.Recording;

            if ( !m_CurrentTVRecording.IsRecording(DateTime.Now,m_CurrentProgramRecording,m_iPreRecordInterval, 0) )
            {
              m_eState=State.PostRecording;
            }
            if ( !m_CurrentTVRecording.IsRecording(DateTime.Now,m_CurrentProgramRecording,0, m_iPostRecordInterval) )
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
    /// Method to cleanup any resources. Used by the recorder when its stopping
    /// </summary>
    public void Stop()
    {
      StopRecording();
      StopTimeShifting();
      DeleteGraph();
    }

    bool CreateGraph()
    {
      if (m_graph==null)
      {
        int iTunerCountry=31;
        string strTunerType="Antenna";
        using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          strTunerType=xmlreader.GetValueAsString("capture","tuner","Antenna");
          iTunerCountry=xmlreader.GetValueAsInt("capture","country",31);
        }
        bool bCable=false;
        if (!strTunerType.Equals("Antenna")) bCable=true;

        m_graph=new SinkGraph(iTunerCountry,bCable,m_strVideoDevice);
        return m_graph.CreateGraph();
      }
      return true;
    }

    bool DeleteGraph()
    {
      if (m_graph!=null)
      {
        m_graph.DeleteGraph();
        m_graph=null;
				GC.Collect();
				GC.Collect();
				GC.Collect();
      }
      m_eState=State.Initialized;
      return true;
    }

    public bool StartTimeShifting()
    {
			int iChannelNr=GetChannelNr(m_strTVChannel);

			if (m_eState==State.Timeshifting) 
			{
				if (m_graph.ChannelNumber!=iChannelNr)
				{
					m_graph.TuneChannel(iChannelNr);
				}
				return true;
			}

      if (m_eState!=State.Initialized) return false;
      CreateGraph();
      string strRecPath;
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
        strRecPath=Utils.RemoveTrailingSlash(strRecPath);
      }
      string strFileName=String.Format(@"{0}\live.tv",strRecPath);
    
      bool bResult=m_graph.StartTimeShifting(iChannelNr, strFileName);
      m_eState=State.Timeshifting;
      return bResult;
    }

    public bool StopTimeShifting()
    {
      if (!IsTimeShifting) return false;
      m_graph.StopTimeShifting();
      m_eState=State.Initialized;
      return true;
    }

     bool StartRecording(bool bContentRecording)
     {
      string strRecPath;
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
        strRecPath=Utils.RemoveTrailingSlash(strRecPath);
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
        strName=String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}{7}", 
          currentRunningProgram.Channel,currentRunningProgram.Title,
          dt.Year,dt.Month,dt.Day,
          dt.Hour,
          dt.Minute,".sbe");
				timeProgStart=currentRunningProgram.StartTime.AddMinutes(-m_iPreRecordInterval);
      }
      else
      {
        DateTime dt=DateTime.Now;
        strName=String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}{7}", 
          m_strTVChannel,m_CurrentTVRecording.Title,
          dt.Year,dt.Month,dt.Day,
          dt.Hour,
          dt.Minute,".sbe");
      }
      

      string strFileName=String.Format(@"{0}\{1}",strRecPath, Utils.MakeFileName(strName) );
      bool bResult=m_graph.StartRecording(strFileName, bContentRecording,timeProgStart);

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

      m_eState=State.Recording;
      return bResult;
    }

    /// <summary>
    /// return the channel number for a channel name
    /// </summary>
    /// <param name="strChannelName">Channel Name</param>
    /// <returns>Channel number</returns>
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

  }
}  
