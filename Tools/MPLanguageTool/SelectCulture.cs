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
      CultureInfo[] cinfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
      foreach (CultureInfo ci in cinfos)
      {
        cbCulture.Items.Add(ci);
      }
      CultureInfo current = System.Threading.Thread.CurrentThread.CurrentCulture;
      if ((current.CultureTypes & CultureTypes.SpecificCultures) != 0 && frmMain.LangType != frmMain.StringsType.DeployTool)
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
  }
}