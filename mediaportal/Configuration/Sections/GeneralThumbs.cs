#region Copyright (C) 2006 Team MediaPortal

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using MediaPortal.Util;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralThumbs : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlThumbnailSettings;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxThumbQuality;
    private TrackBar trackBarQuality;
    private MediaPortal.UserInterface.Controls.MPLabel labelLow;
    private MediaPortal.UserInterface.Controls.MPLabel labelQualityHint;
    private MediaPortal.UserInterface.Controls.MPLabel labelHigh;
    private MediaPortal.UserInterface.Controls.MPLabel labelResolution;
    private MediaPortal.UserInterface.Controls.MPLabel labelSmoothing;
    private MediaPortal.UserInterface.Controls.MPLabel labelInterpolation;
    private MediaPortal.UserInterface.Controls.MPLabel labelCompositing;
    private MediaPortal.UserInterface.Controls.MPLabel labelCurrentSmoothing;
    private MediaPortal.UserInterface.Controls.MPLabel labelCurrentInterpolation;
    private MediaPortal.UserInterface.Controls.MPLabel labelCurrentCompositing;
    private MediaPortal.UserInterface.Controls.MPLabel labelCurrentResolution;
    private MediaPortal.UserInterface.Controls.MPLabel labelRecommendedCurrent;
    private MediaPortal.UserInterface.Controls.MPLabel labelRecommendedHint;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxPictureThumbs;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMusicThumbs;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxPicThumbOnDemand;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxFolderThumbOnDemand;
    private MediaPortal.UserInterface.Controls.MPButton buttonClearMusicCache;
    private MediaPortal.UserInterface.Controls.MPButton buttonClearPictureThumbs;
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        trackBarQuality.Value = xmlreader.GetValueAsInt("thumbnails", "quality", 2);
        checkBoxFolderThumbOnDemand.Checked = xmlreader.GetValueAsBool("thumbnails", "musicfolderondemand", false);
        checkBoxPicThumbOnDemand.Checked = xmlreader.GetValueAsBool("thumbnails", "picturenolargethumbondemand", false);
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("thumbnails", "quality", trackBarQuality.Value);
        xmlwriter.SetValueAsBool("thumbnails", "musicfolderondemand", checkBoxFolderThumbOnDemand.Checked);
        xmlwriter.SetValueAsBool("thumbnails", "picturenolargethumbondemand", checkBoxPicThumbOnDemand.Checked);
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
          labelCurrentResolution.Text = Convert.ToString((int)Thumbs.ThumbResolution) + " + " + Convert.ToString((int)Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "High Speed";
          labelCurrentInterpolation.Text = "Nearest Neighbor";
          labelCurrentSmoothing.Text = "None";
          labelRecommendedCurrent.Text = @"Small CRTs";
          break;
        case 1:
          labelCurrentResolution.Text = Convert.ToString((int)Thumbs.ThumbResolution) + " + " + Convert.ToString((int)Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "High Speed";
          labelCurrentInterpolation.Text = "Low";
          labelCurrentSmoothing.Text = "High Speed";
          labelRecommendedCurrent.Text = "Small wide CRTs, medium CRTs";
          break;
        case 2:
          labelCurrentResolution.Text = Convert.ToString((int)Thumbs.ThumbResolution) + " + " + Convert.ToString((int)Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "Default";
          labelCurrentInterpolation.Text = "Default";
          labelCurrentSmoothing.Text = "Default";
          labelRecommendedCurrent.Text = "Large wide CRTs, small TFTs";
          break;
        case 3:
          labelCurrentResolution.Text = Convert.ToString((int)Thumbs.ThumbResolution) + " + " + Convert.ToString((int)Thumbs.ThumbLargeResolution);
          labelCurrentCompositing.Text = "Assume Linear";
          labelCurrentInterpolation.Text = "High Quality";
          labelCurrentSmoothing.Text = "High Quality";
          labelRecommendedCurrent.Text = "Small wide TFTs, Plasmas";
          break;
        case 4:
          labelCurrentResolution.Text = Convert.ToString((int)Thumbs.ThumbResolution) + " + " + Convert.ToString((int)Thumbs.ThumbLargeResolution);
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
      this.tabControlThumbnailSettings = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageThumbQuality = new System.Windows.Forms.TabPage();
      this.groupBoxThumbQuality = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelRecommendedCurrent = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelRecommendedHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCurrentSmoothing = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCurrentInterpolation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCurrentCompositing = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCurrentResolution = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelSmoothing = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelInterpolation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCompositing = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelResolution = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelHigh = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelLow = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelQualityHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.trackBarQuality = new System.Windows.Forms.TrackBar();
      this.groupBoxMusicThumbs = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBoxPictureThumbs = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxPicThumbOnDemand = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxFolderThumbOnDemand = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.buttonClearMusicCache = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonClearPictureThumbs = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabControlThumbnailSettings.SuspendLayout();
      this.tabPageThumbQuality.SuspendLayout();
      this.groupBoxThumbQuality.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarQuality)).BeginInit();
      this.groupBoxMusicThumbs.SuspendLayout();
      this.groupBoxPictureThumbs.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControlThumbnailSettings
      // 
      this.tabControlThumbnailSettings.Controls.Add(this.tabPageThumbQuality);
      this.tabControlThumbnailSettings.Location = new System.Drawing.Point(0, 0);
      this.tabControlThumbnailSettings.Name = "tabControlThumbnailSettings";
      this.tabControlThumbnailSettings.SelectedIndex = 0;
      this.tabControlThumbnailSettings.Size = new System.Drawing.Size(472, 408);
      this.tabControlThumbnailSettings.TabIndex = 0;
      // 
      // tabPageThumbQuality
      // 
      this.tabPageThumbQuality.Controls.Add(this.groupBoxPictureThumbs);
      this.tabPageThumbQuality.Controls.Add(this.groupBoxMusicThumbs);
      this.tabPageThumbQuality.Controls.Add(this.groupBoxThumbQuality);
      this.tabPageThumbQuality.Location = new System.Drawing.Point(4, 22);
      this.tabPageThumbQuality.Name = "tabPageThumbQuality";
      this.tabPageThumbQuality.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageThumbQuality.Size = new System.Drawing.Size(464, 382);
      this.tabPageThumbQuality.TabIndex = 0;
      this.tabPageThumbQuality.Text = "Thumbnails";
      this.tabPageThumbQuality.UseVisualStyleBackColor = true;
      // 
      // groupBoxThumbQuality
      // 
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
      this.groupBoxThumbQuality.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxThumbQuality.Location = new System.Drawing.Point(16, 16);
      this.groupBoxThumbQuality.Name = "groupBoxThumbQuality";
      this.groupBoxThumbQuality.Size = new System.Drawing.Size(428, 223);
      this.groupBoxThumbQuality.TabIndex = 0;
      this.groupBoxThumbQuality.TabStop = false;
      this.groupBoxThumbQuality.Text = "Quality settings";
      // 
      // labelRecommendedCurrent
      // 
      this.labelRecommendedCurrent.AutoSize = true;
      this.labelRecommendedCurrent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelRecommendedCurrent.Location = new System.Drawing.Point(10, 174);
      this.labelRecommendedCurrent.Name = "labelRecommendedCurrent";
      this.labelRecommendedCurrent.Size = new System.Drawing.Size(141, 13);
      this.labelRecommendedCurrent.TabIndex = 13;
      this.labelRecommendedCurrent.Text = "Large wide CRTs, small TFT";
      // 
      // labelRecommendedHint
      // 
      this.labelRecommendedHint.AutoSize = true;
      this.labelRecommendedHint.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelRecommendedHint.Location = new System.Drawing.Point(10, 153);
      this.labelRecommendedHint.Name = "labelRecommendedHint";
      this.labelRecommendedHint.Size = new System.Drawing.Size(113, 13);
      this.labelRecommendedHint.TabIndex = 12;
      this.labelRecommendedHint.Text = "Recommended for:";
      // 
      // labelCurrentSmoothing
      // 
      this.labelCurrentSmoothing.AutoSize = true;
      this.labelCurrentSmoothing.Location = new System.Drawing.Point(317, 154);
      this.labelCurrentSmoothing.Name = "labelCurrentSmoothing";
      this.labelCurrentSmoothing.Size = new System.Drawing.Size(41, 13);
      this.labelCurrentSmoothing.TabIndex = 11;
      this.labelCurrentSmoothing.Text = "Default";
      // 
      // labelCurrentInterpolation
      // 
      this.labelCurrentInterpolation.AutoSize = true;
      this.labelCurrentInterpolation.Location = new System.Drawing.Point(317, 122);
      this.labelCurrentInterpolation.Name = "labelCurrentInterpolation";
      this.labelCurrentInterpolation.Size = new System.Drawing.Size(41, 13);
      this.labelCurrentInterpolation.TabIndex = 10;
      this.labelCurrentInterpolation.Text = "Default";
      // 
      // labelCurrentCompositing
      // 
      this.labelCurrentCompositing.AutoSize = true;
      this.labelCurrentCompositing.Location = new System.Drawing.Point(317, 91);
      this.labelCurrentCompositing.Name = "labelCurrentCompositing";
      this.labelCurrentCompositing.Size = new System.Drawing.Size(41, 13);
      this.labelCurrentCompositing.TabIndex = 9;
      this.labelCurrentCompositing.Text = "Default";
      // 
      // labelCurrentResolution
      // 
      this.labelCurrentResolution.AutoSize = true;
      this.labelCurrentResolution.Location = new System.Drawing.Point(317, 61);
      this.labelCurrentResolution.Name = "labelCurrentResolution";
      this.labelCurrentResolution.Size = new System.Drawing.Size(55, 13);
      this.labelCurrentResolution.TabIndex = 8;
      this.labelCurrentResolution.Text = "128 + 512";
      // 
      // labelSmoothing
      // 
      this.labelSmoothing.AutoSize = true;
      this.labelSmoothing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSmoothing.Location = new System.Drawing.Point(235, 153);
      this.labelSmoothing.Name = "labelSmoothing";
      this.labelSmoothing.Size = new System.Drawing.Size(66, 13);
      this.labelSmoothing.TabIndex = 7;
      this.labelSmoothing.Text = "Smoothing";
      // 
      // labelInterpolation
      // 
      this.labelInterpolation.AutoSize = true;
      this.labelInterpolation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelInterpolation.Location = new System.Drawing.Point(235, 121);
      this.labelInterpolation.Name = "labelInterpolation";
      this.labelInterpolation.Size = new System.Drawing.Size(78, 13);
      this.labelInterpolation.TabIndex = 6;
      this.labelInterpolation.Text = "Interpolation";
      // 
      // labelCompositing
      // 
      this.labelCompositing.AutoSize = true;
      this.labelCompositing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelCompositing.Location = new System.Drawing.Point(235, 90);
      this.labelCompositing.Name = "labelCompositing";
      this.labelCompositing.Size = new System.Drawing.Size(75, 13);
      this.labelCompositing.TabIndex = 5;
      this.labelCompositing.Text = "Compositing";
      // 
      // labelResolution
      // 
      this.labelResolution.AutoSize = true;
      this.labelResolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelResolution.Location = new System.Drawing.Point(235, 60);
      this.labelResolution.Name = "labelResolution";
      this.labelResolution.Size = new System.Drawing.Size(67, 13);
      this.labelResolution.TabIndex = 4;
      this.labelResolution.Text = "Resolution";
      // 
      // labelHigh
      // 
      this.labelHigh.AutoSize = true;
      this.labelHigh.Location = new System.Drawing.Point(177, 15);
      this.labelHigh.Name = "labelHigh";
      this.labelHigh.Size = new System.Drawing.Size(39, 13);
      this.labelHigh.TabIndex = 3;
      this.labelHigh.Text = "Quality";
      // 
      // labelLow
      // 
      this.labelLow.AutoSize = true;
      this.labelLow.Location = new System.Drawing.Point(177, 193);
      this.labelLow.Name = "labelLow";
      this.labelLow.Size = new System.Drawing.Size(38, 13);
      this.labelLow.TabIndex = 2;
      this.labelLow.Text = "Speed";
      // 
      // labelQualityHint
      // 
      this.labelQualityHint.Location = new System.Drawing.Point(10, 28);
      this.labelQualityHint.Name = "labelQualityHint";
      this.labelQualityHint.Size = new System.Drawing.Size(163, 76);
      this.labelQualityHint.TabIndex = 1;
      this.labelQualityHint.Text = "Depending on your display size \r\nyou might want to decrease \r\nthumbnail quality f" +
          "or faster \r\nthumbnail generation and \r\nbetter browsing / scrolling";
      // 
      // trackBarQuality
      // 
      this.trackBarQuality.LargeChange = 2;
      this.trackBarQuality.Location = new System.Drawing.Point(180, 35);
      this.trackBarQuality.Maximum = 4;
      this.trackBarQuality.Name = "trackBarQuality";
      this.trackBarQuality.Orientation = System.Windows.Forms.Orientation.Vertical;
      this.trackBarQuality.Size = new System.Drawing.Size(45, 152);
      this.trackBarQuality.TabIndex = 0;
      this.trackBarQuality.Value = 2;
      this.trackBarQuality.ValueChanged += new System.EventHandler(this.trackBarQuality_ValueChanged);
      // 
      // groupBoxMusicThumbs
      // 
      this.groupBoxMusicThumbs.Controls.Add(this.buttonClearMusicCache);
      this.groupBoxMusicThumbs.Controls.Add(this.checkBoxFolderThumbOnDemand);
      this.groupBoxMusicThumbs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMusicThumbs.Location = new System.Drawing.Point(16, 245);
      this.groupBoxMusicThumbs.Name = "groupBoxMusicThumbs";
      this.groupBoxMusicThumbs.Size = new System.Drawing.Size(202, 89);
      this.groupBoxMusicThumbs.TabIndex = 1;
      this.groupBoxMusicThumbs.TabStop = false;
      this.groupBoxMusicThumbs.Text = "Music thumbs";
      // 
      // groupBoxPictureThumbs
      // 
      this.groupBoxPictureThumbs.Controls.Add(this.buttonClearPictureThumbs);
      this.groupBoxPictureThumbs.Controls.Add(this.checkBoxPicThumbOnDemand);
      this.groupBoxPictureThumbs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxPictureThumbs.Location = new System.Drawing.Point(242, 245);
      this.groupBoxPictureThumbs.Name = "groupBoxPictureThumbs";
      this.groupBoxPictureThumbs.Size = new System.Drawing.Size(202, 89);
      this.groupBoxPictureThumbs.TabIndex = 2;
      this.groupBoxPictureThumbs.TabStop = false;
      this.groupBoxPictureThumbs.Text = "Picture thumbs";
      // 
      // checkBoxPicThumbOnDemand
      // 
      this.checkBoxPicThumbOnDemand.AutoSize = true;
      this.checkBoxPicThumbOnDemand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxPicThumbOnDemand.Location = new System.Drawing.Point(12, 25);
      this.checkBoxPicThumbOnDemand.Name = "checkBoxPicThumbOnDemand";
      this.checkBoxPicThumbOnDemand.Size = new System.Drawing.Size(164, 17);
      this.checkBoxPicThumbOnDemand.TabIndex = 0;
      this.checkBoxPicThumbOnDemand.Text = "Auto-create only small thumbs";
      this.checkBoxPicThumbOnDemand.UseVisualStyleBackColor = true;
      // 
      // checkBoxFolderThumbOnDemand
      // 
      this.checkBoxFolderThumbOnDemand.AutoSize = true;
      this.checkBoxFolderThumbOnDemand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxFolderThumbOnDemand.Location = new System.Drawing.Point(13, 25);
      this.checkBoxFolderThumbOnDemand.Name = "checkBoxFolderThumbOnDemand";
      this.checkBoxFolderThumbOnDemand.Size = new System.Drawing.Size(173, 17);
      this.checkBoxFolderThumbOnDemand.TabIndex = 1;
      this.checkBoxFolderThumbOnDemand.Text = "Create folder cache on demand";
      this.checkBoxFolderThumbOnDemand.UseVisualStyleBackColor = true;
      // 
      // buttonClearMusicCache
      // 
      this.buttonClearMusicCache.Location = new System.Drawing.Point(13, 53);
      this.buttonClearMusicCache.Name = "buttonClearMusicCache";
      this.buttonClearMusicCache.Size = new System.Drawing.Size(173, 23);
      this.buttonClearMusicCache.TabIndex = 2;
      this.buttonClearMusicCache.Text = "Clear folder cache";
      this.buttonClearMusicCache.UseVisualStyleBackColor = true;
      this.buttonClearMusicCache.Click += new System.EventHandler(this.buttonClearMusicCache_Click);
      // 
      // buttonClearPictureThumbs
      // 
      this.buttonClearPictureThumbs.Location = new System.Drawing.Point(12, 53);
      this.buttonClearPictureThumbs.Name = "buttonClearPictureThumbs";
      this.buttonClearPictureThumbs.Size = new System.Drawing.Size(173, 23);
      this.buttonClearPictureThumbs.TabIndex = 3;
      this.buttonClearPictureThumbs.Text = "Clear picture cache";
      this.buttonClearPictureThumbs.UseVisualStyleBackColor = true;
      this.buttonClearPictureThumbs.Click += new System.EventHandler(this.buttonClearPictureThumbs_Click);
      // 
      // GeneralThumbs
      // 
      this.Controls.Add(this.tabControlThumbnailSettings);
      this.Name = "GeneralThumbs";
      this.Size = new System.Drawing.Size(472, 408);
      this.tabControlThumbnailSettings.ResumeLayout(false);
      this.tabPageThumbQuality.ResumeLayout(false);
      this.groupBoxThumbQuality.ResumeLayout(false);
      this.groupBoxThumbQuality.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarQuality)).EndInit();
      this.groupBoxMusicThumbs.ResumeLayout(false);
      this.groupBoxMusicThumbs.PerformLayout();
      this.groupBoxPictureThumbs.ResumeLayout(false);
      this.groupBoxPictureThumbs.PerformLayout();
      this.ResumeLayout(false);

    }

    private void buttonClearMusicCache_Click(object sender, EventArgs e)
    {
      Util.Utils.DeleteFiles(Thumbs.MusicFolder, String.Format(@"*{0}", Util.Utils.GetThumbExtension()));
    }

    private void buttonClearPictureThumbs_Click(object sender, EventArgs e)
    {
      Util.Utils.DeleteFiles(Thumbs.Pictures, String.Format(@"*{0}", Util.Utils.GetThumbExtension()));    
    }

  }
}
