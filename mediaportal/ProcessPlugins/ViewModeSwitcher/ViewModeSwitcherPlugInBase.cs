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

using MediaPortal.GUI.Library;

namespace ProcessPlugins.ViewModeSwitcher
{
  public abstract class PlugInBase : IPlugin, ISetupForm
  {
    public abstract void Stop();
    public abstract void Start();

    public string PluginName()
    {
      return "ViewModeSwitcher";
    }

    public string Description()
    {
      return
        "Switches to a predefined zoom mode depending on the aspect ratio of the source video. It can also perform overscan and Black Bar cropping.";
    }

    public string Author()
    {
      return "LKuech, -Manfred-";
    }

    public void ShowPlugin()
    {
      ViewModeSwitcherConfig form = new ViewModeSwitcherConfig();
      form.ShowDialog();
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;

      return false;
    }
  }
}