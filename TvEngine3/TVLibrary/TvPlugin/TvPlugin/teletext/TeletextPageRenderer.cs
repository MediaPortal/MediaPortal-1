#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Text;

namespace TvLibrary.Teletext
{
  public class TeletextPageRenderer
  {
    #region constructors

    public TeletextPageRenderer()
    {
      _isRegionalDKorNO =
        (RegionInfo.CurrentRegion.TwoLetterISORegionName.Equals("DK", StringComparison.InvariantCultureIgnoreCase))
                       || (RegionInfo.CurrentRegion.TwoLetterISORegionName.Equals("NO", StringComparison.InvariantCultureIgnoreCase));
    }

    #endregion

    #region constants

    private const int MAX_ROWS = 50;

    #endregion

    #region variables

    //regional stuff
    private readonly bool _isRegionalDKorNO;

    private Font _fontTeletext;

    private bool _hiddenMode = true;
    private bool _transparentMode;
    private bool _fullscreenMode;
    private TextRenderingHint _textRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

    private string _selectedPageText = "";
    private string _selectedSubPageText = "";

    private int _pageRenderWidth = 720;
    private int _pageRenderHeight = 540;
    private int _percentageOfMaximumHeight = 100;

    private int _defaultCharSetDesignation = 0;             // Dafault character set designation code (4-bit)
    private int _secondCharSetDesignation = 0;              // Second character set designation code (7-bit). Contains the subset code
    private G0CharSets _G0CharSet = G0CharSets.Latin;       // Default G0 character set
    private G2CharSets _G2CharSet = G2CharSets.Latin;       // Default G2 character set
    private SubSets _charSubSet = SubSets.English;          // Default national option subset
    private G0CharSets _altG0CharSet = G0CharSets.Latin;    // Alternate G0 character set
    private SubSets _altCharSubSet = SubSets.English;       // Alternate national option subset

    // Active character maps
    private char[] _G0CharMap;
    private char[] _G2CharMap;
    private char[] _altG0CharMap;


    #endregion

    #region enums

    /// <summary>
    /// Enumeration of all availabel colors in teletext
    /// </summary>
    private enum TextColors
    {
      None,
      Black,
      Red,
      Green,
      Yellow,
      Blue,
      Magenta,
      Cyan,
      White,
      Trans1,
      Trans2
    }

    /// <summary>
    /// Enumeration of all possible attributes for a position in teletext
    /// </summary>
    public enum Attributes
    {
      AlphaBlack,
      AlphaRed,
      AlphaGreen,
      AlphaYellow,
      AlphaBlue,
      AlphaMagenta,
      AlphaCyan,
      AlphaWhite,
      Flash,
      Steady,
      EndBox,
      StartBox,
      NormalSize,
      DoubleHeight,
      DoubleWidth,
      DoubleSize,
      MosaicBlack,
      MosaicRed,
      MosaicGreen,
      MosaicYellow,
      MosaicBlue,
      MosaicMagenta,
      MosaicCyan,
      MosaicWhite,
      Conceal,
      ContiguousMosaic,
      SeparatedMosaic,
      Esc,
      BlackBackground,
      NewBackground,
      HoldMosaic,
      ReleaseMosaic
    }
    enum G0CharSets
    {
        Latin = 0,
        Cyrillic1,
        Cyrillic2,
        Cyrillic3,
        Greek,
        Arabic,
        Hebrew
    }
    enum G2CharSets
    {
        Latin = 0,
        Cyrillic,
        Greek,
        Arabic,
        Hebrew
    }

    enum SubSets
    {
        CzechSlovak = 0,
        English,
        Estonian,
        French,
        German,
        Italian,
        LettishLithuanian,
        Polish,
        PortugueseSpanish,
        Romanian,
        SerbianCroatianSlovenian,
        SwedishFinnish,
        Turkish,
        DanishNorwegian,

        NA = English 
    }
    #endregion

    #region character and other tables for multi-language support. Referring the bits C12-C14 in the header

    private readonly char[,] m_charTableA = new char[,]
                                              {
                                                {'#', '\u016F'}, {'\u00A3', '$'},
                                                {'#', '\u00F5'}, {'\u00E9', '\u00EF'}, {'#', '$'}, {'\u00A3', '$'},
                                                {'#', '$'},
                                                {'#', '\u0149'}, {'\u00E7', '$'}, {'#', '\u00A4'}, {'#', '\u00CB'},
                                                {'#', '\u00A4'},
                                                {'\u00A3', '\u011F'}, {'#', '\u00A4'}
                                              };

    private readonly char[] m_charTableB = new char[]
                                             {
                                               '\u010D', '@', '\u0160', '\u00E0', '\u00A7', '\u00E9', '\u0160', '\u0105'
                                               , '\u00A1', '\u0162',
                                               '\u010C', '\u00C9', '\u0130', '\u00C9'
                                             };

    private readonly char[,] m_charTableC = new char[,]
                                              {
                                                {'\u0165', '\u017E', '\u00FD', '\u00ED', '\u0159', '\u00E9'},
                                                {'\u2190', '\u00BD', '\u2192', '\u2191', '#', '\u0336'},
                                                {'\u00C4', '\u00D6', '\u017D', '\u00DC', '\u00D5', '\u0161'},
                                                {'\u00EB', '\u00EA', '\u00F9', '\u00EE', '#', '\u00E8'},
                                                {'\u00C4', '\u00D6', '\u00DC', '^', '_', '\u00B0'},
                                                {'\u00B0', '\u00E7', '\u2192', '\u2191', '#', '\u00F9'},
                                                {'\u00E9', '\u0229', '\u017D', '\u010D', '\u016B', '\u0161'},
                                                {'\u01B5', '\u015A', '\u0141', '\u0107', '\u00F3', '\u0119'},
                                                {'\u00E1', '\u00E9', '\u00ED', '\u00F3', '\u00FA', '\u00BF'},
                                                {'\u00C2', '\u015E', '\u01CD', '\u00CE', '\u0131', '\u0163'},
                                                {'\u0106', '\u017D', '\u0110', '\u0160', '\u00EB', '\u010D'},
                                                {'\u00C4', '\u00D6', '\u00C5', '\u00DC', '_', '\u00E9'},
                                                {'\u015E', '\u00D6', '\u00C7', '\u00DC', '\u01E6', '\u0131'},
                                                {'\u00C6', '\u00D8', '\u00C5', '\u00DC', '_', '\u00E9'}
                                              };

    private readonly char[,] m_charTableD = new char[,]
                                              {
                                                {'\u00E1', '\u011B', '\u00FA', '\u0161'},
                                                {'\u00BC', '\u2016', '\u00BE', '\u00F7'},
                                                {'\u00E4', '\u00F6', '\u017E', '\u00FC'},
                                                {'\u00E2', '\u00F4', '\u00FB', '\u00E7'},
                                                {'\u00E4', '\u00F6', '\u00FC', '\u00DF'},
                                                {'\u00E0', '\u00F2', '\u00E8', '\u00EC'},
                                                {'\u0105', '\u0173', '\u017E', '\u012F'},
                                                {'\u017C', '\u015B', '\u0142', '\u017A'},
                                                {'\u00FC', '\u00F1', '\u00E8', '\u00E0'},
                                                {'\u00E2', '\u015F', '\u01CE', '\u00EE'},
                                                {'\u0107', '\u017E', '\u0111', '\u0161'},
                                                {'\u00E4', '\u00F6', '\u00E5', '\u00FC'},
                                                {'\u015F', '\u00F6', '\u00E7', '\u00FC'},
                                                {'\u00E6', '\u00F8', '\u00E5', '\u00FC'}
                                              };

