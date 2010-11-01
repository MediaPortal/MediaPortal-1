#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

namespace WindowPlugins.VideoEditor
{
  public partial class VideoEditorConfiguration : MPConfigForm
  {
    public VideoEditorConfiguration()
    {
      InitializeComponent();
      LoadSettings();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      OpenFileDialog diag = new OpenFileDialog();
      diag.Filter = "exe-File (*.exe)|*.exe";
      try
      {
        diag.InitialDirectory = Path.GetDirectoryName(mencoderPath.Text);
      }
      catch (Exception) {}

      if (diag.ShowDialog() == DialogResult.OK)
      {
        if (File.Exists(diag.FileName))
        {
          mencoderPath.Text = diag.FileName;
        }
      }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("VideoEditor", "mencoder", mencoderPath.Text);
      }
    }

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        mencoderPath.Text = xmlreader.GetValueAsString("VideoEditor", "mencoder", String.Empty);
      }
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      LoadSettings();
      DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void linkLblMencoderHint_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Process.Start("http://www.mplayerhq.hu");
      }
      catch (Exception) {}
    }
  }
}