#if (UseCaptureCardDefinitions)

using System;
using System.IO;
using System.Management;
using System.Drawing;
using System.Collections;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Video.Database;
using MediaPortal.Player;
using DShowNET;
using TVCapture;
using DirectX.Capture;
using Toub.MediaCenter.Dvrms.Metadata;
using System.Threading;
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
	class RecordingFileInfo : IComparable
	{
		public string   filename;
		public FileInfo info;


		#region IComparable Members

		public int CompareTo(object obj)
		{
			RecordingFileInfo fi=(RecordingFileInfo)obj;
			if (info.CreationTime < fi.info.CreationTime) return -1;
			if (info.CreationTime > fi.info.CreationTime) return 1;
			return 0;
		}

		#endregion
	}
 
	[Serializable]
	public class TVCaptureDevice
	{
		class RecordingFinished
		{
			public string    fileName=String.Empty;
		}
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
		int     priority=1;
		string  m_strRecordingPath="";
		int     m_iMaxSizeLimit=50;
		bool    m_bDeleteOnLowDiskspace=false;
		int     m_iQuality=-1;

		enum State
		{
			None, 
			Initialized, 
			Timeshifting, 
			PreRecording, 
			Recording, 
			PostRecording, 
			Viewing,
			Radio
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
		[NonSerialized] private string      radioStationName="";
		[NonSerialized] private ArrayList m_finishedRecordings=new ArrayList();

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
			get 
			{
				if (_mCaptureCardDefinition==null) return "";
				return _mCaptureCardDefinition.DeviceId;
			}
			set 
			{
				if (_mCaptureCardDefinition==null) return ;
				_mCaptureCardDefinition.DeviceId=value;
			}
		}

		public string RecordingPath
		{
			get 
			{
				m_strRecordingPath = Utils.RemoveTrailingSlash(m_strRecordingPath);
				if (m_strRecordingPath == null || m_strRecordingPath.Length == 0) 
				{
					m_strRecordingPath = System.IO.Directory.GetCurrentDirectory();
					m_strRecordingPath = Utils.RemoveTrailingSlash(m_strRecordingPath);
				}
				return m_strRecordingPath ;
			}
			set {m_strRecordingPath=value;}
		}

		public int Quality
		{
			get { return m_iQuality;}
			set { m_iQuality=value;}
		}
		public int MaxSizeLimit
		{
			get { return m_iMaxSizeLimit;}
			set { m_iMaxSizeLimit=value;}
		}

		public bool DeleteOnLowDiskspace
		{
			get { return m_bDeleteOnLowDiskspace;}
			set { m_bDeleteOnLowDiskspace=value;}
		}

		public int Priority
		{
			get { return priority;}
			set { priority=value;}
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
			get 
			{
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
			get 
			{
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
		
		public int FindInstance(string monikerName)
		{
			Log.Write("    FindInstance:{0}", monikerName);
			
			int pos1=monikerName.IndexOf("#");
			int pos2=monikerName.LastIndexOf("#");
			string left=monikerName.Substring(0,pos1);
			string mid=monikerName.Substring(pos1+1 , (pos2-pos1)-1 );
			mid=mid.Replace("#","/");
			string right=monikerName.Substring(pos2+1);
			string registryKeyName=left+@"\"+mid+@"\"+right;

			if (registryKeyName.StartsWith(@"@device:pnp:\\?\"))
				registryKeyName=registryKeyName.Substring(@"@device:pnp:\\?\".Length);
			registryKeyName=@"SYSTEM\CurrentControlSet\Enum\"+registryKeyName;
			Log.Write("      key:{0}", registryKeyName);
			RegistryKey hklm = Registry.LocalMachine;
			RegistryKey subkey=hklm.OpenSubKey(registryKeyName,false);
			if (subkey!=null)
			{
				string serviceName=(string)subkey.GetValue("Service");
				Log.Write("        serviceName:{0}", serviceName);
				registryKeyName=@"SYSTEM\CurrentControlSet\Services\"+serviceName+@"\Enum";
				Log.Write("        key:{0}", registryKeyName);
				subkey=hklm.OpenSubKey(registryKeyName,false);
				Int32 count=(Int32)subkey.GetValue("Count");
				Log.Write("        Number of cards:{0}", count);
				for (int i=0; i < count; i++)
				{
					string moniker=(string)subkey.GetValue(i.ToString());
					moniker=moniker.Replace(@"\", "#");
					moniker=moniker.Replace(@"/", "#");
					Log.Write("          card#{0}={1}", i,moniker);
				}
				for (int i=0; i < count; i++)
				{
					string moniker=(string)subkey.GetValue(i.ToString());
					moniker=moniker.Replace(@"\", "#");
					moniker=moniker.Replace(@"/", "#");
					if (monikerName.ToLower().IndexOf(moniker.ToLower()) >=0)
					{
						Log.Write("        using card:#{0}", i);
						subkey.Close();
						hklm.Close();
						return i;
					}
				}
				subkey.Close();
			}
			hklm.Close();
			return -1;
		}

		public string FindUniqueFilter(string monikerName, int instance)
		{
			Log.Write("    FindUniqueFilter:card#{0} filter:{1}", instance,monikerName);
			
			int pos1=monikerName.IndexOf("#");
			int pos2=monikerName.LastIndexOf("#");
			string left=monikerName.Substring(0,pos1);
			string mid=monikerName.Substring(pos1+1 , (pos2-pos1)-1 );
			mid=mid.Replace("#","/");
			string right=monikerName.Substring(pos2+1);
			string registryKeyName=left+@"\"+mid+@"\"+right;

			if (registryKeyName.StartsWith(@"@device:pnp:\\?\"))
				registryKeyName=registryKeyName.Substring(@"@device:pnp:\\?\".Length);
			
			registryKeyName=@"SYSTEM\CurrentControlSet\Enum\"+registryKeyName;
			Log.Write("        key:{0}", registryKeyName);
			RegistryKey hklm = Registry.LocalMachine;
			RegistryKey subkey=hklm.OpenSubKey(registryKeyName,false);
			if (subkey!=null)
			{
				string serviceName=(string)subkey.GetValue("Service");
				Log.Write("        serviceName:{0}", serviceName);
				registryKeyName=@"SYSTEM\CurrentControlSet\Services\"+serviceName+@"\Enum";
				Log.Write("        key:{0}", registryKeyName);
				subkey=hklm.OpenSubKey(registryKeyName,false);
				Int32 count=(Int32)subkey.GetValue("Count");
				Log.Write("        filters available:{0}", count);
				for (int i=0; i < count;++i)
				{
					string moniker=(string)subkey.GetValue(instance.ToString());
					moniker=moniker.Replace(@"\", "#");
					moniker=moniker.Replace(@"/", "#");
					Log.Write("          filter#:{0}={1}",instance,moniker);
				}
				string monikerToUse=(string)subkey.GetValue(instance.ToString());
				monikerToUse=monikerToUse.Replace(@"\", "#");
				monikerToUse=monikerToUse.Replace(@"/", "#");
				Log.Write("        using filter #:{0}={1}",instance,monikerToUse);
				subkey.Close();
				hklm.Close();
				return monikerToUse;
			}
			hklm.Close();
			return String.Empty;
		}

		bool FilterBelongsToDevice(Filter filter, string deviceInstance)
		{
			Log.Write("FilterBelongsToFilter device={0}",deviceInstance);
			Log.Write("name:{0}",filter.Name);
			Log.Write("moniker:{0}",filter.MonikerString);

			int p1=filter.MonikerString.IndexOf("{");
			int p2=filter.MonikerString.IndexOf("}");
			string classid=filter.MonikerString.Substring(p1,(p2-p1)+1);

			string registryKeyName=String.Format(@"SYSTEM\CurrentControlSet\Control\DeviceClasses\{0}", classid);
			Log.Write("key:{0}", registryKeyName);
			RegistryKey hklm = Registry.LocalMachine;
			RegistryKey subkey=hklm.OpenSubKey(registryKeyName,false);
			if (subkey!=null)
			{
				string[] subkeynames=subkey.GetSubKeyNames();
				for (int i=0; i < subkeynames.Length;++i)
				{
					Log.Write("  {0}", subkeynames[i]);
					registryKeyName=String.Format(@"SYSTEM\CurrentControlSet\Control\DeviceClasses\{0}\{1}", classid,subkeynames[i]);
					subkey=hklm.OpenSubKey(registryKeyName,false);
					string instance=(string)subkey.GetValue("DeviceInstance");
					
					instance=instance.Replace(@"\", "#");
					instance=instance.Replace(@"/", "#");
					Log.Write("    {0}", instance);
					if (deviceInstance.ToLower().IndexOf(instance.ToLower()) >=0 )
					{
						//found
						subkey.Close();
						hklm.Close();
						Log.Write("   found");
						return true;
					}
				}
				subkey.Close();
			}
			hklm.Close();
			return false;
		}

		/// <summary>
		/// #MW#
		/// </summary>
		/// <returns></returns>
		public bool LoadDefinitions()
		{
			if (_mDefinitionLoaded) return (true);
			_mDefinitionLoaded = true;
			
			Log.WriteFile(Log.LogType.Capture,"LoadDefinitions() card:{0}",ID);
			CaptureCardDefinitions captureCardDefinitions = CaptureCardDefinitions.Instance;
			if (CaptureCardDefinitions.CaptureCards.Count == 0)
			{
				// Load failed!!!
				Log.WriteFile(Log.LogType.Capture," No capturecards defined, or load failed");
				return (false);
			}

			if (m_strVideoDeviceMoniker==null) 
			{
				Log.WriteFile(Log.LogType.Capture," No video device moniker specified");
				return true;
			}

			// Determine the deviceid "hidden" in the moniker of the capture device and use that to load
			// the definitions of the card... The id is between the first and second "#" character
			// example:
			// @device:pnp:\\?\pci#ven_4444&dev_0016&subsys_40090070&rev_01#4&2e98101c&0&68f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\hauppauge wintv pvr pci ii capturez
			string 	 deviceId =m_strVideoDeviceMoniker;
			string[] tmp1			= m_strVideoDeviceMoniker.Split((char[])"#".ToCharArray());
			if (tmp1.Length>=2) 
				deviceId = tmp1[1].ToLower();

			CaptureCardDefinition ccd =null;
			foreach (CaptureCardDefinition cd in CaptureCardDefinitions.CaptureCards)
			{
				if (cd.DeviceId.IndexOf(deviceId)==0 && cd.CaptureName == VideoDevice)
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
				Log.WriteFile(Log.LogType.Capture,true," CaptureCard {0} NOT supported, no definitions found", m_strVideoDevice);
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
			int Instance=-1;

			AvailableFilters af = AvailableFilters.Instance;

			// Determine what PnP device the capture device is. This is done very, very simple by extracting
			// the first part of the moniker display name, which contains device specific information
			// @device:pnp:\\?\pci#ven_4444&dev_0016&subsys_40090070&rev_01#4&2e98101c&0&68f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\hauppauge wintv pvr pci ii capture
			string 	 captureDeviceDeviceName = m_strVideoDeviceMoniker;
			int pos = captureDeviceDeviceName.LastIndexOf("#");
			if (pos>=0)captureDeviceDeviceName = captureDeviceDeviceName.Substring(0,pos);
			Log.WriteFile(Log.LogType.Capture," video device moniker   :{0}", m_strVideoDeviceMoniker);
			Log.WriteFile(Log.LogType.Capture," captureDeviceDeviceName:{0}", captureDeviceDeviceName);

			//Instance = FindInstance(captureDeviceDeviceName);
			//Log.WriteFile(Log.LogType.Capture," Using card#{0}", Instance);
			//for each video filter we need
			foreach (string friendlyName in _mCaptureCardDefinition.Tv.FilterDefinitions.Keys)
			{
				FilterDefinition fd = _mCaptureCardDefinition.Tv.FilterDefinitions[friendlyName] as FilterDefinition;
				bool filterFound         = false;
				Log.WriteFile(Log.LogType.Capture,"  filter {0}={1}",friendlyName,fd.FriendlyName);

				//for each directshow filter present
				foreach (string key in AvailableFilters.Filters.Keys)
				{
					Filter    filter;
					ArrayList al = AvailableFilters.Filters[key] as System.Collections.ArrayList;
					filter    = (Filter)al[0];
					// if directshow filter name == video filter name
					if (filter.Name.Equals(fd.FriendlyName))
					{
						// FriendlyName found. Now check if this name should be checked against a (PnP) device
						// to make sure that we found the right filter...
						if (fd.CheckDevice)
						{	
							for (int filterInst=0; filterInst < al.Count;++filterInst)
							{
								filter=al[filterInst] as Filter;
								if (FilterBelongsToDevice(filter,captureDeviceDeviceName))
								{
									filterFound=true;
									break;
								}
							}
							/*
							filter=al[0] as Filter;
							string 	 filterMoniker = filter.MonikerString;
							int posTmp = filterMoniker.LastIndexOf("#");
							if (posTmp>=0)filterMoniker = filterMoniker.Substring(0,posTmp);

							string moniker=FindUniqueFilter(filterMoniker,Instance);
							for (int filterInst=0; filterInst < al.Count;++filterInst)
							{
								filter=al[filterInst] as Filter;
								if (filter.MonikerString.ToLower().IndexOf(moniker.ToLower())>=0)
								{
									filterFound = true;
									break;
								}
							}*/
						
							if (!filterFound)
							{
								Log.WriteFile(Log.LogType.Capture,true,"  ERROR Cannot find unique filter for filter:{0}", filter.Name);
							}
							else
							{
								Log.WriteFile(Log.LogType.Capture,"    Found {0}={1}", filter.Name, filter.MonikerString);
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
					}//if (filter.Name.Equals(fd.FriendlyName))
				}//foreach (string key in AvailableFilters.Filters.Keys)
				// If no filter found thats in the definitions file, we obviously made a mistake defining it
				// Log the error and return false...
				if (!filterFound)
				{
					Log.WriteFile(Log.LogType.Capture,true,"  Filter {0} not found in definitions file", friendlyName);
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

			//Log.WriteFile(Log.LogType.Capture,"TVCaptureDevice.LoadDefinition() check radio filters");
			foreach (string friendlyName in _mCaptureCardDefinition.Radio.FilterDefinitions.Keys)
			{
				Log.WriteFile(Log.LogType.Capture,"TVCaptureDevice.LoadDefinition()   radio filter:{0}",friendlyName);
				FilterDefinition fd = _mCaptureCardDefinition.Radio.FilterDefinitions[friendlyName] as FilterDefinition;
				bool filterFound         = false;
				foreach (string key in AvailableFilters.Filters.Keys)
				{
					Filter    filter;
					ArrayList al = AvailableFilters.Filters[key] as System.Collections.ArrayList;
					filter    = (Filter)al[0];
					if (filter.Name.Equals(fd.FriendlyName))
					{
						// FriendlyName found. Now check if this name should be checked against a (PnP) device
						// to make sure that we found the right filter...
						if (fd.CheckDevice && al.Count>1)
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
					Log.WriteFile(Log.LogType.Capture,true,"TVCaptureDevice.LoadDefinition: Filter {0} not found in definitions file", friendlyName);
					return (false);
				}
			
				Log.WriteFile(Log.LogType.Capture,"LoadDefinitions() card:{0} done",ID);}

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
			get 
			{ 
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
			get 
			{ 
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
			get 
			{ 
				if (_mState == State.PreRecording) return true;
				if (_mState == State.Recording) return true;
				if (_mState == State.PostRecording) return true;
				return false;
			}
		}


		/// <summary>
		/// Property which returns true if this card is currently has a teletext
		/// </summary>
		public bool HasTeletext
		{
			get 
			{
				if (_mGraph==null) return false;
				return _mGraph.HasTeletext();
			}
		}

		/// <summary>
		/// Property which returns the available audio languages
		/// </summary>
		public ArrayList GetAudioLanguageList()
		{
			if (_mGraph==null) return null;
			return _mGraph.GetAudioLanguageList();			
		}

		/// <summary>
		/// Property which gets the current audio language
		/// </summary>
		public int GetAudioLanguage()
		{
			if (_mGraph==null) return -1;
			return _mGraph.GetAudioLanguage();			
		}

		/// <summary>
		/// Property which sets the new audio language
		/// </summary>
		public void SetAudioLanguage(int audioPid)
		{
			if (_mGraph==null) return;
			_mGraph.SetAudioLanguage(audioPid);			
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
			get 
			{ 
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
		public bool IsRadio
		{
			get { return _mState == State.Radio; }
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
					bool bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
					_mTvChannelName = value;
					if (_mGraph != null)
					{
						TVChannel channel=GetChannel(_mTvChannelName);
						if (_mGraph.ShouldRebuildGraph(channel.Number))
						{
							RebuildGraph();
							// for ss2: restore full screen
							if(m_strVideoDevice=="B2C2 MPEG-2 Source")
								GUIGraphicsContext.IsFullScreenVideo=bFullScreen;
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
						

						_mGraph.TuneChannel(channel);
						if (IsTimeShifting && !View)
						{
							if (g_Player.Playing && g_Player.CurrentFile == Recorder.GetTimeShiftFileName(ID-1))
							{
								double position=g_Player.CurrentPosition;
								double duration=g_Player.Duration;
								if (position < duration-1d)
								{
									g_Player.SeekAbsolute(g_Player.Duration);
								}
							}
						}
					}
				}
			}
		}


		void RebuildGraph()
		{
			Log.WriteFile(Log.LogType.Capture,"Card:{0} rebuild graph",ID);
			State state=_mState;
			if (g_Player.Playing && g_Player.CurrentFile == Recorder.GetTimeShiftFileName(ID-1))
			{
				Log.WriteFile(Log.LogType.Capture,"TVCaptureDevice.Rebuildgraph() stop media");
				g_Player.Stop();
			}
              
			StopTimeShifting();
			View=false;
			DeleteGraph();
			CreateGraph();
			if (state==State.Timeshifting) 
			{
				StartTimeShifting();
        
				Log.WriteFile(Log.LogType.Capture,"TVCaptureDevice.Rebuildgraph() play:{0}",Recorder.GetTimeShiftFileName(ID-1));        
				g_Player.Play(Recorder.GetTimeShiftFileName(ID-1));
			}
			else 
			{
				View=true;
			}
			Log.WriteFile(Log.LogType.Capture,"Card:{0} rebuild graph done",ID);
		}

		string StripIllegalChars(string recordingAttribute)
		{
			if (recordingAttribute==null) return String.Empty;
			if (recordingAttribute.Length==0) return String.Empty;
			recordingAttribute=recordingAttribute.Replace(":"," ");
			recordingAttribute=recordingAttribute.Replace(";"," ");
			return recordingAttribute;
		}
		Hashtable GetRecordingAttributes()
		{
			// set the meta data in the dvr-ms or .wmv file
			TimeSpan ts = (_mNewRecordedTV.EndTime-_mNewRecordedTV.StartTime);
			Hashtable propsToSet = new Hashtable();

			propsToSet.Add("channel",new MetadataItem("channel", StripIllegalChars(_mNewRecordedTV.Channel), MetadataItemType.String));

			propsToSet.Add("recordedby",new MetadataItem("recordedby", "Mediaportal", MetadataItemType.String));

			if (_mNewRecordedTV.Title!=null && _mNewRecordedTV.Title.Length>0)
				propsToSet.Add("title",new MetadataItem("title", StripIllegalChars(_mNewRecordedTV.Title), MetadataItemType.String));
			
			if (_mNewRecordedTV.Genre!=null && _mNewRecordedTV.Genre.Length>0)
				propsToSet.Add("genre",new MetadataItem("genre", StripIllegalChars(_mNewRecordedTV.Genre), MetadataItemType.String));

			if (_mNewRecordedTV.Description!=null && _mNewRecordedTV.Description.Length>0)
				propsToSet.Add("description",new MetadataItem("details", StripIllegalChars(_mNewRecordedTV.Description), MetadataItemType.String));

			propsToSet.Add("id",new MetadataItem("id",  (uint)_mNewRecordedTV.ID, MetadataItemType.Dword));
			propsToSet.Add("cardno",new MetadataItem("cardno", (uint)this.ID, MetadataItemType.Dword));
			propsToSet.Add("duration",new MetadataItem("seconds", (uint)ts.TotalSeconds, MetadataItemType.Dword));
			propsToSet.Add("start",new MetadataItem("start", _mNewRecordedTV.Start.ToString(), MetadataItemType.String));
			propsToSet.Add("end",new MetadataItem("end", _mNewRecordedTV.End.ToString(), MetadataItemType.String));

			return propsToSet;
		}//void GetRecordingAttributes()

		/// <summary>
		/// This method can be used to stop the current recording.
		/// After recording is stopped the card will return to timeshifting mode
		/// </summary>
		public void StopRecording()
		{
			if (!IsRecording) return;

			Log.WriteFile(Log.LogType.Capture,"Card:{0} stop recording",ID);
			// todo : stop recorder
			_mGraph.StopRecording();

			_mNewRecordedTV.End = Utils.datetolong(DateTime.Now);
			TVDatabase.AddRecordedTV(_mNewRecordedTV);

			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				bool addMovieToDatabase= xmlreader.GetValueAsBool("capture", "addrecordingstomoviedatabase", true);
				if (addMovieToDatabase)
				{
					//add new recorded show to video database
					int movieid=VideoDatabase.AddMovieFile(_mNewRecordedTV.FileName);
					IMDBMovie movieDetails = new IMDBMovie();
					if (movieid>=0)
					{
						movieDetails.Title=_mNewRecordedTV.Title;
						movieDetails.Genre=_mNewRecordedTV.Genre;
						movieDetails.Plot=_mNewRecordedTV.Description;
						movieDetails.Year=_mNewRecordedTV.StartTime.Year;
						VideoDatabase.SetMovieInfoById(movieid, ref movieDetails);
					}
				}
			}
			// back to timeshifting state
			_mState = State.Timeshifting;

			_mNewRecordedTV = null;

			// cleanup...
			_mCurrentProgramRecording = null;
			_mCurrentTVRecording = null;
			_mPreRecordInterval = 0;
			_mPostRecordInterval = 0;
			if (!g_Player.Playing)
			{
				DeleteGraph();
				return;
			}
			string timeshiftFilename=String.Format(@"{0}\card{1}\live.tv",RecordingPath, ID);
			if (g_Player.CurrentFile==timeshiftFilename)
			{
				DeleteGraph();
				return;
			}
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

			Log.WriteFile(Log.LogType.Capture,"Card:{0} record {1} on {2} from {3}-{4}",ID, recording.Title,_mTvChannelName,recording.StartTime.ToLongTimeString(),recording.EndTime.ToLongTimeString());
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
					if (StartRecording(recording))
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
			Log.WriteFile(Log.LogType.Capture,"Card:{0} stop",ID);
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
				Log.WriteFile(Log.LogType.Capture,"Card:{0} CreateGraph",ID);
				_mGraph = GraphFactory.CreateGraph(this);
				if (_mGraph == null) return false;
				return _mGraph.CreateGraph(Quality);
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
				Log.WriteFile(Log.LogType.Capture,"Card:{0} DeleteGraph",ID);
				_mGraph.DeleteGraph();
				_mGraph = null;
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

			Log.WriteFile(Log.LogType.Capture,"Card:{0} start timeshifting :{1}",ID, _mTvChannelName);
			TVChannel channel=GetChannel(_mTvChannelName);

			if (_mState == State.Timeshifting) 
			{
				if (_mGraph.GetChannelNumber() != channel.Number)
				{
					if (!_mGraph.ShouldRebuildGraph(channel.Number))
					{
						_mTimeshiftingStartedTime = DateTime.Now;
						_mGraph.TuneChannel(channel);
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

      
      
			string strFileName = Recorder.GetTimeShiftFileName(ID-1);

  
      
			Log.WriteFile(Log.LogType.Capture,"Card:{0} timeshift to file:{1}",ID, strFileName);
			bool bResult = _mGraph.StartTimeShifting(channel, strFileName);
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
			Log.WriteFile(Log.LogType.Capture,"Card:{0} stop timeshifting",ID);
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
		bool StartRecording(TVRecording recording)
		{
			Log.WriteFile(Log.LogType.Capture,"Card:{0} start recording content:{1}",ID, recording.IsContentRecording);

			TVProgram prog=null;
			DateTime dtNow = DateTime.Now.AddMinutes(_mPreRecordInterval);
			TVProgram currentRunningProgram = null;
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			foreach (TVChannel chan in channels)
			{
				if (chan.Name==_mTvChannelName)
				{
					prog = chan.GetProgramAt(dtNow);
					break;
				}
			}
			if (prog != null) currentRunningProgram = prog.Clone();
			

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
      

			string strFileName = String.Format(@"{0}\{1}",RecordingPath, Utils.MakeFileName(strName));
			Log.WriteFile(Log.LogType.Capture,"Card:{0} recording to file:{1}",ID, strFileName);

			TVChannel channel=GetChannel(_mTvChannelName);


			_mNewRecordedTV = new TVRecorded();
			_mNewRecordedTV.Start = Utils.datetolong(DateTime.Now);
			_mNewRecordedTV.Channel = _mTvChannelName;
			_mNewRecordedTV.FileName = strFileName;
			if (currentRunningProgram != null)
			{
				_mNewRecordedTV.Title = currentRunningProgram.Title;
				_mNewRecordedTV.Genre = currentRunningProgram.Genre;
				_mNewRecordedTV.Description = currentRunningProgram.Description;
				_mNewRecordedTV.End = currentRunningProgram.End;
			}
			else
			{
				_mNewRecordedTV.Title = "";
				_mNewRecordedTV.Genre = "";
				_mNewRecordedTV.Description = "";
				
				_mNewRecordedTV.End=Utils.datetolong(DateTime.Now.AddHours(2));
			}

			Hashtable attribtutes=GetRecordingAttributes();
			bool bResult = _mGraph.StartRecording(attribtutes,recording,channel, ref strFileName, recording.IsContentRecording, timeProgStart);

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
		TVChannel GetChannel(string strChannelName)
		{ 
			TVChannel retChannel = new TVChannel();
			retChannel.Number=0;
			retChannel.Name=strChannelName;
			retChannel.ID=0;
			retChannel.TVStandard=AnalogVideoStandard.None;
			retChannel.Country=CountryCode;
			
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			foreach (TVChannel chan in channels)
			{
				if (String.Compare(strChannelName, chan.Name, true) == 0)
				{
					if (chan.Country<=0) chan.Country=CountryCode;
					return chan;
				}
			}
			return retChannel;
		}


		/// <summary>
		/// Property indiciating if the card supports timeshifting
		/// </summary>
		/// <returns>boolean indiciating if the graph supports timeshifting</returns>
		public bool SupportsTimeShifting
		{
			get
			{
				bool result=false;
				if (_mGraph==null)
				{
					_mGraph = GraphFactory.CreateGraph(this);
					if (_mGraph == null) return false;
					result=_mGraph.SupportsTimeshifting();
					_mGraph=null;
				}
				else
				{
					result=_mGraph.SupportsTimeshifting();
				}

				return result;
			}
		}

		public void Tune(TVChannel channel)
		{
			if (_mState != State.Viewing) return;
			_mGraph.TuneChannel( channel);
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
						Log.WriteFile(Log.LogType.Capture,"Card:{0} stop viewing :{1}",ID, _mTvChannelName);
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
						Log.WriteFile(Log.LogType.Capture,"Card:{0} start viewing :{1}",ID, _mTvChannelName);
						TVChannel chan = GetChannel(_mTvChannelName);
						_mGraph.StartViewing(chan);
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

		public bool ViewChannel(TVChannel channel)
		{
			if (_mGraph==null)
			{
				if (!CreateGraph()) return false;
			}
			_mGraph.StartViewing(channel);
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
				if (_mGraph==null) 
				{
					_mGraph = GraphFactory.CreateGraph(this);
					NetworkType netType=_mGraph.Network();
					_mGraph = null;
					return netType;
				}
				return _mGraph.Network();
			}
		}
		public void Tune(object tuningObject, int disecqNo)
		{
			if (_mGraph==null) return ;
			_mGraph.Tune(tuningObject, disecqNo);
		}
		public void StoreTunedChannels(bool radio, bool tv, ref int newChannels, ref int updateChannels)
		{
			if (_mGraph==null) return ;
			_mGraph.StoreChannels(ID, radio,tv, ref newChannels, ref updateChannels);
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
				using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(filename))
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
			using(MediaPortal.Profile.Xml   xmlWriter=new MediaPortal.Profile.Xml(filename))
			{
				xmlWriter.SetValue("tv","contrast",GUIGraphicsContext.Contrast);
				xmlWriter.SetValue("tv","brightness",GUIGraphicsContext.Brightness);
				xmlWriter.SetValue("tv","gamma",GUIGraphicsContext.Gamma);
				xmlWriter.SetValue("tv","saturation",GUIGraphicsContext.Saturation);
				xmlWriter.SetValue("tv","sharpness",GUIGraphicsContext.Sharpness);
			}
		}
		public IBaseFilter Mpeg2DataFilter
		{
			get
			{
				if (_mGraph==null) return null;
				return _mGraph.Mpeg2DataFilter();
			}
		}
		public void StartRadio(RadioStation station)
		{
			if (_mState != State.Radio)
			{
				DeleteGraph();
				CreateGraph();
				_mGraph.StartRadio(station);
				_mState = State.Radio;
			}
			else
			{
				_mGraph.TuneRadioChannel(station);
			}
			radioStationName=station.Name;
		}
		public void TuneRadioChannel(RadioStation station)
		{
			if (_mState != State.Radio)
			{
				DeleteGraph();
				CreateGraph();
				_mGraph.StartRadio(station);
				_mState = State.Radio;
			}
			else
			{
				_mGraph.TuneRadioChannel(station);
			}
			radioStationName=station.Name;
		}
		public void TuneRadioFrequency(int frequency)
		{
			if (_mState != State.Radio)
			{
				DeleteGraph();
				CreateGraph();
				RadioStation station = new RadioStation();
				_mGraph.StartRadio(station);
				_mState =State.Radio;
			}
			else
			{
				_mGraph.TuneRadioFrequency(frequency);
			}
		}

		public string RadioStation
		{
			get { return radioStationName;}
		}
		
		public void GetRecordings(string drive, ref ArrayList recordings)
		{
			if (!DeleteOnLowDiskspace) return;
			if (RecordingPath.ToLower().Substring(0,2) !=drive.ToLower()) return;
			try
			{
				string[] fileNames=System.IO.Directory.GetFiles(RecordingPath,"*.dvr-ms");
				for (int i=0; i < fileNames.Length;++i)
				{
					bool add=true;
					foreach (RecordingFileInfo fi in recordings)
					{
						if (fi.filename.ToLower() == fileNames[i].ToLower())
						{
							add=false;
						}
					}
					if (add)
					{
						FileInfo info = new FileInfo(fileNames[i]);
						RecordingFileInfo fi = new RecordingFileInfo();
						fi.info=info;
						fi.filename=fileNames[i];
						recordings.Add(fi);
					}
				}
			}
			catch(Exception)
			{
			}
		}
  }
}  
#endif