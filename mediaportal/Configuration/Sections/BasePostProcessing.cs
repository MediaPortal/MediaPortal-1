#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using FilterCategory = DirectShowLib.FilterCategory;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class BasePostProcessing : SectionSettings
  {
    private MPGroupBox groupBoxActivatedFilters;
    private MPGroupBox groupBoxAvailableFilters;
    private MPLabel labelWarning;
    private MPLabel labelPropertiesHint;
    private CheckedListBox cLBDSFilter;
    private Button bSetup;
    private ListBox lBDSFilter;
    private IContainer components = null;

    public BasePostProcessing()
      : this("Post Processing") {}

    public BasePostProcessing(string name)
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
    private void InitializeComponent()
    {
      this.groupBoxActivatedFilters = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelPropertiesHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.bSetup = new System.Windows.Forms.Button();
      this.cLBDSFilter = new System.Windows.Forms.CheckedListBox();
      this.groupBoxAvailableFilters = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lBDSFilter = new System.Windows.Forms.ListBox();
      this.labelWarning = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxActivatedFilters.SuspendLayout();
      this.groupBoxAvailableFilters.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxActivatedFilters
      // 
      this.groupBoxActivatedFilters.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxActivatedFilters.Controls.Add(this.labelPropertiesHint);
      this.groupBoxActivatedFilters.Controls.Add(this.bSetup);
      this.groupBoxActivatedFilters.Controls.Add(this.cLBDSFilter);
      this.groupBoxActivatedFilters.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxActivatedFilters.Location = new System.Drawing.Point(6, 25);
      this.groupBoxActivatedFilters.Name = "groupBoxActivatedFilters";
      this.groupBoxActivatedFilters.Size = new System.Drawing.Size(462, 166);
      this.groupBoxActivatedFilters.TabIndex = 0;
      this.groupBoxActivatedFilters.TabStop = false;
      this.groupBoxActivatedFilters.Text = "Activated filters";
      // 
      // labelPropertiesHint
      // 
      this.labelPropertiesHint.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelPropertiesHint.AutoSize = true;
      this.labelPropertiesHint.Location = new System.Drawing.Point(134, 140);
      this.labelPropertiesHint.Name = "labelPropertiesHint";
      this.labelPropertiesHint.Size = new System.Drawing.Size(265, 13);
      this.labelPropertiesHint.TabIndex = 3;
      this.labelPropertiesHint.Text = "Use this button to edit the settings of the selected filter.";
      // 
      // bSetup
      // 
      this.bSetup.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.bSetup.Location = new System.Drawing.Point(16, 135);
      this.bSetup.Name = "bSetup";
      this.bSetup.Size = new System.Drawing.Size(112, 22);
      this.bSetup.TabIndex = 2;
      this.bSetup.Text = "Filter properties";
      this.bSetup.UseVisualStyleBackColor = true;
      this.bSetup.Click += new System.EventHandler(this.bSetup_Click);
      // 
      // cLBDSFilter
      // 
      this.cLBDSFilter.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cLBDSFilter.FormattingEnabled = true;
      this.cLBDSFilter.Location = new System.Drawing.Point(16, 24);
      this.cLBDSFilter.Name = "cLBDSFilter";
      this.cLBDSFilter.Size = new System.Drawing.Size(430, 94);
      this.cLBDSFilter.TabIndex = 1;
      this.cLBDSFilter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.cLBDSFilter_MouseDoubleClick);
      // 
      // groupBoxAvailableFilters
      // 
      this.groupBoxAvailableFilters.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAvailableFilters.Controls.Add(this.lBDSFilter);
      this.groupBoxAvailableFilters.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAvailableFilters.Location = new System.Drawing.Point(6, 197);
      this.groupBoxAvailableFilters.Name = "groupBoxAvailableFilters";
      this.groupBoxAvailableFilters.Size = new System.Drawing.Size(462, 211);
      this.groupBoxAvailableFilters.TabIndex = 1;
      this.groupBoxAvailableFilters.TabStop = false;
      this.groupBoxAvailableFilters.Text = "Available filters";
      // 
      // lBDSFilter
      // 
      this.lBDSFilter.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.lBDSFilter.FormattingEnabled = true;
      this.lBDSFilter.Location = new System.Drawing.Point(16, 24);
      this.lBDSFilter.Name = "lBDSFilter";
      this.lBDSFilter.Size = new System.Drawing.Size(430, 173);
      this.lBDSFilter.TabIndex = 4;
      this.lBDSFilter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lBDSFilter_MouseDoubleClick);
      // 
      // labelWarning
      // 
      this.labelWarning.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.labelWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold,
                                                       System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelWarning.ForeColor = System.Drawing.Color.Red;
      this.labelWarning.Location = new System.Drawing.Point(0, 0);
      this.labelWarning.Name = "labelWarning";
      this.labelWarning.Size = new System.Drawing.Size(472, 22);
      this.labelWarning.TabIndex = 0;
      this.labelWarning.Text = "USE THIS AT YOUR OWN RISK!";
      this.labelWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // BasePostProcessing
      // 
      this.Controls.Add(this.groupBoxAvailableFilters);
      this.Controls.Add(this.groupBoxActivatedFilters);
      this.Controls.Add(this.labelWarning);
      this.Name = "BasePostProcessing";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxActivatedFilters.ResumeLayout(false);
      this.groupBoxActivatedFilters.PerformLayout();
      this.groupBoxAvailableFilters.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    private void lBDSFilter_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      bool booFound = false;
      for (int i = 0; i < cLBDSFilter.Items.Count; i++)
      {
        if (cLBDSFilter.Items[i] == lBDSFilter.SelectedItem)
        {
          booFound = true;
        }
      }
      if (!booFound)
      {
        cLBDSFilter.Items.Add(lBDSFilter.SelectedItem);
      }
      for (int i = 0; i < cLBDSFilter.Items.Count; i++)
      {
        if (cLBDSFilter.Items[i] == lBDSFilter.SelectedItem)
        {
          cLBDSFilter.SelectedIndex = i;
        }
      }
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
      if (tmpIndex == -1)
      {
        return;
      }
      if (tmpIndex == 0)
      {
        tmpIndex = 1;
      }
      cLBDSFilter.Items.RemoveAt(cLBDSFilter.SelectedIndex);
      if (cLBDSFilter.Items.Count > 0)
      {
        cLBDSFilter.SelectedIndex = tmpIndex - 1;
      }
    }

    protected void LoadSettings(string section)
    {
      string strFilters = "";
      string strUsedFilters = "";
      cLBDSFilter.Sorted = false;
      lBDSFilter.Sorted = false;
      cLBDSFilter.DisplayMember = "Name";
      lBDSFilter.DisplayMember = "Name";
      lBDSFilter.FormattingEnabled = true;
      using (Settings xmlreader = new MPSettings())
      {
        int intCount = 0;
        while (xmlreader.GetValueAsString(section, "filter" + intCount.ToString(), "undefined") != "undefined")
        {
          strFilters += xmlreader.GetValueAsString(section, "filter" + intCount.ToString(), "undefined") + ";";
          if (xmlreader.GetValueAsBool(section, "usefilter" + intCount.ToString(), false))
          {
            strUsedFilters += xmlreader.GetValueAsString(section, "filter" + intCount.ToString(), "undefined") + ";";
          }
          intCount++;
        }
      }
      foreach (DsDevice device in DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory))
      {
        try
        {
          if (device.Name != null)
          {
            lBDSFilter.Items.Add(device);
            foreach (string filter in strFilters.Split(';'))
            {
              if (filter.Equals(device.Name))
              {
                cLBDSFilter.Items.Add(device);
                cLBDSFilter.SetItemChecked(cLBDSFilter.Items.Count - 1, strUsedFilters.Contains(device.Name));
              }
            }
          }
        }
        catch (Exception) {}
      }
      cLBDSFilter.Sorted = true;
      lBDSFilter.Sorted = true;
      if (cLBDSFilter.Items.Count > 0)
      {
        cLBDSFilter.SelectedIndex = 0;
      }
      if (lBDSFilter.Items.Count > 0)
      {
        lBDSFilter.SelectedIndex = 0;
      }
    }

    protected void SaveSettings(string section)
    {
      DsDevice tmpDevice = null;
      using (Settings xmlwriter = new MPSettings())
      {
        for (int i = 0; i < cLBDSFilter.Items.Count; i++)
        {
          tmpDevice = (DsDevice)cLBDSFilter.Items[i];
          xmlwriter.SetValue(section, "filter" + i.ToString(), tmpDevice.Name);
          xmlwriter.SetValueAsBool(section, "usefilter" + i.ToString(), cLBDSFilter.GetItemChecked(i));
        }
        xmlwriter.SetValue(section, "filter" + cLBDSFilter.Items.Count.ToString(), "undefined");
      }
    }
  }
}