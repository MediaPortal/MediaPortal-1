using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvLibrary.Interfaces
{
  [Serializable]
  public class ChannelDTO
  {
      public int IdChannel { get; set; }
      public bool IsRadio { get; set; }
      public bool IsTv { get; set; }
      public int TimesWatched { get; set; }
      public DateTime TotalTimeWatched { get; set; }
      public bool GrabEpg { get; set; }
      public DateTime LastGrabTime { get; set; }
      public int SortOrder { get; set; }
      public bool VisibleInGuide { get; set; }
      public string ExternalId { get; set; }
      public string DisplayName { get; set; }
      public bool EpgHasGaps { get; set; }
      public bool Freetoair { get; set; }
      public string Name { get; set; }

      public override string ToString()
      {
        return DisplayName;
      }
  }
}
