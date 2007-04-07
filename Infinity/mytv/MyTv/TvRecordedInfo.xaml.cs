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
using Dialogs;
using TvDatabase;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvRecordedInfo.xaml
  /// </summary>

  public partial class TvRecordedInfo : System.Windows.Controls.Page
  {
    Recording _recording;
    public TvRecordedInfo(Recording recording)
    {
      _recording = recording;
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
      Keyboard.Focus(buttonKeepUntil);
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
      labelHeader.Content = ServiceScope.Get<ILocalisation>().ToString("mytv", 78);// "recorded";

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
      labelTitle.Text = _recording.Title;
      labelDescription.Text = _recording.Description;
      labelStartEnd.Text = String.Format("{0}-{1}", _recording.StartTime.ToString("HH:mm"), _recording.EndTime.ToString("HH:mm"));
      labelGenre.Text = _recording.Genre;
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
    void OnKeepUntil(object sender, EventArgs args)
    { 
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);//"Menu";
      dlgMenu.SubTitle = ServiceScope.Get<ILocalisation>().ToString("mytv", 47);// Keep Until";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 69)/*Until space needed*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 70)/*Until watched*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 71)/*Until Date*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 72)/*Always*/));
      dlgMenu.SelectedIndex = (int)_recording.KeepUntil;
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      _recording.KeepUntil = dlgMenu.SelectedIndex;
      _recording.Persist();
      if (dlgMenu.SelectedIndex == 2)
      {
        dlgMenu = new MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = w;
        dlgMenu.Items.Clear();
        dlgMenu.Header = "Menu";
        dlgMenu.SubTitle = "Date";
        int selected = 0;
        for (int days = 1; days <= 31; days++)
        {
          DateTime dt = DateTime.Now.AddDays(days);
          dlgMenu.Items.Add(new DialogMenuItem(dt.ToLongDateString()));
          if (dt.Date == _recording.KeepUntilDate) selected = days - 1;
        }
        dlgMenu.ShowDialog();
        if (dlgMenu.SelectedIndex < 0) return;//nothing selected
        int daysChoosen = dlgMenu.SelectedIndex + 1;
        _recording.KeepUntilDate = DateTime.Now.AddDays(daysChoosen);
        _recording.Persist();
      }
    }
  }
}