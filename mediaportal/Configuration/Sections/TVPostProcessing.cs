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
    private CheckedListBox cLBDSFilter;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private Label labelPropertiesHint;
    private Button bSetup;
    private ListBox lBDSFilter;
    private MediaPortal.UserInterface.Controls.MPGroupBox allFiltersGroupBox;
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
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
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
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
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
      this.labelPropertiesHint = new System.Windows.Forms.Label();
      this.bSetup = new System.Windows.Forms.Button();
      this.cLBDSFilter = new System.Windows.Forms.CheckedListBox();
      this.lBDSFilter = new System.Windows.Forms.ListBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.allFiltersGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox3.SuspendLayout();
      this.allFiltersGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.labelPropertiesHint);
      this.mpGroupBox3.Controls.Add(this.bSetup);
      this.mpGroupBox3.Controls.Add(this.cLBDSFilter);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(3, 30);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(466, 124);
      this.mpGroupBox3.TabIndex = 0;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Activated filters";
      // 
      // labelPropertiesHint
      // 
      this.labelPropertiesHint.AutoSize = true;
      this.labelPropertiesHint.Location = new System.Drawing.Point(134, 94);
      this.labelPropertiesHint.Name = "labelPropertiesHint";
      this.labelPropertiesHint.Size = new System.Drawing.Size(265, 13);
      this.labelPropertiesHint.TabIndex = 3;
      this.labelPropertiesHint.Text = "Use this button to edit the settings of the selected filter.";
      // 
      // bSetup
      // 
      this.bSetup.Location = new System.Drawing.Point(13, 89);
      this.bSetup.Name = "bSetup";
      this.bSetup.Size = new System.Drawing.Size(115, 23);
      this.bSetup.TabIndex = 2;
      this.bSetup.Text = "Filter properties";
      this.bSetup.UseVisualStyleBackColor = true;
      this.bSetup.Click += new System.EventHandler(this.bSetup_Click);
      // 
      // cLBDSFilter
      // 
      this.cLBDSFilter.FormattingEnabled = true;
      this.cLBDSFilter.Location = new System.Drawing.Point(13, 19);
      this.cLBDSFilter.Name = "cLBDSFilter";
      this.cLBDSFilter.Size = new System.Drawing.Size(441, 64);
      this.cLBDSFilter.TabIndex = 1;
      this.cLBDSFilter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.cLBDSFilter_MouseDoubleClick);
      // 
      // lBDSFilter
      // 
      this.lBDSFilter.FormattingEnabled = true;
      this.lBDSFilter.Location = new System.Drawing.Point(13, 19);
      this.lBDSFilter.Name = "lBDSFilter";
      this.lBDSFilter.Size = new System.Drawing.Size(441, 212);
      this.lBDSFilter.TabIndex = 4;
      this.lBDSFilter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lBDSFilter_MouseDoubleClick);
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.ForeColor = System.Drawing.Color.Red;
      this.label3.Location = new System.Drawing.Point(3, 7);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(440, 13);
      this.label3.TabIndex = 0;
      this.label3.Text = "USE THIS AT YOUR OWN RISK!";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // allFiltersGroupBox
      // 
      this.allFiltersGroupBox.Controls.Add(this.lBDSFilter);
      this.allFiltersGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.allFiltersGroupBox.Location = new System.Drawing.Point(3, 160);
      this.allFiltersGroupBox.Name = "allFiltersGroupBox";
      this.allFiltersGroupBox.Size = new System.Drawing.Size(466, 245);
      this.allFiltersGroupBox.TabIndex = 1;
      this.allFiltersGroupBox.TabStop = false;
      this.allFiltersGroupBox.Text = "Available filters";
      // 
      // TVPostProcessing
      // 
      this.Controls.Add(this.allFiltersGroupBox);
      this.Controls.Add(this.mpGroupBox3);
      this.Controls.Add(this.label3);
      this.Name = "TVPostProcessing";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.allFiltersGroupBox.ResumeLayout(false);
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

