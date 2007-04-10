using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;
using ProjectInfinity.Navigation;

namespace ProjectInfinity.Pictures
{
  public class PictureViewModel : INotifyPropertyChanged
  {
    ///<summary>
    ///Occurs when a property value changes.
    ///</summary>
    public event PropertyChangedEventHandler PropertyChanged;

    private List<MediaItem> _model = new List<MediaItem>();
    private LaunchCommand _launchCommand;
    private CollectionView _itemView;
    private Folder _currentFolder;

    public PictureViewModel()
    {
      //TODO: read starting folder from configuration
      Reload(new Folder(new DirectoryInfo(@"c:\")), false);
    }

    public Folder CurrentFolder { get { return _currentFolder;}}

    private void Reload(Folder dir, bool includeParent)
    {
      DirectoryInfo directoryInfo = dir.Info;
      DirectoryInfo parentInfo = directoryInfo.Parent;
      FileSystemInfo[] entries = directoryInfo.GetFileSystemInfos();
      MediaFactory factory = new MediaFactory();
      _currentFolder = dir;
      OnPropertyChanged(new PropertyChangedEventArgs("CurrentFolder"));
      _model.Clear();
      if (includeParent && parentInfo != null)
        _model.Add(new ParentFolder(parentInfo));
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

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, e);
      }
    }

    private class LaunchCommand : ICommand, IMediaVisitor
    {
      private PictureViewModel _viewModel;

      public LaunchCommand(PictureViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      ///<summary>
      ///Occurs when changes occur which affect whether or not the command should execute.
      ///</summary>
      public event EventHandler CanExecuteChanged;

      ///<summary>
      ///Defines the method to be called when the command is invoked.
      ///</summary>
      ///
      ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public void Execute(object parameter)
      {
        MediaItem item = _viewModel.Items.CurrentItem as MediaItem;
        if (item == null)
        {
          return;
        }
        item.Accept(this); //GOF Visitor Pattern
      }

      ///<summary>
      ///Defines the method that determines whether the command can execute in its current state.
      ///</summary>
      ///
      ///<returns>
      ///true if this command can be executed; otherwise, false.
      ///</returns>
      ///
      ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Visit(Folder folder)
      {
        _viewModel.Reload(folder,true);
      }

      public void Visit(Picture picture)
      {
        ServiceScope.Get<INavigationService>().Navigate(new Uri("/ProjectInfinity.Plugins;component/Pictures/FullScreenPictureView.xaml", UriKind.Relative));
      }
    }
  }

  internal class MediaFactory
  {
    private static readonly Collection<string> pictureExtensions;

    static MediaFactory()
    {
      //TODO: read from configuration
      pictureExtensions = new Collection<string>(new string[] {".jpg", ".gif"});
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
      if (pictureExtensions.Contains(fileInfo.Extension.ToLower()))
      {
        return new Picture(fileInfo);
      }
      return null;
    }

    private MediaItem Create(DirectoryInfo directoryInfo)
    {
      return new Folder(directoryInfo);
    }
  }
}