using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Playlist;

namespace MyVideos
{
  public class VideoModel : IPlaylistItem
  {
    #region variables
    private string _name;
    private double _size;
    private string _path;
    private string _logo = null;
    #endregion

    #region ctors
    public VideoModel()
    {
    }

    /// <summary>
    /// Sets the name of the movie object
    /// </summary>
    /// <param name="movieName">video name</param>
    public VideoModel(string movieName)
    {
      _name = movieName;
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
    }

    /// <summary>
    /// Gets the movie size
    /// </summary>
    public string Size
    {
      get { return _size.ToString() + " MB"; }
    }

    /// <summary>
    /// Gets the movie size (mb) in double
    /// </summary>
    public double RealSize
    {
      get { return _size; }
    }

    public string Logo
    {
      get
      {
        if (_logo == null)
          _logo = Thumbs.MyVideoIconPath;

        return _logo;
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
