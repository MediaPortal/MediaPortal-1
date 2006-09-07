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
  public class DVDPostProcessing : MediaPortal.Configuration.SectionSettings
  {
      private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxCustomFilters;
      private Label labelSetupButtonHint;
      private Button bSetup;
      private CheckedListBox cLBDSFilter;
      private ListBox lBDSFilter;
      private MediaPortal.UserInterface.Controls.MPLabel labelTopHint;
    private System.ComponentModel.IContainer components = null;

    public DVDPostProcessing()
      : this("DVD Post Processing")
    {
    }

    public DVDPostProcessing(string name)
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
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
            int intCount = 0;
            while (xmlreader.GetValueAsString("dvdplayer", "filter" + intCount.ToString(), "undefined") != "undefined")
            {
                strFilters += xmlreader.GetValueAsString("dvdplayer", "filter" + intCount.ToString(), "undefined") + ";";
                if (xmlreader.GetValueAsBool("dvdplayer", "usefilter" + intCount.ToString(), false))
                {
                    strUsedFilters += xmlreader.GetValueAsString("dvdplayer", "filter" + intCount.ToString(), "undefined") + ";";
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
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
            for (int i = 0; i < cLBDSFilter.Items.Count; i++)
            {
                tmpDevice = (DsDevice)cLBDSFilter.Items[i];
                xmlwriter.SetValue("dvdplayer", "filter" + i.ToString(), tmpDevice.Name);
                xmlwriter.SetValueAsBool("dvdplayer", "usefilter" + i.ToString(), cLBDSFilter.GetItemChecked(i));
            }
            xmlwriter.SetValue("dvdplayer", "filter" + cLBDSFilter.Items.Count.ToString(), "undefined");
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
      this.groupBoxCustomFilters = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelSetupButtonHint = new System.Windows.Forms.Label();
      this.bSetup = new System.Windows.Forms.Button();
      this.cLBDSFilter = new System.Windows.Forms.CheckedListBox();
      this.lBDSFilter = new System.Windows.Forms.ListBox();
      this.labelTopHint = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxCustomFilters.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxCustomFilters
      // 
      this.groupBoxCustomFilters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxCustomFilters.Controls.Add(this.labelSetupButtonHint);
      this.groupBoxCustomFilters.Controls.Add(this.bSetup);
      this.groupBoxCustomFilters.Controls.Add(this.cLBDSFilter);
      this.groupBoxCustomFilters.Controls.Add(this.lBDSFilter);
      this.groupBoxCustomFilters.Controls.Add(this.labelTopHint);
      this.groupBoxCustomFilters.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCustomFilters.Location = new System.Drawing.Point(0, 0);
      this.groupBoxCustomFilters.Name = "groupBoxCustomFilters";
      this.groupBoxCustomFilters.Size = new System.Drawing.Size(472, 405);
      this.groupBoxCustomFilters.TabIndex = 0;
      this.groupBoxCustomFilters.TabStop = false;
      this.groupBoxCustomFilters.Text = "Custom Filters";
      // 
      // labelSetupButtonHint
      // 
      this.labelSetupButtonHint.AutoSize = true;
      this.labelSetupButtonHint.Location = new System.Drawing.Point(137, 143);
      this.labelSetupButtonHint.Name = "labelSetupButtonHint";
      this.labelSetupButtonHint.Size = new System.Drawing.Size(265, 13);
      this.labelSetupButtonHint.TabIndex = 10;
      this.labelSetupButtonHint.Text = "Use this button to edit the settings of the selected filter.";
      // 
      // bSetup
      // 
      this.bSetup.Location = new System.Drawing.Point(16, 138);
      this.bSetup.Name = "bSetup";
      this.bSetup.Size = new System.Drawing.Size(115, 23);
      this.bSetup.TabIndex = 8;
      this.bSetup.Text = "Filter properties";
      this.bSetup.UseVisualStyleBackColor = true;
      this.bSetup.Click += new System.EventHandler(this.bSetup_Click);
      // 
      // cLBDSFilter
      // 
      this.cLBDSFilter.FormattingEnabled = true;
      this.cLBDSFilter.Location = new System.Drawing.Point(16, 68);
      this.cLBDSFilter.Name = "cLBDSFilter";
      this.cLBDSFilter.Size = new System.Drawing.Size(441, 64);
      this.cLBDSFilter.TabIndex = 7;
      this.cLBDSFilter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.cLBDSFilter_MouseDoubleClick);
      // 
      // lBDSFilter
      // 
      this.lBDSFilter.FormattingEnabled = true;
      this.lBDSFilter.Location = new System.Drawing.Point(16, 184);
      this.lBDSFilter.Name = "lBDSFilter";
      this.lBDSFilter.Size = new System.Drawing.Size(441, 199);
      this.lBDSFilter.TabIndex = 9;
      this.lBDSFilter.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lBDSFilter_MouseDoubleClick);
      // 
      // labelTopHint
      // 
      this.labelTopHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelTopHint.Location = new System.Drawing.Point(15, 22);
      this.labelTopHint.Name = "labelTopHint";
      this.labelTopHint.Size = new System.Drawing.Size(427, 41);
      this.labelTopHint.TabIndex = 6;
      this.labelTopHint.Text = "With a doubleclick you can add / remove custom filters like ffdshow raw video or " +
          "MoMoLight for playback. You have to enable them explicitly. Be aware, that some " +
          "filters might break your playback!";
      // 
      // DVDPostProcessing
      // 
      this.Controls.Add(this.groupBoxCustomFilters);
      this.Name = "DVDPostProcessing";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxCustomFilters.ResumeLayout(false);
      this.groupBoxCustomFilters.PerformLayout();
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

