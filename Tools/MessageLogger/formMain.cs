using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WindowsUtilities;

namespace MessageLogger
{
  public partial class MainForm : Form
  {
    bool _listening = false;

    public MainForm()
    {
      InitializeComponent();
    }

    protected override void WndProc(ref Message m)
    {
      try
      {
        if (_listening)
        {
          string msgtext = Enum.GetName(typeof(WindowsMessages), m.Msg);
          if (!listBoxBlacklist.Items.Contains(msgtext))
          {
            ListViewItem entry = new ListViewItem(string.Format("Time: {3} - Msg: {0} / Text: {4} , WParam: {1} LParam: {2}", m.Msg, m.WParam.ToString(), m.LParam.ToString(), DateTime.Now.ToString(), msgtext));
            listBoxLog.Items.Add(entry.Text);
          }
        }
      }
      catch (Exception) { }

      base.WndProc(ref m);
    }

    private void btnListen_Click(object sender, EventArgs e)
    {
      _listening = !_listening;

      btnListen.Text = _listening ? "Stop" : "Listen";
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
      listBoxLog.Items.Clear();
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        Clipboard.SetDataObject(listBoxLog.Items[listBoxLog.SelectedIndex].ToString(), true);
      }
      catch (Exception) { }
    }

    private void copyAllToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {        
        StringBuilder text = new StringBuilder();
        foreach (string itemText in listBoxLog.Items)
        {
          text.AppendLine(itemText);
        }
        Clipboard.SetDataObject(text.ToString(), true);
      }
      catch (Exception) { }
    }
  }
}