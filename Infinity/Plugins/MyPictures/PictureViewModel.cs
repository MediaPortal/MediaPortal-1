using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Dialogs;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Settings;

namespace ProjectInfinity.Pictures
{
  public class PictureViewModel : INotifyPropertyChanged
  {
    private readonly PictureSettings settings;
    private readonly List<MediaItem> _model = new List<MediaItem>();
    private LaunchCommand _launchCommand;
    private CollectionView _itemView;
    private Folder _currentFolder;
    private ViewMode _viewMode;
    private ICommand _viewCommand;
    private ICommand _startSlideshowCommand;

    public PictureViewModel()
    {
      //if (Core.IsDesignMode)
      //{
      //  settings = new PictureSettings();
      //  _model.Clear();
      //  _model.Add(new Folder(@"c:\temp\Beach",5));
      //  _model.Add(new );
      //  Reload(new Folder(new DirectoryInfo(@"skin\default\gfx")),false);
      //  return;
      //}
      ISettingsManager settingMgr = ServiceScope.Get<ISettingsManager>();
      settings = new PictureSettings();
      settingMgr.Load(settings);
      //we save the settings here, to make sure they are in the configuration file.
      //because this plugin has no setup yet
      //TODO: remove saving settings here
      settingMgr.Save(settings);
      Reload(new Folder(new DirectoryInfo(settings.PictureFolders[0])), false);
    }

    public Folder CurrentFolder
    {
      get { return _currentFolder; }
    }

    public CollectionView Items
    {
      get { return _itemView; }
      private set
      {
        _itemView = value;
        OnPropertyChanged(new PropertyChangedEventArgs("Items"));
      }
    }

    public ICommand Launch
    {
      get
      {
        if (_launchCommand == null)
        {
          _launchCommand = new LaunchCommand(this);
        }
        return _launchCommand;
      }
    }

    public ICommand View
    {
      get
      {
        if (_viewCommand == null)
        {
          _viewCommand = new ViewCommand(this);
        }
        return _viewCommand;
      }
    }

    public ViewMode ViewMode
    {
      get { return _viewMode; }
      set
      {
        if (_viewMode == value)
        {
          return;
        }
        _viewMode = value;
        OnPropertyChanged(new PropertyChangedEventArgs("ViewMode"));
        OnPropertyChanged(new PropertyChangedEventArgs("ViewLabel"));
      }
    }

    public string ViewLabel
    {
      get { return "View: " + ViewMode; }
    }

    #region INotifyPropertyChanged Members

    ///<summary>
    ///Occurs when a property value changes.
    ///</summary>
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    private void Reload(Folder dir, bool includeParent)
    {
      DirectoryInfo directoryInfo = new DirectoryInfo(dir.Path);
      DirectoryInfo parentInfo = directoryInfo.Parent;
      FileSystemInfo[] entries = directoryInfo.GetFileSystemInfos();
      MediaFactory factory = new MediaFactory(settings);
      _currentFolder = dir;
      OnPropertyChanged(new PropertyChangedEventArgs("CurrentFolder"));
      _model.Clear();
      if (includeParent && parentInfo != null)
      {
        _model.Add(new ParentFolder(parentInfo));
      }
      foreach (FileSystemInfo entry in entries)
      {
        MediaItem item = factory.Create(entry);
        if (item == null)
        {
          continue;
        }
        _model.Add(item);
      }
      Items = new CollectionView(_model);
    }


    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, e);
      }
    }

    private class LaunchCommand : BaseCommand, IMediaVisitor
    {
      public LaunchCommand(PictureViewModel model) : base(model)
      {
      }

      #region IMediaVisitor Members

      public void Visit(Folder folder)
      {
        _viewModel.Reload(folder, true);
      }

      public void Visit(Picture picture)
      {
        ServiceScope.Get<INavigationService>().Navigate(new FullScreenPictureView(_viewModel));
      }

      #endregion

      ///<summary>
      ///Defines the method to be called when the command is invoked.
      ///</summary>
      ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public override void Execute(object parameter)
      {
        MediaItem item = _viewModel.Items.CurrentItem as MediaItem;
        if (item == null)
        {
          return;
        }
        item.Accept(this); //GOF Visitor Pattern
      }
    }

    private class ViewCommand : BaseCommand
    {
      public ViewCommand(PictureViewModel model) : base(model)
      {
      }


      public override void Execute(object parameter)
      {
        // Show a dialog with all sorting options
        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = ServiceScope.Get<INavigationService>().GetWindow();
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mypictures", 11); // "Menu";
        dlgMenu.SubTitle = "";
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mypictures", 22) /*List*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mypictures", 23) /*Icon*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mypictures", 24) /*Details*/));
        dlgMenu.SelectedIndex = (int) _viewModel.ViewMode;
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0)
        {
          return;
        }
        _viewModel.ViewMode = (ViewMode) dlgMenu.SelectedIndex;
      }
    }
  }

  internal class MediaFactory
  {
    private readonly PictureSettings _settings;

    public MediaFactory(PictureSettings settings)
    {
      _settings = settings;
    }

    public MediaItem Create(FileSystemInfo fileSystemInfo)
    {
      if (!fileSystemInfo.Exists)
      {
        return null;
      }
      FileInfo file = fileSystemInfo as FileInfo;
      if (file != null)
      {
        return Create(file);
      }
      DirectoryInfo directory = fileSystemInfo as DirectoryInfo;
      if (directory != null)
      {
        return Create(directory);
      }
      return null;
    }

    private MediaItem Create(FileInfo fileInfo)
    {
      if (_settings.Extensions.Contains(fileInfo.Extension.ToLower()))
      {
        return new Picture(fileInfo);
      }
      return null;
    }

    private static MediaItem Create(DirectoryInfo directoryInfo)
    {
      return new Folder(directoryInfo);
    }
  }
}