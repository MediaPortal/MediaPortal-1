using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;

namespace TvLibrary.Interfaces
{
  public class Instantiator
  {
    /// <summary>
    /// Get or Create an IoC container
    /// </summary>
    /// <returns></returns>
    public IWindsorContainer Container(string configFile = null)
    {
      var container = GlobalServiceProvider.Instance.TryGet<IWindsorContainer>();
      if (container == null)
      {
        container = string.IsNullOrEmpty(configFile) ? new WindsorContainer(new XmlInterpreter()) : new WindsorContainer(configFile);
        GlobalServiceProvider.Instance.Add<IWindsorContainer>(container);
      }
      return container;
    }
  }
}
