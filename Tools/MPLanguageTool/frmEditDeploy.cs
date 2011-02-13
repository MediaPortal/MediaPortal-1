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
using System.Windows.Forms;

namespace MPLanguageTool
{
  public partial class frmEditDeploy : Form
  {
    public frmEditDeploy()
    {
      InitializeComponent();
    }

    public DialogResult ShowDialog(string id, string translation, string defaultTranslation)
    {
      lID.Text = id;
      edValue.Text = translation;
      edDefault.Text = defaultTranslation;
      return ShowDialog();
    }

    public string GetTranslation()
    {
      if (edValue.Text == "")
      {
        return null;
      }
      return edValue.Text;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void frmEdit_Shown(object sender, EventArgs e)
    {
      edValue.Focus();
    }
  }
}