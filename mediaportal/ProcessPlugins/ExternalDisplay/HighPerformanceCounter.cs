using System.Runtime.InteropServices;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// Wrapper class for the Windows HighPerformanceCounter.
  /// </summary>
  public class HighPerformanceCounter
  {
    private static long frequency;
    private long start;
    private long end;

    [DllImport("kernel32")]
    private static extern int QueryPerformanceCounter(out long count);

    [DllImport("kernel32")]
    private static extern int QueryPerformanceFrequency(out long frequency);

    /// <summary>
    /// Static constructor
    /// </summary>
    static HighPerformanceCounter()
    {
      QueryPerformanceFrequency(out frequency);
    }

    /// <summary>
    /// Starts the HighPerformanceCounter
    /// </summary>
    public void Start()
    {
      if (QueryPerformanceCounter(out this.start) == 0)
      {
        this.start = 0;
      }
    }

    /// <summary>
    /// Ends the HighPerformanceCounter
    /// </summary>
    /// <remarks>
    /// After calling this method, the properties for determing the elapsed number
    /// of (micro/milli)seconds make sense.</remarks>
    public void End()
    {
      if (QueryPerformanceCounter(out this.end) == 0)
      {
        this.end = 0;
      }
    }

    /// <summary>
    /// Returns the number of HighPerformanceCounter ticks between the calls to <see cref="Start"/>
    /// and <see cref="End"/>.
    /// </summary>
    /// <value>
    /// The number of ticks
    /// </value>
    public long PeriodCount
    {
      get { return (this.end - this.start); }
    }

    /// <summary>
    /// Returns the number of µseconds that elapsed between the calls to <see cref="Start"/> 
    /// and <see cref="End"/>.
    /// </summary>
    /// <value>
    /// The number of µseconds.
    /// </value>
    public long MicroSeconds
    {
      get
      {
        long period = this.PeriodCount;
        if (period < 0x8637bd05af6)
        {
          return ((period * 1000000) / frequency);
        }
        return ((period / frequency) * 1000000);
      }
    }

    /// <summary>
    /// Returns the number of milliseconds that elapsed between the calls to <see cref="Start"/> 
    /// and <see cref="End"/>.
    /// </summary>
    /// <value>
    /// The number of milliseconds.
    /// </value>
    public long MilliSeconds
    {
      get
      {
        long period = this.PeriodCount;
        if (period < 0x20c49ba5e353f7)
        {
          return ((period * 1000) / frequency);
        }
        return ((period / frequency) * 1000);
      }
    }

    /// <summary>
    /// Returns the number of seconds that elapsed between the calls to <see cref="Start"/> 
    /// and <see cref="End"/>.
    /// </summary>
    /// <value>
    /// The number of seconds.
    /// </value>
    public long Seconds
    {
      get { return (this.PeriodCount / frequency); }
    }

  }
}