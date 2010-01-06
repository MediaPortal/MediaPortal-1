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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Util;

namespace MediaPortal.MPInstaller
{
  public partial class InstallWizard : MPInstallerForm
  {
    public int step = 0;
    public MPpackageStruct package;
    private MPInstallHelper inst = new MPInstallHelper();
    private List<ActionInfo> actions = new List<ActionInfo>();

    private string InstallDir = Config.GetFolder(Config.Dir.Installer);
    private bool update = false;
    private bool working = false;

    public InstallWizard()
    {
      package = new MPpackageStruct();
      InitializeComponent();
    }

    public void starStep()
    {
      inst.LoadFromFile();
      if (!package.InstallerScript.Warning())
        return;
      if (package.InstallerScript.EnableWizard)
      {
        if (inst.IndexOf(package) < 0)
          nextStep(1);
        else if (MessageBox.Show("Extension already installed. Do you want to continue ?", "", MessageBoxButtons.YesNo) ==
                 DialogResult.Yes)
          nextStep(1);
      }
      else
      {
        package.InstallerScript.Install(null, null, null);
      }
    }

    public void StartUpdate()
    {
      update = true;
      inst.LoadFromFile();
      if (package.InstallerScript.EnableWizard)
      {
        nextStep(1);
      }
      else
      {
        package.InstallerScript.Install(null, null, null);
      }
    }

    /// <summary>
    /// Performe the next step.
    /// </summary>
    /// <param name="m">Step incrementer</param>
    public void nextStep(int m)
    {
      step += m;
      test_next_step(m);
      switch (step)
      {
        case 1:
          {
            try
            {
              if (package.InstallPlugin != null)
                package.InstallPlugin.OnStartInstall(ref package);
              package.InstallerScript.OnInstallStart();
            }
            catch (Exception) {}
            this.Text = "MediaPortal extension installer";
            skinlister.Items.Clear();
            Customize_list.Visible = false;
            button_back.Visible = false;
            progressBar1.Visible = false;
            progressBar2.Visible = false;
            skinlister.Visible = false;
            listBox1.Items.Clear();
            listBox1.Visible = false;
            label2.Visible = false;
            title_label.Text = package.InstallerInfo.Name;
            button_next.Text = "Next";
            richTextBox1.Visible = true;
            if (package.InstallerInfo.Logo != null)
            {
              pictureBox2.Visible = true;
              pictureBox2.Image = package.InstallerInfo.Logo;
            }
            else
            {
              pictureBox2.Visible = false;
            }
            richTextBox1.Text =
              String.Format("  Name : {0} \n\n  Author : {1} \n\n  Version : {2} \n\n  Description :\n {3} \n",
                            package.InstallerInfo.Name, package.InstallerInfo.Author, package.InstallerInfo.Version,
                            package.InstallerInfo.Description);
            foreach (string sk in package.SkinList)
            {
              if (package.InstalledSkinList.Contains(sk))
                skinlister.Items.Add(sk, true);
              else
                skinlister.Items.Add(sk, false);
            }
            foreach (string sk in package.InstalledSkinList)
            {
              if (!package.SkinList.Contains(sk))
                skinlister.Items.Add(sk, true);
            }
            if (!this.Visible) this.ShowDialog();
            break;
          }
        case 2:
          {
            label2.Visible = true;
            progressBar1.Visible = false;
            progressBar2.Visible = false;
            skinlister.Visible = false;
            listBox1.Visible = false;
            Customize_list.Visible = false;
            label2.Text = "License Agreement";
            button_next.Text = "I Agree";
            button_back.Visible = true;
            richTextBox1.Visible = true;
            richTextBox1.Text = package.txt_EULA;
            break;
          }
        case 3:
          {
            label2.Visible = true;
            progressBar1.Visible = false;
            progressBar2.Visible = false;
            skinlister.Visible = false;
            listBox1.Visible = false;
            Customize_list.Visible = false;
            label2.Text = "Change log";
            button_next.Text = "Next";
            button_back.Visible = true;
            richTextBox1.Visible = true;
            richTextBox1.Text = package.txt_log;
            break;
          }
        case 4:
          {
            progressBar1.Visible = false;
            progressBar2.Visible = false;
            skinlister.Visible = false;
            listBox1.Visible = false;
            Customize_list.Visible = false;
            label2.Visible = true;
            label2.Text = "Read me";
            button_back.Visible = true;
            richTextBox1.Visible = true;
            richTextBox1.Text = package.txt_readme;
            break;
          }
        case 5:
          {
            progressBar1.Visible = false;
            progressBar2.Visible = false;
            skinlister.Visible = true;
            listBox1.Visible = false;
            label2.Visible = true;
            Customize_list.Visible = false;
            label2.Text = "Select skin";
            button_next.Text = "Next";
            button_back.Visible = true;
            richTextBox1.Visible = false;
            skinlister.Items.Clear();
            foreach (string sk in package.SkinList)
            {
              if (package.InstalledSkinList.Contains(sk))
                skinlister.Items.Add(sk, true);
              else
                skinlister.Items.Add(sk, false);
            }
            foreach (string sk in package.InstalledSkinList)
            {
              if (!package.SkinList.Contains(sk))
                skinlister.Items.Add(sk, true);
            }
            break;
          }
        case 6:
          {
            progressBar1.Visible = false;
            progressBar2.Visible = false;
            skinlister.Visible = true;
            listBox1.Visible = false;
            label2.Visible = true;
            skinlister.Visible = false;
            Customize_list.Visible = true;
            label2.Text = "Customize setup";
            button_next.Text = "Next";
            button_back.Visible = true;
            richTextBox1.Visible = false;
            Customize_list.Visible = true;
            Customize_list.Items.Clear();
            foreach (GroupString gs in package.InstallerInfo.SetupGroups)
            {
              Customize_list.Items.Add(gs.Name, !package.InstallerInfo.ProjectProperties.SingleGroupSelect);
            }
            if (package.InstallerInfo.ProjectProperties.SingleGroupSelect && package.InstallerInfo.SetupGroups.Count > 0)
              Customize_list.SetItemChecked(0, true);
            break;
          }
        case 7:
          {
            progressBar1.Visible = false;
            progressBar2.Visible = false;
            skinlister.Visible = false;
            listBox1.Visible = false;
            Customize_list.Visible = false;
            label2.Visible = true;
            label2.Text = "Instaling ...";
            button_next.Text = "Next";
            button_back.Visible = true;
            richTextBox1.Visible = true;
            richTextBox1.Text = String.Format("Install paths : \n");
            foreach (Config.Dir option in Enum.GetValues(typeof (Config.Dir)))
            {
              richTextBox1.Text += String.Format("{0} - {1}\n", option, Config.GetFolder(option));
            }
            break;
          }
        case 8:
          {
            if (!this.Visible) this.ShowDialog();
            if (package.IsSkinPackage)
            {
              package.InstallableSkinList.AddRange(package.SkinList);
            }
            else
            {
              for (int i = 0; i < skinlister.Items.Count; i++)
              {
                if (skinlister.GetItemChecked(i))
                  package.InstallableSkinList.Add(skinlister.Items[i].ToString());
              }
            }
            for (int i = 0; i < Customize_list.Items.Count; i++)
            {
              package.InstallerInfo.SetupGroups[i].Checked = Customize_list.GetItemChecked(i);
            }
            progressBar1.Visible = false;
            progressBar2.Visible = false;
            skinlister.Visible = false;
            listBox1.Visible = false;
            label2.Visible = true;
            Customize_list.Visible = false;
            progressBar1.Visible = true;
            progressBar2.Visible = true;
            listBox1.Visible = true;
            ;
            label2.Text = "Instaling ...";
            button_next.Visible = false;
            button_back.Visible = true;
            richTextBox1.Text = "";
            richTextBox1.Visible = false;
            install();
            break;
          }
      }
    }

