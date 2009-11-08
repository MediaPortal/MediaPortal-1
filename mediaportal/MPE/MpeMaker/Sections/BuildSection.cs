using System;
using System.IO;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;

namespace MpeMaker.Sections
{
    public partial class BuildSection : UserControl,ISectionControl
    {
        public PackageClass Package { get; set; }
        private bool _loading = false;

        public BuildSection()
        {
            InitializeComponent();
        }

        #region ISectionControl Members

        public void Set(PackageClass pak)
        {
            _loading = true;
            Package = pak;
            txt_outfile.Text = pak.GeneralInfo.Location;
            lbl_file.Text = Package.ReplaceInfo(txt_outfile.Text);
            _loading = false;
        }

        public PackageClass Get()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void btn_browse_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Mpe package file(*.mpe2)|*.mpe2|All files|*.*";
            if(saveFileDialog1.ShowDialog()==DialogResult.OK)
            {
                txt_outfile.Text = saveFileDialog1.FileName;
            }
        }

        private void btn_generate_Click(object sender, EventArgs e)
        {
            list_error.Items.Clear();
            list_message.Items.Clear();
            if (string.IsNullOrEmpty(txt_outfile.Text))
                list_error.Items.Add("No out file is specified");

            foreach (SectionParam item in Package.GeneralInfo.Params.Items)
            {
                if (item.ValueType == ValueTypeEnum.File && !string.IsNullOrEmpty(item.Value) && !File.Exists(item.Value))
                {
                    list_error.Items.Add(string.Format("Params ->{0} file not found", item.Name));
                }
            }

            foreach (GroupItem groupItem in Package.Groups.Items)
            {
                foreach (FileItem fileItem in groupItem.Files.Items )
                {
                    ValidationResponse resp =
                        MpeInstaller.InstallerTypeProviders[fileItem.InstallType].Validate(fileItem);
                    if (!resp.Valid)
                        list_error.Items.Add(string.Format("[{0}][{1}] - {2}", groupItem.Name, fileItem, resp.Message));
                }
            }

            foreach (SectionItem sectionItem in Package.Sections.Items)
            {
              if(!string.IsNullOrEmpty(sectionItem.ConditionGroup) && Package.Groups[sectionItem.ConditionGroup]==null)
                  list_error.Items.Add(string.Format("[{0}] condition group not found [{1}]", sectionItem.Name, sectionItem.ConditionGroup));
                foreach (ActionItem actionItem in sectionItem.Actions.Items)
                {
                    ValidationResponse resp = MpeInstaller.ActionProviders[actionItem.ActionType].Validate(Package,
                                                                                                           actionItem);
                    if (!resp.Valid)
                        list_error.Items.Add(string.Format("[{0}][{1}] - {2}", sectionItem.Name, actionItem.Name, resp.Message));
  
                }
            }

            if(list_error.Items.Count>0)
            {
                tabControl1.SelectTab(1);
                return;
            }
            else
            {
                tabControl1.SelectTab(0);                
            }
            list_message.Items.Add("Creating package started at : "+DateTime.Now.ToLongTimeString());
            list_message.Refresh();
            Refresh();
            MpeInstaller.ZipProvider.Save(Package, Package.ReplaceInfo(txt_outfile.Text));
            list_message.Items.Add("Ended at : " + DateTime.Now.ToLongTimeString());
        }

        /// <summary>
        /// Handles the TextChanged event of the txt_outfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void txt_outfile_TextChanged(object sender, EventArgs e)
        {
            lbl_file.Text = Package.ReplaceInfo(txt_outfile.Text);
            if (_loading)
                return;
            Package.GeneralInfo.Location = txt_outfile.Text;
        }
    }
}
