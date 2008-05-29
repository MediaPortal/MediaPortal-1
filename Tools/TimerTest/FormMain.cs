using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;

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

    protected delegate void MethodListViewItems(string aItem);
    protected delegate void MethodListViewClearer();

    private double fMaxAccuracy = 1;
    private object ExecLock = null;

    public FormMain()
    {
      InitializeComponent();
      ExecLock = new object();
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
      Invoke(new MethodListViewClearer(ThreadedListClearer));
      Thread ExecutionThread = new Thread(new ThreadStart(GetTimerFrequencyThread));
      ExecutionThread.IsBackground = true;
      ExecutionThread.Start();
    }

    unsafe private void GetTimerFrequencyThread()
    {
      try
      {
        for (int j = 0 ; j < numLoopCount.Value ; j++)
        {
          lock (ExecLock)
          {
            for (short i = 0 ; i < Environment.ProcessorCount ; i++)
            {
              Thread TimerThread = new Thread(new ParameterizedThreadStart(CheckTimer));
              TimerThread.Start((object)i);
              TimerThread.IsBackground = true;
            }
            SetThreadAffinityMask(GetCurrentThread(), new IntPtr(0));
          }
        }
      }
      catch (ThreadAbortException) { }
      catch (ThreadStateException) { }
    }

    unsafe private void CheckTimer(object aCpuNo)
    {
      try
      {
        short currentCpu = (short)aCpuNo;
        SetThreadAffinityMask(GetCurrentThread(), new IntPtr(1 << currentCpu));

        float MpAccuracy = DXUtil.Timer(DirectXTimer.GetAbsoluteTime);
        MpAccuracy = DXUtil.Timer(DirectXTimer.GetAbsoluteTime) - MpAccuracy;

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

        string ResultString = string.Format("CPU {0}: Ticks per second: {1} / MP accuracy: {2} / Approx. accurary in ms: {3}", currentCpu, TicksPerSec, MpAccuracy, SysAccuracyResult);

        // Let the GUI thread add our result to the listbox
        Invoke(new MethodListViewItems(ThreadedListItemAdder), new object[] { ResultString });
        Thread.Sleep(0);
      }
      catch (ThreadAbortException) { }
      catch (ThreadStateException) { }
    }

    private void ThreadedListItemAdder(string aItem)
    {
      listBoxResults.BeginUpdate();
      listBoxResults.Items.Add(aItem);
      lblMaxAccurary.Text = fMaxAccuracy.ToString("N9");
      lblMaxDesc.Visible = lblMaxAccurary.Text.Length > 0;
      listBoxResults.EndUpdate();
    }

    private void ThreadedListClearer()
    {
      if (checkBoxClearValues.Checked)
        listBoxResults.Items.Clear();
    }
  }
}