using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using ProjectInfinity;
using ProjectInfinity.Localisation;
using Dialogs;

namespace MyTv
{
  class TvConflictDialogViewModel : DialogViewModel
  {
    #region variables
    ICommand _SkipNewCommand;
    ICommand _SkipOldCommand;
    ICommand _keepConflictCommand;
    #endregion
    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvConflictDialogViewModel"/> class.
    /// </summary>
    /// <param name="page">The page.</param>
    public TvConflictDialogViewModel(Window window)
      :base(window)
    {
    }
    #endregion

    public string SkipNewRecordingLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 130);//Keep conflict
      }
    }
    public string SkipOldRecordingLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 131);//Skip conflicting recording(s)
      }
    }
    public string KeepConflictLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 132);//Keep conflict
      }
    }
    #region commands
    public ICommand SkipOld
    {
      get
      {
        if (_SkipOldCommand == null)
          _SkipOldCommand = new SkipOldCommand(this);
        return _SkipOldCommand;
      }
    }
    public ICommand SkipNew
    {
      get
      {
        if (_SkipNewCommand == null)
          _SkipNewCommand = new SkipNewCommand(this);
        return _SkipNewCommand;
      }
    }
    public ICommand KeepConflict
    {
      get
      {
        if (_keepConflictCommand == null)
          _keepConflictCommand = new KeepConflictCommand(this);
        return _keepConflictCommand;
      }
    }

    #endregion
    public class SkipOldCommand : BaseCommand
    {
      public SkipOldCommand(DialogViewModel model)
        : base(model)
      {
      }

      public override void Execute(object parameter)
      {
        _viewModel.SelectedIndex = 0;
        _viewModel.Window.Close();
      }
    }
    public class SkipNewCommand : BaseCommand
    {
      public SkipNewCommand(DialogViewModel model)
        : base(model)
      {
      }

      public override void Execute(object parameter)
      {
        _viewModel.SelectedIndex = 1;
        _viewModel.Window.Close();
      }
    }
    public class KeepConflictCommand : BaseCommand
    {
      public KeepConflictCommand(DialogViewModel model)
        : base(model)
      {
      }

      public override void Execute(object parameter)
      {
        _viewModel.SelectedIndex = 2;
        _viewModel.Window.Close();
      }
    }
  }
}
