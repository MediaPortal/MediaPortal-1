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
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.MenuManager;
using ProjectInfinity.Settings;
using System.Windows;
using MyWeather.Grabbers;

namespace MyWeather
{
  //[Plugin("My Weather", "My Weather", ListInMenu = true, ImagePath = @"pack://siteoforigin:,,,/skin/default/gfx/tv.png")]
  public class WeatherPlugin : IPlugin, IMenuCommand, IDisposable
  {
    #region IPlugin Members

    public void Initialize(string id)
    {
    }

    #endregion

    #region IMenuCommand Members

    public void Run()
    {
        // Check if there are any settings available already
        WeatherSettings settings = new WeatherSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings);
        if (settings.LocationCode.Equals("<none>"))
        {
            // No Settings found, navigate to Settings first!
            ServiceScope.Get<INavigationService>().Navigate(new WeatherSetup());
        }
        else
        {
            // We already have settings, so lets go to Weather directly
            ServiceScope.Get<INavigationService>().Navigate(new Weather());
        }
    }

    #endregion

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
    }
    #endregion
  }
}
