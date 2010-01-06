using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CSScriptLibrary;


namespace MpeMaker.Dialogs
{
  public partial class EditScript : Form
  {
    public string Script
    {
      get { return textBox_code.Text; }
      set { textBox_code.Text = value; }
    }

    public EditScript()
    {
      InitializeComponent();
    }

    private void validateToolStripMenuItem_Click(object sender, EventArgs e)
    {
      textBox_error.Text = "";
      try
      {
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        CSScript.AssemblyResolvingEnabled = true;
        AsmHelper script =
          new AsmHelper(CSScriptLibrary.CSScript.LoadCode(textBox_code.Text, Path.GetTempFileName(), true));
        MessageBox.Show("No error");
      }
      catch (Exception ex)
      {
        textBox_error.Text = ex.Message;
      }
    }
  }
}