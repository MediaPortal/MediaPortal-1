#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Core.Util;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for FileDetailsForm.
  /// </summary>
  public class FileDetailsForm : Form
  {
    private AppItem m_CurApp;
    private FileItem m_CurFile;
    private ConditionChecker m_Checker;
    private OpenFileDialog openFileDialog1;
    private ToolTip toolTip1;
    private MediaPortal.UserInterface.Controls.MPButton btnOk;
    private MediaPortal.UserInterface.Controls.MPButton btnCancel;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPTabControl tcFileItemData;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbFileDetails;
    private MediaPortal.UserInterface.Controls.MPButton buttonViewImg;
    private MediaPortal.UserInterface.Controls.MPTextBox txtFilepath;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPComboBox cbRating;
    private MediaPortal.UserInterface.Controls.MPTextBox txtOverview;
    private MediaPortal.UserInterface.Controls.MPLabel lblOverview;
    private MediaPortal.UserInterface.Controls.MPTextBox txtSystem;
    private MediaPortal.UserInterface.Controls.MPLabel lblSystem;
    private MediaPortal.UserInterface.Controls.MPTextBox txtCountry;
    private MediaPortal.UserInterface.Controls.MPLabel lblCountry;
    private MediaPortal.UserInterface.Controls.MPLabel lblRating;
    private MediaPortal.UserInterface.Controls.MPTextBox txtYear;
    private MediaPortal.UserInterface.Controls.MPLabel lblYear;
    private MediaPortal.UserInterface.Controls.MPTextBox txtManufacturer;
    private MediaPortal.UserInterface.Controls.MPLabel lblManufacturer;
    private MediaPortal.UserInterface.Controls.MPTextBox txtGenre;
    private MediaPortal.UserInterface.Controls.MPLabel lblGenre;
    private MediaPortal.UserInterface.Controls.MPLabel lblImageFile;
    private MediaPortal.UserInterface.Controls.MPButton btnImageFile;
    private MediaPortal.UserInterface.Controls.MPTextBox txtFilename;
    private MediaPortal.UserInterface.Controls.MPTextBox txtImageFile;
    private MediaPortal.UserInterface.Controls.MPTextBox txtTitle;
    private MediaPortal.UserInterface.Controls.MPLabel lblTitle;
    private MediaPortal.UserInterface.Controls.MPLabel lblFilename;
    private MediaPortal.UserInterface.Controls.MPButton btnFilename;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbExtended;
    private MediaPortal.UserInterface.Controls.MPLabel lblTagData;
    private MediaPortal.UserInterface.Controls.MPLabel lblCategoryData;
    private MediaPortal.UserInterface.Controls.MPTextBox txtTagData;
    private MediaPortal.UserInterface.Controls.MPTextBox txtCategoryData;
    private MediaPortal.UserInterface.Controls.MPTextBox txtGameURL;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private IContainer components;

    public AppItem CurApp
    {
      get
      {
        return m_CurApp;
      }
      set
      {
        m_CurApp = value;
      }
    }

    public FileItem CurFile
    {
      get
      {
        return m_CurFile;
      }
      set
      {
        m_CurFile = value;
      }
    }

    public FileDetailsForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      m_Checker = new ConditionChecker();

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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileDetailsForm));
      this.btnOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.buttonViewImg = new MediaPortal.UserInterface.Controls.MPButton();
      this.tcFileItemData = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.gbFileDetails = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.txtFilepath = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbRating = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.txtOverview = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblOverview = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtSystem = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblSystem = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtCountry = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblCountry = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblRating = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtYear = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblYear = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtManufacturer = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblManufacturer = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtGenre = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblGenre = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblImageFile = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnImageFile = new MediaPortal.UserInterface.Controls.MPButton();
      this.txtFilename = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.txtImageFile = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.txtTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblTitle = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblFilename = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnFilename = new MediaPortal.UserInterface.Controls.MPButton();
      this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.gbExtended = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.txtCategoryData = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblCategoryData = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtTagData = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblTagData = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtGameURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tcFileItemData.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.gbFileDetails.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.gbExtended.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.Location = new System.Drawing.Point(312, 485);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(80, 25);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(400, 485);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 25);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // buttonViewImg
      // 
      this.buttonViewImg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonViewImg.Image = ((System.Drawing.Image)(resources.GetObject("buttonViewImg.Image")));
      this.buttonViewImg.Location = new System.Drawing.Point(400, 131);
      this.buttonViewImg.Name = "buttonViewImg";
      this.buttonViewImg.Size = new System.Drawing.Size(20, 22);
      this.buttonViewImg.TabIndex = 64;
      this.toolTip1.SetToolTip(this.buttonViewImg, "View the default image for this file");
      this.buttonViewImg.UseVisualStyleBackColor = true;
      this.buttonViewImg.Click += new System.EventHandler(this.buttonViewImg_Click);
      // 
      // tcFileItemData
      // 
      this.tcFileItemData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tcFileItemData.Controls.Add(this.tabPage1);
      this.tcFileItemData.Controls.Add(this.tabPage2);
      this.tcFileItemData.Location = new System.Drawing.Point(8, 9);
      this.tcFileItemData.Name = "tcFileItemData";
      this.tcFileItemData.SelectedIndex = 0;
      this.tcFileItemData.Size = new System.Drawing.Size(488, 459);
      this.tcFileItemData.TabIndex = 3;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.gbFileDetails);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(480, 433);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Properties";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // gbFileDetails
      // 
      this.gbFileDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbFileDetails.Controls.Add(this.mpLabel1);
      this.gbFileDetails.Controls.Add(this.txtGameURL);
      this.gbFileDetails.Controls.Add(this.buttonViewImg);
      this.gbFileDetails.Controls.Add(this.txtFilepath);
      this.gbFileDetails.Controls.Add(this.label1);
      this.gbFileDetails.Controls.Add(this.cbRating);
      this.gbFileDetails.Controls.Add(this.txtOverview);
      this.gbFileDetails.Controls.Add(this.lblOverview);
      this.gbFileDetails.Controls.Add(this.txtSystem);
      this.gbFileDetails.Controls.Add(this.lblSystem);
      this.gbFileDetails.Controls.Add(this.txtCountry);
      this.gbFileDetails.Controls.Add(this.lblCountry);
      this.gbFileDetails.Controls.Add(this.lblRating);
      this.gbFileDetails.Controls.Add(this.txtYear);
      this.gbFileDetails.Controls.Add(this.lblYear);
      this.gbFileDetails.Controls.Add(this.txtManufacturer);
      this.gbFileDetails.Controls.Add(this.lblManufacturer);
      this.gbFileDetails.Controls.Add(this.txtGenre);
      this.gbFileDetails.Controls.Add(this.lblGenre);
      this.gbFileDetails.Controls.Add(this.lblImageFile);
      this.gbFileDetails.Controls.Add(this.btnImageFile);
      this.gbFileDetails.Controls.Add(this.txtFilename);
      this.gbFileDetails.Controls.Add(this.txtImageFile);
      this.gbFileDetails.Controls.Add(this.txtTitle);
      this.gbFileDetails.Controls.Add(this.lblTitle);
      this.gbFileDetails.Controls.Add(this.lblFilename);
      this.gbFileDetails.Controls.Add(this.btnFilename);
      this.gbFileDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbFileDetails.Location = new System.Drawing.Point(8, 9);
      this.gbFileDetails.Name = "gbFileDetails";
      this.gbFileDetails.Size = new System.Drawing.Size(464, 416);
      this.gbFileDetails.TabIndex = 1;
      this.gbFileDetails.TabStop = false;
      // 
      // txtFilepath
      // 
      this.txtFilepath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtFilepath.BorderColor = System.Drawing.Color.Empty;
      this.txtFilepath.Location = new System.Drawing.Point(88, 78);
      this.txtFilepath.Name = "txtFilepath";
      this.txtFilepath.ReadOnly = true;
      this.txtFilepath.Size = new System.Drawing.Size(332, 21);
      this.txtFilepath.TabIndex = 62;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 80);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 17);
      this.label1.TabIndex = 63;
      this.label1.Text = "Filepath:";
      // 
      // cbRating
      // 
      this.cbRating.BorderColor = System.Drawing.Color.Empty;
      this.cbRating.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbRating.Items.AddRange(new object[] {
            "0 - poor",
            "1",
            "2",
            "3",
            "4",
            "5 - average",
            "6",
            "7",
            "8",
            "9",
            "10 - perfect"});
      this.cbRating.Location = new System.Drawing.Point(230, 244);
      this.cbRating.Name = "cbRating";
      this.cbRating.Size = new System.Drawing.Size(192, 21);
      this.cbRating.TabIndex = 8;
      // 
      // txtOverview
      // 
      this.txtOverview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtOverview.BorderColor = System.Drawing.Color.Empty;
      this.txtOverview.Location = new System.Drawing.Point(11, 342);
      this.txtOverview.Multiline = true;
      this.txtOverview.Name = "txtOverview";
      this.txtOverview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtOverview.Size = new System.Drawing.Size(441, 68);
      this.txtOverview.TabIndex = 11;
      this.txtOverview.Text = "txtOverview";
      // 
      // lblOverview
      // 
      this.lblOverview.Location = new System.Drawing.Point(8, 322);
      this.lblOverview.Name = "lblOverview";
      this.lblOverview.Size = new System.Drawing.Size(100, 17);
      this.lblOverview.TabIndex = 61;
      this.lblOverview.Text = "Overview:";
      // 
      // txtSystem
      // 
      this.txtSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtSystem.BorderColor = System.Drawing.Color.Empty;
      this.txtSystem.Location = new System.Drawing.Point(88, 298);
      this.txtSystem.Name = "txtSystem";
      this.txtSystem.Size = new System.Drawing.Size(332, 21);
      this.txtSystem.TabIndex = 10;
      // 
      // lblSystem
      // 
      this.lblSystem.Location = new System.Drawing.Point(8, 301);
      this.lblSystem.Name = "lblSystem";
      this.lblSystem.Size = new System.Drawing.Size(64, 17);
      this.lblSystem.TabIndex = 59;
      this.lblSystem.Text = "System:";
      // 
      // txtCountry
      // 
      this.txtCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCountry.BorderColor = System.Drawing.Color.Empty;
      this.txtCountry.Location = new System.Drawing.Point(88, 271);
      this.txtCountry.Name = "txtCountry";
      this.txtCountry.Size = new System.Drawing.Size(332, 21);
      this.txtCountry.TabIndex = 9;
      // 
      // lblCountry
      // 
      this.lblCountry.Location = new System.Drawing.Point(8, 274);
      this.lblCountry.Name = "lblCountry";
      this.lblCountry.Size = new System.Drawing.Size(72, 17);
      this.lblCountry.TabIndex = 57;
      this.lblCountry.Text = "Country:";
      // 
      // lblRating
      // 
      this.lblRating.Location = new System.Drawing.Point(177, 247);
      this.lblRating.Name = "lblRating";
      this.lblRating.Size = new System.Drawing.Size(47, 17);
      this.lblRating.TabIndex = 55;
      this.lblRating.Text = "Rating:";
      // 
      // txtYear
      // 
      this.txtYear.BorderColor = System.Drawing.Color.Empty;
      this.txtYear.Location = new System.Drawing.Point(88, 244);
      this.txtYear.MaxLength = 4;
      this.txtYear.Name = "txtYear";
      this.txtYear.Size = new System.Drawing.Size(48, 21);
      this.txtYear.TabIndex = 7;
      // 
      // lblYear
      // 
      this.lblYear.Location = new System.Drawing.Point(8, 247);
      this.lblYear.Name = "lblYear";
      this.lblYear.Size = new System.Drawing.Size(64, 17);
      this.lblYear.TabIndex = 53;
      this.lblYear.Text = "Year:";
      // 
      // txtManufacturer
      // 
      this.txtManufacturer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtManufacturer.BorderColor = System.Drawing.Color.Empty;
      this.txtManufacturer.Location = new System.Drawing.Point(88, 217);
      this.txtManufacturer.Name = "txtManufacturer";
      this.txtManufacturer.Size = new System.Drawing.Size(332, 21);
      this.txtManufacturer.TabIndex = 6;
      // 
      // lblManufacturer
      // 
      this.lblManufacturer.Location = new System.Drawing.Point(8, 226);
      this.lblManufacturer.Name = "lblManufacturer";
      this.lblManufacturer.Size = new System.Drawing.Size(80, 17);
      this.lblManufacturer.TabIndex = 51;
      this.lblManufacturer.Text = "Manufacturer:";
      // 
      // txtGenre
      // 
      this.txtGenre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtGenre.BorderColor = System.Drawing.Color.Empty;
      this.txtGenre.Location = new System.Drawing.Point(88, 159);
      this.txtGenre.Multiline = true;
      this.txtGenre.Name = "txtGenre";
      this.txtGenre.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtGenre.Size = new System.Drawing.Size(332, 52);
      this.txtGenre.TabIndex = 5;
      // 
      // lblGenre
      // 
      this.lblGenre.Location = new System.Drawing.Point(8, 162);
      this.lblGenre.Name = "lblGenre";
      this.lblGenre.Size = new System.Drawing.Size(64, 17);
      this.lblGenre.TabIndex = 49;
      this.lblGenre.Text = "Genre:";
      // 
      // lblImageFile
      // 
      this.lblImageFile.Location = new System.Drawing.Point(8, 137);
      this.lblImageFile.Name = "lblImageFile";
      this.lblImageFile.Size = new System.Drawing.Size(64, 17);
      this.lblImageFile.TabIndex = 47;
      this.lblImageFile.Text = "Imagefile:";
      // 
      // btnImageFile
      // 
      this.btnImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnImageFile.Image = ((System.Drawing.Image)(resources.GetObject("btnImageFile.Image")));
      this.btnImageFile.Location = new System.Drawing.Point(432, 132);
      this.btnImageFile.Name = "btnImageFile";
      this.btnImageFile.Size = new System.Drawing.Size(20, 22);
      this.btnImageFile.TabIndex = 4;
      this.btnImageFile.UseVisualStyleBackColor = true;
      this.btnImageFile.Click += new System.EventHandler(this.btnImageFile_Click);
      // 
      // txtFilename
      // 
      this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtFilename.BorderColor = System.Drawing.Color.Empty;
      this.txtFilename.Location = new System.Drawing.Point(88, 52);
      this.txtFilename.Name = "txtFilename";
      this.txtFilename.Size = new System.Drawing.Size(332, 21);
      this.txtFilename.TabIndex = 1;
      // 
      // txtImageFile
      // 
      this.txtImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtImageFile.BorderColor = System.Drawing.Color.Empty;
      this.txtImageFile.Location = new System.Drawing.Point(88, 132);
      this.txtImageFile.Name = "txtImageFile";
      this.txtImageFile.Size = new System.Drawing.Size(310, 21);
      this.txtImageFile.TabIndex = 3;
      // 
      // txtTitle
      // 
      this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTitle.BorderColor = System.Drawing.Color.Empty;
      this.txtTitle.Location = new System.Drawing.Point(88, 26);
      this.txtTitle.Name = "txtTitle";
      this.txtTitle.Size = new System.Drawing.Size(332, 21);
      this.txtTitle.TabIndex = 0;
      // 
      // lblTitle
      // 
      this.lblTitle.Location = new System.Drawing.Point(8, 30);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(56, 17);
      this.lblTitle.TabIndex = 46;
      this.lblTitle.Text = "Title:";
      // 
      // lblFilename
      // 
      this.lblFilename.Location = new System.Drawing.Point(8, 54);
      this.lblFilename.Name = "lblFilename";
      this.lblFilename.Size = new System.Drawing.Size(64, 17);
      this.lblFilename.TabIndex = 45;
      this.lblFilename.Text = "Filename:";
      // 
      // btnFilename
      // 
      this.btnFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFilename.Image = ((System.Drawing.Image)(resources.GetObject("btnFilename.Image")));
      this.btnFilename.Location = new System.Drawing.Point(432, 52);
      this.btnFilename.Name = "btnFilename";
      this.btnFilename.Size = new System.Drawing.Size(20, 21);
      this.btnFilename.TabIndex = 2;
      this.btnFilename.UseVisualStyleBackColor = true;
      this.btnFilename.Click += new System.EventHandler(this.btnFilename_Click);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.gbExtended);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(480, 456);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Extended";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // gbExtended
      // 
      this.gbExtended.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbExtended.Controls.Add(this.txtCategoryData);
      this.gbExtended.Controls.Add(this.lblCategoryData);
      this.gbExtended.Controls.Add(this.txtTagData);
      this.gbExtended.Controls.Add(this.lblTagData);
      this.gbExtended.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbExtended.Location = new System.Drawing.Point(8, 9);
      this.gbExtended.Name = "gbExtended";
      this.gbExtended.Size = new System.Drawing.Size(464, 439);
      this.gbExtended.TabIndex = 0;
      this.gbExtended.TabStop = false;
      // 
      // txtCategoryData
      // 
      this.txtCategoryData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCategoryData.BorderColor = System.Drawing.Color.Empty;
      this.txtCategoryData.Location = new System.Drawing.Point(8, 241);
      this.txtCategoryData.Multiline = true;
      this.txtCategoryData.Name = "txtCategoryData";
      this.txtCategoryData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtCategoryData.Size = new System.Drawing.Size(448, 190);
      this.txtCategoryData.TabIndex = 64;
      // 
      // lblCategoryData
      // 
      this.lblCategoryData.Location = new System.Drawing.Point(8, 215);
      this.lblCategoryData.Name = "lblCategoryData";
      this.lblCategoryData.Size = new System.Drawing.Size(100, 18);
      this.lblCategoryData.TabIndex = 65;
      this.lblCategoryData.Text = "Category-Data:";
      // 
      // txtTagData
      // 
      this.txtTagData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTagData.BorderColor = System.Drawing.Color.Empty;
      this.txtTagData.Location = new System.Drawing.Point(8, 34);
      this.txtTagData.Multiline = true;
      this.txtTagData.Name = "txtTagData";
      this.txtTagData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtTagData.Size = new System.Drawing.Size(448, 156);
      this.txtTagData.TabIndex = 62;
      // 
      // lblTagData
      // 
      this.lblTagData.Location = new System.Drawing.Point(8, 17);
      this.lblTagData.Name = "lblTagData";
      this.lblTagData.Size = new System.Drawing.Size(100, 17);
      this.lblTagData.TabIndex = 63;
      this.lblTagData.Text = "Tag-Data:";
      // 
      // txtGameURL
      // 
      this.txtGameURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtGameURL.BorderColor = System.Drawing.Color.Empty;
      this.txtGameURL.Location = new System.Drawing.Point(88, 105);
      this.txtGameURL.Name = "txtGameURL";
      this.txtGameURL.Size = new System.Drawing.Size(332, 21);
      this.txtGameURL.TabIndex = 65;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(8, 108);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(64, 17);
      this.mpLabel1.TabIndex = 66;
      this.mpLabel1.Text = "URL:";
      // 
      // FileDetailsForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
      this.ClientSize = new System.Drawing.Size(498, 520);
      this.Controls.Add(this.tcFileItemData);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "FileDetailsForm";
      this.Text = "File-Details";
      this.Load += new System.EventHandler(this.FileDetailsForm_Load);
      this.tcFileItemData.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.gbFileDetails.ResumeLayout(false);
      this.gbFileDetails.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.gbExtended.ResumeLayout(false);
      this.gbExtended.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    private void updateDisplay()
    {
      gbFileDetails.Text = CurApp.Title + ": " + CurFile.Title;
      txtTitle.Text = m_CurFile.Title;
      txtFilename.Text = m_CurFile.Filename;
      txtFilepath.Text = m_CurFile.Filepath;
      txtGameURL.Text = m_CurFile.GameURL;
      txtImageFile.Text = m_CurFile.Imagefile;
      FileItemToGenre();
      txtManufacturer.Text = m_CurFile.Manufacturer;
      if (m_CurFile.Year > 1900)
      {
        txtYear.Text = m_CurFile.Year.ToString();
      }
      else
      {
        txtYear.Text = "";
      }
      cbRating.SelectedIndex = m_CurFile.Rating;
      txtCountry.Text = m_CurFile.Country;
      txtSystem.Text = m_CurFile.System_;
      txtOverview.Text = m_CurFile.Overview;
      txtTagData.Text = m_CurFile.TagData;
      txtCategoryData.Text = m_CurFile.CategoryData;
    }

    private void FileDetailsForm_Load(object sender, EventArgs e)
    {
      tcFileItemData.TabIndex = 0;
      updateDisplay();
    }

    private void btnFilename_Click(object sender, EventArgs e)
    {
      if (File.Exists(txtFilename.Text))
      {
        openFileDialog1.FileName = txtFilename.Text;
      }
      openFileDialog1.RestoreDirectory = true;
      if (openFileDialog1.ShowDialog(null) == DialogResult.OK)
      {
        txtFilename.Text = openFileDialog1.FileName;
        if ((txtTitle.Text == "") && (txtFilename.Text != "") && (File.Exists(txtFilename.Text)))
        {
          txtTitle.Text = Path.GetFileNameWithoutExtension(txtFilename.Text);
        }
      }
    }

    private void btnImageFile_Click(object sender, EventArgs e)
    {
      openFileDialog1.FileName = txtImageFile.Text;
      openFileDialog1.RestoreDirectory = true;
      if (openFileDialog1.ShowDialog(null) == DialogResult.OK)
      {
        txtImageFile.Text = openFileDialog1.FileName;
      }
    }


    private bool EntriesOK()
    {
      m_Checker.Clear();
      m_Checker.DoCheck(CurFile.Title != "", "No title entered!");
      //01.04.05 no filename is FINE :-)			m_Checker.DoCheck(CurFile.Filename != "", "No filename entered!");
      if (!m_Checker.IsOk)
      {
        string strHeader = "The following entries are invalid: \r\n\r\n";
        MessageBox.Show(strHeader + m_Checker.Problems, "Invalid Entries");
      }
      return m_Checker.IsOk;
    }

    void GenreToFileItem()
    {
      CurFile.Genre = "";
      CurFile.Genre2 = "";
      CurFile.Genre3 = "";
      CurFile.Genre4 = "";
      CurFile.Genre5 = "";
      if (txtGenre.Lines.Length > 0) { CurFile.Genre = txtGenre.Lines[0]; }
      if (txtGenre.Lines.Length > 1) { CurFile.Genre2 = txtGenre.Lines[1]; }
      if (txtGenre.Lines.Length > 2) { CurFile.Genre3 = txtGenre.Lines[2]; }
      if (txtGenre.Lines.Length > 3) { CurFile.Genre4 = txtGenre.Lines[3]; }
      if (txtGenre.Lines.Length > 4) { CurFile.Genre5 = txtGenre.Lines[4]; }
    }

    void FileItemToGenre()
    {
      string sep = "";
      txtGenre.Text = "";
      if (CurFile.Genre != "")
      {
        txtGenre.Text = txtGenre.Text + sep + CurFile.Genre;
        sep = "\r\n";
      }
      if (CurFile.Genre2 != "")
      {
        txtGenre.Text = txtGenre.Text + sep + CurFile.Genre2;
        sep = "\r\n";
      }
      if (CurFile.Genre3 != "")
      {
        txtGenre.Text = txtGenre.Text + sep + CurFile.Genre3;
        sep = "\r\n";
      }
      if (CurFile.Genre4 != "")
      {
        txtGenre.Text = txtGenre.Text + sep + CurFile.Genre4;
        sep = "\r\n";
      }
      if (CurFile.Genre5 != "")
      {
        txtGenre.Text = txtGenre.Text + sep + CurFile.Genre5;
        sep = "\r\n";
      }
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      CurFile.Title = txtTitle.Text;
      CurFile.Filename = txtFilename.Text;
      CurFile.Filepath = txtFilepath.Text;
      CurFile.GameURL = txtGameURL.Text;
      CurFile.Imagefile = txtImageFile.Text;
      GenreToFileItem();
      CurFile.Manufacturer = txtManufacturer.Text;
      CurFile.Year = ProgramUtils.StrToIntDef(txtYear.Text, -1);
      CurFile.Rating = cbRating.SelectedIndex;
      CurFile.Country = txtCountry.Text;
      CurFile.System_ = txtSystem.Text;
      CurFile.Overview = txtOverview.Text;
      CurFile.TagData = txtTagData.Text;
      CurFile.CategoryData = txtCategoryData.Text;
      if (EntriesOK())
      {
        this.DialogResult = DialogResult.OK;
        this.Close();
      }

    }

    private void buttonViewImg_Click(object sender, EventArgs e)
    {
      try
      {
        if (File.Exists(txtImageFile.Text))
        {
          ProcessStartInfo sInfo = new ProcessStartInfo(txtImageFile.Text);
          Process.Start(sInfo);
        }
      }
      catch
      {
        // do nothing! 
      }
    }

  }
}
