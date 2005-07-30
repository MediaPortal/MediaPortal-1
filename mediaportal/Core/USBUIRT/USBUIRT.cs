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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Collections;
using MediaPortal.GUI.Library;
namespace MediaPortal.IR
{
	#region LearningEventArgs
	/// <summary>
	/// This class will handle all communication with an external USBUIRT device
	/// The USB-UIRT, allows your PC to both Receive and Transmit infrared signals -- 
	/// exactly like those used by the collection of remote controls you've acquired for your TV, 
	/// VCR, Audio System, etc. 
	/// See www.usbuirt.com for more details on USBUIRT
	/// </summary>
	public class LearningEventArgs : System.EventArgs
	{
		public bool		Succeeded=false;
		public string Button;
		public string IrCode=String.Empty;
		public LearningEventArgs(string button)
		{
			this.Button = button;
		}
		public LearningEventArgs(string button, string ircode, bool succeeded)
		{
			this.Button = button;
			this.IrCode = ircode;
			this.Succeeded=succeeded;
		}
	}
	#endregion

	public class USBUIRT
	{
		#region USBUIRT imports
		[StructLayout(LayoutKind.Sequential)]
			struct UUINFO
		{
			public int fwVersion;
			public int protVersion;
			public char fwDateDay;
			public char fwDateMonth;
			public char fwDateYear;
		}

		[StructLayout(LayoutKind.Sequential)]
			struct UUGPIO
		{
			byte[]	irCode;
			byte	action;
			byte	duration;
		}

		[DllImport("uuirtdrv.dll")]		
		static extern IntPtr UUIRTOpen();

		[DllImport("uuirtdrv.dll")]		
		static extern bool UUIRTClose(IntPtr hHandle);

		[DllImport("uuirtdrv.dll")]	
		static extern bool UUIRTGetDrvInfo(ref int puDrvVersion);
		
		[DllImport("uuirtdrv.dll")]		
		static extern  bool UUIRTGetUUIRTInfo(IntPtr hHandle, ref UUINFO puuInfo);
		
		[DllImport("uuirtdrv.dll")]		
		static extern bool UUIRTGetUUIRTConfig(IntPtr hHandle, ref uint puConfig);

		[DllImport("uuirtdrv.dll")]		
		static extern bool UUIRTSetUUIRTConfig(IntPtr hHandle, uint uConfig);

		[DllImport("uuirtdrv.dll")]		
		static extern bool UUIRTTransmitIR(IntPtr hHandle, string IRCode, int codeFormat, int repeatCount, int inactivityWaitTime, IntPtr hEvent, int res1, int res2);

		[DllImport("uuirtdrv.dll")]		
		static extern bool UUIRTLearnIR(IntPtr hHandle, int codeFormat,  [MarshalAs(UnmanagedType.LPStr)] StringBuilder ircode, IRLearnCallbackDelegate progressProc, int userData,   ref int pAbort, int param1, [ MarshalAs( UnmanagedType.AsAny )] Object o, [ MarshalAs( UnmanagedType.AsAny )] Object oo);
		
		[DllImport("uuirtdrv.dll")]		
		static extern bool UUIRTSetReceiveCallback(IntPtr hHandle, UUIRTReceiveCallbackDelegate receiveProc, int none);

		[DllImport("uuirtdrv.dll")]		
		static extern bool UUIRTSetUUIRTGPIOCfg(IntPtr hHandle, int index, ref UUGPIO GpioSt);
		//HUUHANDLE	  hHandle, int index, PUUGPIO pGpioSt);

		[DllImport("uuirtdrv.dll")]		
		static extern bool UUIRTGetUUIRTGPIOCfg(IntPtr hHandle, ref int numSlots, ref uint dwPortPins,ref UUGPIO GpioSt);
	
		//(HUUHANDLE hHandle, int *pNumSlots, UINT32 *pdwPortPins, PUUGPIO pGPIOStruct);
		#endregion

		#region delegates
		public delegate void StartLearningEventHandler(object sender, LearningEventArgs e);
		public delegate void EventLearnedHandler(object sender, LearningEventArgs e);
	
		private delegate void UUIRTReceiveCallbackDelegate( string val );
		public delegate void IRLearnCallbackDelegate( uint val, uint val2, ulong val3);
		public delegate void OnRemoteCommand(object command);
		private UUIRTReceiveCallbackDelegate urcb = null;
		#endregion

