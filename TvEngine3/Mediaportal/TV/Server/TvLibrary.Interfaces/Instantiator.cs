using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  public class Instantiator : Singleton<Instantiator>
  {
    /// <summary>
    /// Get or Create an IoC container
    /// </summary>
    /// <returns></returns>
    public IWindsorContainer Container(string configFile = null)
    {
      var container = GlobalServiceProvider.Instance.Get<IWindsorContainer>();      
      if (container == null)
      {
        container = string.IsNullOrEmpty(configFile) ? new WindsorContainer(new XmlInterpreter()) : new WindsorContainer(configFile);
        GlobalServiceProvider.Instance.Add<IWindsorContainer>(container); 
      }
      return container;
    }
  }
}
