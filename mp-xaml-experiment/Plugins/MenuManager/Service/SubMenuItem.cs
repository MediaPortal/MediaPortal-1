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
 *  Code modified from SharpDevelop AddIn code
 *  Thanks goes to: Mike Krüger
 */

#endregion

using System;
using ProjectInfinity.Localisation;
using ProjectInfinity.Logging;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.MenuManager
{
  public class SubMenuItem : MenuItem, IMenuItem
  {
    #region Variables
    IMenuCommand _menuCommand = null;
    #endregion

    #region Constructors/Destructors
    public SubMenuItem(INodeItem item, object caller)
      : base(item, caller)
    {

    }
    #endregion

    #region Properties
    public string SubMenuPath
    {
      get { return base._item["path"]; }
    }
    #endregion

 
    [Obsolete("This method will disappear very soon")]
    void IMenuItem.Execute()
    {
      if (_menuCommand == null)
      {
        try
        {
          _menuCommand = (IMenuCommand)base._item.CreateObject(base._item["class"]);
        }
        catch (Exception e)
        {
          ServiceScope.Get<ILogger>().Error(e.ToString() + "Can't create menu command : " + base._item.Id);
          return;
        }
      }

      _menuCommand.Run();
    }

    void IMenuItem.Accept(IMenuItemVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
