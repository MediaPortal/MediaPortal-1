using Mediaportal.TV.Server.TVService.Interfaces.Enums;

namespace Mediaportal.TV.Server.TVService.Interfaces.Services
{
  public interface ISubChannel
  {
    int IdChannel { get; set; }
    int Id { get; set; }
    TvUsage TvUsage { get; set; }
  }
}