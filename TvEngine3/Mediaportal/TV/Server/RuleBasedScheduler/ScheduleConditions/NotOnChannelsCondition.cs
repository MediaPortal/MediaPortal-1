using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.RuleBasedScheduler.ScheduleConditions
{
  [Serializable]
  public class NotOnChannelsCondition : IScheduleCondition
  {
    private IList<Channel> _channels;
    public IList<Channel> Channels
    {
      get { return _channels; }
      set { _channels = value; }
    }
    public NotOnChannelsCondition(IList<Channel> channels)
    {
      _channels = channels;
    }
    public NotOnChannelsCondition()
    {
    }
    public IQueryable<Program> ApplyCondition(IQueryable<Program> baseQuery)
    {
      return baseQuery.Where(program => !(_channels.Any(ch => ch.IdChannel == program.Channel.IdChannel)));
    }
  }
}
