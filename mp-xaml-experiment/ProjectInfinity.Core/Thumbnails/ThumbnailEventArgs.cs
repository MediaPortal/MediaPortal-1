using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ProjectInfinity.Thumbnails
{

  public class ThumbnailEventArgs:EventArgs
  {
    #region variables
    string _mediaFile;
    string _thumbNail;
    bool _succeeded;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailEventArgs"/> class.
    /// </summary>
    /// <param name="mediaFile">The media file.</param>
    /// <param name="thumbNail">The thumb nail.</param>
    /// <param name="succeeded">if set to <c>true</c> [succeeded].</param>
    public ThumbnailEventArgs(string mediaFile, string thumbNail, bool succeeded)
    {
      _mediaFile = mediaFile;
      _thumbNail = thumbNail;
      _succeeded = succeeded;
    }
    /// <summary>
    /// Gets or sets the media file.
    /// </summary>
    /// <value>The media file.</value>
    public string MediaFile
    {
      get
      {
        return _mediaFile;
      }
      set
      {
        _mediaFile = value;
      }
    }
    /// <summary>
    /// Gets or sets the thumb nail.
    /// </summary>
    /// <value>The thumb nail.</value>
    public string ThumbNail
    {
      get
      {
        return _thumbNail;
      }
      set
      {
        _thumbNail = value;
      }
    }
    /// <summary>
    /// Gets or sets the succeeded.
    /// </summary>
    /// <value>The succeeded.</value>
    public bool Succeeded
    {
      get
      {
        return _succeeded;
      }
      set
      {
        _succeeded = value;
      }
    }
  }
}