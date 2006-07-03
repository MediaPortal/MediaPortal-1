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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for AppSettingsMameDirect.
  /// </summary>
  public class AppSettingsMameDirect : AppSettings
  {
    private MediaPortal.UserInterface.Controls.MPLabel LblPinCode;
    private MediaPortal.UserInterface.Controls.MPTextBox txtPinCode;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkbValidImagesOnly;
    private MediaPortal.UserInterface.Controls.MPLabel lblImgDirectories;
    private MediaPortal.UserInterface.Controls.MPTextBox txtImageDirs;
    private MediaPortal.UserInterface.Controls.MPButton btnImageDirs;
    private MediaPortal.UserInterface.Controls.MPComboBox cbWindowStyle;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPTextBox txtArguments;
    private MediaPortal.UserInterface.Controls.MPLabel lblArg;
    private MediaPortal.UserInterface.Controls.MPLabel lblImageFile;
    private MediaPortal.UserInterface.Controls.MPButton buttonImageFile;
    private MediaPortal.UserInterface.Controls.MPTextBox txtImageFile;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkbEnabled;
    private MediaPortal.UserInterface.Controls.MPTextBox txtTitle;
    private MediaPortal.UserInterface.Controls.MPLabel lblTitle;
    private MediaPortal.UserInterface.Controls.MPLabel lblFilename;
    private MediaPortal.UserInterface.Controls.MPButton buttonLaunchingApp;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPTextBox edFilename;
    private MediaPortal.UserInterface.Controls.MPButton buttonFileDirectory;
    private MediaPortal.UserInterface.Controls.MPTextBox txtFiles;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private System.Windows.Forms.Panel panelAdditionals;
    private System.Windows.Forms.LinkLabel historyDatLink;
    private System.Windows.Forms.LinkLabel catverLink;
    private MediaPortal.UserInterface.Controls.MPLabel labelBestResults;
    private MediaPortal.UserInterface.Controls.MPCheckBox chkbOriginalsOnly;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public AppSettingsMameDirect()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();
      this.txtPinCode.PasswordChar = (char)0x25CF;
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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AppSettingsMameDirect));
      this.LblPinCode = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtPinCode = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.chkbValidImagesOnly = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.lblImgDirectories = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtImageDirs = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.btnImageDirs = new MediaPortal.UserInterface.Controls.MPButton();
      this.cbWindowStyle = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.txtArguments = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblArg = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblImageFile = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonImageFile = new MediaPortal.UserInterface.Controls.MPButton();
      this.txtImageFile = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.chkbEnabled = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.txtTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lblTitle = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblFilename = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonLaunchingApp = new MediaPortal.UserInterface.Controls.MPButton();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.edFilename = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonFileDirectory = new MediaPortal.UserInterface.Controls.MPButton();
      this.txtFiles = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.panelAdditionals = new System.Windows.Forms.Panel();
      this.historyDatLink = new System.Windows.Forms.LinkLabel();
      this.catverLink = new System.Windows.Forms.LinkLabel();
      this.labelBestResults = new MediaPortal.UserInterface.Controls.MPLabel();
      this.chkbOriginalsOnly = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.panelAdditionals.SuspendLayout();
      this.SuspendLayout();
      // 
      // LblPinCode
      // 
      this.LblPinCode.Location = new System.Drawing.Point(0, 88);
      this.LblPinCode.Name = "LblPinCode";
      this.LblPinCode.Size = new System.Drawing.Size(96, 16);
      this.LblPinCode.TabIndex = 42;
      this.LblPinCode.Text = "Pin-Code";
      // 
      // txtPinCode
      // 
      this.txtPinCode.Location = new System.Drawing.Point(120, 88);
      this.txtPinCode.MaxLength = 4;
      this.txtPinCode.Name = "txtPinCode";
      this.txtPinCode.Size = new System.Drawing.Size(64, 20);
      this.txtPinCode.TabIndex = 44;
      this.txtPinCode.Text = "";
      this.txtPinCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPinCode_KeyPress);
      // 
      // chkbValidImagesOnly
      // 
      this.chkbValidImagesOnly.Checked = true;
      this.chkbValidImagesOnly.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbValidImagesOnly.Location = new System.Drawing.Point(120, 184);
      this.chkbValidImagesOnly.Name = "chkbValidImagesOnly";
      this.chkbValidImagesOnly.Size = new System.Drawing.Size(224, 24);
      this.chkbValidImagesOnly.TabIndex = 59;
      this.chkbValidImagesOnly.Text = "Only import files with valid images";
      // 
      // lblImgDirectories
      // 
      this.lblImgDirectories.Location = new System.Drawing.Point(0, 264);
      this.lblImgDirectories.Name = "lblImgDirectories";
      this.lblImgDirectories.TabIndex = 60;
      this.lblImgDirectories.Text = "Image directories:";
      // 
      // txtImageDirs
      // 
      this.txtImageDirs.Location = new System.Drawing.Point(120, 264);
      this.txtImageDirs.Multiline = true;
      this.txtImageDirs.Name = "txtImageDirs";
      this.txtImageDirs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtImageDirs.Size = new System.Drawing.Size(250, 88);
      this.txtImageDirs.TabIndex = 61;
      this.txtImageDirs.Text = "txtImageDirs";
      // 
      // btnImageDirs
      // 
      this.btnImageDirs.Image = ((System.Drawing.Image)(resources.GetObject("btnImageDirs.Image")));
      this.btnImageDirs.Location = new System.Drawing.Point(376, 264);
      this.btnImageDirs.Name = "btnImageDirs";
      this.btnImageDirs.Size = new System.Drawing.Size(20, 20);
      this.btnImageDirs.TabIndex = 62;
      this.btnImageDirs.Click += new System.EventHandler(this.btnImageDirs_Click);
      // 
      // cbWindowStyle
      // 
      this.cbWindowStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbWindowStyle.Items.AddRange(new object[] {
                                                       "Normal",
                                                       "Minimized",
                                                       "Maximized",
                                                       "Hidden"});
      this.cbWindowStyle.Location = new System.Drawing.Point(120, 160);
      this.cbWindowStyle.Name = "cbWindowStyle";
      this.cbWindowStyle.Size = new System.Drawing.Size(250, 21);
      this.cbWindowStyle.TabIndex = 51;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(0, 160);
      this.label6.Name = "label6";
      this.label6.TabIndex = 50;
      this.label6.Text = "Window-Style:";
      // 
      // txtArguments
      // 
      this.txtArguments.Location = new System.Drawing.Point(120, 136);
      this.txtArguments.Name = "txtArguments";
      this.txtArguments.Size = new System.Drawing.Size(250, 20);
      this.txtArguments.TabIndex = 49;
      this.txtArguments.Text = "-joy -skip_disclaimer -skip_gameinfo";
      // 
      // lblArg
      // 
      this.lblArg.Location = new System.Drawing.Point(0, 136);
      this.lblArg.Name = "lblArg";
      this.lblArg.TabIndex = 48;
      this.lblArg.Text = "Arguments:";
      // 
      // lblImageFile
      // 
      this.lblImageFile.Location = new System.Drawing.Point(0, 112);
      this.lblImageFile.Name = "lblImageFile";
      this.lblImageFile.Size = new System.Drawing.Size(80, 20);
      this.lblImageFile.TabIndex = 45;
      this.lblImageFile.Text = "Imagefile:";
      // 
      // buttonImageFile
      // 
      this.buttonImageFile.Image = ((System.Drawing.Image)(resources.GetObject("buttonImageFile.Image")));
      this.buttonImageFile.Location = new System.Drawing.Point(376, 112);
      this.buttonImageFile.Name = "buttonImageFile";
      this.buttonImageFile.Size = new System.Drawing.Size(20, 20);
      this.buttonImageFile.TabIndex = 47;
      this.buttonImageFile.Click += new System.EventHandler(this.buttonImageFile_Click);
      // 
      // txtImageFile
      // 
      this.txtImageFile.Location = new System.Drawing.Point(120, 112);
      this.txtImageFile.Name = "txtImageFile";
      this.txtImageFile.Size = new System.Drawing.Size(250, 20);
      this.txtImageFile.TabIndex = 46;
      this.txtImageFile.Text = "";
      // 
      // chkbEnabled
      // 
      this.chkbEnabled.Checked = true;
      this.chkbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.chkbEnabled.Location = new System.Drawing.Point(320, 8);
      this.chkbEnabled.Name = "chkbEnabled";
      this.chkbEnabled.Size = new System.Drawing.Size(72, 24);
      this.chkbEnabled.TabIndex = 64;
      this.chkbEnabled.Text = "Enabled";
      this.chkbEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // txtTitle
      // 
      this.txtTitle.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.txtTitle.Location = new System.Drawing.Point(120, 40);
      this.txtTitle.Name = "txtTitle";
      this.txtTitle.Size = new System.Drawing.Size(250, 20);
      this.txtTitle.TabIndex = 39;
      this.txtTitle.Text = "MAME";
      // 
      // lblTitle
      // 
      this.lblTitle.Location = new System.Drawing.Point(0, 40);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.TabIndex = 38;
      this.lblTitle.Text = "Title:";
      // 
      // lblFilename
      // 
      this.lblFilename.Location = new System.Drawing.Point(0, 64);
      this.lblFilename.Name = "lblFilename";
      this.lblFilename.Size = new System.Drawing.Size(120, 20);
      this.lblFilename.TabIndex = 40;
      this.lblFilename.Text = "Launching Application:";
      // 
      // buttonLaunchingApp
      // 
      this.buttonLaunchingApp.Image = ((System.Drawing.Image)(resources.GetObject("buttonLaunchingApp.Image")));
      this.buttonLaunchingApp.Location = new System.Drawing.Point(376, 64);
      this.buttonLaunchingApp.Name = "buttonLaunchingApp";
      this.buttonLaunchingApp.Size = new System.Drawing.Size(20, 20);
      this.buttonLaunchingApp.TabIndex = 41;
      this.buttonLaunchingApp.Click += new System.EventHandler(this.buttonLaunchingApp_Click);
      // 
      // label3
      // 
      this.label3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.label3.Location = new System.Drawing.Point(0, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(248, 32);
      this.label3.TabIndex = 37;
      this.label3.Text = "MAME Direct Importer";
      // 
      // edFilename
      // 
      this.edFilename.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
      this.edFilename.Location = new System.Drawing.Point(120, 64);
      this.edFilename.Name = "edFilename";
      this.edFilename.Size = new System.Drawing.Size(250, 20);
      this.edFilename.TabIndex = 69;
      this.edFilename.Text = "<yourpath>mame.exe";
      // 
      // buttonFileDirectory
      // 
      this.buttonFileDirectory.Image = ((System.Drawing.Image)(resources.GetObject("buttonFileDirectory.Image")));
      this.buttonFileDirectory.Location = new System.Drawing.Point(376, 240);
      this.buttonFileDirectory.Name = "buttonFileDirectory";
      this.buttonFileDirectory.Size = new System.Drawing.Size(20, 20);
      this.buttonFileDirectory.TabIndex = 72;
      this.buttonFileDirectory.Click += new System.EventHandler(this.buttonFileDirectory_Click);
      // 
      // txtFiles
      // 
      this.txtFiles.Location = new System.Drawing.Point(120, 240);
      this.txtFiles.Name = "txtFiles";
      this.txtFiles.Size = new System.Drawing.Size(250, 20);
      this.txtFiles.TabIndex = 71;
      this.txtFiles.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(0, 240);
      this.label2.Name = "label2";
      this.label2.TabIndex = 70;
      this.label2.Text = "Rom Directory:";
      // 
      // panelAdditionals
      // 
      this.panelAdditionals.Controls.Add(this.historyDatLink);
      this.panelAdditionals.Controls.Add(this.catverLink);
      this.panelAdditionals.Controls.Add(this.labelBestResults);
      this.panelAdditionals.Location = new System.Drawing.Point(8, 368);
      this.panelAdditionals.Name = "panelAdditionals";
      this.panelAdditionals.Size = new System.Drawing.Size(384, 48);
      this.panelAdditionals.TabIndex = 79;
      // 
      // historyDatLink
      // 
      this.historyDatLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.historyDatLink.Location = new System.Drawing.Point(224, 24);
      this.historyDatLink.Name = "historyDatLink";
      this.historyDatLink.Size = new System.Drawing.Size(160, 16);
      this.historyDatLink.TabIndex = 81;
      this.historyDatLink.TabStop = true;
      this.historyDatLink.Text = "http://www.arcade-history.com";
      this.historyDatLink.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.historyDatLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.historyDatLink_LinkClicked);
      // 
      // catverLink
      // 
      this.catverLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.catverLink.Location = new System.Drawing.Point(224, 8);
      this.catverLink.Name = "catverLink";
      this.catverLink.Size = new System.Drawing.Size(160, 16);
      this.catverLink.TabIndex = 80;
      this.catverLink.TabStop = true;
      this.catverLink.Text = "http://www.catver.com";
      this.catverLink.TextAlign = System.Drawing.ContentAlignment.TopRight;
      this.catverLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.catverLink_LinkClicked);
      // 
      // labelBestResults
      // 
      this.labelBestResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.labelBestResults.BackColor = System.Drawing.SystemColors.Highlight;
      this.labelBestResults.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
      this.labelBestResults.Location = new System.Drawing.Point(0, 0);
      this.labelBestResults.Name = "labelBestResults";
      this.labelBestResults.Size = new System.Drawing.Size(216, 48);
      this.labelBestResults.TabIndex = 79;
      this.labelBestResults.Text = "For best results, you need a current CATVER.INI and a current HISTORY.DAT file in" +
        " the mame directory!";
      this.labelBestResults.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // chkbOriginalsOnly
      // 
      this.chkbOriginalsOnly.Checked = true;
      this.chkbOriginalsOnly.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkbOriginalsOnly.Location = new System.Drawing.Point(120, 208);
      this.chkbOriginalsOnly.Name = "chkbOriginalsOnly";
      this.chkbOriginalsOnly.Size = new System.Drawing.Size(224, 24);
      this.chkbOriginalsOnly.TabIndex = 80;
      this.chkbOriginalsOnly.Text = "Only import Originals (no clones)";
      // 
      // AppSettingsMameDirect
      // 
      this.Controls.Add(this.chkbOriginalsOnly);
      this.Controls.Add(this.panelAdditionals);
      this.Controls.Add(this.buttonFileDirectory);
      this.Controls.Add(this.txtFiles);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.edFilename);
      this.Controls.Add(this.LblPinCode);
      this.Controls.Add(this.txtPinCode);
      this.Controls.Add(this.chkbValidImagesOnly);
      this.Controls.Add(this.lblImgDirectories);
      this.Controls.Add(this.txtImageDirs);
      this.Controls.Add(this.btnImageDirs);
      this.Controls.Add(this.cbWindowStyle);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.txtArguments);
      this.Controls.Add(this.lblArg);
      this.Controls.Add(this.lblImageFile);
      this.Controls.Add(this.buttonImageFile);
      this.Controls.Add(this.txtImageFile);
      this.Controls.Add(this.chkbEnabled);
      this.Controls.Add(this.txtTitle);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.lblFilename);
      this.Controls.Add(this.buttonLaunchingApp);
      this.Controls.Add(this.label3);
      this.Name = "AppSettingsMameDirect";
      this.Size = new System.Drawing.Size(400, 424);
      this.Load += new System.EventHandler(this.AppSettingsMameDirect_Load);
      this.panelAdditionals.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion


    private void SetWindowStyle(ProcessWindowStyle val)
    {
      switch (val)
      {
        case ProcessWindowStyle.Normal:
          cbWindowStyle.SelectedIndex = 0;
          break;
        case ProcessWindowStyle.Minimized:
          cbWindowStyle.SelectedIndex = 1;
          break;
        case ProcessWindowStyle.Maximized:
          cbWindowStyle.SelectedIndex = 2;
          break;
        case ProcessWindowStyle.Hidden:
          cbWindowStyle.SelectedIndex = 3;
          break;
      }

    }

    private ProcessWindowStyle GetSelectedWindowStyle()
    {
      if (this.cbWindowStyle.SelectedIndex == 1)
      {
        return ProcessWindowStyle.Minimized;
      }
      else if (this.cbWindowStyle.SelectedIndex == 2)
      {
        return ProcessWindowStyle.Maximized;
      }
      else if (this.cbWindowStyle.SelectedIndex == 3)
      {
        return ProcessWindowStyle.Hidden;
      }
      else
        return ProcessWindowStyle.Normal;

    }

    private void AppSettingsMameDirect_Load(object sender, System.EventArgs e)
    {
      cbWindowStyle.SelectedIndex = 1; // minimized

    }

    public override bool AppObj2Form(AppItem curApp)
    {
      base.AppObj2Form(curApp);
      this.chkbEnabled.Checked = curApp.Enabled;
      this.txtTitle.Text = curApp.Title;
      this.edFilename.Text = curApp.Filename;
      this.txtArguments.Text = curApp.Arguments;
      SetWindowStyle(curApp.WindowStyle);
      this.txtImageFile.Text = curApp.Imagefile;
      this.txtImageDirs.Text = curApp.ImageDirectory;
      this.txtFiles.Text = curApp.FileDirectory;
      this.chkbValidImagesOnly.Checked = curApp.ImportValidImagesOnly;
      this.chkbOriginalsOnly.Checked = ((appItemMameDirect)curApp).ImportOriginalsOnly;
      if (curApp.Pincode > 0)
      {
        this.txtPinCode.Text = String.Format("{0}", curApp.Pincode);
      }
      else
      {
        this.txtPinCode.Text = "";
      }
      return true;
    }


    public override void Form2AppObj(AppItem curApp)
    {
      base.Form2AppObj(curApp);
      curApp.Enabled = this.chkbEnabled.Checked;
      curApp.Title = this.txtTitle.Text;
      curApp.Filename = this.edFilename.Text;
      curApp.Arguments = this.txtArguments.Text;
      curApp.WindowStyle = GetSelectedWindowStyle();
      curApp.SourceType = myProgSourceType.MAMEDIRECT;
      curApp.Imagefile = this.txtImageFile.Text;
      curApp.FileDirectory = this.txtFiles.Text;
      curApp.ImageDirectory = this.txtImageDirs.Text;
      curApp.ImportValidImagesOnly = this.chkbValidImagesOnly.Checked;
      ((appItemMameDirect)curApp).ImportOriginalsOnly = chkbOriginalsOnly.Checked;
      curApp.Pincode = ProgramUtils.StrToIntDef(this.txtPinCode.Text, -1);
    }

    public override bool EntriesOK(AppItem curApp)
    {
      m_Checker.Clear();
      m_Checker.DoCheck(txtTitle.Text != "", "No title entered!");
      m_Checker.DoCheck(edFilename.Text != "", "No launching filename entered!");
      m_Checker.DoCheck(txtFiles.Text != "", "No filedirectory entered!");
      if (!m_Checker.IsOk)
      {
        string strHeader = "The following entries are invalid: \r\n\r\n";
        string strFooter = "\r\n\r\n(Click DELETE to remove this item)";
        MessageBox.Show(strHeader + m_Checker.Problems + strFooter, "Invalid Entries");
      }
      else
      { }
      return m_Checker.IsOk;
    }

    private void buttonLaunchingApp_Click(object sender, System.EventArgs e)
    {
      if (File.Exists(edFilename.Text))
      {
        dialogFile.FileName = edFilename.Text;
        dialogFile.RestoreDirectory = true;
      }
      else
      {
        dialogFile.FileName = "mame32.exe";
      }
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        edFilename.Text = dialogFile.FileName;
      }
      FillMameDirs();
    }

    private void buttonImageFile_Click(object sender, System.EventArgs e)
    {
      dialogFile.FileName = txtImageFile.Text;
      dialogFile.RestoreDirectory = true;
      if (dialogFile.ShowDialog(null) == DialogResult.OK)
      {
        txtImageFile.Text = dialogFile.FileName;
      }
    }

    private void btnImageDirs_Click(object sender, System.EventArgs e)
    {
      if (txtImageDirs.Text != "")
      {
        dialogFolder.SelectedPath = txtImageDirs.Lines[0];
      }
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        string strSep = "";
        if (txtImageDirs.Text != "")
        {
          strSep = "\r\n";
        }
        txtImageDirs.Text = txtImageDirs.Text + strSep + dialogFolder.SelectedPath;
      }
    }

    private void txtPinCode_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void buttonFileDirectory_Click(object sender, System.EventArgs e)
    {
      dialogFolder.SelectedPath = txtFiles.Text;
      if (dialogFolder.ShowDialog(null) == DialogResult.OK)
      {
        txtFiles.Text = dialogFolder.SelectedPath;
      }
    }

    void catverLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      LaunchLink(catverLink.Text);
    }

    void historyDatLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      LaunchLink(historyDatLink.Text);
    }

    void LaunchLink(string link)
    {
      if (link == null)
        return;
      if (link.Length > 0)
      {
        ProcessStartInfo sInfo = new ProcessStartInfo(link);
        Process.Start(sInfo);
      }
    }

    void FillMameDirs()
    {
      string mameExe = edFilename.Text;
      string mameDir = Path.GetDirectoryName(mameExe);
      if (Directory.Exists(mameDir))
      {
        // try to find directories at the default locations and set them
        // if fields are emtpy

        // 1) ROM Directory
        if (txtFiles.Text == "")
        {
          if (Directory.Exists(mameDir + "\\roms"))
          {
            txtFiles.Text = mameDir + "\\roms";
          }
        }

        // 2) IMG Directories
        if (txtImageDirs.Text == "")
        {
          if (Directory.Exists(mameDir + "\\snap"))
          {
            txtImageDirs.Text = txtImageDirs.Text + mameDir + "\\snap" + "\r\n";
          }
          if (Directory.Exists(mameDir + "\\titles"))
          {
            txtImageDirs.Text = txtImageDirs.Text + mameDir + "\\titles" + "\r\n";
          }
          if (Directory.Exists(mameDir + "\\flyers"))
          {
            txtImageDirs.Text = txtImageDirs.Text + mameDir + "\\flyers" + "\r\n";
          }
          if (Directory.Exists(mameDir + "\\cabinets"))
          {
            txtImageDirs.Text = txtImageDirs.Text + mameDir + "\\cabinets" + "\r\n";
          }
          if (Directory.Exists(mameDir + "\\marquees"))
          {
            txtImageDirs.Text = txtImageDirs.Text + mameDir + "\\marquees" + "\r\n";
          }
        }

        // 3) Title
        if (txtTitle.Text.ToLower() == "new item")
        {
          txtTitle.Text = "Mame";
        }

      }
    }


  }
}
