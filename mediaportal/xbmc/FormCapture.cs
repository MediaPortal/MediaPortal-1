using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using DShowNET;
using DirectX.Capture;

namespace MediaPortal
{
	/// <summary>
	/// Summary description for FormCapture.
	/// </summary>
	public class FormCapture : System.Windows.Forms.Form
	{
    class CapFormat
    {
      public int width;
      public int height;
      public string description;
    };

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox comboVideoDevice;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox comboVideoCompressor;
    private System.Windows.Forms.ComboBox comboAudioDevice;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox comboAudioCompressor;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.ListView listFilters;
    private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.ComponentModel.IContainer components;

    string m_strAudioCompressor="";
    string m_strVideoCompressor="";
    string m_strVideoDevice="";
    string m_strCaptureFormat;
    private System.Windows.Forms.GroupBox groupBox11;
    private System.Windows.Forms.Label label22;
    private System.Windows.Forms.ComboBox comboBoxCaptureFormat;
    string m_strAudioDevice="";
    bool   m_bUseForRecording;
    private System.Windows.Forms.CheckBox checkBoxTV;
    private System.Windows.Forms.CheckBox checkBoxRecord;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox comboFrameSize;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox textBoxFrameRate;
    bool   m_bUseForTV;
    Size   m_FrameSize=new Size(720,576);
    double m_FrameRate=25.0d;
		private System.Windows.Forms.Button buttonSetupFilter;
		private System.Windows.Forms.ToolTip toolTip1;
    int     m_iID=0;
    ArrayList m_formats=new ArrayList();

