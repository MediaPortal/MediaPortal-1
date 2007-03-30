using System;
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
namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvFullscreen.xaml
  /// </summary>

  public partial class TvFullscreen : System.Windows.Controls.Page
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TvFullscreen"/> class.
    /// </summary>
    public TvFullscreen()
    {
      InitializeComponent();
    }
    /// <summary>
    /// Called when [loaded].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));

      if (TvPlayerCollection.Instance.Count > 0)
      {
        MediaPlayer player = TvPlayerCollection.Instance[0];
        player.MediaEnded += new EventHandler(player_MediaEnded);
        VideoDrawing videoDrawing = new VideoDrawing();
        videoDrawing.Player = player;
        videoDrawing.Rect = new Rect(0, 0, gridMain.ActualWidth, gridMain.ActualHeight);
        DrawingBrush videoBrush = new DrawingBrush();
        videoBrush.Drawing = videoDrawing;
        gridMain.Background = videoBrush;
      }
      Keyboard.Focus(gridMain);
      UpdateOsd();
    }

    /// <summary>
    /// Handles the MediaEnded event of the player control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void player_MediaEnded(object sender, EventArgs e)
    {

      this.NavigationService.GoBack();
    }
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape || e.Key == Key.X)
      {
        //return to previous screen
        e.Handled = true;
        this.NavigationService.GoBack();
        return;
      }
      if (e.Key == Key.Space)
      {
        e.Handled = true;
        if (TvPlayerCollection.Instance.Count > 0)
        {
          TvMediaPlayer player = TvPlayerCollection.Instance[0];
          player.Pause();
          UpdateOsd();
        }
        return;
      }
    }
    /// <summary>
    /// Updates the osd.
    /// </summary>
    void UpdateOsd()
    {
      if (TvPlayerCollection.Instance.Count == 0) return;
      TvMediaPlayer player = TvPlayerCollection.Instance[0];
      if (player.IsPaused)
        gridOSD.Visibility = Visibility.Visible;
      else
        gridOSD.Visibility = Visibility.Hidden;

      if (player.Card != null)
      {
        if (player.Card.IsTimeShifting || player.Card.IsTimeShifting)
        {
          Channel channel = ChannelNavigator.Instance.SelectedChannel;
          Program program = channel.CurrentProgram;
          labelStart.Content = channel.CurrentProgram.StartTime.ToString("HH:mm");
          labelEnd.Content = channel.CurrentProgram.EndTime.ToString("HH:mm");
        }
        else
        {
          labelStart.Content = "00:00";
          if (player.Duration.Minutes < 10)
            labelEnd.Content = String.Format("{0}:0{1}", player.Duration.Hours, player.Duration.Minutes);
          else
            labelEnd.Content = String.Format("{0}:{1}", player.Duration.Hours, player.Duration.Minutes);
        }
      }
      else
      {
        labelStart.Content = "00:00";
        if (player.Duration.Minutes < 10)
          labelEnd.Content = String.Format("{0}:0{1}", player.Duration.Hours, player.Duration.Minutes);
        else
          labelEnd.Content = String.Format("{0}:{1}", player.Duration.Hours, player.Duration.Minutes);
      }
      if (player.IsPaused)
        labelState.Content = "|| Paused";
      else
        labelState.Content = "";


    }
  }
}