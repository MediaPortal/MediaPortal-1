using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Collections;
using ProjectInfinity;
using ProjectInfinity.Playlist;

namespace MyVideos
{
  public class VideoPlaylistCollectionView : ListCollectionView
  {
    #region variables
    private VideoHomeViewModel.SortType _sortMode = VideoHomeViewModel.SortType.Date;
    private VideoPlaylistDatabaseModel _model;
    #endregion

    #region ctor
    public VideoPlaylistCollectionView(VideoPlaylistDatabaseModel model)
      : base(model.VideoPlaylist)
    {
      _model = model;
    }
    #endregion

    #region properties
    public VideoHomeViewModel.SortType SortMode
    {
      get { return _sortMode; }
      set
      {
        if (_sortMode != value)
        {
          _sortMode = value;
          this.CustomSort = new VideoComparer(_sortMode);
        }
      }
    }
    #endregion
  }

  public class VideoPlaylistDatabaseModel
  {
   /* #region variables
    private List<VideoModel> _items = new List<VideoModel>();
    #endregion

    #region ctor
    public VideoPlaylistDatabaseModel()
    {
    }
    #endregion

    #region public methods
    /// <summary>
    /// Add a <see cref="VideoModel" /> to the collection.
    /// </summary>
    /// <param name="model"></param>
    public void Add(VideoModel model)
    {
      _items.Add(model);
    }

    /// <summary>
    /// Remove a <see cref="VideoModel" /> from the collection.
    /// </summary>
    /// <param name="model"></param>
    public void Remove(VideoModel model)
    {
      _items.Remove(model);
    }

    /// <summary>
    /// Remove a <see cref="VideoModel" /> from the collection by defining a index.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
      _items.RemoveAt(index);
    }
    #endregion*/

    #region properties
    public IList VideoPlaylist
    {
      get { return ServiceScope.Get<IPlaylistManager>().PlaylistItems; }
    }
    #endregion
  }
}
