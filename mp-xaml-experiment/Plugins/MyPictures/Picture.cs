using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ProjectInfinity.Pictures
{
  public class Picture : MediaItem, INotifyPropertyChanged
  {
    private readonly FileInfo _info;
    private readonly Dispatcher _dispatcher;
    private BitmapSource _thumb;
    private bool _tagsLoaded = false;
    private BitmapMetadata _tags = new BitmapMetadata("jpg"); //empty tag structure to start with

    public Picture(FileInfo path) : base(path.FullName)
    {
      _dispatcher = Dispatcher.CurrentDispatcher;
      _info = path;
    }

    public override string Name
    {
      get { return _info.Name; }
    }

    public Uri Uri
    {
      get { return new Uri(@"file://" + _info.FullName.Replace('\\', '/')); }
    }

    public long Size
    {
      get { return _info.Length; }
    }

    public BitmapSource Thumbnail
    {
      get
      {
        if (_thumb == null)
        {
          _dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new VoidMethod(LoadThumb));
        }
        return _thumb;
      }
    }

    public string Author
    {
      get
      {
        AssertTagsLoaded();
        return BuildString(_tags.Author);
      }
    }

    public string CameraModel
    {
      get
      {
        AssertTagsLoaded();
        return _tags.CameraModel;
      }
    }

    public string CameraManufacturer
    {
      get
      {
        AssertTagsLoaded();
        return _tags.CameraManufacturer;
      }
    }

    public string DateTaken
    {
      get
      {
        AssertTagsLoaded();
        return _tags.DateTaken;
      }
    }

    public string ApplicationName
    {
      get
      {
        AssertTagsLoaded();
        return _tags.ApplicationName;
      }
    }

    public string Comment
    {
      get
      {
        AssertTagsLoaded();
        return _tags.Comment;
      }
    }

    public string Copyright
    {
      get
      {
        AssertTagsLoaded();
        return _tags.Copyright;
      }
    }

    public string Format
    {
      get
      {
        AssertTagsLoaded();
        return _tags.Format;
      }
    }

    public int Rating
    {
      get
      {
        AssertTagsLoaded();
        return _tags.Rating;
      }
    }

    public string Subject
    {
      get
      {
        AssertTagsLoaded();
        return _tags.Subject;
      }
    }

    public string Title
    {
      get
      {
        AssertTagsLoaded();
        return _tags.Title;
      }
    }

    #region INotifyPropertyChanged Members

    ///<summary>
    ///Occurs when a property value changes.
    ///</summary>
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    public override void Accept(IMediaVisitor visitor)
    {
      visitor.Visit(this);
    }

    private void LoadThumb()
    {
      using (FileStream stream = _info.OpenRead())
      {
        BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
        _thumb = decoder.Frames[0].Thumbnail;
      }
      OnPropertyChanged("Thumbnail");
    }

    private void LoadTags()
    {
      using (FileStream stream = _info.OpenRead())
      {
        BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
        _tags = (BitmapMetadata) decoder.Frames[0].Metadata;
        _tagsLoaded = true;
        OnPropertyChanged("ApplicationName");
        OnPropertyChanged("Author");
        OnPropertyChanged("CameraManufacturer");
        OnPropertyChanged("CameraModel");
        OnPropertyChanged("Comment");
        OnPropertyChanged("Copyright");
        OnPropertyChanged("DateTaken");
        OnPropertyChanged("Format");
        OnPropertyChanged("Rating");
        OnPropertyChanged("Subject");
        OnPropertyChanged("Title");
      }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    private static string BuildString(IEnumerable<string> list)
    {
      StringBuilder b = new StringBuilder();
      foreach (string s in list)
      {
        if (b.Length > 0)
        {
          b.Append(", ");
        }
        b.Append(s);
      }
      return b.ToString();
    }

    private void AssertTagsLoaded()
    {
      if (!_tagsLoaded)
      {
        _dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new VoidMethod(LoadTags));
      }
    }

    private delegate void VoidMethod();
  }
}