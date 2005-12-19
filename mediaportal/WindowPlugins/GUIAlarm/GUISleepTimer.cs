/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
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
			private System.Windows.Forms.Timer _SleepTimer = new System.Windows.Forms.Timer();
			private long _SleepCount;
		#endregion
		
		#region Private Enumerations
			enum Controls
			{
				EnableButton = 3,
				Minutes = 4,
				VolumeFade = 5,	
				ReturnHomeButton = 6,
				ResetButton = 7
			}
		#endregion

		#region Constructor
			public GUISleepTimer()
			{
				_SleepTimer.Tick += new EventHandler(OnTimer);
				_SleepTimer.Interval = 1000; //second	
				this.SleepTimerElapsed +=new SleepTimerElapsedEventHandler(GUISleepTimer_SleepTimerElapsed);
				GetID=(int)GUISleepTimer.WindowSleepTimer;
			}
		#endregion

		#region Overrides
			public override bool Init()
			{
				GUIPropertyManager.SetProperty("#currentsleeptime","00:00");
				return Load (GUIGraphicsContext.Skin+@"\myalarmsleeptimer.xml");
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
				switch ( message.Message )
				{

					case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
						base.OnMessage(message);
						GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(850));
						return true;
					//break;
					case GUIMessage.MessageType.GUI_MSG_CLICKED:
					{
						//get sender control
						base.OnMessage(message);
						int iControl=message.SenderControlId;
								
						if (iControl==(int)Controls.EnableButton)
						{
							GUIToggleButtonControl btnEnabled = (GUIToggleButtonControl)GetControl((int)Controls.EnableButton);
							GUISpinControl ctlMinutes = (GUISpinControl)GetControl((int)Controls.Minutes);
							
							if(!btnEnabled.Selected)
							{
								_SleepTimer.Enabled = false;
								GUIPropertyManager.SetProperty("#currentsleeptime","00:00");	
							}
							else
							{

								_SleepCount = ctlMinutes.Value*60;
								if(ctlMinutes.Value > 0)
								{
									_SleepTimer.Enabled = true;
								}
								else
								{
									btnEnabled.Selected = false;
								}
							}
						}	
						if(iControl==(int)Controls.ResetButton)
						{
							GUISpinControl ctlMinutes = (GUISpinControl)GetControl((int)Controls.Minutes);
							_SleepCount = ctlMinutes.Value*60;
							GUIPropertyManager.SetProperty("#currentsleeptime",ConvertToTime(_SleepCount));	
						}
					
						return true;
					}
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
					
					if(_SleepCount == 0)
					{
						OnSleepTimerElapsed(e);
					}
					else
					{
						GUICheckMarkControl chkFadeVolume = (GUICheckMarkControl)GetControl((int)Controls.VolumeFade);
						//calculate if there is 100 seconds left in the sleep timer
						bool MinuteLeft = _SleepCount <= 100;

						if(chkFadeVolume.Selected && MinuteLeft)
						{
							if(g_Player.Volume > 0)
								g_Player.Volume--;
						}
					}

					GUIPropertyManager.SetProperty("#currentsleeptime",ConvertToTime(_SleepCount));
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
					string val = (seconds/60).ToString("00") + ":" + (seconds % 60).ToString("00");
					return val;	
				}

				protected virtual void OnSleepTimerElapsed(EventArgs e) 
				{
					if (SleepTimerElapsed != null) SleepTimerElapsed(this, e);
				}

				/// <summary>
				/// Handles the sleep timer elapsed event
				/// </summary>
				/// <param name="sender"></param>
				/// <param name="e"></param>
				private void GUISleepTimer_SleepTimerElapsed(object sender, EventArgs e)
				{
					_SleepTimer.Enabled=false;
					g_Player.Stop();
					((GUIToggleButtonControl)GetControl((int)Controls.EnableButton)).Selected = false;
						
					//returns to the home screen so powerscheduler plugin can suspend the pc
					if(((GUICheckMarkControl)GetControl((int)Controls.ReturnHomeButton)).Selected)
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
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
