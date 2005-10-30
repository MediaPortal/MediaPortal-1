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
		public bool		Succeeded = false;
		public string	Button;
		public string	IrCode = String.Empty;
		public bool		IsToggledIrCode = false;
		public int		TotalCodeCount = 0;
		public int		CurrentCodeCount = 0;
		
		public LearningEventArgs(string button, string ircode, bool succeeded, 
			bool capturingToggledIrCode, int totalCodeCount, int curCodeCount)
		{
			this.Button = button;
			this.IrCode = ircode;
			this.Succeeded=succeeded;
			this.IsToggledIrCode = capturingToggledIrCode;
			this.TotalCodeCount = totalCodeCount;
			this.CurrentCodeCount = curCodeCount;
		}

		public LearningEventArgs(string button, bool capturingToggledIrCode, 
			int totalCodeCount, int curCodeCount)
		{
			this.Button = button;
			this.IsToggledIrCode = capturingToggledIrCode;
			this.TotalCodeCount = totalCodeCount;
			this.CurrentCodeCount = curCodeCount;
		}
	}
	#endregion

	public class USBUIRT : IDisposable
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
		public delegate void EndLearnedHandler(object sender, EventArgs e);
	
		//private delegate void UUIRTReceiveCallbackDelegate( string val );
		private delegate void UUIRTReceiveCallbackDelegate(string val, IntPtr reserved);
		
		public delegate void IRLearnCallbackDelegate( uint val, uint val2, ulong val3);
		public delegate void OnRemoteCommand(object command);
		#endregion

		#region constants
		static int UUIRTDRV_IRFMT_UUIRT	= 0x0000;
		private const string		remotefile = "UIRTUSB-remote.xml";
		private const string		tunerfile = "UIRTUSB-tuner.xml";
		#endregion

		#region variables
		private IntPtr							UsbUirtHandle = IntPtr.Zero;
		private StringBuilder					ircode = new StringBuilder("1", 2048);
		private int								abort = 0;
		private int								timelaps = 300; // time in milliseconds between two accepted commands
		private IntPtr							empty = new IntPtr(-1);
		private bool							isUsbUirtLoaded = false;
		private string							lastchannel;
		private OnRemoteCommand					remoteCommandCallback = null;
		private UUIRTReceiveCallbackDelegate	urcb = null;
		private bool 							accepRemoteCommands = false;
		private bool 							transmitEventsEnabled = false;
		private bool 							is3DigitTuner = false;
		private bool 							tunerNeedsEnter = false;
		private static USBUIRT					instance = null;
		private string[]						externalTunerCodes = new string[11]; // 10 digits + Enter
		private Hashtable						commandsLearned = new Hashtable();
		private DateTime						timestamp = DateTime.Now;
		private bool							isLearning = false;


		private int								currentButtonIndex = 0;
		private string[]						controlCodeButtonNames;
		private object[]						controlCodeCommands;
		private bool							waitingForIrRxLearnEvent = false;
		private bool							capturingToggledIrCode = false;
		private bool							abortLearn = false;
		private bool							skipLearnForCurrentCode = false;
		private bool							disposed = false;

		private int								commandRepeatCount = 1;
		private int								interCommandDelay = 100;
		private bool							tunerCodesLoaded = false;

		#endregion

		#region events
		public event StartLearningEventHandler	StartLearning;
		public event EventLearnedHandler		OnEventLearned;
		public event EndLearnedHandler			OnEndLearning;
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

		public bool AbortLearn
		{
			set
			{
				abortLearn = value;
				this.abort = abortLearn ? 1 : 0;

				if(abortLearn)
				{
					if(isLearning && waitingForIrRxLearnEvent)
					{
						isLearning = false;
						waitingForIrRxLearnEvent = false;
						capturingToggledIrCode = false;
						this.NotifyTrainingComplete();
					}

					else
					{
						isLearning = false;
						waitingForIrRxLearnEvent = false;
						capturingToggledIrCode = false;
					}
				}
			}
			get
			{
				return abortLearn;
			}
		}

		public bool SkipLearnForCurrentCode
		{
			get
			{
				return skipLearnForCurrentCode;
			}
			set
			{
				skipLearnForCurrentCode = value;
				this.abort = skipLearnForCurrentCode ? 1 : 0;

				if(skipLearnForCurrentCode)
				{
					if(isLearning && waitingForIrRxLearnEvent)
					{
						if(currentButtonIndex < controlCodeButtonNames.Length - 1 || 
							currentButtonIndex < controlCodeButtonNames.Length && !capturingToggledIrCode)
						{
							this.LearnNextCode();
						}

						else
						{
							//isLearning = false;
							//waitingForIrRxLearnEvent = false;
							isLearning = false;
							NotifyTrainingComplete();
						}
					}
				}
			}
		}

		public int CommandRepeatCount
		{
			get
			{
				return commandRepeatCount;
			}
			set
			{
				commandRepeatCount = value;
			}
		}

		public int InterCommandDelay
		{
			get
			{
				return interCommandDelay;
			}
			set
			{
				interCommandDelay = value;
			}
		}

		public bool TunerCodesLoaded
		{
			get
			{
				return tunerCodesLoaded;
			}
		}

		public bool IsUsbUirtLoaded
		{
			get
			{
				return isUsbUirtLoaded;
			}
		}

		#endregion

		#region ctor / dtor
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
					

					Log.Write("USBUIRT:Open success:{0}",GetVersions());
				}
				else
				{
					Log.Write("USBUIRT:Unable to open USBUIRT driver");
				}
				if(isUsbUirtLoaded)
				{
					using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
					{
						ReceiveEnabled = xmlreader.GetValueAsBool("USBUIRT", "internal", false) ;
						TransmitEnabled = xmlreader.GetValueAsBool("USBUIRT", "external", false) ;
						Is3Digit = xmlreader.GetValueAsBool("USBUIRT", "is3digit", false) ;
						tunerNeedsEnter = xmlreader.GetValueAsBool("USBUIRT", "needsenter", false) ;

						CommandRepeatCount = xmlreader.GetValueAsInt("USBUIRT", "repeatcount", 2) ;
						InterCommandDelay = xmlreader.GetValueAsInt("USBUIRT", "commanddelay", 100) ;
					}

					if (System.IO.File.Exists(remotefile))
					{
						LoadValues();
					}

					else 
						Log.Write("USBUIRT:unable to load values from:{0}", remotefile);
					
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

		~USBUIRT()
		{
			Dispose(false);
		}

		#endregion


		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposeManagedResources)
		{
			if(!this.disposed)
			{
				disposed = true;

				if(disposeManagedResources)
				{
					// Dispose any managed resources.
				}

				IntPtr emptyPtr = new IntPtr(-1);

				if(isUsbUirtLoaded && UsbUirtHandle != emptyPtr && UsbUirtHandle != IntPtr.Zero)
				{
					UUIRTClose(UsbUirtHandle);
					UsbUirtHandle = IntPtr.Zero;
					isUsbUirtLoaded = false;
				}
			}
		}

		#endregion

		#region serialisation
		private void LoadValues()
		{
			try
			{
				System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
				xmlDoc.Load(remotefile);
				System.Xml.XmlNodeList entryNodes = xmlDoc.GetElementsByTagName("entry");

				Console.WriteLine(entryNodes.Count.ToString());

				foreach(System.Xml.XmlNode node in entryNodes)
				{		
					System.Xml.XmlAttributeCollection codeAttribs = node.Attributes;
				
					if(codeAttribs != null)
					{
						string irCode = node.InnerText;
						string sActionID = codeAttribs["actionID"].InnerText;
						string actionDesc = codeAttribs["actionDescription"].InnerText;

						if(sActionID.Length > 0)
						{
							int nActionID = int.Parse(sActionID);
							commandsLearned[irCode] = (Action.ActionType)nActionID;
						}
					}
				}
			}

			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void LoadTunerValues()
		{
			try
			{
				System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
				xmlDoc.Load(tunerfile);
				System.Xml.XmlNodeList entryNodes = xmlDoc.GetElementsByTagName("entry");

				Console.WriteLine(entryNodes.Count.ToString());

				foreach(System.Xml.XmlNode node in entryNodes)
				{		
					System.Xml.XmlAttributeCollection codeAttribs = node.Attributes;
			
					if(codeAttribs != null)
					{
						string remoteCode = node.InnerText;
						string sIndex = codeAttribs["index"].InnerText;

						if(sIndex.Length > 0)
						{
							int index = int.Parse(sIndex);
							externalTunerCodes[index] = remoteCode;
						}
					}
				}

				tunerCodesLoaded = true;
			}

			catch(Exception ex)
			{
				tunerCodesLoaded = false;
				Console.WriteLine(ex.Message);
			}
		}

		public bool SaveInternalValues()
		{
			bool result = false;
			System.Xml.XmlTextWriter writer = null;

			try
			{
				writer = new System.Xml.XmlTextWriter(remotefile, System.Text.Encoding.Unicode);
				writer.Formatting = System.Xml.Formatting.Indented;
				writer.WriteStartElement("docElement");

				// We really should do a sort before writing out to a file...

				foreach(object entry in commandsLearned.Keys) 
				{
					string irCode = entry.ToString();
					object command = commandsLearned[irCode];
					
					writer.WriteStartElement("entry");
					writer.WriteAttributeString("actionID", Convert.ToInt32(command).ToString());
					writer.WriteAttributeString("actionDescription", command.ToString().Replace("ACTION_", ""));
					writer.WriteString(irCode);
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				result = true;
			}

			catch(Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.Message);
			}

			finally
			{
				if(writer != null)
					writer.Close();

				writer = null;
			}

			return result;
		}

		public bool SaveTunerValues()
		{
			bool result = false;
			System.Xml.XmlTextWriter writer = null;
			
			try
			{
				writer = new System.Xml.XmlTextWriter(tunerfile, System.Text.Encoding.Unicode);
				writer.Formatting = System.Xml.Formatting.Indented;
				writer.WriteStartElement("docElement");

				for(int i=0; i<11; i++)
				{
					writer.WriteStartElement("entry");
					writer.WriteAttributeString("index", i.ToString());
					writer.WriteString(externalTunerCodes[i]);
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				result = true;
			}

			catch(Exception)
			{
			}

			finally
			{
				if(writer != null)
				{
					writer.Close();

					writer = null;
				}
			}

			tunerCodesLoaded = result;
			return result;
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

		//public void UUIRTReceiveCallback( string irid )
		public void UUIRTReceiveCallback(string irid, IntPtr reserved)
		{
			if(!ReceiveEnabled) 
				return;

			object command = commandsLearned[irid];
			
			if (command == null && !isLearning) 
				return;
			
			TimeSpan ts = DateTime.Now - timestamp;
			
			if (ts.TotalMilliseconds >= timelaps) 
			{
				if(isLearning && waitingForIrRxLearnEvent && ts.TotalMilliseconds > 500)
				{
					if(AbortLearn)
					{
						isLearning = false;
						waitingForIrRxLearnEvent = false;

						NotifyTrainingComplete();
					}

					commandsLearned[irid] = controlCodeCommands[currentButtonIndex];
					int totCodeCount = controlCodeButtonNames.Length;
					int curCodeIndex = currentButtonIndex + 1;
					
					waitingForIrRxLearnEvent = false;
					NotifyEventLearned(controlCodeButtonNames[currentButtonIndex], irid, true, totCodeCount, curCodeIndex);

					//Console.WriteLine("In UUIRTReceiveCallback...");
					
					if(currentButtonIndex < controlCodeButtonNames.Length - 1 || 
						currentButtonIndex < controlCodeButtonNames.Length && !capturingToggledIrCode)
					{
						this.LearnNextCode();
					}

					else
					{
						isLearning = false;
						NotifyTrainingComplete();
					}
				}

				else if(command != null)
				{
					if(remoteCommandCallback != null)
						remoteCommandCallback(command);
				}
				
				timestamp = DateTime.Now;
			}
		}

		#endregion

		#region methods
		public static USBUIRT Create(OnRemoteCommand remoteCommandCallback)
		{
			try
			{
				if(instance == null)
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
			this.Dispose();
		}

		#endregion 

		#region notify events
		/// <summary>
		/// Method used to fire the "StartLearning" event. Any subscribers will be notified with the name of
		/// the button that is to be learned.
		/// </summary>
		/// <param name="button"></param>
		//protected void NotifyStartLearn(string button)
		protected void NotifyStartLearn(string button, int totCodeCount, int curCodeIndex)
		{
			if(StartLearning != null)
			{
				StartLearning(this, new LearningEventArgs(button, capturingToggledIrCode, totCodeCount, curCodeIndex));
			}
		}

		protected void NotifyEventLearned(string button, string ircode, bool isSuccess, int totCodeCount, int curCodeIndex)
		{
			if(OnEventLearned != null)
			{
				OnEventLearned(this, new LearningEventArgs(button, ircode, isSuccess, 
					capturingToggledIrCode, totCodeCount, curCodeIndex));
			}
		}

		protected void NotifyTrainingComplete()
		{
			if(OnEndLearning != null)
			{
				OnEndLearning(this, EventArgs.Empty);
			}
		}
		
		#endregion

		#region Learning methods
		private bool IRLearn()
		{
			try
			{
				if(!UUIRTLearnIR(UsbUirtHandle, UUIRTDRV_IRFMT_UUIRT, this.ircode, null, 0, ref this.abort, 0, null, null))
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
			System.Threading.ThreadStart learnThreadStarter = new ThreadStart(LearnTunerCodesAsync);
			System.Threading.Thread learnThread = new System.Threading.Thread(learnThreadStarter);
			learnThread.Start();
		}

		public void LearnTunerCodesAsync()
		{
			skipLearnForCurrentCode = false;
			AbortLearn = false;
			isLearning = true;
			int retries = 3;
			bool result;
			int totCodeCount = 10 + (tunerNeedsEnter ? 1 : 0); 

			for (int i = 0; i < 10; i++) 
			{
				if(skipLearnForCurrentCode)
				{
					skipLearnForCurrentCode = false;
					abort = 0;
				}

				if(abortLearn)
					break;

				for(int retry = 0; retry < retries; retry++)
				{
					NotifyStartLearn(i.ToString(), totCodeCount, i);
					result = IRLearn();

					if(abort == 1 || abortLearn || skipLearnForCurrentCode)
						break;

					else
						externalTunerCodes[i] = this.ircode.ToString();

					NotifyEventLearned(i.ToString(), this.ircode.ToString(), result, totCodeCount, i + 1);
					
					if(result) 
						break;
				}
			}

			// Always learn the "Enter" key. This will prevent having to re-learn all of the 
			// codes if the user later enables "Send 'Enter' for changing channels".
			//if(tunerNeedsEnter && !abortLearn)
			if(!abortLearn)
			{
				for(int retry = 0; retry < 3; retry++)
				{
					if(skipLearnForCurrentCode)
					{
						skipLearnForCurrentCode = false;
						abort = 0;
					}

					NotifyStartLearn("Enter", totCodeCount, totCodeCount - 1);
					result = IRLearn();

					if(abort == 1 || abortLearn || skipLearnForCurrentCode)
						break;

					else					
						externalTunerCodes[10] = this.ircode.ToString();

					NotifyEventLearned("Enter", this.ircode.ToString(), result, totCodeCount, totCodeCount - 1);
					
					if(result) 
						break;
				}
			}

			isLearning = false;
			NotifyTrainingComplete();
		}

		public void BulkLearn(object[] commands, string[] buttonNames)
		{
			commandsLearned.Clear();
			controlCodeCommands = commands;
			controlCodeButtonNames = buttonNames;
			capturingToggledIrCode = false;

			if (commands.Length != buttonNames.Length)
				throw new Exception("invalid call to BulkLearn");

			skipLearnForCurrentCode = false;
			AbortLearn = false;
			currentButtonIndex = 0;
			isLearning = true;
			waitingForIrRxLearnEvent = true;

			NotifyStartLearn(controlCodeButtonNames[currentButtonIndex], commands.Length, currentButtonIndex);
		}

		private void LearnNextCode()
		{
			// Certain code formats such as Philips RC5 and RC6 toggle a bit on consecutive key presses.  
			// To catch these we need to capture 2 seperate button presses for each button...
			capturingToggledIrCode = !capturingToggledIrCode;
			NotifyStartLearn(controlCodeButtonNames[capturingToggledIrCode ? currentButtonIndex : ++currentButtonIndex], controlCodeCommands.Length, currentButtonIndex);
			
			waitingForIrRxLearnEvent = true;
			isLearning = true;
		}

		#endregion

		#region remote control methods
		public void ChangeTunerChannel(string channel, bool ignoreLastChannel)
		{
			if(ignoreLastChannel)
				lastchannel = "";

			ChangeTunerChannel(channel);
		}
		
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
			
			// Some STB's allow more than 3 digit channel numbers!
			//if ((!this.Is3Digit && length >2) || (length >3))
			if(this.Is3Digit && length > 3)
			{
				Log.Write("USBUIRT: invalid channel:{0}", channel);
				return;
			}

			for (int i = 0; i < length; i++ )
			{
				if (channel[i] < '0' || channel[i] > '9')
					continue;
				Log.Write("USBUIRT: send:{0}", channel[i]);
				Transmit(this.externalTunerCodes[channel[i] - '0'], UUIRTDRV_IRFMT_UUIRT, commandRepeatCount);
			}

			if (this.NeedsEnter)
			{
				Log.Write("USBUIRT: send enter");
				Transmit(this.externalTunerCodes[10], UUIRTDRV_IRFMT_UUIRT, commandRepeatCount);
			}
			
			// All succeeded, remember last channel
			lastchannel = channel;
		}

		public void Transmit(string gIRCode, int gIRCodeFormat, int repeatCount)
		{
			if(!isUsbUirtLoaded) return;
			if (!TransmitEnabled) return;
			
			bool result = UUIRTTransmitIR(UsbUirtHandle,
				gIRCode,		// IRCode 
				gIRCodeFormat,	// codeFormat 
				repeatCount,	// repeatCount 
				0,				// inactivityWaitTime 
				IntPtr.Zero,	// hEvent 
				0,				// reserved1
				0				// reserved2 
				);

			if (!result)
				Log.Write("USBUIRT: unable to transmit code");

			else
				System.Threading.Thread.Sleep(interCommandDelay);
		}
		#endregion
	}
}