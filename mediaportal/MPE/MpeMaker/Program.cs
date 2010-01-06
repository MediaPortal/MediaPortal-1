using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MpeMaker.Classes;

namespace MpeMaker
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      if (args.Length > 0)
      {
        MainForm dlg = new MainForm(new ProgramArguments(args));
        if (!dlg.IsDisposed)
          Application.Run(dlg);
      }
      else
        Application.Run(new MainForm());
    }
  }
}