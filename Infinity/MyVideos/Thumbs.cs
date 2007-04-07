using System;
using System.Collections.Generic;
using System.Text;

namespace MyVideos
{
    public static class Thumbs
    {
        static public readonly string MyVideoIconPath = @"pack://siteoforigin:,,,/media\images\defaultVideo.png";

        public static string GetFullPath(string path)
        {
            Uri _uri = new Uri(path, UriKind.Relative);

            return _uri.LocalPath;
        }
    }
}
