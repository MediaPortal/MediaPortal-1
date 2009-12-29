using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Timers;
using System.Collections;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvEngine.PowerScheduler.Interfaces;

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Represents a network adapter installed on the machine.
  /// Properties of this class can be used to obtain current network speed.
  /// </summary>
  internal class NetworkAdapter
  {
    private PerformanceCounter dlCounter, ulCounter;	// Performance counters to monitor download and upload speed.
    //private long dlSpeed, ulSpeed;			  	          // Download Upload speed in bytes per second.
    //private long dlValue, ulValue;				            // Download Upload counter value in bytes.
    private long dlValueOld, ulValueOld;		          // Download Upload counter value one second earlier, in bytes.
    private DateTime lastSampleTime;

    internal long dlSpeedPeak, ulSpeedPeak;  // Download Upload peak values in KB/s
    internal string name;							       // The name of the adapter.

    /// <summary>
    /// Instances of this class are supposed to be created only in an NetworkMonitorHandler.
    /// </summary>
    internal NetworkAdapter(string name)
    {
      this.name = name;

      // Create performance counters for the adapter.
      dlCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", this.name);
      ulCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", this.name);

      // Since dlValueOld and ulValueOld are used in method update() to calculate network speed,
      // they must have be initialized.
      lastSampleTime = DateTime.Now;
      dlValueOld = dlCounter.NextSample().RawValue;
      ulValueOld = ulCounter.NextSample().RawValue;

      // Clear peak values
      dlSpeedPeak = 0;
      ulSpeedPeak = 0;
    }

    /// <summary>
    /// Obtain new sample from performance counters, and update the values saved in dlSpeed, ulSpeed, etc.
    /// This method is supposed to be called only in NetworkMonitorHandler, one time every second.
    /// </summary>
    internal void update()
    {
      DateTime thisSampleTime = DateTime.Now;
      // Download Upload counter value in bytes.
      long dlValue = dlCounter.NextSample().RawValue;
      long ulValue = ulCounter.NextSample().RawValue;

      // Calculates download and upload speed.
      double monitorInterval = thisSampleTime.Subtract(lastSampleTime).TotalSeconds;
      lastSampleTime = thisSampleTime;
      long dlSpeed = (long)((dlValue - dlValueOld) / monitorInterval);
      long ulSpeed = (long)((ulValue - ulValueOld) / monitorInterval);

      dlValueOld = dlValue;
      ulValueOld = ulValue;

      if ((dlSpeed / 1024) > dlSpeedPeak)     // Store peak values in KB/s
      {
        dlSpeedPeak = (dlSpeed / 1024);
      }

      if ((ulSpeed / 1024) > ulSpeedPeak)     // Store peak values in KB/s
      {
        ulSpeedPeak = (ulSpeed / 1024);
      }
    }
  }

  public class NetworkMonitorHandler : IStandbyHandler
  {
    #region Constants
    
    private const int MonitorInteval = 10; // seconds
    
    #endregion

    #region Variables

    private Timer timer;						        // The timer event executes every second to refresh the values in adapters.
    private Int32 idleLimit;                // Minimum transferrate considered as network activity in KB/s.

    private ArrayList monitoredAdapters = new ArrayList(); 		// The list of monitored adapters on the computer.
    private List<string> _preventers = new List<string>();    // The list of standby preventers.

    #endregion

    #region Constructor
    public NetworkMonitorHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent += new PowerSchedulerEventHandler(NetworkMonitorHandler_OnPowerSchedulerEvent);
    }
    #endregion

    #region Private methods
    void NetworkMonitorHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Elapsed:

          IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
          if (ps == null)
            return;

          TvBusinessLayer layer = new TvBusinessLayer();
          PowerSetting setting;

          bool enabled;

          // Check if standby should be prevented
          setting = ps.Settings.GetSetting("NetworkMonitorEnabled");
          enabled = Convert.ToBoolean(layer.GetSetting("NetworkMonitorEnabled", "false").Value);

          if (setting.Get<bool>() != enabled)  // Setting changed
          {
            setting.Set<bool>(enabled);
            if (enabled) // Start
            {
              Log.Debug("NetworkMonitorHandler: networkMonitor started");
              StartNetworkMonitor();
            }
            else // Stop
            {
              Log.Debug("NetworkMonitorHandler: networkMonitor stopped");
              StopNetworkMonitor();
            }
          }

          if (enabled)   // Get minimum transferrate considered as network activity
          {
            idleLimit = Int32.Parse(layer.GetSetting("NetworkMonitorIdleLimit", "2").Value);
            Log.Debug("NetworkMonitorHandler: idle limit in KB/s: {0}", idleLimit);
          }

          break;
      }
    }

    private void StartNetworkMonitor()
    {
      monitoredAdapters.Clear();

      PerformanceCounterCategory category =
        new PerformanceCounterCategory("Network Interface");

      // Enumerates network adapters installed on the computer.
      foreach (string name in category.GetInstanceNames())
      {
        // This one exists on every computer.
        if (name == "MS TCP Loopback interface") continue;

        // Create an instance of NetworkAdapter class.        
        NetworkAdapter adapter = new NetworkAdapter(name);

        monitoredAdapters.Add(adapter);    // Add it to monitored adapters
      }

      // Create and enable the timer 
      timer = new Timer(MonitorInteval * 1000);
      timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
      timer.Enabled = true;
    }

    // Disable the timer, and clear the monitoredAdapters list.
    private void StopNetworkMonitor()
    {
      monitoredAdapters.Clear();
      timer.Enabled = false;
    }

    // Timer elapsed
    private void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      foreach (NetworkAdapter adapter in monitoredAdapters)
        adapter.update();
    }

    #endregion

    #region IStandbyHandler implementation
    public bool DisAllowShutdown
    {
      get
      {
        _preventers.Clear();

        foreach (NetworkAdapter adapter in monitoredAdapters)
          if ((adapter.ulSpeedPeak >= idleLimit) || (adapter.dlSpeedPeak >= idleLimit))
          {
            Log.Debug("NetworkMonitorHandler: standby prevented: {0}", adapter.name);
            Log.Debug("NetworkMonitorHandler: ulSpeed: {0}", adapter.ulSpeedPeak);
            Log.Debug("NetworkMonitorHandler: dlSpeed: {0}", adapter.dlSpeedPeak);

            adapter.ulSpeedPeak = 0;  // Clear peak values
            adapter.dlSpeedPeak = 0;

            _preventers.Add(adapter.name);  // Add adapter to preventers

            DateTime now = DateTime.Now;    // Get current date and time

            //Network activity is considered as user activity
            IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
            ps.UserActivityDetected(now);
          }

        return (_preventers.Count > 0);
      }
    }
    public void UserShutdownNow()
    {
    }
    public string HandlerName
    {
      get { return "NetworkMonitorHandler"; }
    }
    #endregion
  }
}
