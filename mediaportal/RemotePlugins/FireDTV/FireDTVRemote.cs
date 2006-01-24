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
using System.Windows.Forms;
using System.Data;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using System.Reflection;
using MadMouse;

namespace MediaPortal.RemoteControls
{
	/// <summary>
	/// Summary description for FireDTVRemote.
	/// </summary>
	public class FireDTVRemote : IRemoteControlInterface
	{
		#region Private Variables
		private MadMouse.FireDTV.FireDTVControl FireDTVobject = new MadMouse.FireDTV.FireDTVControl(0);
		private static bool		_remoteEnabled		= false;		
		private static bool		_AdvModeEnabled		= false;
		private static bool		_FireSAPapiFound	= false;
		private static string	_FileName;
		private static string	_RemoteName;
		private static string	_DisplayName;
		private static IntPtr	_windowHandle;
		private static bool		_KeyMapFileLoaded	= false;
		private static DataSet	_KeyMapFile			= new DataSet();
		#endregion

		public FireDTVRemote()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region Private Methods
		private void StartFireDTVComms()
		{
			if (_remoteEnabled)
			{
				FireDTVobject.OpenDrivers(_windowHandle);
				MadMouse.FireDTV.FireDTVSourceFilterInfo sourceFilter = FireDTVobject.GetSourceFilterByDisplayString(_DisplayName);
				if (sourceFilter != null)
				{
					sourceFilter.StartFireDTVRemoteControlSupport();
				}
				else
					Log.Write("FireDTV Source Filter Not Found");
			}
		}
		
		private static object StringToEnum( Type t, string Value )
		{
			foreach ( FieldInfo fi in t.GetFields() )
				if ( fi.Name.ToUpper() == Value.ToUpper() )
					return fi.GetValue( null );    

			Log.Write(string.Format("FireSATRemote StringToEnum : Can't convert {0} to {1}", Value,t.ToString()));
			return null;
		}
		#endregion
		#region IRemoteControlInterface Members

		public void Init(IntPtr hwnd)
		{
			try
			{
				_windowHandle = hwnd;
				_FireSAPapiFound = File.Exists("FiresatApi.dll");
				if (!_FireSAPapiFound) 
					_remoteEnabled=false;

				using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
				{
					_remoteEnabled	= ((xmlreader.GetValueAsBool("remote", "FireDTV", false)) && (_FireSAPapiFound));
					_FileName		= (xmlreader.GetValueAsString("remote", "FireDTVKeyFile", "FireDTVKeyMap.XML"));
					_RemoteName		= (xmlreader.GetValueAsString("remote", "FireDTVRemoteName", "FireDTV Remote Control"));
					_AdvModeEnabled	=  xmlreader.GetValueAsBool("remote", "FireDTVAdvanceMode", false);
					_DisplayName	=  xmlreader.GetValueAsString("remote", "FireDTVDeviceName", string.Empty);
					_AdvModeEnabled =  xmlreader.GetValueAsBool("remote", "FireDTVAdvanceMode", false);
				}

				if (_remoteEnabled)
				{
					StartFireDTVComms();

					if (File.Exists(_FileName))
					{
						_KeyMapFile.ReadXmlSchema(_FileName + ".Schema");
						_KeyMapFile.ReadXml(_FileName);
						_KeyMapFileLoaded = true;
					}
					else
						Log.Write("FireDTV Key Map File Not Found!");
				}
			}
			catch(FileNotFoundException eFileNotFound)
			{
				Log.Write(eFileNotFound.Message);
			}
			catch(MadMouse.FireDTV.FireDTVException eFireDTV)
			{
				Log.Write(eFireDTV.Message);
			}
		}

		public void DeInit()
		{
			FireDTVobject.CloseDrivers();
		}

