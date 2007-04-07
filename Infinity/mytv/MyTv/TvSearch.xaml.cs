using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvDatabase;
using TvControl;
using Dialogs;
using MCEControls;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvNewSchedule.xaml
  /// </summary>

  public partial class TvSearch : System.Windows.Controls.Page, IComparer<Program>
  {
    public enum SearchType
    {
      Title,
      Genre,
      Description,
    }
    enum SortMode
    {
      Channel,
      Date,
      Title,
      Description
    }
    Program _firstProgram;
    Program _selectedProgram;
    SearchType _searchType = SearchType.Title;
    SortMode _sortMode = SortMode.Title;
    private delegate void UpdateResultsDelegate();
    public TvSearch()
    {
      InitializeComponent();
    }
    public TvSearch(SearchType searchType)
    {
      _searchType = searchType;
      InitializeComponent();
    }

    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Sets keyboard focus on the first Button in the sample.
      labelHeader.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 107);
      buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 80);
      buttonType.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 108);
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      textboxSearch.TextChanged += new TextChangedEventHandler(textboxSearch_TextChanged);
      Keyboard.Focus(textboxSearch);
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");

      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        MediaPlayer player = (MediaPlayer)ServiceScope.Get<IPlayerCollectionService>()[0].UnderlyingPlayer;
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        videoWindow.Fill = videoBrush;
      }
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(handleMouse));
      gridList.SelectionChanged += new SelectionChangedEventHandler(gridList_SelectionChanged);
      gridList.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(Button_Click), true);
      gridList.KeyDown += new KeyEventHandler(gridList_KeyDown);
      textboxSearch.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateResultsDelegate(OnUpdateResults));

    }

    void gridList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      UpdateInfoBox();
    }
    void handleMouse(object sender, MouseEventArgs e)
    {
      FrameworkElement element = Mouse.DirectlyOver as FrameworkElement;
      while (element != null)
      {
        if (element as Button != null)
        {
          Keyboard.Focus((Button)element);
          return;
        }
        if (element as ListBoxItem != null)
        {
          gridList.SelectedItem = element.DataContext;
          Keyboard.Focus((ListBoxItem)element);
          //UpdateInfoBox();
          return;
        }
        element = element.TemplatedParent as FrameworkElement;
      }
    }
    void Button_Click(object sender, RoutedEventArgs e)
    {
      if (e.Source != gridList) return;
      OnProgramClicked();
    }
    void gridList_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Up)
      {
        if (gridList.SelectedIndex == 0)
        {
          Keyboard.Focus(buttonType);
          e.Handled = true;
          return;
        }
      }
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        OnProgramClicked();
        e.Handled = true;
        return;
      }
    }
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        //return to previous screen
        this.NavigationService.GoBack();
        return;
      }
      if (e.Key == System.Windows.Input.Key.X)
      {
        if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
        {
          this.NavigationService.Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
          return;
        }
      }
    }

    void textboxSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
      textboxSearch.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateResultsDelegate(OnUpdateResults));
    }


    string GetContentForRightLabel(Program program)
    {
      return program.StartTime.ToString();
    }

    void OnUpdateResults()
    {

      if (textboxSearch.Text == "") return;
      TvBusinessLayer layer = new TvBusinessLayer();
      IList programs;
      if (_searchType == SearchType.Title)
        programs = layer.SearchPrograms(textboxSearch.Text);
      else if (_searchType == SearchType.Genre)
        programs = layer.SearchProgramsPerGenre(textboxSearch.Text, "");
      else
        programs = layer.SearchProgramsByDescription(textboxSearch.Text);
      List<Program> listPrograms = new List<Program>();
      foreach (Program program in programs)
        listPrograms.Add(program);
      listPrograms.Sort(this);
      DialogMenuItemCollection collection = new DialogMenuItemCollection();
      foreach (Program program in listPrograms)
      {
        string logo = String.Format(@"{0}\{1}",System.IO.Directory.GetCurrentDirectory(),Thumbs.GetLogoFileName(program.ReferencedChannel().Name));
        if (!System.IO.File.Exists(logo))
        {
          logo="";
        }
        DialogMenuItem item = new DialogMenuItem(logo,program.Title,program.ReferencedChannel().Name,GetContentForRightLabel(program));
        item.Tag = program;
        collection.Add(item);

      }
      gridList.ItemsSource = collection;
      Keyboard.Focus(textboxSearch);
    }
    void UpdateInfoBox()
    {
      DialogMenuItem item = gridList.SelectedItem as DialogMenuItem;
      if (item == null) return;
      _selectedProgram = item.Tag as Program;

      labelTitle.Text = _selectedProgram.Title;
      labelDescription.Text = _selectedProgram.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", _selectedProgram.StartTime.ToString("HH:mm"), _selectedProgram.EndTime.ToString("HH:mm"));
      labelGenre.Text = _selectedProgram.Genre;
    }

    void OnProgramClicked()
    {
      DialogMenuItem item = gridList.SelectedItem as DialogMenuItem;
      if (item == null) return;
      _selectedProgram = item.Tag as Program;
      TvProgramInfo info = new TvProgramInfo(_selectedProgram);
      NavigationService.Navigate(info);
    }
    void OnSortClicked(object sender, RoutedEventArgs e)
    {
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68); //"Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 2)/*Channel*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 73)/*"Date*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 98)/*"Title*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 109)/*"Description*/));
      dlgMenu.SelectedIndex = (int)_sortMode;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      _sortMode = (SortMode)dlgMenu.SelectedIndex;
      OnUpdateResults();
    }
    void OnTypeClicked(object sender, RoutedEventArgs e)
    {
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68); //"Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 98)/*"Title*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 99)/*Genre*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 109)/*"Description*/));
      dlgMenu.SelectedIndex = (int)_searchType;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      _searchType = (SearchType)dlgMenu.SelectedIndex;
      OnUpdateResults();
    }
    #region IComparer<Recording> Members

    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
    /// </returns>
    public int Compare(Program x, Program y)
    {
      switch (_sortMode)
      {
        case SortMode.Channel:
          return String.Compare(x.ReferencedChannel().Name.ToString(), y.ReferencedChannel().Name, true);
        case SortMode.Date:
          return x.StartTime.CompareTo(y.StartTime);
        case SortMode.Title:
          return String.Compare(x.Title, y.Title, true);
        case SortMode.Description:
          return String.Compare(x.Description, y.Description, true);
      }
      return 0;
    }

    #endregion
  }
}