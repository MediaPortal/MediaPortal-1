using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  public class TvHomeViewModel
  {

    #region properties
    public string DateLabel
    {
      get
      {
        return DateTime.Now.ToString("dd-MM HH:mm");
      }
    }
    public string TvGuideLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 0);//TvGuide
      }
    }
    public string ChannelLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 2);//Channel
      }
    }
    public string TvStreamsLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 3);//TvStreams
      }
    }
    public string TvOnOffLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 4);//TvOnOff
      }
    }
    public string ScheduledLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 5);//Scheduled
      }
    }
    public string RecordedLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 6);//Recorded
      }
    }
    public string SearchLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 7);//Search
      }
    }
    public string TeletextLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 8);//Teletext
      }
    }

    public string HeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 9);//television
      }
    }

    public bool? TvOnOff
    {
      get
      {
        return (TvPlayerCollection.Instance.Count > 0);
      }
    }
    #endregion
  }
}
