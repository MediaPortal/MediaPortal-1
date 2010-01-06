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

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for SearchCityForm.
  /// </summary>
  public class SearchCityForm : MPConfigForm
  {
    private MPGroupBox groupBoxNewCity;
    private ListBox listBoxCityResults;
    private MPTextBox searchTextBox;
    private MPButton buttonCitySearch;
    private MPButton buttonAddCity;
    private MPLabel labelCity;
    private MPLabel labelCityResults;
    private MPGroupBox groupBoxCityDetails;
    private MPTabControl tabControlCityURLs;
    private TabPage tabPageSatImg;
    private TabPage tabPageTempImg;
    private MPTextBox textBoxSatURL;
    private TabPage tabPageUVImg;
    private TabPage tabPageWindsImg;
    private TabPage tabPageHumImg;
    private TabPage tabPagePrecImg;
    private MPTextBox textBoxTempURL;
    private MPTextBox textBoxUVURL;
    private MPTextBox textBoxWindURL;
    private MPTextBox textBoxHumURL;
    private MPTextBox textBoxPrecURL;
    private MPButton buttonCancelCity;
    private LinkLabel lblWeatherDetails;
    private PictureBox pictureBoxPreviewSat;
    private MPButton btnPreviewSat;
    private MPButton btnPreviewTemp;
    private PictureBox pictureBoxPreviewTemp;
    private MPButton btnPreviewUV;
    private PictureBox pictureBoxPreviewUV;
    private MPButton btnPreviewWinds;
    private PictureBox pictureBoxPreviewWinds;
    private MPButton btnPreviewHumidity;
    private PictureBox pictureBoxPreviewHumidity;
    private MPButton btnPreviewPrecip;
    private PictureBox pictureBoxPreviewPrecip;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public SearchCityForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
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
      this.groupBoxNewCity = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.buttonCancelCity = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxCityDetails = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lblWeatherDetails = new System.Windows.Forms.LinkLabel();
      this.tabControlCityURLs = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageSatImg = new System.Windows.Forms.TabPage();
      this.btnPreviewSat = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBoxPreviewSat = new System.Windows.Forms.PictureBox();
      this.textBoxSatURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPageTempImg = new System.Windows.Forms.TabPage();
      this.btnPreviewTemp = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBoxPreviewTemp = new System.Windows.Forms.PictureBox();
      this.textBoxTempURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPageUVImg = new System.Windows.Forms.TabPage();
      this.btnPreviewUV = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBoxPreviewUV = new System.Windows.Forms.PictureBox();
      this.textBoxUVURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPageWindsImg = new System.Windows.Forms.TabPage();
      this.btnPreviewWinds = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBoxPreviewWinds = new System.Windows.Forms.PictureBox();
      this.textBoxWindURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPageHumImg = new System.Windows.Forms.TabPage();
      this.btnPreviewHumidity = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBoxPreviewHumidity = new System.Windows.Forms.PictureBox();
      this.textBoxHumURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.tabPagePrecImg = new System.Windows.Forms.TabPage();
      this.btnPreviewPrecip = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBoxPreviewPrecip = new System.Windows.Forms.PictureBox();
      this.textBoxPrecURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.listBoxCityResults = new System.Windows.Forms.ListBox();
      this.labelCityResults = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelCity = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonCitySearch = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonAddCity = new MediaPortal.UserInterface.Controls.MPButton();
      this.searchTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBoxNewCity.SuspendLayout();
      this.groupBoxCityDetails.SuspendLayout();
      this.tabControlCityURLs.SuspendLayout();
      this.tabPageSatImg.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewSat)).BeginInit();
      this.tabPageTempImg.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewTemp)).BeginInit();
      this.tabPageUVImg.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewUV)).BeginInit();
      this.tabPageWindsImg.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewWinds)).BeginInit();
      this.tabPageHumImg.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewHumidity)).BeginInit();
      this.tabPagePrecImg.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewPrecip)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxNewCity
      // 
      this.groupBoxNewCity.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxNewCity.Controls.Add(this.buttonCancelCity);
      this.groupBoxNewCity.Controls.Add(this.groupBoxCityDetails);
      this.groupBoxNewCity.Controls.Add(this.labelCity);
      this.groupBoxNewCity.Controls.Add(this.buttonCitySearch);
      this.groupBoxNewCity.Controls.Add(this.buttonAddCity);
      this.groupBoxNewCity.Controls.Add(this.searchTextBox);
      this.groupBoxNewCity.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxNewCity.Location = new System.Drawing.Point(8, 8);
      this.groupBoxNewCity.Name = "groupBoxNewCity";
      this.groupBoxNewCity.Size = new System.Drawing.Size(490, 435);
      this.groupBoxNewCity.TabIndex = 0;
      this.groupBoxNewCity.TabStop = false;
      this.groupBoxNewCity.Text = "Add new city";
      // 
      // buttonCancelCity
      // 
      this.buttonCancelCity.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancelCity.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancelCity.Location = new System.Drawing.Point(396, 406);
      this.buttonCancelCity.Name = "buttonCancelCity";
      this.buttonCancelCity.Size = new System.Drawing.Size(75, 23);
      this.buttonCancelCity.TabIndex = 2;
      this.buttonCancelCity.Text = "&Cancel";
      this.buttonCancelCity.UseVisualStyleBackColor = true;
      this.buttonCancelCity.Visible = false;
      this.buttonCancelCity.Click += new System.EventHandler(this.buttonCancelCity_Click);
      // 
      // groupBoxCityDetails
      // 
      this.groupBoxCityDetails.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxCityDetails.Controls.Add(this.lblWeatherDetails);
      this.groupBoxCityDetails.Controls.Add(this.tabControlCityURLs);
      this.groupBoxCityDetails.Controls.Add(this.listBoxCityResults);
      this.groupBoxCityDetails.Controls.Add(this.labelCityResults);
      this.groupBoxCityDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCityDetails.Location = new System.Drawing.Point(18, 47);
      this.groupBoxCityDetails.Name = "groupBoxCityDetails";
      this.groupBoxCityDetails.Size = new System.Drawing.Size(453, 353);
      this.groupBoxCityDetails.TabIndex = 37;
      this.groupBoxCityDetails.TabStop = false;
      // 
      // lblWeatherDetails
      // 
      this.lblWeatherDetails.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.lblWeatherDetails.LinkArea = new System.Windows.Forms.LinkArea(130, 4);
      this.lblWeatherDetails.Location = new System.Drawing.Point(11, 311);
      this.lblWeatherDetails.Name = "lblWeatherDetails";
      this.lblWeatherDetails.Size = new System.Drawing.Size(436, 34);
      this.lblWeatherDetails.TabIndex = 38;
      this.lblWeatherDetails.TabStop = true;
      this.lblWeatherDetails.Text =
        "Here you can enter URLs to detailed weather images.\r\nYou\'ll find many pictures at" +
        " www.weather.com, your local news site or in our wiki.";
      this.lblWeatherDetails.UseCompatibleTextRendering = true;
      this.lblWeatherDetails.LinkClicked +=
        new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblWeatherDetails_LinkClicked);
      // 
      // tabControlCityURLs
      // 
      this.tabControlCityURLs.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlCityURLs.Controls.Add(this.tabPageSatImg);
      this.tabControlCityURLs.Controls.Add(this.tabPageTempImg);
      this.tabControlCityURLs.Controls.Add(this.tabPageUVImg);
      this.tabControlCityURLs.Controls.Add(this.tabPageWindsImg);
      this.tabControlCityURLs.Controls.Add(this.tabPageHumImg);
      this.tabControlCityURLs.Controls.Add(this.tabPagePrecImg);
      this.tabControlCityURLs.Location = new System.Drawing.Point(11, 109);
      this.tabControlCityURLs.Name = "tabControlCityURLs";
      this.tabControlCityURLs.SelectedIndex = 0;
      this.tabControlCityURLs.Size = new System.Drawing.Size(433, 199);
      this.tabControlCityURLs.TabIndex = 37;
      // 
      // tabPageSatImg
      // 
      this.tabPageSatImg.Controls.Add(this.btnPreviewSat);
      this.tabPageSatImg.Controls.Add(this.pictureBoxPreviewSat);
      this.tabPageSatImg.Controls.Add(this.textBoxSatURL);
      this.tabPageSatImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageSatImg.Name = "tabPageSatImg";
      this.tabPageSatImg.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSatImg.Size = new System.Drawing.Size(425, 173);
      this.tabPageSatImg.TabIndex = 0;
      this.tabPageSatImg.Text = "Satellite";
      this.tabPageSatImg.UseVisualStyleBackColor = true;
      // 
      // btnPreviewSat
      // 
      this.btnPreviewSat.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPreviewSat.Location = new System.Drawing.Point(363, 6);
      this.btnPreviewSat.Name = "btnPreviewSat";
      this.btnPreviewSat.Size = new System.Drawing.Size(56, 20);
      this.btnPreviewSat.TabIndex = 27;
      this.btnPreviewSat.Text = "Preview";
      this.btnPreviewSat.UseVisualStyleBackColor = true;
      this.btnPreviewSat.Click += new System.EventHandler(this.btnPreviewSat_Click);
      // 
      // pictureBoxPreviewSat
      // 
      this.pictureBoxPreviewSat.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBoxPreviewSat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pictureBoxPreviewSat.Location = new System.Drawing.Point(6, 32);
      this.pictureBoxPreviewSat.Name = "pictureBoxPreviewSat";
      this.pictureBoxPreviewSat.Size = new System.Drawing.Size(240, 135);
      this.pictureBoxPreviewSat.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxPreviewSat.TabIndex = 26;
      this.pictureBoxPreviewSat.TabStop = false;
      this.pictureBoxPreviewSat.WaitOnLoad = true;
      // 
      // textBoxSatURL
      // 
      this.textBoxSatURL.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxSatURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxSatURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxSatURL.Name = "textBoxSatURL";
      this.textBoxSatURL.Size = new System.Drawing.Size(351, 20);
      this.textBoxSatURL.TabIndex = 25;
      // 
      // tabPageTempImg
      // 
      this.tabPageTempImg.Controls.Add(this.btnPreviewTemp);
      this.tabPageTempImg.Controls.Add(this.pictureBoxPreviewTemp);
      this.tabPageTempImg.Controls.Add(this.textBoxTempURL);
      this.tabPageTempImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageTempImg.Name = "tabPageTempImg";
      this.tabPageTempImg.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageTempImg.Size = new System.Drawing.Size(425, 173);
      this.tabPageTempImg.TabIndex = 1;
      this.tabPageTempImg.Text = "Temperature";
      this.tabPageTempImg.UseVisualStyleBackColor = true;
      // 
      // btnPreviewTemp
      // 
      this.btnPreviewTemp.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPreviewTemp.Location = new System.Drawing.Point(363, 6);
      this.btnPreviewTemp.Name = "btnPreviewTemp";
      this.btnPreviewTemp.Size = new System.Drawing.Size(56, 20);
      this.btnPreviewTemp.TabIndex = 29;
      this.btnPreviewTemp.Text = "Preview";
      this.btnPreviewTemp.UseVisualStyleBackColor = true;
      this.btnPreviewTemp.Click += new System.EventHandler(this.btnPreviewTemp_Click);
      // 
      // pictureBoxPreviewTemp
      // 
      this.pictureBoxPreviewTemp.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBoxPreviewTemp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pictureBoxPreviewTemp.Location = new System.Drawing.Point(6, 32);
      this.pictureBoxPreviewTemp.Name = "pictureBoxPreviewTemp";
      this.pictureBoxPreviewTemp.Size = new System.Drawing.Size(240, 135);
      this.pictureBoxPreviewTemp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxPreviewTemp.TabIndex = 28;
      this.pictureBoxPreviewTemp.TabStop = false;
      this.pictureBoxPreviewTemp.WaitOnLoad = true;
      // 
      // textBoxTempURL
      // 
      this.textBoxTempURL.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxTempURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxTempURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxTempURL.Name = "textBoxTempURL";
      this.textBoxTempURL.Size = new System.Drawing.Size(351, 20);
      this.textBoxTempURL.TabIndex = 27;
      // 
      // tabPageUVImg
      // 
      this.tabPageUVImg.Controls.Add(this.btnPreviewUV);
      this.tabPageUVImg.Controls.Add(this.pictureBoxPreviewUV);
      this.tabPageUVImg.Controls.Add(this.textBoxUVURL);
      this.tabPageUVImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageUVImg.Name = "tabPageUVImg";
      this.tabPageUVImg.Size = new System.Drawing.Size(425, 173);
      this.tabPageUVImg.TabIndex = 2;
      this.tabPageUVImg.Text = "UV Index";
      this.tabPageUVImg.UseVisualStyleBackColor = true;
      // 
      // btnPreviewUV
      // 
      this.btnPreviewUV.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPreviewUV.Location = new System.Drawing.Point(363, 6);
      this.btnPreviewUV.Name = "btnPreviewUV";
      this.btnPreviewUV.Size = new System.Drawing.Size(56, 20);
      this.btnPreviewUV.TabIndex = 42;
      this.btnPreviewUV.Text = "Preview";
      this.btnPreviewUV.UseVisualStyleBackColor = true;
      this.btnPreviewUV.Click += new System.EventHandler(this.btnPreviewUV_Click);
      // 
      // pictureBoxPreviewUV
      // 
      this.pictureBoxPreviewUV.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBoxPreviewUV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pictureBoxPreviewUV.Location = new System.Drawing.Point(6, 32);
      this.pictureBoxPreviewUV.Name = "pictureBoxPreviewUV";
      this.pictureBoxPreviewUV.Size = new System.Drawing.Size(240, 135);
      this.pictureBoxPreviewUV.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxPreviewUV.TabIndex = 41;
      this.pictureBoxPreviewUV.TabStop = false;
      this.pictureBoxPreviewUV.WaitOnLoad = true;
      // 
      // textBoxUVURL
      // 
      this.textBoxUVURL.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxUVURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxUVURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxUVURL.Name = "textBoxUVURL";
      this.textBoxUVURL.Size = new System.Drawing.Size(351, 20);
      this.textBoxUVURL.TabIndex = 40;
      // 
      // tabPageWindsImg
      // 
      this.tabPageWindsImg.Controls.Add(this.btnPreviewWinds);
      this.tabPageWindsImg.Controls.Add(this.pictureBoxPreviewWinds);
      this.tabPageWindsImg.Controls.Add(this.textBoxWindURL);
      this.tabPageWindsImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageWindsImg.Name = "tabPageWindsImg";
      this.tabPageWindsImg.Size = new System.Drawing.Size(425, 173);
      this.tabPageWindsImg.TabIndex = 3;
      this.tabPageWindsImg.Text = "Winds";
      this.tabPageWindsImg.UseVisualStyleBackColor = true;
      // 
      // btnPreviewWinds
      // 
      this.btnPreviewWinds.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPreviewWinds.Location = new System.Drawing.Point(363, 6);
      this.btnPreviewWinds.Name = "btnPreviewWinds";
      this.btnPreviewWinds.Size = new System.Drawing.Size(56, 20);
      this.btnPreviewWinds.TabIndex = 33;
      this.btnPreviewWinds.Text = "Preview";
      this.btnPreviewWinds.UseVisualStyleBackColor = true;
      this.btnPreviewWinds.Click += new System.EventHandler(this.btnPreviewWinds_Click);
      // 
      // pictureBoxPreviewWinds
      // 
      this.pictureBoxPreviewWinds.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBoxPreviewWinds.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pictureBoxPreviewWinds.Location = new System.Drawing.Point(6, 32);
      this.pictureBoxPreviewWinds.Name = "pictureBoxPreviewWinds";
      this.pictureBoxPreviewWinds.Size = new System.Drawing.Size(240, 135);
      this.pictureBoxPreviewWinds.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxPreviewWinds.TabIndex = 32;
      this.pictureBoxPreviewWinds.TabStop = false;
      this.pictureBoxPreviewWinds.WaitOnLoad = true;
      // 
      // textBoxWindURL
      // 
      this.textBoxWindURL.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxWindURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxWindURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxWindURL.Name = "textBoxWindURL";
      this.textBoxWindURL.Size = new System.Drawing.Size(351, 20);
      this.textBoxWindURL.TabIndex = 31;
      // 
      // tabPageHumImg
      // 
      this.tabPageHumImg.Controls.Add(this.btnPreviewHumidity);
      this.tabPageHumImg.Controls.Add(this.pictureBoxPreviewHumidity);
      this.tabPageHumImg.Controls.Add(this.textBoxHumURL);
      this.tabPageHumImg.Location = new System.Drawing.Point(4, 22);
      this.tabPageHumImg.Name = "tabPageHumImg";
      this.tabPageHumImg.Size = new System.Drawing.Size(425, 173);
      this.tabPageHumImg.TabIndex = 4;
      this.tabPageHumImg.Text = "Humidity";
      this.tabPageHumImg.UseVisualStyleBackColor = true;
      // 
      // btnPreviewHumidity
      // 
      this.btnPreviewHumidity.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPreviewHumidity.Location = new System.Drawing.Point(363, 6);
      this.btnPreviewHumidity.Name = "btnPreviewHumidity";
      this.btnPreviewHumidity.Size = new System.Drawing.Size(56, 20);
      this.btnPreviewHumidity.TabIndex = 35;
      this.btnPreviewHumidity.Text = "Preview";
      this.btnPreviewHumidity.UseVisualStyleBackColor = true;
      this.btnPreviewHumidity.Click += new System.EventHandler(this.btnPreviewHumidity_Click);
      // 
      // pictureBoxPreviewHumidity
      // 
      this.pictureBoxPreviewHumidity.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBoxPreviewHumidity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pictureBoxPreviewHumidity.Location = new System.Drawing.Point(6, 32);
      this.pictureBoxPreviewHumidity.Name = "pictureBoxPreviewHumidity";
      this.pictureBoxPreviewHumidity.Size = new System.Drawing.Size(240, 135);
      this.pictureBoxPreviewHumidity.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxPreviewHumidity.TabIndex = 34;
      this.pictureBoxPreviewHumidity.TabStop = false;
      this.pictureBoxPreviewHumidity.WaitOnLoad = true;
      // 
      // textBoxHumURL
      // 
      this.textBoxHumURL.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxHumURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxHumURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxHumURL.Name = "textBoxHumURL";
      this.textBoxHumURL.Size = new System.Drawing.Size(351, 20);
      this.textBoxHumURL.TabIndex = 33;
      // 
      // tabPagePrecImg
      // 
      this.tabPagePrecImg.Controls.Add(this.btnPreviewPrecip);
      this.tabPagePrecImg.Controls.Add(this.pictureBoxPreviewPrecip);
      this.tabPagePrecImg.Controls.Add(this.textBoxPrecURL);
      this.tabPagePrecImg.Location = new System.Drawing.Point(4, 22);
      this.tabPagePrecImg.Name = "tabPagePrecImg";
      this.tabPagePrecImg.Size = new System.Drawing.Size(425, 173);
      this.tabPagePrecImg.TabIndex = 5;
      this.tabPagePrecImg.Text = "Precipitation";
      this.tabPagePrecImg.UseVisualStyleBackColor = true;
      // 
      // btnPreviewPrecip
      // 
      this.btnPreviewPrecip.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPreviewPrecip.Location = new System.Drawing.Point(363, 6);
      this.btnPreviewPrecip.Name = "btnPreviewPrecip";
      this.btnPreviewPrecip.Size = new System.Drawing.Size(56, 20);
      this.btnPreviewPrecip.TabIndex = 37;
      this.btnPreviewPrecip.Text = "Preview";
      this.btnPreviewPrecip.UseVisualStyleBackColor = true;
      this.btnPreviewPrecip.Click += new System.EventHandler(this.btnPreviewPrecip_Click);
      // 
      // pictureBoxPreviewPrecip
      // 
      this.pictureBoxPreviewPrecip.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBoxPreviewPrecip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pictureBoxPreviewPrecip.Location = new System.Drawing.Point(6, 32);
      this.pictureBoxPreviewPrecip.Name = "pictureBoxPreviewPrecip";
      this.pictureBoxPreviewPrecip.Size = new System.Drawing.Size(240, 135);
      this.pictureBoxPreviewPrecip.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxPreviewPrecip.TabIndex = 36;
      this.pictureBoxPreviewPrecip.TabStop = false;
      this.pictureBoxPreviewPrecip.WaitOnLoad = true;
      // 
      // textBoxPrecURL
      // 
      this.textBoxPrecURL.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxPrecURL.BorderColor = System.Drawing.Color.Empty;
      this.textBoxPrecURL.Location = new System.Drawing.Point(6, 6);
      this.textBoxPrecURL.Name = "textBoxPrecURL";
      this.textBoxPrecURL.Size = new System.Drawing.Size(351, 20);
      this.textBoxPrecURL.TabIndex = 35;
      // 
      // listBoxCityResults
      // 
      this.listBoxCityResults.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.listBoxCityResults.Location = new System.Drawing.Point(11, 34);
      this.listBoxCityResults.Name = "listBoxCityResults";
      this.listBoxCityResults.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.listBoxCityResults.Size = new System.Drawing.Size(433, 69);
      this.listBoxCityResults.TabIndex = 6;
      this.listBoxCityResults.SelectedIndexChanged +=
        new System.EventHandler(this.listBoxCityResults_SelectedIndexChanged);
      // 
      // labelCityResults
      // 
      this.labelCityResults.AutoSize = true;
      this.labelCityResults.Location = new System.Drawing.Point(8, 16);
      this.labelCityResults.Name = "labelCityResults";
      this.labelCityResults.Size = new System.Drawing.Size(65, 13);
      this.labelCityResults.TabIndex = 36;
      this.labelCityResults.Text = "Cities found:";
      // 
      // labelCity
      // 
      this.labelCity.AutoSize = true;
      this.labelCity.Location = new System.Drawing.Point(26, 24);
      this.labelCity.Name = "labelCity";
      this.labelCity.Size = new System.Drawing.Size(116, 13);
      this.labelCity.TabIndex = 35;
      this.labelCity.Text = "International city name:";
      // 
      // buttonCitySearch
      // 
      this.buttonCitySearch.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCitySearch.Enabled = false;
      this.buttonCitySearch.Location = new System.Drawing.Point(396, 21);
      this.buttonCitySearch.Name = "buttonCitySearch";
      this.buttonCitySearch.Size = new System.Drawing.Size(75, 20);
      this.buttonCitySearch.TabIndex = 1;
      this.buttonCitySearch.Text = "Search";
      this.buttonCitySearch.UseVisualStyleBackColor = true;
      this.buttonCitySearch.Click += new System.EventHandler(this.buttonCitySearch_Click);
      // 
      // buttonAddCity
      // 
      this.buttonAddCity.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonAddCity.Enabled = false;
      this.buttonAddCity.Location = new System.Drawing.Point(315, 406);
      this.buttonAddCity.Name = "buttonAddCity";
      this.buttonAddCity.Size = new System.Drawing.Size(75, 23);
      this.buttonAddCity.TabIndex = 1;
      this.buttonAddCity.Text = "&Add City";
      this.buttonAddCity.UseVisualStyleBackColor = true;
      this.buttonAddCity.Visible = false;
      this.buttonAddCity.Click += new System.EventHandler(this.buttonAddCity_Click);
      // 
      // searchTextBox
      // 
      this.searchTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.searchTextBox.BorderColor = System.Drawing.Color.Empty;
      this.searchTextBox.Location = new System.Drawing.Point(148, 21);
      this.searchTextBox.Name = "searchTextBox";
      this.searchTextBox.Size = new System.Drawing.Size(242, 20);
      this.searchTextBox.TabIndex = 0;
      this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
      this.searchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchTextBox_KeyDown);
      // 
      // SearchCityForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.buttonCancelCity;
      this.ClientSize = new System.Drawing.Size(506, 451);
      this.Controls.Add(this.groupBoxNewCity);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "SearchCityForm";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add new cities to the weather plugin";
      this.Load += new System.EventHandler(this.SearchCityForm_Load);
      this.groupBoxNewCity.ResumeLayout(false);
      this.groupBoxNewCity.PerformLayout();
      this.groupBoxCityDetails.ResumeLayout(false);
      this.groupBoxCityDetails.PerformLayout();
      this.tabControlCityURLs.ResumeLayout(false);
      this.tabPageSatImg.ResumeLayout(false);
      this.tabPageSatImg.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewSat)).EndInit();
      this.tabPageTempImg.ResumeLayout(false);
      this.tabPageTempImg.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewTemp)).EndInit();
      this.tabPageUVImg.ResumeLayout(false);
      this.tabPageUVImg.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewUV)).EndInit();
      this.tabPageWindsImg.ResumeLayout(false);
      this.tabPageWindsImg.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewWinds)).EndInit();
      this.tabPageHumImg.ResumeLayout(false);
      this.tabPageHumImg.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewHumidity)).EndInit();
      this.tabPagePrecImg.ResumeLayout(false);
      this.tabPagePrecImg.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreviewPrecip)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    private ArrayList selectedCities = new ArrayList();

    public ArrayList SelectedCities
    {
      get { return selectedCities; }
    }

    private void buttonCitySearch_Click(object sender, EventArgs e)
    {
      // Disable add button
      buttonAddCity.Enabled = false;

      try
      {
        // Perform actual search
        WeatherChannel weather = new WeatherChannel();
        ArrayList cities = weather.SearchCity(searchTextBox.Text);

        // Clear previous results
        listBoxCityResults.Items.Clear();

        foreach (WeatherChannel.City city in cities)
        {
          listBoxCityResults.Items.Add(city);

          if (listBoxCityResults.Items.Count == 1)
          {
            listBoxCityResults.SelectedItem = listBoxCityResults.Items[0];
          }
        }
        if (listBoxCityResults.Items.Count > 0)
        {
          groupBoxCityDetails.Visible = true;
          buttonCancelCity.Visible = true;
          buttonAddCity.Visible = true;
          this.Height = 488 + 40;
        }
        else
        {
          if (MessageBox.Show("No cities found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information) ==
              DialogResult.OK)
          {
            searchTextBox.Focus();
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void searchTextBox_TextChanged(object sender, EventArgs e)
    {
      buttonCitySearch.Enabled = searchTextBox.Text.Length > 0;
    }

    private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter && searchTextBox.Focused)
      {
        this.buttonCitySearch.PerformClick();
      }
    }

    private void buttonAddCity_Click(object sender, EventArgs e)
    {
      if (listBoxCityResults.SelectedItems.Count > 0)
      {
        foreach (WeatherChannel.City city in listBoxCityResults.SelectedItems)
        {
          selectedCities.Add(city);
        }

        this.DialogResult = DialogResult.OK;
      }
      this.Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void closeButton_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Hide();
    }

    private void listBoxCityResults_SelectedIndexChanged(object sender, EventArgs e)
    {
      buttonAddCity.Enabled = listBoxCityResults.SelectedItems.Count > 0;
    }

    private void SearchCityForm_Load(object sender, EventArgs e)
    {
      this.Height = 56 + 50;
      groupBoxCityDetails.Visible = false;
    }

    public string SatteliteImage
    {
      get { return textBoxSatURL.Text; }
      set { textBoxSatURL.Text = value; }
    }

    public string TemperatureImage
    {
      get { return textBoxTempURL.Text; }
      set { textBoxTempURL.Text = value; }
    }

    public string UVIndexImage
    {
      get { return textBoxUVURL.Text; }
      set { textBoxUVURL.Text = value; }
    }

    public string WindsImage
    {
      get { return textBoxWindURL.Text; }
      set { textBoxWindURL.Text = value; }
    }

    public string HumidityImage
    {
      get { return textBoxHumURL.Text; }
      set { textBoxHumURL.Text = value; }
    }

    public string PrecipitationImage
    {
      get { return textBoxPrecURL.Text; }
      set { textBoxPrecURL.Text = value; }
    }

    private void buttonCancelCity_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void lblWeatherDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Process.Start("http://wiki.team-mediaportal.com/MediaPortalSetup_WeatherImages");
      }
      catch (Exception) {}
    }

    private void SetPreviewImage(string aFilename, PictureBox aPreviewArea)
    {
      if (File.Exists(aFilename))
      {
        try
        {
          FileInfo fi = new FileInfo(aFilename);
          if (fi.Length > 0)
          {
            // No using since we need the image's lifetime
            Bitmap preview = new Bitmap(aFilename);
            aPreviewArea.Image = preview;
          }
          else
          {
            MessageBox.Show(this, string.Format("Your link is not working!"), "Invalid location", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(this, string.Format("No usable image detected! \n{0}", ex.Message), "Invalid data",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
      }
    }

    private void btnPreviewSat_Click(object sender, EventArgs e)
    {
      string tempFile = PathUtility.GetSecureTempFileName();
      Util.Utils.DownLoadImage(textBoxSatURL.Text, tempFile);
      SetPreviewImage(tempFile, pictureBoxPreviewSat);
    }

    private void btnPreviewTemp_Click(object sender, EventArgs e)
    {
      string tempFile = PathUtility.GetSecureTempFileName();
      Util.Utils.DownLoadImage(textBoxTempURL.Text, tempFile);
      SetPreviewImage(tempFile, pictureBoxPreviewTemp);
    }

    private void btnPreviewUV_Click(object sender, EventArgs e)
    {
      string tempFile = PathUtility.GetSecureTempFileName();
      Util.Utils.DownLoadImage(textBoxUVURL.Text, tempFile);
      SetPreviewImage(tempFile, pictureBoxPreviewUV);
    }

    private void btnPreviewWinds_Click(object sender, EventArgs e)
    {
      string tempFile = PathUtility.GetSecureTempFileName();
      Util.Utils.DownLoadImage(textBoxWindURL.Text, tempFile);
      SetPreviewImage(tempFile, pictureBoxPreviewWinds);
    }

    private void btnPreviewHumidity_Click(object sender, EventArgs e)
    {
      string tempFile = PathUtility.GetSecureTempFileName();
      Util.Utils.DownLoadImage(textBoxHumURL.Text, tempFile);
      SetPreviewImage(tempFile, pictureBoxPreviewHumidity);
    }

    private void btnPreviewPrecip_Click(object sender, EventArgs e)
    {
      string tempFile = PathUtility.GetSecureTempFileName();
      Util.Utils.DownLoadImage(textBoxPrecURL.Text, tempFile);
      SetPreviewImage(tempFile, pictureBoxPreviewPrecip);
    }
  }
}