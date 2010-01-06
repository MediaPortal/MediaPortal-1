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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GeneralSkin : SectionSettings
  {
    private string SkinDirectory;
    private string LanguageDirectory;

    private MPGroupBox groupBoxAppearance;
    private MPGroupBox groupBoxSkin;
    private ListView listViewAvailableSkins;
    private ColumnHeader colName;
    private ColumnHeader colVersion;
    private MPGroupBox mpGroupBox1;
    private CheckBox checkBoxlangRTL;
    private MPComboBox languageComboBox;
    private MPLabel label2;
    private CheckBox checkBoxUsePrefix;
    private Panel panelFitImage;
    private PictureBox previewPictureBox;
    private LinkLabel linkLabel1;
    private new IContainer components = null;

    public GeneralSkin()
      : this("Skin") {}

    public GeneralSkin(string name)
      : base(name)
    {
      SkinDirectory = Config.GetFolder(Config.Dir.Skin);
      LanguageDirectory = Config.GetFolder(Config.Dir.Language);
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      this.linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://wiki.team-mediaportal.com/Extensions-Skins");

      LoadLanguages();
      //
      // Load available skins
      //
      listViewAvailableSkins.Items.Clear();

      if (Directory.Exists(SkinDirectory))
      {
        string[] skinFolders = Directory.GetDirectories(SkinDirectory, "*.*");

        foreach (string skinFolder in skinFolders)
        {
          bool isInvalidDirectory = false;
          string[] invalidDirectoryNames = new string[] {"cvs"};

          string directoryName = skinFolder.Substring(SkinDirectory.Length + 1);

          if (directoryName != null && directoryName.Length > 0)
          {
            foreach (string invalidDirectory in invalidDirectoryNames)
            {
              if (invalidDirectory.Equals(directoryName.ToLower()))
              {
                isInvalidDirectory = true;
                break;
              }
            }

            if (isInvalidDirectory == false)
            {
              //
              // Check if we have a references.xml located in the directory, if so we consider it as a valid skin directory              
              string filename = Path.Combine(SkinDirectory, Path.Combine(directoryName, "references.xml"));
              if (File.Exists(filename))
              {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlNode node = doc.SelectSingleNode("/controls/skin/version");
                ListViewItem item = listViewAvailableSkins.Items.Add(directoryName);
                if (node != null && node.InnerText != null)
                {
                  item.SubItems.Add(node.InnerText);
                }
                else
                {
                  item.SubItems.Add("?");
                }
              }
            }
          }
        }
      }
    }

    private void LoadLanguages()
    {
      string[] languages = GUILocalizeStrings.SupportedLanguages();
      foreach (string language in languages)
      {
        languageComboBox.Items.Add(language);
      }

      languageComboBox.Text = GUILocalizeStrings.LocalSupported();
    }

    private void languageComboBox_DropDownClosed(object sender, EventArgs e)
    {
      try
      {
        // If the user selects another language the amount of chars in the character table might have changed.
        // Delete the font cache to trigger a recreation
        string currentSkin = listViewAvailableSkins.SelectedItems[0].Text;
        string fontCache = String.Format(@"{0}\fonts",
                                         String.Format(@"{0}\{1}", Config.GetFolder(Config.Dir.Cache), currentSkin));
        MediaPortal.Util.Utils.DirectoryDelete(fontCache, true);
      }
      catch (Exception) {}
    }

    private void listViewAvailableSkins_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewAvailableSkins.SelectedItems.Count == 0)
      {
        previewPictureBox.Image = null;
        previewPictureBox.Visible = false;
        return;
      }
      string currentSkin = listViewAvailableSkins.SelectedItems[0].Text;
      string previewFile = Path.Combine(Path.Combine(SkinDirectory, currentSkin), @"media\preview.png");

      //
      // Clear image
      //
      previewPictureBox.Image = null;

      Stream s = GetType().Assembly.GetManifestResourceStream("MediaPortal.Configuration.Resources.mplogo.gif");
      Image img = Image.FromStream(s);

      if (File.Exists(previewFile))
      {
        using (s = new FileStream(previewFile, FileMode.Open, FileAccess.Read))
        {
          img = Image.FromStream(s);
        }
      }
      previewPictureBox.Width = img.Width;
      previewPictureBox.Height = img.Height;
      previewPictureBox.Image = img;
      previewPictureBox.Visible = true;
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

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        checkBoxUsePrefix.Checked = xmlreader.GetValueAsBool("general", "myprefix", true);
        checkBoxlangRTL.Checked = xmlreader.GetValueAsBool("general", "rtllang", false);
        languageComboBox.Text = xmlreader.GetValueAsString("skin", "language", languageComboBox.Text);
        string currentSkin = xmlreader.GetValueAsString("skin", "name", "NoSkin");

        float screenHeight = GUIGraphicsContext.currentFullscreenAdapterInfo.CurrentDisplayMode.Height;
        float screenWidth = GUIGraphicsContext.currentFullscreenAdapterInfo.CurrentDisplayMode.Width;
        float screenRatio = (screenWidth / screenHeight);
        if (currentSkin == "NoSkin")
        {
          //Change default skin based on screen aspect ratio
          if (screenRatio > 1.5)
          {
            currentSkin = "Blue3wide";
          }
          else
          {
            currentSkin = "Blue3";
          }
        }

        //
        // Make sure the skin actually exists before setting it as the current skin
        //
        for (int i = 0; i < listViewAvailableSkins.Items.Count; i++)
        {
          string checkString = listViewAvailableSkins.Items[i].SubItems[0].Text;
          if (checkString.Equals(currentSkin, StringComparison.InvariantCultureIgnoreCase))
          {
            listViewAvailableSkins.Items[i].Selected = true;

            Log.Info("Skin selected: {0} (screenWidth={1}, screenHeight={2}, screenRatio={3})", checkString, screenWidth,
                     screenHeight, screenRatio);
            break;
          }
        }

        //if (listViewAvailableSkins.SelectedIndices.Count == 0)
        //{
        //  //MessageBox.Show(String.Format("The selected skin {0} does not exist!", currentSkin), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
        //  Log.Debug("GeneralSkin: Current skin {0} not selected.", currentSkin);
        //}
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        string prevSkin = xmlwriter.GetValueAsString("skin", "name", "Blue3wide");
        string selectedSkin = prevSkin;
        try
        {
          selectedSkin = listViewAvailableSkins.SelectedItems[0].Text;
        }
        catch (Exception) {}
        if (prevSkin != selectedSkin)
        {
          xmlwriter.SetValueAsBool("general", "dontshowskinversion", false);
          Util.Utils.DeleteFiles(Config.GetSubFolder(Config.Dir.Skin, selectedSkin + @"\fonts"), "*");
        }
        xmlwriter.SetValue("skin", "name", selectedSkin);
        // Set language
        string prevLanguage = xmlwriter.GetValueAsString("skin", "language", "English");
        string skin = xmlwriter.GetValueAsString("skin", "name", "Blue3wide");
        if (prevLanguage != languageComboBox.Text)
        {
          Util.Utils.DeleteFiles(Config.GetSubFolder(Config.Dir.Skin, skin + @"\fonts"), "*");
        }

        xmlwriter.SetValue("skin", "language", languageComboBox.Text);
        xmlwriter.SetValueAsBool("general", "rtllang", checkBoxlangRTL.Checked);
        xmlwriter.SetValueAsBool("general", "myprefix", checkBoxUsePrefix.Checked);
        xmlwriter.SetValue("general", "skinobsoletecount", 0);
      }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Process.Start((string)e.Link.LinkData);
      }
      catch (Exception) {}
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBoxAppearance = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxUsePrefix = new System.Windows.Forms.CheckBox();
      this.checkBoxlangRTL = new System.Windows.Forms.CheckBox();
      this.languageComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxSkin = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.panelFitImage = new System.Windows.Forms.Panel();
      this.previewPictureBox = new System.Windows.Forms.PictureBox();
      this.listViewAvailableSkins = new System.Windows.Forms.ListView();
      this.colName = new System.Windows.Forms.ColumnHeader();
      this.colVersion = new System.Windows.Forms.ColumnHeader();
      this.groupBoxAppearance.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.groupBoxSkin.SuspendLayout();
      this.panelFitImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxAppearance
      // 
      this.groupBoxAppearance.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAppearance.Controls.Add(this.mpGroupBox1);
      this.groupBoxAppearance.Controls.Add(this.groupBoxSkin);
      this.groupBoxAppearance.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAppearance.Location = new System.Drawing.Point(0, 0);
      this.groupBoxAppearance.Name = "groupBoxAppearance";
      this.groupBoxAppearance.Size = new System.Drawing.Size(472, 408);
      this.groupBoxAppearance.TabIndex = 0;
      this.groupBoxAppearance.TabStop = false;
      this.groupBoxAppearance.Text = "Appearance";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.checkBoxUsePrefix);
      this.mpGroupBox1.Controls.Add(this.checkBoxlangRTL);
      this.mpGroupBox1.Controls.Add(this.languageComboBox);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(6, 289);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(460, 113);
      this.mpGroupBox1.TabIndex = 4;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Language Settings";
      // 
      // checkBoxUsePrefix
      // 
      this.checkBoxUsePrefix.AutoSize = true;
      this.checkBoxUsePrefix.Checked = true;
      this.checkBoxUsePrefix.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUsePrefix.Location = new System.Drawing.Point(19, 58);
      this.checkBoxUsePrefix.Name = "checkBoxUsePrefix";
      this.checkBoxUsePrefix.Size = new System.Drawing.Size(199, 17);
      this.checkBoxUsePrefix.TabIndex = 3;
      this.checkBoxUsePrefix.Text = "Use string prefixes (e.g. TV = My TV)";
      this.checkBoxUsePrefix.UseVisualStyleBackColor = true;
      // 
      // checkBoxlangRTL
      // 
      this.checkBoxlangRTL.AutoSize = true;
      this.checkBoxlangRTL.Location = new System.Drawing.Point(19, 81);
      this.checkBoxlangRTL.Name = "checkBoxlangRTL";
      this.checkBoxlangRTL.Size = new System.Drawing.Size(241, 17);
      this.checkBoxlangRTL.TabIndex = 2;
      this.checkBoxlangRTL.Text = "Language contains right to left direction chars";
      this.checkBoxlangRTL.UseVisualStyleBackColor = true;
      // 
      // languageComboBox
      // 
      this.languageComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.languageComboBox.BorderColor = System.Drawing.Color.Empty;
      this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.languageComboBox.Location = new System.Drawing.Point(118, 21);
      this.languageComboBox.Name = "languageComboBox";
      this.languageComboBox.Size = new System.Drawing.Size(325, 21);
      this.languageComboBox.TabIndex = 1;
      this.languageComboBox.DropDownClosed += new System.EventHandler(this.languageComboBox_DropDownClosed);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(96, 16);
      this.label2.TabIndex = 0;
      this.label2.Text = "Display language:";
      // 
      // groupBoxSkin
      // 
      this.groupBoxSkin.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSkin.Controls.Add(this.linkLabel1);
      this.groupBoxSkin.Controls.Add(this.panelFitImage);
      this.groupBoxSkin.Controls.Add(this.listViewAvailableSkins);
      this.groupBoxSkin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSkin.Location = new System.Drawing.Point(6, 23);
      this.groupBoxSkin.Name = "groupBoxSkin";
      this.groupBoxSkin.Size = new System.Drawing.Size(460, 236);
      this.groupBoxSkin.TabIndex = 3;
      this.groupBoxSkin.TabStop = false;
      this.groupBoxSkin.Text = "Skin";
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(16, 209);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(131, 13);
      this.linkLabel1.TabIndex = 10;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "more new and hot skins ...";
      this.linkLabel1.LinkClicked +=
        new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // panelFitImage
      // 
      this.panelFitImage.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.panelFitImage.Controls.Add(this.previewPictureBox);
      this.panelFitImage.Location = new System.Drawing.Point(221, 22);
      this.panelFitImage.Name = "panelFitImage";
      this.panelFitImage.Size = new System.Drawing.Size(222, 179);
      this.panelFitImage.TabIndex = 5;
      // 
      // previewPictureBox
      // 
      this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.previewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.previewPictureBox.Image = global::MediaPortal.Configuration.Properties.Resources.mplogo;
      this.previewPictureBox.Location = new System.Drawing.Point(0, 0);
      this.previewPictureBox.Name = "previewPictureBox";
      this.previewPictureBox.Size = new System.Drawing.Size(222, 179);
      this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.previewPictureBox.TabIndex = 5;
      this.previewPictureBox.TabStop = false;
      // 
      // listViewAvailableSkins
      // 
      this.listViewAvailableSkins.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Left)));
      this.listViewAvailableSkins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                                                     {
                                                       this.colName,
                                                       this.colVersion
                                                     });
      this.listViewAvailableSkins.FullRowSelect = true;
      this.listViewAvailableSkins.HideSelection = false;
      this.listViewAvailableSkins.Location = new System.Drawing.Point(15, 22);
      this.listViewAvailableSkins.MultiSelect = false;
      this.listViewAvailableSkins.Name = "listViewAvailableSkins";
      this.listViewAvailableSkins.Size = new System.Drawing.Size(200, 179);
      this.listViewAvailableSkins.TabIndex = 3;
      this.listViewAvailableSkins.UseCompatibleStateImageBehavior = false;
      this.listViewAvailableSkins.View = System.Windows.Forms.View.Details;
      this.listViewAvailableSkins.SelectedIndexChanged +=
        new System.EventHandler(this.listViewAvailableSkins_SelectedIndexChanged);
      // 
      // colName
      // 
      this.colName.Text = "Name";
      this.colName.Width = 140;
      // 
      // colVersion
      // 
      this.colVersion.Text = "Version";
      this.colVersion.Width = 56;
      // 
      // GeneralSkin
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBoxAppearance);
      this.Name = "GeneralSkin";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxAppearance.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.groupBoxSkin.ResumeLayout(false);
      this.groupBoxSkin.PerformLayout();
      this.panelFitImage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion
  }
}