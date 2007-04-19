using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PluginInstaller
{
    static public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static public void Main(string[] args)
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PluginInstaller(args));
        }
    }
}