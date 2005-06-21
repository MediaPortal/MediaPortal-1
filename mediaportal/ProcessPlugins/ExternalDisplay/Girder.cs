using System;
using System.Runtime.InteropServices;
using GIRDERLib;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// Girder Module driver
  /// </summary>
  public class Girder : IDisplay
  {
    private int maxLines = 2;
    private int maxColumns = 16;
    private string[] textLines;
    private GirderEvent girder;
    private bool girderInstalled = false; //is Girder Module DLL installed and registered?

    public Girder()
    {
      try
      {
        //this will throw a COMException if Girder is not installed or the girder.dll is not registered.
        //If it does, the girderInstalled variable will not be set to true
        girder = new GirderEvent();
        girderInstalled = true;
      }
      catch (COMException)
      {}
    }

    #region IDisplay Members

    /// <summary>
    /// Puts the passed message in the buffer for the given line
    /// </summary>
    /// <param name="line">The line in the buffer to put the message in</param>
    /// <param name="message">The message to put in the buffer</param>
    public void SetLine(int line, string message)
    {
      textLines[line++] = message;
      if (line == maxLines)
      {
        SendToGirder();
      }
    }

    /// <summary>
    /// Returns the name of this driver
    /// </summary>
    public string Name
    {
      get { return "Girder"; }
    }

    /// <summary>
    /// Returns the description of this driver
    /// </summary>
    public string Description
    {
      get
      {
        if (girderInstalled)
        {
          return "Girder Module V1.0";
        }
        return "Girder Module V1.0 (not detected...)";
      }
    }

    /// <summary>
    /// Returns wether this driver supports text mode
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Returns wether this driver supports graphics mode
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public void Configure()
    {}

    /// <summary>
    /// Initializes the driver
    /// </summary>
    /// <param name="port">ignored</param>
    /// <param name="lines">the number of lines to keep in the buffer</param>
    /// <param name="cols">the number of columns for each line</param>
    /// <param name="delay">ignored</param>
    /// <param name="linesG">ignored</param>
    /// <param name="colsG">ignored</param>
    /// <param name="timeG">ignored</param>
    /// <param name="backLight">ignored</param>
    /// <param name="contrast">ignored</param>
    public void Initialize(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight, int contrast)
    {
      maxLines = lines;
      maxColumns = cols;
      this.textLines = new string[lines];
      Clear();
    }

    /// <summary>
    /// Clears the buffer, and sends it to Girder
    /// </summary>
    public void Clear()
    {
      for (int i = 0; i < maxLines; i++)
      {
        textLines[i] = new string(' ', maxColumns);
      }
      SendToGirder();
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Cleanup 
    /// </summary>
    public void Dispose()
    {
      girder = null;
    }

    #endregion

    /// <summary>
    /// Sends the buffer to Girder by raising a MPDisplayUpdated event
    /// </summary>
    private void SendToGirder()
    {
      if (!girderInstalled)
      {
        return;
      }
      Array lines = textLines;
      girder.SendEvent("MPDisplayUpdated", 19, ref lines);
    }
  }
}