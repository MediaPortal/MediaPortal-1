/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Microsoft.DirectX.DirectInput;

namespace MediaPortal
{
  /// <summary>
  /// Summary description for DirectInputHandler.
  /// </summary>
  public class DirectInputHandler
  {
    [DllImport("winmm.dll")]
    public static extern int timeGetTime();

    DirectInputListener diListener = null;
    DeviceList deviceList = null;
    string selectedDeviceGUID = "";
    HCWHandler diMapper = null;

    ArrayList deviceNames = new ArrayList();
    ArrayList deviceGUIDs = new ArrayList();
    bool active = false;
    bool doSendActions = true;
    int delay = 150; // delay in milliseconds, used to filter events
    string buttonComboKill = "2,3";
    string buttonComboClose = "1,3";

    int lastCodeSent = -1;
    int lastAxisValue = 0;
    int timeLastSend = 0;
    int axisLimit = 4200;
    Process lastProc = null;


    enum joyButton
    {
      axisXUp = 3000,
      axisXDown = 3001,
      axisYUp = 3002,
      axisYDown = 3003,
      axisZUp = 3004,
      axisZDown = 3005,

      rotationXUp = 3010,
      rotationXDown = 3011,
      rotationYUp = 3012,
      rotationYDown = 3013,
      rotationZUp = 3014,
      rotationZDown = 3015,

      povN = 3020,
      povNE = 3021,
      povE = 3022,
      povSE = 3023,
      povS = 3024,
      povSW = 3025,
      povW = 3026,
      povNW = 3027,

      button1 = 3030,
      button2 = 3031,
      button3 = 3032,
      button4 = 3033,
      button5 = 3034,
      button6 = 3035,
      button7 = 3036,
      button8 = 3037,
      button9 = 3038,
      button10 = 3039,
      button11 = 3040,
      button12 = 3041,
      button13 = 3042,
      button14 = 3043,
      button15 = 3044,
      button16 = 3045,
      button17 = 3046,
      button18 = 3047,
      button19 = 3048,
      button20 = 3049,

      comboKillProcess = 4000,
      comboCloseProcess = 4001
    }

    // event: debug info on joystick state change 
    public delegate void diStateChangeText(object sender, string stateText);

    public event diStateChangeText OnStateChangeText = null;

    public DirectInputHandler()
    {
      //
      // TODO: Add constructor logic here
      //
      CreateListener();
      CreateMapper();
      InitDeviceList();
      LoadSettings();
      AttachHandlers();
    }

    ~DirectInputHandler()
    {
      DetachHandlers();
      FreeListener();
    }

