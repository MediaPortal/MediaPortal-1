using System.ServiceModel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces
{
  //[ServiceContract(CallbackContract = typeof(ICiMenuEventCallback))]
  public interface ICiMenuEventCallback
  {
    [OperationContract]
    void CiMenuCallback(CiMenu.CiMenu menu);
  }
}