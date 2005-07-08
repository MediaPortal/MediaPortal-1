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
	public class Wizard_DVBTTV : MediaPortal.Configuration.SectionSettings
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.ComboBox cbCountry;
		

		enum State
		{
			ScanFrequencies,
			ScanChannels,
			ScanOffset
		}
		TVCaptureDevice											captureCard;
		
		ArrayList                           frequencies=new ArrayList();
		int                                 currentFrequencyIndex=0;
		int																	scanOffset = 0;
		
		State                               currentState;
		int																	currentOffset=0;
		int                                 tunedFrequency=0;
		int																	newChannels, updatedChannels;
		int																	newRadioChannels, updatedRadioChannels;
		private System.Windows.Forms.Label label3;
		
		private System.Windows.Forms.Panel panel1;

		public Wizard_DVBTTV() : this("DVBT TV")
		{
		}

		public Wizard_DVBTTV(string name) : base(name)
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
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelStatus = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.button1 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.cbCountry = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.panel1);
			this.groupBox1.Controls.Add(this.labelStatus);
			this.groupBox1.Controls.Add(this.progressBar1);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.cbCountry);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(480, 360);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Setup digital tv (DVBT terrestial)";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(40, 264);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(392, 64);
			this.label3.TabIndex = 13;
			this.label3.Text = @"NOTE: if your country is not present or if Mediaportal is unable to find any channels then MediaPortal probably doesnt know which frequencies to scan for your country. Edit the dvbt.xml in the TuningParameters/ subfolder and fill in all the frequencies needed for your country.";
			// 
			// panel1
			// 
			this.panel1.Location = new System.Drawing.Point(432, 328);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1, 1);
			this.panel1.TabIndex = 12;
			// 
			// labelStatus
			// 
			this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.labelStatus.Location = new System.Drawing.Point(40, 169);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(400, 63);
			this.labelStatus.TabIndex = 11;
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(32, 128);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(416, 16);
			this.progressBar1.TabIndex = 4;
			this.progressBar1.Visible = false;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(344, 72);
			this.button1.Name = "button1";
			this.button1.TabIndex = 3;
			this.button1.Text = "Scan...";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 72);
			this.label2.Name = "label2";
			this.label2.TabIndex = 2;
			this.label2.Text = "Country/Region:";
			// 
			// cbCountry
			// 
			this.cbCountry.Location = new System.Drawing.Point(144, 72);
			this.cbCountry.Name = "cbCountry";
			this.cbCountry.Size = new System.Drawing.Size(168, 21);
			this.cbCountry.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(432, 40);
			this.label1.TabIndex = 0;
			this.label1.Text = "Mediaportal has detected one or more digital Tv cards. Select your country and pr" +
				"ess auto tune to scan for the tv and radio channels";
			// 
			// Wizard_DVBTTV
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "Wizard_DVBTTV";
			this.Size = new System.Drawing.Size(496, 384);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion




		public override void OnSectionActivated()
		{
			base.OnSectionActivated ();
			labelStatus.Text="";
			XmlDocument doc= new XmlDocument();
			doc.Load("Tuningparameters/dvbt.xml");

			XmlNodeList countryList=doc.DocumentElement.SelectNodes("/dvbt/country");
			foreach (XmlNode nodeCountry in countryList)
			{
				string name= nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
				cbCountry.Items.Add(name);
			}
			if (cbCountry.Items.Count>0)
				cbCountry.SelectedIndex=0;

		}

		void LoadFrequencies()
		{
			
			string countryName=(string)cbCountry.SelectedItem;
			if (countryName==String.Empty) return;
			frequencies.Clear();

			XmlDocument doc= new XmlDocument();
			doc.Load("Tuningparameters/dvbt.xml");
			XmlNodeList countryList=doc.DocumentElement.SelectNodes("/dvbt/country");
			foreach (XmlNode nodeCountry in countryList)
			{
				string name= nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
				if (name!=countryName) continue;
				try
				{
					scanOffset =  XmlConvert.ToInt32(nodeCountry.Attributes.GetNamedItem(@"offset").InnerText);
				}
				catch(Exception){}

				XmlNodeList frequencyList = nodeCountry.SelectNodes("carrier");
				int[] carrier;
				foreach (XmlNode node in frequencyList)
				{
					carrier = new int[2];
					carrier[0] = XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"frequency").InnerText);
					try
					{
						carrier[1] = XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"bandwidth").InnerText);
					}
					catch(Exception){}

					if (carrier[1]==0) carrier[1]=8;
					frequencies.Add(carrier);
				}
			}
		}
	

		private void button1_Click(object sender, System.EventArgs e)
		{
			LoadFrequencies();
			if (frequencies.Count==0) return;
			progressBar1.Visible=true;
			newChannels=0; updatedChannels=0;
			newRadioChannels=0; updatedRadioChannels=0;
			DoScan();
			labelStatus.Text=String.Format("Imported {0} tv channels, {1} radio channels",newChannels, newRadioChannels);
		}

		private void DoScan()
		{
			GUIGraphicsContext.form=this.FindForm();
			GUIGraphicsContext.VideoWindow=new Rectangle(panel1.Location,panel1.Size);

			TVCaptureCards cards = new TVCaptureCards();
			cards.LoadCaptureCards();
			foreach (TVCaptureDevice dev in cards.captureCards)
			{
				if (dev.Network==NetworkType.DVBT)
				{
					captureCard = dev;
					captureCard.CreateGraph();
					break;
				}
			}

			currentFrequencyIndex=0;
			while (currentFrequencyIndex <frequencies.Count)
			{
				int currentFreq=currentFrequencyIndex;
				if (currentFrequencyIndex<0) currentFreq=0;
				float percent = ((float)currentFreq) / ((float)frequencies.Count);
				percent *= 100.0f;
				progressBar1.Value=((int)percent);
				int[] tmp = frequencies[currentFreq] as int[];
				
				float frequency = tunedFrequency;
				frequency /=1000;
				string description=String.Format("frequency:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);
				labelStatus.Text=description;

				ScanNextFrequency(0);
				if (captureCard.SignalPresent())
				{
					description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", frequency);
					labelStatus.Text=description;
					ScanChannels();
				}
				if (scanOffset!=0)
				{
					ScanNextFrequency(-scanOffset);
					if (captureCard.SignalPresent())
					{
						description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", frequency-scanOffset);
						labelStatus.Text=description;
						ScanChannels();
					}
					ScanNextFrequency(scanOffset);
					if (captureCard.SignalPresent())
					{
						description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", frequency+scanOffset);
						labelStatus.Text=description;
						ScanChannels();
					}
				}
				currentFrequencyIndex++;
			}
			captureCard.DeleteGraph();
			MapTvToOtherCards(captureCard.ID);
			MapRadioToOtherCards(captureCard.ID);
			progressBar1.Value=100;
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
				if (dev.Network==NetworkType.DVBT && dev.ID != id)
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
				if (dev.Network==NetworkType.DVBT && dev.ID != id)
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
			if (currentFrequencyIndex < 0 || currentFrequencyIndex >=frequencies.Count) return;
			int[] tmp;
			tmp = (int[])frequencies[currentFrequencyIndex];
			string description=String.Format("Found signal at frequency:{0:###.##} MHz. Scanning channels", tmp[0]/1000);
			labelStatus.Text=description;
			System.Threading.Thread.Sleep(400);
			Application.DoEvents();

			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
		}

		void ScanNextFrequency(int offset)
		{
			if (currentFrequencyIndex <0) currentFrequencyIndex =0;
			if (currentFrequencyIndex >=frequencies.Count) return;

			DVBChannel chan = new DVBChannel();
			int[] tmp;
			tmp = (int[])frequencies[currentFrequencyIndex];
			chan.Frequency=tmp[0];
			chan.Bandwidth=tmp[1];
			chan.Frequency+=offset;

			captureCard.Tune(chan,0);
			System.Threading.Thread.Sleep(400);

		}
	}
}

