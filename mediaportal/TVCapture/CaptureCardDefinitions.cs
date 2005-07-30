/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System;
using System.Xml;
using DShowNET;
using System.Collections;
using MediaPortal.GUI.Library;
namespace TVCapture
{
	#region Data Classes

	public class CapabilityDefinition
	{
		public bool HasTv;
		public bool HasRadio;
		public bool IsBDADevice;
		public bool IsMceDevice;
		public bool IsMpeg2Device;
		public bool IsSoftwareDevice;
	}

	public class FilterDefinition
	{
		public string Category;
		public string FriendlyName;
		public bool   CheckDevice;
		public string MonikerDisplayName;
		public DShowNET.IBaseFilter DSFilter;
	};

	public class ConnectionDefinition
	{
		public string SourceCategory;
		public string SourcePinName;
		public string SinkCategory;
		public string SinkPinName;
	};

	public class InterfaceDefinition
	{
		public string FilterCategory;
		public string VideoPinName;
		public string AudioPinName;
		public string Mpeg2PinName;
		public string SectionsAndTablesPinName;
	}

	public class CaptureCardDefinition
	{
		public string								DeviceId;
		public string								CommercialName;
		public string								CaptureName;
		public CapabilityDefinition	Capabilities;
		public DeviceDefinition			Tv;
		public DeviceDefinition			Radio;
	}

	public class DeviceDefinition
	{
		public Hashtable					 FilterDefinitions;
		public ArrayList					 ConnectionDefinitions;
		public InterfaceDefinition InterfaceDefinition;
	}
	#endregion

