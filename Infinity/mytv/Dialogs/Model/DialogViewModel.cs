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

namespace Dialogs
{
  public class DialogViewModel : INotifyPropertyChanged
  {
    #region variables
    Window _window;
    public event PropertyChangedEventHandler PropertyChanged;
    public string _title;
    public string _header;
    public string _content;
    ICommand _closeCommand;
    ICommand _yesCommand;
    ICommand _noCommand;
    ICommand _itemSelectedCommand;
    DialogResult _result;
    int _selectedIndex;
    MenuCollectionView _menuCollection;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvBaseViewModel"/> class.
    /// </summary>
    /// <param name="page">The page.</param>
    public DialogViewModel(Window window)
    {
      //store page & window
      _window = window;
      _result = DialogResult.No;
    }
    #endregion


    #region properties
    /// <summary>
    /// Notifies subscribers that property has been changed
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    public void ChangeProperty(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Returns the ListViewCollection containing the recordings
    /// </summary>
    /// <value>The recordings.</value>
    public CollectionView MenuItems
    {
      get
      {
        return _menuCollection;
      }
    }
    public void SetItems(DialogMenuItemCollection items)
    {
      ArrayList listItems = new ArrayList();
      for (int i=0; i < items.Count;++i)
        listItems.Add(items[i]);
      _menuCollection = new MenuCollectionView(listItems);
    }
    /// <summary>
    /// Gets the window.
    /// </summary>
    /// <value>The window.</value>
    public Window Window
    {
      get
      {
        return _window;
      }
    }
    public string Title
    {
      get
      {
        return _title;
      }
      set
      {
        _title = value;
      }
    }
    public string Header
    {
      get
      {
        return _header;
      }
      set
      {
        _header = value;
      }
    }
    public string Content
    {
      get
      {
        return _content;
      }
      set
      {
        _content = value;
      }
    }
    public string YesLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("dialogs", 0);//Yes
      }
    }
    public string NoLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("dialogs", 1);//No
      }
    }
    public DialogResult DialogResult
    {
      get
      {
        return _result;
      }
      set
      {
        _result = value;
      }
    }
    public void SetSelectedIndex(int index)
    {
      _selectedIndex = index;
      ChangeProperty("SelectedIndex");
    }

    public int SelectedIndex
    {
      get
      {
        return _selectedIndex;
      }
      set
      {
        _selectedIndex = value;
      }
    }

    public ICommand Close
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new CloseCommand(this);
        }
        return _closeCommand;
      }
    }
    public ICommand Yes
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new YesCommand(this);
        }
        return _closeCommand;
      }
    }
    public ICommand No
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new NoCommand(this);
        }
        return _closeCommand;
      }
    }
    public ICommand ItemSelected
    {
      get
      {
        if (_itemSelectedCommand == null)
        {
          _itemSelectedCommand = new ItemSelectedCommand(this);
        }
        return _itemSelectedCommand;
      }
    }

    #endregion

    #region command classes
    public abstract class BaseCommand : ICommand
    {
      public event EventHandler CanExecuteChanged;
      protected DialogViewModel _viewModel;
      public BaseCommand(DialogViewModel model)
      {
        _viewModel = model;
      }

      bool ICommand.CanExecute(object parameter)
      {
        return true;
      }

      public virtual void Execute(object parameter)
      {
      }

    }

    public class CloseCommand : BaseCommand
    {
      public CloseCommand(DialogViewModel model)
        : base(model)
      {
      }

      public override void Execute(object parameter)
      {
        _viewModel.Window.Close();
      }
    }
    public class YesCommand : BaseCommand
    {
      public YesCommand(DialogViewModel model)
        : base(model)
      {
      }

      public override void Execute(object parameter)
      {
        _viewModel.DialogResult = DialogResult.Yes;
        _viewModel.Window.Close();
      }
    }
    public class NoCommand : BaseCommand
    {
      public NoCommand(DialogViewModel model)
        : base(model)
      {
      }

      public override void Execute(object parameter)
      {
        _viewModel.DialogResult = DialogResult.No;
        _viewModel.Window.Close();
      }
    }

    public class ItemSelectedCommand : BaseCommand
    {
      public ItemSelectedCommand(DialogViewModel model)
        : base(model)
      {
      }

      public override void Execute(object parameter)
      {
        _viewModel.Window.Close();
      }
    }
    #endregion

    #region menu item collection view
    /// <summary>
    /// This class represents the recording view
    /// </summary>
    public class MenuCollectionView : ListCollectionView
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="MenuCollectionView"/> class.
      /// </summary>
      /// <param name="model">The database model.</param>
      public MenuCollectionView(ArrayList items)
        :base(items)
      {
      }

    }
    #endregion

  }
}
