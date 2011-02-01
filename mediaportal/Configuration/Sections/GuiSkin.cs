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
  public class GuiSkin : SectionSettings
  {
    private string SkinDirectory;

    private MPGroupBox groupBoxSkin;
    private ListView listViewAvailableSkins;
    private ColumnHeader colName;
    private ColumnHeader colVersion;
    private Panel panelFitImage;
    private PictureBox previewPictureBox;
    private LinkLabel linkLabel1;
    private new IContainer components = null;

    public GuiSkin()
      : this("Skin") { }

    public GuiSkin(string name)
      : base(name)
    {
      SkinDirectory = Config.GetFolder(Config.Dir.Skin);
      // This call is required by the Windows Form Designer.
      InitializeComponent();

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
          string[] invalidDirectoryNames = new string[] { "cvs" };

          string directoryName = skinFolder.Substring(SkinDirectory.Length + 1);

          if (!string.IsNullOrEmpty(directoryName))
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

      Image img = Properties.Resources.mplogo;

      if (File.Exists(previewFile))
      {
        using (Stream s = new FileStream(previewFile, FileMode.Open, FileAccess.Read))
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
        string currentSkin = xmlreader.GetValueAsString("skin", "name", "NoSkin");

        float screenHeight = GUIGraphicsContext.currentScreen.Bounds.Height;
        float screenWidth = GUIGraphicsContext.currentScreen.Bounds.Width;
        float screenRatio = (screenWidth / screenHeight);
        if (currentSkin == "NoSkin")
        {
          //Change default skin based on screen aspect ratio
          currentSkin = screenRatio > 1.5 ? "Blue3wide" : "Blue3";
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
        catch (Exception) { }
        if (prevSkin != selectedSkin)
        {
          xmlwriter.SetValueAsBool("general", "dontshowskinversion", false);
          Util.Utils.DeleteFiles(Config.GetSubFolder(Config.Dir.Skin, selectedSkin + @"\fonts"), "*");
        }
        xmlwriter.SetValue("skin", "name", selectedSkin);
        Config.SkinName = selectedSkin;
        xmlwriter.SetValue("general", "skinobsoletecount", 0);
      }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        // This url is a redirect, which shouldn't be changed.
        // If it's target should be changed, contact high, please.
        Process.Start(@"http://www.team-mediaportal.com/MP1/skingallery");
      }
      catch { }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBoxSkin = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.panelFitImage = new System.Windows.Forms.Panel();
      this.previewPictureBox = new System.Windows.Forms.PictureBox();
      this.listViewAvailableSkins = new System.Windows.Forms.ListView();
      this.colName = new System.Windows.Forms.ColumnHeader();
      this.colVersion = new System.Windows.Forms.ColumnHeader();
      this.groupBoxSkin.SuspendLayout();
      this.panelFitImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxSkin
      // 
      this.groupBoxSkin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSkin.Controls.Add(this.linkLabel1);
      this.groupBoxSkin.Controls.Add(this.panelFitImage);
      this.groupBoxSkin.Controls.Add(this.listViewAvailableSkins);
      this.groupBoxSkin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSkin.Location = new System.Drawing.Point(6, 0);
      this.groupBoxSkin.Name = "groupBoxSkin";
      this.groupBoxSkin.Size = new System.Drawing.Size(460, 162);
      this.groupBoxSkin.TabIndex = 3;
      this.groupBoxSkin.TabStop = false;
      this.groupBoxSkin.Text = "Skin Selection";
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(16, 132);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(131, 13);
      this.linkLabel1.TabIndex = 10;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "more new and hot skins ...";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // panelFitImage
      // 
      this.panelFitImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.panelFitImage.Controls.Add(this.previewPictureBox);
      this.panelFitImage.Location = new System.Drawing.Point(221, 22);
      this.panelFitImage.Name = "panelFitImage";
      this.panelFitImage.Size = new System.Drawing.Size(222, 123);
      this.panelFitImage.TabIndex = 5;
      // 
      // previewPictureBox
      // 
      this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.previewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.previewPictureBox.Image = global::MediaPortal.Configuration.Properties.Resources.mplogo;
      this.previewPictureBox.Location = new System.Drawing.Point(0, 0);
      this.previewPictureBox.Name = "previewPictureBox";
      this.previewPictureBox.Size = new System.Drawing.Size(222, 123);
      this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.previewPictureBox.TabIndex = 5;
      this.previewPictureBox.TabStop = false;
      // 
      // listViewAvailableSkins
      // 
      this.listViewAvailableSkins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewAvailableSkins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colVersion});
      this.listViewAvailableSkins.FullRowSelect = true;
      this.listViewAvailableSkins.HideSelection = false;
      this.listViewAvailableSkins.Location = new System.Drawing.Point(15, 22);
      this.listViewAvailableSkins.MultiSelect = false;
      this.listViewAvailableSkins.Name = "listViewAvailableSkins";
      this.listViewAvailableSkins.Size = new System.Drawing.Size(200, 107);
      this.listViewAvailableSkins.TabIndex = 3;
      this.listViewAvailableSkins.UseCompatibleStateImageBehavior = false;
      this.listViewAvailableSkins.View = System.Windows.Forms.View.Details;
      this.listViewAvailableSkins.SelectedIndexChanged += new System.EventHandler(this.listViewAvailableSkins_SelectedIndexChanged);
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
      // GuiSkin
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBoxSkin);
      this.Name = "GuiSkin";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxSkin.ResumeLayout(false);
      this.groupBoxSkin.PerformLayout();
      this.panelFitImage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion
  }
}