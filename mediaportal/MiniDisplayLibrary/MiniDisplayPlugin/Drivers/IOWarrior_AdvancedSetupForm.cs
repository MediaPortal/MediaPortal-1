using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class IOWarrior_AdvancedSetupForm : MPConfigForm
  {
    private MPButton btnOK;
    private MPButton btnReset;
    private RadioButton cbNormalEQ;
    private RadioButton cbStereoEQ;
    private CheckBox cbUseClockOnShutdown;
    private RadioButton cbUseVUmeter;
    private RadioButton cbUseVUmeter2;
    private CheckBox cbVUindicators;
    private MPComboBox cmbBlankIdleTime;
    private MPComboBox cmbDelayEqTime;
    private MPComboBox cmbEqRate;
    private MPComboBox cmbEQTitleDisplayTime;
    private MPComboBox cmbEQTitleShowTime;
    private readonly IContainer components = null;
    private MPGroupBox groupBox1;
    private GroupBox groupBox5;
    private GroupBox groupEQstyle;
    private GroupBox groupEqualizerOptions;
    private MPLabel lblEQTitleDisplay;
    private MPLabel lblRestrictEQ;
    private CheckBox mpBlankDisplayWhenIdle;
    private CheckBox mpBlankDisplayWithVideo;
    private CheckBox mpDelayEQ;
    private CheckBox mpEnableDisplayAction;
    private MPComboBox mpEnableDisplayActionTime;
    private CheckBox mpEqDisplay;
    private CheckBox mpEQTitleDisplay;
    private CheckBox mpRestrictEQ;
    private CheckBox mpSmoothEQ;

    public IOWarrior_AdvancedSetupForm()
    {
      this.InitializeComponent();
      this.mpEqDisplay.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance, "EqDisplay");
      this.mpRestrictEQ.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance, "RestrictEQ");
      this.cmbEqRate.SelectedIndex = 0;
      this.cmbEqRate.DataBindings.Add("SelectedIndex", IOWarrior.AdvancedSettings.Instance, "EqRate");
      this.mpDelayEQ.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance, "DelayEQ");
      this.cmbDelayEqTime.SelectedIndex = 0;
      this.cmbDelayEqTime.DataBindings.Add("SelectedIndex", IOWarrior.AdvancedSettings.Instance, "DelayEqTime");
      this.mpSmoothEQ.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance, "SmoothEQ");
      this.mpEQTitleDisplay.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance, "EQTitleDisplay");
      this.cmbEQTitleDisplayTime.SelectedIndex = 0;
      this.cmbEQTitleDisplayTime.DataBindings.Add("SelectedIndex", IOWarrior.AdvancedSettings.Instance,
                                                  "EQTitleDisplayTime");
      this.cmbEQTitleShowTime.SelectedIndex = 0;
      this.cmbEQTitleShowTime.DataBindings.Add("SelectedIndex", IOWarrior.AdvancedSettings.Instance, "EQTitleShowTime");
      this.mpBlankDisplayWithVideo.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance,
                                                    "BlankDisplayWithVideo");
      this.mpEnableDisplayAction.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance, "EnableDisplayAction");
      this.mpEnableDisplayActionTime.SelectedIndex = 0;
      this.mpEnableDisplayActionTime.DataBindings.Add("SelectedIndex", IOWarrior.AdvancedSettings.Instance,
                                                      "EnableDisplayActionTime");
      this.mpBlankDisplayWhenIdle.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance,
                                                   "BlankDisplayWhenIdle");
      this.cmbBlankIdleTime.SelectedIndex = 0;
      this.cmbBlankIdleTime.DataBindings.Add("SelectedIndex", IOWarrior.AdvancedSettings.Instance, "BlankIdleTime");
      if (IOWarrior.AdvancedSettings.Instance.NormalEQ)
      {
        this.cbNormalEQ.Checked = true;
      }
      else if (IOWarrior.AdvancedSettings.Instance.StereoEQ)
      {
        this.cbStereoEQ.Checked = true;
      }
      else if (IOWarrior.AdvancedSettings.Instance.VUmeter)
      {
        this.cbUseVUmeter.Checked = true;
      }
      else
      {
        this.cbUseVUmeter2.Checked = true;
      }
      this.cbVUindicators.DataBindings.Add("Checked", IOWarrior.AdvancedSettings.Instance, "VUindicators");
      this.SetControlState();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Log.Debug("MatrixGX.AdvancedSetupForm.btnOK_Click() started", new object[0]);
      Log.Debug("VLSYS_Mplay.AdvancedSetupForm.btnOK_Click(): started", new object[0]);
      if (this.cbNormalEQ.Checked)
      {
        MatrixGX.AdvancedSettings.Instance.NormalEQ = true;
        MatrixGX.AdvancedSettings.Instance.StereoEQ = false;
        MatrixGX.AdvancedSettings.Instance.VUmeter = false;
        MatrixGX.AdvancedSettings.Instance.VUmeter2 = false;
      }
      else if (this.cbStereoEQ.Checked)
      {
        MatrixGX.AdvancedSettings.Instance.NormalEQ = false;
        MatrixGX.AdvancedSettings.Instance.StereoEQ = true;
        MatrixGX.AdvancedSettings.Instance.VUmeter = false;
        MatrixGX.AdvancedSettings.Instance.VUmeter2 = false;
      }
      else if (this.cbUseVUmeter.Checked)
      {
        MatrixGX.AdvancedSettings.Instance.NormalEQ = false;
        MatrixGX.AdvancedSettings.Instance.StereoEQ = false;
        MatrixGX.AdvancedSettings.Instance.VUmeter = true;
        MatrixGX.AdvancedSettings.Instance.VUmeter2 = false;
      }
      else
      {
        MatrixGX.AdvancedSettings.Instance.NormalEQ = false;
        MatrixGX.AdvancedSettings.Instance.StereoEQ = false;
        MatrixGX.AdvancedSettings.Instance.VUmeter = false;
        MatrixGX.AdvancedSettings.Instance.VUmeter2 = true;
      }
      MatrixGX.AdvancedSettings.Save();
      base.Hide();
      base.Close();
      Log.Debug("MatrixGX.AdvancedSetupForm.btnOK_Click() Completed", new object[0]);
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      IOWarrior.AdvancedSettings.SetDefaults();
      if (IOWarrior.AdvancedSettings.Instance.NormalEQ)
      {
        this.cbNormalEQ.Checked = true;
      }
      else if (IOWarrior.AdvancedSettings.Instance.StereoEQ)
      {
        this.cbStereoEQ.Checked = true;
      }
      else if (IOWarrior.AdvancedSettings.Instance.VUmeter)
      {
        this.cbUseVUmeter.Checked = true;
      }
      else
      {
        this.cbUseVUmeter2.Checked = true;
      }
      this.mpEqDisplay.Checked = IOWarrior.AdvancedSettings.Instance.EqDisplay;
      this.cbVUindicators.Checked = IOWarrior.AdvancedSettings.Instance.VUindicators;
      this.mpRestrictEQ.Checked = IOWarrior.AdvancedSettings.Instance.RestrictEQ;
      this.cmbEqRate.SelectedIndex = IOWarrior.AdvancedSettings.Instance.EqRate;
      this.mpDelayEQ.Checked = IOWarrior.AdvancedSettings.Instance.DelayEQ;
      this.cmbDelayEqTime.SelectedIndex = IOWarrior.AdvancedSettings.Instance.DelayEqTime;
      this.mpSmoothEQ.Checked = IOWarrior.AdvancedSettings.Instance.SmoothEQ;
      this.mpBlankDisplayWithVideo.Checked = IOWarrior.AdvancedSettings.Instance.BlankDisplayWithVideo;
      this.mpEnableDisplayAction.Checked = IOWarrior.AdvancedSettings.Instance.EnableDisplayAction;
      this.mpEnableDisplayActionTime.SelectedIndex = IOWarrior.AdvancedSettings.Instance.EnableDisplayActionTime;
      this.mpEQTitleDisplay.Checked = IOWarrior.AdvancedSettings.Instance.EQTitleDisplay;
      this.cmbEQTitleDisplayTime.SelectedIndex = IOWarrior.AdvancedSettings.Instance.EQTitleDisplayTime;
      this.cmbEQTitleShowTime.SelectedIndex = IOWarrior.AdvancedSettings.Instance.EQTitleShowTime;
      this.mpBlankDisplayWhenIdle.Checked = IOWarrior.AdvancedSettings.Instance.BlankDisplayWhenIdle;
      this.cmbBlankIdleTime.SelectedIndex = IOWarrior.AdvancedSettings.Instance.BlankIdleTime;
      this.SetControlState();
    }

    private void cbNormalEQ_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void cbStereoEQ_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void cbUseVUmeter_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void cbUseVUmeter2_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
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
      this.groupBox1 = new MPGroupBox();
      this.groupBox5 = new GroupBox();
      this.cbUseClockOnShutdown = new CheckBox();
      this.mpEnableDisplayActionTime = new MPComboBox();
      this.cmbBlankIdleTime = new MPComboBox();
      this.mpEnableDisplayAction = new CheckBox();
      this.mpBlankDisplayWithVideo = new CheckBox();
      this.mpBlankDisplayWhenIdle = new CheckBox();
      this.groupEqualizerOptions = new GroupBox();
      this.groupEQstyle = new GroupBox();
      this.cbUseVUmeter2 = new RadioButton();
      this.cbVUindicators = new CheckBox();
      this.cbUseVUmeter = new RadioButton();
      this.cbStereoEQ = new RadioButton();
      this.cbNormalEQ = new RadioButton();
      this.cmbDelayEqTime = new MPComboBox();
      this.lblRestrictEQ = new MPLabel();
      this.cmbEQTitleDisplayTime = new MPComboBox();
      this.cmbEQTitleShowTime = new MPComboBox();
      this.mpEQTitleDisplay = new CheckBox();
      this.mpSmoothEQ = new CheckBox();
      this.mpEqDisplay = new CheckBox();
      this.mpRestrictEQ = new CheckBox();
      this.cmbEqRate = new MPComboBox();
      this.mpDelayEQ = new CheckBox();
      this.lblEQTitleDisplay = new MPLabel();
      this.btnOK = new MPButton();
      this.btnReset = new MPButton();
      this.groupBox1.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupEqualizerOptions.SuspendLayout();
      this.groupEQstyle.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                 | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.groupBox5);
      this.groupBox1.Controls.Add(this.groupEqualizerOptions);
      this.groupBox1.FlatStyle = FlatStyle.Popup;
      this.groupBox1.Location = new Point(7, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(397, 368);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = " IO-Warrior Graphical LCD Advanced Configuration ";
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.cbUseClockOnShutdown);
      this.groupBox5.Controls.Add(this.mpEnableDisplayActionTime);
      this.groupBox5.Controls.Add(this.cmbBlankIdleTime);
      this.groupBox5.Controls.Add(this.mpEnableDisplayAction);
      this.groupBox5.Controls.Add(this.mpBlankDisplayWithVideo);
      this.groupBox5.Controls.Add(this.mpBlankDisplayWhenIdle);
      this.groupBox5.Location = new Point(6, 19);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new Size(383, 103);
      this.groupBox5.TabIndex = 105;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = " Display Control Options ";
      // 
      // cbUseClockOnShutdown
      // 
      this.cbUseClockOnShutdown.AutoSize = true;
      this.cbUseClockOnShutdown.Location = new Point(8, 81);
      this.cbUseClockOnShutdown.Name = "cbUseClockOnShutdown";
      this.cbUseClockOnShutdown.Size = new Size(204, 17);
      this.cbUseClockOnShutdown.TabIndex = 100;
      this.cbUseClockOnShutdown.Text = "Use Clock on shutdown (If supported)";
      this.cbUseClockOnShutdown.UseVisualStyleBackColor = true;
      this.cbUseClockOnShutdown.Visible = false;
      // 
      // mpEnableDisplayActionTime
      // 
      this.mpEnableDisplayActionTime.BorderColor = Color.Empty;
      this.mpEnableDisplayActionTime.DropDownStyle = ComboBoxStyle.DropDownList;
      this.mpEnableDisplayActionTime.Items.AddRange(new object[]
                                                      {
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
                                                        "20"
                                                      });
      this.mpEnableDisplayActionTime.Location = new Point(180, 36);
      this.mpEnableDisplayActionTime.Name = "mpEnableDisplayActionTime";
      this.mpEnableDisplayActionTime.Size = new Size(49, 21);
      this.mpEnableDisplayActionTime.TabIndex = 96;
      // 
      // cmbBlankIdleTime
      // 
      this.cmbBlankIdleTime.BorderColor = Color.Empty;
      this.cmbBlankIdleTime.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbBlankIdleTime.Items.AddRange(new object[]
                                             {
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
                                               "30"
                                             });
      this.cmbBlankIdleTime.Location = new Point(166, 58);
      this.cmbBlankIdleTime.Name = "cmbBlankIdleTime";
      this.cmbBlankIdleTime.Size = new Size(51, 21);
      this.cmbBlankIdleTime.TabIndex = 98;
      // 
      // mpEnableDisplayAction
      // 
      this.mpEnableDisplayAction.AutoSize = true;
      this.mpEnableDisplayAction.Location = new Point(23, 38);
      this.mpEnableDisplayAction.Name = "mpEnableDisplayAction";
      this.mpEnableDisplayAction.Size = new Size(258, 17);
      this.mpEnableDisplayAction.TabIndex = 97;
      this.mpEnableDisplayAction.Text = "Enable Display on Action for                   Seconds";
      this.mpEnableDisplayAction.UseVisualStyleBackColor = true;
      this.mpEnableDisplayAction.CheckedChanged += new EventHandler(this.mpEnableDisplayAction_CheckedChanged);
      // 
      // mpBlankDisplayWithVideo
      // 
      this.mpBlankDisplayWithVideo.AutoSize = true;
      this.mpBlankDisplayWithVideo.Location = new Point(7, 17);
      this.mpBlankDisplayWithVideo.Name = "mpBlankDisplayWithVideo";
      this.mpBlankDisplayWithVideo.Size = new Size(207, 17);
      this.mpBlankDisplayWithVideo.TabIndex = 95;
      this.mpBlankDisplayWithVideo.Text = "Turn off display during Video Playback";
      this.mpBlankDisplayWithVideo.UseVisualStyleBackColor = true;
      this.mpBlankDisplayWithVideo.CheckedChanged += new EventHandler(this.mpBlankDisplayWithVideo_CheckedChanged);
      // 
      // mpBlankDisplayWhenIdle
      // 
      this.mpBlankDisplayWhenIdle.AutoSize = true;
      this.mpBlankDisplayWhenIdle.Location = new Point(7, 60);
      this.mpBlankDisplayWhenIdle.Name = "mpBlankDisplayWhenIdle";
      this.mpBlankDisplayWhenIdle.Size = new Size(261, 17);
      this.mpBlankDisplayWhenIdle.TabIndex = 99;
      this.mpBlankDisplayWhenIdle.Text = "Turn off display when idle for                    seconds";
      this.mpBlankDisplayWhenIdle.UseVisualStyleBackColor = true;
      this.mpBlankDisplayWhenIdle.CheckedChanged += new EventHandler(this.mpBlankDisplayWhenIdle_CheckedChanged);
      // 
      // groupEqualizerOptions
      // 
      this.groupEqualizerOptions.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                             | AnchorStyles.Left)
                                                            | AnchorStyles.Right)));
      this.groupEqualizerOptions.Controls.Add(this.groupEQstyle);
      this.groupEqualizerOptions.Controls.Add(this.cmbDelayEqTime);
      this.groupEqualizerOptions.Controls.Add(this.lblRestrictEQ);
      this.groupEqualizerOptions.Controls.Add(this.cmbEQTitleDisplayTime);
      this.groupEqualizerOptions.Controls.Add(this.cmbEQTitleShowTime);
      this.groupEqualizerOptions.Controls.Add(this.mpEQTitleDisplay);
      this.groupEqualizerOptions.Controls.Add(this.mpSmoothEQ);
      this.groupEqualizerOptions.Controls.Add(this.mpEqDisplay);
      this.groupEqualizerOptions.Controls.Add(this.mpRestrictEQ);
      this.groupEqualizerOptions.Controls.Add(this.cmbEqRate);
      this.groupEqualizerOptions.Controls.Add(this.mpDelayEQ);
      this.groupEqualizerOptions.Controls.Add(this.lblEQTitleDisplay);
      this.groupEqualizerOptions.Location = new Point(6, 128);
      this.groupEqualizerOptions.Name = "groupEqualizerOptions";
      this.groupEqualizerOptions.Size = new Size(383, 233);
      this.groupEqualizerOptions.TabIndex = 104;
      this.groupEqualizerOptions.TabStop = false;
      this.groupEqualizerOptions.Text = " Equalizer Options";
      // 
      // groupEQstyle
      // 
      this.groupEQstyle.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                   | AnchorStyles.Right)));
      this.groupEQstyle.Controls.Add(this.cbUseVUmeter2);
      this.groupEQstyle.Controls.Add(this.cbVUindicators);
      this.groupEQstyle.Controls.Add(this.cbUseVUmeter);
      this.groupEQstyle.Controls.Add(this.cbStereoEQ);
      this.groupEQstyle.Controls.Add(this.cbNormalEQ);
      this.groupEQstyle.Location = new Point(39, 38);
      this.groupEQstyle.Name = "groupEQstyle";
      this.groupEQstyle.Size = new Size(300, 60);
      this.groupEQstyle.TabIndex = 118;
      this.groupEQstyle.TabStop = false;
      this.groupEQstyle.Text = " Equalizer Style ";
      // 
      // cbUseVUmeter2
      // 
      this.cbUseVUmeter2.AutoSize = true;
      this.cbUseVUmeter2.Location = new Point(211, 17);
      this.cbUseVUmeter2.Name = "cbUseVUmeter2";
      this.cbUseVUmeter2.Size = new Size(79, 17);
      this.cbUseVUmeter2.TabIndex = 122;
      this.cbUseVUmeter2.Text = "VU Meter 2";
      this.cbUseVUmeter2.UseVisualStyleBackColor = true;
      this.cbUseVUmeter2.CheckedChanged += new EventHandler(this.cbUseVUmeter2_CheckedChanged);
      // 
      // cbVUindicators
      // 
      this.cbVUindicators.AutoSize = true;
      this.cbVUindicators.Location = new Point(9, 39);
      this.cbVUindicators.Name = "cbVUindicators";
      this.cbVUindicators.Size = new Size(213, 17);
      this.cbVUindicators.TabIndex = 121;
      this.cbVUindicators.Text = "Show Channel indicators for VU Display";
      this.cbVUindicators.UseVisualStyleBackColor = true;
      // 
      // cbUseVUmeter
      // 
      this.cbUseVUmeter.AutoSize = true;
      this.cbUseVUmeter.Location = new Point(135, 17);
      this.cbUseVUmeter.Name = "cbUseVUmeter";
      this.cbUseVUmeter.Size = new Size(70, 17);
      this.cbUseVUmeter.TabIndex = 2;
      this.cbUseVUmeter.Text = "VU Meter";
      this.cbUseVUmeter.UseVisualStyleBackColor = true;
      this.cbUseVUmeter.CheckedChanged += new EventHandler(this.cbUseVUmeter_CheckedChanged);
      // 
      // cbStereoEQ
      // 
      this.cbStereoEQ.AutoSize = true;
      this.cbStereoEQ.Location = new Point(77, 17);
      this.cbStereoEQ.Name = "cbStereoEQ";
      this.cbStereoEQ.Size = new Size(56, 17);
      this.cbStereoEQ.TabIndex = 1;
      this.cbStereoEQ.Text = "Stereo";
      this.cbStereoEQ.UseVisualStyleBackColor = true;
      this.cbStereoEQ.CheckedChanged += new EventHandler(this.cbStereoEQ_CheckedChanged);
      // 
      // cbNormalEQ
      // 
      this.cbNormalEQ.AutoSize = true;
      this.cbNormalEQ.Checked = true;
      this.cbNormalEQ.Location = new Point(13, 17);
      this.cbNormalEQ.Name = "cbNormalEQ";
      this.cbNormalEQ.Size = new Size(58, 17);
      this.cbNormalEQ.TabIndex = 0;
      this.cbNormalEQ.TabStop = true;
      this.cbNormalEQ.Text = "Normal";
      this.cbNormalEQ.UseVisualStyleBackColor = true;
      this.cbNormalEQ.CheckedChanged += new EventHandler(this.cbNormalEQ_CheckedChanged);
      // 
      // cmbDelayEqTime
      // 
      this.cmbDelayEqTime.BorderColor = Color.Empty;
      this.cmbDelayEqTime.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbDelayEqTime.Items.AddRange(new object[]
                                           {
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
                                             "30"
                                           });
      this.cmbDelayEqTime.Location = new Point(160, 143);
      this.cmbDelayEqTime.Name = "cmbDelayEqTime";
      this.cmbDelayEqTime.Size = new Size(52, 21);
      this.cmbDelayEqTime.TabIndex = 104;
      // 
      // lblRestrictEQ
      // 
      this.lblRestrictEQ.Anchor = ((AnchorStyles) ((AnchorStyles.Top | AnchorStyles.Right)));
      this.lblRestrictEQ.Location = new Point(102, 124);
      this.lblRestrictEQ.Name = "lblRestrictEQ";
      this.lblRestrictEQ.Size = new Size(116, 17);
      this.lblRestrictEQ.TabIndex = 115;
      this.lblRestrictEQ.Text = "updates per Seconds";
      this.lblRestrictEQ.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // cmbEQTitleDisplayTime
      // 
      this.cmbEQTitleDisplayTime.BorderColor = Color.Empty;
      this.cmbEQTitleDisplayTime.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbEQTitleDisplayTime.Items.AddRange(new object[]
                                                  {
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
                                                    "30"
                                                  });
      this.cmbEQTitleDisplayTime.Location = new Point(165, 207);
      this.cmbEQTitleDisplayTime.Name = "cmbEQTitleDisplayTime";
      this.cmbEQTitleDisplayTime.Size = new Size(52, 21);
      this.cmbEQTitleDisplayTime.TabIndex = 110;
      // 
      // cmbEQTitleShowTime
      // 
      this.cmbEQTitleShowTime.BorderColor = Color.Empty;
      this.cmbEQTitleShowTime.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbEQTitleShowTime.Items.AddRange(new object[]
                                               {
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
                                                 "30"
                                               });
      this.cmbEQTitleShowTime.Location = new Point(32, 207);
      this.cmbEQTitleShowTime.Name = "cmbEQTitleShowTime";
      this.cmbEQTitleShowTime.Size = new Size(52, 21);
      this.cmbEQTitleShowTime.TabIndex = 113;
      // 
      // mpEQTitleDisplay
      // 
      this.mpEQTitleDisplay.AutoSize = true;
      this.mpEQTitleDisplay.Location = new Point(21, 187);
      this.mpEQTitleDisplay.Name = "mpEQTitleDisplay";
      this.mpEQTitleDisplay.Size = new Size(120, 17);
      this.mpEQTitleDisplay.TabIndex = 112;
      this.mpEQTitleDisplay.Text = "Show Track Info for";
      this.mpEQTitleDisplay.UseVisualStyleBackColor = true;
      this.mpEQTitleDisplay.CheckedChanged += new EventHandler(this.mpEQTitleDisplay_CheckedChanged);
      // 
      // mpSmoothEQ
      // 
      this.mpSmoothEQ.AutoSize = true;
      this.mpSmoothEQ.Location = new Point(21, 166);
      this.mpSmoothEQ.Name = "mpSmoothEQ";
      this.mpSmoothEQ.Size = new Size(224, 17);
      this.mpSmoothEQ.TabIndex = 109;
      this.mpSmoothEQ.Text = "Use Equalizer Smoothing (Delayed decay)";
      this.mpSmoothEQ.UseVisualStyleBackColor = true;
      // 
      // mpEqDisplay
      // 
      this.mpEqDisplay.AutoSize = true;
      this.mpEqDisplay.Location = new Point(5, 21);
      this.mpEqDisplay.Name = "mpEqDisplay";
      this.mpEqDisplay.Size = new Size(126, 17);
      this.mpEqDisplay.TabIndex = 106;
      this.mpEqDisplay.Text = "Use Equalizer display";
      this.mpEqDisplay.UseVisualStyleBackColor = true;
      this.mpEqDisplay.CheckedChanged += new EventHandler(this.mpEqDisplay_CheckedChanged);
      // 
      // mpRestrictEQ
      // 
      this.mpRestrictEQ.AutoSize = true;
      this.mpRestrictEQ.Location = new Point(21, 103);
      this.mpRestrictEQ.Name = "mpRestrictEQ";
      this.mpRestrictEQ.Size = new Size(185, 17);
      this.mpRestrictEQ.TabIndex = 107;
      this.mpRestrictEQ.Text = "Limit Equalizer display update rate";
      this.mpRestrictEQ.UseVisualStyleBackColor = true;
      this.mpRestrictEQ.CheckedChanged += new EventHandler(this.mpRestrictEQ_CheckedChanged);
      // 
      // cmbEqRate
      // 
      this.cmbEqRate.Anchor = ((AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.cmbEqRate.BorderColor = Color.Empty;
      this.cmbEqRate.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbEqRate.Items.AddRange(new object[]
                                      {
                                        "MAX",
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
                                        "30",
                                        "31",
                                        "32",
                                        "33",
                                        "34",
                                        "35",
                                        "36",
                                        "37",
                                        "38",
                                        "39",
                                        "40",
                                        "41",
                                        "42",
                                        "43",
                                        "44",
                                        "45",
                                        "46",
                                        "47",
                                        "48",
                                        "49",
                                        "50",
                                        "51",
                                        "52",
                                        "53",
                                        "54",
                                        "55",
                                        "56",
                                        "57",
                                        "58",
                                        "59",
                                        "60"
                                      });
      this.cmbEqRate.Location = new Point(32, 122);
      this.cmbEqRate.Name = "cmbEqRate";
      this.cmbEqRate.Size = new Size(69, 21);
      this.cmbEqRate.TabIndex = 103;
      // 
      // mpDelayEQ
      // 
      this.mpDelayEQ.AutoSize = true;
      this.mpDelayEQ.Location = new Point(21, 144);
      this.mpDelayEQ.Name = "mpDelayEQ";
      this.mpDelayEQ.Size = new Size(246, 17);
      this.mpDelayEQ.TabIndex = 108;
      this.mpDelayEQ.Text = "Delay Equalizer Start by                      Seconds";
      this.mpDelayEQ.UseVisualStyleBackColor = true;
      this.mpDelayEQ.CheckedChanged += new EventHandler(this.mpDelayEQ_CheckedChanged);
      // 
      // lblEQTitleDisplay
      // 
      this.lblEQTitleDisplay.Location = new Point(86, 208);
      this.lblEQTitleDisplay.Name = "lblEQTitleDisplay";
      this.lblEQTitleDisplay.Size = new Size(201, 17);
      this.lblEQTitleDisplay.TabIndex = 114;
      this.lblEQTitleDisplay.Text = "Seconds every                     Seconds";
      this.lblEQTitleDisplay.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnOK.Location = new Point(316, 380);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new Size(88, 23);
      this.btnOK.TabIndex = 12;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new EventHandler(this.btnOK_Click);
      // 
      // btnReset
      // 
      this.btnReset.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnReset.Location = new Point(222, 380);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new Size(88, 23);
      this.btnReset.TabIndex = 13;
      this.btnReset.Text = "&RESET";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new EventHandler(this.btnReset_Click);
      // 
      // IOWarrior_AdvancedSetupForm
      // 
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.ClientSize = new Size(411, 409);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.groupBox1);
      this.Name = "IOWarrior_AdvancedSetupForm";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup - Advanced Settings";
      this.groupBox1.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.groupEqualizerOptions.ResumeLayout(false);
      this.groupEqualizerOptions.PerformLayout();
      this.groupEQstyle.ResumeLayout(false);
      this.groupEQstyle.PerformLayout();
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

    private void mpDelayEQ_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void mpEnableDisplayAction_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void mpEqDisplay_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void mpEQTitleDisplay_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void mpRestrictEQ_CheckedChanged(object sender, EventArgs e)
    {
      this.SetControlState();
    }

    private void SetControlState()
    {
      if (this.mpEqDisplay.Checked)
      {
        this.groupEQstyle.Enabled = true;
        if (this.cbUseVUmeter.Checked || this.cbUseVUmeter2.Checked)
        {
          this.cbVUindicators.Enabled = true;
        }
        else
        {
          this.cbVUindicators.Enabled = false;
        }
        this.mpRestrictEQ.Enabled = true;
        if (this.mpRestrictEQ.Checked)
        {
          this.lblRestrictEQ.Enabled = true;
          this.cmbEqRate.Enabled = true;
        }
        else
        {
          this.lblRestrictEQ.Enabled = false;
          this.cmbEqRate.Enabled = false;
        }
        this.mpDelayEQ.Enabled = true;
        if (this.mpDelayEQ.Checked)
        {
          this.cmbDelayEqTime.Enabled = true;
        }
        else
        {
          this.cmbDelayEqTime.Enabled = false;
        }
        this.mpSmoothEQ.Enabled = true;
        this.mpEQTitleDisplay.Enabled = true;
        if (this.mpEQTitleDisplay.Checked)
        {
          this.lblEQTitleDisplay.Enabled = true;
          this.cmbEQTitleDisplayTime.Enabled = true;
          this.cmbEQTitleShowTime.Enabled = true;
        }
        else
        {
          this.lblEQTitleDisplay.Enabled = false;
          this.cmbEQTitleDisplayTime.Enabled = false;
          this.cmbEQTitleShowTime.Enabled = false;
        }
      }
      else
      {
        this.groupEQstyle.Enabled = false;
        this.mpRestrictEQ.Enabled = false;
        this.lblRestrictEQ.Enabled = false;
        this.cmbEqRate.Enabled = false;
        this.mpDelayEQ.Enabled = false;
        this.cmbDelayEqTime.Enabled = false;
        this.mpSmoothEQ.Enabled = false;
        this.mpEQTitleDisplay.Enabled = false;
        this.lblEQTitleDisplay.Enabled = false;
        this.cmbEQTitleDisplayTime.Enabled = false;
        this.cmbEQTitleShowTime.Enabled = false;
      }
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