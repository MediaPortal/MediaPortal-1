using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MpeCore;

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
    }
}
