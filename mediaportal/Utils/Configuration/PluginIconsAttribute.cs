#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Indicates what icons to use for representing a plugin in the Configuration program.
  /// </summary>
  public class PluginIconsAttribute : Attribute
  {
    private string activatedResourceName;
    private string deactivatedResourceName;

    /// <summary>
    /// Indicate what icons to use for representing a plugin in the Configuration program.
    /// </summary>
    /// <param name="activatedResourceName">Indicates the resource to use when the plugin is active.</param>
    /// <param name="deactivatedResourceName">Indicates the resource to use when the plugin is deactivated.</param>
    public PluginIconsAttribute(string activatedResourceName, string deactivatedResourceName)
    {
      ActivatedResourceName = activatedResourceName;
      DeactivatedResourceName = deactivatedResourceName;
    }

    public string ActivatedResourceName
    {
      get { return activatedResourceName; }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("ActivatedResourceName");
        }
        activatedResourceName = value;
      }
    }

    public string DeactivatedResourceName
    {
      get { return deactivatedResourceName; }
      set
      {
        if (value == null)
        {
          throw new ArgumentNullException("DeactivatedResourceName");
        }
        deactivatedResourceName = value;
      }
    }
  }
}