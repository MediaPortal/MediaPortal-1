#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.Windows.Forms;

#pragma warning disable 108

namespace SetupTv.Sections
{
  public partial class Project : SectionSettings
  {
    public Project()
      : this("Project") { }

    public Project(string name)
      : base(name)
    {
      InitializeComponent();
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
        System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(linkLabelOnlineDocumentation.Text);
        System.Diagnostics.Process.Start(sInfo);
      }
    }

    private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelSourceforge.Text == null)
        return;
      if (linkLabelSourceforge.Text.Length > 0)
      {
        System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(linkLabelSourceforge.Text);
        System.Diagnostics.Process.Start(sInfo);
      }
    }

    private void linkLabelPayPal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      if (linkLabelPayPal.Text == null)
        return;
      if (linkLabelPayPal.Text.Length > 0)
      {
        System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(linkLabelPayPal.Text);
        System.Diagnostics.Process.Start(sInfo);
      }
    }
  }
}
