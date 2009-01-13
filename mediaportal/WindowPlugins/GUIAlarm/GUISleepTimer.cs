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
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.GUI.Alarm
{
  public delegate void SleepTimerElapsedEventHandler(object sender, EventArgs e);

  /// <summary>
  /// Summary description for GUISleepTimer.
  /// </summary>
  public class GUISleepTimer : GUIWindow, IDisposable
  {
    public event SleepTimerElapsedEventHandler SleepTimerElapsed;
    public const int WindowSleepTimer = 5002;

    #region Private Variables

    private Timer _SleepTimer = new Timer();
    private long _SleepCount;
    private int _PreviousgPlayerVolume = 0;

    #endregion

    #region Skin control IDs

    private enum Controls
    {
      EnableButton = 3,
      Minutes = 4,
      VolumeFade = 5,
      ReturnHomeButton = 6,
      ResetButton = 7
    }

    [SkinControl(3)] protected GUIToggleButtonControl btnEnabled = null;
    [SkinControl(4)] protected GUISpinControl ctlMinutes = null;
    [SkinControl(5)] protected GUICheckMarkControl chkFadeVolume = null;
    [SkinControl(6)] protected GUICheckMarkControl btnReturnHome = null;
    [SkinControl(7)] protected GUIButtonControl btnReset = null;

    #endregion

    #region Constructor

    public GUISleepTimer()
    {
      _SleepTimer.Tick += new EventHandler(OnTimer);
      _SleepTimer.Interval = 1000; //second	
      this.SleepTimerElapsed += new SleepTimerElapsedEventHandler(GUISleepTimer_SleepTimerElapsed);
      GetID = (int) WindowSleepTimer;
    }

    #endregion

    #region Overrides

    public override bool Init()
    {
      GUIPropertyManager.SetProperty("#currentsleeptime", "00:00");
      return Load(GUIGraphicsContext.Skin + @"\myalarmsleeptimer.xml");
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(850));
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int controlId = message.SenderControlId;
            if (controlId == btnEnabled.GetID)
            {
              if (!btnEnabled.Selected)
              {
                _SleepTimer.Enabled = false;
                GUIPropertyManager.SetProperty("#currentsleeptime", "00:00");
                //revert volume to original volume
                if (_PreviousgPlayerVolume > 0)
                {
                  g_Player.Volume = _PreviousgPlayerVolume;
                }
              }
              else
              {
                _SleepCount = ctlMinutes.Value*60;
                if (ctlMinutes.Value > 0)
                {
                  _SleepTimer.Enabled = true;
                  _PreviousgPlayerVolume = g_Player.Volume;
                }
                else
                {
                  btnEnabled.Selected = false;
                }
              }
            }

            if (controlId == btnReset.GetID)
            {
              _SleepCount = ctlMinutes.Value*60;
              GUIPropertyManager.SetProperty("#currentsleeptime", ConvertToTime(_SleepCount));
            }
          }
          break;
      }

      return base.OnMessage(message);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Executes on the interval of the timer object.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTimer(Object sender, EventArgs e)
    {
      _SleepCount--;

      if (_SleepCount == 0)
      {
        OnSleepTimerElapsed(e);
      }
      else
      {
        //calculate if there is 100 seconds left in the sleep timer
        bool MinuteLeft = _SleepCount <= 100;

        if (chkFadeVolume.Selected && MinuteLeft)
        {
          if (g_Player.Volume > 0)
          {
            g_Player.Volume--;
          }
        }
      }

      GUIPropertyManager.SetProperty("#currentsleeptime", ConvertToTime(_SleepCount));
    }

    /// <summary>
    /// Converts tick counts to a formated time string 00:00
    /// </summary>
    /// <param name="tickCount"></param>
    /// <returns>formatted time string</returns>
    private string ConvertToTime(long tickCount)
    {
      // tickcount is in seconds, convert to a minutes: seconds string
      long seconds = tickCount;
      string val = (seconds/60).ToString("00") + ":" + (seconds%60).ToString("00");
      return val;
    }

    protected virtual void OnSleepTimerElapsed(EventArgs e)
    {
      if (SleepTimerElapsed != null)
      {
        SleepTimerElapsed(this, e);
      }
    }

    /// <summary>
    /// Handles the sleep timer elapsed event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GUISleepTimer_SleepTimerElapsed(object sender, EventArgs e)
    {
      _SleepTimer.Enabled = btnEnabled.Selected = false;
      g_Player.Stop();

      //revert volume to original volume
      if (_PreviousgPlayerVolume > 0)
      {
        g_Player.Volume = _PreviousgPlayerVolume;
      }

      //returns to the home screen so powerscheduler plugin can suspend the pc
      if (btnReturnHome.Selected)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_HOME);
      }

      //Util.WindowsController.ExitWindows(Util.RestartOptions.Hibernate,true);
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      _SleepTimer.Dispose();
    }

    #endregion
  }
}