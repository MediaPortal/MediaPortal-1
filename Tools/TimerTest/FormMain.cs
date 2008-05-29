#region Usings

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace TimerTest
{
  public partial class FormMain : Form
  {
    #region DDL imports

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll")]
    static extern IntPtr GetCurrentThread();

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll")]
    static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32")]
    private static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32")]
    private static extern bool QueryPerformanceCounter(ref long PerformanceCount);

    #endregion

    #region Thread delegates

    protected delegate void MethodListViewItems(CheckResults aItem);

    #endregion

    #region Result structs

    public struct CheckResults
    {
      public string ResultOutput;
      public double TimerResolution;
      public double StopwatchResolution;
    }

    #endregion

    #region Variables

    private double fMaxAccuracy = 1;
    private object fExecLock = null;
    private Dictionary<int, CheckResults> fResultStrings = null;

    #endregion

    #region Constructors

    public FormMain()
    {
      InitializeComponent();
      fExecLock = new object();
    }

    #endregion

    #region Control events

    private void buttonClose_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
      buttonStart.Enabled = false;
      lblAverageAccuracy.Text = "";
      lblMaxAccurary.Text = "";
      lblAverageNetAccuracy.Text = "";
      lblCounter.Text = "";

      fResultStrings = new Dictionary<int, CheckResults>((int)numLoopCount.Value);

      if (checkBoxClearValues.Checked)
        listBoxResults.Items.Clear();

      // Since even setting up thousands of threads will be costy we do this by another thread.
      Thread ExecutionThread = new Thread(new ThreadStart(GetTimerFrequencyThread));
      ExecutionThread.IsBackground = true;
      ExecutionThread.Start();
    }

    #endregion

    #region Thread methods

    unsafe private void GetTimerFrequencyThread()
    {
      for (int j = 0 ; j < numLoopCount.Value ; j++)
      {
        lock (fExecLock)
        {
          for (short i = 0 ; i < Environment.ProcessorCount ; i++)
          {
            try
            {
              Thread TimerThread = new Thread(new ParameterizedThreadStart(CheckTimer));
              TimerThread.Start((object)i);
              TimerThread.IsBackground = true;
            }
            catch (ThreadAbortException) { }
            catch (ThreadStateException) { }
            SetThreadAffinityMask(GetCurrentThread(), new IntPtr(0));
          }
        }
      }
    }

    unsafe private void CheckTimer(object aCpuNo)
    {
      CheckResults ThreadSummary = new CheckResults();
      ThreadSummary.TimerResolution = 0;
      ThreadSummary.StopwatchResolution = 0;

      try
      {
        try
        {
          short currentCpu = (short)aCpuNo;
          SetThreadAffinityMask(GetCurrentThread(), new IntPtr(1 << currentCpu));

          float MpAccuracy = DXUtil.Timer(DirectXTimer.GetAbsoluteTime);
          MpAccuracy = DXUtil.Timer(DirectXTimer.GetAbsoluteTime) - MpAccuracy;

          Stopwatch TimerWatch = new Stopwatch();
          TimerWatch.Start();
          TimerWatch.Stop();
          double NetAccuracyMs = ((double)TimerWatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000;
          string NetAccuracyResult = NetAccuracyMs.ToString("N5");

          long TicksPerSec = 0;
          if (!QueryPerformanceFrequency(ref TicksPerSec))
          {
            MessageBox.Show("This system does not support accurate timing!");
            return;
          }

          long TempTime = 0;
          double StartTime = 0;

          QueryPerformanceCounter(ref TempTime);
          // we neglect the overhead of these assignments in between..
          StartTime = TempTime;
          QueryPerformanceCounter(ref TempTime);

          // The timespan between two timer results in ms
          double SysAccuracyMs = ((TempTime - StartTime) / TicksPerSec) * 1000;
          string SysAccuracyResult = SysAccuracyMs.ToString("N5");

          if (SysAccuracyMs < fMaxAccuracy)
            fMaxAccuracy = SysAccuracyMs;

          ThreadSummary.ResultOutput = string.Format("CPU {0}: Ticks per second: {1} / MP accuracy: {2} / .NET accuracy: {3} / Approx. accurary in ms: {4}", currentCpu, TicksPerSec, MpAccuracy, NetAccuracyResult, SysAccuracyResult);
          ThreadSummary.TimerResolution = SysAccuracyMs;
          ThreadSummary.StopwatchResolution = NetAccuracyMs;

          Thread.Sleep(0);
        }
        catch (ThreadAbortException tae) { ThreadSummary.ResultOutput = tae.Message; }
        catch (ThreadStateException tse) { ThreadSummary.ResultOutput = tse.Message; }

        // Let the GUI thread add our result to the listbox
        Invoke(new MethodListViewItems(ThreadedListItemAdder), new object[] { ThreadSummary });
      }
      catch (Exception) { }
    }

    #endregion

    #region GUI methods

    private void ThreadedListItemAdder(CheckResults aItem)
    {
      int resultCount = fResultStrings.Count + 1;
      fResultStrings.Add(resultCount, aItem);

      if (resultCount % 10 == 0)
      {
        lblCounter.Text = Convert.ToString(resultCount / Environment.ProcessorCount);
        lblCounter.Refresh();
      }

      // Check whether the last result is currently being added
      if (resultCount == (Environment.ProcessorCount * numLoopCount.Value))
      {
        listBoxResults.BeginUpdate();

        double TotalAccuracyVals = 0;
        double TotalNetAccuracyVals = 0;
        foreach (KeyValuePair<int, CheckResults> threadResult in fResultStrings)
        {
          // show 1 as 00001
          listBoxResults.Items.Add(string.Format("{0} - {1}", threadResult.Key.ToString("d5"), threadResult.Value.ResultOutput));
          TotalAccuracyVals += threadResult.Value.TimerResolution;
          TotalNetAccuracyVals += threadResult.Value.StopwatchResolution;
        }
        // The sum of all latencies divided by the amount of checks
        double AverageAccuracy = TotalAccuracyVals / fResultStrings.Count;
        double AverageNetAccuracy = TotalNetAccuracyVals / fResultStrings.Count;

        lblMaxAccurary.Text = fMaxAccuracy.ToString("N9");        
        lblAverageAccuracy.Text = AverageAccuracy.ToString("N9");
        lblAverageNetAccuracy.Text = AverageNetAccuracy.ToString("N9");

        lblMaxDesc.Visible = lblMaxAccurary.Text.Length > 0;
        lblAvgDesc.Visible = lblAverageAccuracy.Text.Length > 0;
        lblAvgNetDesc.Visible = lblAverageNetAccuracy.Text.Length > 0;

        lblIsHighRes.Visible = Stopwatch.IsHighResolution;

        buttonStart.Enabled = true;
        listBoxResults.EndUpdate();
      }
    }

    #endregion
  }
}