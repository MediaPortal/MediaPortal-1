using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
//using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.Util;
using MediaPortal.EPG;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.EPG.TestWebEPG
{
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
			System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            WebEPG epg = new WebEPG();
            epg.Import();

        }
    }
}