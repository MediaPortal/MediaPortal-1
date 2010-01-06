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

using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;


#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class TVDebugOptions : SectionSettings
  {
    private MPGroupBox groupBoxSettings;
    private MPRadioButton radioButton1;
    private IContainer components = null;
    private bool _init = false;
    private MPCheckBox mpUseRtspCheckBox;
    private MPLabel mpWarningLabel;
    private MPCheckBox mpEnableRecordingFromTimeshiftCheckBox;
    private MPCheckBox mpDoNotAllowSlowMotionDuringZappingCheckBox;
    private MPToolTip mpMainToolTip;
    private bool singleSeat;
    private MPGroupBox mpRtspPathsGroupBox;
    private MPLabel mpLabel1;
    private MPLabel mpLabelRecording;
    private MPLabel mpLabelTimeshifting;
    private System.Windows.Forms.TextBox textBoxTimeshifting;
    private System.Windows.Forms.TextBox textBoxRecording;
    private System.Windows.Forms.Button buttonTimeshiftingPath;
    private System.Windows.Forms.Button buttonRecordingPath;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    public int pluginVersion;


    public TVDebugOptions()
      : this("Debug Options") {}

    public TVDebugOptions(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        _init = true;
        LoadSettings();
      }
    }

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
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof (TVDebugOptions));
      this.groupBoxSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpEnableRecordingFromTimeshiftCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpUseRtspCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpWarningLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpMainToolTip = new MediaPortal.UserInterface.Controls.MPToolTip();
      this.mpRtspPathsGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonTimeshiftingPath = new System.Windows.Forms.Button();
      this.buttonRecordingPath = new System.Windows.Forms.Button();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelRecording = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabelTimeshifting = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxTimeshifting = new System.Windows.Forms.TextBox();
      this.textBoxRecording = new System.Windows.Forms.TextBox();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.groupBoxSettings.SuspendLayout();
      this.mpRtspPathsGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSettings.Controls.Add(this.mpDoNotAllowSlowMotionDuringZappingCheckBox);
      this.groupBoxSettings.Controls.Add(this.mpEnableRecordingFromTimeshiftCheckBox);
      this.groupBoxSettings.Controls.Add(this.mpUseRtspCheckBox);
      this.groupBoxSettings.Controls.Add(this.mpWarningLabel);
      this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSettings.Location = new System.Drawing.Point(0, 0);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(472, 158);
      this.groupBoxSettings.TabIndex = 0;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // mpDoNotAllowSlowMotionDuringZappingCheckBox
      // 
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.AutoSize = true;
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.Location = new System.Drawing.Point(9, 70);
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.Name = "mpDoNotAllowSlowMotionDuringZappingCheckBox";
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.Size = new System.Drawing.Size(336, 17);
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.TabIndex = 4;
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.Text =
        "Do not use slow motion to sync video to audio on channel change";
      this.mpMainToolTip.SetToolTip(this.mpDoNotAllowSlowMotionDuringZappingCheckBox,
                                    "Selecting this will prevent live TV from playing video until video is in sync wit" +
                                    "h the audio, instead of playing video in slow motion");
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpEnableRecordingFromTimeshiftCheckBox
      // 
      this.mpEnableRecordingFromTimeshiftCheckBox.AutoSize = true;
      this.mpEnableRecordingFromTimeshiftCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpEnableRecordingFromTimeshiftCheckBox.Location = new System.Drawing.Point(9, 93);
      this.mpEnableRecordingFromTimeshiftCheckBox.Name = "mpEnableRecordingFromTimeshiftCheckBox";
      this.mpEnableRecordingFromTimeshiftCheckBox.Size = new System.Drawing.Size(269, 17);
      this.mpEnableRecordingFromTimeshiftCheckBox.TabIndex = 3;
      this.mpEnableRecordingFromTimeshiftCheckBox.Text = "Include timeshift buffer in \"Record Now\" recordings ";
      this.mpMainToolTip.SetToolTip(this.mpEnableRecordingFromTimeshiftCheckBox,
                                    "When enabled, \"Record Now\" will include in the recording the part of the current " +
                                    "program that has already been timeshifted, instead of starting the recording fro" +
                                    "m the live point.");
      this.mpEnableRecordingFromTimeshiftCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpUseRtspCheckBox
      // 
      this.mpUseRtspCheckBox.AutoSize = true;
      this.mpUseRtspCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpUseRtspCheckBox.Location = new System.Drawing.Point(9, 116);
      this.mpUseRtspCheckBox.Name = "mpUseRtspCheckBox";
      this.mpUseRtspCheckBox.Size = new System.Drawing.Size(144, 17);
      this.mpUseRtspCheckBox.TabIndex = 1;
      this.mpUseRtspCheckBox.Text = "-- Label defined in code --\r\n";
      this.mpUseRtspCheckBox.UseVisualStyleBackColor = true;
      this.mpUseRtspCheckBox.CheckedChanged += new System.EventHandler(this.mpUseRtspCheckBox_Checked);
      // 
      // mpWarningLabel
      // 
      this.mpWarningLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpWarningLabel.ForeColor = System.Drawing.Color.Red;
      this.mpWarningLabel.Location = new System.Drawing.Point(6, 16);
      this.mpWarningLabel.Name = "mpWarningLabel";
      this.mpWarningLabel.Size = new System.Drawing.Size(460, 51);
      this.mpWarningLabel.TabIndex = 0;
      this.mpWarningLabel.Text = "This section provides special/debugging settings that are not supported by the Te" +
                                 "am. Some of these settings are experimental. Do not alter any of the settings be" +
                                 "low unless you know what you are doing.";
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButton1.Location = new System.Drawing.Point(0, 0);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(104, 24);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.UseVisualStyleBackColor = true;
      // 
      // mpRtspPathsGroupBox
      // 
      this.mpRtspPathsGroupBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpRtspPathsGroupBox.Controls.Add(this.buttonTimeshiftingPath);
      this.mpRtspPathsGroupBox.Controls.Add(this.buttonRecordingPath);
      this.mpRtspPathsGroupBox.Controls.Add(this.mpLabel1);
      this.mpRtspPathsGroupBox.Controls.Add(this.mpLabelRecording);
      this.mpRtspPathsGroupBox.Controls.Add(this.mpLabelTimeshifting);
      this.mpRtspPathsGroupBox.Controls.Add(this.textBoxTimeshifting);
      this.mpRtspPathsGroupBox.Controls.Add(this.textBoxRecording);
      this.mpRtspPathsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpRtspPathsGroupBox.Location = new System.Drawing.Point(0, 164);
      this.mpRtspPathsGroupBox.Name = "mpRtspPathsGroupBox";
      this.mpRtspPathsGroupBox.Size = new System.Drawing.Size(472, 231);
      this.mpRtspPathsGroupBox.TabIndex = 1;
      this.mpRtspPathsGroupBox.TabStop = false;
      this.mpRtspPathsGroupBox.Text = "Additional RTSP settings";
      // 
      // buttonTimeshiftingPath
      // 
      this.buttonTimeshiftingPath.Location = new System.Drawing.Point(408, 98);
      this.buttonTimeshiftingPath.Name = "buttonTimeshiftingPath";
      this.buttonTimeshiftingPath.Size = new System.Drawing.Size(58, 20);
      this.buttonTimeshiftingPath.TabIndex = 6;
      this.buttonTimeshiftingPath.Text = "browse";
      this.buttonTimeshiftingPath.UseVisualStyleBackColor = true;
      this.buttonTimeshiftingPath.Click += new System.EventHandler(this.buttonTimeshiftingPath_Click);
      // 
      // buttonRecordingPath
      // 
      this.buttonRecordingPath.Location = new System.Drawing.Point(408, 49);
      this.buttonRecordingPath.Name = "buttonRecordingPath";
      this.buttonRecordingPath.Size = new System.Drawing.Size(58, 20);
      this.buttonRecordingPath.TabIndex = 5;
      this.buttonRecordingPath.Text = "browse";
      this.buttonRecordingPath.UseVisualStyleBackColor = true;
      this.buttonRecordingPath.Click += new System.EventHandler(this.buttonRecordingPath_Click);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(30, 124);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(392, 104);
      this.mpLabel1.TabIndex = 4;
      this.mpLabel1.Text = resources.GetString("mpLabel1.Text");
      // 
      // mpLabelRecording
      // 
      this.mpLabelRecording.AutoSize = true;
      this.mpLabelRecording.Location = new System.Drawing.Point(6, 32);
      this.mpLabelRecording.Name = "mpLabelRecording";
      this.mpLabelRecording.Size = new System.Drawing.Size(83, 13);
      this.mpLabelRecording.TabIndex = 3;
      this.mpLabelRecording.Text = "Recording path:";
      // 
      // mpLabelTimeshifting
      // 
      this.mpLabelTimeshifting.AutoSize = true;
      this.mpLabelTimeshifting.Location = new System.Drawing.Point(6, 80);
      this.mpLabelTimeshifting.Name = "mpLabelTimeshifting";
      this.mpLabelTimeshifting.Size = new System.Drawing.Size(90, 13);
      this.mpLabelTimeshifting.TabIndex = 2;
      this.mpLabelTimeshifting.Text = "Timeshifting path:";
      // 
      // textBoxTimeshifting
      // 
      this.textBoxTimeshifting.Location = new System.Drawing.Point(9, 98);
      this.textBoxTimeshifting.Name = "textBoxTimeshifting";
      this.textBoxTimeshifting.Size = new System.Drawing.Size(398, 20);
      this.textBoxTimeshifting.TabIndex = 1;
      // 
      // textBoxRecording
      // 
      this.textBoxRecording.Location = new System.Drawing.Point(9, 50);
      this.textBoxRecording.Name = "textBoxRecording";
      this.textBoxRecording.Size = new System.Drawing.Size(398, 20);
      this.textBoxRecording.TabIndex = 0;
      // 
      // folderBrowserDialog
      // 
      this.folderBrowserDialog.Description = "Select the appropriate network folder";
      this.folderBrowserDialog.ShowNewFolderButton = false;
      // 
      // TVDebugOptions
      // 
      this.Controls.Add(this.mpRtspPathsGroupBox);
      this.Controls.Add(this.groupBoxSettings);
      this.Name = "TVDebugOptions";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxSettings.ResumeLayout(false);
      this.groupBoxSettings.PerformLayout();
      this.mpRtspPathsGroupBox.ResumeLayout(false);
      this.mpRtspPathsGroupBox.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }

      singleSeat = Common.IsSingleSeat();

      using (Settings xmlreader = new MPSettings())
      {
        textBoxRecording.Text = xmlreader.GetValueAsString("tvservice", "recordingpath", "");
        textBoxTimeshifting.Text = xmlreader.GetValueAsString("tvservice", "timeshiftingpath", "");
        bool rtsp = xmlreader.GetValueAsBool("tvservice", "usertsp", !singleSeat);
        mpUseRtspCheckBox.Checked = singleSeat ? rtsp : !rtsp;
      }
      mpUseRtspCheckBox.Text = singleSeat ? "Single seat setup: force RTSP usage." : "Multi seat setup: use UNC paths.";
      mpRtspPathsGroupBox.Visible = !singleSeat;

      mpEnableRecordingFromTimeshiftCheckBox.Checked = DebugSettings.EnableRecordingFromTimeshift;
      mpDoNotAllowSlowMotionDuringZappingCheckBox.Checked = DebugSettings.DoNotAllowSlowMotionDuringZapping;
    }


    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }

      bool rtsp = singleSeat ? mpUseRtspCheckBox.Checked : !mpUseRtspCheckBox.Checked;

      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("tvservice", "usertsp", rtsp);
        xmlwriter.SetValue("tvservice", "recordingpath", textBoxRecording.Text);
        xmlwriter.SetValue("tvservice", "timeshiftingpath", textBoxTimeshifting.Text);
      }

      DebugSettings.EnableRecordingFromTimeshift = mpEnableRecordingFromTimeshiftCheckBox.Checked;
      DebugSettings.DoNotAllowSlowMotionDuringZapping = mpDoNotAllowSlowMotionDuringZappingCheckBox.Checked;
    }

    private void mpUseRtspCheckBox_Checked(object sender, System.EventArgs e)
    {
      mpRtspPathsGroupBox.Visible = !singleSeat && mpUseRtspCheckBox.Checked;
    }

    private void buttonRecordingPath_Click(object sender, System.EventArgs e)
    {
      folderBrowserDialog.ShowDialog();
      textBoxRecording.Text = folderBrowserDialog.SelectedPath;
    }

    private void buttonTimeshiftingPath_Click(object sender, System.EventArgs e)
    {
      folderBrowserDialog.ShowDialog();
      textBoxTimeshifting.Text = folderBrowserDialog.SelectedPath;
    }
  }
}