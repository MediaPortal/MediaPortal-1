using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;

namespace MpeInstaller.Controls
{
    public partial class ExtensionListControl : UserControl
    {
        public ExtensionListControl()
        {
            InitializeComponent();
            SelectedItem = null;
        }

        public ExtensionControl SelectedItem { get; set; }

        public void Set(ExtensionCollection collection)
        {
            foreach (PackageClass item in collection.Items)
            {
                flowLayoutPanel1.Controls.Add(new ExtensionControl(item));
            }
        }

        private void flowLayoutPanel1_Click(object sender, EventArgs e)
        {
       }

    }
}
