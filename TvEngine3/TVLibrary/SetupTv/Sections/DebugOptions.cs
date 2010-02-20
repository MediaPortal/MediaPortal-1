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

using System.Windows.Forms;
using System.IO;
using TvLibrary;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class DebugOptions : SectionSettings
  {
    public DebugOptions()
      : this("Debug Options") {}

    public DebugOptions(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
    }

    public override void LoadSettings()
    {
      base.LoadSettings();
      ReadSetting(mpResetGraphCheckBox, "ResetGraph");
      ReadSetting(mpUsePatLookupCheckBox, "UsePATLookup");
      ReadSetting(mpDumpRawTSCheckBox, "DumpRawTS");
    }

    public override void SaveSettings()
    {
      base.SaveSettings();
      WriteSetting(mpResetGraphCheckBox, "ResetGraph");
      WriteSetting(mpUsePatLookupCheckBox, "UsePATLookup");
      WriteSetting(mpDumpRawTSCheckBox, "DumpRawTS");
    }

    private void ReadSetting(CheckBox control, string setting)
    {
      control.Checked = DebugSettings.GetSetting(setting);
    }

    private void WriteSetting(CheckBox control, string setting)
    {
      DebugSettings.SetSetting(setting, control.Checked);
    }
  }
}