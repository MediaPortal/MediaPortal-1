using System;
using System.Collections;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TvDatabase;
using TvControl;
using Dialogs;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;

namespace MyTv
{
  public class TvSearchViewModel : TvBaseViewModel
  {
    #region enums
    public enum SearchType
    {
      Title,
      Genre,
      Description,
    }
    public enum SortType
    {
      Channel,
      Date,
      Title,
      Description
    }
    #endregion

    #region variables
    SearchType _searchType = SearchType.Title;
    ICommand _sortCommand;
    ICommand _typeCommand;
    ICommand _contextMenu;
    SearchCollectionView _searchView;
    SearchDatabaseModel _dataModel;
    string _searchText;
    #endregion

    #region ctor
    public TvSearchViewModel(Page page)
      : base(page)
    {
      _dataModel = new SearchDatabaseModel();
      _searchView=new SearchCollectionView(_dataModel);
    }
    #endregion

    #region properties
    public SearchType SearchMode
    {
      get
      {
        return _searchType;
      }
      set
      {
        _searchType = value;
        _dataModel.Search(_searchType, SearchText);
      }
    }
    public override string HeaderLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 107);//search
      }
    }
    public override string SearchLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 126);//Enter search text
      }
    }
    public string SortLabel
    {
      get
      {

        switch (_searchView.SortMode)
        {
          case SortType.Channel:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Sort:Channel";
          case SortType.Date:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 85);//"Sort:Date";
          case SortType.Title:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 88);//"Sort:Title";
          case SortType.Description:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 127);//"Sort:Description";
        }
        return "";
      }
    }
    public string TypeLabel
    {
      get
      {
        return ServiceScope.Get<ILocalisation>().ToString("mytv", 108);//Type
      }
    }

    public string SearchText
    {
      get
      {
        return _searchText;
      }
      set
      {
        _searchText = value;
        _dataModel.Search(SearchMode, _searchText);
      }
    }

    /// <summary>
    /// Returns the ListViewCollection containing the recordings
    /// </summary>
    /// <value>The recordings.</value>
    public CollectionView Programs
    {
      get
      {
        if (_searchView == null)
        {
          _searchView = new SearchCollectionView(_dataModel);
        }
        return _searchView;
      }
    }
    /// <summary>
    /// Returns the datatemplate for the listbox items
    /// </summary>
    /// <value>The datatemplate.</value>
    public DataTemplate ItemTemplate
    {
      get
      {
        return (DataTemplate)Page.Resources["searchItemListTemplate"];
      }
    }
    #endregion

    #region commands
    /// <summary>
    /// Returns a ICommand for sorting
    /// </summary>
    /// <value>The command.</value>
    public ICommand Sort
    {
      get
      {
        if (_sortCommand == null)
        {
          _sortCommand = new SortCommand(this);
        }
        return _sortCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for search type
    /// </summary>
    /// <value>The command.</value>
    public ICommand ChangeSearchType
    {
      get
      {
        if (_typeCommand == null)
        {
          _typeCommand = new SearchTypeCommand(this);
        }
        return _typeCommand;
      }
    }
    /// <summary>
    /// Returns a ICommand for ContextMenu
    /// </summary>
    /// <value>The command.</value>
    public ICommand ContextMenu
    {
      get
      {
        if (_contextMenu == null)
        {
          _contextMenu = new ContextMenuCommand(this);
        }
        return _contextMenu;
      }
    }
    #endregion

    #region Commands subclasses
    #region base command class
    public abstract class SearchBaseCommand : ICommand
    {
      protected TvSearchViewModel _viewModel;
      public event EventHandler CanExecuteChanged;

      public SearchBaseCommand(TvSearchViewModel viewModel)
      {
        _viewModel = viewModel;
      }

      public abstract void Execute(object parameter);

      public virtual bool CanExecute(object parameter)
      {
        return true;
      }

      protected void OnCanExecuteChanged()
      {
        if (this.CanExecuteChanged != null)
        {
          this.CanExecuteChanged(this, EventArgs.Empty);
        }
      }
    }
    #endregion

    #region sort command class
    /// <summary>
    /// SortCommand changes the way the view gets sorted
    /// </summary>
    public class SortCommand : SearchBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SortCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public SortCommand(TvSearchViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        //show dialog menu with all sorting options
        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
        dlgMenu.SubTitle = "";
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 2)/*Channel*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 73)/*"Date*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 98)/*"Title*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 109)/*"Description*/));
        SearchCollectionView view = (SearchCollectionView)_viewModel.Programs;
        dlgMenu.SelectedIndex = (int)view.SortMode;
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected

        //tell the view to sort
        view.SortMode = (TvSearchViewModel.SortType)dlgMenu.SelectedIndex;

        //and tell the model that the sort property is changed
        _viewModel.ChangeProperty("SortLabel");
      }
    }
    #endregion

    #region SearchTypeCommand class
    /// <summary>
    /// SearchTypeCommand changes the way the view gets sorted
    /// </summary>
    public class SearchTypeCommand : SearchBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SortCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public SearchTypeCommand(TvSearchViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        //show dialog menu with all sorting options
        MpMenu dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = _viewModel.Window;
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
        dlgMenu.SubTitle = "";
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 98)/*"Title*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 99)/*Genre*/));
        dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 109)/*"Description*/));
        dlgMenu.SelectedIndex = (int)_viewModel.SearchMode;
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected

        //tell the view to sort
        _viewModel.SearchMode = (TvSearchViewModel.SearchType)dlgMenu.SelectedIndex;

        //and tell the model that the sort property is changed
        _viewModel.ChangeProperty("TypeLabel");
      }
    }
    #endregion
    #region ContextMenu command class
    /// <summary>
    /// ContextMenuCommand changes the way the view gets sorted
    /// </summary>
    public class ContextMenuCommand : SearchBaseCommand
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SortCommand"/> class.
      /// </summary>
      /// <param name="viewModel">The view model.</param>
      public ContextMenuCommand(TvSearchViewModel viewModel)
        : base(viewModel)
      {
      }

      /// <summary>
      /// Executes the command.
      /// </summary>
      /// <param name="parameter">The parameter.</param>
      public override void Execute(object parameter)
      {
        ProgramModel model = parameter as ProgramModel;
        if (model == null) return;
        TvProgramInfo info = new TvProgramInfo(model.Program);
        ServiceScope.Get<INavigationService>().Navigate(info);
      }
    }
    #endregion
    #endregion

    #region SearchDatabaseModel class
    /// <summary>
    /// Class representing our database model.
    /// It simply retrieves all recordings from the tv database and 
    /// creates a list of RecordingModel
    /// </summary>
    public class SearchDatabaseModel : INotifyPropertyChanged
    {
      #region variables
      public event PropertyChangedEventHandler PropertyChanged;
      List<ProgramModel> _listPrograms = new List<ProgramModel>();
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="SearchDatabaseModel"/> class.
      /// </summary>
      public SearchDatabaseModel()
      {
      }
      /// <summary>
      /// Refreshes the list with the database.
      /// </summary>
      public void Search(SearchType mode, string text)
      {
        _listPrograms.Clear();
        if (text == null) return;
        if (text == "") return;

        IList programs;
        TvBusinessLayer layer = new TvBusinessLayer();
        if (mode == SearchType.Title)
          programs = layer.SearchPrograms(text);
        else if (mode == SearchType.Genre)
          programs = layer.SearchProgramsPerGenre(text, "");
        else
          programs = layer.SearchProgramsByDescription(text);

        foreach (Program program in programs)
        {
          ProgramModel item = new ProgramModel(program);
          _listPrograms.Add(item);
        }
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs("Programs"));
        }
      }

      /// <summary>
      /// Gets the programs.
      /// </summary>
      /// <value>IList containing 0 or more ProgramModel instances.</value>
      public IList Programs
      {
        get
        {
          return _listPrograms;
        }
      }
    }
    #endregion

    #region SearchCollectionView class
    /// <summary>
    /// This class represents the recording view
    /// </summary>
    class SearchCollectionView : ListCollectionView
    {
      #region variables
      SortType _sortMode = SortType.Date;
      private SearchDatabaseModel _model;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="SearchCollectionView"/> class.
      /// </summary>
      /// <param name="model">The database model.</param>
      public SearchCollectionView(SearchDatabaseModel model)
        : base(model.Programs)
      {
        _model = model;
        _model.PropertyChanged += new PropertyChangedEventHandler(OnDatabaseChanged);
      }

      void OnDatabaseChanged(object sender, PropertyChangedEventArgs e)
      {
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }

      /// <summary>
      /// Gets or sets the sort mode.
      /// </summary>
      /// <value>The sort mode.</value>
      public SortType SortMode
      {
        get
        {
          return _sortMode;
        }
        set
        {
          if (_sortMode != value)
          {
            _sortMode = value;
            this.CustomSort = new SortComparer(_sortMode);
          }
        }
      }
    }
    #endregion

    #region SortComparer class
    /// <summary>
    /// Helper class to compare 2 RecordingModels
    /// </summary>
    public class SortComparer : IComparer
    {
      #region variables
      SortType _sortMode;
      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="RecordingComparer"/> class.
      /// </summary>
      /// <param name="sortMode">The sort mode.</param>
      public SortComparer(SortType sortMode)
      {
        _sortMode = sortMode;
      }
      /// <summary>
      /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
      /// </summary>
      /// <param name="x">The first object to compare.</param>
      /// <param name="y">The second object to compare.</param>
      /// <returns>
      /// Value Condition Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y.
      /// </returns>
      /// <exception cref="T:System.ArgumentException">Neither x nor y implements the <see cref="T:System.IComparable"></see> interface.-or- x and y are of different types and neither one can handle comparisons with the other. </exception>
      public int Compare(object x, object y)
      {
        ProgramModel model1 = (ProgramModel)x;
        ProgramModel model2 = (ProgramModel)y;
        switch (_sortMode)
        {
          case SortType.Channel:
            return String.Compare(model1.Channel, model2.Channel, true);
          case SortType.Date:
            return model1.StartTime.CompareTo(model2.StartTime);
            return String.Compare(model1.Genre, model2.Genre, true);
          case SortType.Title:
            return String.Compare(model1.Title, model2.Title, true);
          case SortType.Description:
            return String.Compare(model1.Description, model2.Description, true);
        }
        return 0;
      }
    }
    #endregion
  }
}
