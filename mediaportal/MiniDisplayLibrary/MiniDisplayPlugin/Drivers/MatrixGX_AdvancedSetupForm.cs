#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class MatrixGX_AdvancedSetupForm : MPConfigForm
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
    private MPGroupBox groupBox2;
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
    private MPLabel mpLabel1;
    private MPLabel mpLabel2;
    private MPLabel mpLabel3;
    private MPCheckBox mpProgressBar;
    private CheckBox mpRestrictEQ;
    private CheckBox mpSmoothEQ;
    private MPCheckBox mpUseDiskIconForAllMedia;
    private MPCheckBox mpUseIcons;
    private MPCheckBox mpUseInvertedDisplay;
    private MPCheckBox mpVolumeDisplay;
    private TrackBar tbBacklightBLUE;
    private TrackBar tbBacklightGREEN;
    private TrackBar tbBacklightRED;

    public MatrixGX_AdvancedSetupForm()
    {
      this.InitializeComponent();
      this.mpVolumeDisplay.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "VolumeDisplay");
      this.mpProgressBar.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "ProgressDisplay");
      this.mpUseInvertedDisplay.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "UseInvertedDisplay");
      this.mpUseIcons.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "UseIcons");
      this.mpUseDiskIconForAllMedia.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance,
                                                     "UseDiskIconForAllMedia");
      this.tbBacklightRED.DataBindings.Add("Value", MatrixGX.AdvancedSettings.Instance, "BacklightRED");
      this.tbBacklightGREEN.DataBindings.Add("Value", MatrixGX.AdvancedSettings.Instance, "BacklightGREEN");
      this.tbBacklightBLUE.DataBindings.Add("Value", MatrixGX.AdvancedSettings.Instance, "BacklightBLUE");
      this.mpEqDisplay.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "EqDisplay");
      this.mpRestrictEQ.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "RestrictEQ");
      this.cmbEqRate.SelectedIndex = 0;
      this.cmbEqRate.DataBindings.Add("SelectedIndex", MatrixGX.AdvancedSettings.Instance, "EqRate");
      this.mpDelayEQ.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "DelayEQ");
      this.cmbDelayEqTime.SelectedIndex = 0;
      this.cmbDelayEqTime.DataBindings.Add("SelectedIndex", MatrixGX.AdvancedSettings.Instance, "DelayEqTime");
      this.mpSmoothEQ.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "SmoothEQ");
      this.mpEQTitleDisplay.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "EQTitleDisplay");
      this.cmbEQTitleDisplayTime.SelectedIndex = 0;
      this.cmbEQTitleDisplayTime.DataBindings.Add("SelectedIndex", MatrixGX.AdvancedSettings.Instance,
                                                  "EQTitleDisplayTime");
      this.cmbEQTitleShowTime.SelectedIndex = 0;
      this.cmbEQTitleShowTime.DataBindings.Add("SelectedIndex", MatrixGX.AdvancedSettings.Instance, "EQTitleShowTime");
      this.mpBlankDisplayWithVideo.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance,
                                                    "BlankDisplayWithVideo");
      this.mpEnableDisplayAction.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "EnableDisplayAction");
      this.mpEnableDisplayActionTime.SelectedIndex = 0;
      this.mpEnableDisplayActionTime.DataBindings.Add("SelectedIndex", MatrixGX.AdvancedSettings.Instance,
                                                      "EnableDisplayActionTime");
      this.mpBlankDisplayWhenIdle.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "BlankDisplayWhenIdle");
      this.cmbBlankIdleTime.SelectedIndex = 0;
      this.cmbBlankIdleTime.DataBindings.Add("SelectedIndex", MatrixGX.AdvancedSettings.Instance, "BlankIdleTime");
      if (MatrixGX.AdvancedSettings.Instance.NormalEQ)
      {
        this.cbNormalEQ.Checked = true;
      }
      else if (MatrixGX.AdvancedSettings.Instance.StereoEQ)
      {
        this.cbStereoEQ.Checked = true;
      }
      else if (MatrixGX.AdvancedSettings.Instance.VUmeter)
      {
        this.cbUseVUmeter.Checked = true;
      }
      else
      {
        this.cbUseVUmeter2.Checked = true;
      }
      this.cbVUindicators.DataBindings.Add("Checked", MatrixGX.AdvancedSettings.Instance, "VUindicators");
      this.SetControlState();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Log.Debug("MatrixGX.AdvancedSetupForm.btnOK_Click(): started");
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
      Log.Debug("MatrixGX.AdvancedSetupForm.btnOK_Click() Completed");
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      MatrixGX.AdvancedSettings.SetDefaults();
      this.mpVolumeDisplay.Checked = MatrixGX.AdvancedSettings.Instance.VolumeDisplay;
      this.mpProgressBar.Checked = MatrixGX.AdvancedSettings.Instance.ProgressDisplay;
      this.mpUseInvertedDisplay.Checked = MatrixGX.AdvancedSettings.Instance.UseInvertedDisplay;
      this.mpUseIcons.Checked = MatrixGX.AdvancedSettings.Instance.UseIcons;
      this.mpUseDiskIconForAllMedia.Checked = MatrixGX.AdvancedSettings.Instance.UseDiskIconForAllMedia;
      this.tbBacklightRED.Value = MatrixGX.AdvancedSettings.Instance.BacklightRED;
      this.tbBacklightGREEN.Value = MatrixGX.AdvancedSettings.Instance.BacklightGREEN;
      this.tbBacklightBLUE.Value = MatrixGX.AdvancedSettings.Instance.BacklightBLUE;
      if (MatrixGX.AdvancedSettings.Instance.NormalEQ)
      {
        this.cbNormalEQ.Checked = true;
      }
      else if (MatrixGX.AdvancedSettings.Instance.StereoEQ)
      {
        this.cbStereoEQ.Checked = true;
      }
      else if (MatrixGX.AdvancedSettings.Instance.VUmeter)
      {
        this.cbUseVUmeter.Checked = true;
      }
      else
      {
        this.cbUseVUmeter2.Checked = true;
      }
      this.mpEqDisplay.Checked = MatrixGX.AdvancedSettings.Instance.EqDisplay;
      this.cbVUindicators.Checked = MatrixGX.AdvancedSettings.Instance.VUindicators;
      this.mpRestrictEQ.Checked = MatrixGX.AdvancedSettings.Instance.RestrictEQ;
      this.cmbEqRate.SelectedIndex = MatrixGX.AdvancedSettings.Instance.EqRate;
      this.mpDelayEQ.Checked = MatrixGX.AdvancedSettings.Instance.DelayEQ;
      this.cmbDelayEqTime.SelectedIndex = MatrixGX.AdvancedSettings.Instance.DelayEqTime;
      this.mpSmoothEQ.Checked = MatrixGX.AdvancedSettings.Instance.SmoothEQ;
      this.mpBlankDisplayWithVideo.Checked = MatrixGX.AdvancedSettings.Instance.BlankDisplayWithVideo;
      this.mpEnableDisplayAction.Checked = MatrixGX.AdvancedSettings.Instance.EnableDisplayAction;
      this.mpEnableDisplayActionTime.SelectedIndex = MatrixGX.AdvancedSettings.Instance.EnableDisplayActionTime;
      this.mpEQTitleDisplay.Checked = MatrixGX.AdvancedSettings.Instance.EQTitleDisplay;
      this.cmbEQTitleDisplayTime.SelectedIndex = MatrixGX.AdvancedSettings.Instance.EQTitleDisplayTime;
      this.cmbEQTitleShowTime.SelectedIndex = MatrixGX.AdvancedSettings.Instance.EQTitleShowTime;
      this.mpBlankDisplayWhenIdle.Checked = MatrixGX.AdvancedSettings.Instance.BlankDisplayWhenIdle;
      this.cmbBlankIdleTime.SelectedIndex = MatrixGX.AdvancedSettings.Instance.BlankIdleTime;
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
      this.groupBox2 = new MPGroupBox();
      this.mpLabel1 = new MPLabel();
      this.tbBacklightBLUE = new TrackBar();
      this.mpLabel2 = new MPLabel();
      this.tbBacklightGREEN = new TrackBar();
      this.mpLabel3 = new MPLabel();
      this.tbBacklightRED = new TrackBar();
      this.mpVolumeDisplay = new MPCheckBox();
      this.mpProgressBar = new MPCheckBox();
      this.mpUseInvertedDisplay = new MPCheckBox();
      this.mpUseIcons = new MPCheckBox();
      this.mpUseDiskIconForAllMedia = new MPCheckBox();
      this.btnOK = new MPButton();
      this.btnReset = new MPButton();
      this.groupBox1.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupEqualizerOptions.SuspendLayout();
      this.groupEQstyle.SuspendLayout();
      this.groupBox2.SuspendLayout();
      ((ISupportInitialize)(this.tbBacklightBLUE)).BeginInit();
      ((ISupportInitialize)(this.tbBacklightGREEN)).BeginInit();
      ((ISupportInitialize)(this.tbBacklightRED)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                | AnchorStyles.Left)
                                               | AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.groupBox5);
      this.groupBox1.Controls.Add(this.groupEqualizerOptions);
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Controls.Add(this.mpVolumeDisplay);
      this.groupBox1.Controls.Add(this.mpProgressBar);
      this.groupBox1.Controls.Add(this.mpUseInvertedDisplay);
      this.groupBox1.Controls.Add(this.mpUseIcons);
      this.groupBox1.Controls.Add(this.mpUseDiskIconForAllMedia);
      this.groupBox1.FlatStyle = FlatStyle.Popup;
      this.groupBox1.Location = new Point(7, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(397, 503);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = " Matrix Orbital GX Advanced Configuration ";
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                               | AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.cbUseClockOnShutdown);
      this.groupBox5.Controls.Add(this.mpEnableDisplayActionTime);
      this.groupBox5.Controls.Add(this.cmbBlankIdleTime);
      this.groupBox5.Controls.Add(this.mpEnableDisplayAction);
      this.groupBox5.Controls.Add(this.mpBlankDisplayWithVideo);
      this.groupBox5.Controls.Add(this.mpBlankDisplayWhenIdle);
      this.groupBox5.Location = new Point(6, 151);
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
      this.groupEqualizerOptions.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
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
      this.groupEqualizerOptions.Location = new Point(6, 255);
      this.groupEqualizerOptions.Name = "groupEqualizerOptions";
      this.groupEqualizerOptions.Size = new Size(383, 233);
      this.groupEqualizerOptions.TabIndex = 104;
      this.groupEqualizerOptions.TabStop = false;
      this.groupEqualizerOptions.Text = " Equalizer Options";
      // 
      // groupEQstyle
      // 
      this.groupEQstyle.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
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
      this.lblRestrictEQ.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
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
      this.cmbEqRate.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
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
      // groupBox2
      // 
      this.groupBox2.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                               | AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.mpLabel1);
      this.groupBox2.Controls.Add(this.tbBacklightBLUE);
      this.groupBox2.Controls.Add(this.mpLabel2);
      this.groupBox2.Controls.Add(this.tbBacklightGREEN);
      this.groupBox2.Controls.Add(this.mpLabel3);
      this.groupBox2.Controls.Add(this.tbBacklightRED);
      this.groupBox2.FlatStyle = FlatStyle.Popup;
      this.groupBox2.Location = new Point(168, 25);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(221, 125);
      this.groupBox2.TabIndex = 75;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = " Backlight Color ";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new Point(5, 20);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new Size(40, 23);
      this.mpLabel1.TabIndex = 51;
      this.mpLabel1.Text = "Red";
      this.mpLabel1.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // tbBacklightBLUE
      // 
      this.tbBacklightBLUE.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                     | AnchorStyles.Right)));
      this.tbBacklightBLUE.Location = new Point(50, 70);
      this.tbBacklightBLUE.Maximum = 255;
      this.tbBacklightBLUE.Name = "tbBacklightBLUE";
      this.tbBacklightBLUE.Size = new Size(160, 45);
      this.tbBacklightBLUE.TabIndex = 11;
      this.tbBacklightBLUE.TickFrequency = 8;
      this.tbBacklightBLUE.TickStyle = TickStyle.None;
      this.tbBacklightBLUE.Value = 255;
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new Point(5, 45);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new Size(40, 23);
      this.mpLabel2.TabIndex = 51;
      this.mpLabel2.Text = "Green";
      this.mpLabel2.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // tbBacklightGREEN
      // 
      this.tbBacklightGREEN.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                      | AnchorStyles.Right)));
      this.tbBacklightGREEN.AutoSize = false;
      this.tbBacklightGREEN.Location = new Point(50, 45);
      this.tbBacklightGREEN.Maximum = 255;
      this.tbBacklightGREEN.Name = "tbBacklightGREEN";
      this.tbBacklightGREEN.Size = new Size(160, 23);
      this.tbBacklightGREEN.TabIndex = 10;
      this.tbBacklightGREEN.TickFrequency = 8;
      this.tbBacklightGREEN.TickStyle = TickStyle.None;
      this.tbBacklightGREEN.Value = 255;
      // 
      // mpLabel3
      // 
      this.mpLabel3.Location = new Point(5, 70);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new Size(40, 23);
      this.mpLabel3.TabIndex = 51;
      this.mpLabel3.Text = "Blue";
      this.mpLabel3.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // tbBacklightRED
      // 
      this.tbBacklightRED.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                    | AnchorStyles.Right)));
      this.tbBacklightRED.AutoSize = false;
      this.tbBacklightRED.Location = new Point(50, 20);
      this.tbBacklightRED.Maximum = 255;
      this.tbBacklightRED.Name = "tbBacklightRED";
      this.tbBacklightRED.Size = new Size(160, 23);
      this.tbBacklightRED.TabIndex = 73;
      this.tbBacklightRED.TickFrequency = 9;
      this.tbBacklightRED.TickStyle = TickStyle.None;
      this.tbBacklightRED.Value = 255;
      // 
      // mpVolumeDisplay
      // 
      this.mpVolumeDisplay.AutoSize = true;
      this.mpVolumeDisplay.FlatStyle = FlatStyle.Popup;
      this.mpVolumeDisplay.Location = new Point(14, 25);
      this.mpVolumeDisplay.Name = "mpVolumeDisplay";
      this.mpVolumeDisplay.Size = new Size(124, 17);
      this.mpVolumeDisplay.TabIndex = 6;
      this.mpVolumeDisplay.Text = "Show Volume display";
      this.mpVolumeDisplay.UseVisualStyleBackColor = true;
      // 
      // mpProgressBar
      // 
      this.mpProgressBar.AutoSize = true;
      this.mpProgressBar.FlatStyle = FlatStyle.Popup;
      this.mpProgressBar.Location = new Point(14, 48);
      this.mpProgressBar.Name = "mpProgressBar";
      this.mpProgressBar.Size = new Size(132, 17);
      this.mpProgressBar.TabIndex = 7;
      this.mpProgressBar.Text = "Show Progress Display";
      this.mpProgressBar.UseVisualStyleBackColor = true;
      // 
      // mpUseInvertedDisplay
      // 
      this.mpUseInvertedDisplay.AutoSize = true;
      this.mpUseInvertedDisplay.FlatStyle = FlatStyle.Popup;
      this.mpUseInvertedDisplay.Location = new Point(14, 71);
      this.mpUseInvertedDisplay.Name = "mpUseInvertedDisplay";
      this.mpUseInvertedDisplay.Size = new Size(122, 17);
      this.mpUseInvertedDisplay.TabIndex = 8;
      this.mpUseInvertedDisplay.Text = "Use Inverted Display";
      this.mpUseInvertedDisplay.UseVisualStyleBackColor = true;
      // 
      // mpUseIcons
      // 
      this.mpUseIcons.AutoSize = true;
      this.mpUseIcons.FlatStyle = FlatStyle.Popup;
      this.mpUseIcons.Location = new Point(14, 94);
      this.mpUseIcons.Name = "mpUseIcons";
      this.mpUseIcons.Size = new Size(72, 17);
      this.mpUseIcons.TabIndex = 8;
      this.mpUseIcons.Text = "Use Icons";
      this.mpUseIcons.UseVisualStyleBackColor = true;
      // 
      // mpUseDiskIconForAllMedia
      // 
      this.mpUseDiskIconForAllMedia.AutoSize = true;
      this.mpUseDiskIconForAllMedia.FlatStyle = FlatStyle.Popup;
      this.mpUseDiskIconForAllMedia.Location = new Point(14, 117);
      this.mpUseDiskIconForAllMedia.Name = "mpUseDiskIconForAllMedia";
      this.mpUseDiskIconForAllMedia.Size = new Size(155, 17);
      this.mpUseDiskIconForAllMedia.TabIndex = 8;
      this.mpUseDiskIconForAllMedia.Text = "Use Disk Icon For All Media";
      this.mpUseDiskIconForAllMedia.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnOK.Location = new Point(316, 515);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new Size(88, 23);
      this.btnOK.TabIndex = 12;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new EventHandler(this.btnOK_Click);
      // 
      // btnReset
      // 
      this.btnReset.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.btnReset.Location = new Point(222, 515);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new Size(88, 23);
      this.btnReset.TabIndex = 13;
      this.btnReset.Text = "&RESET";
      this.btnReset.UseVisualStyleBackColor = true;
      this.btnReset.Click += new EventHandler(this.btnReset_Click);
      // 
      // MatrixGX_AdvancedSetupForm
      // 
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.ClientSize = new Size(411, 544);
      this.Controls.Add(this.btnReset);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.groupBox1);
      this.Name = "MatrixGX_AdvancedSetupForm";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "MiniDisplay - Setup - Advanced Settings";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.groupEqualizerOptions.ResumeLayout(false);
      this.groupEqualizerOptions.PerformLayout();
      this.groupEQstyle.ResumeLayout(false);
      this.groupEQstyle.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      ((ISupportInitialize)(this.tbBacklightBLUE)).EndInit();
      ((ISupportInitialize)(this.tbBacklightGREEN)).EndInit();
      ((ISupportInitialize)(this.tbBacklightRED)).EndInit();
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