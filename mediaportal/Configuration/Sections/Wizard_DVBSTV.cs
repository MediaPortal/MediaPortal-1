using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
namespace MediaPortal.Configuration.Sections
{
	public class Wizard_DVBSTV : MediaPortal.Configuration.SectionSettings
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label labelStatus;
		

		struct TPList
		{
			public int TPfreq; // frequency
			public int TPpol;  // polarisation 0=hori, 1=vert
			public int TPsymb; // symbol rate
		}
		enum State
		{
			ScanStart,
			ScanTransponders,
			ScanChannels
		}
		TVCaptureDevice											captureCard;
		int                                 currentIndex=-1;
		TPList[]														transp=new TPList[800];
		int																	count = 0;
		
		int newChannels, updatedChannels;
		int																	newRadioChannels, updatedRadioChannels;
		int m_diseqcLoops=1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ProgressBar progressBar2;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ProgressBar progressBar3;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ComboBox cbTransponder;
		int m_currentDiseqc=1;

		public Wizard_DVBSTV() : this("DVB-S TV")
		{
		}

		public Wizard_DVBSTV(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.labelStatus = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.button1 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.progressBar2 = new System.Windows.Forms.ProgressBar();
			this.button2 = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.cbTransponder = new System.Windows.Forms.ComboBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label6 = new System.Windows.Forms.Label();
			this.progressBar3 = new System.Windows.Forms.ProgressBar();
			this.button3 = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			// 
			// labelStatus
			// 
			this.labelStatus.Location = new System.Drawing.Point(0, 0);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.TabIndex = 0;
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(0, 0);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(0, 0);
			this.button1.Name = "button1";
			this.button1.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(0, 0);
			this.label2.Name = "label2";
			this.label2.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			// 
			// groupBox2
			// 
			this.groupBox2.Location = new System.Drawing.Point(0, 0);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabIndex = 0;
			this.groupBox2.TabStop = false;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(0, 0);
			this.label3.Name = "label3";
			this.label3.TabIndex = 0;
			// 
			// progressBar2
			// 
			this.progressBar2.Location = new System.Drawing.Point(0, 0);
			this.progressBar2.Name = "progressBar2";
			this.progressBar2.TabIndex = 0;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(0, 0);
			this.button2.Name = "button2";
			this.button2.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(0, 0);
			this.label4.Name = "label4";
			this.label4.TabIndex = 0;
			// 
			// comboBox1
			// 
			this.comboBox1.Location = new System.Drawing.Point(0, 0);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.TabIndex = 0;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(0, 0);
			this.label5.Name = "label5";
			this.label5.TabIndex = 0;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.cbTransponder);
			this.groupBox3.Controls.Add(this.panel1);
			this.groupBox3.Controls.Add(this.label6);
			this.groupBox3.Controls.Add(this.progressBar3);
			this.groupBox3.Controls.Add(this.button3);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Controls.Add(this.label8);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(4, 4);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(480, 360);
			this.groupBox3.TabIndex = 1;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Setup digital tv (DVBS Satellite)";
			// 
			// cbTransponder
			// 
			this.cbTransponder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbTransponder.Location = new System.Drawing.Point(136, 72);
			this.cbTransponder.Name = "cbTransponder";
			this.cbTransponder.Size = new System.Drawing.Size(192, 21);
			this.cbTransponder.TabIndex = 14;
			this.cbTransponder.SelectedIndexChanged += new System.EventHandler(this.cbTransponder_SelectedIndexChanged);
			// 
			// panel1
			// 
			this.panel1.Location = new System.Drawing.Point(432, 328);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1, 1);
			this.panel1.TabIndex = 13;
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label6.Location = new System.Drawing.Point(40, 169);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(400, 87);
			this.label6.TabIndex = 11;
			// 
			// progressBar3
			// 
			this.progressBar3.Location = new System.Drawing.Point(32, 128);
			this.progressBar3.Name = "progressBar3";
			this.progressBar3.Size = new System.Drawing.Size(416, 16);
			this.progressBar3.TabIndex = 4;
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(344, 72);
			this.button3.Name = "button3";
			this.button3.TabIndex = 3;
			this.button3.Text = "Scan...";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(32, 72);
			this.label7.Name = "label7";
			this.label7.TabIndex = 2;
			this.label7.Text = "Transponder:";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(24, 24);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(432, 40);
			this.label8.TabIndex = 0;
			this.label8.Text = "Mediaportal has detected one or more digital Tv cards. Select your transponder an" +
				"d press auto tune to scan for the tv and radio channels";
			// 
			// Wizard_DVBSTV
			// 
			this.Controls.Add(this.groupBox3);
			this.Name = "Wizard_DVBSTV";
			this.Size = new System.Drawing.Size(488, 368);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion






		public override void OnSectionActivated()
		{
			labelStatus.Text="";
			cbTransponder.Items.Clear();
			string [] files = System.IO.Directory.GetFiles( System.IO.Directory.GetCurrentDirectory()+@"\Tuningparameters");
			foreach (string file in files)
			{
				if (file.ToLower().IndexOf(".tpl") >=0)
				{
					cbTransponder.Items.Add(System.IO.Path.GetFileName(file));
				}
			}
			if (cbTransponder.Items.Count>0)
				cbTransponder.SelectedIndex=0;

		}

		void LoadFrequencies()
		{
			m_diseqcLoops=1;
			string filename=String.Format(@"database\card_{0}.xml",captureCard.FriendlyName);
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(filename))
			{
				if(xmlreader.GetValueAsBool("dvbs","useLNB2",false)==true)
					m_diseqcLoops++;
				if(xmlreader.GetValueAsBool("dvbs","useLNB3",false)==true)
					m_diseqcLoops++;
				if(xmlreader.GetValueAsBool("dvbs","useLNB4",false)==true)
					m_diseqcLoops++;
			}

			currentIndex=-1;

			string countryName=(string)cbTransponder.SelectedItem;
			if (countryName==String.Empty) return;
			count = 0;
			string line;
			string[] tpdata;
			Log.WriteFile(Log.LogType.Capture,"Opening {0}",countryName);
			// load transponder list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(System.IO.Directory.GetCurrentDirectory()+@"\Tuningparameters\"+countryName);
			
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
					if (line.Length > 0)
					{
						if(line.StartsWith(";"))
							continue;
						tpdata = line.Split(new char[]{','});
						if(tpdata.Length!=3)
							tpdata = line.Split(new char[]{';'});
						if (tpdata.Length == 3)
						{
							try
							{
			
								transp[count].TPfreq = Int32.Parse(tpdata[0]) *1000;
								switch (tpdata[1].ToLower())
								{
									case "v":
						
										transp[count].TPpol = 1;
										break;
									case "h":
						
										transp[count].TPpol = 0;
										break;
									default:
						
										transp[count].TPpol = 0;
										break;
								}
								transp[count].TPsymb = Int32.Parse(tpdata[2]);
								count += 1;
							}
							catch
							{}
						}
					}
			} while (!(line == null));
			tin.Close();
		}
	

		private void button1_Click(object sender, System.EventArgs e)
		{

		}

		private void DoScan()
		{
			GUIGraphicsContext.form=this.FindForm();
			GUIGraphicsContext.VideoWindow=new Rectangle(panel1.Location,panel1.Size);

			m_currentDiseqc=0;
			currentIndex=-1;
			while (m_currentDiseqc <m_diseqcLoops)
			{
				while (currentIndex < count)
				{
					int index=currentIndex;
					if (index<0) index=0;
					
					float percent = ((float)index) / ((float)count);
					percent *= 100.0f;
					progressBar1.Value=((int)percent);
					TPList transponder=transp[index];
					string chanDesc=String.Format("freq:{0} Khz, Pol:{1} SR:{2}",transponder.TPfreq, transponder.TPpol, transponder.TPsymb );
					string description=String.Format("Transponder:{0}/{1} {2}", index,count,chanDesc);
					labelStatus.Text=(description);
					Application.DoEvents();

					ScanNextTransponder();
					if (captureCard.SignalPresent())
					{
						description=String.Format("Found signal for transponder:{0} {1}, Scanning channels", currentIndex,chanDesc);
						labelStatus.Text=(description);
						Application.DoEvents();
						ScanChannels();
					}
				}			
			}
			captureCard.DeleteGraph();

			MapTvToOtherCards(captureCard.ID);
			MapRadioToOtherCards(captureCard.ID);
			captureCard=null;
		}
		void MapTvToOtherCards(int id)
		{
			ArrayList tvchannels = new ArrayList();
			TVDatabase.GetChannelsForCard(ref tvchannels,id);
			TVCaptureCards cards = new TVCaptureCards();
			cards.LoadCaptureCards();
			foreach (TVCaptureDevice dev in cards.captureCards)
			{
				if (dev.Network==NetworkType.DVBS && dev.ID != id)
				{
					foreach (TVChannel chan in tvchannels)
					{
						TVDatabase.MapChannelToCard(chan.ID,dev.ID);
					}
				}
			}
		}
		void MapRadioToOtherCards(int id)
		{
			ArrayList radioChans = new ArrayList();
			MediaPortal.Radio.Database.RadioDatabase.GetStationsForCard(ref radioChans,id);
			TVCaptureCards cards = new TVCaptureCards();
			cards.LoadCaptureCards();
			foreach (TVCaptureDevice dev in cards.captureCards)
			{
				if (dev.Network==NetworkType.DVBS && dev.ID != id)
				{
					foreach (MediaPortal.Radio.Database.RadioStation chan in radioChans)
					{
						MediaPortal.Radio.Database.RadioDatabase.MapChannelToCard(chan.ID,dev.ID);
					}
				}
			}
		}
		void ScanChannels()
		{
			System.Threading.Thread.Sleep(400);
			Application.DoEvents();
			
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			
		}

		void ScanNextTransponder()
		{
			currentIndex++;
			if (currentIndex>=count)
			{
				if(m_currentDiseqc>=m_diseqcLoops)
				{
					progressBar1.Value=(100);
					Application.DoEvents();
				}
				else
				{
					m_currentDiseqc++;
					currentIndex=-1;
				}
				return;
			}
			DVBChannel newchan = new DVBChannel();
			newchan.NetworkID=-1;
			newchan.TransportStreamID=-1;
			newchan.ProgramNumber=-1;

			newchan.Polarity=transp[currentIndex].TPpol;
			newchan.Symbolrate=transp[currentIndex].TPsymb;
			newchan.FEC=(int)TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
			newchan.Frequency=transp[currentIndex].TPfreq;
			

			Log.WriteFile(Log.LogType.Capture,"tune transponder:{0} freq:{1} KHz symbolrate:{2} polarisation:{3}",currentIndex,
				newchan.Frequency,newchan.Symbolrate,newchan.Polarity);
			captureCard.Tune(newchan,m_currentDiseqc);
			System.Threading.Thread.Sleep(400);
			Application.DoEvents();

		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			TVCaptureCards cards = new TVCaptureCards();
			cards.LoadCaptureCards();
			foreach (TVCaptureDevice dev in cards.captureCards)
			{
				if (dev.Network==NetworkType.DVBS)
				{
					captureCard = dev;
					captureCard.CreateGraph();
					break;
				}
			}
			LoadFrequencies();
			progressBar1.Visible=true;
			progressBar1.Value=0;
			Application.DoEvents();
			newChannels=0; updatedChannels=0;
			newRadioChannels=0; updatedRadioChannels=0;
			DoScan();
			labelStatus.Text=String.Format("Imported {0} tv channels, {1} radio channels",newChannels, newRadioChannels);
		
			progressBar1.Value=100;
			Application.DoEvents();
		}

		private void cbTransponder_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

	}
}
