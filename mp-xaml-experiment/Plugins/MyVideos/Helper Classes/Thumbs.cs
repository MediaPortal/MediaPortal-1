using System;
using System.Collections.Generic;
using System.Text;

namespace MyVideos
{
  public static class Thumbs
  {
    static public readonly string ParentFolder = @"pack://siteoforigin:,,,/skin\default\gfx\DefaultFolderBack.png";
    static public readonly string Folder = @"pack://siteoforigin:,,,/skin\default\gfx\DefaultFolder.png";
    static public readonly string MyVideoIconPath = @"pack://siteoforigin:,,,/skin\default\gfx\defaultVideo.png";
    static public readonly string UpArrowIconPath = @"pack://siteoforigin:,,,/skin\default\gfx\arrow_round_up_nofocus.png";
    static public readonly string DownArrowIconPath = @"pack://siteoforigin:,,,/skin\default\gfx\arrow_round_down_nofocus.png";

    public static string GetFullPath(string path)
    {
      Uri _uri = new Uri(path, UriKind.Relative);

      return _uri.LocalPath;
    }

    public static string Get(string mediaFile)
    {
      return System.IO.Path.ChangeExtension(mediaFile, ".png");
    }
    public static bool Exists(string mediaFile)
    {
      return (System.IO.File.Exists(Get(mediaFile)));
    }

    public static string GetFolder(string folder)
    {
      if (folder.EndsWith(@"\") )
        return folder + @"folder.jpg";
      else
        return folder + @"\folder.jpg";
    }

    public static bool ExistsFolder(string folder)
    {
      return (System.IO.File.Exists(GetFolder(folder)));
    }
  }
}
