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

#region Usings
using System;
using System.IO;

using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using System.Diagnostics;
using XPBurn;
#endregion

namespace MediaPortal.GUI.GUIBurner
{
  /// <summary>
  /// Summary description for BurnerSetupForm.
  /// </summary>
  public partial class BurnerSetupForm : Form
  {
    #region Private Variables
    private XPBurn.XPBurnCD CDBurner;
    private int selIndx = 0;
    private MediaPortal.UserInterface.Controls.MPButton buttonOK;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxDeviceSettings;
    private ComboBox comboBoxDeviceSelection;
    private MediaPortal.UserInterface.Controls.MPLabel labelDriveletter;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBoxBurnerDriver;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxOptions;
    private Label labelTempHint;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxTempPath;
    private MediaPortal.UserInterface.Controls.MPLabel labelSelectTempPath;
    private MediaPortal.UserInterface.Controls.MPButton buttonSelectTempPathLocation;
    private CheckBox checkBoxDontBurnDVD;
    private CheckBox checkBoxLeaveFileForDebug;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxSupportFiles;
    private MediaPortal.UserInterface.Controls.MPLabel labeldvdburnPath;
    private MediaPortal.UserInterface.Controls.MPButton buttonSelectDvdBurnPathLocation;
    private TextBox textBoxDVDBurnExePath;
    private LinkLabel linkLabelDVDBurnDownload;
    private LinkLabel linkLabelCygwinDownload;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxDVDFormat;
    private RadioButton radioButtonTvFormatNtsc;
    private RadioButton radioButtonTvFormatPal;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;
    #endregion

    #region BurnerSetupForm
    public BurnerSetupForm()
    {
      InitializeComponent();
      LoadSettings();
      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }
    #endregion

    #region Serialization
    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        textBoxTempPath.Text = xmlreader.GetValueAsString("burner", "temp_folder", Path.GetDirectoryName(Path.GetTempPath()));
        textBoxDVDBurnExePath.Text = xmlreader.GetValueAsString("burner", "dvdburnexe_folder", @"C:\Program Files\Windows Resource Kits\Tools");

        selIndx = xmlreader.GetValueAsInt("burner", "recorder", 0);
        mpTextBoxBurnerDriver.Text = xmlreader.GetValueAsString("burner", "recorderdrive", "D:");

        radioButtonTvFormatPal.Checked = xmlreader.GetValueAsBool("burner", "PalTvFormat", true);
        radioButtonTvFormatNtsc.Checked = !radioButtonTvFormatPal.Checked;

        checkBoxLeaveFileForDebug.Checked = xmlreader.GetValueAsBool("burner", "leavedebugfiles", true);
        checkBoxDontBurnDVD.Checked = xmlreader.GetValueAsBool("burner", "dummyburn", false);

