using System;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IParkedUserManagement
  {
    void ParkUser(ref IUser user, double duration, int idChannel);
    void UnParkUser(ref IUser user, double duration, int idChannel);
    bool IsUserParkedOnAnyChannel(string userName);
    bool IsUserParkedOnChannel(string userName, int idChannel, out double parkedDuration, out DateTime parkedAt);    
    bool IsUserParkedOnChannel(string userName, int idChannel);
    void CancelAllParkedUsers();
    bool HasAnyParkedUsers();    
    bool IsAnyUserParkedOnChannel(int idChannel);
    void Dispose();
    bool HasParkedUserWithDuration(int channelId, double duration);
    void CancelParkedUserBySubChannelId(string name, int subchannelId);
  }
}
