#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TvLibrary.Log;
using TvEngine;
using TvControl;
using TvDatabase;
using System.Collections.Generic;

namespace SetupTv.Sections
{
  [CLSCompliant(false)]
  public partial class ComSkipSetup : SetupTv.SectionSettings
  {
    #region Constants

    private const string ParametersMessage =
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
      get { return radioButtonStart.Checked; }
      set
      {
        radioButtonStart.Checked = value;
        radioButtonEnd.Checked = !value;
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

    public ProcessPriorityClass Priority
    {
      get { return (ProcessPriorityClass)comboBoxPriority.SelectedValue; }
      set { comboBoxPriority.SelectedValue = value; }
    }

    #endregion Properties

    #region Constructor

    public ComSkipSetup()
    {
      InitializeComponent();
    }

    #endregion Constructor

    #region SetupTv.SectionSettings

    public override void OnSectionDeActivated()
    {
      Log.Info("ComSkipLauncher: Configuration deactivated");

      ComSkipLauncher.RunAtStart = this.RunAtStart;
      ComSkipLauncher.Program = this.Program;
      ComSkipLauncher.Parameters = this.Parameters;
      ComSkipLauncher.Priority = this.Priority;

      ComSkipLauncher.SaveSettings();

      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      Log.Info("ComSkipLauncher: Configuration activated");

      ComSkipLauncher.LoadSettings();

      EnumCombo.PopulateCombo<ProcessPriorityClass>(comboBoxPriority);

      RunAtStart = ComSkipLauncher.RunAtStart;
      Program = ComSkipLauncher.Program;
      Parameters = ComSkipLauncher.Parameters;
      Priority = ComSkipLauncher.Priority;

      base.OnSectionActivated();
    }

    #endregion SetupTv.SectionSettings

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
        MessageBox.Show(this, "You must specify a program to run", "Missing program name", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
        return;
      }

      try
      {
        string parameters = ComSkipLauncher.ProcessParameters(param, textBoxTest.Text, "test");

        ComSkipLauncher.LaunchProcess(program, parameters, ProcessPriorityClass.Normal, Path.GetDirectoryName(program), ProcessWindowStyle.Normal);
      }
      catch (Exception ex)
      {
        Log.Error("ComSkipLauncher - Config Test: {0}", ex.Message);
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

    private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://www.kaashoek.com/comskip/");
    }
  }
}