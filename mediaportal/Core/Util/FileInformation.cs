using System;
using System.IO;

namespace Core.Util
{
	/// <summary>
	/// Summary description for FileInformation.
	/// </summary>
  public class FileInformation
  {
    long      length=0;
    DateTime  creationTime=DateTime.MinValue;
    string    name=String.Empty;

    public FileInformation()
    {
    }
    public FileInformation(string file)
    {
      System.IO.FileInfo info = new System.IO.FileInfo(file);
      Length=info.Length;
      Name=info.Name;
      CreationTime=info.CreationTime;
    }
    public long Length
    {
      get { return length;}
      set { length=value;}
    }
    
    public string Name
    {
      get { return name;}
      set { name=value;}
    }
    public DateTime CreationTime
    {
      get { return creationTime;}
      set { creationTime=value;}
    }
 	}
}
