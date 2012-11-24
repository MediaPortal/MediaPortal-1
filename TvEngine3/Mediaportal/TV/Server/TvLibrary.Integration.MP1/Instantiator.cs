using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Integration.MP1
{
  public class Instantiator : Singleton<Instantiator>
  {
    /// <summary>
    /// Get or Create an IoC container
    /// </summary>
    /// <returns></returns>
    public IWindsorContainer Container()
    {
      var container = GlobalServiceProvider.Instance.Get<IWindsorContainer>();      
      if (container == null)
      {
        container = new WindsorContainer(new XmlInterpreter());
        GlobalServiceProvider.Instance.Add<IWindsorContainer>(container); 
      }
      return container;
    }
  }
}
