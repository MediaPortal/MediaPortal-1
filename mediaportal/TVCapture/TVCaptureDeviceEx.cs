#if (UseCaptureCardDefinitions)

using System;
using System.Drawing;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using DShowNET;
using TVCapture;
using DirectX.Capture;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Class which handles recording, viewing & timeshifting for a single tvcapture card
  /// 
	/// Analog TV Tuning Interfaces:
	/// * IAMTVTuner - Control a TV tuner device. 
	/// * IAMTVAudio - Control the audio from a TV tuner. 
	/// * IAMAnalogVideoDecoder - Contains methods for selecting the digitization format, indicating the horizontal lock status, and controlling the time constant on the digitizer phase lock loop (PLL).
	/// * IAMLine21Decoder - Used to control the display of closed captioning. 
	/// * IAMWstDecoder - Used to control the dislay of World Standard Teletext (WST).
	/// </summary>
	[Serializable]
	public class TVCaptureDevice
	{
		string  m_strVideoDevice        = "";
		string  m_strVideoDeviceMoniker = "";//@"@device:pnp:\\?\pci#ven_4444&dev_0016&subsys_88010070&rev_01#3&267a616a&0&60#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\{9b365890-165f-11d0-a195-0020afd156e4}";
		string  m_strAudioDevice			  = "";
		string  m_strVideoCompressor		= "";
		string  m_strAudioCompressor		= "";
		bool    m_bUseForRecording			= false;
		bool    m_bUseForTV							= false;
		bool    m_bSupportsMPEG2				= false;							// #MW# Should be part of card definition??
		bool    m_bIsMCECard						= false;							// #MW# Should be part of card definition??
		bool		m_bIsBDACard						= false;
		Size    m_FrameSize;
		double  m_FrameRate							= 0;
		string  m_strAudioInputPin			= "";
		int     _RecordingLevel					= 100;
		string  m_strFriendlyName				= "";
		string  deviceType              ="";

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

		[NonSerialized] private bool				_mIsCableInput;				// #MW# Should be made serializable...??
		[NonSerialized] private int					_mCountryCode;				// #MW# Should be made serializable...??
		[NonSerialized] private int					_mId;
		[NonSerialized] private State       _mState										= State.None;
    [NonSerialized] private TVRecording _mCurrentTVRecording			= null;
		[NonSerialized] private TVProgram   _mCurrentProgramRecording	= null;
		[NonSerialized] private string			_mTvChannelName						= "";
		[NonSerialized] private int					_mPreRecordInterval				= 0;		// In minutes
		[NonSerialized]	private int					_mPostRecordInterval			= 0;		// In minutes
		[NonSerialized]	private IGraph			_mGraph										= null;
		[NonSerialized]	private TVRecorded	_mNewRecordedTV						= null;
		[NonSerialized]	private bool				_mIsAllocated							= false;
		[NonSerialized]	private DateTime		_mRecordingStartTime;
		[NonSerialized]	private DateTime		_mTimeshiftingStartedTime;

		/// <summary>
		/// #MW#
		/// </summary>
		[NonSerialized] private TVCapture.CaptureCardDefinition _mCaptureCardDefinition	= new CaptureCardDefinition();
		[NonSerialized]	private bool														_mDefinitionLoaded			= false;

		/// <summary>
		/// Default constructor
		/// </summary>
		public TVCaptureDevice()
		{
		}

		/// <summary>
		/// #MW#
		/// </summary>
		public string DeviceId
		{
			get {
				if (_mCaptureCardDefinition==null) return "";
				return _mCaptureCardDefinition.DeviceId;
			}
			set 
			{
				if (_mCaptureCardDefinition==null) return ;
				_mCaptureCardDefinition.DeviceId=value;
			}
		}

		public string DeviceType
		{
			get {return deviceType;}
			set {deviceType=value;}
		}

		/// <summary>
		/// #MW#
		/// </summary>
		public string CaptureName
		{
			get {return _mCaptureCardDefinition.CaptureName;}
			set {_mCaptureCardDefinition.CaptureName=value;}
		}
		/// <summary>
		/// #MW#
		/// </summary>
		public string CommercialName
		{
			get {return _mCaptureCardDefinition.CommercialName;}
			set {_mCaptureCardDefinition.CommercialName=value;}
		}
		/// <summary>
		/// #MW#
		/// </summary>
		public CapabilityDefinition Capabilities
		{
			get {return _mCaptureCardDefinition.Capabilities; }
		}

		/// <summary>
		/// #MW#
		/// </summary>
		public Hashtable TvFilterDefinitions
		{
			get {
				if (_mCaptureCardDefinition==null) return null;
				if (_mCaptureCardDefinition.Tv==null) return null;
				return _mCaptureCardDefinition.Tv.FilterDefinitions; 
			}
		}

		/// <summary>
		/// #MW#
		/// </summary>
		public ArrayList TvConnectionDefinitions
		{
			get 
			{
				if (_mCaptureCardDefinition==null) return null;
				if (_mCaptureCardDefinition.Tv==null) return null;
				return _mCaptureCardDefinition.Tv.ConnectionDefinitions; 
			}
		}
		/// <summary>
		/// #MW#
		/// </summary>
		public InterfaceDefinition TvInterfaceDefinition
		{
			get 
			{
				if (_mCaptureCardDefinition.Tv==null) return null;
				return _mCaptureCardDefinition.Tv.InterfaceDefinition; 
			}
		}

		/// <summary>
		/// #MW#
		/// </summary>
		public Hashtable RadioFilterDefinitions
		{
			get 
			{
				if (_mCaptureCardDefinition.Radio==null) return null;
				return _mCaptureCardDefinition.Radio.FilterDefinitions; }
		}

		/// <summary>
		/// #MW#
		/// </summary>
		public ArrayList RadioConnectionDefinitions
		{
			get {
				if (_mCaptureCardDefinition.Radio==null) return null;
				return _mCaptureCardDefinition.Radio.ConnectionDefinitions; 
			}
		}

		/// <summary>
		/// #MW#
		/// </summary>
		public InterfaceDefinition RadioInterfaceDefinition
		{
			get 
			{
				if (_mCaptureCardDefinition.Radio==null) return null;
				return _mCaptureCardDefinition.Radio.InterfaceDefinition; 
			}
		}

		/// <summary>
		/// #MW#
		/// </summary>
		/// <returns></returns>
		public bool LoadDefinitions()
		{
			if (_mDefinitionLoaded) return (true);
			_mDefinitionLoaded = true;
			
			Log.Write("LoadDefinitions()");
			CaptureCardDefinitions captureCardDefinitions = CaptureCardDefinitions.Instance;
			if (CaptureCardDefinitions.CaptureCards.Count == 0)
			{
				// Load failed!!!
				Log.Write("TVCaptureDevice.LoadDefinitions: No capturecards defined, or load failed");
				return (false);
			}

			if (m_strVideoDeviceMoniker==null) return true;

			// Determine the deviceid "hidden" in the moniker of the capture device and use that to load
			// the definitions of the card... The id is between the first and second "#" character
			string 	 deviceId =m_strVideoDeviceMoniker;
			string[] tmp1			= m_strVideoDeviceMoniker.Split((char[])"#".ToCharArray());
			if (tmp1.Length>=2) 
				deviceId = tmp1[1].ToLower();

			CaptureCardDefinition ccd =null;
			foreach (CaptureCardDefinition cd in CaptureCardDefinitions.CaptureCards)
			{
				if (cd.DeviceId==deviceId && cd.CaptureName == VideoDevice)
				{
					ccd = cd;
					break;
				}
			}
			//
			// If card is unsupported, simply return
			if (_mCaptureCardDefinition==null)
				_mCaptureCardDefinition											 = new CaptureCardDefinition();
			if (ccd == null)
			{
				Log.Write("TVCaptureDevice.LoadDefinitions: CaptureCard {0} NOT supported, no definitions found", m_strVideoDevice);
				return (false);
			}
			_mCaptureCardDefinition.CaptureName          = ccd.CaptureName;
			_mCaptureCardDefinition.CommercialName       = ccd.CommercialName;
			_mCaptureCardDefinition.DeviceId             = ccd.DeviceId.ToLower();

			_mCaptureCardDefinition.Capabilities				 = ccd.Capabilities;
			this.IsMCECard     = _mCaptureCardDefinition.Capabilities.IsMceDevice;
			this.IsBDACard     = _mCaptureCardDefinition.Capabilities.IsBDADevice;
			this.SupportsMPEG2 = _mCaptureCardDefinition.Capabilities.IsMpeg2Device;
			_mCaptureCardDefinition.Capabilities				 = ccd.Capabilities;

			_mCaptureCardDefinition.Tv									 = new DeviceDefinition();
			_mCaptureCardDefinition.Tv.FilterDefinitions = new Hashtable();
			foreach(string filterKey in ccd.Tv.FilterDefinitions.Keys)
			{
				FilterDefinition fd = new FilterDefinition();
				fd.FriendlyName       = ((FilterDefinition)ccd.Tv.FilterDefinitions[filterKey]).FriendlyName;
				fd.Category           = ((FilterDefinition)ccd.Tv.FilterDefinitions[filterKey]).Category;
				fd.CheckDevice        = ((FilterDefinition)ccd.Tv.FilterDefinitions[filterKey]).CheckDevice;
				fd.DSFilter           = null;
				fd.MonikerDisplayName = "";
				_mCaptureCardDefinition.Tv.FilterDefinitions.Add(filterKey, fd);
			}
			_mCaptureCardDefinition.Tv.ConnectionDefinitions = ccd.Tv.ConnectionDefinitions;
			_mCaptureCardDefinition.Tv.InterfaceDefinition   = ccd.Tv.InterfaceDefinition;

			AvailableFilters af = AvailableFilters.Instance;

			// Determine what PnP device the capture device is. This is done very, very simple by extracting
			// the first part of the moniker display name, which contains device specific information
			string[] tmp										 = m_strVideoDeviceMoniker.Split((char[])"{".ToCharArray());
			string 	 captureDeviceDeviceName = tmp[0];

			bool filterFound;

			foreach (string friendlyName in _mCaptureCardDefinition.Tv.FilterDefinitions.Keys)
			{
				FilterDefinition fd = _mCaptureCardDefinition.Tv.FilterDefinitions[friendlyName] as FilterDefinition;
				filterFound         = false;
				foreach (string key in AvailableFilters.Filters.Keys)
				{
					Filter    filter;
					ArrayList al = AvailableFilters.Filters[key] as System.Collections.ArrayList;
					filter    = (Filter)al[0];
					if (filter.Name.Equals(fd.FriendlyName))
					{
						// FriendlyName found. Now check if this name should be checked against a (PnP) device
						// to make sure that we found the right filter...
						if (fd.CheckDevice)
						{
							// Check all filters with same name for capture card device...
							for (int i=0; i < al.Count; i++)
							{
								filter = al[i] as Filter;
								if (filter.MonikerString.IndexOf(captureDeviceDeviceName) > -1)
								{
									// Filter found matching the capture card device!!!!!!!!!!!!!!!
									filterFound = true;
									break;
								}
							} 
						}
						else filterFound = true;

						// For found filter, get the unique name, the moniker display name which contains not only
						// things like the type of device, but also a reference (in case of PnP hardware devices)
						// to the actual device number which makes it possible to distinqiush two identical cards!
						if (filterFound)
						{
							((FilterDefinition)_mCaptureCardDefinition.Tv.FilterDefinitions[friendlyName]).MonikerDisplayName = filter.MonikerString;
						}
					}
				}
				// If no filter found thats in the definitions file, we obviously made a mistake defining it
				// Log the error and return false...
				if (!filterFound)
				{
					Log.Write("TVCaptureDevice.LoadDefinition: Filter {0} not found in definitions file", friendlyName);
					return (false);
				}
			}

			// Same for Radio...
			_mCaptureCardDefinition.Radio									 = new DeviceDefinition();
			_mCaptureCardDefinition.Radio.FilterDefinitions = new Hashtable();
			foreach(string filterKey in ccd.Radio.FilterDefinitions.Keys)
			{
				FilterDefinition fd = new FilterDefinition();
				fd.FriendlyName       = ((FilterDefinition)ccd.Radio.FilterDefinitions[filterKey]).FriendlyName;
				fd.Category           = ((FilterDefinition)ccd.Radio.FilterDefinitions[filterKey]).Category;
				fd.CheckDevice        = ((FilterDefinition)ccd.Radio.FilterDefinitions[filterKey]).CheckDevice;
				fd.DSFilter           = null;
				fd.MonikerDisplayName = "";
				_mCaptureCardDefinition.Radio.FilterDefinitions.Add(filterKey, fd);
			}
			_mCaptureCardDefinition.Radio.ConnectionDefinitions = ccd.Radio.ConnectionDefinitions;
			_mCaptureCardDefinition.Radio.InterfaceDefinition   = ccd.Radio.InterfaceDefinition;

			foreach (string friendlyName in _mCaptureCardDefinition.Radio.FilterDefinitions.Keys)
			{
				FilterDefinition fd = _mCaptureCardDefinition.Radio.FilterDefinitions[friendlyName] as FilterDefinition;
				filterFound         = false;
				foreach (string key in AvailableFilters.Filters.Keys)
				{
					Filter    filter;
					ArrayList al = AvailableFilters.Filters[key] as System.Collections.ArrayList;
					filter    = (Filter)al[0];
					if (filter.Name.Equals(fd.FriendlyName))
					{
						// FriendlyName found. Now check if this name should be checked against a (PnP) device
						// to make sure that we found the right filter...
						if (fd.CheckDevice)
						{
							// Check all filters with same name for capture card device...
							for (int i=0; i < al.Count; i++)
							{
								filter = al[i] as Filter;
								if (filter.MonikerString.IndexOf(captureDeviceDeviceName) > -1)
								{
									// Filter found matching the capture card device!!!!!!!!!!!!!!!
									filterFound = true;
									break;
								}
							} 
						}
						else filterFound = true;

						// For found filter, get the unique name, the moniker display name which contains not only
						// things like the type of device, but also a reference (in case of PnP hardware devices)
						// to the actual device number which makes it possible to distinqiush two identical cards!
						if (filterFound)
						{
							((FilterDefinition)_mCaptureCardDefinition.Radio.FilterDefinitions[friendlyName]).MonikerDisplayName = filter.MonikerString;
						}
					}
				}
				// If no filter found thats in the definitions file, we obviously made a mistake defining it
				// Log the error and return false...
				if (!filterFound)
				{
					Log.Write("TVCaptureDevice.LoadDefinition: Filter {0} not found in definitions file", friendlyName);
					return (false);
				}
			
			Log.Write("LoadDefinitions() done");}

			return (true);
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
		/// #MW#
		/// </summary>
		public bool IsCableInput
		{
			get { return _mIsCableInput; }
			set { _mIsCableInput = value; }
		}

		/// <summary>
		/// #MW#
		/// </summary>
		public int CountryCode
		{
			get { return _mCountryCode; }
			set { _mCountryCode = value; }
		}

    public bool IsMCECard
    {
      get { return m_bIsMCECard; }
      set { m_bIsMCECard = value; }
    }
		public bool IsBDACard
		{
			get { return m_bIsBDACard; }
			set { m_bIsBDACard = value; }
		}
    /// <summary>
    /// Property which indicates if this card has an onboard mpeg2 encoder or not
    /// </summary>
    public bool SupportsMPEG2
    {
      get { return m_bSupportsMPEG2; }
      set { m_bSupportsMPEG2 = value; }
    }

    public string FriendlyName
    {
      get { return m_strFriendlyName;}
      set { m_strFriendlyName=value;}
    }

    /// <summary>
    /// Property to set the frame size
    /// </summary>
    public Size FrameSize
    {
      get { return m_FrameSize; }
      set { m_FrameSize = value; }
    }

    /// <summary>
    /// Property to set the frame size
    /// </summary>
    public double FrameRate
    {
      get { return m_FrameRate; }
      set { m_FrameRate = value; }
    }

    /// <summary>
    /// Property to get/set the recording level
    /// </summary>
    public int RecordingLevel
    {
      get { return _RecordingLevel;}
      set { _RecordingLevel=value;}
    }

    public string AudioInputPin
    {
      get { return m_strAudioInputPin; }
      set { m_strAudioInputPin = value; }
    }

    /// <summary>
    /// property which returns the date&time when recording was started
    /// </summary>
    public DateTime TimeRecordingStarted
    {
      get { return _mRecordingStartTime; }
    }

    /// <summary>
    /// property which returns the date&time when timeshifting was started
    /// </summary>
    public DateTime TimeShiftingStarted
    {
      get { return _mTimeshiftingStartedTime; }
    }

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the TV capture card 
    /// </summary>
    public string VideoDevice
    {
      get { return m_strVideoDevice; }
      set { m_strVideoDevice = value; }
    }
		/// <summary>
		/// Property to get/set the (graphedit) monikername name of the TV capture card 
		/// </summary>
		public string VideoDeviceMoniker
		{
			get { return m_strVideoDeviceMoniker; }
			set { m_strVideoDeviceMoniker = value; }
		}

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the audio capture device 
    /// </summary>
    public string AudioDevice
    {
      get { return m_strAudioDevice; }
      set { m_strAudioDevice = value; }
    }

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the video compressor
    /// </summary>
    public string VideoCompressor
    {
      get { return m_strVideoCompressor; }
      set { m_strVideoCompressor = value; }
    }

    /// <summary>
    /// Property to get/set the (graphedit) filtername name of the audio compressor
    /// </summary>
    public string AudioCompressor
    {
      get { return m_strAudioCompressor; }
      set { m_strAudioCompressor = value; }
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
      set { m_bUseForTV = value; }
    }

    /// <summary>
    /// Property to specify if this card is allocated by other processes
    /// like MyRadio or not.
    /// </summary>
		public bool Allocated
		{
			get { return _mIsAllocated; }
			set 
			{
				_mIsAllocated = value;
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
      set { m_bUseForRecording = value; }
    }
    
    /// <summary>
    /// Property to specify the ID of this card
    /// </summary>
    public int ID
    {
      get { return _mId; }
      set 
      {
        _mId = value;
        _mState = State.Initialized;
      }
    }

    /// <summary>
    /// Property which returns true if this card is currently recording
    /// </summary>
    public bool IsRecording
    {
      get { 
        if (_mState == State.PreRecording) return true;
        if (_mState == State.Recording) return true;
        if (_mState == State.PostRecording) return true;
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
        if (_mState == State.Timeshifting) return true;
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
        return _mCurrentTVRecording;
      }
      set 
      {
        _mCurrentTVRecording=value;
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
        return _mCurrentProgramRecording;
      }
    }

    /// <summary>
    /// Property which returns true when we're in the post-processing stage of a recording
    /// if we're not recording it returns false;
    /// if we're recording but are NOT in the post-processing stage it returns false;
    /// </summary>
    public bool IsPostRecording
    {
      get { return _mState == State.PostRecording; }
    }

    /// <summary>
    /// Propery to get/set the name of the current TV channel. 
    /// If the TV channelname is changed and the card is timeshifting then it will
    /// tune to the newly specified tv channel
    /// </summary>
		public string TVChannel
		{
			get { return _mTvChannelName; }
			set
			{
				if (value==null) 
					value=GetFirstChannel();
				else if (value!=null && value.Length==0)
					value=GetFirstChannel();
        
				if (value.Equals(_mTvChannelName)) return;

				if (!IsRecording)
				{
					_mTvChannelName = value;
					if (_mGraph != null)
					{
						AnalogVideoStandard standard;
						int country;
						int ichannel=GetChannelNr(_mTvChannelName, out standard, out country);
						if (_mGraph.ShouldRebuildGraph(ichannel))
						{
							RebuildGraph();
							return;
						}/*
            if (IsTimeShifting && !View)
            {
              bool bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
              g_Player.Stop();
              RebuildGraph();
              GUIGraphicsContext.IsFullScreenVideo=bFullScreen;
              return;
            }*/
						_mGraph.TuneChannel(standard, ichannel,country);
						if (IsTimeShifting && !View)
						{
							if (g_Player.Playing && g_Player.CurrentFile == Recorder.GetTimeShiftFileName(ID-1))
							{
								GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_FILE_END, 0, 0, 0, 0, 0, null);
								msg.Param1 = 99;
								GUIWindowManager.SendThreadMessage(msg);
							}
						}
					}
				}
			}
		}

    void RebuildGraph()
    {
      Log.Write("Card:{0} rebuild graph",ID);
      State state=_mState;
      if (g_Player.Playing && g_Player.CurrentFile == Recorder.GetTimeShiftFileName(ID-1))
      {
        g_Player.Stop();
      }
              
      StopTimeShifting();
      View=false;
      DeleteGraph();
      CreateGraph();
      if (state==State.Timeshifting) 
      {
        StartTimeShifting();
                
        g_Player.Play(Recorder.GetTimeShiftFileName(ID-1));
      }
      else 
      {
        View=true;
      }
      Log.Write("Card:{0} rebuild graph done",ID);
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
      _mGraph.StopRecording();

			_mNewRecordedTV.End = Utils.datetolong(DateTime.Now);
			TVDatabase.AddRecordedTV(_mNewRecordedTV);
			_mNewRecordedTV = null;

      // cleanup...
      _mCurrentProgramRecording = null;
      _mCurrentTVRecording = null;
      _mPreRecordInterval = 0;
      _mPostRecordInterval = 0;

      // back to timeshifting state
      _mState = State.Timeshifting;
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
    public void Record(TVRecording recording, TVProgram currentProgram, int iPreRecordInterval, int iPostRecordInterval)
    {
      if (_mState != State.Initialized && _mState != State.Timeshifting)
      {
        DeleteGraph();
      }
      if (!UseForRecording) return;

			if (currentProgram != null)
				_mCurrentProgramRecording = currentProgram.Clone();
      _mCurrentTVRecording = recording;
      _mPreRecordInterval = iPreRecordInterval;
      _mPostRecordInterval = iPostRecordInterval;
      _mTvChannelName = recording.Channel;

      Log.Write("Card:{0} record new program on {1}",ID, _mTvChannelName);
      // create sink graph
      if (CreateGraph())
      {
        bool bContinue = false;
        if (_mGraph.SupportsTimeshifting())
        {
          if (StartTimeShifting())
          {
            bContinue = true;
          }
        }
        else 
        {
          bContinue = true;
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
        if (_mCurrentTVRecording != null) 
        {
          if (_mCurrentTVRecording.IsRecordingAtTime(DateTime.Now, _mCurrentProgramRecording, _mPreRecordInterval, _mPostRecordInterval))
          {
            _mState = State.Recording;

            if (!_mCurrentTVRecording.IsRecordingAtTime(DateTime.Now, _mCurrentProgramRecording, _mPreRecordInterval, 0))
            {
              _mState = State.PostRecording;
            }
            if (!_mCurrentTVRecording.IsRecordingAtTime(DateTime.Now, _mCurrentProgramRecording, 0, _mPostRecordInterval))
            {
              _mState = State.PreRecording;
            }
          }
          else
          {
            //recording ended
            StopRecording();
          }
        }
      }
			
			if (_mGraph!=null)
			{
				_mGraph.Process();
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
    public bool CreateGraph()
    {
      if (Allocated) return false;
      if (_mGraph == null)
			{
				LoadContrastGammaBrightnessSettings();
        Log.Write("Card:{0} CreateGraph",ID);
        _mGraph = GraphFactory.CreateGraph(this);
        if (_mGraph == null) return false;
        return _mGraph.CreateGraph();
      }
      return true;
    }

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool DeleteGraph()
    {
      if (_mGraph != null)
      {
				SaveContrastGammaBrightnessSettings();
        Log.Write("Card:{0} DeleteGraph",ID);
        _mGraph.DeleteGraph();
        _mGraph = null;
				GC.Collect();
				GC.Collect();
				GC.Collect();
      }
      _mState = State.Initialized;
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

      Log.Write("Card:{0} start timeshifting :{1}",ID, _mTvChannelName);
      AnalogVideoStandard standard;
			int country;
      int iChannelNr = GetChannelNr(_mTvChannelName, out standard, out country);

			if (_mState == State.Timeshifting) 
			{
				if (_mGraph.GetChannelNumber() != iChannelNr)
				{
          if (!_mGraph.ShouldRebuildGraph(iChannelNr))
          {
            _mTimeshiftingStartedTime = DateTime.Now;
            _mGraph.TuneChannel(standard, iChannelNr,country);
            return true;
          }
				}
        else return true;
			}

      if (_mState != State.Initialized) 
      {
        DeleteGraph();
      }
      if (!CreateGraph()) return false;
      string strRecPath;
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strRecPath = xmlreader.GetValueAsString("capture","recordingpath","");
        strRecPath = Utils.RemoveTrailingSlash(strRecPath);
        if (strRecPath == null || strRecPath.Length == 0) 
        {
          strRecPath = System.IO.Directory.GetCurrentDirectory();
          strRecPath = Utils.RemoveTrailingSlash(strRecPath);
        }
      }
      string strFileName = Recorder.GetTimeShiftFileName(ID-1);

  
      
      Log.Write("Card:{0} timeshift to file:{1}",ID, strFileName);
      bool bResult = _mGraph.StartTimeShifting(country,standard, iChannelNr, strFileName);
      if ( bResult ==true)
      {
        _mTimeshiftingStartedTime = DateTime.Now;
        _mState = State.Timeshifting;
      }
			SetTvSettings();
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
      _mGraph.StopTimeShifting();
      _mState = State.Initialized;
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
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strRecPath = xmlreader.GetValueAsString("capture","recordingpath","");
        strRecPath = Utils.RemoveTrailingSlash(strRecPath);
        if (strRecPath == null || strRecPath.Length == 0) 
        {
          strRecPath = System.IO.Directory.GetCurrentDirectory();
          strRecPath = Utils.RemoveTrailingSlash(strRecPath);
        }
      }

      DateTime dtNow = DateTime.Now.AddMinutes(_mPreRecordInterval);
      TVUtil util = new TVUtil();
      TVProgram currentRunningProgram = null;
      TVProgram prog = util.GetProgramAt(_mTvChannelName, dtNow);
      if (prog != null) currentRunningProgram = prog.Clone();
      util = null;

			DateTime timeProgStart = new DateTime(1971, 11, 6, 20, 0, 0, 0);
      string strName;
      if (currentRunningProgram != null)
      {
        DateTime dt = currentRunningProgram.StartTime;
        strName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}{9}", 
                                currentRunningProgram.Channel, currentRunningProgram.Title, 
                                dt.Year, dt.Month, dt.Day, 
                                dt.Hour, 
                                dt.Minute, 
                                DateTime.Now.Minute, DateTime.Now.Second, 
                                ".dvr-ms");
				timeProgStart = currentRunningProgram.StartTime.AddMinutes(- _mPreRecordInterval);
      }
      else
      {
        DateTime dt = DateTime.Now;
        strName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}{9}", 
                                _mTvChannelName, _mCurrentTVRecording.Title, 
                                dt.Year, dt.Month, dt.Day, 
                                dt.Hour, 
                                dt.Minute, 
                                DateTime.Now.Minute, DateTime.Now.Second, 
                                ".dvr-ms");
      }
      

      string strFileName = String.Format(@"{0}\{1}",strRecPath, Utils.MakeFileName(strName));
      Log.Write("Card:{0} recording to file:{1}",ID, strFileName);

      AnalogVideoStandard standard;
			int country;
      int iChannelNr=GetChannelNr(_mTvChannelName, out standard, out country);

      bool bResult = _mGraph.StartRecording(country,standard,iChannelNr, ref strFileName, bContentRecording, timeProgStart);

			_mNewRecordedTV = new TVRecorded();
			_mNewRecordedTV.Start = Utils.datetolong(DateTime.Now);
			_mNewRecordedTV.Channel = _mTvChannelName;
			_mNewRecordedTV.FileName = strFileName;
			if (currentRunningProgram != null)
			{
				_mNewRecordedTV.Title = currentRunningProgram.Title;
				_mNewRecordedTV.Genre = currentRunningProgram.Genre;
				_mNewRecordedTV.Description = currentRunningProgram.Description;
			}
			else
			{
				_mNewRecordedTV.Title = "";
				_mNewRecordedTV.Genre = "";
				_mNewRecordedTV.Description = "";
			}

      _mRecordingStartTime = DateTime.Now;
      _mState = State.Recording;
			 SetTvSettings();
      return bResult;
    }

    string GetFirstChannel()
    {
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (chan.Number<(int)ExternalInputs.svhs) return chan.Name;
      }
      foreach (TVChannel chan in channels)
      {
        return chan.Name;
      }
      return "";
    }
    /// <summary>
    /// Returns the channel number for a channel name
    /// </summary>
    /// <param name="strChannelName">Channel Name</param>
    /// <returns>Channel number (or 0 if channelname is unknown)</returns>
    /// <remarks>
    /// Channel names and numbers are stored in the TVDatabase
    /// </remarks>
    int GetChannelNr(string strChannelName, out AnalogVideoStandard standard, out int Country)
    { 
      standard=AnalogVideoStandard.None;
			Country=CountryCode;
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (String.Compare(strChannelName, chan.Name, true) == 0)
        {
          if (chan.Number <= 0)
          {
            Log.Write("error TV Channel:{0} has an invalid channel number:{1} (freq:{2})", 
              strChannelName, chan.Number, chan.Frequency);
          }
          standard=chan.TVStandard;
					if (chan.Country>=0) Country=chan.Country;
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
          return (_mGraph.SupportsTimeshifting());
        }
        return false;
      }
    }

    public void Tune(int channel, int CountryCode)
    {
      AnalogVideoStandard standard=AnalogVideoStandard.None;
      if (_mState != State.Viewing) return;
      _mGraph.TuneChannel( standard, channel, CountryCode);
    }

    /// <summary>
    /// Property to turn on/off tv viewing
    /// </summary>
    public bool View
    {
      get
      {
        return (_mState == State.Viewing);
      }
      set
      {
        if (value == false)
        {
          if (View)
          {
            Log.Write("Card:{0} stop viewing :{1}",ID, _mTvChannelName);
            _mGraph.StopViewing();
            DeleteGraph();
          }
        }
        else
        {
          if (View) return;
          if (IsRecording) return;
          DeleteGraph();
          if (CreateGraph())
          {
						int country;
            Log.Write("Card:{0} start viewing :{1}",ID, _mTvChannelName);
            AnalogVideoStandard standard;
            int iChannelNr = GetChannelNr(_mTvChannelName, out standard, out country);
            _mGraph.StartViewing(standard, iChannelNr,country);
						SetTvSettings();
            _mState = State.Viewing;
          }
        }
      }
    }

    public long VideoFrequency()
		{
			if (_mGraph==null) return 0;
      return _mGraph.VideoFrequency();
    }

    public bool SignalPresent()
		{
			if (_mGraph==null) return false;
      return _mGraph.SignalPresent();
    }

		public bool ViewChannel(int channelNumber, int country, AnalogVideoStandard standard)
		{
			if (_mGraph==null)
			{
				if (!CreateGraph()) return false;
			}
			_mGraph.StartViewing(standard, channelNumber,country);
			SetTvSettings();

			return true;
		}

		public PropertyPageCollection PropertyPages 
		{
			get
			{
				if (_mGraph==null) return null;
				return _mGraph.PropertyPages();
			}
		}

		public bool SupportsFrameSize(Size framesize)
		{
			if (_mGraph==null) return false;
			return _mGraph.SupportsFrameSize(framesize);
		}

		public IBaseFilter AudiodeviceFilter
		{
			get 
			{ 
				if (_mGraph==null) return null;
				return _mGraph.AudiodeviceFilter();
			}
		}

		public NetworkType Network
		{
			get
			{
				if (_mGraph==null) return NetworkType.ATSC;
				return _mGraph.Network();
			}
		}
		public void Tune(object tuningObject)
		{
			if (_mGraph==null) return ;
			_mGraph.Tune(tuningObject);
		}
		public void StoreTunedChannels(bool radio, bool tv)
		{
			if (_mGraph==null) return ;
			_mGraph.StoreChannels(radio,tv);
		}
		
		void SetTvSettings()
		{
			int gamma=GUIGraphicsContext.Gamma;
			GUIGraphicsContext.Gamma=-2;
			GUIGraphicsContext.Gamma=-1;
			if (gamma>=0)
				GUIGraphicsContext.Gamma=gamma;
		}

		void LoadContrastGammaBrightnessSettings()
		{
			try
			{
				string filename=String.Format(@"database\card_{0}.xml",m_strFriendlyName);
				using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml(filename))
				{
					int contrast=xmlreader.GetValueAsInt("tv","contrast",-1);
					int brightness=xmlreader.GetValueAsInt("tv","brightness",-1);
					int gamma=xmlreader.GetValueAsInt("tv","gamma",-1);
					int saturation=xmlreader.GetValueAsInt("tv","saturation",-1);
					int sharpness=xmlreader.GetValueAsInt("tv","sharpness",-1);
					GUIGraphicsContext.Contrast		= contrast;
					GUIGraphicsContext.Brightness = brightness;
					GUIGraphicsContext.Gamma			= gamma;
					GUIGraphicsContext.Saturation = saturation;
					GUIGraphicsContext.Sharpness = sharpness;
				}
			}
			catch(Exception)
			{}
		}
		void SaveContrastGammaBrightnessSettings()
		{
			string filename=String.Format(@"database\card_{0}.xml",m_strFriendlyName);
			using(AMS.Profile.Xml   xmlWriter=new AMS.Profile.Xml(filename))
			{
				xmlWriter.SetValue("tv","contrast",GUIGraphicsContext.Contrast);
				xmlWriter.SetValue("tv","brightness",GUIGraphicsContext.Brightness);
				xmlWriter.SetValue("tv","gamma",GUIGraphicsContext.Gamma);
				xmlWriter.SetValue("tv","saturation",GUIGraphicsContext.Saturation);
				xmlWriter.SetValue("tv","sharpness",GUIGraphicsContext.Sharpness);
			}
		}

  }
}  
#endif