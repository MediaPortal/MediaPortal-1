/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using Yeti.MMedia;
using Yeti.MMedia.Mp3;
using WaveLib;
using Yeti.Lame;

namespace MediaPortal.Configuration.Sections
{
  public class MusicImport : MediaPortal.Configuration.SectionSettings
  {
    private FolderBrowserDialog folderBrowserDialog;
    private System.ComponentModel.IContainer components = null;

    private CheckBox checkBoxReplace;
    private CheckBox checkBoxCBR;
    private CheckBox checkBoxMono;
    private CheckBox checkBoxFastMode;
    private CheckBox checkBoxDatabase;
    private CheckBox checkBoxOrganize;
    private Button buttonBrowse;
    private Button buttonDefault;
    private GroupBox groupBoxGeneralSettings;
    private GroupBox groupBoxPerformance;
    private GroupBox groupBoxTarget;
    private GroupBox groupBoxQuality;
    private GroupBox groupBoxBitrate;
    private HScrollBar hScrollBarPriority;
    private HScrollBar hScrollBarQuality;
    private HScrollBar hScrollBarBitrate;
    private Label labelFasterImport;
    private Label labelBetterResponse;
    private Label labelLibraryFolder;
    private Label labelTarget;
    private Label labelBitrate;
    private TabControl tabControlMusicImport;
    private TabPage tabPageEncoderSettings;
    private TabPage tabPageImportSettings;
    private TextBox textBoxImportDir;
    private RadioButton radioButtonBitrate;
    private RadioButton radioButtonQuality;

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

    private const string Mpeg1BitRates = "128,160,192,224,256,320";
    private string[] Rates;
    private Preset[] Presets = new Preset[7];

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
      Rates = Mpeg1BitRates.Split(',');

      Presets[0] = new Preset("245,220,260");
      Presets[1] = new Preset("225,200,250");
      Presets[2] = new Preset("190,170,210");
      Presets[3] = new Preset("175,155,195");
      Presets[4] = new Preset("165,145,185");
      Presets[5] = new Preset("130,110,150");
      Presets[6] = new Preset("115,95,135");
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
      this.tabControlMusicImport = new System.Windows.Forms.TabControl();
      this.tabPageImportSettings = new System.Windows.Forms.TabPage();
      this.groupBoxGeneralSettings = new System.Windows.Forms.GroupBox();
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
      this.buttonDefault = new System.Windows.Forms.Button();
      this.tabControlMusicImport.SuspendLayout();
      this.tabPageImportSettings.SuspendLayout();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.groupBoxPerformance.SuspendLayout();
      this.tabPageEncoderSettings.SuspendLayout();
      this.groupBoxQuality.SuspendLayout();
      this.groupBoxBitrate.SuspendLayout();
      this.groupBoxTarget.SuspendLayout();
      this.SuspendLayout();
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
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxDatabase);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxOrganize);
      this.groupBoxGeneralSettings.Controls.Add(this.checkBoxReplace);
      this.groupBoxGeneralSettings.Controls.Add(this.labelLibraryFolder);
      this.groupBoxGeneralSettings.Controls.Add(this.buttonBrowse);
      this.groupBoxGeneralSettings.Controls.Add(this.textBoxImportDir);
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(16, 16);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(432, 136);
      this.groupBoxGeneralSettings.TabIndex = 16;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General Settings";
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
      this.labelLibraryFolder.Location = new System.Drawing.Point(16, 104);
      this.labelLibraryFolder.Name = "labelLibraryFolder";
      this.labelLibraryFolder.Size = new System.Drawing.Size(73, 13);
      this.labelLibraryFolder.TabIndex = 7;
      this.labelLibraryFolder.Text = "Library Folder:";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(344, 99);
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
      this.textBoxImportDir.Location = new System.Drawing.Point(96, 100);
      this.textBoxImportDir.Name = "textBoxImportDir";
      this.textBoxImportDir.Size = new System.Drawing.Size(240, 20);
      this.textBoxImportDir.TabIndex = 5;
      // 
      // groupBoxPerformance
      // 
      this.groupBoxPerformance.Controls.Add(this.hScrollBarPriority);
      this.groupBoxPerformance.Controls.Add(this.labelFasterImport);
      this.groupBoxPerformance.Controls.Add(this.labelBetterResponse);
      this.groupBoxPerformance.Location = new System.Drawing.Point(16, 160);
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
      this.checkBoxMono.Location = new System.Drawing.Point(16, 72);
      this.checkBoxMono.Name = "checkBoxMono";
      this.checkBoxMono.Size = new System.Drawing.Size(100, 17);
      this.checkBoxMono.TabIndex = 2;
      this.checkBoxMono.Text = "Mono encoding";
      this.checkBoxMono.UseVisualStyleBackColor = true;
      // 
      // radioButtonQuality
      // 
      this.radioButtonQuality.AutoSize = true;
      this.radioButtonQuality.Location = new System.Drawing.Point(16, 48);
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
      this.radioButtonBitrate.Location = new System.Drawing.Point(16, 24);
      this.radioButtonBitrate.Name = "radioButtonBitrate";
      this.radioButtonBitrate.Size = new System.Drawing.Size(55, 17);
      this.radioButtonBitrate.TabIndex = 0;
      this.radioButtonBitrate.TabStop = true;
      this.radioButtonBitrate.Text = "Bitrate";
      this.radioButtonBitrate.UseVisualStyleBackColor = true;
      this.radioButtonBitrate.CheckedChanged += new System.EventHandler(this.radioButtonBitrate_CheckedChanged);
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
      // MusicImport
      // 
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.Controls.Add(this.buttonDefault);
      this.Controls.Add(this.tabControlMusicImport);
      this.DoubleBuffered = true;
      this.Name = "MusicImport";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControlMusicImport.ResumeLayout(false);
      this.tabPageImportSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.groupBoxPerformance.ResumeLayout(false);
      this.groupBoxPerformance.PerformLayout();
      this.tabPageEncoderSettings.ResumeLayout(false);
      this.groupBoxQuality.ResumeLayout(false);
      this.groupBoxQuality.PerformLayout();
      this.groupBoxBitrate.ResumeLayout(false);
      this.groupBoxBitrate.PerformLayout();
      this.groupBoxTarget.ResumeLayout(false);
      this.groupBoxTarget.PerformLayout();
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
  }
}