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

using System.Windows.Forms;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class containing the different Actions that can be performed.
  /// </summary>
  public class Action
  {
    #region enums

    public enum ActionType
    {
      ACTION_INVALID = 0,
      ACTION_MOVE_LEFT = 1,
      ACTION_MOVE_RIGHT = 2,
      ACTION_MOVE_UP = 3,
      ACTION_MOVE_DOWN = 4,
      ACTION_PAGE_UP = 5,
      ACTION_PAGE_DOWN = 6,
      ACTION_SELECT_ITEM = 7,
      ACTION_HIGHLIGHT_ITEM = 8,
      ACTION_PARENT_DIR = 9,
      ACTION_PREVIOUS_MENU = 10,
      ACTION_SHOW_INFO = 11,
      ACTION_PAUSE = 12,
      ACTION_STOP = 13,
      ACTION_NEXT_ITEM = 14,
      ACTION_PREV_ITEM = 15,
      ACTION_FORWARD = 16,
      ACTION_REWIND = 17,
      ACTION_SHOW_GUI = 18, // toggle between GUI and movie or GUI and visualisation. 
      ACTION_ASPECT_RATIO = 19,
      // toggle between zoom/stretch/normal during a movie. Can b used in VideoFullScreen.xml window id=2005 
      ACTION_STEP_FORWARD = 20, // seek +1% in the movie. Can b used in VideoFullScreen.xml window id=2005 
      ACTION_STEP_BACK = 21, // seek -1% in the movie. Can b used in VideoFullScreen.xml window id=2005 
      ACTION_BIG_STEP_FORWARD = 22, // seek +10% in the movie. Can b used in VideoFullScreen.xml window id=2005 
      ACTION_BIG_STEP_BACK = 23, // seek -10% in the movie. Can b used in VideoFullScreen.xml window id=2005 
      ACTION_SHOW_OSD = 24, // show/hide OSD. Can b used in VideoFullScreen.xml window id=2005 
      ACTION_SHOW_SUBTITLES = 25, // turn subtitles on/off. Can b used in VideoFullScreen.xml window id=2005 
      ACTION_NEXT_AUDIO = 26, // switch to next audio stream. Can b used in VideoFullScreen.xml window id=2005 
      ACTION_SHOW_CODEC = 27,
      // show information about file. Can b used in VideoFullScreen.xml window id=2005 and in slideshow.xml window id=2007
      ACTION_NEXT_PICTURE = 28, // show next picture of slideshow. Can b used in slideshow.xml window id=2007
      ACTION_PREV_PICTURE = 29, // show previous picture of slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_OUT = 30, // zoom in picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_IN = 31, // zoom out picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_TOGGLE_SOURCE_DEST = 32,
      // used to toggle between source view and destination view. Can be used in myfiles.xml window id=3 
      ACTION_SHOW_PLAYLIST = 33,
      // used to toggle between current view and playlist view. Can b used in all mymusic xml files
      ACTION_QUEUE_ITEM = 34, // used to queue a item to the playlist. Can b used in all mymusic xml files
      ACTION_REMOVE_ITEM = 35, // not used anymore
      ACTION_SHOW_FULLSCREEN = 36, // not used anymore
      ACTION_ZOOM_LEVEL_NORMAL = 37, // zoom 1x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_1 = 38, // zoom 2x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_2 = 39, // zoom 3x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_3 = 40, // zoom 4x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_4 = 41, // zoom 5x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_5 = 42, // zoom 6x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_6 = 43, // zoom 7x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_7 = 44, // zoom 8x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_8 = 45, // zoom 9x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_ZOOM_LEVEL_9 = 46, // zoom 10x picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_CALIBRATE_SWAP_ARROWS = 47, // select next arrow. Can b used in: settingsScreenCalibration.xml windowid=11
      ACTION_CALIBRATE_RESET = 48,
      // reset calibration to defaults. Can b used in: settingsScreenCalibration.xml windowid=11/settingsUICalibration.xml windowid=10
      ACTION_ANALOG_MOVE = 49,
      // analog thumbstick move. Can b used in: slideshow.xml window id=2007/settingsScreenCalibration.xml windowid=11/settingsUICalibration.xml windowid=10
      ACTION_ROTATE_PICTURE = 50, // rotate current picture during slideshow. Can b used in slideshow.xml window id=2007
      ACTION_CLOSE_DIALOG = 51, // action for closing the dialog. Can b used in any dialog
      ACTION_SUBTITLE_DELAY_MIN = 52,
      // Decrease subtitle/movie Delay.  Can b used in VideoFullScreen.xml window id=2005
      ACTION_SUBTITLE_DELAY_PLUS = 53,
      // Increase subtitle/movie Delay.  Can b used in VideoFullScreen.xml window id=2005
      ACTION_AUDIO_DELAY_MIN = 54, // Increase avsync delay.  Can b used in VideoFullScreen.xml window id=2005
      ACTION_AUDIO_DELAY_PLUS = 55, // Decrease avsync delay.  Can b used in VideoFullScreen.xml window id=2005
      ACTION_AUDIO_NEXT_LANGUAGE = 56,
      // Select next language in movie.  Can b used in VideoFullScreen.xml window id=2005
      ACTION_CHANGE_RESOLUTION = 57,
      // switch 2 next resolution. Can b used during screen calibration settingsScreenCalibration.xml windowid=11
      REMOTE_0 = 58, // remote keys 0-9. are used by multiple windows
      REMOTE_1 = 59, // for example in VideoFullScreen.xml window id=2005 you can
      REMOTE_2 = 60, // enter time (mmss) to jump to particular point in the movie
      REMOTE_3 = 61,
      REMOTE_4 = 62, // with spincontrols you can enter 3digit number to quickly set
      REMOTE_5 = 63, // spincontrol to desired value
      REMOTE_6 = 64,
      REMOTE_7 = 65,
      REMOTE_8 = 66,
      REMOTE_9 = 67,
      ACTION_PLAY = 68,
      // Play current movie. Unpauses movie and sets playspeed to 1x.  Can b used in VideoFullScreen.xml window id=2005
      ACTION_OSD_SHOW_LEFT = 69, // Move left in OSD. Can b used in VideoFullScreen.xml window id=2005
      ACTION_OSD_SHOW_RIGHT = 70, // Move right in OSD. Can b used in VideoFullScreen.xml window id=2005
      ACTION_OSD_SHOW_UP = 71, // Move up in OSD. Can b used in VideoFullScreen.xml window id=2005
      ACTION_OSD_SHOW_DOWN = 72, // Move down in OSD. Can b used in VideoFullScreen.xml window id=2005
      ACTION_OSD_SHOW_SELECT = 73, // toggle/select option in OSD. Can b used in VideoFullScreen.xml window id=2005
      ACTION_OSD_SHOW_VALUE_PLUS = 74,
      // increase value of current option in OSD. Can b used in VideoFullScreen.xml window id=2005
      ACTION_OSD_SHOW_VALUE_MIN = 75,
      // decrease value of current option in OSD. Can b used in VideoFullScreen.xml window id=2005
      ACTION_SMALL_STEP_BACK = 76,
      // jumps a few seconds back during playback of movie. Can b used in VideoFullScreen.xml window id=2005
      ACTION_MUSIC_FORWARD = 77, // FF in current song. global action, can be used anywhere
      ACTION_MUSIC_REWIND = 78, // RW in current song. global action, can be used anywhere
      ACTION_MUSIC_PLAY = 79,
      // Play current song. Unpauses song and sets playspeed to 1x. global action, can be used anywhere
      ACTION_DELETE_ITEM = 80,
      // delete current selected item. Can be used in myfiles.xml window id=3 and in myVideoTitle.xml window id=25
      ACTION_COPY_ITEM = 81, // copy current selected item. Can be used in myfiles.xml window id=3 
      ACTION_MOVE_ITEM = 82, // move current selected item. Can be used in myfiles.xml window id=3
      ACTION_SHOW_MPLAYER_OSD = 83, // toggles mplayers OSD. Can be used in Videofullscreen.xml window id=2005
      ACTION_OSD_HIDESUBMENU = 84, // removes an OSD sub menu. Can be used in VideoOSD.xml window id=2901
      ACTION_TAKE_SCREENSHOT = 85, // take a screenshot
      ACTION_INCREASE_TIMEBLOCK = 86,
      ACTION_DECREASE_TIMEBLOCK = 87,
      ACTION_DEFAULT_TIMEBLOCK = 88,
      ACTION_RECORD = 89,
      ACTION_DVD_MENU = 90,
      ACTION_NEXT_CHAPTER = 91,
      ACTION_PREV_CHAPTER = 92,
      ACTION_KEY_PRESSED = 93,
      ACTION_PREV_CHANNEL = 94,
      ACTION_NEXT_CHANNEL = 95,
      ACTION_TVGUIDE_RESET = 96,
      ACTION_EXIT = 97,
      ACTION_REBOOT = 98,
      ACTION_SHUTDOWN = 99,
      ACTION_EJECTCD = 100,
      ACTION_BACKGROUND_TOGGLE = 101,
      ACTION_VOLUME_DOWN = 102,
      ACTION_VOLUME_UP = 103,
      ACTION_TOGGLE_WINDOWED_FULLSCREEN = 104,
      ACTION_PAUSE_PICTURE = 105,
      ACTION_CONTEXT_MENU = 106,
      ACTION_SHOW_MSN_WINDOW = 107,
      ACTION_SHOW_MSN_OSD = 108,
      ACTION_HOME = 109, // home
      ACTION_END = 110, // end
      ACTION_LAST_VIEWED_CHANNEL = 111, // switches TV to the last viewed channel / mPod
      ACTION_IMPORT_TRACK = 112,
      ACTION_IMPORT_DISC = 113,
      ACTION_CANCEL_IMPORT = 114,
      ACTION_SWITCH_HOME = 115, // allow switching between regular / basic home
      ACTION_MOVE_SELECTED_ITEM_UP = 116, // move selected playlist item up
      ACTION_MOVE_SELECTED_ITEM_DOWN = 117, // move selected playlist item down
      ACTION_DELETE_SELECTED_ITEM = 118, // delete selected playlist item
      ACTION_NEXT_SUBTITLE = 119, // switches to next available subtitle
      ACTION_SHOW_ACTIONMENU = 120, // show the action menue for the current window
      ACTION_TOGGLE_SMS_INPUT = 121, // Toggle SMS / alpha keyboard
      ACTION_AUTOZAP = 122, // Start autozapping in TV mode
      ACTION_MPRESTORE = 123,
      ACTION_MOUSE_MOVE = 2000,
      ACTION_MOUSE_CLICK = 2001,
      ACTION_MOUSE_DOUBLECLICK = 2002,
      ACTION_PREV_BOOKMARK = 140,
      ACTION_NEXT_BOOKMARK = 141,
      ACTION_REMOTE_RED_BUTTON = 9975,
      ACTION_REMOTE_GREEN_BUTTON = 9976,
      ACTION_REMOTE_YELLOW_BUTTON = 9977,
      ACTION_REMOTE_BLUE_BUTTON = 9978,
      ACTION_REMOTE_SUBPAGE_UP = 9979,
      ACTION_REMOTE_SUBPAGE_DOWN = 9980,
      ACTION_SHOW_VOLUME = 9981,
      ACTION_VOLUME_MUTE = 9982,
      ACTION_SHOW_CURRENT_TV_INFO = 9983,
      ACTION_AUTOCROP = 9884, // If the AutoCropper is used, request new crop detection          
      ACTION_TOGGLE_AUTOCROP = 9885, // Switch AutoCropper operating mode: off/auto/on request
      ACTION_TOGGLE_MUSIC_GAP = 9886, // Toggles Music Playback Normal -> Gapless -> Crossfade -> Normal    
      ACTION_NEXT_TELETEXTPAGE = 9984, // Switch to next teletext page window id=7701 || 7700
      ACTION_PREV_TELETEXTPAGE = 9985, // Switch to previous teletext page window id=7701 || 7700
      ACTION_SWITCH_TELETEXT_HIDDEN = 9986, // Switch on/off the hidden mode in teletext window id=7701 || 7700
      ACTION_SWITCH_TELETEXT_TRANSPARENT = 9987, // Switch on/off the hidden mode in teletext window id=7701 || 7700
      ACTION_SHOW_INDEXPAGE = 9988, // Go to index page window id=7701 | 7700
      ACTION_SKIN_NEXT = 9989, // used for the changeskin plugin.
      ACTION_SKIN_PREVIOUS = 9990, // used for the changeskin plugin.
      ACTION_TVGUIDE_INCREASE_DAY = 9991,
      ACTION_TVGUIDE_DECREASE_DAY = 9992,
      ACTION_TVGUIDE_NEXT_GROUP = 9995, // switch to the next tv group in guide
      ACTION_TVGUIDE_PREV_GROUP = 9996  // switch to the previous tv group in guide
    } ;

    #endregion

    #region variables

    public ActionType wID = 0;
    public float fAmount1 = 0.0f;
    public float fAmount2 = 0.0f;
    public Key m_key = null;
    public MouseButtons m_mouseButtons = MouseButtons.None;
    public string m_SoundFileName = "";

    #endregion

    #region ctor/dtor

    /// <summary>
    /// The (emtpy) constructur of the Action class.
    /// </summary>
    public Action()
    {
    }

    /// <summary>
    /// Creates an action.
    /// </summary>
    /// <param name="id">The action type.</param>
    /// <param name="f1">First parameter (E.g., x coordintate of an ACTION_MOUSE_MOVE).</param>
    /// <param name="f2">Second parameter (E.g., y coordintate of an ACTION_MOUSE_MOVE).</param>
    public Action(ActionType id, float f1, float f2)
    {
      wID = id;
      fAmount1 = f1;
      fAmount2 = f2;
    }

    /// <summary>
    /// Creates an action.
    /// </summary>
    /// <param name="key">The key that caused the action. (E.g., a key press)</param>
    /// <param name="id">The action type</param>
    /// <param name="f1">First parameter.</param>
    /// <param name="f2">Second parameter.</param>
    public Action(Key key, ActionType id, float f1, float f2)
    {
      // TODO: No key action requires the additional parameters. Are they still needed?
      m_key = key;
      wID = id;
      fAmount1 = f1;
      fAmount2 = f2;
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets and sets the state of the MouseButtons when the action occured.
    /// </summary>
    public MouseButtons MouseButton
    {
      get { return m_mouseButtons; }
      set { m_mouseButtons = value; }
    }

    /// <summary>
    /// Get/set the filename of a soundfile that is contained within the action.
    /// </summary>
    public string SoundFileName
    {
      get { return m_SoundFileName; }
      set { m_SoundFileName = value; }
    }

    public bool IsUserAction()
    {
      return true; //all current actions are user actions
    }

    /// <summary>
    /// This method tells if the action should remove the screensaver and start showing the GUI again
    /// </summary>
    public bool ShouldDisableScreenSaver
    {
      get
      {
        if (wID == ActionType.ACTION_NEXT_ITEM)
        {
          return false;
        }
        if (wID == ActionType.ACTION_PREV_ITEM)
        {
          return false;
        }
        if (wID == ActionType.ACTION_VOLUME_MUTE)
        {
          return false;
        }
        if (wID == ActionType.ACTION_SHOW_VOLUME)
        {
          return false;
        }
        if (wID == ActionType.ACTION_VOLUME_DOWN)
        {
          return false;
        }
        if (wID == ActionType.ACTION_VOLUME_UP)
        {
          return false;
        }
        return true;
      }
    }

    #endregion
  }
}