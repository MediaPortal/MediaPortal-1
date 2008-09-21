using System;

namespace TvLibrary.Channels
{
  [Serializable]
  public class DVBIPChannel : DVBBaseChannel
  {
    #region variables

    string _url;

    #endregion

    public string Url
    {
      get
      {
        return _url;
      }
      set
      {
        _url = value;
      }
    }

    public override string ToString()
    {
      return String.Format("DVBIP:{0} Url:{1}", base.ToString(), Url);
    }

    public override bool Equals(object obj)
    {
      if ((obj as DVBIPChannel) == null) return false;
      if (!base.Equals(obj)) return false;
      DVBIPChannel ch = obj as DVBIPChannel;
      if (ch.Url != Url) return false;

      return true;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _url.GetHashCode();
    }
  }
}