		public bool WndProc(ref System.Windows.Forms.Message msg, out MediaPortal.GUI.Library.Action action, out char key, out System.Windows.Forms.Keys keyCode)
		{
			keyCode= System.Windows.Forms.Keys.A;
			key=(char)0;
			action=null;
			switch ((MadMouse.FireDTV.FireDTVConstants.FireDTVWindowMessages)msg.Msg)
			{
				case MadMouse.FireDTV.FireDTVConstants.FireDTVWindowMessages.DeviceAttached :
					StartFireDTVComms();
					break;

				case MadMouse.FireDTV.FireDTVConstants.FireDTVWindowMessages.DeviceDetached :
					FireDTVobject.SourceFilterCollection.Remove(FireDTVobject.GetSourceFilterIndexByDeviceHandle((uint)msg.WParam));
				break;

				case MadMouse.FireDTV.FireDTVConstants.FireDTVWindowMessages.DeviceChanged :
					StartFireDTVComms();
					break;

				case MadMouse.FireDTV.FireDTVConstants.FireDTVWindowMessages.RemoteControlEvent :
					if (_remoteEnabled) 
					{
						if ((_KeyMapFileLoaded) && (_AdvModeEnabled))
						{
							try
							{
								DataRow foundRemoteRow;
								object[] findThisRemoteName = new object[1];
								findThisRemoteName[0] = _RemoteName;
								foundRemoteRow = _KeyMapFile.Tables["RemoteControl"].Rows.Find(findThisRemoteName);
								if (foundRemoteRow != null)
								{	
									DataRow foundKeyMapRow;
									object[] findThisKeyValue = new object[2];
									findThisKeyValue[0] = foundRemoteRow["RemoteID"].ToString();
									findThisKeyValue[1] = msg.LParam.ToString();

									foundKeyMapRow = _KeyMapFile.Tables["RemoteControlKeys"].Rows.Find(findThisKeyValue);
									if ((foundKeyMapRow != null) && (Convert.ToBoolean(foundKeyMapRow["Enabled"]) == true))
									{
										if (foundKeyMapRow["ActionName"] != System.DBNull.Value)
										{
											action=new Action(((Action.ActionType)StringToEnum(typeof(Action.ActionType),foundKeyMapRow["ActionName"].ToString())),0,0);
											return true;
										}
										if ((foundKeyMapRow["Key_Value"] != System.DBNull.Value) ||
											(foundKeyMapRow["Key_Code"] != System.DBNull.Value))
										{
											if (foundKeyMapRow["Key_Value"] != System.DBNull.Value)
												key = Convert.ToChar(foundKeyMapRow["Key_Value"].ToString());

											if (foundKeyMapRow["Key_Code"] != System.DBNull.Value)
												keyCode = ((Keys)StringToEnum(typeof(Keys),foundKeyMapRow["Key_Code"].ToString()));
											return true;
										}			

										if (foundKeyMapRow["DestinationWindow"] != System.DBNull.Value)
										{
											GUIMessage msgMusic = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)((GUIWindow.Window)StringToEnum(typeof(GUIWindow.Window),foundKeyMapRow["DestinationWindow"].ToString())),0,null);
											GUIGraphicsContext.SendMessage(msgMusic);
											return true;
										}

									}
								
								}
							}
							catch(InvalidCastException eCast)
							{
								Log.Write("FireDTV Map Key Error : " + eCast.Message);
							}
						}
				
						#region Handle Code with Default Actions.
						switch ((MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes)msg.LParam.ToInt32())
						{
								#region Remote Keys
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_0 :
								action=new Action(Action.ActionType.REMOTE_0,0,0);
								return true;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_1 :
								action=new Action(Action.ActionType.REMOTE_1,0,0);
								return true;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_2 :
								action=new Action(Action.ActionType.REMOTE_2,0,0);
								return true;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_3 :
								action=new Action(Action.ActionType.REMOTE_3,0,0);
								return true;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_4 :
								action=new Action(Action.ActionType.REMOTE_4,0,0);
								return true;
					
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_5 :
								action=new Action(Action.ActionType.REMOTE_5,0,0);
								return true;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_6 :
								action=new Action(Action.ActionType.REMOTE_6,0,0);
								return true;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_7 :
								action=new Action(Action.ActionType.REMOTE_7,0,0);
								return true;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_8 :
								action=new Action(Action.ActionType.REMOTE_8,0,0);
								return true;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_9 :
								action=new Action(Action.ActionType.REMOTE_9,0,0);
								return true;
								#endregion

								#region Audio Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Audio :
								GUIMessage msgMusic = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_MUSIC_FILES,0,null);
								GUIGraphicsContext.SendMessage(msgMusic);
								return true;
								#endregion

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_AUX :
								break;

								#region Blue Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Blue :
								GUIMessage msgPics = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_PICTURES,0,null);
								GUIGraphicsContext.SendMessage(msgPics);
								return true;
								#endregion

								#region Channel Down
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_ChannelDown :
								action=new Action(Action.ActionType.ACTION_PREV_CHANNEL,0,0);
								return true;
								#endregion

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_ChannelList :
								break;

								#region Channel Up
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_ChannelUp :
								action=new Action(Action.ActionType.ACTION_NEXT_CHANNEL,0,0);
								return true;
								#endregion

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_CI :
								break;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Display16_9 :
								break;

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Display4_3 :
								break;

