using System;
using System.ServiceModel;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface ISettingService
  {
    [OperationContract(Name = "SaveValueInt")]
    void SaveValue(string tagName, int defaultValue);

    [OperationContract(Name = "SaveValueDouble")]
    void SaveValue(string tagName, double defaultValue);

    [OperationContract(Name = "SaveValueBool")]
    void SaveValue(string tagName, bool defaultValue);

    [OperationContract(Name = "SaveValueString")]
    void SaveValue(string tagName, string defaultValue);

    [OperationContract(Name = "SaveValueDateTime")]
    void SaveValue(string tagName, DateTime defaultValue);
    
    [OperationContract(Name = "GetValueInt")]
    int GetValue(string tagName, int defaultValue);

    [OperationContract(Name = "GetValueDouble")]
    double GetValue(string tagName, double defaultValue);

    [OperationContract(Name = "GetValueBool")]
    bool GetValue(string tagName, bool defaultValue);

    [OperationContract(Name = "GetValueString")]
    string GetValue(string tagName, string defaultValue);

    [OperationContract(Name = "GetValueDateTime")]
    DateTime GetValue(string tagName, DateTime defaultValue);
  }
}