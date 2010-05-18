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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MPTvClient
{
  public partial class frmExternalPlayerConfig : Form
  {
    public frmExternalPlayerConfig()
    {
      InitializeComponent();
    }
    public void InitForm(string exe, string parameters, bool useOverride, string overrideUrl)
    {
      edExe.Text = exe;
      edParams.Text = parameters;
      cbURLOverride.Checked = useOverride;
      edVLCURL.Text = overrideUrl;
    }
    public void GetConfig(ref string exe, ref string parameters, ref bool useOverride,ref string overrideUrl)
    {
      exe = edExe.Text;
      parameters = edParams.Text;
      useOverride = cbURLOverride.Checked;
      overrideUrl = edVLCURL.Text;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
      DialogResult result = OpenDlg.ShowDialog();
      if (result == DialogResult.OK)
        edExe.Text = OpenDlg.FileName;
    }
  }
}