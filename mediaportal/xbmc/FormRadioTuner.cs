using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.Win32;

using DirectX.Capture;
using MediaPortal.GUI.Library;
using MediaPortal.WinControls;
using MediaPortal.Radio.Database;
using MediaPortal.Util;

namespace MediaPortal
{
  /// <summary>
  /// Summary description for FormRadioTuner.
  /// </summary>
  public class FormRadioTuner : System.Windows.Forms.Form
  {
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label labelCurrentChannel;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelTotal;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Timer timer1;
    private System.ComponentModel.IContainer components;
    int     m_iChannel=0;
    int     m_iChannelsFound=0;
    int     m_iChannelStart=0;
    int     m_iChannelEnd=0;
    Capture m_capture=null;
    private System.Windows.Forms.Label labelFreq;
    ArrayList radioChannels = new ArrayList();
    public FormRadioTuner()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if(components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.label1 = new System.Windows.Forms.Label();
      this.labelCurrentChannel = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.labelTotal = new System.Windows.Forms.Label();
      this.btnCancel = new System.Windows.Forms.Button();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.labelFreq = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(16, 56);
      this.progressBar1.Maximum = 300;
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(352, 16);
      this.progressBar1.Step = 1;
      this.progressBar1.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(24, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(56, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Channel:";
      // 
      // labelCurrentChannel
      // 
      this.labelCurrentChannel.Location = new System.Drawing.Point(80, 8);
      this.labelCurrentChannel.Name = "labelCurrentChannel";
      this.labelCurrentChannel.Size = new System.Drawing.Size(48, 16);
      this.labelCurrentChannel.TabIndex = 2;
      this.labelCurrentChannel.Text = "0";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(176, 8);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(104, 16);
      this.label2.TabIndex = 3;
      this.label2.Text = "Channels found:";
      // 
      // labelTotal
      // 
      this.labelTotal.Location = new System.Drawing.Point(272, 8);
      this.labelTotal.Name = "labelTotal";
      this.labelTotal.Size = new System.Drawing.Size(112, 16);
      this.labelTotal.TabIndex = 4;
      this.labelTotal.Text = "0";
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(304, 80);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(64, 23);
      this.btnCancel.TabIndex = 5;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // timer1
      // 
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // labelFreq
      // 
      this.labelFreq.Location = new System.Drawing.Point(80, 32);
      this.labelFreq.Name = "labelFreq";
      this.labelFreq.Size = new System.Drawing.Size(96, 16);
      this.labelFreq.TabIndex = 6;
      this.labelFreq.Text = "label3";
      // 
      // FormRadioTuner
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(400, 109);
      this.Controls.Add(this.labelFreq);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.labelTotal);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.labelCurrentChannel);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.progressBar1);
      this.Name = "FormRadioTuner";
      this.Text = "FormRadioTuner";
      this.Closing += new System.ComponentModel.CancelEventHandler(this.FormRadioTuner_Closing);
      this.Load += new System.EventHandler(this.FormRadioTuner_Load);
      this.ResumeLayout(false);

    }
    #endregion

    private void timer1_Tick(object sender, System.EventArgs e)
    {
      try
      {
        if (m_capture.Tuner.SignalPresent)
        {
          AddChannel(m_iChannel,m_capture.Tuner.Channel);
          m_iChannelsFound++;
        }
      }
      catch(Exception)
      {
      }
      m_iChannel+=250000;
      if (m_iChannel>m_iChannelEnd)
      {
        this.Close();
      }
      else
      {
        try
        {
          progressBar1.Value=m_iChannel;
          this.labelCurrentChannel.Text=m_iChannel.ToString();
          this.labelTotal.Text=m_iChannelsFound.ToString();
          m_capture.Tuner.Channel=m_iChannel;
          this.labelFreq.Text=String.Format("{0} MHz", ((float)m_capture.Tuner.GetAudioFrequency)/1000000f);
        }
        catch(Exception)
        {
        }
      }
    }
    void AddChannel(int Channel, int Frequency)
    {
      foreach (RadioStation station in radioChannels)
      {
        if (station.Channel==Channel) return;
      }
      RadioStation NewStation = new RadioStation();
      NewStation.Name = String.Format("Channel{0}", radioChannels.Count+1);
      NewStation.Frequency=Channel;
      radioChannels.Add(NewStation);
    }

    private void FormRadioTuner_Load(object sender, System.EventArgs e)
    {
      radioChannels.Clear();
      RadioDatabase.GetStations(ref radioChannels);

      Filters filters = new Filters();
      if (filters.VideoInputDevices==null) return ;
      if (filters.VideoInputDevices.Count==0) return ;
      string strRadioDevice="";
      int iTunerCountry=31;
      string strTunerType="Antenna";
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strTunerType=xmlreader.GetValueAsString("radio","tuner","Antenna");
        iTunerCountry=xmlreader.GetValueAsInt("capture","country",31);
        strRadioDevice=xmlreader.GetValueAsString("radio","device","");
      }
      if (strRadioDevice.Length==0) return;
      for (int i=0; i < filters.VideoInputDevices.Count;++i)
      {
        if (strRadioDevice.Equals(filters.VideoInputDevices[i].Name))
        {
          m_capture = new Capture(filters.VideoInputDevices[i],null);
          break;
        }
      }
      if (m_capture==null) return;

      foreach (DirectX.Capture.CrossbarSource source in m_capture.VideoSources)
      {
        if (source.IsTuner)
        {
          m_capture.VideoSource=source;
          break;
        }
      }
      m_capture.FixCrossbarRouting(true);
      if (m_capture.Tuner==null)
      {
        m_capture=null;
        return ;
      }


      m_capture.Tuner.AudioMode= DirectX.Capture.Tuner.AMTunerModeType.FMRadio;
      


      m_capture.Tuner.Mode=DShowNET.AMTunerModeType.FMRadio;

      if (strTunerType.Equals("Antenna"))
        m_capture.Tuner.InputType=DirectX.Capture.TunerInputType.Antenna;
      else
        m_capture.Tuner.InputType=DirectX.Capture.TunerInputType.Cable;
        
      m_capture.Tuner.Country=iTunerCountry;
      m_capture.Tuner.TuningSpace=66;

      m_capture.AudioPreview=true;

      
      int[] minmaxchan=m_capture.Tuner.ChanelMinMax;
      m_iChannelStart=minmaxchan[0];
      m_iChannelEnd=minmaxchan[1];
      m_iChannel=m_iChannelStart;
      progressBar1.Minimum=m_iChannelStart;
      progressBar1.Maximum=m_iChannelEnd;
      progressBar1.Value=m_iChannel;
      progressBar1.Step=250000;
      
      m_capture.Tuner.Channel=m_iChannel;
      System.Threading.Thread.Sleep(1000);
      
      timer1.Interval=1000;
      timer1.Enabled=true;
    }

    private void btnCancel_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }

    private void FormRadioTuner_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (m_capture!=null)
      {
        m_capture=null;
      }

    }

    public ArrayList RadioChannels
    {
      get { return radioChannels;}
    }
  }
}
