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
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class DVDPlayer : SectionSettings
  {
    private MPGroupBox mpGroupBox1;
    private MPLabel label1;
    private MPTextBox fileNameTextBox;
    private MPLabel label2;
    private MPTextBox textBox1;
    private MPButton button2;
    private MPButton fileNameButton;
    private MPButton parametersButton;
    private MPTextBox parametersTextBox;
    private MPCheckBox internalPlayerCheckBox;
    private OpenFileDialog openFileDialog;
    private MPGroupBox mpGroupBox3;
    private MPComboBox defaultZoomModeComboBox;
    private MPLabel label6;
    private IContainer components = null;

    //string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };
    private string[] aspectRatio = {"normal", "original", "stretch", "zoom", "zoom149", "letterbox", "panscan"};

    public DVDPlayer()
      : this("DVD Player")
    {
    }

    public DVDPlayer(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        fileNameTextBox.Text = xmlreader.GetValueAsString("dvdplayer", "path", @"");
        parametersTextBox.Text = xmlreader.GetValueAsString("dvdplayer", "arguments", "");


        //
        // Fake a check changed to force a CheckChanged event
        //
        internalPlayerCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "internal", true);
        internalPlayerCheckBox.Checked = !internalPlayerCheckBox.Checked;

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("dvdplayer", "defaultar", "normal");

        for (int index = 0; index < aspectRatio.Length; index++)
        {
          if (aspectRatio[index].Equals(defaultAspectRatio))
          {
            if (index < defaultZoomModeComboBox.Items.Count)
            {
              defaultZoomModeComboBox.SelectedIndex = index;
            }
            break;
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("dvdplayer", "path", fileNameTextBox.Text);
        xmlwriter.SetValue("dvdplayer", "arguments", parametersTextBox.Text);

        xmlwriter.SetValueAsBool("dvdplayer", "internal", !internalPlayerCheckBox.Checked);

        xmlwriter.SetValue("dvdplayer", "defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void internalPlayerCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      fileNameTextBox.Enabled =
        parametersTextBox.Enabled = fileNameButton.Enabled = parametersButton.Enabled = internalPlayerCheckBox.Checked;
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.internalPlayerCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.parametersButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.parametersTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.fileNameButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.fileNameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultZoomModeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBox1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // internalPlayerCheckBox
      // 
      this.internalPlayerCheckBox.AutoSize = true;
      this.internalPlayerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.internalPlayerCheckBox.Location = new System.Drawing.Point(168, 20);
      this.internalPlayerCheckBox.Name = "internalPlayerCheckBox";
      this.internalPlayerCheckBox.Size = new System.Drawing.Size(231, 17);
      this.internalPlayerCheckBox.TabIndex = 0;
      this.internalPlayerCheckBox.Text = "Use external player (replaces internal player)";
      this.internalPlayerCheckBox.UseVisualStyleBackColor = true;
      this.internalPlayerCheckBox.CheckedChanged += new System.EventHandler(this.internalPlayerCheckBox_CheckedChanged);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.internalPlayerCheckBox);
      this.mpGroupBox1.Controls.Add(this.parametersButton);
      this.mpGroupBox1.Controls.Add(this.parametersTextBox);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.Controls.Add(this.fileNameButton);
      this.mpGroupBox1.Controls.Add(this.fileNameTextBox);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 68);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 104);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "External Player";
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.Location = new System.Drawing.Point(384, 67);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(72, 22);
      this.parametersButton.TabIndex = 6;
      this.parametersButton.Text = "List";
      this.parametersButton.UseVisualStyleBackColor = true;
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.BorderColor = System.Drawing.Color.Empty;
      this.parametersTextBox.Location = new System.Drawing.Point(168, 68);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(208, 20);
      this.parametersTextBox.TabIndex = 5;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 15);
      this.label2.TabIndex = 4;
      this.label2.Text = "Parameters:";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.Location = new System.Drawing.Point(384, 43);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(72, 22);
      this.fileNameButton.TabIndex = 3;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.UseVisualStyleBackColor = true;
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // fileNameTextBox
      // 
      this.fileNameTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameTextBox.BorderColor = System.Drawing.Color.Empty;
      this.fileNameTextBox.Location = new System.Drawing.Point(168, 44);
      this.fileNameTextBox.Name = "fileNameTextBox";
      this.fileNameTextBox.Size = new System.Drawing.Size(208, 20);
      this.fileNameTextBox.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Path/Filename:";
      // 
      // defaultZoomModeComboBox
      // 
      this.defaultZoomModeComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultZoomModeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultZoomModeComboBox.Items.AddRange(new object[]
                                                    {
                                                      "Normal",
                                                      "Original Source Format",
                                                      "Stretch",
                                                      "Zoom",
                                                      "Zoom 14:9",
                                                      "4:3 Letterbox",
                                                      "4:3 Pan and scan"
                                                    });
      this.defaultZoomModeComboBox.Location = new System.Drawing.Point(168, 24);
      this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
      this.defaultZoomModeComboBox.Size = new System.Drawing.Size(288, 21);
      this.defaultZoomModeComboBox.TabIndex = 3;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 28);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(112, 16);
      this.label6.TabIndex = 2;
      this.label6.Text = "Default zoom mode:";
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(0, 0);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 0;
      this.button2.UseVisualStyleBackColor = true;
      // 
      // textBox1
      // 
      this.textBox1.BorderColor = System.Drawing.Color.Empty;
      this.textBox1.Location = new System.Drawing.Point(0, 0);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(100, 20);
      this.textBox1.TabIndex = 0;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.label6);
      this.mpGroupBox3.Controls.Add(this.defaultZoomModeComboBox);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(472, 60);
      this.mpGroupBox3.TabIndex = 0;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Settings";
      // 
      // DVDPlayer
      // 
      this.Controls.Add(this.mpGroupBox3);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "DVDPlayer";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox3.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void parametersButton_Click(object sender, EventArgs e)
    {
      ParameterForm parameters = new ParameterForm();

      parameters.AddParameter("%filename%", "This will be replaced by the selected media file");

      if (parameters.ShowDialog(parametersButton) == DialogResult.OK)
      {
        parametersTextBox.Text += parameters.SelectedParameter;
      }
    }

    private void fileNameButton_Click(object sender, EventArgs e)
    {
      using (openFileDialog = new OpenFileDialog())
      {
        openFileDialog.FileName = fileNameTextBox.Text;
        openFileDialog.CheckFileExists = true;
        openFileDialog.RestoreDirectory = true;
        openFileDialog.Filter = "exe files (*.exe)|*.exe";
        openFileDialog.FilterIndex = 0;
        openFileDialog.Title = "Select DVD player";

        DialogResult dialogResult = openFileDialog.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
          fileNameTextBox.Text = openFileDialog.FileName;
        }
      }
    }
  }
}