    private void install()
    {
      button_next.Visible = false;
      button_back.Visible = false;
      button_cancel.Enabled = false;
      if (progressBar1 != null)
      {
        progressBar1.Minimum = 0;
        progressBar1.Maximum = package.InstallerInfo.FileList.Count + 1;
      }

      package.InstallerScript.Install(progressBar2, progressBar1, listBox1);

      button_next.Visible = false;
      button_cancel.Enabled = true;
      inst.Add(package);
      inst.SaveToFile();
      label2.Text = "Done ...";
      ActionInfo ac = package.InstallerInfo.FindAction("POSTSETUP");
      if (ac != null)
      {
        actions.Add(ac);
        listBox1.Visible = false;
        skinlister.Items.Clear();
        skinlister.Visible = true;
        progressBar1.Visible = false;
        progressBar2.Visible = false;
        skinlister.Items.Add(ac.ToString());
      }
      button_cancel.Text = "Finish";
      try
      {
        if (package.InstallPlugin != null)
          package.InstallPlugin.OnEndInstall(ref package);
        if (package.InstallerInfo.ProjectProperties.ClearSkinCache)
        {
          Directory.Delete(Config.GetFolder(Config.Dir.Cache), true);
        }
      }
      catch (Exception) {}
      package.InstallerScript.OnInstallDone();
    }

