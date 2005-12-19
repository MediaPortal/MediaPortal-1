/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Yeti.MMedia;
using Yeti.MMedia.Mp3;
using WaveLib;
using Yeti.Lame;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
  public class MusicImport : MediaPortal.Configuration.SectionSettings
  {
    private class Preset
    {
      int target;
      int minimum;
      int maximum;

      public int Target { get { return target; } }
      public int Minimum { get { return minimum; } }
      public int Maximum { get { return maximum; } }

      public Preset(string presetString)
      {
        target = Convert.ToInt16(presetString.Split(',')[0]);
        minimum = Convert.ToInt16(presetString.Split(',')[1]);
        maximum = Convert.ToInt16(presetString.Split(',')[2]);
      }
    }

    private System.ComponentModel.IContainer components = null;
    private Button buttonDefault;
    private Button buttonBrowse;
    private Button buttonLocateLAME;
    private CheckBox checkBoxFastMode;
    private CheckBox checkBoxCBR;
    private CheckBox checkBoxDatabase;
    private CheckBox checkBoxOrganize;
    private CheckBox checkBoxReplace;
    private CheckBox checkBoxMono;
    private CheckBox checkBoxBackground;
    private FolderBrowserDialog folderBrowserDialog;
    private GroupBox groupBoxQuality;
    private GroupBox groupBoxBitrate;
    private GroupBox groupBoxTarget;
    private GroupBox groupBoxGeneralSettings;
    private GroupBox groupBoxPerformance;
    private GroupBox groupBoxMissing;
    private HScrollBar hScrollBarQuality;
    private HScrollBar hScrollBarBitrate;
    private HScrollBar hScrollBarPriority;
    private Label labelTarget;
    private Label labelBitrate;
    private Label labelLibraryFolder;
    private Label labelFasterImport;
    private Label labelBetterResponse;
    private Label labelDisabled;
    private LinkLabel linkLabelLAME;
    private OpenFileDialog openFileDialog;
    private RadioButton radioButtonQuality;
    private RadioButton radioButtonBitrate;
    private TabControl tabControlMusicImport;
    private TabControl tabControlMissing;
    private TabPage tabPageMissing;
    private TabPage tabPageEncoderSettings;
    private TabPage tabPageImportSettings;
    private TextBox textBoxImportDir;

    private Preset[] Presets = new Preset[7];

    private const string Mpeg1BitRates = "128,160,192,224,256,320";
    private string[] Rates;
    private string LameDir;

    /// <summary>
    /// 
    /// </summary>
    public MusicImport()
      : this("Music Import")
    {
    }

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

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        checkBoxReplace.Checked = xmlreader.GetValueAsBool("musicimport", "mp3replaceexisting", true);
        checkBoxMono.Checked = xmlreader.GetValueAsBool("musicimport", "mp3mono", false);
        checkBoxCBR.Checked = xmlreader.GetValueAsBool("musicimport", "mp3cbr", false);
        checkBoxOrganize.Checked = xmlreader.GetValueAsBool("musicimport", "mp3organize", true);
        checkBoxDatabase.Checked = xmlreader.GetValueAsBool("musicimport", "mp3database", true);
        checkBoxBackground.Checked = xmlreader.GetValueAsBool("musicimport", "mp3background", false);
        hScrollBarPriority.Value = xmlreader.GetValueAsInt("musicimport", "mp3priority", 0) * 10;
        hScrollBarBitrate.Value = xmlreader.GetValueAsInt("musicimport", "mp3bitrate", 2);
        hScrollBarQuality.Value = xmlreader.GetValueAsInt("musicimport", "mp3quality", 2);
        radioButtonQuality.Checked = xmlreader.GetValueAsBool("musicimport", "mp3vbr", true);
        radioButtonBitrate.Checked = !radioButtonQuality.Checked;
        textBoxImportDir.Text = xmlreader.GetValueAsString("musicimport", "mp3importdir", "C:");
        checkBoxFastMode.Checked = xmlreader.GetValueAsBool("musicimport", "mp3fastmode", false);
        labelTarget.Text = "Bitrate: " + Presets[hScrollBarQuality.Value].Target + " kBps (" + Presets[hScrollBarQuality.Value].Minimum + "..." + Presets[hScrollBarQuality.Value].Maximum + ")";
        labelBitrate.Text = "Target Bitrate: " + Rates[hScrollBarBitrate.Value] + " kBps";
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("musicimport", "mp3importdir", textBoxImportDir.Text);
        xmlwriter.SetValue("musicimport", "mp3priority", (Math.Round((decimal)hScrollBarPriority.Value / 10)));
        xmlwriter.SetValue("musicimport", "mp3bitrate", hScrollBarBitrate.Value);
        xmlwriter.SetValue("musicimport", "mp3quality", hScrollBarQuality.Value);
        xmlwriter.SetValueAsBool("musicimport", "mp3replaceexisting", checkBoxReplace.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3vbr", radioButtonQuality.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3mono", checkBoxMono.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3cbr", checkBoxCBR.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3fastmode", checkBoxFastMode.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3organize", checkBoxOrganize.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3database", checkBoxDatabase.Checked);
        xmlwriter.SetValueAsBool("musicimport", "mp3background", checkBoxBackground.Checked);
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MusicImport));
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.buttonDefault = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.tabPageEncoderSettings = new System.Windows.Forms.TabPage();
      this.groupBoxQuality = new System.Windows.Forms.GroupBox();
      this.checkBoxFastMode = new System.Windows.Forms.CheckBox();
      this.labelTarget = new System.Windows.Forms.Label();
      this.hScrollBarQuality = new System.Windows.Forms.HScrollBar();
      this.groupBoxBitrate = new System.Windows.Forms.GroupBox();
      this.labelBitrate = new System.Windows.Forms.Label();
      this.checkBoxCBR = new System.Windows.Forms.CheckBox();
      this.hScrollBarBitrate = new System.Windows.Forms.HScrollBar();
      this.groupBoxTarget = new System.Windows.Forms.GroupBox();
      this.checkBoxMono = new System.Windows.Forms.CheckBox();
      this.radioButtonQuality = new System.Windows.Forms.RadioButton();
      this.radioButtonBitrate = new System.Windows.Forms.RadioButton();
      this.tabPageImportSettings = new System.Windows.Forms.TabPage();
      this.groupBoxGeneralSettings = new System.Windows.Forms.GroupBox();
      this.checkBoxBackground = new System.Windows.Forms.CheckBox();
      this.checkBoxDatabase = new System.Windows.Forms.CheckBox();
      this.checkBoxOrganize = new System.Windows.Forms.CheckBox();
      this.checkBoxReplace = new System.Windows.Forms.CheckBox();
      this.labelLibraryFolder = new System.Windows.Forms.Label();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.textBoxImportDir = new System.Windows.Forms.TextBox();
      this.groupBoxPerformance = new System.Windows.Forms.GroupBox();
      this.hScrollBarPriority = new System.Windows.Forms.HScrollBar();
      this.labelFasterImport = new System.Windows.Forms.Label();
      this.labelBetterResponse = new System.Windows.Forms.Label();
      this.tabControlMusicImport = new System.Windows.Forms.TabControl();
      this.tabControlMissing = new System.Windows.Forms.TabControl();
      this.tabPageMissing = new System.Windows.Forms.TabPage();
      this.groupBoxMissing = new System.Windows.Forms.GroupBox();
      this.buttonLocateLAME = new System.Windows.Forms.Button();
      this.linkLabelLAME = new System.Windows.Forms.LinkLabel();
      this.labelDisabled = new System.Windows.Forms.Label();
      this.tabPageEncoderSettings.SuspendLayout();
      this.groupBoxQuality.SuspendLayout();
      this.groupBoxBitrate.SuspendLayout();
      this.groupBoxTarget.SuspendLayout();
      this.tabPageImportSettings.SuspendLayout();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.groupBoxPerformance.SuspendLayout();
      this.tabControlMusicImport.SuspendLayout();
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
      this.tabPageEncoderSettings.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPageEncoderSettings.BackgroundImage")));
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
      this.groupBoxQuality.Controls.Add(this.checkBoxFastMode);
      this.groupBoxQuality.Controls.Add(this.labelTarget);
      this.groupBoxQuality.Controls.Add(this.hScrollBarQuality);
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
      this.checkBoxFastMode.Location = new System.Drawing.Point(16, 72);
      this.checkBoxFastMode.Name = "checkBoxFastMode";
      this.checkBoxFastMode.Size = new System.Drawing.Size(135, 17);
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
      this.checkBoxCBR.Location = new System.Drawing.Point(16, 72);
      this.checkBoxCBR.Name = "checkBoxCBR";
      this.checkBoxCBR.Size = new System.Drawing.Size(244, 17);
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
      this.checkBoxMono.Location = new System.Drawing.Point(20, 72);
      this.checkBoxMono.Name = "checkBoxMono";
      this.checkBoxMono.Size = new System.Drawing.Size(100, 17);
      this.checkBoxMono.TabIndex = 2;
      this.checkBoxMono.Text = "Mono encoding";
      this.checkBoxMono.UseVisualStyleBackColor = true;
      // 
      // radioButtonQuality
      // 
      this.radioButtonQuality.AutoSize = true;
      this.radioButtonQuality.Location = new System.Drawing.Point(20, 48);
      this.radioButtonQuality.Name = "radioButtonQuality";
      this.radioButtonQuality.Size = new System.Drawing.Size(57, 17);
      this.radioButtonQuality.TabIndex = 1;
      this.radioButtonQuality.TabStop = true;
      this.radioButtonQuality.Text = "Quality";
      this.radioButtonQuality.UseVisualStyleBackColor = true;
      this.radioButtonQuality.CheckedChanged += new System.EventHandler(this.radioButtonQuality_CheckedChanged);
      // 
      // radioButtonBitrate
      // 
      this.radioButtonBitrate.AutoSize = true;
      this.radioButtonBitrate.Location = new System.Drawing.Point(20, 24);
      this.radioButtonBitrate.Name = "radioButtonBitrate";
      this.radioButtonBitrate.Size = new System.Drawing.Size(55, 17);
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
      this.groupBoxGeneralSettings.BackColor = System.Drawing.Color.Transparent;
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxBackground);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxDatabase);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxOrganize);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxReplace);
      this.groupBoxGeneralSettings.Controls.Add(this.labelLibraryFolder);
      this.groupBoxGeneralSettings.Controls.Add(this.buttonBrowse);
      this.groupBoxGeneralSettings.Controls.Add(this.textBoxImportDir);
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(16, 16);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(432, 160);
      this.groupBoxGeneralSettings.TabIndex = 16;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General Settings";
      // 
      // checkBoxBackground
      // 
      this.checkBoxBackground.AutoSize = true;
      this.checkBoxBackground.Location = new System.Drawing.Point(20, 96);
      this.checkBoxBackground.Name = "checkBoxBackground";
      this.checkBoxBackground.Size = new System.Drawing.Size(148, 17);
      this.checkBoxBackground.TabIndex = 12;
      this.checkBoxBackground.Text = "Background import (silent)";
      this.checkBoxBackground.UseVisualStyleBackColor = true;
      // 
      // checkBoxDatabase
      // 
      this.checkBoxDatabase.AutoSize = true;
      this.checkBoxDatabase.Location = new System.Drawing.Point(20, 72);
      this.checkBoxDatabase.Name = "checkBoxDatabase";
      this.checkBoxDatabase.Size = new System.Drawing.Size(122, 17);
      this.checkBoxDatabase.TabIndex = 11;
      this.checkBoxDatabase.Text = "Import into database";
      this.checkBoxDatabase.UseVisualStyleBackColor = true;
      // 
      // checkBoxOrganize
      // 
      this.checkBoxOrganize.AutoSize = true;
      this.checkBoxOrganize.Location = new System.Drawing.Point(20, 48);
      this.checkBoxOrganize.Name = "checkBoxOrganize";
      this.checkBoxOrganize.Size = new System.Drawing.Size(99, 17);
      this.checkBoxOrganize.TabIndex = 10;
      this.checkBoxOrganize.Text = "Organize songs";
      this.checkBoxOrganize.UseVisualStyleBackColor = true;
      // 
      // checkBoxReplace
      // 
      this.checkBoxReplace.AutoSize = true;
      this.checkBoxReplace.Location = new System.Drawing.Point(20, 24);
      this.checkBoxReplace.Name = "checkBoxReplace";
      this.checkBoxReplace.Size = new System.Drawing.Size(125, 17);
      this.checkBoxReplace.TabIndex = 9;
      this.checkBoxReplace.Text = "Replace existing files";
      this.checkBoxReplace.UseVisualStyleBackColor = true;
      // 
      // labelLibraryFolder
      // 
      this.labelLibraryFolder.AutoSize = true;
      this.labelLibraryFolder.BackColor = System.Drawing.Color.Transparent;
      this.labelLibraryFolder.Location = new System.Drawing.Point(16, 128);
      this.labelLibraryFolder.Name = "labelLibraryFolder";
      this.labelLibraryFolder.Size = new System.Drawing.Size(73, 13);
      this.labelLibraryFolder.TabIndex = 7;
      this.labelLibraryFolder.Text = "Library Folder:";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(344, 123);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(72, 22);
      this.buttonBrowse.TabIndex = 6;
      this.buttonBrowse.Text = "Browse";
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // textBoxImportDir
      // 
      this.textBoxImportDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
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
      this.tabControlMusicImport.Controls.Add(this.tabPageEncoderSettings);
      this.tabControlMusicImport.Location = new System.Drawing.Point(0, 8);
      this.tabControlMusicImport.Name = "tabControlMusicImport";
      this.tabControlMusicImport.SelectedIndex = 0;
      this.tabControlMusicImport.Size = new System.Drawing.Size(472, 400);
      this.tabControlMusicImport.TabIndex = 0;
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
      this.tabPageMissing.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPageMissing.BackgroundImage")));
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
      this.groupBoxMissing.Controls.Add(this.buttonLocateLAME);
      this.groupBoxMissing.Controls.Add(this.linkLabelLAME);
      this.groupBoxMissing.Controls.Add(this.labelDisabled);
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
      this.Controls.Add(this.buttonDefault);
      this.Controls.Add(this.tabControlMissing);
      this.Controls.Add(this.tabControlMusicImport);
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
      checkBoxOrganize.Checked = true;
      checkBoxDatabase.Checked = true;
      checkBoxBackground.Checked = false;
      labelBitrate.Text = "Target Bitrate: " + Rates[hScrollBarBitrate.Value] + " kBps";
      labelTarget.Text = "Bitrate: " + Presets[hScrollBarQuality.Value].Target + " kBps (" + Presets[hScrollBarQuality.Value].Minimum + "..." + Presets[hScrollBarQuality.Value].Maximum + ")";
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
      labelTarget.Text = "Bitrate: " + Presets[hScrollBarQuality.Value].Target + " kBps (" + Presets[hScrollBarQuality.Value].Minimum + "..." + Presets[hScrollBarQuality.Value].Maximum + ")";
    }

    private void linkLabelLAME_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelLAME.Text == null)
        return;
      if (linkLabelLAME.Text.Length > 0)
      {
        System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(linkLabelLAME.Text);
        System.Diagnostics.Process.Start(sInfo);
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
          catch
          {
          }
        }
      }
      Directory.SetCurrentDirectory(currDir);
    }
  }
}