    private readonly char[] m_charTableE = new char[]
                                             {
                                               '\u2190', '\u2192', '\u2191', '\u2193', 'O', 'K', '\u2190', '\u2190',
                                               '\u2190'
                                             };

    
    private readonly char[] m_cyrillicG0SetOpt1 = new char[] { // Serbian/Croatian
        ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?',
        '\u0427', '\u0410', '\u0411', '\u0426', '\u0414', '\u0415', '\u0424', '\u0413', '\u0425', '\u0418', '\u0408', '\u041A', '\u041B', '\u041C', '\u041D', '\u041E',
        '\u041F', '\u040C', '\u0420', '\u0421', '\u0422', '\u0423', '\u0412', '\u0403', '\u0409', '\u040A', '\u0417', '\u040B', '\u0416', '\u0402', '\u0428', '\u040F',
        '\u0447', '\u0430', '\u0431', '\u0446', '\u0434', '\u0435', '\u0444', '\u0433', '\u0445', '\u0438', '\u0458', '\u043A', '\u043B', '\u043C', '\u043D', '\u043E',
        '\u043F', '\u045C', '\u0440', '\u0441', '\u0442', '\u0443', '\u0432', '\u0453', '\u0459', '\u045A', '\u0437', '\u045B', '\u0436', '\u0452', '\u0448', '\u25A0'
    };

    private readonly char[] m_cyrillicG0SetOpt2 = new char[] { // Russian/Bulgarian
        ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?',
        '\u042E', '\u0410', '\u0411', '\u0426', '\u0414', '\u0415', '\u0424', '\u0413', '\u0425', '\u0418', '\u0419', '\u041A', '\u041B', '\u041C', '\u041D', '\u041E',
        '\u041F', '\u042F', '\u0420', '\u0421', '\u0422', '\u0423', '\u0416', '\u0412', '\u042C', '\u042A', '\u0417', '\u0428', '\u042D', '\u0429', '\u0427', '\u042B',
        '\u044E', '\u0430', '\u0431', '\u0446', '\u0434', '\u0435', '\u0444', '\u0433', '\u0445', '\u0438', '\u0439', '\u043A', '\u043B', '\u043C', '\u043D', '\u043E',
        '\u043F', '\u044F', '\u0440', '\u0441', '\u0442', '\u0443', '\u0436', '\u0432', '\u044C', '\u044A', '\u0437', '\u0448', '\u044D', '\u0449', '\u0447', '\u25A0'
    };

    private readonly char[] m_cyrillicG0SetOpt3 = new char[] { // Ukranian
        ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?',
        '\u042E', '\u0410', '\u0411', '\u0426', '\u0414', '\u0415', '\u0424', '\u0413', '\u0425', '\u0418', '\u0419', '\u041A', '\u041B', '\u041C', '\u041D', '\u041E',
        '\u041F', '\u042F', '\u0420', '\u0421', '\u0422', '\u0423', '\u0416', '\u0412', '\u042C', '\u0406', '\u0417', '\u0428', '\u0404', '\u0429', '\u0427', '\u0407',
        '\u044E', '\u0430', '\u0431', '\u0446', '\u0434', '\u0435', '\u0444', '\u0433', '\u0445', '\u0438', '\u0439', '\u043A', '\u043B', '\u043C', '\u043D', '\u043E',
        '\u043F', '\u044F', '\u0440', '\u0441', '\u0442', '\u0443', '\u0436', '\u0432', '\u044C', '\u0456', '\u0437', '\u0448', '\u0454', '\u0449', '\u0447', '\u25A0'
    };

    private readonly char[] m_cyrillicG2Set = new char[] { // 
        ' ', '\u00A1', '\u00A2', '\u00A3', '$', '\u00A5', ' ', '\u00A7', ' ', '\u2018', '\u201C', '\u00AB', '\u2190', '\u2191', '\u2192', '\u2193',
        '\u00B0', '\u00B1', '\u00B2', '\u00B3', '\u00D7', '\u03BC', '\u00B6', '\u00B7', '\u00F7', '\u2019', '\u201D', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00BF',
        '\u0020', '\u0300', '\u0301', '\u0302', '\u0303', '\u0304', '\u0306', '\u0307', '\u0308', '\u0323', '\u030A', '\u0318', '\u0331', '\u030B', '\u0319', '\u030C',
        '\u2014', '\u00B9', '\u00AE', '\u00A9', '\u2122', '\u266A', '\u20A0', '\u2030', '\u221D', '\u0141', '\u0142', '\u03B2', '\u215B', '\u215C', '\u215D', '\u215E',
        'D', 'E', 'F', 'G', 'I', 'J', 'K', 'L', 'N', 'Q', 'R', 'S', 'U', 'V', 'W', 'Z',
        'd', 'e', 'f', 'g', 'i', 'j', 'k', 'l', 'n', 'q', 'r', 's', 'u', 'v', 'w', 'z'
    };

    private readonly char[] m_greekG0Set = new char[] {
        ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '\u00AB', '=', '\u00BB', '?',
        '\u0390', '\u0391', '\u0392', '\u0393', '\u0394', '\u0395', '\u0396', '\u0397', '\u0398', '\u0399', '\u039A', '\u039B', '\u039C', '\u039D', '\u039E', '\u039F',
        '\u03A0', '\u03A1', '\u0384', '\u03A3', '\u03A4', '\u03A5', '\u03A6', '\u03A7', '\u03A8', '\u03A9', '\u03AA', '\u03AB', '\u03AC', '\u03AD', '\u03AE', '\u03AF',
        '\u03B0', '\u03B1', '\u03B2', '\u03B3', '\u03B4', '\u03B5', '\u03B6', '\u03B7', '\u03B8', '\u03B9', '\u03BA', '\u03BB', '\u03BC', '\u03BD', '\u03BE', '\u03BF',
        '\u03C0', '\u03C1', '\u03C2', '\u03C3', '\u03C4', '\u03C5', '\u03C6', '\u03C7', '\u03C8', '\u03C9', '\u03CA', '\u03CB', '\u03CC', '\u03CD', '\u03CE', ' '
    };

    private readonly char[] m_greekG2Set = new char[] {
        ' ', 'a', 'b', '\u00A3', 'e', 'h', 'i', '\u00A7', ':', '\u2018', '\u201C', 'k', '\u2190', '\u2191', '\u2192', '\u2193',
        '\u00B0', '\u00B1', '\u00B2', '\u00B3', 'x', 'm', 'n', 'p', '\u00F7', '\u2019', '\u201D', 't', '\u00BC', '\u00BD', '\u00BE', 'x',
        ' ', '\u0300', '\u0301', '\u0302', '\u0303', '\u0304', '\u0306', '\u0307', '\u0308', '\u0323', '\u030A', '\u0318', '\u0331', '\u030B', '\u0319', '\u030C',
        '?', '\u00B9', '\u00AE', '\u00A9', '\u2122', '\u266A', '\u20AC', '\u2030', '\u221D', '\u038A', '\u038E', '\u038F', '\u215B', '\u215C', '\u215D', '\u215E',
        'C', 'D', 'F', 'G', 'J', 'L', 'Q', 'R', 'S', 'U', 'V', 'W', 'Y', 'Z', '\u0386', '\u0389',
        'c', 'd', 'f', 'g', 'j', 'l', 'q', 'r', 's', 'u', 'v', 'w', 'y', 'z', '\u0388', ' '
    };

