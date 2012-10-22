using System;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  public class PathManager
  {
    /// <summary>
    /// Returns the path to the Application Data location
    /// </summary>
    /// <returns>Application data path of TvServer</returns>
    public static string GetDataPath
    {
      get
      {
        return GlobalServiceProvider.Instance.Get<IIntegrationProvider>().PathManager.GetPath("<TVCORE>");
      }
    }
  }
}
