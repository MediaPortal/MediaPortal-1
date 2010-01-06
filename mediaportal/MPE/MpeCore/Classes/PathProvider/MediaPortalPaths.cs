using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Interfaces;
using MediaPortal.Configuration;

namespace MpeCore.Classes.PathProvider
{
  internal class MediaPortalPaths : IPathProvider
  {
    private Dictionary<string, string> _paths;

    public MediaPortalPaths()
    {
      _paths = new Dictionary<string, string>();
      foreach (string options in Enum.GetNames(typeof (Config.Dir)))
      {
        _paths.Add(string.Format("%{0}%", options),
                   Config.GetFolder((Config.Dir)Enum.Parse(typeof (Config.Dir), options)));
      }
    }


    public string Name
    {
      get { return "MediaPortalPaths"; }
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
          fileName = fileName.Replace(path.Value, path.Key);
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
  }
}