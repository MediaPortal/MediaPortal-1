using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TimerTest
{
  public partial class FormMain : Form
  {
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

    public FormMain()
    {
      InitializeComponent();
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
      if (checkBoxClearValues.Checked)
        listBoxResults.Items.Clear();
      GetTimerFrequency();
    }

    unsafe private void GetTimerFrequency()
    {
      for (int i = 0; i < Environment.ProcessorCount; i++)
      {
        SetThreadAffinityMask(GetCurrentThread(), new IntPtr(1 << i));

        long TicksPerSec = 0;
        if (!QueryPerformanceFrequency(ref TicksPerSec))
        {
          MessageBox.Show("This system does not support accurate timing!");
          return;
        }

        listBoxResults.Items.Add(string.Format("CPU {0}: Ticks per second: {1}", i, TicksPerSec));
      }
      SetThreadAffinityMask(GetCurrentThread(), new IntPtr(0));
    }
  }
}