using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;

namespace MpeMaker.Sections
{
    public partial class BuildSection : UserControl,ISectionControl
    {
        public PackageClass Package { get; set; }

        public BuildSection()
        {
            InitializeComponent();
        }

        #region ISectionControl Members

        public void Set(PackageClass pak)
        {
            Package = pak;
        }

        public MpeCore.PackageClass Get()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void btn_browse_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog()==DialogResult.OK)
            {
                txt_outfile.Text = saveFileDialog1.FileName;
            }
        }

        private void btn_generate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_outfile.Text))
                list_error.Items.Add("No out file is specified");
            foreach (GroupItem groupItem in this.Package.Groups.Items)
            {
                foreach (FileItem fileItem in groupItem.Files.Items )
                {
                    ValidationResponse resp =
                        MpeInstaller.InstallerTypeProviders[fileItem.InstallType].Validate(fileItem);
                    if (!resp.Valid)
                        list_error.Items.Add(string.Format("[{0}][{1}] - {2}", groupItem.Name, fileItem, resp.Message));
                }
            }
            if(list_error.Items.Count>0)
            {
                tabControl1.SelectTab(1);
                return;
            }
            MpeInstaller.ZipProvider.Save(Package, txt_outfile.Text);
        }
    }
}
