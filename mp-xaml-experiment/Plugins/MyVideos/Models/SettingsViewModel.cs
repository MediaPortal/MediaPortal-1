//
// TODO: - Add support for different languages (databindings)
//
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using ProjectInfinity;
using ProjectInfinity.Localisation;
using System.IO;
using System.Collections;
using System.Windows.Data;
using System.Windows.Threading;
using Dialogs;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using System.Windows.Media;
using System.Windows.Navigation;
using ProjectInfinity.Playlist;
using ProjectInfinity.Settings;
using ProjectInfinity.Navigation;
using Dialogs;
using MediaLibrary;

namespace MyVideos
{
  public class SettingsViewModel : VideoHomeViewModel
  {
    #region ctor
    public SettingsViewModel()
    {
    }
    #endregion



    public string HeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("settings", 0);//settings
      }
    }

    public string SharesLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("settings", 0);//settings
      }
    }
    public string FoldersLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("settings", 1);//folders
      }
    }
    public string ExtensionsLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("settings", 2);//extensions
      }
    }
    public string ThumbnailsLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("settings", 3);//thumbnails
      }
    }

    public ICommand Shares
    {
      get
      {
        return new SharesCommand();
      }
    }
    public ICommand Extensions
    {
      get
      {
        return new ExtensionsCommand();
      }
    }
    public ICommand Thumbnails
    {
      get
      {
        return new ThumbnailsCommand();
      }
    }

    public class SharesCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;
      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {

        VideoSettings settings = new VideoSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings);
        FolderDialog dlg = new FolderDialog();
        Window w = ServiceScope.Get<INavigationService>().GetWindow();
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "";
        dlg.Header = ServiceScope.Get<ILocalisation>().ToString("myvideos", 35);//Video folders
        dlg.ShowDialog();
        List<Folder> shares = dlg.SelectedFolders;

        foreach (Folder share in shares)
        {
          settings.Shares.Add(share.FullPath);
        }
        ServiceScope.Get<ISettingsManager>().Save(settings);
      }

    }

    public class ExtensionsCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;
      public bool CanExecute(object parameter)
      {
        return true;
      }


      public void Execute(object parameter)
      {
        VideoSettings settings = new VideoSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings);

        DialogTextInput dlg = new DialogTextInput();
        Window w = ServiceScope.Get<INavigationService>().GetWindow();
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "";
        dlg.Header = ServiceScope.Get<ILocalisation>().ToString("settings", 2);//extensions
        dlg.InputText = settings.VideoExtensions;
        dlg.ShowDialog();
        settings.VideoExtensions = dlg.InputText;
        ServiceScope.Get<ISettingsManager>().Save(settings);
      }
    }

    public class ThumbnailsCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;
      public bool CanExecute(object parameter)
      {
        return true;
      }


      public void Execute(object parameter)
      {
        VideoSettings settings = new VideoSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings);
        MpMenu dlg = new MpMenu();
        Window w = ServiceScope.Get<INavigationService>().GetWindow();
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "";
        dlg.Header = ServiceScope.Get<ILocalisation>().ToString("settings", 3);//thumbnails
        dlg.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 36)));//Create thumbnails
        dlg.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 37)));//Dont create thumbnails
        if (settings.AutoCreateThumbnails)
          dlg.SelectedIndex = 0;
        else
          dlg.SelectedIndex = 1;
        dlg.ShowDialog();
        if (dlg.SelectedIndex < 0) return;
        settings.AutoCreateThumbnails = (dlg.SelectedIndex == 0);
        ServiceScope.Get<ISettingsManager>().Save(settings);
      }
    }
  }
}
