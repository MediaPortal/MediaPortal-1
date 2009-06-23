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
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  public abstract class VirtualKeyboard : GUIWindow, IRenderLayer
  {
    #region constants

    public const int GAP_WIDTH = 0;
    public const int GAP2_WIDTH = 4;
    public int MODEKEY_WIDTH = 110;
    public const int KEY_INSET = 1;

    // How often (per second) the caret blinks
    private const float fCARET_BLINK_RATE = 1.0f;

    // During the blink period, the amount the caret is visible. 0.5 equals
    // half the time, 0.75 equals 3/4ths of the time, etc.
    public const float fCARET_ON_RATIO = 0.75f;

    // Text colors for keys
    public const long COLOR_SEARCHTEXT = 0xff000000; // black (0xff10e010)
    public const long COLOR_HIGHLIGHT = 0xff00ff00; // green
    public const long COLOR_PRESSED = 0xff808080; // gray
    public const long COLOR_NORMAL = 0xff000000; // black
    public const long COLOR_DISABLED = 0xffffffff; // white
    public const long COLOR_HELPTEXT = 0xffffffff; // white
    public const long COLOR_FONT_DISABLED = 0xff808080; // gray
    public const long COLOR_INVISIBLE = 0xff0000ff; // blue
    public const long COLOR_RED = 0xffff0000; // red
    // Font sizes
    public const int FONTSIZE_BUTTONCHARS = 24;
    public const int FONTSIZE_BUTTONSTRINGS = 18;
    public const int FONTSIZE_SEARCHTEXT = 20;

    // Controller repeat values
    public const float fINITIAL_REPEAT = 0.333f; // 333 mS recommended for first repeat
    public const float fSTD_REPEAT = 0.085f; // 85 mS recommended for repeat rate

    // Maximum number of characters in string
    protected int _max_chars = 64;
    public int MAX_CHARS {
      get { return _max_chars; }
    }

    // Width of text box
    public float fTEXTBOX_WIDTH = 576.0f - 64.0f - 4.0f - 4.0f - 10.0f;
    public const int KEY_WIDTH = 34; // width of std key in pixels

    public bool _usingKeyboard;
    public char _currentKeyb = (char) 0;
    public char _previousKey = (char) 0;
    public DateTime _timerKey = DateTime.Now;

    #endregion

    #region enums

    public enum SearchKinds
    {
      SEARCH_STARTS_WITH = 0,
      SEARCH_CONTAINS,
      SEARCH_ENDS_WITH,
      SEARCH_IS
    }

    public enum KeyboardTypes
    {
      TYPE_ALPHABET = 0,
      TYPE_SYMBOLS,
      TYPE_ACCENTS,

      TYPE_HIRAGANA,
      TYPE_KATAKANA,
      TYPE_ANS,

      TYPE_MAX
    } ;

    public enum State
    {
      STATE_BACK, // Main menu
      STATE_KEYBOARD, // Keyboard display
      STATE_MAX
    } ;

    public enum Event
    {
      EV_NULL, // No events
      EV_A_BUTTON, // A button
      EV_START_BUTTON, // Start button
      EV_B_BUTTON, // B button
      EV_BACK_BUTTON, // Back button
      EV_X_BUTTON, // X button
      EV_Y_BUTTON, // Y button
      EV_WHITE_BUTTON, // White button
      EV_BLACK_BUTTON, // Black button
      EV_LEFT_BUTTON, // Left trigger
      EV_RIGHT_BUTTON, // Right trigger
      EV_UP, // Up Dpad or left joy
      EV_DOWN, // Down Dpad or left joy
      EV_LEFT, // Left Dpad or left joy
      EV_RIGHT, // Right Dpad or left joy

      EVENT_MAX
    } ;

    public enum Xkey
    {
      XK_NULL = 0,

      XK_SPACE = ' ',
      XK_LBRACK = '[',
      XK_RBRACK = ']',
      XK_LBRACE = '{',
      XK_RBRACE = '}',
      XK_LPAREN = '(',
      XK_RPAREN = ')',
      XK_FSLASH = '/',
      XK_BSLASH = '\\',
      XK_LT = '<',
      XK_GT = '>',
      XK_AT = '@',
      XK_SEMI = ';',
      XK_COLON = ':',
      XK_QUOTE = '\'',
      XK_DQUOTE = '\"',
      XK_AMPER = '&',
      XK_STAR = '*',
      XK_QMARK = '?',
      XK_COMMA = ',',
      XK_PERIOD = '.',
      XK_DASH = '-',
      XK_UNDERS = '_',
      XK_PLUS = '+',
      XK_EQUAL = '=',
      XK_DOLLAR = '$',
      XK_PERCENT = '%',
      XK_CARET = '^',
      XK_TILDE = '~',
      XK_APOS = '`',
      XK_EXCL = '!',
      XK_VERT = '|',
      XK_NSIGN = '#',

      // Numbers
      XK_0 = '0',
      XK_1,
      XK_2,
      XK_3,
      XK_4,
      XK_5,
      XK_6,
      XK_7,
      XK_8,
      XK_9,

      // Letters
      XK_A = 'A',
      XK_B,
      XK_C,
      XK_D,
      XK_E,
      XK_F,
      XK_G,
      XK_H,
      XK_I,
      XK_J,
      XK_K,
      XK_L,
      XK_M,
      XK_N,
      XK_O,
      XK_P,
      XK_Q,
      XK_R,
      XK_S,
      XK_T,
      XK_U,
      XK_V,
      XK_W,
      XK_X,
      XK_Y,
      XK_Z,

      // Accented characters and other special characters

      XK_INVERTED_EXCL = 0xA1, // ¡
      XK_CENT_SIGN = 0xA2, // ¢
      XK_POUND_SIGN = 0xA3, // £
      XK_YEN_SIGN = 0xA5, // ¥
      XK_COPYRIGHT_SIGN = 0xA9, // ©
      XK_LT_DBL_ANGLE_QUOTE = 0xAB, // <<
      XK_REGISTERED_SIGN = 0xAE, // ®
      XK_SUPERSCRIPT_TWO = 0xB2, // ²
      XK_SUPERSCRIPT_THREE = 0xB3, // ³
      XK_ACUTE_ACCENT = 0xB4, // ´
      XK_MICRO_SIGN = 0xB5, // µ
      XK_SUPERSCRIPT_ONE = 0xB9, // ¹
      XK_RT_DBL_ANGLE_QUOTE = 0xBB, // >>
      XK_INVERTED_QMARK = 0xBF, // ¿
      XK_CAP_A_GRAVE = 0xC0, // À
      XK_CAP_A_ACUTE = 0xC1, // Á
      XK_CAP_A_CIRCUMFLEX = 0xC2, // Â
      XK_CAP_A_TILDE = 0xC3, // Ã
      XK_CAP_A_DIAERESIS = 0xC4, // Ä
      XK_CAP_A_RING = 0xC5, // Å
      XK_CAP_AE = 0xC6, // Æ
      XK_CAP_C_CEDILLA = 0xC7, // Ç
      XK_CAP_E_GRAVE = 0xC8, // È
      XK_CAP_E_ACUTE = 0xC9, // É
      XK_CAP_E_CIRCUMFLEX = 0xCA, // Ê
      XK_CAP_E_DIAERESIS = 0xCB, // Ë
      XK_CAP_I_GRAVE = 0xCC, // Ì
      XK_CAP_I_ACUTE = 0xCD, // Í
      XK_CAP_I_CIRCUMFLEX = 0xCE, // Î
      XK_CAP_I_DIAERESIS = 0xCF, // Ï
      XK_CAP_N_TILDE = 0xD1, // Ñ
      XK_CAP_O_GRAVE = 0xD2, // Ò
      XK_CAP_O_ACUTE = 0xD3, // Ó
      XK_CAP_O_CIRCUMFLEX = 0xD4, // Ô
      XK_CAP_O_TILDE = 0xD5, // Õ
      XK_CAP_O_DIAERESIS = 0xD6, // Ö
      XK_CAP_O_STROKE = 0xD8, // Ø
      XK_CAP_U_GRAVE = 0xD9, // Ù
      XK_CAP_U_ACUTE = 0xDA, // Ú
      XK_CAP_U_CIRCUMFLEX = 0xDB, // Û
      XK_CAP_U_DIAERESIS = 0xDC, // Ü
      XK_CAP_Y_ACUTE = 0xDD, // Ý
      XK_SM_SHARP_S = 0xDF, // ß
      XK_SM_A_GRAVE = 0xE0, // à
      XK_SM_A_ACUTE = 0xE1, // á
      XK_SM_A_CIRCUMFLEX = 0xE2, // â
      XK_SM_A_TILDE = 0xE3, // ã
      XK_SM_A_DIAERESIS = 0xE4, // ä
      XK_SM_A_RING = 0xE5, // å
      XK_SM_AE = 0xE6, // æ
      XK_SM_C_CEDILLA = 0xE7, // ç
      XK_SM_E_GRAVE = 0xE8, // è
      XK_SM_E_ACUTE = 0xE9, // é
      XK_SM_E_CIRCUMFLEX = 0xEA, // ê
      XK_SM_E_DIAERESIS = 0xEB, // ë
      XK_SM_I_GRAVE = 0xEC, // ì
      XK_SM_I_ACUTE = 0xED, // í
      XK_SM_I_CIRCUMFLEX = 0xEE, // î
      XK_SM_I_DIAERESIS = 0xEF, // ï
      XK_SM_N_TILDE = 0xF1, // ñ
      XK_SM_O_GRAVE = 0xF2, // ò
      XK_SM_O_ACUTE = 0xF3, // ó
      XK_SM_O_CIRCUMFLEX = 0xF4, // ô
      XK_SM_O_TILDE = 0xF5, // õ
      XK_SM_O_DIAERESIS = 0xF6, // ö
      XK_SM_O_STROKE = 0xF8, // ø
      XK_SM_U_GRAVE = 0xF9, // ù
      XK_SM_U_ACUTE = 0xFA, // ú
      XK_SM_U_CIRCUMFLEX = 0xFB, // û
      XK_SM_U_DIAERESIS = 0xFC, // ü
      XK_SM_Y_ACUTE = 0xFD, // ý
      XK_SM_Y_DIAERESIS = 0xFF, // ÿ

      // Unicode
      XK_CAP_Y_DIAERESIS = 0x0178, // Y umlaut
      XK_EURO_SIGN = 0x20AC, // Euro symbol
      XK_ARROWLEFT = '<', // left arrow
      XK_ARROWRIGHT = '>', // right arrow

      // Special
      XK_BACKSPACE = 0x10000, // backspace
      XK_DELETE, // delete           // !!!
      XK_SHIFT, // shift
      XK_CAPSLOCK, // caps lock
      XK_ALPHABET, // alphabet
      XK_SYMBOLS, // symbols
      XK_ACCENTS, // accents
      XK_OK, // "done"
      XK_HIRAGANA, // Hiragana
      XK_KATAKANA, // Katakana
      XK_ANS, // Alphabet/numeral/symbol

      // Special Search-Keys
      XK_SEARCH_START_WITH = 0x11000, // to search music that starts with string
      XK_SEARCH_CONTAINS, // ...contains string
      XK_SEARCH_ENDS_WITH, // ...ends with string
      XK_SEARCH_IS, // is the search text
      XK_SEARCH_ALBUM, // search for album
      XK_SEARCH_TITLE, // search for title
      XK_SEARCH_ARTIST, // search for artist
      XK_SEARCH_GENERE // search for genere
    } ;

    public enum StringID
    {
      STR_MENU_KEYBOARD_NAME,
      STR_MENU_CHOOSE_KEYBOARD,
      STR_MENU_ILLUSTRATIVE_GRAPHICS,
      STR_MENU_A_SELECT,
      STR_MENU_B_BACK,
      STR_MENU_Y_HELP,
      STR_KEY_SPACE,
      STR_KEY_BACKSPACE,
      STR_KEY_SHIFT,
      STR_KEY_CAPSLOCK,
      STR_KEY_ALPHABET,
      STR_KEY_SYMBOLS,
      STR_KEY_ACCENTS,
      STR_KEY_DONE,
      STR_HELP_SELECT,
      STR_HELP_CANCEL,
      STR_HELP_TOGGLE,
      STR_HELP_HELP,
      STR_HELP_BACKSPACE,
      STR_HELP_SPACE,
      STR_HELP_TRIGGER,

      STR_MAX,
    } ;

    #endregion

    #region variables

    public string _textEntered = "";
    public bool _capsLockTurnedOn = true;
    public bool _shiftTurnedOn;
    public State _state;
    public int _position;
    public KeyboardTypes _currentKeyboard;
    public int _currentRow;
    public int _currentKey;
    public int _lastColumn;
    public CachedTexture.Frame _keyTexture;
    public float _keyHeight;
    public int _maxRows;
    public bool _pressedEnter;
    public GUIFont _font18;
    public GUIFont _font12;
    public GUIFont _fontButtons;
    public GUIFont _fontSearchText;
    public DateTime _caretTimer = DateTime.Now;
    public bool _previousOverlayVisible = true;
    public bool _password;
    public GUIImage image;
    public bool _useSearchLayout;

    // added by Agree
    public int _searchKind; // 0=Starts with, 1=Contains, 2=Ends with
    //

    public ArrayList _keyboardList = new ArrayList(); // list of rows = keyboard

    public float SkinRatio;

    #endregion

    #region Base Dialog Variables

    private bool _isVisible;
    private int _parentWindowId;
    private GUIWindow _parentWindow;

    #endregion

    public class Key
    {
      public Xkey xKey; // virtual key code
      public int dwWidth = KEY_WIDTH; // width of the key
      public string name = ""; // name of key when vKey >= 0x10000

      public Key(Xkey key)
      {
        xKey = key;
      }

      public Key(Xkey key, int iwidth)
      {
        xKey = key;
        dwWidth = iwidth;

        // Special keys get their own names
        switch (xKey)
        {
          case Xkey.XK_SPACE:
            name = "SPACE";
            break;
          case Xkey.XK_BACKSPACE:
            name = "BKSP";
            break;
          case Xkey.XK_SHIFT:
            name = "SHIFT";
            break;
          case Xkey.XK_CAPSLOCK:
            name = "CAPS";
            break;
          case Xkey.XK_ALPHABET:
            name = "ALPHABET";
            break;
          case Xkey.XK_SYMBOLS:
            name = "SYMB";
            break;
          case Xkey.XK_ACCENTS:
            name = "ACCENTS";
            break;
          case Xkey.XK_OK:
            name = GUILocalizeStrings.Get(804);
            break;
          case Xkey.XK_SEARCH_CONTAINS:
            name = GUILocalizeStrings.Get(801);
            break;
          case Xkey.XK_SEARCH_ENDS_WITH:
            name = GUILocalizeStrings.Get(802);
            break;
          case Xkey.XK_SEARCH_START_WITH:
            name = GUILocalizeStrings.Get(800);
            break;
          case Xkey.XK_SEARCH_IS:
            name = GUILocalizeStrings.Get(803);
            break;
        }
      }
    } ;

    // lets do some event stuff
    public delegate void TextChangedEventHandler(int kindOfSearch, string evtData);

    public event TextChangedEventHandler TextChanged;
    //
    protected VirtualKeyboard()
    {
      InitializeInstance();
    }

    public override bool Init()
    {
      return true;
    }

    public bool IsConfirmed
    {
      get { return _pressedEnter; }
    }

    public bool IsSearchKeyboard
    {
      set { _useSearchLayout = value; }
    }

    protected void Initialize()
    {
      _font12 = GUIFontManager.GetFont("font12");
      _font18 = GUIFontManager.GetFont("font18");
      _fontButtons = GUIFontManager.GetFont("dingbats");
      _fontSearchText = GUIFontManager.GetFont("font14");

      int iImages = GUITextureManager.Load("keyNF.bmp", 0, 0, 0);
      if (iImages == 1)
      {
        int iTextureWidth, iTextureHeight;
        _keyTexture = GUITextureManager.GetTexture("keyNF.bmp", 0, out iTextureWidth, out iTextureHeight);
      }
      image = new GUIImage(GetID, 1, 0, 0, 10, 10, "white.bmp", 1);
      image.AllocResources();
    }

    protected void DeInitialize()
    {
      if (image != null)
      {
        image.FreeResources();
      }
      image = null;
    }

    public void Reset()
    {
      _password = false;
      _pressedEnter = false;
      _capsLockTurnedOn = false;
      _shiftTurnedOn = false;
      _state = State.STATE_KEYBOARD;
      _position = 0;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 0;
      //m_fRepeatDelay   = fINITIAL_REPEAT;
      _keyHeight = 42.0f*SkinRatio;
      _maxRows = 5;
      _position = 0;
      _textEntered = "";
      _caretTimer = DateTime.Now;

      _searchKind = (int) SearchKinds.SEARCH_CONTAINS; // default search Contains

      int y = 411;
      int x = 40;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x, ref y);

      int width = (int) (42*SkinRatio);
      GUIGraphicsContext.ScaleVertical(ref width);
      _keyHeight = width;

      width = (int) (576.0f - 64.0f - 4.0f - 4.0f - 10.0f - 80.0f);
      GUIGraphicsContext.ScaleHorizontal(ref width);
      fTEXTBOX_WIDTH = width;

      InitBoard();
    }

    public bool Password
    {
      get { return _password; }
      set { _password = value; }
    }

    protected void PageLoad()
    {
      _previousOverlayVisible = GUIGraphicsContext.Overlay;
      _pressedEnter = false;
      GUIGraphicsContext.Overlay = false;
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
      Log.Debug("Window: {0} init", ToString());
      Initialize();
    }

    protected void PageDestroy()
    {
      GUIGraphicsContext.Overlay = _previousOverlayVisible;
      DeInitialize();

      Log.Debug("Window: {0} deinit", ToString());
      FreeResources();
    }

    public string Text
    {
      get { return _textEntered; }
      set { _textEntered = value; }
    }

    public int KindOfSearch
    {
      get { return _searchKind; }
      set
      {
        _searchKind = value;
        SetSearchKind();
      }
    }

    public void SelectActiveButton(float x, float y)
    {
      // Draw each row
      int y1 = 250;
      int x1 = (int) (64*SkinRatio);
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      float fY = y1;
      ArrayList keyBoard = (ArrayList) _keyboardList[(int) _currentKeyboard];
      for (int row = 0; row < _maxRows; ++row, fY += _keyHeight)
      {
        float fX = x1;
        float fWidthSum = 0.0f;
        ArrayList keyRow = (ArrayList) keyBoard[row];
        int dwIndex = 0;
        for (int i = 0; i < keyRow.Count; i++)
        {
          Key key = (Key) keyRow[i];
          int width = (int) (key.dwWidth*SkinRatio);
          GUIGraphicsContext.ScaleHorizontal(ref width);
          if (x >= fX + fWidthSum && x <= fX + fWidthSum + width)
          {
            if (y >= fY && y < fY + _keyHeight)
            {
              _currentRow = row;
              _currentKey = dwIndex;
              return;
            }
          }
          fWidthSum += width;
          // There's a slightly larger gap between the leftmost keys (mode
          // keys) and the main keyboard
          if (dwIndex == 0)
          {
            width = (int) (GAP2_WIDTH*SkinRatio);
            GUIGraphicsContext.ScaleHorizontal(ref width);
            fWidthSum += width;
          }
          else
          {
            width = GAP_WIDTH;
            GUIGraphicsContext.ScaleHorizontal(ref width);
            fWidthSum += width;
          }
          ++dwIndex;
        }
      }

      // Default no key found no key highlighted
      if (_currentKey != -1)
      {
        _lastColumn = _currentKey;
      }
      _currentKey = -1;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU ||
          action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        Close();
        return;
      }

      Event ev;
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOUSE_MOVE:
          SelectActiveButton(action.fAmount1, action.fAmount2);
          break;
        case Action.ActionType.ACTION_MOUSE_CLICK:
          ev = Event.EV_A_BUTTON;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_SELECT_ITEM:
          if (_currentKey == -1)
          {
            Close();
            _pressedEnter = true;
          }
          ev = Event.EV_A_BUTTON;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          ev = Event.EV_DOWN;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          ev = Event.EV_UP;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
          ev = Event.EV_LEFT;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
          ev = Event.EV_RIGHT;
          UpdateState(ev);
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
          ev = Event.EV_BACK_BUTTON;
          UpdateState(ev);
          break;
        case Action.ActionType.ACTION_REMOVE_ITEM:
          Press(Xkey.XK_BACKSPACE);
          break;
        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
          {
            if (action.m_key.KeyChar >= 32)
            {
              Press((char) action.m_key.KeyChar);
            }
            if (action.m_key.KeyChar == 8)
            {
              Press(Xkey.XK_BACKSPACE);
            }
          }
          break;
      }
    }

    protected void Close()
    {
      _isVisible = false;
    }

    public void DoModal(int dwParentId)
    {
      _parentWindowId = dwParentId;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowId);
      if (null == _parentWindow)
      {
        _parentWindowId = 0;
        return;
      }
      GUIWindowManager.IsSwitchingToNewWindow = true;

      GUIWindowManager.RouteToWindow(GetID);

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);

      // active this window... (with its own OnPageLoad)
      PageLoad();

      GUIWindowManager.IsSwitchingToNewWindow = false;
      _isVisible = true;
      _position = _textEntered.Length;
      while (_isVisible && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        // deactive this window... (with its own OnPageDestroy)
        PageDestroy();

        GUIWindowManager.UnRoute();
        _parentWindow = null;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
      GUILayerManager.UnRegisterLayer(this);
    }

    public override void Render(float timePassed)
    {
      lock (this)
      {
        GUIGraphicsContext.SetScalingResolution(0, 0, false);
        TransformMatrix transform = new TransformMatrix();
        GUIGraphicsContext.SetWindowTransform(transform);

        // render the parent window
        RenderKeyboardLatin(timePassed);
      }
    }

    protected virtual void InitBoard()
    {
      if (_useSearchLayout)
      {
        MODEKEY_WIDTH = 130; // Searchkeyboard
      }

      // Restore keyboard to default state
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 1;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _capsLockTurnedOn = false;
      _shiftTurnedOn = false;
      _textEntered = "";
      _position = 0;
      int height = (int) (42*SkinRatio);
      GUIGraphicsContext.ScaleVertical(ref height);
      _keyHeight = height;
      _maxRows = 5;

      // Destroy old keyboard
      _keyboardList.Clear();


      //-------------------------------------------------------------------------
      // Alpha keyboard
      //-------------------------------------------------------------------------

      ArrayList keyBoard = new ArrayList();

      // First row is Done, 1-0
      ArrayList keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_OK, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_1));
      keyRow.Add(new Key(Xkey.XK_2));
      keyRow.Add(new Key(Xkey.XK_3));
      keyRow.Add(new Key(Xkey.XK_4));
      keyRow.Add(new Key(Xkey.XK_5));
      keyRow.Add(new Key(Xkey.XK_6));
      keyRow.Add(new Key(Xkey.XK_7));
      keyRow.Add(new Key(Xkey.XK_8));
      keyRow.Add(new Key(Xkey.XK_9));
      keyRow.Add(new Key(Xkey.XK_0));

      keyBoard.Add(keyRow);

      // Second row is Shift, A-J
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_A));
      keyRow.Add(new Key(Xkey.XK_B));
      keyRow.Add(new Key(Xkey.XK_C));
      keyRow.Add(new Key(Xkey.XK_D));
      keyRow.Add(new Key(Xkey.XK_E));
      keyRow.Add(new Key(Xkey.XK_F));
      keyRow.Add(new Key(Xkey.XK_G));
      keyRow.Add(new Key(Xkey.XK_H));
      keyRow.Add(new Key(Xkey.XK_I));
      keyRow.Add(new Key(Xkey.XK_J));
      keyBoard.Add(keyRow);

      // Third row is Caps Lock, K-T
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_K));
      keyRow.Add(new Key(Xkey.XK_L));
      keyRow.Add(new Key(Xkey.XK_M));
      keyRow.Add(new Key(Xkey.XK_N));
      keyRow.Add(new Key(Xkey.XK_O));
      keyRow.Add(new Key(Xkey.XK_P));
      keyRow.Add(new Key(Xkey.XK_Q));
      keyRow.Add(new Key(Xkey.XK_R));
      keyRow.Add(new Key(Xkey.XK_S));
      keyRow.Add(new Key(Xkey.XK_T));
      keyBoard.Add(keyRow);

      // Fourth row is Accents, U-Z, Backspace
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_U));
      keyRow.Add(new Key(Xkey.XK_V));
      keyRow.Add(new Key(Xkey.XK_W));
      keyRow.Add(new Key(Xkey.XK_X));
      keyRow.Add(new Key(Xkey.XK_Y));
      keyRow.Add(new Key(Xkey.XK_Z));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH*4) + (GAP_WIDTH*3)));
      keyBoard.Add(keyRow);

      // Fifth row is <empty>, Space, Left, Right
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_NULL, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH*6) + (GAP_WIDTH*5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH*2) + (GAP_WIDTH*1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH*2) + (GAP_WIDTH*1)));
      keyBoard.Add(keyRow);

      // Add the alpha keyboard to the list
      _keyboardList.Add(keyBoard);

      //-------------------------------------------------------------------------
      // Symbol keyboard
      //-------------------------------------------------------------------------

      keyBoard = new ArrayList();

      // First row
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_OK, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_LPAREN));
      keyRow.Add(new Key(Xkey.XK_RPAREN));
      keyRow.Add(new Key(Xkey.XK_AMPER));
      keyRow.Add(new Key(Xkey.XK_UNDERS));
      keyRow.Add(new Key(Xkey.XK_CARET));
      keyRow.Add(new Key(Xkey.XK_PERCENT));
      keyRow.Add(new Key(Xkey.XK_BSLASH));
      keyRow.Add(new Key(Xkey.XK_FSLASH));
      keyRow.Add(new Key(Xkey.XK_AT));
      keyRow.Add(new Key(Xkey.XK_NSIGN));

      keyBoard.Add(keyRow);

      // Second row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_LBRACK));
      keyRow.Add(new Key(Xkey.XK_RBRACK));
      keyRow.Add(new Key(Xkey.XK_DOLLAR));
      keyRow.Add(new Key(Xkey.XK_POUND_SIGN));
      keyRow.Add(new Key(Xkey.XK_YEN_SIGN));
      keyRow.Add(new Key(Xkey.XK_EURO_SIGN));
      keyRow.Add(new Key(Xkey.XK_SEMI));
      keyRow.Add(new Key(Xkey.XK_COLON));
      keyRow.Add(new Key(Xkey.XK_QUOTE));
      keyRow.Add(new Key(Xkey.XK_DQUOTE));
      keyBoard.Add(keyRow);

      // Third row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_LT));
      keyRow.Add(new Key(Xkey.XK_GT));
      keyRow.Add(new Key(Xkey.XK_QMARK));
      keyRow.Add(new Key(Xkey.XK_EXCL));
      keyRow.Add(new Key(Xkey.XK_INVERTED_QMARK));
      keyRow.Add(new Key(Xkey.XK_INVERTED_EXCL));
      keyRow.Add(new Key(Xkey.XK_DASH));
      keyRow.Add(new Key(Xkey.XK_STAR));
      keyRow.Add(new Key(Xkey.XK_PLUS));
      keyRow.Add(new Key(Xkey.XK_EQUAL));
      keyBoard.Add(keyRow);

      // Fourth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_ALPHABET, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_LBRACE));
      keyRow.Add(new Key(Xkey.XK_RBRACE));
      keyRow.Add(new Key(Xkey.XK_LT_DBL_ANGLE_QUOTE));
      keyRow.Add(new Key(Xkey.XK_RT_DBL_ANGLE_QUOTE));
      keyRow.Add(new Key(Xkey.XK_COMMA));
      keyRow.Add(new Key(Xkey.XK_PERIOD));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH*4) + (GAP_WIDTH*3)));
      keyBoard.Add(keyRow);

      // Fifth row is Accents, Space, Left, Right
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_ALPHABET, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_NULL, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH*6) + (GAP_WIDTH*5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH*2) + (GAP_WIDTH*1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH*2) + (GAP_WIDTH*1)));
      keyBoard.Add(keyRow);

      // Add the symbol keyboard to the list
      _keyboardList.Add(keyBoard);

      //-------------------------------------------------------------------------
      // Accents keyboard
      //-------------------------------------------------------------------------

      keyBoard = new ArrayList();

      // First row
      keyRow = new ArrayList();
      // Swedish - Finnish
      keyRow.Add(new Key(Xkey.XK_OK, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_CAP_A_RING));
      keyRow.Add(new Key(Xkey.XK_CAP_A_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_O_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_A_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_A_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_A_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_I_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_I_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_I_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_I_DIAERESIS));
      keyBoard.Add(keyRow);

      // Second row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));
      }

      //Danish - Norwegian
      keyRow.Add(new Key(Xkey.XK_CAP_A_RING));
      keyRow.Add(new Key(Xkey.XK_CAP_AE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_STROKE));
      keyRow.Add(new Key(Xkey.XK_CAP_C_CEDILLA));
      keyRow.Add(new Key(Xkey.XK_CAP_E_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_E_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_E_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_E_DIAERESIS));

      keyBoard.Add(keyRow);

      // Third row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));
      }

      // German
      keyRow.Add(new Key(Xkey.XK_CAP_U_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_O_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_A_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_SM_SHARP_S));
      keyRow.Add(new Key(Xkey.XK_CAP_O_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_O_TILDE));

      keyBoard.Add(keyRow);

      // Fourth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SYMBOLS, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_CAP_N_TILDE));
      keyRow.Add(new Key(Xkey.XK_CAP_U_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_U_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_U_CIRCUMFLEX));

      keyRow.Add(new Key(Xkey.XK_CAP_Y_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_Y_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH*4) + (GAP_WIDTH*3)));
      keyBoard.Add(keyRow);

      // Fifth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SYMBOLS, MODEKEY_WIDTH)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_NULL, MODEKEY_WIDTH));
      }

      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH*6) + (GAP_WIDTH*5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH*2) + (GAP_WIDTH*1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH*2) + (GAP_WIDTH*1)));
      keyBoard.Add(keyRow);

      // Add the accents keyboard to the list
      _keyboardList.Add(keyBoard);
    }

    protected void UpdateState(Event ev)
    {
      switch (_state)
      {
        case State.STATE_KEYBOARD:
          switch (ev)
          {
            case Event.EV_A_BUTTON: // Select current key
            case Event.EV_START_BUTTON:
              PressCurrent();
              break;

            case Event.EV_B_BUTTON: // Shift mode
            case Event.EV_BACK_BUTTON: // Back
              _state = State.STATE_BACK;
              Close(); //Added by JM to close automatically
              break;

            case Event.EV_X_BUTTON: // Toggle keyboard
              Press(_currentKeyboard == KeyboardTypes.TYPE_SYMBOLS ? Xkey.XK_ALPHABET : Xkey.XK_SYMBOLS);
              break;
            case Event.EV_WHITE_BUTTON: // Backspace
              Press(Xkey.XK_BACKSPACE);
              break;
            case Event.EV_BLACK_BUTTON: // Space
              Press(Xkey.XK_SPACE);
              break;
            case Event.EV_LEFT_BUTTON: // Left
              Press(Xkey.XK_ARROWLEFT);
              break;
            case Event.EV_RIGHT_BUTTON: // Right
              Press(Xkey.XK_ARROWRIGHT);
              break;

              // Navigation
            case Event.EV_UP:
              MoveUp();
              break;
            case Event.EV_DOWN:
              MoveDown();
              break;
            case Event.EV_LEFT:
              MoveLeft();
              break;
            case Event.EV_RIGHT:
              MoveRight();
              break;
          }
          break;
        default:
          Close();
          break;
      }
    }

    protected void ChangeKey(int iBoard, int iRow, int iKey, Key newkey)
    {
      ArrayList board = (ArrayList) _keyboardList[iBoard];
      ArrayList row = (ArrayList) board[iRow];
      row[iKey] = newkey;
    }

    protected void PressCurrent()
    {
      if (_currentKey == -1)
      {
        return;
      }

      ArrayList board = (ArrayList) _keyboardList[(int) _currentKeyboard];
      ArrayList row = (ArrayList) board[_currentRow];
      Key key = (Key) row[_currentKey];

      // Press it
      Press(key.xKey);
    }

    protected void Press(Xkey xk)
    {
      if (xk == Xkey.XK_NULL) // happens in Japanese keyboard (keyboard type)
      {
        xk = Xkey.XK_SPACE;
      }

      // If the key represents a character, add it to the word
      if (((uint) xk) < 0x10000 && xk != Xkey.XK_ARROWLEFT && xk != Xkey.XK_ARROWRIGHT)
      {
        // Don't add more than the maximum characters, and don't allow 
        // text to exceed the width of the text entry field
        if (_textEntered.Length < MAX_CHARS)
        {
          float fWidth = 0, fHeight = 0;
          _font18.GetTextExtent(_textEntered, ref fWidth, ref fHeight);

          if (fWidth < fTEXTBOX_WIDTH)
          {
            if (_position >= _textEntered.Length)
            {
              _textEntered += GetChar(xk).ToString();
              if (TextChanged != null)
              {
                TextChanged(_searchKind, _textEntered);
              }
            }
            else
            {
              _textEntered = _textEntered.Insert(_position, GetChar(xk).ToString());
              if (TextChanged != null)
              {
                TextChanged(_searchKind, _textEntered);
              }
            }
            ++_position; // move the caret
          }
        }

        // Unstick the shift key
        _shiftTurnedOn = false;
      }

        // Special cases
      else
      {
        switch (xk)
        {
          case Xkey.XK_BACKSPACE:
            if (_position > 0)
            {
              --_position; // move the caret
              _textEntered = _textEntered.Remove(_position, 1);
              if (TextChanged != null)
              {
                TextChanged(_searchKind, _textEntered);
              }
            }
            break;
          case Xkey.XK_DELETE: // Used for Japanese only
            if (_textEntered.Length > 0)
            {
              _textEntered = _textEntered.Remove(_position, 1);
              if (TextChanged != null)
              {
                TextChanged(_searchKind, _textEntered);
              }
            }
            break;
          case Xkey.XK_SHIFT:
            _shiftTurnedOn = !_shiftTurnedOn;
            break;
          case Xkey.XK_CAPSLOCK:
            _capsLockTurnedOn = !_capsLockTurnedOn;
            break;
          case Xkey.XK_ALPHABET:
            _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
            break;
          case Xkey.XK_SYMBOLS:
            _currentKeyboard = KeyboardTypes.TYPE_SYMBOLS;
            break;
          case Xkey.XK_ACCENTS:
            _currentKeyboard = KeyboardTypes.TYPE_ACCENTS;
            break;
          case Xkey.XK_ARROWLEFT:
            if (_position > 0)
            {
              --_position;
            }
            break;
          case Xkey.XK_ARROWRIGHT:
            if (_position < _textEntered.Length)
            {
              ++_position;
            }
            break;
          case Xkey.XK_OK:
            Close();
            _pressedEnter = true;
            break;
            // added to the original code VirtualKeyboard.cs
            // by Agree
            // starts here...

          case Xkey.XK_SEARCH_IS:
            _searchKind = (int) SearchKinds.SEARCH_STARTS_WITH;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_CONTAINS:
            _searchKind = (int) SearchKinds.SEARCH_ENDS_WITH;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_ENDS_WITH:
            _searchKind = (int) SearchKinds.SEARCH_IS;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_START_WITH:
            _searchKind = (int) SearchKinds.SEARCH_CONTAINS;
            SetSearchKind();
            break;
            // code by Agree ends here
            //
        }
      }
    }

    protected void SetSearchKind()
    {
      switch (_searchKind)
      {
        case (int) SearchKinds.SEARCH_STARTS_WITH:
          ChangeKey((int) _currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_START_WITH, MODEKEY_WIDTH));
          break;

        case (int) SearchKinds.SEARCH_ENDS_WITH:
          ChangeKey((int) _currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_ENDS_WITH, MODEKEY_WIDTH));
          break;

        case (int) SearchKinds.SEARCH_IS:
          ChangeKey((int) _currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_IS, MODEKEY_WIDTH));
          break;

        case (int) SearchKinds.SEARCH_CONTAINS:
          ChangeKey((int) _currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_CONTAINS, MODEKEY_WIDTH));
          break;
      }
      if (TextChanged != null)
      {
        TextChanged(_searchKind, _textEntered);
      }
    }

    protected void MoveUp()
    {
      if (_currentKey == -1)
      {
        _currentKey = _lastColumn;
      }

      do
      {
        // Update key index for special cases
        switch (_currentRow)
        {
          case 0:
            if (1 < _currentKey && _currentKey < 7) // 2 - 6
            {
              _lastColumn = _currentKey; // remember column
              _currentKey = 1; // move to spacebar
            }
            else if (6 < _currentKey && _currentKey < 9) // 7 - 8
            {
              _lastColumn = _currentKey; // remember column
              _currentKey = 2; // move to left arrow
            }
            else if (_currentKey > 8) // 9 - 0
            {
              _lastColumn = _currentKey; // remember column
              _currentKey = 3; // move to right arrow
            }
            break;
          case 3:
            if (_currentKey == 7) // backspace
            {
              _currentKey = Math.Max(7, _lastColumn); // restore column
            }
            break;
          case 4:
            if (_currentKey == 1) // spacebar
            {
              _currentKey = Math.Min(6, _lastColumn); // restore column
            }
            else if (_currentKey > 1) // left and right
            {
              _currentKey = 7; // backspace
            }
            break;
        }

        // Update row
        _currentRow = (_currentRow == 0) ? _maxRows - 1 : _currentRow - 1;
      } while (IsKeyDisabled());
    }

    protected void MoveDown()
    {
      if (_currentKey == -1)
      {
        _currentKey = _lastColumn;
      }

      do
      {
        // Update key index for special cases
        switch (_currentRow)
        {
          case 2:
            if (_currentKey > 7) // q - t
            {
              _lastColumn = _currentKey; // remember column
              _currentKey = 7; // move to backspace
            }
            break;
          case 3:
            if (0 < _currentKey && _currentKey < 7) // u - z
            {
              _lastColumn = _currentKey; // remember column
              _currentKey = 1; // move to spacebar
            }
            else if (_currentKey > 6) // backspace
            {
              _currentKey = _lastColumn > 8 ? 3 : 2;
            }
            break;
          case 4:
            switch (_currentKey)
            {
              case 1: // spacebar
                _currentKey = Math.Min(6, _lastColumn);
                break;
              case 2: // left arrow
                _currentKey = Math.Max(Math.Min(8, _lastColumn), 7);
                break;
              case 3: // right arrow
                _currentKey = Math.Max(9, _lastColumn);
                break;
            }
            break;
        }

        // Update row
        _currentRow = (_currentRow == _maxRows - 1) ? 0 : _currentRow + 1;
      } while (IsKeyDisabled());
    }

    public void SetLastColumn()
    {
      if (_currentKey == -1)
      {
        return;
      }

      // If the new key is a single character, remember it for later
      ArrayList board = (ArrayList) _keyboardList[(int) _currentKeyboard];
      ArrayList row = (ArrayList) board[_currentRow];
      Key key = (Key) row[_currentKey];
      if (key.name == "")
      {
        switch (key.xKey)
        {
            // Adjust the last column for the arrow keys to confine it
            // within the range of the key width
          case Xkey.XK_ARROWLEFT:
            _lastColumn = (_lastColumn <= 7) ? 7 : 8;
            break;
          case Xkey.XK_ARROWRIGHT:
            _lastColumn = (_lastColumn <= 9) ? 9 : 10;
            break;

            // Single char, non-arrow
          default:
            _lastColumn = _currentKey;
            break;
        }
      }
    }

    public bool IsKeyDisabled()
    {
      if (_currentKey == -1)
      {
        return true;
      }

      ArrayList board = (ArrayList) _keyboardList[(int) _currentKeyboard];
      ArrayList row = (ArrayList) board[_currentRow];
      Key key = (Key) row[_currentKey];

      // On the symbols keyboard, Shift and Caps Lock are disabled
      if (_currentKeyboard == KeyboardTypes.TYPE_SYMBOLS)
      {
        if (key.xKey == Xkey.XK_SHIFT || key.xKey == Xkey.XK_CAPSLOCK)
        {
          return true;
        }
      }
      return false;
    }

    protected char GetChar(Xkey xk)
    {
      // Handle case conversion
      char wc = (char) (((uint) xk) & 0xffff);

      if ((_capsLockTurnedOn && !_shiftTurnedOn) || (!_capsLockTurnedOn && _shiftTurnedOn))
      {
        wc = Char.ToUpper(wc);
      }
      else
      {
        wc = Char.ToLower(wc);
      }

      return wc;
    }

    protected void RenderKey(float fX, float fY, Key key, long keyColor, long textColor)
    {
      if (keyColor == COLOR_INVISIBLE || key.xKey == Xkey.XK_NULL)
      {
        return;
      }


      string strKey = GetChar(key.xKey).ToString();
      string name = (key.name.Length == 0) ? strKey : key.name;

      int width = (int) ((key.dwWidth - KEY_INSET)*SkinRatio) + 2;
      int height = (int) (KEY_INSET*SkinRatio) + 2;
      GUIGraphicsContext.ScaleHorizontal(ref width);
      GUIGraphicsContext.ScaleVertical(ref height);

      float x = fX + (int) (KEY_INSET*SkinRatio);
      float y = fY + (int) (KEY_INSET*SkinRatio);
      float z = fX + width; //z
      float w = fY + _keyHeight - height; //w

      float nw = width;
      float nh = _keyHeight - height;

      const float uoffs = 0;
      const float v = 1.0f;
      const float u = 1.0f;

      _keyTexture.Draw(x, y, nw, nh, uoffs, 0.0f, u, v, (int) keyColor);

      // Draw the key text. If key name is, use a slightly smaller font.
      float textWidth = 0;
      float textHeight = 0;
      float positionX = (x + z)/2.0f;
      float positionY = (y + w)/2.0f;
      if (key.name.Length > 1 && Char.IsUpper(key.name[1]))
      {
        _font12.GetTextExtent(name, ref textWidth, ref textHeight);
        positionX -= (textWidth/2);
        positionY -= (textHeight/2);
        _font12.DrawText(positionX, positionY, textColor, name, GUIControl.Alignment.ALIGN_LEFT, -1);
      }
      else
      {
        _font18.GetTextExtent(name, ref textWidth, ref textHeight);
        positionX -= (textWidth/2);
        positionY -= (textHeight/2);
        _font18.DrawText(positionX, positionY, textColor, name, GUIControl.Alignment.ALIGN_LEFT, -1);
      }
    }

    protected void DrawTextBox(float timePassed, int x1, int y1, int x2, int y2)
    {
      //long lColor=0xaaffffff;

      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      GUIGraphicsContext.ScalePosToScreenResolution(ref x2, ref y2);

      /*
            Rectangle[] rect = new Rectangle[1];
            rect[0].X=x1;
            rect[0].Y=y1;
            rect[0].Width=x2-x1;
            rect[0].Height=y2-y1;
            GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target|ClearFlags.Target, (int)lColor, 1.0f, 0, rect );
      */
      //image.ColourDiffuse=lColor;
      image.SetPosition(x1, y1);
      image.Width = (x2 - x1);
      image.Height = (y2 - y1);
      image.Render(timePassed);
    }

    protected void DrawText(int x, int y)
    {
      GUIGraphicsContext.ScalePosToScreenResolution(ref x, ref y);
      string textLine = _textEntered;
      if (_password)
      {
        textLine = "";
        for (int i = 0; i < _textEntered.Length; ++i)
        {
          textLine += "*";
        }
      }

      _fontSearchText.DrawText(x, y, COLOR_SEARCHTEXT, textLine, GUIControl.Alignment.ALIGN_LEFT, -1);


      // Draw blinking caret using line primitives.
      TimeSpan ts = DateTime.Now - _caretTimer;
      if ((ts.TotalSeconds%fCARET_BLINK_RATE) < fCARET_ON_RATIO)
      {
        string line = textLine.Substring(0, _position);

        float caretWidth = 0.0f;
        float caretHeight = 0.0f;
        _fontSearchText.GetTextExtent(line, ref caretWidth, ref caretHeight);
        x += (int) caretWidth;
        _fontSearchText.DrawText(x, y, 0xff202020, "|", GUIControl.Alignment.ALIGN_LEFT, -1);
      }
    }

    protected virtual void OnTextChanged()
    {
      if (TextChanged != null)
      {
        TextChanged(_searchKind, _textEntered);
      }
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion

    #region abstract methods

    protected abstract void MoveLeft();
    protected abstract void MoveRight();
    protected abstract void Press(char k);
    protected abstract void RenderKeyboardLatin(float timePassed);
    protected abstract void InitializeInstance();

    #endregion
  }

  public class StandardKeyboard : VirtualKeyboard
  {
    public void SetMaxLength(int maxLen)
    {
      _max_chars = maxLen;
    }
    protected override void InitializeInstance()
    {
      GetID = (int) Window.WINDOW_VIRTUAL_KEYBOARD;
      _capsLockTurnedOn = false;
      _shiftTurnedOn = false;
      _state = State.STATE_KEYBOARD;
      _position = 0;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 0;
      //m_fRepeatDelay   = fINITIAL_REPEAT;
      _keyTexture = null;

      int tempwidth = (int) (576.0f - 64.0f - 4.0f - 4.0f - 10.0f - 80.0f);
      GUIGraphicsContext.ScaleHorizontal(ref tempwidth);
      fTEXTBOX_WIDTH = tempwidth;

      SkinRatio = GUIGraphicsContext.SkinSize.Width/720.0f;
      _keyHeight = 42.0f*SkinRatio;
      _maxRows = 5;
      _pressedEnter = false;
      _caretTimer = DateTime.Now;
      // construct search def.
      _searchKind = (int) SearchKinds.SEARCH_CONTAINS; // default search Contains

      if (GUIGraphicsContext.DX9Device != null)
      {
        InitBoard();
      }
    }

    protected override void MoveLeft()
    {
      if (_currentKey == -1)
      {
        _currentKey = _lastColumn;
      }

      do
      {
        if (_currentKey <= 0)
        {
          ArrayList board = (ArrayList) _keyboardList[(int) _currentKeyboard];
          ArrayList row = (ArrayList) board[_currentRow];
          _currentKey = row.Count - 1;
        }
        else
        {
          --_currentKey;
        }
      } while (IsKeyDisabled());

      SetLastColumn();
    }

    protected override void MoveRight()
    {
      if (_currentKey == -1)
      {
        _currentKey = _lastColumn;
      }

      do
      {
        ArrayList board = (ArrayList) _keyboardList[(int) _currentKeyboard];
        ArrayList row = (ArrayList) board[_currentRow];

        if (_currentKey == row.Count - 1)
        {
          _currentKey = 0;
        }
        else
        {
          ++_currentKey;
        }
      } while (IsKeyDisabled());

      SetLastColumn();
    }

    protected override void Press(char k)
    {
      // Don't add more than the maximum characters, and don't allow 
      // text to exceed the width of the text entry field
      if (_textEntered.Length < MAX_CHARS)
      {
        float fWidth = 0, fHeight = 0;
        _fontSearchText.GetTextExtent(_textEntered, ref fWidth, ref fHeight);

        if (fWidth < (fTEXTBOX_WIDTH*SkinRatio))
        {
          if (_position >= _textEntered.Length)
          {
            _textEntered += k.ToString();
            OnTextChanged();
          }
          else
          {
            _textEntered = _textEntered.Insert(_position, k.ToString());
            OnTextChanged();
          }
          ++_position; // move the caret
        }
      }

      // Unstick the shift key
      _shiftTurnedOn = false;
    }

    protected override void RenderKeyboardLatin(float timePassed)
    {
      // Show text and caret
      DrawTextBox(timePassed, (int) (64*SkinRatio), 208,
                  (int) ((MODEKEY_WIDTH + GAP_WIDTH*9 + GAP2_WIDTH + KEY_WIDTH*10 + 67.0f)*SkinRatio), 248);
        //- 64.0f - 4.0f - 4.0f - 10.0f
      DrawText((int) (82*SkinRatio), 208);


      int x1 = (int) (64*SkinRatio);
      int y1 = 250;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      // Draw each row
      float fY = y1;
      ArrayList keyBoard = (ArrayList) _keyboardList[(int) _currentKeyboard];
      for (int row = 0; row < _maxRows; ++row, fY += _keyHeight)
      {
        float fX = x1;
        float fWidthSum = 0.0f;
        ArrayList keyRow = (ArrayList) keyBoard[row];
        int dwIndex = 0;
        for (int i = 0; i < keyRow.Count; i++)
        {
          // Determine key name
          Key key = (Key) keyRow[i];
          long selKeyColor = 0xffffffff;
          long selTextColor = COLOR_NORMAL;

          // Handle special key coloring
          switch (key.xKey)
          {
            case Xkey.XK_SHIFT:
              switch (_currentKeyboard)
              {
                case KeyboardTypes.TYPE_ALPHABET:
                case KeyboardTypes.TYPE_ACCENTS:
                  if (_shiftTurnedOn)
                  {
                    selKeyColor = COLOR_PRESSED;
                  }
                  break;
                case KeyboardTypes.TYPE_SYMBOLS:
                  selKeyColor = COLOR_DISABLED;
                  selTextColor = COLOR_FONT_DISABLED;
                  break;
              }
              break;
            case Xkey.XK_CAPSLOCK:
              switch (_currentKeyboard)
              {
                case KeyboardTypes.TYPE_ALPHABET:
                case KeyboardTypes.TYPE_ACCENTS:
                  if (_capsLockTurnedOn)
                  {
                    selKeyColor = COLOR_PRESSED;
                  }
                  break;
                case KeyboardTypes.TYPE_SYMBOLS:
                  selKeyColor = COLOR_DISABLED;
                  selTextColor = COLOR_FONT_DISABLED;
                  break;
              }
              break;
              /* case Xkey.XK_ACCENTS:
               selKeyColor = COLOR_INVISIBLE;
               selTextColor = COLOR_INVISIBLE;
               break;*/
          }

          // Highlight the current key
          if (row == _currentRow && dwIndex == _currentKey)
          {
            selKeyColor = COLOR_HIGHLIGHT;
          }

          RenderKey(fX + fWidthSum, fY, key, selKeyColor, selTextColor);

          int width = (int) (key.dwWidth*SkinRatio);
          GUIGraphicsContext.ScaleHorizontal(ref width);
          fWidthSum += width;

          // There's a slightly larger gap between the leftmost keys (mode
          // keys) and the main keyboard
          if (dwIndex == 0)
          {
            width = (int) (GAP2_WIDTH*SkinRatio);
          }
          else
          {
            width = GAP_WIDTH;
          }
          GUIGraphicsContext.ScaleHorizontal(ref width);
          fWidthSum += width;

          ++dwIndex;
        }
      }
    }
  }

  public class SmsStyledKeyboard : VirtualKeyboard
  {
    protected override void InitializeInstance()
    {
      GetID = (int) Window.WINDOW_VIRTUAL_SMS_KEYBOARD;
      _capsLockTurnedOn = true;
      _shiftTurnedOn = false;
      _state = State.STATE_KEYBOARD;
      _position = 0;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 0;
      //m_fRepeatDelay   = fINITIAL_REPEAT;
      _keyTexture = null;

      _keyHeight = 42.0f;
      _maxRows = 5;
      _pressedEnter = false;
      _caretTimer = DateTime.Now;
      // construct search def.
      _searchKind = (int) SearchKinds.SEARCH_CONTAINS; // default search Contains

      if (GUIGraphicsContext.DX9Device != null)
      {
        InitBoard();
      }
    }

    protected override void MoveLeft()
    {
      if (_position > 0)
      {
        _position -= 1;
      }
    }

    protected override void MoveRight()
    {
      if (_position < _textEntered.Length)
      {
        _position += 1;
      }
    }

    protected override void Press(char k)
    {
      if (k < '0' || k > '9')
      {
        _usingKeyboard = true;
      }
      if ((k == (char) 126))
      {
        _capsLockTurnedOn = _capsLockTurnedOn == false;
      }

      if (!_usingKeyboard)
      {
        // Check different key presse
        if (k != _previousKey && _currentKeyb != (char) 0)
        {
          if (_position == _textEntered.Length)
          {
            _textEntered += _currentKeyb;
          }
          else
          {
            _textEntered = _textEntered.Insert(_position, _currentKeyb.ToString());
          }

          _previousKey = (char) 0;
          _currentKeyb = (char) 0;
          _timerKey = DateTime.Now;
          _position++;
        }

        CheckTimer();
        if (k >= '0' && k <= '9')
        {
          _previousKey = k;
        }
        if (k == '0')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = ' ';
          }
          else if (_currentKeyb == ' ')
          {
            _currentKeyb = '0';
          }
          else if (_currentKeyb == '0')
          {
            _currentKeyb = ' ';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '1')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = '1';
          }
          _timerKey = DateTime.Now;
        }

        if (k == '2')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'a';
          }
          else if (_currentKeyb == 'a')
          {
            _currentKeyb = 'b';
          }
          else if (_currentKeyb == 'b')
          {
            _currentKeyb = 'c';
          }
          else if (_currentKeyb == 'c')
          {
            _currentKeyb = '2';
          }
          else if (_currentKeyb == '2')
          {
            _currentKeyb = 'a';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '3')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'd';
          }
          else if (_currentKeyb == 'd')
          {
            _currentKeyb = 'e';
          }
          else if (_currentKeyb == 'e')
          {
            _currentKeyb = 'f';
          }
          else if (_currentKeyb == 'f')
          {
            _currentKeyb = '3';
          }
          else if (_currentKeyb == '3')
          {
            _currentKeyb = 'd';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '4')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'g';
          }
          else if (_currentKeyb == 'g')
          {
            _currentKeyb = 'h';
          }
          else if (_currentKeyb == 'h')
          {
            _currentKeyb = 'i';
          }
          else if (_currentKeyb == 'i')
          {
            _currentKeyb = '4';
          }
          else if (_currentKeyb == '4')
          {
            _currentKeyb = 'g';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '5')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'j';
          }
          else if (_currentKeyb == 'j')
          {
            _currentKeyb = 'k';
          }
          else if (_currentKeyb == 'k')
          {
            _currentKeyb = 'l';
          }
          else if (_currentKeyb == 'l')
          {
            _currentKeyb = '5';
          }
          else if (_currentKeyb == '5')
          {
            _currentKeyb = 'j';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '6')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'm';
          }
          else if (_currentKeyb == 'm')
          {
            _currentKeyb = 'n';
          }
          else if (_currentKeyb == 'n')
          {
            _currentKeyb = 'o';
          }
          else if (_currentKeyb == 'o')
          {
            _currentKeyb = '6';
          }
          else if (_currentKeyb == '6')
          {
            _currentKeyb = 'm';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '7')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'p';
          }
          else if (_currentKeyb == 'p')
          {
            _currentKeyb = 'q';
          }
          else if (_currentKeyb == 'q')
          {
            _currentKeyb = 'r';
          }
          else if (_currentKeyb == 'r')
          {
            _currentKeyb = 's';
          }
          else if (_currentKeyb == 's')
          {
            _currentKeyb = '7';
          }
          else if (_currentKeyb == '7')
          {
            _currentKeyb = 'p';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '8')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 't';
          }
          else if (_currentKeyb == 't')
          {
            _currentKeyb = 'u';
          }
          else if (_currentKeyb == 'u')
          {
            _currentKeyb = 'v';
          }
          else if (_currentKeyb == 'v')
          {
            _currentKeyb = '8';
          }
          else if (_currentKeyb == '8')
          {
            _currentKeyb = 't';
          }
          _timerKey = DateTime.Now;
        }
        if (k == '9')
        {
          if (_currentKeyb == 0)
          {
            _currentKeyb = 'w';
          }
          else if (_currentKeyb == 'w')
          {
            _currentKeyb = 'x';
          }
          else if (_currentKeyb == 'x')
          {
            _currentKeyb = 'y';
          }
          else if (_currentKeyb == 'y')
          {
            _currentKeyb = 'z';
          }
          else if (_currentKeyb == 'z')
          {
            _currentKeyb = '9';
          }
          else if (_currentKeyb == '9')
          {
            _currentKeyb = 'w';
          }
          _timerKey = DateTime.Now;
        }
      }
      else
      {
        if ((k != (char) 126))
        {
          if (k == (char) 8) // Backspace
          {
            if (_position > 0)
            {
              _textEntered = _textEntered.Remove(_position - 1, 1);
              _position--;
            }
          }
          else
          {
            if (_position >= _textEntered.Length)
            {
              _textEntered += k;
            }
            else
            {
              _textEntered = _textEntered.Insert(_position, k.ToString());
            }
            _position++;
          }
        }
        _previousKey = (char) 0;
        _currentKeyb = (char) 0;
        _timerKey = DateTime.Now;
      }

      _usingKeyboard = false;

      // Unstick the shift key
      _shiftTurnedOn = false;
    }

    protected override void RenderKeyboardLatin(float timePassed)
    {
      // Show text and caret
      DrawTextBox(timePassed, 200, 155, 1150, 230);
      DrawText(250, 173);
      CheckTimer();
    }

    private void CheckTimer()
    {
      TimeSpan ts = DateTime.Now - _timerKey;
      if (ts.TotalMilliseconds >= 800)
      {
        if (_currentKeyb != (char) 0)
        {
          if (_capsLockTurnedOn)
          {
            _currentKeyb = char.ToUpper(_currentKeyb);
          }
          if (_position == _textEntered.Length)
          {
            _textEntered += _currentKeyb;
          }
          else
          {
            _textEntered = _textEntered.Insert(_position, _currentKeyb.ToString());
          }
          _position++;
        }
        _previousKey = (char) 0;
        _currentKeyb = (char) 0;
        _timerKey = DateTime.Now;
      }
    }

    protected override void InitBoard()
    {
      base.InitBoard();
      _capsLockTurnedOn = true;
    }
  }
}