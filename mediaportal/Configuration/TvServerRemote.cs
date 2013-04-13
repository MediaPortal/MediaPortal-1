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
using System.Collections.Generic;
using System.Reflection;
using MediaPortal.GUI.Library;
using TvLibrary.Epg;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// This class provides TV server remote method calls using late binding to TvControl.dll
  /// The late binding prevents MediaPortal from depending on TvControl and TvLibrary.Interfaces projects.
  /// </summary>
  public static class TvServerRemote
  {

    /// <summary>
    /// Sets the master TV server hostname for the TV server RemoteControl (calls TvControl.TvServer.SetHostName()).
    /// </summary>
    public static string HostName
    {
      set
      {
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
                Log.Error("SetHostName: Failed to set TV server hostname {0}", ex.ToString());
              }
              catch (Exception gex)
              {
                Log.Error("SetHostName: Failed to load settings {0}", gex.Message);
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("SetHostName: Exception loading TvControl assembly - {0}", ex);
        }
      }
    }

    /// <summary>
    /// Retrieve a list of MediaPortal genres from the TV server (calls TvControl.TvServer.GetMpGenres()).
    /// </summary>
    /// <returns>List of MediaPortal genre objects</returns>
    public static List<MpGenre> GetMpGenres()
    {
      List<MpGenre> genres = new List<MpGenre>();
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
                List<MpGenre> result = methodInfo.Invoke(exportedObject, null) as List<MpGenre>;
                if (result != null)
                  genres = result;
                break;
              }
            }
            catch (TargetInvocationException ex)
            {
              Log.Error("GetMpGenres: Failed to load program genres {0}", ex.ToString());
            }
            catch (Exception gex)
            {
              Log.Error("GetMpGenres: Failed to load settings {0}", gex.Message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("GetMpGenres: Exception loading TvControl assembly - {0}", ex);
      }
      return genres;
    }

    /// <summary>
    /// Retrieves the tv database connection string and provider from the Tv server
    /// Calls TvControl.TvServer.GetDatabaseConnectionString().
    /// </summary>
    /// <param name="connectionString">The database connection string</param>
    /// <param name="provider">The database default provider</param>
    public static void GetDatabaseConnectionString(out string connectionString, out string provider)
    {
      connectionString = null;
      provider = null;
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
                Object exportedObject = Activator.CreateInstance(exportedType);
                object[] parametersArray = new object[] { null, null };
                MethodInfo methodInfo = exportedType.GetMethod("GetDatabaseConnectionString",
                                                               BindingFlags.Public | BindingFlags.Instance);
                methodInfo.Invoke(exportedObject, parametersArray);
                connectionString = (string)parametersArray[0];
                provider = (string)parametersArray[1];
                break;
              }
            }
            catch (TargetInvocationException ex)
            {
              Log.Error("GetDatabaseConnectionString: Failed to load the database connection string {0}", ex.ToString());
            }
            catch (Exception gex)
            {
              Log.Error("GetDatabaseConnectionString: Failed to load settings {0}", gex.Message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("GetDatabaseConnectionString: Exception loading TvControl assembly - {0}", ex);
      }
    }

    /// <summary>
    /// Retrieves the radio channel group names from the TV server (calls TvControl.TvServer.GetRadioChannelGroupNames()).
    /// </summary>
    /// <returns>List of the radio channel group names</returns>
    public static List<string> GetRadioChannelGroupNames()
    {
      List<string> groupNames = new List<string>();
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
                MethodInfo methodInfo = exportedType.GetMethod("GetRadioChannelGroupNames",
                                                                BindingFlags.Public | BindingFlags.Instance);
                List<string> result = methodInfo.Invoke(exportedObject, null) as List<string>;
                if (result != null)
                  groupNames = result;
                break;
              }
            }
            catch (TargetInvocationException ex)
            {
              Log.Error("GetRadioChannelGroupNames: Failed to load radio channel group names - {0}", ex.ToString());
            }
            catch (Exception gex)
            {
              Log.Error("GetRadioChannelGroupNames: Failed to load settings - {0}", gex.Message);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("GetRadioChannelGroupNames: Exception loading TvControl assembly - {0}", ex);
      }
      return groupNames;
    }

  }
}