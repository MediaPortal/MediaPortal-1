#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
      GUI_MSG_GETLIST,
      GUI_MSG_SEEK_POSITION,
      GUI_MSG_PLAYER_POSITION_CHANGED,
      GUI_MSG_RECORD,
      GUI_MSG_GETFOCUS,
      GUI_MSG_WRONG_PASSWORD,
      GUI_MSG_RECORDER_STOP,
      GUI_MSG_RECORDER_START,
      GUI_MSG_DISABLEGUIDEREFRESH,
      GUI_MSG_ENABLEGUIDEREFRESH
    };

    private string _label = "";
    private string _label2 = "";
    private string _label3 = "";
    private string _label4 = "";
    private int _senderControlId = 0;
    private int _targetControlId = 0;
    private MessageType _messageType = 0;
    private object _object = null;
    private object _object2 = null;
    private int _parameter1 = 0;
    private int _parameter2 = 0;
    private int _parameter3 = 0;
    private int _parameter4 = 0;
    private int _targetWindowId = 0;
    private bool _sendToTargetWindow = false;


    /// <summary>
    /// Get/Set the ID of the target Control.
    /// </summary>
    public int TargetControlId
    {
      get { return _targetControlId; }
      set { _targetControlId = value; }
    }

    /// <summary>
    /// Indicates if message should be send to the target window directly
    /// </summary>
    public bool SendToTargetWindow
    {
      get { return _sendToTargetWindow; }
      set { _sendToTargetWindow = value; }
    }

    /// <summary>
    /// Get/set the messagetype.
    /// </summary>
    public MessageType Message
    {
      get { return _messageType; }
      set { _messageType = value; }
    }

    /// <summary>
    /// Get/Set an object. This can be used to include an arbitrary object in the message.
    /// </summary>
    public object Object
    {
      get { return _object; }
      set { _object = value; }
    }
    public object Object2
    {
      get { return _object2; }
      set { _object2 = value; }
    }

    /// <summary>
    /// Get/Set the first parameter.
    /// </summary>
    public int Param1
    {
      get { return _parameter1; }
      set { _parameter1 = value; }
    }

    /// <summary>
    /// Get/Set the second parameter.
    /// </summary>
    public int Param2
    {
      get { return _parameter2; }
      set { _parameter2 = value; }
    }
    public int Param3
    {
      get { return _parameter3; }
      set { _parameter3 = value; }
    }
    public int Param4
    {
      get { return _parameter4; }
      set { _parameter4 = value; }
    }

    /// <summary>
    /// Get/Set the SenderID
    /// </summary>
    public int SenderControlId
    {
      get { return _senderControlId; }
      set { _senderControlId = value; }
    }

    /// <summary>
    /// Get/Set the WindowID
    /// </summary>
    public int TargetWindowId
    {
      get { return _targetWindowId; }
      set { _targetWindowId = value; }
    }

    /// <summary>
    /// Get/Set a text message that is included in the message.
    /// </summary>
    public string Label
    {
      get { return _label; }
      set { _label = value; }
    }


    /// <summary>
    /// Get/Set a text message that is included in the message.
    /// </summary>
    public string Label2
    {
      get { return _label2; }
      set { _label2 = value; }
    }


    /// <summary>
    /// Get/Set a text message that is included in the message.
    /// </summary>
    public string Label3
    {
      get { return _label3; }
      set { _label3 = value; }
    }


    /// <summary>
    /// Get/Set a text message that is included in the message.
    /// </summary>
    public string Label4
    {
      get { return _label4; }
      set { _label4 = value; }
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
      _targetWindowId = iWindowId;
      _messageType = dwMsg;
      _senderControlId = dwSenderId;
      _targetControlId = dwControlID;
      _parameter1 = dwParam1;
      _parameter2 = dwParam2;
      _object = lpVoid;
    }
  }
}