		#region constants
		static int UUIRTDRV_IRFMT_UUIRT	= 0x0000;
		private const string		remotefile = "UIRTUSB-remote.xml";
		private const string		tunerfile = "UIRTUSB-tuner.xml";
		#endregion

		#region variables
		private IntPtr					UsbUirtHandle = IntPtr.Zero;
		private StringBuilder		ircode = new StringBuilder("1",2048);
		private int							abort = 0;
		private int							timelaps = 300; // time in milliseconds between two accepted commands
		private IntPtr					empty = new IntPtr(-1);
		private bool						isUsbUirtLoaded = false;
		private string					lastchannel;
		private OnRemoteCommand remoteCommandCallback = null;
		private bool 						accepRemoteCommands = false;
		private bool 						transmitEventsEnabled = false;
		private bool 						is3DigitTuner = false;
		private bool 						tunerNeedsEnter = false;
		static USBUIRT					instance = null;
		string[]								externalTunerCodes = new string[11]; // 10 digits + Enter
		Hashtable								commandsLearned = new Hashtable();
		DateTime								timestamp = DateTime.Now;
		private bool            isLearning=false;

		#endregion

		#region events
		public event StartLearningEventHandler StartLearning;
		public event EventLearnedHandler OnEventLearned;
		#endregion

		#region properties
		public static USBUIRT Instance 
		{
			get 
			{
				return instance;
			}
		}

		public bool Is3Digit 
		{
			get 
			{
				return is3DigitTuner;
			}

			set 
			{
				is3DigitTuner = value;
			}
		}

		public bool NeedsEnter 
		{
			get
			{
				return tunerNeedsEnter;
			}

			set
			{
				tunerNeedsEnter = value;
			}
		}


		public bool ReceiveEnabled 
		{
			get 
			{
				return this.accepRemoteCommands;
			}

			set 
			{
				this.accepRemoteCommands = value;
			}
		}

		public bool TransmitEnabled 
		{
			get 
			{
				return this.transmitEventsEnabled;
			}

			set 
			{
				this.transmitEventsEnabled = value;
			}
		}

		public int TimeLaps 
		{
			set 
			{
				timelaps = value;
			}
			get 
			{
				return timelaps;
			}
		}
		#endregion

		#region ctor
		private USBUIRT()
		{
		}

		private USBUIRT(OnRemoteCommand callback)
		{
			try
			{
				Log.Write("USBUIRT:Open");
				commandsLearned = new Hashtable();
				UsbUirtHandle = UUIRTOpen();
				if(UsbUirtHandle !=  empty )
				{
					isUsbUirtLoaded = true;
					

					Log.Write("USBUIRT:Open succes:{0}",GetVersions());
				}
				else
				{
					Log.Write("USBUIRT:Unable to open USBUIRT driver");
				}
				if(isUsbUirtLoaded)
				{
					using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
					{
						ReceiveEnabled = xmlreader.GetValueAsBool("USBUIRT", "internal", false) ;
						TransmitEnabled = xmlreader.GetValueAsBool("USBUIRT", "external", false) ;
						Is3Digit = xmlreader.GetValueAsBool("USBUIRT", "is3digit", false) ;
						tunerNeedsEnter = xmlreader.GetValueAsBool("USBUIRT", "needsenter", false) ;
					}
					if (System.IO.File.Exists(remotefile))
					{
						LoadValues();
					}
					else Log.Write("USBUIRT:unable to load values from:{0}", remotefile);
					if (System.IO.File.Exists(tunerfile))
					{
						LoadTunerValues();
					}
					else Log.Write("USBUIRT:unable to load tunervalues from:{0}", tunerfile);
				}
				//setup callack to receive IR messages
				urcb = new UUIRTReceiveCallbackDelegate(this.UUIRTReceiveCallback);		
				UUIRTSetReceiveCallback(UsbUirtHandle,urcb,0);
				RemoteCommandCallback = callback;
			}
			catch(Exception )
			{
				//most users dont have the dll on their system so will get a exception here
			}
		}
		#endregion

