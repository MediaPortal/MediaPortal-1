using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

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
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Sets keyboard focus on the first Button in the sample.
      labelHeader.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 78);//recorded
      buttonView.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 79);//View
      buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 80);//Sort
      buttonSwitch.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 81);//Switch
      buttonCleanup.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 82);//Cleanup
      buttonCompress.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 83);//Compress
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(handleMouse));

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
      gridList.SelectionChanged += new SelectionChangedEventHandler(gridList_SelectionChanged);
      gridList.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(Button_Click), true);
      gridList.KeyDown += new KeyEventHandler(gridList_KeyDown);
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
          UpdateInfoBox();
          return;
        }
        element = element.TemplatedParent as FrameworkElement;
      }
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
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 84);//"Sort:Channel";
          break;
        case SortMode.Date:
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 85);//"Sort:Date";
          break;
        case SortMode.Duration:
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 86);//"Sort:Duration";
          break;
        case SortMode.Genre:
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 87);//"Sort:Genre";
          break;
        case SortMode.Title:
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 88);//"Sort:Title";
          break;
        case SortMode.Watched:
          buttonSort.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 89);//"Sort:Watched";
          break;
      }
      IList recordings = Recording.ListAll();
      List<Recording> listRecordings = new List<Recording>();
      foreach (Recording recording in recordings)
        listRecordings.Add(recording);
      listRecordings.Sort(this);
      DialogMenuItemCollection collection = new DialogMenuItemCollection();
      switch (_viewMode)
      {
        case ViewMode.List:
          {
            gridList.ItemTemplate = (DataTemplate)Application.Current.Resources["itemListTemplate"];
          }
          break;
        case ViewMode.Icon:
          {
            gridList.ItemTemplate = (DataTemplate)Application.Current.Resources["itemIconTemplate"];
          }
          break;
      }
      foreach (Recording recording in listRecordings)
      {
        string logo = System.IO.Path.ChangeExtension(recording.FileName, ".png");
        if (!System.IO.File.Exists(logo))
        {
          logo = "";
        }
        DialogMenuItem item = new DialogMenuItem(logo, recording.Title, recording.Genre, GetContentForRightLabel(recording));
        item.Tag = recording;
        collection.Add(item);
      }
      gridList.ItemsSource = collection;


    }
    void Button_Click(object sender, RoutedEventArgs e)
    {
      if (e.Source != gridList) return;
      OnRecordingClicked();
    }
    void gridList_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        OnRecordingClicked();
        e.Handled = true;
        return;
      }
    }
    /// <summary>
    /// Called when user has clicked on a recording
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnRecordingClicked()
    {
      DialogMenuItem item = gridList.SelectedItem as DialogMenuItem;
      Recording recording = item.Tag as Recording;
      if (recording == null) return;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);//"Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 92)/*Play recording*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 93)/*Delete recording*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 94)/*Settings*/));
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
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);//"Menu";
      dlgMenu.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 95);//"Are you sure to delete this recording ?";
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
        dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);// "Cannot open file";
        dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);// "Error";
        dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 96)/*File not found*/+ " " + recording.FileName;
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
          dlgError.Title = ServiceScope.Get<ILocalisation>().ToString("mytv", 37);// "Cannot open file";
          dlgError.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 10);// "Error";
          dlgError.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 38)/*Unable to open the file*/+ " " + TvPlayerCollection.Instance[0].ErrorMessage;
          dlgError.ShowDialog();
        }
      }
      TvPlayerCollection.Instance.DisposeAll();
    }
    void UpdateInfoBox()
    {
      DialogMenuItem item = gridList.SelectedItem as DialogMenuItem;
      if (item == null) return;
      Recording recording = item.Tag as Recording;
      if (recording == null) return;

      labelTitle.Text = recording.Title;
      labelDescription.Text = recording.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", recording.StartTime.ToString("HH:mm"), recording.EndTime.ToString("HH:mm"));
      labelGenre.Text = recording.Genre;
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
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 97)/*Duration*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 2)/*Channel*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 73)/*Date*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 98)/*Title*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 99)/*Genre*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 100)/*Watched*/));
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
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);// "Menu";
      dlgMenu.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 101);//"This will delete all recordings you have watched. Are you sure?";
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
      if (e.Key == System.Windows.Input.Key.Left)
      {
        Keyboard.Focus(buttonView);
        e.Handled = true;
        return;
      }
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        //return to previous screen
        this.NavigationService.GoBack();
        return;
      }
      if (e.Key == System.Windows.Input.Key.X)
      {
        if (TvPlayerCollection.Instance.Count > 0)
        {
          this.NavigationService.Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
          return;
        }
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