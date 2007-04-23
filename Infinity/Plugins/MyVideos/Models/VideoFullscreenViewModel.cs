using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MyVideos
{
  public class VideoFullscreenViewModel : VideoHomeViewModel
  {

    private VideoDatabaseModel _dataModel;

    public VideoFullscreenViewModel()
    {
      _dataModel = new VideoDatabaseModel();

    }
  }
}
