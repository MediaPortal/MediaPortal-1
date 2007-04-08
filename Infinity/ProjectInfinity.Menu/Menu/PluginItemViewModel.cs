using System;
using System.Windows.Input;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  /// <summary>
  /// ViewModel for a <see cref="IPluginItem"/>.
  /// </summary>
  /// <remarks>
  /// The purpose of this class is to be databound by UIs.
  /// Therefore it includes an <see cref="ICommand"/> that
  /// can bound to buttons and other controls to 
  /// start the actual <see cref="IPlugin"/> that is represented
  /// by this item.
  /// </remarks>
  public class PluginItemViewModel
  {
    private IPluginItem item;
    private ICommand startCommand;

    public PluginItemViewModel(IPluginItem item)
    {
      this.item = item;
    }

    /// <summary>
    /// Returns the <see cref="IPluginItem.Text"/>
    /// </summary>
    public string Text
    {
      get { return item.Text; }
    }

    /// <summary>
    /// Returns the <see cref="IPluginItem.Description"/>
    /// </summary>
    public string Description
    {
      get { return item.Description; }
    }

    public string ImagePath
    {
      get { return item.ImagePath; }
    }
    /// <summary>
    /// Returns an <see cref="ICommand"/> implementation that can be used to start
    /// the plugin that is represented by this item.
    /// </summary>
    public ICommand Start
    {
      get
      {
        if (startCommand == null)
        {
          startCommand = new StartCommand(this);
        }
        return startCommand;
      }
    }

    private class StartCommand : ICommand
    {
      private PluginItemViewModel _viewViewModel;

      public StartCommand(PluginItemViewModel _viewViewModel)
      {
        this._viewViewModel = _viewViewModel;
      }

      public void Execute(object parameter)
      {
        ServiceScope.Get<IPluginManager>().Start(_viewViewModel.item.Text);
      }

      public event EventHandler CanExecuteChanged;
      // Most commands are enabled most of the time on this model
      public virtual bool CanExecute(object parameter)
      {
        return true;
      }

      protected void OnCanExecuteChanged()
      {
        if (CanExecuteChanged != null)
        {
          CanExecuteChanged(this, EventArgs.Empty);
        }
      }
    }


  }
}