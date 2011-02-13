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

namespace MPTail
{
  public partial class frmSearchParams : Form
  {
    public frmSearchParams(string caption, SearchParameters searchParams)
    {
      InitializeComponent();
      Text = caption;
      edSearch.Text = searchParams.searchStr;
      edColor.BackColor = searchParams.highlightColor;
      cbCase.Checked = searchParams.caseSensitive;
    }

    public void GetConfig(SearchParameters searchParams)
    {
      searchParams.searchStr = edSearch.Text;
      searchParams.highlightColor = edColor.BackColor;
      searchParams.caseSensitive = cbCase.Checked;
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void btnChooseColor_Click(object sender, EventArgs e)
    {
      colorDialog1.Color = edColor.BackColor;
      if (colorDialog1.ShowDialog() == DialogResult.OK)
        edColor.BackColor = colorDialog1.Color;
    }
  }
}