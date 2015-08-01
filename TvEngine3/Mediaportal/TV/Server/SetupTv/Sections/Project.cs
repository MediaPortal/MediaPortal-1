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
using Mediaportal.TV.Server.SetupControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Project : SectionSettings
  {
    public Project()
      : base("Project")
    {
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      labelVersion2.Text = versionInfo.FileVersion;
      labelVersion3.Visible = versionInfo.FileVersion.Length - versionInfo.FileVersion.LastIndexOf('.') > 2;
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (!string.IsNullOrEmpty(linkLabelHomepage.Text))
      {
        Process.Start(new ProcessStartInfo(linkLabelHomepage.Text));
      }
    }

    private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (!string.IsNullOrEmpty(linkLabelForums.Text))
      {
        Process.Start(new ProcessStartInfo(linkLabelForums.Text));
      }
    }

    private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (!string.IsNullOrEmpty(linkLabelOnlineDocumentation.Text))
      {
        Process.Start(new ProcessStartInfo(linkLabelOnlineDocumentation.Text));
      }
    }

    private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (!string.IsNullOrEmpty(linkLabelSourceforge.Text))
      {
        Process.Start(new ProcessStartInfo(linkLabelSourceforge.Text));
      }
    }

    private void paypalPictureBox_Click(object sender, System.EventArgs e)
    {
      try
      {
        Process.Start("http://www.team-mediaportal.com/donate.html");
      }
      catch
      {
      }
    }
  }
}