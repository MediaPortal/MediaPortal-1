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
//using MediaPortal.GUI.Library;
using JH.CommBase;
using System.Windows.Forms;

namespace MediaPortal.SerialIR
{
	/// <summary>
	/// This class will handle all communication with an external SerialUIR device
	/// The Serial UIR, allows your PC to Receive infrared signals -- 
	/// exactly like those used by the collection of remote controls you've acquired for your TV, 
	/// VCR, Audio System, etc. 
	/// </summary>
	public class LearningEventArgs : System.EventArgs
	{
		public string Button;
		public LearningEventArgs(string button)
		{
			this.Button = button;
		}
	}

	public delegate void StartLearningEventHandler(object sender, LearningEventArgs e);

	public class SerialUIR : CommBase
	{
		/// <summary>
		/// Private constructor disables the user from creating an instance of the class. All access
		/// to methods should be done through the singleton property "Instance".
		/// </summary>
		private SerialUIR()
		{
		}

		public delegate void OnRemoteCommand(object command);

		private int timeout = 5000; // time out in milliseconds
		private int timespan = 300; // time between 2 commands

		private bool loaded = false;

		private OnRemoteCommand remoteCommandCallback = null;

		private bool recInternalCommands = false;
		private const string remotefile = "remotevalues.xml";

		static SerialUIR instance = null;

		/// <summary>
		/// Event fired when we start learning a remote command
		/// </summary>
		public event StartLearningEventHandler StartLearning;

		public OnRemoteCommand RemoteCommandCallback
		{
			set 
			{
				remoteCommandCallback = value;
			}
		}

		public string commport = "";

		protected override CommBaseSettings CommSettings() 
		{
			CommBaseSettings cs = new CommBaseSettings();
			cs.SetStandard(commport,9600,Handshake.none);
			return cs;
		}


		Hashtable commandsLearned = new Hashtable();

		public static SerialUIR Create(OnRemoteCommand remoteCommandCallback)
		{
			try
			{
				if (instance == null)
					instance = new SerialUIR(remoteCommandCallback);
			}
			catch (Exception)
			{
			}
			return instance;
		}

		public static SerialUIR Instance 
		{
			get 
			{
				return instance;
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

		public bool SetPort(string CommPort)
		{
			commport = CommPort;
			base.Close();
			if (base.IsPortAvailable(commport) == PortStatus.available)
				return base.Open();
			else
				return false;
		}

		private SerialUIR(OnRemoteCommand remoteCommandCallback)
		{
			try
			{
				using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
				{
					recInternalCommands = xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";
					commport = xmlreader.GetValueAsString("SerialUIR", "commport", "COM1:");
				}
				this.remoteCommandCallback = remoteCommandCallback;
				commandsLearned = new Hashtable();
				if (System.IO.File.Exists(remotefile))
					LoadValues();
				base.Open();
			}
			catch(Exception )
			{
				//most users dont have serial device on their system so will get a exception here
			}
		}

		public string GetName()
		{
			return "SerialUIR";
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

		public string GetVersions()
		{
			if(loaded)
			{
				DateTime plugdate = new DateTime(2004,12,10);
				string plug = "Plugin Version: 1.1 ("+plugdate.ToString("MMMM, dd, yyyy")+")";
				return plug;
			}
			else
			{
				return "plugin is offline";
			}
		}

		/// <summary>
		/// Method used to fire the "StartLearning" event. Any subscribers will be notified with the name of
		/// the button that is to be learned.
		/// </summary>
		/// <param name="button"></param>
		protected void OnStartLearning(string button)
		{
			if(StartLearning != null)
			{
				StartLearning(this, new LearningEventArgs(button));
			}
		}

		object commandToLearn = null;
		bool   internalLearnDone = false;
		
		private bool InternalLearn(object command, string buttonName)
		{
			try
			{
				this.commandToLearn = command;
				internalLearnDone = false;

				OnStartLearning(buttonName);
				
				timestamp = DateTime.Now;
					
				while (! internalLearnDone )
				{
					base.Sleep(10);
					if ((DateTime.Now - timestamp) > new TimeSpan(0,0,0,0,timeout)) 
						return false;
				}
				return true;
			}
			catch(Exception )
			{
				return false;
			}
		}

		bool learning = false;
		public void BulkLearn(object[] commands, string[] buttonNames)
		{
			//erase old learn
			commandsLearned = new Hashtable();
			if (commands.Length != buttonNames.Length)
				throw new Exception("invalid call to BulkLearn");
			for(int i= 0; i< commands.Length; i++)
			{
				learning = true;
				if (! InternalLearn(commands[i], buttonNames[i]))
				{
					switch(MessageBox.Show("Learn failed (timeout). Try again ?","IR Learn failed",MessageBoxButtons.AbortRetryIgnore,MessageBoxIcon.Exclamation,MessageBoxDefaultButton.Button1))
					{
						case DialogResult.Abort:
							return;
						case DialogResult.Retry:
							i--;
							continue;
					}
				}
			}
		}

		bool ignore = true;
		bool initok = false;
		byte lastbyte = 0x00;
		public int charcount=0;
		string lastcommand;
		string lastcommand2;
		string lastbuff;

		//string lastCommand = null;
		DateTime timestamp = DateTime.Now;

		protected override void OnRxChar(byte ch) 
		{
			if (ignore)
				return;
			if (! initok)
			{
				if (ch == (byte)'O')
					lastbyte = ch;
				if (ch == (byte)'K' && lastbyte == (byte)'O')
				{
					initok = true;
				}
				return;
			}
			charcount = (charcount+1) % 6;
			if(charcount == 0)
			{
				lastcommand2 = lastcommand;
				lastcommand = lastbuff + ch.ToString();
				lastbuff = "";
//				Debug.WriteLine(lastcommand);

				if(lastcommand.Equals(lastcommand2))
				{
					if (learning)
					{
						if (commandsLearned[lastcommand] == null)
						{
							learning = false;
							internalLearnDone = true;
							commandsLearned[lastcommand] = commandToLearn;
						}
					}
					else
					{
						internalLearnDone = true;
						Trace.WriteLine("received IR command " + lastcommand + " "+DateTime.Now.ToString());
						if (recInternalCommands)
						{
							object command = commandsLearned[lastcommand];
							if ((DateTime.Now - timestamp) > new TimeSpan(0,0,0,0,timespan)) 
							{
								Trace.WriteLine("invoking callback");
								this.remoteCommandCallback(command);
								timestamp = DateTime.Now;
							}
						}
					}
				}
			}
			else
				lastbuff += ch.ToString() + " ";
		}

		protected override bool AfterOpen() 
		{
			base.RTS = true;
			base.DTR = true;
			Sleep(50);
			ignore = false;
			base.SendImmediate((byte)'I');
			Sleep(5);
			base.SendImmediate((byte)'R');
			return true;
		}

		
	}
}