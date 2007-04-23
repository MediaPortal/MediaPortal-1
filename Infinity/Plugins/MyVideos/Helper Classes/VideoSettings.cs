using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Settings;
namespace MyVideos
{
  public class VideoSettings
  {
    #region ctor
    bool _showExtensions;
    List<string> _shares;
    string _videoExtensions;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoSettings"/> class.
    /// </summary>
    public VideoSettings()
    {
    }
    /// <summary>
    /// Gets or sets the video folder shares.
    /// </summary>
    /// <value>The shares.</value>
    [Setting(SettingScope.User, "")]
    public List<string> Shares
    {
      get { return this._shares; }
      set { this._shares = value; }
    }

    /// <summary>
    /// Gets or sets the video extensions.
    /// </summary>
    /// <value>The video extensions.</value>
    [Setting(SettingScope.User, ".mpeg;.mpg;.avi;.mkv;.ts;.wmv")]
    public string VideoExtensions
    {
      get { return this._videoExtensions; }
      set { this._videoExtensions = value; }
    }

    [Setting(SettingScope.User, "false")]
    public bool ShowExtensions
    {
      get { return this._showExtensions; }
      set { this._showExtensions = value; }
    }
  }
}
