using System.IO;
using System.Reflection;
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

    /// <summary>
    /// Builds a full path for a given <paramref name="fileName"/> that is located in the same folder as the <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>Combined path</returns>
    public static string BuildAssemblyRelativePath(string fileName)
    {
      string executingPath = Assembly.GetCallingAssembly().Location;
      return Path.Combine(Path.GetDirectoryName(executingPath), fileName);
    }
  }
}
