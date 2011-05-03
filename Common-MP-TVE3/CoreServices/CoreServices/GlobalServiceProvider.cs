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
using System.IO;
using MediaPortal.Configuration;

namespace MediaPortal.CoreServices
{
  /// <summary>
  /// The global service provider
  /// </summary>
  public class GlobalServiceProvider
  {
    private static readonly object _syncObj = new object();
    private static ServiceProvider _provider;

    /// <summary>
    /// returns the instance of the global ServiceProvider
    /// </summary>
    public static ServiceProvider Instance
    {
      get
      {
        lock (_syncObj)
        {
          if (_provider == null)
          {
            _provider = new ServiceProvider();
            RegisterCoreServices();
          }
        }
        return _provider;
      }
    }

    public static void RegisterCoreServices()
    {
      string loggerName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
      string dataPath = Configuration.Config.GetFolder(Configuration.Config.Dir.Config);
#if DEBUG
      loggerName = loggerName.Replace(".vshost", "");
#endif
      ILogger logger = new Log4netLogger(loggerName, dataPath);
      Instance.Add<ILogger>(logger);
      logger.Debug("RegisterCoreServicies: Registering ILogger service as [{0}], {1}", loggerName, dataPath);
      logger.Debug("Config Directories:");
      logger.Debug("  -> Base    : {0}", Config.GetFolder(Config.Dir.Base));
      logger.Debug("  -> Cache   : {0}", Config.GetFolder(Config.Dir.Cache));
      logger.Debug("  -> Config  : {0}", Config.GetFolder(Config.Dir.Config));
      logger.Debug("  -> Database: {0}", Config.GetFolder(Config.Dir.Database));
      logger.Debug("  -> Language: {0}", Config.GetFolder(Config.Dir.Language));
      logger.Debug("  -> Log     : {0}", Config.GetFolder(Config.Dir.Log));
      logger.Debug("  -> Plugins : {0}", Config.GetFolder(Config.Dir.Plugins));
      logger.Debug("  -> Skin    : {0}", Config.GetFolder(Config.Dir.Skin));
      logger.Debug("  -> Thumbs  : {0}", Config.GetFolder(Config.Dir.Thumbs));
      logger.Debug("  -> Weather : {0}", Config.GetFolder(Config.Dir.Weather));
    }

  }
}
