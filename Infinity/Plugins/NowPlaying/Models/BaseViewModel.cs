using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using System.Windows.Media;
using System.Windows.Navigation;
using ProjectInfinity;
using ProjectInfinity.Playlist;
using ProjectInfinity.Settings;
using ProjectInfinity.Navigation;
using ProjectInfinity.Localisation;
using ProjectInfinity.Messaging;
using ProjectInfinity.Messaging.Files;
using Dialogs;
using MediaLibrary;

namespace NowPlaying
{
  public class BaseViewModel
  {

    public BaseViewModel()
    {
    }

    public void OnPlaybackStopped()
    {
      IPlayer player = ServiceScope.Get<IPlayerCollectionService>()[0];

      if (player.MediaType == PlayerMediaType.DVD || player.MediaType == PlayerMediaType.Movie)
      {
        IMediaLibrary library = ServiceScope.Get<IMediaLibrary>();
        IMLSection section = library.FindSection("Videos", false);
        if (section == null) return;
        IMLItem item = section.FindItemByLocation(player.FileName);
        if (item == null) return;

        item.Tags["ResumeTime"] = player.Position.TotalSeconds;
        item.SaveTags();
      }
    }

    public void OnPlaybackStarted()
    {
      IPlayer player = ServiceScope.Get<IPlayerCollectionService>()[0];
      if (player.MediaType == PlayerMediaType.DVD || player.MediaType == PlayerMediaType.Movie)
      {
        IMediaLibrary library = ServiceScope.Get<IMediaLibrary>();
        IMLSection section = library.FindSection("Videos", true);
        IMLItem item1 = section.FindItemByLocation(player.FileName);
        if (item1 != null) return;

        IMLItem newItem = section.AddNewItem(player.FileName, player.FileName);
        newItem.Tags["Watched"] = 0;
        newItem.Tags["ResumeTime"] = "";
        newItem.Tags["Duration"] = player.Duration.TotalSeconds;
        newItem.Tags["Width"] = player.Width;
        newItem.Tags["Height"] = player.Height;
        newItem.SaveTags();
      }
    }

    public void OnPlaybackEnded()
    {
      IPlayer player = ServiceScope.Get<IPlayerCollectionService>()[0];

      if (player.MediaType == PlayerMediaType.DVD || player.MediaType == PlayerMediaType.Movie)
      {
        IMediaLibrary library = ServiceScope.Get<IMediaLibrary>();
        IMLSection section = library.FindSection("Videos", false);
        if (section == null) return;
        IMLItem item = section.FindItemByLocation(player.FileName);
        if (item == null) return;

        int watched = Int32.Parse((string)item.Tags["Watched"]);
        watched++;
        item.Tags["Watched"] = watched;
        item.Tags["ResumeTime"] = "";
        item.SaveTags();
      }
    }
    #region properties
    public string NowPlayingLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("nowplaying", 0);// "Now Playing";
      }
    }

    public string DateLabel
    {
      get
      {
        return DateTime.Now.ToString("dd-MM HH:mm");
      }
    }
    public string DoneLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("global", 6);// "Done";
      }
    }
    public string CompletedLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("global", 7);// "Finished";
      }
    }


    public string RestartLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("global", 3);// "Restart";
      }
    }

    public string DeleteLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("global", 4);// "Delete";
      }
    }
    public string FileName
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          string fileName = ServiceScope.Get<IPlayerCollectionService>()[0].FileName;
          int pos = fileName.LastIndexOf(@"\");
          if (pos >= 0) fileName = fileName.Substring(pos + 1);
          return fileName;
        }
        return "";
      }
    }
    public Brush VideoBrush
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          MediaPlayer player = (MediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0].UnderlyingPlayer;

          VideoDrawing videoDrawing = new VideoDrawing();
          videoDrawing.Player = player;
          videoDrawing.Rect = new Rect(0, 0, 800, 600);
          DrawingBrush videoBrush = new DrawingBrush();
          videoBrush.Stretch = Stretch.Fill;
          videoBrush.Drawing = videoDrawing;
          return videoBrush;
        }

        return null;
      }
    }

    public Brush VideoOpacityMask
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          return Application.Current.Resources["VideoOpacityMask"] as Brush;
        }
        return null;
      }
    }

    public Visibility IsVideoPresent
    {
      get { return (ServiceScope.Get<IPlayerCollectionService>().Count != 0) ? Visibility.Visible : Visibility.Collapsed; }
    }

    public ICommand Done
    {
      get
      {
        return new DoneCommand();
      }
    }

    public ICommand Restart
    {
      get
      {
        return new RestartCommand();
      }
    }

    public ICommand Delete
    {
      get
      {
        return new DeleteCommand();
      }
    }
    #endregion

    #region commands
    public class DoneCommand : ICommand
    {
      #region ICommand Members

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public event EventHandler CanExecuteChanged;

      public void Execute(object parameter)
      {
        ServiceScope.Get<IPlayerCollectionService>().Clear();
        ServiceScope.Get<INavigationService>().GoBack();
      }

      #endregion
    }
    public class RestartCommand : ICommand
    {
      #region ICommand Members

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public event EventHandler CanExecuteChanged;

      public void Execute(object parameter)
      {
        IPlayer player = ServiceScope.Get<IPlayerCollectionService>()[0];
        player.Position = new TimeSpan(0, 0, 0, 0, 0);
        player.Play();
        ServiceScope.Get<INavigationService>().GoBack();
      }

      #endregion
    }
    public class DeleteCommand : ICommand
    {

      [MessagePublication(typeof(FileDeleteMessage))]
      public event MessageHandler<FileDeleteMessage> OnFileDelete;
      public bool CanExecute(object parameter)
      {
        return true;
      }

      public event EventHandler CanExecuteChanged;

      public void Execute(object parameter)
      {
        MpDialogYesNo dlgMenu = new MpDialogYesNo();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = ServiceScope.Get<INavigationService>().GetWindow();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
        dlgMenu.Content = ServiceScope.Get<ILocalisation>().ToString("global", 8);//"Are you sure to delete this file?";
        dlgMenu.ShowDialog();
        if (dlgMenu.DialogResult == DialogResult.No) return;
        IPlayer player = ServiceScope.Get<IPlayerCollectionService>()[0];
        string fileName = player.FileName;
        ServiceScope.Get<IPlayerCollectionService>().Clear();
        try
        {
          System.IO.File.Delete(fileName);
        }
        catch (Exception)
        {
        }
        if (player.MediaType == PlayerMediaType.DVD || player.MediaType == PlayerMediaType.Movie)
        {
          IMediaLibrary library = ServiceScope.Get<IMediaLibrary>();
          IMLSection section = library.FindSection("Videos", false);
          if (section != null)
          {
            IMLItem item1 = section.FindItemByLocation(player.FileName);
            if (item1 != null)
            {
              section.DeleteItem(item1);
            }
          }
        }

        ServiceScope.Get<IMessageBroker>().Register(this);
        if (OnFileDelete != null)
        {
          OnFileDelete(new FileDeleteMessage(fileName));
        }
        ServiceScope.Get<IMessageBroker>().Unregister(this);
        ServiceScope.Get<INavigationService>().GoBack();
      }
    }
    #endregion

  }
}