        try
        {
          CDBurner = new XPBurn.XPBurnCD();
          GetRecorder();
          comboBoxDeviceSelection.SelectedIndex = selIndx;
          comboBoxDeviceSelection.Enabled = true;
        }
        catch (Exception ex)
        {
          MessageBox.Show("Some components are missing!");
        }        
        textBoxTempPath.Enabled = true;
        buttonSelectTempPathLocation.Enabled = true;        
      }
    }

    private void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.SetValue("burner", "temp_folder", textBoxTempPath.Text);
        xmlwriter.SetValue("burner", "dvdburnexe_folder", textBoxDVDBurnExePath.Text);

        xmlwriter.SetValue("burner", "recorder", comboBoxDeviceSelection.SelectedIndex);
        xmlwriter.SetValue("burner", "recorderdrive", mpTextBoxBurnerDriver.Text);

        xmlwriter.SetValueAsBool("burner", "PalTvFormat", radioButtonTvFormatPal.Checked);

        xmlwriter.SetValueAsBool("burner", "leavedebugfiles", checkBoxLeaveFileForDebug.Checked);
        xmlwriter.SetValueAsBool("burner", "dummyburn", checkBoxDontBurnDVD.Checked);
      }
    }
    #endregion

    #region Overrides
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
    #endregion

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.buttonOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxDeviceSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxDeviceSelection = new System.Windows.Forms.ComboBox();
      this.labelDriveletter = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxBurnerDriver = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxOptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelTempHint = new System.Windows.Forms.Label();
      this.textBoxTempPath = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelSelectTempPath = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonSelectTempPathLocation = new MediaPortal.UserInterface.Controls.MPButton();
      this.checkBoxDontBurnDVD = new System.Windows.Forms.CheckBox();
      this.checkBoxLeaveFileForDebug = new System.Windows.Forms.CheckBox();
      this.groupBoxSupportFiles = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labeldvdburnPath = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonSelectDvdBurnPathLocation = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxDVDBurnExePath = new System.Windows.Forms.TextBox();
      this.linkLabelDVDBurnDownload = new System.Windows.Forms.LinkLabel();
      this.linkLabelCygwinDownload = new System.Windows.Forms.LinkLabel();
      this.groupBoxDVDFormat = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonTvFormatNtsc = new System.Windows.Forms.RadioButton();
      this.radioButtonTvFormatPal = new System.Windows.Forms.RadioButton();
      this.groupBoxDeviceSettings.SuspendLayout();
      this.groupBoxOptions.SuspendLayout();
      this.groupBoxSupportFiles.SuspendLayout();
      this.groupBoxDVDFormat.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(437, 393);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(88, 24);
      this.buttonOK.TabIndex = 2;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // groupBoxDeviceSettings
      // 
      this.groupBoxDeviceSettings.Controls.Add(this.comboBoxDeviceSelection);
      this.groupBoxDeviceSettings.Controls.Add(this.labelDriveletter);
      this.groupBoxDeviceSettings.Controls.Add(this.mpTextBoxBurnerDriver);
      this.groupBoxDeviceSettings.Controls.Add(this.label1);
      this.groupBoxDeviceSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxDeviceSettings.Location = new System.Drawing.Point(12, 21);
      this.groupBoxDeviceSettings.Name = "groupBoxDeviceSettings";
      this.groupBoxDeviceSettings.Size = new System.Drawing.Size(616, 57);
      this.groupBoxDeviceSettings.TabIndex = 58;
      this.groupBoxDeviceSettings.TabStop = false;
      this.groupBoxDeviceSettings.Text = "Device settings";
      // 
      // comboBoxDeviceSelection
      // 
      this.comboBoxDeviceSelection.Enabled = false;
      this.comboBoxDeviceSelection.FormattingEnabled = true;
      this.comboBoxDeviceSelection.Location = new System.Drawing.Point(120, 21);
      this.comboBoxDeviceSelection.Name = "comboBoxDeviceSelection";
      this.comboBoxDeviceSelection.Size = new System.Drawing.Size(346, 21);
      this.comboBoxDeviceSelection.TabIndex = 48;
      // 
      // labelDriveletter
      // 
      this.labelDriveletter.AutoSize = true;
      this.labelDriveletter.Location = new System.Drawing.Point(499, 24);
      this.labelDriveletter.Name = "labelDriveletter";
      this.labelDriveletter.Size = new System.Drawing.Size(61, 13);
      this.labelDriveletter.TabIndex = 47;
      this.labelDriveletter.Text = "Drive letter:";
      // 
      // mpTextBoxBurnerDriver
      // 
      this.mpTextBoxBurnerDriver.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxBurnerDriver.Location = new System.Drawing.Point(566, 21);
      this.mpTextBoxBurnerDriver.Name = "mpTextBoxBurnerDriver";
      this.mpTextBoxBurnerDriver.Size = new System.Drawing.Size(32, 20);
      this.mpTextBoxBurnerDriver.TabIndex = 46;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(96, 13);
      this.label1.TabIndex = 45;
      this.label1.Text = "Select burner drive";
      // 
      // buttonCancel
      // 
      this.buttonCancel.Location = new System.Drawing.Point(540, 393);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(88, 24);
      this.buttonCancel.TabIndex = 59;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // groupBoxOptions
      // 
      this.groupBoxOptions.Controls.Add(this.groupBoxDVDFormat);
      this.groupBoxOptions.Controls.Add(this.checkBoxDontBurnDVD);
      this.groupBoxOptions.Controls.Add(this.checkBoxLeaveFileForDebug);
      this.groupBoxOptions.Controls.Add(this.labelTempHint);
      this.groupBoxOptions.Controls.Add(this.textBoxTempPath);
      this.groupBoxOptions.Controls.Add(this.labelSelectTempPath);
      this.groupBoxOptions.Controls.Add(this.buttonSelectTempPathLocation);
      this.groupBoxOptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxOptions.Location = new System.Drawing.Point(12, 94);
      this.groupBoxOptions.Name = "groupBoxOptions";
      this.groupBoxOptions.Size = new System.Drawing.Size(616, 144);
      this.groupBoxOptions.TabIndex = 60;
      this.groupBoxOptions.TabStop = false;
      this.groupBoxOptions.Text = "Options";
      // 
      // labelTempHint
      // 
      this.labelTempHint.AutoSize = true;
      this.labelTempHint.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelTempHint.Location = new System.Drawing.Point(120, 55);
      this.labelTempHint.Name = "labelTempHint";
      this.labelTempHint.Size = new System.Drawing.Size(469, 13);
      this.labelTempHint.TabIndex = 60;
      this.labelTempHint.Text = "All temp files for the burn process will be stored there (This may be up to 5 gb " +
          "for a complete DVD).";
      // 
      // textBoxTempPath
      // 
      this.textBoxTempPath.BorderColor = System.Drawing.Color.Empty;
      this.textBoxTempPath.Enabled = false;
      this.textBoxTempPath.Location = new System.Drawing.Point(120, 27);
      this.textBoxTempPath.Name = "textBoxTempPath";
      this.textBoxTempPath.Size = new System.Drawing.Size(431, 20);
      this.textBoxTempPath.TabIndex = 57;
      // 
      // labelSelectTempPath
      // 
      this.labelSelectTempPath.AutoSize = true;
      this.labelSelectTempPath.Location = new System.Drawing.Point(15, 30);
      this.labelSelectTempPath.Name = "labelSelectTempPath";
      this.labelSelectTempPath.Size = new System.Drawing.Size(87, 13);
      this.labelSelectTempPath.TabIndex = 58;
      this.labelSelectTempPath.Text = "Select temp path";
      // 
      // buttonSelectTempPathLocation
      // 
      this.buttonSelectTempPathLocation.Enabled = false;
      this.buttonSelectTempPathLocation.Location = new System.Drawing.Point(568, 24);
      this.buttonSelectTempPathLocation.Name = "buttonSelectTempPathLocation";
      this.buttonSelectTempPathLocation.Size = new System.Drawing.Size(30, 24);
      this.buttonSelectTempPathLocation.TabIndex = 59;
      this.buttonSelectTempPathLocation.Text = "...";
      this.buttonSelectTempPathLocation.UseVisualStyleBackColor = true;
      this.buttonSelectTempPathLocation.Click += new System.EventHandler(this.buttonSelectTempPathLocation_Click);
      // 
      // checkBoxDontBurnDVD
      // 
      this.checkBoxDontBurnDVD.AutoSize = true;
      this.checkBoxDontBurnDVD.Location = new System.Drawing.Point(120, 114);
      this.checkBoxDontBurnDVD.Name = "checkBoxDontBurnDVD";
      this.checkBoxDontBurnDVD.Size = new System.Drawing.Size(126, 17);
      this.checkBoxDontBurnDVD.TabIndex = 67;
      this.checkBoxDontBurnDVD.Text = "Do not burn the DVD";
      this.checkBoxDontBurnDVD.UseVisualStyleBackColor = true;
      // 
      // checkBoxLeaveFileForDebug
      // 
      this.checkBoxLeaveFileForDebug.AutoSize = true;
      this.checkBoxLeaveFileForDebug.Location = new System.Drawing.Point(120, 91);
      this.checkBoxLeaveFileForDebug.Name = "checkBoxLeaveFileForDebug";
      this.checkBoxLeaveFileForDebug.Size = new System.Drawing.Size(178, 17);
      this.checkBoxLeaveFileForDebug.TabIndex = 63;
      this.checkBoxLeaveFileForDebug.Text = "Keep Temp Files For Debugging";
      this.checkBoxLeaveFileForDebug.UseVisualStyleBackColor = true;
      // 
      // groupBoxSupportFiles
      // 
      this.groupBoxSupportFiles.Controls.Add(this.linkLabelCygwinDownload);
      this.groupBoxSupportFiles.Controls.Add(this.linkLabelDVDBurnDownload);
      this.groupBoxSupportFiles.Controls.Add(this.labeldvdburnPath);
      this.groupBoxSupportFiles.Controls.Add(this.buttonSelectDvdBurnPathLocation);
      this.groupBoxSupportFiles.Controls.Add(this.textBoxDVDBurnExePath);
      this.groupBoxSupportFiles.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSupportFiles.Location = new System.Drawing.Point(12, 253);
      this.groupBoxSupportFiles.Name = "groupBoxSupportFiles";
      this.groupBoxSupportFiles.Size = new System.Drawing.Size(616, 125);
      this.groupBoxSupportFiles.TabIndex = 61;
      this.groupBoxSupportFiles.TabStop = false;
      this.groupBoxSupportFiles.Text = "Necessary support files";
      // 
      // labeldvdburnPath
      // 
      this.labeldvdburnPath.AutoSize = true;
      this.labeldvdburnPath.Location = new System.Drawing.Point(18, 92);
      this.labeldvdburnPath.Name = "labeldvdburnPath";
      this.labeldvdburnPath.Size = new System.Drawing.Size(138, 13);
      this.labeldvdburnPath.TabIndex = 61;
      this.labeldvdburnPath.Text = "Select path to dvdburn.exe ";
      // 
      // buttonSelectDvdBurnPathLocation
      // 
      this.buttonSelectDvdBurnPathLocation.Location = new System.Drawing.Point(568, 86);
      this.buttonSelectDvdBurnPathLocation.Name = "buttonSelectDvdBurnPathLocation";
      this.buttonSelectDvdBurnPathLocation.Size = new System.Drawing.Size(30, 24);
      this.buttonSelectDvdBurnPathLocation.TabIndex = 60;
      this.buttonSelectDvdBurnPathLocation.Text = "...";
      this.buttonSelectDvdBurnPathLocation.UseVisualStyleBackColor = true;
      this.buttonSelectDvdBurnPathLocation.Click += new System.EventHandler(this.buttonSelectDvdBurnPathLocation_Click);
      // 
      // textBoxDVDBurnExePath
      // 
      this.textBoxDVDBurnExePath.Location = new System.Drawing.Point(171, 89);
      this.textBoxDVDBurnExePath.Name = "textBoxDVDBurnExePath";
      this.textBoxDVDBurnExePath.Size = new System.Drawing.Size(380, 20);
      this.textBoxDVDBurnExePath.TabIndex = 59;
      this.textBoxDVDBurnExePath.Text = "C:\\Program Files\\Windows Resource Kits\\Tools";
      // 
      // linkLabelDVDBurnDownload
      // 
      this.linkLabelDVDBurnDownload.AutoSize = true;
      this.linkLabelDVDBurnDownload.LinkArea = new System.Windows.Forms.LinkArea(28, 9);
      this.linkLabelDVDBurnDownload.Location = new System.Drawing.Point(18, 66);
      this.linkLabelDVDBurnDownload.Name = "linkLabelDVDBurnDownload";
      this.linkLabelDVDBurnDownload.Size = new System.Drawing.Size(378, 17);
      this.linkLabelDVDBurnDownload.TabIndex = 66;
      this.linkLabelDVDBurnDownload.TabStop = true;
      this.linkLabelDVDBurnDownload.Text = "Please download and install this file from Microsoft containing dvdburn.exe";
      this.linkLabelDVDBurnDownload.UseCompatibleTextRendering = true;
      this.linkLabelDVDBurnDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDVDBurnDownload_LinkClicked);
      // 
      // linkLabelCygwinDownload
      // 
      this.linkLabelCygwinDownload.AutoSize = true;
      this.linkLabelCygwinDownload.LinkArea = new System.Windows.Forms.LinkArea(16, 19);
      this.linkLabelCygwinDownload.Location = new System.Drawing.Point(18, 28);
      this.linkLabelCygwinDownload.Name = "linkLabelCygwinDownload";
      this.linkLabelCygwinDownload.Size = new System.Drawing.Size(443, 17);
      this.linkLabelCygwinDownload.TabIndex = 67;
      this.linkLabelCygwinDownload.TabStop = true;
      this.linkLabelCygwinDownload.Text = "Please download this cygwin package and put its folder into MediaPortal\'s root di" +
          "rectory";
      this.linkLabelCygwinDownload.UseCompatibleTextRendering = true;
      this.linkLabelCygwinDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCygwinDownload_LinkClicked);
      // 
      // groupBoxDVDFormat
      // 
      this.groupBoxDVDFormat.Controls.Add(this.radioButtonTvFormatNtsc);
      this.groupBoxDVDFormat.Controls.Add(this.radioButtonTvFormatPal);
      this.groupBoxDVDFormat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxDVDFormat.Location = new System.Drawing.Point(410, 86);
      this.groupBoxDVDFormat.Name = "groupBoxDVDFormat";
      this.groupBoxDVDFormat.Size = new System.Drawing.Size(141, 45);
      this.groupBoxDVDFormat.TabIndex = 68;
      this.groupBoxDVDFormat.TabStop = false;
      this.groupBoxDVDFormat.Text = "DVD Format";
      // 
      // radioButtonTvFormatNtsc
      // 
      this.radioButtonTvFormatNtsc.AutoSize = true;
      this.radioButtonTvFormatNtsc.Location = new System.Drawing.Point(80, 19);
      this.radioButtonTvFormatNtsc.Name = "radioButtonTvFormatNtsc";
      this.radioButtonTvFormatNtsc.Size = new System.Drawing.Size(54, 17);
      this.radioButtonTvFormatNtsc.TabIndex = 64;
      this.radioButtonTvFormatNtsc.TabStop = true;
      this.radioButtonTvFormatNtsc.Text = "NTSC";
      this.radioButtonTvFormatNtsc.UseVisualStyleBackColor = true;
      // 
      // radioButtonTvFormatPal
      // 
      this.radioButtonTvFormatPal.AutoSize = true;
      this.radioButtonTvFormatPal.Checked = true;
      this.radioButtonTvFormatPal.Location = new System.Drawing.Point(15, 19);
      this.radioButtonTvFormatPal.Name = "radioButtonTvFormatPal";
      this.radioButtonTvFormatPal.Size = new System.Drawing.Size(45, 17);
      this.radioButtonTvFormatPal.TabIndex = 63;
      this.radioButtonTvFormatPal.TabStop = true;
      this.radioButtonTvFormatPal.Text = "PAL";
      this.radioButtonTvFormatPal.UseVisualStyleBackColor = true;
      // 
      // BurnerSetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(640, 429);
      this.Controls.Add(this.groupBoxSupportFiles);
      this.Controls.Add(this.groupBoxOptions);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.groupBoxDeviceSettings);
      this.Controls.Add(this.buttonOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "BurnerSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "My Burner setup";
      this.groupBoxDeviceSettings.ResumeLayout(false);
      this.groupBoxDeviceSettings.PerformLayout();
      this.groupBoxOptions.ResumeLayout(false);
      this.groupBoxOptions.PerformLayout();
      this.groupBoxSupportFiles.ResumeLayout(false);
      this.groupBoxSupportFiles.PerformLayout();
      this.groupBoxDVDFormat.ResumeLayout(false);
      this.groupBoxDVDFormat.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    #region Private Methods
    private void GetRecorder()
    {
      //Fill The Combobox with available drives
      string name;

      for (int i = 0; i < CDBurner.NumberOfDrives; i++)
      {
        CDBurner.BurnerDrive = CDBurner.RecorderDrives[i].ToString();
        name = CDBurner.Vendor + " " + CDBurner.ProductID + " " + CDBurner.Revision;
        comboBoxDeviceSelection.Items.Add(name);
        comboBoxDeviceSelection.SelectedIndex = 0;
      }
    }

    private void buttonOK_Click(object sender, System.EventArgs e)
    {
      if (textBoxTempPath.Text == "")
      {
        MessageBox.Show("Please select a Temp folder");
      }
      else
      {
        SaveSettings();
        this.Close();
      }
    }

    private void buttonSelectTempPathLocation_Click(object sender, System.EventArgs e)
    {
      using (folderBrowserDialog1 = new FolderBrowserDialog())
      {
        folderBrowserDialog1.Description = "Select a temporary file folder";
        folderBrowserDialog1.ShowNewFolderButton = true;
        folderBrowserDialog1.SelectedPath = textBoxTempPath.Text;
        DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          textBoxTempPath.Text = folderBrowserDialog1.SelectedPath;
        }
      }
    }

    private void buttonSelectDvdBurnPathLocation_Click(object sender, EventArgs e)
    {
      using (folderBrowserDialog1 = new FolderBrowserDialog())
      {
        folderBrowserDialog1.Description = "Select where DVDBurn.exe is installed";
        folderBrowserDialog1.ShowNewFolderButton = true;
        folderBrowserDialog1.SelectedPath = textBoxDVDBurnExePath.Text;

        DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
          textBoxDVDBurnExePath.Text = folderBrowserDialog1.SelectedPath;
        }
      }
    }


    #endregion

    private void linkLabelDVDBurnDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Help.ShowHelp(this, @"http://www.microsoft.com/downloads/details.aspx?FamilyID=9D467A69-57FF-4AE7-96EE-B18C4790CFFD&displaylang=en");
      }
      catch
      {
      }
    }

    private void linkLabelCygwinDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Help.ShowHelp(this, @"http://www.team-mediaportal.com/files/Download/SystemUtilities/BurnerSupportFiles.rar/");
      }
      catch
      {
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}
