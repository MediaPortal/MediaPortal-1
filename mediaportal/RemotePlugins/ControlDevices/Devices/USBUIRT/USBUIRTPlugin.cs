#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.ControlDevices;

namespace MediaPortal.ControlDevices.USBUIRT
{
  /// <summary>
  /// This class will handle all communication with an external USBUIRT device
  /// The USB-UIRT, allows your PC to both Receive and Transmit infrared signals -- 
  /// exactly like those used by the collection of remote controls you've acquired for your TV, 
  /// VCR, Audio System, etc. 
  /// See www.usbuirt.com for more details on USBUIRT
  /// </summary>
  public class LearningEventArgs : System.EventArgs
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
      this.Button = button;
      this.IrCode = ircode;
      this.Succeeded = succeeded;
      this.IsToggledIrCode = capturingToggledIrCode;
      this.TotalCodeCount = totalCodeCount;
      this.CurrentCodeCount = curCodeCount;
    }

    public LearningEventArgs(string button, bool capturingToggledIrCode,
      int totalCodeCount, int curCodeCount)
    {
      this.Button = button;
      this.IsToggledIrCode = capturingToggledIrCode;
      this.TotalCodeCount = totalCodeCount;
      this.CurrentCodeCount = curCodeCount;
    }
  }


  public partial class USBUIRTPlugin : AbstractControlPlugin, IDisposable, IControlPlugin
  {
    public string DeviceName { get { return "USB-UIRT"; } }
    public Uri VendorUri { get { return new Uri("http://www.usbuirt.com/"); } }
    public IControlInput InputInterface { get { return null; } }
    public IControlOutput OutputInterface { get { return null; } }
    public bool DriverInstalled { get { return false; } }
    public string DriverVersion
    {
      get
      {/*
        if (this.Connected)
        {
          USBUIRTAPI.UUINFO p = new USBUIRTAPI.UUINFO();
          USBUIRTAPI.UUIRTGetUUIRTInfo(UsbUirtHandle, ref p);

          DateTime firmdate = new DateTime(p.fwDateYear + 2000, p.fwDateMonth, p.fwDateDay);
          DateTime plugdate = new DateTime(2004, 4, 1);

          string firmversion = (p.fwVersion >> 8) + "." + (p.fwVersion & 0xff);
          //string plug = string.Format("Plugin Version: {0}", USBUIRT_PLUGINVER);
          //string firm = string.Format("Firmware Version: {0} ({1})", firmversion, firmdate.ToString("MMMM, dd, yyyy"));
          //return string.Format("{0}\r\n{1}", plug, firm);

          return string.Format("Firmware Version: {0} ({1})", firmversion, firmdate.ToString("MMMM, dd, yyyy"));
        }

        else
        {
*/          return "USBUIRT device not detected!";
  //      }
      }
    }

    public string DeviceDescription { get { return "lalala"; } }
    public string DevicePrefix { get { return _settings.Prefix; } }

    public bool HardwareInstalled { get { return true; } }
    public string HardwareVersion { get { return "UDddg "; } }

    public bool Capability(EControlCapabilities capability)
    {
      switch (capability)
      {
        case EControlCapabilities.CAP_INPUT:
          return true;
        case EControlCapabilities.CAP_OUTPUT:
          return true;
        case EControlCapabilities.CAP_VERBOSELOG:
          return false;
        default:
          return false;
      }
    }

    public IControlSettings Settings { get { return _settings; } }


    public string PluginName()
    {
      return "USB-UIRT";
    }

    public string Description()
    {
      return "Enables USB-UIRT input and output in MediaPortal";
    }

    public string Author()
    {
      return "Trax";
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    private IntPtr _uirtHandle = IntPtr.Zero;

    public USBUIRTPlugin()
    {
      _settings = new USBUIRTSettings(this);
    }

    ~USBUIRTPlugin()
    {
  //    Dispose(false);
    }


    public void Dispose()
    {
//      Dispose(true);
      GC.SuppressFinalize(this);
    }


    protected bool DiscoverDrivers()
    {
      return false;
    }

    /// <summary>
    /// Opens the USB-UIRT driver
    /// </summary>
    /// <returns>true when successfully loaded</returns>
    protected bool LoadDrivers()
    {
      try 
      {
        _uirtHandle = USBUIRTAPI.UUIRTOpen();

        if (_uirtHandle != USBUIRTAPI.UUPTR_EMPTY)
        {
          _log.Info("USBUIRT:Open success:{0}", DriverVersion);
        }
        else
        {
          _log.Error("USBUIRT:Unable to open USBUIRT driver");
          _uirtHandle = USBUIRTAPI.UUPTR_NULL;
          return false;
        }

        //setup callack to receive IR messages
        //urcb = new USBUIRTAPI.UUIRTReceiveCallbackDelegate(this.USBUIRTReceiveCallback);
        //USBUIRTAPI.UUIRTSetReceiveCallback(UsbUirtHandle, urcb, 0);
        //RemoteCommandCallback = callback;
      }

      catch (System.DllNotFoundException e)
      {
        _log.Error("USBUIRT: Error loading driver uuirtdrv.dll : ",e.Message);
        _log.Error(e);
      }

      catch (Exception e)
      {
        _log.Error("USBUIRT: Error opening driver uuirtdrv.dll : ", e.Message);
        _log.Error(e);
      }

      return false;
    }

    protected bool DiscoverDevices()
    {
      return false;
    }

    protected bool ConnectDevice()
    {
      //setup callack to receive IR messages
      //urcb = new USBUIRTAPI.UUIRTReceiveCallbackDelegate(this.USBUIRTReceiveCallback);
      //USBUIRTAPI.UUIRTSetReceiveCallback(UsbUirtHandle, urcb, 0);
      //RemoteCommandCallback = callback;
      return false;
    }

    protected void DisconnectDevice()
    {
    }

    protected void UnloadDrivers()
    {
      if (_uirtHandle != USBUIRTAPI.UUPTR_EMPTY && _uirtHandle != USBUIRTAPI.UUPTR_NULL)
      {
        USBUIRTAPI.UUIRTClose(_uirtHandle);
        _uirtHandle = USBUIRTAPI.UUPTR_NULL;
      }
    }


    /// <summary>
    /// Transmit a single IR command
    /// </summary>
    /// <param name="irCode"></param>
    /// <param name="codeFormat"></param>
    /// <param name="repeatCount"></param>
    public void Transmit(string irCode, int codeFormat, int repeatCount)
    {
      bool result = USBUIRTAPI.UUIRTTransmitIR(_uirtHandle,
        irCode,		        // IRCode 
        codeFormat,	      // codeFormat 
        repeatCount,	    // repeatCount 
        0,				        // inactivityWaitTime 
        IntPtr.Zero,	    // hEvent 
        0,				        // reserved1
        0				          // reserved2 
        );

      if (!result)
      {
        _log.Error("USBUIRT: Unable to transmit code");
      }
      // System.Threading.Thread.Sleep(interCommandDelay);
    }



/*
    private void Dispose(bool disposeManagedResources)
    {
      if (!this.disposed)
      {
        disposed = true;

        if (disposeManagedResources)
        {
          // Dispose any managed resources.
        }

        IntPtr emptyPtr = new IntPtr(-1);

        if (UsbUirtHandle != emptyPtr && UsbUirtHandle != IntPtr.Zero)
        {
          USBUIRTAPI.UUIRTClose(UsbUirtHandle);
          _uirtHandle = IntPtr.Zero;
        }
      }
    }
    */




    /*

    static int UUIRTDRV_IRFMT_UUIRT = 0x0000;
    private const string remotefile = "UIRTUSB-remote.xml";
    private const string tunerfile = "UIRTUSB-tuner.xml";

    private IntPtr UsbUirtHandle = IntPtr.Zero;
    private StringBuilder ircode = new StringBuilder("1", 2048);
    private int abort = 0;
    private int timelaps = 300; // time in milliseconds between two accepted commands
    private IntPtr empty = new IntPtr(-1);
    private USBUIRTAPI.OnRemoteCommand remoteCommandCallback = null;
    private USBUIRTAPI.UUIRTReceiveCallbackDelegate urcb = null;
    private static USBUIRT instance = null;
    private Hashtable commandsLearned = new Hashtable();
    private Hashtable stbCommandsLearned = new Hashtable();
    private Hashtable stbToggleCommandsLearned = new Hashtable();
    private DateTime timestamp = DateTime.Now;
    private bool isLearning = false;

    private int currentButtonIndex = 0;
    private bool waitingForIrRxLearnEvent = false;
    private bool capturingToggledIrCode = false;
    private bool abortLearn = false;
    private bool skipLearnForCurrentCode = false;
    private bool disposed = false;

    private int commandRepeatCount = 1;
    private int interCommandDelay = 100;
    private string lastIRCodeSent = string.Empty;

    public event USBUIRTAPI.StartLearningEventHandler StartLearning;
    public event USBUIRTAPI.EventLearnedHandler OnEventLearned;
    public event USBUIRTAPI.EndLearnedHandler OnEndLearning;

    public event USBUIRTAPI.RemoteCommandFeedbackHandler OnRemoteCommandFeedback;

    public static USBUIRT Instance
    {
      get
      {
        return instance;
      }
    }

    public int TimeLaps
    {
      set
      {
        timelaps = value;
      }
      get
      {
        return timelaps;
      }
    }

    public bool AbortLearn
    {
      set
      {
        abortLearn = value;
        this.abort = abortLearn ? 1 : 0;

        if (abortLearn)
        {
          if (isLearning && waitingForIrRxLearnEvent)
          {
            isLearning = false;
            waitingForIrRxLearnEvent = false;
            capturingToggledIrCode = false;
            this.NotifyTrainingComplete();
          }

          else
          {
            isLearning = false;
            waitingForIrRxLearnEvent = false;
            capturingToggledIrCode = false;
          }
        }
      }
      get
      {
        return abortLearn;
      }
    }


    public int CommandRepeatCount
    {
      get
      {
        return commandRepeatCount;
      }
      set
      {
        commandRepeatCount = value;
      }
    }

    public int InterCommandDelay
    {
      get
      {
        return interCommandDelay;
      }
      set
      {
        interCommandDelay = value;
      }
    }


    public bool Connected
    {
      get
      {
        if (UsbUirtHandle == IntPtr.Zero || UsbUirtHandle == empty)
          return false;

        uint puConfig = uint.MaxValue;

        try
        {
          return USBUIRTAPI.UUIRTGetUUIRTConfig(UsbUirtHandle, ref puConfig);
        }

        catch (Exception)
        {
          return false;
        }
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


    ~USBUIRT()
    {
      Dispose(false);
    }


    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposeManagedResources)
    {
      if (!this.disposed)
      {
        disposed = true;

        if (disposeManagedResources)
        {
          // Dispose any managed resources.
        }

        IntPtr emptyPtr = new IntPtr(-1);

        if (UsbUirtHandle != emptyPtr && UsbUirtHandle != IntPtr.Zero)
        {
          USBUIRTAPI.UUIRTClose(UsbUirtHandle);
          UsbUirtHandle = IntPtr.Zero;
        }
      }
    }

    #endregion


    #region remote receiver methods
    public USBUIRTAPI.OnRemoteCommand RemoteCommandCallback
    {
      set
      {
        remoteCommandCallback = value;
      }
    }

    public void USBUIRTReceiveCallback(string irid, IntPtr reserved)
    {
      Debug.Assert(true == _settings.Enabled);
      Debug.Assert(true == _settings.EnableInput);

      object command = commandsLearned[irid];

      if (command == null && !isLearning)
      {
        if (OnRemoteCommandFeedback != null)
          OnRemoteCommandFeedback(Action.ActionType.ACTION_INVALID, irid);

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

        //  commandsLearned[irid] = controlCodeCommands[currentButtonIndex];
      //    int totCodeCount = controlCodeButtonNames.Length;
          int curCodeIndex = currentButtonIndex + 1;

          waitingForIrRxLearnEvent = false;
          NotifyEventLearned(controlCodeButtonNames[currentButtonIndex], irid, true, totCodeCount, curCodeIndex);

          if (currentButtonIndex < controlCodeButtonNames.Length - 1 ||
            currentButtonIndex < controlCodeButtonNames.Length && !capturingToggledIrCode)
          {
            this.LearnNextCode();
          }

          else
          {
            isLearning = false;
            NotifyTrainingComplete();
          }
        }

        // removed 

        timestamp = DateTime.Now;
      }
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

    protected void NotifyEventLearned(string button, string ircode, bool isSuccess, int totCodeCount, int curCodeIndex)
    {
      if (OnEventLearned != null)
      {
        OnEventLearned(this, new LearningEventArgs(button, ircode, isSuccess,
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
        if (!USBUIRTAPI.UUIRTLearnIR(UsbUirtHandle, UUIRTDRV_IRFMT_UUIRT, this.ircode, null, 0, ref this.abort, 0, null, null))
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

      System.Threading.ThreadStart learnThreadStarter = new ThreadStart(LearnTunerCodesAsync);
      System.Threading.Thread learnThread = new System.Threading.Thread(learnThreadStarter);
      learnThread.Start();
    }

    public void LearnTunerCodesAsync()
    {
      if (stbControlCodeCommands.Length == 0)
        return;

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
          break;

        for (int retry = 0; retry < retries * 2; retry++)
        {
          NotifyStartLearn(btnName, totCodeCount, (capturingToggledIrCode ? i + 1 : i));
          result = IRLearn();

          if (abort == 1 || abortLearn || skipLearnForCurrentCode)
            break;

          else
          {
            string irCodeString = this.ircode.ToString();
            Console.WriteLine("Last Code Learned: " + lastIrCodeLearned);
            Console.WriteLine(" New Code Learned: " + irCodeString + "\r\n");

            // Certain code formats such as Philips RC5 and RC6 toggle a bit on consecutive key presses.  
            // To catch these we need to capture 2 seperate button presses for each button...
            if (capturingToggledIrCode && irCodeString.CompareTo(lastIrCodeLearned) != 0)
              this.stbToggleCommandsLearned[keyVal] = irCodeString;

            else
              stbCommandsLearned[keyVal] = this.ircode.ToString();

            lastIrCodeLearned = irCodeString;
          }

          NotifyEventLearned(btnName, this.ircode.ToString(), result, totCodeCount, i + 1);

          if (result && capturingToggledIrCode)
          {
            capturingToggledIrCode = false;
            break;
          }

          else
            capturingToggledIrCode = true;
        }
      }

      isLearning = false;
      NotifyTrainingComplete();
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

    private string GetSTBIrCode(int codeIndex, ref bool isToggledCode)
    {
      string irTxString = "";
      string toggledIrTxString = "";
      string irOut = "";

      if (stbCommandsLearned.ContainsKey(codeIndex))
        irTxString = stbCommandsLearned[codeIndex].ToString();

      if (stbToggleCommandsLearned.ContainsKey(codeIndex))
        toggledIrTxString = stbToggleCommandsLearned[codeIndex].ToString();

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

    #endregion

   */ 
  }
}