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

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class containing the datastructure of the GUIMessages that are sent between windows and components.
	/// </summary>
	public class GUIMessage
	{
		public enum MessageType
		{
			// messages we have defined
				GUI_MSG_WINDOW_INIT,
				GUI_MSG_WINDOW_DEINIT,
				GUI_MSG_SETFOCUS,
				GUI_MSG_LOSTFOCUS,
				GUI_MSG_CLICKED,
				GUI_MSG_VISIBLE,
				GUI_MSG_HIDDEN,
				GUI_MSG_ENABLED,
				GUI_MSG_DISABLED,
				GUI_MSG_SELECTED,
				GUI_MSG_DESELECTED,
				GUI_MSG_LABEL_ADD,
				GUI_MSG_LABEL_SET,
				GUI_MSG_LABEL_RESET,
				GUI_MSG_ITEM_SELECTED,
				GUI_MSG_ITEM_SELECT,
        GUI_MSG_ITEM_FOCUS,
				GUI_MSG_LABEL2_SET,
				GUI_MSG_SHOWRANGE,
        GUI_MSG_GET_ITEM,
        GUI_MSG_START_SLIDESHOW,
        GUI_MSG_ITEMS,
        GUI_MSG_GET_SELECTED_ITEM,
        GUI_MSG_PLAYBACK_STOPPED,
        GUI_MSG_PLAYLIST_CHANGED,
        GUI_MSG_PLAYBACK_ENDED,
        GUI_MSG_PLAYBACK_STARTED,
        GUI_MSG_PERCENTAGE_CHANGED,
        GUI_MSG_PLAY_FILE,
        GUI_MSG_STOP_FILE,
        GUI_MSG_SEEK_FILE_PERCENTAGE,
				GUI_MSG_RECORDER_ALLOC_CARD,
        GUI_MSG_RECORDER_FREE_CARD,
        GUI_MSG_RECORDER_STOP_TIMESHIFT,
        GUI_MSG_TUNE_EXTERNAL_CHANNEL,
        GUI_MSG_GET_STRING,
        GUI_MSG_GET_PASSWORD,
        GUI_MSG_SWITCH_FULL_WINDOWED,
        GUI_MSG_PLAY_AUDIO_CD,
        GUI_MSG_CD_REMOVED,
        GUI_MSG_CD_INSERTED,
        GUI_MSG_PLAYING_10SEC,//file is playing 10 sec
        GUI_MSG_PLAY_RADIO_STATION,
        GUI_MSG_SHOW_WARNING,
        GUI_MSG_RESUME_TV,
        GUI_MSG_SEEK_FILE_END,
        GUI_MSG_REFRESH,
        GUI_MSG_ASKYESNO,
        GUI_MSG_NEW_LINE_ENTERED,
        GUI_MSG_FILE_DOWNLOADED,
        GUI_MSG_FILE_DOWNLOADING,
        GUI_MSG_USER,
        GUI_MSG_MSN_MESSAGE,
        GUI_MSG_MSN_STATUS_MESSAGE,
        GUI_MSG_MSN_CLOSECONVERSATION,
        GUI_MSG_ITEM_FOCUS_CHANGED,
				GUI_MSG_PLAY_ITEM,
				GUI_MSG_RECORDER_VIEW_CHANNEL,
				GUI_MSG_RECORDER_STOP_VIEWING,
				GUI_MSG_GOTO_WINDOW,
				GUI_MSG_RECORDER_TUNE_RADIO,
				GUI_MSG_RECORDER_STOP_RADIO,
				GUI_MSG_VOLUME_INSERTED,
				GUI_MSG_VOLUME_REMOVED,
				GUI_MSG_AUTOPLAY_VOLUME,
				GUI_MSG_SHOW_DIRECTORY,
				GUI_MSG_SHOW_MESSAGE,
				GUI_MSG_HIDE_MESSAGE,
				GUI_MSG_NOTIFY,
				GUI_MSG_RECORDER_STOP_TV,
				GUI_MSG_CLICKED_UP,
				GUI_MSG_CLICKED_DOWN,
				GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING,
				GUI_MSG_NOTIFY_TV_PROGRAM,
				GUI_MSG_RESTART_REMOTE_CONTROLS,
				GetList,
				GUI_MSG_SEEK_POSITION,
				GUI_MSG_PLAYER_POSITION_CHANGED
		};

		private string 				m_strLabel="";
		private string 				m_strLabel2="";
		private string 				m_strLabel3="";
		private string 				m_strLabel4="";
		private int 					m_dwSenderControlID=0;
		private int 					m_dwTargetControlID=0;
		private MessageType 	m_dwMessage=0;
		private object 			  m_object=null;
		private object 			  m_object2=null;
		private int 					m_dwParam1=0;
    private int 					m_dwParam2=0;
    private int 					m_dwParam3=0;
    private int 					m_dwParam4=0;
    private int           m_dwTargetWindowId=0;
    private bool          m_bSendTargetWindow=false;


		/// <summary>
		/// Get/Set the ID of the target Control.
		/// </summary>
		public int 	TargetControlId
		{
			get { return m_dwTargetControlID;}
			set { m_dwTargetControlID=value;}
		}

    /// <summary>
    /// Indicates if message should be send to the target window directly
    /// </summary>
    public bool 	SendToTargetWindow
    {
      get { return m_bSendTargetWindow;}
      set { m_bSendTargetWindow=value;}
    }

		/// <summary>
		/// Get/set the messagetype.
		/// </summary>
		public MessageType Message
		{
			get { return m_dwMessage;}
			set { m_dwMessage=value;}
		}
		
		/// <summary>
		/// Get/Set an object. This can be used to include an arbitrary object in the message.
		/// </summary>
		public object Object
		{
			get { return m_object;}
			set { m_object=value;}
		}
		public object Object2
		{
			get { return m_object2;}
			set { m_object2=value;}
		}

		/// <summary>
		/// Get/Set the first parameter.
		/// </summary>
		public int  Param1
		{
			get { return m_dwParam1;}
			set { m_dwParam1=value;}
		}

		/// <summary>
		/// Get/Set the second parameter.
		/// </summary>
		public int  Param2
		{
			get { return m_dwParam2;}
			set { m_dwParam2=value;}
		}
    public int  Param3
    {
      get { return m_dwParam3;}
      set { m_dwParam3=value;}
    }
    public int  Param4
    {
      get { return m_dwParam4;}
      set { m_dwParam4=value;}
    }

		/// <summary>
		/// Get/Set the SenderID
		/// </summary>
		public int  SenderControlId
		{
			get { return m_dwSenderControlID;}
			set { m_dwSenderControlID=value;}
		}

    /// <summary>
    /// Get/Set the WindowID
    /// </summary>
    public int  TargetWindowId
    {
      get { return m_dwTargetWindowId;}
      set { m_dwTargetWindowId=value;}
    }

		/// <summary>
		/// Get/Set a text message that is included in the message.
		/// </summary>
		public string Label
		{
			get { return m_strLabel;}
			set { m_strLabel=value;}
		}


    /// <summary>
    /// Get/Set a text message that is included in the message.
    /// </summary>
    public string Label2
    {
      get { return m_strLabel2;}
      set { m_strLabel2=value;}
		}


		/// <summary>
		/// Get/Set a text message that is included in the message.
		/// </summary>
		public string Label3
		{
			get { return m_strLabel3;}
			set { m_strLabel3=value;}
		}


		/// <summary>
		/// Get/Set a text message that is included in the message.
		/// </summary>
		public string Label4
		{
			get { return m_strLabel4;}
			set { m_strLabel4=value;}
		}

		/// <summary>
		/// The (empty) contstructor of the GUIMessage class.
		/// </summary>
		public GUIMessage()
		{
		}
		
		/// <summary>
		/// The constructor of the GUIMessage class.
		/// </summary>
    /// <param name="dwMsg">The MessageType.</param>
    /// <param name="iWindowId">The ID of the target window.</param>
		/// <param name="dwSenderId">The ID of the control sending the message.</param>
		/// <param name="dwControlID">The ID of the target control.</param>
		/// <param name="dwParam1">The first parameter.</param>
		/// <param name="dwParam2">The second parameter.</param>
		/// <param name="lpVoid">An object containing data that is carried in the message.</param>
		public GUIMessage(MessageType dwMsg, int iWindowId, int dwSenderId, int dwControlID, int dwParam1, int dwParam2, object lpVoid)
		{
      m_dwTargetWindowId=iWindowId;
			m_dwMessage=dwMsg;
			m_dwSenderControlID=dwSenderId;
			m_dwTargetControlID=dwControlID;
			m_dwParam1=dwParam1;
			m_dwParam2=dwParam2;
			m_object=lpVoid;
		}
	}
}
