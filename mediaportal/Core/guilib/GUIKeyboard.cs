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

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class implementing a GUIKeyboard.
  /// </summary>
  public abstract class GUIKeyboard : GUIControl
  {
    [XMLSkinElement("keyboardPosX")] protected int _keyboardPosX = 64;
    [XMLSkinElement("keyboardPosY")] protected int _keyboardPosY = 250;
    [XMLSkinElement("keyWidth")] protected int _keyWidth = 34;
    [XMLSkinElement("keyHeight")] protected int _keyHeight = 54;
    [XMLSkinElement("keyHSpacing")] protected int _keyHorizontalSpacing = 0;
    [XMLSkinElement("keyVSpacing")] protected int _keyVerticalSpacing = 2;
    [XMLSkinElement("modeKeySpacing")] protected int _modeKeySpacing = 4;
    [XMLSkinElement("modeKeyWidth")] protected int _modeKeyWidth = 110;
    [XMLSkinElement("searchModeKeyWidth")] protected int _searchModeKeyWidth = 130;
    [XMLSkinElement("label")] protected string _labelText = "";
    [XMLSkinElement("labelBoxPosX")] protected int _labelBoxPosX = 64;
    [XMLSkinElement("labelBoxPosY")] protected int _labelBoxPosY = 218;
    [XMLSkinElement("labelBoxWidth")] protected int _labelBoxWidth = -1; //calculated
    [XMLSkinElement("labelBoxHeight")] protected int _labelBoxHeight = 30;
    [XMLSkinElement("labelBoxTexture")] protected string _labelBoxTexture = "white.bmp";
    [XMLSkinElement("labelAlign")] protected Alignment _labelAlign = Alignment.ALIGN_LEFT;
    [XMLSkinElement("labelOffX")] protected int _labelOffX = 18;
    [XMLSkinElement("labelOffY")] protected int _labelOffY = 2;
    [XMLSkinElement("labelFont")] protected string _labelFont = "font10";
    [XMLSkinElement("labelColor")] protected long _labelColor = 0xff000000;
    [XMLSkinElement("labelShadowAngle")] protected int _labelShadowAngle = 0;
    [XMLSkinElement("labelShadowDistance")] protected int _labelShadowDistance = 0;
    [XMLSkinElement("labelShadowColor")] protected long _labelShadowColor = 0xFF000000;
    [XMLSkinElement("inputTextBoxPosX")] protected int _inputTextBoxPosX = 64;
    [XMLSkinElement("inputTextBoxPosY")] protected int _inputTextBoxPosY = 218;
    [XMLSkinElement("inputTextBoxWidth")] protected int _inputTextBoxWidth = -1; //calculated
    [XMLSkinElement("inputTextBoxHeight")] protected int _inputTextBoxHeight = 30;
    [XMLSkinElement("inputTextBoxTexture")] protected string _inputTextBoxTexture = "white.bmp";
    [XMLSkinElement("showLabelAsInitialText")] protected bool _showLabelAsInitialText = false;
    [XMLSkinElement("inputTextAlign")] protected Alignment _inputTextAlign = Alignment.ALIGN_LEFT;
    [XMLSkinElement("inputTextOffX")] protected int _inputTextOffX = 18;
    [XMLSkinElement("inputTextOffY")] protected int _inputTextOffY = 2;
    [XMLSkinElement("inputTextFont")] protected string _inputTextFont = "font10";
    [XMLSkinElement("inputTextColor")] protected long _inputTextColor = 0xff000000;
    [XMLSkinElement("inputTextShadowAngle")] protected int _inputTextShadowAngle = 0;
    [XMLSkinElement("inputTextShadowDistance")] protected int _inputTextShadowDistance = 0;
    [XMLSkinElement("inputTextShadowColor")] protected long _inputTextShadowColor = 0xFF000000;
    [XMLSkinElement("charKeyFont")] protected string _charKeyFont = "font14";
    [XMLSkinElement("namedKeyFont")] protected string _namedKeyFont = "font10";
    [XMLSkinElement("keyTextShadowAngle")] protected int _keyTextShadowAngle = 0;
    [XMLSkinElement("keyTextShadowDistance")] protected int _keyTextShadowDistance = 0;
    [XMLSkinElement("keyTextShadowColor")] protected long _keyTextShadowColor = 0xFF000000;
    [XMLSkinElement("keyTextureFocus")] protected string _keyTextureFocus = "keyNF.bmp";
    [XMLSkinElement("keyTextureNoFocus")] protected string _keyTextureNoFocus = "keyNF.bmp";
    [XMLSkinElement("keyHighlightColor")] protected long _keyHighlightColor = 0xff00ff00;
    [XMLSkinElement("keyPressedColor")] protected long _keyPressedColor = 0xff808080;
    [XMLSkinElement("keyDisabledColor")] protected long _keyDisabledColor = 0xffffffff;
    [XMLSkinElement("keyFontColor")] protected long _keyFontColor = 0xff000000;
    [XMLSkinElement("keySelFontColor")] protected long _keySelFontColor = 0xff000000;
    [XMLSkinElement("keyDisabledFontColor")] protected long _keyDisabledFontColor = 0xff808080;
    [XMLSkin("keyTextureFocus", "border")] protected string _strBorderKTF = "";

    [XMLSkin("keyTextureFocus", "position")] protected GUIImage.BorderPosition _borderPositionKTF =
      GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;

    [XMLSkin("keyTextureFocus", "textureRepeat")] protected bool _borderTextureRepeatKTF = false;
    [XMLSkin("keyTextureFocus", "textureRotate")] protected bool _borderTextureRotateKTF = false;
    [XMLSkin("keyTextureFocus", "texture")] protected string _borderTextureFileNameKTF = "image_border.png";
    [XMLSkin("keyTextureFocus", "colorKey")] protected long _borderColorKeyKTF = 0xFFFFFFFF;
    [XMLSkin("keyTextureFocus", "corners")] protected bool _borderHasCornersKTF = false;
    [XMLSkin("keyTextureFocus", "cornerRotate")] protected bool _borderCornerTextureRotateKTF = true;
    [XMLSkin("keyTextureNoFocus", "border")] protected string _strBorderKTNF = "";

    [XMLSkin("keyTextureNoFocus", "position")] protected GUIImage.BorderPosition _borderPositionKTNF =
      GUIImage.BorderPosition.BORDER_IMAGE_OUTSIDE;

    [XMLSkin("keyTextureNoFocus", "textureRepeat")] protected bool _borderTextureRepeatKTNF = false;
    [XMLSkin("ketTextureNoFocus", "textureRotate")] protected bool _borderTextureRotateKTNF = false;
    [XMLSkin("keyTextureNoFocus", "texture")] protected string _borderTextureFileNameKTNF = "image_border.png";
    [XMLSkin("keyTextureNoFocus", "colorKey")] protected long _borderColorKeyKTNF = 0xFFFFFFFF;
    [XMLSkin("keyTextureNoFocus", "corners")] protected bool _borderHasCornersKTNF = false;
    [XMLSkin("keyTextureNoFocus", "cornerRotate")] protected bool _borderCornerTextureRotateKTNF = true;

    #region constants

    // How often (per second) the caret blinks
    private const float fCARET_BLINK_RATE = 1.0f;

    // During the blink period, the amount the caret is visible. 0.5 equals
    // half the time, 0.75 equals 3/4ths of the time, etc.
    public const float fCARET_ON_RATIO = 0.5f;

    // Controller repeat values
    public const float fINITIAL_REPEAT = 0.333f; // 333 mS recommended for first repeat
    public const float fSTD_REPEAT = 0.085f; // 85 mS recommended for repeat rate

    // Maximum number of characters in string
    protected int _max_chars = 64;

    public int MAX_CHARS
    {
      get { return _max_chars; }
    }

    protected float fLabelBoxWidth = 0.0f;
    protected float fTextBoxWidth = 0.0f;
    public bool _usingKeyboard;
    public char _currentKeyb = (char)0;
    public char _previousKey = (char)0;
    public DateTime _timerKey = DateTime.Now;
    private bool _isAllocated = false;
    private bool _labelClearedFromText = false;

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
      XK_SMS, //SMS Toggle

      // Special Search-Keys
      XK_SEARCH_START_WITH = 0x11000, // to search music that starts with string
      XK_SEARCH_CONTAINS, // ...contains string
      XK_SEARCH_ENDS_WITH, // ...ends with string
      XK_SEARCH_IS, // is the search text
      XK_SEARCH_ALBUM, // search for album
      XK_SEARCH_TITLE, // search for title
      XK_SEARCH_ARTIST, // search for artist
      XK_SEARCH_GENERE, // search for genere

      // SMS keyboard keys
      XK_SMS0 = 0x12000,
      XK_SMS1,
      XK_SMS2,
      XK_SMS3,
      XK_SMS4,
      XK_SMS5,
      XK_SMS6,
      XK_SMS7,
      XK_SMS8,
      XK_SMS9
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
    public float _keyHeightScaled;
    public int _maxRows;
    public bool _pressedEnter;
    public DateTime _caretTimer = DateTime.Now;
    public bool _password;
    public bool _useSearchLayout;
    public int keyboardX, keyboardY;
    public GUIFont _fontCharKey;
    public GUIFont _fontNamedKey;
    public GUIFont _fontSearchText;

    public GUIImage labelBox;
    public GUILabelControl label;
    public GUIImage inputTextBox;
    public GUILabelControl inputText;
    public GUILabelControl inputTextCaret;

    // added by Agree
    public int _searchKind; // 0=Starts with, 1=Contains, 2=Ends with
    //

    public ArrayList _keyboardList = new ArrayList(); // list of rows = keyboard

    #endregion

    #region abstract methods

    protected abstract void MoveLeft();
    protected abstract void MoveRight();
    protected abstract void Press(char k);
    protected abstract void RenderKeyboardLatin(float timePassed);
    public abstract void InitializeInstance();

    #endregion

    #region SMS style for virtual

    private int smsLastKeyPressTime = 0;
    private int smsLastKeyPressed = -1;
    private int smsLastKeyInternalPos = 0;
    private bool smsLastShiftState = false;
    private bool _useSmsStyleTextInsertion = true;

    private string[] smsKeyMap =
      {
        " 0",
        ".!?-*_\\/1",
        "abcäáà2",
        "deféè3",
        "ghií4",
        "jkl5",
        "mnoóö6",
        "pqrsß7",
        "tuvúü8",
        "wxyz9"
      };

    #endregion

    public bool SmsStyleText
    {
      get { return _useSmsStyleTextInsertion; }
      set
      {
        if (value != _useSmsStyleTextInsertion)
        {
          if (Password) return; //sms is disabled during password input - we cannot see chars!
          smsLastKeyPressed = -1;
          _lastColumn = 0;
          _useSmsStyleTextInsertion = value;
          InitBoard();
        }
        GUIPropertyManager.SetProperty("#VirtualKeyboard.SMSStyleInput", SmsStyleText.ToString().ToLowerInvariant());
      }
    }


    public class Key
    {
      public Xkey xKey; // virtual key code
      public int dwWidth = 0; // width of the key
      public string name = ""; // name of key when vKey >= 0x10000
      public bool inFocus = false;
      public GUIButtonControl button;

      public Key(Xkey key, int iwidth, GUIKeyboard kb)
      {
        xKey = key;
        dwWidth = iwidth;

        // Create a button control template.
        if (button != null)
        {
          button.Dispose();
        }
        button = new GUIButtonControl(kb.GetID, -1, 0, 0, 0, 0, kb._keyTextureFocus, kb._keyTextureNoFocus,
                                      kb._keyTextShadowAngle, kb._keyTextShadowDistance, kb._keyTextShadowColor);

        button.SetBorderTF(
          kb._strBorderKTF,
          kb._borderPositionKTF,
          kb._borderTextureRepeatKTF,
          kb._borderTextureRotateKTF,
          kb._borderTextureFileNameKTF,
          kb._borderColorKeyKTF,
          kb._borderHasCornersKTF,
          kb._borderCornerTextureRotateKTF);

        button.SetBorderTNF(
          kb._strBorderKTNF,
          kb._borderPositionKTNF,
          kb._borderTextureRepeatKTNF,
          kb._borderTextureRotateKTNF,
          kb._borderTextureFileNameKTNF,
          kb._borderColorKeyKTNF,
          kb._borderHasCornersKTNF,
          kb._borderCornerTextureRotateKTNF);

        button.AllocResources();

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
          case Xkey.XK_SMS:
            name = "SMS";
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
          case Xkey.XK_SMS0:
            name = "0 [ ]";
            break;
        }
      }
    }

    public GUIKeyboard(int dwParentID)
      : base(dwParentID) {}

    /// <summary>
    /// The constructor of the GUIKeyboard class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    public GUIKeyboard(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      FinalizeConstruction();
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
    }

    public override void AllocResources()
    {
      _fontNamedKey = GUIFontManager.GetFont(_namedKeyFont);
      _fontCharKey = GUIFontManager.GetFont(_charKeyFont);
      _fontSearchText = GUIFontManager.GetFont(_inputTextFont);

      labelBox = new GUIImage(GetID, 1, 0, 0, 10, 10, _labelBoxTexture, 1);
      labelBox.AllocResources();

      label = new GUILabelControl(GetID);
      label.FontName = _labelFont;
      label.SetShadow(_labelShadowAngle, _labelShadowDistance, _labelShadowColor);
      label.AllocResources();

      inputTextBox = new GUIImage(GetID, 1, 0, 0, 10, 10, _inputTextBoxTexture, 1);
      inputTextBox.AllocResources();

      inputText = new GUILabelControl(GetID);
      inputText.FontName = _inputTextFont;
      inputText.SetShadow(_inputTextShadowAngle, _inputTextShadowDistance, _inputTextShadowColor);
      inputText.AllocResources();

      inputTextCaret = new GUILabelControl(GetID);
      inputTextCaret.FontName = _inputTextFont;
      inputTextCaret.SetShadow(_inputTextShadowAngle, _inputTextShadowDistance, _inputTextShadowColor);
      inputTextCaret.AllocResources();

      base.AllocResources();
      _isAllocated = true;
    }

    public override void Dispose()
    {
      if (_isAllocated)
      {
        // Free the keyboard directx resources.
        inputTextBox.Dispose();
        inputText.Dispose();
        inputTextCaret.Dispose();

        ArrayList keyBoard = null;
        for (int kb = 0; kb < _keyboardList.Count; kb++)
          keyBoard = (ArrayList)_keyboardList[kb];
        for (int row = 0; row < _maxRows; ++row)
        {
          ArrayList keyRow = (ArrayList)keyBoard[row];
          for (int i = 0; i < keyRow.Count; i++)
          {
            Key key = (Key)keyRow[i];
            key.button.Dispose();
          }
        }

        base.Dispose();
        _isAllocated = false;
      }
    }

    public void Reset()
    {
      _capsLockTurnedOn = false;
      _shiftTurnedOn = false;
      _state = State.STATE_KEYBOARD;
      _position = 0;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 0;
      _keyHeightScaled = _keyHeight;
      _maxRows = 5;
      _pressedEnter = false;
      _caretTimer = DateTime.Now;

      _searchKind = (int)SearchKinds.SEARCH_CONTAINS; // default search Contains

      _password = false;
      _textEntered = "";

      int height = _keyHeight;
      GUIGraphicsContext.ScaleVertical(ref height);
      _keyHeightScaled = height;

      // Free and reallocate resources to incorporate new keys added to the keyboard.
      RestoreToDefault();
      InitBoard();
    }

    public delegate void TextChangedEventHandler(int kindOfSearch, string evtData);

    public event TextChangedEventHandler TextChanged;

    public virtual void OnTextChanged()
    {
      if (TextChanged != null)
      {
        TextChanged(_searchKind, _textEntered);
      }
    }

    public void SelectActiveButton(float x, float y)
    {
      // Draw each row
      int y1 = keyboardY;
      int x1 = keyboardX;
      float fY = y1;
      ArrayList keyBoard = (ArrayList)_keyboardList[(int)_currentKeyboard];
      for (int row = 0; row < _maxRows; ++row, fY += _keyHeightScaled)
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
          if (x >= fX + fWidthSum && x <= fX + fWidthSum + width)
          {
            if (y >= fY && y < fY + _keyHeightScaled)
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
            width = _modeKeySpacing;
            GUIGraphicsContext.ScaleHorizontal(ref width);
            fWidthSum += width;
          }
          else
          {
            width = _keyHorizontalSpacing;
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

      switch (action.wID)
      {
        case Action.ActionType.REMOTE_0:
          ProcessSmsInsertion(0);
          break;
        case Action.ActionType.REMOTE_1:
          ProcessSmsInsertion(1);
          break;
        case Action.ActionType.REMOTE_2:
          ProcessSmsInsertion(2);
          break;
        case Action.ActionType.REMOTE_3:
          ProcessSmsInsertion(3);
          break;
        case Action.ActionType.REMOTE_4:
          ProcessSmsInsertion(4);
          break;
        case Action.ActionType.REMOTE_5:
          ProcessSmsInsertion(5);
          break;
        case Action.ActionType.REMOTE_6:
          ProcessSmsInsertion(6);
          break;
        case Action.ActionType.REMOTE_7:
          ProcessSmsInsertion(7);
          break;
        case Action.ActionType.REMOTE_8:
          ProcessSmsInsertion(8);
          break;
        case Action.ActionType.REMOTE_9:
          ProcessSmsInsertion(9);
          break;
        case Action.ActionType.ACTION_MOVE_LEFT:
          if (_useSmsStyleTextInsertion && _currentKey == 0)
          {
            Press(Xkey.XK_BACKSPACE);
            smsLastKeyPressed = -1;
            return;
          }
          break;
        case Action.ActionType.ACTION_MOVE_UP:
          if (_useSmsStyleTextInsertion && _currentRow == 0)
          {
            _shiftTurnedOn = !_shiftTurnedOn;
            return;
          }
          break;
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
              char chKey = (char)action.m_key.KeyChar;
              if ((chKey >= '0' && chKey <= '9')) //Make sure it's only for the remote
              {
                if (_useSmsStyleTextInsertion)
                {
                  ProcessSmsInsertion(int.Parse(chKey.ToString()));
                }
                else
                {
                  Press(chKey);
                }
              }
              else
              {
                Press(chKey);
              }
            }
            if (action.m_key.KeyChar == 8)
            {
              Press(Xkey.XK_BACKSPACE);
            }
          }
          break;
        case Action.ActionType.ACTION_TOGGLE_SMS_INPUT:
          SmsStyleText = !SmsStyleText;
          break;
      }
    }

    private void ProcessSmsInsertion(int keyPressed)
    {
      if (_useSmsStyleTextInsertion)
      {
        if (smsLastKeyPressTime + 1000 < System.Environment.TickCount || smsLastKeyPressed != keyPressed)
        {
          smsLastKeyInternalPos = 0;

          string tmpKeys = smsKeyMap[keyPressed];
          //if (_shiftTurnedOn) tmpKeys = tmpKeys.ToUpper();
          if ((_capsLockTurnedOn && !_shiftTurnedOn) || (!_capsLockTurnedOn && _shiftTurnedOn))
          {
            tmpKeys = tmpKeys.ToUpper();
            smsLastShiftState = true;
          }
          else
          {
            smsLastShiftState = false;
          }
          char tmpChar = tmpKeys[smsLastKeyInternalPos];

          /*Action tmpAction = new Action(new MediaPortal.GUI.Library.Key(tmpChar, (int)tmpChar),
                                        Action.ActionType.ACTION_KEY_PRESSED, 0, 0);*/
          Press(tmpChar);
          //OnAction(tmpAction);
        }
        else
        {
          smsLastKeyInternalPos++;
          if (smsLastKeyInternalPos >= smsKeyMap[keyPressed].Length) smsLastKeyInternalPos = 0;

          Press(Xkey.XK_BACKSPACE);

          string tmpKeys = smsKeyMap[keyPressed];
          if (smsLastShiftState) tmpKeys = tmpKeys.ToUpper();
          char tmpChar = tmpKeys[smsLastKeyInternalPos];

          /*Action tmpAction = new Action(new MediaPortal.GUI.Library.Key(tmpChar, (int)tmpChar),
                                        Action.ActionType.ACTION_KEY_PRESSED, 0, 0);*/
          Press(tmpChar);

          //OnAction(tmpAction);
        }
        smsLastKeyPressed = keyPressed;
        smsLastKeyPressTime = System.Environment.TickCount;
      }
      else
      {
        char tmpChar = (char)('0' + keyPressed);
        Action tmpAction = new Action(new MediaPortal.GUI.Library.Key(tmpChar, (int)tmpChar),
                                      Action.ActionType.ACTION_KEY_PRESSED, 0, 0);

        OnAction(tmpAction);
      }
    }


    protected void Close()
    {
      IsVisible = false;
    }

    public override void Render(float timePassed)
    {
      lock (this)
      {
        RenderKeyboardLatin(timePassed);
        base.Render(timePassed);
      }
    }

    protected virtual void RestoreToDefault()
    {
      // Restore keyboard to default state
      _currentRow = 0;
      _currentKey = 0;
      _lastColumn = 1;
      _currentKeyboard = KeyboardTypes.TYPE_ALPHABET;
      _capsLockTurnedOn = false;
      _shiftTurnedOn = false;
      _textEntered = "";
      _position = 0;
      int height = _keyHeight;
      GUIGraphicsContext.ScaleVertical(ref height);
      _keyHeightScaled = height;
      _maxRows = 5;
    }

    protected virtual void InitBoard()
    {
      // Destroy old keyboard
      _keyboardList.Clear();

      //-------------------------------------------------------------------------
      // Alpha keyboard
      //-------------------------------------------------------------------------

      ArrayList keyBoard = new ArrayList();

      // First row is Done, 1-0
      ArrayList keyRow = new ArrayList();
      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_OK, _searchModeKeyWidth, this));
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_OK, _modeKeyWidth, this));
      }
      if (!_useSmsStyleTextInsertion)
      {
        keyRow.Add(new Key(Xkey.XK_1, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_2, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_3, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_4, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_5, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_6, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_7, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_8, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_9, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_0, _keyWidth, this));
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SMS1, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
        keyRow.Add(new Key(Xkey.XK_SMS2, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
        keyRow.Add(new Key(Xkey.XK_SMS3, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
      }

      keyBoard.Add(keyRow);

      // Second row is Shift, A-J
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, _modeKeyWidth, this));
      }
      if (!_useSmsStyleTextInsertion)
      {
        keyRow.Add(new Key(Xkey.XK_A, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_B, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_C, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_D, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_E, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_F, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_G, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_H, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_I, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_J, _keyWidth, this));
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SMS4, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
        keyRow.Add(new Key(Xkey.XK_SMS5, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
        keyRow.Add(new Key(Xkey.XK_SMS6, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
      }

      keyBoard.Add(keyRow);

      // Third row is Caps Lock, K-T
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, _modeKeyWidth, this));
      }
      if (!_useSmsStyleTextInsertion)
      {
      keyRow.Add(new Key(Xkey.XK_K, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_L, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_M, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_N, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_O, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_P, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_Q, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_R, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_S, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_T, _keyWidth, this));
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SMS7, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
        keyRow.Add(new Key(Xkey.XK_SMS8, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
        keyRow.Add(new Key(Xkey.XK_SMS9, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
      }

      keyBoard.Add(keyRow);

      // Fourth row is Accents, U-Z, Backspace
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_ACCENTS, _modeKeyWidth, this));
      }
      if (!_useSmsStyleTextInsertion)
      {
        keyRow.Add(new Key(Xkey.XK_U, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_V, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_W, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_X, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_Y, _keyWidth, this));
        keyRow.Add(new Key(Xkey.XK_Z, _keyWidth, this));
      }
      else
      {
        //keyRow.Add(new Key(Xkey.XK_NULL, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
        keyRow.Add(new Key(Xkey.XK_SMS0, (_keyWidth * 6) + (_keyHorizontalSpacing * 5), this));
        //keyRow.Add(new Key(Xkey.XK_NULL, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
      }
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (_keyWidth * 4) + (_keyHorizontalSpacing * 3), this));

      keyBoard.Add(keyRow);

      // Fifth row is SMS, Space, Left, Right
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_ACCENTS, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        //keyRow.Add(new Key(Xkey.XK_NULL, _modeKeyWidth, this));
        keyRow.Add(new Key(Xkey.XK_SMS, _modeKeyWidth, this));
      }
      //keyRow.Add(new Key(Xkey.XK_SMS, _modeKeyWidth, this));
      keyRow.Add(new Key(Xkey.XK_SPACE, (_keyWidth * 6) + (_keyHorizontalSpacing * 5), this));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
      keyBoard.Add(keyRow);

      // Add the alpha keyboard to the list
      _keyboardList.Add(keyBoard);

      //-------------------------------------------------------------------------
      // Symbol keyboard
      //-------------------------------------------------------------------------

      keyBoard = new ArrayList();

      // First row
      keyRow = new ArrayList();
      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_OK, _searchModeKeyWidth, this));
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_OK, _modeKeyWidth, this));
      }
      keyRow.Add(new Key(Xkey.XK_LPAREN, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_RPAREN, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_AMPER, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_UNDERS, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CARET, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_PERCENT, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_BSLASH, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_FSLASH, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_AT, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_NSIGN, _keyWidth, this));

      keyBoard.Add(keyRow);

      // Second row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, _modeKeyWidth, this));
      }

      keyRow.Add(new Key(Xkey.XK_LBRACK, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_RBRACK, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_DOLLAR, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_POUND_SIGN, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_YEN_SIGN, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_EURO_SIGN, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_SEMI, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_COLON, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_QUOTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_DQUOTE, _keyWidth, this));
      keyBoard.Add(keyRow);

      // Third row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, _modeKeyWidth, this));
      }

      keyRow.Add(new Key(Xkey.XK_LT, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_GT, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_QMARK, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_EXCL, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_INVERTED_QMARK, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_INVERTED_EXCL, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_DASH, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_STAR, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_PLUS, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_EQUAL, _keyWidth, this));
      keyBoard.Add(keyRow);

      // Fourth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_ALPHABET, _modeKeyWidth, this));
      }

      keyRow.Add(new Key(Xkey.XK_LBRACE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_RBRACE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_LT_DBL_ANGLE_QUOTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_RT_DBL_ANGLE_QUOTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_COMMA, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_PERIOD, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (_keyWidth * 4) + (_keyHorizontalSpacing * 3), this));
      keyBoard.Add(keyRow);

      // Fifth row is Accents, Space, Left, Right
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_ALPHABET, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        //keyRow.Add(new Key(Xkey.XK_NULL, _modeKeyWidth, this));
        keyRow.Add(new Key(Xkey.XK_SMS, _modeKeyWidth, this));
      }

      keyRow.Add(new Key(Xkey.XK_SPACE, (_keyWidth * 6) + (_keyHorizontalSpacing * 5), this));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
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
      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_OK, _searchModeKeyWidth, this));
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_OK, _modeKeyWidth, this));
      }
      keyRow.Add(new Key(Xkey.XK_CAP_A_RING, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_A_DIAERESIS, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_O_DIAERESIS, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_A_GRAVE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_A_ACUTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_A_CIRCUMFLEX, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_I_GRAVE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_I_ACUTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_I_CIRCUMFLEX, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_I_DIAERESIS, _keyWidth, this));
      keyBoard.Add(keyRow);

      // Second row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SEARCH_CONTAINS, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, _modeKeyWidth, this));
      }

      //Danish - Norwegian
      keyRow.Add(new Key(Xkey.XK_CAP_A_RING, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_AE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_O_STROKE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_C_CEDILLA, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_E_GRAVE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_E_ACUTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_E_CIRCUMFLEX, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_E_DIAERESIS, _keyWidth, this));

      keyBoard.Add(keyRow);

      // Third row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SHIFT, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, _modeKeyWidth, this));
      }

      // German
      keyRow.Add(new Key(Xkey.XK_CAP_U_DIAERESIS, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_O_DIAERESIS, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_A_DIAERESIS, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_SM_SHARP_S, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_O_GRAVE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_O_ACUTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_O_CIRCUMFLEX, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_O_TILDE, _keyWidth, this));

      keyBoard.Add(keyRow);

      // Fourth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_CAPSLOCK, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        keyRow.Add(new Key(Xkey.XK_SYMBOLS, _modeKeyWidth, this));
      }

      keyRow.Add(new Key(Xkey.XK_CAP_N_TILDE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_U_GRAVE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_U_ACUTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_U_CIRCUMFLEX, _keyWidth, this));

      keyRow.Add(new Key(Xkey.XK_CAP_Y_ACUTE, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_CAP_Y_DIAERESIS, _keyWidth, this));
      keyRow.Add(new Key(Xkey.XK_BACKSPACE, (_keyWidth * 4) + (_keyHorizontalSpacing * 3), this));
      keyBoard.Add(keyRow);

      // Fifth row
      keyRow = new ArrayList();

      if (_useSearchLayout)
      {
        keyRow.Add(new Key(Xkey.XK_SYMBOLS, _modeKeyWidth, this)); // Searchkeyboard
      }
      else
      {
        //keyRow.Add(new Key(Xkey.XK_NULL, _modeKeyWidth, this));
        keyRow.Add(new Key(Xkey.XK_SMS, _modeKeyWidth, this));
      }

      keyRow.Add(new Key(Xkey.XK_SPACE, (_keyWidth * 6) + (_keyHorizontalSpacing * 5), this));
      keyRow.Add(new Key(Xkey.XK_ARROWLEFT, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
      keyRow.Add(new Key(Xkey.XK_ARROWRIGHT, (_keyWidth * 2) + (_keyHorizontalSpacing * 1), this));
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
      ArrayList board = (ArrayList)_keyboardList[iBoard];
      ArrayList row = (ArrayList)board[iRow];
      row[iKey] = newkey;
    }

    protected void PressCurrent()
    {
      if (_currentKey == -1)
      {
        return;
      }

      ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
      ArrayList row = (ArrayList)board[_currentRow];
      Key key = (Key)row[_currentKey];

      // Press it
      Press(key.xKey);
    }

    protected void Press(Xkey xk)
    {
      ClearLabelAsInitialText();

      if (xk == Xkey.XK_NULL) // happens in Japanese keyboard (keyboard type)
      {
        xk = Xkey.XK_SPACE;
      }

      // If the key represents a character, add it to the word
      if (((uint)xk) < 0x10000 && xk != Xkey.XK_ARROWLEFT && xk != Xkey.XK_ARROWRIGHT)
      {
        // Don't add more than the maximum characters, and don't allow 
        // text to exceed the width of the text entry field
        if (_textEntered.Length < MAX_CHARS)
        {
          float fWidth = 0, fHeight = 0;
          _fontCharKey.GetTextExtent(_textEntered, ref fWidth, ref fHeight);

          if (fWidth < (GUIGraphicsContext.ScaleHorizontal((int) fTextBoxWidth)))
          {
            if (_position >= _textEntered.Length)
            {
              _textEntered += GetChar(xk);
              if (TextChanged != null)
              {
                TextChanged(_searchKind, _textEntered);
              }
            }
            else
            {
              _textEntered = _textEntered.Insert(_position, GetChar(xk));
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
          case Xkey.XK_SMS:
            //_useSmsStyleTextInsertion = !_useSmsStyleTextInsertion;
            SmsStyleText = !SmsStyleText;
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
            _searchKind = (int)SearchKinds.SEARCH_STARTS_WITH;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_CONTAINS:
            _searchKind = (int)SearchKinds.SEARCH_ENDS_WITH;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_ENDS_WITH:
            _searchKind = (int)SearchKinds.SEARCH_IS;
            SetSearchKind();
            break;

          case Xkey.XK_SEARCH_START_WITH:
            _searchKind = (int)SearchKinds.SEARCH_CONTAINS;
            SetSearchKind();
            break;
            // code by Agree ends here
            //

          case Xkey.XK_SMS0:
            ProcessSmsInsertion(0);
            break;
          case Xkey.XK_SMS1:
            ProcessSmsInsertion(1);
            break;
          case Xkey.XK_SMS2:
            ProcessSmsInsertion(2);
            break;
          case Xkey.XK_SMS3:
            ProcessSmsInsertion(3);
            break;
          case Xkey.XK_SMS4:
            ProcessSmsInsertion(4);
            break;
          case Xkey.XK_SMS5:
            ProcessSmsInsertion(5);
            break;
          case Xkey.XK_SMS6:
            ProcessSmsInsertion(6);
            break;
          case Xkey.XK_SMS7:
            ProcessSmsInsertion(7);
            break;
          case Xkey.XK_SMS8:
            ProcessSmsInsertion(8);
            break;
          case Xkey.XK_SMS9:
            ProcessSmsInsertion(9);
            break;
        }
      }
    }

    protected void SetSearchKind()
    {
      switch (_searchKind)
      {
        case (int)SearchKinds.SEARCH_STARTS_WITH:
          ChangeKey((int)_currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_START_WITH, _searchModeKeyWidth, this));
          break;

        case (int)SearchKinds.SEARCH_ENDS_WITH:
          ChangeKey((int)_currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_ENDS_WITH, _searchModeKeyWidth, this));
          break;

        case (int)SearchKinds.SEARCH_IS:
          ChangeKey((int)_currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_IS, _searchModeKeyWidth, this));
          break;

        case (int)SearchKinds.SEARCH_CONTAINS:
          ChangeKey((int)_currentKeyboard, 1, 0, new Key(Xkey.XK_SEARCH_CONTAINS, _searchModeKeyWidth, this));
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
            if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
            {
              if (_currentKey == 7) // backspace
              {
                _currentKey = Math.Max(7, _lastColumn); // restore column
              }
            }
            else
            {
              if (_currentKey == 2) // backspace
              {
                _currentKey = _lastColumn = 3;
              }
              else if (_currentKey == 1) //0
              {
                _currentKey = Math.Min(3, _lastColumn);
                _lastColumn = _currentKey;
              }
            }
            if (_currentKeyboard == KeyboardTypes.TYPE_ACCENTS && _currentKey > 8)
            {
              _currentKey = 8;
            }
            break;
          case 4:
            if (_currentKey == 1) // spacebar
            {
              if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
              {
                _currentKey = Math.Min(6, _lastColumn); // restore column
              }
              else
              {
                _currentKey = 1; //0
              }
            }
            else if (_currentKey > 1) // left and right
            {
              if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
              {
                _currentKey = 7; // backspace
              }
              else
              {
                _currentKey = 2; //backspace
              }
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
          case 0:
            if (_currentKeyboard == KeyboardTypes.TYPE_ACCENTS && _currentKey > 8)
            {
              _currentKey = 8;
            }
            break;
          case 2:
            if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
            {
              if (_currentKey > 7) // q - t
              {
                _lastColumn = _currentKey; // remember column
                _currentKey = 7; // move to backspace
              }
            }
            else
            {
              if (_currentKey > 0)
              {
                _lastColumn = _currentKey; // remember column
                _currentKey = 1; // move to 0
              }
            }
            break;
          case 3:
            if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
            {
              if (0 < _currentKey && _currentKey < 7) // u - z
              {
                _lastColumn = _currentKey; // remember column
                _currentKey = 1; // move to spacebar
              }
              else if (_currentKey > 6) // backspace
              {
                _currentKey = _lastColumn > 8 ? 3 : 2;
              }
            }
            break;
          case 4:
            switch (_currentKey)
            {
              case 1: // spacebar
                _currentKey = Math.Min(6, _lastColumn);
                break;
              case 2: // left arrow
                if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
                {
                  _currentKey = Math.Max(Math.Min(8, _lastColumn), 7);
                }
                else
                {
                  _currentKey = _lastColumn = 3;
                }
                break;
              case 3: // right arrow
                if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
                {
                  _currentKey = Math.Max(9, _lastColumn);
                }
                else
                {
                  _currentKey = _lastColumn = 3;
                }
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
      ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
      ArrayList row = (ArrayList)board[_currentRow];
      Key key = (Key)row[_currentKey];
      if (key.name == "" || _lastColumn == 0)
      {
        switch (key.xKey)
        {
            // Adjust the last column for the arrow keys to confine it
            // within the range of the key width
          case Xkey.XK_ARROWLEFT:
            if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
            {
              _lastColumn = (_lastColumn <= 7) ? 7 : 8;
            }
            break;
          case Xkey.XK_ARROWRIGHT:
            if (!_useSmsStyleTextInsertion || _currentKeyboard != KeyboardTypes.TYPE_ALPHABET)
            {
              _lastColumn = (_lastColumn <= 9) ? 9 : 10;
            }
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

      ArrayList board = (ArrayList)_keyboardList[(int)_currentKeyboard];
      ArrayList row = (ArrayList)board[_currentRow];
      Key key = (Key)row[_currentKey];

      // On the symbols keyboard, Shift and Caps Lock are disabled
      if (_currentKeyboard == KeyboardTypes.TYPE_SYMBOLS)
      {
        if (key.xKey == Xkey.XK_SHIFT || key.xKey == Xkey.XK_CAPSLOCK)
        {
          return true;
        }
      }

      if (_password && key.xKey == Xkey.XK_SMS)
      {
        return true;
      }

      return false;
    }

    protected string GetChar(Xkey xk)
    {
      string smsResult = string.Empty;
      switch (xk)
      {
        case Xkey.XK_SMS1:
          smsResult = "1 (.!?)";
          break;
        case Xkey.XK_SMS2:
          smsResult = "2 (abc)";
          break;
        case Xkey.XK_SMS3:
          smsResult = "3 (def)";
          break;
        case Xkey.XK_SMS4:
          smsResult = "4 (ghi)";
          break;
        case Xkey.XK_SMS5:
          smsResult = "5 (jkl)";
          break;
        case Xkey.XK_SMS6:
          smsResult = "6 (mno)";
          break;
        case Xkey.XK_SMS7:
          smsResult = "7 (pqrs)";
          break;
        case Xkey.XK_SMS8:
          smsResult = "8 (tuv)";
          break;
        case Xkey.XK_SMS9:
          smsResult = "9 (wxyz)";
          break;
      }

      if (!string.IsNullOrEmpty(smsResult))
      {
        if ((_capsLockTurnedOn && !_shiftTurnedOn) || (!_capsLockTurnedOn && _shiftTurnedOn))
        {
          return smsResult.ToUpperInvariant();
        }
        else
        {
          return smsResult.ToLowerInvariant();
        }
      }
      
      // Handle case conversion
      char wc = (char)(((uint)xk) & 0xffff);

      if ((_capsLockTurnedOn && !_shiftTurnedOn) || (!_capsLockTurnedOn && _shiftTurnedOn))
      {
        wc = Char.ToUpper(wc);
      }
      else
      {
        wc = Char.ToLower(wc);
      }

      return wc.ToString();
    }

    protected void RenderKey(float timePassed, float fX, float fY, Key key, long keyColor, long textColor)
    {
      if (key.xKey == Xkey.XK_NULL)
      {
        return;
      }

      string strKey = GetChar(key.xKey);
      string name = (key.name.Length == 0) ? strKey : key.name;

      int width = key.dwWidth;
      GUIGraphicsContext.ScaleHorizontal(ref width);

      float x = fX;
      float y = fY;
      float z = fX + width;
      float w = fY + _keyHeightScaled;

      float nw = width;
      float nh = _keyHeightScaled;

      key.button.SetPosition((int)x, (int)y);
      key.button.Width = (int)nw;
      key.button.Height = (int)nh;
      key.button.ColourDiffuse = keyColor;
      key.button.Focus = key.inFocus;

      // Draw the key text. If key name is, use a slightly smaller font.
      float textWidth = 0;
      float textHeight = 0;
      float positionX = (x + z) / 2.0f;
      float positionY = (y + w) / 2.0f;

      if (key.name.Length > 1 && Char.IsUpper(key.name[1]))
      {
        _fontNamedKey.GetTextExtent(name, ref textWidth, ref textHeight);
        positionX -= (textWidth / 2);
        positionY -= (textHeight / 2);

        key.button.Label = name;
        key.button.FontName = _namedKeyFont;
        key.button.TextAlignment = GUIControl.Alignment.ALIGN_CENTER;
        key.button.TextVAlignment = GUIControl.VAlignment.ALIGN_MIDDLE;
        key.button.TextColorNoFocus = textColor;
        key.button.TextColor = textColor;
      }
      else
      {
        _fontCharKey.GetTextExtent(name, ref textWidth, ref textHeight);
        positionX -= (textWidth / 2);
        positionY -= (textHeight / 2);

        key.button.Label = name;
        key.button.FontName = _charKeyFont;
        key.button.TextAlignment = GUIControl.Alignment.ALIGN_CENTER;
        key.button.TextVAlignment = GUIControl.VAlignment.ALIGN_MIDDLE;
        key.button.TextColor = textColor;
        key.button.TextColorNoFocus = textColor;
      }
      key.button.Render(timePassed);
    }

    protected void DrawLabelBox(float timePassed, int x1, int y1, int x2, int y2)
    {
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      GUIGraphicsContext.ScalePosToScreenResolution(ref x2, ref y2);

      labelBox.SetPosition(x1, y1);
      labelBox.Width = (x2 - x1);
      labelBox.Height = (y2 - y1);
      labelBox.Render(timePassed);
    }

    protected void DrawLabel(float timePassed, int x, int y)
    {
      GUIGraphicsContext.ScalePosToScreenResolution(ref x, ref y);

      float labelWidth = 0.0f;
      float labelHeight = 0.0f;
      _fontSearchText.GetTextExtent(_labelText, ref labelWidth, ref labelHeight);

      int xoff = 0;
      switch (_inputTextAlign)
      {
        case GUIControl.Alignment.ALIGN_LEFT:
          break;
        case GUIControl.Alignment.ALIGN_RIGHT:
          xoff = -(int)labelWidth;
          break;
        case GUIControl.Alignment.ALIGN_CENTER:
          xoff = -(int)(labelWidth / 2);
          break;
      }

      label.SetPosition(x + xoff, y);
      label.Label = _labelText;
      label.TextColor = _labelColor;
      label.Render(timePassed);
    }

    protected void DrawTextBox(float timePassed, int x1, int y1, int x2, int y2)
    {
      GUIGraphicsContext.ScalePosToScreenResolution(ref x1, ref y1);
      GUIGraphicsContext.ScalePosToScreenResolution(ref x2, ref y2);

      inputTextBox.SetPosition(x1, y1);
      inputTextBox.Width = (x2 - x1);
      inputTextBox.Height = (y2 - y1);
      inputTextBox.Render(timePassed);
    }

    protected void DrawText(float timePassed, int x, int y)
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

      float textWidth = 0.0f;
      float textHeight = 0.0f;
      _fontSearchText.GetTextExtent(textLine, ref textWidth, ref textHeight);

      int xoff = 0;
      switch (_inputTextAlign)
      {
        case GUIControl.Alignment.ALIGN_LEFT:
          break;
        case GUIControl.Alignment.ALIGN_RIGHT:
          xoff = -(int)textWidth;
          break;
        case GUIControl.Alignment.ALIGN_CENTER:
          xoff = -(int)(textWidth / 2);
          break;
      }

      inputText.SetPosition(x + xoff, y);
      inputText.Label = textLine;
      inputText.TextColor = _inputTextColor;
      inputText.Render(timePassed);

      if (!IsLabelInitialText()) // Don't render the caret if displaying the label as initial text.
      {
        // Draw blinking caret using line primitives.
        TimeSpan ts = DateTime.Now - _caretTimer;
        if ((ts.TotalSeconds % fCARET_BLINK_RATE) < fCARET_ON_RATIO)
        {
          string line = textLine.Substring(0, _position);

          float caretWidth = 0.0f;
          float caretHeight = 0.0f;
          _fontSearchText.GetTextExtent("|", ref caretWidth, ref caretHeight);

          float lineWidth = 0.0f;
          float lineHeight = 0.0f;
          _fontSearchText.GetTextExtent(line, ref lineWidth, ref lineHeight);

          switch (_inputTextAlign)
          {
            case GUIControl.Alignment.ALIGN_LEFT:
              xoff = (int)lineWidth - (int)(caretWidth / 2);
              break;
            case GUIControl.Alignment.ALIGN_RIGHT:
              xoff = -(int)textWidth + (int)lineWidth - (int)(caretWidth / 2);
              break;
            case GUIControl.Alignment.ALIGN_CENTER:
              xoff = -((int)textWidth / 2) + (int)lineWidth - (int)(caretWidth / 2);
              break;
          }

          inputTextCaret.SetPosition(x + xoff, y);
          inputTextCaret.Label = "|";
          inputTextCaret.TextColor = _inputTextColor;
          inputTextCaret.Render(timePassed);
        }
      }
    }

    public bool Password
    {
      get { return _password; }
      set 
      {
        if (_password != value)
        {
          if (value)
          {
            SmsStyleText = false;
          }
          _password = value;
        }
      }
    }

    public string Text
    {
      get { return _textEntered; }
      set { _textEntered = value; }
    }

    public string Label
    {
      get { return _labelText; }
      set { _labelText = value; }
    }

    public void SetMaxLength(int maxLen)
    {
      _max_chars = maxLen;
    }

    public void SetLabelAsInitialText(bool value)
    {
      _showLabelAsInitialText = value;
    }

    public void ClearLabelAsInitialText()
    {
      if (IsLabelInitialText())
      {
        _textEntered = ""; // Clear the label from the text.
        _position = 0;
        _labelClearedFromText = true;
      }
    }

    public void ResetLabelAsInitialText()
    {
      if (_showLabelAsInitialText)
      {
        _textEntered = _labelText;
        _labelClearedFromText = false;
      }
    }

    public bool IsLabelInitialText()
    {
      return _showLabelAsInitialText && (_labelClearedFromText == false);
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

    public int Position
    {
      get { return _position; }
      set { _position = value; }
    }

    public string TextEntered
    {
      get { return _textEntered; }
      set { _textEntered = value; }
    }

    public bool PressedEnter
    {
      get { return _pressedEnter; }
      set { _pressedEnter = value; }
    }

    public bool IsSearchKeyboard
    {
      get { return _useSearchLayout; }
      set
      {
        if (_useSearchLayout != value)
        {
          if (value)
          {
            SmsStyleText = false;
            _useSearchLayout = value;
            return;
          }
          _useSearchLayout = value;
          InitBoard();
        }
      }
    }
  }
}