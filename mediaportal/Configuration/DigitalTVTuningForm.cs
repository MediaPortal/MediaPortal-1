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
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using TVCapture;
using MediaPortal.Configuration.Sections;
using MediaPortal.TV.Scanning;

namespace MediaPortal
{
  /// <summary>
  /// Summary description for DigitalTVTuningForm.
  /// </summary>
  public class DigitalTVTuningForm : System.Windows.Forms.Form, AutoTuneCallback
  {
    private MediaPortal.UserInterface.Controls.MPLabel labelStatus;
    private System.Windows.Forms.Panel panel1;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    ArrayList m_tvcards = new ArrayList();
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    ITuning tuningInterface;
    private System.Windows.Forms.ProgressBar progressBar1;
    private MediaPortal.UserInterface.Controls.MPLabel labelStatus2;
    private MediaPortal.UserInterface.Controls.MPButton button1;
    private System.Windows.Forms.ImageList imageList1;
    private System.ComponentModel.IContainer components;
    private MediaPortal.UserInterface.Controls.MPLabel labelChannels;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private System.Windows.Forms.ProgressBar signalStrength;
    private System.Windows.Forms.ProgressBar signalQuality;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private System.Windows.Forms.Timer timer1;
    TVCaptureDevice captureCard;
    bool _isAutoTuning = false;
    char rotater = '-';
    int rotaterState = 0;

    public DigitalTVTuningForm()
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DigitalTVTuningForm));
      this.labelStatus = new MediaPortal.UserInterface.Controls.MPLabel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.labelStatus2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.labelChannels = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.signalStrength = new System.Windows.Forms.ProgressBar();
      this.signalQuality = new System.Windows.Forms.ProgressBar();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
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
      // panel1
      // 
      this.panel1.Location = new System.Drawing.Point(1, 1);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(1, 1);
      this.panel1.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 136);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(56, 16);
      this.label1.TabIndex = 6;
      this.label1.Text = "Channels:";
      // 
      // listView1
      // 
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.listView1.Location = new System.Drawing.Point(16, 168);
      this.listView1.MultiSelect = false;
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(280, 168);
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
      // labelStatus2
      // 
      this.labelStatus2.Location = new System.Drawing.Point(8, 32);
      this.labelStatus2.Name = "labelStatus2";
      this.labelStatus2.Size = new System.Drawing.Size(336, 16);
      this.labelStatus2.TabIndex = 10;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(523, 313);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(64, 23);
      this.button1.TabIndex = 11;
      this.button1.Text = "Start";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // labelChannels
      // 
      this.labelChannels.Location = new System.Drawing.Point(72, 136);
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
      // timer1
      // 
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // DigitalTVTuningForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(608, 358);
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
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.listView1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.labelStatus);
      this.Name = "DigitalTVTuningForm";
      this.Text = "Scan Tv / Radio channels";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DigitalTVTuningForm_FormClosed);
      this.Load += new System.EventHandler(this.DigitalTVTuningForm_Load);
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


    private void DigitalTVTuningForm_Load(object sender, System.EventArgs e)
    {
      UpdateList();
      GUIGraphicsContext.form = this;
      //			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      //			{
      //				videoRenderer = xmlreader.GetValueAsInt("mytv", "vmr9", 0);
      //				xmlreader.SetValue("mytv", "vmr9","0");
      //}
      GUIGraphicsContext.VideoWindow = new Rectangle(panel1.Location, panel1.Size);

      tuningInterface.AutoTuneTV(captureCard, this);
      OnStatus("");
      OnStatus2("");
      OnProgress(0);

    }



    public ITuning Tuning
    {
      set
      {
        tuningInterface = value;
      }
    }

    public TVCaptureDevice Card
    {
      set
      {
        captureCard = value;
      }
    }
    #region AutoTuneCallback Members

    public void OnEnded()
    {
      signalQuality.Value = 0;
      signalStrength.Value = 0;
    }
    public void OnNewChannel()
    {
    }

    public void OnProgress(int percentDone)
    {
      progressBar1.Value = percentDone;
    }

    public void OnStatus(string description)
    {
      labelStatus.Text = description ;
    }

    public void OnStatus2(string description)
    {
      labelStatus2.Text = description;
    }

    public void UpdateList()
    {
      UpdateSignal();
      listView1.Items.Clear();

      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel channel in channels)
      {
        if (channel.Number < (int)ExternalInputs.svhs)
        {
          ListViewItem item = new ListViewItem();
          item.ImageIndex = 0;
          if (channel.Scrambled)
            item.ImageIndex = 1;
          item.Text = channel.Name;
          item.SubItems.Add(channel.Number.ToString());
          listView1.Items.Add(item);
        }
      }
      labelChannels.Text = listView1.Items.Count.ToString();
    }
    #endregion

    private void DigitalTVTuningForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      try
      {
        if (tuningInterface == null) return;
      }
      catch (Exception) { }
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      if (_isAutoTuning)
      {
        _isAutoTuning = false;
        timer1.Enabled = false;
      }
      else
      {
        captureCard.DeleteGraph();

        timer1.Interval = 100 ;
        timer1.Enabled = true;
        if (tuningInterface == null) return;
        _isAutoTuning = true;
        tuningInterface.Start();
        button1.Enabled = true;
        button1.Focus();
        button1.Text = "Stop";
        Thread workerThread = new Thread( new ThreadStart(DoScan));
        workerThread.Start();
      }
    }
    public void OnSignal(int quality, int strength)
    {
      if (quality < 0) quality = 0;
      if (quality > 100) quality = 100;
      if (strength < 0) strength = 0;
      if (strength > 100) strength = 100;
      signalQuality.Value = quality;
      signalStrength.Value = strength;
    }

    bool reentrant = false;
    void UpdateSignal()
    {
      if (captureCard == null) return;
      if (reentrant) return;
      try
      {
        reentrant = true;
        OnSignal(captureCard.SignalQuality, captureCard.SignalStrength);
      }
      finally
      {
        reentrant = false;
      }
    }

    private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
    {
    }



    private void timer1_Tick(object sender, System.EventArgs e)
    {
      if (_isAutoTuning)
        UpdateStatus();
    }

    void DoScan()
    {
      captureCard.CreateGraph();
      while (_isAutoTuning)
      {
        tuningInterface.Next();
        UpdateList();
        if (tuningInterface.IsFinished())
        {
          OnStatus("Finished");

          OnProgress(100);
          _isAutoTuning = false;
          button1.Enabled = true;
          button1.Text = "Start";
        }
      }
      button1.Enabled = true;
      button1.Text = "Start";
      this.Text = "Scan Tv / Radio channels";
      captureCard.DeleteGraph();
    }

    private void DigitalTVTuningForm_FormClosed(object sender, FormClosedEventArgs e)
    {
    }
    void UpdateStatus()
    {
      switch (rotaterState)
      {
        case 0: rotater = '\\'; rotaterState = 1; break;
        case 1: rotater = '|'; rotaterState = 2; break;
        case 2: rotater = '/'; rotaterState = 3; break;
        case 3: rotater = '-'; rotaterState = 0; break;
      }
      this.Text = "Scanning... "+ rotater;
    }
  }
}
