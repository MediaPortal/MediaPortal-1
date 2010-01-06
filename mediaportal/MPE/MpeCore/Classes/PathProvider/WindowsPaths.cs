using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Interfaces;

namespace MpeCore.Classes.PathProvider
{
  public class WindowsPaths : IPathProvider
  {
    private Dictionary<string, string> _paths;

    public WindowsPaths()
    {
      _paths = new Dictionary<string, string>();
      //_paths.Add("%ProgramFiles", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

      foreach (string options in Enum.GetNames(typeof (Environment.SpecialFolder)))
      {
        _paths.Add(string.Format("%{0}%", options),
                   Environment.GetFolderPath(
                     (Environment.SpecialFolder)Enum.Parse(typeof (Environment.SpecialFolder), options)));
      }
    }

    #region IPathProvider Members

    public string Name
    {
      get { return "WindowsPaths"; }
    }

    public Dictionary<string, string> Paths
    {
      get { return _paths; }
    }

    public string Colapse(string fileName)
    {
      foreach (KeyValuePair<string, string> path in Paths)
      {
        if (!string.IsNullOrEmpty(path.Key) && !string.IsNullOrEmpty(path.Value))
        {
          fileName = fileName.Replace(path.Key, path.Value);
        }
      }
      return fileName;
    }

    public string Expand(string filenameTemplate)
    {
      foreach (KeyValuePair<string, string> path in Paths)
      {
        if (!string.IsNullOrEmpty(path.Key) && !string.IsNullOrEmpty(path.Value))
        {
          filenameTemplate = filenameTemplate.Replace(path.Key, path.Value);
        }
      }
      return filenameTemplate;
    }

    #endregion
  }
}