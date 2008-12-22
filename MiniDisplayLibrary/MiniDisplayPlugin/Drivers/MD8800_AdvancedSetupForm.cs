using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class MD8800_AdvancedSetupForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    private MPButton btnOK;
    private MPButton btnReset;
    private MPComboBox cmbBlankIdleTime;
    private readonly IContainer components = null;
    private object ControlStateLock = new object();
    private MPGroupBox groupBox1;
    private GroupBox groupBox5;
    private CheckBox mpBlankDisplayWhenIdle;
    private CheckBox mpBlankDisplayWithVideo;
    private CheckBox mpEnableDisplayAction;
    private MPComboBox mpEnableDisplayActionTime;

    public MD8800_AdvancedSetupForm()
    {
      Log.Debug("MD8800_AdvancedSetupForm(): Constructor started", new object[0]);
      this.InitializeComponent();
      this.mpBlankDisplayWithVideo.DataBindings.Add("Checked", MD8800.AdvancedSettings.Instance, "BlankDisplayWithVideo");
      this.mpEnableDisplayAction.DataBindings.Add("Checked", MD8800.AdvancedSettings.Instance, "EnableDisplayAction");
      this.mpEnableDisplayActionTime.DataBindings.Add("SelectedIndex", MD8800.AdvancedSettings.Instance, "EnableDisplayActionTime");
      this.mpBlankDisplayWhenIdle.DataBindings.Add("Checked", MD8800.AdvancedSettings.Instance, "BlankDisplayWhenIdle");
      this.cmbBlankIdleTime.DataBindings.Add("SelectedIndex", MD8800.AdvancedSettings.Instance, "BlankIdleTime");
      this.Refresh();
      this.SetControlState();
      Log.Debug("MD8800_AdvancedSetupForm(): Constructor completed", new object[0]);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Log.Debug("MD8800_AdvancedSetupForm.btnOK_Click(): started", new object[0]);
      MD8800.AdvancedSettings.Save();
      base.Hide();
      base.Close();
      Log.Debug("MD8800_AdvancedSetupForm.btnOK_Click(): Completed", new object[0]);
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      Log.Debug("MD8800_AdvancedSetupForm.btnReset_Click(): started", new object[0]);
      MD8800.AdvancedSettings.SetDefaults();
      this.mpBlankDisplayWithVideo.Checked = MD8800.AdvancedSettings.Instance.BlankDisplayWithVideo;
      this.mpEnableDisplayAction.Checked = MD8800.AdvancedSettings.Instance.EnableDisplayAction;
      this.mpEnableDisplayActionTime.SelectedIndex = MD8800.AdvancedSettings.Instance.EnableDisplayActionTime;
      this.mpBlankDisplayWhenIdle.Checked = MD8800.AdvancedSettings.Instance.BlankDisplayWhenIdle;
      this.cmbBlankIdleTime.SelectedIndex = MD8800.AdvancedSettings.Instance.BlankIdleTime;
      this.Refresh();
      Log.Debug("MD8800_AdvancedSetupForm.btnReset_Click(): Completed", new object[0]);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && (this.components != null))
      {
        this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox5 = new System.Windows.Forms.GroupBox();
      this.mpEnableDisplayActionTime = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cmbBlankIdleTime = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpEnableDisplayAction = new System.Windows.Forms.CheckBox();
      this.mpBlankDisplayWithVideo = new System.Windows.Forms.CheckBox();
      this.mpBlankDisplayWhenIdle = new System.Windows.Forms.CheckBox();
      this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnReset = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.groupBox5);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(9, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(357, 124);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = " MD8800 (Dritek) Display Configuration ";
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.mpEnableDisplayActionTime);
      this.groupBox5.Controls.Add(this.cmbBlankIdleTime);
      this.groupBox5.Controls.Add(this.mpEnableDisplayAction);
      this.groupBox5.Controls.Add(this.mpBlankDisplayWithVideo);
      this.groupBox5.Controls.Add(this.mpBlankDisplayWhenIdle);
      this.groupBox5.Location = new System.Drawing.Point(10, 19);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(338, 97);
      this.groupBox5.TabIndex = 23;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = " Display Control Options ";
      // 
      // mpEnableDisplayActionTime
      // 
      this.mpEnableDisplayActionTime.BorderColor = System.Drawing.Color.Empty;
      this.mpEnableDisplayActionTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpEnableDisplayActionTime.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20"});
      this.mpEnableDisplayActionTime.Location = new System.Drawing.Point(181, 36);
      this.mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
      this.mpEnableDisplayActionTime.Size = new System.Drawing.Size(42, 21);
      this.mpEnableDisplayActionTime.TabIndex = 96;
      // 
      // cmbBlankIdleTime
      // 
      this.cmbBlankIdleTime.BorderColor = System.Drawing.Color.Empty;
      this.cmbBlankIdleTime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbBlankIdleTime.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30"});
      this.cmbBlankIdleTime.Location = new System.Drawing.Point(167, 58);
      this.cmbBlankIdleTime.Name = "cmbBlankIdleTime";
      this.cmbBlankIdleTime.Size = new System.Drawing.Size(42, 21);
      this.cmbBlankIdleTime.TabIndex = 98;
      // 
      // mpEnableDisplayAction
      // 
      this.mpEnableDisplayAction.AutoSize = true;
      this.mpEnableDisplayAction.Location = new System.Drawing.Point(23, 38);
      this.mpEnableDisplayAction.Name = "mpEnableDisplayAction";
      this.mpEnableDisplayAction.Size = new System.Drawing.Size(258, 17);
      this.mpEnableDisplayAction.TabIndex = 97;
      this.mpEnableDisplayAction.Text = "Enable Display on Action for                   Seconds";
      this.mpEnableDisplayAction.UseVisualStyleBackColor = true;
      this.mpEnableDisplayAction.CheckedChanged += new System.EventHandler(this.mpEnableDisplayAction_CheckedChanged);
      // 
      // mpBlankDisplayWithVideo
      // 
      this.mpBlankDisplayWithVideo.AutoSize = true;
      this.mpBlankDisplayWithVideo.Location = new System.Drawing.Point(7, 17);
      this.mpBlankDisplayWithVideo.Name = "mpBlankDisplayWithVideo";
      this.mpBlankDisplayWithVideo.Size = new System.Drawing.Size(207, 17);
      this.mpBlankDisplayWithVideo.TabIndex = 95;
      this.mpBlankDisplayWithVideo.Text = "Turn off display during Video Playback";
      this.mpBlankDisplayWithVideo.UseVisualStyleBackColor = true;
      this.mpBlankDisplayWithVideo.CheckedChanged += new System.EventHandler(this.mpBlankDisplayWithVideo_CheckedChanged);
      // 
      // mpBlankDisplayWhenIdle
      // 
      this.mpBlankDisplayWhenIdle.AutoSize = true;
      this.mpBlankDisplayWhenIdle.Location = new System.Drawing.Point(7, 60);
      this.mpBlankDisplayWhenIdle.Name = "mpBlankDisplayWhenIdle";
      this.mpBlankDisplayWhenIdle.Size = new System.Drawing.Size(261, 17);
      this.mpBlankDisplayWhenIdle.TabIndex = 99;
      this.mpBlankDisplayWhenIdle.Text = "Turn off display when idle for                    seconds";
      this.mpBlankDisplayWhenIdle.UseVisualStyleBackColor = true;
      this.mpBlankDisplayWhenIdle.CheckedChanged += new System.EventHandler(this.mpBlankDisplayWhenIdle_CheckedChanged);
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(286, 136);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(80, 23);
      this.btnOK.TabIndex = 108;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnReset
      // 
      this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnReset.Location = new System.Drawing.Point(200, 136);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new System.Drawing.Size(80, 23);
      this.btnReset.TabIndex = 109;
      this.btnReset.Text = "&RESET";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
      // 
      // MD8800_AdvancedSetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(378, 165);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(this.groupBox1);
      this.Name = "MD8800_AdvancedSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup - Advanced Settings";
      this.groupBox1.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.ResumeLayout(false);

    }

    private void mpBlankDisplayWhenIdle_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void mpBlankDisplayWithVideo_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void mpEnableDisplayAction_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void SetControlState()
    {
      if (this.mpBlankDisplayWithVideo.Checked)
      {
        this.mpEnableDisplayAction.Enabled = true;
        if (this.mpEnableDisplayAction.Checked)
        {
          this.mpEnableDisplayActionTime.Enabled = true;
        }
        else
        {
          this.mpEnableDisplayActionTime.Enabled = false;
        }
      }
      else
      {
        this.mpEnableDisplayAction.Enabled = false;
        this.mpEnableDisplayActionTime.Enabled = false;
      }
      if (this.mpBlankDisplayWhenIdle.Checked)
      {
        this.cmbBlankIdleTime.Enabled = true;
      }
      else
      {
        this.cmbBlankIdleTime.Enabled = false;
      }
    }
  }
}

