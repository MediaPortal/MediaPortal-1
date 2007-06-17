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
using ProjectInfinity.Messaging;
using MediaLibrary;

namespace ProjectInfinity.Pictures
{
  public class SettingsViewModel : DispatcherObject, INotifyPropertyChanged, IDisposable
  {
    public event PropertyChangedEventHandler PropertyChanged;    
    
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
   
    #region IDisposable Members

    public void Dispose()
    {
      ServiceScope.Get<IMessageBroker>().Unregister(this);
    }

    #endregion
    
    public class SharesCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;
      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
        PictureSettings settings = new PictureSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings);
        FolderDialog dlg = new FolderDialog();
        Window w = ServiceScope.Get<INavigationService>().GetWindow();

        foreach (string folder in settings.PictureFolders)
        {
          dlg.SelectedFolders.Add(new Dialogs.Folder(folder));
        }
        
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "";
        dlg.Header = ServiceScope.Get<ILocalisation>().ToString("mypictures", 35);//Pictures folders
        dlg.ShowDialog();
        List<Dialogs.Folder> shares = dlg.SelectedFolders;
        settings.PictureFolders.Clear();
        foreach (Dialogs.Folder share in shares)
        {
          settings.PictureFolders.Add(share.FullPath);
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
        PictureSettings settings = new PictureSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings);

        DialogTextInput dlg = new DialogTextInput();
        Window w = ServiceScope.Get<INavigationService>().GetWindow();
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "";
        dlg.Header = ServiceScope.Get<ILocalisation>().ToString("settings", 2);//extensions
        dlg.InputText = settings.Extensions;
        dlg.ShowDialog();
        settings.Extensions = dlg.InputText;
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
        PictureSettings settings = new PictureSettings();
        ServiceScope.Get<ISettingsManager>().Load(settings);
        MpMenu dlg = new MpMenu();
        Window w = ServiceScope.Get<INavigationService>().GetWindow();
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "";
        dlg.Header = ServiceScope.Get<ILocalisation>().ToString("settings", 3);//thumbnails
        dlg.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mypictures", 36)));//Create thumbnails
        dlg.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mypictures", 37)));//Dont create thumbnails
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
