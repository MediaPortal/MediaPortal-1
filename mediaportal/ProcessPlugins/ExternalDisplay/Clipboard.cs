using System;
using System.Text;
using System.Threading;
using System.Timers;
using Clip = System.Windows.Forms.Clipboard;
using Timer = System.Timers.Timer;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This class copies the generated text lines to the Windows Clipboard
  /// </summary>
  /// <remarks>
  /// The copying is done in a separate thread because communication with a COM component needs a 
  /// Single Threaded Appartment (STA) thread</remarks>
  public class Clipboard : IDisplay
  {
    private string[] lines;
    private System.Timers.Timer timer;
    private Thread th; //pointer to the thread that does the copying

    /// <summary>
    /// Constructor
    /// </summary>
    public Clipboard()
    {
      lines = new string[Settings.Instance.TextHeight];
      Clear();
      timer = new Timer(Settings.Instance.ScrollDelay);
      timer.Enabled = false;
      timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
    }

    /// <summary>
    /// Starts the display.
    /// </summary>
    /// <remarks>
    public void Start()
    {
      timer.Enabled = true;
    }

    /// <summary>
    /// Stops the display.
    /// </summary>
    public void Stop()
    {
      timer.Enabled = false;
      if (th != null)
      {
        if (th.IsAlive)
        {
          th.Abort();
        }
      }
      Clip.SetDataObject("");
    }


    /// <summary>
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="_line">The line to thow the message on.</param>
    /// <param name="_message">The message to show.</param>
    public void SetLine(int _line, string _message)
    {
      lines[_line] = _message;
    }

    /// <summary>
    /// Starts the copy thread
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (th != null)
      {
        if (th.IsAlive)
        {
          th.Abort();
        }
      }
      th = new Thread(new ThreadStart(CopySTA));
      th.ApartmentState = ApartmentState.STA;
      th.Start();
    }


    /// <summary>
    /// Because the .NET Clipboard class uses COM, the thread that uses it needs to be STA.
    /// That is why we start this method in a separate thread and wait until it is done.
    /// </summary>
    [STAThread]
    private void CopySTA()
    {
      try
      {
        StringBuilder b = new StringBuilder();
        for (int i = 0; i < Settings.Instance.TextHeight; i++)
        {
          b.Append(lines[i]);
          b.Append(Environment.NewLine);
        }
        Clip.SetDataObject(b.ToString(), true);
      }
      catch (Exception)
      {}
      Thread.CurrentThread.Join(); //blocks the calling thread until we are done
    }


    /// <summary>
    /// Cleanup when disposed
    /// </summary>
    public void Dispose()
    {
      Stop();
    }

    /// <summary>
    /// Advanced configuration for this display driver
    /// </summary>
    public void Configure()
    {}

    /// <summary>
    /// Initializes the display driver
    /// </summary>
    /// <param name="_port">ignored</param>
    /// <param name="_lines">ignored</param>
    /// <param name="_cols">ignored</param>
    /// <param name="_time">ignored</param>
    /// <param name="_linesG">ignored</param>
    /// <param name="_colsG">ignored</param>
    /// <param name="_timeG">ignored</param>
    /// <param name="_backLight">ignored</param>
    public void Initialize(string _port, int _lines, int _cols, int _time, int _linesG, int _colsG, int _timeG, bool _backLight)
    {
      Clear();
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Clear()
    {
      for (int i = 0; i < Settings.Instance.TextHeight; i++)
      {
        lines[i] = new string(' ', Settings.Instance.TextWidth);
      }
    }

    /// <summary>
    /// The display driver's (short) name
    /// </summary>
    public string Name
    {
      get { return "Clipboard"; }
    }

    /// <summary>
    /// The display driver's description
    /// </summary>
    public string Description
    {
      get { return "Clipboard driver V1.0"; }
    }

    /// <summary>
    /// Does this driver supports textmode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Does this driver supports graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

  }
}