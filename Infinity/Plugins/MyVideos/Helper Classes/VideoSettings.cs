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
    bool _createThumbnails;
    string _seekSteps;
    string _currentFolder;
    int _currentViewType;
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

    [Setting(SettingScope.User, "True")]
    public bool AutoCreateThumbnails
    {
      get { return this._createThumbnails; }
      set { this._createThumbnails = value; }
    }

    /// <summary>
    /// Gets or sets the  steps used for seeking forward/backward
    /// steps are in seconds
    /// </summary>
    /// <value>The seek steps.</value>
    [Setting(SettingScope.Global, "0,15,30,60,180,300,600,900,1800,3600,7200")]
    public string SeekSteps
    {
      get
      {
        return _seekSteps;
      }
      set
      {
        _seekSteps = value;
      }
    }
    [Setting(SettingScope.User, "")]
    public string CurrentFolder
    {
      get
      {
        return _currentFolder;
      }
      set
      {
        _currentFolder = value;
      }
    }
    [Setting(SettingScope.User, "1")]
    public int ViewMode
    {
      get
      {
        return _currentViewType;
      }
      set
      {
        _currentViewType = value;
      }
    }
  }
}
