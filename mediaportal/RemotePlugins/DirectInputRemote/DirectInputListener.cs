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
    Device device = null;
    Thread inputListener = null;
    AutoResetEvent deviceUpdated;
    ManualResetEvent appShutdown;
    bool isRunning = false;

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


    public bool InitDevice(System.Guid guid)
    {
      device = new Device(guid);
      device.SetCooperativeLevel(null, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
      device.Properties.AxisModeAbsolute = true;

      // Enumerate axes
      foreach (DeviceObjectInstance doi in device.Objects)
      {
        if ((doi.ObjectId & (int) DeviceObjectTypeFlags.Axis) != 0)
        {
          // We found an axis, set the range to a max of 10,000
          device.Properties.SetRange(ParameterHow.ById,
                                     doi.ObjectId, new InputRange(-5000, 5000));
        }
      }
      deviceUpdated = new AutoResetEvent(false);
      appShutdown = new ManualResetEvent(false);

      device.SetEventNotification(deviceUpdated);
      StopListener();
      StartListener();
      device.Acquire();
      return true;
    }

    public void DeInitDevice()
    {
      if (null != device)
      {
        device.Unacquire();
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

    string ButtonComboAsString(JoystickState state)
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

    void ThreadFunction()
    {
      WaitHandle[] handles = {deviceUpdated, appShutdown};
      // Continue running this thread until the app has closed
      while (true)
      {
        int index = WaitHandle.WaitAny(handles);
        if (index == 0)
        {
//          if (isRunning)
          {
            UpdateInputState();
          }
        }
        else if (index == 1)
        {
          return;
        }
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

    void UpdateInputState()
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
        inputListener.Abort();
        inputListener = null;
        isRunning = false;
      }
    }

    void StartListener()
    {
      inputListener = new Thread(new ThreadStart(this.ThreadFunction));
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