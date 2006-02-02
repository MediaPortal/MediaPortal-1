#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

#endregion

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
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using TVCapture;
using MediaPortal.Configuration.Sections;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for AnalogRadioTuningForm.
	/// </summary>
	public class AnalogRadioTuningForm : System.Windows.Forms.Form, AutoTuneCallback
	{
    private System.Windows.Forms.Label labelStatus;
        private MediaPortal.UserInterface.Controls.MPButton btnOk;
    private MediaPortal.UserInterface.Controls.MPButton buttonMap;
    private System.Windows.Forms.Label label1;
    ArrayList     m_tvcards    = new ArrayList();
    private MediaPortal.UserInterface.Controls.MPButton buttonSkip;
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    ITuning tuningInterface;
		private System.Windows.Forms.ProgressBar progressBar1;
		//int videoRenderer;
        private MediaPortal.UserInterface.Controls.MPButton buttonAdd;
		private MediaPortal.UserInterface.Controls.MPButton buttonStop;
        private MediaPortal.UserInterface.Controls.MPButton buttonStart;
        private System.Windows.Forms.ImageList imageList1;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar signalStrength;
		private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timer1;
        private Label label3;
        private ComboBox sensitivityComboBox;
        private Label labelStatus2;
        private Label labelChannels;
		TVCaptureDevice captureCard;

		public AnalogRadioTuningForm()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnalogRadioTuningForm));
            this.labelStatus = new System.Windows.Forms.Label();
            this.btnOk = new MediaPortal.UserInterface.Controls.MPButton();
            this.buttonMap = new MediaPortal.UserInterface.Controls.MPButton();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSkip = new MediaPortal.UserInterface.Controls.MPButton();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonAdd = new MediaPortal.UserInterface.Controls.MPButton();
            this.buttonStop = new MediaPortal.UserInterface.Controls.MPButton();
            this.buttonStart = new MediaPortal.UserInterface.Controls.MPButton();
            this.label2 = new System.Windows.Forms.Label();
            this.signalStrength = new System.Windows.Forms.ProgressBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.sensitivityComboBox = new System.Windows.Forms.ComboBox();
            this.labelStatus2 = new System.Windows.Forms.Label();
            this.labelChannels = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(19, 13);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(413, 21);
            this.labelStatus.TabIndex = 0;
            // 
            // btnOk
            // 
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOk.Location = new System.Drawing.Point(604, 81);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(56, 23);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Close";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonStart.Location = new System.Drawing.Point(604, 12);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(56, 23);
            this.buttonStart.TabIndex = 3;
            this.buttonStart.Text = "Start";
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonMap
            // 
            this.buttonMap.Enabled = false;
            this.buttonMap.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonMap.Location = new System.Drawing.Point(511, 171);
            this.buttonMap.Name = "buttonMap";
            this.buttonMap.Size = new System.Drawing.Size(136, 23);
            this.buttonMap.TabIndex = 13;
            this.buttonMap.Text = "Map to existing station";
            this.buttonMap.Click += new System.EventHandler(this.buttonMap_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 11;
            this.label1.Text = "Stations:";
            // 
            // buttonSkip
            // 
            this.buttonSkip.Enabled = false;
            this.buttonSkip.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSkip.Location = new System.Drawing.Point(511, 142);
            this.buttonSkip.Name = "buttonSkip";
            this.buttonSkip.Size = new System.Drawing.Size(136, 23);
            this.buttonSkip.TabIndex = 12;
            this.buttonSkip.Text = "Skip this station";
            this.buttonSkip.Click += new System.EventHandler(this.buttonSkip_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.Location = new System.Drawing.Point(19, 142);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(472, 168);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 12;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 300;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Frequency";
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Station Number";
            this.columnHeader3.Width = 72;
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
            this.progressBar1.Location = new System.Drawing.Point(19, 52);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(472, 16);
            this.progressBar1.TabIndex = 6;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Enabled = false;
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(511, 200);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(136, 23);
            this.buttonAdd.TabIndex = 14;
            this.buttonAdd.Text = "Add as new station";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonStop.Location = new System.Drawing.Point(604, 45);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(56, 23);
            this.buttonStop.TabIndex = 4;
            this.buttonStop.Text = "Stop";
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "Signal strength (dB):";
            // 
            // signalStrength
            // 
            this.signalStrength.Location = new System.Drawing.Point(154, 88);
            this.signalStrength.Name = "signalStrength";
            this.signalStrength.Size = new System.Drawing.Size(232, 16);
            this.signalStrength.Step = 1;
            this.signalStrength.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(392, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 10;
            this.label4.Text = "100dB";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(124, 88);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 16);
            this.label5.TabIndex = 8;
            this.label5.Text = "0dB";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Location = new System.Drawing.Point(448, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 23);
            this.label3.TabIndex = 1;
            this.label3.Text = "Sensitivity";
            // 
            // sensitivityComboBox
            // 
            this.sensitivityComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.sensitivityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sensitivityComboBox.Items.AddRange(new object[] {
            "High",
            "Medium",
            "Low"});
            this.sensitivityComboBox.Location = new System.Drawing.Point(518, 13);
            this.sensitivityComboBox.Name = "sensitivityComboBox";
            this.sensitivityComboBox.Size = new System.Drawing.Size(72, 21);
            this.sensitivityComboBox.TabIndex = 2;
            this.sensitivityComboBox.SelectedItem = "Medium";

            // 
            // labelStatus2
            // 
            this.labelStatus2.Location = new System.Drawing.Point(19, 29);
            this.labelStatus2.Name = "labelStatus2";
            this.labelStatus2.Size = new System.Drawing.Size(413, 20);
            this.labelStatus2.TabIndex = 15;
            // 
            // channelsNumber
            // 
            this.labelChannels.AutoSize = true;
            this.labelChannels.Location = new System.Drawing.Point(78, 123);
            this.labelChannels.Name = "channelsNumber";
            this.labelChannels.Size = new System.Drawing.Size(0, 13);
            this.labelChannels.TabIndex = 16;
            // 
            // AnalogRadioTuningForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(672, 332);
            this.Controls.Add(this.labelChannels);
            this.Controls.Add(this.labelStatus2);
            this.Controls.Add(this.sensitivityComboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.signalStrength);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.buttonSkip);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonMap);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.labelStatus);
            this.Name = "AnalogRadioTuningForm";
            this.Text = "Find all Radio stations";
            this.Closed += new System.EventHandler(this.AnalogRadioTuningForm_Closed);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AnalogRadioTuningForm_Closing);
            this.Load += new System.EventHandler(this.AnalogRadioTuningForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

    }
		#endregion

    private void btnOk_Click(object sender, System.EventArgs e)
    {
			RadioStations.UpdateList();
            this.Close();
    }

    private void buttonMap_Click(object sender, System.EventArgs e)
    {
		if (listView1.SelectedItems.Count<=0) 
		{
			MessageBox.Show("Please select a station from the list", "Map Radio Station", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
        string selectedStation=listView1.SelectedItems[0].Text;
        int stationNumber = tuningInterface.MapToChannel(selectedStation);
        RadioStation station;
        RadioDatabase.GetStation(selectedStation, out station);
        float frequency = ((float)station.Frequency) / 1000000f;
        string description = String.Format("{0:###.##} MHz.", frequency);
        listView1.SelectedItems[0].SubItems[1].Text = description;
        listView1.SelectedItems[0].SubItems[2].Text = station.Channel.ToString();
        NextStation();
    }

    private void AnalogRadioTuningForm_Load(object sender, System.EventArgs e)
    {	
			buttonMap.Enabled=false;
			buttonSkip.Enabled=false;
            buttonAdd.Enabled = false;
            buttonStop.Enabled = false;
            buttonStart.Enabled = true;
            btnOk.Enabled = true;
			UpdateList();
    }

    private void buttonStart_Click(object sender, System.EventArgs e)
    {
        buttonStart.Enabled = false;
        btnOk.Enabled = false;
        sensitivityComboBox.Enabled = false;
        buttonMap.Enabled = false;
        buttonSkip.Enabled = false;
        buttonAdd.Enabled = false;
        buttonStop.Enabled = true;
        int Sensitivity = 1;
        switch (sensitivityComboBox.Text)
        {
            case "High":
                Sensitivity = 10;
                break;

            case "Medium":
                Sensitivity = 2;
                break;

            case "Low":
                Sensitivity = 1;
                break;
        }

        captureCard.RadioSensitivity = Sensitivity;
        tuningInterface.AutoTuneRadio(captureCard, this);
    }


    private void buttonSkip_Click(object sender, System.EventArgs e)
    {
      NextStation();
    }

    void NextStation()
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
			btnOk.Enabled = true;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            buttonMap.Enabled = false;
            buttonSkip.Enabled = false;
            buttonAdd.Enabled = false;
            sensitivityComboBox.Enabled = true;
			signalStrength.Value=0;
		}
		public void OnNewChannel()
		{
            buttonMap.Enabled = true;
            buttonSkip.Enabled = true;
            buttonAdd.Enabled = true;
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
      
			ArrayList stations=new ArrayList();
            RadioDatabase.GetStations(ref stations);
            foreach (RadioStation station in stations)
			{
				ListViewItem item = new ListViewItem();
				item.ImageIndex=0;
                if (station.Scrambled)
					item.ImageIndex=1;
                item.Text = station.Name;
                float frequency = ((float)station.Frequency) / 1000000f;
                string description = String.Format("{0:###.##} MHz.", frequency);
                item.SubItems.Add(description);
                item.SubItems.Add(station.Channel.ToString());
				listView1.Items.Add(item);
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


			RadioStation station = new RadioStation();
            station.Name = formName.ChannelName;
            station.Channel = GetUniqueNumber();
            station.Scrambled = false;
            int id = RadioDatabase.AddStation(ref station);
			tuningInterface.MapToChannel(station.Name);

			UpdateList();
			NextStation();
		}
		int GetUniqueNumber()
		{
			ArrayList stations=new ArrayList();
            RadioDatabase.GetStations(ref stations);
			int number=1;
			while (true)
			{
				bool unique=true;
				foreach (RadioStation station in stations)
				{
                    if (station.Channel == number)
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

		private void AnalogRadioTuningForm_Closed(object sender, System.EventArgs e)
		{
			
			
			try
			{
				if (tuningInterface==null) return;
				tuningInterface.Stop();		
			}
			catch (Exception){}
		}

		private void AnalogRadioTuningForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			
			
			try
			{
				if (tuningInterface==null) return;
				tuningInterface.Stop();		
			}
			catch (Exception){}
		
		}

		private void buttonStop_Click(object sender, System.EventArgs e)
		{
			
			if (tuningInterface==null) return;
			tuningInterface.Stop();		

			buttonMap.Enabled=false;
			buttonSkip.Enabled=false;
			buttonAdd.Enabled=false;
			buttonStop.Enabled=false;
            buttonStart.Enabled = true;
            sensitivityComboBox.Enabled = true;
            btnOk.Enabled = true;
			btnOk.Focus();
		}
		public void OnSignal(int quality, int strength)
		{
			if (strength< 0) strength=0;
			if (strength>100) strength=100;
			signalStrength.Value=strength;
			System.Windows.Forms.Application.DoEvents();
		}
	}
}
