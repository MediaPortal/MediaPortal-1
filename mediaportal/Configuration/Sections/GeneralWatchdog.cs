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

using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralWatchdog : SectionSettings
  {
    #region ctor

    public GeneralWatchdog()
      : this("Watchdog") {}

    public GeneralWatchdog(string name)
      : base("Watchdog")
    {
      InitializeComponent();
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        checkBoxEnableWatchdog.Checked = xmlreader.GetValueAsBool("general", "watchdogEnabled", false);
        checkBoxAutoRestart.Checked = xmlreader.GetValueAsBool("general", "restartOnError", true);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "restart delay", 10);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValueAsBool("general", "watchdogEnabled", checkBoxEnableWatchdog.Checked);
        xmlreader.SetValueAsBool("general", "restartOnError", checkBoxAutoRestart.Checked);
        xmlreader.SetValue("general", "restart delay", numericUpDownDelay.Value);
      }
    }

    #endregion
  }
}