using System;

namespace MediaPortal.TagReader
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
  public class ITagReader
  {
    public ITagReader()
    {
    }

    public virtual bool SupportsFile(string strFileName)
    {
      return false;
    }
    public virtual bool ReadTag(string strFileName)
    {
      return false;
    }

    public virtual MusicTag Tag
    {
      get { return null;}
    }
	}
}