    /// <summary>
    /// Test if the next step is need to be done.
    /// </summary>
    /// <param name="m">Step incrementer</param>
    private void test_next_step(int m)
    {
      switch (step)
      {
        case 1:
          break;
        case 2:
          if (update) step = 7;
          if (String.IsNullOrEmpty(package.txt_EULA))
          {
            step += m;
            test_next_step(m);
          }
          break;
        case 3:
          if (String.IsNullOrEmpty(package.txt_log))
          {
            step += m;
            test_next_step(m);
          }
          break;
        case 4:
          if (String.IsNullOrEmpty(package.txt_readme))
          {
            step += m;
            test_next_step(m);
          }
          break;
        case 5:
          if (!package.containsSkin || package.IsSkinPackage)
          {
            step += m;
            test_next_step(m);
          }
          break;
        case 6:
          if (package.InstallerInfo.SetupGroups.Count < 1)
          {
            step += m;
            test_next_step(m);
          }
          break;
        case 7:
          step += m;
          break;
        default:
          break;
      }
    }

    private void button_next_Click(object sender, EventArgs e)
    {
      nextStep(1);
    }

    private void button_cancel_Click(object sender, EventArgs e)
    {
      if (step == 8)
      {
        foreach (ActionInfo ac in actions)
        {
          int i = skinlister.Items.IndexOf(ac.ToString());
          if (skinlister.GetSelected(i))
            ac.ExecuteAction(package.InstallerInfo);
        }

        this.Close();
      }
      else
      {
        this.Close();
      }
    }

    private void button_back_Click(object sender, EventArgs e)
    {
      nextStep(-1);
    }

    public void uninstall(string tit)
    {
      inst.LoadFromFile();
      int index = -1;
      int ind = -1;
      foreach (MPpackageStruct p in inst.Items)
      {
        ind++;
        if (p.InstallerInfo.Name.Trim() == tit.Trim())
        {
          index = ind;
          break;
        }
      }
      if (index > -1)
      {
        if (!((MPpackageStruct)inst.Items[index]).InstallerScript.EnableWizard)
        {
          if (((MPpackageStruct)inst.Items[index]).InstallerScript.UnInstall())
          {
            inst.Items.RemoveAt(index);
            inst.SaveToFile();
          }
        }
        else
        {
          if (((MPpackageStruct)inst.Items[index]).InstallerInfo.Uninstall.Count > 0)
          {
            if (
              MessageBox.Show("Uninstalling extension." + tit + "\nDo you want continue ?", "", MessageBoxButtons.YesNo) ==
              DialogResult.Yes)
            {
              if (!this.Visible) this.Show();
              MPpackageStruct p = (MPpackageStruct)inst.Items[index];
              MPpackageStruct p_temp = new MPpackageStruct();
              if (File.Exists(InstallDir + @"\" + p.FileName))
                p_temp.LoadFromFile(InstallDir + @"\" + p.FileName);
              try
              {
                if (p_temp.InstallPlugin != null)
                  p_temp.InstallPlugin.OnStartUnInstall(ref p);
              }
              catch (Exception) {}
              label2.Visible = true;
              progressBar1.Visible = true;
              progressBar2.Visible = false;
              listBox1.Visible = true;
              this.Text = "Uninstalling " + p.InstallerInfo.Name;
              title_label.Text = p.InstallerInfo.Name;
              label2.Text = "Uninstalling ...";
              button_next.Visible = false;
              button_back.Visible = false;
              richTextBox1.Text = "";
              richTextBox1.Visible = false;
              progressBar1.Maximum = p.InstallerInfo.Uninstall.Count;
              for (int i = 0; i < p.InstallerInfo.Uninstall.Count; i++)
              {
                UninstallInfo u = (UninstallInfo)p.InstallerInfo.Uninstall[i];
                progressBar1.Value++;
                progressBar1.Update();
                progressBar1.Refresh();
                if (System.IO.File.Exists(u.Path))
                {
                  if (System.IO.File.GetCreationTime(u.Path) == u.Date)
                  {
                    try
                    {
                      System.IO.File.Delete(u.Path);
                      listBox1.Items.Add(u.Path);
                      listBox1.Update();
                      listBox1.Refresh();
                      this.Refresh();
                      this.Update();
                    }
                    catch (Exception) {}
                  }
                  else
                    listBox1.Items.Add("File date changed :" + u.Path);
                }
                else listBox1.Items.Add("File not found :" + u.Path);
              }
              inst.Items.RemoveAt(index);
              inst.SaveToFile();
              if (p_temp.InstallPlugin != null)
              {
                try
                {
                  if (p_temp.InstallPlugin != null)
                    p_temp.InstallPlugin.OnEndUnInstall(ref p);
                }
                catch (Exception) {}
              }
            }
          }
          else
            MessageBox.Show("Uninstall information not found !");
        }
      }
      else
        MessageBox.Show("Uninstall information not found !");

      button_cancel.Text = "Finish";
    }

    private void Customize_list_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      if (working) return;
      working = true;
      if (package.InstallerInfo.ProjectProperties.SingleGroupSelect)
      {
        for (int i = 0; i < Customize_list.Items.Count; i++)
          Customize_list.SetItemChecked(i, false);
      }
      Customize_list.SetItemChecked(e.Index, true);
      working = false;
    }
  }
}