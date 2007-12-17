using System;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.MenuManager
{
  public class MenuManagerPlugin : IPlugin, IAutoStart
  {
    #region IPlugin Members

    public void Initialize(string id)
    {
    }

    #endregion

    #region IAutoStart Members

    public void Startup()
    {
      ServiceScope.Add<IMenuManager>(new MenuManager());
    }

    #endregion

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
    }

    #endregion
  }
}