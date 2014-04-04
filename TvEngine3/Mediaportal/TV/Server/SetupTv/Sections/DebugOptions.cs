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

using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;

namespace Mediaportal.TV.Server.SetupTV.Sections
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
      mpCheckBoxTsWriterDumpInputs.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tsWriterDumpInputs", false);
      mpCheckBoxTsMuxerDumpInputs.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue("tsMuxerDumpInputs", false);
    }

    public override void SaveSettings()
    {
      base.SaveSettings();
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tsWriterDumpInputs", mpCheckBoxTsWriterDumpInputs.Checked);
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("tsMuxerDumpInputs", mpCheckBoxTsMuxerDumpInputs.Checked);
    }
  }
}