using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;

namespace Mediaportal.TV.Server.TVService.Interfaces.Services
{
  public interface ISubChannel
  {
    [DataMember]
    int IdChannel { get; set; }

    [DataMember]
    int Id { get; set; }

    [DataMember]
    TvUsage TvUsage { get; set; }
  }
}