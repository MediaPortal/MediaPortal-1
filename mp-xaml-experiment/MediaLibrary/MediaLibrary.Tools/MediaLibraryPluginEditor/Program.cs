using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PluginDescriptionEditor
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
            Application.Run(new EditorForm(args));
        }
    }
}