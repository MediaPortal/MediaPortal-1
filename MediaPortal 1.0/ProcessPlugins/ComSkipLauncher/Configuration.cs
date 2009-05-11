using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using MediaPortal.GUI.Library;

namespace ProcessPlugins.ComSkipLauncher
{

  public partial class Configuration : MediaPortal.UserInterface.Controls.MPConfigForm
  {

    #region Constants

    const string ParametersMessage =
@"{0} = Recorded filename (includes path)
{1} = Recorded filename (w/o path)
{2} = Recorded filename (w/o path or extension)
{3} = Recorded file path
{4} = Current date
{5} = Current time
{6} = Channel name";

    #endregion Constants

    #region Properties

    public bool RunAtStart
    {
      get
      {
        return radioButtonStart.Checked;
      }
      set
      {
        radioButtonStart.Checked  = value;
        radioButtonEnd.Checked    = !value;
      }
    }
    public string Program
    {
      get { return textBoxProgram.Text; }
      set { textBoxProgram.Text = value; }
    }
    public string Parameters
    {
      get { return textBoxParameters.Text; }
      set { textBoxParameters.Text = value; }
    }

    #endregion Properties

    #region Constructors

    public Configuration()
    {
      InitializeComponent();
    }

    #endregion Constructors

    private void buttonOK_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close();
    }
    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void buttonParamQuestion_Click(object sender, EventArgs e)
    {
      MessageBox.Show(this, ParametersMessage, "Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void buttonTest_Click(object sender, EventArgs e)
    {
      string program = textBoxProgram.Text.Trim();
      string param = textBoxParameters.Text;

      if (program.Length == 0)
      {
        MessageBox.Show(this, "You must specify a program to run", "Missing program name", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return;
      }

      try
      {
        string parameters = ComSkipLauncher.ProcessParameters(param, textBoxTest.Text, "test");

        ComSkipLauncher.LaunchProcess(program, parameters, Path.GetDirectoryName(program), ProcessWindowStyle.Normal);
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher plugin: {0}", ex.Message);
      }
    }
    private void buttonFindTestFile_Click(object sender, EventArgs e)
    {
      openFileDialog.Title = "Select Test File";
      if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        textBoxTest.Text = openFileDialog.FileName;
    }
    private void buttonProgram_Click(object sender, EventArgs e)
    {
      openFileDialog.Title = "Select Program To Execute";
      if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        textBoxProgram.Text = openFileDialog.FileName;
    }

  }

}