		#region serialisation
		private void LoadValues()
		{
			System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(remotefile);
			reader.MoveToContent();
			// skip the document element
			reader.Read();

			while(!reader.EOF) 
			{
				Debug.Assert(reader.LocalName == "entry");
				try
				{
					int actionInt = Int32.Parse(reader.GetAttribute("actionInt"));
					reader.Read();
					string remoteCode = reader.ReadString();
					reader.ReadEndElement();
					commandsLearned[remoteCode] = actionInt;
				}
				catch(Exception)
				{
					break;
				}
				if(reader.LocalName != "entry")
					break;
			}
			reader.Close();
			
		}

		private void LoadTunerValues()
		{
			try
			{
				System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(tunerfile);
				reader.MoveToContent();
				// skip the document element
				reader.Read();

				while(!reader.EOF) 
				{
					Debug.Assert(reader.LocalName == "entry");
					int index = Int32.Parse(reader.GetAttribute("index"));
					reader.Read();
					string remoteCode = reader.ReadString();
					reader.ReadEndElement();
					externalTunerCodes[index] = remoteCode;
					if(reader.LocalName != "entry")
						break;
				}
				reader.Close();
			}
			catch(Exception)
			{
			}
			
		}

		public void SaveInternalValues()
		{
			try
			{
				System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(remotefile, System.Text.Encoding.Unicode);

				writer.WriteStartElement("docElement");
				foreach(string key in commandsLearned.Keys) 
				{
					writer.WriteStartElement("entry");
					writer.WriteAttributeString("actionInt", ((int)commandsLearned[key]).ToString());
					writer.WriteAttributeString("actionDescription", (commandsLearned[key]).ToString());
					writer.WriteString(key);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
				writer.Close();
			}
			catch(Exception)
			{
			}
		}

		public void SaveTunerValues()
		{
			try
			{
				System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(tunerfile, System.Text.Encoding.Unicode);

				writer.WriteStartElement("docElement");
				for(int i=0; i<11; i++)
				{
					writer.WriteStartElement("entry");
					writer.WriteAttributeString("index", i.ToString());
					writer.WriteString(externalTunerCodes[i]);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
				writer.Close();
			}
			catch(Exception)
			{
			}
		}
		#endregion

		#region remote receiver methods
		public OnRemoteCommand RemoteCommandCallback
		{
			set 
			{
				remoteCommandCallback = value;
			}
		}

		public void UUIRTReceiveCallback( string irid )
		{
			if (isLearning) return;
			if (!ReceiveEnabled) return;
			object command = commandsLearned[irid];
			if (command==null) return;
			TimeSpan ts=DateTime.Now - timestamp;
			if (ts.TotalMilliseconds >= timelaps) 
			{
				this.remoteCommandCallback(command);
				timestamp = DateTime.Now;
			}
		}

		#endregion

		#region methods
		public static USBUIRT Create(OnRemoteCommand remoteCommandCallback)
		{
			try
			{
				if (instance!=null)
				{
					instance.Close();
					instance=null;
				}
				instance = new USBUIRT(remoteCommandCallback);
			}
			catch (Exception)
			{
			}
			return instance;
		}

		public string GetName()
		{
			return "USB-UIRT";
		}


		public string GetVersions()
		{
			if(isUsbUirtLoaded)
			{
				UUINFO p = new UUINFO();			
				UUIRTGetUUIRTInfo(UsbUirtHandle,ref p);	
				DateTime firmdate = new DateTime(p.fwDateYear + 2000,p.fwDateMonth,p.fwDateDay);
				DateTime plugdate = new DateTime(2004,4,1);
				string firmversion = (p.fwVersion>>8) +"."+(p.fwVersion&0xff);
				string plug = "Plugin Version: 1.1 ("+plugdate.ToString("MMMM, dd, yyyy")+")";
				string firm = "Firmware Version: "+firmversion+" ("+firmdate.ToString("MMMM, dd, yyyy")+")";			
				return plug+"\n"+ firm;
			}
			else
			{
				return "plugin is offline";
			}
		}

		public int GetCurrentPreferences()
		{
			uint config = 0;
			if(isUsbUirtLoaded)
				UUIRTGetUUIRTConfig(this.UsbUirtHandle,ref config);
			return (int) config;
		}

		public void SetPreferences(int pref)
		{			
			if(isUsbUirtLoaded)
				UUIRTSetUUIRTConfig(this.UsbUirtHandle,(uint)pref);
		}
		public void Close()
		{			
			if(isUsbUirtLoaded)
				UUIRTClose(UsbUirtHandle);
			isUsbUirtLoaded=false;
			UsbUirtHandle=IntPtr.Zero;
		}
		#endregion 

		#region notify events
		/// <summary>
		/// Method used to fire the "StartLearning" event. Any subscribers will be notified with the name of
		/// the button that is to be learned.
		/// </summary>
		/// <param name="button"></param>
		protected void NotifyStartLearn(string button)
		{
			if(StartLearning != null)
			{
				StartLearning(this, new LearningEventArgs(button));
			}
		}

		protected void NotifyEventLearned(string button, string ircode, bool isSuccess)
		{
			if(OnEventLearned != null)
			{
				OnEventLearned(this, new LearningEventArgs(button, ircode,isSuccess));
			}
		}
		#endregion

		#region Learning methods
		private bool IRLearn()
		{
			try
			{
				if(!UUIRTLearnIR(UsbUirtHandle,UUIRTDRV_IRFMT_UUIRT, this.ircode,null,0,ref this.abort,0,null,null))
				{
					return false;
				}
				else
				{
					//uirt-raw is the format
				}
			}
			catch(Exception )
			{
				return false;
			}
			return true;
		}

		public void LearnTunerCodes()
		{
			isLearning=true;
			bool result;
			for (int i = 0; i< 10; i++) 
			{
				for (int retry=0; retry < 3; retry++)
				{
					NotifyStartLearn(i.ToString());

					result=IRLearn();

					externalTunerCodes[i] = this.ircode.ToString();
					
					NotifyEventLearned(i.ToString(),this.ircode.ToString(),result);
					if (result) break;
				}
			}

			if (tunerNeedsEnter)
			{
				for (int retry=0; retry < 3; retry++)
				{
					NotifyStartLearn("Enter");

					result=IRLearn();

					externalTunerCodes[10] = this.ircode.ToString();

					NotifyEventLearned("Enter",this.ircode.ToString(),result);
					if (result) break;
				}
			}
			isLearning=false;
		}


		public void BulkLearn(object[] commands, string[] buttonNames)
		{
			if (commands.Length != buttonNames.Length)
				throw new Exception("invalid call to BulkLearn");

			isLearning=true;
			for(int i= 0; i< commands.Length; i++)
			{
				for (int retry=0; retry < 3; retry++)
				{
					NotifyStartLearn(buttonNames[i]);
					bool result=IRLearn();
					commandsLearned[i]=this.ircode.ToString();
					NotifyEventLearned(buttonNames[i],this.ircode.ToString(),result);
					if (result) break;
				}
			}
			isLearning=false;
		}
		#endregion

		#region remote control methods
		public void ChangeTunerChannel(string channel)
		{
			if(!isUsbUirtLoaded)
				return;

			if (!TransmitEnabled) return;

			Log.Write("USBUIRT: NewChannel={0} LastChannel={1}", channel, lastchannel);

			// Already tuned to this channel?
			if(channel == lastchannel)
				return;
			int length = channel.Length;
			if ((!this.Is3Digit && length >2) || (length >3))
			{
				Log.Write("USBUIRT: invalid channel:{0}", channel);
				return;
			}
			for (int i = 0; i<length; i++ )
			{
				if (channel[i] < '0' || channel[i] > '9')
					continue;
				Log.Write("USBUIRT: send:{0}", channel[i]);
				Transmit(this.externalTunerCodes[channel[i] - '0'], UUIRTDRV_IRFMT_UUIRT, 1);
			}

			if (this.NeedsEnter)
			{
				Log.Write("USBUIRT: send enter");
				Transmit(this.externalTunerCodes[10], UUIRTDRV_IRFMT_UUIRT, 1);
			}
			// All succeeded, remember last channel
			lastchannel = channel;
		}

		public void Transmit(string gIRCode, int gIRCodeFormat, int repeatCount)
		{
			if(!isUsbUirtLoaded) return;
			if (!TransmitEnabled) return;
			
			bool result=UUIRTTransmitIR(UsbUirtHandle,
				gIRCode, // IRCode 
				gIRCodeFormat, // codeFormat 
				repeatCount, // repeatCount 
				0, // inactivityWaitTime 
				IntPtr.Zero, // hEvent 
				0, // reserved1
				0 // reserved2 
				);
			if (!result)
				Log.Write("USBUIRT: unable to transmit code");
		}
		#endregion
		
	}
}