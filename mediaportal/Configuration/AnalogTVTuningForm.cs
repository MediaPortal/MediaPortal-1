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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;

using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using TVCapture;
using MediaPortal.Configuration.Sections;


namespace MediaPortal
{
	/// <summary>
	/// Summary description for AnalogTVTuningForm.
	/// </summary>
    public class AnalogTVTuningForm : System.Windows.Forms.Form, AutoTuneCallback
	{
    private System.Windows.Forms.Label labelStatus;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button buttonMap;
    private System.Windows.Forms.Label label1;
    ArrayList     m_tvcards    = new ArrayList();
    private System.Windows.Forms.Button buttonSkip;
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
		ITuning				tuningInterface;
		private System.Windows.Forms.ProgressBar progressBar1;
		//int videoRenderer;
		private System.Windows.Forms.Button buttonAdd;
		private System.Windows.Forms.Label labelStatus2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ImageList imageList1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label labelChannels;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ProgressBar signalStrength;
		private System.Windows.Forms.ProgressBar signalQuality;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Timer timer1;
		TVCaptureDevice captureCard;

		public AnalogTVTuningForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Update();
      
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnalogTVTuningForm));
            this.labelStatus = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonMap = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSkip = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.labelStatus2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.labelChannels = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.signalStrength = new System.Windows.Forms.ProgressBar();
            this.signalQuality = new System.Windows.Forms.ProgressBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(16, 8);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(496, 16);
            this.labelStatus.TabIndex = 0;
            // 
            // btnOk
            // 
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOk.Location = new System.Drawing.Point(600, 48);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(56, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "Close";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.panel1.Location = new System.Drawing.Point(16, 160);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(248, 200);
            this.panel1.TabIndex = 3;
            // 
            // buttonMap
            // 
            this.buttonMap.Enabled = false;
            this.buttonMap.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonMap.Location = new System.Drawing.Point(280, 352);
            this.buttonMap.Name = "buttonMap";
            this.buttonMap.Size = new System.Drawing.Size(136, 23);
            this.buttonMap.TabIndex = 2;
            this.buttonMap.Text = "Map to existing channel";
            this.buttonMap.Click += new System.EventHandler(this.buttonMap_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(288, 152);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Channels:";
            // 
            // buttonSkip
            // 
            this.buttonSkip.Enabled = false;
            this.buttonSkip.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSkip.Location = new System.Drawing.Point(424, 352);
            this.buttonSkip.Name = "buttonSkip";
            this.buttonSkip.Size = new System.Drawing.Size(104, 23);
            this.buttonSkip.TabIndex = 3;
            this.buttonSkip.Text = "Skip this channel";
            this.buttonSkip.Click += new System.EventHandler(this.buttonSkip_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.Location = new System.Drawing.Point(288, 176);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(264, 168);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 155;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Channel Number";
            this.columnHeader2.Width = 100;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(24, 64);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(472, 16);
            this.progressBar1.TabIndex = 9;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Enabled = false;
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(280, 376);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(136, 23);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "Add as new channel";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // labelStatus2
            // 
            this.labelStatus2.Location = new System.Drawing.Point(8, 32);
            this.labelStatus2.Name = "labelStatus2";
            this.labelStatus2.Size = new System.Drawing.Size(336, 16);
            this.labelStatus2.TabIndex = 10;
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(600, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Stop";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelChannels
            // 
            this.labelChannels.Location = new System.Drawing.Point(360, 152);
            this.labelChannels.Name = "labelChannels";
            this.labelChannels.Size = new System.Drawing.Size(100, 16);
            this.labelChannels.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 16);
            this.label2.TabIndex = 13;
            this.label2.Text = "Signal strength (dB):";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 16);
            this.label3.TabIndex = 14;
            this.label3.Text = "Signal quality  (%):";
            // 
            // signalStrength
            // 
            this.signalStrength.Location = new System.Drawing.Point(160, 88);
            this.signalStrength.Name = "signalStrength";
            this.signalStrength.Size = new System.Drawing.Size(232, 16);
            this.signalStrength.Step = 1;
            this.signalStrength.TabIndex = 15;
            // 
            // signalQuality
            // 
            this.signalQuality.Location = new System.Drawing.Point(160, 112);
            this.signalQuality.Name = "signalQuality";
            this.signalQuality.Size = new System.Drawing.Size(232, 16);
            this.signalQuality.Step = 1;
            this.signalQuality.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(392, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 17;
            this.label4.Text = "100dB";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(136, 88);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 16);
            this.label5.TabIndex = 18;
            this.label5.Text = "0dB";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(136, 112);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 16);
            this.label6.TabIndex = 19;
            this.label6.Text = "0%";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(392, 112);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 16);
            this.label7.TabIndex = 20;
            this.label7.Text = "100%";
            // 
            // AnalogTVTuningForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(672, 454);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.signalQuality);
            this.Controls.Add(this.signalStrength);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelChannels);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelStatus2);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.buttonSkip);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonMap);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.labelStatus);
            this.Name = "AnalogTVTuningForm";
            this.Text = "Find all TV channels";
            this.Closed += new System.EventHandler(this.AnalogTVTuningForm_Closed);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AnalogTVTuningForm_Closing);
            this.Load += new System.EventHandler(this.AnalogTVTuningForm_Load);
            this.ResumeLayout(false);

    }
		#endregion

    private void btnOk_Click(object sender, System.EventArgs e)
    {
     
			//using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			//{
			//	xmlreader.SetValue("mytv", "vmr9",videoRenderer.ToString());
			//}
			TVChannels.UpdateList();
			RadioStations.UpdateList();
      this.Close();
    }

    private void buttonMap_Click(object sender, System.EventArgs e)
    {
			if (listView1.SelectedItems.Count<=0) 
			{
				MessageBox.Show("Please select a channel from the list", "Map TV Channel", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
      string selectedChannel=listView1.SelectedItems[0].Text;
      for (int i=0; i < listView1.Items.Count;++i)
      {
        if (listView1.Items[i].Text==(string)selectedChannel)
        {
					int channel=tuningInterface.MapToChannel(selectedChannel);
          listView1.Items[i].SubItems[1].Text=channel.ToString();
          NextChannel();
          return;
        }
      }
    }

    private void AnalogTVTuningForm_Load(object sender, System.EventArgs e)
    {	
			buttonMap.Enabled=false;
			buttonMap.Enabled=false;
			btnOk.Enabled=false;
			UpdateList();
			GUIGraphicsContext.ActiveForm=this.Handle;
            GUIGraphicsContext.VideoWindow=new Rectangle(panel1.Location,panel1.Size);
			tuningInterface.AutoTuneTV(captureCard,this);
    }


    private void buttonSkip_Click(object sender, System.EventArgs e)
    {
      NextChannel();
    }

    void NextChannel()
    {
      buttonMap.Enabled=false;
      buttonSkip.Enabled=false;
			buttonAdd.Enabled=false;

      tuningInterface.Continue();
    }
		public ITuning Tuning
		{
			set 
			{
				tuningInterface=value;
			}
		}
		public TVCaptureDevice Card
		{
			set 
			{
				captureCard=value;
			}
		}
		#region AutoTuneCallback Members

		public void OnEnded()
		{
			btnOk.Enabled=true;
			button1.Enabled=false;
			signalQuality.Value=0;
			signalStrength.Value=0;
		}
		public void OnNewChannel()
		{
			buttonMap.Enabled=true;
			buttonSkip.Enabled=true;
			buttonAdd.Enabled=true;
		}

		public void OnProgress(int percentDone)
		{
			progressBar1.Value=percentDone;
		}

		public void OnStatus(string description)
		{
			labelStatus.Text=description;
		}

		public void OnStatus2(string description)
		{
			labelStatus2.Text=description;
		}

		public void UpdateList()
		{
			listView1.Items.Clear();
      
			ArrayList channels=new ArrayList();
			TVDatabase.GetChannels(ref channels);
			foreach (TVChannel channel in channels)
			{
				if (channel.Number<(int)ExternalInputs.svhs)
				{
					ListViewItem item = new ListViewItem();
					item.ImageIndex=0;
					if (channel.Scrambled)
						item.ImageIndex=1;
					item.Text=channel.Name;
					item.SubItems.Add(channel.Number.ToString());
					listView1.Items.Add(item);
				}
			}
			labelChannels.Text=listView1.Items.Count.ToString();
		}
		#endregion

		private void buttonAdd_Click(object sender, System.EventArgs e)
		{
			editName formName = new editName();
			formName.ShowDialog(this);
			if (formName.ChannelName==String.Empty)
			{
				return;
			}


			TVChannel chan = new TVChannel();
			chan.Name = formName.ChannelName;
			chan.Number=GetUniqueNumber();
			chan.VisibleInGuide=true;
			TVDatabase.AddChannel(chan);
			tuningInterface.MapToChannel(chan.Name);

			UpdateList();
			NextChannel();
		}
		int GetUniqueNumber()
		{
			ArrayList channels=new ArrayList();
			TVDatabase.GetChannels(ref channels);
			int number=1;
			while (true)
			{
				bool unique=true;
				foreach (TVChannel chan in channels)
				{
					if (chan.Number==number)
					{
						unique=false;
						break;
					}
				}
				if (!unique)
				{
					number++;
				}
				else
				{
					return number;
				}
			}
		}

		private void AnalogTVTuningForm_Closed(object sender, System.EventArgs e)
		{
			
			
			try
			{
				if (tuningInterface==null) return;
				tuningInterface.Stop();		
			}
			catch (Exception){}
		}

		private void AnalogTVTuningForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			
			
			try
			{
				if (tuningInterface==null) return;
				tuningInterface.Stop();		
			}
			catch (Exception){}
		
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			
			if (tuningInterface==null) return;
			tuningInterface.Stop();		

			buttonMap.Enabled=false;
			buttonSkip.Enabled=false;
			buttonAdd.Enabled=false;
			button1.Enabled=false;
			btnOk.Enabled=true;
			btnOk.Focus();
		}
		public void OnSignal(int quality, int strength)
		{
			if (quality< 0) quality=0;
			if (quality>100) quality=100;
			if (strength< 0) strength=0;
			if (strength>100) strength=100;
			signalQuality.Value=quality;
			signalStrength.Value=strength;
			System.Windows.Forms.Application.DoEvents();
		}

		private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

	}
}
