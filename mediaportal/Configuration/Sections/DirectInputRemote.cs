using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	/// <summary>
	/// Summary description for DirectInputRemote.
	/// </summary>
  public class DirectInputRemote : SectionSettings
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private Container components = null;

    private DirectInputHandler diHandler = null;
    private System.Windows.Forms.Label label2sec;
    private System.Windows.Forms.Label label0sec;
    private System.Windows.Forms.Label labelDelay;
    public System.Windows.Forms.TrackBar trackBarDelay;
    private System.Windows.Forms.Button buttonDefault;
    private System.Windows.Forms.Button btnRunControlPanel;
    private System.Windows.Forms.Label lblDInputDevice;
    private System.Windows.Forms.ComboBox cbDevices;
    private System.Windows.Forms.TextBox txtMonitor;
    private System.Windows.Forms.GroupBox gbI;
    private System.Windows.Forms.GroupBox gbSettings;
    private System.Windows.Forms.Button btnMapping;
    private CheckBox cbEnable;

    public DirectInputRemote() : this("Direct Input")
    {
    }


    public DirectInputRemote(string name) : base(name)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
      diHandler = new DirectInputHandler();
      diHandler.DoSendActions = false; // only debug/display actions
      diHandler.OnStateChangeText += new DirectInputHandler.diStateChangeText(StateChangeAsText);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
      diHandler.OnStateChangeText -= new DirectInputHandler.diStateChangeText(StateChangeAsText);
      diHandler.Stop();
      diHandler = null;
      if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.cbEnable = new System.Windows.Forms.CheckBox();
      this.gbI = new System.Windows.Forms.GroupBox();
      this.txtMonitor = new System.Windows.Forms.TextBox();
      this.gbSettings = new System.Windows.Forms.GroupBox();
      this.btnRunControlPanel = new System.Windows.Forms.Button();
      this.lblDInputDevice = new System.Windows.Forms.Label();
      this.cbDevices = new System.Windows.Forms.ComboBox();
      this.label2sec = new System.Windows.Forms.Label();
      this.label0sec = new System.Windows.Forms.Label();
      this.labelDelay = new System.Windows.Forms.Label();
      this.trackBarDelay = new System.Windows.Forms.TrackBar();
      this.buttonDefault = new System.Windows.Forms.Button();
      this.btnMapping = new System.Windows.Forms.Button();
      this.gbI.SuspendLayout();
      this.gbSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarDelay)).BeginInit();
      this.SuspendLayout();
      // 
      // cbEnable
      // 
      this.cbEnable.Location = new System.Drawing.Point(8, 8);
      this.cbEnable.Name = "cbEnable";
      this.cbEnable.Size = new System.Drawing.Size(176, 24);
      this.cbEnable.TabIndex = 1;
      this.cbEnable.Text = "Enable Direct Input";
      this.cbEnable.CheckedChanged += new System.EventHandler(this.cbEnable_CheckedChanged);
      // 
      // gbI
      // 
      this.gbI.Controls.Add(this.txtMonitor);
      this.gbI.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.gbI.Location = new System.Drawing.Point(8, 208);
      this.gbI.Name = "gbI";
      this.gbI.Size = new System.Drawing.Size(432, 176);
      this.gbI.TabIndex = 14;
      this.gbI.TabStop = false;
      this.gbI.Text = "Information";
      // 
      // txtMonitor
      // 
      this.txtMonitor.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.txtMonitor.Location = new System.Drawing.Point(3, 21);
      this.txtMonitor.Multiline = true;
      this.txtMonitor.Name = "txtMonitor";
      this.txtMonitor.ReadOnly = true;
      this.txtMonitor.Size = new System.Drawing.Size(426, 152);
      this.txtMonitor.TabIndex = 4;
      this.txtMonitor.Text = "";
      // 
      // gbSettings
      // 
      this.gbSettings.Controls.Add(this.btnRunControlPanel);
      this.gbSettings.Controls.Add(this.lblDInputDevice);
      this.gbSettings.Controls.Add(this.cbDevices);
      this.gbSettings.Controls.Add(this.label2sec);
      this.gbSettings.Controls.Add(this.label0sec);
      this.gbSettings.Controls.Add(this.labelDelay);
      this.gbSettings.Controls.Add(this.trackBarDelay);
      this.gbSettings.Controls.Add(this.buttonDefault);
      this.gbSettings.Enabled = false;
      this.gbSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.gbSettings.Location = new System.Drawing.Point(8, 40);
      this.gbSettings.Name = "gbSettings";
      this.gbSettings.Size = new System.Drawing.Size(440, 160);
      this.gbSettings.TabIndex = 13;
      this.gbSettings.TabStop = false;
      this.gbSettings.Text = "Settings";
      // 
      // btnRunControlPanel
      // 
      this.btnRunControlPanel.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnRunControlPanel.Location = new System.Drawing.Point(320, 28);
      this.btnRunControlPanel.Name = "btnRunControlPanel";
      this.btnRunControlPanel.Size = new System.Drawing.Size(112, 24);
      this.btnRunControlPanel.TabIndex = 15;
      this.btnRunControlPanel.Text = "Run Control Panel";
      this.btnRunControlPanel.Click += new System.EventHandler(this.btnRunControlPanel_Click);
      // 
      // lblDInputDevice
      // 
      this.lblDInputDevice.Location = new System.Drawing.Point(12, 32);
      this.lblDInputDevice.Name = "lblDInputDevice";
      this.lblDInputDevice.Size = new System.Drawing.Size(104, 23);
      this.lblDInputDevice.TabIndex = 14;
      this.lblDInputDevice.Text = "Direct Input Device:";
      // 
      // cbDevices
      // 
      this.cbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDevices.Location = new System.Drawing.Point(116, 28);
      this.cbDevices.Name = "cbDevices";
      this.cbDevices.Size = new System.Drawing.Size(200, 21);
      this.cbDevices.TabIndex = 13;
      this.cbDevices.SelectedIndexChanged += new System.EventHandler(this.cbDevices_SelectedIndexChanged);
      // 
      // label2sec
      // 
      this.label2sec.BackColor = System.Drawing.SystemColors.Control;
      this.label2sec.Enabled = false;
      this.label2sec.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label2sec.Location = new System.Drawing.Point(228, 88);
      this.label2sec.Name = "label2sec";
      this.label2sec.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.label2sec.Size = new System.Drawing.Size(40, 16);
      this.label2sec.TabIndex = 12;
      this.label2sec.Text = "2 sec.";
      // 
      // label0sec
      // 
      this.label0sec.BackColor = System.Drawing.SystemColors.Control;
      this.label0sec.Enabled = false;
      this.label0sec.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label0sec.Location = new System.Drawing.Point(116, 88);
      this.label0sec.Name = "label0sec";
      this.label0sec.Size = new System.Drawing.Size(40, 16);
      this.label0sec.TabIndex = 11;
      this.label0sec.Text = "0 sec.";
      // 
      // labelDelay
      // 
      this.labelDelay.Enabled = false;
      this.labelDelay.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.labelDelay.Location = new System.Drawing.Point(16, 72);
      this.labelDelay.Name = "labelDelay";
      this.labelDelay.Size = new System.Drawing.Size(96, 23);
      this.labelDelay.TabIndex = 10;
      this.labelDelay.Text = "Repeat-delay:";
      // 
      // trackBarDelay
      // 
      this.trackBarDelay.Enabled = false;
      this.trackBarDelay.LargeChange = 100;
      this.trackBarDelay.Location = new System.Drawing.Point(116, 64);
      this.trackBarDelay.Maximum = 2000;
      this.trackBarDelay.Name = "trackBarDelay";
      this.trackBarDelay.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.trackBarDelay.Size = new System.Drawing.Size(152, 45);
      this.trackBarDelay.SmallChange = 100;
      this.trackBarDelay.TabIndex = 3;
      this.trackBarDelay.TickFrequency = 1000;
      this.trackBarDelay.TickStyle = System.Windows.Forms.TickStyle.None;
      // 
      // buttonDefault
      // 
      this.buttonDefault.Enabled = false;
      this.buttonDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonDefault.Location = new System.Drawing.Point(320, 120);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(112, 24);
      this.buttonDefault.TabIndex = 9;
      this.buttonDefault.Text = "Reset to &default";
      // 
      // btnMapping
      // 
      this.btnMapping.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnMapping.Location = new System.Drawing.Point(355, 12);
      this.btnMapping.Name = "btnMapping";
      this.btnMapping.TabIndex = 16;
      this.btnMapping.Text = "Mapping";
      this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
      // 
      // DirectInputRemote
      // 
      this.Controls.Add(this.btnMapping);
      this.Controls.Add(this.gbI);
      this.Controls.Add(this.gbSettings);
      this.Controls.Add(this.cbEnable);
      this.Name = "DirectInputRemote";
      this.Size = new System.Drawing.Size(456, 392);
      this.Load += new System.EventHandler(this.DirectInputRemote_Load);
      this.gbI.ResumeLayout(false);
      this.gbSettings.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarDelay)).EndInit();
      this.ResumeLayout(false);

    }
		#endregion

    private void cbDevices_SelectedIndexChanged(object sender, EventArgs e)
    {
      int index = cbDevices.SelectedIndex;
      if (index >= 0)
      {
        diHandler.SelectDevice(diHandler.DeviceGUIDs[index].ToString());
        txtMonitor.Text = "";
      }
    }

    void SyncCombo()
    {
      cbDevices.Items.Clear();
      foreach (string deviceName in diHandler.DeviceNames)
      {
        cbDevices.Items.Add(deviceName);
      }
    }

    private void cbEnable_CheckedChanged(object sender, EventArgs e)
    {
      diHandler.Active = cbEnable.Checked;
      gbSettings.Enabled = cbEnable.Checked;
      if (cbEnable.Checked)
      {
        SyncCombo();
      }
      else
      {
        txtMonitor.Text = "";
      }
    }

    void StateChangeAsText(object sender, string stateText)
    {
      txtMonitor.Text = stateText;
    }

    private void btnRunControlPanel_Click(object sender, System.EventArgs e)
    {
      diHandler.RunControlPanel(); 
    }

    private void DirectInputRemote_Load(object sender, System.EventArgs e)
    {
      cbEnable.Checked = diHandler.Active;
      cbDevices.SelectedIndex = diHandler.SelectedDeviceIndex;
    }


    public override void LoadSettings()
    {
      diHandler.LoadSettings();
    }

    public override void SaveSettings()
    {
      diHandler.SaveSettings();
    }

    private void btnMapping_Click(object sender, System.EventArgs e)
    {
      HCWMappingForm dlg = new HCWMappingForm("DirectInput");
      dlg.ShowDialog(this);    
    }



	}
}