    // TODO: Build Arabic charset tables
    //private readonly char[] m_arabicG0Set = new char[] {
    //};
    //private readonly char[] m_arabicG2Set = new char[] {
    //};

    // TODO: Build Hebrew charset tables
    //private readonly char[] m_hebrewG0Set = new char[] {
    //};

/*
    readonly char[][] m_G0Sets = new char[][] { 
        null, 
        m_cyrillicG0SetOpt1, 
        m_cyrillicG0SetOpt2, 
        m_cyrillicG0SetOpt3, 
        m_greekG0Set, 
        null,
        null
       };

    readonly char[][] m_G2Sets = new char[][] { 
        null, 
        m_cyrillicG2Set, 
        m_greekG2Set, 
        null,
        null
       };
 */
    #endregion

    #region properties

    /// <summary>
    /// Width of the bitmap
    /// </summary>
    public int Width
    {
      get { return _pageRenderWidth; }
      set { _pageRenderWidth = value; }
    }

    /// <summary>
    /// Height of the bitmap
    /// </summary>
    public int Height
    {
      get { return _pageRenderHeight; }
      set { _pageRenderHeight = value; }
    }

    /// <summary>
    /// Draw also hidden information
    /// </summary>
    public bool HiddenMode
    {
      get { return _hiddenMode; }
      set { _hiddenMode = value; }
    }

    /// <summary>
    /// Draw the background transparent, only allowed in combination with fullscreen
    /// </summary>
    public bool TransparentMode
    {
      get { return _transparentMode; }
      set { _transparentMode = value; }
    }

    /// <summary>
    /// Draw for windowed mode or fullscreen mode
    /// </summary>
    public bool FullscreenMode
    {
      get { return _fullscreenMode; }
      set { _fullscreenMode = value; }
    }

    /// <summary>
    /// This text describe the selected page.
    /// </summary>
    public string PageSelectText
    {
      get { return _selectedPageText; }
      set
      {
        _selectedPageText = "";
        if (value.Length == 3)
        {
          _selectedPageText = value;
        }
        if (value.Length > 0 && value.Length < 3)
        {
          _selectedPageText = value + (new string('-', 3 - value.Length));
        }
      }
    }

    public string SubPageSelectText
    {
      get { return _selectedSubPageText; }
      set
      {
        _selectedSubPageText = value;
      }
    }

    /// <summary>
    /// Gets/Sets  the percentage of the maximum height for the font size
    /// </summary>
    public int PercentageOfMaximumHeight
    {
      get { return _percentageOfMaximumHeight; }
      set { _percentageOfMaximumHeight = value; }
    }
    /// <summary>
    /// Selects the default (primary) character set designation
    /// </summary>
    public int DefaultCharSetDesignation
    {
      get
      {
        return _defaultCharSetDesignation;
      }
      set
      {
        _defaultCharSetDesignation = value;
      }
    }

    /// <summary>
    /// Selects the secondary (alternate) character set designation
    /// Each time ESC is encountered in the data stream, the renderer
    /// switches the default and secondary character sets.
    /// </summary>
    public int SecondCharSetDesignation
    {
      get
      {
        return _secondCharSetDesignation;
      }
      set
      {
        _secondCharSetDesignation = value;
      }
    }

    /// <summary>
    /// Determines the font quality (font smoothing and hinting)
    /// </summary>
    public TextRenderingHint TextRenderingHint
    {
      get { return _textRenderingHint; }
      set { _textRenderingHint = value; }
    }
    #endregion

    /// <summary>
    /// Return the G0 character map specified in charSet
    /// </summary>
    /// <param name="charSet">The requested G0 character set</param>
    /// <returns>The character map</returns>
    private char[] GetG0CharMap(G0CharSets charSet)
    {
      switch (charSet)
      {
        case G0CharSets.Cyrillic1:
          return m_cyrillicG0SetOpt1;
        case G0CharSets.Cyrillic2:
          return m_cyrillicG0SetOpt2;
        case G0CharSets.Cyrillic3:
          return m_cyrillicG0SetOpt3;
        case G0CharSets.Greek:
          return m_greekG0Set;
        default:
          return null;
      }
    }

    /// <summary>
    /// Return the G2 character map specified in charSet
    /// </summary>
    /// <param name="charSet">The requested G2 character set</param>
    /// <returns>The character map</returns>
    private char[] GetG2CharMap(G2CharSets charSet)
    {
      switch (charSet)
      {
        case G2CharSets.Cyrillic:
          return m_cyrillicG2Set;
        case G2CharSets.Greek:
          return m_greekG2Set;
        case G2CharSets.Arabic:
          return null;
        case G2CharSets.Hebrew:
          return null;
        default:
          return null;
      }
    }

