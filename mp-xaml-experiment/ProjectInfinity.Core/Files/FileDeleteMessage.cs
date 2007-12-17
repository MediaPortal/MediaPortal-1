using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Messaging.Files
{
  public class FileDeleteMessage : Message
  {
    string _fileName;
    /// <summary>
    /// Initializes a new instance of the <see cref="FileDeleteMessage"/> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public FileDeleteMessage(string fileName)
    {
      _fileName = fileName;
    }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    public string FileName
    {

      get
      {
        return _fileName;
      }
      set
      {
        _fileName = value;
      }
    }
  }
}
