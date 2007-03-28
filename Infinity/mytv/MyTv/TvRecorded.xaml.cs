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
using Gentle.Common;
using Gentle.Framework;
using Dialogs;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvRecorded.xaml
  /// </summary>

  public partial class TvRecorded : System.Windows.Controls.Page, IComparer<Recording>
  {
    enum ViewMode
    {
      List,
      Icon
    };
    enum SortMode
    {
      Duration,
      Channel,
      Date,
      Title,
      Genre,
      Watched
    };
    ViewMode _viewMode = ViewMode.List;
    SortMode _sortMode = SortMode.Date;

    public TvRecorded()
    {
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
      Keyboard.Focus(buttonView);

      if (ChannelNavigator.Instance.Card != null)
      {
        if (ChannelNavigator.Instance.Card.IsTimeShifting)
        {
          Uri uri = new Uri(ChannelNavigator.Instance.Card.TimeShiftFileName, UriKind.Absolute);
          for (int i = 0; i < TvPlayerCollection.Instance.Count; ++i)
          {
            if (TvPlayerCollection.Instance[i].Source == uri)
            {
              MediaPlayer player = TvPlayerCollection.Instance[i];
              VideoDrawing videoDrawing = new VideoDrawing();
              videoDrawing.Player = player;
              videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
              DrawingBrush videoBrush = new DrawingBrush();
              videoBrush.Drawing = videoDrawing;
              videoWindow.Fill = videoBrush;
              break;
            }
          }
        }
      }

      LoadRecordings();
    }
    string GetContentForRightLabel(Recording recording)
    {
      switch (_sortMode)
      {
        case SortMode.Channel:
          return recording.ReferencedChannel().Name;
        case SortMode.Date:
          return recording.StartTime.ToLongDateString();
        case SortMode.Duration:
          {
            TimeSpan ts = recording.EndTime - recording.StartTime;
            if (ts.Minutes < 10)
              return String.Format("{0}:0{1}", ts.Hours, ts.Minutes);
            else
              return String.Format("{0}:{1}", ts.Hours, ts.Minutes);
          }
        case SortMode.Genre:
          return recording.Genre;
        case SortMode.Title:
          {
            TimeSpan ts = recording.EndTime - recording.StartTime;
            if (ts.Minutes < 10)
              return String.Format("{0}:0{1}", ts.Hours, ts.Minutes);
            else
              return String.Format("{0}:{1}", ts.Hours, ts.Minutes);
          }
        case SortMode.Watched:
          return recording.TimesWatched.ToString();
      }
      return "";
    }
    void LoadRecordings()
    {
      Grid grid = new Grid();
      gridList.Children.Clear();
      IList recordings = Recording.ListAll();
      List<Recording> listRecordings = new List<Recording>();
      foreach (Recording recording in recordings)
        listRecordings.Add(recording);
      listRecordings.Sort(this);
      int row = 0;
      foreach (Recording recording in listRecordings)
      {
        grid.RowDefinitions.Add(new RowDefinition());
        Button button = new Button();
        button.Template = (ControlTemplate)Application.Current.Resources["MpButton"];

        switch (_viewMode)
        {
          case ViewMode.List:
            {
              buttonView.Content = "View:List";
              Grid gridSub = new Grid();
              gridSub.ColumnDefinitions.Add(new ColumnDefinition());
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
              string logo = Thumbs.GetLogoFileName(recording.ReferencedChannel().Name);
              if (System.IO.File.Exists(logo))
              {
                Image image = new Image();
                PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(logo, UriKind.Relative), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                image.Source = decoder.Frames[0];
                image.Width = 32;
                image.Height = 32;
                Grid.SetColumn(image, 0);
                Grid.SetRow(image, 0);
                gridSub.Children.Add(image);
              }
              Label label = new Label();
              label.Content = recording.Title;
              label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
              Grid.SetColumn(label, 1);
              Grid.SetRow(label, 0);
              Grid.SetColumnSpan(label, 8);
              gridSub.Children.Add(label);
              label = new Label();
              label.Content = GetContentForRightLabel(recording);
              label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
              label.HorizontalAlignment = HorizontalAlignment.Right;
              //label.Margin = new Thickness(0, 0, 60, 0);
              Grid.SetColumn(label, 7);
              Grid.SetColumnSpan(label, 2);
              Grid.SetRow(label, 0);
              gridSub.Children.Add(label);
              gridSub.Loaded += new RoutedEventHandler(gridSub_Loaded);
              button.Content = gridSub;
            }
            break;
          case ViewMode.Icon:
            {
              //icon view...
              buttonView.Content = "View:Icons";
              Grid gridSub = new Grid();
              gridSub.ColumnDefinitions.Add(new ColumnDefinition());
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
              string logo = Thumbs.GetLogoFileName(recording.ReferencedChannel().Name);
              if (System.IO.File.Exists(logo))
              {
                Image image = new Image();
                PngBitmapDecoder decoder = new PngBitmapDecoder(new Uri(logo, UriKind.Relative), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                image.Source = decoder.Frames[0];
                Grid.SetColumn(image, 0);
                Grid.SetRow(image, 0);
                Grid.SetRowSpan(image, 2);
                gridSub.Children.Add(image);
              }
              Label label = new Label();
              label.Content = recording.Title;
              label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
              Grid.SetColumn(label, 1);
              Grid.SetRow(label, 0);
              Grid.SetColumnSpan(label, 8);
              gridSub.Children.Add(label);

              label = new Label();
              label.Content = recording.Genre;
              label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
              Grid.SetColumn(label, 1);
              Grid.SetColumnSpan(label, 6);
              Grid.SetRow(label, 1);
              gridSub.Children.Add(label);

              label = new Label();
              label.Content = GetContentForRightLabel(recording);
              label.Style = (Style)Application.Current.Resources["LabelSmallStyleWhite"];
              label.HorizontalAlignment = HorizontalAlignment.Right;
              //label.Margin = new Thickness(0, 0, 60, 0);
              Grid.SetColumn(label, 7);
              Grid.SetColumnSpan(label, 2);
              Grid.SetRow(label, 1);
              gridSub.Children.Add(label);
              gridSub.Loaded += new RoutedEventHandler(gridSub_Loaded);


              button.Content = gridSub;
            }
            break;
        }
        button.Tag = recording;
        button.GotFocus += new RoutedEventHandler(button_GotFocus);
        button.MouseEnter += new MouseEventHandler(OnMouseEnter);
        //label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
        Grid.SetColumn(button, 0);
        Grid.SetRow(button, row);
        grid.Children.Add(button);
        row++;
      }
      gridList.Children.Add(grid);
      gridList.VerticalAlignment = VerticalAlignment.Top;
    }

    void button_GotFocus(object sender, RoutedEventArgs e)
    {
      Button b = sender as Button;
      if (b == null) return;
      Recording recording = b.Tag as Recording;
      if (recording == null) return;

      labelTitle.Text = recording.Title;
      labelDescription.Text = recording.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", recording.StartTime.ToString("HH:mm"), recording.EndTime.ToString("HH:mm"));
      labelGenre.Text = recording.Genre;
    }

    void gridSub_Loaded(object sender, RoutedEventArgs e)
    {
      Grid g = sender as Grid;
      if (g == null) return;
      g.Width = ((Button)(g.Parent)).ActualWidth;
    }

    void OnViewClicked(object sender, RoutedEventArgs e)
    {
      switch (_viewMode)
      {
        case ViewMode.List:
          _viewMode = ViewMode.Icon;
          break;
        case ViewMode.Icon:
          _viewMode = ViewMode.List;
          break;
      }
      LoadRecordings();
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
      dlgMenu.Items.Add(new DialogMenuItem("Duration"));
      dlgMenu.Items.Add(new DialogMenuItem("Channel"));
      dlgMenu.Items.Add(new DialogMenuItem("Date"));
      dlgMenu.Items.Add(new DialogMenuItem("Title"));
      dlgMenu.Items.Add(new DialogMenuItem("Genre"));
      dlgMenu.Items.Add(new DialogMenuItem("Watched"));
      dlgMenu.SelectedIndex = (int)_sortMode;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      _sortMode = (SortMode)dlgMenu.SelectedIndex;
      LoadRecordings();
    }

    #region IComparer<Recording> Members

    public int Compare(Recording x, Recording y)
    {
      switch (_sortMode)
      {
        case SortMode.Channel:
          return String.Compare(x.ReferencedChannel().Name.ToString(), y.ReferencedChannel().Name, true);
        case SortMode.Date:
          return x.StartTime.CompareTo(y.StartTime);
        case SortMode.Duration:
          TimeSpan t1 = x.EndTime - x.StartTime;
          TimeSpan t2 = y.EndTime - y.StartTime;
          return t1.CompareTo(t2);
        case SortMode.Genre:
          return String.Compare(x.Genre, y.Genre, true);
        case SortMode.Title:
          return String.Compare(x.Title, y.Title, true);
        case SortMode.Watched:
          return x.TimesWatched.CompareTo(y.TimesWatched);
      }
      return 0;
    }

    #endregion
  }
}