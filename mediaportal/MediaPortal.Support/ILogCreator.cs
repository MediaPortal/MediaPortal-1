using System;

namespace MediaPortal.Support
{
  public interface ILogCreator
  {
    void CreateLogs(string destinationFolder);
    string ActionMessage { get; }
  }
}
