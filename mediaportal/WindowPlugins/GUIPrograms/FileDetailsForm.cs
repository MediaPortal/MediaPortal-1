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
  public class FileDetailsForm: Form
  {
    private AppItem m_CurApp;
    private FileItem m_CurFile;
    private ConditionChecker m_Checker;
    private OpenFileDialog openFileDialog1;
    private ToolTip toolTip1;
    private Button btnOk;
    private Button btnCancel;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private TabControl tcFileItemData;
    private GroupBox gbFileDetails;
    private Button buttonViewImg;
    private TextBox txtFilepath;
    private Label label1;
    private ComboBox cbRating;
    private TextBox txtOverview;
    private Label lblOverview;
    private TextBox txtSystem;
    private Label lblSystem;
    private TextBox txtCountry;
    private Label lblCountry;
    private Label lblRating;
    private TextBox txtYear;
    private Label lblYear;
    private TextBox txtManufacturer;
    private Label lblManufacturer;
    private TextBox txtGenre;
    private Label lblGenre;
    private Label lblImageFile;
    private Button btnImageFile;
    private TextBox txtFilename;
    private TextBox txtImageFile;
    private TextBox txtTitle;
    private Label lblTitle;
    private Label lblFilename;
    private Button btnFilename;
    private GroupBox gbExtended;
    private Label lblTagData;
    private Label lblCategoryData;
    private TextBox txtTagData;
    private TextBox txtCategoryData;
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FileDetailsForm));
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.buttonViewImg = new System.Windows.Forms.Button();
      this.tcFileItemData = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.gbFileDetails = new System.Windows.Forms.GroupBox();
      this.txtFilepath = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbRating = new System.Windows.Forms.ComboBox();
      this.txtOverview = new System.Windows.Forms.TextBox();
      this.lblOverview = new System.Windows.Forms.Label();
      this.txtSystem = new System.Windows.Forms.TextBox();
      this.lblSystem = new System.Windows.Forms.Label();
      this.txtCountry = new System.Windows.Forms.TextBox();
      this.lblCountry = new System.Windows.Forms.Label();
      this.lblRating = new System.Windows.Forms.Label();
      this.txtYear = new System.Windows.Forms.TextBox();
      this.lblYear = new System.Windows.Forms.Label();
      this.txtManufacturer = new System.Windows.Forms.TextBox();
      this.lblManufacturer = new System.Windows.Forms.Label();
      this.txtGenre = new System.Windows.Forms.TextBox();
      this.lblGenre = new System.Windows.Forms.Label();
      this.lblImageFile = new System.Windows.Forms.Label();
      this.btnImageFile = new System.Windows.Forms.Button();
      this.txtFilename = new System.Windows.Forms.TextBox();
      this.txtImageFile = new System.Windows.Forms.TextBox();
      this.txtTitle = new System.Windows.Forms.TextBox();
      this.lblTitle = new System.Windows.Forms.Label();
      this.lblFilename = new System.Windows.Forms.Label();
      this.btnFilename = new System.Windows.Forms.Button();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.gbExtended = new System.Windows.Forms.GroupBox();
      this.txtCategoryData = new System.Windows.Forms.TextBox();
      this.lblCategoryData = new System.Windows.Forms.Label();
      this.txtTagData = new System.Windows.Forms.TextBox();
      this.lblTagData = new System.Windows.Forms.Label();
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
      this.btnOk.Location = new System.Drawing.Point(312, 472);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(80, 23);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "OK";
      this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(400, 472);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      // 
      // buttonViewImg
      // 
      this.buttonViewImg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonViewImg.Image = ((System.Drawing.Image)(resources.GetObject("buttonViewImg.Image")));
      this.buttonViewImg.Location = new System.Drawing.Point(399, 96);
      this.buttonViewImg.Name = "buttonViewImg";
      this.buttonViewImg.Size = new System.Drawing.Size(20, 20);
      this.buttonViewImg.TabIndex = 64;
      this.toolTip1.SetToolTip(this.buttonViewImg, "View the default image for this file");
      this.buttonViewImg.Click += new System.EventHandler(this.buttonViewImg_Click);
      // 
      // tcFileItemData
      // 
      this.tcFileItemData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tcFileItemData.Controls.Add(this.tabPage1);
      this.tcFileItemData.Controls.Add(this.tabPage2);
      this.tcFileItemData.Location = new System.Drawing.Point(8, 8);
      this.tcFileItemData.Name = "tcFileItemData";
      this.tcFileItemData.SelectedIndex = 0;
      this.tcFileItemData.Size = new System.Drawing.Size(488, 448);
      this.tcFileItemData.TabIndex = 3;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.gbFileDetails);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(480, 422);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Properties";
      // 
      // gbFileDetails
      // 
      this.gbFileDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
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
      this.gbFileDetails.Location = new System.Drawing.Point(8, 8);
      this.gbFileDetails.Name = "gbFileDetails";
      this.gbFileDetails.Size = new System.Drawing.Size(464, 408);
      this.gbFileDetails.TabIndex = 1;
      this.gbFileDetails.TabStop = false;
      // 
      // txtFilepath
      // 
      this.txtFilepath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtFilepath.Location = new System.Drawing.Point(88, 72);
      this.txtFilepath.Name = "txtFilepath";
      this.txtFilepath.ReadOnly = true;
      this.txtFilepath.Size = new System.Drawing.Size(332, 20);
      this.txtFilepath.TabIndex = 62;
      this.txtFilepath.Text = "";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 74);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 16);
      this.label1.TabIndex = 63;
      this.label1.Text = "Filepath:";
      // 
      // cbRating
      // 
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
      this.cbRating.Location = new System.Drawing.Point(230, 194);
      this.cbRating.Name = "cbRating";
      this.cbRating.Size = new System.Drawing.Size(192, 21);
      this.cbRating.TabIndex = 8;
      // 
      // txtOverview
      // 
      this.txtOverview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtOverview.Location = new System.Drawing.Point(8, 288);
      this.txtOverview.Multiline = true;
      this.txtOverview.Name = "txtOverview";
      this.txtOverview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtOverview.Size = new System.Drawing.Size(448, 110);
      this.txtOverview.TabIndex = 11;
      this.txtOverview.Text = "txtOverview";
      // 
      // lblOverview
      // 
      this.lblOverview.Location = new System.Drawing.Point(8, 272);
      this.lblOverview.Name = "lblOverview";
      this.lblOverview.Size = new System.Drawing.Size(100, 16);
      this.lblOverview.TabIndex = 61;
      this.lblOverview.Text = "Overview:";
      // 
      // txtSystem
      // 
      this.txtSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtSystem.Location = new System.Drawing.Point(88, 242);
      this.txtSystem.Name = "txtSystem";
      this.txtSystem.Size = new System.Drawing.Size(332, 20);
      this.txtSystem.TabIndex = 10;
      this.txtSystem.Text = "";
      // 
      // lblSystem
      // 
      this.lblSystem.Location = new System.Drawing.Point(8, 246);
      this.lblSystem.Name = "lblSystem";
      this.lblSystem.Size = new System.Drawing.Size(64, 16);
      this.lblSystem.TabIndex = 59;
      this.lblSystem.Text = "System:";
      // 
      // txtCountry
      // 
      this.txtCountry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCountry.Location = new System.Drawing.Point(88, 218);
      this.txtCountry.Name = "txtCountry";
      this.txtCountry.Size = new System.Drawing.Size(332, 20);
      this.txtCountry.TabIndex = 9;
      this.txtCountry.Text = "";
      // 
      // lblCountry
      // 
      this.lblCountry.Location = new System.Drawing.Point(8, 222);
      this.lblCountry.Name = "lblCountry";
      this.lblCountry.Size = new System.Drawing.Size(72, 16);
      this.lblCountry.TabIndex = 57;
      this.lblCountry.Text = "Country:";
      // 
      // lblRating
      // 
      this.lblRating.Location = new System.Drawing.Point(177, 197);
      this.lblRating.Name = "lblRating";
      this.lblRating.Size = new System.Drawing.Size(47, 16);
      this.lblRating.TabIndex = 55;
      this.lblRating.Text = "Rating:";
      // 
      // txtYear
      // 
      this.txtYear.Location = new System.Drawing.Point(88, 194);
      this.txtYear.MaxLength = 4;
      this.txtYear.Name = "txtYear";
      this.txtYear.Size = new System.Drawing.Size(48, 20);
      this.txtYear.TabIndex = 7;
      this.txtYear.Text = "";
      // 
      // lblYear
      // 
      this.lblYear.Location = new System.Drawing.Point(8, 194);
      this.lblYear.Name = "lblYear";
      this.lblYear.Size = new System.Drawing.Size(64, 16);
      this.lblYear.TabIndex = 53;
      this.lblYear.Text = "Year:";
      // 
      // txtManufacturer
      // 
      this.txtManufacturer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtManufacturer.Location = new System.Drawing.Point(88, 170);
      this.txtManufacturer.Name = "txtManufacturer";
      this.txtManufacturer.Size = new System.Drawing.Size(332, 20);
      this.txtManufacturer.TabIndex = 6;
      this.txtManufacturer.Text = "";
      // 
      // lblManufacturer
      // 
      this.lblManufacturer.Location = new System.Drawing.Point(8, 170);
      this.lblManufacturer.Name = "lblManufacturer";
      this.lblManufacturer.Size = new System.Drawing.Size(80, 16);
      this.lblManufacturer.TabIndex = 51;
      this.lblManufacturer.Text = "Manufacturer:";
      // 
      // txtGenre
      // 
      this.txtGenre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtGenre.Location = new System.Drawing.Point(88, 120);
      this.txtGenre.Multiline = true;
      this.txtGenre.Name = "txtGenre";
      this.txtGenre.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtGenre.Size = new System.Drawing.Size(332, 48);
      this.txtGenre.TabIndex = 5;
      this.txtGenre.Text = "";
      // 
      // lblGenre
      // 
      this.lblGenre.Location = new System.Drawing.Point(8, 124);
      this.lblGenre.Name = "lblGenre";
      this.lblGenre.Size = new System.Drawing.Size(64, 16);
      this.lblGenre.TabIndex = 49;
      this.lblGenre.Text = "Genre:";
      // 
      // lblImageFile
      // 
      this.lblImageFile.Location = new System.Drawing.Point(8, 100);
      this.lblImageFile.Name = "lblImageFile";
      this.lblImageFile.Size = new System.Drawing.Size(64, 16);
      this.lblImageFile.TabIndex = 47;
      this.lblImageFile.Text = "Imagefile:";
      // 
      // btnImageFile
      // 
      this.btnImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnImageFile.Image = ((System.Drawing.Image)(resources.GetObject("btnImageFile.Image")));
      this.btnImageFile.Location = new System.Drawing.Point(432, 96);
      this.btnImageFile.Name = "btnImageFile";
      this.btnImageFile.Size = new System.Drawing.Size(20, 20);
      this.btnImageFile.TabIndex = 4;
      this.btnImageFile.Click += new System.EventHandler(this.btnImageFile_Click);
      // 
      // txtFilename
      // 
      this.txtFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtFilename.Location = new System.Drawing.Point(88, 48);
      this.txtFilename.Name = "txtFilename";
      this.txtFilename.Size = new System.Drawing.Size(332, 20);
      this.txtFilename.TabIndex = 1;
      this.txtFilename.Text = "";
      // 
      // txtImageFile
      // 
      this.txtImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtImageFile.Location = new System.Drawing.Point(88, 96);
      this.txtImageFile.Name = "txtImageFile";
      this.txtImageFile.Size = new System.Drawing.Size(310, 20);
      this.txtImageFile.TabIndex = 3;
      this.txtImageFile.Text = "";
      // 
      // txtTitle
      // 
      this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTitle.Location = new System.Drawing.Point(88, 24);
      this.txtTitle.Name = "txtTitle";
      this.txtTitle.Size = new System.Drawing.Size(332, 20);
      this.txtTitle.TabIndex = 0;
      this.txtTitle.Text = "";
      // 
      // lblTitle
      // 
      this.lblTitle.Location = new System.Drawing.Point(8, 28);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(56, 16);
      this.lblTitle.TabIndex = 46;
      this.lblTitle.Text = "Title:";
      // 
      // lblFilename
      // 
      this.lblFilename.Location = new System.Drawing.Point(8, 50);
      this.lblFilename.Name = "lblFilename";
      this.lblFilename.Size = new System.Drawing.Size(64, 16);
      this.lblFilename.TabIndex = 45;
      this.lblFilename.Text = "Filename:";
      // 
      // btnFilename
      // 
      this.btnFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFilename.Image = ((System.Drawing.Image)(resources.GetObject("btnFilename.Image")));
      this.btnFilename.Location = new System.Drawing.Point(432, 48);
      this.btnFilename.Name = "btnFilename";
      this.btnFilename.Size = new System.Drawing.Size(20, 20);
      this.btnFilename.TabIndex = 2;
      this.btnFilename.Click += new System.EventHandler(this.btnFilename_Click);
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.gbExtended);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(480, 422);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Extended";
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
      this.gbExtended.Location = new System.Drawing.Point(8, 8);
      this.gbExtended.Name = "gbExtended";
      this.gbExtended.Size = new System.Drawing.Size(464, 408);
      this.gbExtended.TabIndex = 0;
      this.gbExtended.TabStop = false;
      // 
      // txtCategoryData
      // 
      this.txtCategoryData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCategoryData.Location = new System.Drawing.Point(8, 224);
      this.txtCategoryData.Multiline = true;
      this.txtCategoryData.Name = "txtCategoryData";
      this.txtCategoryData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtCategoryData.Size = new System.Drawing.Size(448, 176);
      this.txtCategoryData.TabIndex = 64;
      this.txtCategoryData.Text = "";
      // 
      // lblCategoryData
      // 
      this.lblCategoryData.Location = new System.Drawing.Point(8, 200);
      this.lblCategoryData.Name = "lblCategoryData";
      this.lblCategoryData.Size = new System.Drawing.Size(100, 16);
      this.lblCategoryData.TabIndex = 65;
      this.lblCategoryData.Text = "Category-Data:";
      // 
      // txtTagData
      // 
      this.txtTagData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.txtTagData.Location = new System.Drawing.Point(8, 32);
      this.txtTagData.Multiline = true;
      this.txtTagData.Name = "txtTagData";
      this.txtTagData.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtTagData.Size = new System.Drawing.Size(448, 144);
      this.txtTagData.TabIndex = 62;
      this.txtTagData.Text = "";
      // 
      // lblTagData
      // 
      this.lblTagData.Location = new System.Drawing.Point(8, 16);
      this.lblTagData.Name = "lblTagData";
      this.lblTagData.Size = new System.Drawing.Size(100, 16);
      this.lblTagData.TabIndex = 63;
      this.lblTagData.Text = "Tag-Data:";
      // 
      // FileDetailsForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(498, 504);
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
      this.tabPage2.ResumeLayout(false);
      this.gbExtended.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion 

    private void updateDisplay()
    {
      gbFileDetails.Text = CurApp.Title + ": " + CurFile.Title;
      txtTitle.Text = m_CurFile.Title;
      txtFilename.Text = m_CurFile.Filename;
      txtFilepath.Text = m_CurFile.Filepath;
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
      if (txtGenre.Lines.Length > 0){ CurFile.Genre = txtGenre.Lines[0]; }
      if (txtGenre.Lines.Length > 1){ CurFile.Genre2 = txtGenre.Lines[1]; }
      if (txtGenre.Lines.Length > 2){ CurFile.Genre3 = txtGenre.Lines[2]; }
      if (txtGenre.Lines.Length > 3){ CurFile.Genre4 = txtGenre.Lines[3]; }
      if (txtGenre.Lines.Length > 4){ CurFile.Genre5 = txtGenre.Lines[4]; }
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
      CurFile.Imagefile = txtImageFile.Text;
      GenreToFileItem();
      CurFile.Manufacturer = txtManufacturer.Text;
      CurFile.Year = ProgramUtils.StrToIntDef(txtYear.Text,  - 1);
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
