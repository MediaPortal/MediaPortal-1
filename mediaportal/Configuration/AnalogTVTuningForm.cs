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

namespace MediaPortal
{
	/// <summary>
	/// Summary description for AnalogTVTuningForm.
	/// </summary>
	public class AnalogTVTuningForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Label labelStatus;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.ComboBox comboBoxTV;
    private System.Windows.Forms.Button buttonMap;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Timer timer1;
    private System.ComponentModel.IContainer components;
    ArrayList     m_tvcards    = new ArrayList();
    private System.Windows.Forms.Button buttonSkip;
    int           m_iChannel   =1;

		public AnalogTVTuningForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

      comboBoxTV.Items.Clear();
      ArrayList channels=new ArrayList();
			TVDatabase.GetChannels(ref channels);
      foreach (TVChannel channel in channels)
      {
        if (channel.Number<1000)
        {
          comboBoxTV.Items.Add(channel.Name);
        }
      }
      if (comboBoxTV.Items.Count>0)
        comboBoxTV.SelectedIndex=0;

      m_tvcards.Clear();
      try
      {
        using (Stream r = File.Open(@"capturecards.xml", FileMode.Open, FileAccess.Read))
        {
          SoapFormatter c = new SoapFormatter();
          m_tvcards = (ArrayList)c.Deserialize(r);
          r.Close();
        } 
      }
      catch(Exception)
      {
      }
      if (m_tvcards.Count==0) 
      {
        MessageBox.Show(this,"No TVcapture cards configured. Please configure your capture card first","AutoTUne",MessageBoxButtons.OK);
        buttonMap.Enabled=false;
        comboBoxTV.Enabled=false;
        return;
      }
      for (int i=0; i < m_tvcards.Count;i++)
      {
        TVCaptureDevice card=(TVCaptureDevice)m_tvcards[i];
        card.ID=(i+1);
      }

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
      this.labelStatus = new System.Windows.Forms.Label();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.comboBoxTV = new System.Windows.Forms.ComboBox();
      this.buttonMap = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.buttonSkip = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // labelStatus
      // 
      this.labelStatus.Location = new System.Drawing.Point(16, 16);
      this.labelStatus.Name = "labelStatus";
      this.labelStatus.Size = new System.Drawing.Size(496, 16);
      this.labelStatus.TabIndex = 0;
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(424, 232);
      this.btnOk.Name = "btnOk";
      this.btnOk.TabIndex = 2;
      this.btnOk.Text = "Ok";
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // panel1
      // 
      this.panel1.Location = new System.Drawing.Point(16, 48);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(248, 200);
      this.panel1.TabIndex = 3;
      // 
      // comboBoxTV
      // 
      this.comboBoxTV.Location = new System.Drawing.Point(296, 88);
      this.comboBoxTV.Name = "comboBoxTV";
      this.comboBoxTV.Size = new System.Drawing.Size(136, 21);
      this.comboBoxTV.TabIndex = 4;
      // 
      // buttonMap
      // 
      this.buttonMap.Enabled = false;
      this.buttonMap.Location = new System.Drawing.Point(448, 88);
      this.buttonMap.Name = "buttonMap";
      this.buttonMap.Size = new System.Drawing.Size(40, 23);
      this.buttonMap.TabIndex = 5;
      this.buttonMap.Text = "Map";
      this.buttonMap.Click += new System.EventHandler(this.buttonMap_Click);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(296, 64);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(112, 23);
      this.label1.TabIndex = 6;
      this.label1.Text = "TVGuide channels:";
      // 
      // timer1
      // 
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // buttonSkip
      // 
      this.buttonSkip.Enabled = false;
      this.buttonSkip.Location = new System.Drawing.Point(448, 128);
      this.buttonSkip.Name = "buttonSkip";
      this.buttonSkip.Size = new System.Drawing.Size(40, 23);
      this.buttonSkip.TabIndex = 7;
      this.buttonSkip.Text = "Skip";
      this.buttonSkip.Click += new System.EventHandler(this.buttonSkip_Click);
      // 
      // AnalogTVTuningForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(520, 273);
      this.Controls.Add(this.buttonSkip);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.buttonMap);
      this.Controls.Add(this.comboBoxTV);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.labelStatus);
      this.Name = "AnalogTVTuningForm";
      this.Text = "AnalogTVTuningForm";
      this.Load += new System.EventHandler(this.AnalogTVTuningForm_Load);
      this.ResumeLayout(false);

    }
		#endregion

    private void btnOk_Click(object sender, System.EventArgs e)
    {
      Close();
    
    }

    private void buttonMap_Click(object sender, System.EventArgs e)
    {
    
    }

    private void AnalogTVTuningForm_Load(object sender, System.EventArgs e)
    {
      if (m_tvcards.Count<=0) return;
      timer1.Interval=100;
      timer1.Enabled=true;
      TVCaptureDevice card=(TVCaptureDevice)m_tvcards[0];
      GUIGraphicsContext.form=this;
      GUIGraphicsContext.VideoWindow=new Rectangle(panel1.Location,panel1.Size);
      card.View=true;
      Tune(card, m_iChannel);
    }

    void Tune(TVCaptureDevice card, int channel)
    {
      buttonMap.Enabled=false;
      buttonSkip.Enabled=false;
      card.Tune(channel);
      labelStatus.Text=String.Format("Channel:{0} frequency:{1} locked:{2}", channel, card.VideoFrequency(), card.SignalPresent());
    }

    private void timer1_Tick(object sender, System.EventArgs e)
    {
      TVCaptureDevice card=(TVCaptureDevice)m_tvcards[0];
      if (!card.SignalPresent())
      {
        NextChannel();
      }
      else 
      {
        buttonMap.Enabled=true;
        buttonSkip.Enabled=true;
      }
    }

    private void buttonSkip_Click(object sender, System.EventArgs e)
    {
      NextChannel();
    }

    void NextChannel()
    {
      buttonMap.Enabled=false;
      buttonSkip.Enabled=false;
      if (m_iChannel>127) return;
      TVCaptureDevice card=(TVCaptureDevice)m_tvcards[0];
      m_iChannel++;
      Tune(card,m_iChannel);
      
    }
	}
}
