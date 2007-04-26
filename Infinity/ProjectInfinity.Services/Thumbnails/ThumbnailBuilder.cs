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
    #region variables
    public event ThumbNailGenerateHandler OnThumbnailGenerated;
    List<string> _workTodo = new List<string>();
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
        _workTodo.Add(mediaFile);
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
        _workTodo.AddRange(mediaFiles);
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
        string mediaFile;
        lock (_workTodo)
        {
          mediaFile = _workTodo[0];
          _workTodo.RemoveAt(0);
        }
        if (mediaFile != null)
        {
          string thumbNail;
          bool result = Create(mediaFile, out thumbNail);
          if (OnThumbnailGenerated != null)
          {
            OnThumbnailGenerated(this, new ThumbnailEventArgs(mediaFile,thumbNail,result));
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
    bool Create(string mediaFileName, out string thumbNail)
    {
      thumbNail = null;
      try
      {
        thumbNail = System.IO.Path.ChangeExtension(mediaFileName, ".png");
        if (!System.IO.File.Exists(thumbNail))
        {
          if (System.IO.File.Exists(mediaFileName))
          {
            try
            {
              MediaPlayer player = new MediaPlayer();
              player.Open(new Uri(mediaFileName, UriKind.Absolute));
              player.ScrubbingEnabled = true;
              player.Play();
              player.Pause();
              player.Position = new TimeSpan(0, 0, 20);
              System.Threading.Thread.Sleep(4000);
              RenderTargetBitmap rtb = new RenderTargetBitmap(320, 240, 1 / 200, 1 / 200, PixelFormats.Pbgra32);
              DrawingVisual dv = new DrawingVisual();
              DrawingContext dc = dv.RenderOpen();
              dc.DrawVideo(player, new Rect(0, 0, 320, 240));
              dc.Close();
              rtb.Render(dv);
              PngBitmapEncoder encoder = new PngBitmapEncoder();
              encoder.Frames.Add(BitmapFrame.Create(rtb));
              using (FileStream stream = new FileStream(thumbNail, FileMode.OpenOrCreate))
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