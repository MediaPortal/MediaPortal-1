using System;
using System.Windows.Input;

namespace ProjectInfinity.Pictures
{
  internal abstract class BaseCommand : ICommand
  {
    ///<summary>
    ///Occurs when changes occur which affect whether or not the command should execute.
    ///</summary>
    public event EventHandler CanExecuteChanged;

    protected readonly PictureViewModel _viewModel;

    public BaseCommand(PictureViewModel model)
    {
      _viewModel = model;
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

    public abstract void Execute(object parameters);

  }
}
