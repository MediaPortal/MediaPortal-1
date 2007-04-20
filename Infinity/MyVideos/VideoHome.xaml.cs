//
// TODO: - View Film strip, list, icon
//       - Switch view
//       - Playlist
//

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

using ProjectInfinity;
using ProjectInfinity.Menu;
using ProjectInfinity.Messaging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Players;
using System.IO;
using System.Windows.Markup;

namespace MyVideos
{
  /// <summary>
  /// Interaction logic for TestPage.xaml
  /// </summary>

  public partial class VideoHome : System.Windows.Controls.Page
  {
    VideoHomeViewModel _model;

    public VideoHome()
    {
      InitializeComponent();

      ServiceScope.Get<IMessageBroker>().Register(this);
    }

    /// <summary>
    /// When the page is loaded, this event is called to do some
    /// basic things like viewmodels, keybindings etc etc
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      gridMain.Children.Clear();
      using (FileStream steam = new FileStream(@"skin\default\myvideos\VideoHome.xaml", FileMode.Open, FileAccess.Read))
      {
        UIElement documentRoot = (UIElement)XamlReader.Load(steam);
        gridMain.Children.Add(documentRoot);
      }

      // Add a input binding for the "back"-command
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));

      // View Model
      _model = new VideoHomeViewModel(this);
      gridMain.DataContext = _model;

      // Keyboard
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      //Keyboard.Focus(buttonView);

      // Mouse
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMoveEvent));
      Mouse.AddMouseDownHandler(this, new MouseButtonEventHandler(OnMouseButtonDownEvent));

      this.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(OnMouseButtonDownEvent), true);
      this.KeyDown += new KeyEventHandler(onKeyDown);

      // Display the video in the background (if there is any)
      // This one we might need to rewrite to get the (buttonVideo.Visibility...) away / or maybe we should just
      // show it even if the background is playing video, what do you think?
      if (ServiceScope.Get<IPlayerCollectionService>().Count > 0)
      {
        VideoPlayer player = (VideoPlayer)ServiceScope.Get<IPlayerCollectionService>()[0];

        if (player.HasMedia)
        {
          gridMain.Background = _model.VideoBrush;
          //buttonVideo.Visibility = Visibility.Hidden;
        }
      }
    }

    /// <summary>
    /// When we click the video window, this event is called
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnVideoWindowClicked(object sender, EventArgs args)
    {
      this.NavigationService.Navigate(new Uri("/MyVideos;component/VideoFullscreen.xaml", UriKind.Relative));
    }

    /// <summary>
    /// Handles mouse clicks
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseButtonDownEvent(object sender, RoutedEventArgs e)
    {
      if ((e.Source as ListBox) != null)
      {
        ListBox box = (ListBox)e.Source;

        if ((e as MouseButtonEventArgs).RightButton == MouseButtonState.Pressed)
        {
          
        }
        else
        {
          OnVideoItemClicked(box);
          e.Handled = true;

          return;
        }
      }
    }

    /// <summary>
    /// Handles mouse moves, focusing of objects etc
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseMoveEvent(object sender, MouseEventArgs e)
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
          Keyboard.Focus((ListBoxItem)element);
          return;
        }
        element = element.TemplatedParent as FrameworkElement;
      }
    }

    /// <summary>
    /// When a button gets pressed, this event is called
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      // If we've pressed enter, call for VideoItemClicked()
      /*if ((e.Key == Key.Enter) &&
          (e.Source as ListBox) != null)
      {
        ListBox box = (ListBox)e.Source;
        OnVideoItemClicked(box);
        e.Handled = true;

        return;
      }*/

      if ((e.Key == Key.I) &&
          (e.Source as ProjectInfinity.Controls.ListBox) != null)
      {
        // add the item to the playlist

        ProjectInfinity.Controls.ListBox box = (ProjectInfinity.Controls.ListBox)e.Source;

        Dialogs.MpMenu dlgMenu = new Dialogs.MpMenu();
        dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlgMenu.Owner = Window.GetWindow(this);
        dlgMenu.Items.Clear();
        dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68); // Menu
        dlgMenu.SubTitle = (box.SelectedItem as VideoModel).Title;
        dlgMenu.Items.Add(new Dialogs.DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 30))); // Add to playlist
        dlgMenu.Items.Add(new Dialogs.DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 29))); // View information
        dlgMenu.Items.Add(new Dialogs.DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 27))); // Download information
        dlgMenu.Items.Add(new Dialogs.DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("myvideos", 28))); // Delete from disk
        dlgMenu.ShowDialog();

        switch (dlgMenu.SelectedIndex)
        {
          case 0:
            // Add to playlist
            ICommand addToPlaylist = new AddToPlaylistCommand(_model);
            addToPlaylist.Execute((VideoModel)box.SelectedItem);
            break;
        }
      }
    }

    /// <summary>
    /// Called when a user clicks on a video in the listbox.
    /// </summary>
    /// <param name="listBox"></param>
    private void OnVideoItemClicked(ListBox listBox)
    {
      VideoModel item = (VideoModel)listBox.SelectedItem;

      ICommand playCommand = _model.Play;
      playCommand.Execute(item);

      OnVideoWindowClicked(null, new EventArgs());
    }
  }
}