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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.Alarm
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
  [PluginIcons("WindowPlugins.GUIAlarm.Alarm.gif", "WindowPlugins.GUIAlarm.Alarm_disabled.gif")]
  public class AlarmSetupForm : MediaPortal.UserInterface.Controls.MPConfigForm, ISetupForm, IShowPlugin
  {
    private MediaPortal.UserInterface.Controls.MPButton btnCancel;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private MediaPortal.UserInterface.Controls.MPButton btnOk;
    private MediaPortal.UserInterface.Controls.MPGroupBox grpSounds;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private System.Windows.Forms.NumericUpDown RepeatCount;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private System.Windows.Forms.NumericUpDown RepeatSeconds;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPButton btnAlarmSoundsFolder;
    private MediaPortal.UserInterface.Controls.MPTextBox txtAlarmSoundsFolder;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private GroupBox grpAlarmTimeout;
    private NumericUpDown NUDAlarmTimeout;
    private Label lblAlarmTimeout;
    private Label LblRadioOnlyMsg;
    private GroupBox grpAlarmVol;
    private NumericUpDown NUDAlarmVol;
    private CheckBox chkEnableDefaultVol;
    private Label lblAlarmVol2;
    private Label lblAlarmVol1;
    private Label lblAlarmVol3;
    private Label lblAlarmVol4;
    private Button cmdAlarmVolTest;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public AlarmSetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.grpSounds = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.RepeatCount = new System.Windows.Forms.NumericUpDown();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.RepeatSeconds = new System.Windows.Forms.NumericUpDown();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnAlarmSoundsFolder = new MediaPortal.UserInterface.Controls.MPButton();
      this.txtAlarmSoundsFolder = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.grpAlarmTimeout = new System.Windows.Forms.GroupBox();
      this.LblRadioOnlyMsg = new System.Windows.Forms.Label();
      this.NUDAlarmTimeout = new System.Windows.Forms.NumericUpDown();
      this.lblAlarmTimeout = new System.Windows.Forms.Label();
      this.grpAlarmVol = new System.Windows.Forms.GroupBox();
      this.cmdAlarmVolTest = new System.Windows.Forms.Button();
      this.lblAlarmVol4 = new System.Windows.Forms.Label();
      this.lblAlarmVol3 = new System.Windows.Forms.Label();
      this.lblAlarmVol2 = new System.Windows.Forms.Label();
      this.lblAlarmVol1 = new System.Windows.Forms.Label();
      this.NUDAlarmVol = new System.Windows.Forms.NumericUpDown();
      this.chkEnableDefaultVol = new System.Windows.Forms.CheckBox();
      this.grpSounds.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.RepeatCount)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.RepeatSeconds)).BeginInit();
      this.grpAlarmTimeout.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.NUDAlarmTimeout)).BeginInit();
      this.grpAlarmVol.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.NUDAlarmVol)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.Location = new System.Drawing.Point(245, 286);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(75, 23);
      this.btnOk.TabIndex = 3;
      this.btnOk.Text = "&Ok";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(325, 286);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 4;
      this.btnCancel.Text = "&Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // grpSounds
      // 
      this.grpSounds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpSounds.Controls.Add(this.label6);
      this.grpSounds.Controls.Add(this.RepeatCount);
      this.grpSounds.Controls.Add(this.label3);
      this.grpSounds.Controls.Add(this.label4);
      this.grpSounds.Controls.Add(this.RepeatSeconds);
      this.grpSounds.Controls.Add(this.label2);
      this.grpSounds.Controls.Add(this.btnAlarmSoundsFolder);
      this.grpSounds.Controls.Add(this.txtAlarmSoundsFolder);
      this.grpSounds.Controls.Add(this.label1);
      this.grpSounds.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.grpSounds.Location = new System.Drawing.Point(8, 12);
      this.grpSounds.Name = "grpSounds";
      this.grpSounds.Size = new System.Drawing.Size(392, 103);
      this.grpSounds.TabIndex = 0;
      this.grpSounds.TabStop = false;
      this.grpSounds.Text = "Sounds";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(96, 71);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(56, 16);
      this.label6.TabIndex = 51;
      this.label6.Text = "times.";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // RepeatCount
      // 
      this.RepeatCount.Location = new System.Drawing.Point(48, 71);
      this.RepeatCount.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
      this.RepeatCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.RepeatCount.Name = "RepeatCount";
      this.RepeatCount.Size = new System.Drawing.Size(40, 20);
      this.RepeatCount.TabIndex = 3;
      this.RepeatCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.RepeatCount.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 71);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(32, 16);
      this.label3.TabIndex = 49;
      this.label3.Text = "Loop";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(192, 47);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(56, 16);
      this.label4.TabIndex = 48;
      this.label4.Text = "(seconds)";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // RepeatSeconds
      // 
      this.RepeatSeconds.Location = new System.Drawing.Point(136, 47);
      this.RepeatSeconds.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
      this.RepeatSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.RepeatSeconds.Name = "RepeatSeconds";
      this.RepeatSeconds.Size = new System.Drawing.Size(48, 20);
      this.RepeatSeconds.TabIndex = 2;
      this.RepeatSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.RepeatSeconds.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 47);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(136, 16);
      this.label2.TabIndex = 46;
      this.label2.Text = "Loop sounds less than ";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnAlarmSoundsFolder
      // 
      this.btnAlarmSoundsFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAlarmSoundsFolder.Location = new System.Drawing.Point(312, 19);
      this.btnAlarmSoundsFolder.Name = "btnAlarmSoundsFolder";
      this.btnAlarmSoundsFolder.Size = new System.Drawing.Size(64, 23);
      this.btnAlarmSoundsFolder.TabIndex = 1;
      this.btnAlarmSoundsFolder.Text = "&Browse";
      this.btnAlarmSoundsFolder.UseVisualStyleBackColor = true;
      this.btnAlarmSoundsFolder.Click += new System.EventHandler(this.btnAlarmSoundsFolder_Click);
      // 
      // txtAlarmSoundsFolder
      // 
      this.txtAlarmSoundsFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtAlarmSoundsFolder.BorderColor = System.Drawing.Color.Empty;
      this.txtAlarmSoundsFolder.Location = new System.Drawing.Point(88, 19);
      this.txtAlarmSoundsFolder.Name = "txtAlarmSoundsFolder";
      this.txtAlarmSoundsFolder.Size = new System.Drawing.Size(216, 20);
      this.txtAlarmSoundsFolder.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 24);
      this.label1.TabIndex = 43;
      this.label1.Text = "Sounds folder:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // grpAlarmTimeout
      // 
      this.grpAlarmTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpAlarmTimeout.Controls.Add(this.LblRadioOnlyMsg);
      this.grpAlarmTimeout.Controls.Add(this.NUDAlarmTimeout);
      this.grpAlarmTimeout.Controls.Add(this.lblAlarmTimeout);
      this.grpAlarmTimeout.Location = new System.Drawing.Point(8, 121);
      this.grpAlarmTimeout.Name = "grpAlarmTimeout";
      this.grpAlarmTimeout.Size = new System.Drawing.Size(392, 67);
      this.grpAlarmTimeout.TabIndex = 1;
      this.grpAlarmTimeout.TabStop = false;
      this.grpAlarmTimeout.Text = "Alarm Timeout";
      // 
      // LblRadioOnlyMsg
      // 
      this.LblRadioOnlyMsg.AutoSize = true;
      this.LblRadioOnlyMsg.Location = new System.Drawing.Point(8, 16);
      this.LblRadioOnlyMsg.Name = "LblRadioOnlyMsg";
      this.LblRadioOnlyMsg.Size = new System.Drawing.Size(250, 13);
      this.LblRadioOnlyMsg.TabIndex = 1;
      this.LblRadioOnlyMsg.Text = "This feature only applies to radio and playlist alarms.";
      // 
      // NUDAlarmTimeout
      // 
      this.NUDAlarmTimeout.Location = new System.Drawing.Point(107, 36);
      this.NUDAlarmTimeout.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
      this.NUDAlarmTimeout.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.NUDAlarmTimeout.Name = "NUDAlarmTimeout";
      this.NUDAlarmTimeout.Size = new System.Drawing.Size(65, 20);
      this.NUDAlarmTimeout.TabIndex = 0;
      this.NUDAlarmTimeout.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // lblAlarmTimeout
      // 
      this.lblAlarmTimeout.AutoSize = true;
      this.lblAlarmTimeout.Location = new System.Drawing.Point(8, 38);
      this.lblAlarmTimeout.Name = "lblAlarmTimeout";
      this.lblAlarmTimeout.Size = new System.Drawing.Size(93, 13);
      this.lblAlarmTimeout.TabIndex = 0;
      this.lblAlarmTimeout.Text = "Timeout (minutes):";
      // 
      // grpAlarmVol
      // 
      this.grpAlarmVol.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpAlarmVol.Controls.Add(this.cmdAlarmVolTest);
      this.grpAlarmVol.Controls.Add(this.lblAlarmVol4);
      this.grpAlarmVol.Controls.Add(this.lblAlarmVol3);
      this.grpAlarmVol.Controls.Add(this.lblAlarmVol2);
      this.grpAlarmVol.Controls.Add(this.lblAlarmVol1);
      this.grpAlarmVol.Controls.Add(this.NUDAlarmVol);
      this.grpAlarmVol.Controls.Add(this.chkEnableDefaultVol);
      this.grpAlarmVol.Location = new System.Drawing.Point(8, 194);
      this.grpAlarmVol.Name = "grpAlarmVol";
      this.grpAlarmVol.Size = new System.Drawing.Size(392, 86);
      this.grpAlarmVol.TabIndex = 2;
      this.grpAlarmVol.TabStop = false;
      this.grpAlarmVol.Text = "Alarm Volume";
      // 
      // cmdAlarmVolTest
      // 
      this.cmdAlarmVolTest.Location = new System.Drawing.Point(287, 51);
      this.cmdAlarmVolTest.Name = "cmdAlarmVolTest";
      this.cmdAlarmVolTest.Size = new System.Drawing.Size(89, 23);
      this.cmdAlarmVolTest.TabIndex = 2;
      this.cmdAlarmVolTest.Text = "Test Volume";
      this.cmdAlarmVolTest.UseVisualStyleBackColor = true;
      this.cmdAlarmVolTest.Click += new System.EventHandler(this.cmdAlarmVolTest_Click);
      // 
      // lblAlarmVol4
      // 
      this.lblAlarmVol4.AutoSize = true;
      this.lblAlarmVol4.Location = new System.Drawing.Point(249, 56);
      this.lblAlarmVol4.Name = "lblAlarmVol4";
      this.lblAlarmVol4.Size = new System.Drawing.Size(15, 13);
      this.lblAlarmVol4.TabIndex = 5;
      this.lblAlarmVol4.Text = "%";
      // 
      // lblAlarmVol3
      // 
      this.lblAlarmVol3.AutoSize = true;
      this.lblAlarmVol3.Location = new System.Drawing.Point(96, 56);
      this.lblAlarmVol3.Name = "lblAlarmVol3";
      this.lblAlarmVol3.Size = new System.Drawing.Size(79, 13);
      this.lblAlarmVol3.TabIndex = 4;
      this.lblAlarmVol3.Text = "Volume setting:";
      // 
      // lblAlarmVol2
      // 
      this.lblAlarmVol2.AutoSize = true;
      this.lblAlarmVol2.Location = new System.Drawing.Point(8, 29);
      this.lblAlarmVol2.Name = "lblAlarmVol2";
      this.lblAlarmVol2.Size = new System.Drawing.Size(126, 13);
      this.lblAlarmVol2.TabIndex = 3;
      this.lblAlarmVol2.Text = "(except message alarms).";
      // 
      // lblAlarmVol1
      // 
      this.lblAlarmVol1.AutoSize = true;
      this.lblAlarmVol1.Location = new System.Drawing.Point(8, 16);
      this.lblAlarmVol1.Name = "lblAlarmVol1";
      this.lblAlarmVol1.Size = new System.Drawing.Size(345, 13);
      this.lblAlarmVol1.TabIndex = 2;
      this.lblAlarmVol1.Text = "When set, this volume setting will be set whenever an alarm is activated";
      // 
      // NUDAlarmVol
      // 
      this.NUDAlarmVol.Enabled = false;
      this.NUDAlarmVol.Location = new System.Drawing.Point(181, 52);
      this.NUDAlarmVol.Name = "NUDAlarmVol";
      this.NUDAlarmVol.Size = new System.Drawing.Size(62, 20);
      this.NUDAlarmVol.TabIndex = 1;
      // 
      // chkEnableDefaultVol
      // 
      this.chkEnableDefaultVol.AutoSize = true;
      this.chkEnableDefaultVol.Location = new System.Drawing.Point(11, 55);
      this.chkEnableDefaultVol.Name = "chkEnableDefaultVol";
      this.chkEnableDefaultVol.Size = new System.Drawing.Size(65, 17);
      this.chkEnableDefaultVol.TabIndex = 0;
      this.chkEnableDefaultVol.Text = "Enable?";
      this.chkEnableDefaultVol.UseVisualStyleBackColor = true;
      this.chkEnableDefaultVol.CheckedChanged += new System.EventHandler(this.chkEnableDefaultVol_CheckedChanged);
      // 
      // AlarmSetupForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(410, 316);
      this.Controls.Add(this.grpAlarmVol);
      this.Controls.Add(this.grpAlarmTimeout);
      this.Controls.Add(this.grpSounds);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "AlarmSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Alarm - Setup";
      this.Load += new System.EventHandler(this.AlarmSetupFrom_Load);
      this.grpSounds.ResumeLayout(false);
      this.grpSounds.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.RepeatCount)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.RepeatSeconds)).EndInit();
      this.grpAlarmTimeout.ResumeLayout(false);
      this.grpAlarmTimeout.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.NUDAlarmTimeout)).EndInit();
      this.grpAlarmVol.ResumeLayout(false);
      this.grpAlarmVol.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.NUDAlarmVol)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    #region ISetupFormEx Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "An alarm plugin for MediaPortal";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return 5000;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(850); //My Alarm
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "hover_my alarm.png";
      return true;
    }

    public string Author()
    {
      return "Devo";
    }

    public string PluginName()
    {
      return "Alarm";
    }

    public void ShowPlugin()
    {
      ShowDialog();
    }

    public bool HasSetup()
    {
      return true;
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    #region Button Events
    /// <summary>
    /// Opens a folder dialog for the alarm sounds
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnAlarmSoundsFolder_Click(object sender, System.EventArgs e)
    {
      using (folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the folder where alarm sounds will be stored";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = txtAlarmSoundsFolder.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          txtAlarmSoundsFolder.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    /// <summary>
    /// Cancels modifing the properties of myalarm
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnCancel_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }

    /// <summary>
    /// Saves settings to config file then closes the window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnOk_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    /// <summary>
    /// Enables/disables the volume setting box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void chkEnableDefaultVol_CheckedChanged(object sender, EventArgs e)
    {
      if (chkEnableDefaultVol.Checked == true)
      {
        NUDAlarmVol.Enabled = true;
        cmdAlarmVolTest.Enabled = true;
      }
      else
      {
        NUDAlarmVol.Enabled = false;
        cmdAlarmVolTest.Enabled = false;
      }
    }

    /// <summary>
    /// Plays a test sound to test out the set alarm volume setting
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cmdAlarmVolTest_Click(object sender, EventArgs e)
    {
      MediaPortal.Player.VolumeHandler volumeHandler = new MediaPortal.Player.VolumeHandler();

      //convert the volume percentage into real value
      int realVolume;
      realVolume = ConvertPercentageToRealVolume(NUDAlarmVol.Value);

      //save the current volume setting
      int existingVolume;
      existingVolume = volumeHandler.Volume;

      //set the test volume setting
      volumeHandler.Volume = realVolume;

      //play windows sound
      const string testAudioFilePath = @"C:\windows\Media\chimes.wav";
      if (System.IO.File.Exists(testAudioFilePath))
      {
        MediaPortal.Util.Utils.PlaySound(testAudioFilePath, true, true);
      }
      else
      {
        MessageBox.Show("Cannot find the file " + testAudioFilePath + "." + Environment.NewLine + Environment.NewLine + "Try using another program to play something now - the current volume setting is the alarm volume setting. Pressing OK now will revert the volume to the previous setting.");
      }

      //revert the volume setting to the previous one
      volumeHandler.Volume = existingVolume;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Form Load method
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AlarmSetupFrom_Load(object sender, System.EventArgs e)
    {
      LoadSettings();
    }
    /// <summary>
    /// Saves my alarm settings to the profile xml.
    /// </summary>
    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("alarm", "alarmSoundsFolder", txtAlarmSoundsFolder.Text);
        xmlwriter.SetValue("alarm", "alarmTimeout", NUDAlarmTimeout.Value);
        xmlwriter.SetValue("alarm", "alarmRepeatSeconds", RepeatSeconds.Value);
        xmlwriter.SetValue("alarm", "alarmRepeatCount", RepeatCount.Value);
        xmlwriter.SetValueAsBool("alarm", "alarmAlarmVolEnable", chkEnableDefaultVol.Checked);

        //convert the volume percentage into real value first
        xmlwriter.SetValue("alarm", "alarmAlarmVol", ConvertPercentageToRealVolume(NUDAlarmVol.Value));
      }
    }

    /// <summary>
    /// Loads my alarm settings from the profile xml.
    /// </summary>
    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        txtAlarmSoundsFolder.Text = xmlreader.GetValueAsString("alarm", "alarmSoundsFolder", string.Empty);
        NUDAlarmTimeout.Value = xmlreader.GetValueAsInt("alarm", "alarmTimeout", 60);
        RepeatSeconds.Value = xmlreader.GetValueAsInt("alarm", "alarmRepeatSeconds", 120);
        RepeatCount.Value = xmlreader.GetValueAsInt("alarm", "alarmRepeatCount", 5);
        chkEnableDefaultVol.Checked = xmlreader.GetValueAsBool("alarm", "alarmAlarmVolEnable", false);
        //make sure the button states are correct
        chkEnableDefaultVol_CheckedChanged(this, new System.EventArgs());

        //convert real volume value to percentage first
        NUDAlarmVol.Value = ConvertRealVolumeToPercentage(xmlreader.GetValueAsInt("alarm", "alarmAlarmVol", 0));
      }
    }

    /// <summary>
    /// Converts the real volume value into a volume percentage
    /// </summary>
    /// <param name="realVolume">real volume value</param>
    /// <returns></returns>
    private int ConvertRealVolumeToPercentage(decimal realVolume)
    {
      decimal tmpVolume;
      MediaPortal.Player.VolumeHandler volumeHandler = new MediaPortal.Player.VolumeHandler();
      tmpVolume = realVolume;
      tmpVolume = tmpVolume / (volumeHandler.Maximum - volumeHandler.Minimum);
      tmpVolume = tmpVolume * 100;
      return (int)System.Math.Round(tmpVolume, 0);
    }

    /// <summary>
    /// Converts the volume percentage into a real volume value
    /// </summary>
    /// <param name="percentVolume">volume percentage value, e.g. 50</param>
    /// <returns></returns>
    private int ConvertPercentageToRealVolume(decimal percentVolume)
    {
      MediaPortal.Player.VolumeHandler volumeHandler = new MediaPortal.Player.VolumeHandler();

      decimal tmpVolume;
      tmpVolume = percentVolume;
      tmpVolume = tmpVolume / 100;
      tmpVolume = tmpVolume * (volumeHandler.Maximum - volumeHandler.Minimum);
      tmpVolume = tmpVolume + volumeHandler.Minimum;

      return (int)System.Math.Round(tmpVolume, 0);
    }

    #endregion
  }
}
