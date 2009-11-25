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
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) && !string.IsNullOrEmpty(GetFolder((string[])e.Data.GetData(DataFormats.FileDrop))))
                e.Effect = DragDropEffects.All;
        }

        private void txt_skinpath_DragDrop(object sender, DragEventArgs e)
        {
            txt_skinpath.Text = GetFolder((string[]) e.Data.GetData(DataFormats.FileDrop));
        }

        private string GetFolder(string[] files)
        {
            if (files.Length < 1)
                return "";
            string dir = files[0];
            if(File.Exists(dir))
            {
                dir = Path.GetDirectoryName(dir);
            }
            if(Directory.Exists(dir))
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
            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if(files.Length>0)
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
           if(string.IsNullOrEmpty(txt_skinpath.Text))
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
            //--skin folder 
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
            //-------------
            //fonts-------
            if (!string.IsNullOrEmpty(txt_font1.Text + txt_font2.Text + txt_font3.Text))
            {
                GroupItem fontGroup = new GroupItem("Font files");
                if (!string.IsNullOrEmpty(txt_font1.Text) && File.Exists(txt_font1.Text))
                {
                    FileItem fileitem = new FileItem
                                            {
                                                DestinationFilename =
                                                    "%Skin%\\" + fontName + "\\Fonts" + Path.GetFileName(txt_font1.Text),
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
                                                    "%Skin%\\" + fontName + "\\Fonts" + Path.GetFileName(txt_font2.Text),
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
                                                    "%Skin%\\" + fontName + "\\Fonts" + Path.GetFileName(txt_font3.Text),
                                                InstallType = "CopyFont",
                                                LocalFileName = Path.GetFullPath(txt_font3.Text)
                                            };
                    fontGroup.Files.Add(fileitem);
                }

                Package.Groups.Add(fontGroup);
            }
            //-----------
            //-- install sections
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
            //------------------
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



    }
}
