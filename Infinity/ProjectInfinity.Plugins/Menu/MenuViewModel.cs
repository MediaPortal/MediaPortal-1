using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using ProjectInfinity;
using ProjectInfinity.Plugins;
using ProjectInfinity.Localisation;
using ProjectInfinity.Controls;
using ProjectInfinity.TaskBar;
using ProjectInfinity.Navigation;

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
    ICommand _fullScreenCommand;
    string _id;

    public MenuViewModel(string id)
    {
      _id = id;
      IList<IMenuItem> model = ServiceScope.Get<IMenuManager>().GetMenu(id);
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

    public Window Window
    {
      get { return ServiceScope.Get<INavigationService>().GetWindow(); }
    }

    public string HeaderLabel
    {
      get { return ServiceScope.Get<IMenuManager>().GetMenuName(_id); }
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
    /// <summary>
    /// Returns a ICommand for toggeling between fullscreen mode and windowed mode
    /// </summary>
    /// <value>The command.</value>
    public ICommand FullScreen
    {
      get
      {
        if (_fullScreenCommand == null)
        {
          _fullScreenCommand = new FullScreenCommand(this);
        }
        return _fullScreenCommand;
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
        if (menuItem == null)
          return;

        IPluginItem pluginItem;

        IMenu menu = menuItem.Menu as IMenu;
        if (menu != null)
          pluginItem = menu.MenuItem as IPluginItem;
        else
          pluginItem = menuItem.Menu as IPluginItem;

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

    #region FullScreenCommand  class
    /// <summary>
    /// FullScreenCommand will toggle application between normal and fullscreen mode
    /// </summary> 
    public class FullScreenCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;
      MenuViewModel _viewModel;
      /// <summary>
      /// Initializes a new instance of the <see cref="FullScreenCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public FullScreenCommand(MenuViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public void Execute(object parameter)
      {
        Window window = _viewModel.Window;
        if (window.WindowState == System.Windows.WindowState.Maximized)
        {
          window.ShowInTaskbar = true;
          ServiceScope.Get<IWindowsTaskBar>().Show();
          window.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
          window.WindowState = System.Windows.WindowState.Normal;
        }
        else
        {
          window.ShowInTaskbar = false;
          window.WindowStyle = System.Windows.WindowStyle.None;
          ServiceScope.Get<IWindowsTaskBar>().Hide();
          window.WindowState = System.Windows.WindowState.Maximized;
        }
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
    #endregion

  }
}