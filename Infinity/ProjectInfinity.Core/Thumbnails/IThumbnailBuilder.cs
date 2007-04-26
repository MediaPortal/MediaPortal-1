using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ProjectInfinity.Thumbnails
{

  public delegate void ThumbNailGenerateHandler(object sender, ThumbnailEventArgs e);
  public interface IThumbnailBuilder
  {
    event ThumbNailGenerateHandler OnThumbnailGenerated;
    void Generate(string mediaFile);
    void Generate(List<string> mediaFiles);
  }
}