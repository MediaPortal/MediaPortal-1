using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TvEngine.PowerScheduler
{
  public partial class SelectProcessForm : Form
  {
    private string _selectedProcess = String.Empty;

    public SelectProcessForm()
    {
      InitializeComponent();
      LoadProcesses();
    }
    void LoadProcesses()
    {
      comboBox1.Items.Clear();
      foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
      {
        comboBox1.Items.Add(p.ProcessName);
      }
    }
    public String SelectedProcess
    {
      get { return _selectedProcess; }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (comboBox1.SelectedIndex != -1)
        _selectedProcess = (string) comboBox1.Items[comboBox1.SelectedIndex];
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}