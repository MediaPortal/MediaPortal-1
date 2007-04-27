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

      /// <summary>
      /// Initializes a new instance of the <see cref="ThumbNail"/> class.
      /// </summary>
      /// <param name="mediaFile">The media file.</param>
      public ThumbNail(string mediaFile)
      {
        MediaFile = mediaFile;
      }
      /// <summary>
      /// Initializes a new instance of the <see cref="ThumbNail"/> class.
      /// </summary>
      /// <param name="mediaFile">The media file.</param>
      /// <param name="size">The size.</param>
      /// <param name="destinationFolder">The destination folder.</param>
      public ThumbNail(string mediaFile, Size size, string destinationFolder)
      {
        MediaFile = mediaFile;
        Size = size;
        DestinationFolder = destinationFolder;
      }

      /// <summary>
      /// Gets a value indicating whether the media file is a folder or a file
      /// </summary>
      /// <value><c>true</c> if this media file  is folder; otherwise, <c>false</c>.</value>
      public bool IsFolder
      {
        get
        {
          return System.IO.Directory.Exists(MediaFile);
        }
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
          bool result;
          if (thumb.IsFolder)
          {
            result = CreateThumbnailForFolder(thumb, out thumbNail);
          }
          else
          {
            result = CreateThumbnailForFile(thumb, out thumbNail);
          }
          if (OnThumbnailGenerated != null)
          {
            OnThumbnailGenerated(this, new ThumbnailEventArgs(thumb.MediaFile, thumbNail, result));
          }
        }
      }
      _workerThread = null;
    }

    /// <summary>
    /// Creates the thumbnail for a folder.
    /// </summary>
    /// <param name="thumb">The thumb.</param>
    /// <param name="thumbnailFileName">Name of the thumbnail file.</param>
    /// <returns></returns>
    bool CreateThumbnailForFolder(ThumbNail thumb, out string thumbnailFileName)
    {
      thumbnailFileName = null;
      try
      {
        thumbnailFileName = thumb.MediaFile;
        if (!thumbnailFileName.EndsWith(@"\")) thumbnailFileName += @"\";
        thumbnailFileName += "folder.jpg";
        if (System.IO.File.Exists(thumbnailFileName))
        {
          return true;
        }

        //find media files within the folder
        string[] subNails = new string[4];
        int currentThumb = 0;
        string[] files = System.IO.Directory.GetFiles(thumb.MediaFile);
        for (int i = 0; i < files.Length; ++i)
        {
          string ext = System.IO.Path.GetExtension(files[i]).ToLower();
          if (ext == ".wmv" || ext == ".mpg" || ext == ".mpeg" || ext == ".avi" || ext == ".mkv" || ext == ".dvr-ms" || ext == ".ts")
          {
            //media file found, create a thumbnail for this media file
            ThumbNail sub = new ThumbNail(files[i], thumb.Size, thumb.MediaFile);
            string subNail;
            if (CreateThumbnailForFile(sub, out subNail))
            {
              subNails[currentThumb] = subNail;
              currentThumb++;
            }
            if (currentThumb >= 4) break;
          }
        }

        //no media files found?
        if (currentThumb <= 0) return false;

        //create folder thumb 
        RenderTargetBitmap rtb = new RenderTargetBitmap((int)thumb.Size.Width, (int)thumb.Size.Height, 1 / 200, 1 / 200, PixelFormats.Pbgra32);
        DrawingVisual dv = new DrawingVisual();
        DrawingContext dc = dv.RenderOpen();
        for (int i = 0; i < currentThumb; ++i)
        {
          double width = ((thumb.Size.Width - 20) / 2);
          double height = ((thumb.Size.Height - 20) / 2);
          if (currentThumb == 2)
          {
            width = (thumb.Size.Width - 20)/2;
            height = (thumb.Size.Height - 20);
          }
          if (currentThumb == 1)
          {
            height = (thumb.Size.Height - 20) ;
            width = (thumb.Size.Width - 20) ;
          }
          PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(subNails[i], UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnDemand);
          BitmapFrame frame = decoder.Frames[0];
          Rect rect = new Rect();
          rect.X = (i % 2) * width + 10;
          rect.Y = (i / 2) * height + 10;
          rect.Width = width;
          rect.Height = height;
          dc.DrawRectangle(new ImageBrush(frame), null, rect);
          //dc.DrawImage(frame, rect);
          
        }

        dc.Close();
        rtb.Render(dv);
        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        using (FileStream stream = new FileStream(thumbnailFileName, FileMode.OpenOrCreate))
        {
          encoder.Save(stream);
        }
        return true;
      }
      catch (Exception)
      {
      }
      return false;
    }

    /// <summary>
    /// Creates a thumbnail for file.
    /// </summary>
    /// <param name="thumb">The thumb.</param>
    /// <param name="thumbnailFileName">Name of the thumbnail file.</param>
    /// <returns></returns>
    bool CreateThumbnailForFile(ThumbNail thumb, out string thumbnailFileName)
    {
      thumbnailFileName = null;
      string ext = System.IO.Path.GetExtension(thumb.MediaFile).ToLower();
      if (ext == ".wmv" || ext == ".mpg" || ext == ".mpeg" || ext == ".avi" || ext == ".mkv" || ext == ".dvr-ms" || ext == ".ts")
      {
        try
        {
          if (thumb.DestinationFolder == null)
          {
            thumbnailFileName = System.IO.Path.ChangeExtension(thumb.MediaFile, ".png");
          }
          else
          {
            thumbnailFileName = String.Format(@"{0}\{1}.png", thumb.DestinationFolder, System.IO.Path.GetFileNameWithoutExtension(thumb.MediaFile));
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
                GC.Collect();
                GC.Collect();
                GC.Collect();
                return true;
              }
              catch (Exception)
              {
              }
            }
          }
          else
          {
            return true;
          }
        }
        catch (Exception)
        {
        }
      }
      return false;
    }
  }
}