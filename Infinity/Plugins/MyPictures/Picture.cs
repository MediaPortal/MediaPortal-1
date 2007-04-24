using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ProjectInfinity.Pictures
{
  public class Picture : MediaItem ,INotifyPropertyChanged
  {
    ///<summary>
    ///Occurs when a property value changes.
    ///</summary>
    public event PropertyChangedEventHandler PropertyChanged;

    private delegate void VoidMethod();
    private readonly FileInfo _info;
    private readonly Dispatcher _dispatcher;
    private BitmapSource _thumb;

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
      get
      {
        return new Uri(@"file://"+_info.FullName.Replace('\\','/'));
      }
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

    public long  Size
    {
      get
      {
        return _info.Length;
      }
    }

    public override void Accept(IMediaVisitor visitor)
    {
      visitor.Visit(this);
    }

    private void LoadThumb()
    {

      using (FileStream stream = _info.OpenRead())
      {
        JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
        _thumb = decoder.Frames[0].Thumbnail;
      }
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs("Thumbnail"));
    }
  }
}