using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Input;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  /// <summary>
  /// ViewModel for a list of <see cref="IMenuItem"/>s.
  /// </summary>
  /// <remarks>
  /// This purpose of this class is to be databound by UIs.
  /// This ViewModel will translate each item in the <see cref="Items"/> list to its 
  /// respective ViewModel.
  /// </remarks>
  /// <seealso cref="http://blogs.sqlxml.org/bryantlikes/archive/2006/09/27/WPF-Patterns.aspx"/>
  public class MenuItemViewModel
  {
    private CollectionView menuView;
    private ICommand _launchCommand;

    public MenuItemViewModel(IList<IMenuItem> model)
    {
      menuView = new CollectionView(model);
    }

    public CollectionView Items
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
      private MenuItemViewModel _viewModel;

      public LaunchCommand(MenuItemViewModel viewModel)
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
        IPluginItem pluginItem = _viewModel.Items.CurrentItem as IPluginItem;
        if (pluginItem !=null)
          ServiceScope.Get<IPluginManager>().Start(pluginItem.Text);

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

    //private class MenuCollectionView : CollectionView
    //{
    //  public MenuCollectionView(IList<IMenuItem> model) : base(model)
    //  {
    //  }

    //  public override int IndexOf(object item)
    //  {
    //    return base.IndexOf(item);
    //  }
    //  public override object GetItemAt(int index)
    //  {
    //    IMenuItem item = (IMenuItem) base.GetItemAt(index);

    //    return TranslateItem(item);
    //  }

    //  private static object TranslateItem(IMenuItem item)
    //  {
    //    IPluginItem plugin = item as IPluginItem;
    //    if (plugin != null)
    //    {
    //      return new PluginItemViewModel(plugin);
    //    }
    //    IMenu menu = item as IMenu;
    //    if (menu != null)
    //    {
    //      return new MenuViewModel(menu);
    //    }
    //    return null;
    //  }

    //  public override object CurrentItem
    //  {
    //    get { return TranslateItem((IMenuItem) base.CurrentItem); }
    //  }
    //}
  }
}