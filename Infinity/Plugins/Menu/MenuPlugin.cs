using System;
using ProjectInfinity.Navigation;
using ProjectInfinity.Plugins;
using ProjectInfinity.MenuManager;

namespace ProjectInfinity.Menu
{
  //[Plugin("Menu", "Project Infinity's main menu", AutoStart=true)]
  public class MenuPlugin : IPlugin, IAutoStart, IMenuCommand
  {
    #region Variables
    private string _id;
    #endregion

    #region IPlugin Members

    public void Initialize(string id)
    {
      _id = id;
    }

    #endregion

    #region IAutoStart Members

    public void Startup()
    {
      Run();
    }

    #endregion

    #region IMenuCommand Members

    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new MenuView(_id));
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