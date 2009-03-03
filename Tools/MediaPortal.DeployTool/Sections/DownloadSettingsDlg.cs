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
using System.Globalization;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.Sections
{
  public partial class DownloadSettingsDlg : DeployDialog
  {
    private bool arch64;

    public DownloadSettingsDlg()
    {
      InitializeComponent();
      type = DialogType.DownloadSettings;
      labelSectionHeader.Text = "";
      if (Utils.Check64bit())
      {
        b64bit.Image = Images.Choose_button_on;
        arch64 = true;
      }
      else
      {
        b32bit.Image = Images.Choose_button_on;
        arch64 = false;
      }

      CultureInfo[] cinfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
      foreach (CultureInfo ci in cinfos)
      {
        ListViewItem item = new ListViewItem(new string[] { ci.NativeName, ci.Name, ci.ThreeLetterWindowsLanguageName });
        listViewLang.Items.Add(item);
      }
      listViewLang.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
      listViewLang.Columns[2].Width = 0;
      listViewLang.Select();
      listViewLang.Items[0].Selected = true;
      listViewLang.Items[0].Focused = true;
      for (int i = 0; i < listViewLang.Items.Count; i++)
      {
        if (listViewLang.Items[i].SubItems[1].Text == CultureInfo.InstalledUICulture.Name)
        {
          listViewLang.Items[i].Selected = true;
          listViewLang.Items[i].Focused = true;
          listViewLang.Items[i].EnsureVisible();
          break;
        }
      }
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelSectionHeader.Text = Localizer.GetBestTranslation("DownloadSettings_labelSectionHeader");
    }
    public override DeployDialog GetNextDialog()
    {
#if DEBUG
      MessageBox.Show(String.Format("arch = {0}, langA = {1}, langB = {2}", arch64 ? "64" : "32", listViewLang.FocusedItem.SubItems[1].Text, listViewLang.FocusedItem.SubItems[2].Text));
#endif
      InstallationProperties.Instance.Set("DownloadArch", arch64 ? "64" : "32");
      InstallationProperties.Instance.Set("DownloadThreeLetterWindowsLanguageName", listViewLang.FocusedItem.SubItems[2].Text);
      InstallationProperties.Instance.Set("DownloadLanguageName", listViewLang.FocusedItem.SubItems[1].Text);
      InstallationProperties.Instance.Set("InstallType", "download_only");
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Installation);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

    
    private void b32bit_Click(object sender, EventArgs e)
    {
      b32bit.Image = Images.Choose_button_on;
      b64bit.Image = Images.Choose_button_off;
      arch64 = false;
    }

    private void b64bit_Click(object sender, EventArgs e)
    {
      b32bit.Image = Images.Choose_button_off;
      b64bit.Image = Images.Choose_button_on;
      arch64 = true;
    }
  }
}
