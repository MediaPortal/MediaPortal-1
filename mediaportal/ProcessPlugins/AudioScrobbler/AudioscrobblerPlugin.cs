#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion


using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Utils.Services;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;

namespace ProcessPlugins.Audioscrobbler 
{

  public class AudioscrobblerPlugin : IPlugin, ISetupForm 
  {  
    private const int STARTED_LATE = 5;

    // gconf key locations
    private const string GCONF_USERNAME = "/apps/muine/plugins/audioscrobbler/username";
    private const string GCONF_PASSWORD = "/apps/muine/plugins/audioscrobbler/password";
    private const string GCONF_ENABLED = "/apps/muine/plugins/audioscrobbler/enabled";
    private const string GCONF_UPGRADE = "/apps/muine/plugins/audioscrobbler/upgrade_alerts";
    private const string GCONF_SKIP_THRESHOLD = "/apps/muine/plugins/audioscrobbler/skip_threshold";

    /* If the position varies by more than this between tick events, then
       the user is skipping through the song and it won't be submitted.
       This is the most sensitive setting!  There may be problems
    */
    private int skipThreshold;
  
    // songs longer or shorter than this won't be submitted
    private const int MIN_DURATION = 30;
    private const int MAX_DURATION = 1800;

    private const int INFINITE_TIME = Int32.MaxValue;

    // how many events to store in the history textfield
    private const int MAX_HISTORY_LINES = 250;

    private Song currentSong;
    // whether the current song has been submitted    
    private bool queued; 
    // when to submbit the current song
    private int alertTime;
    // check for skipping
    private int lastPosition;
    private bool pluginEnabled;
    private bool showUpgrade;
 
   // store arguments from Audioscrobbler events
    private SubmitEventArgs lastSubmitArgs;
    private bool lastConnectArgs;

    //private IPlayer player;
    //private GConf.Client gconfClient;
    private AudioscrobblerBase scrobbler;

    //[Glade.Widget]
    //private Gtk.Entry UsernameBox;
    //[Glade.Widget]
    //private Gtk.Entry PasswordBox;
    //[Glade.Widget]
    //private Gtk.Window Window;

    //[Glade.Widget]
    //private Gtk.CheckButton CheckEnable;
    //[Glade.Widget]
    //private Gtk.CheckButton CheckShowUpgrade;
    //[Glade.Widget]
    //private Gtk.Label CacheSizeLabel;
    //[Glade.Widget]
    //private Gtk.Label SubmitTimeLabel;
    //[Glade.Widget]
    //private Gtk.TextView TextHistory;
    //[Glade.Widget]
    //private Gtk.Button DisconnectButton;
    //[Glade.Widget]
    //private Gtk.Button ConnectButton;
    //[Glade.Widget]
    //private Gtk.Statusbar StatusBar;

    //private uint StatusID; //< Required by the Gtk.Statusbar.
    //private Gtk.TextTag italicTag;
    //private Gtk.TextTag boldTag;

    //public override void Initialize(IPlayer player)
    //{
      //this.player = player;

      //// where to write the log and queue files
      //string pluginDir = GetPluginDir();
      //// attempt to open the log file
      
      //// Add listeners for Muine events
      //player.SongChangedEvent  += new SongChangedEventHandler(OnSongChangedEvent);
      //player.TickEvent         += new TickEventHandler(OnTickEvent);
      //player.StateChangedEvent += new StateChangedEventHandler(OnStateChangedEvent);

      //InitialiseUI();

      // get AS username and password
      //string username = "";
      //string password = "";
      ////gconfClient = new GConf.Client();
      //try {
      //  username = (string)gconfClient.Get(GCONF_USERNAME);
      //  password = (string)gconfClient.Get(GCONF_PASSWORD);
      //} catch (Exception e) {
      //  // add dummy entries here so the user can find them
      //  //gconfClient.Set(GCONF_USERNAME, username);
      //  //gconfClient.Set(GCONF_PASSWORD, password);
      //}

      // other AS preferences
      //showUpgrade = true;
      //try {
      //  showUpgrade = (bool)gconfClient.Get(GCONF_UPGRADE);
      //} catch (Exception e) {
      //  gconfClient.Set(GCONF_UPGRADE, showUpgrade);
      //}

      //skipThreshold = 2;
      //try {
      //  skipThreshold = (int)gconfClient.Get(GCONF_SKIP_THRESHOLD);
      //} catch (Exception e) {
      //  gconfClient.Set(GCONF_SKIP_THRESHOLD, skipThreshold);
      //}

