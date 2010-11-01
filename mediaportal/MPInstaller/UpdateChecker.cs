#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.IO;
using System.Windows.Forms;
using System.Xml;

//using System.Data;

namespace MediaPortal.MPInstaller
{
  public partial class UpdateChecker : MPInstallerForm
  {
    private MPInstallHelper lst = new MPInstallHelper();
    private MPInstallHelper lst_online = new MPInstallHelper();
    private string remoteFile = MPinstallerStruct.DEFAULT_UPDATE_SITE + "/MPExtensionFileList.xml";

    public UpdateChecker()
    {
      InitializeComponent();
      lst.LoadFromFile();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    public void Check()
    {
      this.ShowDialog();
    }

    private void UpdateChecker_Shown(object sender, EventArgs e)
    {
      string file_name = "MPExtensionFileList.xml";
      string temp_file = Path.GetFullPath(Environment.GetEnvironmentVariable("TEMP")) + @"\" + file_name;
      DownloadForm dw = new DownloadForm(remoteFile, temp_file);
      dw.Text = "Download files list ...";
      dw.ShowDialog();
      if (File.Exists(temp_file))
      {
        try
        {
          lst_online.LoadFromFile(temp_file);
          lst_online.Compare(lst);
          LoadToListview(lst_online, listView1);
        }
        catch (XmlException)
        {
          MessageBox.Show("Xml format error.Update aborted !");
        }
        catch
        {
          MessageBox.Show("Unknow error.Update aborted !");
        }
      }
    }

    public void LoadToListview(MPInstallHelper mpih, ListView lv)
    {
      lv.Items.Clear();
      for (int i = 0; i < mpih.Items.Count; i++)
      {
        MPpackageStruct pk = (MPpackageStruct)mpih.Items[i];
        if (pk.isUpdated)
        {
          ListViewItem item1 = new ListViewItem(pk.InstallerInfo.Name, 0);
          if (pk.InstallerInfo.Logo != null)
          {
            imageList1.Images.Add(pk.InstallerInfo.Logo);
            item1.ImageIndex = imageList1.Images.Count - 1;
          }
          item1.ToolTipText = pk.InstallerInfo.Description;
          //item1.SubItems.Add(pk._intalerStruct.Author);
          item1.SubItems.Add(pk.InstallerInfo.Version);
          //item1.SubItems.Add(Path.GetFileName(pk.FileName));
          //item1.SubItems.Add(pk._intalerStruct.Group);
          lv.Items.AddRange(new ListViewItem[] {item1});
        }
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem it in listView1.Items)
      {
        MPpackageStruct pk = lst_online.Find(it.SubItems[0].Text);
        if (pk != null)
        {
          string file_name = pk.FileName;
          string temp_file = Path.GetFullPath(Environment.GetEnvironmentVariable("TEMP")) + @"\" + file_name;
          DownloadForm dw1 = new DownloadForm(pk.InstallerInfo.UpdateURL, temp_file);
          dw1.Text = pk.InstallerInfo.UpdateURL + "/" + pk.InstallerInfo.Version;
          dw1.ShowDialog();
          if (File.Exists(temp_file))
          {
            InstallWizard wiz = new InstallWizard();
            wiz.package.LoadFromFile(temp_file);
            if (wiz.package.isValid)
            {
              //wiz.nextStep(6);
              wiz.StartUpdate();
            }
            else
            {
              MessageBox.Show("Invalid package !");
            }
          }
        }
      }
      this.Close();
    }
  }
}