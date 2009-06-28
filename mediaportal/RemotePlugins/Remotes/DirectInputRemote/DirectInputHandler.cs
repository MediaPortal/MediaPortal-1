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
using System.Diagnostics;
using System.Runtime.InteropServices;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.DirectX.DirectInput;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Summary description for DirectInputHandler.
  /// </summary>
  public class DirectInputHandler
  {
    [DllImport("winmm.dll")]
    public static extern int timeGetTime();

    private DirectInputListener _diListener = null;
    private DeviceList _deviceList = null;
    private string _selectedDeviceGUID = string.Empty;
    private InputHandler _inputHandler = null;

    private ArrayList _deviceNames = new ArrayList();
    private ArrayList _deviceGUIDs = new ArrayList();
    private bool _active = false;
    private bool _doSendActions = true;
    private int _delay = 150; // delay in milliseconds, used to filter events
    private string _buttonComboKill = "2,3";
    private string _buttonComboClose = "1,3";

    private int _lastCodeSent = -1;
    private int _lastAxisValue = 0;
    private int _timeLastSend = 0;
    private int _axisLimit = 4200;
    private Process _lastProc = null;


    private enum joyButton
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
    }

    ~DirectInputHandler()
    {
      DetachHandlers();
      FreeListener();
    }

    public void Init()
    {
      bool _controlEnabled = false;
      using (Settings xmlreader = new MPSettings())
      {
        _controlEnabled = xmlreader.GetValueAsBool("remote", "DirectInput", false);
      }
      if (_controlEnabled)
      {
        CreateMapper();
        if (_inputHandler.IsLoaded)
        {
          CreateListener();
          InitDeviceList();
          LoadSettings();
          AttachHandlers();
        }
        else
        {
          Log.Info("DirectInput: Error loading default mapping file - please reinstall MediaPortal");
        }
      }
      else
      {
        Log.Info("DirectInput: not enabled");
      }
    }

    public void InitDeviceList()
    {
      try
      {
        _deviceList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
        _deviceNames.Clear();
        _deviceGUIDs.Clear();
        if (null == _deviceList)
        {
          return;
        }
        _deviceList.Reset();
        foreach (DeviceInstance di in _deviceList)
        {
          if (Manager.GetDeviceAttached(di.InstanceGuid))
          {
            _deviceNames.Add(di.InstanceName);
            _deviceGUIDs.Add(di.InstanceGuid);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("DirectInputHandler: error in InitDeviceList");
        Log.Info(ex.Message.ToString());
      }
    }

    public bool Active
    {
      get { return _active; }
      set { SetActive(value); }
    }

    public bool DoSendActions
    {
      get { return _doSendActions; }
      set { _doSendActions = value; }
    }

    public int Delay
    {
      get { return _delay; }
      set { _delay = value; }
    }

    public string ButtonComboKill
    {
      get { return _buttonComboKill; }
      set { _buttonComboKill = value; }
    }

    public string ButtonComboClose
    {
      get { return _buttonComboClose; }
      set { _buttonComboClose = value; }
    }

    public string GetCurrentButtonCombo()
    {
      string res = "";
      if (null != _diListener)
      {
        res = _diListener.GetCurrentButtonCombo();
      }
      return res;
    }

    private void SetActive(bool value)
    {
      _active = value;
      if (_active)
      {
        Start();
      }
      else
      {
        Stop();
      }
    }

    private int GetSelectedDeviceIndex()
    {
      if (null == _deviceList)
      {
        return -1;
      }
      int res = -1;
      int i = 0;
      if ((null != _diListener.SelectedDevice) && (_selectedDeviceGUID != string.Empty))
      {
        _deviceList.Reset();
        foreach (DeviceInstance di in _deviceList)
        {
          if (di.InstanceGuid.ToString() == _selectedDeviceGUID)
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
      if (_diListener != null)
      {
        _diListener.StopListener();
      }
    }

    public string SelectedDeviceGUID
    {
      get { return _selectedDeviceGUID; }
    }

    public int SelectedDeviceIndex
    {
      get { return GetSelectedDeviceIndex(); }
    }

    public int DeviceCount
    {
      get
      {
        if (null == _deviceList)
        {
          return 0;
        }
        else
        {
          return _deviceList.Count;
        }
      }
    }

    public ArrayList DeviceNames
    {
      get { return _deviceNames; }
    }

    public ArrayList DeviceGUIDs
    {
      get { return _deviceGUIDs; }
    }


    private void UnacquireDevice()
    {
      _diListener.DeInitDevice();
      _selectedDeviceGUID = string.Empty;
    }

    private bool AcquireDevice(string devGUID)
    {
      bool res = false;
      if (null == _deviceList)
      {
        return false;
      }
      _deviceList.Reset();
      foreach (DeviceInstance di in _deviceList)
      {
        if (di.InstanceGuid.ToString() == devGUID)
        {
          _selectedDeviceGUID = devGUID;
          // create and init device
          res = _diListener.InitDevice(di.InstanceGuid);
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


    private void SendActions(JoystickState state)
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
        if (null != _lastProc)
        {
          actionCode = (int) joyButton.comboKillProcess;
          actionParam = _lastProc.Id;
        }
      }
      else if ((ButtonComboClose != "") && (ButtonComboClose == pressedButtons))
      {
        if (null != _lastProc)
        {
          actionCode = (int) joyButton.comboCloseProcess;
          actionParam = _lastProc.Id;
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
        if (Math.Abs(state.X) > _axisLimit)
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
        else if (Math.Abs(state.Y) > _axisLimit)
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
        else if (Math.Abs(state.Z) > _axisLimit)
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
        if (Math.Abs(state.Rx) > _axisLimit)
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
        else if (Math.Abs(state.Ry) > _axisLimit)
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
        else if (Math.Abs(state.Rz) > _axisLimit)
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

      if (VerifyAction(actionCode, curAxisValue))
      {
        Log.Info("mapping action {0}", actionCode);
        _inputHandler.MapAction(actionCode, actionParam);
      }
    }

    private bool VerifyAction(int actionCode, int curAxisValue)
    {
      bool res = false;
      if (_diListener.IsRunning)
      {
        res = (actionCode > 0) && (actionCode < 4000) && (!FilterAction(actionCode, curAxisValue));
      }
      else
      {
        res = (actionCode >= 4000) && (actionCode < 5000);
      }
      return res;
    }

    private string GetStateAsText(JoystickState state)
    {
      string strText = string.Empty;

      string joyState = string.Format("Axis    : {0:+0000;-0000} / {1:+0000;-0000} / {2:+0000;-0000}\n", state.X,
                                      state.Y, state.Z);
      joyState += string.Format("Rotation: {0:+0000;-0000} / {1:+0000;-0000} / {2:+0000;-0000}\n\n", state.Rx, state.Ry,
                                state.Rz);

      int[] slider = state.GetSlider();
      joyState += string.Format("Slider  : 0: {0:+0000;-0000} 1: {1:+0000;-0000}\n\n", slider[0], slider[1]);

      int[] pov = state.GetPointOfView();
      switch (pov[0])
      {
        case 0:
          {
            joyState += string.Format("POV     : North\n");
            break;
          }
        case 4500:
          {
            joyState += string.Format("POV     : NorthEast\n");
            break;
          }
        case 9000:
          {
            joyState += string.Format("POV     : East\n");
            break;
          }
        case 13500:
          {
            joyState += string.Format("POV     : SouthEast\n");
            break;
          }
        case 18000:
          {
            joyState += string.Format("POV     : South\n");
            break;
          }
        case 22500:
          {
            joyState += string.Format("POV     : SouthWest\n");
            break;
          }
        case 27000:
          {
            joyState += string.Format("POV     : West\n");
            break;
          }
        case 31500:
          {
            joyState += string.Format("POV     : NorthWest\n");
            break;
          }
        default:
          {
            break;
          }
      }

      // Fill up text with which buttons are pressed
      byte[] buttons = state.GetButtons();

      int button = 0;
      foreach (byte b in buttons)
      {
        if (0 != (b & 0x80))
        {
          strText += button.ToString("00 ");
        }
        button++;
      }
      if (strText != string.Empty)
      {
        joyState += "Buttons : " + strText;
      }
      return joyState;
    }

    public void RunControlPanel()
    {
      if (null != _diListener.SelectedDevice)
      {
        _diListener.SelectedDevice.RunControlPanel();
      }
    }


    public void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _active = xmlreader.GetValueAsBool("remote", "DirectInput", false);
        string strGUID = xmlreader.GetValueAsString("remote", "DirectInputDeviceGUID", "");
        if (_active && ("" != strGUID))
        {
          SelectDevice(strGUID);
        }
        _delay = xmlreader.GetValueAsInt("remote", "DirectInputDelayMS", 150);
        _buttonComboKill = xmlreader.GetValueAsString("remote", "DirectInputKillCombo", "");
        _buttonComboClose = xmlreader.GetValueAsString("remote", "DirectInputCloseCombo", "");
      }
    }

    public void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("remote", "DirectInput", _active);
        xmlwriter.SetValue("remote", "DirectInputDeviceGUID", SelectedDeviceGUID);
        xmlwriter.SetValue("remote", "DirectInputDelayMS", _delay);
        xmlwriter.SetValue("remote", "DirectInputKillCombo", _buttonComboKill);
        xmlwriter.SetValue("remote", "DirectInputCloseCombo", _buttonComboClose);
      }
    }

    private void CreateListener()
    {
      _diListener = new DirectInputListener();
      _diListener.Delay = this.Delay;
      _diListener.OnStateChange += new DirectInputListener.diStateChange(diListener_OnStateChange);
    }

    private void FreeListener()
    {
      if (_diListener != null)
      {
        _diListener.OnStateChange -= new DirectInputListener.diStateChange(diListener_OnStateChange);
        _diListener = null;
      }
    }

    private void diListener_OnStateChange(object sender, JoystickState state)
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

    private bool FilterAction(int actionCode, int axisValue)
    {
      bool res = false;
      // filter actionCodes only if
      // 1) the last code that was sent is the one we'd like to re-send now
      // AND 
      // 2) the time elapsed when sending the same code is smaller than the delay threshold
      if (actionCode == _lastCodeSent)
      {
/*
        int timeNow = timeGetTime();
        int timeElapsed = timeNow - timeLastSend;
        if (timeElapsed < delay)
        {
          res = true;
        }
        else 
*/
        if (Math.Abs(axisValue) < Math.Abs(_lastAxisValue))
        {
          // axis is being released => don't send action!
          res = true;
        }
      }
      if (!res)
      {
        _lastCodeSent = actionCode;
        _timeLastSend = timeGetTime();
        _lastAxisValue = axisValue;
      }
      return res;
    }


    private void CreateMapper()
    {
      _inputHandler = new InputHandler("DirectInput");
    }

    private void AttachHandlers()
    {
      Util.Utils.OnStartExternal += new Util.Utils.UtilEventHandler(OnStartExternal);
      Util.Utils.OnStopExternal += new Util.Utils.UtilEventHandler(OnStopExternal);
    }

    private void DetachHandlers()
    {
      Util.Utils.OnStartExternal -= new Util.Utils.UtilEventHandler(OnStartExternal);
      Util.Utils.OnStopExternal -= new Util.Utils.UtilEventHandler(OnStopExternal);
    }

    public void OnStartExternal(Process proc, bool waitForExit)
    {
      if (_active && waitForExit)
      {
        _lastProc = proc;
        _diListener.Pause();
      }
    }

    public void OnStopExternal(Process proc, bool waitForExit)
    {
      if (_active)
      {
        _lastProc = null;
        _diListener.Resume();
      }
    }
  }
}