#region Copyright (C) 2005-2022 Team MediaPortal

// Copyright (C) 2005-2022 Team MediaPortal
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

using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
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

    public override void LoadSettings()
    {
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      string version = ConfigurationManager.AppSettings["version"];
      string[] strVersion = version.Split('-');
      if (System.IntPtr.Size == 8)
      {
        version = strVersion[0] + " x64";
      }
      if (strVersion.Length == 2)
      {
        version = version + " " + strVersion[1];
      }
      labelVersion2.Text = string.Format("{0} ({1})", version, versionInfo.FileVersion);
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelHomepage.Text == null)
      {
        return;
      }
      if (linkLabelHomepage.Text.Length > 0)
      {
        ProcessStartInfo sInfo = new ProcessStartInfo(linkLabelHomepage.Text);
        Process.Start(sInfo);
      }
    }

    private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelForums.Text == null)
      {
        return;
      }
      if (linkLabelForums.Text.Length > 0)
      {
        ProcessStartInfo sInfo = new ProcessStartInfo(linkLabelForums.Text);
        Process.Start(sInfo);
      }
    }

    private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelOnlineDocumentation.Text == null)
      {
        return;
      }
      if (linkLabelOnlineDocumentation.Text.Length > 0)
      {
        ProcessStartInfo sInfo = new ProcessStartInfo(linkLabelOnlineDocumentation.Text);
        Process.Start(sInfo);
      }
    }

    private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelSourceforge.Text == null)
      {
        return;
      }
      if (linkLabelSourceforge.Text.Length > 0)
      {
        ProcessStartInfo sInfo = new ProcessStartInfo(linkLabelSourceforge.Text);
        Process.Start(sInfo);
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