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
using TvLibrary.Epg;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// This class provides TV server remote method calls using late binding to TvControl.dll and TvLibrary.Interfaces.dll.
  /// The late binding prevents MediaPortal from depending on TvControl and TvDatabase projects.
  /// </summary>
  public static class TvServerRemote
  {

    /// <summary>
    /// Sets the master TV server hostname for the TV server RemoteControl.  Used by the configuration tool only.
    /// </summary>
    public static string HostName
    {
      set {
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
                  MethodInfo methodInfo = exportedType.GetMethod("SetHostName",
                                                                  BindingFlags.Public | BindingFlags.Instance);
                  object[] parameters = new object[] { value };
                  methodInfo.Invoke(exportedObject, parameters);
                  break;
                }
              }
              catch (TargetInvocationException ex)
              {
                Log.Error("DlgSkinSettings: Failed to set TV server hostname {0}", ex.ToString());
              }
              catch (Exception gex)
              {
                Log.Error("DlgSkinSettings: Failed to load settings {0}", gex.Message);
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Configuration: Loading TvControl assembly");
          Log.Error("Configuration: Exception: {0}", ex);
        }
      }
    }

    /// <summary>
    /// Retrieve a list of MediaPortal genres from the TV server (calls TvBusinessLayer.GetMpGenres()).
    /// </summary>
    /// <returns>List of MediaPortal genre objects</returns>
    public static List<MpGenre> GetMpGenres()
    {
      List<MpGenre> genres = null;
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
                genres = methodInfo.Invoke(exportedObject, null) as List<MpGenre>;
                break;
              }
            }
            catch (TargetInvocationException ex)
            {
              Log.Error("DlgSkinSettings: Failed to load program genres {0}", ex.ToString());
            }
            catch (Exception gex)
            {
              Log.Error("DlgSkinSettings: Failed to load settings {0}", gex.Message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Configuration: Loading TvControl assembly");
        Log.Error("Configuration: Exception: {0}", ex);
      }
      return genres;
    }

    /// <summary>
    /// Retrieves a list of available languages and language codes from the TvServer.
    /// Calls TvLibrary.Epg.Languages.GetLanguages() and TvLibrary.Epg.Languages.GetLanguageCode().
    /// </summary>
    /// <param name="languagesAvailable">A list of available Epg languages</param>
    /// <param name="languageCodes">A list of Epg language codes</param>
    public static void GetLanguages(out List<string> languagesAvailable, out List<string> languageCodes)
    {
      languagesAvailable = null;
      languageCodes = null;
      try
      {
        Assembly assem = Assembly.LoadFrom(Config.GetFolder(Config.Dir.Base) + "\\TvLibrary.Interfaces.dll");
        if (assem != null)
        {
          Type[] types = assem.GetExportedTypes();
          foreach (Type exportedType in types)
          {
            try
            {
              if (exportedType.Name == "Languages")
              {
                // Load available languages into variables. 
                Object languageObject = null;
                languageObject = Activator.CreateInstance(exportedType);
                MethodInfo methodInfo = exportedType.GetMethod("GetLanguages",
                                                               BindingFlags.Public | BindingFlags.Instance);
                languagesAvailable = methodInfo.Invoke(languageObject, null) as List<String>;
                methodInfo = exportedType.GetMethod("GetLanguageCodes", BindingFlags.Public | BindingFlags.Instance);
                languageCodes = (List<String>)methodInfo.Invoke(languageObject, null);
              }
            }
            catch (TargetInvocationException ex)
            {
              Log.Error("TVClient: Failed to load languages {0}", ex.ToString());
            }
            catch (Exception gex)
            {
              Log.Error("TVClient: Failed to load settings {0}", gex.Message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Configuration: Loading TvLibrary.Interface assembly");
        Log.Error("Configuration: Exception: {0}", ex);
      }
    }

  }
}