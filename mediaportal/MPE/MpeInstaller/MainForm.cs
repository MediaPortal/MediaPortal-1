using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeInstaller.Controls;
using MpeInstaller.Dialogs;

namespace MpeInstaller
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            extensionListControl.UnInstallExtension += extensionListControl_UnInstallExtension;
        }

        void extensionListControl_UnInstallExtension(object sender, PackageClass packageClass)
        {
            UnInstall dlg = new UnInstall();
            dlg.Execute(packageClass);
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MpeCore.MpeInstaller.Init();
            extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {

                MpeCore.MpeInstaller.Init();
                PackageClass pak = new PackageClass();
                pak = pak.ZipProvider.Load(dialog.FileName);
                if (pak.CheckDependency(false))
                {
                    pak.StartInstallWizard();
                    extensionListControl.Set(MpeCore.MpeInstaller.InstalledExtensions);
                }
            }
        }
    }
}
