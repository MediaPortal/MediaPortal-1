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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class CMSetup : SetupTv.SectionSettings
  {
    #region constructors

    public CMSetup()
      : this("Conflicts Manager Setup") {}

    public CMSetup(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Public Members

    public override void OnSectionActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      analyzeMode.SelectedIndex = Convert.ToInt32(layer.GetSetting("CMAnalyzeMode", "0").Value);
      debug.Checked = layer.GetSetting("CMDebugMode", "false").Value == "true";
    }

    public override void OnSectionDeActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("CMDebugMode", "false");
      if (debug.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();
      base.OnSectionDeActivated();
    }

    #endregion
  }
}