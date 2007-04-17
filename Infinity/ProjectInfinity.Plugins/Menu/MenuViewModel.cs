using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Input;
using ProjectInfinity;
using ProjectInfinity.Plugins;
using ProjectInfinity.Localisation;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Menu
{
  /// <summary>
  /// ViewModel for a list of <see cref="IMenuItem"/>s.
  /// </summary>
  /// <remarks>
  /// This purpose of this class is to be databound by UIs.
  /// </remarks>
  /// <seealso cref="http://blogs.sqlxml.org/bryantlikes/archive/2006/09/27/WPF-Patterns.aspx"/>
  public class MenuViewModel
  {
    private MenuCollection menuView;
    private ICommand _launchCommand;

    public MenuViewModel()
    {
      IList<IMenuItem> model = ServiceScope.Get<IMenuManager>().GetMenu();
      menuView = new MenuCollection();
      foreach (IMenuItem item in model)
      {
        Menu menu = item as Menu;
        if (menu != null)
        {
          PluginMenuItem newItem = new PluginMenuItem(item);
          MenuCollection subMenus = new MenuCollection();
          for (int i = 0; i < menu.Items.Count; ++i)
          {
            subMenus.Add(new PluginMenuItem(menu.Items[i]));
          }
          newItem.SubMenus = subMenus;
          menuView.Add(newItem);
        }
        else
        {
          menuView.Add(new PluginMenuItem(item));
        }
      }
    }

    public string HeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("global", 0);//infinity
      }
    }
    public MenuCollection Items
    {
      get { return menuView; }
    }

    public ICommand Launch
    {
      get
      {
        if (_launchCommand == null)
        {
          _launchCommand = new LaunchCommand(this);
        }
        return _launchCommand;
      }
    }

    private class LaunchCommand : ICommand
    {
      private MenuViewModel _viewModel;

      public LaunchCommand(MenuViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      ///<summary>
      ///Occurs when changes occur which affect whether or not the command should execute.
      ///</summary>
      public event EventHandler CanExecuteChanged;

      ///<summary>
      ///Defines the method to be called when the command is invoked.
      ///</summary>
      ///
      ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public void Execute(object parameter)
      {
        PluginMenuItem menuItem = parameter as PluginMenuItem;
        if (menuItem == null) return;
        IPluginItem pluginItem = menuItem.Menu as IPluginItem;
        if (pluginItem != null)
          pluginItem.Execute();
        //ServiceScope.Get<IPluginManager>().Start(pluginItem.Text);

      }

      ///<summary>
      ///Defines the method that determines whether the command can execute in its current state.
      ///</summary>
      ///<returns>
      ///true if this command can be executed; otherwise, false.
      ///</returns>
      ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public bool CanExecute(object parameter)
      {
        return true;
      }

    }

  }
}