		public FormCapture()
		{
      CapFormat fmt=new CapFormat();
      fmt.width=320; 
      fmt.height=240; 
      fmt.description = String.Format("{0}x{1}", fmt.width, fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=352; 
      fmt.height=240;
      fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=352; fmt.height=288;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=640; fmt.height=240;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=640; fmt.height=288;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=640; fmt.height=480;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=704; fmt.height=576;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=720; fmt.height=240;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=720; fmt.height=288;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=720; fmt.height=480;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

      fmt=new CapFormat();
      fmt.width=720; fmt.height=576;fmt.description=String.Format("{0}x{1}", fmt.width,fmt.height);
      m_formats.Add(fmt);

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			toolTip1.SetToolTip(comboVideoDevice,"Select your TV capture card from the list");
			toolTip1.SetToolTip(comboAudioDevice,"If you have a physical cable from your TV capture card\rto your soundcard then here you should select your soundcard.\rOtherwise leave it to NONE");
			toolTip1.SetToolTip(comboVideoCompressor,"Select which video compressor to use when recording\rNot needed if your TV capture card has an hardware MPEG2 encoder");
			toolTip1.SetToolTip(comboAudioCompressor,"Select which audio compressor to use when recording\rNot needed if your TV capture card has an hardware MPEG2 encoder");
			toolTip1.SetToolTip(listFilters,"This list shows which filters are running when you watch or record tv.\rYou can configure one by selecting it\rand choose Setup Filter");
			toolTip1.SetToolTip(buttonSetupFilter,"Setup selected filter");
			toolTip1.SetToolTip(comboBoxCaptureFormat,"Select which format you want for your recordings\rIf your TV capture card has an onboard MPEG2 encoder then choose .mpg");
			toolTip1.SetToolTip(checkBoxTV,"Use this TV capture card for watching live TV");
			toolTip1.SetToolTip(checkBoxRecord,"Use this TV capture card for recording TV");
			toolTip1.SetToolTip(comboFrameSize,"Select the resolution you want to record/view TV");
			toolTip1.SetToolTip(textBoxFrameRate,"Select the framerate record/view TV");
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormCapture));
			this.label1 = new System.Windows.Forms.Label();
			this.comboVideoDevice = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.comboAudioDevice = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.comboVideoCompressor = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.comboAudioCompressor = new System.Windows.Forms.ComboBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.listFilters = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.groupBox11 = new System.Windows.Forms.GroupBox();
			this.label22 = new System.Windows.Forms.Label();
			this.comboBoxCaptureFormat = new System.Windows.Forms.ComboBox();
			this.checkBoxTV = new System.Windows.Forms.CheckBox();
			this.checkBoxRecord = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.comboFrameSize = new System.Windows.Forms.ComboBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBoxFrameRate = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.buttonSetupFilter = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox11.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Video:";
			// 
			// comboVideoDevice
			// 
			this.comboVideoDevice.Location = new System.Drawing.Point(128, 24);
			this.comboVideoDevice.Name = "comboVideoDevice";
			this.comboVideoDevice.Size = new System.Drawing.Size(248, 21);
			this.comboVideoDevice.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 23);
			this.label2.TabIndex = 2;
			this.label2.Text = "Audio:";
			// 
			// comboAudioDevice
			// 
			this.comboAudioDevice.Location = new System.Drawing.Point(128, 56);
			this.comboAudioDevice.Name = "comboAudioDevice";
			this.comboAudioDevice.Size = new System.Drawing.Size(248, 21);
			this.comboAudioDevice.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 96);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 23);
			this.label3.TabIndex = 4;
			this.label3.Text = "Video Compressor:";
			// 
			// comboVideoCompressor
			// 
			this.comboVideoCompressor.Location = new System.Drawing.Point(128, 96);
			this.comboVideoCompressor.Name = "comboVideoCompressor";
			this.comboVideoCompressor.Size = new System.Drawing.Size(248, 21);
			this.comboVideoCompressor.TabIndex = 2;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 128);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 23);
			this.label4.TabIndex = 6;
			this.label4.Text = "Audio Compressor:";
			// 
			// comboAudioCompressor
			// 
			this.comboAudioCompressor.Location = new System.Drawing.Point(128, 128);
			this.comboAudioCompressor.Name = "comboAudioCompressor";
			this.comboAudioCompressor.Size = new System.Drawing.Size(248, 21);
			this.comboAudioCompressor.TabIndex = 3;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(544, 320);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.buttonOK.Size = new System.Drawing.Size(40, 23);
			this.buttonOK.TabIndex = 8;
			this.buttonOK.Text = "Ok";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// listFilters
			// 
			this.listFilters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																									this.columnHeader1});
			this.listFilters.Location = new System.Drawing.Point(24, 184);
			this.listFilters.Name = "listFilters";
			this.listFilters.Size = new System.Drawing.Size(352, 128);
			this.listFilters.TabIndex = 4;
			this.listFilters.View = System.Windows.Forms.View.Details;
			this.listFilters.DoubleClick += new System.EventHandler(this.listFilters_DoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Filter";
			this.columnHeader1.Width = 327;
			// 
			// groupBox11
			// 
			this.groupBox11.Controls.Add(this.label22);
			this.groupBox11.Controls.Add(this.comboBoxCaptureFormat);
			this.groupBox11.Location = new System.Drawing.Point(400, 16);
			this.groupBox11.Name = "groupBox11";
			this.groupBox11.Size = new System.Drawing.Size(176, 72);
			this.groupBox11.TabIndex = 12;
			this.groupBox11.TabStop = false;
			this.groupBox11.Text = "Recording format:";
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(16, 24);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(40, 16);
			this.label22.TabIndex = 11;
			this.label22.Text = "Format:";
			// 
			// comboBoxCaptureFormat
			// 
			this.comboBoxCaptureFormat.Items.AddRange(new object[] {
																															 ".avi",
																															 ".asf",
																															 ".mpg"});
			this.comboBoxCaptureFormat.Location = new System.Drawing.Point(64, 24);
			this.comboBoxCaptureFormat.Name = "comboBoxCaptureFormat";
			this.comboBoxCaptureFormat.Size = new System.Drawing.Size(88, 21);
			this.comboBoxCaptureFormat.TabIndex = 0;
			this.comboBoxCaptureFormat.Text = ".avi";
			// 
			// checkBoxTV
			// 
			this.checkBoxTV.Location = new System.Drawing.Point(416, 120);
			this.checkBoxTV.Name = "checkBoxTV";
			this.checkBoxTV.Size = new System.Drawing.Size(136, 24);
			this.checkBoxTV.TabIndex = 5;
			this.checkBoxTV.Text = "Use for watching TV";
			// 
			// checkBoxRecord
			// 
			this.checkBoxRecord.Location = new System.Drawing.Point(416, 144);
			this.checkBoxRecord.Name = "checkBoxRecord";
			this.checkBoxRecord.Size = new System.Drawing.Size(120, 24);
			this.checkBoxRecord.TabIndex = 6;
			this.checkBoxRecord.Text = "Use for recording";
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(400, 96);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(176, 88);
			this.groupBox1.TabIndex = 15;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Purpose:";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(464, 320);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(56, 23);
			this.btnCancel.TabIndex = 7;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// comboFrameSize
			// 
			this.comboFrameSize.Location = new System.Drawing.Point(432, 224);
			this.comboFrameSize.Name = "comboFrameSize";
			this.comboFrameSize.Size = new System.Drawing.Size(112, 21);
			this.comboFrameSize.TabIndex = 16;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textBoxFrameRate);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Location = new System.Drawing.Point(400, 192);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(176, 120);
			this.groupBox2.TabIndex = 17;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Size/Rate";
			// 
			// textBoxFrameRate
			// 
			this.textBoxFrameRate.Location = new System.Drawing.Point(32, 88);
			this.textBoxFrameRate.Name = "textBoxFrameRate";
			this.textBoxFrameRate.Size = new System.Drawing.Size(64, 20);
			this.textBoxFrameRate.TabIndex = 2;
			this.textBoxFrameRate.Text = "";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 64);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(64, 16);
			this.label6.TabIndex = 1;
			this.label6.Text = "Framerate:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 16);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(104, 16);
			this.label5.TabIndex = 0;
			this.label5.Text = "Framesize:";
			// 
			// buttonSetupFilter
			// 
			this.buttonSetupFilter.Location = new System.Drawing.Point(24, 320);
			this.buttonSetupFilter.Name = "buttonSetupFilter";
			this.buttonSetupFilter.TabIndex = 18;
			this.buttonSetupFilter.Text = "Setup Filter";
			this.buttonSetupFilter.Click += new System.EventHandler(this.buttonSetupFilter_Click);
			// 
			// FormCapture
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(592, 349);
			this.Controls.Add(this.buttonSetupFilter);
			this.Controls.Add(this.comboFrameSize);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.checkBoxRecord);
			this.Controls.Add(this.checkBoxTV);
			this.Controls.Add(this.groupBox11);
			this.Controls.Add(this.listFilters);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.comboAudioCompressor);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.comboVideoCompressor);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.comboAudioDevice);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.comboVideoDevice);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBox2);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormCapture";
			this.Text = "FormCapture";
			this.Load += new System.EventHandler(this.FormCapture_Load);
			this.groupBox11.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

    private void FormCapture_Load(object sender, System.EventArgs e)
    {
      Filters filters = new Filters();
      // video capture devices
      comboVideoDevice.Items.Clear();
      comboVideoDevice.Items.Add("none");
      int i=1;
      int index=0;
      foreach (Filter filter in filters.VideoInputDevices)
      {
        comboVideoDevice.Items.Add(filter.Name);
        if (m_strVideoDevice.Length>0 && String.Compare(filter.Name,m_strVideoDevice,true)==0) index=i;
        ++i;
      }
      comboVideoDevice.SelectedIndex=index;

      // audio capture devices
      comboAudioDevice.Items.Clear();
      comboAudioDevice.Items.Add("none");
      i=1;
      index=0;
      foreach (Filter filter in filters.AudioInputDevices)
      {
        comboAudioDevice.Items.Add(filter.Name);
        if (m_strAudioDevice.Length>0 && String.Compare(filter.Name,m_strAudioDevice,true)==0) index=i;
        ++i;
      }
      comboAudioDevice.SelectedIndex=index;

      // audio compressors
      comboAudioCompressor.Items.Clear();
      comboAudioCompressor.Items.Add("none");
      i=1;
      index=0;
      foreach (Filter filter in filters.AudioCompressors)
      {           
        comboAudioCompressor.Items.Add(filter.Name);
        if (m_strAudioCompressor.Length>0 && String.Compare(filter.Name,m_strAudioCompressor,true)==0) index=i;
        ++i;
      }
      comboAudioCompressor.SelectedIndex=index;

      // Video compressors
      comboVideoCompressor.Items.Clear();
      comboVideoCompressor.Items.Add("none");
      i=1;
      index=0;
      foreach (Filter filter in filters.VideoCompressors)
      {
        comboVideoCompressor.Items.Add(filter.Name);
        if (m_strVideoCompressor.Length>0 && String.Compare(filter.Name,m_strVideoCompressor,true)==0) index=i;
        ++i;
      }
      comboVideoCompressor.SelectedIndex=index;

      for (int x=0; x < comboBoxCaptureFormat.Items.Count;++x)
      {
        if ( ((string)comboBoxCaptureFormat.Items[x]) ==CaptureFormat)
        {
          comboBoxCaptureFormat.SelectedIndex=x;
          break;
        }
      }

      checkBoxRecord.Checked=m_bUseForRecording;
      checkBoxTV.Checked=m_bUseForTV;
      SetupPropertyPageList();

      textBoxFrameRate.Text=m_FrameRate.ToString();
      comboFrameSize.Items.Clear();
      int iformat=0;
      int isel=0;
      foreach (CapFormat fmt in m_formats)
      {
        comboFrameSize.Items.Add(fmt.description);
        if (FrameSize.Width==fmt.width && FrameSize.Height==fmt.height) isel=iformat;
        iformat++;
      }
      comboFrameSize.SelectedIndex=isel;
      this.comboVideoDevice.SelectedIndexChanged += new System.EventHandler(this.comboVideoDevice_SelectedIndexChanged_1);

    }


    private void buttonOK_Click(object sender, System.EventArgs e)
    {
      VideoDevice=(string)comboVideoDevice.SelectedItem;
      AudioDevice=(string)comboAudioDevice.SelectedItem;
      AudioCompressor=(string)comboAudioCompressor.SelectedItem;
      VideoCompressor=(string)comboVideoCompressor.SelectedItem;
      CaptureFormat=(string)comboBoxCaptureFormat.SelectedItem;
      m_bUseForRecording=checkBoxRecord.Checked;
      m_bUseForTV=checkBoxTV.Checked;
      string strItem=(string)comboFrameSize.SelectedItem;
      foreach (CapFormat fmt in m_formats)
      {
        if (String.Compare(fmt.description,strItem,true)==0)
        {
          m_FrameSize=new Size(fmt.width,fmt.height);
        }
      }
      m_FrameRate=Convert.ToDouble(textBoxFrameRate.Text);
      this.Close();
    }

    private void btnCancel_Click(object sender, System.EventArgs e)
    {
      VideoDevice="";
      AudioDevice="";
      AudioCompressor="";
      VideoCompressor="";
      this.Close();
    }

    public Size FrameSize
    {
      get { return m_FrameSize;}
      set {m_FrameSize=value;}
    }

    public double FrameRate
    {
      get { return m_FrameRate;}
      set {m_FrameRate=value;}
    }

    public string AudioCompressor
    {
      get { return m_strAudioCompressor;}
      set { m_strAudioCompressor=value;}
    }

    public string VideoCompressor
    {
      get { return m_strVideoCompressor;}
      set { m_strVideoCompressor=value;}
    }

    public string VideoDevice
    {
      get { return m_strVideoDevice;}
      set { m_strVideoDevice=value;}
    }
    public string AudioDevice
    {
      get { return m_strAudioDevice;}
      set { m_strAudioDevice=value;}
    }
    public string CaptureFormat
    {
      get { return m_strCaptureFormat;}
      set { m_strCaptureFormat=value;}
    }
    public bool UseForTV
    {
      get { return m_bUseForTV;}
      set { m_bUseForTV=value;}
    }
    public bool UseForRecording
    {
      get { return m_bUseForRecording;}
      set { m_bUseForRecording=value;}
    }
    
    public int ID
    {
      get { return m_iID;}
      set { m_iID=value;}
    }
    private void SetupPropertyPageList()
    {
      listFilters.Items.Clear();
      Capture cap=setupgraph();
      if (cap==null) 
      {
        comboVideoCompressor.Enabled=true;
        comboAudioCompressor.Enabled=true;
        comboAudioDevice.Enabled=true;
        comboBoxCaptureFormat.Enabled=true;
        return;
      }

      if (cap.SupportsTimeShifting)
      {
        comboVideoCompressor.SelectedIndex=0;
        comboVideoCompressor.Enabled=false;

        comboAudioCompressor.SelectedIndex=0;
        comboAudioCompressor.Enabled=false;

        comboAudioDevice.SelectedIndex=0;
        comboAudioDevice.Enabled=false;

        comboBoxCaptureFormat.SelectedIndex=2;
        comboBoxCaptureFormat.Enabled=false;
      }

      if (cap.PropertyPages!=null)
      {
        foreach (PropertyPage page in cap.PropertyPages)
        {
          listFilters.Items.Add( page.Name);
        }
      }
      cap.Stop();
      cap.Dispose();
      cap=null;
    }
    private void listFilters_DoubleClick(object sender, System.EventArgs e)
    {
      if (listFilters.SelectedItems.Count==0) return;
      int iItem=listFilters.SelectedIndices[0];
      int i=0;
      Capture cap=setupgraph();
      if (cap==null) return;
      if (cap.PropertyPages!=null)
      {
        foreach (PropertyPage page in cap.PropertyPages)
        {
          if (i==iItem)
          {
            page.Show(this);
            cap.SaveSettings(m_iID);
            break;
          }
          i++;
        }
      }
      cap.Stop();
      cap.Dispose();
      cap=null;
    }

    private void comboVideoDevice_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }

    private void comboAudioDevice_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }

    private void comboVideoCompressor_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }

    private void comboAudioCompressor_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }

    private Capture setupgraph()
    {
      string strVideoDevice=(string )comboVideoDevice.SelectedItem;
      string strAudioDevice=(string )comboAudioDevice.SelectedItem;
      string strCompressorAudio=(string)comboAudioCompressor.SelectedItem;
      string strCompressorVideo=(string)comboVideoCompressor.SelectedItem;

      
      DShowNET.Filter videoDevice=null;
      DShowNET.Filter audioDevice=null;
      Filters filters=new Filters();
      // find video capture device
      foreach (Filter filter in filters.VideoInputDevices)
      {
        if (String.Compare(filter.Name,strVideoDevice)==0)
        {
          videoDevice=filter;
          break;
        }
      }
      // find audio capture device
      foreach (Filter filter in filters.AudioInputDevices)
      {
        if (String.Compare(filter.Name,strAudioDevice)==0)
        {
          audioDevice=filter;
          break;
        }
      }
      
    
      // create new capture!
      Capture capture=null;
      try
      {
        capture = new Capture(videoDevice,audioDevice);
      }
      catch(Exception)
      {
        return null;
      }

      try
      {
        // add audio compressor
        foreach (Filter filter in filters.AudioCompressors)
        {
          if (String.Compare(filter.Name,strCompressorAudio)==0)
          {
            capture.AudioCompressor=filter;
            break;
          }
        }
      }
      catch (Exception)
      {
      }

      try
      {
        //add video compressor
        foreach (Filter filter in filters.VideoCompressors)
        {
          if (String.Compare(filter.Name,strCompressorVideo)==0)
          {
            capture.VideoCompressor=filter;
            break;
          }
        }

      }
      catch (Exception)
      {
      }
      if (capture!=null) capture.LoadSettings(ID);
      return capture;
    }

    private void comboVideoDevice_SelectedIndexChanged_1(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }

		private void buttonSetupFilter_Click(object sender, System.EventArgs e)
		{
			listFilters_DoubleClick(null,null);		
		}


	}
}
