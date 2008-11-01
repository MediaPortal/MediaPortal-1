namespace CybrDisplayPlugin.Drivers
{
    using CybrDisplayPlugin;
    using MCEDisplay_Interop;
    using MediaPortal.GUI.Library;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;

    public class MCEDisplay : BaseDisplay, IDisplay, IDisposable
    {
        private readonly string[] _lines = new string[2];
        public const bool ExtensiveLogging = true;
      private bool IDisplayToMCE = false;
        private Thread mainThread;
        private MCESession mediaSession = new HomeSession();
        private MiniDisplay.SystemStatus MPStatus = new MiniDisplay.SystemStatus();
        private bool playSwitched;
        private static bool stopRequested = false;
        private static object ThreadMutex = new object();

        public void CleanUp()
        {
            Log.Info("MCEDisplay.Cleanup(): called", new object[0]);
            this.Stop();
            Log.Info("MCEDisplay.Cleanup(): completed", new object[0]);
        }

        public void Configure()
        {
        }

        public virtual void Dispose()
        {
        }

        public void DrawImage(Bitmap bitmap)
        {
        }

        private void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
        {
            Log.Info("MCEDisplay.GUIPropertyManager_OnPropertyChanged(): received notification TAG = \"{0}\", value = \"{1}\"", new object[] { tag, tagValue });
            if (tag == "#Play.Current.File")
            {
                Log.Info("MCEDisplay.GUIPropertyManager_OnPropertyChanged(): Play switch detected", new object[0]);
                this.playSwitched = true;
                if (GUIPropertyManager.GetProperty("#duration").Length == 0)
                {
                    Log.Info("MCEDisplay.GUIPropertyManager_OnPropertyChanged(): duration not filled yet", new object[0]);
                }
            }
            else if (tag == "#currentmodule")
            {
                Log.Info("MCEDisplay.GUIPropertyManager_OnPropertyChanged(): menu switch detected", new object[0]);
                this.playSwitched = true;
            }
        }

        public void Initialize()
        {
            Log.Info("MCEDisplay.Initialize(): called", new object[0]);
            this.Start();
            Log.Info("MCEDisplay.Initialize(): completed", new object[0]);
        }

        private void Run()
        {
            Log.Info("MCEDisplay: plugin thread starting...", new object[0]);
        Label_0010:
            try
            {
                Thread.Sleep(50);
                this.mediaSession.Process();
                if (this.playSwitched)
                {
                    Thread.Sleep(100);
                    MiniDisplay.GetSystemStatus(ref this.MPStatus);
                    if (this.mediaSession != null)
                    {
                        this.mediaSession.Dispose();
                    }
                    if (this.MPStatus.Media_IsMusic)
                    {
                        this.mediaSession = new MusicSession();
                    }
                    else if (this.MPStatus.Media_IsVideo)
                    {
                        this.mediaSession = new VideoSession();
                    }
                    else if (this.MPStatus.Media_IsRadio)
                    {
                        this.mediaSession = new RadioSession();
                    }
                    else if (this.MPStatus.Media_IsTVRecording)
                    {
                        this.mediaSession = new RecordedTVSession();
                    }
                    else if (this.MPStatus.Media_IsTV)
                    {
                        this.mediaSession = new TVSession();
                    }
                    else
                    {
                        this.mediaSession = new HomeSession();
                    }
                    this.playSwitched = false;
                }
                if (this.mediaSession != null)
                {
                    this.mediaSession.Process();
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Thread was being aborted"))
                {
                    if (this.mediaSession != null)
                    {
                        this.mediaSession.Dispose();
                    }
                    Log.Info("MCEDisplay: caught thread stop request... plugin thread stopping...", new object[0]);
                    return;
                }
                Log.Info("MCEDisplay: Caught the following exception: {0}\n{1}", new object[] { exception.Message, exception.StackTrace });
            }
            lock (ThreadMutex)
            {
                if (!stopRequested)
                {
                    goto Label_0010;
                }
                Log.Debug("MCEDisplay.Run(): MCEDisplay Thread terminating", new object[0]);
                if (this.mediaSession != null)
                {
                    this.mediaSession.Dispose();
                    this.mediaSession = null;
                }
                stopRequested = false;
            }
        }

        public void SetCustomCharacters(int[][] customCharacters)
        {
        }

        public void SetLine(int line, string message)
        {
            if (!this.IDisplayToMCE)
            {
            }
        }

        public void Setup(string _port, int _lines, int _cols, int _time, int _linesG, int _colsG, int _timeG, bool _backLight, int _backlightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
        {
            Log.Info("{0}", new object[] { this.Description });
            Log.Info("MCEDisplay.Setup(): called", new object[0]);
        }

        public void Start()
        {
            Log.Info("MCEDisplay.Start(): Starting MCEDisplay driver", new object[0]);
            this.mainThread = new Thread(new ThreadStart(this.Run));
            this.mainThread.Start();
            GUIPropertyManager.OnPropertyChanged += new GUIPropertyManager.OnPropertyChangedHandler(this.GUIPropertyManager_OnPropertyChanged);
            Log.Info("MCEDisplay.Start(): MCEDisplay driver started", new object[0]);
        }

        public void Stop()
        {
            Log.Info("MCEDisplay.Stop(): Stopping MCEDisplay driver", new object[0]);
            stopRequested = true;
            Thread.Sleep(100);
            while ((this.mainThread != null) && this.mainThread.IsAlive)
            {
                Log.Info("MCEDisplay.Stop(): waiting for display thread to stop...!", new object[0]);
                Application.DoEvents();
                Thread.Sleep(100);
            }
            Log.Info("MCEDisplay.Stop(): MCEDisplay driver STOPPED!", new object[0]);
        }

        public string Description
        {
            get
            {
                return "MCE Compatible Display driver v_03_09_2008b";
            }
        }

        public string ErrorMessage
        {
            get
            {
                return "";
            }
        }

        public bool IsDisabled
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return "MCEDisplay";
            }
        }

        public bool SupportsGraphics
        {
            get
            {
                return false;
            }
        }

        public bool SupportsText
        {
            get
            {
                return true;
            }
        }

        public class HomeSession : MCEDisplay.MCESession
        {
            private MiniDisplay.SystemStatus MPStatus = new MiniDisplay.SystemStatus();
            private MediaStatusPropertyTag oldMenu;
            private string oldTitle = string.Empty;

            public HomeSession()
            {
                Log.Info("MCEDisplay.HomeSession: Creating HOME session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[1];
                object[] vals = new object[1];
                tags[0] = MediaStatusPropertyTag.FS_Home;
                vals[0] = true;
                base.SetStatus(tags, vals);
            }

            public override void Dispose()
            {
                Log.Info("MCEDisplay.HomeSession: Closing Home Session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.SessionEnd };
                object[] vals = new object[] { true };
                base.SetStatus(tags, vals);
                base.Dispose();
            }

            public override void Process()
            {
                MediaStatusPropertyTag tag;
                MiniDisplay.GetSystemStatus(ref this.MPStatus);
                switch (GUIWindowManager.ActiveWindow)
                {
                    case 0:
                        tag = MediaStatusPropertyTag.FS_Home;
                        break;

                    case 1:
                    case 0x1e14:
                    case 0x1e15:
                    case 600:
                    case 0x259:
                    case 0x25a:
                    case 0x25b:
                    case 0x25c:
                    case 0x25d:
                    case 0x25e:
                        tag = MediaStatusPropertyTag.FS_TV;
                        break;

                    case 2:
                    case 0x7d7:
                        tag = MediaStatusPropertyTag.FS_Photos;
                        break;

                    case 5:
                    case 500:
                    case 0x1f5:
                    case 0x1f6:
                    case 0x1f7:
                    case 0x1f8:
                    case 0x1f9:
                    case 0x1fa:
                    case 0x1fb:
                    case 0x7d1:
                    case 0x7d4:
                        tag = MediaStatusPropertyTag.FS_Music;
                        break;

                    case 6:
                    case 0x15:
                    case 0x16:
                    case 0x17:
                    case 0x19:
                    case 0x1c:
                    case 0xbb8:
                    case 0x7d3:
                        tag = MediaStatusPropertyTag.FS_Videos;
                        break;

                    case 30:
                        tag = MediaStatusPropertyTag.FS_Radio;
                        break;

                    case 0x7d5:
                        if (this.MPStatus.Media_IsTV || this.MPStatus.Media_IsTVRecording)
                        {
                            tag = MediaStatusPropertyTag.FS_RecordedShows;
                        }
                        else
                        {
                            tag = MediaStatusPropertyTag.FS_Extensibility;
                        }
                        break;

                    default:
                        tag = MediaStatusPropertyTag.FS_Extensibility;
                        break;
                }
                if (tag != this.oldMenu)
                {
                    Log.Info("MCEDisplay.HomeSession: Updating home status to {0} (window = {1})", new object[] { tag.ToString(), GUIWindowManager.ActiveWindow.ToString() });
                    MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { this.oldMenu, tag };
                    object[] vals = new object[] { false, true };
                    base.SetStatus(tags, vals);
                    this.oldMenu = tag;
                }
            }

            public void Reset()
            {
                this.oldMenu = MediaStatusPropertyTag.FS_Extensibility;
                this.Process();
            }
        }

        public abstract class MCESession : ISession, IDisposable
        {
            protected IMediaStatusSession[] session;
            protected static IMediaStatusSink[] sinks;

            static MCESession()
            {
                Log.Info("MCESession: Enumerating IMediaStatusSink implementations", new object[0]);
                ArrayList list = new ArrayList();
                RegistryKey key = Registry.ClassesRoot.OpenSubKey("CLSID");
                foreach (string str in key.GetSubKeyNames())
                {
                    using (RegistryKey key2 = key.OpenSubKey(str))
                    {
                        if (key2 != null)
                        {
                            using (RegistryKey key3 = key2.OpenSubKey("Implemented Categories"))
                            {
                                if (key3 != null)
                                {
                                    string[] subKeyNames = key3.GetSubKeyNames();
                                    for (int k = 0; k < subKeyNames.Length; k++)
                                    {
                                        if (string.Compare(subKeyNames[k], "{FCB0C2A3-9747-4C95-9D02-820AFEDEF13F}", true, CultureInfo.InvariantCulture) == 0)
                                        {
                                            string str2 = (string) key2.OpenSubKey("InprocServer32").GetValue("");
                                            Log.Info("MCESession: Trying to start an instance of a COM component with GUID {0}... Server = \"{1}\"", new object[] { str, str2 });
                                            try
                                            {
                                                object obj2 = Activator.CreateInstance(System.Type.GetTypeFromCLSID(new Guid(str)));
                                                list.Add((IMediaStatusSink) obj2);
                                            }
                                            catch (Exception exception)
                                            {
                                                Log.Info("MCESession: MCESession: Starting of COM component failed.  " + exception.Message, new object[0]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                sinks = new IMediaStatusSink[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    sinks[i] = (IMediaStatusSink) list[i];
                }
                for (int j = 0; j < sinks.Length; j++)
                {
                    sinks[j].Initialize();
                }
                Log.Info("MCESession: Enumerating IMediaStatusSink completed", new object[0]);
            }

            public MCESession()
            {
                Log.Info("MCESession: creating MCESession", new object[0]);
                this.session = CreateSession();
                Log.Info("MCESession: MCESession created", new object[0]);
            }

            protected void CloseSession()
            {
                if (this.session == null)
                {
                    Log.Info("MCEDisplay.MCESession.CloseSession(): SESSION DOES NOT EXIST", new object[0]);
                }
                else
                {
                    Log.Info("MCEDisplay.MCESession.CloseSession(): CLOSING SESSION", new object[0]);
                    for (int i = 0; i < this.session.Length; i++)
                    {
                        try
                        {
                            this.session[i].Close();
                        }
                        catch (Exception exception)
                        {
                            Log.Info("MCEDisplay.MCESession.CloseSession(): CAUGHT EXCEPTION closing session driver # {0}: {1}", new object[] { i, exception });
                        }
                    }
                    Log.Info("MCEDisplay.MCESession.CloseSession(): SESSION CLOSED", new object[0]);
                }
            }

            protected static IMediaStatusSession[] CreateSession()
            {
                Log.Info("MCEDisplay.MCESession.CreateSession(): creating MCESession", new object[0]);
                IMediaStatusSession[] sessionArray = new IMediaStatusSession[sinks.Length];
                for (int i = 0; i < sinks.Length; i++)
                {
                    Log.Info("MCEDisplay.MCESession.CreateSession():    creating MCESession for sink {0} of {1}", new object[] { i + 1, sinks.Length });
                    sessionArray[i] = sinks[i].CreateSession();
                    Log.Info("MCEDisplay.MCESession.CreateSession():    CREATED MCESession for sink {0} of {1}", new object[] { i + 1, sinks.Length });
                }
                Log.Info("MCEDisplay.MCESession.CreateSession(): MCESession created", new object[0]);
                return sessionArray;
            }

            public virtual void Dispose()
            {
                Log.Info("MCESession: disposing session.", new object[0]);
                if (this.session != null)
                {
                    this.CloseSession();
                    Log.Info("MCESession: SESSION DISPOSED.", new object[0]);
                }
            }

            protected int Duration2Int(string dur)
            {
                int num = -1;
                if (dur.Length <= 0)
                {
                    return num;
                }
                if (dur.Length < 6)
                {
                    dur = "00:" + dur;
                }
                return (int) TimeSpan.Parse(dur).TotalSeconds;
            }

            protected int GetDuration2Int(string _property)
            {
                return this.Duration2Int(GUIPropertyManager.GetProperty(_property));
            }

            public abstract void Process();
            protected void SetStatus(MediaStatusPropertyTag[] tags, object[] vals)
            {
                SetStatus(this.session, tags, vals);
            }

            protected static void SetStatus(IMediaStatusSession[] sessions, MediaStatusPropertyTag[] tags, object[] vals)
            {
                for (int i = 0; i < sessions.Length; i++)
                {
                    try
                    {
                        sessions[i].MediaStatusChange(tags, vals);
                    }
                    catch (Exception exception)
                    {
                        Log.Info("MCEDisplay.MCESession.SetStatus(): CAUGHT EXCEPTION setting status for session driver # {0}: {1}", new object[] { i, exception });
                    }
                }
            }

            protected int Time2Int(string _property)
            {
                int totalSeconds = -1;
                string property = GUIPropertyManager.GetProperty(_property);
                if (property.Length > 0)
                {
                    totalSeconds = (int) DateTime.Parse(property).TimeOfDay.TotalSeconds;
                }
                return totalSeconds;
            }
        }

        public class MusicSession : MCEDisplay.MCESession
        {
            private int LastPlayState;
            private string LastPlayTime;
            private MiniDisplay.SystemStatus MPStatus = new MiniDisplay.SystemStatus();

            public MusicSession()
            {
                Log.Info("MCEDisplay.MusicSession: Creating music session", new object[0]);
                string property = GUIPropertyManager.GetProperty("#duration");
                string str2 = GUIPropertyManager.GetProperty("#Play.Current.Album");
                string str3 = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                string str4 = GUIPropertyManager.GetProperty("#Play.Current.Title");
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.StreamingContentAudio, MediaStatusPropertyTag.Shuffle, MediaStatusPropertyTag.RepeatSet, MediaStatusPropertyTag.MediaName, MediaStatusPropertyTag.ArtistName, MediaStatusPropertyTag.TrackName, MediaStatusPropertyTag.TrackDuration, MediaStatusPropertyTag.TrackTime };
                object[] vals = new object[] { true, false, false, str2, str3, str4, base.GetDuration2Int("#duration"), 0 };
                base.SetStatus(tags, vals);
                Log.Info("MCEDisplay.MusicSession: Playing {0} by {1} from the album {2} duration {3}", new object[] { str4, str3, str2, property });
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Play };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
            }

            public override void Dispose()
            {
                Log.Info("MCEDisplay.MusicSession: STOPPING music session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                object[] vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.StreamingContentAudio };
                vals = new object[] { false };
                base.SetStatus(tags, vals);
                base.Dispose();
            }

            public override void Process()
            {
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[0];
                object[] vals = new object[0];
                string property = GUIPropertyManager.GetProperty("#currentplaytime");
                int num = base.Duration2Int(property);
                MiniDisplay.GetSystemStatus(ref this.MPStatus);
                int num2 = -1;
                if (this.MPStatus.MediaPlayer_Paused)
                {
                    num2 = 1;
                }
                else if (this.MPStatus.Media_Speed == 1)
                {
                    num2 = 2;
                }
                else if (this.MPStatus.Media_Speed > 1)
                {
                    num2 = 3;
                }
                else if (this.MPStatus.Media_Speed < 0)
                {
                    num2 = 4;
                }
                else if (((this.MPStatus.Media_Speed == 0) || (!this.MPStatus.MediaPlayer_Paused & !this.MPStatus.MediaPlayer_Playing)) || !this.MPStatus.MediaPlayer_Active)
                {
                    num2 = 5;
                }
                if (num2 != this.LastPlayState)
                {
                    switch (num2)
                    {
                        case 1:
                            tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Pause };
                            vals = new object[] { true };
                            Log.Info("MCEDisplay.MusicSession.Process(): Updating PlayState to PAUSED", new object[0]);
                            base.SetStatus(tags, vals);
                            break;

                        case 2:
                            tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Play };
                            vals = new object[] { true };
                            Log.Info("MCEDisplay.MusicSession.Process(): Updating PlayState to PLAY", new object[0]);
                            base.SetStatus(tags, vals);
                            break;

                        case 3:
                            tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.FF1 };
                            vals = new object[] { true };
                            Log.Info("MCEDisplay.MusicSession.Process(): Updating PlayState to FASTFORWARD", new object[0]);
                            base.SetStatus(tags, vals);
                            break;

                        case 4:
                            tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Rewind1 };
                            vals = new object[] { true };
                            Log.Info("MCEDisplay.MusicSession.Process(): Updating PlayState to REWIND", new object[0]);
                            base.SetStatus(tags, vals);
                            break;

                        default:
                            if (num2 == 4)
                            {
                                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop, MediaStatusPropertyTag.MediaControl };
                                vals = new object[] { true, false };
                                Log.Info("MCEDisplay.MusicSession.Process(): Updating PlayState to STOP", new object[0]);
                                base.SetStatus(tags, vals);
                            }
                            break;
                    }
                    this.LastPlayState = num2;
                }
                if (property != this.LastPlayTime)
                {
                    tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TrackTime };
                    vals = new object[] { num };
                    base.SetStatus(tags, vals);
                    this.LastPlayTime = property;
                    Log.Info("MCEDisplay.MusicSession.Process(): Updating TrackTime to {0} ", new object[] { num });
                }
            }
        }

        private class NullSession : ISession, IDisposable
        {
            public void Dispose()
            {
            }

            public void Process()
            {
            }
        }

        public class RadioSession : MCEDisplay.MCESession
        {
            private string LastStation = string.Empty;

            public RadioSession()
            {
                Log.Info("MCEDisplay.RadioSession: Creating Radio session", new object[0]);
                string property = GUIPropertyManager.GetProperty("#Play.Current.Title");
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Radio, MediaStatusPropertyTag.RadioFrequency };
                object[] vals = new object[] { true, property };
                base.SetStatus(tags, vals);
                Log.Info("MCEDisplay.RadioSession: Playing radio station {0}", new object[] { property });
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Play };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
            }

            public override void Dispose()
            {
                Log.Info("MCEDisplay.RadioSession: STOPPING Radio session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                object[] vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Radio, MediaStatusPropertyTag.RadioFrequency };
                vals = new object[] { false, string.Empty };
                base.SetStatus(tags, vals);
                base.Dispose();
            }

            public override void Process()
            {
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[0];
                object[] vals = new object[0];
                string property = GUIPropertyManager.GetProperty("#Play.Current.Title");
                base.GetDuration2Int("#currentplaytime");
                if (!property.Equals(this.LastStation))
                {
                    tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.RadioFrequency };
                    vals = new object[] { property };
                    base.SetStatus(tags, vals);
                    Log.Info("MCEDisplay.RadioSession.Process(): Updating currentStation to \"{0}\" ", new object[] { property });
                    this.LastStation = property;
                }
            }
        }

        public class RecordedTVSession : MCEDisplay.MCESession
        {
            private int duration;
            private string oldProgram = "";
            private int oldTime;
            private string program = "";
            private int time;

            public RecordedTVSession()
            {
                Log.Info("MCEDisplay.RecordedTVSession: Creating Recording session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.PVR };
                object[] vals = new object[] { true };
                base.SetStatus(tags, vals);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Play };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
            }

            public override void Dispose()
            {
                Log.Info("MCEDisplay.RecordedTVSession: Stopping Recording session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                object[] vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.PVR };
                vals = new object[] { false };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                base.Dispose();
            }

            public override void Process()
            {
                try
                {
                    this.program = GUIPropertyManager.GetProperty("#TV.RecordedTV.Title");
                    this.time = base.GetDuration2Int("#currentplaytime");
                    this.duration = base.GetDuration2Int("#duration");
                    Log.Info("MCEDisplay.RecordedTVSession: Playing TV Recording: {0} ({1} of {2})", new object[] { this.program, this.time, this.duration });
                    if (((this.program.Length != 0) && (this.time >= 0)) && (this.duration > 0))
                    {
                        MediaStatusPropertyTag[] tagArray;
                        object[] objArray;
                        if (this.oldProgram != this.program)
                        {
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.ParentalAdvisoryRating };
                            objArray = new object[] { GUIPropertyManager.GetProperty("#TV.RecordedTV.Time") };
                            base.SetStatus(tagArray, objArray);
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TrackTime };
                            objArray = new object[] { this.time };
                            base.SetStatus(tagArray, objArray);
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.MediaName };
                            objArray = new object[] { this.program };
                            base.SetStatus(tagArray, objArray);
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.MediaTime };
                            objArray = new object[] { this.duration };
                            base.SetStatus(tagArray, objArray);
                            this.oldProgram = this.program;
                        }
                        if (this.time != this.oldTime)
                        {
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TrackTime };
                            objArray = new object[] { this.time };
                            base.SetStatus(tagArray, objArray);
                            this.oldTime = this.time;
                        }
                    }
                }
                catch (ApplicationException exception)
                {
                    Log.Info("MCEDisplay.RecordedTVSession: Exception occurred: {0}\nStackTrace:{1} ", new object[] { exception.Message, exception.StackTrace });
                }
            }
        }

        public class TVSession : MCEDisplay.MCESession
        {
            private string channel = "";
            private int end = -1;
            private string oldChannel = "";
            private int oldEnd = -1;
            private string oldProgram = "";
            private int oldStart = -1;
            private int oldTime;
            private bool playing = true;
            private string program = "";
            private int start = -1;

            public TVSession()
            {
                Log.Info("MCEDisplay.TVSession: Creating TV session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TVTuner };
                object[] vals = new object[] { true };
                base.SetStatus(tags, vals);
            }

            public override void Dispose()
            {
                Log.Info("MCEDisplay.TVSession: Stopping TV session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                object[] vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TVTuner };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                base.Dispose();
            }

            public override void Process()
            {
                try
                {
                    this.program = GUIPropertyManager.GetProperty("#TV.View.title");
                    this.channel = GUIPropertyManager.GetProperty("#TV.View.channel");
                    this.start = base.Time2Int("#TV.View.start");
                    this.end = base.Time2Int("#TV.View.stop");
                    Log.Info("MCEDisplay.TVSession: Playing TV: {0} on {1} from {2} -> {3}", new object[] { this.program, this.channel, this.start, this.end });
                    if (((this.program.Length != 0) && (this.channel.Length != 0)) && ((this.end >= 0) && (this.start >= 0)))
                    {
                        MediaStatusPropertyTag[] tagArray;
                        object[] objArray;
                        int num3;
                        if (this.channel != this.oldChannel)
                        {
                            int index = this.channel.IndexOf(' ');
                            int num2 = 1;
                            if (((index > 0) && (this.channel[0] >= '0')) && (this.channel[1] <= '9'))
                            {
                                num2 = int.Parse(this.channel.Substring(0, index));
                            }
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TrackNumber };
                            objArray = new object[] { num2 };
                            base.SetStatus(tagArray, objArray);
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Play };
                            objArray = new object[] { this.playing };
                            base.SetStatus(tagArray, objArray);
                            if (this.oldChannel.Length != 0)
                            {
                                Thread.Sleep(100);
                                base.SetStatus(tagArray, objArray);
                            }
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.ParentalAdvisoryRating };
                            objArray = new object[] { "" };
                            base.SetStatus(tagArray, objArray);
                            this.oldChannel = this.channel;
                        }
                        if (((this.program != this.oldProgram) || (this.end != this.oldEnd)) || (this.start != this.oldStart))
                        {
                            num3 = ((int) DateTime.Now.TimeOfDay.TotalSeconds) - this.start;
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TrackTime };
                            objArray = new object[] { num3 };
                            base.SetStatus(tagArray, objArray);
                            this.oldTime = num3;
                            if (this.program.Length == 0)
                            {
                                this.program = GUILocalizeStrings.Get(0x2e0);
                            }
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.MediaName };
                            objArray = new object[] { this.program };
                            base.SetStatus(tagArray, objArray);
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.MediaTime };
                            objArray = new object[] { this.end - this.start };
                            base.SetStatus(tagArray, objArray);
                            this.oldProgram = this.program;
                            this.oldEnd = this.end;
                            this.oldStart = this.start;
                        }
                        num3 = ((int) DateTime.Now.TimeOfDay.TotalSeconds) - this.start;
                        if (num3 != this.oldTime)
                        {
                            tagArray = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TrackTime };
                            objArray = new object[] { num3 };
                            base.SetStatus(tagArray, objArray);
                            this.oldTime = num3;
                        }
                    }
                }
                catch (ApplicationException exception)
                {
                    Log.Info("MCEDisplay.TVSession: Exception occurred: {0}\nStackTrace:{1} ", new object[] { exception.Message, exception.StackTrace });
                }
            }
        }

        public class VideoSession : MCEDisplay.MCESession
        {
            private int LastPlayState;
            private MiniDisplay.SystemStatus MPStatus = new MiniDisplay.SystemStatus();

            public VideoSession()
            {
                Log.Info("MCEDisplay.VideoSession: Creating Video session", new object[0]);
                MediaStatusPropertyTag[] tags = null;
                object[] vals = null;
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.StreamingContentVideo, MediaStatusPropertyTag.MediaName, MediaStatusPropertyTag.MediaTime, MediaStatusPropertyTag.TrackTime };
                vals = new object[] { true, GUIPropertyManager.GetProperty("#Play.Current.Title"), base.GetDuration2Int("#duration"), 0 };
                base.SetStatus(tags, vals);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Play };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
            }

            public override void Dispose()
            {
                Log.Info("MCEDisplay.VideoSession: Stopping Video session", new object[0]);
                MediaStatusPropertyTag[] tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                object[] vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Stop };
                vals = new object[] { true };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.StreamingContentVideo };
                vals = new object[] { false };
                base.SetStatus(tags, vals);
                Thread.Sleep(0x19);
                base.Dispose();
            }

            public override void Process()
            {
                MediaStatusPropertyTag[] tags = null;
                object[] vals = null;
                base.SetStatus(tags, vals);
                MiniDisplay.GetSystemStatus(ref this.MPStatus);
                int num = -1;
                if (this.MPStatus.MediaPlayer_Paused)
                {
                    num = 1;
                }
                else if (this.MPStatus.Media_Speed == 1)
                {
                    num = 2;
                }
                else if (this.MPStatus.Media_Speed > 1)
                {
                    num = 3;
                }
                else if (this.MPStatus.Media_Speed < 0)
                {
                    num = 4;
                }
                else if (!this.MPStatus.MediaPlayer_Paused & !this.MPStatus.MediaPlayer_Playing)
                {
                    num = 4;
                }
                if (num != this.LastPlayState)
                {
                    switch (num)
                    {
                        case 1:
                            tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Pause };
                            vals = new object[] { true };
                            Log.Info("MCEDisplay.VideoSession.Process(): Updating PlayState to PAUSED", new object[0]);
                            base.SetStatus(tags, vals);
                            break;

                        case 2:
                            tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Play };
                            vals = new object[] { true };
                            Log.Info("MCEDisplay.VideoSession.Process(): Updating PlayState to PLAY", new object[0]);
                            base.SetStatus(tags, vals);
                            break;

                        case 3:
                            tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.FF1 };
                            vals = new object[] { true };
                            Log.Info("MCEDisplay.VideoSession.Process(): Updating PlayState to FASTFORWARD", new object[0]);
                            base.SetStatus(tags, vals);
                            break;

                        case 4:
                            tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.Rewind1 };
                            vals = new object[] { true };
                            Log.Info("MCEDisplay.VideoSession.Process(): Updating PlayState to REWIND", new object[0]);
                            base.SetStatus(tags, vals);
                            break;
                    }
                    this.LastPlayState = num;
                }
                tags = new MediaStatusPropertyTag[] { MediaStatusPropertyTag.TrackTime };
                vals = new object[] { base.GetDuration2Int("#currentplaytime") };
                base.SetStatus(tags, vals);
            }
        }
    }
}

