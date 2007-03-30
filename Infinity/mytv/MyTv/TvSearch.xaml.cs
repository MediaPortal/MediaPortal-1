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
    /// Called when mouse enters a button
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void OnMouseEnter(object sender, MouseEventArgs e)
    {
      IInputElement b = sender as IInputElement;
      if (b != null)
      {
        Keyboard.Focus(b);
      }
    }
    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Sets keyboard focus on the first Button in the sample.
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      textboxSearch.TextChanged += new TextChangedEventHandler(textboxSearch_TextChanged);
      Keyboard.Focus(textboxSearch);
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");

      if (TvPlayerCollection.Instance.Count > 0)
      {
        MediaPlayer player = TvPlayerCollection.Instance[0];
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        videoWindow.Fill = videoBrush;
      }
    }

    void textboxSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
      textboxSearch.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateResultsDelegate(OnUpdateResults));
    }
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        //return to previous screen
        this.NavigationService.GoBack();
        return;
      }
    }
    protected void OnScrollKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Up)
      {
        if (_selectedProgram != null && _firstProgram != null)
        {
          if (_selectedProgram.IdProgram == _firstProgram.IdProgram)
          {
            Keyboard.Focus(buttonSort);
            e.Handled = true;
          }
        }
      }
    }

    string GetContentForRightLabel(Program program)
    {
      return program.StartTime.ToString();
    }

    void OnUpdateResults()
    {
      //Grid grid = new Grid();
      gridList.Children.Clear();
      gridList.RowDefinitions.Clear();

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
      int row = 0;
      foreach (Program program in listPrograms)
      {
        if (row > 80) break;
        gridList.RowDefinitions.Add(new RowDefinition());
        Button button = new Button();
        button.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
        button.Height = 54;

        Grid gridSub = new Grid();
        gridSub.VerticalAlignment = VerticalAlignment.Top;
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.ColumnDefinitions.Add(new ColumnDefinition());
        gridSub.RowDefinitions.Add(new RowDefinition());
        gridSub.RowDefinitions.Add(new RowDefinition());
        string logo = Thumbs.GetLogoFileName(program.ReferencedChannel().Name);
        if (System.IO.File.Exists(logo))
        {
          Image image = new Image();
          PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(logo, UriKind.Relative), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
          image.Source = decoder.Frames[0];
          image.Width = 32;
          image.Height = 32;
          Grid.SetColumn(image, 0);
          Grid.SetRow(image, 0);
          Grid.SetRowSpan(image, 2);
          gridSub.Children.Add(image);
        }
        Label label = new Label();
        label.Content = program.Title;
        label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
        Grid.SetColumn(label, 1);
        Grid.SetRow(label, 0);
        Grid.SetColumnSpan(label, 8);
        gridSub.Children.Add(label);

        label = new Label();
        label.Content = program.ReferencedChannel().Name;
        label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
        Grid.SetColumn(label, 1);
        Grid.SetColumnSpan(label, 6);
        Grid.SetRow(label, 1);
        gridSub.Children.Add(label);

        label = new Label();
        label.Content = GetContentForRightLabel(program);
        label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
        label.HorizontalAlignment = HorizontalAlignment.Right;
        label.Margin = new Thickness(0, 0, 20, 0);
        Grid.SetColumn(label, 7);
        Grid.SetColumnSpan(label, 2);
        Grid.SetRow(label, 1);
        gridSub.Children.Add(label);

        gridSub.Loaded += new RoutedEventHandler(gridSub_Loaded);
        button.Content = gridSub;
        button.Tag = program;
        button.GotFocus += new RoutedEventHandler(button_GotFocus);
        button.MouseEnter += new MouseEventHandler(OnMouseEnter);
        button.Click += new RoutedEventHandler(OnProgramClicked);
        if (row == 0)
          _firstProgram = program;
        Grid.SetColumn(button, 0);
        Grid.SetRow(button, row);
        gridList.Children.Add(button);
        row++;
      }
      //gridList.Children.Add(grid);
      gridList.VerticalAlignment = VerticalAlignment.Top;
      Keyboard.Focus(textboxSearch);
    }
    void button_GotFocus(object sender, RoutedEventArgs e)
    {
      Button b = sender as Button;
      if (b == null) return;
      _selectedProgram = b.Tag as Program;

      labelTitle.Text = _selectedProgram.Title;
      labelDescription.Text = _selectedProgram.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", _selectedProgram.StartTime.ToString("HH:mm"), _selectedProgram.EndTime.ToString("HH:mm"));
      labelGenre.Text = _selectedProgram.Genre;
    }

    void gridSub_Loaded(object sender, RoutedEventArgs e)
    {
      Grid g = sender as Grid;
      if (g == null) return;
      g.Width = ((Button)(g.Parent)).ActualWidth;
    }
    void OnProgramClicked(object sender, RoutedEventArgs e)
    {
      Button b = sender as Button;
      if (b == null) return;
      _selectedProgram = b.Tag as Program;
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
      dlgMenu.Header = "Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem("Channel"));
      dlgMenu.Items.Add(new DialogMenuItem("Date"));
      dlgMenu.Items.Add(new DialogMenuItem("Title"));
      dlgMenu.Items.Add(new DialogMenuItem("Description"));
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
      dlgMenu.Header = "Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem("Title"));
      dlgMenu.Items.Add(new DialogMenuItem("Genre"));
      dlgMenu.Items.Add(new DialogMenuItem("Description"));
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