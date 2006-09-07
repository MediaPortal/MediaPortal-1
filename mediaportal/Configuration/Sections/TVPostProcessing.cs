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
using System.Drawing;
using System.Windows.Forms;
using DShowNET.Helper;
using DirectShowLib;

using MediaPortal.Util;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class TVPostProcessing : MediaPortal.Configuration.SectionSettings
  {
      private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox bottomscanlinesTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPTextBox topscanlinesTextBox;
    private MediaPortal.UserInterface.Controls.MPTextBox leftcolumnsTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel3;
      private MediaPortal.UserInterface.Controls.MPTextBox rightcolumnsTextBox;
      private CheckedListBox cLBDSFilter;
      private MediaPortal.UserInterface.Controls.MPLabel label3;
      private Label labelPropertiesHint;
      private Button bSetup;
      private ListBox lBDSFilter;
    private System.ComponentModel.IContainer components = null;

    public TVPostProcessing()
      : this("Post Processing")
    {
    }

    public TVPostProcessing(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
    }

    public override void LoadSettings()
    {
      string strFilters = "";
      string strUsedFilters = "";
      cLBDSFilter.Sorted = false;
      lBDSFilter.Sorted = false;
      cLBDSFilter.DisplayMember = "Name";
      lBDSFilter.DisplayMember = "Name";
      lBDSFilter.FormattingEnabled = true;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
          topscanlinesTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("mytv", "topscanlinestoremove", 0));
          bottomscanlinesTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("mytv", "bottomscanlinestoremove", 0));
          leftcolumnsTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("mytv", "leftcolumnstoremove", 0));
          rightcolumnsTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("mytv", "rightcolumnstoremove", 0));
          int intCount = 0;
          while (xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") != "undefined")
          {
              strFilters += xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") + ";";
              if (xmlreader.GetValueAsBool("mytv", "usefilter" + intCount.ToString(), false))
              {
                  strUsedFilters += xmlreader.GetValueAsString("mytv", "filter" + intCount.ToString(), "undefined") + ";";
              }
              intCount++;
          }
      }
      foreach (DsDevice device in DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.LegacyAmFilterCategory))
      {
          try
          {
              if (device.Name != null)
              {
                  lBDSFilter.Items.Add(device);
                  if (strFilters.Contains(device.Name))
                  {
                      cLBDSFilter.Items.Add(device);
                      cLBDSFilter.SetItemChecked(cLBDSFilter.Items.Count - 1, strUsedFilters.Contains(device.Name));
                  }
              }
          }
          catch (Exception)
          {
          }
      }
      cLBDSFilter.Sorted = true;
      lBDSFilter.Sorted = true;
      if (cLBDSFilter.Items.Count > 0)
          cLBDSFilter.SelectedIndex = 0;
      if (lBDSFilter.Items.Count > 0)
          lBDSFilter.SelectedIndex = 0;
    }

    public override void SaveSettings()
    {
      DsDevice tmpDevice = null;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
          xmlwriter.SetValue("mytv", "topscanlinestoremove", topscanlinesTextBox.Text);
          xmlwriter.SetValue("mytv", "bottomscanlinestoremove", bottomscanlinesTextBox.Text);
          xmlwriter.SetValue("mytv", "leftcolumnstoremove", leftcolumnsTextBox.Text);
          xmlwriter.SetValue("mytv", "rightcolumnstoremove", rightcolumnsTextBox.Text);
          for (int i = 0; i < cLBDSFilter.Items.Count; i++)
          {
              tmpDevice = (DsDevice)cLBDSFilter.Items[i];
              xmlwriter.SetValue("mytv", "filter" + i.ToString(), tmpDevice.Name);
              xmlwriter.SetValueAsBool("mytv", "usefilter" + i.ToString(), cLBDSFilter.GetItemChecked(i));
          }
          xmlwriter.SetValue("mytv", "filter" + cLBDSFilter.Items.Count.ToString(), "undefined");
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

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lBDSFilter = new System.Windows.Forms.ListBox();
      this.labelPropertiesHint = new System.Windows.Forms.Label();
      this.bSetup = new System.Windows.Forms.Button();
      this.cLBDSFilter = new System.Windows.Forms.CheckedListBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.rightcolumnsTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.leftcolumnsTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bottomscanlinesTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.topscanlinesTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox3.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.lBDSFilter);
      this.mpGroupBox3.Controls.Add(this.labelPropertiesHint);
      this.mpGroupBox3.Controls.Add(this.bSetup);
      this.mpGroupBox3.Controls.Add(this.cLBDSFilter);
      this.mpGroupBox3.Controls.Add(this.label3);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(3, 121);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(466, 284);
      this.mpGroupBox3.TabIndex = 0;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Custom Filters";
      // 
      // lBDSFilter
      // 
      this.lBDSFilter.FormattingEnabled = true;
      this.lBDSFilter.Location = new System.Drawing.Point(13, 161);
      this.lBDSFilter.Name = "lBDSFilter";
      this.lBDSFilter.Size = new System.Drawing.Size(441, 108);
      this.lBDSFilter.TabIndex = 8;
      this.lBDSFilter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lBDSFilter_MouseDoubleClick);
      // 
      // labelPropertiesHint
      // 
      this.labelPropertiesHint.AutoSize = true;
      this.labelPropertiesHint.Location = new System.Drawing.Point(134, 123);
      this.labelPropertiesHint.Name = "labelPropertiesHint";
      this.labelPropertiesHint.Size = new System.Drawing.Size(265, 13);
      this.labelPropertiesHint.TabIndex = 7;
      this.labelPropertiesHint.Text = "Use this button to edit the settings of the selected filter.";
      // 
      // bSetup
      // 
      this.bSetup.Location = new System.Drawing.Point(13, 118);
      this.bSetup.Name = "bSetup";
      this.bSetup.Size = new System.Drawing.Size(115, 23);
      this.bSetup.TabIndex = 6;
      this.bSetup.Text = "Filter properties";
      this.bSetup.UseVisualStyleBackColor = true;
      this.bSetup.Click += new System.EventHandler(this.bSetup_Click);
      // 
      // cLBDSFilter
      // 
      this.cLBDSFilter.FormattingEnabled = true;
      this.cLBDSFilter.Location = new System.Drawing.Point(13, 63);
      this.cLBDSFilter.Name = "cLBDSFilter";
      this.cLBDSFilter.Size = new System.Drawing.Size(441, 49);
      this.cLBDSFilter.TabIndex = 3;
      this.cLBDSFilter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.cLBDSFilter_MouseDoubleClick);
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(14, 17);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(440, 41);
      this.label3.TabIndex = 2;
      this.label3.Text = "With a doubleclick you can add / remove custom filters like ffdshow raw video or " +
          "MoMoLight for playback. You have to enable them explicitly. Be aware, that some " +
          "filters might break your playback!";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.rightcolumnsTextBox);
      this.mpGroupBox1.Controls.Add(this.leftcolumnsTextBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel4);
      this.mpGroupBox1.Controls.Add(this.mpLabel3);
      this.mpGroupBox1.Controls.Add(this.bottomscanlinesTextBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.Controls.Add(this.topscanlinesTextBox);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(3, 3);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(466, 112);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Cropping";
      // 
      // rightcolumnsTextBox
      // 
      this.rightcolumnsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.rightcolumnsTextBox.BorderColor = System.Drawing.Color.Empty;
      this.rightcolumnsTextBox.Location = new System.Drawing.Point(263, 66);
      this.rightcolumnsTextBox.MaxLength = 3;
      this.rightcolumnsTextBox.Name = "rightcolumnsTextBox";
      this.rightcolumnsTextBox.Size = new System.Drawing.Size(26, 20);
      this.rightcolumnsTextBox.TabIndex = 9;
      this.rightcolumnsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // leftcolumnsTextBox
      // 
      this.leftcolumnsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.leftcolumnsTextBox.BorderColor = System.Drawing.Color.Empty;
      this.leftcolumnsTextBox.Location = new System.Drawing.Point(200, 66);
      this.leftcolumnsTextBox.MaxLength = 3;
      this.leftcolumnsTextBox.Name = "leftcolumnsTextBox";
      this.leftcolumnsTextBox.Size = new System.Drawing.Size(26, 20);
      this.leftcolumnsTextBox.TabIndex = 8;
      this.leftcolumnsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(295, 69);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(124, 13);
      this.mpLabel4.TabIndex = 7;
      this.mpLabel4.Text = "Right columns to remove";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(77, 69);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(117, 13);
      this.mpLabel3.TabIndex = 6;
      this.mpLabel3.Text = "Left columns to remove";
      // 
      // bottomscanlinesTextBox
      // 
      this.bottomscanlinesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.bottomscanlinesTextBox.BorderColor = System.Drawing.Color.Empty;
      this.bottomscanlinesTextBox.Location = new System.Drawing.Point(232, 86);
      this.bottomscanlinesTextBox.MaxLength = 3;
      this.bottomscanlinesTextBox.Name = "bottomscanlinesTextBox";
      this.bottomscanlinesTextBox.Size = new System.Drawing.Size(26, 20);
      this.bottomscanlinesTextBox.TabIndex = 5;
      this.bottomscanlinesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(264, 89);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(137, 13);
      this.mpLabel2.TabIndex = 4;
      this.mpLabel2.Text = "Bottom scanlines to remove";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(103, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(123, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "Top scanlines to remove";
      // 
      // topscanlinesTextBox
      // 
      this.topscanlinesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.topscanlinesTextBox.BorderColor = System.Drawing.Color.Empty;
      this.topscanlinesTextBox.Location = new System.Drawing.Point(232, 45);
      this.topscanlinesTextBox.MaxLength = 3;
      this.topscanlinesTextBox.Name = "topscanlinesTextBox";
      this.topscanlinesTextBox.Size = new System.Drawing.Size(26, 20);
      this.topscanlinesTextBox.TabIndex = 2;
      this.topscanlinesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpLabel1.Location = new System.Drawing.Point(16, 24);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(434, 24);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "MediaPortal can crop the picture for you if you need to remove unwanted video.";
      // 
      // TVPostProcessing
      // 
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.mpGroupBox3);
      this.Name = "TVPostProcessing";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    private void lBDSFilter_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        bool booFound = false;
        for (int i = 0; i < cLBDSFilter.Items.Count; i++)
            if (cLBDSFilter.Items[i] == lBDSFilter.SelectedItem)
                booFound = true;
        if (!booFound)
            cLBDSFilter.Items.Add(lBDSFilter.SelectedItem);
        for (int i = 0; i < cLBDSFilter.Items.Count; i++)
            if (cLBDSFilter.Items[i] == lBDSFilter.SelectedItem)
                cLBDSFilter.SelectedIndex = i;
    }

    private void bSetup_Click(object sender, EventArgs e)
    {
        if (cLBDSFilter.SelectedIndex != -1)
        {
            DirectShowPropertyPage page = new DirectShowPropertyPage((DsDevice)cLBDSFilter.SelectedItem);
            page.Show(this);
        }
    }

      private void cLBDSFilter_MouseDoubleClick(object sender, MouseEventArgs e)
      {
          int tmpIndex = cLBDSFilter.SelectedIndex;
          if (tmpIndex == 0)
              tmpIndex = 1;
          cLBDSFilter.Items.RemoveAt(cLBDSFilter.SelectedIndex);
          if (cLBDSFilter.Items.Count > 0)
              cLBDSFilter.SelectedIndex = tmpIndex - 1;

      }

  }
}

