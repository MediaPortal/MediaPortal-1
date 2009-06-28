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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

namespace MediaPortal.FoobarPlugin
{
  /// <summary>
  /// Summary description for FoobarConfigForm.
  /// </summary>
  public class FoobarConfigForm : MPConfigForm
  {
    private LinkLabel linkLabelPluginSource;
    private GroupBox groupBoxFoobarLocation;
    private MPLabel labelPortNumber;
    private MPTextBox textBoxPortNumber;
    private MPTextBox textBoxHostname;
    private MPButton buttonBrowseFoobarLoc;
    private MPTextBox textBoxFoobarPath;
    private MPLabel labelHostname;
    private MPLabel labelLocation;
    private GroupBox groupBoxFoobarSettings;
    private MPTextBox textBoxFoobarExtensions;
    private MPLabel labelFoobarFileTypes;
    private LinkLabel linkLabelStartupParameter;
    private TextBox textBoxStartupParameter;
    private MPButton btnOK;
    private MPButton btnCancel;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    public FoobarConfigForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      LoadSettings();
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
      this.linkLabelPluginSource = new System.Windows.Forms.LinkLabel();
      this.groupBoxFoobarLocation = new System.Windows.Forms.GroupBox();
      this.labelPortNumber = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxPortNumber = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.textBoxHostname = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.buttonBrowseFoobarLoc = new MediaPortal.UserInterface.Controls.MPButton();
      this.textBoxFoobarPath = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelHostname = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelLocation = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxFoobarSettings = new System.Windows.Forms.GroupBox();
      this.linkLabelStartupParameter = new System.Windows.Forms.LinkLabel();
      this.textBoxStartupParameter = new System.Windows.Forms.TextBox();
      this.textBoxFoobarExtensions = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.labelFoobarFileTypes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxFoobarLocation.SuspendLayout();
      this.groupBoxFoobarSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // linkLabelPluginSource
      // 
      this.linkLabelPluginSource.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.linkLabelPluginSource.LinkArea = new System.Windows.Forms.LinkArea(37, 19);
      this.linkLabelPluginSource.Location = new System.Drawing.Point(12, 254);
      this.linkLabelPluginSource.Name = "linkLabelPluginSource";
      this.linkLabelPluginSource.Size = new System.Drawing.Size(392, 32);
      this.linkLabelPluginSource.TabIndex = 5;
      this.linkLabelPluginSource.TabStop = true;
      this.linkLabelPluginSource.Text =
        "NOTE:  Remember to install and setup foo_httpserver_ctrl (version B1) in your foo" +
        "bar\\components directory.";
      this.linkLabelPluginSource.UseCompatibleTextRendering = true;
      this.linkLabelPluginSource.LinkClicked +=
        new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPluginSource_LinkClicked);
      // 
      // groupBoxFoobarLocation
      // 
      this.groupBoxFoobarLocation.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxFoobarLocation.Controls.Add(this.labelPortNumber);
      this.groupBoxFoobarLocation.Controls.Add(this.textBoxPortNumber);
      this.groupBoxFoobarLocation.Controls.Add(this.textBoxHostname);
      this.groupBoxFoobarLocation.Controls.Add(this.buttonBrowseFoobarLoc);
      this.groupBoxFoobarLocation.Controls.Add(this.textBoxFoobarPath);
      this.groupBoxFoobarLocation.Controls.Add(this.labelHostname);
      this.groupBoxFoobarLocation.Controls.Add(this.labelLocation);
      this.groupBoxFoobarLocation.Location = new System.Drawing.Point(12, 12);
      this.groupBoxFoobarLocation.Name = "groupBoxFoobarLocation";
      this.groupBoxFoobarLocation.Size = new System.Drawing.Size(392, 91);
      this.groupBoxFoobarLocation.TabIndex = 7;
      this.groupBoxFoobarLocation.TabStop = false;
      this.groupBoxFoobarLocation.Text = "Location";
      // 
      // labelPortNumber
      // 
      this.labelPortNumber.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelPortNumber.AutoSize = true;
      this.labelPortNumber.Location = new System.Drawing.Point(293, 57);
      this.labelPortNumber.Name = "labelPortNumber";
      this.labelPortNumber.Size = new System.Drawing.Size(26, 13);
      this.labelPortNumber.TabIndex = 13;
      this.labelPortNumber.Text = "Port";
      // 
      // textBoxPortNumber
      // 
      this.textBoxPortNumber.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxPortNumber.BorderColor = System.Drawing.Color.Empty;
      this.textBoxPortNumber.Enabled = false;
      this.textBoxPortNumber.Location = new System.Drawing.Point(326, 54);
      this.textBoxPortNumber.Name = "textBoxPortNumber";
      this.textBoxPortNumber.Size = new System.Drawing.Size(54, 20);
      this.textBoxPortNumber.TabIndex = 11;
      this.textBoxPortNumber.Text = "8989";
      this.textBoxPortNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // textBoxHostname
      // 
      this.textBoxHostname.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxHostname.BorderColor = System.Drawing.Color.Empty;
      this.textBoxHostname.Enabled = false;
      this.textBoxHostname.Location = new System.Drawing.Point(92, 54);
      this.textBoxHostname.Name = "textBoxHostname";
      this.textBoxHostname.Size = new System.Drawing.Size(195, 20);
      this.textBoxHostname.TabIndex = 10;
      this.textBoxHostname.Text = "localhost";
      // 
      // buttonBrowseFoobarLoc
      // 
      this.buttonBrowseFoobarLoc.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowseFoobarLoc.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonBrowseFoobarLoc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.buttonBrowseFoobarLoc.Location = new System.Drawing.Point(356, 19);
      this.buttonBrowseFoobarLoc.Name = "buttonBrowseFoobarLoc";
      this.buttonBrowseFoobarLoc.Size = new System.Drawing.Size(24, 23);
      this.buttonBrowseFoobarLoc.TabIndex = 8;
      this.buttonBrowseFoobarLoc.Text = "...";
      this.buttonBrowseFoobarLoc.UseVisualStyleBackColor = true;
      this.buttonBrowseFoobarLoc.Click += new System.EventHandler(this.buttonBrowseFoobarLoc_Click);
      // 
      // textBoxFoobarPath
      // 
      this.textBoxFoobarPath.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFoobarPath.BorderColor = System.Drawing.Color.Empty;
      this.textBoxFoobarPath.Enabled = false;
      this.textBoxFoobarPath.Location = new System.Drawing.Point(92, 21);
      this.textBoxFoobarPath.Name = "textBoxFoobarPath";
      this.textBoxFoobarPath.Size = new System.Drawing.Size(252, 20);
      this.textBoxFoobarPath.TabIndex = 12;
      // 
      // labelHostname
      // 
      this.labelHostname.AutoSize = true;
      this.labelHostname.Location = new System.Drawing.Point(10, 57);
      this.labelHostname.Name = "labelHostname";
      this.labelHostname.Size = new System.Drawing.Size(55, 13);
      this.labelHostname.TabIndex = 9;
      this.labelHostname.Text = "Hostname";
      // 
      // labelLocation
      // 
      this.labelLocation.AutoSize = true;
      this.labelLocation.Location = new System.Drawing.Point(10, 24);
      this.labelLocation.Name = "labelLocation";
      this.labelLocation.Size = new System.Drawing.Size(76, 13);
      this.labelLocation.TabIndex = 7;
      this.labelLocation.Text = "foobar2k v.8.3";
      // 
      // groupBoxFoobarSettings
      // 
      this.groupBoxFoobarSettings.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxFoobarSettings.Controls.Add(this.linkLabelStartupParameter);
      this.groupBoxFoobarSettings.Controls.Add(this.textBoxStartupParameter);
      this.groupBoxFoobarSettings.Controls.Add(this.textBoxFoobarExtensions);
      this.groupBoxFoobarSettings.Controls.Add(this.labelFoobarFileTypes);
      this.groupBoxFoobarSettings.Location = new System.Drawing.Point(12, 109);
      this.groupBoxFoobarSettings.Name = "groupBoxFoobarSettings";
      this.groupBoxFoobarSettings.Size = new System.Drawing.Size(392, 132);
      this.groupBoxFoobarSettings.TabIndex = 8;
      this.groupBoxFoobarSettings.TabStop = false;
      this.groupBoxFoobarSettings.Text = "Settings";
      // 
      // linkLabelStartupParameter
      // 
      this.linkLabelStartupParameter.AutoSize = true;
      this.linkLabelStartupParameter.LinkArea = new System.Windows.Forms.LinkArea(8, 9);
      this.linkLabelStartupParameter.Location = new System.Drawing.Point(13, 25);
      this.linkLabelStartupParameter.Name = "linkLabelStartupParameter";
      this.linkLabelStartupParameter.Size = new System.Drawing.Size(95, 17);
      this.linkLabelStartupParameter.TabIndex = 9;
      this.linkLabelStartupParameter.TabStop = true;
      this.linkLabelStartupParameter.Text = "Startup parameter";
      this.linkLabelStartupParameter.UseCompatibleTextRendering = true;
      this.linkLabelStartupParameter.LinkClicked +=
        new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelStartupParameter_LinkClicked);
      // 
      // textBoxStartupParameter
      // 
      this.textBoxStartupParameter.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxStartupParameter.Location = new System.Drawing.Point(13, 45);
      this.textBoxStartupParameter.Name = "textBoxStartupParameter";
      this.textBoxStartupParameter.Size = new System.Drawing.Size(367, 20);
      this.textBoxStartupParameter.TabIndex = 7;
      this.textBoxStartupParameter.Text = "/hide /command:\"Playback/Order/Default\"";
      // 
      // textBoxFoobarExtensions
      // 
      this.textBoxFoobarExtensions.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFoobarExtensions.BorderColor = System.Drawing.Color.Empty;
      this.textBoxFoobarExtensions.Location = new System.Drawing.Point(13, 93);
      this.textBoxFoobarExtensions.Name = "textBoxFoobarExtensions";
      this.textBoxFoobarExtensions.Size = new System.Drawing.Size(367, 20);
      this.textBoxFoobarExtensions.TabIndex = 6;
      this.textBoxFoobarExtensions.Text = ".cda,.mp3,.mid,.wav,.mpc,.aac,.shn,.wma,.ac3,.ogg";
      // 
      // labelFoobarFileTypes
      // 
      this.labelFoobarFileTypes.AutoSize = true;
      this.labelFoobarFileTypes.Location = new System.Drawing.Point(10, 77);
      this.labelFoobarFileTypes.Name = "labelFoobarFileTypes";
      this.labelFoobarFileTypes.Size = new System.Drawing.Size(328, 13);
      this.labelFoobarFileTypes.TabIndex = 5;
      this.labelFoobarFileTypes.Text = "Supported file types (other files will be played by MP\'s internal player)";
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(248, 291);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 9;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(329, 291);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 10;
      this.btnCancel.Text = "&Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // FoobarConfigForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(416, 326);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.groupBoxFoobarSettings);
      this.Controls.Add(this.groupBoxFoobarLocation);
      this.Controls.Add(this.linkLabelPluginSource);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "FoobarConfigForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Foobar2000 - Setup";
      this.groupBoxFoobarLocation.ResumeLayout(false);
      this.groupBoxFoobarLocation.PerformLayout();
      this.groupBoxFoobarSettings.ResumeLayout(false);
      this.groupBoxFoobarSettings.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// This method is called whenever the browse button is clicked
    /// </summary>
    /// <param name="sender">the sender instance</param>
    /// <param name="e">the event.  In this case click!</param>
    private void buttonBrowseFoobarLoc_Click(object sender, EventArgs e)
    {
      string curDir = Directory.GetCurrentDirectory();
      // The filter for the dialog window is foobar2000.exe
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.AddExtension = true;
      dlg.Filter = "Foobar2000 (Foobar2000.exe)|Foobar2000.exe|All files (*.*)|*.*";
      // start in media folder
      //dlg.InitialDirectory = @"C:\";    
      // open dialog
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        textBoxFoobarPath.Text = dlg.FileName;
      }
      Directory.SetCurrentDirectory(curDir);
    }

    /// <summary>
    /// When this form loads, read the configuration file for the variables that this
    /// form sets up
    /// </summary>
    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        textBoxFoobarExtensions.Text = xmlreader.GetValueAsString("foobarplugin", "enabledextensions",
                                                                  ".cda,.mp3,.mid,.wav,.mpc,.aac,.shn,.wma,.ac3,.ogg");
        textBoxPortNumber.Text = xmlreader.GetValueAsString("foobarplugin", "port", "8989");
        textBoxHostname.Text = xmlreader.GetValueAsString("foobarplugin", "host", "localhost");
        textBoxFoobarPath.Text = xmlreader.GetValueAsString("foobarplugin", "path", "");
        textBoxStartupParameter.Text = xmlreader.GetValueAsString("foobarplugin", "startupparameter",
                                                                  "/hide /command:\"Playback/Order/Default\"");
      }
      if (textBoxFoobarPath.Text.Equals(String.Empty))
      {
        using (RegistryKey pRegKey = Registry.CurrentUser)
        {
          using (RegistryKey subkey = pRegKey.OpenSubKey("Software\\foobar2000"))
          {
            if (subkey != null)
            {
              Object val = subkey.GetValue("InstallDir");
              if (val.ToString().Trim().Length > 0)
              {
                textBoxFoobarPath.Text = val.ToString() + "\\foobar2000.exe";
              }
            }
            else
            {
              MessageBox.Show(
                "Foobar2000 is not installed on your system!\r\nPlease install it first before actually trying to use this plugin!",
                "Foobar2000 plugin error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              Close();
            }
          }
        }
      }
    }

    /// <summary>
    /// Write the variables from the form to the configuration file when this form closes
    /// </summary>
    private void SaveSettings()
    {
      using (Settings xmlWriter = new MPSettings())
      {
        xmlWriter.SetValue("foobarplugin", "port", textBoxPortNumber.Text);
        xmlWriter.SetValue("foobarplugin", "host", textBoxHostname.Text);
        xmlWriter.SetValue("foobarplugin", "path", textBoxFoobarPath.Text);
        xmlWriter.SetValue("foobarplugin", "startupparameter", textBoxStartupParameter.Text);
        // make sure all the extensions starts with "."  If not, add it in...
        string[] exts = textBoxFoobarExtensions.Text.Split(new char[] {','});
        StringBuilder buff = new StringBuilder();
        foreach (string ext in exts)
        {
          if (buff.Length != 0)
          {
            buff.Append(',');
          }
          if (!ext.StartsWith("."))
          {
            buff.Append('.');
          }
          buff.Append(ext);
        }
        xmlWriter.SetValue("foobarplugin", "enabledextensions", buff.ToString());
      }
    }

    private void linkLabelStartupParameter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      // Determine which link was clicked within the LinkLabel.
      this.linkLabelStartupParameter.Links[linkLabelStartupParameter.Links.IndexOf(e.Link)].Visited = true;
      try
      {
        Help.ShowHelp(this, "http://wiki.hydrogenaudio.org/index.php?title=Foobar2000:Commandline_Guide");
      }
      catch
      {
      }
    }

    /// <summary>
    /// The link will open the link on a browser to get the foobar plugin from the source
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void linkLabelPluginSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      // Determine which link was clicked within the LinkLabel.
      this.linkLabelPluginSource.Links[linkLabelPluginSource.Links.IndexOf(e.Link)].Visited = true;
      try
      {
        Help.ShowHelp(this, "http://sourceforge.net/projects/foohttpserver");
      }
      catch
      {
      }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      LoadSettings();
      this.Close();
    }
  }
}