    /// <summary>
    /// Select the appropriate default character sets based on the 
    /// national subset code in the page control bits and the
    /// character set designation code (supplied by the user 
    /// for level 1 decoders, or transmitted in X/28 and/or 
    /// M/29 packets for levels 1.5 and up)
    /// </summary>
    /// <param name="defaultSetDesignation">the default G0 and G2 character set designation code (7-bit)</param>
    /// <param name="subSetSelector">the national subset code (3-bit)</param>
    protected void SetupDefaultCharSets(int defaultSetDesignation, int subSetSelector)
    {
      switch (defaultSetDesignation)
      {
        case 0:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Latin;
          switch(subSetSelector)
          {
            case 0:
              _charSubSet  = SubSets.English;
              break;
            case 1:
              _charSubSet = SubSets.German;
              break;
            case 2:
              _charSubSet = _isRegionalDKorNO? SubSets.DanishNorwegian : SubSets.SwedishFinnish;
              break;
            case 3:
              _charSubSet = SubSets.Italian;
              break;
            case 4:
              _charSubSet = SubSets.French;
              break;
            case 5:
              _charSubSet = SubSets.PortugueseSpanish;
              break;
            case 6:
              _charSubSet = SubSets.CzechSlovak;
              break;
            default:
              _charSubSet = SubSets.NA;
              break;
          }
          break;

        case 1:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Latin;
          switch (subSetSelector)
          {
            case 0:
              _charSubSet = SubSets.Polish;
              break;
            case 1:
              _charSubSet = SubSets.German;
              break;
            case 2:
              _charSubSet = _isRegionalDKorNO ? SubSets.DanishNorwegian : SubSets.SwedishFinnish;
              break;
            case 3:
              _charSubSet = SubSets.Italian;
              break;
            case 4:
              _charSubSet = SubSets.French;
              break;
            case 6:
              _charSubSet = SubSets.CzechSlovak;
              break;
            default:
              _charSubSet = SubSets.NA;
              break;
          }
          break;

        case 2:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Latin;
          switch (subSetSelector)
          {
            case 0:
              _charSubSet = SubSets.English;
              break;
            case 1:
              _charSubSet = SubSets.German;
              break;
            case 2:
              _charSubSet = _isRegionalDKorNO ? SubSets.DanishNorwegian : SubSets.SwedishFinnish;
              break;
            case 3:
              _charSubSet = SubSets.Italian;
              break;
            case 4:
              _charSubSet = SubSets.French;
              break;
            case 5:
              _charSubSet = SubSets.PortugueseSpanish;
              break;
            case 6:
              _charSubSet = SubSets.Turkish;
              break;
            default:
              _charSubSet = SubSets.NA;
              break;
          }
          break;

        case 3:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Latin;
          switch (subSetSelector)
          {
            case 5:
              _charSubSet = SubSets.SerbianCroatianSlovenian;
              break;
            case 7:
              _charSubSet = SubSets.Romanian;
              break;
            default:
              _charSubSet = SubSets.NA;
              break;
          }
          break;

        case 4:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Latin;
          switch (subSetSelector)
          {
            case 0:
              _G0CharSet = G0CharSets.Cyrillic1;
              _G2CharSet = G2CharSets.Cyrillic;
              _charSubSet = SubSets.NA;
              break;
            case 1:
              _charSubSet = SubSets.German;
              break;
            case 2:
              _charSubSet = SubSets.Estonian;
              break;
            case 3:
              _charSubSet = SubSets.LettishLithuanian;
              break;
            case 4:
              _G0CharSet = G0CharSets.Cyrillic2;
              _G2CharSet = G2CharSets.Cyrillic;
              _charSubSet = SubSets.NA;
              break;
            case 5:
              _G0CharSet = G0CharSets.Cyrillic3;
              _G2CharSet = G2CharSets.Cyrillic;
              _charSubSet = SubSets.NA;
              break;
            case 6:
              _charSubSet = SubSets.CzechSlovak;
              break;
            default:
              _charSubSet = SubSets.NA;
              break;
          }
          break;

        case 6:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Latin;
          switch (subSetSelector)
          {
            case 6:
              _charSubSet = SubSets.Turkish;
              break;
            case 7:
              _G0CharSet = G0CharSets.Greek;
              _G2CharSet = G2CharSets.Greek;
              _charSubSet = SubSets.NA;
              break;
            default:
              _charSubSet = SubSets.NA;
              break;
          }
          break;

        case 8:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Arabic;
          switch (subSetSelector)
          {
            case 0:
              _charSubSet = SubSets.English;
              break;
            case 4:
              _charSubSet = SubSets.French;
              break;
            case 7:
              _G0CharSet = G0CharSets.Arabic;
              _charSubSet = SubSets.NA;
              break;
            default:
              _charSubSet = SubSets.NA;
              break;
          }
          break;

        case 10:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Latin;
          switch (subSetSelector)
          {
            case 5:
              _G0CharSet = G0CharSets.Hebrew;
              _G2CharSet = G2CharSets.Arabic;
              _charSubSet = SubSets.NA;
              break;
            case 7:
              _G0CharSet = G0CharSets.Arabic;
              _G2CharSet = G2CharSets.Arabic;
              _charSubSet = SubSets.NA;
              break;
            default:
              _charSubSet = SubSets.NA;
              break;
          }
          break;

        default:
          _G0CharSet = G0CharSets.Latin;
          _G2CharSet = G2CharSets.Latin;
          _charSubSet = SubSets.NA;
          break;
      }

      _G0CharMap = GetG0CharMap(_G0CharSet);
      _G2CharMap = GetG2CharMap(_G2CharSet);
    }

    /// <summary>
    /// Select the appropriate second G0 character set based on the 
    /// character set designation code (supplied by the user 
    /// for level 1 decoders, or transmitted in X/28 and/or 
    /// M/29 packets for levels 1.5 and up)
    /// </summary>
    /// <param name="secondSetDesignation">the second G0 character set designation code (7-bit)</param>
    protected void SetupSecondG0CharSet(int secondSetDesignation)
    {
      _altG0CharSet = G0CharSets.Latin;
      _altCharSubSet = SubSets.English;

      switch (secondSetDesignation)
      {
        case 0x00:
          _altCharSubSet = SubSets.English;
          break;
        case 0x01:
          _altCharSubSet = SubSets.German;
          break;
        case 0x02:
          _altCharSubSet = SubSets.SwedishFinnish;
          break;
        case 0x03:
          _altCharSubSet = SubSets.Italian;
          break;
        case 0x04:
          _altCharSubSet = SubSets.French;
          break;
        case 0x05:
          _altCharSubSet = SubSets.PortugueseSpanish;
          break;
        case 0x06:
          _altCharSubSet = SubSets.CzechSlovak;
          break;
        case 0x08:
          _altCharSubSet = SubSets.Polish;
          break;
        case 0x09:
          _altCharSubSet = SubSets.German;
          break;
        case 0x0a:
          _altCharSubSet = SubSets.SwedishFinnish;
          break;
        case 0x0b:
          _altCharSubSet = SubSets.Italian;
          break;
        case 0x0c:
          _altCharSubSet = SubSets.French;
          break;
        case 0x0e:
          _altCharSubSet = SubSets.CzechSlovak;
          break;
        case 0x10:
          _altCharSubSet = SubSets.English;
          break;
        case 0x11:
          _altCharSubSet = SubSets.German;
          break;
        case 0x12:
          _altCharSubSet = SubSets.SwedishFinnish;
          break;
        case 0x13:
          _altCharSubSet = SubSets.Italian;
          break;
        case 0x14:
          _altCharSubSet = SubSets.French;
          break;
        case 0x15:
          _altCharSubSet = SubSets.PortugueseSpanish;
          break;
        case 0x16:
          _altCharSubSet = SubSets.Turkish;
          break;
        case 0x1d:
          _altCharSubSet = SubSets.SerbianCroatianSlovenian;
          break;
        case 0x1f:
          _altCharSubSet = SubSets.Romanian;
          break;
        case 0x20:
          _altG0CharSet = G0CharSets.Cyrillic1;
          _altCharSubSet = SubSets.NA;
          break;
        case 0x21:
          _altCharSubSet = SubSets.German;
          break;
        case 0x22:
          _altCharSubSet = SubSets.Estonian;
          break;
        case 0x23:
          _altCharSubSet = SubSets.LettishLithuanian;
          break;
        case 0x24:
          _altG0CharSet = G0CharSets.Cyrillic2;
          _altCharSubSet = SubSets.NA;
          break;
        case 0x25:
          _altG0CharSet = G0CharSets.Cyrillic3;
          _altCharSubSet = SubSets.NA;
          break;
        case 0x26:
          _altCharSubSet = SubSets.CzechSlovak;
          break;
        case 0x36:
          _altCharSubSet = SubSets.Turkish;
          break;
        case 0x37:
          _altG0CharSet = G0CharSets.Greek;
          _altCharSubSet = SubSets.NA;
          break;
        case 0x40:
          _altCharSubSet = SubSets.English;
          break;
        case 0x44:
          _altCharSubSet = SubSets.French;
          break;
        case 0x47:
          _altG0CharSet = G0CharSets.Arabic;
          _altCharSubSet = SubSets.NA;
          break;
        case 0x55:
          _altG0CharSet = G0CharSets.Hebrew;
          _altCharSubSet = SubSets.NA;
          break;
        case 0x57:
          _altG0CharSet = G0CharSets.Arabic;
          _altCharSubSet = SubSets.NA;
          break;

        default:
          _altG0CharSet = G0CharSets.Latin;
          _altCharSubSet = SubSets.NA;
          break;
      }
      _altG0CharMap = GetG0CharMap(_altG0CharSet);
    }

