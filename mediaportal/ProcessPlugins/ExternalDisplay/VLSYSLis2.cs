using System;
using Communications;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// VL System L.I.S. 2 Driver
  /// </summary>
  /// <author>Nopap</author>
  public class VLSYSLis2 : IDisplay
  {
    private Rs232 commPort = null;
    private int lines = 2;
    private int cols = 40;

    #region IDisplay Members

    /// <summary>
    /// Displays the message on the indicated line
    /// </summary>
    /// <param name="line">The line to display the message on</param>
    /// <param name="message">The message to display</param>
    public void SetLine(int line, string message)
    {
      commPort.Write(new byte[] {0});
      if (line == 0)
      {
        commPort.Write(new byte[] {0xA1});
      }
      else if (line == 1)
      {
        commPort.Write(new byte[] {0xA2});
      }
      else
      {
        Log.Write("VLSYSLis2.SetLine: error bad line number" + line);
        return;
      }
      commPort.Write(new byte[] {0});
      commPort.Write(new byte[] {0xA7});
      commPort.Write(message);
      commPort.Write(new byte[] {0});
    }

    /// <summary>
    /// Short name of this display driver
    /// </summary>
    public string Name
    {
      get { return "VLSYSLis2"; }
    }

    /// <summary>
    /// Description of this display driver
    /// </summary>
    public string Description
    {
      get { return "VL System L.I.S 2 driver V1.0, by Nopap"; }
    }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

    /// <summary>
    /// Shows the advanced configuration screen
    /// </summary>
    public void Configure()
    {
      //Nothing to configure
    }

    /// <summary>
    /// Initializes the display
    /// </summary>
    /// <param name="port">The port the display is connected to</param>
    /// <param name="lines">The number of lines in text mode</param>
    /// <param name="cols">The number of columns in text mode</param>
    /// <param name="delay">Communication delay in text mode</param>
    /// <param name="linesG">The height in pixels in graphic mode</param>
    /// <param name="colsG">The width in pixels in graphic mode</param>
    /// <param name="delayG">Communication delay in graphic mode</param>
    /// <param name="backLight">Backlight on?</param>
    public void Initialize(string port, int lines, int cols, int delay, int linesG, int colsG, int delayG, bool backLight, int contrast)
    {
      try
      {
        commPort = new Rs232();
        commPort.BaudRate = 19200;
        commPort.DataBit = 8;
        commPort.Parity = Rs232.DataParity.Parity_None;
        commPort.StopBit = Rs232.DataStopBit.StopBit_1;

        switch (port.ToLower())
        {
          case "com1":
            commPort.Port = 1;
            break;
          case "com2":
            commPort.Port = 2;
            break;
          case "com4":
            commPort.Port = 4;
            break;
          default: // default to com3
            commPort.Port = 3;
            break;
        }
        commPort.Open();
      }
      catch (Exception ex)
      {
        Log.Write("VLSYSLis2.Initialize: " + ex.Message);
      }
    }

    public void Clear()
    {
      string s = new string(' ', this.cols);
      for (int i = 0; i < this.lines; i++)
      {
        SetLine(i, s);
      }
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      try
      {
        if ((commPort != null) && (commPort.IsOpen))
        {
          commPort.Close();
        }
      }
      catch (Exception ex)
      {
        Log.Write("VLSYSLis2.Dispose: " + ex.Message);
      }
    }

    #endregion
  }
}