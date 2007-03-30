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
using System.Threading;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using Dialogs;
using TvControl;

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
    private delegate void MediaPlayerErrorDelegate();
    private delegate void UpdateListDelegate();

    /// <summary>
    /// Initializes a new instance of the <see cref="TvRecorded"/> class.
    /// </summary>
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
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Keyboard.Focus(buttonView);
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

      LoadRecordings();
      Thread thumbNailThread = new Thread(new ThreadStart(CreateThumbnailsThread));
      thumbNailThread.Start();

    }
    /// <summary>
    /// Gets the content for right label of each recording.
    /// This depends on the current sort mode
    /// </summary>
    /// <param name="recording">The recording.</param>
    /// <returns></returns>
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
    /// <summary>
    /// Loads the recordings and shows them onscreen.
    /// </summary>
    void LoadRecordings()
    {
      switch (_sortMode)
      {
        case SortMode.Channel:
          buttonSort.Content = "Sort:Channel";
          break;
        case SortMode.Date:
          buttonSort.Content = "Sort:Date";
          break;
        case SortMode.Duration:
          buttonSort.Content = "Sort:Duration";
          break;
        case SortMode.Genre:
          buttonSort.Content = "Sort:Genre";
          break;
        case SortMode.Title:
          buttonSort.Content = "Sort:Title";
          break;
        case SortMode.Watched:
          buttonSort.Content = "Sort:Watched";
          break;
      }
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
              string logo = System.IO.Path.ChangeExtension(recording.FileName, ".png");
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
              string logo = System.IO.Path.ChangeExtension(recording.FileName, ".png");
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
        button.Click += new RoutedEventHandler(OnRecordingClicked);
        //label.Style = (Style)Application.Current.Resources["LabelNormalStyleWhite"];
        Grid.SetColumn(button, 0);
        Grid.SetRow(button, row);
        grid.Children.Add(button);
        row++;
      }
      gridList.Children.Add(grid);
      gridList.VerticalAlignment = VerticalAlignment.Top;
    }

    /// <summary>
    /// Called when user has clicked on a recording
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnRecordingClicked(object sender, RoutedEventArgs e)
    {
      Button b = sender as Button;
      if (b == null) return;
      Recording recording = b.Tag as Recording;
      if (recording == null) return;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = "Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem("Play recording"));
      dlgMenu.Items.Add(new DialogMenuItem("Delete recording"));
      dlgMenu.Items.Add(new DialogMenuItem("Settings"));
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      switch (dlgMenu.SelectedIndex)
      {
        case 0:
          PlayRecording(recording);
          break;
        case 1:
          DeleteRecording(recording);
          break;
        case 2:
          {
            TvRecordedInfo infopage = new TvRecordedInfo(recording);
            this.NavigationService.Navigate(infopage);
          }
          break;
      }
    }
    void DeleteRecording(Recording recording)
    {
      MpDialogYesNo dlgMenu = new MpDialogYesNo();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Header = "Menu";
      dlgMenu.Content = "Are you sure to delete this recording ?";
      dlgMenu.ShowDialog();
      if (dlgMenu.DialogResult == DialogResult.No) return;

      TvServer server = new TvServer();
      server.DeleteRecording(recording.IdRecording);
      LoadRecordings();
    }
    void PlayRecording(Recording recording)
    {
      if (!System.IO.File.Exists(recording.FileName))
      {
        MpDialogOk dlgError = new MpDialogOk();
        Window w = Window.GetWindow(this);
        dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgError.Owner = w;
        dlgError.Title = "Cannot open file";
        dlgError.Header = "Error";
        dlgError.Content = "File not found " + recording.FileName;
        dlgError.ShowDialog();
        return;
      }
      videoWindow.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
      TvPlayerCollection.Instance.DisposeAll();
      TvMediaPlayer player = TvPlayerCollection.Instance.Get(null, recording.FileName);
      player.MediaFailed += new EventHandler<ExceptionEventArgs>(_mediaPlayer_MediaFailed);

      //create video drawing which draws the video in the video window
      VideoDrawing videoDrawing = new VideoDrawing();
      videoDrawing.Player = player;
      videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
      DrawingBrush videoBrush = new DrawingBrush();
      videoBrush.Drawing = videoDrawing;
      videoWindow.Fill = videoBrush;
      videoDrawing.Player.Play();
    }
    /// <summary>
    /// Handles the MediaFailed event of the _mediaPlayer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Media.ExceptionEventArgs"/> instance containing the event data.</param>
    void _mediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
    {
      // media player failed to open file
      // show error dialog (via dispatcher)
      buttonView.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new MediaPlayerErrorDelegate(OnMediaPlayerError));
    }

    /// <summary>
    /// Called when media player has an error condition
    /// show messagebox to user and close media playback
    /// </summary>
    void OnMediaPlayerError()
    {
      videoWindow.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
      if (TvPlayerCollection.Instance.Count > 0)
      {
        if (TvPlayerCollection.Instance[0].HasError)
        {
          MpDialogOk dlgError = new MpDialogOk();
          Window w = Window.GetWindow(this);
          dlgError.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          dlgError.Owner = w;
          dlgError.Title = "Cannot open file";
          dlgError.Header = "Error";
          dlgError.Content = "Unable to open the file " + TvPlayerCollection.Instance[0].ErrorMessage;
          dlgError.ShowDialog();
        }
      }
      TvPlayerCollection.Instance.DisposeAll();
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

    /// <summary>
    /// Called when view button gets clicked
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Called when sort button is clicked
    /// show sort dialog-menu
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Called when cleanup button is clicked
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnCleanupClicked(object sender, RoutedEventArgs e)
    {
      MpDialogYesNo dlgMenu = new MpDialogYesNo();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Header = "Menu";
      dlgMenu.Content = "This will delete all recordings you have watched. Are you sure?";
      dlgMenu.ShowDialog();
      if (dlgMenu.DialogResult == DialogResult.No) return;
      IList itemlist = Recording.ListAll();
      foreach (Recording rec in itemlist)
      {
        if (rec.TimesWatched > 0)
        {
          TvServer server = new TvServer();
          server.DeleteRecording(rec.IdRecording);
        }
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
    }
    /// <summary>
    /// background thread which creates thumbnails for all recordings.
    /// </summary>
    void CreateThumbnailsThread()
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      IList recordings = Recording.ListAll();
      foreach (Recording rec in recordings)
      {
        ThumbnailGenerator generator = new ThumbnailGenerator();
        if (generator.GenerateThumbnail(rec.FileName))
        {
          buttonView.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateListDelegate(LoadRecordings));
        }
      }
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