    public void InitDeviceList()
    {
      try
      {
        deviceList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
        deviceNames.Clear();
        deviceGUIDs.Clear();
        if (null == deviceList) return;
        deviceList.Reset();
        foreach (DeviceInstance di in deviceList)
        {
          if (Manager.GetDeviceAttached(di.InstanceGuid))
          {
            deviceNames.Add(di.InstanceName);
            deviceGUIDs.Add(di.InstanceGuid);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write("DirectInputHandler: error in InitDeviceList");
        Log.Write(ex.Message.ToString());
      }
    }

    public bool Active
    {
      get { return active; }
      set { SetActive(value); }
    }

    public bool DoSendActions
    {
      get { return doSendActions; }
      set { doSendActions = value; }
    }

    public int Delay
    {
      get { return delay; }
      set { delay = value; }
    }

    public string ButtonComboKill
    {
      get { return buttonComboKill;}
      set { buttonComboKill = value;}
    }

    public string ButtonComboClose
    {
      get { return buttonComboClose;}
      set { buttonComboClose = value;}
    }

    public string GetCurrentButtonCombo()
    {
      string res = "";
      if (null != diListener)
      {
        res = diListener.GetCurrentButtonCombo();
      }
      return res;
    }

    void SetActive(bool value)
    {
      active = value;
      if (active)
      {
        Start();
      }
      else
      {
        Stop();
      }
    }

    int GetSelectedDeviceIndex()
    {
      if (null == deviceList) return -1;
      int res = -1;
      int i = 0;
      if ((null != diListener.SelectedDevice) && (selectedDeviceGUID != ""))
      {
        deviceList.Reset();
        foreach (DeviceInstance di in deviceList)
        {
          if (di.InstanceGuid.ToString() == selectedDeviceGUID)
          {
            res = i;
            break;
          }
          i++;
        }
      }
      return res;
    }

    public void Start()
    {
      // nop
    }

    public void Stop()
    {
      diListener.StopListener();
    }

    public string SelectedDeviceGUID
    {
      get { return selectedDeviceGUID; }
    }

    public int SelectedDeviceIndex
    {
      get { return GetSelectedDeviceIndex(); }
    }

    public int DeviceCount
    {
      get
      {
        if (null == deviceList)
        {
          return 0;
        }
        else
        {
          return deviceList.Count;
        }
      }
    }

    public ArrayList DeviceNames
    {
      get { return deviceNames; }
    }

    public ArrayList DeviceGUIDs
    {
      get { return deviceGUIDs; }
    }


    void UnacquireDevice()
    {
      diListener.DeInitDevice();
      selectedDeviceGUID = "";
    }

    bool AcquireDevice(string devGUID)
    {
      bool res = false;
      if (null == deviceList) return false;
      deviceList.Reset();
      foreach (DeviceInstance di in deviceList)
      {
        if (di.InstanceGuid.ToString() == devGUID)
        {
          selectedDeviceGUID = devGUID;
          // create and init device
          res = diListener.InitDevice(di.InstanceGuid);
        }
      }
      return res;
    }

    public bool SelectDevice(string devGUID)
    {
      bool res = false;
      UnacquireDevice();
      res = AcquireDevice(devGUID);
      return res;
    }


    void SendActions(JoystickState state)
    {
      int actionCode = -1;
      int actionParam = -1;
      int curAxisValue = 0;
      // todo: timer stuff!!

      // buttons first!
      byte[] buttons = state.GetButtons();
      int button = 0;
      string pressedButtons = "";

      // button combos
      string sep = "";
      foreach (byte b in buttons)
      {
        if (0 != (b & 0x80))
        {
          pressedButtons += sep + button.ToString("00");
          sep = ",";
        }
        button++;
      }

      if ((ButtonComboKill != "") && (ButtonComboKill == pressedButtons))
      {
        if (null != lastProc)
        {
          actionCode = (int) joyButton.comboKillProcess;
          actionParam = lastProc.Id;
        }
      }
      else if ((ButtonComboClose != "") && (ButtonComboClose == pressedButtons))
      {
        if (null != lastProc)
        {
          actionCode = (int) joyButton.comboCloseProcess;
          actionParam = lastProc.Id;
        }
      }

      // single buttons
      if (actionCode == -1)
      {
        button = 0;
        bool foundButton = false;
        foreach (byte b in buttons)
        {
          if (0 != (b & 0x80))
          {
            foundButton = true;
            break;
          }
          button++;
        }
        if (foundButton)
        {
          if ((button >= 0) && (button <= 19))
          {
            // don't need no stinkin' enum-constants here....
            actionCode = 3030 + button;
          }
        }
      }

      // pov next
      if (actionCode == -1)
      {
        int[] pov = state.GetPointOfView();
        switch (pov[0])
        {
          case 0:
            {
              actionCode = (int) joyButton.povN;
              break;
            }
          case 9000:
            {
              actionCode = (int) joyButton.povE;
              break;
            }
          case 18000:
            {
              actionCode = (int) joyButton.povS;
              break;
            }
          case 27000:
            {
              actionCode = (int) joyButton.povW;
              break;
            }
        }
      }

      if (actionCode == -1)
      {
        // axes next
        if (Math.Abs(state.X) > axisLimit)
        {
          curAxisValue = state.X;
          if (state.X > 0)
          {
            actionCode = (int) joyButton.axisXUp; // right
          }
          else
          {
            actionCode = (int) joyButton.axisXDown; // left
          }
        }
        else if (Math.Abs(state.Y) > axisLimit)
        {
          curAxisValue = state.Y;
          if (state.Y > 0)
          {
            // down
            actionCode = (int) joyButton.axisYUp;
          }
          else
          {
            // up
            actionCode = (int) joyButton.axisYDown;
          }
        }
        else if (Math.Abs(state.Z) > axisLimit)
        {
          curAxisValue = state.Z;
          if (state.Z > 0)
          {
            actionCode = (int) joyButton.axisZUp;
          }
          else
          {
            actionCode = (int) joyButton.axisZDown;
          }
        }
      }

      if (actionCode == -1)
      {
        // rotation
        if (Math.Abs(state.Rx) > axisLimit)
        {
          curAxisValue = state.Rx;
          if (state.Rx > 0)
          {
            actionCode = (int) joyButton.rotationXUp;
          }
          else
          {
            actionCode = (int) joyButton.rotationXDown;
          }
        }
        else if (Math.Abs(state.Ry) > axisLimit)
        {
          curAxisValue = state.Ry;
          if (state.Ry > 0)
          {
            actionCode = (int) joyButton.rotationYUp;
          }
          else
          {
            actionCode = (int) joyButton.rotationYDown;
          }
        }
        else if (Math.Abs(state.Rz) > axisLimit)
        {
          curAxisValue = state.Rz;
          if (state.Rz > 0)
          {
            actionCode = (int) joyButton.rotationZUp;
          }
          else
          {
            actionCode = (int) joyButton.rotationZDown;
          }
        }
      }

//      if ((actionCode > 0) && (!FilterAction(actionCode, curAxisValue)))
      if (VerifyAction(actionCode, curAxisValue))
      {
        Log.Write("mapping action {0}", actionCode);
        diMapper.MapAction(actionCode, actionParam);
      }
    }

    bool VerifyAction(int actionCode, int curAxisValue)
    {
      Log.Write(" dw 1 ");
      bool res = false;
      if (diListener.IsRunning)
      {
        Log.Write(" dw 2 ");
        res = (actionCode > 0) && (actionCode < 4000) && (!FilterAction(actionCode, curAxisValue));
      }
      else
      {
        res = (actionCode >= 4000) && (actionCode < 5000);
  /*
 *         Log.Write(" dw 3 ");
        // test:
        if ((null != lastProc) && (actionCode == 4000))
        {
          Log.Write(" ready to kill ");
          //          if (!lastProc.CloseMainWindow())
          lastProc.Kill();
          lastProc = null;
        }
*/        
      }
      return res;
    }

    string GetStateAsText(JoystickState state)
    {
      string joyState = "Using JoystickState: \r\n";
      string strText = null;

      joyState += string.Format("axis: {0}x{1}x{2}\r\n", state.X, state.Y, state.Z);
      joyState += string.Format("rotation: {0}x{1}x{2}\r\n", state.Rx, state.Ry, state.Rz);

      joyState += "\r\n";

      int[] slider = state.GetSlider();
      joyState += string.Format("slider: 0: {0} 1: {1}\r\n", slider[0], slider[1]);

      int[] pov = state.GetPointOfView();
      switch (pov[0])
      {
        case 0:
          {
            joyState += string.Format("pov: North\r\n");
            break;
          }
        case 4500:
          {
            joyState += string.Format("pov: NorthEast\r\n");
            break;
          }
        case 9000:
          {
            joyState += string.Format("pov: East\r\n");
            break;
          }
        case 13500:
          {
            joyState += string.Format("pov: SouthEast\r\n");
            break;
          }
        case 18000:
          {
            joyState += string.Format("pov: South\r\n");
            break;
          }
        case 22500:
          {
            joyState += string.Format("pov: SouthWest\r\n");
            break;
          }
        case 27000:
          {
            joyState += string.Format("pov: West\r\n");
            break;
          }
        case 31500:
          {
            joyState += string.Format("pov: NorthWest\r\n");
            break;
          }
        default:
          {
            joyState += string.Format("pov: \r\n");
            break;
          }
      }
      joyState += "\r\n";

      // Fill up text with which buttons are pressed
      byte[] buttons = state.GetButtons();

      int button = 0;
      foreach (byte b in buttons)
      {
        if (0 != (b & 0x80))
          strText += button.ToString("00 ");
        button++;
      }
      joyState += "Buttons: " + strText;
      return joyState;
    }

    public void RunControlPanel()
    {
      if (null != diListener.SelectedDevice)
      {
        diListener.SelectedDevice.RunControlPanel();
      }
    }


    public void LoadSettings()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        active = xmlreader.GetValueAsBool("remote", "DirectInput", false);
        string strGUID = xmlreader.GetValueAsString("remote", "DirectInputDeviceGUID", "");
        if (active && ("" != strGUID))
        {
          SelectDevice(strGUID);
        }
        delay = xmlreader.GetValueAsInt("remote", "DirectInputDelayMS", 150);
        buttonComboKill = xmlreader.GetValueAsString("remote", "DirectInputKillCombo", ""); 
        buttonComboClose = xmlreader.GetValueAsString("remote", "DirectInputCloseCombo", ""); 
      }
    }

