using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Text;

namespace MyTv
{
  public class ThumbnailGenerator
  {
    /// <summary>
    /// Generates a thumbnail for a video file.
    /// </summary>
    /// <param name="mediaFileName">Name of the media file.</param>
    public bool GenerateThumbnail(string mediaFileName)
    {
      string thumbNail=System.IO.Path.ChangeExtension(mediaFileName,".png");
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
            System.Threading.Thread.Sleep(10000);
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
      return false;
    }
  }
}
