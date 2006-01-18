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
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class VirtualKeyboard : GUIWindow, IRenderLayer
  {
    const int GAP_WIDTH = 0;
    const int GAP2_WIDTH = 4;
    const int MODEKEY_WIDTH = 110;
    const int KEY_INSET = 1;

    const int MAX_KEYS_PER_ROW = 14;

    // Must be this far from center on 0.0 - 1.0 scale
    const float JOY_THRESHOLD = 0.25f;

    // How often (per second) the caret blinks
    const float fCARET_BLINK_RATE = 1.0f;

    // During the blink period, the amount the caret is visible. 0.5 equals
    // half the time, 0.75 equals 3/4ths of the time, etc.
    const float fCARET_ON_RATIO = 0.75f;

    // Text colors for keys
    const long COLOR_SEARCHTEXT = 0xff000000;   // black (0xff10e010)
    const long COLOR_HIGHLIGHT = 0xff00ff00;   // green
    const long COLOR_PRESSED = 0xff808080;   // gray
    const long COLOR_NORMAL = 0xff000000;   // black
    const long COLOR_DISABLED = 0xffffffff;   // white
    const long COLOR_HELPTEXT = 0xffffffff;   // white
    const long COLOR_FONT_DISABLED = 0xff808080;   // gray
    const long COLOR_INVISIBLE = 0xff0000ff;   // blue
    const long COLOR_RED = 0xffff0000;   // red
    // Font sizes
    const int FONTSIZE_BUTTONCHARS = 24;
    const int FONTSIZE_BUTTONSTRINGS = 18;
    const int FONTSIZE_SEARCHTEXT = 20;


    // Controller repeat values
    const float fINITIAL_REPEAT = 0.333f; // 333 mS recommended for first repeat
    const float fSTD_REPEAT = 0.085f; // 85 mS recommended for repeat rate

    // Maximum number of characters in string
    const int MAX_CHARS = 64;

    // Width of text box
    float fTEXTBOX_WIDTH = 576.0f - 64.0f - 4.0f - 4.0f - 10.0f;
    float BUTTON_Y_POS = 411.0f;      // button text line
    float BUTTON_X_OFFSET = 40.0f;      // space between button and text

    const long BUTTONTEXT_COLOR = 0xffffffff;
    const float FIXED_JSL_SIZE = 3.0f;

    // Xboxdings font button mappings
    const string TEXT_A_BUTTON = "A";
    const string TEXT_B_BUTTON = "B";
    const string TEXT_X_BUTTON = "C";
    const string TEXT_Y_BUTTON = "D";
    const int KEY_WIDTH = 34;   // width of std key in pixels

    string[] g_strEnglish = 
    {
      "English",
      "Choose Keyboard",
      "Sample graphics. Don't use in your game",
      "Select",
      "Back",
      "Help",
      "SPACE",
      "BKSP",
      "SHIFT",
      "CAPS",
      "ALPHABET",
      "SYMB",
      "ACEENTS",
      "DONE",
      "Select",
      "Cance",
      "Toggle\nmode",
      "Display help",
      "Backspace",
      "Space",
      "Trigger buttons move cursor",
    };
    enum KeyboardTypes
    {
      TYPE_ALPHABET = 0,
      TYPE_SYMBOLS,
      TYPE_ACCENTS,

      TYPE_HIRAGANA,
      TYPE_KATAKANA,
      TYPE_ANS,

      TYPE_MAX
    };

    enum State
    {
      STATE_BACK,         // Main menu
      STATE_KEYBOARD,     // Keyboard display
      STATE_MAX
    };

    enum ControllerSState
    {
      XKJ_START = 1 << 0,
      XKJ_BACK = 1 << 1,
      XKJ_A = 1 << 2,
      XKJ_B = 1 << 3,
      XKJ_X = 1 << 4,
      XKJ_Y = 1 << 5,
      XKJ_BLACK = 1 << 6,
      XKJ_WHITE = 1 << 7,
      XKJ_LEFTTR = 1 << 8,
      XKJ_RIGHTTR = 1 << 9,

      XKJ_DUP = 1 << 12,
      XKJ_DDOWN = 1 << 13,
      XKJ_DLEFT = 1 << 14,
      XKJ_DRIGHT = 1 << 15,
      XKJ_UP = 1 << 16,
      XKJ_DOWN = 1 << 17,
      XKJ_LEFT = 1 << 18,
      XKJ_RIGHT = 1 << 19
    };

    enum Event
    {
      EV_NULL,            // No events
      EV_A_BUTTON,        // A button
      EV_START_BUTTON,    // Start button
      EV_B_BUTTON,        // B button
      EV_BACK_BUTTON,     // Back button
      EV_X_BUTTON,        // X button
      EV_Y_BUTTON,        // Y button
      EV_WHITE_BUTTON,    // White button
      EV_BLACK_BUTTON,    // Black button
      EV_LEFT_BUTTON,     // Left trigger
      EV_RIGHT_BUTTON,    // Right trigger
      EV_UP,              // Up Dpad or left joy
      EV_DOWN,            // Down Dpad or left joy
      EV_LEFT,            // Left Dpad or left joy
      EV_RIGHT,           // Right Dpad or left joy

      EVENT_MAX
    };

    enum Xkey
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
      XK_DELETE,              // delete           // !!!
      XK_SHIFT,               // shift
      XK_CAPSLOCK,            // caps lock
      XK_ALPHABET,            // alphabet
      XK_SYMBOLS,             // symbols
      XK_ACCENTS,             // accents
      XK_OK,                  // "done"
      XK_HIRAGANA,            // Hiragana
      XK_KATAKANA,            // Katakana
      XK_ANS,                 // Alphabet/numeral/symbol
    };

    enum KeyboardLanguageType
    {
      KEYBOARD_ENGLISH,
    };
    enum StringID
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
    };


    class Key
    {
      public Xkey xKey;       // virtual key code
      public int dwWidth = KEY_WIDTH;    // width of the key
      public string strName = "";    // name of key when vKey >= 0x10000
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
            strName = "SPACE";
            break;
          case Xkey.XK_BACKSPACE:
            strName = "BKSP";
            break;
          case Xkey.XK_SHIFT:
            strName = "SHIFT";
            break;
          case Xkey.XK_CAPSLOCK:
            strName = "CAPS";
            break;
          case Xkey.XK_ALPHABET:
            strName = "ALPHABET";
            break;
          case Xkey.XK_SYMBOLS:
            strName = "SYMB";
            break;
          case Xkey.XK_ACCENTS:
            strName = "ACCENTS";
            break;
          case Xkey.XK_OK:
            strName = "DONE";
            break;
        }
      }
    };

    string m_strData = "";
    bool m_bIsCapsLockOn = false;
    bool m_bIsShiftOn = false;
    State m_State;
    int m_iPos;
    KeyboardTypes m_iCurrBoard;
    int m_iCurrRow;
    int m_iCurrKey;
    int m_iLastColumn;
    //float         m_fRepeatDelay;
    CachedTexture.Frame m_pKeyTexture;
    float m_fKeyHeight;
    int m_dwMaxRows;
    bool m_bConfirmed;
    GUIFont m_Font18;
    GUIFont m_Font12;
    GUIFont m_FontButtons;
    GUIFont m_FontSearchText;
    DateTime m_CaretTimer = DateTime.Now;
    bool m_bPrevOverlay = true;
    bool _Password = false;
    GUIImage image;

    ArrayList m_KeyboardList = new ArrayList();         // list of rows = keyboard


    #region Base Dialog Variables
    bool m_bRunning = false;
    int m_dwParentWindowID = 0;
    GUIWindow m_pParentWindow = null;
    #endregion

    public VirtualKeyboard()
    {
      GetID = (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD;
      m_bIsCapsLockOn = false;
      m_bIsShiftOn = false;
      m_State = State.STATE_KEYBOARD;
      m_iPos = 0;
      m_iCurrBoard = KeyboardTypes.TYPE_ALPHABET;
      m_iCurrRow = 0;
      m_iCurrKey = 0;
      m_iLastColumn = 0;
      //m_fRepeatDelay   = fINITIAL_REPEAT;
      m_pKeyTexture = null;

      m_fKeyHeight = 42.0f;
      m_dwMaxRows = 5;
      m_bConfirmed = false;
      m_CaretTimer = DateTime.Now;


      if (GUIGraphicsContext.DX9Device != null)
        InitBoard();
    }

    public override bool Init()
    {
      return true;
    }

    public bool IsConfirmed
    {
      get { return m_bConfirmed; }
    }

    void Initialize()
    {
      m_Font12 = GUIFontManager.GetFont("font12");
      m_Font18 = GUIFontManager.GetFont("font18");
      m_FontButtons = GUIFontManager.GetFont("dingbats");
      m_FontSearchText = GUIFontManager.GetFont("font14");

      int iTextureWidth, iTextureHeight;
      int iImages = GUITextureManager.Load("keyNF.bmp", 0, 0, 0);
      if (iImages == 1)
      {
        m_pKeyTexture = GUITextureManager.GetTexture("keyNF.bmp", 0, out iTextureWidth, out iTextureHeight);
      }
      image = new GUIImage(this.GetID, 1, 0, 0, 10, 10, "white.bmp", 1);
      image.AllocResources();
    }

    void DeInitialize()
    {
      image.FreeResources();
      image = null;
    }

    public void Reset()
    {
      _Password = false;
      m_bConfirmed = false;
      m_bIsCapsLockOn = false;
      m_bIsShiftOn = false;
      m_State = State.STATE_KEYBOARD;
      m_iPos = 0;
      m_iCurrBoard = KeyboardTypes.TYPE_ALPHABET;
      m_iCurrRow = 0;
      m_iCurrKey = 0;
      m_iLastColumn = 0;
      //m_fRepeatDelay   = fINITIAL_REPEAT;
      m_fKeyHeight = 42.0f;
      m_dwMaxRows = 5;
      m_iPos = 0;
      m_strData = "";
      m_CaretTimer = DateTime.Now;


      int y = 411;
      int x = 40;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x, ref y);
      BUTTON_Y_POS = x;      // button text line
      BUTTON_X_OFFSET = y;      // space between button and text

      int width = 42;
      GUIGraphicsContext.ScaleHorizontal(ref width);
      m_fKeyHeight = width;

      width = (int)(576.0f - 64.0f - 4.0f - 4.0f - 10.0f);
      GUIGraphicsContext.ScaleHorizontal(ref width);
      fTEXTBOX_WIDTH = width;

      InitBoard();
    }

    public bool Password
    {
      get { return _Password; }
      set { _Password = value; }
    }

    protected void PageLoad()
    {
      m_bPrevOverlay = GUIGraphicsContext.Overlay;
      m_bConfirmed = false;
      GUIGraphicsContext.Overlay = false;
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD));
      Log.Write("window:{0} init", this.ToString());
      Initialize();
    }

    protected void PageDestroy()
    {
      GUIGraphicsContext.Overlay = m_bPrevOverlay;
      DeInitialize();

      Log.Write("window:{0} deinit", this.ToString());
      FreeResources();
    }

    public string Text
    {
      get { return m_strData; }
      set { m_strData = value; }
    }

    public void SelectActiveButton(float x, float y)
    {
      // Draw each row
      int y1 = 250, x1 = 64;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      float fY = y1;
      ArrayList keyBoard = (ArrayList)m_KeyboardList[(int)m_iCurrBoard];
      for (int row = 0; row < m_dwMaxRows; ++row, fY += m_fKeyHeight)
      {
        float fX = x1;
        float fWidthSum = 0.0f;
        ArrayList keyRow = (ArrayList)keyBoard[row];
        int dwIndex = 0;
        for (int i = 0; i < keyRow.Count; i++)
        {
          Key key = (Key)keyRow[i];
          int width = key.dwWidth;
          GUIGraphicsContext.ScaleHorizontal(ref width);
          if (x >= fX + fWidthSum && x <= fX + fWidthSum + key.dwWidth)
          {
            if (y >= fY && y < fY + m_fKeyHeight)
            {
              m_iCurrRow = row;
              m_iCurrKey = dwIndex;
              return;
            }
          }
          fWidthSum += width;
          // There's a slightly larger gap between the leftmost keys (mode
          // keys) and the main keyboard
          if (dwIndex == 0)
          {
            width = GAP2_WIDTH;
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
      if (m_iCurrKey != -1) m_iLastColumn = m_iCurrKey;
      m_iCurrKey = -1;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
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
          if (m_iCurrKey == -1)
          {
            Close();
            m_bConfirmed = true;
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

        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
          {
            if (action.m_key.KeyChar >= 32)
              Press((char)action.m_key.KeyChar);
            if (action.m_key.KeyChar == 8)
            {
              Press(Xkey.XK_BACKSPACE);
            }
          }
          break;
      }
    }
    void Close()
    {

      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        // deactive this window... (with its own OnPageDestroy)
        PageDestroy();

        GUIWindowManager.UnRoute();
        m_pParentWindow = null;
        m_bRunning = false;
      }

      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    public void DoModal(int dwParentId)
    {

      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        m_dwParentWindowID = 0;
        return;
      }
      GUIWindowManager.IsSwitchingToNewWindow = true;

      GUIWindowManager.RouteToWindow(GetID);

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      // active this window... (with its own OnPageLoad)
      PageLoad();

      GUIWindowManager.IsSwitchingToNewWindow = false;
      m_bRunning = true;
      m_iPos = m_strData.Length;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
      GUILayerManager.UnRegisterLayer(this);
    }

    public override void Render(float timePassed)
    {

      lock (this)
      {

        // render the parent window
        RenderKeyboardLatin(timePassed);
      }
    }

    void InitBoard()
    {
      // Restore keyboard to default state
      m_iCurrRow = 0;
      m_iCurrKey = 0;
      m_iLastColumn = 1;
      m_iCurrBoard = KeyboardTypes.TYPE_ALPHABET;
      m_bIsCapsLockOn = false;
      m_bIsShiftOn = false;
      m_strData = "";
      m_iPos = 0;
      int height = 42;
      GUIGraphicsContext.ScaleVertical(ref height);
      m_fKeyHeight = height;
      m_dwMaxRows = 5;

      // Destroy old keyboard
      m_KeyboardList.Clear();


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
      keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));
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
      keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));
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

      // Fourth row is Symbols, U-Z, Backspace
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_SYMBOLS, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_U));
      keyRow.Add(new Key(Xkey.XK_V));
      keyRow.Add(new Key(Xkey.XK_W));
      keyRow.Add(new Key(Xkey.XK_X));
      keyRow.Add(new Key(Xkey.XK_Y));
      keyRow.Add(new Key(Xkey.XK_Z));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH * 4) + (GAP_WIDTH * 3)));
      keyBoard.Add(keyRow);

      // Fifth row is Accents, Space, Left, Right
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH * 6) + (GAP_WIDTH * 5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyBoard.Add(keyRow);

      // Add the alpha keyboard to the list
      m_KeyboardList.Add(keyBoard);

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
      keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));
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
      keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));
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
      keyRow.Add(new Key(Xkey.XK_ALPHABET, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_LBRACE));
      keyRow.Add(new Key(Xkey.XK_RBRACE));
      keyRow.Add(new Key(Xkey.XK_LT_DBL_ANGLE_QUOTE));
      keyRow.Add(new Key(Xkey.XK_RT_DBL_ANGLE_QUOTE));
      keyRow.Add(new Key(Xkey.XK_COMMA));
      keyRow.Add(new Key(Xkey.XK_PERIOD));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH * 4) + (GAP_WIDTH * 3)));
      keyBoard.Add(keyRow);

      // Fifth row is Accents, Space, Left, Right
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH * 6) + (GAP_WIDTH * 5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyBoard.Add(keyRow);

      // Add the symbol keyboard to the list
      m_KeyboardList.Add(keyBoard);

      //-------------------------------------------------------------------------
      // Accents keyboard
      //-------------------------------------------------------------------------

      keyBoard = new ArrayList();

      // First row
      keyRow = new ArrayList();
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

      // Second row
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_SHIFT, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_CAP_A_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_A_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_A_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_A_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_C_CEDILLA));
      keyRow.Add(new Key(Xkey.XK_CAP_E_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_E_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_E_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_E_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_I_GRAVE));
      keyBoard.Add(keyRow);

      // Third row
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_CAPSLOCK, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_CAP_I_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_I_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_I_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_N_TILDE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_O_TILDE));
      keyRow.Add(new Key(Xkey.XK_CAP_O_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_SM_SHARP_S));
      keyBoard.Add(keyRow);

      // Fourth row
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_ALPHABET, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_CAP_U_GRAVE));
      keyRow.Add(new Key(Xkey.XK_CAP_U_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_U_CIRCUMFLEX));
      keyRow.Add(new Key(Xkey.XK_CAP_U_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_CAP_Y_ACUTE));
      keyRow.Add(new Key(Xkey.XK_CAP_Y_DIAERESIS));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (KEY_WIDTH * 4) + (GAP_WIDTH * 3)));
      keyBoard.Add(keyRow);

      // Fifth row
      keyRow = new ArrayList();
      keyRow.Add(new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH));
      keyRow.Add(new Key(Xkey.XK_SPACE, (KEY_WIDTH * 6) + (GAP_WIDTH * 5)));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (KEY_WIDTH * 2) + (GAP_WIDTH * 1)));
      keyBoard.Add(keyRow);

      // Add the accents keyboard to the list
      m_KeyboardList.Add(keyBoard);

    }

    void UpdateState(Event ev)
    {
      switch (m_State)
      {
        case State.STATE_KEYBOARD:
          switch (ev)
          {
            case Event.EV_A_BUTTON:           // Select current key
            case Event.EV_START_BUTTON:
              PressCurrent();
              break;

            case Event.EV_B_BUTTON:           // Shift mode
            case Event.EV_BACK_BUTTON:        // Back
              m_State = State.STATE_BACK;
              Close();	//Added by JM to close automatically
              break;

            case Event.EV_X_BUTTON:           // Toggle keyboard
              Press(m_iCurrBoard == KeyboardTypes.TYPE_SYMBOLS ? Xkey.XK_ALPHABET : Xkey.XK_SYMBOLS);
              /*if( m_iKeyboard == KEYBOARD_ENGLISH )
              {
                Press( m_iCurrBoard == KeyboardTypes.TYPE_SYMBOLS ?Xkey.XK_ALPHABET : Xkey.XK_SYMBOLS );
              }
              else
              {
                switch( m_iCurrBoard )
                {
                  case KeyboardTypes.TYPE_ALPHABET: Press( Xkey.XK_SYMBOLS  ); break;
                  case KeyboardTypes.TYPE_SYMBOLS:  Press( Xkey.XK_ACCENTS  ); break;
                  case KeyboardTypes.TYPE_ACCENTS:  Press( Xkey.XK_ALPHABET ); break;
                }
              }*/
              break;
            case Event.EV_WHITE_BUTTON:       // Backspace
              Press(Xkey.XK_BACKSPACE);
              break;
            case Event.EV_BLACK_BUTTON:       // Space
              Press(Xkey.XK_SPACE);
              break;
            case Event.EV_LEFT_BUTTON:        // Left
              Press(Xkey.XK_ARROWLEFT);
              break;
            case Event.EV_RIGHT_BUTTON:       // Right
              Press(Xkey.XK_ARROWRIGHT);
              break;

            // Navigation
            case Event.EV_UP: MoveUp(); break;
            case Event.EV_DOWN: MoveDown(); break;
            case Event.EV_LEFT: MoveLeft(); break;
            case Event.EV_RIGHT: MoveRight(); break;
          }
          break;
        default:
          Close();
          break;
      }
    }

    void ChangeKey(int iBoard, int iRow, int iKey, Key newkey)
    {
      ArrayList board = (ArrayList)m_KeyboardList[iBoard];
      ArrayList row = (ArrayList)board[iRow];
      row[iKey] = newkey;
    }

    void PressCurrent()
    {
      if (m_iCurrKey == -1) return;

      ArrayList board = (ArrayList)m_KeyboardList[(int)m_iCurrBoard];
      ArrayList row = (ArrayList)board[m_iCurrRow];
      Key key = (Key)row[m_iCurrKey];

      // Press it
      Press(key.xKey);
    }

    void Press(char k)
    {
      // Don't add more than the maximum characters, and don't allow 
      // text to exceed the width of the text entry field
      if (m_strData.Length < MAX_CHARS)
      {
        float fWidth = 0, fHeight = 0;
        m_Font18.GetTextExtent(m_strData, ref fWidth, ref fHeight);

        if (fWidth < fTEXTBOX_WIDTH)
        {
          if (m_iPos >= m_strData.Length)
            m_strData += k.ToString();
          else
            m_strData = m_strData.Insert(m_iPos, k.ToString());
          ++m_iPos; // move the caret
        }
      }

      // Unstick the shift key
      m_bIsShiftOn = false;
    }

    void Press(Xkey xk)
    {
      if (xk == Xkey.XK_NULL) // happens in Japanese keyboard (keyboard type)
        xk = Xkey.XK_SPACE;

      // If the key represents a character, add it to the word
      if (((uint)xk) < 0x10000 && xk != Xkey.XK_ARROWLEFT && xk != Xkey.XK_ARROWRIGHT)
      {
        // Don't add more than the maximum characters, and don't allow 
        // text to exceed the width of the text entry field
        if (m_strData.Length < MAX_CHARS)
        {
          float fWidth = 0, fHeight = 0;
          m_Font18.GetTextExtent(m_strData, ref fWidth, ref fHeight);

          if (fWidth < fTEXTBOX_WIDTH)
          {
            if (m_iPos >= m_strData.Length)
              m_strData += GetChar(xk).ToString();
            else
              m_strData = m_strData.Insert(m_iPos, GetChar(xk).ToString());
            ++m_iPos; // move the caret
          }
        }

        // Unstick the shift key
        m_bIsShiftOn = false;
      }

        // Special cases
      else switch (xk)
        {
          case Xkey.XK_BACKSPACE:
            if (m_iPos > 0)
            {
              --m_iPos; // move the caret
              m_strData = m_strData.Remove(m_iPos, 1);
            }
            break;
          case Xkey.XK_DELETE: // Used for Japanese only
            if (m_strData.Length > 0)
              m_strData = m_strData.Remove(m_iPos, 1);
            break;
          case Xkey.XK_SHIFT:
            m_bIsShiftOn = !m_bIsShiftOn;
            break;
          case Xkey.XK_CAPSLOCK:
            m_bIsCapsLockOn = !m_bIsCapsLockOn;
            break;
          case Xkey.XK_ALPHABET:
            m_iCurrBoard = KeyboardTypes.TYPE_ALPHABET;

            // Adjust mode keys
            ChangeKey((int)m_iCurrBoard, 3, 0, new Key(Xkey.XK_SYMBOLS, MODEKEY_WIDTH));
            ChangeKey((int)m_iCurrBoard, 4, 0, new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH));

            break;
          case Xkey.XK_SYMBOLS:
            m_iCurrBoard = KeyboardTypes.TYPE_SYMBOLS;

            // Adjust mode keys
            ChangeKey((int)m_iCurrBoard, 3, 0, new Key(Xkey.XK_ALPHABET, MODEKEY_WIDTH));
            ChangeKey((int)m_iCurrBoard, 4, 0, new Key(Xkey.XK_ACCENTS, MODEKEY_WIDTH));

            break;
          case Xkey.XK_ACCENTS:
            m_iCurrBoard = KeyboardTypes.TYPE_ACCENTS;

            // Adjust mode keys
            ChangeKey((int)m_iCurrBoard, 3, 0, new Key(Xkey.XK_ALPHABET, MODEKEY_WIDTH));
            ChangeKey((int)m_iCurrBoard, 4, 0, new Key(Xkey.XK_SYMBOLS, MODEKEY_WIDTH));
            break;
          case Xkey.XK_ARROWLEFT:
            if (m_iPos > 0)
              --m_iPos;
            break;
          case Xkey.XK_ARROWRIGHT:
            if (m_iPos < m_strData.Length)
              ++m_iPos;
            break;
          case Xkey.XK_OK:
            Close();
            m_bConfirmed = true;
            break;
        }
    }

    void MoveUp()
    {
      if (m_iCurrKey == -1) m_iCurrKey = m_iLastColumn;

      do
      {
        // Update key index for special cases
        switch (m_iCurrRow)
        {
          case 0:
            if (1 < m_iCurrKey && m_iCurrKey < 7)      // 2 - 6
            {
              m_iLastColumn = m_iCurrKey;             // remember column
              m_iCurrKey = 1;                         // move to spacebar
            }
            else if (6 < m_iCurrKey && m_iCurrKey < 9) // 7 - 8
            {
              m_iLastColumn = m_iCurrKey;             // remember column
              m_iCurrKey = 2;                         // move to left arrow
            }
            else if (m_iCurrKey > 8)                   // 9 - 0
            {
              m_iLastColumn = m_iCurrKey;             // remember column
              m_iCurrKey = 3;                         // move to right arrow
            }
            break;
          case 3:
            if (m_iCurrKey == 7)                       // backspace
              m_iCurrKey = Math.Max(7, m_iLastColumn);   // restore column
            break;
          case 4:
            if (m_iCurrKey == 1)                       // spacebar
              m_iCurrKey = Math.Min(6, m_iLastColumn);   // restore column
            else if (m_iCurrKey > 1)                   // left and right
              m_iCurrKey = 7;                         // backspace
            break;
        }

        // Update row
        m_iCurrRow = (m_iCurrRow == 0) ? m_dwMaxRows - 1 : m_iCurrRow - 1;

      } while (IsKeyDisabled());
    }

    void MoveDown()
    {
      if (m_iCurrKey == -1) m_iCurrKey = m_iLastColumn;

      do
      {
        // Update key index for special cases
        switch (m_iCurrRow)
        {
          case 2:
            if (m_iCurrKey > 7)                    // q - t
            {
              m_iLastColumn = m_iCurrKey;         // remember column
              m_iCurrKey = 7;                     // move to backspace
            }
            break;
          case 3:
            if (0 < m_iCurrKey && m_iCurrKey < 7)  // u - z
            {
              m_iLastColumn = m_iCurrKey;         // remember column
              m_iCurrKey = 1;                     // move to spacebar
            }
            else if (m_iCurrKey > 6)               // backspace
            {
              if (m_iLastColumn > 8)
                m_iCurrKey = 3;                 // move to right arrow
              else
                m_iCurrKey = 2;                 // move to left arrow
            }
            break;
          case 4:
            switch (m_iCurrKey)
            {
              case 1:                             // spacebar
                m_iCurrKey = Math.Min(6, m_iLastColumn);
                break;
              case 2:                             // left arrow
                m_iCurrKey = Math.Max(Math.Min(8, m_iLastColumn), 7);
                break;
              case 3:                             // right arrow
                m_iCurrKey = Math.Max(9, m_iLastColumn);
                break;
            }
            break;
        }

        // Update row
        m_iCurrRow = (m_iCurrRow == m_dwMaxRows - 1) ? 0 : m_iCurrRow + 1;

      } while (IsKeyDisabled());
    }

    void MoveLeft()
    {
      if (m_iCurrKey == -1) m_iCurrKey = m_iLastColumn;
      do
      {
        if (m_iCurrKey <= 0)
        {
          ArrayList board = (ArrayList)m_KeyboardList[(int)m_iCurrBoard];
          ArrayList row = (ArrayList)board[m_iCurrRow];
          m_iCurrKey = row.Count - 1;

        }
        else
          --m_iCurrKey;

      } while (IsKeyDisabled());

      SetLastColumn();
    }

    void MoveRight()
    {
      if (m_iCurrKey == -1) m_iCurrKey = m_iLastColumn;
      do
      {
        ArrayList board = (ArrayList)m_KeyboardList[(int)m_iCurrBoard];
        ArrayList row = (ArrayList)board[m_iCurrRow];

        if (m_iCurrKey == row.Count - 1)
          m_iCurrKey = 0;
        else
          ++m_iCurrKey;

      } while (IsKeyDisabled());

      SetLastColumn();
    }

    void SetLastColumn()
    {
      if (m_iCurrKey == -1) return;
      // If the new key is a single character, remember it for later
      ArrayList board = (ArrayList)m_KeyboardList[(int)m_iCurrBoard];
      ArrayList row = (ArrayList)board[m_iCurrRow];
      Key key = (Key)row[m_iCurrKey];
      if (key.strName == "")
      {
        switch (key.xKey)
        {
          // Adjust the last column for the arrow keys to confine it
          // within the range of the key width
          case Xkey.XK_ARROWLEFT:
            m_iLastColumn = (m_iLastColumn <= 7) ? 7 : 8; break;
          case Xkey.XK_ARROWRIGHT:
            m_iLastColumn = (m_iLastColumn <= 9) ? 9 : 10; break;

          // Single char, non-arrow
          default:
            m_iLastColumn = m_iCurrKey; break;
        }
      }
    }

    bool IsKeyDisabled()
    {
      if (m_iCurrKey == -1) return true;

      ArrayList board = (ArrayList)m_KeyboardList[(int)m_iCurrBoard];
      ArrayList row = (ArrayList)board[m_iCurrRow];
      Key key = (Key)row[m_iCurrKey];

      // On the symbols keyboard, Shift and Caps Lock are disabled
      if (m_iCurrBoard == KeyboardTypes.TYPE_SYMBOLS)
      {
        if (key.xKey == Xkey.XK_SHIFT || key.xKey == Xkey.XK_CAPSLOCK)
          return true;
      }

      // On the English keyboard, the Accents key is disabled
      //if( m_iKeyboard == KeyboardLanguageType.KEYBOARD_ENGLISH )
      {
        if (key.xKey == Xkey.XK_ACCENTS)
          return true;
      }

      return false;
    }

    char GetChar(Xkey xk)
    {
      // Handle case conversion
      char wc = (char)(((uint)xk) & 0xffff);

      if ((m_bIsCapsLockOn && !m_bIsShiftOn) || (!m_bIsCapsLockOn && m_bIsShiftOn))
        wc = Char.ToUpper(wc);
      else
        wc = Char.ToLower(wc);

      return wc;
    }

    void RenderKey(float fX, float fY, Key key, long KeyColor, long TextColor)
    {
      if (KeyColor == COLOR_INVISIBLE || key.xKey == Xkey.XK_NULL) return;


      string strKey = GetChar(key.xKey).ToString();
      string strName = (key.strName.Length == 0) ? strKey : key.strName;

      int width = key.dwWidth - KEY_INSET + 2;
      int height = (int)(KEY_INSET + 2);
      GUIGraphicsContext.ScaleHorizontal(ref width);
      GUIGraphicsContext.ScaleVertical(ref height);

      float x = fX + KEY_INSET;
      float y = fY + KEY_INSET;
      float z = fX + width;//z
      float w = fY + m_fKeyHeight - height;//w

      float nw = width;
      float nh = m_fKeyHeight - height;

      float uoffs = 0;
      float v = 1.0f;
      float u = 1.0f;

      m_pKeyTexture.Draw(x, y, nw, nh, uoffs, 0.0f, u, v, (int)KeyColor);
      /*
            VertexBuffer m_vbBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
                                              4, GUIGraphicsContext.DX9Device, 
                                              0, CustomVertex.TransformedColoredTextured.Format, 
                                              Pool.Managed);

            CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vbBuffer.Lock(0,0);
            verts[0].X=x- 0.5f; verts[0].Y=y+nh- 0.5f;verts[0].Z= 0.0f;verts[0].Rhw=1.0f ;
            verts[0].Color = (int)KeyColor;
            verts[0].Tu = uoffs;
            verts[0].Tv = v;

            verts[1].X= x- 0.5f; verts[1].Y=y- 0.5f;verts[1].Z= 0.0f; verts[1].Rhw=1.0f ;
            verts[1].Color = (int)KeyColor;
            verts[1].Tu = uoffs;
            verts[1].Tv = 0.0f;

            verts[2].X= x+nw- 0.5f; verts[2].Y=y+nh- 0.5f;verts[2].Z= 0.0f;verts[2].Rhw=1.0f;
            verts[2].Color = (int)KeyColor;
            verts[2].Tu = uoffs+u;
            verts[2].Tv = v;

            verts[3].X=  x+nw- 0.5f;verts[3].Y=  y- 0.5f;verts[3].Z=   0.0f;verts[3].Rhw=  1.0f ;
            verts[3].Color = (int)KeyColor;
            verts[3].Tu = uoffs+u;
            verts[3].Tv = 0.0f;


            m_vbBuffer.Unlock();

            GUIGraphicsContext.DX9Device.SetTexture( 0, m_pKeyTexture);

            // Render the image
            GUIGraphicsContext.DX9Device.SetStreamSource( 0, m_vbBuffer, 0);
            GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
            GUIGraphicsContext.DX9Device.DrawPrimitives( PrimitiveType.TriangleStrip, 0, 2 );
        

            // unset the texture and palette or the texture caching crashes because the runtime still has a reference
            GUIGraphicsContext.DX9Device.SetTexture( 0, null);
            m_vbBuffer.Dispose();
      */
      // Draw the key text. If key name is, use a slightly smaller font.
      float fW = 0;
      float fH = 0;
      float fposX = (x + z) / 2.0f;
      float fposY = (y + w) / 2.0f;
      fposX -= GUIGraphicsContext.OffsetX;
      fposY -= GUIGraphicsContext.OffsetY;
      if (key.strName.Length > 1 && Char.IsUpper(key.strName[1]))
      {
        m_Font12.GetTextExtent(strName, ref fW, ref fH);
        fposX -= (fW / 2);
        fposY -= (fH / 2);
        m_Font12.DrawText(fposX, fposY, TextColor, strName, GUIControl.Alignment.ALIGN_LEFT, -1);
      }
      else
      {
        m_Font18.GetTextExtent(strName, ref fW, ref fH);
        fposX -= (fW / 2);
        fposY -= (fH / 2);
        m_Font18.DrawText(fposX, fposY, TextColor, strName, GUIControl.Alignment.ALIGN_LEFT, -1);
      }
    }

    void DrawTextBox(float timePassed, int x1, int y1, int x2, int y2)
    {
      //long lColor=0xaaffffff;

      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      GUIGraphicsContext.ScalePosToScreenResolution(ref x2, ref y2);
      x1 += GUIGraphicsContext.OffsetX;
      x2 += GUIGraphicsContext.OffsetX;
      y1 += GUIGraphicsContext.OffsetY;
      y2 += GUIGraphicsContext.OffsetY;
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

    void DrawText(int x, int y)
    {
      GUIGraphicsContext.ScalePosToScreenResolution(ref x, ref y);
      x += GUIGraphicsContext.OffsetX;
      y += GUIGraphicsContext.OffsetY;
      string strTxt = m_strData;
      if (_Password)
      {
        strTxt = "";
        for (int i = 0; i < m_strData.Length; ++i) strTxt += "*";
      }

      m_FontSearchText.DrawText((float)x, (float)y, COLOR_SEARCHTEXT, strTxt, GUIControl.Alignment.ALIGN_LEFT, -1);


      // Draw blinking caret using line primitives.
      TimeSpan ts = DateTime.Now - m_CaretTimer;
      if ((ts.TotalSeconds % fCARET_BLINK_RATE) < fCARET_ON_RATIO)
      {
        string strLine = strTxt.Substring(0, m_iPos);

        float fCaretWidth = 0.0f;
        float fCaretHeight = 0.0f;
        m_FontSearchText.GetTextExtent(strLine, ref fCaretWidth, ref fCaretHeight);
        x += (int)fCaretWidth;
        m_FontSearchText.DrawText((float)x, (float)y, 0xff202020, "|", GUIControl.Alignment.ALIGN_LEFT, -1);

      }
    }

    void RenderKeyboardLatin(float timePassed)
    {
      // Show text and caret
      DrawTextBox(timePassed, 64, 208, 576, 248);
      DrawText(68, 208);


      int x1 = 64;
      int y1 = 250;
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      x1 += GUIGraphicsContext.OffsetX;
      y1 += GUIGraphicsContext.OffsetY;
      // Draw each row
      float fY = y1;
      ArrayList keyBoard = (ArrayList)m_KeyboardList[(int)m_iCurrBoard];
      for (int row = 0; row < m_dwMaxRows; ++row, fY += m_fKeyHeight)
      {
        float fX = x1;
        float fWidthSum = 0.0f;
        ArrayList keyRow = (ArrayList)keyBoard[row];
        int dwIndex = 0;
        for (int i = 0; i < keyRow.Count; i++)
        {
          // Determine key name
          Key key = (Key)keyRow[i];
          long selKeyColor = 0xffffffff;
          long selTextColor = COLOR_NORMAL;

          // Handle special key coloring
          switch (key.xKey)
          {
            case Xkey.XK_SHIFT:
              switch (m_iCurrBoard)
              {
                case KeyboardTypes.TYPE_ALPHABET:
                case KeyboardTypes.TYPE_ACCENTS:
                  if (m_bIsShiftOn)
                    selKeyColor = COLOR_PRESSED;
                  break;
                case KeyboardTypes.TYPE_SYMBOLS:
                  selKeyColor = COLOR_DISABLED;
                  selTextColor = COLOR_FONT_DISABLED;
                  break;
              }
              break;
            case Xkey.XK_CAPSLOCK:
              switch (m_iCurrBoard)
              {
                case KeyboardTypes.TYPE_ALPHABET:
                case KeyboardTypes.TYPE_ACCENTS:
                  if (m_bIsCapsLockOn)
                    selKeyColor = COLOR_PRESSED;
                  break;
                case KeyboardTypes.TYPE_SYMBOLS:
                  selKeyColor = COLOR_DISABLED;
                  selTextColor = COLOR_FONT_DISABLED;
                  break;
              }
              break;
            case Xkey.XK_ACCENTS:
              selKeyColor = COLOR_INVISIBLE;
              selTextColor = COLOR_INVISIBLE;
              break;
          }

          // Highlight the current key
          if (row == m_iCurrRow && dwIndex == m_iCurrKey)
            selKeyColor = COLOR_HIGHLIGHT;

          RenderKey(fX + fWidthSum, fY, key, selKeyColor, selTextColor);

          int width = key.dwWidth;
          GUIGraphicsContext.ScaleHorizontal(ref width);
          fWidthSum += width;

          // There's a slightly larger gap between the leftmost keys (mode
          // keys) and the main keyboard
          if (dwIndex == 0)
            width = GAP2_WIDTH;
          else
            width = GAP_WIDTH;
          GUIGraphicsContext.ScaleHorizontal(ref width);
          fWidthSum += width;

          ++dwIndex;
        }
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

  }
}
