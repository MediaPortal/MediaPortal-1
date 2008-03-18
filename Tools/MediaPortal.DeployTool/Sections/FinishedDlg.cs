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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;

namespace MediaPortal.DeployTool
{
  public partial class FinishedDlg : DeployDialog, IDeployDialog
  {
    public FinishedDlg()
    {
      InitializeComponent();
      type=DialogType.Finished;
      UpdateUI();
    }

    #region IDeplayDialog interface
    public override void UpdateUI()
    {
      labelHeading1.Text = Localizer.Instance.GetString("Finished_labelHeading1");
      labelHeading2.Text = Localizer.Instance.GetString("Finished_labelHeading2");
      labelHeading3.Text = Localizer.Instance.GetString("Finished_labelHeading3");
      linkHomepage.Text = Localizer.Instance.GetString("Finished_linkHomepage");
      linkForum.Text = Localizer.Instance.GetString("Finished_linkForum");
      linkWiki.Text = Localizer.Instance.GetString("Finished_linkWiki");
      labelEbay.Text = Localizer.Instance.GetString("Finished_labelEbay");
    }
    public override DeployDialog GetNextDialog()
    {
      return null;
    }
    public override bool SettingsValid()
    {
      return false;
    }
    public override void SetProperties()
    {
    }
    #endregion

    #region Hyperlink handler
    private void OpenURL(string url)
    {
      System.Diagnostics.Process.Start(url);
    }
    private void linkHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      OpenURL("http://www.team-mediaportal.com");
    }
    private void linkForum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      OpenURL("http://forum.team-mediaportal.com");
    }
    private void linkWiki_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
    {
      OpenURL("http://wiki.team-mediaportal.com");
    }
    #endregion

    private void label2_Click(object sender, EventArgs e)
    {

    }
  }
}
