using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay
{
	/// <summary>
	/// Summary description for iMON.
	/// </summary>
	public class iMON : IDisplay
	{
    private const int VfdType = 4;
    private string[] lines = new string[2];

    public iMON()
		{

    }

	  private void OpenLcd()
	  {
      if (!Open(VfdType,0))
        Log.Write("ExternalDisplay.iMON.Start: Could not open display");
	  }

	  /// <summary>
	  /// Stops the display.
	  /// </summary>
    private void CloseLcd()
    {
      if (IsOpen())
        Close();
    }

    private void DisplayLines()
    {
      SetText(lines[0],lines[1]);
    }

    [DllImport("SG_VFD.dll", EntryPoint="iMONVFD_Init")]
    private static extern bool Open(int vfdType, int resevered);

    [DllImport("SG_VFD.dll", EntryPoint="iMONVFD_Uninit")]
    private static extern void Close();

    [DllImport("SG_VFD.dll", EntryPoint="iMONVFD_IsInited")]
    private static extern bool IsOpen();

    [DllImport("SG_VFD.dll", EntryPoint="iMONVFD_SetText")]
    private static extern bool SetText(string firstLine, string secondLine);

	  /// <summary>
	  /// Shows the given message on the indicated line.
	  /// </summary>
	  /// <param name="line">The line to thow the message on.</param>
	  /// <param name="message">The message to show.</param>
	  public void SetLine(int line, string message)
	  {
      lines[line] = message;
      if (line==1)
        DisplayLines();
	  }

	  /// <summary>
	  /// Gets the short name of the display
	  /// </summary>
	  public string Name
	  {
	    get { return "iMON"; }
	  }

	  /// <summary>
	  /// Gets the description of the display
	  /// </summary>
	  public string Description
	  {
	    get { return "SoundGraph iMON USB Driver V1.0"; }
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
	    //No advanced configuration needed
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
	  /// <param name="timeG">Communication delay in graphic mode</param>
	  /// <param name="backLight">Backlight on?</param>
	  public void Initialize(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight)
	  {
	    OpenLcd();
	  }

	  /// <summary>
	  /// Clears the display
	  /// </summary>
	  public void Clear()
	  {
      for(int i=0; i<2; i++)
        lines[i]=new string(' ',Settings.Instance.TextWidth);
	  }

    /// <summary>
    /// Cleanup
    /// </summary>
	  public void Dispose()
	  {
      CloseLcd();
	  }
	}
}
