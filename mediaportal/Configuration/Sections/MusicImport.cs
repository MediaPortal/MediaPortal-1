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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MusicImport : SectionSettings
  {
    private class Preset
    {
      private int target;
      private int minimum;
      private int maximum;

      public int Target
      {
        get { return target; }
      }

      public int Minimum
      {
        get { return minimum; }
      }

      public int Maximum
      {
        get { return maximum; }
      }

      public Preset(string presetString)
      {
        target = Convert.ToInt16(presetString.Split(',')[0]);
        minimum = Convert.ToInt16(presetString.Split(',')[1]);
        maximum = Convert.ToInt16(presetString.Split(',')[2]);
      }
    }

    private IContainer components = null;
    private MPButton buttonDefault;
    private MPButton buttonBrowse;
    private MPButton buttonLocateLAME;
    private MPCheckBox checkBoxFastMode;
    private MPCheckBox checkBoxCBR;
    private MPCheckBox checkBoxDatabase;
    private MPCheckBox checkBoxReplace;
    private MPCheckBox checkBoxMono;
    private MPCheckBox checkBoxBackground;
    private FolderBrowserDialog folderBrowserDialog;
    private MPGroupBox groupBoxQuality;
    private MPGroupBox groupBoxBitrate;
    private MPGroupBox groupBoxTarget;
    private MPGroupBox groupBoxGeneralSettings;
    private MPGroupBox groupBoxPerformance;
    private MPGroupBox groupBoxMissing;
    private HScrollBar hScrollBarQuality;
    private HScrollBar hScrollBarBitrate;
    private HScrollBar hScrollBarPriority;
    private MPLabel labelTarget;
    private MPLabel labelBitrate;
    private MPLabel labelLibraryFolder;
    private MPLabel labelFasterImport;
    private MPLabel labelBetterResponse;
    private MPLabel labelDisabled;
    private LinkLabel linkLabelLAME;
    private OpenFileDialog openFileDialog;
    private MPRadioButton radioButtonQuality;
    private MPRadioButton radioButtonBitrate;
    private MPTabControl tabControlMusicImport;
    private MPTabControl tabControlMissing;
    private MPTabPage tabPageMissing;
    private MPTabPage tabPageEncoderSettings;
    private MPTabPage tabPageFilenames;
    private MPTabPage tabPageImportSettings;
    private MPTextBox textBoxImportDir;
    private MPGroupBox groupBox2;
    private MPGroupBox groupBox1;
    private MPLabel label12;
    private MPLabel label10;
    private MPLabel label9;
    private MPLabel label11;
    private MPTextBox textBoxSample;
    private MPLabel label7;
    private MPLabel label6;
    private MPTextBox textBoxFormat;
    private MPLabel label38;
    private MPCheckBox checkBoxUnknown;

    private Preset[] Presets = new Preset[7];
    private const string Mpeg1BitRates = "128,160,192,224,256,320";
    private string[] Rates;
    private string LameDir;

    /// <summary>
    /// 
    /// </summary>
    public MusicImport()
      : this("Music Import") {}

    /// <summary>
    /// 
    /// </summary>
    public MusicImport(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      groupBoxQuality.Location = groupBoxBitrate.Location;
      groupBoxMissing.Location = groupBoxGeneralSettings.Location;
      Rates = Mpeg1BitRates.Split(',');

      Presets[0] = new Preset("245,220,260");
      Presets[1] = new Preset("225,200,250");
      Presets[2] = new Preset("190,170,210");
      Presets[3] = new Preset("175,155,195");
      Presets[4] = new Preset("165,145,185");
      Presets[5] = new Preset("130,110,150");
      Presets[6] = new Preset("115,95,135");

      if (!File.Exists("lame_enc.dll"))
      {
        tabControlMissing.Visible = true;
        tabControlMusicImport.Visible = false;
        buttonDefault.Visible = false;
      }
      else
      {
        tabControlMissing.Visible = false;
        tabControlMusicImport.Visible = true;
        buttonDefault.Visible = true;
      }
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        checkBoxReplace.Checked = xmlreader.GetValueAsBool("musicimport", "mp3replaceexisting", false);
        checkBoxMono.Checked = xmlreader.GetValueAsBool("musicimport", "mp3mono", false);
        checkBoxCBR.Checked = xmlreader.GetValueAsBool("musicimport", "mp3cbr", false);
        checkBoxDatabase.Checked = xmlreader.GetValueAsBool("musicimport", "mp3database", true);
        checkBoxBackground.Checked = xmlreader.GetValueAsBool("musicimport", "mp3background", false);
        hScrollBarPriority.Value = xmlreader.GetValueAsInt("musicimport", "mp3priority", 0);
        hScrollBarBitrate.Value = xmlreader.GetValueAsInt("musicimport", "mp3bitrate", 2);
        hScrollBarQuality.Value = xmlreader.GetValueAsInt("musicimport", "mp3quality", 2);
        radioButtonQuality.Checked = xmlreader.GetValueAsBool("musicimport", "mp3vbr", true);
        radioButtonBitrate.Checked = !radioButtonQuality.Checked;
        textBoxImportDir.Text = xmlreader.GetValueAsString("musicimport", "mp3importdir", "C:");
        checkBoxFastMode.Checked = xmlreader.GetValueAsBool("musicimport", "mp3fastmode", false);
        labelTarget.Text = "Bitrate: " + Presets[hScrollBarQuality.Value].Target + " kBps (" +
                           Presets[hScrollBarQuality.Value].Minimum + "..." + Presets[hScrollBarQuality.Value].Maximum +
                           ")";
        labelBitrate.Text = "Target Bitrate: " + Rates[hScrollBarBitrate.Value] + " kBps";
        textBoxFormat.Text = xmlreader.GetValueAsString("musicimport", "format", "%artist%\\%album%\\%track% %title%");
        checkBoxUnknown.Enabled = checkBoxDatabase.Checked;
        checkBoxUnknown.Checked = !xmlreader.GetValueAsBool("musicimport", "importunknown", false);
      }
      textBoxSample.Text = ShowExample(textBoxFormat.Text);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("musicimport", "mp3importdir", textBoxImportDir.Text);
        xmlwriter.SetValue("musicimport", "mp3priority", hScrollBarPriority.Value);
        xmlwriter.SetValue("musicimport", "mp3bitrate", hScrollBarBitrate.Value);
        xmlwriter.SetValue("musicimport", "mp3quality", hScrollBarQuality.Value);
        xmlwriter.SetValueAsBool("musicimport", "mp3replaceexisting", checkBoxReplace.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3vbr", radioButtonQuality.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3mono", checkBoxMono.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3cbr", checkBoxCBR.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3fastmode", checkBoxFastMode.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3database", checkBoxDatabase.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3background", checkBoxBackground.Checked);
        xmlwriter.SetValueAsBool("musicimport", "importunknown", !checkBoxUnknown.Checked);
        if (textBoxFormat.Text != string.Empty)
        {
          xmlwriter.SetValue("musicimport", "format", textBoxFormat.Text);
        }
        else
        {
          xmlwriter.SetValue("musicimport", "format", "%artist%\\%album%\\%track% %title%");
        }
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
        new System.ComponentModel.ComponentResourceManager(typeof (MusicImport));
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.buttonDefault = new MediaPortal.UserInterface.Controls.MPButton();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.tabPageEncoderSettings = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxQuality = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxFastMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.labelTarget = new MediaPortal.UserInterface.Controls.MPLabel();
      this.hScrollBarQuality = new System.Windows.Forms.HScrollBar();
      this.groupBoxBitrate = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelBitrate = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxCBR = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.hScrollBarBitrate = new System.Windows.Forms.HScrollBar();
      this.groupBoxTarget = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxMono = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButtonQuality = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonBitrate = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.tabPageImportSettings = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxGeneralSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxUnknown = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxBackground = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxDatabase = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxReplace = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.labelLibraryFolder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonBrowse = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxImportDir = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBoxPerformance = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.hScrollBarPriority = new System.Windows.Forms.HScrollBar();
      this.labelFasterImport = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelBetterResponse = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControlMusicImport = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageFilenames = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxSample = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxFormat = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label38 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControlMissing = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageMissing = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBoxMissing = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonLocateLAME = new MediaPortal.UserInterface.Controls.MPButton();
      this.linkLabelLAME = new System.Windows.Forms.LinkLabel();
      this.labelDisabled = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageEncoderSettings.SuspendLayout();
      this.groupBoxQuality.SuspendLayout();
      this.groupBoxBitrate.SuspendLayout();
      this.groupBoxTarget.SuspendLayout();
      this.tabPageImportSettings.SuspendLayout();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.groupBoxPerformance.SuspendLayout();
      this.tabControlMusicImport.SuspendLayout();
      this.tabPageFilenames.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabControlMissing.SuspendLayout();
      this.tabPageMissing.SuspendLayout();
      this.groupBoxMissing.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonDefault
      // 
      this.buttonDefault.AutoSize = true;
      this.buttonDefault.Location = new System.Drawing.Point(352, 368);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(104, 23);
      this.buttonDefault.TabIndex = 14;
      this.buttonDefault.Text = "Reset to default";
      this.buttonDefault.UseVisualStyleBackColor = true;
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // openFileDialog
      // 
      this.openFileDialog.FileName = "openFileDialog";
      // 
      // tabPageEncoderSettings
      // 
      this.tabPageEncoderSettings.BackgroundImage =
        ((System.Drawing.Image)(resources.GetObject("tabPageEncoderSettings.BackgroundImage")));
      this.tabPageEncoderSettings.Controls.Add(this.groupBoxQuality);
      this.tabPageEncoderSettings.Controls.Add(this.groupBoxBitrate);
      this.tabPageEncoderSettings.Controls.Add(this.groupBoxTarget);
      this.tabPageEncoderSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageEncoderSettings.Name = "tabPageEncoderSettings";
      this.tabPageEncoderSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageEncoderSettings.Size = new System.Drawing.Size(464, 374);
      this.tabPageEncoderSettings.TabIndex = 2;
      this.tabPageEncoderSettings.Text = "Encoder Settings";
      this.tabPageEncoderSettings.UseVisualStyleBackColor = true;
      // 
      // groupBoxQuality
      // 
      this.groupBoxQuality.BackColor = System.Drawing.Color.Transparent;
      this.groupBoxQuality.Controls.Add(this.checkBoxFastMode);
      this.groupBoxQuality.Controls.Add(this.labelTarget);
      this.groupBoxQuality.Controls.Add(this.hScrollBarQuality);
      this.groupBoxQuality.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxQuality.Location = new System.Drawing.Point(160, 128);
      this.groupBoxQuality.Name = "groupBoxQuality";
      this.groupBoxQuality.Size = new System.Drawing.Size(288, 104);
      this.groupBoxQuality.TabIndex = 3;
      this.groupBoxQuality.TabStop = false;
      this.groupBoxQuality.Text = "Quality";
      this.groupBoxQuality.Visible = false;
      // 
      // checkBoxFastMode
      // 
      this.checkBoxFastMode.AutoSize = true;
      this.checkBoxFastMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxFastMode.Location = new System.Drawing.Point(16, 72);
      this.checkBoxFastMode.Name = "checkBoxFastMode";
      this.checkBoxFastMode.Size = new System.Drawing.Size(133, 17);
      this.checkBoxFastMode.TabIndex = 3;
      this.checkBoxFastMode.Text = "Fast mode (less quality)";
      this.checkBoxFastMode.UseVisualStyleBackColor = true;
      // 
      // labelTarget
      // 
      this.labelTarget.AutoSize = true;
      this.labelTarget.Location = new System.Drawing.Point(73, 50);
      this.labelTarget.Name = "labelTarget";
      this.labelTarget.Size = new System.Drawing.Size(142, 13);
      this.labelTarget.TabIndex = 2;
      this.labelTarget.Text = "Bitrate: 000 kBps (000...000)";
      this.labelTarget.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // hScrollBarQuality
      // 
      this.hScrollBarQuality.LargeChange = 1;
      this.hScrollBarQuality.Location = new System.Drawing.Point(16, 24);
      this.hScrollBarQuality.Maximum = 6;
      this.hScrollBarQuality.Name = "hScrollBarQuality";
      this.hScrollBarQuality.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.hScrollBarQuality.Size = new System.Drawing.Size(256, 17);
      this.hScrollBarQuality.TabIndex = 1;
      this.hScrollBarQuality.Value = 2;
      this.hScrollBarQuality.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBarQuality_Scroll);
      // 
      // groupBoxBitrate
      // 
      this.groupBoxBitrate.Controls.Add(this.labelBitrate);
      this.groupBoxBitrate.Controls.Add(this.checkBoxCBR);
      this.groupBoxBitrate.Controls.Add(this.hScrollBarBitrate);
      this.groupBoxBitrate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxBitrate.Location = new System.Drawing.Point(160, 16);
      this.groupBoxBitrate.Name = "groupBoxBitrate";
      this.groupBoxBitrate.Size = new System.Drawing.Size(288, 104);
      this.groupBoxBitrate.TabIndex = 2;
      this.groupBoxBitrate.TabStop = false;
      this.groupBoxBitrate.Text = "Bitrate";
      // 
      // labelBitrate
      // 
      this.labelBitrate.AutoSize = true;
      this.labelBitrate.Location = new System.Drawing.Point(83, 50);
      this.labelBitrate.Name = "labelBitrate";
      this.labelBitrate.Size = new System.Drawing.Size(122, 13);
      this.labelBitrate.TabIndex = 2;
      this.labelBitrate.Text = "Target Bitrate: 000 kBps";
      this.labelBitrate.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // checkBoxCBR
      // 
      this.checkBoxCBR.AutoSize = true;
      this.checkBoxCBR.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxCBR.Location = new System.Drawing.Point(16, 72);
      this.checkBoxCBR.Name = "checkBoxCBR";
      this.checkBoxCBR.Size = new System.Drawing.Size(242, 17);
      this.checkBoxCBR.TabIndex = 1;
      this.checkBoxCBR.Text = "Restrict to constant bitrate (not recommended)";
      this.checkBoxCBR.UseVisualStyleBackColor = true;
      // 
      // hScrollBarBitrate
      // 
      this.hScrollBarBitrate.LargeChange = 1;
      this.hScrollBarBitrate.Location = new System.Drawing.Point(16, 24);
      this.hScrollBarBitrate.Maximum = 5;
      this.hScrollBarBitrate.Name = "hScrollBarBitrate";
      this.hScrollBarBitrate.Size = new System.Drawing.Size(256, 17);
      this.hScrollBarBitrate.TabIndex = 0;
      this.hScrollBarBitrate.TabStop = true;
      this.hScrollBarBitrate.Value = 2;
      this.hScrollBarBitrate.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBarBitrate_Scroll);
      // 
      // groupBoxTarget
      // 
      this.groupBoxTarget.Controls.Add(this.checkBoxMono);
      this.groupBoxTarget.Controls.Add(this.radioButtonQuality);
      this.groupBoxTarget.Controls.Add(this.radioButtonBitrate);
      this.groupBoxTarget.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxTarget.Location = new System.Drawing.Point(16, 16);
      this.groupBoxTarget.Name = "groupBoxTarget";
      this.groupBoxTarget.Size = new System.Drawing.Size(128, 104);
      this.groupBoxTarget.TabIndex = 0;
      this.groupBoxTarget.TabStop = false;
      this.groupBoxTarget.Text = "Encoding Mode";
      // 
      // checkBoxMono
      // 
      this.checkBoxMono.AutoSize = true;
      this.checkBoxMono.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxMono.Location = new System.Drawing.Point(20, 72);
      this.checkBoxMono.Name = "checkBoxMono";
      this.checkBoxMono.Size = new System.Drawing.Size(98, 17);
      this.checkBoxMono.TabIndex = 2;
      this.checkBoxMono.Text = "Mono encoding";
      this.checkBoxMono.UseVisualStyleBackColor = true;
      // 
      // radioButtonQuality
      // 
      this.radioButtonQuality.AutoSize = true;
      this.radioButtonQuality.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonQuality.Location = new System.Drawing.Point(20, 48);
      this.radioButtonQuality.Name = "radioButtonQuality";
      this.radioButtonQuality.Size = new System.Drawing.Size(56, 17);
      this.radioButtonQuality.TabIndex = 1;
      this.radioButtonQuality.TabStop = true;
      this.radioButtonQuality.Text = "Quality";
      this.radioButtonQuality.UseVisualStyleBackColor = true;
      this.radioButtonQuality.CheckedChanged += new System.EventHandler(this.radioButtonQuality_CheckedChanged);
      // 
      // radioButtonBitrate
      // 
      this.radioButtonBitrate.AutoSize = true;
      this.radioButtonBitrate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonBitrate.Location = new System.Drawing.Point(20, 24);
      this.radioButtonBitrate.Name = "radioButtonBitrate";
      this.radioButtonBitrate.Size = new System.Drawing.Size(54, 17);
      this.radioButtonBitrate.TabIndex = 0;
      this.radioButtonBitrate.TabStop = true;
      this.radioButtonBitrate.Text = "Bitrate";
      this.radioButtonBitrate.UseVisualStyleBackColor = true;
      this.radioButtonBitrate.CheckedChanged += new System.EventHandler(this.radioButtonBitrate_CheckedChanged);
      // 
      // tabPageImportSettings
      // 
      this.tabPageImportSettings.Controls.Add(this.groupBoxGeneralSettings);
      this.tabPageImportSettings.Controls.Add(this.groupBoxPerformance);
      this.tabPageImportSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageImportSettings.Name = "tabPageImportSettings";
      this.tabPageImportSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageImportSettings.Size = new System.Drawing.Size(464, 374);
      this.tabPageImportSettings.TabIndex = 0;
      this.tabPageImportSettings.Text = "Import Settings";
      this.tabPageImportSettings.UseVisualStyleBackColor = true;
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxUnknown);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxBackground);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxDatabase);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxReplace);
      this.groupBoxGeneralSettings.Controls.Add(this.labelLibraryFolder);
      this.groupBoxGeneralSettings.Controls.Add(this.buttonBrowse);
      this.groupBoxGeneralSettings.Controls.Add(this.textBoxImportDir);
      this.groupBoxGeneralSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(16, 16);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(432, 160);
      this.groupBoxGeneralSettings.TabIndex = 16;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General Settings";
      // 
      // checkBoxUnknown
      // 
      this.checkBoxUnknown.AutoSize = true;
      this.checkBoxUnknown.Checked = true;
      this.checkBoxUnknown.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUnknown.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUnknown.Location = new System.Drawing.Point(20, 72);
      this.checkBoxUnknown.Name = "checkBoxUnknown";
      this.checkBoxUnknown.Size = new System.Drawing.Size(218, 17);
      this.checkBoxUnknown.TabIndex = 13;
      this.checkBoxUnknown.Text = "Don\'t import unknown tracks to database";
      this.checkBoxUnknown.UseVisualStyleBackColor = true;
      // 
      // checkBoxBackground
      // 
      this.checkBoxBackground.AutoSize = true;
      this.checkBoxBackground.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxBackground.Location = new System.Drawing.Point(20, 96);
      this.checkBoxBackground.Name = "checkBoxBackground";
      this.checkBoxBackground.Size = new System.Drawing.Size(146, 17);
      this.checkBoxBackground.TabIndex = 12;
      this.checkBoxBackground.Text = "Background import (silent)";
      this.checkBoxBackground.UseVisualStyleBackColor = true;
      // 
      // checkBoxDatabase
      // 
      this.checkBoxDatabase.AutoSize = true;
      this.checkBoxDatabase.Checked = true;
      this.checkBoxDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDatabase.Location = new System.Drawing.Point(20, 48);
      this.checkBoxDatabase.Name = "checkBoxDatabase";
      this.checkBoxDatabase.Size = new System.Drawing.Size(120, 17);
      this.checkBoxDatabase.TabIndex = 11;
      this.checkBoxDatabase.Text = "Import into database";
      this.checkBoxDatabase.UseVisualStyleBackColor = true;
      this.checkBoxDatabase.CheckedChanged += new System.EventHandler(this.checkBoxDatabase_CheckedChanged);
      // 
      // checkBoxReplace
      // 
      this.checkBoxReplace.AutoSize = true;
      this.checkBoxReplace.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxReplace.Location = new System.Drawing.Point(20, 24);
      this.checkBoxReplace.Name = "checkBoxReplace";
      this.checkBoxReplace.Size = new System.Drawing.Size(123, 17);
      this.checkBoxReplace.TabIndex = 9;
      this.checkBoxReplace.Text = "Replace existing files";
      this.checkBoxReplace.UseVisualStyleBackColor = true;
      // 
      // labelLibraryFolder
      // 
      this.labelLibraryFolder.AutoSize = true;
      this.labelLibraryFolder.Location = new System.Drawing.Point(16, 128);
      this.labelLibraryFolder.Name = "labelLibraryFolder";
      this.labelLibraryFolder.Size = new System.Drawing.Size(73, 13);
      this.labelLibraryFolder.TabIndex = 7;
      this.labelLibraryFolder.Text = "Library Folder:";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(344, 123);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(72, 22);
      this.buttonBrowse.TabIndex = 6;
      this.buttonBrowse.Text = "Browse";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // textBoxImportDir
      // 
      this.textBoxImportDir.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxImportDir.BorderColor = System.Drawing.Color.Empty;
      this.textBoxImportDir.Location = new System.Drawing.Point(96, 124);
      this.textBoxImportDir.Name = "textBoxImportDir";
      this.textBoxImportDir.Size = new System.Drawing.Size(240, 20);
      this.textBoxImportDir.TabIndex = 5;
      // 
      // groupBoxPerformance
      // 
      this.groupBoxPerformance.Controls.Add(this.hScrollBarPriority);
      this.groupBoxPerformance.Controls.Add(this.labelFasterImport);
      this.groupBoxPerformance.Controls.Add(this.labelBetterResponse);
      this.groupBoxPerformance.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxPerformance.Location = new System.Drawing.Point(16, 184);
      this.groupBoxPerformance.Name = "groupBoxPerformance";
      this.groupBoxPerformance.Size = new System.Drawing.Size(432, 56);
      this.groupBoxPerformance.TabIndex = 8;
      this.groupBoxPerformance.TabStop = false;
      this.groupBoxPerformance.Text = "Performance";
      // 
      // hScrollBarPriority
      // 
      this.hScrollBarPriority.LargeChange = 1;
      this.hScrollBarPriority.Location = new System.Drawing.Point(104, 22);
      this.hScrollBarPriority.Maximum = 4;
      this.hScrollBarPriority.Name = "hScrollBarPriority";
      this.hScrollBarPriority.Size = new System.Drawing.Size(240, 17);
      this.hScrollBarPriority.TabIndex = 18;
      // 
      // labelFasterImport
      // 
      this.labelFasterImport.AutoSize = true;
      this.labelFasterImport.Location = new System.Drawing.Point(351, 24);
      this.labelFasterImport.Name = "labelFasterImport";
      this.labelFasterImport.Size = new System.Drawing.Size(67, 13);
      this.labelFasterImport.TabIndex = 2;
      this.labelFasterImport.Text = "Faster import";
      // 
      // labelBetterResponse
      // 
      this.labelBetterResponse.AutoSize = true;
      this.labelBetterResponse.Location = new System.Drawing.Point(16, 24);
      this.labelBetterResponse.Name = "labelBetterResponse";
      this.labelBetterResponse.Size = new System.Drawing.Size(81, 13);
      this.labelBetterResponse.TabIndex = 1;
      this.labelBetterResponse.Text = "Better response";
      // 
      // tabControlMusicImport
      // 
      this.tabControlMusicImport.Controls.Add(this.tabPageImportSettings);
      this.tabControlMusicImport.Controls.Add(this.tabPageFilenames);
      this.tabControlMusicImport.Controls.Add(this.tabPageEncoderSettings);
      this.tabControlMusicImport.Location = new System.Drawing.Point(0, 8);
      this.tabControlMusicImport.Name = "tabControlMusicImport";
      this.tabControlMusicImport.SelectedIndex = 0;
      this.tabControlMusicImport.Size = new System.Drawing.Size(472, 400);
      this.tabControlMusicImport.TabIndex = 0;
      // 
      // tabPageFilenames
      // 
      this.tabPageFilenames.Controls.Add(this.groupBox2);
      this.tabPageFilenames.Location = new System.Drawing.Point(4, 22);
      this.tabPageFilenames.Name = "tabPageFilenames";
      this.tabPageFilenames.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageFilenames.Size = new System.Drawing.Size(464, 374);
      this.tabPageFilenames.TabIndex = 3;
      this.tabPageFilenames.Text = "Custom Paths and Filenames";
      this.tabPageFilenames.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.groupBox1);
      this.groupBox2.Controls.Add(this.textBoxSample);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.textBoxFormat);
      this.groupBox2.Controls.Add(this.label38);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(16, 16);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(432, 212);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label12);
      this.groupBox1.Controls.Add(this.label10);
      this.groupBox1.Controls.Add(this.label9);
      this.groupBox1.Controls.Add(this.label11);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(80, 116);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(336, 80);
      this.groupBox1.TabIndex = 21;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Available Tags";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(216, 24);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(104, 39);
      this.label12.TabIndex = 15;
      this.label12.Text = "tracknumber of song\r\nyear of song\r\ngenre of song";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(74, 24);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(76, 39);
      this.label10.TabIndex = 13;
      this.label10.Text = "name of artist\r\nsong title\r\nname of album";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(9, 24);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(60, 52);
      this.label9.TabIndex = 12;
      this.label9.Text = "%artist% =\r\n%title% =\r\n%album% =\r\n\r\n";
      this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(153, 24);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(59, 39);
      this.label11.TabIndex = 14;
      this.label11.Text = "%track% =\r\n%year% =\r\n%genre% =\r\n";
      this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxSample
      // 
      this.textBoxSample.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxSample.BackColor = System.Drawing.SystemColors.ControlLight;
      this.textBoxSample.BorderColor = System.Drawing.Color.Empty;
      this.textBoxSample.Location = new System.Drawing.Point(80, 76);
      this.textBoxSample.Name = "textBoxSample";
      this.textBoxSample.ReadOnly = true;
      this.textBoxSample.Size = new System.Drawing.Size(336, 20);
      this.textBoxSample.TabIndex = 19;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(80, 24);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(327, 13);
      this.label7.TabIndex = 0;
      this.label7.Text = "Use blockquotes [ ] to specify optional fields and \\ for relative paths.";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(16, 80);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(45, 13);
      this.label6.TabIndex = 5;
      this.label6.Text = "Sample:";
      // 
      // textBoxFormat
      // 
      this.textBoxFormat.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFormat.BorderColor = System.Drawing.Color.Empty;
      this.textBoxFormat.Location = new System.Drawing.Point(80, 48);
      this.textBoxFormat.Name = "textBoxFormat";
      this.textBoxFormat.Size = new System.Drawing.Size(336, 20);
      this.textBoxFormat.TabIndex = 4;
      this.textBoxFormat.TextChanged += new System.EventHandler(this.textBoxFormat_TextChanged);
      this.textBoxFormat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFormat_KeyPress);
      // 
      // label38
      // 
      this.label38.AutoSize = true;
      this.label38.Location = new System.Drawing.Point(16, 52);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(42, 13);
      this.label38.TabIndex = 3;
      this.label38.Text = "Format:";
      // 
      // tabControlMissing
      // 
      this.tabControlMissing.Controls.Add(this.tabPageMissing);
      this.tabControlMissing.Location = new System.Drawing.Point(0, 8);
      this.tabControlMissing.Name = "tabControlMissing";
      this.tabControlMissing.SelectedIndex = 0;
      this.tabControlMissing.Size = new System.Drawing.Size(472, 400);
      this.tabControlMissing.TabIndex = 15;
      // 
      // tabPageMissing
      // 
      this.tabPageMissing.BackgroundImage =
        ((System.Drawing.Image)(resources.GetObject("tabPageMissing.BackgroundImage")));
      this.tabPageMissing.Controls.Add(this.groupBoxMissing);
      this.tabPageMissing.Location = new System.Drawing.Point(4, 22);
      this.tabPageMissing.Name = "tabPageMissing";
      this.tabPageMissing.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageMissing.Size = new System.Drawing.Size(464, 374);
      this.tabPageMissing.TabIndex = 0;
      this.tabPageMissing.Text = "Missing File";
      this.tabPageMissing.UseVisualStyleBackColor = true;
      // 
      // groupBoxMissing
      // 
      this.groupBoxMissing.BackColor = System.Drawing.Color.Transparent;
      this.groupBoxMissing.Controls.Add(this.buttonLocateLAME);
      this.groupBoxMissing.Controls.Add(this.linkLabelLAME);
      this.groupBoxMissing.Controls.Add(this.labelDisabled);
      this.groupBoxMissing.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMissing.Location = new System.Drawing.Point(16, 16);
      this.groupBoxMissing.Name = "groupBoxMissing";
      this.groupBoxMissing.Size = new System.Drawing.Size(432, 208);
      this.groupBoxMissing.TabIndex = 20;
      this.groupBoxMissing.TabStop = false;
      // 
      // buttonLocateLAME
      // 
      this.buttonLocateLAME.Location = new System.Drawing.Point(312, 168);
      this.buttonLocateLAME.Name = "buttonLocateLAME";
      this.buttonLocateLAME.Size = new System.Drawing.Size(104, 23);
      this.buttonLocateLAME.TabIndex = 2;
      this.buttonLocateLAME.Text = "Locate LAME";
      this.buttonLocateLAME.UseVisualStyleBackColor = true;
      this.buttonLocateLAME.Click += new System.EventHandler(this.buttonLocateLAME_Click);
      // 
      // linkLabelLAME
      // 
      this.linkLabelLAME.AutoSize = true;
      this.linkLabelLAME.Location = new System.Drawing.Point(120, 99);
      this.linkLabelLAME.Name = "linkLabelLAME";
      this.linkLabelLAME.Size = new System.Drawing.Size(207, 13);
      this.linkLabelLAME.TabIndex = 1;
      this.linkLabelLAME.TabStop = true;
      this.linkLabelLAME.Text = "http://mitiok.maresweb.org/lame-3.97b.zip";
      this.linkLabelLAME.LinkClicked +=
        new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLAME_LinkClicked);
      // 
      // labelDisabled
      // 
      this.labelDisabled.AutoSize = true;
      this.labelDisabled.Location = new System.Drawing.Point(16, 24);
      this.labelDisabled.Name = "labelDisabled";
      this.labelDisabled.Size = new System.Drawing.Size(373, 130);
      this.labelDisabled.TabIndex = 0;
      this.labelDisabled.Text = resources.GetString("labelDisabled.Text");
      // 
      // MusicImport
      // 
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.Controls.Add(this.tabControlMusicImport);
      this.Controls.Add(this.buttonDefault);
      this.Controls.Add(this.tabControlMissing);
      this.DoubleBuffered = true;
      this.Name = "MusicImport";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabPageEncoderSettings.ResumeLayout(false);
      this.groupBoxQuality.ResumeLayout(false);
      this.groupBoxQuality.PerformLayout();
      this.groupBoxBitrate.ResumeLayout(false);
      this.groupBoxBitrate.PerformLayout();
      this.groupBoxTarget.ResumeLayout(false);
      this.groupBoxTarget.PerformLayout();
      this.tabPageImportSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.groupBoxPerformance.ResumeLayout(false);
      this.groupBoxPerformance.PerformLayout();
      this.tabControlMusicImport.ResumeLayout(false);
      this.tabPageFilenames.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabControlMissing.ResumeLayout(false);
      this.tabPageMissing.ResumeLayout(false);
      this.groupBoxMissing.ResumeLayout(false);
      this.groupBoxMissing.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void buttonDefault_Click(object sender, EventArgs e)
    {
      checkBoxReplace.Checked = true;
      hScrollBarPriority.Value = 0;
      radioButtonQuality.Checked = true;
      radioButtonBitrate.Checked = !radioButtonQuality.Checked;
      checkBoxMono.Checked = false;
      hScrollBarBitrate.Value = 2;
      checkBoxCBR.Checked = false;
      hScrollBarQuality.Value = 2;
      checkBoxFastMode.Checked = false;
      checkBoxDatabase.Checked = true;
      checkBoxBackground.Checked = false;
      labelBitrate.Text = "Target Bitrate: " + Rates[hScrollBarBitrate.Value] + " kBps";
      labelTarget.Text = "Bitrate: " + Presets[hScrollBarQuality.Value].Target + " kBps (" +
                         Presets[hScrollBarQuality.Value].Minimum + "..." + Presets[hScrollBarQuality.Value].Maximum +
                         ")";
    }

    /// <summary>
    /// 
    /// </summary>
    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      using (folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the folder where imported music files will be stored";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = textBoxImportDir.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          textBoxImportDir.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    private void radioButtonBitrate_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxBitrate.Visible = true;
      groupBoxQuality.Visible = false;
    }

    private void radioButtonQuality_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxQuality.Visible = true;
      groupBoxBitrate.Visible = false;
    }

    private void hScrollBarBitrate_Scroll(object sender, ScrollEventArgs e)
    {
      labelBitrate.Text = "Target Bitrate: " + Rates[hScrollBarBitrate.Value] + " kBps";
    }

    private void hScrollBarQuality_Scroll(object sender, ScrollEventArgs e)
    {
      labelTarget.Text = "Bitrate: " + Presets[hScrollBarQuality.Value].Target + " kBps (" +
                         Presets[hScrollBarQuality.Value].Minimum + "..." + Presets[hScrollBarQuality.Value].Maximum +
                         ")";
    }

    private void linkLabelLAME_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelLAME.Text == null)
      {
        return;
      }
      if (linkLabelLAME.Text.Length > 0)
      {
        ProcessStartInfo sInfo = new ProcessStartInfo(linkLabelLAME.Text);
        Process.Start(sInfo);
      }
    }

    private void buttonLocateLAME_Click(object sender, EventArgs e)
    {
      string currDir = Directory.GetCurrentDirectory();
      using (openFileDialog = new OpenFileDialog())
      {
        openFileDialog.Title = "Select the file lame_enc.dll";
        openFileDialog.FileName = "lame_enc.dll";
        openFileDialog.Filter = "LAME encoder DLL|lame_enc.dll";
        DialogResult dialogResult = openFileDialog.ShowDialog(this);
        if (dialogResult == DialogResult.OK)
        {
          LameDir = openFileDialog.FileName;
          try
          {
            File.Copy(LameDir, currDir + "/lame_enc.dll");

            if (!File.Exists("lame_enc.dll"))
            {
              tabControlMissing.Visible = true;
              tabControlMusicImport.Visible = false;
              buttonDefault.Visible = false;
            }
            else
            {
              tabControlMissing.Visible = false;
              tabControlMusicImport.Visible = true;
              buttonDefault.Visible = true;
            }
          }
          catch {}
        }
      }
      Directory.SetCurrentDirectory(currDir);
    }

    private string ShowExample(string strInput)
    {
      string artist = "Queen";
      string album = "Greatest Hits";
      string title = "Barcelona";
      string trackNr = "03";
      string year = "1973";
      string genre = "Pop";
      string strName = string.Empty;
      string strDirectory = string.Empty;
      string strDefault = "%artist%\\%album%\\%track% %title%";

      if (strInput == string.Empty)
      {
        strInput = strDefault;
      }
      strInput = Util.Utils.ReplaceTag(strInput, "%artist%", artist, "unknown");
      strInput = Util.Utils.ReplaceTag(strInput, "%title%", title, "unknown");
      strInput = Util.Utils.ReplaceTag(strInput, "%album%", album, "unknown");
      strInput = Util.Utils.ReplaceTag(strInput, "%track%", trackNr, "unknown");
      strInput = Util.Utils.ReplaceTag(strInput, "%year%", year, "unknown");
      strInput = Util.Utils.ReplaceTag(strInput, "%genre%", genre, "unknown");

      int index = strInput.LastIndexOf('\\');
      switch (index)
      {
        case -1:
          strName = strInput;
          break;
        case 0:
          strName = strInput.Substring(1);
          break;
        default:
          {
            strDirectory = "\\" + strInput.Substring(0, index);
            strName = strInput.Substring(index + 1);
          }
          break;
      }
      strDirectory = Util.Utils.MakeDirectoryPath(strDirectory);
      strName = Util.Utils.MakeFileName(strName);
      string strReturn = strDirectory;
      if (strDirectory != string.Empty)
      {
        strReturn += "\\";
      }
      strReturn += strName + ".mp3";
      return strReturn;
    }

    private void textBoxFormat_TextChanged(object sender, EventArgs e)
    {
      textBoxSample.Text = ShowExample(textBoxFormat.Text);
    }

    private void checkBoxDatabase_CheckedChanged(object sender, EventArgs e)
    {
      checkBoxUnknown.Enabled = checkBoxDatabase.Checked;
    }

    private void textBoxFormat_KeyPress(object sender, KeyPressEventArgs e)
    {
      if ((e.KeyChar == '/') || (e.KeyChar == ':') || (e.KeyChar == '*') ||
          (e.KeyChar == '?') || (e.KeyChar == '\"') || (e.KeyChar == '<') ||
          (e.KeyChar == '>') || (e.KeyChar == '|'))
      {
        e.Handled = true;
      }
    }
  }
}