    #region private methods

    /// <summary>
    /// Render a single position in the bitmap
    /// </summary>
    /// <param name="graph">Graphics</param>
    /// <param name="chr">Character</param>
    /// <param name="attrib">Attributes</param>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <param name="w">width of the font</param>
    /// <param name="h">height of the font</param>
    /// <param name="txtLanguage">Teletext language</param>
    private void Render(ref Graphics graph, ref Bitmap pageBitmap, byte chr, int attrib, ref int x, ref int y, int w, int h)
    {
      bool charReady;
      char chr2 = '?';
      G0CharSets G0CharacterSet = ((attrib & 1 << 11) == 0 ? _G0CharSet : _altG0CharSet);
      SubSets subSet = ((attrib & 1 << 11) == 0 ? _charSubSet : _altCharSubSet);

      // Skip the character if 0xFF
      if (chr == 0xFF)
      {
        x += w;
        return;
      }
      // Generate mosaic
      int[] mosaicY = new int[4];
      mosaicY[0] = 0;
      mosaicY[1] = (h + 1) / 3;
      mosaicY[2] = (h * 2 + 1) / 3;
      mosaicY[3] = h;

      /* get colors */
      int fColor = attrib & 0x0F;
      int bColor = (attrib >> 4) & 0x0F;
      Color bgColor = GetColor(bColor);
      // We are in transparent mode and fullscreen. Make beckground transparent
      if (_transparentMode && _fullscreenMode)
      {
        bgColor = Color.Transparent;
      }
      Brush backBrush = new SolidBrush(bgColor);
      Brush foreBrush = new SolidBrush(GetColor(fColor));
      Pen backPen = new Pen(backBrush, 1);
      Pen forePen = new Pen(foreBrush, 1);
      // Draw the graphic
      try
      {
        if (((attrib & 0x300) > 0) && ((chr & 0xA0) == 0x20))
        {
          int w1 = w / 2;
          int w2 = w - w1;
          int y1;

          chr = (byte)((chr & 0x1f) | ((chr & 0x40) >> 1));
          if ((attrib & 0x200) > 0)
          {
            for (y1 = 0; y1 < 3; y1++)
            {
              graph.FillRectangle(backBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              if ((chr & 1) > 0)
              {
                graph.FillRectangle(foreBrush, x + 1, y + mosaicY[y1] + 1, w1 - 2, mosaicY[y1 + 1] - mosaicY[y1] - 2);
              }
              graph.FillRectangle(backBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);
              if ((chr & 2) > 0)
              {
                graph.FillRectangle(foreBrush, x + w1 + 1, y + mosaicY[y1] + 1, w2 - 2,
                                    mosaicY[y1 + 1] - mosaicY[y1] - 2);
              }
              chr >>= 2;
            }
          }
          else
          {
            for (y1 = 0; y1 < 3; y1++)
            {
              if ((chr & 1) > 0)
              {
                graph.FillRectangle(foreBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              }
              else
              {
                graph.FillRectangle(backBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              }
              if ((chr & 2) > 0)
              {
                graph.FillRectangle(foreBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);
              }
              else
              {
                graph.FillRectangle(backBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);
              }

              chr >>= 2;
            }
          }

          x += w;
          return;
        }

        int factor = (attrib & 1 << 10) > 0 ? 2 : 1;

        charReady = false;
        // If character is still not drawn, then we analyse it again
        if (G0CharacterSet == G0CharSets.Latin || chr >= 0x80)
        {
          switch (chr)
          {
            case 0x00:
            case 0x20:
              graph.FillRectangle(backBrush, x, y, w, h);
              if (factor == 2)
              {
                graph.FillRectangle(backBrush, x, y + h, w, h);
              }
              x += w;
              charReady = true;
              break;
            case 0x23:
            case 0x24:
              chr2 = m_charTableA[(int)subSet, chr - 0x23];
              break;
            case 0x40:
              chr2 = m_charTableB[(int)subSet];
              break;
            case 0x5B:
            case 0x5C:
            case 0x5D:
            case 0x5E:
            case 0x5F:
            case 0x60:
              chr2 = m_charTableC[(int)subSet, chr - 0x5B];
              break;
            case 0x7B:
            case 0x7C:
            case 0x7D:
            case 0x7E:
              chr2 = m_charTableD[(int)subSet, chr - 0x7B];
              break;
            case 0x7F:
            graph.FillRectangle(backBrush, x, y, w, factor * h);
            graph.FillRectangle(foreBrush, x + (w / 12), y + factor * (h * 5 / 20), w * 10 / 12, factor * (h * 11 / 20));
              x += w;
              charReady = true;
              break;
            case 0xE0:
              graph.FillRectangle(backBrush, x + 1, y + 1, w - 1, h - 1);
              graph.DrawLine(forePen, x, y, x + w, y);
              graph.DrawLine(forePen, x, y, x, y + h);
              x += w;
              charReady = true;
              break;
            case 0xE1:
              graph.FillRectangle(backBrush, x, y + 1, w, h - 1);
              graph.DrawLine(forePen, x, y, x + w, y);
              x += w;
              charReady = true;
              break;
            case 0xE2:
              graph.FillRectangle(backBrush, x, y + 1, w - 1, h - 1);
              graph.DrawLine(forePen, x, y, x + w, y);
              graph.DrawLine(forePen, x + w - 1, y + 1, x + w - 1, y + h - 1);
              x += w;
              charReady = true;
              break;
            case 0xE3:
              graph.FillRectangle(backBrush, x + 1, y, w - 1, h);
              graph.DrawLine(forePen, x, y, x, y + h);
              x += w;
              charReady = true;
              break;
            case 0xE4:
              graph.FillRectangle(backBrush, x, y, w - 1, h);
              graph.DrawLine(forePen, x + w - 1, y, x + w - 1, y + h);
              x += w;
              charReady = true;
              break;
            case 0xE5:
              graph.FillRectangle(backBrush, x + 1, y, w - 1, h - 1);
              graph.DrawLine(forePen, x, y + h - 1, x + w, y + h - 1);
              graph.DrawLine(forePen, x, y, x, y + h - 1);
              x += w;
              charReady = true;
              break;
            case 0xE6:
              graph.FillRectangle(backBrush, x, y, w, h - 1);
              graph.DrawLine(forePen, x, y + h - 1, x + w, y + h - 1);
              x += w;
              charReady = true;
              break;
            case 0xE7:
              graph.FillRectangle(backBrush, x, y, w - 1, h - 1);
              graph.DrawLine(forePen, x, y + h - 1, x + w, y + h - 1);
              graph.DrawLine(forePen, x + w - 1, y, x + w - 1, y + h - 1);
              x += w;
              charReady = true;
              break;
            case 0xE8:
              graph.FillRectangle(backBrush, x + 1, y, w - 1, h);
            for (int r = 0; r < w / 2; r++)
            {
                graph.DrawLine(forePen, x + r, y + r, x + r, y + h - r);
            }
              x += w;
              charReady = true;
              break;
            case 0xE9:
            graph.FillRectangle(backBrush, x + w / 2, y, (w + 1) / 2, h);
            graph.FillRectangle(foreBrush, x, y, w / 2, h);
              x += w;
              charReady = true;
              break;
            case 0xEA:
              graph.FillRectangle(backBrush, x, y, w, h);
            graph.FillRectangle(foreBrush, x, y, w / 2, h / 2);
              x += w;
              charReady = true;
              break;
            case 0xEB:
              graph.FillRectangle(backBrush, x, y + 1, w, h - 1);
            for (int r = 0; r < w / 2; r++)
            {
                graph.DrawLine(forePen, x + r, y + r, x + w - r, y + r);
            }
              x += w;
              charReady = true;
              break;
            case 0xEC:
            graph.FillRectangle(backBrush, x, y + (w / 2), w, h - (w / 2));
            graph.FillRectangle(foreBrush, x, y, w, h / 2);
              x += w;
              charReady = true;
              break;
            case 0xED:
            case 0xEE:
            case 0xEF:
            case 0xF0:
            case 0xF1:
            case 0xF2:
            case 0xF3:
            case 0xF4:
            case 0xF5:
            case 0xF6:
              chr2 = m_charTableE[chr - 0xED];
              break;
            default:
            chr2 = (char)chr;
              break;
          }
        }
        else // otherwise process international characters
        {
          if (chr == 0x00 || chr == 0x20)
          {
            graph.FillRectangle(backBrush, x, y, w, h);
            if (factor == 2)
              graph.FillRectangle(backBrush, x, y + h, w, h);
            x += w;
            charReady = true;
          }
          else if (chr == 0x7f)
          {
            graph.FillRectangle(backBrush, x, y, w, factor * h);
            graph.FillRectangle(foreBrush, x + (w / 12), y + factor * (h * 5 / 20), w * 10 / 12, factor * (h * 11 / 20));
            x += w;
            charReady = true;
          }
          else // use the selected charset mapping table
          {
            char[] map = ((attrib & (1 << 11)) == 0 ? _G0CharMap : _altG0CharMap);
            if (map == null || chr < 0x20)
            {
              chr2 = (char)chr;
            }
            else
            {
              chr2 = map[chr - 0x20];
            }
          }
        }

        // If still not drawn than it's a text and we draw the string
        if (charReady == false)
        {
          string text = "" + chr2;
          graph.FillRectangle(backBrush, x, y, w, h);
          SizeF width = graph.MeasureString(text, _fontTeletext);
          PointF xyPos = new PointF((float)x + ((w - ((int)width.Width)) / 2), y);
          graph.DrawString(text, _fontTeletext, foreBrush, xyPos);
          if (factor == 2)
          {
            graph.FillRectangle(backBrush, x, y + h, w, h);
            Color[,] pixelColor = new Color[w + 1,h + 1];
            // save char
            for (int ypos = 0; ypos < h; ypos++)
            {
              for (int xpos = 0; xpos < w; xpos++)
              {
                pixelColor[xpos, ypos] = pageBitmap.GetPixel(xpos + x, ypos + y); // backup old line
              }
            }
            // draw doubleheight
            for (int ypos = 0; ypos < h; ypos++)
            {
              for (int xpos = 0; xpos < w; xpos++)
              {
                try
                {
                  if (y + (ypos * 2) + 1 < pageBitmap.Height)
                  {
                    pageBitmap.SetPixel(x + xpos, y + (ypos * 2), pixelColor[xpos, ypos]); // backup old line
                    pageBitmap.SetPixel(x + xpos, y + (ypos * 2) + 1, pixelColor[xpos, ypos]);
                  }
                }
                catch {}
                }
              }
            }
          x += w;
        }
      }
      finally
      {
        foreBrush.Dispose();
        backBrush.Dispose();
        forePen.Dispose();
        backPen.Dispose();
      }
      return;
    }

    /// <summary>
    /// Converts the color of the teletext informations to a system color
    /// </summary>
    /// <param name="colorNumber">Number of the teletext color, referring to the enumeration TextColors </param>
    /// <returns>Corresponding System Color, or black if the value is not defined</returns>
    private static Color GetColor(int colorNumber)
    {
      switch (colorNumber)
      {
        case (int)TextColors.Black:
          return Color.Black;
        case (int)TextColors.Red:
          return Color.Red;
        case (int)TextColors.Green:
          return Color.FromArgb(0, 255, 0);
        case (int)TextColors.Yellow:
          return Color.Yellow;
        case (int)TextColors.Blue:
          return Color.Blue;
        case (int)TextColors.Magenta:
          return Color.Magenta;
        case (int)TextColors.White:
          return Color.White;
        case (int)TextColors.Cyan:
          return Color.Cyan;
        case (int)TextColors.Trans1:
          return Color.Transparent;
          //return Color.HotPink;
        case (int)TextColors.Trans2:
          return Color.Transparent;
          //return Color.HotPink;
      }
      return Color.Black;
    }

    /// <summary>
    /// Checks if is a valid page to be displayed
    /// </summary>
    /// <param name="i">Pagenumber to check</param>
    /// <returns>True, if page should be displayed</returns>
    private static bool IsDecimalPage(int i)
    {
      return ((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90);
    }


    private int GetLanguageCode(byte code)
    {
      int languageCode;

      if (code == 0xff)
      {
        languageCode = 0;
      }
      else
      {
        languageCode = ((code >> 3) & 0x01) | (((code >> 2) & 0x01) << 1) | (((code >> 1) & 0x01) << 2);
      }

      //switch (languageCode)
      //{
      //  case 0:
      //    return 1;
      //  case 1:
      //    return 4;
      //  case 2:
      //    return 11;
      //  case 3:
      //    return 5;
      //  case 4:
      //    return 3;
      //  case 5:
      //    return 8;
      //  case 6:
      //    return 0;
      //  default:
      //    return 1;
      //}
      return languageCode;
    }

    #endregion

    #region public methods

    /// <summary>
    /// Renders a teletext page to a bitmap using the preselected 
    /// default charset and second G0 charset designation.
    /// </summary>
    /// <param name="pageBitmap">The bitmap to render to</param>
    /// <param name="byPage">Teletext page data</param>
    /// <param name="mPage">Pagenumber</param>
    /// <param name="sPage">Subpagenumber</param>
    public void RenderPage(ref Bitmap pageBitmap, byte[] byPage, int mPage, int sPage)
    {
      RenderPage(ref pageBitmap, byPage, mPage, sPage, false, -1, -1);
    }

    public void RenderPage(ref Bitmap pageBitmap, byte[] byPage, int mPage, int sPage, bool waiting)
    {
      RenderPage(ref pageBitmap, byPage, mPage, sPage, waiting, -1, -1);
    }

    /// <summary>
    /// Renders a teletext page to a bitmap using the designated 
    /// default charset and second G0 charset designation.
    /// If either designation is -1 use the corresponding 
    /// preselected designation.
    /// </summary>
    /// <param name="byPage">Teletext page data</param>
    /// <param name="mPage">Pagenumber</param>
    /// <param name="sPage">Subpagenumber</param>
    /// <param name="defaultCharSet">The default charset designation</param>
    /// <param name="secondCharSet">The second G0 charset designation</param>
    /// <returns>Rendered teletext page as bitmap</returns>
    public void RenderPage(ref Bitmap pageBitmap, byte[] byPage, int mPage, int sPage, bool waiting, int defaultCharSet, int secondCharSet)
    {
      int col;
      int hold;
      int foreground, background, doubleheight, charset, mosaictype, alternateSet;
      byte held_mosaic;
      bool flag = false;
      bool isBoxed = false;
      byte[] pageChars = new byte[31 * 40];
      int[] pageAttribs = new int[31 * 40];
      bool row24 = false;

      if (pageBitmap == null)
      {
        return;
      }

      Graphics renderGraphics = Graphics.FromImage(pageBitmap);

      // Decode the page data (Hamming 8/4 or odd parity)
      for (int rowNr = 0; rowNr < MAX_ROWS; rowNr++)
      {
        if (rowNr * 42 >= byPage.Length)
        {
          break;
        }
        int packetNumber = Hamming.GetPacketNumber(rowNr * 42, ref byPage);
        // Only the packets 0-25 are accepted
        if (packetNumber < 0 || packetNumber > 25)
        {
          continue;
        }
        bool stripParity = true;
        // Packets 0 and 25 are hamming 8/4 encoded
        if (packetNumber == 25 || packetNumber == 0)
        {
          stripParity = false;
        }
        // Decode the whole row and remove the first two bytes
        for (col = 2; col < 42; col++)
        {
          // After pageheader in packet 0 (Bit 10) odd parity is used
          if (col >= 10 && packetNumber == 0)
          {
            stripParity = true;
          }
          byte kar = byPage[rowNr * 42 + col];
          if (stripParity)
          {
            kar &= 0x7f;
          }
          pageChars[packetNumber * 40 + col - 2] = kar;
        }
        // Exists a packet 24 (Toptext line)
        if (packetNumber == 24)
        {
          row24 = true;
        }
      }
      int row;
      //int txtLanguage;
      // language detection. Extract the bit C12-C14 from the teletext header and set the language code
      int languageCode = GetLanguageCode(Hamming.Decode[byPage[9]]);

      // Setup character sets
      SetupDefaultCharSets(defaultCharSet == -1? _defaultCharSetDesignation : defaultCharSet, languageCode);
      SetupSecondG0CharSet(secondCharSet == -1? _secondCharSetDesignation : secondCharSet);
      // Detect if it's a boxed page. Boxed Page = subtitle and/or newsflash bit is set
      bool isSubtitlePage = Hamming.IsSubtitleBitSet(0, ref byPage);
      bool isNewsflash = Hamming.IsNewsflash(0, ref byPage);
      isBoxed = isNewsflash | isSubtitlePage;
      MediaPortal.GUI.Library.Log.Debug("Newsflash: " + isNewsflash);
      MediaPortal.GUI.Library.Log.Debug("Subtitle: " + isSubtitlePage);
      MediaPortal.GUI.Library.Log.Debug("Boxed: " + isBoxed);

      // Determine if the header or toptext line sould be displayed.
      bool displayHeaderAndTopText = !_fullscreenMode || !isBoxed || (isBoxed && _selectedPageText.IndexOf("-") != -1)
                                     ||
                                     (isBoxed && _selectedPageText.IndexOf("-") == -1 &&
                                      !_selectedPageText.Equals(Convert.ToString(mPage, 16)));

      // Iterate over all lines of the teletext page and prepare the rendering
      for (row = 0; row <= 24; row++)
      {
        // If row 24 and no toptext exists, then skip this row
        if (row == 24 && !row24)
        {
          continue;
        }
        // If not display the header and toptext line, then clear these two rows
        if ((row == 0 || row == 24) && !displayHeaderAndTopText)
        {
          for (int i = 0; i < 40; ++i)
          {
            pageChars[row * 40 + i] = 32;
            pageAttribs[row * 40 + i] = ((int)TextColors.Trans1 << 4) | ((int)TextColors.White);
          }
        }
        else
        {
          // Otherwise, analyse the information. First set the forground to white and the background to:
          // - Transparent, if transparent mode or boxed and fullscreen and not display the header and toptext line
          // - Black otherwise
          foreground = (int)TextColors.White;
          if ((isBoxed || _transparentMode) && _fullscreenMode && !displayHeaderAndTopText)
          {
            background = (int)TextColors.Trans1;
          }
          else
          {
            background = (int)TextColors.Black;
          }

          // Reset the attributes
          doubleheight = 0;
          charset = 0;
          mosaictype = 0;
          hold = 0;
          held_mosaic = 32;
          alternateSet = 0;
          // Iterate over all columns in the row and check if a box starts
          for (int loop1 = 0; loop1 < 40; loop1++)
          {
            // Box starts in this row
            if (pageChars[(row * 40) + loop1] == (int)Attributes.StartBox)
            {
              flag = true;
              break;
            }
          }

          // If boxed page and box doesn't start in this line, than set foreground and background to black or transparent
          // depending on the mode (fullscreen <-> windowed)
          if (isBoxed && flag == false)
          {
            if (_fullscreenMode)
            {
              foreground = (int)TextColors.Trans1;
              background = (int)TextColors.Trans1;
            }
            else
            {
              foreground = (int)TextColors.Black;
              background = (int)TextColors.Black;
            }
          }

          // Iterate over all columns in the row again and now analyse every byte
          for (col = 0; col < 40; col++)
          {
            int index = row * 40 + col;

            // Set the attributes
            pageAttribs[index] = (alternateSet << 11 | doubleheight << 10 | charset << 8 | background << 4 | foreground);
            // Boxed and no flag and not row 24 than delete the characters
            if (isBoxed && !flag && row != 24)
            {
              pageChars[index] = 32;
            }
            // Analyse the attributes
            if (pageChars[index] < 32)
            {
              switch (pageChars[index])
              {
                case (int)Attributes.AlphaBlack:
                  foreground = (int)TextColors.Black;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaRed:
                  foreground = (int)TextColors.Red;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaGreen:
                  foreground = (int)TextColors.Green;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaYellow:
                  foreground = (int)TextColors.Yellow;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaBlue:
                  foreground = (int)TextColors.Blue;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaMagenta:
                  foreground = (int)TextColors.Magenta;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaCyan:
                  foreground = (int)TextColors.Cyan;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaWhite:
                  foreground = (int)TextColors.White;
                  charset = 0;
                  break;

                case (int)Attributes.Flash:
                  break;

                case (int)Attributes.Steady:
                  break;

                case (int)Attributes.EndBox:
                  if (isBoxed)
                  {
                    if (_fullscreenMode)
                    {
                      foreground = (int)TextColors.Trans1;
                      background = (int)TextColors.Trans1;
                    }
                    else
                    {
                      foreground = (int)TextColors.Black;
                      background = (int)TextColors.Black;
                    }
                  }
                  break;

                case (int)Attributes.StartBox:
                  if (isBoxed)
                  {
                    // Clear everything until this position in the line
                    if (col > 0)
                    {
                      for (int loop1 = 0; loop1 < col; loop1++)
                      {
                        pageChars[(row * 40) + loop1] = 32;
                      }
                    }
                    // Clear also the page attributes
                    for (int clear = 0; clear < col; clear++)
                    {
                      if (_fullscreenMode)
                      {
                        pageAttribs[row * 40 + clear] = alternateSet << 11 | doubleheight << 10 | charset << 8 |
														(int)TextColors.Trans1 << 4 | (int)TextColors.Trans1;
                      }
                      else
                      {
                        pageAttribs[row * 40 + clear] = alternateSet << 11 | doubleheight << 10 | charset << 8 |
														(int)TextColors.Black << 4 | (int)TextColors.Black;
                      }
                    }
                    // Set the standard background color
                    if (background == (int)TextColors.Trans1)
                    {
                      background = (int)TextColors.Black;
                    }
                  }
                  break;

                case (int)Attributes.NormalSize:
                  doubleheight = 0;
                  pageAttribs[index] = (alternateSet << 11 | doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int)Attributes.DoubleHeight:
                  if (row < 23)
                  {
                    doubleheight = 1;
                  }
                  break;

                case (int)Attributes.MosaicBlack:
                  foreground = (int)TextColors.Black;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicRed:
                  foreground = (int)TextColors.Red;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicGreen:
                  foreground = (int)TextColors.Green;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicYellow:
                  foreground = (int)TextColors.Yellow;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicBlue:
                  foreground = (int)TextColors.Blue;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicMagenta:
                  foreground = (int)TextColors.Magenta;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicCyan:
                  foreground = (int)TextColors.Cyan;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicWhite:
                  foreground = (int)TextColors.White;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.Conceal:
                  if (_hiddenMode == false)
                  {
                    foreground = background;
                    pageAttribs[index] = (alternateSet << 11 | doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int)Attributes.ContiguousMosaic:
                  mosaictype = 0;
                  if (charset > 0)
                  {
                    charset = 1;
                    pageAttribs[index] = (alternateSet << 11 | doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int)Attributes.SeparatedMosaic:
                  mosaictype = 1;
                  if (charset > 0)
                  {
                    charset = 2;
                    pageAttribs[index] = (alternateSet << 11 | doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int)Attributes.Esc:
                  alternateSet ^= 1;
                  pageAttribs[index] = (alternateSet << 11 | doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int)Attributes.BlackBackground:
                  background = (int)TextColors.Black;
                  pageAttribs[index] = (alternateSet << 11 | doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int)Attributes.NewBackground:
                  background = foreground;
                  pageAttribs[index] = (alternateSet << 11 | doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int)Attributes.HoldMosaic:
                  hold = 1;
                  break;

                case (int)Attributes.ReleaseMosaic:
                  hold = 2;
                  break;
              }

              if (hold > 0 && charset > 0)
              {
                pageChars[index] = held_mosaic;
              }
              else
              {
                pageChars[index] = 32;
              }

              if (hold == 2)
              {
                hold = 0;
              }
            }
            else
            {
              if (charset > 0)
              {
                held_mosaic = pageChars[index];
              }
              // If doubleheight is selected than delete the following line
              if (doubleheight > 0)
              {
                pageChars[index + 40] = 0xFF;
              }
            }
          }
          // Check, if there is double height selected in than set the attributes for the next row and skip it
          for (int count = (row + 1) * 40; count < ((row + 1) * 40) + 40; count++)
          {
            if (pageChars[count] == 255)
            {
              for (int loop1 = 0; loop1 < 40; loop1++)
              {
                pageAttribs[(row + 1) * 40 + loop1] = ((pageAttribs[(row * 40) + loop1] & 0xF0) |
                                                       ((pageAttribs[(row * 40) + loop1] & 0xF0) >> 4));
              }

              row++;
              break;
            }
          }
        }
      } //for (int rowNr = 0; rowNr < 24; rowNr++)

      // Generate header line, if it should be displayed
      if (IsDecimalPage(mPage) && displayHeaderAndTopText)
      {
        int i;
        string pageNumber;
        int lineColor;
        // Determine the state, of the header line.
        // Red=Incomplete page number
        // Yellow=Waiting for page
        // Green=Page is displayed
        if (_selectedPageText.IndexOf("-") == -1)
        {
          if (waiting)
          {
            lineColor = (int)TextColors.Yellow;
            pageNumber = _selectedPageText + (_selectedSubPageText == string.Empty ? "" : ("/" + _selectedSubPageText));
          }
          else
          {
            lineColor = (int)TextColors.Green;
            pageNumber = Convert.ToString(mPage, 16) + "/" + Convert.ToString(sPage + 1, 16);
          }
        }
        else
        {
          lineColor = (int)TextColors.Red;
          pageNumber = _selectedPageText;
        }
        string headline = "MediaPortal P." + pageNumber;
        headline += new string((char)32, 32 - headline.Length);
        byte[] mpText = Encoding.ASCII.GetBytes(headline);
        Array.Copy(mpText, 0, pageChars, 0, mpText.Length);
        alternateSet = _G0CharSet == G0CharSets.Latin ? 0 : 1 << 11;
        for (i = 0; i < 11; i++)
        {
          pageAttribs[i] = alternateSet | ((int)TextColors.Black << 4) | lineColor;
        }
        for (i = 11; i < 14; i++)
        {
          pageAttribs[i] = alternateSet | ((int)TextColors.Black << 4) | ((int)TextColors.White);
        }
        for (i = 14; i < 40; i++)
        {
          pageAttribs[i] = ((int) TextColors.Black << 4) | ((int) TextColors.White);
        }
      }

      // Now we generate the bitmap
      int y = 0;
      int x;
      int width = _pageRenderWidth / 40;
      int height = _pageRenderHeight / 25;
      float fntSize = height; //Math.Min(width, height);
      float nPercentage = ((float)_percentageOfMaximumHeight / 100);
      _fontTeletext = new Font("Lucida Console", fntSize, FontStyle.Regular, GraphicsUnit.Pixel);
      float fntHeight = _fontTeletext.GetHeight(renderGraphics);
      while (fntHeight > nPercentage * height) // || fntHeight > nPercentage * width)
      {
        fntSize -= 0.1f;
        _fontTeletext = new Font("Lucida Console", fntSize, FontStyle.Bold, GraphicsUnit.Pixel);
        fntHeight = _fontTeletext.GetHeight(renderGraphics);
      }
      MediaPortal.GUI.Library.Log.Debug("FONT SIZE OF TELETEXT: " + fntSize);

      try
      {
        // Select the brush, depending on the page and mode
        // Draw the base rectangle
        if ((isBoxed || _transparentMode) && _fullscreenMode)
        {
          renderGraphics.Clear(Color.Transparent);
        }
        else
        {
          renderGraphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, _pageRenderWidth, _pageRenderHeight);
        }

		    renderGraphics.TextRenderingHint = _textRenderingHint;
        // Fill the rectangle with the teletext page informations
          for (row = 0; row < 25; row++)
          {
            // If not display a toptext line than abort
            if (!displayHeaderAndTopText && row == 24)
            {
              break;
            }
            x = 0;
            // Draw a single point
            for (col = 0; col < 40; col++)
            {
              Render(ref renderGraphics, ref pageBitmap, pageChars[row * 40 + col], pageAttribs[row * 40 + col], ref x, ref y, width, height);
            }

            y += height + (row == 23 ? 2 : 0);
          }
        }
      finally
      {
        _fontTeletext.Dispose();
        _fontTeletext = null;
      }
    }

    #endregion
  }
}