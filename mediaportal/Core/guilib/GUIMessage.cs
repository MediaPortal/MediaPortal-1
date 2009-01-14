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
      GUI_MSG_WINDOW_INIT = 0,
      GUI_MSG_WINDOW_DEINIT = 1,
      GUI_MSG_SETFOCUS = 2,
      GUI_MSG_LOSTFOCUS = 3,
      GUI_MSG_CLICKED = 4,
      GUI_MSG_VISIBLE = 5,
      GUI_MSG_HIDDEN = 6,
      GUI_MSG_ENABLED = 7,
      GUI_MSG_DISABLED = 8,
      GUI_MSG_SELECTED = 9,
      GUI_MSG_DESELECTED = 10,
      GUI_MSG_LABEL_ADD = 11,
      GUI_MSG_LABEL_SET = 12,
      GUI_MSG_LABEL_RESET = 13,
      GUI_MSG_ITEM_SELECTED = 14,
      GUI_MSG_ITEM_SELECT = 15,
      GUI_MSG_ITEM_FOCUS = 16,
      GUI_MSG_LABEL2_SET = 17,
      GUI_MSG_SHOWRANGE = 18,
      GUI_MSG_GET_ITEM = 19,
      GUI_MSG_START_SLIDESHOW = 20,
      GUI_MSG_ITEMS = 21,
      GUI_MSG_GET_SELECTED_ITEM = 22,
      GUI_MSG_PLAYBACK_STOPPED = 23,
      GUI_MSG_PLAYLIST_CHANGED = 24,
      GUI_MSG_PLAYBACK_ENDED = 25,
      GUI_MSG_PLAYBACK_STARTED = 26,
      GUI_MSG_PERCENTAGE_CHANGED = 27,
      GUI_MSG_PLAY_FILE = 28,
      GUI_MSG_STOP_FILE = 29,
      GUI_MSG_SEEK_FILE_PERCENTAGE = 30,
      GUI_MSG_RECORDER_ALLOC_CARD = 31,
      GUI_MSG_RECORDER_FREE_CARD = 32,
      GUI_MSG_RECORDER_STOP_TIMESHIFT = 33,
      GUI_MSG_TUNE_EXTERNAL_CHANNEL = 34,
      GUI_MSG_GET_STRING = 35,
      GUI_MSG_GET_PASSWORD = 36,
      GUI_MSG_SWITCH_FULL_WINDOWED = 37,
      GUI_MSG_PLAY_AUDIO_CD = 38,
      GUI_MSG_CD_REMOVED = 39,
      GUI_MSG_CD_INSERTED = 40,
      GUI_MSG_PLAYING_10SEC = 41, //file is playing 10 sec
      GUI_MSG_PLAY_RADIO_STATION = 42,
      GUI_MSG_SHOW_WARNING = 43,
      GUI_MSG_RESUME_TV = 44,
      GUI_MSG_SEEK_FILE_END = 45,
      GUI_MSG_REFRESH = 46,
      GUI_MSG_ASKYESNO = 47,
      GUI_MSG_NEW_LINE_ENTERED = 48,
      GUI_MSG_FILE_DOWNLOADED = 49,
      GUI_MSG_FILE_DOWNLOADING = 50,
      GUI_MSG_USER = 51,
      GUI_MSG_MSN_MESSAGE = 52,
      GUI_MSG_MSN_STATUS_MESSAGE = 53,
      GUI_MSG_MSN_CLOSECONVERSATION = 54,
      GUI_MSG_ITEM_FOCUS_CHANGED = 55,
      GUI_MSG_PLAY_ITEM = 56,
      GUI_MSG_RECORDER_VIEW_CHANNEL = 57,
      GUI_MSG_RECORDER_STOP_VIEWING = 58,
      GUI_MSG_GOTO_WINDOW = 59,
      GUI_MSG_RECORDER_TUNE_RADIO = 60,
      GUI_MSG_RECORDER_STOP_RADIO = 61,
      GUI_MSG_VOLUME_INSERTED = 62,
      GUI_MSG_VOLUME_REMOVED = 63,
      GUI_MSG_AUTOPLAY_VOLUME = 64,
      GUI_MSG_SHOW_DIRECTORY = 65,
      GUI_MSG_SHOW_MESSAGE = 66,
      GUI_MSG_HIDE_MESSAGE = 67,
      GUI_MSG_NOTIFY = 68,
      GUI_MSG_RECORDER_STOP_TV = 69,
      GUI_MSG_CLICKED_UP = 70,
      GUI_MSG_CLICKED_DOWN = 71,
      GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING = 72,
      GUI_MSG_NOTIFY_TV_PROGRAM = 73,
      GUI_MSG_RESTART_REMOTE_CONTROLS = 74,
      GUI_MSG_GETLIST = 75,
      GUI_MSG_SEEK_POSITION = 76,
      GUI_MSG_PLAYER_POSITION_CHANGED = 77,
      GUI_MSG_RECORD = 78,
      GUI_MSG_GETFOCUS = 79,
      GUI_MSG_WRONG_PASSWORD = 80,
      GUI_MSG_RECORDER_STOP = 81,
      GUI_MSG_RECORDER_START = 82,
      GUI_MSG_DISABLEGUIDEREFRESH = 83,
      GUI_MSG_ENABLEGUIDEREFRESH = 84,
      GUI_MSG_PLANESCENE_CROP = 85,
      GUI_MSG_PLAYBACK_CROSSFADING = 86, //SV Added by SteveV 2006-09-07
      GUI_MSG_SHOW_BALLONTIP = 87, // rtv
      GUI_MSG_SHOW_BALLONTIP_SONGCHANGE = 88, // rtv
      GUI_MSG_STOP_SERVER_TIMESHIFTING = 89, //joboehl
      GUI_MSG_AUDIOVOLUME_CHANGED = 90, //joboehl 
      GUI_MSG_CALLBACK = 91, // dero: callback in MPMain thread
      GUI_MSG_BLURAY_DISK_INSERTED = 92,
      GUI_MSG_HDDVD_DISK_INSERTED = 93
    } ;

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
    public GUIMessage(MessageType dwMsg, int iWindowId, int dwSenderId, int dwControlID, int dwParam1, int dwParam2,
                      object lpVoid)
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