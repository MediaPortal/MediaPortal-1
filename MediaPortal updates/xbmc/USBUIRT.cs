/*
 * Created by SharpDevelop.
 * Date: 7/7/2004
 * Time: 8:22 AM
 * 
 */
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Collections;
using System.Windows.Forms;


	public class USBUIRT
	{
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



		private delegate void UUIRTReceiveCallbackDelegate( string val );
		public delegate void IRLearnCallbackDelegate( uint val, uint val2, ulong val3);
		public delegate void OnRemoteCommand(object command);

		static int UUIRTDRV_IRFMT_UUIRT	= 0x0000;
		//static int UUIRTDRV_IRFMT_PRONTO=	0x0010;

		private IntPtr handle = IntPtr.Zero;
		private  StringBuilder ircode = new StringBuilder("1",2048);

		private int abort = 0;
		private int timelaps = 300; // time in milliseconds between two accepted commands

		private UUIRTReceiveCallbackDelegate urcb = null;

		private IntPtr empty = new IntPtr(-1);
		private bool loaded = false;

		private OnRemoteCommand remoteCommandCallback = null;
		private bool recInternalCommands = false;
		private bool recExternalCommands = false;
		private bool is3DigitTuner = false;
		private bool tunerNeedsEnter = false;
		private const string remotefile = "remotevalues.xml";
		private const string tunerfile = "tunervalues.xml";

		MediaPortal.IRLearnFORM form = new MediaPortal.IRLearnFORM();

		static USBUIRT instance = null;
		string[] externalTunerCodes = new string[11]; // 10 digits + Enter


		public OnRemoteCommand RemoteCommandCallback
		{
		set 
			{
			remoteCommandCallback = value;
			}
		}
		
		Hashtable commandsLearned = new Hashtable();

		public static USBUIRT Create(OnRemoteCommand remoteCommandCallback)
		{
			if (instance == null)
				instance = new USBUIRT(remoteCommandCallback);
			return instance;
		}

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


		public bool InternalCommandsActive 
		{
			get 
			{
				return this.recInternalCommands;
			}

			set 
			{
				this.recInternalCommands = value;
			}
		}

		public bool ExternalCommandsActive 
		{
			get 
			{
				return this.recExternalCommands;
			}

			set 
			{
				this.recExternalCommands = value;
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

		private USBUIRT(OnRemoteCommand remoteCommandCallback)
		{
			try
			{
				handle = UUIRTOpen();
				if(handle !=  empty )
					loaded = true;

				if(loaded)
				{
					urcb = new UUIRTReceiveCallbackDelegate(this.UUIRTReceiveCallback);		
					UUIRTSetReceiveCallback(handle,urcb,0);
					using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
					{
						recInternalCommands = xmlreader.GetValueAsString("USBUIRT", "internal", "false") == "true";
						recExternalCommands = xmlreader.GetValueAsString("USBUIRT", "external", "false") == "true";
						is3DigitTuner = xmlreader.GetValueAsString("USBUIRT", "is3digit", "false") == "true";
						tunerNeedsEnter = xmlreader.GetValueAsString("USBUIRT", "needsenter", "false") == "true";
					}
					this.remoteCommandCallback = remoteCommandCallback;
					commandsLearned = new Hashtable();
					if (System.IO.File.Exists(remotefile))
						LoadValues();
					if (System.IO.File.Exists(tunerfile))
						LoadValues();

				}
			}
			catch(Exception )
			{
				//most users dont have the dll on their system so will get a exception here
			}

			//Thread t = new Thread(new ThreadStart(s));
			//t.Start();
			//ShowDialog();
			//int slots = 4;
			//UUIRTGetUUIRTGPIOCfg(handle, ref slots, ref uint m,
		}

		public string GetName()
		{
			return "USB-UIRT";
		}


		private void LoadValues()
		{
			System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(remotefile);
			reader.MoveToContent();
			// skip the document element
			reader.Read();

			while(!reader.EOF) 
			{
				Debug.Assert(reader.LocalName == "entry");
				int actionInt = Int32.Parse(reader.GetAttribute("actionInt"));
				reader.Read();
				string remoteCode = reader.ReadString();
				reader.ReadEndElement();
				commandsLearned[remoteCode] = actionInt;
				if(reader.LocalName != "entry")
					break;
			}
			reader.Close();
			
		}

		private void LoadTunerValues()
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


		public void SaveInternalValues()
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

		public void SaveTunerValues()
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


		public string GetVersions()
		{
			if(loaded)
			{
				UUINFO p = new UUINFO();			
				UUIRTGetUUIRTInfo(handle,ref p);	
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
			if(loaded)
				UUIRTGetUUIRTConfig(this.handle,ref config);
			return (int) config;
		}

		public void SetPreferences(int pref)
		{			
			if(loaded)
				UUIRTSetUUIRTConfig(this.handle,(uint)pref);
		}



		private void IRLearn()
		{
			try
			{
				if(!UUIRTLearnIR(handle,UUIRTDRV_IRFMT_UUIRT, this.ircode,null,0,ref this.abort,0,null,null))
				{
					//Console.WriteLine("ERROR calling UUIRTLearnIR!");
				}
				else
				{
					//uirt-raw is the format
				}
			}
			catch(Exception )
			{
			}
		}

		public void LearnTunerCodes()
		{
			for (int i = 0; i< 10; i++) 
			{
				form.label1.Text = "Press and hold the '" + i + "' button on your TUNER remote";
				form.Show();

				form.Refresh();

				IRLearn();

				externalTunerCodes[i] = this.ircode.ToString();

				form.Hide();
				Thread.Sleep(1000);
			}
			form.label1.Text = "Press and hold the 'ENTER' button on your TUNER remote";
			form.Show();

			form.Refresh();

			IRLearn();

			externalTunerCodes[10] = this.ircode.ToString();

			form.Hide();
		}

		public void Close()
		{			
			if(loaded)
				UUIRTClose(handle);
		}

		object commandToLearn = null;
		bool   internalLearnDone = false;
		
		private void InternalLearn(object command, string buttonName) {
			this.commandToLearn = command;
			internalLearnDone = false;
			form.label1.Text = "Press the " + buttonName + " button on your remote";
			form.Show();

			form.Refresh();
			

			UUIRTReceiveCallbackDelegate urcblearn = new UUIRTReceiveCallbackDelegate(this.UUIRTReceiveCallbackLearn);		
			UUIRTSetReceiveCallback(handle,urcblearn,0);
			while (! internalLearnDone ){
				Thread.Sleep(100);
			}
			UUIRTSetReceiveCallback(handle,urcb,0);
			form.Hide();
		}

		public void BulkLearn(object[] commands, string[] buttonNames)
		{
			if (commands.Length != buttonNames.Length)
				throw new Exception("invalid call to BulkLearn");
			for(int i= 0; i< commands.Length; i++)
				InternalLearn(commands[i], buttonNames[i]);
		}


		//string lastCommand = null;
		DateTime timestamp = DateTime.Now;
		
		public void UUIRTReceiveCallbackLearn( string irid )
		{
			internalLearnDone = true;
			commandsLearned[irid] = commandToLearn;			
			timestamp = DateTime.Now;
		}


		public void UUIRTReceiveCallback( string irid )
		{
			internalLearnDone = true;
			//Trace.WriteLine("received IR command "+DateTime.Now.ToString());
			if (recInternalCommands){
				object command = commandsLearned[irid];
				if ((DateTime.Now - timestamp) > new TimeSpan(0,0,0,0,timelaps) && command != null) 
				{

					Trace.WriteLine("invoking callback");
					this.remoteCommandCallback(command);
					timestamp = DateTime.Now;
				}
			}
		}

		public void ChangeTunerChannel(string channel)
		{
			int length = channel.Length;
			if ((!this.is3DigitTuner && length >2) || (length >3))
				throw new System.Exception("invalid channel length");

			for (int i = 0; i<length; i++ )
			{
				if (channel[i] < '0' || channel[i] > '9')
					throw new System.Exception("invalid digit in channel: "+channel);
				Transmit(this.externalTunerCodes[channel[i] - '0'], UUIRTDRV_IRFMT_UUIRT, 1);
			}

			if (this.NeedsEnter)
			    Transmit(this.externalTunerCodes[10], UUIRTDRV_IRFMT_UUIRT, 1);

		}

		public void Transmit(string gIRCode, int gIRCodeFormat, int repeatCount)
		{
			if(loaded){

				UUIRTTransmitIR(handle,
					gIRCode, // IRCode 
					gIRCodeFormat, // codeFormat 
					repeatCount, // repeatCount 
					0, // inactivityWaitTime 
					IntPtr.Zero, // hEvent 
					0, // reserved1
					0 // reserved2 
					);
			}
		}
		
	}
