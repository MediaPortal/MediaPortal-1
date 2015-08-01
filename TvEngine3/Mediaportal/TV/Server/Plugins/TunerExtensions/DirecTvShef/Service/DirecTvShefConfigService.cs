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

using System.Collections.Generic;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Service
{
  internal class DirecTvShefConfigService : IDirecTvShefConfigService
  {
    public delegate void OnSetTopBoxConfigChange(SetTopBoxConfig config);
    public event OnSetTopBoxConfigChange OnConfigChange;

    public SetTopBoxConfig GetSetTopBoxConfigurationForTuner(string tunerExternalId)
    {
      return SetTopBoxConfig.LoadSettings(tunerExternalId);
    }

    public void SaveSetTopBoxConfiguration(ICollection<SetTopBoxConfig> settings)
    {
      foreach (SetTopBoxConfig config in settings)
      {
        config.SaveSettings();
        if (OnConfigChange != null)
        {
          OnConfigChange(config);
        }
      }
    }
  }
}