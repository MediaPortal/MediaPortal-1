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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using MediaPortal.GUI.Library;
using TvLibrary.Interfaces;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// This class provides TV server remote method calls.
  /// </summary>
  public static class TvServerRemote
  {

    /// <summary>
    /// Retrieve a list of MediaPortal genres from the TV server (calls TvBusinessLayer.GetMpGenres()).
    /// </summary>
    /// <returns>List of genre strings</returns>
    public static List<IMpGenre> GetMpGenres()
    {
      List<IMpGenre> genres = new List<IMpGenre>();
      try
      {
        Assembly assem = Assembly.LoadFrom(Config.GetFolder(Config.Dir.Base) + "\\TvControl.dll");
        if (assem != null)
        {
          Type[] types = assem.GetExportedTypes();
          foreach (Type exportedType in types)
          {
            try
            {
              if (exportedType.Name == "TvServer")
              {
                // Execute the remote method call to the tv server.
                Object exportedObject = null;
                exportedObject = Activator.CreateInstance(exportedType);
                MethodInfo methodInfo = exportedType.GetMethod("GetMpGenres",
                                                                BindingFlags.Public | BindingFlags.Instance);
                genres = methodInfo.Invoke(exportedObject, null) as List<IMpGenre>;
              }
            }
            catch (TargetInvocationException ex)
            {
              Log.Warn("DlgSkinSettings: Failed to load program genres {0}", ex.ToString());
              continue;
            }
            catch (Exception gex)
            {
              Log.Warn("DlgSkinSettings: Failed to load settings {0}", gex.Message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Debug("Configuration: Loading TVLibrary.Interface assembly");
        Log.Debug("Configuration: Exception: {0}", ex);
      }
      return genres;
    }
  }
}