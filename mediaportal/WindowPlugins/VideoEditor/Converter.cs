using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Core.Transcoding;
using MediaPortal.Profile;

namespace WindowPlugins.VideoEditor
{
  internal class Converter
  {
    private bool hasMencoder = false;
    private string mencoderPath;
    private EditSettings settings;
    private ProcessStartInfo mencoderProcessInfo;
    private Process mencoderProcess;
    private StreamReader consoleReader;

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
      using (Settings xmlreader = new MPSettings())
      {
        mencoderPath = xmlreader.GetValueAsString("VideoEditor", "mencoder", "");
      }
      hasMencoder = File.Exists(mencoderPath);
      if (!hasMencoder)
      {
        hasMencoder = File.Exists(Application.StartupPath + @"\mencoder.exe");
        if (hasMencoder)
        {
          mencoderPath = Application.StartupPath + @"\mencoder.exe";
        }
      }
      return hasMencoder;
    }

    public void Convert(VideoFormat format)
    {
      switch (format)
      {
        case VideoFormat.Divx:
          Thread convert = new Thread(new ThreadStart(ConvertToDivx));
          convert.Priority = ThreadPriority.BelowNormal;
          convert.Name = "DivxConverter";
          convert.Start();
          //ConvertToDivx();
          break;
        case VideoFormat.Mpeg2:
          break;
        case VideoFormat.Wmv:
          break;
        case VideoFormat.MP4:
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
            mencoderProcessInfo = new ProcessStartInfo(mencoderPath);
            mencoderProcessInfo.Arguments = "-ffourcc DX50 -oac lavc -ovc lavc -lavcopts vcodec=mpeg4:vbitrate=" +
                                            compressSettings.videoQuality.ToString() + ":acodec=mp3:abitrate=" +
                                            compressSettings.audioQuality.ToString() +
                                            " -vf scale=" + compressSettings.resolutionX.ToString() + ":" +
                                            compressSettings.resolutionY.ToString() + " -o \"" +
                                            settings.FileName.Replace(Path.GetExtension(settings.FileName), ".avi") +
                                            "\" \"" + settings.FileName + "\"";
            mencoderProcessInfo.CreateNoWindow = true;
            mencoderProcessInfo.RedirectStandardOutput = true;
            mencoderProcess = new Process();
            mencoderProcessInfo.UseShellExecute = false;
            mencoderProcess.StartInfo = mencoderProcessInfo;
            mencoderProcess.Start();
            mencoderProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
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
                    {
                      percent = percent.Remove(i, 1);
                    }
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
                        {
                          OnProgress(lastPerc);
                        }
                      }
                    }
                  }
                  catch
                  {
                  }
                  Thread.Sleep(20);
                }
              }
            }
          }
        }
        if (OnFinished != null)
        {
          OnFinished();
        }
        if (OnProgress != null)
        {
          OnProgress(100);
        }
      }
      catch (Exception)
      {
      }
    }

    public bool HasMencoder
    {
      get { return hasMencoder; }
    }
  }
}