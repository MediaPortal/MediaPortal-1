#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace MediaPortal.IR
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

    public LearningEventArgs(string button, string ircode, bool succeeded,
                             bool capturingToggledIrCode, int totalCodeCount, int curCodeCount)
    {
      Button = button;
      IrCode = ircode;
      Succeeded = succeeded;
      IsToggledIrCode = capturingToggledIrCode;
      TotalCodeCount = totalCodeCount;
      CurrentCodeCount = curCodeCount;
    }

    public LearningEventArgs(string button, bool capturingToggledIrCode,
                             int totalCodeCount, int curCodeCount)
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

    private const int WM_KEYDOWN = 0x0100;

    [DllImport("user32.dll")]
    public static extern void PostMessage(IntPtr window, int message, int wparam, int lparam);

    #endregion

    #region USBUIRT imports

    [StructLayout(LayoutKind.Sequential)]
    private struct UUINFO
    {
      public int fwVersion;
      public int protVersion;
      public char fwDateDay;
      public char fwDateMonth;
      public char fwDateYear;
    }

    //Not used
    //[StructLayout(LayoutKind.Sequential)]
    //private struct UUGPIO
    //{
    //  public byte[] irCode;
    //  public byte action;
    //  public byte duration;
    //}

    [DllImport("uuirtdrv.dll")]
    private static extern IntPtr UUIRTOpen();

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTClose(IntPtr hHandle);

    //[DllImport("uuirtdrv.dll")]
    //private static extern bool UUIRTGetDrvInfo(ref int puDrvVersion);

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
    private static extern bool UUIRTTransmitIR(IntPtr hHandle, string IRCode, int codeFormat, int repeatCount,
                                               int inactivityWaitTime, IntPtr hEvent, int res1, int res2);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTLearnIR(IntPtr hHandle, int codeFormat,
                                            [MarshalAs(UnmanagedType.LPStr)] StringBuilder ircode,
                                            IRLearnCallbackDelegate progressProc, int userData, ref int pAbort,
                                            int param1, [MarshalAs(UnmanagedType.AsAny)] Object o,
                                            [MarshalAs(UnmanagedType.AsAny)] Object oo);

    [DllImport("uuirtdrv.dll")]
    private static extern bool UUIRTSetReceiveCallback(IntPtr hHandle, UUIRTReceiveCallbackDelegate receiveProc,
                                                       int none);

    //[DllImport("uuirtdrv.dll")]
    //private static extern bool UUIRTSetUUIRTGPIOCfg(IntPtr hHandle, int index, ref UUGPIO GpioSt);

    //HUUHANDLE	  hHandle, int index, PUUGPIO pGpioSt);

    //[DllImport("uuirtdrv.dll")]
    //private static extern bool UUIRTGetUUIRTGPIOCfg(IntPtr hHandle, ref int numSlots, ref uint dwPortPins,
    //                                                ref UUGPIO GpioSt);

    //(HUUHANDLE hHandle, int *pNumSlots, UINT32 *pdwPortPins, PUUGPIO pGPIOStruct);

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

    private static int UUIRTDRV_IRFMT_UUIRT = 0x0000;
    static readonly string remotefile = Config.GetFile(Config.Dir.Config, "UIRTUSB-remote.xml");
    static readonly string tunerfile  = Config.GetFile(Config.Dir.Config, "UIRTUSB-tuner.xml");
    //private const string    USBUIRT_PLUGINVER = "1.1 (December 23, 2005)";

    #endregion

    #region variables

    private IntPtr UsbUirtHandle = IntPtr.Zero;
    private StringBuilder ircode = new StringBuilder("1", 2048);
    private int abort = 0;
    private int timelaps = 300; // time in milliseconds between two accepted commands
    private IntPtr empty = new IntPtr(-1);
    private bool isUsbUirtLoaded = false;
    private string lastchannel;
    private OnRemoteCommand remoteCommandCallback = null;
    private UUIRTReceiveCallbackDelegate urcb = null;
    private bool accepRemoteCommands = false;
    private bool transmitEventsEnabled = false;
    private bool is3DigitTuner = false;
    private bool tunerNeedsEnter = false;
    private static USBUIRT instance = null;
    private Hashtable commandsLearned = new Hashtable();
    private Hashtable stbCommandsLearned = new Hashtable();
    private Hashtable stbToggleCommandsLearned = new Hashtable();
    private Hashtable jumpToCommands = null;
    private DateTime timestamp = DateTime.Now;
    private bool isLearning = false;

    private int currentButtonIndex = 0;
    private string[] controlCodeButtonNames;
    private object[] controlCodeCommands;
    private int[] stbControlCodeCommands;
    private bool waitingForIrRxLearnEvent = false;
    private bool capturingToggledIrCode = false;
    private bool abortLearn = false;
    private bool skipLearnForCurrentCode = false;
    private bool disposed = false;

    private int commandRepeatCount = 1;
    private int interCommandDelay = 100;
    private bool tunerCodesLoaded = false;
    private string lastIRCodeSent = string.Empty;
    //private bool                            lastIRCodeSentWasToggle = false;

    #endregion

    #region jumpTo enums

    //private const int FirstJumpToVal = 10000;

    public enum JumpToActionType
    {
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
      JUMP_TO_MY_WEATHER, //	WINDOW_WEATHER
      JUMP_TO_LASTINVALID,
    }

    #endregion

    #region events

    public event StartLearningEventHandler StartLearning;
    public event EventLearnedHandler OnEventLearned;
    public event EndLearnedHandler OnEndLearning;

    public event RemoteCommandFeedbackHandler OnRemoteCommandFeedback;

    #endregion

    #region properties

    public static USBUIRT Instance
    {
      get { return instance; }
    }

    public bool Is3Digit
    {
      get { return is3DigitTuner; }

      set { is3DigitTuner = value; }
    }

    public bool NeedsEnter
    {
      get { return tunerNeedsEnter; }

      set { tunerNeedsEnter = value; }
    }


    public bool ReceiveEnabled
    {
      get { return accepRemoteCommands; }

      set { accepRemoteCommands = value; }
    }

    public bool TransmitEnabled
    {
      get { return transmitEventsEnabled; }

      set { transmitEventsEnabled = value; }
    }

    public int TimeLaps
    {
      set { timelaps = value; }
      get { return timelaps; }
    }

    public bool AbortLearn
    {
      set
      {
        abortLearn = value;
        abort = abortLearn ? 1 : 0;

        if (abortLearn)
        {
          if (isLearning && waitingForIrRxLearnEvent)
          {
            isLearning = false;
            waitingForIrRxLearnEvent = false;
            capturingToggledIrCode = false;
            NotifyTrainingComplete();
          }

          else
          {
            isLearning = false;
            waitingForIrRxLearnEvent = false;
            capturingToggledIrCode = false;
          }
        }
      }
      get { return abortLearn; }
    }

    public bool SkipLearnForCurrentCode
    {
      get { return skipLearnForCurrentCode; }
      set
      {
        skipLearnForCurrentCode = value;
        abort = skipLearnForCurrentCode ? 1 : 0;

        if (skipLearnForCurrentCode)
        {
          if (isLearning && waitingForIrRxLearnEvent)
          {
            if (currentButtonIndex < controlCodeButtonNames.Length - 1 ||
                currentButtonIndex < controlCodeButtonNames.Length && !capturingToggledIrCode)
            {
              LearnNextCode();
            }

            else
            {
              isLearning = false;
              NotifyTrainingComplete();
            }
          }
        }
      }
    }

    public int CommandRepeatCount
    {
      get { return commandRepeatCount; }
      set { commandRepeatCount = value; }
    }

    public int InterCommandDelay
    {
      get { return interCommandDelay; }
      set { interCommandDelay = value; }
    }

    public bool TunerCodesLoaded
    {
      get { return tunerCodesLoaded; }
    }

    public bool IsUsbUirtLoaded
    {
      get { return isUsbUirtLoaded; }
    }

    public bool IsUsbUirtConnected
    {
      get
      {
        if (UsbUirtHandle == IntPtr.Zero || UsbUirtHandle == empty)
        {
          return false;
        }

        uint puConfig = uint.MaxValue;

        try
        {
          isUsbUirtLoaded = UUIRTGetUUIRTConfig(UsbUirtHandle, ref puConfig);
        }

        catch (Exception)
        {
        }

        return isUsbUirtLoaded;
      }
    }

    public Hashtable LearnedMediaPortalCodesTable
    {
      get { return commandsLearned; }
    }

    public Hashtable LearnedSTBCodesTable
    {
      get { return stbCommandsLearned; }
    }

    #endregion

    #region ctor / dtor

    private USBUIRT(OnRemoteCommand callback)
    {
      try
      {
        Log.Info("USBUIRT:Open");
        commandsLearned = new Hashtable();
        jumpToCommands = new Hashtable();
        CreateJumpToCommands();

        UsbUirtHandle = UUIRTOpen();

        if (UsbUirtHandle != empty)
        {
          isUsbUirtLoaded = true;
          Log.Info("USBUIRT:Open success:{0}", GetVersions());
        }

        else
        {
          Log.Info("USBUIRT:Unable to open USBUIRT driver");
        }

        if (isUsbUirtLoaded)
        {
          Initialize();

          //setup callack to receive IR messages
          urcb = new UUIRTReceiveCallbackDelegate(UUIRTReceiveCallback);
          UUIRTSetReceiveCallback(UsbUirtHandle, urcb, 0);
          RemoteCommandCallback = callback;
        }
      }

      catch (DllNotFoundException)
      {
        //most users dont have the dll on their system so will get a exception here
        Log.Info("USBUIRT:uuirtdrv.dll not found");
      }

      catch (Exception)
      {
        //most users dont have the dll on their system so will get a exception here
      }
    }

    ~USBUIRT()
    {
      Dispose(false);
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposeManagedResources)
    {
      if (!disposed)
      {
        disposed = true;

        if (disposeManagedResources)
        {
          // Dispose any managed resources.
        }

        IntPtr emptyPtr = new IntPtr(-1);

        if (isUsbUirtLoaded && UsbUirtHandle != emptyPtr && UsbUirtHandle != IntPtr.Zero)
        {
          UUIRTClose(UsbUirtHandle);
          UsbUirtHandle = IntPtr.Zero;
          isUsbUirtLoaded = false;
        }
      }
    }

    #endregion

    #region serialisation

    private void Initialize()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        ReceiveEnabled = xmlreader.GetValueAsBool("USBUIRT", "internal", false);
        TransmitEnabled = xmlreader.GetValueAsBool("USBUIRT", "external", false);
        Is3Digit = xmlreader.GetValueAsBool("USBUIRT", "is3digit", false);
        tunerNeedsEnter = xmlreader.GetValueAsBool("USBUIRT", "needsenter", false);

        CommandRepeatCount = xmlreader.GetValueAsInt("USBUIRT", "repeatcount", 2);
        InterCommandDelay = xmlreader.GetValueAsInt("USBUIRT", "commanddelay", 100);
      }

      if (!LoadValues())
      {
        Log.Info("USBUIRT:unable to load values from:{0}", remotefile);
      }

      if (!LoadTunerValues())
      {
        Log.Info("USBUIRT:unable to load tunervalues from:{0}", tunerfile);
      }
    }

    private bool LoadValues()
    {
      bool result;

      try
      {
        if (!File.Exists(remotefile))
        {
          return false;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(remotefile);
        XmlNodeList entryNodes = xmlDoc.GetElementsByTagName("entry");

        Console.WriteLine(entryNodes.Count.ToString());

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
              if (nActionID < (int) JumpToActionType.JUMP_TO_INVALID)
              {
                commandsLearned[irCode] = (Action.ActionType) nActionID;
              }

              else if (nActionID > (int) JumpToActionType.JUMP_TO_INVALID &&
                       nActionID < (int) JumpToActionType.JUMP_TO_LASTINVALID)
              {
                commandsLearned[irCode] = (JumpToActionType) nActionID;
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

    private bool LoadTunerValues()
    {
      tunerCodesLoaded = false;

      try
      {
        if (!File.Exists(tunerfile))
        {
          return false;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(tunerfile);
        XmlNodeList entryNodes = xmlDoc.GetElementsByTagName("entry");

        Console.WriteLine(entryNodes.Count.ToString());

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
                tunerCodesLoaded = true;
              }

              if (bIsToggle)
              {
                stbToggleCommandsLearned[index] = remoteCode;
              }

              else
              {
                stbCommandsLearned[index] = remoteCode;
              }
              //externalTunerCodes[index] = remoteCode;
            }
          }
        }
      }

      catch (Exception ex)
      {
        tunerCodesLoaded = false;
        Console.WriteLine(ex.Message);
      }

      return tunerCodesLoaded;
    }

    public bool SaveInternalValues()
    {
      bool result = false;
      XmlTextWriter writer = null;

      try
      {
        writer = new XmlTextWriter(remotefile, Encoding.Unicode);
        writer.Formatting = Formatting.Indented;
        writer.WriteStartElement("docElement");

        // Sort by Action.ActionType before writing out to file... 
        ArrayList commandsArr = new ArrayList(commandsLearned);
        commandsArr.Sort(this);

        for (int i = 0; i < commandsArr.Count; i++)
        {
          // Key:		IR Code String
          // Value:	Action.ActionType
          DictionaryEntry entry = (DictionaryEntry) commandsArr[i];

          string irCode = entry.Key.ToString();
          object command = entry.Value;

          writer.WriteStartElement("entry");
          writer.WriteAttributeString("actionID", Convert.ToInt32(command).ToString());
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

    public bool SaveTunerValues()
    {
      bool result = false;
      XmlTextWriter writer = null;

      try
      {
        writer = new XmlTextWriter(tunerfile, Encoding.Unicode);
        writer.Formatting = Formatting.Indented;
        writer.WriteStartElement("docElement");

        for (int i = 0; i < 11; i++)
        {
          // Write the element and attributes for a "normal" code...
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("index", i.ToString());

          string irTxCode = string.Empty;

          if (stbCommandsLearned.ContainsKey(i))
          {
            writer.WriteAttributeString("istoggle", false.ToString());
            irTxCode = (string) stbCommandsLearned[i];
          }

          writer.WriteString(irTxCode);
          writer.WriteEndElement();

          // Write the element and attributes for a "toggled" code...
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("index", i.ToString());

          irTxCode = string.Empty;

          if (stbToggleCommandsLearned.ContainsKey(i))
          {
            writer.WriteAttributeString("istoggle", true.ToString());
            irTxCode = (string) stbToggleCommandsLearned[i];
          }

          writer.WriteString(irTxCode);
          writer.WriteEndElement();
        }

        writer.WriteEndElement();
        result = true;
      }

      catch (Exception)
      {
      }

      finally
      {
        if (writer != null)
        {
          writer.Close();
        }
      }

      tunerCodesLoaded = result;
      return result;
    }

    #endregion

    #region remote receiver methods

    public OnRemoteCommand RemoteCommandCallback
    {
      set { remoteCommandCallback = value; }
    }

    public void UUIRTReceiveCallback(string irid, IntPtr reserved)
    {
      if (!ReceiveEnabled)
      {
        return;
      }

      object command = commandsLearned[irid];

      if (command == null && !isLearning)
      {
        if (OnRemoteCommandFeedback != null)
        {
          OnRemoteCommandFeedback(Action.ActionType.ACTION_INVALID, irid);
        }

        return;
      }

      TimeSpan ts = DateTime.Now - timestamp;

      if (ts.TotalMilliseconds >= timelaps)
      {
        if (isLearning && waitingForIrRxLearnEvent && ts.TotalMilliseconds > 500)
        {
          if (AbortLearn)
          {
            isLearning = false;
            waitingForIrRxLearnEvent = false;

            NotifyTrainingComplete();
          }

          commandsLearned[irid] = controlCodeCommands[currentButtonIndex];
          int totCodeCount = controlCodeButtonNames.Length;
          int curCodeIndex = currentButtonIndex + 1;

          waitingForIrRxLearnEvent = false;
          NotifyEventLearned(controlCodeButtonNames[currentButtonIndex], irid, true, totCodeCount, curCodeIndex);

          if (currentButtonIndex < controlCodeButtonNames.Length - 1 ||
              currentButtonIndex < controlCodeButtonNames.Length && !capturingToggledIrCode)
          {
            LearnNextCode();
          }

          else
          {
            isLearning = false;
            NotifyTrainingComplete();
          }
        }

        else if (command != null)
        {
          int cmdVal = Convert.ToInt32(command);

          if (cmdVal < (int) JumpToActionType.JUMP_TO_INVALID)
          {
            // If one of the romote numeric keys was presses, mimic a keyboard keydown message...
            if (cmdVal >= (int) Action.ActionType.REMOTE_0 && cmdVal <= (int) Action.ActionType.REMOTE_9)
            {
              int digit = cmdVal - (int) Action.ActionType.REMOTE_0;
              Keys keyVal = (Keys) ((int) Keys.D0 + digit);

              ThreadSafeSendMessage(WM_KEYDOWN, (int) keyVal, 0);
            }

              //else if (remoteCommandCallback != null)
              //        remoteCommandCallback(command);
            else
            {
              Action.ActionType action = (Action.ActionType) command;

              if (action == Action.ActionType.ACTION_PREVIOUS_MENU)
              {
                //GUIWindowManager.ShowPreviousWindow();
                ThreadSafeSendMessage(WM_KEYDOWN, (int) Keys.Escape, 0);
              }

              else if (remoteCommandCallback != null)
              {
                remoteCommandCallback(command);
              }
            }
          }

          else if (cmdVal > (int) JumpToActionType.JUMP_TO_INVALID && cmdVal < (int) JumpToActionType.JUMP_TO_LASTINVALID)
          {
            object windowID = jumpToCommands[(int) command];
            command = (JumpToActionType) command;

            if (windowID != null)
            {
              GUIMessage msg =
                new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int) windowID, 0, null);
              GUIWindowManager.SendThreadMessage(msg);
            }
          }

          if (OnRemoteCommandFeedback != null)
          {
            OnRemoteCommandFeedback(command, irid);
          }
        }

        timestamp = DateTime.Now;
      }
    }

    #endregion

    #region methods

    public static USBUIRT Create(OnRemoteCommand remoteCommandCallback)
    {
      try
      {
        if (instance == null)
        {
          instance = new USBUIRT(remoteCommandCallback);
        }
      }

      catch (Exception)
      {
      }

      return instance;
    }

    public string GetName()
    {
      return "USB-UIRT";
    }

    // this method returns the API version number wich is integrated
    // in the DLL. We don't need to open driver before. 
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
        //most users dont have the dll on their system so will get a exception here
        return "Driver not installed";
      }
    }

    // this method returns the DLL version number wich is integrated in the DLL
    // We don't need to open driver before. 
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
        //most users dont have the dll on their system so will get a exception here
        return "DLL uuirtdrv.dll not found";
      }
    }

    // this method separate string with dots for better look on display.
    private string AddDotsToInteger(int value)
    {
      string Stringedvalue = Convert.ToString(value);
      string StringConcatenedWithDot = "";
      Char character;
      for (int i = 0; i < Stringedvalue.Length; i++)
      {
        character = Stringedvalue[i];
        if (i != Stringedvalue.Length - 1)
        {
          StringConcatenedWithDot = StringConcatenedWithDot + String.Concat(character, ".");
        }
        else
        {
          StringConcatenedWithDot = StringConcatenedWithDot + character;
        }
      }

      return StringConcatenedWithDot;
    }

    public string GetVersions()
    {
      if (isUsbUirtLoaded)
      {
        UUINFO p = new UUINFO();
        UUIRTGetUUIRTInfo(UsbUirtHandle, ref p);

        DateTime firmdate = new DateTime(p.fwDateYear + 2000, p.fwDateMonth, p.fwDateDay);

        string firmversion = (p.fwVersion >> 8) + "." + (p.fwVersion & 0xff);
        //string plug = string.Format("Plugin Version: {0}", USBUIRT_PLUGINVER);
        //string firm = string.Format("Firmware Version: {0} ({1})", firmversion, firmdate.ToString("MMMM, dd, yyyy"));
        //return string.Format("{0}\r\n{1}", plug, firm);

        return string.Format("Firmware Version: {0} ({1})", firmversion, firmdate.ToString("MMMM, dd, yyyy"));
      }

      else
      {
        return "USBUIRT device not detected!";
      }
    }

    public int GetCurrentPreferences()
    {
      uint config = 0;
      if (isUsbUirtLoaded)
      {
        UUIRTGetUUIRTConfig(UsbUirtHandle, ref config);
      }
      return (int) config;
    }

    public void SetPreferences(int pref)
    {
      if (isUsbUirtLoaded)
      {
        UUIRTSetUUIRTConfig(UsbUirtHandle, (uint) pref);
      }
    }

    public bool Reconnect()
    {
      try
      {
        isUsbUirtLoaded = false;
        Log.Info("USBUIRT:Re-connecting");

        if (UsbUirtHandle == IntPtr.Zero || UsbUirtHandle == empty)
        {
          UsbUirtHandle = UUIRTOpen();
          isUsbUirtLoaded = IsUsbUirtConnected;
        }

        else
        {
          // Release existing handle...
          UUIRTClose(UsbUirtHandle);
          UsbUirtHandle = IntPtr.Zero;
          UsbUirtHandle = UUIRTOpen();
          isUsbUirtLoaded = IsUsbUirtConnected;
        }

        if (isUsbUirtLoaded)
        {
          Initialize();
          urcb = new UUIRTReceiveCallbackDelegate(UUIRTReceiveCallback);
          UUIRTSetReceiveCallback(UsbUirtHandle, urcb, 0);
        }

        else
        {
          Log.Info("USBUIRT:Unable to open USBUIRT driver");
        }
      }

      catch (DllNotFoundException)
      {
        //most users dont have the dll on their system so will get a exception here
        Log.Info("USBUIRT:uuirtdrv.dll not found");
      }

      catch (Exception)
      {
        //most users dont have the dll on their system so will get a exception here
      }

      return isUsbUirtLoaded;
    }

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
    //protected void NotifyStartLearn(string button)
    protected void NotifyStartLearn(string button, int totCodeCount, int curCodeIndex)
    {
      if (StartLearning != null)
      {
        StartLearning(this, new LearningEventArgs(button, capturingToggledIrCode, totCodeCount, curCodeIndex));
      }
    }

    protected void NotifyEventLearned(string button, string _ircode, bool isSuccess, int totCodeCount, int curCodeIndex)
    {
      if (OnEventLearned != null)
      {
        OnEventLearned(this, new LearningEventArgs(button, _ircode, isSuccess,
                                                   capturingToggledIrCode, totCodeCount, curCodeIndex));
      }
    }

    protected void NotifyTrainingComplete()
    {
      if (OnEndLearning != null)
      {
        OnEndLearning(this, EventArgs.Empty);
      }
    }

    #endregion

    #region Learning methods

    private bool IRLearn()
    {
      try
      {
        if (!UUIRTLearnIR(UsbUirtHandle, UUIRTDRV_IRFMT_UUIRT, ircode, null, 0, ref abort, 0, null, null))
        {
          return false;
        }

        else
        {
          //uirt-raw is the format
        }
      }

      catch (Exception)
      {
        return false;
      }

      return true;
    }

    public void LearnTunerCodes(int[] stbControlCodes)
    {
      stbControlCodeCommands = stbControlCodes;

      ThreadStart learnThreadStarter = new ThreadStart(LearnTunerCodesAsync);
      Thread learnThread = new Thread(learnThreadStarter);
      learnThread.IsBackground = true;
      learnThread.Name = "USBUIRTLearner";
      learnThread.Start();
    }

    public void LearnTunerCodesAsync()
    {
      if (stbControlCodeCommands.Length == 0)
      {
        return;
      }

      bool result;
      skipLearnForCurrentCode = false;
      AbortLearn = false;
      isLearning = true;
      int retries = 3;
      int totCodeCount = stbControlCodeCommands.Length;
      string lastIrCodeLearned = string.Empty;

      for (int i = 0; i < stbControlCodeCommands.Length; i++)
      {
        int keyVal = stbControlCodeCommands[i];
        string btnName = (keyVal == 10 ? "Enter" : keyVal.ToString());

        if (skipLearnForCurrentCode)
        {
          skipLearnForCurrentCode = false;
          abort = 0;
        }

        if (abortLearn)
        {
          break;
        }

        for (int retry = 0; retry < retries*2; retry++)
        {
          NotifyStartLearn(btnName, totCodeCount, (capturingToggledIrCode ? i + 1 : i));
          result = IRLearn();

          if (abort == 1 || abortLearn || skipLearnForCurrentCode)
          {
            break;
          }

          else
          {
            string irCodeString = ircode.ToString();
            Console.WriteLine("Last Code Learned: " + lastIrCodeLearned);
            Console.WriteLine(" New Code Learned: " + irCodeString + "\r\n");

            // Certain code formats such as Philips RC5 and RC6 toggle a bit on consecutive key presses.  
            // To catch these we need to capture 2 seperate button presses for each button...
            if (capturingToggledIrCode && irCodeString.CompareTo(lastIrCodeLearned) != 0)
            {
              stbToggleCommandsLearned[keyVal] = irCodeString;
            }

            else
            {
              stbCommandsLearned[keyVal] = ircode.ToString();
            }

            lastIrCodeLearned = irCodeString;
          }

          NotifyEventLearned(btnName, ircode.ToString(), result, totCodeCount, i + 1);

          if (result && capturingToggledIrCode)
          {
            capturingToggledIrCode = false;
            break;
          }

          else
          {
            capturingToggledIrCode = true;
          }
        }
      }

      isLearning = false;
      NotifyTrainingComplete();
    }

    public void BulkLearn(object[] commands, string[] buttonNames)
    {
      BulkLearn(commands, buttonNames, false);
    }

    public void BulkLearn(object[] commands, string[] buttonNames, bool clearCommands)
    {
      if (clearCommands)
      {
        commandsLearned.Clear();
      }

      controlCodeCommands = commands;
      controlCodeButtonNames = buttonNames;
      capturingToggledIrCode = false;

      if (commands.Length != buttonNames.Length)
      {
        throw new Exception("invalid call to BulkLearn");
      }

      skipLearnForCurrentCode = false;
      AbortLearn = false;
      currentButtonIndex = 0;
      isLearning = true;
      waitingForIrRxLearnEvent = true;

      NotifyStartLearn(controlCodeButtonNames[currentButtonIndex], commands.Length, currentButtonIndex);
    }

    private void LearnNextCode()
    {
      // Certain code formats such as Philips RC5 and RC6 toggle a bit on consecutive key presses.  
      // To catch these we need to capture 2 seperate button presses for each button...
      capturingToggledIrCode = !capturingToggledIrCode;
      NotifyStartLearn(controlCodeButtonNames[capturingToggledIrCode ? currentButtonIndex : ++currentButtonIndex],
                       controlCodeCommands.Length, currentButtonIndex);

      waitingForIrRxLearnEvent = true;
      isLearning = true;
    }

    #endregion

    #region remote control methods

    public void ChangeTunerChannel(string channel, bool ignoreLastChannel)
    {
      if (ignoreLastChannel)
      {
        lastchannel = "";
      }

      ChangeTunerChannel(channel);
    }

    public void ChangeTunerChannel(string channel)
    {
      if (!isUsbUirtLoaded)
      {
        return;
      }

      if (!TransmitEnabled)
      {
        return;
      }

      Log.Info("USBUIRT: NewChannel={0} LastChannel={1}", channel, lastchannel);

      // Already tuned to this channel?
      if (channel == lastchannel)
      {
        return;
      }
      int length = channel.Length;

      // Some STB's allow more than 3 digit channel numbers!
      //if ((!this.Is3Digit && length >2) || (length >3))
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

        Transmit(irTxString, UUIRTDRV_IRFMT_UUIRT, commandRepeatCount);
      }

      if (NeedsEnter)
      {
        int codeIndex = 10;
        bool isToggledCode = false;
        string irTxString = GetSTBIrCode(codeIndex, ref isToggledCode);
        Log.Info("USBUIRT: send enter{0}", (isToggledCode ? " (toggled)" : ""));

        if (irTxString.Length == 0)
        {
          Log.Info("USBUIRT: IR Code for enter button is empty");
        }

        else
        {
          Transmit(irTxString, UUIRTDRV_IRFMT_UUIRT, commandRepeatCount);
        }
      }

      // All succeeded, remember last channel
      lastchannel = channel;
    }

    private string GetSTBIrCode(int codeIndex, ref bool isToggledCode)
    {
      string irTxString = "";
      string toggledIrTxString = "";
      string irOut;

      if (stbCommandsLearned.ContainsKey(codeIndex))
      {
        irTxString = stbCommandsLearned[codeIndex].ToString();
      }

      if (stbToggleCommandsLearned.ContainsKey(codeIndex))
      {
        toggledIrTxString = stbToggleCommandsLearned[codeIndex].ToString();
      }

      // is the code we're sending identical to the last one sent?
      // If so, check if there's a toggled version of the code...
      if (toggledIrTxString.Length > 0 && (lastIRCodeSent.CompareTo(irTxString) == 0))
      {
        isToggledCode = true;
        irOut = toggledIrTxString;
      }

      else
      {
        isToggledCode = false;
        irOut = irTxString;
      }

      lastIRCodeSent = irOut;
      return irOut;
    }

    public void Transmit(string gIRCode, int gIRCodeFormat, int repeatCount)
    {
      if (!isUsbUirtLoaded)
      {
        return;
      }
      if (!TransmitEnabled)
      {
        return;
      }

      bool result = UUIRTTransmitIR(UsbUirtHandle,
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
        Thread.Sleep(interCommandDelay);
      }
    }

    #endregion

    #region misc methods

    private void CreateJumpToCommands()
    {
      jumpToCommands[(int) JumpToActionType.JUMP_TO_HOME] = GUIWindow.Window.WINDOW_HOME;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_MY_TV] = GUIWindow.Window.WINDOW_TV;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_MY_TV_FULLSCREEN] = GUIWindow.Window.WINDOW_TVFULLSCREEN;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_MY_MOVIES] = GUIWindow.Window.WINDOW_VIDEOS;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_MY_MOVIES_FULLSCREEN] = GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_MY_MUSIC] = GUIWindow.Window.WINDOW_MUSIC_FILES;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_MY_PICTURES] = GUIWindow.Window.WINDOW_PICTURES;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_TV_GUIDE] = GUIWindow.Window.WINDOW_TVGUIDE;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_MY_RADIO] = GUIWindow.Window.WINDOW_RADIO;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_TELETEXT] = GUIWindow.Window.WINDOW_TELETEXT;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_TELETEXT_FULLSCREEN] = GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT;
      jumpToCommands[(int) JumpToActionType.JUMP_TO_MY_WEATHER] = GUIWindow.Window.WINDOW_WEATHER;
    }

    public bool GetCommandIrStrings(Action.ActionType actionType, ref string irCmd1, ref string irCmd2)
    {
      bool result;
      bool irCmd1Found = false;
      bool irCmd2Found = false;

      foreach (object entry in commandsLearned.Keys)
      {
        string irCode = entry.ToString();
        object command = commandsLearned[irCode];

        if ((Action.ActionType) command == actionType)
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

      result = irCmd1Found || irCmd2Found;
      return result;
    }

    public bool ClearLearnedCommand(object command)
    {
      bool result = false;

      // no point interating through the hashtable if it doesn't 
      // contain this ActionType
      if (!commandsLearned.ContainsValue(command))
      {
        return false;
      }

      // Can't remove items while enumerating a Hashtable so we do it this way...
      ArrayList commandsArr = new ArrayList(commandsLearned);
      commandsArr.Sort(this);

      for (int i = 0; i < commandsArr.Count; i++)
      {
        // Key:		IR Code String
        // Value:	Action.ActionType
        DictionaryEntry entry = (DictionaryEntry) commandsArr[i];

        if ((int) entry.Value == (int) command)
        {
          commandsLearned.Remove(entry.Key);
          result = true;
        }
      }

      if (result)
      {
        SaveInternalValues();
      }

      return result;
    }

    public bool ClearAllLearnedCommands()
    {
      int entryCount = commandsLearned.Count;
      commandsLearned.Clear();

      bool result = entryCount != commandsLearned.Count;

      if (result)
      {
        SaveInternalValues();
      }

      return result;
    }

    public bool ClearAllLearnedSTBCommands()
    {
      // is there anything to clear?
      if (stbCommandsLearned.Count + stbToggleCommandsLearned.Count == 0)
      {
        return false;
      }

      stbCommandsLearned.Clear();
      stbToggleCommandsLearned.Clear();

      bool result = stbCommandsLearned.Count == 0 && stbToggleCommandsLearned.Count == 0;

      if (result)
      {
        SaveTunerValues();
      }

      return result;
    }

    public bool ClearLearnedSTBCommand(int command)
    {
      bool result = false;

      // no point interating through the hashtables if they don't
      // contain this command
      if (!stbCommandsLearned.ContainsKey(command) && !stbToggleCommandsLearned.ContainsKey(command))
      {
        return false;
      }

      // Key:		STB button number (10 == Enter)
      // Value:	txIRString

      if (stbCommandsLearned.ContainsKey(command))
      {
        stbCommandsLearned.Remove(command);
        result = true;
      }

      if (stbToggleCommandsLearned.ContainsKey(command))
      {
        stbToggleCommandsLearned.Remove(command);
        result = true;
      }

      if (result)
      {
        SaveTunerValues();
      }

      return result;
    }

    public bool GetSTBCommandIrStrings(int cmdNumber, ref string irCmd1, ref string irCmd2)
    {
      bool result = false;

      if (stbCommandsLearned.ContainsKey(cmdNumber))
      {
        result = true;
        irCmd1 = (string) stbCommandsLearned[cmdNumber];
      }

      if (stbToggleCommandsLearned.ContainsKey(cmdNumber))
      {
        result = true;
        irCmd2 = (string) stbToggleCommandsLearned[cmdNumber];
      }

      return result;
    }

    private void ThreadSafeSendMessage(int wmMsg, int wparam, int lparam)
    {
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        ThreadSafeSendMessageDelegate d = new ThreadSafeSendMessageDelegate(ThreadSafeSendMessage);
        GUIGraphicsContext.form.Invoke(d, new object[] {wmMsg, wparam, lparam});
      }

      else
      {
        PostMessage(GUIGraphicsContext.form.Handle, wmMsg, wparam, lparam);
      }
    }

    #endregion

    #region IComparer Members

    public int Compare(object x, object y)
    {
      // Key is the IR Code String
      // Value is the Action.ActionType

      DictionaryEntry dictX = (DictionaryEntry) x;
      DictionaryEntry dictY = (DictionaryEntry) y;

      int actionValX = (int) dictX.Value;
      int actionValY = (int) dictY.Value;

      return actionValX.CompareTo(actionValY);
    }

    #endregion
  }
}