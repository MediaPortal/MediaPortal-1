using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.CardManagement.CardHandler
{
  public class ParkedUser : User, IDisposable
  {        
    private readonly IDictionary<int, ManualResetEvent> _events; //key is subchannelid
    private readonly IDictionary<int, double> _parkedDurations; //key is subchannelid
    private readonly IDictionary<int, DateTime> _parkedAtList; //key is subchannelid
    //key is subch id
    //private readonly int _parkedSubChannel;

    public ParkedUser(IUser user) : base(user.Name, user.UserType, user.CardId, user.Priority.GetValueOrDefault())
    {
      //_parkedSubChannel = user.GetSubChannelIdByChannelId(channelId);            
      SubChannels = new SortedDictionary<int, ISubChannel>(user.SubChannels);
      TvStoppedReason = user.TvStoppedReason;      
      _parkedAtList = new Dictionary<int, DateTime>();
      _parkedDurations = new Dictionary<int, double>();
      _events = new Dictionary<int, ManualResetEvent>();

      foreach (ISubChannel subChannel in SubChannels.Values)
      {
        if (subChannel.TvUsage == TvUsage.Parked)
        {
          var evt = new ManualResetEvent(false);
          _events.Add(subChannel.Id, evt);
        }
      }
    }

    /*~ParkedUser()
    {
      Dispose();
    }*/

    public IDictionary<int, double> ParkedDurations
    {
      get { return _parkedDurations; }
    }

    public IDictionary<int, ManualResetEvent> Events
    {
      get { return _events; }
    }

    public IDictionary<int, DateTime> ParkedAtList
    {
      get { return _parkedAtList; }
    }

   
    public void Dispose()
    {
      foreach(ManualResetEvent evt in _events.Values)
      {
        evt.Set();
        evt.Dispose();
      }
      _events.Clear();
    }
  }
}
