using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class OnChannelsCondition : IScheduleCondition
  {    
    private IList<ChannelDTO> _channels;
    public IList<ChannelDTO> Channels
    {
      get { return _channels; }
      set { _channels = value; }
    }
    public OnChannelsCondition(IList<ChannelDTO> channels)
    {
      _channels = channels;
    }
    public OnChannelsCondition()
    {
    }
    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => (_channels.Any(ch => ch.IdChannel == program.ReferencedChannel.IdChannel)));
    }
   

    /*
    so you you would have OnChannelsCondition and NotOnChannelsCondition
    parameters for these would be (channelList)
    
    each condition would be able to describe what parameters it needs and what data type each one is
     
    if the data type is enumeration, it would also provide a list of possible values (name-value pairs)
     
    other datatypes could be: integer, date, dayofweek, set<enumeration datatype> etc
     
    for example the OnDayCondition would have a "days" param that would be a set of DayOfWeek
     */
   
  }
}
