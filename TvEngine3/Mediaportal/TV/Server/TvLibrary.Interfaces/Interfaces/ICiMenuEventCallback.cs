using System.ServiceModel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces
{
  public interface ICiMenuEventCallback
  {
    [OperationContract(IsOneWay = true)]
    void CiMenuCallback(CiMenu.CiMenu menu);
  }
}