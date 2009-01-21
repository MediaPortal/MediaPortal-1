#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
            download_form dw = new download_form(remoteFile, temp_file);
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
                    ListViewItem item1 = new ListViewItem(pk._intalerStruct.Name, 0);
                    if (pk._intalerStruct.Logo != null)
                    {
                        imageList1.Images.Add(pk._intalerStruct.Logo);
                        item1.ImageIndex = imageList1.Images.Count - 1;
                    }
                    item1.ToolTipText = pk._intalerStruct.Description;
                    //item1.SubItems.Add(pk._intalerStruct.Author);
                    item1.SubItems.Add(pk._intalerStruct.Version);
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
                    download_form dw1 = new download_form(pk._intalerStruct.UpdateURL, temp_file);
                    dw1.Text = pk._intalerStruct.UpdateURL + "/" +  pk._intalerStruct.Version;
                    dw1.ShowDialog();
                    if (File.Exists(temp_file))
                    {
                        wizard_1 wiz = new wizard_1();
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
