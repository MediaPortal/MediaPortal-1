using System;

namespace TvLibrary.Interfaces
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
        return String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server",
                 Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        ;
      }
    }

  }
}
