using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Thumbnails;
using ProjectInfinity.Settings;
using ProjectInfinity.Playlist;
using System.ComponentModel;

namespace MyVideos
{
  public class VideoModel : IPlaylistItem, INotifyPropertyChanged
  {
    #region variables
    bool _isFolder;
    private string _name;
    private double _size;
    private string _path;
    private string _parentFolder;
    private string _logo = null;
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region ctors
    public VideoModel()
    {
      Init();
    }

    /// <summary>
    /// Sets the name of the movie object
    /// </summary>
    /// <param name="movieName">video name</param>
    public VideoModel(string movieName)
    {
      _name = movieName;
      Init();
    }

    /// <summary>
    /// Sets the name and size of the movie object
    /// </summary>
    /// <param name="movieName">video name</param>
    /// <param name="movieSize">video size</param>
    public VideoModel(string movieName, int movieSize)
    {
      _name = movieName;
      //_size = Math.Round((double)(((movieSize / 1024) / 1024)), 2);

      int tmpSize = (movieSize / 1024) / 1024;
      _size = Math.Round((double)tmpSize, 2);
      Init();
    }
    void Init()
    {
      ServiceScope.Get<IThumbnailBuilder>().OnThumbnailGenerated += new ThumbNailGenerateHandler(VideoModel_OnThumbnailGenerated);
    }

    /// <summary>
    /// Sets the name, size and path of the movie object
    /// </summary>
    /// <param name="movieName">video name</param>
    /// <param name="movieSize">video size in bytes</param>
    /// <param name="moviePath">video path</param>
    public VideoModel(string movieName, int movieSize, string moviePath)
    {
      _name = movieName;
      //_size = Math.Round(((movieSize / 1024) / 1024), 2);
      int tmpSize = (movieSize / 1024) / 1024;
      _size = Math.Round((double)tmpSize, 2);

      _path = moviePath;
      ServiceScope.Get<IThumbnailBuilder>().OnThumbnailGenerated += new ThumbNailGenerateHandler(VideoModel_OnThumbnailGenerated);

    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the movie name
    /// </summary>
    public string Title
    {
      get { return _name; }
    }

    /// <summary>
    /// Gets the path to the movie
    /// </summary>
    public string Path
    {
      get { return _path; }
      set { _path = value; }
    }

    /// <summary>
    /// Gets the movie size
    /// </summary>
    /// <value></value>
    public string Size
    {
      get
      {
        if (IsFolder) return "";
        return _size.ToString() + " MB";
      }
    }

    /// <summary>
    /// Gets the movie size (mb) in double
    /// </summary>
    /// <value></value>
    public double RealSize
    {
      get { return _size; }
    }
    /// <summary>
    /// Gets or sets a value indicating whether this instance is folder.
    /// </summary>
    /// <value><c>true</c> if this instance is folder; otherwise, <c>false</c>.</value>
    public bool IsFolder
    {
      get { return _isFolder; }
      set { _isFolder = value; }
    }
    /// <summary>
    /// If you want a custom icon beside the Title, set this to point
    /// to the file (absolute/relative)
    /// </summary>
    /// <value></value>
    public string Logo
    {
      get
      {
        if (IsFolder && Title == "..")
          _logo = Thumbs.ParentFolder;
        else if (IsFolder)
        {
          if (Thumbs.ExistsFolder(Path))
            return Thumbs.GetFolder(Path);
          _logo = Thumbs.Folder;
        }
        else
        {
          if (!Thumbs.Exists(Path))
          {
            _logo = Thumbs.MyVideoIconPath;
            VideoSettings settings = new VideoSettings();
            ServiceScope.Get<ISettingsManager>().Load(settings);
            if (settings.AutoCreateThumbnails)
            {
              ServiceScope.Get<IThumbnailBuilder>().Generate(Path);
            }
          }
          else
          {
            _logo = Thumbs.Get(Path);
          }
        }
        return _logo;
      }
    }

    void VideoModel_OnThumbnailGenerated(object sender, ThumbnailEventArgs e)
    {
      if (e.Succeeded)
      {
        if (e.MediaFile == Path)
        {
          if (PropertyChanged != null)
          {
            PropertyChanged(this, new PropertyChangedEventArgs("Logo"));
          }
        }
      }
    }
    #endregion

    #region playlist properties
    public string UpArrow
    {
      get { return Thumbs.UpArrowIconPath; }
    }

    public string DownArrow
    {
      get { return Thumbs.DownArrowIconPath; }
    }
    #endregion
  }
}
