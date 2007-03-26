using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Dialogs;
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using DirectShowLib;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>

  public partial class TvHome : System.Windows.Controls.Page//System.Windows.Window
  {
    #region variables
    TvMediaPlayer _mediaPlayer;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvHome"/> class.
    /// </summary>
    public TvHome()
    {
      this.ShowsNavigationUI = false;
      WindowTaskbar.Show();
      InitializeComponent();
    }
    #endregion


    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Sets keyboard focus on the first Button in the sample.
      Keyboard.Focus(buttonTvGuide);

      try
      {
        RemoteControl.HostName = UserSettings.GetString("tv", "serverHostName");

        string connectionString, provider;
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);

        XmlDocument doc = new XmlDocument();
        doc.Load("gentle.config");
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
        node.InnerText = connectionString;
        nodeProvider.InnerText = provider;
        doc.Save("gentle.config");
        ChannelNavigator.Instance.Initialize();

        int cards = RemoteControl.Instance.Cards;
        IList channels = Channel.ListAll();
      }
      catch (Exception)
      {
        this.NavigationService.Navigate(new Uri("TvSetup.xaml", UriKind.Relative));
      }
      UpdateInfoBox();
      if (ChannelNavigator.Instance.Card != null)
      {
        if (ChannelNavigator.Instance.Card.IsTimeShifting)
        {
          Uri uri = new Uri(ChannelNavigator.Instance.Card.TimeShiftFileName, UriKind.Absolute);
          for (int i = 0; i < TvPlayerCollection.Instance.Count; ++i)
          {
            if (TvPlayerCollection.Instance[i].Source == uri)
            {
              _mediaPlayer = TvPlayerCollection.Instance[i];
              VideoDrawing videoDrawing = new VideoDrawing();
              videoDrawing.Player = _mediaPlayer;
              videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
              DrawingBrush videoBrush = new DrawingBrush();
              videoBrush.Drawing = videoDrawing;
              videoWindow.Fill = videoBrush;
              break;
            }
          }
        }
      }
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
    /// Called when tv guide button is clicked.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnTvGuideClicked(object sender, EventArgs args)
    {
      this.NavigationService.Navigate(new Uri("TvGuide.xaml", UriKind.Relative));
    }

    /// <summary>
    /// Called when recordnow button gets clicked
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnRecordClicked(object sender, EventArgs args)
    {
      Window window = (Window)this.Parent;
      if (window.WindowState == System.Windows.WindowState.Maximized)
      {
        window.ShowInTaskbar = true;
        WindowTaskbar.Show(); ;
        window.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
        window.WindowState = System.Windows.WindowState.Normal;
      }
      else
      {
        window.ShowInTaskbar = false;
        window.WindowStyle = System.Windows.WindowStyle.None;
        WindowTaskbar.Hide(); ;
        window.WindowState = System.Windows.WindowState.Maximized;
      }
    }

    /// <summary>
    /// Called when tv on/off button gets clicked
    /// 
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnTvOnOff(object sender, EventArgs args)
    {
      if (_mediaPlayer != null)
      {
        videoWindow.Fill = new SolidColorBrush(Color.FromArgb(0xff, 0, 0, 0));
        _mediaPlayer.Dispose();
        _mediaPlayer = null;
      }
      else
      {
        if (ChannelNavigator.Instance.SelectedChannel != null)
        {
          ViewChannel(ChannelNavigator.Instance.SelectedChannel);
        }
      }
    }

    /// <summary>
    /// Called when Channel button gets clicked.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnChannelClicked(object sender, EventArgs args)
    {
      //show dialog menu showing all channels of current tvgroup
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      IList groups = ChannelNavigator.Instance.CurrentGroup.ReferringGroupMap();
      foreach (GroupMap map in groups)
      {
        dlgMenu.Items.Add(new DialogMenuItem(map.ReferencedChannel().Name));
      }

      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected

      //get the selected tv channel
      GroupMap selectedMap = groups[dlgMenu.SelectedIndex] as GroupMap;
      ChannelNavigator.Instance.SelectedChannel = selectedMap.ReferencedChannel();

      //and view it
      ViewChannel(ChannelNavigator.Instance.SelectedChannel);
    }

    /// <summary>
    /// Called when scheduled button gets clicked].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnScheduledClicked(object sender, EventArgs args)
    {
      if (_mediaPlayer == null) return;
      _mediaPlayer.Position = new TimeSpan(0, 0, 0);
    }

    /// <summary>
    /// Start viewing the tv channel
    /// </summary>
    /// <param name="channel">The channel.</param>
    void ViewChannel(Channel channel)
    {
      //tell server to start timeshifting the channel
      TvServer server = new TvServer();
      VirtualCard card;

      User user = new User();
      TvResult succeeded = TvResult.Succeeded;
      succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);
      if (succeeded == TvResult.Succeeded)
      {
        //timeshifting worked, now view the channel
        ChannelNavigator.Instance.Card = card;
        //do we already have a media player ?
        if (_mediaPlayer == null)
        {
          //no then create a new media player 
          _mediaPlayer = TvPlayerCollection.Instance.Get(card, new Uri(card.TimeShiftFileName, UriKind.Absolute));

          //create video drawing which draws the video in the video window
          VideoDrawing videoDrawing = new VideoDrawing();
          videoDrawing.Player = _mediaPlayer;
          videoDrawing.Rect = new Rect(0, 0, videoWindow.ActualWidth, videoWindow.ActualHeight);
          DrawingBrush videoBrush = new DrawingBrush();
          videoBrush.Drawing = videoDrawing;
          videoWindow.Fill = videoBrush;
          videoDrawing.Player.Play();
        }
        else
        {
          //we already have a media player, seek to end of file
          if (_mediaPlayer.NaturalDuration.HasTimeSpan)
            _mediaPlayer.Position = _mediaPlayer.NaturalDuration.TimeSpan;
        }
        //set tv button on
        buttonTvOnOff.IsChecked = true;

        //update screen
        UpdateInfoBox();
      }
      else
      {
        //close media player
        if (_mediaPlayer != null)
        {
          _mediaPlayer.Dispose();
          _mediaPlayer = null;
        }
        //tun tv button off
        buttonTvOnOff.IsChecked = false;

        //show error to user
        MpDialogOk dlg = new MpDialogOk();
        Window w = Window.GetWindow(this);
        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        dlg.Owner = w;
        dlg.Title = "Failed to start TV";
        dlg.Header = "Error";
        switch (succeeded)
        {
          case TvResult.AllCardsBusy:
            dlg.Content = "All cards are currently busy";
            break;
          case TvResult.CardIsDisabled:
            dlg.Content = "Card is disabled";
            break;
          case TvResult.ChannelIsScrambled:
            dlg.Content = "Channel is scrambled";
            break;
          case TvResult.ChannelNotMappedToAnyCard:
            dlg.Content = "Channel is not mapped to any tv card";
            break;
          case TvResult.ConnectionToSlaveFailed:
            dlg.Content = "Failed to connect to slave server";
            break;
          case TvResult.NotTheOwner:
            dlg.Content = "Card is owned by another user";
            break;
          case TvResult.NoTuningDetails:
            dlg.Content = "Channel does not have tuning information";
            break;
          case TvResult.NoVideoAudioDetected:
            dlg.Content = "No Video/Audio streams detected";
            break;
          case TvResult.UnableToStartGraph:
            dlg.Content = "Unable to start graph";
            break;
          case TvResult.UnknownChannel:
            dlg.Content = "Unknown channel";
            break;
          case TvResult.UnknownError:
            dlg.Content = "Unknown error occured";
            break;
        }
        dlg.ShowDialog();
      }
    }

    /// <summary>
    /// Updates the info like program title,description,time start/end and progress bar on screen.
    /// </summary>
    void UpdateInfoBox()
    {
      labelDate.Content = DateTime.Now.ToString("dd-MM HH:mm");
      if (ChannelNavigator.Instance.SelectedChannel == null)
      {
        labelStart.Content = "";
        labelTitle.Content = "";
        labelChannel.Content = "";
        labelDescription.Text = "";
        progressBar.Value = 0;
        return;
      }
      Program program = ChannelNavigator.Instance.SelectedChannel.CurrentProgram;
      if (program == null)
      {
        labelStart.Content = "";
        labelTitle.Content = "";
        labelChannel.Content = "";
        labelDescription.Text = "";
        progressBar.Value = 0;
        return;
      }
      labelStart.Content = String.Format("{0}-{1}", program.StartTime.ToString("HH:mm"), program.EndTime.ToString("HH:mm"));
      labelTitle.Content = program.Title;
      labelChannel.Content = ChannelNavigator.Instance.SelectedChannel.Name;
      labelDescription.Text = program.Description;

      TimeSpan duration = program.EndTime - program.StartTime;
      TimeSpan passed = DateTime.Now - program.StartTime;
      float percent = (float)(passed.TotalMinutes / duration.TotalMinutes);
      progressBar.Value = (int)(percent * 100);
    }

  }
}