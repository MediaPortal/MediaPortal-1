using System;
using System.Collections.Generic;
using System.Text;

namespace MyVideos
{
  public static class Thumbs
  {
    static public readonly string MyVideoIconPath = @"pack://siteoforigin:,,,/media\images\defaultVideo.png";
    static public readonly string UpArrowIconPath = @"pack://siteoforigin:,,,/media\images\arrow_round_up_nofocus.png";
    static public readonly string DownArrowIconPath = @"pack://siteoforigin:,,,/media\images\arrow_round_down_nofocus.png";

    public static string GetFullPath(string path)
    {
      Uri _uri = new Uri(path, UriKind.Relative);

      return _uri.LocalPath;
    }
  }
}