      // is the plugin enabled?
      //pluginEnabled = true;
      //try {
      //  pluginEnabled = (bool)gconfClient.Get(GCONF_ENABLED);
      //} catch (Exception e) {
      //  gconfClient.Set(GCONF_ENABLED, pluginEnabled);
      //}

      // let us know whenever the gconf stuff changes
      //gconfClient.AddNotify(GCONF_USERNAME, 
      //                      new GConf.NotifyEventHandler(OnNameChangedEvent));
      //gconfClient.AddNotify(GCONF_PASSWORD, 
      //                      new GConf.NotifyEventHandler(OnPassChangedEvent));
      //gconfClient.AddNotify(GCONF_UPGRADE, 
      //                      new GConf.NotifyEventHandler(OnUpgradeChangedEvent));
      //gconfClient.AddNotify(GCONF_ENABLED, 
      //                      new GConf.NotifyEventHandler(OnEnabledChangedEvent));





    //  currentSong = null;
    //  queued      = false;
    //  alertTime   = INFINITE_TIME;
    //  scrobbler   = new GAudioscrobbler(username, password, pluginDir);
    //  StatusID    = StatusBar.GetContextId("message");
    //  StatusBar.Push(StatusID, "Ready.");

    //  // catch events from Audioscobbler object
    //  scrobbler.AuthErrorEventLazy    += OnAuthErrorEvent;
    //  scrobbler.NetworkErrorEventLazy += OnNetworkErrorEvent;
    //  scrobbler.SubmitEventLazy       += OnSubmitEvent;
    //  scrobbler.ConnectEvent          += OnConnectEvent;
    //  scrobbler.DisconnectEvent       += OnDisconnectEvent;

    //  // connect to Audioscrobbler (in a new thread)
    //  if (pluginEnabled) {
    //    StatusBar.Pop(StatusID);
    //    StatusBar.Push(StatusID, "Connecting...");
    //    OnConnectButtonClicked(null, null);
    //  }

    //  Global.Log(0, "AudioscrobblerPlugin.Initialise", "Finished");
    //}

    //private void InitialiseUI()
    //{
    //  Global.Log(0, "AudioscrobblerPlugin.InitialiseUI", "Starting");
    //  // set up the preferences UI - code reuse thanks to Fernando Herrera
    //  Gtk.ActionEntry [] actionEntries = new Gtk.ActionEntry [] {
    //    new Gtk.ActionEntry ("SetAudioscrobblerDetails", null, "Audioscrobbler...", "", null, new EventHandler(OnAudioscrobblerWindow))
    //  };
    //  Gtk.ActionGroup actionGroup = new Gtk.ActionGroup("AudioscrobblerActions");
    //  actionGroup.Add(actionEntries);
    
    //  player.UIManager.InsertActionGroup(actionGroup, -1);
    //  player.UIManager.AddUi(this.player.UIManager.NewMergeId(),
    //                         "/MenuBar/FileMenu/ExtraFileActions",
    //                         "AudioscrobblerMenuItem",
    //                         "SetAudioscrobblerDetails",
    //                         Gtk.UIManagerItemType.Menuitem,
    //                         false);
    
    //  Glade.XML glade = new Glade.XML(null, "Audioscrobbler.glade", "Window", null);
    //  glade.Autoconnect(this);

    //  // add tags for bold and italic text in the history buffer
    //  italicTag = new Gtk.TextTag("italic");
    //  italicTag.Style = Pango.Style.Italic;
    //  TextHistory.Buffer.TagTable.Add(italicTag);

    //  boldTag = new Gtk.TextTag("bold");
    //  boldTag.Weight = Pango.Weight.Bold;
    //  TextHistory.Buffer.TagTable.Add(boldTag);    

    //  SubmitTimeLabel.Text = "Nothing to submit.";
    //  Global.Log(0, "AudioscrobblerPlugin.InitialiseUI", "Finished");
    //}

    // Figure out where we should put the log and cache files
    //private string GetPluginDir()
    //{
    //  string retVal = "/tmp"; // if all else fails we can store the queue here
    //  try {
    //    retVal = Path.Combine (Gnome.User.DirGet(), "muine");
    //  } catch (Exception e) {
    //    Console.WriteLine("AudioscrobblerPlugin Warning: Couldn't find $HOME directory\nCache and log files will be stored in /tmp");
    //  }
    //  return retVal;
    //}

    //
    // Properties
    //

    /* The number of seconds at which the current song will be queued */
    public int AlertTime
    {
      get {
        return alertTime;
      }
    }

