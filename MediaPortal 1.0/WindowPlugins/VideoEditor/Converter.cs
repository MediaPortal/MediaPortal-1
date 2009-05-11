using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WindowPlugins.VideoEditor
{
  class Converter
  {
    bool hasMencoder = false;
    string mencoderPath;
    EditSettings settings;
    System.Diagnostics.ProcessStartInfo mencoderProcessInfo;
    System.Diagnostics.Process mencoderProcess;
    System.IO.StreamReader consoleReader;
    public delegate void Finished();
    public event Finished OnFinished;
    public delegate void Progress(int percentage);
    public event Progress OnProgress;


    public Converter(EditSettings settings)
    {
      this.settings = settings;
      CheckHasMencoder();
    }

    public bool CheckHasMencoder()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Config, "MediaPortal.xml")))
      {
        mencoderPath = xmlreader.GetValueAsString("VideoEditor", "mencoder", "");
      }
      hasMencoder = System.IO.File.Exists(mencoderPath);
      if (!hasMencoder)
      {
        hasMencoder = System.IO.File.Exists(System.Windows.Forms.Application.StartupPath + @"\mencoder.exe");
        if (hasMencoder)
          mencoderPath = System.Windows.Forms.Application.StartupPath + @"\mencoder.exe";
      }
      return hasMencoder;
    }

    public void Convert(MediaPortal.Core.Transcoding.VideoFormat format)
    {
      switch (format)
      {
        case MediaPortal.Core.Transcoding.VideoFormat.Divx:
          Thread convert = new Thread(new ThreadStart(ConvertToDivx));
          convert.Priority = ThreadPriority.BelowNormal;
          convert.Name = "DivxConverter";
          convert.Start();
          //ConvertToDivx();
          break;
        case MediaPortal.Core.Transcoding.VideoFormat.Mpeg2:
          break;
        case MediaPortal.Core.Transcoding.VideoFormat.Wmv:
          break;
        case MediaPortal.Core.Transcoding.VideoFormat.MP4:
          break;
        default:
          break;
      }
    }

    private void ConvertToDivx()
    {
      try
      {
        if (hasMencoder)
        {
          if (settings.Settings is CompressionSettings)
          {
            CompressionSettings compressSettings = settings.Settings as CompressionSettings;
            mencoderProcessInfo = new System.Diagnostics.ProcessStartInfo(mencoderPath);
            mencoderProcessInfo.Arguments = "-ffourcc DX50 -oac lavc -ovc lavc -lavcopts vcodec=mpeg4:vbitrate=" + compressSettings.videoQuality.ToString() + ":acodec=mp3:abitrate=" + compressSettings.audioQuality.ToString() +
                    " -vf scale=" + compressSettings.resolutionX.ToString() + ":" + compressSettings.resolutionY.ToString() + " -o \"" + settings.FileName.Replace(System.IO.Path.GetExtension(settings.FileName), ".avi") + "\" \"" + settings.FileName + "\"";
            mencoderProcessInfo.CreateNoWindow = true;
            mencoderProcessInfo.RedirectStandardOutput = true;
            mencoderProcess = new System.Diagnostics.Process();
            mencoderProcessInfo.UseShellExecute = false;
            mencoderProcess.StartInfo = mencoderProcessInfo;
            mencoderProcess.Start();
            mencoderProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            consoleReader = mencoderProcess.StandardOutput;

            string line;
            int wherePerc;
            int lastPerc = 0;
            while (!mencoderProcess.HasExited)
            {
              line = consoleReader.ReadLine();
              if (line != null)
              {
                wherePerc = line.IndexOf('%');
                if (wherePerc >= 0)
                {
                  string percent = line.Substring(wherePerc - 3, 3);
                  for (int i = 0; i < percent.Length; i++)
                  {
                    if (percent[i] >= '\x39' || percent[i] <= '\x30')
                      percent = percent.Remove(i, 1);
                  }
                  percent.Trim();
                  try
                  {
                    if (percent.Length > 0 && percent != " ")
                    {
                      if (System.Convert.ToInt32(percent) > lastPerc)
                      {
                        lastPerc = System.Convert.ToInt32(percent);
                        if (OnProgress != null)
                          OnProgress(lastPerc);
                      }
                    }
                  }
                  catch { }
                  System.Threading.Thread.Sleep(20);
                }
              }
            }
          }
        }
        if (OnFinished != null)
          OnFinished();
        if (OnProgress != null)
          OnProgress(100);
      }
      catch (Exception)
      {

      }
    }
    public bool HasMencoder
    {
      get
      {
        return hasMencoder;
      }
    }
  }
}
