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
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralThumbs : SectionSettings
  {
    private MPTabControl tabControlThumbnailSettings;
    private MPGroupBox groupBoxThumbQuality;
    private TrackBar trackBarQuality;
    private MPLabel labelLow;
    private MPLabel labelQualityHint;
    private MPLabel labelHigh;
    private MPLabel labelResolution;
    private MPLabel labelSmoothing;
    private MPLabel labelInterpolation;
    private MPLabel labelCompositing;
    private MPLabel labelCurrentSmoothing;
    private MPLabel labelCurrentInterpolation;
    private MPLabel labelCurrentCompositing;
    private MPLabel labelCurrentResolution;
    private MPLabel labelRecommendedCurrent;
    private MPLabel labelRecommendedHint;
    private MPGroupBox groupBoxPictureThumbs;
    private MPGroupBox groupBoxMusicThumbs;
    private MPCheckBox checkBoxPicThumbOnDemand;
    private MPCheckBox checkBoxFolderThumbOnDemand;
    private MPButton buttonClearMusicCache;
    private MPButton buttonClearPictureThumbs;
    private MPGroupBox groupBoxTVThumbs;
    private MPButton buttonClearTVThumbs;
    private MPCheckBox checkBoxTVThumbs;
    private TabPage tabPageThumbQuality;

    public GeneralThumbs()
      : this("Thumbnails")
    {
    }

    public GeneralThumbs(string name)
      : base(name)
    {
      InitializeComponent();
    }

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

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        trackBarQuality.Value = xmlreader.GetValueAsInt("thumbnails", "quality", 2);
        checkBoxFolderThumbOnDemand.Checked = xmlreader.GetValueAsBool("thumbnails", "musicfolderondemand", true);
        checkBoxPicThumbOnDemand.Checked = xmlreader.GetValueAsBool("thumbnails", "picturenolargethumbondemand", false);
        checkBoxTVThumbs.Checked = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("thumbnails", "quality", trackBarQuality.Value);
        xmlwriter.SetValueAsBool("thumbnails", "musicfolderondemand", checkBoxFolderThumbOnDemand.Checked);
        xmlwriter.SetValueAsBool("thumbnails", "picturenolargethumbondemand", checkBoxPicThumbOnDemand.Checked);
        xmlwriter.SetValueAsBool("thumbnails", "tvrecordedondemand", checkBoxTVThumbs.Checked);
      }
    }

    private void trackBarQuality_ValueChanged(object sender, EventArgs e)
    {
      switch (trackBarQuality.Value)
      {
        case 0:
          Thumbs.Quality = Thumbs.ThumbQuality.fastest;
          break;
        case 1:
          Thumbs.Quality = Thumbs.ThumbQuality.fast;
          break;
        case 2:
          Thumbs.Quality = Thumbs.ThumbQuality.average;
          break;
        case 3:
          Thumbs.Quality = Thumbs.ThumbQuality.higher;
          break;
        case 4:
          Thumbs.Quality = Thumbs.ThumbQuality.highest;
          break;
      }
      setThumbQualityLabels();
    }

    private void setThumbQualityLabels()
    {
      switch (trackBarQuality.Value)
      {
        case 0:
          labelCurrentResolution.Text = Convert.ToString((int) Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int) Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "High Speed";
          labelCurrentInterpolation.Text = "Nearest Neighbor";
          labelCurrentSmoothing.Text = "None";
          labelRecommendedCurrent.Text = @"Small CRTs";
          break;
        case 1:
          labelCurrentResolution.Text = Convert.ToString((int) Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int) Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "High Speed";
          labelCurrentInterpolation.Text = "Low";
          labelCurrentSmoothing.Text = "High Speed";
          labelRecommendedCurrent.Text = "Small wide CRTs, medium CRTs";
          break;
        case 2:
          labelCurrentResolution.Text = Convert.ToString((int) Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int) Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "Default";
          labelCurrentInterpolation.Text = "Default";
          labelCurrentSmoothing.Text = "Default";
          labelRecommendedCurrent.Text = "Large wide CRTs, small TFTs";
          break;
        case 3:
          labelCurrentResolution.Text = Convert.ToString((int) Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int) Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "Assume Linear";
          labelCurrentInterpolation.Text = "High Quality";
          labelCurrentSmoothing.Text = "High Quality";
          labelRecommendedCurrent.Text = "Small wide TFTs, Plasmas";
          break;
        case 4:
          labelCurrentResolution.Text = Convert.ToString((int) Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int) Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "High Quality";
          labelCurrentInterpolation.Text = "High Quality Bicubic";
          labelCurrentSmoothing.Text = "High Quality";
          labelRecommendedCurrent.Text = "Very large TFTs, Projectors";
          break;
      }
    }

    // designer generated code
    private void InitializeComponent()
    {
      this.tabControlThumbnailSettings = new MPTabControl();
      this.tabPageThumbQuality = new TabPage();
      this.groupBoxTVThumbs = new MPGroupBox();
      this.buttonClearTVThumbs = new MPButton();
      this.checkBoxTVThumbs = new MPCheckBox();
      this.groupBoxPictureThumbs = new MPGroupBox();
      this.buttonClearPictureThumbs = new MPButton();
      this.checkBoxPicThumbOnDemand = new MPCheckBox();
      this.groupBoxMusicThumbs = new MPGroupBox();
      this.buttonClearMusicCache = new MPButton();
      this.checkBoxFolderThumbOnDemand = new MPCheckBox();
      this.groupBoxThumbQuality = new MPGroupBox();
      this.labelRecommendedCurrent = new MPLabel();
      this.labelRecommendedHint = new MPLabel();
      this.labelCurrentSmoothing = new MPLabel();
      this.labelCurrentInterpolation = new MPLabel();
      this.labelCurrentCompositing = new MPLabel();
      this.labelCurrentResolution = new MPLabel();
      this.labelSmoothing = new MPLabel();
      this.labelInterpolation = new MPLabel();
      this.labelCompositing = new MPLabel();
      this.labelResolution = new MPLabel();
      this.labelHigh = new MPLabel();
      this.labelLow = new MPLabel();
      this.labelQualityHint = new MPLabel();
      this.trackBarQuality = new TrackBar();
      this.tabControlThumbnailSettings.SuspendLayout();
      this.tabPageThumbQuality.SuspendLayout();
      this.groupBoxTVThumbs.SuspendLayout();
      this.groupBoxPictureThumbs.SuspendLayout();
      this.groupBoxMusicThumbs.SuspendLayout();
      this.groupBoxThumbQuality.SuspendLayout();
      ((ISupportInitialize) (this.trackBarQuality)).BeginInit();
      this.SuspendLayout();
      // 
      // tabControlThumbnailSettings
      // 
      this.tabControlThumbnailSettings.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                                   | AnchorStyles.Left)
                                                                  | AnchorStyles.Right)));
      this.tabControlThumbnailSettings.Controls.Add(this.tabPageThumbQuality);
      this.tabControlThumbnailSettings.Location = new Point(0, 0);
      this.tabControlThumbnailSettings.Name = "tabControlThumbnailSettings";
      this.tabControlThumbnailSettings.SelectedIndex = 0;
      this.tabControlThumbnailSettings.Size = new Size(472, 408);
      this.tabControlThumbnailSettings.TabIndex = 0;
      // 
      // tabPageThumbQuality
      // 
      this.tabPageThumbQuality.Controls.Add(this.groupBoxTVThumbs);
      this.tabPageThumbQuality.Controls.Add(this.groupBoxPictureThumbs);
      this.tabPageThumbQuality.Controls.Add(this.groupBoxMusicThumbs);
      this.tabPageThumbQuality.Controls.Add(this.groupBoxThumbQuality);
      this.tabPageThumbQuality.Location = new Point(4, 22);
      this.tabPageThumbQuality.Name = "tabPageThumbQuality";
      this.tabPageThumbQuality.Padding = new Padding(3);
      this.tabPageThumbQuality.Size = new Size(464, 382);
      this.tabPageThumbQuality.TabIndex = 0;
      this.tabPageThumbQuality.Text = "Thumbnails";
      this.tabPageThumbQuality.UseVisualStyleBackColor = true;
      // 
      // groupBoxTVThumbs
      // 
      this.groupBoxTVThumbs.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.groupBoxTVThumbs.Controls.Add(this.buttonClearTVThumbs);
      this.groupBoxTVThumbs.Controls.Add(this.checkBoxTVThumbs);
      this.groupBoxTVThumbs.FlatStyle = FlatStyle.Popup;
      this.groupBoxTVThumbs.Location = new Point(16, 288);
      this.groupBoxTVThumbs.Name = "groupBoxTVThumbs";
      this.groupBoxTVThumbs.Size = new Size(207, 82);
      this.groupBoxTVThumbs.TabIndex = 3;
      this.groupBoxTVThumbs.TabStop = false;
      this.groupBoxTVThumbs.Text = "TV thumbs";
      // 
      // buttonClearTVThumbs
      // 
      this.buttonClearTVThumbs.Location = new Point(13, 46);
      this.buttonClearTVThumbs.Name = "buttonClearTVThumbs";
      this.buttonClearTVThumbs.Size = new Size(178, 23);
      this.buttonClearTVThumbs.TabIndex = 1;
      this.buttonClearTVThumbs.Text = "Clear recorded TV thumbs";
      this.buttonClearTVThumbs.UseVisualStyleBackColor = true;
      this.buttonClearTVThumbs.Click += new EventHandler(this.buttonClearTVThumbs_Click);
      // 
      // checkBoxTVThumbs
      // 
      this.checkBoxTVThumbs.AutoSize = true;
      this.checkBoxTVThumbs.Checked = true;
      this.checkBoxTVThumbs.CheckState = CheckState.Checked;
      this.checkBoxTVThumbs.FlatStyle = FlatStyle.Popup;
      this.checkBoxTVThumbs.Location = new Point(13, 23);
      this.checkBoxTVThumbs.Name = "checkBoxTVThumbs";
      this.checkBoxTVThumbs.Size = new Size(180, 17);
      this.checkBoxTVThumbs.TabIndex = 0;
      this.checkBoxTVThumbs.Text = "Auto-create thumbs of recordings";
      this.checkBoxTVThumbs.UseVisualStyleBackColor = true;
      // 
      // groupBoxPictureThumbs
      // 
      this.groupBoxPictureThumbs.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.groupBoxPictureThumbs.Controls.Add(this.buttonClearPictureThumbs);
      this.groupBoxPictureThumbs.Controls.Add(this.checkBoxPicThumbOnDemand);
      this.groupBoxPictureThumbs.FlatStyle = FlatStyle.Popup;
      this.groupBoxPictureThumbs.Location = new Point(237, 201);
      this.groupBoxPictureThumbs.Name = "groupBoxPictureThumbs";
      this.groupBoxPictureThumbs.Size = new Size(207, 82);
      this.groupBoxPictureThumbs.TabIndex = 2;
      this.groupBoxPictureThumbs.TabStop = false;
      this.groupBoxPictureThumbs.Text = "Picture thumbs";
      // 
      // buttonClearPictureThumbs
      // 
      this.buttonClearPictureThumbs.Location = new Point(14, 46);
      this.buttonClearPictureThumbs.Name = "buttonClearPictureThumbs";
      this.buttonClearPictureThumbs.Size = new Size(178, 23);
      this.buttonClearPictureThumbs.TabIndex = 1;
      this.buttonClearPictureThumbs.Text = "Clear picture cache";
      this.buttonClearPictureThumbs.UseVisualStyleBackColor = true;
      this.buttonClearPictureThumbs.Click += new EventHandler(this.buttonClearPictureThumbs_Click);
      // 
      // checkBoxPicThumbOnDemand
      // 
      this.checkBoxPicThumbOnDemand.AutoSize = true;
      this.checkBoxPicThumbOnDemand.FlatStyle = FlatStyle.Popup;
      this.checkBoxPicThumbOnDemand.Location = new Point(14, 23);
      this.checkBoxPicThumbOnDemand.Name = "checkBoxPicThumbOnDemand";
      this.checkBoxPicThumbOnDemand.Size = new Size(164, 17);
      this.checkBoxPicThumbOnDemand.TabIndex = 0;
      this.checkBoxPicThumbOnDemand.Text = "Auto-create only small thumbs";
      this.checkBoxPicThumbOnDemand.UseVisualStyleBackColor = true;
      // 
      // groupBoxMusicThumbs
      // 
      this.groupBoxMusicThumbs.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.groupBoxMusicThumbs.Controls.Add(this.buttonClearMusicCache);
      this.groupBoxMusicThumbs.Controls.Add(this.checkBoxFolderThumbOnDemand);
      this.groupBoxMusicThumbs.FlatStyle = FlatStyle.Popup;
      this.groupBoxMusicThumbs.Location = new Point(16, 202);
      this.groupBoxMusicThumbs.Name = "groupBoxMusicThumbs";
      this.groupBoxMusicThumbs.Size = new Size(207, 81);
      this.groupBoxMusicThumbs.TabIndex = 1;
      this.groupBoxMusicThumbs.TabStop = false;
      this.groupBoxMusicThumbs.Text = "Music thumbs";
      // 
      // buttonClearMusicCache
      // 
      this.buttonClearMusicCache.Location = new Point(13, 46);
      this.buttonClearMusicCache.Name = "buttonClearMusicCache";
      this.buttonClearMusicCache.Size = new Size(178, 23);
      this.buttonClearMusicCache.TabIndex = 1;
      this.buttonClearMusicCache.Text = "Delete all music thumbs";
      this.buttonClearMusicCache.UseVisualStyleBackColor = true;
      this.buttonClearMusicCache.Click += new EventHandler(this.buttonClearMusicCache_Click);
      // 
      // checkBoxFolderThumbOnDemand
      // 
      this.checkBoxFolderThumbOnDemand.AutoSize = true;
      this.checkBoxFolderThumbOnDemand.FlatStyle = FlatStyle.Popup;
      this.checkBoxFolderThumbOnDemand.Location = new Point(13, 23);
      this.checkBoxFolderThumbOnDemand.Name = "checkBoxFolderThumbOnDemand";
      this.checkBoxFolderThumbOnDemand.Size = new Size(173, 17);
      this.checkBoxFolderThumbOnDemand.TabIndex = 0;
      this.checkBoxFolderThumbOnDemand.Text = "Create folder cache on demand";
      this.checkBoxFolderThumbOnDemand.UseVisualStyleBackColor = true;
      // 
      // groupBoxThumbQuality
      // 
      this.groupBoxThumbQuality.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                            | AnchorStyles.Left)
                                                           | AnchorStyles.Right)));
      this.groupBoxThumbQuality.Controls.Add(this.labelRecommendedCurrent);
      this.groupBoxThumbQuality.Controls.Add(this.labelRecommendedHint);
      this.groupBoxThumbQuality.Controls.Add(this.labelCurrentSmoothing);
      this.groupBoxThumbQuality.Controls.Add(this.labelCurrentInterpolation);
      this.groupBoxThumbQuality.Controls.Add(this.labelCurrentCompositing);
      this.groupBoxThumbQuality.Controls.Add(this.labelCurrentResolution);
      this.groupBoxThumbQuality.Controls.Add(this.labelSmoothing);
      this.groupBoxThumbQuality.Controls.Add(this.labelInterpolation);
      this.groupBoxThumbQuality.Controls.Add(this.labelCompositing);
      this.groupBoxThumbQuality.Controls.Add(this.labelResolution);
      this.groupBoxThumbQuality.Controls.Add(this.labelHigh);
      this.groupBoxThumbQuality.Controls.Add(this.labelLow);
      this.groupBoxThumbQuality.Controls.Add(this.labelQualityHint);
      this.groupBoxThumbQuality.Controls.Add(this.trackBarQuality);
      this.groupBoxThumbQuality.FlatStyle = FlatStyle.Popup;
      this.groupBoxThumbQuality.Location = new Point(16, 16);
      this.groupBoxThumbQuality.Name = "groupBoxThumbQuality";
      this.groupBoxThumbQuality.Size = new Size(428, 179);
      this.groupBoxThumbQuality.TabIndex = 0;
      this.groupBoxThumbQuality.TabStop = false;
      this.groupBoxThumbQuality.Text = "Quality settings";
      // 
      // labelRecommendedCurrent
      // 
      this.labelRecommendedCurrent.Anchor = AnchorStyles.None;
      this.labelRecommendedCurrent.AutoSize = true;
      this.labelRecommendedCurrent.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point,
                                                   ((byte) (0)));
      this.labelRecommendedCurrent.Location = new Point(10, 155);
      this.labelRecommendedCurrent.Name = "labelRecommendedCurrent";
      this.labelRecommendedCurrent.Size = new Size(141, 13);
      this.labelRecommendedCurrent.TabIndex = 2;
      this.labelRecommendedCurrent.Text = "Large wide CRTs, small TFT";
      // 
      // labelRecommendedHint
      // 
      this.labelRecommendedHint.Anchor = AnchorStyles.None;
      this.labelRecommendedHint.AutoSize = true;
      this.labelRecommendedHint.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point,
                                                ((byte) (0)));
      this.labelRecommendedHint.Location = new Point(10, 133);
      this.labelRecommendedHint.Name = "labelRecommendedHint";
      this.labelRecommendedHint.Size = new Size(113, 13);
      this.labelRecommendedHint.TabIndex = 1;
      this.labelRecommendedHint.Text = "Recommended for:";
      // 
      // labelCurrentSmoothing
      // 
      this.labelCurrentSmoothing.Anchor = AnchorStyles.None;
      this.labelCurrentSmoothing.AutoSize = true;
      this.labelCurrentSmoothing.Location = new Point(317, 134);
      this.labelCurrentSmoothing.Name = "labelCurrentSmoothing";
      this.labelCurrentSmoothing.Size = new Size(41, 13);
      this.labelCurrentSmoothing.TabIndex = 13;
      this.labelCurrentSmoothing.Text = "Default";
      // 
      // labelCurrentInterpolation
      // 
      this.labelCurrentInterpolation.Anchor = AnchorStyles.None;
      this.labelCurrentInterpolation.AutoSize = true;
      this.labelCurrentInterpolation.Location = new Point(317, 102);
      this.labelCurrentInterpolation.Name = "labelCurrentInterpolation";
      this.labelCurrentInterpolation.Size = new Size(41, 13);
      this.labelCurrentInterpolation.TabIndex = 11;
      this.labelCurrentInterpolation.Text = "Default";
      // 
      // labelCurrentCompositing
      // 
      this.labelCurrentCompositing.Anchor = AnchorStyles.None;
      this.labelCurrentCompositing.AutoSize = true;
      this.labelCurrentCompositing.Location = new Point(317, 71);
      this.labelCurrentCompositing.Name = "labelCurrentCompositing";
      this.labelCurrentCompositing.Size = new Size(41, 13);
      this.labelCurrentCompositing.TabIndex = 9;
      this.labelCurrentCompositing.Text = "Default";
      // 
      // labelCurrentResolution
      // 
      this.labelCurrentResolution.Anchor = AnchorStyles.None;
      this.labelCurrentResolution.AutoSize = true;
      this.labelCurrentResolution.Location = new Point(317, 41);
      this.labelCurrentResolution.Name = "labelCurrentResolution";
      this.labelCurrentResolution.Size = new Size(55, 13);
      this.labelCurrentResolution.TabIndex = 7;
      this.labelCurrentResolution.Text = "120 + 500";
      // 
      // labelSmoothing
      // 
      this.labelSmoothing.Anchor = AnchorStyles.None;
      this.labelSmoothing.AutoSize = true;
      this.labelSmoothing.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point,
                                          ((byte) (0)));
      this.labelSmoothing.Location = new Point(235, 133);
      this.labelSmoothing.Name = "labelSmoothing";
      this.labelSmoothing.Size = new Size(66, 13);
      this.labelSmoothing.TabIndex = 12;
      this.labelSmoothing.Text = "Smoothing";
      // 
      // labelInterpolation
      // 
      this.labelInterpolation.Anchor = AnchorStyles.None;
      this.labelInterpolation.AutoSize = true;
      this.labelInterpolation.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point,
                                              ((byte) (0)));
      this.labelInterpolation.Location = new Point(235, 101);
      this.labelInterpolation.Name = "labelInterpolation";
      this.labelInterpolation.Size = new Size(78, 13);
      this.labelInterpolation.TabIndex = 10;
      this.labelInterpolation.Text = "Interpolation";
      // 
      // labelCompositing
      // 
      this.labelCompositing.Anchor = AnchorStyles.None;
      this.labelCompositing.AutoSize = true;
      this.labelCompositing.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point,
                                            ((byte) (0)));
      this.labelCompositing.Location = new Point(235, 70);
      this.labelCompositing.Name = "labelCompositing";
      this.labelCompositing.Size = new Size(75, 13);
      this.labelCompositing.TabIndex = 8;
      this.labelCompositing.Text = "Compositing";
      // 
      // labelResolution
      // 
      this.labelResolution.Anchor = AnchorStyles.None;
      this.labelResolution.AutoSize = true;
      this.labelResolution.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point,
                                           ((byte) (0)));
      this.labelResolution.Location = new Point(235, 40);
      this.labelResolution.Name = "labelResolution";
      this.labelResolution.Size = new Size(67, 13);
      this.labelResolution.TabIndex = 6;
      this.labelResolution.Text = "Resolution";
      // 
      // labelHigh
      // 
      this.labelHigh.Anchor = AnchorStyles.None;
      this.labelHigh.AutoSize = true;
      this.labelHigh.Location = new Point(177, 18);
      this.labelHigh.Name = "labelHigh";
      this.labelHigh.Size = new Size(39, 13);
      this.labelHigh.TabIndex = 3;
      this.labelHigh.Text = "Quality";
      // 
      // labelLow
      // 
      this.labelLow.Anchor = AnchorStyles.None;
      this.labelLow.AutoSize = true;
      this.labelLow.Location = new Point(177, 155);
      this.labelLow.Name = "labelLow";
      this.labelLow.Size = new Size(38, 13);
      this.labelLow.TabIndex = 5;
      this.labelLow.Text = "Speed";
      // 
      // labelQualityHint
      // 
      this.labelQualityHint.Anchor = AnchorStyles.None;
      this.labelQualityHint.Location = new Point(10, 28);
      this.labelQualityHint.Name = "labelQualityHint";
      this.labelQualityHint.Size = new Size(163, 76);
      this.labelQualityHint.TabIndex = 0;
      this.labelQualityHint.Text =
        "Depending on your display size \r\nyou might want to decrease \r\nthumbnail quality f" +
        "or faster \r\nthumbnail generation and \r\nbetter browsing / scrolling";
      // 
      // trackBarQuality
      // 
      this.trackBarQuality.Anchor = AnchorStyles.None;
      this.trackBarQuality.LargeChange = 2;
      this.trackBarQuality.Location = new Point(180, 35);
      this.trackBarQuality.Maximum = 4;
      this.trackBarQuality.Name = "trackBarQuality";
      this.trackBarQuality.Orientation = Orientation.Vertical;
      this.trackBarQuality.Size = new Size(40, 116);
      this.trackBarQuality.TabIndex = 4;
      this.trackBarQuality.Value = 2;
      this.trackBarQuality.ValueChanged += new EventHandler(this.trackBarQuality_ValueChanged);
      // 
      // GeneralThumbs
      // 
      this.Controls.Add(this.tabControlThumbnailSettings);
      this.Name = "GeneralThumbs";
      this.Size = new Size(472, 408);
      this.tabControlThumbnailSettings.ResumeLayout(false);
      this.tabPageThumbQuality.ResumeLayout(false);
      this.groupBoxTVThumbs.ResumeLayout(false);
      this.groupBoxTVThumbs.PerformLayout();
      this.groupBoxPictureThumbs.ResumeLayout(false);
      this.groupBoxPictureThumbs.PerformLayout();
      this.groupBoxMusicThumbs.ResumeLayout(false);
      this.groupBoxMusicThumbs.PerformLayout();
      this.groupBoxThumbQuality.ResumeLayout(false);
      this.groupBoxThumbQuality.PerformLayout();
      ((ISupportInitialize) (this.trackBarQuality)).EndInit();
      this.ResumeLayout(false);
    }

    private void buttonClearMusicCache_Click(object sender, EventArgs e)
    {
      Util.Utils.DeleteFiles(Thumbs.MusicFolder, String.Format(@"*{0}", Util.Utils.GetThumbExtension()));
      Util.Utils.DeleteFiles(Thumbs.MusicAlbum, String.Format(@"*{0}", Util.Utils.GetThumbExtension()));
      Util.Utils.DeleteFiles(Thumbs.MusicArtists, String.Format(@"*{0}", Util.Utils.GetThumbExtension()));
      Util.Utils.DeleteFiles(Thumbs.MusicGenre, String.Format(@"*{0}", Util.Utils.GetThumbExtension()));
    }

    private void buttonClearPictureThumbs_Click(object sender, EventArgs e)
    {
      Util.Utils.DeleteFiles(Thumbs.Pictures, String.Format(@"*{0}", Util.Utils.GetThumbExtension()));
    }

    private void buttonClearTVThumbs_Click(object sender, EventArgs e)
    {
      Util.Utils.DeleteFiles(Thumbs.TVRecorded, String.Format(@"*{0}", Util.Utils.GetThumbExtension()));
    }
  }
}