	public sealed class AvailableFilters
	{
		#region Private Members
		private static volatile AvailableFilters _mInstance;
		private static          object					 _mSyncRoot					= new Object();
		private static					Hashtable				 _mAvailableFilters = new Hashtable();
		#endregion

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		private AvailableFilters()
		{
			// Get all available filters categories and use this in a loop to fetch all filters
			FilterCollection filters = new Filters().AllFilters;
			Hashtable				 fcol		 = new Hashtable();
			for (int i = 0; i < filters.Count; i++)
			{
				Guid     clsId;
				string   clsIdName;
				Filter   f   = filters[i];

				// The last part of the moniker contains the classid category of the filter
				// Use this to fetch another FilterCollection to be added to the complete list of filters
				// 
				string[] tmp = f.MonikerString.Split(@"\".ToCharArray());
				clsIdName = tmp[tmp.GetUpperBound(0)];
				clsId     = new Guid(clsIdName);
			  FilterCollection fc = new FilterCollection(clsId);
				if (null != fc)
				{
					for (int y = 0; y < fc.Count; y++)
					{
						Filter ff = fc[y];

						// If the friendly name of the filter already exists, add this device/filter
						// to the list belonging to this friendly name. Note that this is THE situation which
						// occurs when having more than one capture card of the same type...
						if (_mAvailableFilters.ContainsKey(ff.Name))
						{
							System.Collections.ArrayList al = _mAvailableFilters[ff.Name] as System.Collections.ArrayList;
							al.Add(ff);
						}
						else
						{
							// Add new entry to list
							System.Collections.ArrayList al = new System.Collections.ArrayList();
							al.Add(ff);
							_mAvailableFilters.Add(ff.Name, al);
						}
					}
				}
			}
		}

		/// <summary>
		/// Instance of Singleton class AvailableFilters
		/// 
		/// If instance not yet created, it will be created and the instance will be returned
		/// Note that the creation of the filter list is thread-safe.
		/// </summary>
		public static AvailableFilters Instance
		{
			get 
			{
				if (_mInstance == null) 
				{
					lock (_mSyncRoot) 
					{
						if (_mInstance == null) 
							_mInstance = new AvailableFilters();
					}
				}

				return _mInstance;
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Property that returns the list of filters
		/// </summary>
		public static Hashtable Filters
		{
			get 
			{
				return AvailableFilters._mAvailableFilters;
			}
		}
		#endregion
	}

	/// <summary>
	/// 
	/// </summary>
	public sealed class CaptureCardDefinitions
	{
		#region Private Members
		private static volatile CaptureCardDefinitions _mInstance;
		private static					object 								 _mSyncRoot = new Object();

		// Next list holds CaptureCardDefinition for all supported capture cards
		private static ArrayList _mCaptureCardDefinitions = new ArrayList();
		#endregion

		#region Constructors
		/// <summary>
		/// This Singleton based class contains the configuration of the supported capture cards
		/// It contains the filters and pin connections required to buid the graph upto and including the
		/// filter that outputs the audio/video or mpeg2 stream. The rest of the graph is dynamically build
		/// depending on watching just TV / listening Radio or using timeshifting / recording...
		/// 
		/// The format of the required SupportedCaptureCards.xml file is as follows:
		/// <?xml version="1.0"?>
		/// <!-- This file contains the capture card definitions to create the capture card dependant part of a graph -->
		/// <!-- -->
		/// <!-- Each capture card is identified by its friendly name and its its unique identification: its guid. -->
		/// <!-- Depending on the cards capabilities, it has a TV and Radio section that defines the graph -->
		/// <!-- -->
		/// <!-- Each graph is defined upto a certain capture card dependant filter, upon which point the software will take over and -->
		/// <!-- create the rest of the graph, which should be generic, ie INDEPENDANT of the capture card!!! -->
		/// <!-- -->
		/// <!-- The last filter/output pin is defined under the interface section: -->
		/// <!--    name="...", "Video" (for video pin), "Audio" (for audio pin) and "Mpeg2" (for mpeg2 output video/audio). -->
		/// <!-- It shall depend on the kind of card (MPEG2/Software) what exactly is offered on the video, audio and mpeg2 pins... -->
		/// <!-- MPEG2 cards will usually only have the mpeg2 output and audio output if they have radio also... -->
		/// <!-- -->
		/// <!-- Regarding connection pin definition: -->
		/// <!--  1. Define the Name of the pin. This might work and might not as this might be language dependant -->
		/// <!--  2. Define the 0-based index of the pin. This always should work... -->
		/// <!-- -->
		/// <capturecards>
		/// 	<capturecard commercialname="PVR 150MCE" capturename="Hauppauge WinTV PVR PCI II Capture" devid="ven_4444&#38;dev_0016&#38;subsys_88010070&#38;rev_01">
		///			<capabilities tv="true" radio="true" mpeg2="true" mce="true" sw="false"></capabilities>
		/// 		<tv>
		/// 			<interface cat="encoder" video="" audio="" mpeg2="0"></interface>
		/// 			<filters>
		/// 				<filter cat="tvtuner"  name="Hauppauge WinTV PVR PCI II TvTuner"  checkdevice="true"></filter>
		/// 				<filter cat="tvaudio"  name="Hauppauge WinTV PVR PCI II TvAudio"  checkdevice="true"></filter>
		/// 				<filter cat="crossbar" name="Hauppauge WinTV PVR PCI II Crossbar" checkdevice="true"></filter>
		/// 				<filter cat="capture"  name="Hauppauge WinTV PVR PCI II Capture"  checkdevice="true"></filter>
		/// 				<filter cat="encoder"  name="Hauppauge WinTV PVR PCI II Encoder"  checkdevice="true"></filter>
		/// 			</filters>
		/// 			<connections>
		/// 				<connection sourcefilter="tvtuner" sourcepin="Analog Audio" sinkfilter="tvaudio" sinkpin="TVAudio In"></connection>
		/// 				<connection sourcefilter="tvtuner" sourcepin="Analog Video" sinkfilter="crossbar" sinkpin="0: Video Tuner In"></connection>
		/// 				<connection sourcefilter="tvaudio" sourcepin="TVAudio Out" sinkfilter="crossbar" sinkpin="1: Audio Tuner In"></connection>
		/// 				<connection sourcefilter="crossbar" sourcepin="0: Video Decoder Out" sinkfilter="capture" sinkpin="0"></connection>
		/// 				<connection sourcefilter="capture" sourcepin="3" sinkfilter="encoder" sinkpin="656"></connection>
		/// 			</connections>
		/// 		</tv>
		/// 		<radio>
		/// 			<interface cat="capture" video="" audio="1" mpeg2=""></interface>
		/// 			<filters>
		/// 				<filter cat="tvtuner"  name="Hauppauge WinTV PVR PCI II TvTuner"  checkdevice="true"></filter>
		/// 				<filter cat="tvaudio"  name="Hauppauge WinTV PVR PCI II TvAudio"  checkdevice="true"></filter>
		/// 				<filter cat="crossbar" name="Hauppauge WinTV PVR PCI II Crossbar" checkdevice="true"></filter>
		/// 				<filter cat="capture"  name="Hauppauge WinTV PVR PCI II Capture"  checkdevice="true"></filter>
		/// 			</filters>
		/// 			<connections>
		/// 				<connection sourcefilter="tvtuner" sourcepin="Analog Audio" sinkfilter="tvaudio" sinkpin="TVAudio In"></connection>
		/// 				<connection sourcefilter="tvtuner" sourcepin="Analog Video" sinkfilter="crossbar" sinkpin="0: Video Tuner In"></connection>
		/// 				<connection sourcefilter="tvaudio" sourcepin="TVAudio Out" sinkfilter="crossbar" sinkpin="1: Audio Tuner In"></connection>
		/// 				<connection sourcefilter="crossbar" sourcepin="0: Video Decoder Out" sinkfilter="capture" sinkpin="0"></connection>
		/// 				<connection sourcefilter="crossbar" sourcepin="1: Audio Decoder Out" sinkfilter="capture" sinkpin="1"></connection>
		/// 				<connection sourcefilter="capture" sourcepin="1" sinkfilter="exit" sinkpin="audio"></connection>
		/// 			</connections>
		/// 		</radio>
		/// 	</capturecard>
		/// </capturecards>
		/// </summary>
		private CaptureCardDefinitions()
		{
//			DirectShowUtil.DebugWrite("CaptureCardDefinitions:ctor IN");

			XmlDocument doc = new XmlDocument();
			if (!System.IO.File.Exists(@"CaptureCardDefinitions.xml"))
			{
				DirectShowUtil.DebugWrite(" Error: CaptureCardDefinitions.xml file not found!");
				DirectShowUtil.DebugWrite("CaptureCardDefinitions:ctor OUT");
				return;
			}

			try
			{
				doc.Load("CaptureCardDefinitions.xml");
			}
			catch (XmlException e)
			{
				DirectShowUtil.DebugWrite(" Error: Unable to load CaptureCardDefinitions.xml, probably contains xml errors!");
				DirectShowUtil.DebugWrite(" Error: {0} in {1} at line {2}", e.Message, e.Source, e.LineNumber);
				DirectShowUtil.DebugWrite("CaptureCardDefinitions:ctor OUT");
				return;
			}

			XmlElement  root     = doc.DocumentElement;
			XmlNodeList nodeList = root.SelectNodes("capturecard");

			try
			{
				if (nodeList != null)
				{
					//DirectShowUtil.DebugWrite(" Loading: capturecards...");

					foreach (XmlNode cc in nodeList)
					{
						CaptureCardDefinition cardConfig			 = new CaptureCardDefinition();
						cardConfig.Capabilities                = new CapabilityDefinition();
						cardConfig.Tv													 = new DeviceDefinition();
						cardConfig.Radio											 = new DeviceDefinition();
						cardConfig.Tv.FilterDefinitions				 = new System.Collections.Hashtable();
						cardConfig.Tv.ConnectionDefinitions		 = new System.Collections.ArrayList();
						cardConfig.Tv.InterfaceDefinition      = new InterfaceDefinition();
						cardConfig.Radio.FilterDefinitions     = new System.Collections.Hashtable();
						cardConfig.Radio.ConnectionDefinitions = new System.Collections.ArrayList();
						cardConfig.Radio.InterfaceDefinition   = new InterfaceDefinition();

						// Each card has some identification, partly unique, partly common
						// 1. commercialname
						//		The name of the card under which it is sold, ie Hauppauge WinTV PVR150MCE
						// 2. capturename
						//		The name of the capture device, as listed in Windows/GraphEdit etc. is NOT unique
						// 3. devid
						//		The UNIQUE device identification used to identify the card!!!
						//		This id is part of the filter monikerstring, but can also be checked using the
						//		devicemanager in Windows XP...
						//
						// Cards are "stored" according to their UNIQUE device id
						//
						// NOTE: the device id is always converted / checked using lowercase.
						//			 All the other names are CaSe SeNsItIvE !!!!
						//
						cardConfig.DeviceId				= cc.Attributes.GetNamedItem(@"devid").InnerText.ToLower();
						cardConfig.CommercialName = cc.Attributes.GetNamedItem(@"commercialname").InnerText;
						cardConfig.CaptureName    = cc.Attributes.GetNamedItem(@"capturename").InnerText;
						_mCaptureCardDefinitions.Add(cardConfig);
						//DirectShowUtil.DebugWrite("device:{0}", cardConfig.CommercialName);

						// Get the cards capabilities...
						XmlNode capNode     = cc.SelectSingleNode(@"capabilities");
						if (capNode != null)
						{
							//DirectShowUtil.DebugWrite("  Getting capabilities...");
							cardConfig.Capabilities.HasTv						 = XmlConvert.ToBoolean(capNode.Attributes.GetNamedItem(@"tv").InnerText);
							cardConfig.Capabilities.HasRadio				 = XmlConvert.ToBoolean(capNode.Attributes.GetNamedItem(@"radio").InnerText);
							cardConfig.Capabilities.IsBDADevice			 = XmlConvert.ToBoolean(capNode.Attributes.GetNamedItem(@"bda").InnerText);
							cardConfig.Capabilities.IsMceDevice			 = XmlConvert.ToBoolean(capNode.Attributes.GetNamedItem(@"mce").InnerText);
							cardConfig.Capabilities.IsMpeg2Device    = XmlConvert.ToBoolean(capNode.Attributes.GetNamedItem(@"mpeg2").InnerText);
							cardConfig.Capabilities.IsSoftwareDevice = XmlConvert.ToBoolean(capNode.Attributes.GetNamedItem(@"sw").InnerText);

							//DirectShowUtil.DebugWrite("    TV:{0} radio:{1} bda:{2} mce:{3} mpeg2:{4} s/w:{5}",
							//					cardConfig.Capabilities.HasTv,cardConfig.Capabilities.HasRadio,cardConfig.Capabilities.IsBDADevice,
							//					cardConfig.Capabilities.IsMceDevice,	cardConfig.Capabilities.IsMpeg2Device,cardConfig.Capabilities.IsSoftwareDevice);																																																																

						}
						else
						{
							//DirectShowUtil.DebugWrite("  Failed getting capabilities...");
						}

						// First do the tv part, then the (optional) radio part...
						XmlNode tvNode      = cc.SelectSingleNode(@"tv");
						if (tvNode != null)
						{
//							DirectShowUtil.DebugWrite("  Getting tv...");

							XmlNode filterNodes = tvNode.SelectSingleNode(@"filters");
							FilterDefinition dsfilter;
							string strstr;
							foreach (XmlNode ff in filterNodes.ChildNodes)
							{
								dsfilter              = new FilterDefinition();
								dsfilter.Category     = ff.Attributes.GetNamedItem(@"cat").InnerText;
								dsfilter.FriendlyName = ff.Attributes.GetNamedItem(@"name").InnerText;
								strstr                = ff.Attributes.GetNamedItem(@"checkdevice").InnerText;
								dsfilter.CheckDevice  = XmlConvert.ToBoolean(strstr);
								cardConfig.Tv.FilterDefinitions.Add(dsfilter.Category, dsfilter);
							}

							XmlNode connections = tvNode.SelectSingleNode(@"connections");
							ConnectionDefinition connectionDefinition;
							foreach (XmlNode con in connections.ChildNodes)
							{
								connectionDefinition = new ConnectionDefinition();
								connectionDefinition.SourceCategory = con.Attributes.GetNamedItem(@"sourcefilter").InnerText;
								connectionDefinition.SourcePinName  = con.Attributes.GetNamedItem(@"sourcepin").InnerText;
								connectionDefinition.SinkCategory   = con.Attributes.GetNamedItem(@"sinkfilter").InnerText;
								connectionDefinition.SinkPinName    = con.Attributes.GetNamedItem(@"sinkpin").InnerText;
								cardConfig.Tv.ConnectionDefinitions.Add(connectionDefinition);
							}

							XmlNode filterInterface = tvNode.SelectSingleNode(@"interface");
							cardConfig.Tv.InterfaceDefinition.FilterCategory = filterInterface.Attributes.GetNamedItem(@"cat").InnerText;
							cardConfig.Tv.InterfaceDefinition.VideoPinName   = filterInterface.Attributes.GetNamedItem(@"video").InnerText;
							cardConfig.Tv.InterfaceDefinition.AudioPinName   = filterInterface.Attributes.GetNamedItem(@"audio").InnerText;
							cardConfig.Tv.InterfaceDefinition.Mpeg2PinName   = filterInterface.Attributes.GetNamedItem(@"mpeg2").InnerText;
							try 
							{
								cardConfig.Tv.InterfaceDefinition.SectionsAndTablesPinName = filterInterface.Attributes.GetNamedItem(@"sectionsandtables").InnerText;
							}
							catch 
							{
								cardConfig.Tv.InterfaceDefinition.SectionsAndTablesPinName = "";
							}
						}

						// Now check if this device also has radio
						XmlNode radioNode = cc.SelectSingleNode(@"radio");
						if (radioNode != null)
						{
							//DirectShowUtil.DebugWrite("  Getting radio...");

							XmlNode filterNodes = radioNode.SelectSingleNode(@"filters");
							FilterDefinition dsfilter;
							string strstr;
							foreach (XmlNode ff in filterNodes.ChildNodes)
							{
								dsfilter              = new FilterDefinition();
								dsfilter.Category     = ff.Attributes.GetNamedItem(@"cat").InnerText;
								dsfilter.FriendlyName = ff.Attributes.GetNamedItem(@"name").InnerText;
								strstr                = ff.Attributes.GetNamedItem(@"checkdevice").InnerText;
								dsfilter.CheckDevice  = XmlConvert.ToBoolean(strstr);
								cardConfig.Radio.FilterDefinitions.Add(dsfilter.Category, dsfilter);
							}

							XmlNode connections = radioNode.SelectSingleNode(@"connections");
							ConnectionDefinition connectionDefinition;
							foreach (XmlNode con in connections.ChildNodes)
							{
								connectionDefinition = new ConnectionDefinition();
								connectionDefinition.SourceCategory = con.Attributes.GetNamedItem(@"sourcefilter").InnerText;
								connectionDefinition.SourcePinName  = con.Attributes.GetNamedItem(@"sourcepin").InnerText;
								connectionDefinition.SinkCategory   = con.Attributes.GetNamedItem(@"sinkfilter").InnerText;
								connectionDefinition.SinkPinName    = con.Attributes.GetNamedItem(@"sinkpin").InnerText;
								cardConfig.Radio.ConnectionDefinitions.Add(connectionDefinition);
							}

							XmlNode filterInterface = radioNode.SelectSingleNode(@"interface");
							InterfaceDefinition interfaceDefinition;
							interfaceDefinition = cardConfig.Radio.InterfaceDefinition;
							interfaceDefinition.FilterCategory = filterInterface.Attributes.GetNamedItem(@"cat").InnerText;
							interfaceDefinition.VideoPinName   = filterInterface.Attributes.GetNamedItem(@"video").InnerText;
							interfaceDefinition.AudioPinName   = filterInterface.Attributes.GetNamedItem(@"audio").InnerText;
							interfaceDefinition.Mpeg2PinName   = filterInterface.Attributes.GetNamedItem(@"mpeg2").InnerText;
														
							try 
							{
								interfaceDefinition.SectionsAndTablesPinName = filterInterface.Attributes.GetNamedItem(@"sectionsandtables").InnerText;
							}
							catch 
							{
								interfaceDefinition.SectionsAndTablesPinName = "";
							}


						}
						//DirectShowUtil.DebugWrite("  Loaded: DeviceId {0}, CommercialName {1}, CaptureName {2}",
						//	cardConfig.DeviceId, cardConfig.CommercialName, cardConfig.CaptureName);
					}
				}
			}
			catch (System.Exception e)
			{
				DirectShowUtil.DebugWrite(" Error: Error while getting values from CaptureCardDefinitions.xml");
				DirectShowUtil.DebugWrite(" Error: {0} in {1} at {2}", e.Message, e.Source, e.StackTrace);
				DirectShowUtil.DebugWrite("CaptureCardDefinitions:ctor OUT");
			}
		}

		public static CaptureCardDefinitions Instance
		{
			get 
			{
				if (_mInstance == null) 
				{
					lock (_mSyncRoot) 
					{
						if (_mInstance == null) 
							_mInstance = new CaptureCardDefinitions();
					}
				}

				return _mInstance;
			}
		}
		#endregion Constructors

		#region Properties
		public static ArrayList CaptureCards
		{
			get 
			{
				if (CaptureCardDefinitions.Instance == null) {};
				return CaptureCardDefinitions._mCaptureCardDefinitions;
			}
		}
		#endregion Properties
	}
}