								#region Down Arrow Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_DownArrow :
								keyCode = Keys.Down;
								return true;
								#endregion
								#region DVD Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_DVD :
								GUIMessage msgDVD = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_DVD,0,null);
								GUIGraphicsContext.SendMessage(msgDVD);
								return true;
								#endregion
								#region EPG Button
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_EPG :
								GUIMessage msgtv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TVGUIDE,0,null);
								GUIGraphicsContext.SendMessage(msgtv);
								return true;
								#endregion
								#region Exit Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Exit :
								keyCode = Keys.Escape;
								return true;
								#endregion
								#region FastFoward
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_FastFoward :
								action=new Action(Action.ActionType.ACTION_FORWARD,0,0);
								return true;
								#endregion

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Favourites :
								break;
					
								#region Full Screen Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Full :
								key = 'x';  
								return true;
								#endregion
								#region Green Button
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Green :
								GUIMessage msgHome = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_HOME,0,null);
								GUIGraphicsContext.SendMessage(msgHome);
								return true;
								#endregion
								#region Info Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Info :
								action=new Action(Action.ActionType.ACTION_SHOW_INFO,0,0);
								return true;
								#endregion

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Last :
								break;
						
								#region Left Arrow Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_LeftArrow :
								keyCode = Keys.Left;
								return true;
								#endregion

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_List :
								break;
								#region menu Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Menu :
								if (g_Player.Playing && g_Player.IsDVD)
								{
									action = new Action(Action.ActionType.ACTION_DVD_MENU,0,0);  
									return true;
								}
								return true;
								#endregion
								#region Mute Key

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Mute :
								keyCode = Keys.VolumeMute;
								return true;
								#endregion
								#region next Chapter
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_NextChapter :
								if ((g_Player.Playing) && (g_Player.IsDVD))
									action=new Action(Action.ActionType.ACTION_NEXT_CHAPTER,0,0);
								else
									action=new Action(Action.ActionType.ACTION_NEXT_ITEM,0,0);
								return true;
								#endregion
								#region OSD
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_OnScreenDisplay :
						
								if (GUIGraphicsContext.IsFullScreenVideo)                   
								{
									//pop up OSD during fullscreen video or live tv (even without timeshift)
									action=new Action(Action.ActionType.ACTION_SHOW_OSD,0,0);
									return true;
								}
								break;
								#endregion
								#region Pause & Play
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_PausePlay :
								if (GUIGraphicsContext.IsPlaying)
									action=new Action(Action.ActionType.ACTION_PAUSE,0,0);
								else
									action=new Action(Action.ActionType.ACTION_PLAY,0,0);
								return true;
								#endregion

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Power :
								action=new Action(Action.ActionType.ACTION_EXIT,0,0);
								return true;
								#region Previous Chapter Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_PreviousChapter :
								if ((g_Player.Playing) && (g_Player.IsDVD))
									action=new Action(Action.ActionType.ACTION_PREV_CHAPTER,0,0);
								else
									action=new Action(Action.ActionType.ACTION_PREV_ITEM,0,0);
								return true;
								#endregion
						
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Record :
								action=new Action(Action.ActionType.ACTION_RECORD,0,0);
								return true;
								#region Red Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Red :
								action = new Action(Action.ActionType.ACTION_SHOW_GUI,0,0);
								return true;
								#endregion
								#region Rewind Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Rewind :
								action=new Action(Action.ActionType.ACTION_REWIND,0,0);
								return true;
								#endregion
								#region Right Arrow Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_RightArrow :
								keyCode = Keys.Right;
								return true;
								#endregion
								#region Select Button
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Select :
								keyCode = Keys.Return;
								return true;
								#endregion
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Sleep :
								break;

								#region Stop Eject
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_StopEject :
						
								if ((g_Player.Playing) ||(g_Player.IsTVRecording))
									action=new Action(Action.ActionType.ACTION_STOP,0,0);
								else
									action=new Action(Action.ActionType.ACTION_EJECTCD,0,0);
						
								return true;
								#endregion

							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_SubTitle :
								action = new Action(Action.ActionType.ACTION_DVD_MENU,0,0); 
								return true;
								#region Text Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Text :
								if (g_Player.IsTV)
								{
									if (GUIGraphicsContext.IsFullScreenVideo)
									{
										// Activate fullscreen teletext
										GUIMessage msgTxt1 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT,0,null);
										GUIGraphicsContext.SendMessage(msgTxt1);
									}
									else
									{
										// Activate teletext in window
										GUIMessage msgTxt2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TELETEXT,0,null);
										GUIGraphicsContext.SendMessage(msgTxt2);
									}
								}
								return true;
								#endregion
								#region TV Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_TV :
								GUIMessage msgtv2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_TV,0,null);
								GUIGraphicsContext.SendMessage(msgtv2);
								return true;
								#endregion
								#region Up Arrow
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_UpArrow :
								keyCode = Keys.Up;
								return true;
								#endregion
								#region VCR Key
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_VCR :
								GUIMessage msgVideo = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW,0,0,0,(int)GUIWindow.Window.WINDOW_VIDEOS,0,null);
								GUIGraphicsContext.SendMessage(msgVideo);
								return true;
								#endregion
								#region Volume Down
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_VolumeDown :
								action=new Action(Action.ActionType.ACTION_VOLUME_DOWN,0,0);
								return true;
								#endregion
								#region Volume Up
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_VolumeUp :
								action=new Action(Action.ActionType.ACTION_VOLUME_UP,0,0);
								return true;
								#endregion
							case MadMouse.FireDTV.FireDTVConstants.FireDTVRemoteControlKeyCodes.RemoteKey_Yellow :
								break;
						}
						#endregion
					}
					break;
			}
			
			return false;
		}

		public Guid RCGuid
		{
			get
			{
				return new Guid ("{73DF3DFD-855A-418c-B98B-121C513BD2E4}");
				//{6FAF9F89-4247-4456-8957-F69E119E60D9}
			}
		}

		#endregion
	}
}
