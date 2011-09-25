using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class NotOnChannelsCondition : IScheduleCondition
  {
    private IList<ChannelDTO> _channels;
    public IList<ChannelDTO> Channels
    {
      get { return _channels; }
      set { _channels = value; }
    }
    public NotOnChannelsCondition(IList<ChannelDTO> channels)
    {
      _channels = channels;
    }
    public NotOnChannelsCondition()
    {
    }
    public IQueryable<ProgramDTO> ApplyCondition(IQueryable<ProgramDTO> baseQuery)
    {
      return baseQuery.Where(program => !(_channels.Any(ch => ch.IdChannel == program.ReferencedChannel.IdChannel)));
    }
  }
}
