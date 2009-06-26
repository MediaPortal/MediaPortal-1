using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using MediaPortal.Configuration;
using CSScriptLibrary;

namespace MediaPortal.MPInstaller
{
  public partial class ScriptEditorForm : MPInstallerForm
  {
    public ScriptEditorForm()
    {
      InitializeComponent();
    }

    public void Reset()
    {
      textBox_erro.Text = "";
      textBox_code.Text = richTextBox1.Text;
      textBox_code.ProcessAllLines();
    }

    private void ScriptEditorForm_Load(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(textBox_code.Text))
      {
        textBox_code.Text = richTextBox1.Text;
      }

      // Add the keywords to the list.
      textBox_code.Settings.Keywords.Add("function");
      textBox_code.Settings.Keywords.Add("if");
      textBox_code.Settings.Keywords.Add("else");
      textBox_code.Settings.Keywords.Add("void");
      textBox_code.Settings.Keywords.Add("using");
      textBox_code.Settings.Keywords.Add("public");
      textBox_code.Settings.Comment = "//";

      // Set the colors that will be used.
      textBox_code.Settings.KeywordColor = Color.Blue;
      textBox_code.Settings.CommentColor = Color.Green;
      textBox_code.Settings.StringColor = Color.Gray;
      textBox_code.Settings.IntegerColor = Color.Red;

      // Let's not process strings and integers.
      textBox_code.Settings.EnableStrings = false;
      textBox_code.Settings.EnableIntegers = false;

      // Let's make the settings we just set valid by compiling
      // the keywords to a regular expression.
      textBox_code.CompileKeywords();
      textBox_code.ProcessAllLines();

    }

    private void testToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
        AsmHelper script = new AsmHelper(CSScriptLibrary.CSScript.LoadCode(textBox_code.Text, Path.GetTempFileName(), true));
        MPInstallerScript scr = (MPInstallerScript)script.CreateObject("InstallScript");
        textBox_erro.Text = "";
        textBox_code.ProcessAllLines();
      }
      catch (Exception ex)
      {
        textBox_erro.Text = ex.Message;
      }
    }

    private void ScriptEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      e.Cancel = true;
      this.Hide();
    }

    private void textBox_erro_TextChanged(object sender, EventArgs e)
    {

    }

    private void resetToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show(string.Format("Do you want to continue ?"), "", MessageBoxButtons.YesNo) == DialogResult.Yes)
      {
        textBox_code.Text = richTextBox1.Text;
        textBox_erro.Text = "";
      }
    }
  }
}