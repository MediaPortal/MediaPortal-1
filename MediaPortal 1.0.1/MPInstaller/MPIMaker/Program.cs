using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MediaPortal.MPInstaller;

namespace MPIMaker
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      string fil = string.Empty;
      if (args.Length > 0)
      {
        fil = args[0];
      }
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      if (Path.GetExtension(fil) == ".xmp")
      {
        Application.Run(new EditForm(Path.GetFullPath(fil)));
      }
      else
      {
        Application.Run(new EditForm());
      }
    }
  }
}