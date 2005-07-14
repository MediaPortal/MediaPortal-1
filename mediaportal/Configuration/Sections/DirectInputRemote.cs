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
    private System.Windows.Forms.Button buttonDefault;
    private System.Windows.Forms.Button btnRunControlPanel;
    private System.Windows.Forms.Label lblDInputDevice;
    private System.Windows.Forms.ComboBox cbDevices;
    private System.Windows.Forms.TextBox txtMonitor;
    private System.Windows.Forms.GroupBox gbI;
    private System.Windows.Forms.GroupBox gbSettings;
    private System.Windows.Forms.Button btnMapping;
    private System.Windows.Forms.Label lblDelayMS;
    private System.Windows.Forms.NumericUpDown numDelay;
    private System.Windows.Forms.GroupBox groupBox1;
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
      this.buttonDefault = new System.Windows.Forms.Button();
      this.numDelay = new System.Windows.Forms.NumericUpDown();
      this.cbDevices = new System.Windows.Forms.ComboBox();
      this.lblDelayMS = new System.Windows.Forms.Label();
      this.lblDInputDevice = new System.Windows.Forms.Label();
      this.btnMapping = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.gbI.SuspendLayout();
      this.gbSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // cbEnable
      // 
      this.cbEnable.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbEnable.Location = new System.Drawing.Point(16, 24);
      this.cbEnable.Name = "cbEnable";
      this.cbEnable.Size = new System.Drawing.Size(120, 16);
      this.cbEnable.TabIndex = 1;
      this.cbEnable.Text = "Enable Direct Input";
      this.cbEnable.CheckedChanged += new System.EventHandler(this.cbEnable_CheckedChanged);
      // 
      // gbI
      // 
      this.gbI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.gbI.Controls.Add(this.txtMonitor);
      this.gbI.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.gbI.Location = new System.Drawing.Point(0, 176);
      this.gbI.Name = "gbI";
      this.gbI.Size = new System.Drawing.Size(472, 232);
      this.gbI.TabIndex = 14;
      this.gbI.TabStop = false;
      this.gbI.Text = "Information";
      // 
      // txtMonitor
      // 
      this.txtMonitor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtMonitor.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.txtMonitor.Location = new System.Drawing.Point(16, 24);
      this.txtMonitor.Multiline = true;
      this.txtMonitor.Name = "txtMonitor";
      this.txtMonitor.ReadOnly = true;
      this.txtMonitor.Size = new System.Drawing.Size(440, 192);
      this.txtMonitor.TabIndex = 4;
      this.txtMonitor.Text = "";
      // 
      // gbSettings
      // 
      this.gbSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.gbSettings.Controls.Add(this.btnRunControlPanel);
      this.gbSettings.Controls.Add(this.buttonDefault);
      this.gbSettings.Controls.Add(this.numDelay);
      this.gbSettings.Controls.Add(this.cbDevices);
      this.gbSettings.Controls.Add(this.lblDelayMS);
      this.gbSettings.Controls.Add(this.lblDInputDevice);
      this.gbSettings.Enabled = false;
      this.gbSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.gbSettings.Location = new System.Drawing.Point(0, 64);
      this.gbSettings.Name = "gbSettings";
      this.gbSettings.Size = new System.Drawing.Size(472, 104);
      this.gbSettings.TabIndex = 13;
      this.gbSettings.TabStop = false;
      this.gbSettings.Text = "Settings";
      // 
      // btnRunControlPanel
      // 
      this.btnRunControlPanel.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnRunControlPanel.Location = new System.Drawing.Point(344, 28);
      this.btnRunControlPanel.Name = "btnRunControlPanel";
      this.btnRunControlPanel.Size = new System.Drawing.Size(112, 22);
      this.btnRunControlPanel.TabIndex = 15;
      this.btnRunControlPanel.Text = "Run Control Panel";
      this.btnRunControlPanel.Click += new System.EventHandler(this.btnRunControlPanel_Click);
      // 
      // buttonDefault
      // 
      this.buttonDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonDefault.Location = new System.Drawing.Point(344, 64);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(112, 22);
      this.buttonDefault.TabIndex = 9;
      this.buttonDefault.Text = "Reset to &default";
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // numDelay
      // 
      this.numDelay.Increment = new System.Decimal(new int[] {
                                                               10,
                                                               0,
                                                               0,
                                                               0});
      this.numDelay.Location = new System.Drawing.Point(128, 64);
      this.numDelay.Maximum = new System.Decimal(new int[] {
                                                             2000,
                                                             0,
                                                             0,
                                                             0});
      this.numDelay.Name = "numDelay";
      this.numDelay.Size = new System.Drawing.Size(52, 20);
      this.numDelay.TabIndex = 18;
      this.numDelay.Value = new System.Decimal(new int[] {
                                                           150,
                                                           0,
                                                           0,
                                                           0});
      this.numDelay.ValueChanged += new System.EventHandler(this.numDelay_ValueChanged);
      // 
      // cbDevices
      // 
      this.cbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDevices.Location = new System.Drawing.Point(128, 28);
      this.cbDevices.Name = "cbDevices";
      this.cbDevices.Size = new System.Drawing.Size(200, 21);
      this.cbDevices.TabIndex = 13;
      this.cbDevices.SelectedIndexChanged += new System.EventHandler(this.cbDevices_SelectedIndexChanged);
      // 
      // lblDelayMS
      // 
      this.lblDelayMS.Location = new System.Drawing.Point(16, 68);
      this.lblDelayMS.Name = "lblDelayMS";
      this.lblDelayMS.Size = new System.Drawing.Size(72, 16);
      this.lblDelayMS.TabIndex = 16;
      this.lblDelayMS.Text = "Delay [ms]:";
      // 
      // lblDInputDevice
      // 
      this.lblDInputDevice.Location = new System.Drawing.Point(16, 32);
      this.lblDInputDevice.Name = "lblDInputDevice";
      this.lblDInputDevice.Size = new System.Drawing.Size(104, 16);
      this.lblDInputDevice.TabIndex = 14;
      this.lblDInputDevice.Text = "Direct Input Device:";
      // 
      // btnMapping
      // 
      this.btnMapping.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnMapping.Location = new System.Drawing.Point(384, 20);
      this.btnMapping.Name = "btnMapping";
      this.btnMapping.Size = new System.Drawing.Size(72, 22);
      this.btnMapping.TabIndex = 16;
      this.btnMapping.Text = "Mapping";
      this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.btnMapping);
      this.groupBox1.Controls.Add(this.cbEnable);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 56);
      this.groupBox1.TabIndex = 15;
      this.groupBox1.TabStop = false;
      // 
      // DirectInputRemote
      // 
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.gbI);
      this.Controls.Add(this.gbSettings);
      this.Name = "DirectInputRemote";
      this.Size = new System.Drawing.Size(472, 408);
      this.Load += new System.EventHandler(this.DirectInputRemote_Load);
      this.gbI.ResumeLayout(false);
      this.gbSettings.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
      this.groupBox1.ResumeLayout(false);
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
//      numDelay.Value = diHandler.Delay; FIX THIS WAEBERD
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

    private void buttonDefault_Click(object sender, System.EventArgs e)
    {
      numDelay.Value = (decimal)150;
    }

    private void numDelay_ValueChanged(object sender, System.EventArgs e)
    {
//      diHandler.Delay = (int)numDelay.Value; FIX THIS WAEBERD
    }
	}
}
