using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Text;
using System.Threading;

namespace ProjectInfinity.Thumbnails
{
  public class ThumbnailBuilder : IThumbnailBuilder
  {
    #region subclasses

    class ThumbNail
    {
      public Size Size = new Size(320, 240);
      public string MediaFile;
      public string DestinationFolder;
      public ThumbNail(string mediaFile)
      {
        MediaFile = mediaFile;
      }
      public ThumbNail(string mediaFile, Size size, string destinationFolder)
      {
        MediaFile = mediaFile;
        Size = size;
        DestinationFolder = destinationFolder;
      }
    }
    #endregion
    #region variables
    public event ThumbNailGenerateHandler OnThumbnailGenerated;
    List<ThumbNail> _workTodo = new List<ThumbNail>();
    Thread _workerThread;
    #endregion

    /// <summary>
    /// Generates a thumbnail for the specified media file
    /// </summary>
    /// <param name="mediaFile">The media file.</param>
    public void Generate(string mediaFile)
    {
      lock (_workTodo)
      {
        ThumbNail thumb = new ThumbNail(mediaFile);
        _workTodo.Add(thumb);
      }
      StartWork();
    }

    /// <summary>
    /// Generates a thumbnail for the specified media files
    /// </summary>
    /// <param name="mediaFile">The media files.</param>
    public void Generate(List<string> mediaFiles)
    {
      lock (_workTodo)
      {
        foreach (string mediaFile in mediaFiles)
        {
          ThumbNail thumb = new ThumbNail(mediaFile);
          _workTodo.Add(thumb);
        }
      }
      StartWork();
    }

    /// <summary>
    /// Generates a thumbnail for the specified media file
    /// </summary>
    /// <param name="mediaFile">The media file.</param>
    public void Generate(string mediaFile, Size size, string destinationFolder)
    {
      lock (_workTodo)
      {
        ThumbNail thumb = new ThumbNail(mediaFile, size, destinationFolder);
        _workTodo.Add(thumb);
      }
      StartWork();
    }

    /// <summary>
    /// Generates a thumbnail for the specified media files
    /// </summary>
    /// <param name="mediaFile">The media files.</param>
    public void Generate(List<string> mediaFiles, Size size, string destinationFolder)
    {
      lock (_workTodo)
      {
        foreach (string mediaFile in mediaFiles)
        {
          ThumbNail thumb = new ThumbNail(mediaFile, size, destinationFolder);
          _workTodo.Add(thumb);
        }
      }
      StartWork();
    }

    /// <summary>
    /// Starts the workerthread
    /// </summary>
    void StartWork()
    {
      if (_workerThread == null)
      {
        if (_workTodo.Count > 0)
        {
          _workerThread = new Thread(new ThreadStart(WorkerThread));
          _workerThread.Priority = ThreadPriority.BelowNormal;
          _workerThread.IsBackground = true;
          _workerThread.Start();
        }
      }
    }
    /// <summary>
    /// Workers the thread.
    /// </summary>
    void WorkerThread()
    {
      while (_workTodo.Count > 0)
      {
        ThumbNail thumb;
        lock (_workTodo)
        {
          thumb = _workTodo[0];
          _workTodo.RemoveAt(0);
        }
        if (thumb != null)
        {
          string thumbNail;
          bool result = Create(thumb, out thumbNail);
          if (OnThumbnailGenerated != null)
          {
            OnThumbnailGenerated(this, new ThumbnailEventArgs(thumb.MediaFile, thumbNail, result));
          }
        }
      }
      _workerThread = null;
    }

    /// <summary>
    /// Creates a thumbnail for the media file name.
    /// </summary>
    /// <param name="mediaFileName">Name of the media file.</param>
    /// <param name="thumbNail">The thumb nail created.</param>
    /// <returns>true if succeeded, else false</returns>
    bool Create(ThumbNail thumb, out string thumbnailFileName)
    {
      thumbnailFileName = null;
      try
      {
        if (thumb.DestinationFolder == null)
        {
          thumbnailFileName = System.IO.Path.ChangeExtension(thumb.MediaFile, ".png");
        }
        else
        {
          thumbnailFileName =String.Format(@"{0}\{1}.png",thumb.DestinationFolder, System.IO.Path.GetFileNameWithoutExtension(thumb.MediaFile));
        }
        if (!System.IO.File.Exists(thumbnailFileName))
        {
          if (System.IO.File.Exists(thumb.MediaFile))
          {
            try
            {
              MediaPlayer player = new MediaPlayer();
              player.Open(new Uri(thumb.MediaFile, UriKind.Absolute));
              player.ScrubbingEnabled = true;
              player.Play();
              player.Pause();
              player.Position = new TimeSpan(0, 0, 20);
              System.Threading.Thread.Sleep(4000);
              RenderTargetBitmap rtb = new RenderTargetBitmap((int)thumb.Size.Width, (int)thumb.Size.Height, 1 / 200, 1 / 200, PixelFormats.Pbgra32);
              DrawingVisual dv = new DrawingVisual();
              DrawingContext dc = dv.RenderOpen();
              dc.DrawVideo(player, new Rect(0, 0, thumb.Size.Width, thumb.Size.Height));
              dc.Close();
              rtb.Render(dv);
              PngBitmapEncoder encoder = new PngBitmapEncoder();
              encoder.Frames.Add(BitmapFrame.Create(rtb));
              using (FileStream stream = new FileStream(thumbnailFileName, FileMode.OpenOrCreate))
              {
                encoder.Save(stream);
              }
              player.Stop();
              player.Close();
              return true;
            }
            catch (Exception)
            {
            }
          }
        }
      }
      catch (Exception)
      {
      }
      return false;
    }
  }
}