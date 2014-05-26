#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using Action = MediaPortal.GUI.Library.Action;

// ReSharper disable CheckNamespace
namespace MediaPortal.IR
// ReSharper restore CheckNamespace
{

  #region LearningEventArgs

  /// <summary>
  /// This class will handle all communication with an external USBUIRT device
  /// The USB-UIRT, allows your PC to both Receive and Transmit infrared signals -- 
  /// exactly like those used by the collection of remote controls you've acquired for your TV, 
  /// VCR, Audio System, etc. 
  /// See www.usbuirt.com for more details on USBUIRT
  /// </summary>
  public class LearningEventArgs : EventArgs
  {
    public bool Succeeded = false;
    public string Button;
    public string IrCode = string.Empty;
    public bool IsToggledIrCode = false;
    public int TotalCodeCount = 0;
    public int CurrentCodeCount = 0;

    public LearningEventArgs(string button, string ircode, bool succeeded, bool capturingToggledIrCode, int totalCodeCount, int curCodeCount)
    {
      Button = button;
      IrCode = ircode;
      Succeeded = succeeded;
      IsToggledIrCode = capturingToggledIrCode;
      TotalCodeCount = totalCodeCount;
      CurrentCodeCount = curCodeCount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="button"></param>
    /// <param name="capturingToggledIrCode"></param>
    /// <param name="totalCodeCount"></param>
    /// <param name="curCodeCount"></param>
    public LearningEventArgs(string button, bool capturingToggledIrCode, int totalCodeCount, int curCodeCount)
    {
      Button = button;
      IsToggledIrCode = capturingToggledIrCode;
      TotalCodeCount = totalCodeCount;
      CurrentCodeCount = curCodeCount;
    }
  }

  #endregion

  public class USBUIRT : IDisposable, IComparer
  {
    #region Win API imports

    // ReSharper disable InconsistentNaming
    private const int WM_KEYDOWN = 0x0100;
    // ReSharper restore InconsistentNaming

    [DllImport("user32.dll")]
    public static extern void PostMessage(IntPtr window, int message, int wparam, int lparam);

    #endregion

    #region USBUIRT imports

    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable InconsistentNaming
    private struct UUINFO
    // ReSharper restore InconsistentNaming
    {
      // ReSharper disable FieldCanBeMadeReadOnly.Local
      // ReSharper disable MemberCanBePrivate.Local
      public int fwVersion;
      public int protVersion;
      public char fwDateDay;
      public char fwDateMonth;
      public char fwDateYear;
      // ReSharper restore MemberCanBePrivate.Local
      // ReSharper restore FieldCanBeMadeReadOnly.Local
    }

    [DllImport("uuirtdrv.dll")]
    private static extern IntPtr UUIRTOpen();

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTClose(IntPtr hHandle);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTGetDrvInfo(ref int drvAPIVersion);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTGetDrvVersion(ref int drvDLLVersion);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTGetUUIRTInfo(IntPtr hHandle, ref UUINFO puuInfo);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTGetUUIRTConfig(IntPtr hHandle, ref uint puConfig);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTSetUUIRTConfig(IntPtr hHandle, uint uConfig);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTTransmitIR(IntPtr hHandle, string irCode, int codeFormat, int repeatCount,
                                               int inactivityWaitTime, IntPtr hEvent, int res1, int res2);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTLearnIR(IntPtr hHandle, int codeFormat,
                                            [MarshalAs(UnmanagedType.LPStr)] StringBuilder ircode,
                                            IRLearnCallbackDelegate progressProc, int userData, ref int pAbort,
                                            int param1, [MarshalAs(UnmanagedType.AsAny)] Object o,
                                            [MarshalAs(UnmanagedType.AsAny)] Object oo);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTSetReceiveCallback(IntPtr hHandle, UUIRTReceiveCallbackDelegate receiveProc, int none);
    
    #endregion

    #region delegates

    public delegate void StartLearningEventHandler(object sender, LearningEventArgs e);

    public delegate void EventLearnedHandler(object sender, LearningEventArgs e);

    public delegate void EndLearnedHandler(object sender, EventArgs e);

    public delegate void RemoteCommandFeedbackHandler(object command, string irCode);

    private delegate void UUIRTReceiveCallbackDelegate(string val, IntPtr reserved);

    public delegate void IRLearnCallbackDelegate(uint val, uint val2, ulong val3);

    public delegate void OnRemoteCommand(object command);

    private delegate void ThreadSafeSendMessageDelegate(int wmMsg, int wparam, int lparam);

    #endregion

    #region constants

    // ReSharper disable InconsistentNaming
    private const int UUIRTDRV_IRFMT_UUIRT = 0x0000;
    // ReSharper restore InconsistentNaming
    
    private static readonly string Remotefile = Config.GetFile(Config.Dir.Config, "UIRTUSB-remote.xml");
    private static readonly string Tunerfile = Config.GetFile(Config.Dir.Config, "UIRTUSB-tuner.xml");

    #endregion

    #region variables

    private IntPtr _usbUirtHandle = IntPtr.Zero;
    private readonly StringBuilder _ircode = new StringBuilder("1", 2048);
    private int _abort;
    private readonly IntPtr _empty = new IntPtr(-1);
    private string _lastchannel;
    private OnRemoteCommand _remoteCommandCallback;
    private UUIRTReceiveCallbackDelegate _urcb;
    private readonly Hashtable _commandsLearned = new Hashtable();
    private readonly Hashtable _stbCommandsLearned = new Hashtable();
    private readonly Hashtable _stbToggleCommandsLearned = new Hashtable();
    private readonly Hashtable _jumpToCommands;
    private DateTime _timestamp = DateTime.Now;
    private DateTime _timestampRepeat = DateTime.Now;
    private DateTime _timestampRepeatNumbers = DateTime.Now;
    private int _lastCommand = -1;
    private bool _isLearning;

    private int _currentButtonIndex;
    private string[] _controlCodeButtonNames;
    private object[] _controlCodeCommands;
    private int[] _stbControlCodeCommands;
    private bool _waitingForIrRxLearnEvent;
    private bool _capturingToggledIrCode;
    private bool _abortLearn;
    private bool _skipLearnForCurrentCode;
    private bool _disposed;
    private string _lastIRCodeSent = string.Empty;

    #endregion

    #region jumpTo enums

    public enum JumpToActionType
    {
      // ReSharper disable InconsistentNaming
      JUMP_TO_INVALID = 10000,
      JUMP_TO_HOME, //	WINDOW_HOME
      JUMP_TO_MY_TV, //	WINDOW_TV
      JUMP_TO_MY_TV_FULLSCREEN, //	WINDOW_TVFULLSCREEN	
      JUMP_TO_MY_MOVIES, //	WINDOW_VIDEOS
      JUMP_TO_MY_MOVIES_FULLSCREEN, //	WINDOW_FULLSCREEN_VIDEO
      JUMP_TO_MY_MUSIC, //	WINDOW_MUSIC_FILES
      JUMP_TO_MY_PICTURES, //	WINDOW_PICTURES
      JUMP_TO_TV_GUIDE, //	WINDOW_TVGUIDE
      JUMP_TO_MY_RADIO, //	WINDOW_RADIO
      JUMP_TO_TELETEXT, //	WINDOW_TELETEXT
      JUMP_TO_TELETEXT_FULLSCREEN, //	WINDOW_FULLSCREEN_TELETEXT
      JUMP_TO_LASTINVALID,
      // ReSharper restore InconsistentNaming
    }

    #endregion

    #region events

    public event StartLearningEventHandler StartLearning;
    public event EventLearnedHandler OnEventLearned;
    public event EndLearnedHandler OnEndLearning;

    public event RemoteCommandFeedbackHandler OnRemoteCommandFeedback;

    #endregion

    #region properties

    /// <summary>
    /// 
    /// </summary>
    public static USBUIRT Instance { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Is3Digit { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool NeedsEnter { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool ReceiveEnabled { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool TransmitEnabled { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool AbortLearn
    {
      set
      {
        _abortLearn = value;
        _abort = _abortLearn ? 1 : 0;

        if (_abortLearn)
        {
          if (_isLearning && _waitingForIrRxLearnEvent)
          {
            _isLearning = false;
            _waitingForIrRxLearnEvent = false;
            _capturingToggledIrCode = false;
            NotifyTrainingComplete();
          }
          else
          {
            _isLearning = false;
            _waitingForIrRxLearnEvent = false;
            _capturingToggledIrCode = false;
          }
        }
      }
      get { return _abortLearn; }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool SkipLearnForCurrentCode
    {
      get { return _skipLearnForCurrentCode; }
      set
      {
        _skipLearnForCurrentCode = value;
        _abort = _skipLearnForCurrentCode ? 1 : 0;

        if (_skipLearnForCurrentCode)
        {
          if (_isLearning && _waitingForIrRxLearnEvent)
          {
            if (_currentButtonIndex < _controlCodeButtonNames.Length - 1 ||
                _currentButtonIndex < _controlCodeButtonNames.Length && !_capturingToggledIrCode)
            {
              LearnNextCode();
            }
            else
            {
              _isLearning = false;
              NotifyTrainingComplete();
            }
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int RepeatWait { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int RepeatDelay { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int CommandRepeatCount  { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int InterCommandDelay  { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool TunerCodesLoaded { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsUsbUirtLoaded { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsUsbUirtConnected
    {
      get
      {
        if (_usbUirtHandle == IntPtr.Zero || _usbUirtHandle == _empty)
        {
          return false;
        }

        uint puConfig = uint.MaxValue;
        try
        {
          IsUsbUirtLoaded = UUIRTGetUUIRTConfig(_usbUirtHandle, ref puConfig);
        }
        catch (Exception) {}

        return IsUsbUirtLoaded;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public Hashtable LearnedMediaPortalCodesTable
    {
      get { return _commandsLearned; }
    }

    /// <summary>
    /// 
    /// </summary>
    public Hashtable LearnedSTBCodesTable
    {
      get { return _stbCommandsLearned; }
    }

    #endregion

    #region ctor / dtor

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    private USBUIRT(OnRemoteCommand callback)
    {
      try
      {
        Log.Info("USBUIRT:Open");
        _commandsLearned = new Hashtable();
        _jumpToCommands = new Hashtable();
        CreateJumpToCommands();

        _usbUirtHandle = UUIRTOpen();

        if (_usbUirtHandle != _empty)
        {
          IsUsbUirtLoaded = true;
          Log.Info("USBUIRT:Open success:{0}", GetVersions());
        }
        else
        {
          Log.Info("USBUIRT:Unable to open USBUIRT driver");
        }

        if (IsUsbUirtLoaded)
        {
          Initialize();
          //setup callback to receive IR messages
          _urcb = UUIRTReceiveCallback;
          UUIRTSetReceiveCallback(_usbUirtHandle, _urcb, 0);
          RemoteCommandCallback = callback;
        }
      }

      catch (DllNotFoundException)
      {
        //most users don't have the dll on their system so will get a exception here
        Log.Info("USBUIRT:uuirtdrv.dll not found");
      }
      catch (Exception)
      {
        //most users don't have the dll on their system so will get a exception here
      }
    }

    /// <summary>
    /// 
    /// </summary>
    ~USBUIRT()
    {
      Dispose(false);
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposeManagedResources"></param>
    private void Dispose(bool disposeManagedResources)
    {
      if (!_disposed)
      {
        _disposed = true;

        if (disposeManagedResources)
        {
          // Dispose any managed resources.
        }

        var emptyPtr = new IntPtr(-1);

        if (IsUsbUirtLoaded && _usbUirtHandle != emptyPtr && _usbUirtHandle != IntPtr.Zero)
        {
          UUIRTClose(_usbUirtHandle);
          _usbUirtHandle = IntPtr.Zero;
          IsUsbUirtLoaded = false;
        }
      }
    }

    #endregion

    #region serialisation

    /// <summary>
    /// 
    /// </summary>
    private void Initialize()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        ReceiveEnabled = xmlreader.GetValueAsBool("USBUIRT", "internal", false);
        TransmitEnabled = xmlreader.GetValueAsBool("USBUIRT", "external", false);
        Is3Digit = xmlreader.GetValueAsBool("USBUIRT", "is3digit", false);
        NeedsEnter = xmlreader.GetValueAsBool("USBUIRT", "needsenter", false);
        RepeatWait = xmlreader.GetValueAsInt("USBUIRT", "repeatwait", 300);
        RepeatDelay = xmlreader.GetValueAsInt("USBUIRT", "repeatdelay", 30);
        CommandRepeatCount = xmlreader.GetValueAsInt("USBUIRT", "repeatcount", 2);
        InterCommandDelay = xmlreader.GetValueAsInt("USBUIRT", "commanddelay", 100);
      }

      if (!LoadValues())
      {
        Log.Info("USBUIRT:unable to load values from:{0}", Remotefile);
      }

      if (!LoadTunerValues())
      {
        Log.Info("USBUIRT:unable to load tunervalues from:{0}", Tunerfile);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool LoadValues()
    {
      bool result;

      try
      {
        if (!File.Exists(Remotefile))
        {
          return false;
        }

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Remotefile);
        XmlNodeList entryNodes = xmlDoc.GetElementsByTagName("entry");

        Console.WriteLine(entryNodes.Count.ToString(CultureInfo.InvariantCulture));

        foreach (XmlNode node in entryNodes)
        {
          XmlAttributeCollection codeAttribs = node.Attributes;

          if (codeAttribs != null)
          {
            string irCode = node.InnerText;
            string sActionID = codeAttribs["actionID"].InnerText;

            if (sActionID.Length > 0)
            {
              int nActionID = int.Parse(sActionID);
              if (nActionID < (int)JumpToActionType.JUMP_TO_INVALID)
              {
                _commandsLearned[irCode] = (Action.ActionType)nActionID;
              }
              else if (nActionID > (int)JumpToActionType.JUMP_TO_INVALID && nActionID < (int)JumpToActionType.JUMP_TO_LASTINVALID)
              {
                _commandsLearned[irCode] = (JumpToActionType)nActionID;
              }
            }
          }
        }

        result = true;
      }

      catch (Exception ex)
      {
        result = false;
        Console.WriteLine(ex.Message);
      }

      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool LoadTunerValues()
    {
      TunerCodesLoaded = false;

      try
      {
        if (!File.Exists(Tunerfile))
        {
          return false;
        }

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Tunerfile);
        XmlNodeList entryNodes = xmlDoc.GetElementsByTagName("entry");

        Console.WriteLine(entryNodes.Count.ToString(CultureInfo.InvariantCulture));

        foreach (XmlNode node in entryNodes)
        {
          XmlAttributeCollection codeAttribs = node.Attributes;

          if (codeAttribs != null)
          {
            string remoteCode = node.InnerText;
            string sIndex = string.Empty;

            bool bIsToggle = false;
            string sIsToggle = string.Empty;

            if (codeAttribs["index"] != null)
            {
              sIndex = codeAttribs["index"].InnerText;
            }

            if (codeAttribs["istoggle"] != null)
            {
              sIsToggle = codeAttribs["istoggle"].InnerText;
            }

            if (sIsToggle.Length > 0)
            {
              bIsToggle = bool.Parse(sIsToggle);
            }

            if (sIndex.Length > 0)
            {
              int index = int.Parse(sIndex);

              if (remoteCode.Length > 0)
              {
                TunerCodesLoaded = true;
              }

              if (bIsToggle)
              {
                _stbToggleCommandsLearned[index] = remoteCode;
              }
              else
              {
                _stbCommandsLearned[index] = remoteCode;
              }
            }
          }
        }
      }

      catch (Exception ex)
      {
        TunerCodesLoaded = false;
        Console.WriteLine(ex.Message);
      }

      return TunerCodesLoaded;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool SaveInternalValues()
    {
      bool result = false;
      XmlTextWriter writer = null;

      try
      {
        writer = new XmlTextWriter(Remotefile, Encoding.Unicode) {Formatting = Formatting.Indented};
        writer.WriteStartElement("docElement");

        // Sort by Action.ActionType before writing out to file... 
        var commandsArr = new ArrayList(_commandsLearned);
        commandsArr.Sort(this);

        foreach (object learnedCommand in commandsArr)
        {
          var entry = (DictionaryEntry)learnedCommand;

          string irCode = entry.Key.ToString();
          object command = entry.Value;

          writer.WriteStartElement("entry");
          writer.WriteAttributeString("actionID", Convert.ToInt32(command).ToString(CultureInfo.InvariantCulture));
          writer.WriteAttributeString("actionDescription", command.ToString().Replace("ACTION_", ""));
          writer.WriteString(irCode);
          writer.WriteEndElement();
        }

        writer.WriteEndElement();
        result = true;
      }

      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }

      finally
      {
        if (writer != null)
        {
          writer.Close();
        }
      }

      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool SaveTunerValues()
    {
      bool result = false;
      XmlTextWriter writer = null;

      try
      {
        writer = new XmlTextWriter(Tunerfile, Encoding.Unicode) {Formatting = Formatting.Indented};
        writer.WriteStartElement("docElement");

        for (int i = 0; i < 11; i++)
        {
          // Write the element and attributes for a "normal" code...
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("index", i.ToString(CultureInfo.InvariantCulture));

          string irTxCode = string.Empty;

          if (_stbCommandsLearned.ContainsKey(i))
          {
            writer.WriteAttributeString("istoggle", false.ToString());
            irTxCode = (string)_stbCommandsLearned[i];
          }

          writer.WriteString(irTxCode);
          writer.WriteEndElement();

          // Write the element and attributes for a "toggled" code...
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("index", i.ToString(CultureInfo.InvariantCulture));

          irTxCode = string.Empty;

          if (_stbToggleCommandsLearned.ContainsKey(i))
          {
            writer.WriteAttributeString("istoggle", true.ToString());
            irTxCode = (string)_stbToggleCommandsLearned[i];
          }

          writer.WriteString(irTxCode);
          writer.WriteEndElement();
        }

        writer.WriteEndElement();
        result = true;
      }

      catch (Exception) {}

      finally
      {
        if (writer != null)
        {
          writer.Close();
        }
      }

      TunerCodesLoaded = result;
      return result;
    }

    #endregion

    #region remote receiver methods

    /// <summary>
    /// 
    /// </summary>
    public OnRemoteCommand RemoteCommandCallback
    {
      set { _remoteCommandCallback = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="irid"></param>
    /// <param name="reserved"></param>
    public void UUIRTReceiveCallback(string irid, IntPtr reserved)
    {
      if (!ReceiveEnabled)
      {
        return;
      }

      object command = _commandsLearned[irid];

      if (command == null && !_isLearning)
      {
        if (OnRemoteCommandFeedback != null)
        {
          OnRemoteCommandFeedback(Action.ActionType.ACTION_INVALID, irid);
        }
        return;
      }

      TimeSpan ts = DateTime.Now - _timestamp;
      if (ts.TotalMilliseconds >= RepeatDelay)
      {
        if (_isLearning && _waitingForIrRxLearnEvent && ts.TotalMilliseconds > 500)
        {
          if (AbortLearn)
          {
            _isLearning = false;
            _waitingForIrRxLearnEvent = false;
            NotifyTrainingComplete();
          }

          _commandsLearned[irid] = _controlCodeCommands[_currentButtonIndex];
          int totCodeCount = _controlCodeButtonNames.Length;
          int curCodeIndex = _currentButtonIndex + 1;

          _waitingForIrRxLearnEvent = false;
          NotifyEventLearned(_controlCodeButtonNames[_currentButtonIndex], irid, true, totCodeCount, curCodeIndex);

          if (_currentButtonIndex < _controlCodeButtonNames.Length - 1 || _currentButtonIndex < _controlCodeButtonNames.Length && !_capturingToggledIrCode)
          {
            LearnNextCode();
          }
          else
          {
            _isLearning = false;
            NotifyTrainingComplete();
          }
        }
        else if (command != null)
        {
          int cmdVal = Convert.ToInt32(command);

          bool executeCommand;
          // execute new command and start the repeat delay timer
          if (cmdVal != _lastCommand)
          {
            executeCommand = true;
            _timestampRepeat = DateTime.Now;
            _timestampRepeatNumbers = DateTime.Now;
          }
          // only repeat the same command after an initial delay
          else
          {
            TimeSpan timeSpan = DateTime.Now - _timestampRepeat;
            executeCommand = timeSpan.TotalMilliseconds >= RepeatWait;

            // do not repeat numbers quickly for SMS style input
            if (executeCommand && (cmdVal >= (int) Action.ActionType.REMOTE_0 && cmdVal <= (int) Action.ActionType.REMOTE_9))
            {
              TimeSpan timeSpanNumbers = DateTime.Now - _timestampRepeatNumbers;
              if (timeSpanNumbers.TotalMilliseconds < RepeatWait)
              {
                executeCommand = false;
              }
              _timestampRepeatNumbers = DateTime.Now;
            }
          }
          _lastCommand = cmdVal;

          if (executeCommand)
          {
            if (cmdVal < (int)JumpToActionType.JUMP_TO_INVALID)
            {
              // If one of the remote numeric keys was presses, mimic a keyboard keydown message...
              if (cmdVal >= (int)Action.ActionType.REMOTE_0 && cmdVal <= (int)Action.ActionType.REMOTE_9)
              {
                int digit = cmdVal - (int)Action.ActionType.REMOTE_0;
                var keyVal = (Keys)((int)Keys.D0 + digit);
                ThreadSafeSendMessage(WM_KEYDOWN, (int)keyVal, 0);
              }
              else
              {
                var action = (Action.ActionType)command;

                if (action == Action.ActionType.ACTION_PREVIOUS_MENU)
                {
                  ThreadSafeSendMessage(WM_KEYDOWN, (int)Keys.Escape, 0);
                }
                else if (_remoteCommandCallback != null)
                {
                  _remoteCommandCallback(command);
                }
              }
            }
            else if (cmdVal > (int)JumpToActionType.JUMP_TO_INVALID && cmdVal < (int)JumpToActionType.JUMP_TO_LASTINVALID)
            {
              object windowID = _jumpToCommands[(int)command];
              command = (JumpToActionType)command;

              if (windowID != null)
              {
                var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)windowID, 0, null);
                GUIWindowManager.SendThreadMessage(msg);
              }
            }
          }

          if (OnRemoteCommandFeedback != null)
          {
            OnRemoteCommandFeedback(command, irid);
          }
        }
        _timestamp = DateTime.Now;
      }
    }

    #endregion

    #region methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="remoteCommandCallback"></param>
    /// <returns></returns>
    public static USBUIRT Create(OnRemoteCommand remoteCommandCallback)
    {
      try
      {
        if (Instance == null)
        {
          Instance = new USBUIRT(remoteCommandCallback);
        }
      }
      catch (Exception) {}

      return Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
      return "USB-UIRT";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetAPIVersions()
    {
      try
      {
        int drvAPIVersion = 0;
        UUIRTGetDrvInfo(ref drvAPIVersion);
        string drvAPIVersionName = AddDotsToInteger(drvAPIVersion);

        return drvAPIVersionName;
      }
      catch (DllNotFoundException)
      {
        // most users don't have the dll on their system so will get a exception here
        return "Driver not installed";
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetDLLVersions()
    {
      try
      {
        int drvDLLVersion = 0;
        UUIRTGetDrvVersion(ref drvDLLVersion);
        string drvDLLVersionName = AddDotsToInteger(drvDLLVersion);

        return drvDLLVersionName;
      }
      catch (DllNotFoundException)
      {
        // most users don't have the dll on their system so will get a exception here
        return "DLL uuirtdrv.dll not found";
      }
    }

    /// <summary>
    /// This method separate string with dots for better look on display.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string AddDotsToInteger(int value)
    {
      string stringedvalue = Convert.ToString(value);
      string stringConcatenedWithDot = "";
      for (int i = 0; i < stringedvalue.Length; i++)
      {
        Char character = stringedvalue[i];
        if (i != stringedvalue.Length - 1)
        {
          stringConcatenedWithDot = stringConcatenedWithDot + String.Concat(character, ".");
        }
        else
        {
          stringConcatenedWithDot = stringConcatenedWithDot + character;
        }
      }

      return stringConcatenedWithDot;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetVersions()
    {
      if (IsUsbUirtLoaded)
      {
        var p = new UUINFO();
        UUIRTGetUUIRTInfo(_usbUirtHandle, ref p);
        var firmdate = new DateTime(p.fwDateYear + 2000, p.fwDateMonth, p.fwDateDay);
        string firmversion = (p.fwVersion >> 8) + "." + (p.fwVersion & 0xff);
        return string.Format("Firmware Version: {0} ({1})", firmversion, firmdate.ToString("MMMM, dd, yyyy"));
      }
      return "USBUIRT device not detected!";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetCurrentPreferences()
    {
      uint config = 0;
      if (IsUsbUirtLoaded)
      {
        UUIRTGetUUIRTConfig(_usbUirtHandle, ref config);
      }
      return (int)config;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pref"></param>
    public void SetPreferences(int pref)
    {
      if (IsUsbUirtLoaded)
      {
        UUIRTSetUUIRTConfig(_usbUirtHandle, (uint)pref);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool Reconnect()
    {
      try
      {
        IsUsbUirtLoaded = false;
        Log.Info("USBUIRT:Re-connecting");

        if (_usbUirtHandle == IntPtr.Zero || _usbUirtHandle == _empty)
        {
          _usbUirtHandle = UUIRTOpen();
          IsUsbUirtLoaded = IsUsbUirtConnected;
        }
        else
        {
          // Release existing handle...
          UUIRTClose(_usbUirtHandle);
          _usbUirtHandle = IntPtr.Zero;
          _usbUirtHandle = UUIRTOpen();
          IsUsbUirtLoaded = IsUsbUirtConnected;
        }

        if (IsUsbUirtLoaded)
        {
          Initialize();
          _urcb = UUIRTReceiveCallback;
          UUIRTSetReceiveCallback(_usbUirtHandle, _urcb, 0);
        }
        else
        {
          Log.Info("USBUIRT:Unable to open USBUIRT driver");
        }
      }

      catch (DllNotFoundException)
      {
        // most users don't have the dll on their system so will get a exception here
        Log.Info("USBUIRT:uuirtdrv.dll not found");
      }

      catch (Exception)
      {
        // most users don't have the dll on their system so will get a exception here
      }

      return IsUsbUirtLoaded;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Close()
    {
      Dispose();
    }

    #endregion

    #region notify events

    /// <summary>
    /// Method used to fire the "StartLearning" event. Any subscribers will be notified with the name of
    /// the button that is to be learned.
    /// </summary>
    /// <param name="button"></param>
    /// <param name="totCodeCount"></param>
    /// <param name="curCodeIndex"></param>
    //protected void NotifyStartLearn(string button)
    protected void NotifyStartLearn(string button, int totCodeCount, int curCodeIndex)
    {
      if (StartLearning != null)
      {
        StartLearning(this, new LearningEventArgs(button, _capturingToggledIrCode, totCodeCount, curCodeIndex));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="button"></param>
    /// <param name="ircode"></param>
    /// <param name="isSuccess"></param>
    /// <param name="totCodeCount"></param>
    /// <param name="curCodeIndex"></param>
    protected void NotifyEventLearned(string button, string ircode, bool isSuccess, int totCodeCount, int curCodeIndex)
    {
      if (OnEventLearned != null)
      {
        OnEventLearned(this, new LearningEventArgs(button, ircode, isSuccess, _capturingToggledIrCode, totCodeCount, curCodeIndex));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    protected void NotifyTrainingComplete()
    {
      if (OnEndLearning != null)
      {
        OnEndLearning(this, EventArgs.Empty);
      }
    }

    #endregion

    #region Learning methods

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool IRLearn()
    {
      try
      {
        if (!UUIRTLearnIR(_usbUirtHandle, UUIRTDRV_IRFMT_UUIRT, _ircode, null, 0, ref _abort, 0, null, null))
        {
          return false;
        }
      }

      catch (Exception)
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stbControlCodes"></param>
    public void LearnTunerCodes(int[] stbControlCodes)
    {
      _stbControlCodeCommands = stbControlCodes;

      ThreadStart learnThreadStarter = LearnTunerCodesAsync;
      var learnThread = new Thread(learnThreadStarter) {IsBackground = true, Name = "USBUIRTLearner"};
      learnThread.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    public void LearnTunerCodesAsync()
    {
      if (_stbControlCodeCommands.Length == 0)
      {
        return;
      }

      _skipLearnForCurrentCode = false;
      AbortLearn = false;
      _isLearning = true;
      const int retries = 3;
      int totCodeCount = _stbControlCodeCommands.Length;
      string lastIrCodeLearned = string.Empty;

      for (int i = 0; i < _stbControlCodeCommands.Length; i++)
      {
        int keyVal = _stbControlCodeCommands[i];
        string btnName = (keyVal == 10 ? "Enter" : keyVal.ToString(CultureInfo.InvariantCulture));

        if (_skipLearnForCurrentCode)
        {
          _skipLearnForCurrentCode = false;
          _abort = 0;
        }

        if (_abortLearn)
        {
          break;
        }

        for (int retry = 0; retry < retries * 2; retry++)
        {
          NotifyStartLearn(btnName, totCodeCount, (_capturingToggledIrCode ? i + 1 : i));
          bool result = IRLearn();

          if (_abort == 1 || _abortLearn || _skipLearnForCurrentCode)
          {
            break;
          }

          string irCodeString = _ircode.ToString();
          Console.WriteLine("Last Code Learned: " + lastIrCodeLearned);
          Console.WriteLine(" New Code Learned: " + irCodeString + "\r\n");

          // Certain code formats such as RC5 and RC6 toggle a bit on consecutive key presses.  
          // To catch these we need to capture 2 separate button presses for each button...
          if (_capturingToggledIrCode && String.Compare(irCodeString, lastIrCodeLearned, StringComparison.Ordinal) != 0)
          {
            _stbToggleCommandsLearned[keyVal] = irCodeString;
          }
          else
          {
            _stbCommandsLearned[keyVal] = _ircode.ToString();
          }

          lastIrCodeLearned = irCodeString;

          NotifyEventLearned(btnName, _ircode.ToString(), result, totCodeCount, i + 1);

          if (result && _capturingToggledIrCode)
          {
            _capturingToggledIrCode = false;
            break;
          }

          _capturingToggledIrCode = true;
        }
      }

      _isLearning = false;
      NotifyTrainingComplete();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="buttonNames"></param>
    public void BulkLearn(object[] commands, string[] buttonNames)
    {
      BulkLearn(commands, buttonNames, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="buttonNames"></param>
    /// <param name="clearCommands"></param>
    public void BulkLearn(object[] commands, string[] buttonNames, bool clearCommands)
    {
      if (clearCommands)
      {
        _commandsLearned.Clear();
      }

      _controlCodeCommands = commands;
      _controlCodeButtonNames = buttonNames;
      _capturingToggledIrCode = false;

      if (commands.Length != buttonNames.Length)
      {
        throw new Exception("invalid call to BulkLearn");
      }

      _skipLearnForCurrentCode = false;
      AbortLearn = false;
      _currentButtonIndex = 0;
      _isLearning = true;
      _waitingForIrRxLearnEvent = true;

      NotifyStartLearn(_controlCodeButtonNames[_currentButtonIndex], commands.Length, _currentButtonIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    private void LearnNextCode()
    {
      // Certain code formats such as RC5 and RC6 toggle a bit on consecutive key presses.  
      // To catch these we need to capture 2 separate button presses for each button...
      _capturingToggledIrCode = !_capturingToggledIrCode;
      NotifyStartLearn(_controlCodeButtonNames[_capturingToggledIrCode ? _currentButtonIndex : ++_currentButtonIndex],
                       _controlCodeCommands.Length, _currentButtonIndex);

      _waitingForIrRxLearnEvent = true;
      _isLearning = true;
    }

    #endregion

    #region remote control methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="ignoreLastChannel"></param>
    public void ChangeTunerChannel(string channel, bool ignoreLastChannel)
    {
      if (ignoreLastChannel)
      {
        _lastchannel = "";
      }
      ChangeTunerChannel(channel);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channel"></param>
    public void ChangeTunerChannel(string channel)
    {
      if (!IsUsbUirtLoaded)
      {
        return;
      }

      if (!TransmitEnabled)
      {
        return;
      }

      Log.Info("USBUIRT: NewChannel={0} LastChannel={1}", channel, _lastchannel);

      // Already tuned to this channel?
      if (channel == _lastchannel)
      {
        return;
      }
      int length = channel.Length;

      // Some STB's allow more than 3 digit channel numbers!
      if (Is3Digit && length > 3)
      {
        Log.Info("USBUIRT: invalid channel:{0}", channel);
        return;
      }

      for (int i = 0; i < length; i++)
      {
        if (channel[i] < '0' || channel[i] > '9')
        {
          continue;
        }

        int codeIndex = channel[i] - '0';
        bool isToggledCode = false;
        string irTxString = GetSTBIrCode(codeIndex, ref isToggledCode);
        Log.Info("USBUIRT: send:{0}{1}", channel[i], (isToggledCode ? " (toggled)" : ""));

        if (irTxString.Length == 0)
        {
          Log.Info(string.Format("USBUIRT: IR Code for [{0}] button is empty", codeIndex));
          continue;
        }

        Transmit(irTxString, UUIRTDRV_IRFMT_UUIRT, CommandRepeatCount);
      }

      if (NeedsEnter)
      {
        const int codeIndex = 10;
        bool isToggledCode = false;
        string irTxString = GetSTBIrCode(codeIndex, ref isToggledCode);
        Log.Info("USBUIRT: send enter{0}", (isToggledCode ? " (toggled)" : ""));

        if (irTxString.Length == 0)
        {
          Log.Info("USBUIRT: IR Code for enter button is empty");
        }

        else
        {
          Transmit(irTxString, UUIRTDRV_IRFMT_UUIRT, CommandRepeatCount);
        }
      }

      // All succeeded, remember last channel
      _lastchannel = channel;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="codeIndex"></param>
    /// <param name="isToggledCode"></param>
    /// <returns></returns>
    // ReSharper disable RedundantAssignment
    private string GetSTBIrCode(int codeIndex, ref bool isToggledCode)
    // ReSharper restore RedundantAssignment
    {
      string irTxString = "";
      string toggledIrTxString = "";
      string irOut;

      if (_stbCommandsLearned.ContainsKey(codeIndex))
      {
        irTxString = _stbCommandsLearned[codeIndex].ToString();
      }

      if (_stbToggleCommandsLearned.ContainsKey(codeIndex))
      {
        toggledIrTxString = _stbToggleCommandsLearned[codeIndex].ToString();
      }

      // is the code we're sending identical to the last one sent?
      // If so, check if there's a toggled version of the code...
      if (toggledIrTxString.Length > 0 && (String.Compare(_lastIRCodeSent, irTxString, StringComparison.Ordinal) == 0))
      {
        isToggledCode = true;
        irOut = toggledIrTxString;
      }
      else
      {
        isToggledCode = false;
        irOut = irTxString;
      }

      _lastIRCodeSent = irOut;
      return irOut;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gIRCode"></param>
    /// <param name="gIRCodeFormat"></param>
    /// <param name="repeatCount"></param>
    public void Transmit(string gIRCode, int gIRCodeFormat, int repeatCount)
    {
      if (!IsUsbUirtLoaded)
      {
        return;
      }
      if (!TransmitEnabled)
      {
        return;
      }

      bool result = UUIRTTransmitIR(_usbUirtHandle,
                                    gIRCode, // IRCode 
                                    gIRCodeFormat, // codeFormat 
                                    repeatCount, // repeatCount 
                                    0, // inactivityWaitTime 
                                    IntPtr.Zero, // hEvent 
                                    0, // reserved1
                                    0 // reserved2 
                                   );

      if (!result)
      {
        Log.Info("USBUIRT: unable to transmit code");
      }
      else
      {
        Thread.Sleep(InterCommandDelay);
      }
    }

    #endregion

    #region misc methods

    /// <summary>
    /// 
    /// </summary>
    private void CreateJumpToCommands()
    {
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_HOME] = GUIWindow.Window.WINDOW_HOME;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_MY_TV] = GUIWindow.Window.WINDOW_TV;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_MY_TV_FULLSCREEN] = GUIWindow.Window.WINDOW_TVFULLSCREEN;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_MY_MOVIES] = GUIWindow.Window.WINDOW_VIDEOS;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_MY_MOVIES_FULLSCREEN] = GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_MY_MUSIC] = GUIWindow.Window.WINDOW_MUSIC_FILES;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_MY_PICTURES] = GUIWindow.Window.WINDOW_PICTURES;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_TV_GUIDE] = GUIWindow.Window.WINDOW_TVGUIDE;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_MY_RADIO] = GUIWindow.Window.WINDOW_RADIO;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_TELETEXT] = GUIWindow.Window.WINDOW_TELETEXT;
      _jumpToCommands[(int)JumpToActionType.JUMP_TO_TELETEXT_FULLSCREEN] = GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actionType"></param>
    /// <param name="irCmd1"></param>
    /// <param name="irCmd2"></param>
    /// <returns></returns>
    public bool GetCommandIrStrings(Action.ActionType actionType, ref string irCmd1, ref string irCmd2)
    {
      bool irCmd1Found = false;
      bool irCmd2Found = false;

      foreach (object entry in _commandsLearned.Keys)
      {
        string irCode = entry.ToString();
        object command = _commandsLearned[irCode];

        if ((Action.ActionType)command == actionType)
        {
          if (!irCmd1Found)
          {
            irCmd1 = irCode;
            irCmd1Found = true;
          }
          else
          {
            irCmd2 = irCode;
            irCmd2Found = true;
          }
        }

        if (irCmd1Found && irCmd2Found)
        {
          break;
        }
      }

      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      return irCmd1Found || irCmd2Found;
      // ReSharper restore ConditionIsAlwaysTrueOrFalse
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool ClearLearnedCommand(object command)
    {
      bool result = false;

      // no point iterating through the hash table if it doesn't contain this ActionType
      if (!_commandsLearned.ContainsValue(command))
      {
        return false;
      }

      // Can't remove items while enumerating a Hashtable so we do it this way...
      var commandsArr = new ArrayList(_commandsLearned);
      commandsArr.Sort(this);

      foreach (var entry in commandsArr.Cast<DictionaryEntry>().Where(entry => (int)entry.Value == (int)command))
      {
        _commandsLearned.Remove(entry.Key);
        result = true;
      }

      if (result)
      {
        SaveInternalValues();
      }

      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool ClearAllLearnedCommands()
    {
      int entryCount = _commandsLearned.Count;
      _commandsLearned.Clear();

      bool result = entryCount != _commandsLearned.Count;
      if (result)
      {
        SaveInternalValues();
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool ClearAllLearnedSTBCommands()
    {
      // is there anything to clear?
      if (_stbCommandsLearned.Count + _stbToggleCommandsLearned.Count == 0)
      {
        return false;
      }

      _stbCommandsLearned.Clear();
      _stbToggleCommandsLearned.Clear();

      bool result = _stbCommandsLearned.Count == 0 && _stbToggleCommandsLearned.Count == 0;
      if (result)
      {
        SaveTunerValues();
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool ClearLearnedSTBCommand(int command)
    {
      bool result = false;

      // no point iterating through the hash tables if they don't contain this command
      if (!_stbCommandsLearned.ContainsKey(command) && !_stbToggleCommandsLearned.ContainsKey(command))
      {
        return false;
      }

      if (_stbCommandsLearned.ContainsKey(command))
      {
        _stbCommandsLearned.Remove(command);
        result = true;
      }

      if (_stbToggleCommandsLearned.ContainsKey(command))
      {
        _stbToggleCommandsLearned.Remove(command);
        result = true;
      }

      if (result)
      {
        SaveTunerValues();
      }

      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cmdNumber"></param>
    /// <param name="irCmd1"></param>
    /// <param name="irCmd2"></param>
    /// <returns></returns>
    public bool GetSTBCommandIrStrings(int cmdNumber, ref string irCmd1, ref string irCmd2)
    {
      bool result = false;

      if (_stbCommandsLearned.ContainsKey(cmdNumber))
      {
        result = true;
        irCmd1 = (string)_stbCommandsLearned[cmdNumber];
      }

      if (_stbToggleCommandsLearned.ContainsKey(cmdNumber))
      {
        result = true;
        irCmd2 = (string)_stbToggleCommandsLearned[cmdNumber];
      }

      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wmMsg"></param>
    /// <param name="wparam"></param>
    /// <param name="lparam"></param>
    private static void ThreadSafeSendMessage(int wmMsg, int wparam, int lparam)
    {
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        ThreadSafeSendMessageDelegate d = ThreadSafeSendMessage;
        GUIGraphicsContext.form.Invoke(d, new object[] {wmMsg, wparam, lparam});
      }
      else
      {
        PostMessage(GUIGraphicsContext.form.Handle, wmMsg, wparam, lparam);
      }
    }

    #endregion

    #region IComparer Members

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int Compare(object x, object y)
    {
      var dictX = (DictionaryEntry)x;
      var dictY = (DictionaryEntry)y;

      var actionValX = (int)dictX.Value;
      var actionValY = (int)dictY.Value;

      return actionValX.CompareTo(actionValY);
    }

    #endregion
  }
}