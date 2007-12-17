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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectInfinity;
using ProjectInfinity.Logging;
using System.IO;
using System.Windows.Markup;
using ProjectInfinity.MenuManager;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Pictures
{
  /// <summary>
  /// Interaction logic for PixtureSetup.xaml
  /// </summary>

  public class PictureSetup : View, IMenuCommand, IDisposable
  {

    public PictureSetup()
    {
      DataContext = new SettingsViewModel();
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
    }

    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new PictureSetup());
    }

    #region IDisposable Members

    public void Dispose()
    {
    }

    #endregion
  }
}