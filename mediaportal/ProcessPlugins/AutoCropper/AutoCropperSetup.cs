#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace ProcessPlugins.AutoCropper
{
  public class AutoCropperConfig : MPConfigForm
  {
    private IContainer components = null;
    private MPCheckBox enableAutoCropper;
    private GroupBox Parameters;
    private MPLabel topScanStartLabel;
    private MPLabel bottomScanEndLabel;
    private MPLabel bottomScanStartLabel;
    private MPLabel topScanEndLabel;
    private MPGroupBox manualModeGroup;
    private MPCheckBox enableManualMode;
    private MPGroupBox automodeGroup;
    private MPCheckBox enableMoveSubs;
    private MPCheckBox enableAutoMode;
    private MPLabel autoSampleIntervalLabel;
    private MPLabel manualSampleLengthLabel;
    private MPNumericUpDown bottomScanEndInput;
    private MPNumericUpDown topScanEndInput;
    private MPNumericUpDown bottomScanStartInput;
    private MPNumericUpDown topScanStartInput;
    private MPNumericUpDown manualSampleLengthInput;
    private MPButton okButton;
    private MPButton defaultsButton;
    private MPLabel minSubtitleHeightLabel;
    private MPNumericUpDown minSubtitleHeightInput;
    private MPNumericUpDown bottomMemLength;
    private MPNumericUpDown topMemLength;
    private MPLabel labelBottomMemLength;
    private MPLabel labelTopMemLength;
    private MPRadioButton rbDefaultManual;
    private MPRadioButton rbDefaultAutomatic;
    private MPGroupBox groupDefaultMode;
    private MPCheckBox cbVerboseLog;
    private MPCheckBox cbUseForVideos;
    private MPLabel labelWarning;
    private LinkLabel linkHelp;
    private MPButton cancelButton;
    private MPLabel maxBrightnessTresholdLabel;
    private MPNumericUpDown maxBrightnessTresholdInput;
    private MPLabel minBrightnessTresholdLabel;
    private MPNumericUpDown minBrightnessTresholdInput;
    private MPNumericUpDown sampleIntervalInput;

    public AutoCropperConfig(string name)
      : base()
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      LoadSettings();
      OnChange(null, null);
    }

    /*public override void OnSectionActivated()
     {
       base.OnSectionActivated();
       if (_init == false)
       {
         LoadSettings();
         OnChange(null, null);
         _init = true;
       }
     }*/

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.enableAutoCropper = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.Parameters = new System.Windows.Forms.GroupBox();
      this.maxBrightnessTresholdLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.maxBrightnessTresholdInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.minBrightnessTresholdLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.minBrightnessTresholdInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.cbUseForVideos = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbVerboseLog = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.minSubtitleHeightLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.minSubtitleHeightInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.bottomScanEndInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.topScanEndInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.bottomScanStartInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.topScanStartInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.bottomScanEndLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bottomScanStartLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.topScanEndLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.topScanStartLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.manualModeGroup = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.manualSampleLengthInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.manualSampleLengthLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.enableManualMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.automodeGroup = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.bottomMemLength = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.topMemLength = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.labelBottomMemLength = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelTopMemLength = new MediaPortal.UserInterface.Controls.MPLabel();
      this.sampleIntervalInput = new MediaPortal.UserInterface.Controls.MPNumericUpDown();
      this.autoSampleIntervalLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.enableMoveSubs = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.enableAutoMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.defaultsButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.rbDefaultManual = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.rbDefaultAutomatic = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupDefaultMode = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelWarning = new MediaPortal.UserInterface.Controls.MPLabel();
      this.linkHelp = new System.Windows.Forms.LinkLabel();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.Parameters.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.maxBrightnessTresholdInput)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.minBrightnessTresholdInput)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.minSubtitleHeightInput)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.bottomScanEndInput)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.topScanEndInput)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.bottomScanStartInput)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.topScanStartInput)).BeginInit();
      this.manualModeGroup.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.manualSampleLengthInput)).BeginInit();
      this.automodeGroup.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.bottomMemLength)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.topMemLength)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.sampleIntervalInput)).BeginInit();
      this.groupDefaultMode.SuspendLayout();
      this.SuspendLayout();
      // 
      // enableAutoCropper
      // 
      this.enableAutoCropper.AutoSize = true;
      this.enableAutoCropper.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableAutoCropper.Location = new System.Drawing.Point(9, 10);
      this.enableAutoCropper.Name = "enableAutoCropper";
      this.enableAutoCropper.Size = new System.Drawing.Size(119, 17);
      this.enableAutoCropper.TabIndex = 0;
      this.enableAutoCropper.Text = "Enable AutoCropper";
      this.enableAutoCropper.UseVisualStyleBackColor = true;
      this.enableAutoCropper.Visible = false;
      this.enableAutoCropper.CheckedChanged += new System.EventHandler(this.OnChange);
      // 
      // Parameters
      // 
      this.Parameters.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.Parameters.Controls.Add(this.maxBrightnessTresholdLabel);
      this.Parameters.Controls.Add(this.maxBrightnessTresholdInput);
      this.Parameters.Controls.Add(this.minBrightnessTresholdLabel);
      this.Parameters.Controls.Add(this.minBrightnessTresholdInput);
      this.Parameters.Controls.Add(this.cbUseForVideos);
      this.Parameters.Controls.Add(this.cbVerboseLog);
      this.Parameters.Controls.Add(this.minSubtitleHeightLabel);
      this.Parameters.Controls.Add(this.minSubtitleHeightInput);
      this.Parameters.Controls.Add(this.bottomScanEndInput);
      this.Parameters.Controls.Add(this.topScanEndInput);
      this.Parameters.Controls.Add(this.bottomScanStartInput);
      this.Parameters.Controls.Add(this.topScanStartInput);
      this.Parameters.Controls.Add(this.bottomScanEndLabel);
      this.Parameters.Controls.Add(this.bottomScanStartLabel);
      this.Parameters.Controls.Add(this.topScanEndLabel);
      this.Parameters.Controls.Add(this.topScanStartLabel);
      this.Parameters.Location = new System.Drawing.Point(9, 192);
      this.Parameters.Name = "Parameters";
      this.Parameters.Size = new System.Drawing.Size(443, 235);
      this.Parameters.TabIndex = 1;
      this.Parameters.TabStop = false;
      this.Parameters.Text = "Parameters";
      // 
      // maxBrightnessTresholdLabel
      // 
      this.maxBrightnessTresholdLabel.AutoSize = true;
      this.maxBrightnessTresholdLabel.Location = new System.Drawing.Point(13, 139);
      this.maxBrightnessTresholdLabel.Name = "maxBrightnessTresholdLabel";
      this.maxBrightnessTresholdLabel.Size = new System.Drawing.Size(123, 13);
      this.maxBrightnessTresholdLabel.TabIndex = 23;
      this.maxBrightnessTresholdLabel.Text = "Noise sensitivity (0=high)";
      // 
      // maxBrightnessTresholdInput
      // 
      this.maxBrightnessTresholdInput.Location = new System.Drawing.Point(154, 137);
      this.maxBrightnessTresholdInput.Maximum = new decimal(new int[]
                                                              {
                                                                255,
                                                                0,
                                                                0,
                                                                0
                                                              });
      this.maxBrightnessTresholdInput.Name = "maxBrightnessTresholdInput";
      this.maxBrightnessTresholdInput.Size = new System.Drawing.Size(61, 20);
      this.maxBrightnessTresholdInput.TabIndex = 22;
      // 
      // minBrightnessTresholdLabel
      // 
      this.minBrightnessTresholdLabel.AutoSize = true;
      this.minBrightnessTresholdLabel.Location = new System.Drawing.Point(13, 174);
      this.minBrightnessTresholdLabel.Name = "minBrightnessTresholdLabel";
      this.minBrightnessTresholdLabel.Size = new System.Drawing.Size(133, 13);
      this.minBrightnessTresholdLabel.TabIndex = 21;
      this.minBrightnessTresholdLabel.Text = "Content sensitivity (0=Low)";
      // 
      // minBrightnessTresholdInput
      // 
      this.minBrightnessTresholdInput.Location = new System.Drawing.Point(154, 172);
      this.minBrightnessTresholdInput.Maximum = new decimal(new int[]
                                                              {
                                                                238,
                                                                0,
                                                                0,
                                                                0
                                                              });
      this.minBrightnessTresholdInput.Name = "minBrightnessTresholdInput";
      this.minBrightnessTresholdInput.Size = new System.Drawing.Size(61, 20);
      this.minBrightnessTresholdInput.TabIndex = 20;
      // 
      // cbUseForVideos
      // 
      this.cbUseForVideos.AutoSize = true;
      this.cbUseForVideos.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbUseForVideos.Location = new System.Drawing.Point(113, 203);
      this.cbUseForVideos.Name = "cbUseForVideos";
      this.cbUseForVideos.Size = new System.Drawing.Size(127, 17);
      this.cbUseForVideos.TabIndex = 19;
      this.cbUseForVideos.Text = "Also use in My Videos";
      this.cbUseForVideos.UseVisualStyleBackColor = true;
      // 
      // cbVerboseLog
      // 
      this.cbVerboseLog.AutoSize = true;
      this.cbVerboseLog.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbVerboseLog.Location = new System.Drawing.Point(21, 203);
      this.cbVerboseLog.Name = "cbVerboseLog";
      this.cbVerboseLog.Size = new System.Drawing.Size(84, 17);
      this.cbVerboseLog.TabIndex = 18;
      this.cbVerboseLog.Text = "Verbose Log";
      this.cbVerboseLog.UseVisualStyleBackColor = true;
      // 
      // minSubtitleHeightLabel
      // 
      this.minSubtitleHeightLabel.AutoSize = true;
      this.minSubtitleHeightLabel.Location = new System.Drawing.Point(13, 106);
      this.minSubtitleHeightLabel.Name = "minSubtitleHeightLabel";
      this.minSubtitleHeightLabel.Size = new System.Drawing.Size(103, 13);
      this.minSubtitleHeightLabel.TabIndex = 16;
      this.minSubtitleHeightLabel.Text = "Min subtitle height %";
      // 
      // minSubtitleHeightInput
      // 
      this.minSubtitleHeightInput.Location = new System.Drawing.Point(154, 104);
      this.minSubtitleHeightInput.Name = "minSubtitleHeightInput";
      this.minSubtitleHeightInput.Size = new System.Drawing.Size(61, 20);
      this.minSubtitleHeightInput.TabIndex = 15;
      // 
      // bottomScanEndInput
      // 
      this.bottomScanEndInput.Location = new System.Drawing.Point(355, 69);
      this.bottomScanEndInput.Name = "bottomScanEndInput";
      this.bottomScanEndInput.Size = new System.Drawing.Size(61, 20);
      this.bottomScanEndInput.TabIndex = 14;
      // 
      // topScanEndInput
      // 
      this.topScanEndInput.Location = new System.Drawing.Point(355, 37);
      this.topScanEndInput.Name = "topScanEndInput";
      this.topScanEndInput.Size = new System.Drawing.Size(61, 20);
      this.topScanEndInput.TabIndex = 13;
      // 
      // bottomScanStartInput
      // 
      this.bottomScanStartInput.Location = new System.Drawing.Point(154, 69);
      this.bottomScanStartInput.Name = "bottomScanStartInput";
      this.bottomScanStartInput.Size = new System.Drawing.Size(61, 20);
      this.bottomScanStartInput.TabIndex = 12;
      // 
      // topScanStartInput
      // 
      this.topScanStartInput.Location = new System.Drawing.Point(154, 37);
      this.topScanStartInput.Name = "topScanStartInput";
      this.topScanStartInput.Size = new System.Drawing.Size(61, 20);
      this.topScanStartInput.TabIndex = 11;
      // 
      // bottomScanEndLabel
      // 
      this.bottomScanEndLabel.AutoSize = true;
      this.bottomScanEndLabel.Location = new System.Drawing.Point(232, 76);
      this.bottomScanEndLabel.Name = "bottomScanEndLabel";
      this.bottomScanEndLabel.Size = new System.Drawing.Size(101, 13);
      this.bottomScanEndLabel.TabIndex = 5;
      this.bottomScanEndLabel.Text = "Bottom Scan End %";
      // 
      // bottomScanStartLabel
      // 
      this.bottomScanStartLabel.AutoSize = true;
      this.bottomScanStartLabel.Location = new System.Drawing.Point(12, 76);
      this.bottomScanStartLabel.Name = "bottomScanStartLabel";
      this.bottomScanStartLabel.Size = new System.Drawing.Size(104, 13);
      this.bottomScanStartLabel.TabIndex = 4;
      this.bottomScanStartLabel.Text = "Bottom Scan Start %";
      // 
      // topScanEndLabel
      // 
      this.topScanEndLabel.AutoSize = true;
      this.topScanEndLabel.Location = new System.Drawing.Point(232, 44);
      this.topScanEndLabel.Name = "topScanEndLabel";
      this.topScanEndLabel.Size = new System.Drawing.Size(87, 13);
      this.topScanEndLabel.TabIndex = 2;
      this.topScanEndLabel.Text = "Top Scan End %";
      // 
      // topScanStartLabel
      // 
      this.topScanStartLabel.AutoSize = true;
      this.topScanStartLabel.Location = new System.Drawing.Point(12, 44);
      this.topScanStartLabel.Name = "topScanStartLabel";
      this.topScanStartLabel.Size = new System.Drawing.Size(90, 13);
      this.topScanStartLabel.TabIndex = 1;
      this.topScanStartLabel.Text = "Top Scan Start %";
      // 
      // manualModeGroup
      // 
      this.manualModeGroup.Controls.Add(this.manualSampleLengthInput);
      this.manualModeGroup.Controls.Add(this.manualSampleLengthLabel);
      this.manualModeGroup.Controls.Add(this.enableManualMode);
      this.manualModeGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.manualModeGroup.Location = new System.Drawing.Point(12, 33);
      this.manualModeGroup.Name = "manualModeGroup";
      this.manualModeGroup.Size = new System.Drawing.Size(214, 113);
      this.manualModeGroup.TabIndex = 2;
      this.manualModeGroup.TabStop = false;
      this.manualModeGroup.Text = "Manual Mode";
      // 
      // manualSampleLengthInput
      // 
      this.manualSampleLengthInput.Location = new System.Drawing.Point(110, 111);
      this.manualSampleLengthInput.Name = "manualSampleLengthInput";
      this.manualSampleLengthInput.Size = new System.Drawing.Size(39, 20);
      this.manualSampleLengthInput.TabIndex = 10;
      this.manualSampleLengthInput.Visible = false;
      // 
      // manualSampleLengthLabel
      // 
      this.manualSampleLengthLabel.AutoSize = true;
      this.manualSampleLengthLabel.Location = new System.Drawing.Point(15, 113);
      this.manualSampleLengthLabel.Name = "manualSampleLengthLabel";
      this.manualSampleLengthLabel.Size = new System.Drawing.Size(89, 13);
      this.manualSampleLengthLabel.TabIndex = 9;
      this.manualSampleLengthLabel.Text = "Frames to sample";
      this.manualSampleLengthLabel.Visible = false;
      // 
      // enableManualMode
      // 
      this.enableManualMode.AutoSize = true;
      this.enableManualMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableManualMode.Location = new System.Drawing.Point(18, 19);
      this.enableManualMode.Name = "enableManualMode";
      this.enableManualMode.Size = new System.Drawing.Size(57, 17);
      this.enableManualMode.TabIndex = 4;
      this.enableManualMode.Text = "Enable";
      this.enableManualMode.UseVisualStyleBackColor = true;
      this.enableManualMode.CheckedChanged += new System.EventHandler(this.OnChange);
      // 
      // automodeGroup
      // 
      this.automodeGroup.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.automodeGroup.Controls.Add(this.bottomMemLength);
      this.automodeGroup.Controls.Add(this.topMemLength);
      this.automodeGroup.Controls.Add(this.labelBottomMemLength);
      this.automodeGroup.Controls.Add(this.labelTopMemLength);
      this.automodeGroup.Controls.Add(this.sampleIntervalInput);
      this.automodeGroup.Controls.Add(this.autoSampleIntervalLabel);
      this.automodeGroup.Controls.Add(this.enableMoveSubs);
      this.automodeGroup.Controls.Add(this.enableAutoMode);
      this.automodeGroup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.automodeGroup.Location = new System.Drawing.Point(232, 33);
      this.automodeGroup.Name = "automodeGroup";
      this.automodeGroup.Size = new System.Drawing.Size(220, 113);
      this.automodeGroup.TabIndex = 3;
      this.automodeGroup.TabStop = false;
      this.automodeGroup.Text = "Automatic Mode";
      // 
      // bottomMemLength
      // 
      this.bottomMemLength.Location = new System.Drawing.Point(153, 84);
      this.bottomMemLength.Name = "bottomMemLength";
      this.bottomMemLength.Size = new System.Drawing.Size(61, 20);
      this.bottomMemLength.TabIndex = 15;
      // 
      // topMemLength
      // 
      this.topMemLength.Location = new System.Drawing.Point(153, 58);
      this.topMemLength.Name = "topMemLength";
      this.topMemLength.Size = new System.Drawing.Size(61, 20);
      this.topMemLength.TabIndex = 14;
      // 
      // labelBottomMemLength
      // 
      this.labelBottomMemLength.AutoSize = true;
      this.labelBottomMemLength.Location = new System.Drawing.Point(19, 86);
      this.labelBottomMemLength.Name = "labelBottomMemLength";
      this.labelBottomMemLength.Size = new System.Drawing.Size(125, 13);
      this.labelBottomMemLength.TabIndex = 13;
      this.labelBottomMemLength.Text = "Bottom memory length (s)";
      // 
      // labelTopMemLength
      // 
      this.labelTopMemLength.AutoSize = true;
      this.labelTopMemLength.Location = new System.Drawing.Point(19, 64);
      this.labelTopMemLength.Name = "labelTopMemLength";
      this.labelTopMemLength.Size = new System.Drawing.Size(111, 13);
      this.labelTopMemLength.TabIndex = 12;
      this.labelTopMemLength.Text = "Top memory length (s)";
      // 
      // sampleIntervalInput
      // 
      this.sampleIntervalInput.Location = new System.Drawing.Point(153, 34);
      this.sampleIntervalInput.Name = "sampleIntervalInput";
      this.sampleIntervalInput.Size = new System.Drawing.Size(61, 20);
      this.sampleIntervalInput.TabIndex = 11;
      // 
      // autoSampleIntervalLabel
      // 
      this.autoSampleIntervalLabel.AutoSize = true;
      this.autoSampleIntervalLabel.Location = new System.Drawing.Point(19, 41);
      this.autoSampleIntervalLabel.Name = "autoSampleIntervalLabel";
      this.autoSampleIntervalLabel.Size = new System.Drawing.Size(101, 13);
      this.autoSampleIntervalLabel.TabIndex = 10;
      this.autoSampleIntervalLabel.Text = "Sample interval (ms)";
      // 
      // enableMoveSubs
      // 
      this.enableMoveSubs.AutoSize = true;
      this.enableMoveSubs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableMoveSubs.Location = new System.Drawing.Point(16, 113);
      this.enableMoveSubs.Name = "enableMoveSubs";
      this.enableMoveSubs.Size = new System.Drawing.Size(94, 17);
      this.enableMoveSubs.TabIndex = 6;
      this.enableMoveSubs.Text = "Move Subtitles";
      this.enableMoveSubs.UseVisualStyleBackColor = true;
      this.enableMoveSubs.Visible = false;
      this.enableMoveSubs.CheckedChanged += new System.EventHandler(this.OnChange);
      // 
      // enableAutoMode
      // 
      this.enableAutoMode.AutoSize = true;
      this.enableAutoMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableAutoMode.Location = new System.Drawing.Point(15, 19);
      this.enableAutoMode.Name = "enableAutoMode";
      this.enableAutoMode.Size = new System.Drawing.Size(57, 17);
      this.enableAutoMode.TabIndex = 5;
      this.enableAutoMode.Text = "Enable";
      this.enableAutoMode.UseVisualStyleBackColor = true;
      this.enableAutoMode.CheckedChanged += new System.EventHandler(this.OnChange);
      // 
      // okButton
      // 
      this.okButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(266, 446);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(93, 27);
      this.okButton.TabIndex = 4;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnOk);
      // 
      // defaultsButton
      // 
      this.defaultsButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultsButton.Location = new System.Drawing.Point(160, 446);
      this.defaultsButton.Name = "defaultsButton";
      this.defaultsButton.Size = new System.Drawing.Size(100, 27);
      this.defaultsButton.TabIndex = 5;
      this.defaultsButton.Text = "&Defaults";
      this.defaultsButton.UseVisualStyleBackColor = true;
      this.defaultsButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnDefaults);
      // 
      // rbDefaultManual
      // 
      this.rbDefaultManual.AutoSize = true;
      this.rbDefaultManual.Checked = true;
      this.rbDefaultManual.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.rbDefaultManual.Location = new System.Drawing.Point(110, 13);
      this.rbDefaultManual.Name = "rbDefaultManual";
      this.rbDefaultManual.Size = new System.Drawing.Size(59, 17);
      this.rbDefaultManual.TabIndex = 9;
      this.rbDefaultManual.TabStop = true;
      this.rbDefaultManual.Text = "Manual";
      this.rbDefaultManual.UseVisualStyleBackColor = true;
      // 
      // rbDefaultAutomatic
      // 
      this.rbDefaultAutomatic.AutoSize = true;
      this.rbDefaultAutomatic.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.rbDefaultAutomatic.Location = new System.Drawing.Point(279, 13);
      this.rbDefaultAutomatic.Name = "rbDefaultAutomatic";
      this.rbDefaultAutomatic.Size = new System.Drawing.Size(71, 17);
      this.rbDefaultAutomatic.TabIndex = 10;
      this.rbDefaultAutomatic.TabStop = true;
      this.rbDefaultAutomatic.Text = "Automatic";
      this.rbDefaultAutomatic.UseVisualStyleBackColor = true;
      // 
      // groupDefaultMode
      // 
      this.groupDefaultMode.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupDefaultMode.Controls.Add(this.rbDefaultAutomatic);
      this.groupDefaultMode.Controls.Add(this.rbDefaultManual);
      this.groupDefaultMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupDefaultMode.Location = new System.Drawing.Point(12, 152);
      this.groupDefaultMode.Name = "groupDefaultMode";
      this.groupDefaultMode.Size = new System.Drawing.Size(440, 36);
      this.groupDefaultMode.TabIndex = 11;
      this.groupDefaultMode.TabStop = false;
      this.groupDefaultMode.Text = "Default Mode";
      // 
      // labelWarning
      // 
      this.labelWarning.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelWarning.AutoSize = true;
      this.labelWarning.ForeColor = System.Drawing.Color.Red;
      this.labelWarning.Location = new System.Drawing.Point(119, 9);
      this.labelWarning.Name = "labelWarning";
      this.labelWarning.Size = new System.Drawing.Size(323, 13);
      this.labelWarning.TabIndex = 20;
      this.labelWarning.Text = "Note that Auto mode is experimental and very resource demanding!";
      // 
      // linkHelp
      // 
      this.linkHelp.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkHelp.AutoSize = true;
      this.linkHelp.Location = new System.Drawing.Point(10, 453);
      this.linkHelp.Name = "linkHelp";
      this.linkHelp.Size = new System.Drawing.Size(125, 13);
      this.linkHelp.TabIndex = 20;
      this.linkHelp.TabStop = true;
      this.linkHelp.Text = "Click for more information";
      this.linkHelp.Click += new System.EventHandler(this.OnHelp);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(365, 446);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(87, 27);
      this.cancelButton.TabIndex = 21;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.OnCancel);
      // 
      // AutoCropperConfig
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(467, 485);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.linkHelp);
      this.Controls.Add(this.labelWarning);
      this.Controls.Add(this.groupDefaultMode);
      this.Controls.Add(this.defaultsButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.automodeGroup);
      this.Controls.Add(this.manualModeGroup);
      this.Controls.Add(this.enableAutoCropper);
      this.Controls.Add(this.Parameters);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "AutoCropperConfig";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "AutoCropper - Setup";
      this.Parameters.ResumeLayout(false);
      this.Parameters.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.maxBrightnessTresholdInput)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.minBrightnessTresholdInput)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.minSubtitleHeightInput)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.bottomScanEndInput)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.topScanEndInput)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.bottomScanStartInput)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.topScanStartInput)).EndInit();
      this.manualModeGroup.ResumeLayout(false);
      this.manualModeGroup.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.manualSampleLengthInput)).EndInit();
      this.automodeGroup.ResumeLayout(false);
      this.automodeGroup.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.bottomMemLength)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.topMemLength)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.sampleIntervalInput)).EndInit();
      this.groupDefaultMode.ResumeLayout(false);
      this.groupDefaultMode.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private void OnCancel(object sender, EventArgs e)
    {
      this.Close();
    }

    private void OnHelp(object sender, EventArgs e)
    {
      try
      {
        System.Diagnostics.Process.Start("IExplore",
                                         "http://wiki.team-mediaportal.com/MediaPortalSetup_Plugins/AutoCropper");
      }
      catch (Exception ex)
      {
        Log.Warn(ex.ToString());
      }
    }

    #endregion

    public void OnOk(object o, MouseEventArgs args)
    {
      SaveSettings();
      this.Close();
    }

    public void OnDefaults(object o, MouseEventArgs args)
    {
      this.enableAutoCropper.Checked = false;
      this.enableAutoMode.Checked = false;
      this.enableManualMode.Checked = true;
      this.manualSampleLengthInput.Value = 30;
      this.enableMoveSubs.Checked = false;
      this.topScanStartInput.Value = 35;
      this.topScanEndInput.Value = 85;
      this.bottomScanEndInput.Value = 100;
      this.bottomScanStartInput.Value = 0;
      this.sampleIntervalInput.Value = 500;
      this.topMemLength.Value = 10;
      this.bottomMemLength.Value = 60;
      this.rbDefaultAutomatic.Checked = false;
      this.rbDefaultManual.Checked = true;
      this.cbUseForVideos.Checked = false;
      this.cbVerboseLog.Checked = false;
      this.minBrightnessTresholdInput.Value = 4;
      this.maxBrightnessTresholdInput.Value = 40;
    }

    // if any settings change check if some options such be hidden
    public void OnChange(object o, EventArgs args)
    {
      //enableManualMode.Enabled = enableAutoCropper.Checked;

      /*if (!enableAutoCropper.Checked)
      {
          this.Parameters.Enabled = false;
          this.groupDefaultMode.Enabled = false;
          this.automodeGroup.Enabled = false;

          this.rbDefaultManual.Enabled = false;
          this.rbDefaultAutomatic.Enabled = false;
      }
      else {
          this.Parameters.Enabled = true;
          this.groupDefaultMode.Enabled = true;
          this.automodeGroup.Enabled = true;

          this.rbDefaultManual.Enabled = true;
          this.rbDefaultAutomatic.Enabled = true;
      }*/

      //this.autoSampleIntervalInput.Enabled = enableAutoMode.Enabled && enableAutoMode.Checked;
      //this.enableMoveSubs.Enabled = false;

      //this.manualSampleLengthInput.Enabled = enableManualMode.Enabled && enableManualMode.Checked;
      //this.automodeGroup.Enabled = false; // enableAutoCropper.Checked;
      // this.enableAutoMode.Enabled = false;
      // this.autoSampleIntervalInput.Enabled = false; // temp
      // this.enableMoveSubs.Enabled = false; // temp
    }


    // parameter names
    public static string enableAutoCropSetting = "enableautocrop";
    public static string enableManualModeSetting = "enablemanualmode";
    public static string enableAutoModeSetting = "enableautomode";
    public static string parmTopStartSetting = "parmtopstart";
    public static string parmTopEndSetting = "parmtopend";
    public static string parmBottomStartSetting = "parmbottomstart";
    public static string parmBottomEndSetting = "parmbottomend";
    public static string parmManualSampleLength = "parmmanualsamplelength";
    public static string parmMinSubtitleHeight = "parmminsubtitleheight";
    public static string parmTopMemoryLength = "parmtopmemlen";
    public static string parmBottomMemoryLength = "parmbottommemlen";
    public static string parmDefaultModeIsManual = "parmDefaultModeIsManual";
    public static string parmSampleInterval = "parmsampleinterval";
    public static string parmVerboseLog = "parmverboselog";
    public static string autoCropSectionName = "autocrop";
    public static string parmUseForMyVideos = "parmuseformyvideos";
    public static string parmMinBrightnessTreshold = "parmminbrigthnesstreshold";
    public static string parmMaxBrightnessTreshold = "parmmaxbrightnesstreshold";

    public void LoadSettings()
    {
      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        this.enableAutoCropper.Checked = reader.GetValueAsBool(autoCropSectionName, enableAutoCropSetting, false);
        this.enableAutoMode.Checked = reader.GetValueAsBool(autoCropSectionName, enableAutoModeSetting, false);
        this.enableManualMode.Checked = reader.GetValueAsBool(autoCropSectionName, enableManualModeSetting, true);
        this.topScanStartInput.Value = reader.GetValueAsInt(autoCropSectionName, parmTopStartSetting, 35);
        this.topScanEndInput.Value = reader.GetValueAsInt(autoCropSectionName, parmTopEndSetting, 80);
        this.bottomScanStartInput.Value = reader.GetValueAsInt(autoCropSectionName, parmBottomStartSetting, 0);
        this.bottomScanEndInput.Value = reader.GetValueAsInt(autoCropSectionName, parmBottomEndSetting, 100);
        //this.manualSampleLengthInput.Value = reader.GetValueAsInt(autoCropSectionName, parmManualSampleLength, 30);
        this.minSubtitleHeightInput.Value = reader.GetValueAsInt(autoCropSectionName, parmMinSubtitleHeight, 10);
        this.topMemLength.Value = reader.GetValueAsInt(autoCropSectionName, parmTopMemoryLength, 10);
          // 10 seconds default
        this.bottomMemLength.Value = reader.GetValueAsInt(autoCropSectionName, parmBottomMemoryLength, 60);
          // 60 seconds default
        this.rbDefaultManual.Checked = reader.GetValueAsBool(autoCropSectionName, parmDefaultModeIsManual, true);
        this.cbVerboseLog.Checked = reader.GetValueAsBool(autoCropSectionName, parmVerboseLog, false);
        this.rbDefaultAutomatic.Checked = !rbDefaultManual.Checked;
        this.cbUseForVideos.Checked = reader.GetValueAsBool(autoCropSectionName, parmUseForMyVideos, false);
        this.sampleIntervalInput.Maximum = 10000;
        this.sampleIntervalInput.Minimum = 0;
        this.sampleIntervalInput.Value = reader.GetValueAsInt(autoCropSectionName, parmSampleInterval, 500);
          // 2 times a second default
        this.minBrightnessTresholdInput.Value = reader.GetValueAsInt(autoCropSectionName, parmMinBrightnessTreshold, 4);
        this.maxBrightnessTresholdInput.Value = reader.GetValueAsInt(autoCropSectionName, parmMaxBrightnessTreshold, 40);
      }
    }

    public void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool(autoCropSectionName, enableAutoCropSetting, enableAutoCropper.Checked);
        xmlwriter.SetValueAsBool(autoCropSectionName, enableManualModeSetting, enableManualMode.Checked);
        xmlwriter.SetValueAsBool(autoCropSectionName, enableAutoModeSetting, enableAutoMode.Checked);
        xmlwriter.SetValue(autoCropSectionName, parmTopStartSetting, topScanStartInput.Value);
        xmlwriter.SetValue(autoCropSectionName, parmTopEndSetting, topScanEndInput.Value);
        xmlwriter.SetValue(autoCropSectionName, parmBottomStartSetting, bottomScanStartInput.Value);
        xmlwriter.SetValue(autoCropSectionName, parmBottomEndSetting, bottomScanEndInput.Value);
        //xmlwriter.SetValue(autoCropSectionName, parmManualSampleLength, manualSampleLengthInput.Value);
        xmlwriter.SetValue(autoCropSectionName, parmMinSubtitleHeight, minSubtitleHeightInput.Value);
        xmlwriter.SetValue(autoCropSectionName, parmMaxBrightnessTreshold, maxBrightnessTresholdInput.Value);
        xmlwriter.SetValue(autoCropSectionName, parmMinBrightnessTreshold, minBrightnessTresholdInput.Value);
        xmlwriter.SetValue(autoCropSectionName, parmBottomMemoryLength, bottomMemLength.Value);
        xmlwriter.SetValue(autoCropSectionName, parmTopMemoryLength, topMemLength.Value);
        xmlwriter.SetValueAsBool(autoCropSectionName, parmDefaultModeIsManual, rbDefaultManual.Checked);
        xmlwriter.SetValue(autoCropSectionName, parmSampleInterval, sampleIntervalInput.Value);
        xmlwriter.SetValueAsBool(autoCropSectionName, parmUseForMyVideos, cbUseForVideos.Checked);
        xmlwriter.SetValueAsBool(autoCropSectionName, parmVerboseLog, cbVerboseLog.Checked);
      }
    }
  }
}