    public void SaveSettings()
    {
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("remote", "DirectInput", active);
        xmlwriter.SetValue("remote", "DirectInputDeviceGUID", SelectedDeviceGUID);
        xmlwriter.SetValue("remote", "DirectInputDelayMS", delay);
        xmlwriter.SetValue("remote", "DirectInputKillCombo", buttonComboKill);
        xmlwriter.SetValue("remote", "DirectInputCloseCombo", buttonComboClose);
      }
    }

    void CreateListener()
    {
      diListener = new DirectInputListener();
      diListener.OnStateChange += new MediaPortal.DirectInputListener.diStateChange(diListener_OnStateChange);
    }

    void FreeListener()
    {
      diListener.OnStateChange -= new MediaPortal.DirectInputListener.diStateChange(diListener_OnStateChange);
      diListener = null;
    }

    void diListener_OnStateChange(object sender, JoystickState state)
    {
      if (null != this.OnStateChangeText)
      {
        string stateText = GetStateAsText(state);
        OnStateChangeText(this, stateText);
      }
      if (DoSendActions)
      {
        SendActions(state);
      }
    }

    bool FilterAction(int actionCode, int axisValue)
    {
      bool res = false;
      // filter actionCodes only if
      // 1) the last code that was sent is the one we'd like to re-send now
      // AND 
      // 2) the time elapsed when sending the same code is smaller than the delay threshold
      if (actionCode == lastCodeSent)
      {
        int timeNow = timeGetTime();
        int timeElapsed = timeNow - timeLastSend;
        if (timeElapsed < delay)
        {
          res = true;
        }
        else if (Math.Abs(axisValue) < Math.Abs(lastAxisValue))
        {
          // axis is being released => don't send action!
          res = true;
        }
      }
      if (!res)
      {
        lastCodeSent = actionCode;
        timeLastSend = timeGetTime();
        lastAxisValue = axisValue;
      }
      return res;
    }


    void CreateMapper()
    {
      bool result;
      diMapper = new HCWHandler("DirectInput", out result);
    }

    void AttachHandlers()
    {
      Utils.OnStartExternal += new Utils.UtilEventHandler(OnStartExternal);
      Utils.OnStopExternal += new Utils.UtilEventHandler(OnStopExternal);
    }

    void DetachHandlers()
    {
      Utils.OnStartExternal -= new Utils.UtilEventHandler(OnStartExternal);
      Utils.OnStopExternal -= new Utils.UtilEventHandler(OnStopExternal);
    }

    public void OnStartExternal(Process proc, bool waitForExit)
    {
      if (active && waitForExit)
      {
        lastProc = proc;
        diListener.Pause();
      }
    }

    public void OnStopExternal(Process proc, bool waitForExit)
    {
      if (active)
      {
        lastProc = null;
        diListener.Resume();
      }
    }


  }
}