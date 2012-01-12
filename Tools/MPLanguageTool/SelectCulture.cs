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
using System.Globalization;

namespace MPLanguageTool
{
  public partial class SelectCulture : Form
  {
    public SelectCulture()
    {
      InitializeComponent();

      CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
      Array.Sort(allCultures, CompareCultureInfo);
      cbCulture.Items.AddRange(allCultures);

      CultureInfo current = System.Threading.Thread.CurrentThread.CurrentCulture;
      if ((current.CultureTypes & CultureTypes.SpecificCultures) != 0 &&
          frmMain.LangType != frmMain.StringsType.DeployTool)
      {
        // Select neutral culture like "it"
        cbCulture.SelectedItem = current.Parent;
      }
      else
      {
        // Select specific culture like "it-IT"
        cbCulture.SelectedItem = current;
      }
    }

    public CultureInfo GetSelectedCulture()
    {
      return (CultureInfo)cbCulture.SelectedItem;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private static int CompareCultureInfo(CultureInfo x, CultureInfo y)
    {
      return x.DisplayName.CompareTo(y.DisplayName);
    }
  }
}