using System.ServiceModel;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface ISettingService
  {
    [OperationContract]    
    Setting GetSetting(string tagName);

    [OperationContract]    
    Setting GetSettingWithDefaultValue(string tagName, string defaultValue);

    [OperationContract]    
    void SaveSetting(string tagName, string defaultValue);
  }
}
