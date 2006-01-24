/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
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
		class Transponder
		{
			public string SatName;
			public string FileName;
			public override string ToString()
			{
				return SatName;
			}
		}

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
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ProgressBar progressBar3;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ComboBox cbTransponder;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cbTransponder2;
		private System.Windows.Forms.ComboBox cbTransponder3;
		private System.Windows.Forms.ComboBox cbTransponder4;
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
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.cbTransponder4 = new System.Windows.Forms.ComboBox();
      this.cbTransponder3 = new System.Windows.Forms.ComboBox();
      this.cbTransponder2 = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.cbTransponder = new System.Windows.Forms.ComboBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblStatus = new System.Windows.Forms.Label();
      this.progressBar3 = new System.Windows.Forms.ProgressBar();
      this.button3 = new System.Windows.Forms.Button();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.cbTransponder4);
      this.groupBox3.Controls.Add(this.cbTransponder3);
      this.groupBox3.Controls.Add(this.cbTransponder2);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Controls.Add(this.cbTransponder);
      this.groupBox3.Controls.Add(this.panel1);
      this.groupBox3.Controls.Add(this.lblStatus);
      this.groupBox3.Controls.Add(this.progressBar3);
      this.groupBox3.Controls.Add(this.button3);
      this.groupBox3.Controls.Add(this.label7);
      this.groupBox3.Controls.Add(this.label8);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox3.Location = new System.Drawing.Point(0, 0);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(472, 408);
      this.groupBox3.TabIndex = 0;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Setup Digital TV (DVB-S Satellite)";
      // 
      // cbTransponder4
      // 
      this.cbTransponder4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder4.Location = new System.Drawing.Point(152, 140);
      this.cbTransponder4.Name = "cbTransponder4";
      this.cbTransponder4.Size = new System.Drawing.Size(304, 21);
      this.cbTransponder4.TabIndex = 8;
      // 
      // cbTransponder3
      // 
      this.cbTransponder3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder3.Location = new System.Drawing.Point(152, 116);
      this.cbTransponder3.Name = "cbTransponder3";
      this.cbTransponder3.Size = new System.Drawing.Size(304, 21);
      this.cbTransponder3.TabIndex = 6;
      // 
      // cbTransponder2
      // 
      this.cbTransponder2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder2.Location = new System.Drawing.Point(152, 92);
      this.cbTransponder2.Name = "cbTransponder2";
      this.cbTransponder2.Size = new System.Drawing.Size(304, 21);
      this.cbTransponder2.TabIndex = 4;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 144);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(120, 16);
      this.label3.TabIndex = 7;
      this.label3.Text = "Transponder for LNB4:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 120);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(120, 16);
      this.label2.TabIndex = 5;
      this.label2.Text = "Transponder for LNB3:";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 96);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(120, 16);
      this.label1.TabIndex = 3;
      this.label1.Text = "Transponder for LNB2:";
      // 
      // cbTransponder
      // 
      this.cbTransponder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cbTransponder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbTransponder.Location = new System.Drawing.Point(152, 68);
      this.cbTransponder.Name = "cbTransponder";
      this.cbTransponder.Size = new System.Drawing.Size(304, 21);
      this.cbTransponder.TabIndex = 2;
      this.cbTransponder.SelectedIndexChanged += new System.EventHandler(this.cbTransponder_SelectedIndexChanged);
      // 
      // panel1
      // 
      this.panel1.Location = new System.Drawing.Point(432, 360);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(1, 1);
      this.panel1.TabIndex = 12;
      // 
      // lblStatus
      // 
      this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.lblStatus.Location = new System.Drawing.Point(16, 232);
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.Size = new System.Drawing.Size(440, 87);
      this.lblStatus.TabIndex = 10;
      // 
      // progressBar3
      // 
      this.progressBar3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar3.Location = new System.Drawing.Point(16, 200);
      this.progressBar3.Name = "progressBar3";
      this.progressBar3.Size = new System.Drawing.Size(440, 16);
      this.progressBar3.Step = 1;
      this.progressBar3.TabIndex = 9;
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button3.Location = new System.Drawing.Point(384, 336);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(72, 22);
      this.button3.TabIndex = 11;
      this.button3.Text = "Scan";
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 72);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(120, 16);
      this.label7.TabIndex = 1;
      this.label7.Text = "Transponder for LNB1:";
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.label8.Location = new System.Drawing.Point(16, 24);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(440, 32);
      this.label8.TabIndex = 0;
      this.label8.Text = "Mediaportal has detected one or more digital TV cards. Select your transponder an" +
        "d press \"Scan\" to get TV and radio channels.";
      // 
      // Wizard_DVBSTV
      // 
      this.Controls.Add(this.groupBox3);
      this.Name = "Wizard_DVBSTV";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox3.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion






		public override void OnSectionActivated()
		{
			lblStatus.Text="";
			cbTransponder.Items.Clear();
			cbTransponder2.Items.Clear();
			cbTransponder3.Items.Clear();
			cbTransponder4.Items.Clear();
			string [] files = System.IO.Directory.GetFiles( System.IO.Directory.GetCurrentDirectory()+@"\Tuningparameters");
			foreach (string file in files)
			{
				if (file.ToLower().IndexOf(".tpl") >=0)
				{
					Transponder ts = LoadTransponder(file);
					if (ts!=null)
					{
						cbTransponder.Items.Add(ts);
						cbTransponder2.Items.Add(ts);
						cbTransponder3.Items.Add(ts);
						cbTransponder4.Items.Add(ts);
					}
				}
			}
			if (cbTransponder.Items.Count>0)
				cbTransponder.SelectedIndex=0;
			if (cbTransponder2.Items.Count>0)
				cbTransponder2.SelectedIndex=0;
			if (cbTransponder3.Items.Count>0)
				cbTransponder3.SelectedIndex=0;
			if (cbTransponder4.Items.Count>0)
				cbTransponder4.SelectedIndex=0;

			m_diseqcLoops=1;
			cbTransponder2.Enabled=false;
			cbTransponder3.Enabled=false;
			cbTransponder4.Enabled=false;

			TVCaptureCards cards = new TVCaptureCards();
			cards.LoadCaptureCards();
			foreach (TVCaptureDevice dev in cards.captureCards)
			{
				if (dev.Network==NetworkType.DVBS)
				{
					captureCard = dev;
					break;
				}
			}

			if (captureCard==null) return;
			string filename=String.Format(@"database\card_{0}.xml",captureCard.FriendlyName);
			using(MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings(filename))
			{
				if(xmlreader.GetValueAsBool("dvbs","useLNB2",false)==true)
				{
					m_diseqcLoops++;
					cbTransponder2.Enabled=true;
				}
				if(xmlreader.GetValueAsBool("dvbs","useLNB3",false)==true)
				{
					m_diseqcLoops++;
					cbTransponder3.Enabled=true;
				}
				if(xmlreader.GetValueAsBool("dvbs","useLNB4",false)==true)
				{
					m_diseqcLoops++;
					cbTransponder4.Enabled=true;
				}
			}
		}

		Transponder LoadTransponder(string file)
		{
			System.IO.TextReader tin = System.IO.File.OpenText(file);
			Transponder ts = new Transponder();
			ts.FileName=file;
			string line=null;
			do
			{
				line = null;
				line = tin.ReadLine();
				if(line!=null)
				{
					if (line.Length > 0)
					{
						if(line.StartsWith(";"))
							continue;
						int pos=line.IndexOf("satname=");
						if (pos>=0)
						{
							ts.SatName=line.Substring(pos+"satname=".Length);
							tin.Close();
							return ts;
						}
					}
				}
			} while (!(line == null));
			tin.Close();
			return null;
		}

		void LoadFrequencies()
		{

			currentIndex=-1;

			string countryName=String.Empty;
			Transponder ts=null;
			if (m_currentDiseqc==1) ts=(Transponder)cbTransponder.SelectedItem;
			if (m_currentDiseqc==2) ts=(Transponder)cbTransponder2.SelectedItem;
			if (m_currentDiseqc==3) ts=(Transponder)cbTransponder3.SelectedItem;
			if (m_currentDiseqc==4) ts=(Transponder)cbTransponder4.SelectedItem;

			if (ts==null) return;
			countryName=ts.FileName;
			if (countryName==String.Empty) return;
			count = 0;
			string line;
			string[] tpdata;
			Log.WriteFile(Log.LogType.Capture,"Opening {0}",countryName);
			// load transponder list and start scan
			System.IO.TextReader tin = System.IO.File.OpenText(countryName);
			
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
	


		private void DoScan()
		{
			GUIGraphicsContext.form=this.FindForm();
			GUIGraphicsContext.VideoWindow=new Rectangle(panel1.Location,panel1.Size);

			m_currentDiseqc=1;
			LoadFrequencies();

			currentIndex=-1;
			while (m_currentDiseqc <=m_diseqcLoops)
			{
				while (currentIndex < count)
				{
					int index=currentIndex;
					if (index<0) index=0;
					
					float percent = ((float)index) / ((float)count);
					percent *= 100.0f;
					progressBar3.Value=((int)percent);
					TPList transponder=transp[index];
					string chanDesc=String.Format("freq:{0} Khz, Pol:{1} SR:{2}",transponder.TPfreq, transponder.TPpol, transponder.TPsymb );
					string description=String.Format("Transponder:{0}/{1} {2}", index,count,chanDesc);
					lblStatus.Text=(description);
					System.Windows.Forms.Application.DoEvents();

					ScanNextTransponder();
					if (captureCard.SignalPresent())
					{
						description=String.Format("Found signal for transponder:{0} {1}, Scanning channels", currentIndex,chanDesc);
						lblStatus.Text=(description);
						System.Windows.Forms.Application.DoEvents();
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
			System.Windows.Forms.Application.DoEvents();
			
			captureCard.StoreTunedChannels(false,true,ref newChannels, ref updatedChannels, ref newRadioChannels, ref updatedRadioChannels);
			
		}

		void ScanNextTransponder()
		{
			currentIndex++;
			if (currentIndex>=count)
			{
				if(m_currentDiseqc>=m_diseqcLoops)
				{
					m_currentDiseqc++;
					progressBar3.Value=(100);
					System.Windows.Forms.Application.DoEvents();
				}
				else
				{
					m_currentDiseqc++;
					LoadFrequencies();
					currentIndex=0;
				}
			}
			if (currentIndex<0 || currentIndex >=count) return;
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
			if (captureCard.SignalQuality <40)
				System.Threading.Thread.Sleep(400);
			System.Windows.Forms.Application.DoEvents();

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
			progressBar3.Visible=true;
			progressBar3.Value=0;
			System.Windows.Forms.Application.DoEvents();
			newChannels=0; updatedChannels=0;
			newRadioChannels=0; updatedRadioChannels=0;
			DoScan();
			lblStatus.Text=String.Format("Imported {0} tv channels, {1} radio channels",newChannels, newRadioChannels);
		
			progressBar3.Value=100;
			System.Windows.Forms.Application.DoEvents();
		}

		private void cbTransponder_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

	}
}
