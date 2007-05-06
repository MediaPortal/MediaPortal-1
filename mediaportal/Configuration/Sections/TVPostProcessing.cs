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
  public class TVPostProcessing : MediaPortal.Configuration.Sections.PostProcessing
  {
    private System.ComponentModel.IContainer components = null;

    public TVPostProcessing()
      : this("TV Post Processing")
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
      components = new System.ComponentModel.Container();
    }
    #endregion
  }
}

