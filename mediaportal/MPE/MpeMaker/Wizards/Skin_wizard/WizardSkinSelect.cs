using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;
using MpeCore.Classes.Project;
using MpeMaker.Wizards;

namespace MpeMaker.Wizards.Skin_wizard
{
  public partial class WizardSkinSelect : Form, IWizard
  {
    public PackageClass Package = new PackageClass();

    public WizardSkinSelect()
    {
      InitializeComponent();
    }

    private void btn_browse_skin_Click(object sender, EventArgs e)
    {
      folderBrowserDialog1.Description = "Select the skin folder ";
      folderBrowserDialog1.SelectedPath = txt_skinpath.Text;
      if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
      {
        if (!File.Exists(Path.Combine(folderBrowserDialog1.SelectedPath, "references.xml")))
        {
          MessageBox.Show("The skin folder should contain references.xml file !");
          return;
        }
        txt_skinpath.Text = folderBrowserDialog1.SelectedPath;
      }
    }

    public bool Execute(PackageClass packageClass)
    {
      Package = packageClass;
      if (ShowDialog() == DialogResult.OK)
      {
        return true;
      }
      return false;
    }

    private void txt_skinpath_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false) &&
          !string.IsNullOrEmpty(GetFolder((string[])e.Data.GetData(DataFormats.FileDrop))))
        e.Effect = DragDropEffects.All;
    }

    private void txt_skinpath_DragDrop(object sender, DragEventArgs e)
    {
      txt_skinpath.Text = GetFolder((string[])e.Data.GetData(DataFormats.FileDrop));
    }

    private string GetFolder(string[] files)
    {
      if (files.Length < 1)
        return "";
      string dir = files[0];
      if (File.Exists(dir))
      {
        dir = Path.GetDirectoryName(dir);
      }
      if (Directory.Exists(dir))
      {
        if (File.Exists(Path.Combine(dir, "references.xml")))
        {
          return dir;
        }
      }
      return "";
    }

    private void txt_skin_folder_DragDrop(object sender, DragEventArgs e)
    {
      string file = "";
      string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (files.Length > 0)
      {
        file = files[0];
      }
      if (sender == txt_font1)
        txt_font1.Text = file;
      if (sender == txt_font2)
        txt_font2.Text = file;
      if (sender == txt_font3)
        txt_font3.Text = file;
    }

    private void txt_skin_folder_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
        e.Effect = DragDropEffects.All;
    }

    private void btn_next1_Click(object sender, EventArgs e)
    {
      tabControl1.SelectedIndex++;
    }

    private void btn_done_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(txt_skinpath.Text))
      {
        MessageBox.Show("No skin folder specified !");
        return;
      }
      Package.GeneralInfo.Name = txt_name.Text;
      Package.GeneralInfo.Author = txt_author.Text;
      Package.GeneralInfo.Version = new VersionInfo()
                                      {
                                        Major = txt_version1.Text,
                                        Minor = txt_version2.Text,
                                        Build = txt_version3.Text,
                                        Revision = txt_version4.Text
                                      };
      string fontName = Path.GetFileName(txt_skinpath.Text);

      #region Skin 

      GroupItem skinGroup = new GroupItem("Skin files");
      FolderGroup skinfoldergroup = new FolderGroup()
                                      {
                                        DestinationFilename = "%Skin%\\" + fontName,
                                        Folder = txt_skinpath.Text,
                                        InstallType = "CopyFile",
                                        UpdateOption = UpdateOptionEnum.OverwriteIfOlder,
                                        Param1 = "",
                                        Recursive = true,
                                        Group = skinGroup.Name
                                      };
      Package.Groups.Add(skinGroup);
      Package.ProjectSettings.Add(skinfoldergroup);
      ProjectSettings.UpdateFiles(Package, skinfoldergroup);

      #endregion

      #region fonts 

      if (!string.IsNullOrEmpty(txt_font1.Text + txt_font2.Text + txt_font3.Text))
      {
        GroupItem fontGroup = new GroupItem("Font files");
        if (!string.IsNullOrEmpty(txt_font1.Text) && File.Exists(txt_font1.Text))
        {
          FileItem fileitem = new FileItem
                                {
                                  DestinationFilename =
                                    "%Skin%\\" + fontName + "\\Fonts\\" + Path.GetFileName(txt_font1.Text),
                                  InstallType = "CopyFont",
                                  LocalFileName = Path.GetFullPath(txt_font1.Text)
                                };
          fontGroup.Files.Add(fileitem);
        }

        if (!string.IsNullOrEmpty(txt_font2.Text) && File.Exists(txt_font2.Text))
        {
          FileItem fileitem = new FileItem
                                {
                                  DestinationFilename =
                                    "%Skin%\\" + fontName + "\\Fonts\\" + Path.GetFileName(txt_font2.Text),
                                  InstallType = "CopyFont",
                                  LocalFileName = Path.GetFullPath(txt_font2.Text)
                                };
          fontGroup.Files.Add(fileitem);
        }

        if (!string.IsNullOrEmpty(txt_font3.Text) && File.Exists(txt_font3.Text))
        {
          FileItem fileitem = new FileItem
                                {
                                  DestinationFilename =
                                    "%Skin%\\" + fontName + "\\Fonts\\" + Path.GetFileName(txt_font3.Text),
                                  InstallType = "CopyFont",
                                  LocalFileName = Path.GetFullPath(txt_font3.Text)
                                };
          fontGroup.Files.Add(fileitem);
        }

        Package.Groups.Add(fontGroup);
      }

      #endregion

      #region dlls %Plugins%

      if (!string.IsNullOrEmpty(txt_plugin_procces.Text + txt_plugin_window.Text + txt_plugin_exe.Text))
      {
        GroupItem dllGroup = new GroupItem("Plugin files");
        if (!string.IsNullOrEmpty(txt_plugin_procces.Text) && File.Exists(txt_plugin_procces.Text))
        {
          FileItem fileitem = new FileItem
                                {
                                  DestinationFilename =
                                    "%Plugins%\\process\\" + Path.GetFileName(txt_plugin_procces.Text),
                                  InstallType = "CopyFile",
                                  LocalFileName = Path.GetFullPath(txt_plugin_procces.Text)
                                };
          dllGroup.Files.Add(fileitem);
        }

        if (!string.IsNullOrEmpty(txt_plugin_window.Text) && File.Exists(txt_plugin_window.Text))
        {
          FileItem fileitem = new FileItem
                                {
                                  DestinationFilename =
                                    "%Plugins%\\Windows\\" + Path.GetFileName(txt_plugin_window.Text),
                                  InstallType = "CopyFile",
                                  LocalFileName = Path.GetFullPath(txt_plugin_window.Text)
                                };
          dllGroup.Files.Add(fileitem);
        }

        if (!string.IsNullOrEmpty(txt_plugin_exe.Text) && File.Exists(txt_plugin_exe.Text))
        {
          FileItem fileitem = new FileItem
                                {
                                  DestinationFilename =
                                    "%Base%\\" + Path.GetFileName(txt_plugin_exe.Text),
                                  InstallType = "CopyFile",
                                  LocalFileName = Path.GetFullPath(txt_plugin_exe.Text)
                                };
          dllGroup.Files.Add(fileitem);
        }

        Package.Groups.Add(dllGroup);
      }

      #endregion

      #region install sections

      Package.Sections.Add("Welcome Screen").WizardButtonsEnum = WizardButtonsEnum.NextCancel;
      Package.Sections.Add("Install Section").WizardButtonsEnum = WizardButtonsEnum.Next;
      var item = new ActionItem("InstallFiles")
                   {
                     Params =
                       new SectionParamCollection(
                       MpeInstaller.ActionProviders["InstallFiles"].GetDefaultParams())
                   };
      Package.Sections.Items[1].Actions.Add(item);
      Package.Sections.Add("Setup Complete").WizardButtonsEnum = WizardButtonsEnum.Finish;

      #endregion

      #region config

      if (radioButton1.Checked && File.Exists(txt_plugin_procces.Text))
        Package.GeneralInfo.Params[ParamNamesConst.CONFIG].Value = "%Plugins%\\process\\" +
                                                                   Path.GetFileName(txt_plugin_procces.Text);
      if (radioButton1.Checked && File.Exists(txt_plugin_window.Text))
        Package.GeneralInfo.Params[ParamNamesConst.CONFIG].Value = "%Plugins%\\Windows\\" +
                                                                   Path.GetFileName(txt_plugin_window.Text);
      if (radioButton1.Checked && File.Exists(txt_plugin_exe.Text))
        Package.GeneralInfo.Params[ParamNamesConst.CONFIG].Value = "%Base%\\" +
                                                                   Path.GetFileName(txt_plugin_exe.Text);
      if (File.Exists(txt_ico.Text))
      {
        string icofile = Path.GetFullPath(txt_ico.Text);
        Package.GeneralInfo.Params[ParamNamesConst.ICON].Value = icofile;
        Package.Sections.Items[0].Params[ParamNamesConst.SECTION_ICON].Value = icofile;
        Package.Sections.Items[1].Params[ParamNamesConst.SECTION_ICON].Value = icofile;
        Package.Sections.Items[2].Params[ParamNamesConst.SECTION_ICON].Value = icofile;
      }
      Package.GeneralInfo.Location = txt_mpe_folder.Text + "\\" + Package.GeneralInfo.Name + ".mpe1";

      #endregion

      Close();
    }

    private void btn_browse_font1_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "Font file (*.ttf)|*.ttf|All files(*.*)|*.*";
      openFileDialog1.Multiselect = false;
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        if (sender == btn_browse_font1)
          txt_font1.Text = openFileDialog1.FileName;
        if (sender == btn_browse_font2)
          txt_font2.Text = openFileDialog1.FileName;
        if (sender == btn_browse_font3)
          txt_font3.Text = openFileDialog1.FileName;
      }
    }

    private void btn_brows_dll1_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "Plugin file (*.dll)|*.dll|Executable (*.exe)|*.exe|All files(*.*)|*.*";
      openFileDialog1.Multiselect = false;
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        if (sender == btn_brows_dll1)
          txt_plugin_procces.Text = openFileDialog1.FileName;
        if (sender == btn_brows_dll2)
          txt_plugin_window.Text = openFileDialog1.FileName;
        if (sender == btn_brows_dll3)
          txt_plugin_exe.Text = openFileDialog1.FileName;
      }
    }

    private void txt_plugin_procces_DragDrop(object sender, DragEventArgs e)
    {
      string file = "";
      string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (files.Length > 0)
      {
        file = files[0];
      }
      if (sender == txt_plugin_procces)
        txt_plugin_procces.Text = file;
      if (sender == txt_plugin_window)
        txt_plugin_window.Text = file;
      if (sender == txt_plugin_exe)
        txt_plugin_exe.Text = file;
    }

    private void btn_browse_ico_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
      openFileDialog1.Multiselect = false;
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        txt_ico.Text = openFileDialog1.FileName;
      }
    }

    private void txt_ico_DragDrop(object sender, DragEventArgs e)
    {
      string file = "";
      string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (files.Length > 0)
      {
        file = files[0];
      }
      txt_ico.Text = file;
    }

    private void txt_skinpath_TextChanged(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(txt_name.Text))
        txt_name.Text = Path.GetFileName(txt_skinpath.Text);
      if (string.IsNullOrEmpty(txt_mpe_folder.Text))
        txt_mpe_folder.Text = Path.GetDirectoryName(txt_skinpath.Text);
    }

    private void btn_browse_folder_Click(object sender, EventArgs e)
    {
      folderBrowserDialog1.SelectedPath = txt_mpe_folder.Text;
      if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
      {
        txt_mpe_folder.Text = folderBrowserDialog1.SelectedPath;
      }
    }
  }
}