#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class TVDebugOptions : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPRadioButton radioButton1;
    private IContainer components = null;
    private bool _init = false;
    private MPCheckBox mpUseRtspInSingleSeatCheckBox;
    private MPLabel mpWarningLabel;
    private MPCheckBox mpEnableRecordingFromTimeshiftCheckBox;
    private MPCheckBox mpNoRtspInMultiSeatCheckBox;
    private MPCheckBox mpDoNotAllowSlowMotionDuringZappingCheckBox;
    private MPToolTip mpMainToolTip;
    public int pluginVersion;

    public TVDebugOptions()
      : this("Debug Options")
    {
    }

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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TVDebugOptions));
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpEnableRecordingFromTimeshiftCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpNoRtspInMultiSeatCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpUseRtspInSingleSeatCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpWarningLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpMainToolTip = new MediaPortal.UserInterface.Controls.MPToolTip();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.mpDoNotAllowSlowMotionDuringZappingCheckBox);
      this.groupBox1.Controls.Add(this.mpEnableRecordingFromTimeshiftCheckBox);
      this.groupBox1.Controls.Add(this.mpNoRtspInMultiSeatCheckBox);
      this.groupBox1.Controls.Add(this.mpUseRtspInSingleSeatCheckBox);
      this.groupBox1.Controls.Add(this.mpWarningLabel);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 187);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // mpDoNotAllowSlowMotionDuringZappingCheckBox
      // 
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.AutoSize = true;
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.Location = new System.Drawing.Point(9, 142);
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.Name = "mpDoNotAllowSlowMotionDuringZappingCheckBox";
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.Size = new System.Drawing.Size(336, 17);
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.TabIndex = 4;
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.Text = "Do not use slow motion to sync video to audio on channel change";
      this.mpMainToolTip.SetToolTip(this.mpDoNotAllowSlowMotionDuringZappingCheckBox, "Selecting this will prevent live TV from playing video until video is in sync wit" +
              "h the audio, instead of playing video in slow motion");
      this.mpDoNotAllowSlowMotionDuringZappingCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpEnableRecordingFromTimeshiftCheckBox
      // 
      this.mpEnableRecordingFromTimeshiftCheckBox.AutoSize = true;
      this.mpEnableRecordingFromTimeshiftCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpEnableRecordingFromTimeshiftCheckBox.Location = new System.Drawing.Point(9, 118);
      this.mpEnableRecordingFromTimeshiftCheckBox.Name = "mpEnableRecordingFromTimeshiftCheckBox";
      this.mpEnableRecordingFromTimeshiftCheckBox.Size = new System.Drawing.Size(269, 17);
      this.mpEnableRecordingFromTimeshiftCheckBox.TabIndex = 3;
      this.mpEnableRecordingFromTimeshiftCheckBox.Text = "Include timeshift buffer in \"Record Now\" recordings ";
      this.mpMainToolTip.SetToolTip(this.mpEnableRecordingFromTimeshiftCheckBox, "When enabled, \"Record Now\" will include in the recording the part of the current " +
              "program that has already been timeshifted, instead of starting the recording fro" +
              "m the live point.");
      this.mpEnableRecordingFromTimeshiftCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpNoRtspInMultiSeatCheckBox
      // 
      this.mpNoRtspInMultiSeatCheckBox.AutoSize = true;
      this.mpNoRtspInMultiSeatCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpNoRtspInMultiSeatCheckBox.Location = new System.Drawing.Point(9, 94);
      this.mpNoRtspInMultiSeatCheckBox.Name = "mpNoRtspInMultiSeatCheckBox";
      this.mpNoRtspInMultiSeatCheckBox.Size = new System.Drawing.Size(195, 17);
      this.mpNoRtspInMultiSeatCheckBox.TabIndex = 2;
      this.mpNoRtspInMultiSeatCheckBox.Text = "Do not use RTSP in multi seat setup";
      this.mpMainToolTip.SetToolTip(this.mpNoRtspInMultiSeatCheckBox, resources.GetString("mpNoRtspInMultiSeatCheckBox.ToolTip"));
      this.mpNoRtspInMultiSeatCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpUseRtspInSingleSeatCheckBox
      // 
      this.mpUseRtspInSingleSeatCheckBox.AutoSize = true;
      this.mpUseRtspInSingleSeatCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpUseRtspInSingleSeatCheckBox.Location = new System.Drawing.Point(9, 70);
      this.mpUseRtspInSingleSeatCheckBox.Name = "mpUseRtspInSingleSeatCheckBox";
      this.mpUseRtspInSingleSeatCheckBox.Size = new System.Drawing.Size(195, 17);
      this.mpUseRtspInSingleSeatCheckBox.TabIndex = 1;
      this.mpUseRtspInSingleSeatCheckBox.Text = "Use RTSP even in single seat setup";
      this.mpUseRtspInSingleSeatCheckBox.UseVisualStyleBackColor = true;
      // 
      // mpWarningLabel
      // 
      this.mpWarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
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
      // TVDebugOptions
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "TVDebugOptions";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }


      using (Settings xmlreader = new MPSettings())
      {
        mpNoRtspInMultiSeatCheckBox.Checked = !xmlreader.GetValueAsBool("tvservice", "usertsp", true);
      }
      mpUseRtspInSingleSeatCheckBox.Checked = DebugSettings.UseRTSP;
      mpEnableRecordingFromTimeshiftCheckBox.Checked = DebugSettings.EnableRecordingFromTimeshift;
      mpDoNotAllowSlowMotionDuringZappingCheckBox.Checked = DebugSettings.DoNotAllowSlowMotionDuringZapping;

    }


    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("tvservice", "usertsp", !mpNoRtspInMultiSeatCheckBox.Checked);
      }
      DebugSettings.UseRTSP = mpUseRtspInSingleSeatCheckBox.Checked;
      DebugSettings.EnableRecordingFromTimeshift = mpEnableRecordingFromTimeshiftCheckBox.Checked;
      DebugSettings.DoNotAllowSlowMotionDuringZapping = mpDoNotAllowSlowMotionDuringZappingCheckBox.Checked;
    }
  }
}
