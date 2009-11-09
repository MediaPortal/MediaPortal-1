using System;
using System.IO;
using System.Windows.Forms;
using MpeCore.Classes;

namespace MpeMaker.Dialogs
{
    public partial class AddFolder2Group : Form
    {
        private GroupItem _groupItem;
        public AddFolder2Group(GroupItem groupItem)
        {
            InitializeComponent();
            _groupItem = groupItem;
        }

        private void add_folder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txt_folder.Text;
            if(folderBrowserDialog1.ShowDialog()==DialogResult.OK)
            {
                txt_folder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btn_add_template_Click(object sender, EventArgs e)
        {
            PathTemplateSelector dlg = new PathTemplateSelector();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txt_template.Text = dlg.Result +"\\"+ Path.GetFileName(txt_folder.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_folder.Text))
            {
                MessageBox.Show("Source folder not specified !");
                return;
            }

            if (string.IsNullOrEmpty(txt_template.Text))
            {
                MessageBox.Show("Template not specified !");
                return;
            }

            if (!Directory.Exists(txt_folder.Text))
            {
                MessageBox.Show("Folder not found !");
                return;
            }

            DirectoryInfo di = new DirectoryInfo(txt_folder.Text);
            FileInfo[] fileList;
            fileList = chk_recurs.Checked ? di.GetFiles("*.*", SearchOption.AllDirectories) : di.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            string dir = txt_folder.Text;
            string templ = txt_template.Text;
            if (!dir.EndsWith("\\"))
            {
                dir += "\\";
            }
            if (!templ.EndsWith("\\"))
            {
               templ += "\\";
            }

            foreach (FileInfo f in fileList)
            {
                if (!f.DirectoryName.Contains(".svn"))
                {
                    FileItem fileItem = new FileItem(f.FullName, false);
                    fileItem.DestinationFilename = f.FullName.Replace(dir, templ);
                    if (_groupItem.Files.Get(f.FullName, fileItem.DestinationFilename) != null)
                        continue;
                    _groupItem.Files.Add(fileItem);
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
