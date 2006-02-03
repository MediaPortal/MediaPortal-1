using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MediaPortal.Tests.Support
{
  public class FileHelper
  {
    public static void Touch(string file)
    {
      IDisposable disposable = File.CreateText(file);
      disposable.Dispose();
    }
  }
}
