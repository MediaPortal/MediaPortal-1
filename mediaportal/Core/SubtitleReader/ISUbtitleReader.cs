using System;

namespace MediaPortal.Subtitle
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class ISubtitleReader
  {
    public ISubtitleReader()
    {
    }

    public virtual bool SupportsFile(string strFileName)
    {
      return false;
    }
    public virtual bool ReadSubtitles(string strFileName)
    {
      return false;
    }

    public virtual SubTitles Subs
    {
      get { return null;}
    }
  }
}
