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
using MediaPortal.Profile;
using MediaPortal.ServiceImplementations;

namespace MediaPortal.Configuration.Sections
{
  public partial class FiltersVideoRenderer : SectionSettings
  {
    private bool _init = false;

    public FiltersVideoRenderer()
      : this("Video Renderer Settings") {}

    public FiltersVideoRenderer(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      base.LoadSettings();
      if (_init == false)
      {
        using (Settings xmlreader = new MPSettings())
        {
          //VMR9 settings
          checkboxMpNonsquare.Checked = xmlreader.GetValueAsBool("general", "nonsquare", true);
          // http://msdn2.microsoft.com/en-us/library/ms787438(VS.85).aspx
          checkboxDXEclusive.Checked = xmlreader.GetValueAsBool("general", "exclusivemode", true);
          mpVMR9FilterMethod.Text = xmlreader.GetValueAsString("general", "dx9filteringmode", "Gaussian Quad Filtering");
          // http://msdn2.microsoft.com/en-us/library/ms788066.aspx
          checkBoxVMRWebStreams.Checked = xmlreader.GetValueAsBool("general", "usevrm9forwebstreams", true);
          checkBoxDecimateMask.Checked = xmlreader.GetValueAsBool("general", "dx9decimatemask", false);
          // http://msdn2.microsoft.com/en-us/library/ms787452(VS.85).aspx

          bool ValueEVR = false;

          try
          {
            //EVR - VMR9 selection
            int ver = (OSInfo.OSInfo.OSMajorVersion * 10) + OSInfo.OSInfo.OSMinorVersion;
            ValueEVR = ver >= 60 ? true : false;
          }
          catch (Exception ex)
          {
            Log.Error("FilterVideoRendererConfig: Os detection unsuccessful - {0}", ex.Message);
          }

          radioButtonEVR.Checked = xmlreader.GetValueAsBool("general", "useEVRenderer", ValueEVR);
        }
        _init = true;
      }
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadSettings();
    }

    public override void SaveSettings()
    {
      if (_init == false) return;
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "nonsquare", checkboxMpNonsquare.Checked);
        xmlwriter.SetValueAsBool("general", "exclusivemode", checkboxDXEclusive.Checked);
        xmlwriter.SetValue("general", "dx9filteringmode", mpVMR9FilterMethod.Text);
        xmlwriter.SetValueAsBool("general", "usevrm9forwebstreams", checkBoxVMRWebStreams.Checked);
        xmlwriter.SetValueAsBool("general", "dx9decimatemask", checkBoxDecimateMask.Checked);
        xmlwriter.SetValueAsBool("general", "useEVRenderer", radioButtonEVR.Checked);
      }
    }

    private void radioButtonEVR_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonEVR.Checked == true)
      {
        checkBoxVMRWebStreams.Enabled = false;
        checkboxDXEclusive.Enabled = false;
        checkboxMpNonsquare.Enabled = false;
        checkBoxDecimateMask.Enabled = false;
        mpVMR9FilterMethod.Enabled = false;
        labelFilteringHint.Enabled = false;
      }
    }

    private void radioButtonVMR9_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonVMR9.Checked == true)
      {
        checkBoxVMRWebStreams.Enabled = true;
        checkboxDXEclusive.Enabled = true;
        checkboxMpNonsquare.Enabled = true;
        checkBoxDecimateMask.Enabled = true;
        mpVMR9FilterMethod.Enabled = true;
        labelFilteringHint.Enabled = true;
      }
    }
  }
}