    /* Whether the current song has been added to the queue */
    public bool Queued
    {
      get {
        return queued;
      }
    }

    public AudioscrobblerBase Audioscrobbler
    {
      get {
        return scrobbler;
      }
    }

    //
    // UI events
    //

    private void OnAudioscrobblerWindow(object sender, EventArgs args)
    {
      //ShowAudioscrobblerWindow();
    }

    /* ShowAudioscrobblerWindow
       This function is suitable as a delegate in the idle loop */
    //private bool ShowAudioscrobblerWindow ()
    //{
    //  // set the text in the input boxes
    //  UsernameBox.Text = scrobbler.Username;
    //  PasswordBox.Text = scrobbler.Password;
    //  CheckShowUpgrade.Active = showUpgrade;
    //  CheckEnable.Active = pluginEnabled;
    //  Window.Show();
    //  return false;
    //}
    



    private void OnDisconnectButtonClicked(object sender, EventArgs args)
    {
      scrobbler.Disconnect();
    }

    private void OnConnectButtonClicked(object sender, EventArgs args)
    {
      scrobbler.Connect();
    }

    private void OnClearCacheButtonClicked(object sender, EventArgs args)
    {
      scrobbler.ClearQueue();
      //CacheSizeLabel.Text = scrobbler.QueueLength.ToString();
      AddToHistory("Cache cleared", null);
    }

    // 
    // GConf events
    //

    private void OnNameChangedEvent(string ASUsername)
    {
      scrobbler.Username = ASUsername;
    }

    private void OnPassChangedEvent(string ASPassword)
    {
      scrobbler.Password = ASPassword;
    }

    private void OnEnabledChangedEvent(bool isEnabled)
    {
      if (isEnabled)
        OnConnectButtonClicked(null, null);
      else {
        scrobbler.Disconnect();
      }
    }

    private void OnUpgradeChangedEvent(bool ShowUpgrade)
    {
      showUpgrade = ShowUpgrade;
    }

    //
    // Audioscrobbler events
    //

    private void OnAuthErrorEvent(AuthErrorEventArgs args)
    {
      string report = "Audioscrobbler did not recognize your username/password."
                    + " Please check your details (File/Audioscrobbler...)";
      ShowErrorMessage(report);
      OnDisconnectButtonClicked(null, null);
    }

    private void OnNetworkErrorEvent(NetworkErrorEventArgs args)
    {
      //StatusBar.Pop(StatusID);
      //StatusBar.Push(StatusID, args.Details);
      //OnDisconnectButtonClicked(null, null);
    }

    private void OnUpdateAvailableEvent(UpdateAvailableEventArgs args)
    {
      //if (!CheckShowUpgrade.Active)
        //return;
      string report = "A new version of the Audioscrobbler plugin is"
                    + "available at:\n" + args.version;
      ShowErrorMessage(report);
    }
    
    private void OnSubmitEvent (SubmitEventArgs args)
    {
      //CacheSizeLabel.Text = scrobbler.QueueLength.ToString();
      string title = args.song.Artist + " - " + args.song.Title;
      //StatusBar.Pop(StatusID);
      //StatusBar.Push(StatusID, "Submitted: " + title);
      //SubmitTimeLabel.Text = "Nothing to submit.";
      // Consider the song to be submitted if a connection was made -
      // AS can't reject individual songs.
      AddToHistory("Submitted", args.song);
    }
  
    private void OnConnectEvent(ConnectEventArgs args)
    {
      //StatusBar.Pop(StatusID);
      //StatusBar.Push(StatusID, "Connected.");
    }

    private void OnDisconnectEvent(DisconnectEventArgs args)
    {
      //StatusBar.Pop(StatusID);
      //StatusBar.Push(StatusID, "Disconnected.");
    }


    //
    // Muine events
    //
    private void OnSongChangedEvent(Song song)
    {
      //Global.Log(0, "Plugin.OnSongChangedEvent", "Start");
      queued      = false;
      alertTime   = INFINITE_TIME;
      currentSong = null;
    
      if (!pluginEnabled || song == null)
        return;

      currentSong = new Song();
      //if (player.Playing)
        currentSong.DateTimePlayed = DateTime.UtcNow;

      // Only submit if we have reasonable info about the song
      if (currentSong.Artist == "" || currentSong.Title == "")
        return;

      // Don't queue if the song didn't start at 0
      //if (player.Position <= STARTED_LATE) {
//        alertTime = GetAlertTime();
        //return;
      //}

      // Weirdly, this event seems to fire just as the current song 
      // is finishing?
      //if (player.Position < currentSong.Duration) {
        //string logmessage = "Not submitting! Starting at "
        //                  + player.Position
        //                  + " of "
        //                  + currentSong.Duration;
        //Global.Log(1, "Plugin.OnSongChangedEvent", logmessage);
        //SubmitTimeLabel.Text = "Current song started late. Not submitting.";
        //AddToHistory("Ignored (started late)", currentSong);
      //}
      //Global.Log(0, "Plugin.OnSongChangedEvent", "End");
    }

