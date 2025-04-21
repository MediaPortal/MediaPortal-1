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

using System.Diagnostics;
using System.Windows.Forms;

#pragma warning disable 108

namespace SetupTv.Sections
{
  public partial class Project : SectionSettings
  {
    public Project()
      : this("Project") {}

    public Project(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      labelVersion2.Text = versionInfo.FileVersion + (System.IntPtr.Size == 8 ? " | x64 |" : " | x86 |");
      labelVersion3.Visible = versionInfo.FileVersion.Length - versionInfo.FileVersion.LastIndexOf('.') > 2;
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelHomepage.Text == null)
        return;
      if (linkLabelHomepage.Text.Length > 0)
      {
        System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(linkLabelHomepage.Text);
        System.Diagnostics.Process.Start(sInfo);
      }
    }

    private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelForums.Text == null)
        return;
      if (linkLabelForums.Text.Length > 0)
      {
        System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(linkLabelForums.Text);
        System.Diagnostics.Process.Start(sInfo);
      }
    }

    private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelOnlineDocumentation.Text == null)
        return;
      if (linkLabelOnlineDocumentation.Text.Length > 0)
      {
        System.Diagnostics.ProcessStartInfo sInfo =
          new System.Diagnostics.ProcessStartInfo(linkLabelOnlineDocumentation.Text);
        System.Diagnostics.Process.Start(sInfo);
      }
    }

    private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelGithub.Text == null)
        return;
      if (linkLabelGithub.Text.Length > 0)
      {
        System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(linkLabelGithub.Text);
        System.Diagnostics.Process.Start(sInfo);
      }
    }

    private void paypalPictureBox_Click(object sender, System.EventArgs e)
    {
      try
      {
        Process.Start("http://www.team-mediaportal.com/donate.html");
      }
      catch {}
    }
  }
}