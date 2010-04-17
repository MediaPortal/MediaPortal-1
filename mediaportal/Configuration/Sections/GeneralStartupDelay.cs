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

using System.ComponentModel;
using System.ServiceProcess;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralStartupDelay : SectionSettings
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private new const IContainer components = null;

    #region ctor

    public GeneralStartupDelay()
      : this("Startup Delay") { }

    public GeneralStartupDelay(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        nudDelay.Value = xmlreader.GetValueAsInt("general", "delay", 0);
        mpCheckBoxMpStartup.Checked = xmlreader.GetValueAsBool("general", "delay startup", false);
        mpCheckBoxMpResume.Checked = xmlreader.GetValueAsBool("general", "delay resume", false);
      }
      //
      // On single seat WaitForTvService is forced enabled !
      //
      cbWaitForTvService.Checked = Common.IsSingleSeat();
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValue("general", "delay", nudDelay.Value);
        xmlreader.SetValueAsBool("general", "delay startup", mpCheckBoxMpStartup.Checked);
        xmlreader.SetValueAsBool("general", "delay resume", mpCheckBoxMpResume.Checked);
        xmlreader.SetValueAsBool("general", "wait for tvserver", cbWaitForTvService.Checked);
      }
    }

    #endregion
  }
}