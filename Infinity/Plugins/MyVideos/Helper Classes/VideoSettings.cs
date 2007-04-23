using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Settings;
namespace MyVideos
{
  public class VideoSettings
  {
    List<string> _shares;
    string _videoExtensions;

    public VideoSettings()
    {
    }
    [Setting(SettingScope.User, "null")]
    public List<string> Shares
    {
      get { return this._shares; }
      set { this._shares = value; }
    }

    [Setting(SettingScope.User, ".mpeg;.mpg;.avi;.mkv;.ts;.wmv")]
    public string VideoExtensions
    {
      get { return this._videoExtensions; }
      set { this._videoExtensions = value; }
    }
  }
}
