using System;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.TvBusinessLayer.Interfaces.Interfaces
{  
  public interface INowAndNext
  {
    [DataMember]
    int IdChannel { get; set; }

    [DataMemberAttribute]
    DateTime NowStartTime { get; set; }

    [DataMemberAttribute]
    DateTime NowEndTime { get; set; }

    [DataMemberAttribute]
    string TitleNow { get; set; }

    [DataMemberAttribute]
    string TitleNext { get; set; }

    [DataMemberAttribute]
    int IdProgramNow { get; set; }

    [DataMemberAttribute]
    int IdProgramNext { get; set; }

    [DataMemberAttribute]
    string EpisodeName { get; set; }

    [DataMemberAttribute]
    string EpisodeNameNext { get; set; }

    [DataMemberAttribute]
    string SeriesNum { get; set; }

    [DataMemberAttribute]
    string SeriesNumNext { get; set; }

    [DataMemberAttribute]
    string EpisodeNum { get; set; }

    [DataMemberAttribute]
    string EpisodeNumNext { get; set; }

    [DataMemberAttribute]
    string EpisodePart { get; set; }

    [DataMemberAttribute]
    string EpisodePartNext { get; set; }
  }
}