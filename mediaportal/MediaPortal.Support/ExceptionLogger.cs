using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MediaPortal.Support
{
  public class ExceptionLogger : ILogCreator
  {
    private Exception exception;
    public ExceptionLogger(Exception exception)
    {
      this.exception = exception;
    }

    public void CreateLogs(string destinationFolder)
    {
      string filename = Path.GetFullPath(destinationFolder) + "\\exception.log";
      using (TextWriter writer = File.CreateText(filename))
      {
        writer.WriteLine("ExceptionType: {0}",exception.GetType());
        writer.WriteLine("Message: {0}", exception.Message);
        writer.WriteLine("Source: {0}", exception.Source);
        writer.WriteLine();
        writer.WriteLine("Stack trace:");
        writer.WriteLine(exception.StackTrace);
      }
    }

    public string ActionMessage
    {
      get { throw new NotImplementedException(); }
    }
  }
}
