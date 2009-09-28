using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;

namespace MpeInstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {

                MpeCore.MpeInstaller.Init();
                PackageClass pak = MpeCore.MpeInstaller.ZipProvider.Load(dialog.FileName);
                pak.StartInstallWizard();
            }

            //Application.Run(new Form1());
        }
    }
}
