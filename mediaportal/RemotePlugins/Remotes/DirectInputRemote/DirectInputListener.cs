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
using System.Threading;
using Microsoft.DirectX.DirectInput;

namespace MediaPortal
{
  /// <summary>
  /// Summary description for DirectInputListener.
  /// </summary>
  /// 
  public class DirectInputListener
  {
    private Device device = null;
    private Thread inputListener = null;
    private bool isRunning = false;
    private int delay = 150; // sleep time in milliseconds

    // event: send info on joystick state change 
    public delegate void diStateChange(object sender, JoystickState state);

    public event diStateChange OnStateChange = null;


    public DirectInputListener()
    {
      //
      // TODO: Add constructor logic here
      //
    }


    ~DirectInputListener()
    {
      StopListener();
      DeInitDevice();
    }

    public Device SelectedDevice
    {
      get { return device; }
    }

    public bool IsRunning
    {
      get { return isRunning; }
    }

    public int Delay
    {
      get { return delay; }
      set { delay = value; }
    }

    public bool InitDevice(Guid guid)
    {
      device = new Device(guid);
      device.SetCooperativeLevel(null, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
      device.Properties.AxisModeAbsolute = true;

      // Enumerate axes
      foreach (DeviceObjectInstance doi in device.Objects)
      {
        if ((doi.ObjectId & (int)DeviceObjectTypeFlags.Axis) != 0)
        {
          // We found an axis, set the range to a max of 10,000
          device.Properties.SetRange(ParameterHow.ById,
                                     doi.ObjectId, new InputRange(-5000, 5000));
        }
      }
      StopListener();
      StartListener();
      device.Acquire();
      return true;
    }

    public void DeInitDevice()
    {
      if (null != device)
      {
        try
        {
          device.Unacquire();
        }
        catch (NullReferenceException) {}
        device.Dispose();
        device = null;
      }
    }


    public string GetCurrentButtonCombo()
    {
      string res = "";
      JoystickState state;
      if (CheckDevice())
      {
        // Get the state of the device.
        try
        {
          state = device.CurrentJoystickState;
          return ButtonComboAsString(state);
        }
          // Catch any exceptions. None will be handled here, 
          // any device re-aquisition will be handled above.  
        catch (InputException)
        {
          return res;
        }
      }
      return res;
    }

    private string ButtonComboAsString(JoystickState state)
    {
      byte[] buttons = state.GetButtons();
      int button = 0;
      string res = "";

      // button combos
      string sep = "";
      foreach (byte b in buttons)
      {
        if (0 != (b & 0x80))
        {
          res += sep + button.ToString("00");
          sep = ",";
        }
        button++;
      }
      return res;
    }

    private void ThreadFunction()
    {
      while (true)
      {
        UpdateInputState();
        Thread.Sleep(delay);
      }
    }


    public bool CheckDevice()
    {
      if (null == device)
      {
        return false;
      }
      try
      {
        // Poll the device for info.
        device.Poll();
      }
      catch (InputException inputex)
      {
        if ((inputex is NotAcquiredException) || (inputex is InputLostException))
        {
          // Check to see if either the app
          // needs to acquire the device, or
          // if the app lost the device to another
          // process.
          try
          {
            // Acquire the device.
            device.Acquire();
          }
          catch (InputException)
          {
            // Failed to acquire the device.
            // This could be because the app
            // doesn't have focus.
            return false;
          }
        }
      } //catch(InputException inputex)

      return (device != null);
    }

    private void UpdateInputState()
    {
      JoystickState state;
      if (CheckDevice())
      {
        // Get the state of the device.
        try
        {
          state = device.CurrentJoystickState;
        }
          // Catch any exceptions. None will be handled here, 
          // any device re-aquisition will be handled above.  
        catch (InputException)
        {
          return;
        }
        // send events here
        if (null != this.OnStateChange)
        {
          OnStateChange(this, state);
        }
      }
    }


    public void StopListener()
    {
      if (null != inputListener)
      {
        isRunning = false;
        inputListener.Abort();
        inputListener = null;
      }
    }

    private void StartListener()
    {
      inputListener = new Thread(new ThreadStart(this.ThreadFunction));
      inputListener.IsBackground = true;
      inputListener.Name = "DirectInputListener";
      inputListener.Start();
      isRunning = true;
    }

    public void Pause()
    {
      isRunning = false;
    }

    public void Resume()
    {
      isRunning = true;
    }
  }
}