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
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class BaseFileExtensions : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPButton removeButton;
    private MPButton addButton;
    private MPLabel label1;
    private MPTextBox extensionTextBox;
    private ListBox extensionsListBox;
    private IContainer components = null;

    public string Extensions
    {
      get
      {
        string extensions = string.Empty;

        foreach (string extension in extensionsListBox.Items)
        {
          if (extensions.Length > 0)
          {
            extensions += ",";
          }

          extensions += extension;
        }

        return extensions;
      }
      set
      {
        string[] extensions = ((string)value).Split(',');
        extensionsListBox.Items.AddRange(extensions);
      }
    }

    public BaseFileExtensions()
      : base("<Unknown>")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    public BaseFileExtensions(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
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

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    protected void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.addButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.removeButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.extensionsListBox = new System.Windows.Forms.ListBox();
      this.extensionTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.removeButton);
      this.groupBox1.Controls.Add(this.extensionsListBox);
      this.groupBox1.Controls.Add(this.extensionTextBox);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // label1
      // 
      this.label1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(440, 32);
      this.label1.TabIndex = 0;
      this.label1.Text = "Files matching an extension listed below will be considered a known media type.";
      // 
      // addButton
      // 
      this.addButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.addButton.Location = new System.Drawing.Point(384, 64);
      this.addButton.Name = "addButton";
      this.addButton.Size = new System.Drawing.Size(72, 22);
      this.addButton.TabIndex = 2;
      this.addButton.Text = "Add";
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // removeButton
      // 
      this.removeButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.removeButton.Enabled = false;
      this.removeButton.Location = new System.Drawing.Point(384, 88);
      this.removeButton.Name = "removeButton";
      this.removeButton.Size = new System.Drawing.Size(72, 22);
      this.removeButton.TabIndex = 4;
      this.removeButton.Text = "Remove";
      this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
      // 
      // extensionsListBox
      // 
      this.extensionsListBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.extensionsListBox.Location = new System.Drawing.Point(16, 88);
      this.extensionsListBox.Name = "extensionsListBox";
      this.extensionsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.extensionsListBox.Size = new System.Drawing.Size(360, 303);
      this.extensionsListBox.TabIndex = 3;
      this.extensionsListBox.SelectedIndexChanged += new System.EventHandler(this.extensionsListBox_SelectedIndexChanged);
      // 
      // extensionTextBox
      // 
      this.extensionTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.extensionTextBox.Location = new System.Drawing.Point(16, 64);
      this.extensionTextBox.Name = "extensionTextBox";
      this.extensionTextBox.Size = new System.Drawing.Size(360, 20);
      this.extensionTextBox.TabIndex = 1;
      this.extensionTextBox.Text = "";
      // 
      // FileExtensions
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "FileExtensions";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void addButton_Click(object sender, EventArgs e)
    {
      string extension = extensionTextBox.Text;

      if (extension != null && extension.Length != 0)
      {
        //
        // Only grab what we got after the first .
        //
        int dotPosition = extension.IndexOf(".");

        if (dotPosition < 0)
        {
          //
          // We got no dot in the extension, append it
          //
          extension = String.Format(".{0}", extension);
        }
        else
        {
          //
          // Remove everything before the dot
          //
          extension = extension.Substring(dotPosition);
        }

        //
        // Remove unwanted characters
        //
        extension = extension.Replace("*", "");

        //
        // Add extension to the list
        //
        extensionsListBox.Items.Add(extension);

        //
        // Clear text
        //
        extensionTextBox.Text = string.Empty;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void removeButton_Click(object sender, EventArgs e)
    {
      int itemsSelected = extensionsListBox.SelectedIndices.Count;

      //
      // Make sure we have a valid item selected
      //
      for (int index = 0; index < itemsSelected; index++)
      {
        extensionsListBox.Items.RemoveAt(extensionsListBox.SelectedIndices[0]);
      }
    }

    private void extensionsListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      removeButton.Enabled = (extensionsListBox.SelectedItems.Count > 0);
    }

    public override object GetSetting(string name)
    {
      switch (name.ToLower())
      {
        case "extensions":
          return Extensions;
      }

      return null;
    }

    protected void LoadSettings(string section, string defaultExt)
    {
      using (Settings xmlreader = new MPSettings())
      {
        Extensions = xmlreader.GetValueAsString(section, "extensions", defaultExt);
      }
    }

    protected void SaveSettings(string section)
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(section, "extensions", Extensions);
      }
    }
  }
}