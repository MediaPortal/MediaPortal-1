#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Media.Animation;
using MediaPortal.Drawing;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  public sealed class GUIWaitCursor : GUIControl
  {
    #region Constructors

    private GUIWaitCursor() {}

    #endregion Constructors

    #region Methods

    private static Thread guiWaitCursorThread = null;

    public new static void Dispose()
    {
      if (_animation != null)
      {
        _animation.SafeDispose();
      }

      _animation = null;
    }

    public static void Show()
    {
        //do increment here rather than in thread since if wait cursor is running
        //thread is NOT null, we will not incrment _showCount
        //after that hide is called twice which result in _showCount less than 0
        //read - not working wait cursor any more
        Interlocked.Increment(ref _showCount);
        if (guiWaitCursorThread == null)
        {
            guiWaitCursorThread = new Thread(GUIWaitCursorThread);
            guiWaitCursorThread.IsBackground = true;
            guiWaitCursorThread.Name = "Waitcursor";
            guiWaitCursorThread.Start();
        }
    }

    public static void Hide()
    {
      Interlocked.Decrement(ref _showCount);
      guiWaitCursorThread = null;
      _elapsedEventRunning = false;
      _countDown = false;
    }

    private static void GUIWaitCursorThread()
    {
      //start animation only if _showCount equals 1
      //this is to prevent animation starting from beginning every time Show() is called, making it "jumpy"
      if (Interlocked.Equals(_showCount, 1))
      {
        _animation.Begin();
      }
    }

    public static void Init()
    {
      if (_countLabel == null)
      {
        _countLabel = new GUILabelControl(0);
        _countLabel.FontName = "waitcursor";
        _countLabel.TextColor = 0x66ffffff;
        _countLabel.TextAlignment = Alignment.ALIGN_CENTER;
        _countLabel.TextVAlignment = VAlignment.ALIGN_MIDDLE;
        _countLabel.Width = 96;
        _countLabel.Height = 96;
      }

      if (_animation == null)
      {
        _animation = new GUIAnimation();

        string themedFilename;
        foreach (string filename in Directory.GetFiles(GUIGraphicsContext.Skin + @"\media\", "common.waiting.*.png"))
        {
          themedFilename = GUIGraphicsContext.GetThemedSkinFile(filename);
          _animation.Filenames.Add(Path.GetFileName(themedFilename));
        }

        // dirty hack because the files are 96x96 - unfortunately no property gives the correct size at runtime when init is called :S
        int scaleWidth = (GUIGraphicsContext.Width / 2) - 48;
        int scaleHeigth = (GUIGraphicsContext.Height / 2) - 48;

        _animation.SetPosition(scaleWidth, scaleHeigth);

        // broken!?
        _animation.HorizontalAlignment = HorizontalAlignment.Center;
        _animation.VerticalAlignment = VerticalAlignment.Center;

        Log.Debug("GUIWaitCursor: init at position {0}:{1}", scaleWidth, scaleHeigth);
        _animation.AllocResources();
        _animation.Duration = new Duration(800);
        _animation.RepeatBehavior = RepeatBehavior.Forever;
      }
    }

    public delegate void OnWaitCursorElapsed(object paramObj);

    /// <summary>
    /// Invokes a user specified method when the wait cursor count down has reached zero.
    /// </summary>
    public static void ElapsedEvent(int countDownMs, OnWaitCursorElapsed target, object paramObj)
    {
      _eventTarget = target;
      _eventParam = paramObj;
      _countValue = countDownMs / 1000;
      _countDown = (countDownMs > 0);
      _startTime = DXUtil.timeGetTime();
      _elapsedEventRunning = true;

      // The count label depends on font rendering.  If the users callback involves reloading fonts then we need to reallocate
      // the labels resources to be sure everything calculates okay (e.g., the x,y position of the control).
      _countLabel.Dispose();
      _countLabel.AllocResources();

      Show();
    }

    /// <summary>
    /// Send a user specified message when the wait cursor count down has reached zero.
    /// </summary>
    public static void ElapsedEvent(int countDownMs, GUIMessage message)
    {
      _eventTarget = SendElapsedEventMessage;
      _eventParam = message;
      _countValue = countDownMs / 1000;
      _countDown = (countDownMs > 0);
      _startTime = DXUtil.timeGetTime();
      _elapsedEventRunning = true;

      // The count label depends on font rendering.  If the users callback involves reloading fonts then we need to reallocate
      // the labels resources to be sure everything calculates okay (e.g., the x,y position of the control).
      _countLabel.Dispose();
      _countLabel.AllocResources();

      Show();
    }

    private static void SendElapsedEventMessage(object data)
    {
      GUIWindowManager.SendThreadMessage((GUIMessage)data);
    }

    public static bool ElapsedEventRunning()
    {
      return _elapsedEventRunning;
    }

    private static void GUIWaitCursorElapsedEventThread()
    {
      // Execute the users callback.
      _eventTarget(_eventParam);
    }

    public override void Render(float timePassed) {}

    public static void Render()
    {
      if (_showCount <= 0)
      {
        return;
      }

      GUIGraphicsContext.SetScalingResolution(0, 0, false);

      if (_countDown)
      {
        // Set the count label value and position in the center of the window.
        _timeElapsed = DXUtil.timeGetTime() - _startTime;
        if (_timeElapsed >= ONE_SECOND)
        {
          _startTime = DXUtil.timeGetTime();
          _timeElapsed = 0.0f;
          _countValue--;
          if (_countValue < 0)
          {
            // Invoke the users callback method and disable the cursor.
            // The callback must be invoked in a separte thread otherwise it gets executed in the window render loop which
            // can cause conflicts (especially if the users callback method makes calls that may change the window rendering).
            guiWaitCursorThread = new Thread(GUIWaitCursorElapsedEventThread);
            guiWaitCursorThread.IsBackground = true;
            guiWaitCursorThread.Name = "WaitcursorElapsedEvent";
            guiWaitCursorThread.Start();
            Hide();
            return;
          }
        }

        int offset = 5;
        int xPos = (GUIGraphicsContext.Width / 2) - 48;
        int yPos = (GUIGraphicsContext.Height / 2) - 48 + offset;
        _countLabel.SetPosition(xPos, yPos);
        _countLabel.Label = _countValue.ToString();
        _countLabel.Render(GUIGraphicsContext.TimePassed);
      }

      _animation.Render(GUIGraphicsContext.TimePassed);
    }

    #endregion Methods

    #region Fields

    private static GUIAnimation _animation;
    private static GUILabelControl _countLabel;
    private static int _countValue = 0;
    private static bool _countDown = false;
    private const float ONE_SECOND = 1000.0f;
    private static float _timeElapsed = 0.0f;
    private static float _startTime;
    private static OnWaitCursorElapsed _eventTarget = null;
    private static object _eventParam;
    private static bool _elapsedEventRunning = false;
    private static int _showCount = 0;

    #endregion Fields
  }
}