#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

namespace MediaPortal.GUI.Alarm
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
  public class AlarmSetupForm : System.Windows.Forms.Form, ISetupForm, IShowPlugin
  {
    private MediaPortal.UserInterface.Controls.MPButton btnCancel;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private MediaPortal.UserInterface.Controls.MPButton btnOk;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPLabel label9;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private System.Windows.Forms.NumericUpDown MessageDisplayLength;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private System.Windows.Forms.NumericUpDown SnoozeLength;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private System.Windows.Forms.NumericUpDown RepeatCount;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private System.Windows.Forms.NumericUpDown RepeatSeconds;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPButton btnAlarmSoundsFolder;
    private MediaPortal.UserInterface.Controls.MPTextBox txtAlarmSoundsFolder;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AlarmSetupForm));
      this.btnOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.SnoozeLength = new System.Windows.Forms.NumericUpDown();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.MessageDisplayLength = new System.Windows.Forms.NumericUpDown();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.RepeatCount = new System.Windows.Forms.NumericUpDown();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.RepeatSeconds = new System.Windows.Forms.NumericUpDown();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnAlarmSoundsFolder = new MediaPortal.UserInterface.Controls.MPButton();
      this.txtAlarmSoundsFolder = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.SnoozeLength)).BeginInit();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.MessageDisplayLength)).BeginInit();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.RepeatCount)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.RepeatSeconds)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(240, 280);
      this.btnOk.Name = "btnOk";
      this.btnOk.TabIndex = 3;
      this.btnOk.Text = "&Ok";
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(320, 280);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.TabIndex = 4;
      this.btnCancel.Text = "&Cancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.SnoozeLength);
      this.groupBox3.Controls.Add(this.label5);
      this.groupBox3.Controls.Add(this.label9);
      this.groupBox3.Location = new System.Drawing.Point(8, 152);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(392, 56);
      this.groupBox3.TabIndex = 35;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Snooze";
      // 
      // SnoozeLength
      // 
      this.SnoozeLength.Location = new System.Drawing.Point(96, 24);
      this.SnoozeLength.Maximum = new System.Decimal(new int[] {
																		 59,
																		 0,
																		 0,
																		 0});
      this.SnoozeLength.Minimum = new System.Decimal(new int[] {
																		 1,
																		 0,
																		 0,
																		 0});
      this.SnoozeLength.Name = "SnoozeLength";
      this.SnoozeLength.Size = new System.Drawing.Size(40, 20);
      this.SnoozeLength.TabIndex = 24;
      this.SnoozeLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.SnoozeLength.Value = new System.Decimal(new int[] {
																	   5,
																	   0,
																	   0,
																	   0});
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(144, 24);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(56, 16);
      this.label5.TabIndex = 25;
      this.label5.Text = "(minutes)";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(8, 24);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(64, 24);
      this.label9.TabIndex = 9;
      this.label9.Text = "Duration:";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.MessageDisplayLength);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Location = new System.Drawing.Point(8, 216);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(392, 56);
      this.groupBox2.TabIndex = 34;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Message";
      // 
      // MessageDisplayLength
      // 
      this.MessageDisplayLength.Location = new System.Drawing.Point(96, 24);
      this.MessageDisplayLength.Name = "MessageDisplayLength";
      this.MessageDisplayLength.Size = new System.Drawing.Size(40, 20);
      this.MessageDisplayLength.TabIndex = 23;
      this.MessageDisplayLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(144, 24);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(56, 16);
      this.label8.TabIndex = 22;
      this.label8.Text = "(seconds)";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(8, 24);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(88, 16);
      this.label7.TabIndex = 20;
      this.label7.Text = "Display length:";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.RepeatCount);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.RepeatSeconds);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.btnAlarmSoundsFolder);
      this.groupBox1.Controls.Add(this.txtAlarmSoundsFolder);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(8, 16);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(392, 128);
      this.groupBox1.TabIndex = 43;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Sounds";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(96, 88);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(56, 16);
      this.label6.TabIndex = 51;
      this.label6.Text = "times.";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // RepeatCount
      // 
      this.RepeatCount.Location = new System.Drawing.Point(48, 88);
      this.RepeatCount.Maximum = new System.Decimal(new int[] {
																		59,
																		0,
																		0,
																		0});
      this.RepeatCount.Minimum = new System.Decimal(new int[] {
																		1,
																		0,
																		0,
																		0});
      this.RepeatCount.Name = "RepeatCount";
      this.RepeatCount.Size = new System.Drawing.Size(40, 20);
      this.RepeatCount.TabIndex = 50;
      this.RepeatCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.RepeatCount.Value = new System.Decimal(new int[] {
																	  5,
																	  0,
																	  0,
																	  0});
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 88);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(32, 16);
      this.label3.TabIndex = 49;
      this.label3.Text = "Loop";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(192, 64);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(56, 16);
      this.label4.TabIndex = 48;
      this.label4.Text = "(seconds)";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // RepeatSeconds
      // 
      this.RepeatSeconds.Location = new System.Drawing.Point(136, 64);
      this.RepeatSeconds.Maximum = new System.Decimal(new int[] {
																		  120,
																		  0,
																		  0,
																		  0});
      this.RepeatSeconds.Minimum = new System.Decimal(new int[] {
																		  1,
																		  0,
																		  0,
																		  0});
      this.RepeatSeconds.Name = "RepeatSeconds";
      this.RepeatSeconds.Size = new System.Drawing.Size(48, 20);
      this.RepeatSeconds.TabIndex = 47;
      this.RepeatSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.RepeatSeconds.Value = new System.Decimal(new int[] {
																		120,
																		0,
																		0,
																		0});
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 64);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(136, 16);
      this.label2.TabIndex = 46;
      this.label2.Text = "Loop sounds less than ";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnAlarmSoundsFolder
      // 
      this.btnAlarmSoundsFolder.Location = new System.Drawing.Point(312, 32);
      this.btnAlarmSoundsFolder.Name = "btnAlarmSoundsFolder";
      this.btnAlarmSoundsFolder.Size = new System.Drawing.Size(64, 23);
      this.btnAlarmSoundsFolder.TabIndex = 45;
      this.btnAlarmSoundsFolder.Text = "&Browse";
      this.btnAlarmSoundsFolder.Click += new System.EventHandler(this.btnAlarmSoundsFolder_Click);
      // 
      // txtAlarmSoundsFolder
      // 
      this.txtAlarmSoundsFolder.Location = new System.Drawing.Point(88, 32);
      this.txtAlarmSoundsFolder.Name = "txtAlarmSoundsFolder";
      this.txtAlarmSoundsFolder.Size = new System.Drawing.Size(216, 20);
      this.txtAlarmSoundsFolder.TabIndex = 44;
      this.txtAlarmSoundsFolder.Text = "";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 32);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 24);
      this.label1.TabIndex = 43;
      this.label1.Text = "Sounds folder:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // AlarmSetupForm
      // 
      this.AutoScaleMode = AutoScaleMode.None;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(410, 312);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AlarmSetupForm";
      this.Text = "My Alarm Setup";
      this.Load += new System.EventHandler(this.AlarmSetupFrom_Load);
      this.groupBox3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.SnoozeLength)).EndInit();
      this.groupBox2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.MessageDisplayLength)).EndInit();
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.RepeatCount)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.RepeatSeconds)).EndInit();
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
      return "My Alarm";
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
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("alarm", "alarmSoundsFolder", txtAlarmSoundsFolder.Text);
        xmlwriter.SetValue("alarm", "alarmSnoozeTime", SnoozeLength.Value);
        xmlwriter.SetValue("alarm", "alarmMessageDisplayLength", MessageDisplayLength.Value);
        xmlwriter.SetValue("alarm", "alarmRepeatSeconds", RepeatSeconds.Value);
        xmlwriter.SetValue("alarm", "alarmRepeatCount", RepeatCount.Value);
      }
    }

    /// <summary>
    /// Loads my alarm settings from the profile xml.
    /// </summary>
    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        txtAlarmSoundsFolder.Text = xmlreader.GetValueAsString("alarm", "alarmSoundsFolder", string.Empty);
        SnoozeLength.Value = xmlreader.GetValueAsInt("alarm", "alarmSnoozeTime", 5);
        MessageDisplayLength.Value = xmlreader.GetValueAsInt("alarm", "alarmMessageDisplayLength", 10);
        RepeatSeconds.Value = xmlreader.GetValueAsInt("alarm", "alarmRepeatSeconds", 120);
        RepeatCount.Value = xmlreader.GetValueAsInt("alarm", "alarmRepeatCount", 5);
      }
    }

    #endregion

  }
}
