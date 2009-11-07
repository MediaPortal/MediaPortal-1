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
        public event UnInstallExtensionHandler UnInstallExtension;
        public delegate void UnInstallExtensionHandler(object sender, PackageClass packageClass);

        public event UpdateExtensionHandler UpdateExtension;

        public delegate void UpdateExtensionHandler(
            object sender, PackageClass packageClass, PackageClass newpackageClass);

        public event ConfigureExtensionHandler ConfigureExtension;
        public delegate void ConfigureExtensionHandler(object sender, PackageClass packageClass);


        public ExtensionListControl()
        {
            InitializeComponent();
            SelectedItem = null;
        }

        public ExtensionControl SelectedItem { get; set; }

        public void Set(ExtensionCollection collection)
        {
            flowLayoutPanel1.Controls.Clear();
            foreach (PackageClass item in collection.Items)
            {
                flowLayoutPanel1.Controls.Add(new ExtensionControl(item));
            }
        }

        private void flowLayoutPanel1_Click(object sender, EventArgs e)
        {
        }

        public void OnUninstallExtension(ExtensionControl control)
        {
            if (UnInstallExtension != null)
                UnInstallExtension(control, control.Package);
        }

        
        public void OnUpdateExtension(ExtensionControl control)
        {
            if (UpdateExtension != null)
                UpdateExtension(control, control.Package, control.UpdatePackage);
        }

        public void OnConfigureExtension(ExtensionControl control)
        {
            if (ConfigureExtension != null)
                ConfigureExtension(control, control.Package);
        }

    }
}
