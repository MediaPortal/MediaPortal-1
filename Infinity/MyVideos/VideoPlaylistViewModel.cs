using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;
using ProjectInfinity;
using ProjectInfinity.Localisation;

namespace MyVideos
{
  public class VideoPlaylistViewModel
  {
    #region variables
    VideoPlaylistCollectionView _playlistView;
    VideoPlaylistDatabaseModel _dataModel;

    Window _window;
    Page _page;

    VideoHomeViewModel.ViewType _viewType = VideoHomeViewModel.ViewType.List;
    #endregion

    #region ctor
    public VideoPlaylistViewModel(Page page)
    {
      _dataModel = new VideoPlaylistDatabaseModel();
      _playlistView = new VideoPlaylistCollectionView(_dataModel);

      _page = page;
      _window = Window.GetWindow(page);
    }
    #endregion

    #region ILocalisation members
    public string PartymixLabel
    {
      get { return ServiceScope.Get<ILocalisation>().ToString("myvideos", 31); } // Partymix
    }

    public string NextLabel
    {
      get { return ServiceScope.Get<ILocalisation>().ToString("myvideos", 32); } // Next track
    }

    public string PrevLabel
    {
      get { return ServiceScope.Get<ILocalisation>().ToString("myvideos", 33); } // Previous track
    }

    public string SortLabel
    {
      get
      {
        switch (_playlistView.SortMode)
        {
          case VideoHomeViewModel.SortType.Name:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 5); // Sort by: Name
          case VideoHomeViewModel.SortType.Date:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 6); // Sort by: Date
          case VideoHomeViewModel.SortType.Size:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 7); // Sort by: Size
          case VideoHomeViewModel.SortType.Year:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 8); // Sort by: Year
          case VideoHomeViewModel.SortType.Rating:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 9); // Sort by: Rating
          case VideoHomeViewModel.SortType.DVD:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 10); // Sort by: DVD
          default:
            return ServiceScope.Get<ILocalisation>().ToString("myvideos", 1); // Sort
        }
      }
    }
    #endregion

    #region properties
    public string DateLabel
    {
      get { return DateTime.Now.ToString("dd-MM HH:mm"); }
    }

    public DataTemplate ItemTemplate
    {
      get
      {
        switch (_viewType)
        {
          case VideoHomeViewModel.ViewType.List:
            return (DataTemplate)_page.Resources["videoItemListTemplate"];
          default:
            return (DataTemplate)_page.Resources["videoItemListTemplate"];
        }
      }
    }

    public CollectionView VideoPlaylist
    {
      get
      {
        if (_playlistView == null)
          _playlistView = new VideoPlaylistCollectionView(_dataModel);

        return _playlistView;
      }
    }
    #endregion
  }
}
