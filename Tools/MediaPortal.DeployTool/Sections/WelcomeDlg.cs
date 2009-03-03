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
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace MediaPortal.DeployTool.Sections
{
  public partial class WelcomeDlg : DeployDialog
  {
    public WelcomeDlg()
    {
      InitializeComponent();
      type = DialogType.Welcome;

      //Default culture en-US is not in a resource file
      CultureInfo ci = new CultureInfo("en-US");
      SimpleCultureInfo sci = new SimpleCultureInfo("en-US", ci.NativeName);
      cbLanguage.Items.Add(sci);
      cbLanguage.SelectedItem = sci;
      //

      DirectoryInfo dir = new DirectoryInfo(Application.StartupPath);
      DirectoryInfo[] subDirs = dir.GetDirectories();
      foreach (DirectoryInfo d in subDirs)
      {
        try
        {
          ci = new CultureInfo(d.Name);
        }
        catch (Exception)
        {
          continue;
        }
        sci = new SimpleCultureInfo(d.Name, ci.NativeName);
        cbLanguage.Items.Add(sci);
        if (ci.Name == System.Threading.Thread.CurrentThread.CurrentCulture.Name)
          cbLanguage.SelectedItem = sci;
      }
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelHeading1.Text = Localizer.GetBestTranslation("Welcome_labelHeading1");
      labelHeading2.Text = Localizer.GetBestTranslation("Welcome_labelHeading2");
      labelHeading3.Text = Localizer.GetBestTranslation("Welcome_labelHeading3");
    }
    public override DeployDialog GetNextDialog()
    {
      DialogFlowHandler.Instance.ResetHistory();
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.DownloadOnly);
    }
    public override bool SettingsValid()
    {
      return true;
    }
    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("language", GetLanguageId());
    }
    #endregion

    private string GetLanguageId()
    {
      if (cbLanguage.SelectedIndex == -1) return "en-US";
      SimpleCultureInfo sci = (SimpleCultureInfo)cbLanguage.SelectedItem;
      return sci.name;
    }

    private void cbLanguage_SelectedIndexChanged(object sender, EventArgs e)
    {
      Localizer.SwitchCulture(GetLanguageId());
      if (ParentForm != null)
        ((IDeployDialog)ParentForm).UpdateUI();
      UpdateUI();

    }

  }
}