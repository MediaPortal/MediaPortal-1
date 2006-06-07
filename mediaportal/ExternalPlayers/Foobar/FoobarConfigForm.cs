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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace MediaPortal.FoobarPlugin
{
  /// <summary>
  /// Summary description for FoobarConfigForm.
  /// </summary>
  public class FoobarConfigForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.LinkLabel linkLabelPluginSource;
    private GroupBox groupBoxFoobarLocation;
    private MediaPortal.UserInterface.Controls.MPLabel labelPortNumber;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxPortNumber;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxHostname;
    private MediaPortal.UserInterface.Controls.MPButton buttonBrowseFoobarLoc;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxFoobarPath;
    private MediaPortal.UserInterface.Controls.MPLabel labelHostname;
    private MediaPortal.UserInterface.Controls.MPLabel labelLocation;
    private GroupBox groupBoxFoobarSettings;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxFoobarExtensions;
    private MediaPortal.UserInterface.Controls.MPLabel labelFoobarFileTypes;
    private LinkLabel linkLabelStartupParameter;
    private TextBox textBoxStartupParameter;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public FoobarConfigForm()
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FoobarConfigForm));
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
      this.groupBoxFoobarLocation.SuspendLayout();
      this.groupBoxFoobarSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // linkLabelPluginSource
      // 
      this.linkLabelPluginSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.linkLabelPluginSource.LinkArea = new System.Windows.Forms.LinkArea(37, 19);
      this.linkLabelPluginSource.Location = new System.Drawing.Point(12, 254);
      this.linkLabelPluginSource.Name = "linkLabelPluginSource";
      this.linkLabelPluginSource.Size = new System.Drawing.Size(392, 32);
      this.linkLabelPluginSource.TabIndex = 5;
      this.linkLabelPluginSource.TabStop = true;
      this.linkLabelPluginSource.Text = "NOTE:  Remember to install and setup foo_httpserver_ctrl (version B1) in your foo" +
          "bar\\components directory.";
      this.linkLabelPluginSource.UseCompatibleTextRendering = true;
      this.linkLabelPluginSource.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPluginSource_LinkClicked);
      // 
      // groupBoxFoobarLocation
      // 
      this.groupBoxFoobarLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
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
      this.labelPortNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelPortNumber.AutoSize = true;
      this.labelPortNumber.Location = new System.Drawing.Point(300, 57);
      this.labelPortNumber.Name = "labelPortNumber";
      this.labelPortNumber.Size = new System.Drawing.Size(26, 13);
      this.labelPortNumber.TabIndex = 13;
      this.labelPortNumber.Text = "Port";
      // 
      // textBoxPortNumber
      // 
      this.textBoxPortNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
      this.textBoxHostname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxHostname.Enabled = false;
      this.textBoxHostname.Location = new System.Drawing.Point(92, 54);
      this.textBoxHostname.Name = "textBoxHostname";
      this.textBoxHostname.Size = new System.Drawing.Size(195, 20);
      this.textBoxHostname.TabIndex = 10;
      this.textBoxHostname.Text = "localhost";
      // 
      // buttonBrowseFoobarLoc
      // 
      this.buttonBrowseFoobarLoc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
      this.textBoxFoobarPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
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
      this.groupBoxFoobarSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
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
      this.linkLabelStartupParameter.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelStartupParameter_LinkClicked);
      // 
      // textBoxStartupParameter
      // 
      this.textBoxStartupParameter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxStartupParameter.Location = new System.Drawing.Point(13, 45);
      this.textBoxStartupParameter.Name = "textBoxStartupParameter";
      this.textBoxStartupParameter.Size = new System.Drawing.Size(367, 20);
      this.textBoxStartupParameter.TabIndex = 7;
      this.textBoxStartupParameter.Text = "/hide /command:\"Playback/Order/Default\"";
      // 
      // textBoxFoobarExtensions
      // 
      this.textBoxFoobarExtensions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
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
      // FoobarConfigForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(416, 293);
      this.Controls.Add(this.groupBoxFoobarSettings);
      this.Controls.Add(this.groupBoxFoobarLocation);
      this.Controls.Add(this.linkLabelPluginSource);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "FoobarConfigForm";
      this.Text = "Foobar2000 plugin settings";
      this.Closing += new System.ComponentModel.CancelEventHandler(this.FoobarConfigForm_Closing);
      this.Load += new System.EventHandler(this.FoobarConfigForm_Load);
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
    private void buttonBrowseFoobarLoc_Click(object sender, System.EventArgs e)
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
    /// <param name="sender">the sender instance</param>
    /// <param name="e">the event.  Form load!</param>
    private void FoobarConfigForm_Load(object sender, System.EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        textBoxFoobarExtensions.Text = xmlreader.GetValueAsString("foobarplugin", "enabledextensions", ".cda,.mp3,.mid,.wav,.mpc,.aac,.shn,.wma,.ac3,.ogg");
        textBoxPortNumber.Text = xmlreader.GetValueAsString("foobarplugin", "port", "8989");
        textBoxHostname.Text = xmlreader.GetValueAsString("foobarplugin", "host", "localhost");
        textBoxFoobarPath.Text = xmlreader.GetValueAsString("foobarplugin", "path", "");
        textBoxStartupParameter.Text = xmlreader.GetValueAsString("foobarplugin", "startupparameter", "/hide /command:\"Playback/Order/Default\"");
      }
      if (textBoxFoobarPath.Text == "")
      {
        using (RegistryKey pRegKey = Registry.CurrentUser)
        {
          using (RegistryKey subkey = pRegKey.OpenSubKey("Software\\foobar2000"))
          {
            Object val = subkey.GetValue("InstallDir");
            if (val.ToString().Trim().Length > 0)
            {
              textBoxFoobarPath.Text = val.ToString() + "\\foobar2000.exe";
            }
          }
        }
      }
    }

    /// <summary>
    /// Write the variables from the form to the configuration file when this form closes
    /// </summary>
    /// <param name="sender">the sender instance</param>
    /// <param name="e">the event.  Closing!</param>
    private void FoobarConfigForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlWriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlWriter.SetValue("foobarplugin", "port", textBoxPortNumber.Text);
        xmlWriter.SetValue("foobarplugin", "host", textBoxHostname.Text);
        xmlWriter.SetValue("foobarplugin", "path", textBoxFoobarPath.Text);
        xmlWriter.SetValue("foobarplugin", "startupparameter", textBoxStartupParameter.Text);
        // make sure all the extensions starts with "."  If not, add it in...
        string[] exts = textBoxFoobarExtensions.Text.Split(new char[] { ',' });
        StringBuilder buff = new StringBuilder();
        foreach (string ext in exts)
        {
          if (buff.Length != 0)
            buff.Append(',');
          if (!ext.StartsWith("."))
            buff.Append('.');
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
  }
}






