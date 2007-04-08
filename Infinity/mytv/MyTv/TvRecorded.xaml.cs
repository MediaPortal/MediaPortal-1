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

  public partial class TvRecorded : System.Windows.Controls.Page
  {
    #region variables
    TvRecordedViewModel _model;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvRecorded"/> class.
    /// </summary>
    public TvRecorded()
    {
      InitializeComponent();
    }
    #endregion

    #region event handlers
    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      //create new view model
      _model = new TvRecordedViewModel(this);
      //and set the gui's datacontext to our model
      gridMain.DataContext = _model;
      //this.InputBindings.Add(new KeyBinding(_model.FullScreenTv, new KeyGesture(System.Windows.Input.Key.X, ModifierKeys.None)));
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      // Sets keyboard focus on the first Button in the sample.

      //add some event handlers to keep mouse/keyboard focused together...
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onPreviewKeyDown));
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(OnMouseMoveEvent));
      this.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(OnMouseButtonDownEvent), true);
      this.KeyDown += new KeyEventHandler(onKeyDown);

      Thread thumbNailThread = new Thread(new ThreadStart(CreateThumbnailsThread));
      thumbNailThread.Start();
    }

    /// <summary>
    /// Event handler for mouse events
    /// When mouse enters an control, this method will give the control keyboardfocus
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void OnMouseMoveEvent(object sender, MouseEventArgs e)
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
    /// Event handler for OnKeyDown
    /// Handles some basic navigation
    /// Guess this should be done via command binding?
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    protected void onPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Left)
      {
        Keyboard.Focus(buttonView);
        e.Handled = true;
        return;
      }
      if (e.Key == System.Windows.Input.Key.X)
      {
        ICommand command = _model.FullScreenTv;
        if (command.CanExecute(this))
        {
          command.Execute(this);
          e.Handled = true;
        }
        return;
      }
    }
    /// <summary>
    /// Handles the KeyDown event 
    /// When keydown=enter, OnRecordingClicked() gets called
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    void onKeyDown(object sender, KeyEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        ListBox box = e.Source as ListBox;
        OnRecordingClicked(box);
        e.Handled = true;
        return;
      }
    }
    /// <summary>
    /// Handles the mouse button down event
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnMouseButtonDownEvent(object sender, RoutedEventArgs e)
    {
      if ((e.Source as ListBox) == null) return;
      ListBox box = e.Source as ListBox;
      OnRecordingClicked(box);
    }
    #endregion

    #region button handlers
    /// <summary>
    /// Called when user has clicked on a recording
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnRecordingClicked(ListBox listBox)
    {
      RecordingModel item = listBox.SelectedItem as RecordingModel;
      ICommand contextMenu = _model.ContextMenu;
      contextMenu.Execute(item);
    }

    #endregion

    #region thumnail thread
    /// <summary>
    /// background thread which creates thumbnails for all recordings.
    /// </summary>
    void CreateThumbnailsThread()
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      IList recordings = Recording.ListAll();
      foreach (Recording rec in recordings)
      {
        //ThumbnailGenerator generator = new ThumbnailGenerator();
        //if (generator.GenerateThumbnail(rec.FileName))
        //{
        //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateListDelegate(LoadRecordings));
        //}
      }
    }
    #endregion
  }
}