    private void OnStateChangedEvent(bool playing)
    {
      if (playing && currentSong != null &&
          currentSong.DateTimePlayed == DateTime.MinValue)
      {
        // we've started the player - note the time
        currentSong.DateTimePlayed = DateTime.UtcNow;
      }
    }  

    private void OnTickEvent(int position)
    {
      if (!pluginEnabled)
        return;
    
      // attempt to detect skipping
      if (currentSong != null &&
          alertTime < INFINITE_TIME && 
          position > lastPosition + skipThreshold) {
//        Global.Log(1, "Plugin.OnTickEvent", "Skipping detected " +
//                   "(from " + lastPosition + " to " + position + ")" +
//                   " - not queueing");
        alertTime = INFINITE_TIME;
//        SubmitTimeLabel.Text = "Skipping detected. Not submitting the current song.";
        AddToHistory("Ignored (skipping)", currentSong);
      }

      // update the time-to-submission label
      //if (Window.Visible && position != lastPosition && 
      //    !queued && alertTime > position && alertTime != INFINITE_TIME)
      //  UpdateSubmitTimeLabel(alertTime - position);

      // then actually queue the song if we're that far along
      if (!queued && 
          position >= alertTime && 
          alertTime > 14 && position > 14 && // stop double queueing bug?
          currentSong != null) {
        //SubmitTimeLabel.Text = "Submitting...";
        scrobbler.pushQueue(currentSong);
        queued = true;
      }

      /* We don't set lastPosition back to 0 in SongChanged.  
         This avoids problems where songs start late.  It might also 
         mean the user can somehow start a song late and skipping won't 
         be detected
      */
      lastPosition = position;
    }


    //
    // Utilities.
    //
    
    /**
     * Show a dialog box with the message from Audioscrobbler.
     */
    private bool ShowErrorMessage (string message)
    {
      //MessageBox md =
      //  new MessageBox(player.Window, 
      //                         Gtk.DialogFlags.DestroyWithParent,
      //                         Gtk.MessageType.Info, 
      //                         Gtk.ButtonsType.Close, 
      //                         message);
      //md.Modal = false;
      //md.Run ();
      //md.Destroy();
      return false;
    }
    
    // Logic about when we should submit a song to Audioscrobbler
    private int GetAlertTime()
    {
      if (currentSong.Duration > MAX_DURATION) {
        //SubmitTimeLabel.Text = "Current song is too long. Not submitting.";
        AddToHistory("Ignored (too long)", currentSong);
        return INFINITE_TIME;
      }
      else if (currentSong.Duration < MIN_DURATION) {
        //SubmitTimeLabel.Text = "Current song is too short. Not submitting.";
        AddToHistory("Ignored (too short)", currentSong);
        return INFINITE_TIME;
      }

      // If the duration is less then 480 secs, alert when the song
      // is half over, otherwise after 240 seconds.
      if (currentSong.Duration < 480)
        return currentSong.Duration / 2;
      else
        return 240;

      return INFINITE_TIME;
    }

    // Add a message to the submissions history text area
    private void AddToHistory(string desc, Song song)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      ILog _log = services.Get<ILog>();

      if (desc != "")
      {
        _log.Info("Audioscrobbler: {0}", desc);
      }
    
      if (song != null)
        _log.Info("Audioscrobbler: {0}", song.ToShortString());
    }

    #region IPlugin Members

    public void Start()
    {
      //_timer.Enabled = true;
    }

    public void Stop()
    {
      //_timer.Enabled = false;
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return false;
    }

    public string Description()
    {
      // This offers the possibility to get a radio stream based on your own music taste and "smart playlists"
      return "The Audioscrobbler plugin populates your profile on http://www.last.fm";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "rtv";
    }

    public string PluginName()
    {
      return "AudioScrobbler";
    }

    public bool HasSetup()
    {
      return false;
    }

    public void ShowPlugin()
    {
      //
    }

    #endregion
  }
}
