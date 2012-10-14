using System.ServiceModel;

namespace Mediaportal.TV.Server.Plugins.XmlTvImport
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IXMLTVImportService
  {
    [OperationContract]
    void ImportNow();
  }
}