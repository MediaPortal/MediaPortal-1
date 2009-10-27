using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;

namespace MpeMaker.Dialogs
{
    public partial class InstalledExtensionsSelector : Form
    {
        public InstalledExtensionsSelector()
        {
            InitializeComponent();
            Result = null;
        }

        public PackageClass Result { get; set; }

        private void InstalledExtensionsSelector_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            foreach (var extension in MpeInstaller.InstalledExtensions.Items)
            {
                var item = new ListViewItem(extension.GeneralInfo.Id) {Tag = extension};
                item.SubItems.Add(extension.GeneralInfo.Name);
                item.SubItems.Add(extension.GeneralInfo.Version.ToString());
                listView1.Items.Add(item);
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                DialogResult = DialogResult.OK;
                Result = listView1.SelectedItems[0].Tag as PackageClass;
                Close();
            }
        }

    }
}
