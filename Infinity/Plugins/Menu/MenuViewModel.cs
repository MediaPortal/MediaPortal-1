using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ProjectInfinity.Controls;
using ProjectInfinity.Messaging;
using ProjectInfinity.Navigation;
using ProjectInfinity.Players;
using ProjectInfinity.TaskBar;
using ProjectInfinity.Plugins;

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
    private readonly MenuCollection menuView;
    private readonly string _id;
    private ICommand _launchCommand;
    private ICommand _fullScreenCommand;

    public MenuViewModel(string id)
    {
      _id = id;

      IList<IMenuItem> model;

      //The IMenuManager.GetMenu method returns a list of IMenuItem implementations of the 
      //correct type (IMenu, ICommandItem, IMessageItem, ...)
      Plugins.Menu menuInfo = (Plugins.Menu)ServiceScope.Get<IPluginManager>().BuildItem<Plugins.Menu>("/Menus", id);
      if (menuInfo != null)
      {
        model = ServiceScope.Get<IMenuManager>().GetMenu(menuInfo.Path);
      }
      else
      {
        model = new List<IMenuItem>();
      }
      //We translate this list to a list of menu items that the menu control can use by means
      //of the MenuViewCreator
      menuView = MenuViewCreator.Build(model);
    }

    public Brush VideoBrush
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          MediaPlayer player = (MediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0].UnderlyingPlayer;

          VideoDrawing videoDrawing = new VideoDrawing();
          videoDrawing.Player = player;
          videoDrawing.Rect = new Rect(0, 0, 800, 600);
          DrawingBrush videoBrush = new DrawingBrush();
          videoBrush.Stretch = Stretch.Fill;
          videoBrush.Drawing = videoDrawing;
          return videoBrush;
        }

        return null;
      }
    }

    public double MenuOffset
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          return 350;
        }
        return 150;
      }
    }

    public Brush VideoOpacityMask
    {
      get
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          return Application.Current.Resources["VideoOpacityMask"] as Brush;
        }
        return null;
      }
    }

    public Visibility IsVideoPresent
    {
      get { return (ServiceScope.Get<IPlayerCollectionService>().Count != 0) ? Visibility.Visible : Visibility.Collapsed; }
    }

    public Window Window
    {
      get { return ServiceScope.Get<INavigationService>().GetWindow(); }
    }

    public string HeaderLabel
    {
      get
      {

        Plugins.Menu menuInfo = (Plugins.Menu)ServiceScope.Get<IPluginManager>().BuildItem<Plugins.Menu>("/Menus", _id);
        if (menuInfo != null)
        {
          return menuInfo.Name;
        }
        return String.Empty;
      }
    }

    public Visibility HeaderLabelVisibility
    {
      get { return (ServiceScope.Get<IPlayerCollectionService>().Count == 0) ? Visibility.Visible : Visibility.Collapsed; }
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

    /// <summary>
    /// Launches the current selected <see cref="IMenuItem"/>
    /// </summary>
    /// <remarks>
    /// This class implements the Visitor Pattern <seealso Visitor Pattern cref="http://www.dofactory.com/Patterns/PatternVisitor.aspx"/>
    /// </remarks>
    private class LaunchCommand : ICommand, IMenuItemVisitor
    {
      private MenuViewModel _viewModel;

      public LaunchCommand(MenuViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      #region ICommand Members

      ///<summary>
      ///Defines the method that determines whether the command can execute in its current state.
      ///</summary>
      ///<returns>
      ///true if this command can be executed; otherwise, false.
      ///</returns>
      ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public bool CanExecute(object parameter)
      {
        return parameter != null;
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
        MenuMenuItem item = parameter as MenuMenuItem;
        if (item == null)
        {
          return;
        }
        IMenuItem menuItem = item.GetIMenuItem();
        Debug.Assert(menuItem != null);
        //Visitor Pattern:
        //We pass ourselves to the item and the item will call the correct overload
        //of our Visit method.  Look ma, no Casting ;-)
        menuItem.Accept(this);
      }

      #endregion

      //User clicked on a Menu

      #region IMenuItemVisitor Members

      public void Visit(IMenu menu)
      {
        if (menu.DefaultItem != null)
        {
          menu.DefaultItem.Accept(this);
        }
      }

      //User clicked on Plugin
      public void Visit(IPluginItem plugin)
      {
        plugin.Execute();
        //ServiceScope.Get<IPluginManager>().Start(pluginItem.Text);
      }

      //User clicked on a Message
      public void Visit(IMessageItem message)
      {
        IMessageBroker broker = ServiceScope.Get<IMessageBroker>(false);
        if (broker == null)
        {
          return;
        }
        broker.Send(message.Message);
      }

      //User clicked on a Command
      public void Visit(ICommandItem command)
      {
        command.Execute();
      }

      [Obsolete("This overload should not be used. Use one of the specified overloads instead")]
      public void Visit(IMenuItem item)
      {
        item.Execute();
      }

      #endregion
    }

    #region FullScreenCommand  class

    /// <summary>
    /// FullScreenCommand will toggle application between normal and fullscreen mode
    /// </summary> 
    public class FullScreenCommand : ICommand
    {
      private readonly MenuViewModel _viewModel;

      /// <summary>
      /// Initializes a new instance of the <see cref="FullScreenCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public FullScreenCommand(MenuViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      #region ICommand Members

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

      public event EventHandler CanExecuteChanged;

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public void Execute(object parameter)
      {
        Window window = _viewModel.Window;
        if (window.WindowState == WindowState.Maximized)
        {
          window.ShowInTaskbar = true;
          ServiceScope.Get<IWindowsTaskBar>().Show();
          window.WindowStyle = WindowStyle.SingleBorderWindow;
          window.WindowState = WindowState.Normal;
        }
        else
        {
          window.ShowInTaskbar = false;
          window.WindowStyle = WindowStyle.None;
          ServiceScope.Get<IWindowsTaskBar>().Hide();
          window.WindowState = WindowState.Maximized;
        }
      }

      #endregion
    }

    #endregion
  }
}