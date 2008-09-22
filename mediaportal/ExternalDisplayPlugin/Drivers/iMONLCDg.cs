#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 * 
 *  Modified iMONLCD.cs Aug 8, 2007 by RalphY
 *  Modified iMONLCDg.cs September 25, 2007 by CybrMage
 *  Thanks to JoeDalton for T6963C.cs driver
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Player;
using MediaPortal.Ripper;
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.UserInterface.Controls;

namespace ProcessPlugins.ExternalDisplay.Drivers
{

  /// <summary>
  /// SoundGraph iMONLCD USB Driver for Soundgraph OEM LCD and UltraBay
  /// With Graphics support
  /// <author>Ralph Youie</author>
  /// </summary>
  public class iMONLCDg : BaseDisplay, IDisplay
  {
    #region Private Class variables
    private static Win32.Utils.Cd.DeviceVolumeMonitor DVM;
    private static readonly int[] Inserted_Media = new int[27];

    private static Int32 _VfdType = 0x18; // iMon version 5 initialised with 0x16
    // iMon version 6 uses 0x18 - no idea why the difference
    private static readonly Int32 _VfdReserved = 0x00008888; // both versions send this long int 
    private static int _DisplayType = 1;    // 0 = VFD, 1 = LCD, other = unsupported - default to using an LCD
    private readonly string[] _lines = new string[2];
    private bool _mpIsIdle = false;
    private bool _isDisabled = false;
    private string _errorMessage = "";
    private readonly SHA256Managed _sha256 = new SHA256Managed(); //instance of crypto engine used to calculate hashes
    private byte[] _lastHash; //hash of the last bitmap that was sent to the display

    private bool _Backlight = false;
    private Int64 _Contrast = 0x0A;

    private int _grows = 16;    // default 16 characters (both VFD and LCD)
    private int _gcols = 96;    // default 96 columns for an LCD display
    private int _delay = 0;     // milliseconds of delay between sending each data word to iMON Text mode
    private int _delayG = 0;    // milliseconds of delay between sending each data word to iMON Graphics
    // really nothing distinguishing text or graphics.
    private byte[] bitmapData;
    private bool _displayTest = false;

    // UpdateIcon Thread structures
    private static readonly object DWriteMutex = new object();   // Mutex control object - used to prevent main thread and 
    // Update thread from writing to the display simultaneously
    private bool _stopUpdateIconThread = false;
    private Thread _iconThread;
    private static bool _useDiskForAllMedia = true;
    private static bool _useVolumeDisplay;
    private static bool _useProgressDisplay;
    private static bool _useCustomFont;
    private static bool _UseLargeIcons;
    private static bool _UseCustomIcons;
    private static bool _UseInvertedIcons;
    private int _CurrentLargeIcon;
    private CustomFont CFont = null;
    private LargeIcon CustomLargeIcon = null;
    #endregion

    // custom font structures
    private class CustomFont
    {
      private static byte[,] CstmFont = null;
      private DataTable FontData = new DataTable("Character");
      private readonly DataColumn CID = new DataColumn("CharID");
      private readonly DataColumn CData0 = new DataColumn("CData0");
      private readonly DataColumn CData1 = new DataColumn("CData1");
      private readonly DataColumn CData2 = new DataColumn("CData2");
      private readonly DataColumn CData3 = new DataColumn("CData3");
      private readonly DataColumn CData4 = new DataColumn("CData4");
      private readonly DataColumn CData5 = new DataColumn("CData5");

      private void SaveDefaultFontData()
      {
        Log.Debug("SaveFontData() - called");

        // Creates a DataSet; adds a table, column, and ten rows.
        Log.Debug("SaveFontData() - Converting font data");
        FontData.Rows.Clear();
        for (int i = 0; i < 256; i++)
        {
          DataRow Character = FontData.NewRow();
          Character[0] = i;
          Character[1] = _Font8x5[i, 0];
          Character[2] = _Font8x5[i, 1];
          Character[3] = _Font8x5[i, 2];
          Character[4] = _Font8x5[i, 3];
          Character[5] = _Font8x5[i, 4];
          Character[6] = _Font8x5[i, 5];
          FontData.Rows.Add(Character);
        }
        XmlSerializer ser = new XmlSerializer(typeof(DataTable));
        TextWriter writer = new StreamWriter(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg_font.xml"));
        Log.Debug("SaveFontData() - Serializing data");
        ser.Serialize(writer, FontData);
        Log.Debug("SaveFontData() - Writing data to file");
        writer.Close();
        Log.Debug("SaveFontData() - completed");
      }
      public byte PixelData(int CharID, int CharIndex)
      {
        return CstmFont[CharID, CharIndex];
      }

      private bool LoadCustomFontData()
      {
        Log.Debug("LoadCustomFontData() - called");

        if (File.Exists(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg_font.xml")))
        {
          FontData.Rows.Clear();

          XmlSerializer ser = new XmlSerializer(typeof(DataTable));
          XmlTextReader rdr = new XmlTextReader(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg_font.xml"));
          Log.Debug("LoadCustomFontData() - DeSerializing data");
          FontData = (DataTable)ser.Deserialize(rdr);
          Log.Debug("LoadCustomFontData() - Read data from file");
          rdr.Close();

          Log.Debug("LoadCustomFontData() - Converting font data");
          for (int i = 0; i < 256; i++)
          {
            DataRow Character = FontData.Rows[i];
            CstmFont[i, 0] = (byte)Character[1];
            CstmFont[i, 1] = (byte)Character[2];
            CstmFont[i, 2] = (byte)Character[3];
            CstmFont[i, 3] = (byte)Character[4];
            CstmFont[i, 4] = (byte)Character[5];
            CstmFont[i, 5] = (byte)Character[6];
          }
          Log.Debug("LoadCustomFontData() - completed");
          return true;
        }
        else
        {
          Log.Debug("LoadCustomFontData() - Loading Custom Font from default Font");
          for (int i = 0; i < 256; i++)
          {
            for (int j = 0; j < 6; j++)
            {
              CstmFont[i, j] = _Font8x5[i, j];
            }
          }
          Log.Debug("LoadCustomFontData() - completed");
          return false;
        }
      }

      public void CloseFont()
      {
        FontData.Dispose();
      }

      public void InitializeCustomFont()
      {
        // set up the custom font if needed
        if (_useCustomFont)
        {
          if (FontData.Columns.Count == 0)
          {
            FontData.Rows.Clear();
            FontData.Columns.Clear();
            CstmFont = new byte[256, 6];
            CID.DataType = typeof(byte);
            FontData.Columns.Add(CID);
            CData0.DataType = typeof(byte);
            FontData.Columns.Add(CData0);
            CData1.DataType = typeof(byte);
            FontData.Columns.Add(CData1);
            CData2.DataType = typeof(byte);
            FontData.Columns.Add(CData2);
            CData3.DataType = typeof(byte);
            FontData.Columns.Add(CData3);
            CData4.DataType = typeof(byte);
            FontData.Columns.Add(CData4);
            CData5.DataType = typeof(byte);
            FontData.Columns.Add(CData5);
            FontData.Clear();
          }
          if (LoadCustomFontData())
          {
            Log.Debug("IDisplay(API) iMONLCDg.InitializeCustomFont() - Custom font data loaded");
          }
          else
          {
            SaveDefaultFontData();
            Log.Debug("IDisplay(API) iMONLCDg.InitializeCustomFont() - Custom font file not found. Template file saved. loaded default file.");
          }
        }
      }
    }

    // Large Icon support
    private class LargeIcon
    {
      private static byte[,] CustomIcons = null;
      private DataTable LIconData = new DataTable("LargeIcons");
      private readonly DataColumn LIID = new DataColumn("IconID");
      private readonly DataColumn IData0 = new DataColumn("IData0");
      private readonly DataColumn IData1 = new DataColumn("IData1");
      private readonly DataColumn IData2 = new DataColumn("IData2");
      private readonly DataColumn IData3 = new DataColumn("IData3");
      private readonly DataColumn IData4 = new DataColumn("IData4");
      private readonly DataColumn IData5 = new DataColumn("IData5");
      private readonly DataColumn IData6 = new DataColumn("IData6");
      private readonly DataColumn IData7 = new DataColumn("IData7");
      private readonly DataColumn IData8 = new DataColumn("IData8");
      private readonly DataColumn IData9 = new DataColumn("IData9");
      private readonly DataColumn IData10 = new DataColumn("IData10");
      private readonly DataColumn IData11 = new DataColumn("IData11");
      private readonly DataColumn IData12 = new DataColumn("IData12");
      private readonly DataColumn IData13 = new DataColumn("IData13");
      private readonly DataColumn IData14 = new DataColumn("IData14");
      private readonly DataColumn IData15 = new DataColumn("IData15");
      private readonly DataColumn IData16 = new DataColumn("IData16");
      private readonly DataColumn IData17 = new DataColumn("IData17");
      private readonly DataColumn IData18 = new DataColumn("IData18");
      private readonly DataColumn IData19 = new DataColumn("IData19");
      private readonly DataColumn IData20 = new DataColumn("IData20");
      private readonly DataColumn IData21 = new DataColumn("IData21");
      private readonly DataColumn IData22 = new DataColumn("IData22");
      private readonly DataColumn IData23 = new DataColumn("IData23");
      private readonly DataColumn IData24 = new DataColumn("IData24");
      private readonly DataColumn IData25 = new DataColumn("IData25");
      private readonly DataColumn IData26 = new DataColumn("IData26");
      private readonly DataColumn IData27 = new DataColumn("IData27");
      private readonly DataColumn IData28 = new DataColumn("IData28");
      private readonly DataColumn IData29 = new DataColumn("IData29");
      private readonly DataColumn IData30 = new DataColumn("IData30");
      private readonly DataColumn IData31 = new DataColumn("IData31");

      public byte PixelData(int IconID, int ByteIndex)
      {
        return CustomIcons[IconID, ByteIndex];
      }

      private void SaveDefaultLargeIconData()
      {
        Log.Debug("SaveDefaultLargeIconData() - called");

        // Creates a DataSet; adds a table, column, and ten rows.
        Log.Debug("SaveDefaultLargeIconData() - Converting icon data");
        LIconData.Rows.Clear();
        for (int i = 0; i < 10; i++)
        {
          DataRow LgIcon = LIconData.NewRow();
          LgIcon[0] = i;
          for (int j = 1; j < 33; j++)
          {
            LgIcon[j] = _InternalLargeIcons[i, (j - 1)];
          }
          LIconData.Rows.Add(LgIcon);
        }
        XmlSerializer ser = new XmlSerializer(typeof(DataTable));
        TextWriter writer = new StreamWriter(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg_icons.xml"));
        Log.Debug("SaveDefaultLargeIconData() - Serializing data");
        ser.Serialize(writer, LIconData);
        Log.Debug("SaveDefaultLargeIconData() - Writing data to file");
        writer.Close();
        Log.Debug("SaveDefaultLargeIconData() - completed");
      }

      private bool LoadLargeIconData()
      {
        Log.Debug("LoadLargeIconData() - called");

        if (File.Exists(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg_icons.xml")))
        {
          LIconData.Rows.Clear();

          XmlSerializer ser = new XmlSerializer(typeof(DataTable));
          XmlTextReader rdr = new XmlTextReader(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg_icons.xml"));
          Log.Debug("LoadLargeIconData() - DeSerializing data");
          LIconData = (DataTable)ser.Deserialize(rdr);
          Log.Debug("LoadLargeIconData() - Read data from file");
          rdr.Close();

          Log.Debug("LoadLargeIconData() - Converting icon data");
          for (int i = 0; i < 10; i++)
          {
            DataRow LgIcon = LIconData.Rows[i];
            for (int j = 1; j < 33; j++)
            {
              CustomIcons[i, (j - 1)] = (byte)LgIcon[j];
            }
          }
          Log.Debug("LoadLargeIconData() - completed");
          return true;
        }
        else
        {
          Log.Debug("LoadLargeIconData() - Loading Custom Large Icons from default Large Icons");
          for (int i = 0; i < 10; i++)
          {
            for (int j = 0; j < 32; j++)
            {
              CustomIcons[i, j] = _InternalLargeIcons[i, j];
            }
          }
          Log.Debug("LoadLargeIconData() - completed");
          return false;
        }
      }

      public void CloseIcons()
      {
        LIconData.Dispose();
      }

      public void InitializeLargeIcons()
      {
        // set up the custom icons if needed
        if (_UseLargeIcons || _UseCustomIcons)
        {
          Log.Debug("IDisplay(API) iMONLCDg.InitializeLargeIcons() - Using Large Icons.");
          if (!_UseCustomIcons)
          {
            Log.Debug("IDisplay(API) iMONLCDg.InitializeLargeIcons() - Using Internal Large Icon Data.");
          }
          else
          {
            Log.Debug("IDisplay(API) iMONLCDg.InitializeLargeIcons() - Using Custom Large Icon Data.");
            if (LIconData.Columns.Count == 0)
            {
              LIconData.Rows.Clear();
              LIconData.Columns.Clear();
              CustomIcons = new byte[10, 32];
              LIID.DataType = typeof(byte);
              LIconData.Columns.Add(LIID);
              IData0.DataType = typeof(byte);
              LIconData.Columns.Add(IData0);
              IData1.DataType = typeof(byte);
              LIconData.Columns.Add(IData1);
              IData2.DataType = typeof(byte);
              LIconData.Columns.Add(IData2);
              IData3.DataType = typeof(byte);
              LIconData.Columns.Add(IData3);
              IData4.DataType = typeof(byte);
              LIconData.Columns.Add(IData4);
              IData5.DataType = typeof(byte);
              LIconData.Columns.Add(IData5);
              IData6.DataType = typeof(byte);
              LIconData.Columns.Add(IData6);
              IData7.DataType = typeof(byte);
              LIconData.Columns.Add(IData7);
              IData8.DataType = typeof(byte);
              LIconData.Columns.Add(IData8);
              IData9.DataType = typeof(byte);
              LIconData.Columns.Add(IData9);
              IData10.DataType = typeof(byte);
              LIconData.Columns.Add(IData10);
              IData11.DataType = typeof(byte);
              LIconData.Columns.Add(IData11);
              IData12.DataType = typeof(byte);
              LIconData.Columns.Add(IData12);
              IData13.DataType = typeof(byte);
              LIconData.Columns.Add(IData13);
              IData14.DataType = typeof(byte);
              LIconData.Columns.Add(IData14);
              IData15.DataType = typeof(byte);
              LIconData.Columns.Add(IData15);
              IData16.DataType = typeof(byte);
              LIconData.Columns.Add(IData16);
              IData17.DataType = typeof(byte);
              LIconData.Columns.Add(IData17);
              IData18.DataType = typeof(byte);
              LIconData.Columns.Add(IData18);
              IData19.DataType = typeof(byte);
              LIconData.Columns.Add(IData19);
              IData20.DataType = typeof(byte);
              LIconData.Columns.Add(IData20);
              IData21.DataType = typeof(byte);
              LIconData.Columns.Add(IData21);
              IData22.DataType = typeof(byte);
              LIconData.Columns.Add(IData22);
              IData23.DataType = typeof(byte);
              LIconData.Columns.Add(IData23);
              IData24.DataType = typeof(byte);
              LIconData.Columns.Add(IData24);
              IData25.DataType = typeof(byte);
              LIconData.Columns.Add(IData25);
              IData26.DataType = typeof(byte);
              LIconData.Columns.Add(IData26);
              IData27.DataType = typeof(byte);
              LIconData.Columns.Add(IData27);
              IData28.DataType = typeof(byte);
              LIconData.Columns.Add(IData28);
              IData29.DataType = typeof(byte);
              LIconData.Columns.Add(IData29);
              IData30.DataType = typeof(byte);
              LIconData.Columns.Add(IData30);
              IData31.DataType = typeof(byte);
              LIconData.Columns.Add(IData31);
              LIconData.Clear();
            }
            if (LoadLargeIconData())
            {
              Log.Debug("IDisplay(API) iMONLCDg.InitializeLargeIcons() - Custom Large Icon data loaded");
            }
            else
            {
              SaveDefaultLargeIconData();
              Log.Debug("IDisplay(API) iMONLCDg.InitializeLargeIcons() - Custom Large Icon file not found. Template file saved. loaded default data.");
            }
          }
        }
      }

    }

    private class DisplayType
    {
      public static int VFD
      {
        get { return 0; }
      }
      public static int LCD
      {
        get { return 1; }
      }
      public static int Unsupported
      {
        get { return 2; }
      }
    }

    #region ExternalDisplay API Functions

    public bool IsDisabled
    {
      get
      {
        if (!File.Exists(Config.GetFile(Config.Dir.Base, "SG_VFDv5.dll")))
        {
          _errorMessage = "Required file \"SG_VFDv5.dll\" is not installed!";
          _isDisabled = true;
        }
        if (!File.Exists(Config.GetFile(Config.Dir.Base, "SG_VFD.dll")))
        {
          _errorMessage = "Required file \"SG_VFD.dll\" is not installed!";
          _isDisabled = true;
        }
        return _isDisabled;
      }
    }

    public string ErrorMessage
    {
      get { return _errorMessage; }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
      // This function is not needed... VFD does not support custom characters and LCD suports complete Font sets
    }

    public void DrawImage(Bitmap bitmap)
    {
      Log.Debug("IDisplay(API) iMONLCDg.DrawImage()");
      if (bitmap == null)
      {
        Log.Debug("IDisplay(API) iMONLCDg.DrawImage():  bitmap null");
        return;
      }
      BitmapData data =
        bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
      try
      {
        if (bitmapData == null)
        {
          bitmapData = new byte[data.Stride * _grows];
        }
        Marshal.Copy(data.Scan0, bitmapData, 0, bitmapData.Length);
      }
      finally
      {
        bitmap.UnlockBits(data);
      }
      //Calculate its hash so we can compare it to the previous bitmap more efficiently
      byte[] hash = _sha256.ComputeHash(bitmapData);
      //Compare the new hash with the previous one to determine whether the new image is
      //equal to the one that is already shown.  If they are equal, then we are done
      if (ByteArray.AreEqual(hash, _lastHash))
      {
        Log.Debug("IDisplay(API) iMONLCDg.DrawImage():  bitmap not changed");
        return;
      }

      byte[] PixelArray = new byte[96 * 2]; // must force a 96 byte * 2 rows to allign the display properly
      for (int i = 0; i < _gcols - 1; i++)
      {
        PixelArray[i] = 0; // line1
        PixelArray[i + 96] = 0; // line2 - MUST force a 96 byte * 2 rows to align the display properly
        for (int j = 0; j < 8; j++)
        {
          int pixel = j * data.Stride + i * 4;
          if (Color.FromArgb(bitmapData[pixel + 2],
                             bitmapData[pixel + 1],
                             bitmapData[pixel]).GetBrightness() < 0.5f)
          {
            PixelArray[i] = (byte)(PixelArray[i] | (byte)(1 << (7 - j))); // must force a 96 byte * 2 rows to align the display properly
          }
        }

        for (int j = 8; j < 16; j++)
        {
          int pixel = j * data.Stride + i * 4;
          if (Color.FromArgb(bitmapData[pixel + 2],
                             bitmapData[pixel + 1],
                             bitmapData[pixel]).GetBrightness() < 0.5f)
          {
            // must force a 96 byte * 2 rows to align the display properly
            PixelArray[i + 96] = (byte)(PixelArray[i + 96] | (byte)(1 << (15 - j)));
          }
        }
        //       Log.Debug("PixelArray i {0}: {1}{2}",i, PixelArray[i].ToString("X2"),PixelArray[i+gcols/2].ToString("X2"));
      }
      SendPixelArray(PixelArray);
      Log.Debug("IDisplay(API) iMONLCDg.DrawImage() Sending pixel array to iMON Handler");
      _lastHash = hash;
    }

    /// <summary>
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="line">The line to thow the message on.</param>
    /// <param name="message">The message to show.</param>
    public void SetLine(int line, string message)
    {
      Log.Debug("IDisplay(API) iMONLCDg.SetLine() called for Line {0} msg: '{1}'", line.ToString(), message);
      _lines[line] = message;
      if (line == 1)
      {
        DisplayLines();
      }
      Log.Debug("IDisplay(API) iMONLCDg.SetLine() completed");
    }

    /// <summary>
    /// Gets the short name of the display
    /// </summary>
    public string Name
    {
      get { return "iMONLCDg"; }
    }

    /// <summary>
    /// Gets the description of the display
    /// </summary>
    public string Description
    {
      get { return "SoundGraph iMON Integrated USB VFD/LCD Driver V1.0"; }
    }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }  // both displays support text mode ( VFD natively, LCD emulated)
    }

    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get
      {
        if (_DisplayType == DisplayType.VFD)
        {
          Log.Debug("IDisplay(API) iMONLCDg.SupportsGraphics() returned false");
          return false;
        }
        Log.Debug("IDisplay(API) iMONLCDg.SupportsGraphics() returned true");
        return true;
      }
    }

    /// <summary>
    /// Shows the advanced configuration screen
    /// </summary>
    public void Configure()
    {
      Form AdvSettings = new AdvancedSetupForm();
      AdvSettings.ShowDialog();
      AdvSettings.Dispose();
    }

    /// <summary>
    /// Initializes the display
    /// </summary>
    /// <param name="port">The port the display is connected to</param>
    /// <param name="lines">The number of lines in text mode</param>
    /// <param name="cols">The number of columns in text mode</param>
    /// <param name="delay">Communication delay in text mode</param>
    /// <param name="linesG">The height in pixels in graphic mode</param>
    /// <param name="colsG">The width in pixels in graphic mode</param>
    /// <param name="timeG">Communication delay in graphic mode</param>
    /// <param name="backLight">Backlight on?</param>
    /// <param name="contrast">Contrast</param>
    public void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG,
                      bool backLight, int contrast)
    {
      //        Log.SetLogLevel(MediaPortal.Services.Level.Debug);   // uncomment this line to get excessive logging
      Log.Debug("iMONLCDg Driver - v09_28_2007");

      #region iMONLCDg Advanced Configuration and display detection

      Log.Debug("IDisplay(API) iMONLCDg.Setup() called");

      int found_display = -1;
      int reg_found_display = -1;
      RegistryKey rKey;
      int rVAL;

      Log.Debug("IDisplay(API) iMONLCDg.Setip() Loading Advanced Settings - called");
      // parse the advanced settings
      AdvancedSettings AdvSettings = AdvancedSettings.Load();
      _useDiskForAllMedia = !AdvSettings.iMONLCDg_DiskOnlyDisplay;
      _useVolumeDisplay = AdvSettings.iMONLCDg_VolumeDisplay;
      _useProgressDisplay = AdvSettings.iMONLCDg_ProgressDisplay;
      _useCustomFont = AdvSettings.iMONLCDg_UseCustomFont;
      _UseLargeIcons = AdvSettings.iMONLCDg_UseLargeIcons;
      _UseCustomIcons = AdvSettings.iMONLCDg_UseCustomIcons;
      _UseInvertedIcons = AdvSettings.iMONLCDg_UseInvertedIcons;
      string _ForceDisplay = AdvSettings.iMONLCDg_DisplayType; // LCD display
      Log.Debug("IDisplay(API) iMONLCDg.Setup() - set Advanced options - DFAM: {0} Vol: {1} Prog: {2} LIcons: {3} CIcons: {4} Display: {5}", _useDiskForAllMedia.ToString(), _useVolumeDisplay.ToString(), _useProgressDisplay.ToString(), _UseLargeIcons.ToString(), _UseCustomIcons.ToString(), _ForceDisplay);


      // make sure VFD or iMON Manager is configured right
      Check_iMON_Manager_Status();

      if (_ForceDisplay == "LCD")
      {
        _DisplayType = DisplayType.LCD;  // LCD display
        Log.Debug("IDisplay(API) iMONLCDg.Setup() - Advanced options forces display type to LCD");
      }
      else if (_ForceDisplay == "VFD")
      {
        _DisplayType = DisplayType.VFD;   // VFD display
        Log.Debug("IDisplay(API) iMONLCDg.Setup() - Advanced options forces display type to VFD");
      }
      else
      {
        // AutoDetect the display
        Log.Debug("ExternalDisplay.iMONLCDg.Setup() - Autodetecting iMON Display device");
        try
        {
          Log.Debug("IDisplay(API) iMONLCDg.Setup() - attempting hardware information test - Opening SG_RC.dll");
          bool rc_open = RC_Init(0x77, 0x83, 0x8888);   // rcSet, rcType, rcReserved - derived from VFD.exe and iMON.exe
          if (rc_open)
          {
            // set the RC type
            RC_ChangeRCSet(0x77);
            RC_ChangeRC6(0x1); // set RC6 mode - for FWVer = 0xA0 or 0xA1
            long _rcDriver = RC_CheckDriverVersion();
            int _rcFwVer = RC_GetFirmwareVer();
            int _rcHWType = RC_GetHWType();
            int _rcRFMode = RC_GetLastFRMode();
            Log.Debug("IDisplay(API) iMONLCDg.Setup() - RC TEST returned DRVR: {0}, FW: {1}, HW: {2}, RF: {3}", _rcDriver.ToString("x0000000000000000"), _rcFwVer.ToString("x00000000"), _rcHWType.ToString("x00000000"), _rcRFMode.ToString("x00000000"));
            if (_rcFwVer > 0)
            {
              // we found a hardware type (for the RC controller) and firmware version
              found_display = _rcFwVer;
            }
          }
          RC_Uninit();
          Log.Debug("IDisplay(API) iMONLCDg.Setup() - Closing SG_RC.dll");
        }
        catch (Exception ex)
        {
          Log.Debug("IDisplay(API) iMONLCDg.Setup() - RC TEST FAILED... SG_RC.dll not found in MediaPortal directory. Exception: {0}", ex.ToString());
        }

        try
        {
          // If we did not detect a display type via the hardware tests, check the registry
          // check the Antec registry settings
          Log.Debug("IDisplay() iMONLCDg.Setup() - checking registry for ANTEC entries");
          rKey = Registry.CurrentUser.OpenSubKey("Software\\Antec\\VFD", false);
          if (rKey != null)
          {
            rVAL = (int)rKey.GetValue("LastVFD", 0);
            if (rVAL > 0)
            {
              Log.Debug("IDisplay(API) iMONLCDg.Setup() - ANTEC registry entries found - HW: {0}", rVAL.ToString("x00"));
              reg_found_display = rVAL;
            }
          }
          Registry.CurrentUser.Close();
          if (reg_found_display < 0)
          {
            // check the Soundgraph registry settings
            Log.Debug("IDisplay() iMONLCDg.Setup() - checking registry for SOUNDGRAPH entries");
            rKey = Registry.CurrentUser.OpenSubKey("Software\\SOUNDGRAPH\\iMON", false);
            if (rKey != null)
            {
              rVAL = (int)rKey.GetValue("LastVFD", 0);
              if (rVAL > 0)
              {
                Log.Debug("IDisplay(API) iMONLCDg.Setup() - SOUNDGRAPH registry entries found - HW: {0}", rVAL.ToString("x00"));
                reg_found_display = rVAL;
              }
            }
            Registry.CurrentUser.Close();
          }
        }
        catch (Exception ex)
        {
          Log.Info("IDisplay(API) iMONLCDg.Setup() - registry test caught exception {0}", ex.ToString());
        }
        // set the display type - prefer probe of hardware over probe of registry
        if (found_display > -1)
        {
          // determine display via registry entries
          GetDisplayInfoFromFirmware(found_display);
          Log.Debug("IDisplay(API) iMONLCDg.Setup() - Hardware tests determined - VFD: {0}, DT: {1} Rsrvd: {2}", _VfdType.ToString("x00"), _DisplayType.ToString("x00"), _VfdReserved.ToString("x00"));
        }
        else if (reg_found_display > -1)
        {
          GetDisplayInfoFromRegistry(reg_found_display);
          Log.Debug("IDisplay() iMONLCDg.Setup() - Registry tests determined - VFD: {0}, DT: {1} Rsrvd: {2}", _VfdType, _DisplayType, _VfdReserved);
        }
        else
        {
          // use the defaults set by the advanced configuration screen
          Log.Debug("IDisplay(API) iMONLCDg.Setup() - Display Type could not be determined");
        }
        if (_DisplayType == DisplayType.Unsupported)
        {
          _isDisabled = true;
          Log.Debug("IDisplay(API) iMONLCDg.Setup() - Display Type is not supported - Plugin disabled");
        }
      }

      if (!_isDisabled)
      {
        try
        {
          Log.Debug("IDisplay(API) iMONLCDg.Setup() - Testing iMON Display device");
          if (IsOpen())
          {
            Log.Debug("IDisplay(API) iMONLCDg.Setup() - iMON Display found");
            Close();
          }
          Log.Debug("IDisplay(API) iMONLCDg.Setup() - opening display type {0}", _DisplayType);
          if (!Open(_VfdType, _VfdReserved))
          {
            Log.Debug("IDisplay(API) iMONLCDg.Setup() - Open failed - No iMON device found");
            _isDisabled = true;
            _errorMessage = "iMONLCDg could not find an iMON LCD display";
          }
          else
          {
            // cleanup so the Setup and OpenLcd functions work correctly.
            Log.Debug("IDisplay(API) iMONLCDg.Setup() - iMON Display device found");
            Close();
          }
        }
        catch (Exception ex)
        {
          _isDisabled = true;
          _errorMessage = ex.Message;
          Log.Info("IDisplay(API) iMONLCDg.Setup() - caught an exception. Did you copy SG_VFD.dll to you windows\\system32 directory??");
        }
      }
      #endregion


      string currentModule = GUIPropertyManager.GetProperty("#currentmodule");
      Log.Debug("IDisplay(API) iMONLCDg.Setup() current module = {0}", currentModule);

      _Contrast = ((Int64)contrast >> 2);
      _Backlight = backLight;

      if (_DisplayType == DisplayType.LCD)
      {
        _grows = linesG;
        if (_grows > 16)
        {
          _grows = 16;
          Log.Info("IDisplay(API) iMONLCDg.Setup() - DISPLAY CONFIGURATION ERROR - Rows must be less then or equal to 16");
        }
        _gcols = colsG; // should be set 96 or 78 (if Large Icons are used)
        if (_gcols > 96)
        {
          _gcols = 96;
          Log.Info("IDisplay(API) iMONLCDg.Setup() - DISPLAY CONFIGURATION ERROR - Columns must be less then or equal to 96");
        }
      }
      _delay = delay;
      _delayG = timeG;
      _delay = Math.Max(_delay, _delayG);

      Log.Debug("IDisplay(API) iMONLCDg.Setup() - Completed");
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Initialize()
    {
      Log.Debug("IDisplay(API) iMONLCDg.Initialize() called");
      OpenLcd();
      Clear();
      Log.Debug("IDisplay(API) iMONLCDg.Initialize() completed");
    }

    public void CleanUp()
    {
      Log.Debug("IDisplay(API) iMONLCDg.CleanUp() called");
      CloseLcd();
      Log.Debug("IDisplay(API) iMONLCDg.CleanUp() completed");
    }

    /// <summary>
    /// Cleanup/Dispose
    /// </summary>
    public void Dispose()
    {
      Log.Debug("iMONLCDg.Dispose() called");
      //      The Dispose() method is no longer needed by this driver
      Log.Debug("iMONLCDg.Dispose() completed");
    }

    #endregion

    #region  iMON LCD Specific methods

    /// <summary>
    /// Sends the text to the display
    /// </summary>
    private void DisplayLines()
    {
      //SetText(lines[0], lines[1]);
      if (_DisplayType == DisplayType.LCD)
      {
        // send lines to LCD
        SendText(_lines[0], _lines[1]);
      }
      else
      {
        // send lines to VFD
        SetText(_lines[0], _lines[1]);
      }
      if (_lines[0] == "  MediaPortal   ") _mpIsIdle = true; else _mpIsIdle = false;
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Clear()
    {
      Log.Debug("iMONLCDg.Clear() called");
      for (int i = 0; i < 2; i++)
      {
        _lines[i] = new string(' ', Settings.Instance.TextWidth);
      }
      DisplayLines();
      Log.Debug("iMONLCDg.Clear() completed");
    }

    /// <summary>
    /// Opens the display driver
    /// </summary>
    private void OpenLcd()
    {
      Log.Debug("iMONLCDg.OpenLcd() called");
      if (!IsOpen())
      {
        Log.Debug("iMONLCDg.OpenLcd() opening display");
        if (!Open(_VfdType, _VfdReserved))
        {
          Log.Debug("iMONLCDg.OpenLcd() Could not open display with Open({0},{1})", _VfdType.ToString("x00"), _VfdReserved.ToString("x0000"));
        }
        else
        {
          Log.Debug("iMONLCDg.OpenLcd() display opened");
          // start the thread that will update the icons if we are driving an LCD display
          if ((!_displayTest) & (_DisplayType == DisplayType.LCD))
          {
            if (_useCustomFont)
            {
              CFont = new CustomFont();
              CFont.InitializeCustomFont();
            }
            if (_UseLargeIcons)
            {
              CustomLargeIcon = new LargeIcon();
              CustomLargeIcon.InitializeLargeIcons();
            }
            _iconThread = new Thread(this.UpdateIcons);
            _iconThread.IsBackground = true;
            _iconThread.Priority = ThreadPriority.BelowNormal;
            _iconThread.Name = "UpdateIconThread";
            _iconThread.Start();
            if (_iconThread.IsAlive)
            {
              Log.Debug("iMONLCDg.UpdateIcons() Thread Started");
            }
            else
            {
              Log.Debug("iMONLCDg.UpdateIcons() FAILED TO START");
            }
          }
        }
      }
      else
      {
        Log.Debug("iMONLCDg.OpenLcd: LCD already open");
      }
      lock (DWriteMutex)
      {
        if (_DisplayType == DisplayType.LCD)
        {
          SendData(Command.DisplayOn);             // turn the LCD display on
          SendData(Command.ClearAlarm);             // clear the alarm
          SendData(Command.SetContrast, _Contrast); // set LCD contrast
          ClearPixels();
        }
      }
      ClearDisplay();
      Log.Debug("iMONLCDg.OpenLcd() completed");
    }

    /// <summary>
    /// Closes the display driver
    /// </summary>
    /// 
    private void CloseLcd()
    {
      Log.Debug("iMONLCDg.CloseLcd() called");
      if (IsOpen())
      {
        // wait for the UpdateIcons thread (if it exists) to be finished writing to the display
        lock (DWriteMutex)
        {
          if (_DisplayType == DisplayType.LCD)
          {
            // stop the Icon Update Thread
            if (!_displayTest)
            {
              Log.Debug("iMONLCDg.CloseLcd() Stoping iMONLCDg.UpdateIcons() Thread");
              _stopUpdateIconThread = true;
              while (_iconThread.IsAlive) Thread.Sleep(100);
            }

            Log.Debug("iMONLCDg.CloseLcd() Preparing for shutdown");
            SendData(Command.SetIcons); // remove top and bottom lines
            SendData(0x1100000000000000);
            SendData(0x1200000000000000);
            if (_Backlight)
            {
              Log.Debug("iMONLCDg.CloseLcd() sending display shutdown command");
              // shut down the display
              SendData(Command.Shutdown);
            }
            else
            {
              Log.Debug("iMONLCDg.CloseLcd() sending clock enable command");
              // display the built-in clock
              DateTime st = DateTime.Now;
              Int64 data;
              data = ((Int64)0x50 << 56);
              data += ((Int64)st.Second << 48);
              data += ((Int64)st.Minute << 40);
              data += ((Int64)st.Hour << 32);
              data += ((Int64)st.Day << 24);
              data += ((Int64)st.Month << 16);
              data += (((Int64)st.Year & 0x0F) << 8);
              data += 0x80;
              SendData(data);
            }
            // dispose of the custom font data
            if (_useCustomFont)
            {
                CFont.CloseFont();
            }
            // dispose of the custom Large Icon data
            if (_UseCustomIcons || _UseLargeIcons)
            {
              CustomLargeIcon.CloseIcons();
            }
          }
          else
          {
            if (_Backlight)
            {
              Log.Debug("iMONLCDg.CloseLcd() Shutting down VFD display!!");
              SetText("", "");
            }
            else
            {
              Log.Debug("iMONLCDg.CloseLcd() Sending Shutdown message to VFD display!!");
              SetText("   MediaPortal  ", "   not active   ");
            }
          }
        }
        Close();
      }
      else
      {
        Log.Debug("iMONLCDg.CloseLcd() Display is not open!!");
      }
      Log.Debug("iMONLCDg.CloseLcd() completed");
    }

    public void DoDisplayTest()   // this routine is no longer used, but may eventually be re-implimented
    {
      BuiltinIconMask bICON = new BuiltinIconMask();
      if (_DisplayType == DisplayType.LCD)
      {
        // we are in the configuration screen - do a display test
        Log.Debug("IDisplay(API) iMONLCDg.Setup() configure - do display test");
        _displayTest = true;
        OpenLcd();
        ClearDisplay();
        Thread.Sleep(500);
        SendText("iMONLCDg", "Display Test");
        Thread.Sleep(500);
        // all icons
        SendText("iMONLCDg", "All Icons");
        for (int i = 0; i < 2; i++)
        {
          SendData(Command.SetIcons, bICON.ICON_ALL);
          Thread.Sleep(500);
          SendData(Command.SetIcons);
          Thread.Sleep(500);
        }
        // disk icon
        DiskIcon Disk = new DiskIcon();
        Disk.Reset();
        Disk.On();
        SendText("iMONLCDg", "Disk On");
        Thread.Sleep(500);
        SendText("iMONLCDg", "Disk Spin CW");
        Disk.RotateCW();
        for (int i = 0; i < 16; i++)
        {
          Disk.Animate();
          SendData(Command.SetIcons, Disk.Mask);
          Thread.Sleep(250);
        }
        SendText("iMONLCDg", "Disk Spin CCW");
        Disk.RotateCCW();
        for (int i = 0; i < 16; i++)
        {
          Disk.Animate();
          SendData(Command.SetIcons, Disk.Mask);
          Thread.Sleep(250);
        }
        SendText("iMONLCDg", "Disk Flash");
        Disk.RotateOff();
        Disk.FlashOn();
        for (int i = 0; i < 16; i++)
        {
          Disk.Animate();
          SendData(Command.SetIcons, Disk.Mask);
          Thread.Sleep(250);
        }
        CloseLcd();
        _displayTest = false;
        Log.Debug("IDisplay(API) iMONLCDg.Setup() configure - display test complete");
      }
    }

    private void SetText(string Line1, string Line2)
    {
      // exception handler is not needed here - this function is in all versions of SG_VFD.dll
      lock (DWriteMutex)
      {
        iMONVFD_SetText(Line1, Line2);
        Log.Debug("iMONLCDg.SetText() Sending text to display");
        Thread.Sleep(_delay);
      }
    }

    // This routine must have an error handler - iMONLCD_SendData is not included in all versions of SG_VFD.dll
    private void SendData(ulong data)
    {
      try
      {
        iMONLCD_SendData(ref data);
        Log.Debug("iMONLCDg.SendData() Sending {0} to display", data.ToString("x0000000000000000"));
        Thread.Sleep(_delay);
      }
      catch (Exception ex)
      {
        _isDisabled = true;
        _errorMessage = ex.Message;
        Log.Info("iMONLCDg.SendData() caught exception '{0}'\nIs your SG_VFD.dll version 5.1 or higher??");
      }
    }

    //  overload to handle sending Int64 data
    private void SendData(Int64 data)
    {
      SendData((ulong)data);
    }

    // overload to handle a display command (class Command) and an additional option bitmask
    private void SendData(Command command, Int64 optionBitmask)
    {
      SendData((ulong)((Int64)command | optionBitmask));
    }

    // overload to handle a display command (class Command)
    private void SendData(Command command)
    {
      SendData((ulong)command);
    }

    // emulate the iMONVFD_SetText() function to support "Text Mode" on LCD displays
    private void SendText(string Line1, string Line2)
    {
      if (_useCustomFont) Log.Debug("iMONLCDg.SendText(): Using CustomFont");

      int k = 0;
      byte[] pixel = new byte[192];
      for (int i = 0; i < Math.Min(16, Line1.Length); i++)
      {
        char ch = Line1[i];
        int j;
        for (j = 5; j >= 0; j--)
        {
          if ((j + k) < 96)
          {
            if (_useCustomFont)
            {
              pixel[k + j] = BitReverse(CFont.PixelData(ch, j));
            }
            else
            {
              pixel[k + j] = BitReverse(_Font8x5[ch, j]);
            }
          }
        }
        k += 6;
      }
      k = 96; // do second line
      for (int i = 0; i < Math.Min(16, Line2.Length); i++)
      {
        char ch = Line2[i];
        int j;
        for (j = 5; j >= 0; j--)
        {
          if ((j + k) < 192)
          {
            if (_useCustomFont)
            {
              pixel[k + j] = BitReverse(CFont.PixelData(ch, j));
            }
            else
            {
              pixel[k + j] = BitReverse(_Font8x5[ch, j]);
            }
          }
        }
        k += 6;
      }

      SendPixelArray(pixel);
    }


    /// <summary>
    /// Send an array of 8x96x2 pixelse to the 16x96 display
    /// </summary>
    /// <param name="PixelArray[192]">byte</param>
    /// <returns></returns>		
    private void SendPixelArray(byte[] PixelArray)
    {
      // Array starts from the top 8 pixels on the left, across 96 columns,
      // then the bottom 8 pixels from the left, across 96 columns.
      // Bit 7 of the first 96 bytes is the top row of the display
      // Bit 0 is the 8th row
      if (PixelArray.Length > 192)
      {
        Log.Error("ERROR in iMONLCDg SendPixelArray");
      }

      // add the large icons if required
      if (_UseCustomIcons || _UseLargeIcons)
      {
        // shift the pixel array 18 rows to the right
        for (int i = 95; i > 17; i--)
        {
          PixelArray[i] = PixelArray[i - 18];
          PixelArray[i + 96] = PixelArray[i + 96 - 18];
        }
        for (int i = 0; i < 18; i++)
        {
          PixelArray[i] = 0;
          PixelArray[i + 96] = 0;
        }

        if (_UseLargeIcons & !_UseCustomIcons) Log.Debug("iMONLCDg.SendText(): Inserting Large Icons");
        if (_UseCustomIcons) Log.Debug("iMONLCDg.SendText(): Inserting Custom Large Icons");
        if (_UseInvertedIcons) Log.Debug("iMONLCDg.SendText(): Using inverted Large Icon data");
        for (int i = 0; i < 16; i++)
        {
          if (_UseCustomIcons)
          {
            PixelArray[i] = CustomLargeIcon.PixelData(_CurrentLargeIcon, i);
            PixelArray[i + 96] = CustomLargeIcon.PixelData(_CurrentLargeIcon, i + 16);
          }
          else
          {
            PixelArray[i] = _InternalLargeIcons[_CurrentLargeIcon, i];
            PixelArray[i + 96] = _InternalLargeIcons[_CurrentLargeIcon, i + 16];
          }
          if (_UseInvertedIcons)
          {
            PixelArray[i] ^= 0xFF;
            PixelArray[i + 96] ^= 0xFF;
          }
        }
      }

      // send the data to the display
      int DataControl = 0x20;

      // if scrolling too quickly, calls may get banked up and grind to a halt???
      lock (DWriteMutex) // must send all the data to LCD without being interrupted.?
      {
        for (int k = 0; k <= 27 * 7; k += 7)
        {
          Int64 PixelWord = DataControl;
          for (int i = 6; i >= 0; i--)
          {
            PixelWord <<= 8;
            if ((k + i) < PixelArray.Length)
            {
              PixelWord += PixelArray[k + i];
            }
          }
          if (DataControl <= 0x3B)
          {
            SendData(PixelWord);
          }
          DataControl++;
        }
      }
    }

    /// <summary>
    /// Displays the lines at the top and bottom of the LCD, based on a bit map input
    /// Used for Volume display and Progress display
    /// </summary>
    /// <returns></returns>	
    private void SetLinePixels(UInt32 TopLine, UInt32 BotLine, UInt32 TopProgress, UInt32 BotProgress)
    {
      lock (DWriteMutex)
      {
        Int64 Data;
        //Least sig. bit is on the right

        Data = ((Int64)TopProgress) << 8 * 4;
        Data += TopLine;
        Data &= 0x00FFFFFFFFFFFFFF;
        SendData(Command.SetLines0, Data);

        Data = ((Int64)TopProgress) >> 8 * 3;
        Data += ((Int64)BotProgress) << 8;
        Data += ((Int64)BotLine) << 8 * 5;
        Data &= 0x00FFFFFFFFFFFFFF;
        SendData(Command.SetLines1, Data);

        Data = ((Int64)BotLine) >> 8 * 2;
        SendData(Command.SetLines2, Data);
      }
    }

    /// <summary>
    /// Displays the lines at the top and bottom, based on a line length
    /// </summary>
    /// <description
    /// Positive length display bar from the left, negative length displays from the right
    /// No input checking; data should range from -32 to + 32
    /// </description>
    /// <returns></returns>	
    private void SetLineLength(int TopLine, int BotLine, int TopProgress, int BotProgress)
    {
      SetLinePixels(LengthToPixels(TopLine),
                    LengthToPixels(BotLine),
                    LengthToPixels(TopProgress),
                    LengthToPixels(BotProgress));
    }

    /// <summary>
    /// Private helper to convert length of a bar to a pixel bit map
    /// </summary>
    /// <param name="Length"int></param>
    /// <returns>UInt 32Bit map equivalent of the line length</returns>	
    private static UInt32 LengthToPixels(int Length)
    {
      UInt32[] PixLen =
        {
          0x00, 0x00000080, 0x000000c0, 0x000000e0, 0x000000f0,
          0x000000f8, 0x000000fc, 0x000000fe, 0x000000ff,
          0x000080ff, 0x0000c0ff, 0x0000e0ff, 0x0000f0ff,
          0x0000f8ff, 0x0000fcff, 0x0000feff, 0x0000ffff,
          0x0080ffff, 0x00c0ffff, 0x00e0ffff, 0x00f0ffff,
          0x00f8ffff, 0x00fcffff, 0x00feffff, 0x00ffffff,
          0x80ffffff, 0xc0ffffff, 0xe0ffffff, 0xf0ffffff,
          0xf8ffffff, 0xfcffffff, 0xfeffffff, 0xffffffff
        };

      if (Math.Abs(Length) > 32)
      {
        return (0);
      }

      if (Length >= 0)
      {
        return PixLen[Length];
      }
      else
      {
        return (PixLen[32 + Length] ^ 0xFFFFFFFF);
      }
    }


    /// <summary>
    /// Clear Display
    /// Send the same intialisation sequence debugged from an iMon run
    /// </summary>
    /// <returns></returns>	
    private void ClearDisplay()
    {
      Clear();    // functionality has been moved to clear() - kept for future expansion
    }

    private void ClearPixels()
    {
      Clear();    // functionality has been moved to clear() - kept for future expansion
    }


    /// <summary>
    /// Reverse the bits in a byte
    /// </summary>
    /// <param name="inByte">byte</param>
    /// <returns>byte</returns>
    private static byte BitReverse(byte inByte)
    {
      byte result = 0x00;
      byte mask;

      for (mask = 0x80;
           Convert.ToInt32(mask) > 0;
           mask >>= 1)
      {
        result >>= 1;
        byte tempbyte = (byte)(inByte & mask);
        if (tempbyte != 0x00)
        {
          result |= 0x80;
        }
      }
      return (result);
    }

    public void Check_iMON_Manager_Status()
    {
      // check the registry and make sure that the iMON manager or Antec VFD is configured
      // correctly so that we can use the display. If the configuration is wrong, fix it so that
      // the display can be used properly and restart the manager (so that remote still functions)

      Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Checking iMON/VFD Manager configuration");

      // check for Antec registry entries first
      RegistryKey rKey;
      Process[] VFDproc;
      Process VFDnew;
      int rValue;
      bool hasExited;

      Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Checking Antec VFD Manager registry subkey.");
      rKey = Registry.CurrentUser.OpenSubKey("Software\\Antec\\VFD", true);
      if (rKey != null)
      {
        Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager registry subkey found.");
        rValue = (int)rKey.GetValue("RunFront", 0);
        if (rValue > 0)
        {
          // the manager is set to run always or automatically
          Log.Info("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager is not set correctly and is controlling the display. The configuration has been corrected.");
          rKey.SetValue("RunFront", 0, RegistryValueKind.DWord);
          // ensure the change is saved to the registry
          Registry.CurrentUser.Close();
          Thread.Sleep(100);
          // attempt to restart the VFD program
          VFDproc = Process.GetProcessesByName("VFD");
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Found {0} instances of Antec VFD Manager", VFDproc.Length);
          if (VFDproc.Length > 0)
          {
            // the VFD manager is running.. restart it
            Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Stopping VFD Manager");
            VFDproc[0].Kill();
            hasExited = false;
            while (!hasExited)
            {
              Thread.Sleep(100);
              Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for VFD Manager to exit");
              VFDproc[0].Dispose();
              VFDproc = Process.GetProcessesByName("VFD");
              if (VFDproc.Length == 0) hasExited = true;
            }
            Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): VFD Manager Stopped");
            // restart the VFD.exe process
            VFDnew = new Process();
            VFDnew.StartInfo.WorkingDirectory = "c:\\Program Files\\ANTEC\\VFD\\";
            VFDnew.StartInfo.FileName = "VFD.exe";
            Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): ReStarting VFD Manager");
            Process.Start(VFDnew.StartInfo);
          }
        }
        else
        {
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager registry entries are correct.");
          rKey.SetValue("RunFront", 0, RegistryValueKind.DWord);
        }
        Registry.CurrentUser.Close();
      }
      else
      {
        Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager registry subkey NOT FOUND.");
        Registry.CurrentUser.Close();
        VFDproc = Process.GetProcessesByName("VFD");
        Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): state check: Found {0} instances of Antec VFD Manager", VFDproc.Length);
        if (VFDproc.Length > 0)
        {
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: Forcing shutdown of Antec VFD Manager");
          VFDproc[0].Kill();
          hasExited = false;
          while (!hasExited)
          {
            Thread.Sleep(100);
            Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for VFD Manager to exit");
            VFDproc[0].Dispose();
            VFDproc = Process.GetProcessesByName("VFD");
            if (VFDproc.Length == 0) hasExited = true;
          }
          Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: Antec VFD Manager Stopped");
        }
      }
      // then check for SoundGraph registry entries
      Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Checking SoundGraph iMON Manager registry subkey.");
      rKey = Registry.CurrentUser.OpenSubKey("Software\\SOUNDGRAPH\\iMON", true);
      if (rKey != null)
      {
        Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The Soundgraph iMON Manager registry subkey found.");
        rValue = (int)rKey.GetValue("RunFront", 0);
        if (rValue > 0)
        {
          // the manager is set to run always or automatically
          Log.Info("IDisplay: iMONLCDgCheck_iMON_Manager_Status(): The Soundgraph iMON Manager is not set correctly and is controlling the display. The configuration has been corrected.");
          rKey.SetValue("RunFront", 0, RegistryValueKind.DWord);
          // ensure the change is saved to the registry
          Registry.CurrentUser.Close();
          Thread.Sleep(100);
          // attempt to restart the iMON program
          VFDproc = Process.GetProcessesByName("iMON");
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Found {0} instances of SoundGraph iMON Manager", VFDproc.Length);
          if (VFDproc.Length > 0)
          {
            // the iMON manager is running.. restart it
            Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Stopping iMON Manager");
            VFDproc[0].Kill();
            hasExited = false;
            while (!hasExited)
            {
              Thread.Sleep(100);
              Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for iMON Manager to exit");
              VFDproc[0].Dispose();
              VFDproc = Process.GetProcessesByName("iMON");
              if (VFDproc.Length == 0) hasExited = true;
            }
            Log.Info("iMONLCDg.Check_iMON_Manager_Status(): iMON Manager Stopped");
            // restart the iMON.exe process
            VFDnew = new Process();
            VFDnew.StartInfo.WorkingDirectory = "c:\\Program Files\\SOUNDGRAPH\\iMON\\";
            VFDnew.StartInfo.FileName = "iMON.exe";
            Log.Info("iMONLCDg.Check_iMON_Manager_Status(): ReStarting iMON Manager");
            Process.Start(VFDnew.StartInfo);
          }
        }
        else
        {
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The SoundGraph iMON Manager registry entries are correct.");
          rKey.SetValue("RunFront", 0, RegistryValueKind.DWord);
        }
        Registry.CurrentUser.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
        Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): SoundGraph Registry subkey NOT FOUND");
        VFDproc = Process.GetProcessesByName("iMON");
        Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): state check: Found {0} instances of SoundGraph iMON Manager", VFDproc.Length);
        if (VFDproc.Length > 0)
        {
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: Forcing shutdown of SoundGraph iMON Manager");
          VFDproc[0].Kill();
          hasExited = false;
          while (!hasExited)
          {
            Thread.Sleep(100);
            Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for iMON Manager to exit");
            VFDproc[0].Dispose();
            VFDproc = Process.GetProcessesByName("iMON");
            if (VFDproc.Length == 0) hasExited = true;
          }
          Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: Soundgraph iMON Manager Stopped");
        }
      }
      Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): iMON/VFD Manager configuration check completed");
    }


    #endregion

    #region Autodetect functions and structures
    // these functions help determine the display type from firmware and hardware information returned by SG_RC.dll
    private static readonly int[,] _iMON_FW_Display = {
            // firmware version, _VfdType, _VfdReserved, _DisplayType
            {0x08, 0x02, 0x0000, 0},
          	{0x1E, 0x17, 0x0000, 0},
          	{0x30, 0x04, 0x0000, 0},
          	{0x31, 0x04, 0x0000, 0},
          	{0x32, 0x04, 0x0000, 0},
          	{0x33, 0x06, 0x0000, 0},
          	{0x34, 0x06, 0x0000, 0},
          	{0x35, 0x06, 0x0000, 0},
          	{0x36, 0x06, 0x0000, 0},
          	{0x37, 0x06, 0x0000, 0},		// Antec Fusion (original version) VFD - no remote receiver
          	{0x38, 0x06, 0x0000, 0},
          	{0x39, 0x0A, 0x0000, 0},
          	{0x39, 0x06, 0x0000, 0},
          	{0x3A, 0x09, 0x0000, 2},		// 3Rsystems custom OEM LCD device - unsupported
          	{0x3B, 0x11, 0x0000, 2},		// 3Rsystems custom OEM LCD device - unsupported
          	{0x3C, 0x06, 0x0000, 0},
          	{0x3D, 0x10, 0x0000, 0},
          	{0x3E, 0x0B, 0x0000, 0},
          	{0x3F, 0x0B, 0x0000, 0},
          	{0x40, 0x08, 0x0000, 0},
          	{0x41, 0x0C, 0x0000, 0},
          	{0x42, 0x0C, 0x0000, 0},
          	{0x43, 0x0C, 0x0000, 0},
          	{0x44, 0x0C, 0x0000, 0},
          	{0x45, 0x0C, 0x0000, 0},
          	{0x46, 0x0C, 0x0000, 0},
          	{0x47, 0x0C, 0x0000, 0},
          	{0x48, 0x0D, 0x0000, 0},
          	{0x49, 0x12, 0x0000, 0},
          	{0x4A, 0x0D, 0x0000, 0},
          	{0x4B, 0x14, 0x0000, 0},
          	{0x4C, 0x0D, 0x0000, 0},
          	{0x4D, 0x0D, 0x0000, 0},
          	{0x4E, 0x0D, 0x0000, 0},
          	{0x4F, 0x0D, 0x0000, 0},
          	{0x70, 0x07, 0x0000, 0},
          	{0x71, 0x07, 0x0000, 0},
          	{0x72, 0x07, 0x0000, 0},
          	{0x73, 0x07, 0x0000, 0},
          	{0x74, 0x07, 0x0000, 0},
          	{0x75, 0x07, 0x0000, 0},
          	{0x76, 0x07, 0x0000, 0},
          	{0x77, 0x07, 0x0000, 0},
          	{0x78, 0x0E, 0x0000, 0},
          	{0x79, 0x0E, 0x0000, 0},
          	{0x7A, 0x0E, 0x0000, 0},
          	{0x7B, 0x0E, 0x0000, 0},
          	{0x7C, 0x0E, 0x0000, 0},
          	{0x7D, 0x0E, 0x0000, 0},
          	{0x7E, 0x0E, 0x0000, 0},
          	{0x7F, 0x0E, 0x0000, 0},
          	{0x80, 0x0F, 0x0000, 0},
          	{0x81, 0x0F, 0x0000, 0},
          	{0x82, 0x0F, 0x0000, 0},
          	{0x83, 0x0F, 0x0000, 0},
          	{0x84, 0x10, 0x0000, 0},
          	{0x85, 0x10, 0x0000, 0},
          	{0x86, 0x10, 0x0000, 0},
          	{0x87, 0x10, 0x0000, 0},        // Thermaltake MediaLab VFD - has remote receiver
          	{0x88, 0x10, 0x0000, 0},
          	{0x89, 0x10, 0x0000, 0},
          	{0x8A, 0x10, 0x0000, 0},
          	{0x8B, 0x10, 0x0000, 0},
          	{0x8C, 0x10, 0x0000, 0},
          	{0x8D, 0x10, 0x0000, 0},
          	{0x8E, 0x10, 0x0000, 0},
          	{0x8F, 0x10, 0x0000, 0},
          	{0x90, 0x13, 0x8888, 1},		// Unknown LCD
          	{0x91, 0x13, 0x8888, 1},		// Unknown LCD
          	{0x92, 0x15, 0x8888, 1},		// Unknown LCD
          	{0x93, 0x15, 0x8888, 1},		// Unknown LCD
          	{0x94, 0x15, 0x8888, 1},		// Unknown LCD
          	{0x95, 0x15, 0x8888, 1},		// Unknown LCD
          	{0x96, 0x15, 0x8888, 1},		// Unknown LCD
          	{0x97, 0x15, 0x8888, 1},		// Unknown LCD
          	{0x98, 0x18, 0x8888, 1},		// SilverStone GDMX-01 / MFC51 LCD
          	{0x99, 0x18, 0x8888, 1},		// Unknown LCD
          	{0x9A, 0x19, 0x8888, 1},		// Unknown LCD
          	{0x9B, 0x19, 0x8888, 1},		// Unknown LCD
          	{0x9C, 0x16, 0x8888, 1},		// Unknown LCD
          	{0x9D, 0x16, 0x8888, 1},		// Unknown LCD
          	{0x9E, 0x16, 0x8888, 1},		// Unknown LCD
          	{0x9F, 0x16, 0x8888, 1},		// Unknown LCD
          	{0xA0, 0x10, 0x0000, 0},        // Unknown Antec VFD
          	{0xA1, 0x16, 0x8888, 1},		// Antec Fusion Black (Model 420/430) LCD
          	{0x00, 0x00, 0x0000, 2}
      };

    private static void GetDisplayInfoFromFirmware(int FWVersion)
    {
      int i = 0;
      while ((_iMON_FW_Display[i, 0] != 0) & (i < _iMON_FW_Display.Length))
      {
        if (_iMON_FW_Display[i, 0] == FWVersion)
        {
          _VfdType = _iMON_FW_Display[i, 1];
          _DisplayType = _iMON_FW_Display[i, 3];
          return;
        }
        i++;
      }
      return;
    }

    private static void GetDisplayInfoFromRegistry(int REGVersion)
    {
      int i = 0;
      while ((_iMON_FW_Display[i, 0] != 0) & (i < _iMON_FW_Display.Length))
      {
        if (_iMON_FW_Display[i, 1] == REGVersion)
        {
          _VfdType = _iMON_FW_Display[i, 1];
          _DisplayType = _iMON_FW_Display[i, 3];
          return;
        }
        i++;
      }
      return;
    }

    #endregion

    #region Status functions for ICON Update Thread

    public class DiskIcon
    {
      private readonly Int64[] _DiskMask =  {   0x0080FE0000000000,
                                                  0x0080FD0000000000,
                                                  0x0080FB0000000000,
                                                  0x0080F70000000000,
                                                  0x0080EF0000000000,
                                                  0x0080DF0000000000,
                                                  0x0080BF0000000000,
                                                  0x00807F0000000000};
      private readonly Int64[] _DiskMaskInv = { 0x0080010000000000,
                                                  0x0080020000000000,
                                                  0x0080040000000000,
                                                  0x0080080000000000,
                                                  0x0080100000000000,
                                                  0x0080200000000000,
                                                  0x0080400000000000,
                                                  0x0080800000000000};
      private readonly Int64 _diskSolidOnMask = 0x0080FF0000000000;
      private readonly Int64 _diskSolidOffMask = 0x0000000000000000;
      private bool _diskOn = false;
      private bool _diskFlash = false;
      private bool _diskRotate = false;
      private int _flashState = 1;
      private int _diskSegment = 0;
      private bool _diskRotateClockwise = true;
      private bool _diskInverted = false;
      private bool _diskSRWFlash = true;

      public Int64 Mask
      {
        get
        {
          //                Log.Debug("flashing: {0} FLASH : {1} Rotate: {2}",_diskFlash, _flashState.ToString(),_diskRotate );
          if (!_diskOn)
          {
            // disk is off
            return _diskSolidOffMask;
          }
          else
          {
            // disk is on
            if (!_diskRotate)
            {
              // disk is on and not rotating
              if (!_diskFlash)
              {
                // disk is on and not flashing
                return _diskSolidOnMask;
              }
              else
              {
                // disk is on solid and flashing
                if (_flashState == 1)
                {
                  // disk flash state is on
                  return _diskSolidOnMask;
                }
                else
                {
                  // disk flash state if off
                  return _diskSolidOffMask;
                }
              }
            }
            else
            {
              if (!_diskFlash)
              {
                // disk is on and rotating and not flashing
                if (!_diskInverted)
                {
                  return _DiskMask[_diskSegment];
                }
                else
                {
                  return _DiskMaskInv[_diskSegment];
                }
              }
              else
              {
                // disk is on and rotating and flashing
                if (_flashState > 0)
                {
                  // disk flash state is on
                  if (!_diskInverted)
                  {
                    return _DiskMask[_diskSegment];
                  }
                  else
                  {
                    return _DiskMaskInv[_diskSegment];
                  }
                }
                else
                {
                  // disk flash state is off
                  return _diskSolidOffMask;
                }
              }
            }
          }
        }
      }

      public void On()
      {
        _diskOn = true;
      }
      public void Off()
      {
        _diskOn = false;
      }
      //
      public void InvertOn()
      {
        _diskInverted = true;
      }
      public void InvertOff()
      {
        _diskInverted = false;
      }
      //
      public void FlashOn()
      {
        _diskFlash = true;
      }
      public void FlashOff()
      {
        _diskFlash = false;
        _flashState = 1;
      }
      //
      public void RotateCW()
      {
        _diskRotateClockwise = true;
        _diskRotate = true;
      }
      public void RotateCCW()
      {
        _diskRotateClockwise = false;
        _diskRotate = true;
      }
      public void RotateOff()
      {
        _diskRotateClockwise = false;
        _diskRotate = false;
      }
      //
      public void Reset()
      {
        _diskFlash = false;
        _diskRotate = false;
        _diskSegment = 0;
        _diskRotateClockwise = true;
        _diskOn = false;
        _flashState = 1;
        _diskInverted = false;
        _diskSRWFlash = true;
      }
      //
      public bool IsOn
      {
        get { return _diskOn; }
      }
      public bool IsFlashing
      {
        get { return _diskFlash; }
      }
      public bool IsInverted
      {
        get { return _diskInverted; }
      }
      public bool IsRotating
      {
        get { return _diskFlash; }
      }
      //
      public void SRWFlashOn()
      {
        _diskSRWFlash = true;
      }
      public void SRWFlashOff()
      {
        _diskSRWFlash = false;
      }
      //
      public void Animate()
      {
        if ((_diskRotate & !_diskFlash) || (_diskRotate & (_diskFlash & !_diskSRWFlash)))
        {
          if (_diskRotateClockwise)
          {
            _diskSegment++;
            if (_diskSegment > 7) _diskSegment = 0;
          }
          else
          {
            _diskSegment--;
            if (_diskSegment < 0) _diskSegment = 7;
          }
        }
        if (_diskFlash)
        {
          if (_flashState == 1)
          {
            _flashState = 0;
          }
          else
          {
            _flashState = 1;
          }
        }
      }
      //
    }

    // this method runs in a seperate thread, collects system statistics and updates the LCD
    // icons appropriately
    private void UpdateIcons()
    {
      string currentItem;
      string[] temp;
      Int64 ICON_STATUS = 0; ;
      bool flashStatus = false;
      DiskIcon Disk = new DiskIcon();
      Log.Debug("iMONLCDg.UpdateIcons() Starting Icon Update Thread");
      char[] DriveLetters;
      int volLevel = 0;
      int progLevel = 0;
      BuiltinIconMask bICON = new BuiltinIconMask();
      CDDrive CD = new CDDrive();

      // initialize drive monitoring
      for (int i = 0; i < 27; i++)
      {
        Inserted_Media[i] = 0;
      }
      // set initial state of inserted volumes
      DriveLetters = CDDrive.GetCDDriveLetters();
      Log.Debug("iMONLCDg.UpdateIcons() Found {0} CD/DVD Drives.", DriveLetters.Length.ToString());
      for (int i = 0; i < DriveLetters.Length; i++)
      {
        if (CD.Open(DriveLetters[i]))
        {
          Log.Debug("iMONLCDg.UpdateIcons() Checking media in Drive {0}.", DriveLetters[i].ToString());
          bool DriveReady = false;
          // allow 1 second for drive to become ready
          for (int j = 0; j < 10; j++)
          {
            if (CD.IsCDReady())
            {
              DriveReady = true;
            }
            else
            {
              Thread.Sleep(50);
            }
          }
          if (DriveReady)
          {
            Log.Debug("iMONLCDg.UpdateIcons() Waiting for Drive {0} to refresh.", DriveLetters[i].ToString());
            CD.Refresh();
            int isCD = CD.GetNumAudioTracks();
            if (isCD > 0)
            {
              Inserted_Media[DriveLetters[i] - 'A'] = 1;
              Log.Debug("iMONLCDg.UpdateIcons() Found Audio CD in Drive {0}.", DriveLetters[i].ToString());
            }
            else if (File.Exists(DriveLetters[i] + "\\VIDEO_TS"))
            {
              Inserted_Media[DriveLetters[i] - 'A'] = 2;
              Log.Debug("iMONLCDg.UpdateIcons() Found DVD in Drive {0}.", DriveLetters[i].ToString());
            }
            else
            {
              Inserted_Media[DriveLetters[i] - 'A'] = 4;
              Log.Debug("iMONLCDg.UpdateIcons() Unknown media found in Drive {0}.", DriveLetters[i].ToString());
            }
          }
          else
          {
            Inserted_Media[DriveLetters[i] - 'A'] = 0;
            Log.Debug("iMONLCDg.UpdateIcons() No media found in Drive {0}.", DriveLetters[i].ToString());
          }
        }
        CD.Close();
      }

      // Set up a device monitor to monitor media insertion and removal from CD/DVD drives
      DVM = new Win32.Utils.Cd.DeviceVolumeMonitor();
      DVM.OnVolumeInserted += VolumeInserted;
      DVM.OnVolumeRemoved += VolumeRemoved;
      DVM.AsynchronousEvents = true;
      DVM.Enabled = true;

      Disk.Reset();
      while (true)
      {
        if (_stopUpdateIconThread)
        {
          Log.Debug("iMONLCDg.UpdateIcons() - Icon Update Thread terminating");
          _stopUpdateIconThread = false;
          if (DVM != null)
            DVM.Dispose();
          DVM = null;
          return;
        }
        Int64 Current_Status = ICON_STATUS;
        flashStatus = !flashStatus;
        int LastCurrentLargeIcon = _CurrentLargeIcon;
        int LastVolLevel = volLevel;
        int LastProgLevel = progLevel;
        int NewCurrentLargeIcon = 0;
        ICON_STATUS = 0x0;
        Disk.Off();
        Disk.Animate();
        // check to see if the recording icon nedds to be turned on
        Log.Debug("iMONLCDg.UpdateIcons() - Checking TV Card status: IsAnyCardRecording = {0}, IsViewing = {1}", Recorder.IsAnyCardRecording().ToString(), Recorder.IsViewing().ToString());
        if (Recorder.IsAnyCardRecording())
        {
          ICON_STATUS |= bICON.ICON_Rec;
          NewCurrentLargeIcon = (int)LargeIconType.RECORDING;
          Log.Debug("iMONLCDg.UpdateIcons() - Setting RECORDING icon");
        }
        else if (Recorder.IsViewing())
        {
          ICON_STATUS |= bICON.ICON_TV;
          NewCurrentLargeIcon = (int)LargeIconType.TV;
          Log.Debug("iMONLCDg.UpdateIcons() - Setting TV icon");
          if (g_Player.IsTimeShifting == false)
          {
            Disk.On();
            Disk.InvertOn();
            Disk.RotateCW();
          }
        }
        Log.Debug("iMONLCDg.UpdateIcons() - Checking g_player status: IsTV = {0}, IsTVRecording = {1}, Playing = {2}, Paused = {3}, IsTimeshifting = {4}", Recorder.IsAnyCardRecording().ToString(), Recorder.IsViewing().ToString(), g_Player.Playing.ToString(), g_Player.Paused.ToString(), g_Player.IsTimeShifting.ToString());
        if (g_Player.Playing)
        {
          // determine the type of file that is playing
          if ((g_Player.IsTV || g_Player.IsTVRecording) & !(g_Player.IsDVD || g_Player.IsCDA))
          {
            ICON_STATUS |= bICON.ICON_TV;
            NewCurrentLargeIcon = (int)LargeIconType.TV;
            Log.Debug("iMONLCDg.UpdateIcons() - setting TV Icon");
            Disk.On();
            Disk.InvertOn();
            if (g_Player.Playing & !g_Player.Paused)
            {
              if (g_Player.Speed > 0)
              {
                Disk.RotateCW();
              }
              else if (g_Player.Speed < 0)
              {
                Disk.RotateCCW();
              }
              Disk.FlashOff();
            }
            else
            {
              Disk.FlashOn();
              NewCurrentLargeIcon = (int)LargeIconType.PAUSED;
              Log.Debug("iMONLCDg.UpdateIcons() - Setting PAUSED icon");
            }
          }

          if (g_Player.IsDVD || g_Player.IsCDA)
          {
            if (g_Player.IsDVD & g_Player.IsVideo)
            {
              ICON_STATUS |= bICON.ICON_Movie;
              NewCurrentLargeIcon = (int)LargeIconType.MOVIE;
              Log.Debug("iMONLCDg.UpdateIcons() - Setting MOVIE icon");
            }
            else if (g_Player.IsCDA & !g_Player.IsVideo)
            {
              ICON_STATUS |= bICON.ICON_Music;
              NewCurrentLargeIcon = (int)LargeIconType.MUSIC;
              Log.Debug("iMONLCDg.UpdateIcons() - Setting MUSIC icon");
            }
            ICON_STATUS |= bICON.ICON_CD_DVD;
            Disk.On();
            Disk.InvertOff();
            if (g_Player.Playing & !g_Player.Paused)
            {
              if (g_Player.Speed > 0)
              {
                Disk.RotateCW();
              }
              else if (g_Player.Speed < 0)
              {
                Disk.RotateCCW();
              }
              Disk.FlashOff();
            }
            else
            {
              Disk.FlashOn();
              NewCurrentLargeIcon = (int)LargeIconType.PAUSED;
              Log.Debug("iMONLCDg.UpdateIcons() - setting PAUSED icon");
            }
          }

          if (g_Player.IsMusic)
          {
            ICON_STATUS |= bICON.ICON_Music;
            NewCurrentLargeIcon = (int)LargeIconType.MUSIC;
            Log.Debug("iMONLCDg.UpdateIcons() - Setting MUSIC icon");
            Disk.On();
            Disk.InvertOn();
            if (g_Player.Playing & !g_Player.Paused)
            {
              if (g_Player.Speed > 0)
              {
                Disk.RotateCW();
              }
              else if (g_Player.Speed < 0)
              {
                Disk.RotateCCW();
              }
              Disk.FlashOff();
            }
            else
            {
              Disk.FlashOn();
              NewCurrentLargeIcon = (int)LargeIconType.PAUSED;
              Log.Debug("iMONLCDg.UpdateIcons() - Setting PAUSED icon");
            }
            if (!_useDiskForAllMedia) Disk.Off();
            // determine the media type
            currentItem = GUIPropertyManager.GetProperty("#Play.Current.File");
            if (currentItem.Length > 0)
            {
              // there is a currently playing file - set appropriate icons
              temp = currentItem.Split('.');
              if (temp.Length > 1)
              {
                switch (temp[1])
                {
                  case "mp3":
                    ICON_STATUS |= bICON.ICON_MP3;
                    break;
                  case "ogg":
                    ICON_STATUS |= bICON.ICON_OGG;
                    break;
                  case "wma":
                    ICON_STATUS |= bICON.ICON_WMA;
                    break;
                  case "wav":
                    ICON_STATUS |= bICON.ICON_WAV;
                    break;
                }
              }
            }
          }

          if (g_Player.IsVideo & !g_Player.IsDVD)
          {
            ICON_STATUS |= bICON.ICON_Movie;
            NewCurrentLargeIcon = (int)LargeIconType.VIDEO;
            Log.Debug("iMONLCDg.UpdateIcons() - Setting MOVIE/VIDEO icon");
            Disk.On();
            Disk.InvertOn();
            if (g_Player.Playing & !g_Player.Paused)
            {
              if (g_Player.Speed > 0)
              {
                Disk.RotateCW();
              }
              else if (g_Player.Speed < 0)
              {
                Disk.RotateCCW();
              }
              Disk.FlashOff();
            }
            else
            {
              Disk.FlashOn();
              NewCurrentLargeIcon = (int)LargeIconType.PAUSED;
              Log.Debug("iMONLCDg.UpdateIcons() - Setting PAUSED icon");
            }
            if (!_useDiskForAllMedia) Disk.Off();
            // determine the media type
            currentItem = GUIPropertyManager.GetProperty("#Play.Current.File");
            Log.Debug("current file: {0}", currentItem);
            if (currentItem.Length > 0)
            {
              ICON_STATUS |= bICON.ICON_Music;
              // there is a currently playing file - set appropriate icons
              temp = currentItem.Split('.');
              if (temp.Length > 1)
              {
                switch (temp[1].ToLower())
                {
                  case "ifo":
                  case "vob":
                  case "mpg":
                    ICON_STATUS |= bICON.ICON_MPG;
                    break;
                  case "wmv":
                    ICON_STATUS |= bICON.ICON_WMV;
                    break;
                  case "divx":
                    ICON_STATUS |= bICON.ICON_DivX;
                    break;
                  case "xvid":
                    ICON_STATUS |= bICON.ICON_xVid;
                    break;
                }
              }
            }
            //                      Log.Debug("ExternalDisplay.iMONLCDg.UpdateIcons: Setting MOVIE icon");
          }
        }
        if ((!g_Player.Playing & !Recorder.IsViewing()) || !_useDiskForAllMedia)
        {
          int DiskType = 0;
          for (int i = 0; i < 27; i++)
          {
            DiskType |= Inserted_Media[i];
          }
          if (DiskType == 1)
          {
            ICON_STATUS |= bICON.ICON_CDIn;
          }
          else if (DiskType == 2)
          {
            ICON_STATUS |= bICON.ICON_DVDIn;
          }
          else if (DiskType > 0)
          {
            ICON_STATUS |= bICON.ICON_DiskOn;
          }
        }
        if (g_Player.Player == null)
        {
          if (_mpIsIdle)
          {
            // MediaPortal is in an idle state
            ICON_STATUS |= bICON.ICON_Time;
            NewCurrentLargeIcon = (int)LargeIconType.IDLE;
            Log.Debug("iMONLCDg.UpdateIcons() - Setting IDLE icon");
          }
        }
        //              if (flashStatus) ICON_STATUS |= ICON_SCR1;
        ICON_STATUS |= Disk.Mask;
        lock (DWriteMutex)
        {
          if (ICON_STATUS != Current_Status)
          {
            SendData(Command.SetIcons, ICON_STATUS);
            Log.Debug("iMONLCDg.UpdateIcons() Sending {0} to LCD.", ICON_STATUS.ToString("x0000000000000000"));
          }
          if (_useVolumeDisplay || _useProgressDisplay)
          {
            progLevel = 0;
            volLevel = 0;
            if ((g_Player.Playing || Recorder.IsViewing()) & _useVolumeDisplay)
            {
              if (!VolumeHandler.Instance.IsMuted) volLevel = (DShowNET.AudioMixer.AudioMixerHelper.GetVolume() / 2048);
            }
            if (g_Player.Playing & _useProgressDisplay) progLevel = (int)((((float)g_Player.CurrentPosition / (float)g_Player.Duration) - 0.01) * 32) + 1;
            if ((LastVolLevel != volLevel) || (LastProgLevel != progLevel))
            {
              SetLineLength(volLevel, progLevel, volLevel, progLevel);
              Log.Debug("iMONLCDg.UpdateIcons() Sending vol: {0} prog: {1} cur: {2} dur: {3} to LCD.", volLevel.ToString(), progLevel.ToString(), g_Player.CurrentPosition.ToString(), g_Player.Duration.ToString());
            }
          }
        }

        if (NewCurrentLargeIcon != LastCurrentLargeIcon) _CurrentLargeIcon = NewCurrentLargeIcon;
        // test code for RC receiver
        //              if (USBLink.USB_IsOpen())
        //              {
        //                  // check for an RC packet
        //                  ulong RCPacket;
        //                  RCPacket = USBLink.USB_GetRCPacket();
        //                  if ( (RCPacket != 0xFFFFFFFFFFFF9FFF) & (RCPacket != 0xFFFFFFFFFFFFFFFF) )
        //                  {
        //                      Log.Debug("iMONLCDg.UpdateIcons() Received RC packet {0}", RCPacket.ToString("x0000000000000000"));
        //                  }
        //              }
        Thread.Sleep(250);
      }
    }

    private static void VolumeInserted(int bitMask)
    {
      string driveLetter = DVM.MaskToLogicalPaths(bitMask);
      Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted() - volume inserted in drive {0}", driveLetter);
      // determine the media type
      CDDrive m_Drive = new CDDrive();

      if (m_Drive.IsOpened)
        m_Drive.Close();
      m_Drive.Open(driveLetter[0]);
      while (!m_Drive.IsCDReady())
      {
        Thread.Sleep(100);
      }
      m_Drive.Refresh();
      if (m_Drive.GetNumAudioTracks() > 0)
      {
        Inserted_Media[driveLetter[0] - 'A'] = 1;
        Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted() - Audio CD inserted in drive {0}", driveLetter);
        m_Drive.Close();
        return;
      }
      m_Drive.Close();
      if (Directory.Exists(driveLetter + "\\VIDEO_TS"))
      {
        Inserted_Media[driveLetter[0] - 'A'] = 2;
        Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted() - DVD inserted in drive {0}", driveLetter);
      }
      else
      {
        Inserted_Media[driveLetter[0] - 'A'] = 4;
        Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted() - Unknown Media inserted in drive {0}", driveLetter);
      }
    }

    private static void VolumeRemoved(int bitMask)
    {
      string driveLetter = DVM.MaskToLogicalPaths(bitMask);
      Log.Info("iMONLCDg.UpdateDisplay.VolumeRemoved() - volume removed from drive {0}", driveLetter);
      Inserted_Media[driveLetter[0] - 'A'] = 0;
    }
    //}


    #endregion


    #region Display Commands

    private enum Command : long
    {
      SetIcons = 0x0100000000000000,
      SetContrast = 0x0300000000000000,
      //Display = 0x5000000000000000,
      Shutdown = 0x5000000000000008,
      DisplayOn = 0x5000000000000040,
      ClearAlarm = 0x5100000000000000,
      SetLines0 = 0x1000000000000000,
      SetLines1 = 0x1100000000000000,
      SetLines2 = 0x1200000000000000
    }

    #endregion

    #region Display Icons (Built-in to the LCD unit)

    /// <summary>
    /// Display Icons bitmaps
    /// </summary>
    /// <returns>Int 64 with the icon bit set</returns>	
    //    public class Icons
    //    {
    public class BuiltinIconMask
    {
      public readonly Int64 ICON_ALL = 0x00FFFFFFFFFFFFFF;

      public readonly Int64 ICON_DiskOff = 0x7F7000FFFFFFFFFF;
      public readonly Int64 ICON_DiskOn = 0x0080FF0000000000;
      public readonly Int64 ICON_CDIn = 0x00806B0000000000;
      public readonly Int64 ICON_DVDIn = 0x0080550000000000;

      // Byte 5 of Command.SetIcon
      public readonly Int64 ICON_WMA2 = (Int64)0x1 << 39;
      public readonly Int64 ICON_WAV = (Int64)0x1 << 38;
      public readonly Int64 ICON_REP = (Int64)0x1 << 37;
      public readonly Int64 ICON_SFL = (Int64)0x1 << 36;
      public readonly Int64 ICON_Alarm = (Int64)0x1 << 35;
      public readonly Int64 ICON_Rec = (Int64)0x1 << 34;
      public readonly Int64 ICON_Vol = (Int64)0x1 << 33;
      public readonly Int64 ICON_Time = (Int64)0x1 << 32;

      // Byte 4 of Command.SetIcon
      public readonly Int64 ICON_xVid = 0x1 << 31;
      public readonly Int64 ICON_WMV = 0x1 << 30;
      public readonly Int64 ICON_MPG2 = 0x1 << 29;
      public readonly Int64 ICON_AC3 = 0x1 << 28;
      public readonly Int64 ICON_DTS = 0x1 << 27;
      public readonly Int64 ICON_WMA = 0x1 << 26;
      public readonly Int64 ICON_MP3 = 0x1 << 25;
      public readonly Int64 ICON_OGG = 0x1 << 24;

      //Byte 3 of Command.SetIcon
      public readonly Int64 ICON_SRC = 0x1 << 23;
      public readonly Int64 ICON_FIT = 0x1 << 22;
      public readonly Int64 ICON_TV_2 = 0x1 << 21;
      public readonly Int64 ICON_HDTV = 0x1 << 20;
      public readonly Int64 ICON_SCR1 = 0x1 << 19;
      public readonly Int64 ICON_SCR2 = 0x1 << 18;
      public readonly Int64 ICON_MPG = 0x1 << 17;
      public readonly Int64 ICON_DivX = 0x1 << 16;

      // Byte 2 of Command.SetIcon
      public readonly Int64 SPKR_FC = 0x1 << 15;
      public readonly Int64 SPKR_FR = 0x1 << 14;
      public readonly Int64 SPKR_SL = 0x1 << 13;
      public readonly Int64 SPKR_LFE = 0x1 << 12;
      public readonly Int64 SPKR_SR = 0x1 << 11;
      public readonly Int64 SPKR_RL = 0x1 << 10;
      public readonly Int64 SPKR_SPDIF = 0x1 << 9;
      public readonly Int64 SPKR_RR = 0x1 << 8;

      // Byte 1 of Command.SetIcon
      public readonly Int64 ICON_Music = 0x1 << 7;
      public readonly Int64 ICON_Movie = 0x1 << 6;
      public readonly Int64 ICON_Photo = 0x1 << 5;
      public readonly Int64 ICON_CD_DVD = 0x1 << 4;
      public readonly Int64 ICON_TV = 0x1 << 3;
      public readonly Int64 ICON_WebCast = 0x1 << 2;
      public readonly Int64 ICON_News = 0x1 << 1;
      public readonly Int64 SPKR_FL = 0x1;
    }
    #endregion


    #region Large Icon BitMap
    // enum to encapsulate the Large Icons definitions
    private enum LargeIconType
    {
      IDLE = 0,
      TV = 1,
      MOVIE = 2,
      MUSIC = 3,
      VIDEO = 4,
      RECORDING = 5,
      PAUSED = 6,
      //zSPARE2 = 7,
      //zSPARE3 = 8,
      //zSPARE4 = 9
    }

    // default data for Large Icons
    private static readonly byte[,] _InternalLargeIcons = { 
   // Large Icon 0 (Idle)
		{0xC0, 0x80, 0x80, 0xC0, 0xFF, 0xC0, 0x80, 0x80, 0xC0, 0xFF, 0xC7, 0x83, 0x93, 0x83, 0xC7, 0xFF,
         0x03, 0x01, 0x01, 0x03, 0xFF, 0x03, 0x01, 0x01, 0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF},
   // Large Icon 1 (TV)
        {0xFF, 0xFE, 0xFE, 0xBD, 0xDD, 0xED, 0xF5, 0xF9, 0xF9, 0xF5, 0xED, 0xDD, 0xBD, 0xFE, 0xFE, 0xFF,
         0xFF, 0x01, 0xF9, 0xFD, 0xFD, 0xFD, 0xFD, 0xFD, 0xFD, 0xFD, 0xFD, 0xFD, 0xFD, 0xF9, 0x01, 0xFF},
   // Large Icon 2 (MOVIE)
        {0xFF, 0x80, 0xAF, 0x8F, 0xAF, 0x8F, 0xAF, 0x8F, 0xAF, 0x8F, 0xAF, 0x8F, 0xAF, 0x8F, 0xA0, 0x8F,
         0xF5, 0x01, 0xF5, 0xF1, 0xF5, 0xF1, 0xF5, 0xF1, 0xF5, 0xF1, 0xF5, 0xF1, 0xF5, 0xF1, 0x05, 0x01},
   // Large Icon 3 (MUSIC)
        {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x80, 0xC7, 0xC3, 0xE3, 0xE3, 0xE3, 0xE3, 0xFF, 0xFF, 0xFF, 0xFF,
         0xFF, 0xFF, 0xF3, 0xE1, 0xE1, 0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF},
   // Large Icon 4 (VIDEO)
        {0xE3, 0xDE, 0xBF, 0xBB, 0xBF, 0xDE, 0xE1, 0xE1, 0xDE, 0xBF, 0xBB, 0xBF, 0xDE, 0xE1, 0xFF, 0xFF,
         0xFF, 0x01, 0x7D, 0x7D, 0x7D, 0x7D, 0x7D, 0x7D, 0x7D, 0x7D, 0x7D, 0x0D, 0xCF, 0xB7, 0xB7, 0x87},
   // Large Icon 5 (RECORDING)
		{0xF8, 0xF8, 0xF2, 0xF2, 0xF2, 0xE6, 0xE6, 0xCE, 0xCE, 0xCE, 0x9E, 0x9E, 0x3E, 0x3E, 0x3E, 0xFF,
         0x00, 0x1E, 0x3E, 0x7E, 0x5E, 0x1E, 0x3E, 0x7E, 0x5E, 0x1E, 0x3E, 0x7E, 0x5E, 0x1E, 0x0E, 0xFF},
   // Large Icon 6 (PAUSED)
		{0xFF, 0xFF, 0xFF, 0xFF, 0xC0, 0x80, 0x80, 0xC0, 0xFF, 0xC0, 0x80, 0x80, 0xC0, 0xFF, 0xFF, 0xFF, 
         0xFF, 0xFF, 0xFF, 0xFF, 0x03, 0x01, 0x01, 0x03, 0xFF, 0x03, 0x01, 0x01, 0x03, 0xFF, 0xFF, 0xFF},
   // Large Icon 7
		{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
   // Large Icon 8
		{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
   // Large Icon 9
		{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}
    };
    #endregion

    #region Font Bit Map

    // bit map updated Aug 31 2007 to fix numerous incorrect characters
    private static readonly byte[,] _Font8x5 = { 
												{0x00, 0x00, 0x00, 0x00, 0x00, 0x00},   //   0x 0 0
												{0x00, 0x64, 0x18, 0x04, 0x64, 0x18},   //  0x 1 1
												{0x00, 0x3c, 0x40, 0x40, 0x20, 0x7c},   //  0x 2 2
												{0x00, 0x0c, 0x30, 0x40, 0x30, 0x0c},   //  0x 3 3
												{0x00, 0x3c, 0x40, 0x30, 0x40, 0x3c},   //  0x 4 4
												{0x00, 0x00, 0x3e, 0x1c, 0x08, 0x00},   //  0x 5 5
												{0x00, 0x04, 0x1e, 0x1f, 0x1e, 0x04},   //  0x 6 6
												{0x00, 0x10, 0x3c, 0x7c, 0x3c, 0x10},   //  0x 7 7
												{0x00, 0x20, 0x40, 0x3e, 0x01, 0x02},   //  0x 8 8
												{0x00, 0x22, 0x14, 0x08, 0x14, 0x22},   //   0x 9 9
												{0x00, 0x00, 0x38, 0x28, 0x38, 0x00},   //   0x a 10
												{0x00, 0x00, 0x10, 0x38, 0x10, 0x00},   //  0x b 11
												{0x00, 0x00, 0x00, 0x10, 0x00, 0x00},   //  0x c 12
												{0x00, 0x08, 0x78, 0x08, 0x00, 0x00},   //   0x d 13
												{0x00, 0x00, 0x15, 0x15, 0x0a, 0x00},   //  0x e 14
												{0x00, 0x7f, 0x7f, 0x09, 0x09, 0x01},   //  0x f 15
												{0x00, 0x10, 0x20, 0x7f, 0x01, 0x01},   //  0x10 16
												{0x00, 0x04, 0x04, 0x00, 0x01, 0x1f},   //  0x11 17
												{0x00, 0x00, 0x19, 0x15, 0x12, 0x00},   //  0x12 18
												{0x00, 0x40, 0x60, 0x50, 0x48, 0x44},   //  0x13 19
												{0x00, 0x06, 0x09, 0x09, 0x06, 0x00},   //  0x14 20
												{0x00, 0x0f, 0x02, 0x01, 0x01, 0x00},   //  0x15 21
												{0x00, 0x00, 0x01, 0x1f, 0x01, 0x00},   //  0x16 22
												{0x00, 0x44, 0x44, 0x4a, 0x4a, 0x51},   //  0x17 23
												{0x00, 0x14, 0x74, 0x1c, 0x17, 0x14},   //  0x18 24
												{0x00, 0x51, 0x4a, 0x4a, 0x44, 0x44},   //  0x19 25
												{0x00, 0x00, 0x00, 0x04, 0x04, 0x04},   //   0x1a 26
												{0x00, 0x00, 0x7c, 0x54, 0x54, 0x44},   //  0x1b 27
												{0x00, 0x08, 0x08, 0x2a, 0x1c, 0x08},   //   0x1c 28
												{0x00, 0x7c, 0x00, 0x7c, 0x44, 0x7c},   //   0x1d 29
												{0x00, 0x04, 0x02, 0x7f, 0x02, 0x04},   //   0x1e 30
												{0x00, 0x10, 0x20, 0x7f, 0x20, 0x10},   //   0x1f 31
												{0x00, 0x00, 0x00, 0x00, 0x00, 0x00},   //   0x20 32
												{0x00, 0x00, 0x00, 0x6f, 0x00, 0x00},   // ! 0x21 33
												{0x00, 0x00, 0x07, 0x00, 0x07, 0x00},   // " 0x22 34
												{0x00, 0x14, 0x7f, 0x14, 0x7f, 0x14},   // # 0x23 35
												{0x00, 0x00, 0x07, 0x04, 0x1e, 0x00},   // $ 0x24 36
												{0x00, 0x23, 0x13, 0x08, 0x64, 0x62},   // % 0x25 37
												{0x00, 0x36, 0x49, 0x56, 0x20, 0x50},   // & 0x26 38
												{0x00, 0x00, 0x00, 0x07, 0x00, 0x00},   // ' 0x27 39
												{0x00, 0x00, 0x1c, 0x22, 0x41, 0x00},   // ( 0x28 40
												{0x00, 0x00, 0x41, 0x22, 0x1c, 0x00},   // ) 0x29 41
												{0x00, 0x14, 0x08, 0x3e, 0x08, 0x14},   // * 0x2a 42
												{0x00, 0x08, 0x08, 0x3e, 0x08, 0x08},   // + 0x2b 43
												{0x00, 0x00, 0x50, 0x30, 0x00, 0x00},   // , 0x2c 44
												{0x00, 0x08, 0x08, 0x08, 0x08, 0x08},   // - 0x2d 45
												{0x00, 0x00, 0x60, 0x60, 0x00, 0x00},   // . 0x2e 46
												{0x00, 0x20, 0x10, 0x08, 0x04, 0x02},   // / 0x2f 47
												{0x00, 0x3e, 0x51, 0x49, 0x45, 0x3e},   // 0 0x30 48
												{0x00, 0x00, 0x42, 0x7f, 0x40, 0x00},   // 1 0x31 49
												{0x00, 0x42, 0x61, 0x51, 0x49, 0x46},   // 2 0x32 50
												{0x00, 0x21, 0x41, 0x45, 0x4b, 0x31},   // 3 0x33 51
												{0x00, 0x18, 0x14, 0x12, 0x7f, 0x10},   // 4 0x34 52
												{0x00, 0x27, 0x45, 0x45, 0x45, 0x39},   // 5 0x35 53
												{0x00, 0x3c, 0x4a, 0x49, 0x49, 0x30},   // 6 0x36 54
												{0x00, 0x01, 0x71, 0x09, 0x05, 0x03},   // 7 0x37 55
												{0x00, 0x36, 0x49, 0x49, 0x49, 0x36},   // 8 0x38 56
												{0x00, 0x06, 0x49, 0x49, 0x29, 0x1e},   // 9 0x39 57
												{0x00, 0x00, 0x36, 0x36, 0x00, 0x00},   // : 0x3a 58
												{0x00, 0x00, 0x56, 0x36, 0x00, 0x00},   // ; 0x3b 59
												{0x00, 0x08, 0x14, 0x22, 0x41, 0x00},   // < 0x3c 60
												{0x00, 0x14, 0x14, 0x14, 0x14, 0x14},   // = 0x3d 61
												{0x00, 0x00, 0x41, 0x22, 0x14, 0x08},   // > 0x3e 62
												{0x00, 0x02, 0x01, 0x51, 0x09, 0x06},   // ? 0x3f 63
												{0x00, 0x3e, 0x41, 0x5d, 0x49, 0x4e},   // @ 0x40 64
												{0x00, 0x7e, 0x09, 0x09, 0x09, 0x7e},   // A 0x41 65
												{0x00, 0x7f, 0x49, 0x49, 0x49, 0x36},   // B 0x42 66
												{0x00, 0x3e, 0x41, 0x41, 0x41, 0x22},   // C 0x43 67
												{0x00, 0x7f, 0x41, 0x41, 0x41, 0x3e},   // D 0x44 68
												{0x00, 0x7f, 0x49, 0x49, 0x49, 0x41},   // E 0x45 69
												{0x00, 0x7f, 0x09, 0x09, 0x09, 0x01},   // F 0x46 70
												{0x00, 0x3e, 0x41, 0x49, 0x49, 0x7a},   // G 0x47 71
												{0x00, 0x7f, 0x08, 0x08, 0x08, 0x7f},   // H 0x48 72
												{0x00, 0x00, 0x41, 0x7f, 0x41, 0x00},   // I 0x49 73
												{0x00, 0x20, 0x40, 0x41, 0x3f, 0x01},   // J 0x4a 74
												{0x00, 0x7f, 0x08, 0x14, 0x22, 0x41},   // K 0x4b 75
												{0x00, 0x7f, 0x40, 0x40, 0x40, 0x40},   // L 0x4c 76
												{0x00, 0x7f, 0x02, 0x0c, 0x02, 0x7f},   // M 0x4d 77
												{0x00, 0x7f, 0x04, 0x08, 0x10, 0x7f},   // N 0x4e 78
												{0x00, 0x3e, 0x41, 0x41, 0x41, 0x3e},   // O 0x4f 79
												{0x00, 0x7f, 0x09, 0x09, 0x09, 0x06},   // P 0x50 80
												{0x00, 0x3e, 0x41, 0x51, 0x21, 0x5e},   // Q 0x51 81
												{0x00, 0x7f, 0x09, 0x19, 0x29, 0x46},   // R 0x52 82
												{0x00, 0x46, 0x49, 0x49, 0x49, 0x31},   // S 0x53 83
												{0x00, 0x01, 0x01, 0x7f, 0x01, 0x01},   // T 0x54 84
												{0x00, 0x3f, 0x40, 0x40, 0x40, 0x3f},   // U 0x55 85
												{0x00, 0x0f, 0x30, 0x40, 0x30, 0x0f},   // V 0x56 86
												{0x00, 0x3f, 0x40, 0x30, 0x40, 0x3f},   // W 0x57 87
												{0x00, 0x63, 0x14, 0x08, 0x14, 0x63},   // X 0x58 88
												{0x00, 0x07, 0x08, 0x70, 0x08, 0x07},   // Y 0x59 89
												{0x00, 0x61, 0x51, 0x49, 0x45, 0x43},   // Z 0x5a 90
												{0x00, 0x00, 0x7f, 0x41, 0x00, 0x00},   // [ 0x5b 91   
												{0x00, 0x02, 0x04, 0x08, 0x10, 0x20},   // \ 0x5c 92
												{0x00, 0x00, 0x41, 0x7f, 0x00, 0x00},   // ] 0x5d 93
												{0x00, 0x04, 0x02, 0x01, 0x02, 0x04},   // ^ 0x5e 94
												{0x00, 0x40, 0x40, 0x40, 0x40, 0x40},   // _ 0x5f 95
												{0x00, 0x00, 0x00, 0x03, 0x04, 0x00},   // ` 0x60 96
												{0x00, 0x20, 0x54, 0x54, 0x54, 0x78},   // a 0x61 97
												{0x00, 0x7f, 0x48, 0x44, 0x44, 0x38},   // b 0x62 98
												{0x00, 0x38, 0x44, 0x44, 0x44, 0x20},   // c 0x63 99
												{0x00, 0x38, 0x44, 0x44, 0x48, 0x7f},   // d 0x64 100
												{0x00, 0x38, 0x54, 0x54, 0x54, 0x18},   // e 0x65 101
												{0x00, 0x08, 0x7e, 0x09, 0x01, 0x02},   // f 0x66 102
												{0x00, 0x0c, 0x52, 0x52, 0x52, 0x3e},   // g 0x67 103
												{0x00, 0x7f, 0x08, 0x04, 0x04, 0x78},   // h 0x68 104
												{0x00, 0x00, 0x44, 0x7d, 0x40, 0x00},   // i 0x69 105
												{0x00, 0x20, 0x40, 0x44, 0x3d, 0x00},   // j 0x6a 106
												{0x00, 0x00, 0x7f, 0x10, 0x28, 0x44},   // k 0x6b 107
												{0x00, 0x00, 0x41, 0x7f, 0x40, 0x00},   // l 0x6c 108
												{0x00, 0x7c, 0x04, 0x18, 0x04, 0x78},   // m 0x6d 109
												{0x00, 0x7c, 0x08, 0x04, 0x04, 0x78},   // n 0x6e 110
												{0x00, 0x38, 0x44, 0x44, 0x44, 0x38},   // o 0x6f 111
												{0x00, 0x7c, 0x14, 0x14, 0x14, 0x08},   // p 0x70 112
												{0x00, 0x08, 0x14, 0x14, 0x18, 0x7c},   // q 0x71 113
												{0x00, 0x7c, 0x08, 0x04, 0x04, 0x08},   // r 0x72 114
												{0x00, 0x48, 0x54, 0x54, 0x54, 0x20},   // s 0x73 115
												{0x00, 0x04, 0x3f, 0x44, 0x40, 0x20},   // t 0x74 116
												{0x00, 0x3c, 0x40, 0x40, 0x20, 0x7c},   // u 0x75 117
												{0x00, 0x1c, 0x20, 0x40, 0x20, 0x1c},   // v 0x76 118
												{0x00, 0x3c, 0x40, 0x30, 0x40, 0x3c},   // w 0x77 119
												{0x00, 0x44, 0x28, 0x10, 0x28, 0x44},   // x 0x78 120
												{0x00, 0x0c, 0x50, 0x50, 0x50, 0x3c},   // y 0x79 121
												{0x00, 0x44, 0x64, 0x54, 0x4c, 0x44},   // z 0x7a 122
												{0x00, 0x00, 0x08, 0x36, 0x41, 0x41},   // { 0x7b 123
												{0x00, 0x00, 0x00, 0x7f, 0x00, 0x00},   // | 0x7c 124
												{0x00, 0x41, 0x41, 0x36, 0x08, 0x00},   // } 0x7d 125
												{0x00, 0x04, 0x02, 0x04, 0x08, 0x04},   // ~ 0x7e 126
												{0x00, 0x7f, 0x6b, 0x6b, 0x6b, 0x7f},   //  0x7f 127
												{0x00, 0x00, 0x7c, 0x44, 0x7c, 0x00},   // ¨ 0x80 128
												{0x00, 0x00, 0x08, 0x7c, 0x00, 0x00},   // Å 0x81 129
												{0x00, 0x00, 0x64, 0x54, 0x48, 0x00},   // Ç 0x82 130
												{0x00, 0x00, 0x44, 0x54, 0x28, 0x00},   // É 0x83 131
												{0x00, 0x00, 0x1c, 0x10, 0x78, 0x00},   // Ñ 0x84 132
												{0x00, 0x00, 0x5c, 0x54, 0x24, 0x00},   // Ö 0x85 133
												{0x00, 0x00, 0x78, 0x54, 0x74, 0x00},   // Ü 0x86 134
												{0x00, 0x00, 0x64, 0x14, 0x0c, 0x00},   // á 0x87 135
												{0x00, 0x00, 0x7c, 0x54, 0x7c, 0x00},   // à 0x88 136
												{0x00, 0x00, 0x5c, 0x54, 0x3c, 0x00},   // â 0x89 137
												{0x00, 0x78, 0x24, 0x26, 0x25, 0x78},   // ä 0x8a 138
												{0x00, 0x78, 0x25, 0x26, 0x24, 0x78},   // ã 0x8b 139
												{0x00, 0x70, 0x2a, 0x29, 0x2a, 0x70},   // å 0x8c 140
												{0x00, 0x78, 0x25, 0x24, 0x25, 0x78},   // ç 0x8d 141
												{0x00, 0x20, 0x54, 0x56, 0x55, 0x78},   // } 0x8e 142
												{0x00, 0x20, 0x55, 0x56, 0x54, 0x78},   // è 0x8f 143
												{0x00, 0x20, 0x56, 0x55, 0x56, 0x78},   // ê 0x90 144
												{0x00, 0x20, 0x55, 0x54, 0x55, 0x78},   // ë 0x91 145
												{0x00, 0x7c, 0x54, 0x56, 0x55, 0x44},   // í 0x92 146
												{0x00, 0x7c, 0x55, 0x56, 0x54, 0x44},   // ì 0x93 147
												{0x00, 0x7c, 0x56, 0x55, 0x56, 0x44},   // î 0x94 148
												{0x00, 0x7c, 0x55, 0x54, 0x55, 0x44},   // ï 0x95 149
												{0x00, 0x38, 0x54, 0x56, 0x55, 0x18},   // ñ 0x96 150
												{0x00, 0x38, 0x55, 0x56, 0x54, 0x18},   // ó 0x97 151
												{0x00, 0x38, 0x56, 0x55, 0x56, 0x18},   // ò 0x98 152
												{0x00, 0x38, 0x55, 0x54, 0x55, 0x18},   // ô 0x99 153
												{0x00, 0x00, 0x44, 0x7e, 0x45, 0x00},   // ö 0x9a 154
												{0x00, 0x00, 0x45, 0x7e, 0x44, 0x00},   // õ 0x9b 155
												{0x00, 0x00, 0x46, 0x7d, 0x46, 0x00},   // ú 0x9c 156
												{0x00, 0x00, 0x45, 0x7c, 0x45, 0x00},   // ù 0x9d 157
												{0x00, 0x00, 0x48, 0x7a, 0x41, 0x00},   // ~ 0x9e 158
												{0x00, 0x00, 0x49, 0x7a, 0x40, 0x00},   // ü 0x9f 159
												{0x00, 0x00, 0x4a, 0x79, 0x42, 0x00},   //   0xa0 160
												{0x00, 0x00, 0x49, 0x78, 0x41, 0x00},   // ° 0xa1 161
												{0x00, 0x38, 0x44, 0x46, 0x45, 0x38},   // ¢ 0xa2 162
												{0x00, 0x38, 0x45, 0x46, 0x44, 0x38},   // £ 0xa3 163
												{0x00, 0x38, 0x46, 0x45, 0x46, 0x38},   // § 0xa4 164
												{0x00, 0x38, 0x45, 0x44, 0x45, 0x38},   // • 0xa5 165
												{0x00, 0x30, 0x48, 0x4a, 0x49, 0x30},   // ¶ 0xa6 166
												{0x00, 0x30, 0x49, 0x4a, 0x48, 0x30},   // ß 0xa7 167
												{0x00, 0x30, 0x4a, 0x49, 0x4a, 0x30},   // ® 0xa8 168
												{0x00, 0x30, 0x49, 0x48, 0x49, 0x30},   // © 0xa9 169
												{0x00, 0x3c, 0x40, 0x42, 0x41, 0x3c},   // ™ 0xaa 170
												{0x00, 0x3c, 0x41, 0x42, 0x40, 0x3c},   // ´ 0xab 171
												{0x00, 0x3c, 0x42, 0x41, 0x42, 0x3c},   // ¨ 0xac 172
												{0x00, 0x3c, 0x41, 0x40, 0x41, 0x3c},   // ≠ 0xad 173
												{0x00, 0x3c, 0x40, 0x42, 0x21, 0x7c},   // Æ 0xae 174
												{0x00, 0x3c, 0x41, 0x42, 0x20, 0x7c},   // Ø 0xaf 175
												{0x00, 0x38, 0x42, 0x41, 0x22, 0x78},   // ∞ 0xb0 176
												{0x00, 0x3c, 0x41, 0x40, 0x21, 0x7c},   // ± 0xb1 177
												{0x00, 0x4e, 0x51, 0x71, 0x11, 0x0a},   // ≤ 0xb2 178
												{0x00, 0x58, 0x64, 0x64, 0x24, 0x10},   // ≥ 0xb3 179
												{0x00, 0x7c, 0x0a, 0x11, 0x22, 0x7d},   // ¥ 0xb4 180
												{0x00, 0x78, 0x12, 0x09, 0x0a, 0x71},   // µ 0xb5 181
												{0x00, 0x00, 0x00, 0x04, 0x02, 0x01},   // ∂ 0xb6 182
												{0x00, 0x01, 0x02, 0x04, 0x00, 0x00},   // ∑ 0xb7 183
												{0x00, 0x00, 0x02, 0x00, 0x02, 0x00},   // ∏ 0xb8 184
												{0x00, 0x30, 0x48, 0x45, 0x40, 0x20},   // π 0xb9 185
												{0x00, 0x00, 0x00, 0x7b, 0x00, 0x00},   // ∫ 0xba 186
												{0x00, 0x38, 0x44, 0x44, 0x38, 0x44},   // ª 0xbb 187
												{0x00, 0x40, 0x3e, 0x49, 0x49, 0x36},   // º 0xbc 188
												{0x00, 0x08, 0x04, 0x08, 0x70, 0x0c},   // Ω 0xbd 189
												{0x00, 0x60, 0x50, 0x48, 0x50, 0x60},   // æ 0xbe 190
												{0x00, 0x30, 0x48, 0x45, 0x40, 0x00},   // ø 0xbf 191
												{0x00, 0x7c, 0x13, 0x12, 0x12, 0x7c},   // ¿ 0xc0 192
												{0x00, 0x7c, 0x12, 0x12, 0x13, 0x7c},   // ¡ 0xc1 193
												{0x00, 0xf0, 0x2a, 0x29, 0x2a, 0xf0},   // ¬ 0xc2 194
												{0x00, 0xf0, 0x2a, 0x29, 0x2a, 0xf1},   // √ 0xc3 195
												{0x00, 0x7c, 0x13, 0x12, 0x13, 0x7c},   // ƒ 0xc4 196
												{0x00, 0x40, 0x3c, 0x12, 0x12, 0x0c},   // ≈ 0xc5 197
												{0x00, 0x7c, 0x01, 0x7f, 0x49, 0x41},   // ∆ 0xc6 198
												{0x00, 0x0e, 0x11, 0xb1, 0xd1, 0x0a},   // « 0xc7 199
												{0x00, 0x7c, 0x55, 0x56, 0x54, 0x00},   // » 0xc8 200
												{0x00, 0x7c, 0x54, 0x56, 0x55, 0x00},   // … 0xc9 201
												{0x00, 0x7f, 0x49, 0x49, 0x49, 0x00},   //   0xca 202
												{0x00, 0x7c, 0x55, 0x54, 0x55, 0x00},   // À 0xcb 203
												{0x00, 0x00, 0x41, 0x7f, 0x48, 0x00},   // Ã 0xcc 204
												{0x00, 0x00, 0x48, 0x7a, 0x49, 0x00},   // Õ 0xcd 205
												{0x00, 0x00, 0x4a, 0x79, 0x4a, 0x00},   // Œ 0xce 206
												{0x00, 0x00, 0x45, 0x7c, 0x45, 0x00},   // œ 0xcf 207
												{0x00, 0x08, 0x7f, 0x49, 0x41, 0x3e},   // – 0xd0 208
												{0x00, 0x78, 0x0a, 0x11, 0x22, 0x79},   // — 0xd1 209
												{0x00, 0x38, 0x45, 0x46, 0x44, 0x38},   // “ 0xd2 210
												{0x00, 0x38, 0x44, 0x46, 0x45, 0x38},   // ” 0xd3 211
												{0x00, 0x30, 0x4a, 0x49, 0x4a, 0x30},   // ‘ 0xd4 212
												{0x00, 0x30, 0x4a, 0x49, 0x41, 0x31},   // ’ 0xd5 213
												{0x00, 0x38, 0x45, 0x44, 0x45, 0x38},   // ÷ 0xd6 214
												{0x00, 0x00, 0x14, 0x08, 0x14, 0x00},   // ◊ 0xd7 215
												{0x00, 0x3e, 0x51, 0x49, 0x44, 0x3e},   // ÿ 0xd8 216
												{0x00, 0x3c, 0x41, 0x42, 0x40, 0x3c},   // Ÿ 0xd9 217
												{0x00, 0x3c, 0x40, 0x42, 0x41, 0x3c},   // ⁄ 0xda 218
												{0x00, 0x3f, 0x40, 0x40, 0x40, 0x3f},   // € 0xdb 219
												{0x00, 0x3c, 0x41, 0x40, 0x41, 0x3c},   // ‹ 0xdc 220
												{0x00, 0x0c, 0x10, 0x62, 0x11, 0x0c},   // › 0xdd 221
												{0x00, 0x7f, 0x22, 0x22, 0x22, 0x1c},   // ﬁ 0xde 222
												{0x00, 0x7e, 0x21, 0x2d, 0x2d, 0x12},   // ﬂ 0xdf 223
												{0x00, 0x40, 0xa9, 0xaa, 0xa8, 0xf0},   // ‡ 0xe0 224
												{0x00, 0x40, 0xa8, 0xaa, 0xa9, 0xf0},   // · 0xe1 225
												{0x00, 0x40, 0xaa, 0xa9, 0xaa, 0xf0},   // ‚ 0xe2 226
												{0x00, 0x40, 0xaa, 0xa9, 0xaa, 0xf1},   // „ 0xe3 227
												{0x00, 0x20, 0x55, 0x54, 0x55, 0x78},   // ‰ 0xe4 228
												{0x00, 0x50, 0x55, 0x55, 0x54, 0x78},   // Â 0xe5 229
												{0x00, 0x40, 0x5e, 0x45, 0x5e, 0x40},   // Ê 0xe6 230
												{0x00, 0x0e, 0x91, 0xb1, 0x51, 0x08},   // Á 0xe7 231
												{0x00, 0x38, 0x55, 0x56, 0x54, 0x18},   // Ë 0xe8 232
												{0x00, 0x38, 0x54, 0x56, 0x55, 0x18},   // È 0xe9 233
												{0x00, 0x70, 0xaa, 0xa9, 0xaa, 0x30},   // Í 0xea 234
												{0x00, 0x38, 0x55, 0x54, 0x55, 0x18},   // Î 0xeb 235
												{0x00, 0x00, 0x44, 0x7d, 0x42, 0x00},   // Ï 0xec 236
												{0x00, 0x00, 0x48, 0x7a, 0x41, 0x00},   // Ì 0xed 237
												{0x00, 0x00, 0x4a, 0x79, 0x42, 0x00},   // Ó 0xee 238
												{0x00, 0x00, 0x44, 0x7d, 0x40, 0x00},   // Ô 0xef 239
												{0x00, 0x10, 0x3e, 0x7e, 0x3e, 0x10},   //  0xf0 240
												{0x00, 0x55, 0x2a, 0x55, 0x2a, 0x55},   // Ò 0xf1 241
												{0x00, 0x30, 0x49, 0x4a, 0x48, 0x30},   // Ú 0xf2 242
												{0x00, 0x30, 0x48, 0x4a, 0x49, 0x30},   // Û 0xf3 243
												{0x00, 0x30, 0x4a, 0x49, 0x4a, 0x30},   // Ù 0xf4 244
												{0x00, 0x38, 0x45, 0x44, 0x45, 0x38},   // ı 0xf5 245
												{0x00, 0x38, 0x45, 0x44, 0x45, 0x38},   // ˆ 0xf6 246
												{0x00, 0x3c, 0x41, 0x40, 0x41, 0x3c},   // ˜ 0xf7 247
												{0x00, 0x38, 0x44, 0x44, 0x44, 0x38},   // ¯ 0xf8 248
												{0x00, 0x3c, 0x41, 0x42, 0x20, 0x7c},   // ˘ 0xf9 249
												{0x00, 0x3c, 0x40, 0x42, 0x21, 0x7c},   // ˙ 0xfa 250
												{0x00, 0x38, 0x42, 0x41, 0x22, 0x7c},   // ˚ 0xfb 251
												{0x00, 0x3c, 0x41, 0x40, 0x21, 0x7c},   // ¸ 0xfc 252
												{0x00, 0x0c, 0x50, 0x52, 0x50, 0x3c},   // ˝ 0xfd 253
												{0x00, 0x7c, 0x28, 0x28, 0x10, 0x00},   // ˛ 0xfe 254
												{0x00, 0x0c, 0x51, 0x50, 0x51, 0x3c}    // ˇ 0xff 255
                                              };

    #endregion

    #region AdvancedSetup form

    public class AdvancedSetupForm : Form
    {
      /// <summary>
      /// Required designer variable.
      /// </summary>

      private readonly IContainer components = null;
      private MPGroupBox groupBox1;
      private MPLabel label1;
      private MPComboBox cmbType;
      //          private MPButton btnTestDisplay;    // function currently disabled
      private MPCheckBox ckForceDiskUseOnly;
      private MPCheckBox mpVolumeDisplay;
      private MPCheckBox mpProgressBar;
      private MPButton btnOK;
      private MPCheckBox mpUseCustomFont;
      private MPCheckBox mpUseLargeIcons;
      private MPCheckBox mpUseCustomIcons;
      private MPCheckBox mpUseInvertedIcons;

      public AdvancedSetupForm()
      {
        InitializeComponent();

        cmbType.SelectedIndex = 0;
        cmbType.DataBindings.Add("SelectedItem", AdvancedSettings.Instance, "iMONLCDg_DisplayType");
        mpVolumeDisplay.DataBindings.Add("Checked", AdvancedSettings.Instance, "iMONLCDg_VolumeDisplay");
        mpProgressBar.DataBindings.Add("Checked", AdvancedSettings.Instance, "iMONLCDg_ProgressDisplay");
        ckForceDiskUseOnly.DataBindings.Add("Checked", AdvancedSettings.Instance, "iMONLCDg_DiskOnlyDisplay");
        mpUseCustomFont.DataBindings.Add("Checked", AdvancedSettings.Instance, "iMONLCDg_UseCustomFont");
        mpUseLargeIcons.DataBindings.Add("Checked", AdvancedSettings.Instance, "iMONLCDg_UseLargeIcons");
        mpUseCustomIcons.DataBindings.Add("Checked", AdvancedSettings.Instance, "iMONLCDg_UseCustomIcons");
        mpUseInvertedIcons.DataBindings.Add("Checked", AdvancedSettings.Instance, "iMONLCDg_UseInvertedIcons");
      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
        if (disposing && (components != null))
        {
          components.Dispose();
        }
        base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
        this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.mpVolumeDisplay = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpProgressBar = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpUseCustomFont = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpUseLargeIcons = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpUseCustomIcons = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.mpUseInvertedIcons = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.ckForceDiskUseOnly = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.cmbType = new MediaPortal.UserInterface.Controls.MPComboBox();
#if TESTIMONLCDG
        this.btnTestDisplay = new MediaPortal.UserInterface.Controls.MPButton();
#endif
        this.btnOK = new MediaPortal.UserInterface.Controls.MPButton();
        this.groupBox1.SuspendLayout();
        this.SuspendLayout();
        // 
        // groupBox1
        // 
        this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox1.Controls.Add(this.mpVolumeDisplay);
        this.groupBox1.Controls.Add(this.mpProgressBar);
        this.groupBox1.Controls.Add(this.mpUseCustomFont);
        this.groupBox1.Controls.Add(this.mpUseLargeIcons);
        this.groupBox1.Controls.Add(this.mpUseCustomIcons);
        this.groupBox1.Controls.Add(this.mpUseInvertedIcons);
        this.groupBox1.Controls.Add(this.ckForceDiskUseOnly);
        this.groupBox1.Controls.Add(this.label1);
        this.groupBox1.Controls.Add(this.cmbType);
        //        this.groupBox1.Controls.Add(this.btnTestDisplay);
        this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox1.Location = new System.Drawing.Point(9, 6);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(236, 239);
        this.groupBox1.TabIndex = 4;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Configuration";
        // 
        // mpVolumeDisplay
        // 
        this.mpVolumeDisplay.AutoSize = true;
        this.mpVolumeDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpVolumeDisplay.Location = new System.Drawing.Point(16, 65);
        this.mpVolumeDisplay.Name = "mpVolumeDisplay";
        this.mpVolumeDisplay.Size = new System.Drawing.Size(171, 17);
        this.mpVolumeDisplay.TabIndex = 75;
        this.mpVolumeDisplay.Text = "Use Top Bar as Volume display";
        this.mpVolumeDisplay.UseVisualStyleBackColor = true;
        // 
        // mpProgressBar
        // 
        this.mpProgressBar.AutoSize = true;
        this.mpProgressBar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpProgressBar.Location = new System.Drawing.Point(16, 88);
        this.mpProgressBar.Name = "mpProgressBar";
        this.mpProgressBar.Size = new System.Drawing.Size(193, 17);
        this.mpProgressBar.TabIndex = 74;
        this.mpProgressBar.Text = "Use Bottom Bar as Progress Display";
        this.mpProgressBar.UseVisualStyleBackColor = true;
        // 
        // mpUseCustomFont
        // 
        this.mpUseCustomFont.AutoSize = true;
        this.mpUseCustomFont.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpUseCustomFont.Location = new System.Drawing.Point(16, 111);
        this.mpUseCustomFont.Name = "mpUseCustomFont";
        this.mpUseCustomFont.Size = new System.Drawing.Size(193, 17);
        this.mpUseCustomFont.TabIndex = 77;
        this.mpUseCustomFont.Text = "Use Custom Font";
        this.mpUseCustomFont.UseVisualStyleBackColor = true;
        // 
        // mpUseLargeIcons
        // 
        this.mpUseLargeIcons.AutoSize = true;
        this.mpUseLargeIcons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpUseLargeIcons.Location = new System.Drawing.Point(16, 134);
        this.mpUseLargeIcons.Name = "mpUseLargeIcons";
        this.mpUseLargeIcons.Size = new System.Drawing.Size(193, 17);
        this.mpUseLargeIcons.TabIndex = 77;
        this.mpUseLargeIcons.Text = "Use Large Icons";
        this.mpUseLargeIcons.UseVisualStyleBackColor = true;
        // 
        // mpUseCustomIcons
        // 
        this.mpUseCustomIcons.AutoSize = true;
        this.mpUseCustomIcons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpUseCustomIcons.Location = new System.Drawing.Point(16, 157);
        this.mpUseCustomIcons.Name = "mpUseCustomIcons";
        this.mpUseCustomIcons.Size = new System.Drawing.Size(193, 17);
        this.mpUseCustomIcons.TabIndex = 77;
        this.mpUseCustomIcons.Text = "Use Custom Large Icons";
        this.mpUseCustomIcons.UseVisualStyleBackColor = true;
        // 
        // mpUseInvertedIcons
        // 
        this.mpUseInvertedIcons.AutoSize = true;
        this.mpUseInvertedIcons.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.mpUseInvertedIcons.Location = new System.Drawing.Point(16, 180);
        this.mpUseInvertedIcons.Name = "mpUseCustomIcons";
        this.mpUseInvertedIcons.Size = new System.Drawing.Size(193, 17);
        this.mpUseInvertedIcons.TabIndex = 77;
        this.mpUseInvertedIcons.Text = "Invert (reverse) the Large Icons";
        this.mpUseInvertedIcons.UseVisualStyleBackColor = true;
        // 
        // ckForceDiskUseOnly
        // 
        this.ckForceDiskUseOnly.AutoSize = true;
        this.ckForceDiskUseOnly.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.ckForceDiskUseOnly.Location = new System.Drawing.Point(16, 42);
        this.ckForceDiskUseOnly.Name = "ckForceDiskUseOnly";
        this.ckForceDiskUseOnly.Size = new System.Drawing.Size(188, 17);
        this.ckForceDiskUseOnly.TabIndex = 73;
        this.ckForceDiskUseOnly.Text = "Use Disk Icon as for CD/DVD only";
        this.ckForceDiskUseOnly.UseVisualStyleBackColor = true;
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(8, 16);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(73, 23);
        this.label1.TabIndex = 11;
        this.label1.Text = "Display Type";
        this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // cmbType
        // 
        this.cmbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbType.BorderColor = System.Drawing.Color.Empty;
        this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbType.Location = new System.Drawing.Point(87, 16);
        this.cmbType.Name = "cmbType";
        this.cmbType.Size = new System.Drawing.Size(122, 21);
        this.cmbType.Sorted = true;
        this.cmbType.TabIndex = 10;
        this.cmbType.Items.AddRange(new object[] {
            "AutoDetect",
            "LCD",
            "VFD"});
#if TESTIMONLCDG
                // 
                // btnTestDisplay
                // 
                this.btnTestDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
                this.btnTestDisplay.Location = new System.Drawing.Point(142, 211);
                this.btnTestDisplay.Name = "btnTestDisplay";
                this.btnTestDisplay.Size = new System.Drawing.Size(88, 23);
                this.btnTestDisplay.TabIndex = 70;
                this.btnTestDisplay.Text = "&Test Display";
                this.btnTestDisplay.UseVisualStyleBackColor = true;
                this.btnTestDisplay.Click += new System.EventHandler(this.btnTestDisplay_Click);
#endif
        // 
        // btnOK
        // 
        this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.btnOK.Location = new System.Drawing.Point(167, 251);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(78, 23);
        this.btnOK.TabIndex = 6;
        this.btnOK.Text = "&OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(257, 286);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.groupBox1);
        this.Name = "Form1";
        this.Text = "Advanced Settings";
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        this.ResumeLayout(false);

      }

      #endregion

      #region UI Buttons

#if TESTIMONLCDG
      private void btnTestDisplay_Click(object sender, EventArgs e)
      {
          Log.Debug("IDisplay iMONLCDg.AdvancedSetupForm.btnTestDisplay() clicked");
      }
#endif
      private void btnOK_Click(object sender, EventArgs e)
      {
        Log.Debug("IDisplay iMONLCDg.AdvancedSetupForm.btnOK_Click() started");
        AdvancedSettings.Save();
        Hide();
        Close();
        Log.Debug("IDisplay iMONLCDg.AdvancedSetupForm.btnOK_Click() Completed");
      }

      #endregion

    }
    #endregion


    #region Advanced Settings class
    [Serializable]
    public class AdvancedSettings
    {
      #region Static Fields

      private static AdvancedSettings m_Instance; //Reference to the single instance of this task

      #endregion

      #region Fields

      /// <summary>
      /// String representation of the selected display type.  Used for (de)serializing
      /// </summary>
      private string m_DisplayType = null;
      private bool m_DiskOnlyDisplay = false;
      private bool m_VolumeDisplay = false;
      private bool m_ProgressDisplay = false;
      private bool m_UseCustomFont = false;
      private bool m_UseLargeIcons = false;
      private bool m_UseCustomIcons = false;
      private bool m_UseInvertedIcons = false;

      #endregion

      #region Properties

      /// <summary>
      /// Gets the single instance
      /// </summary>
      /// <value>The single instance of this class</value>
      public static AdvancedSettings Instance
      {
        get
        {
          if (m_Instance == null)
          {
            m_Instance = Load();
          }
          return m_Instance;
        }
      }

      [XmlAttribute]
      public string iMONLCDg_DisplayType //Has to be a property, in order to be bindable to a control
      {
        get { return m_DisplayType; }
        set { m_DisplayType = value; }
      }

      [XmlAttribute]
      public bool iMONLCDg_DiskOnlyDisplay
      {
        get { return m_DiskOnlyDisplay; }
        set { m_DiskOnlyDisplay = value; }
      }

      [XmlAttribute]
      public bool iMONLCDg_VolumeDisplay
      {
        get { return m_VolumeDisplay; }
        set { m_VolumeDisplay = value; }
      }

      [XmlAttribute]
      public bool iMONLCDg_ProgressDisplay
      {
        get { return m_ProgressDisplay; }
        set { m_ProgressDisplay = value; }
      }

      [XmlAttribute]
      public bool iMONLCDg_UseCustomFont
      {
        get { return m_UseCustomFont; }
        set { m_UseCustomFont = value; }
      }

      [XmlAttribute]
      public bool iMONLCDg_UseLargeIcons
      {
        get { return m_UseLargeIcons; }
        set { m_UseLargeIcons = value; }
      }

      [XmlAttribute]
      public bool iMONLCDg_UseCustomIcons
      {
        get { return m_UseCustomIcons; }
        set { m_UseCustomIcons = value; }
      }

      [XmlAttribute]
      public bool iMONLCDg_UseInvertedIcons
      {
        get { return m_UseInvertedIcons; }
        set { m_UseInvertedIcons = value; }
      }

      #endregion

      #region Functions
      /// <summary>
      /// Loads the settings from XML
      /// </summary>
      /// <returns>The loaded settings</returns>
      public static AdvancedSettings Load()
      {
        Log.Debug("IDisplay iMONLCDg.AdvancedSettings.Load() started");
        AdvancedSettings Settings;
        if (File.Exists(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg.xml")))
        {
          Log.Debug("IDisplay iMONLCDg.AdvancedSettings.Load() Loading settings from XML file");
          XmlSerializer ser = new XmlSerializer(typeof(AdvancedSettings));
          XmlTextReader rdr = new XmlTextReader(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg.xml"));
          Settings = (AdvancedSettings)ser.Deserialize(rdr);
          rdr.Close();
        }
        else
        {
          Log.Debug("IDisplay iMONLCDg.AdvancedSettings.Load() Loading settings from defaults");
          Settings = new AdvancedSettings();
          Default(Settings);
        }
        Log.Debug("IDisplay iMONLCDg.AdvancedSettings.Load() completed");
        return Settings;
      }

      /// <summary>
      /// Saves the settings to XML
      /// </summary>
      public static void Save()
      {
        Log.Debug("IDisplay iMONLCDg.AdvancedSettings.Save() Saving settings to XML file");
        XmlSerializer ser = new XmlSerializer(typeof(AdvancedSettings));
        XmlTextWriter w = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "ExternalDisplay_imonlcdg.xml"), Encoding.UTF8);
        w.Formatting = Formatting.Indented;
        w.Indentation = 2;
        ser.Serialize(w, Instance);
        w.Close();
        Log.Debug("IDisplay iMONLCDg.AdvancedSettings.Save() completed");
      }

      /// <summary>
      /// Creates the default settings when config file cannot be found
      /// </summary>
      /// <param name="_settings"></param>
      private static void Default(AdvancedSettings _settings)
      {
        _settings.iMONLCDg_DisplayType = null;
        _settings.iMONLCDg_DiskOnlyDisplay = false;
        _settings.iMONLCDg_ProgressDisplay = false;
        _settings.iMONLCDg_VolumeDisplay = false;
        _settings.iMONLCDg_UseLargeIcons = false;
        _settings.iMONLCDg_UseCustomIcons = false;
        _settings.iMONLCDg_UseInvertedIcons = false;
      }
      #endregion
    }
    #endregion


    #region Interop declarations SG_VFD.dll

    [DllImport("SG_VFDv5.dll", EntryPoint = "iMONVFD_Init")]
    private static extern bool Open(int vfdType, int resevered);

    [DllImport("SG_VFDv5.dll", EntryPoint = "iMONVFD_Uninit")]
    private static extern void Close();

    [DllImport("SG_VFDv5.dll", EntryPoint = "iMONVFD_IsInited")]
    private static extern bool IsOpen();

    [DllImport("SG_VFD.dll", EntryPoint = "iMONVFD_SetText")]         // VFD specific
    private static extern bool iMONVFD_SetText(string firstLine, string secondLine);

    [DllImport("SG_VFDv5.dll", EntryPoint = "iMONVFD_SetEQ")]
    public static extern bool SetEQ(int arEQValue);

    [DllImport("SG_VFDv5.dll", EntryPoint = "iMONLCD_SendData")]        // LCD specific
    public static extern bool iMONLCD_SendData(ref ulong bitMap);
    //    public static extern unsafe bool iMONLCD_SendData(Int64* bitMap);

    #endregion

    #region Interop declarations SG_RC.dll

    // verified funtion calls

    /// <summary>
    /// Initialize the Remote Control Hardware interface
    /// </summary>
    /// <param name="rcSet">??</param>
    /// <param name="rcType">RC Controller Type?. hardcoded to 0x83 in the SG Software</param>
    /// <param name="rc_reserved">Reserved configuration bits. Hardcoded to 0x8888 in the SG Software</param>
    /// <returns>Boolean - Initialization status</returns>
    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_Init")]
    private static extern bool RC_Init(int rcSet, int rcType, int rc_reserved);

    /// <summary>
    /// Closes the Remote Control Hardware interface
    /// </summary>
    /// <returns>no return value</returns>
    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_Uninit")]
    private static extern void RC_Uninit();

    /// <summary>
    /// Determine the Initialization status of the Remote Control Hardware interface
    /// </summary>
    /// <returns>Boolean - Initialization status</returns>
    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_IsInited")]
    private static extern bool RC_IsInited();

    /// <summary>
    /// Determine the Remote Control Hardware type
    /// </summary>
    /// <returns>Integer - Remote Control Hardware Type value</returns>
    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_GetHWType")]
    private static extern int RC_GetHWType();
    // HWVer = 0x02  Silverstone LCD, 0x05  Antec LCD

    /// <summary>
    /// Determine the Firmware Version of the Remote Control Hardware
    /// </summary>
    /// <returns>Integer - Remote Control Hardware Firmware Version</returns>
    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_GetFirmwareVer")]
    private static extern int RC_GetFirmwareVer();
    // FWVer = 0x98  Silverstone, 0xA0  Antec LCD, 0xA1  Antec LCD

    /// <summary>
    /// Change the Remote Control set used by the Remote Control Hardware
    /// </summary>
    /// <param name="rcSet">RC Controller Set.</param>
    /// <returns>Boolean - rcSet change status</returns>
    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_ChangeiMONRCSet")]
    private static extern bool RC_ChangeRCSet(int rcSet);
    // rcSet    = 0x65  iMON_RC,    0x66  iMON_RSC,         0x6B  iMON_MM,      0x70  iMON_EX
    //            0x73  iMON_PAD,   0x67  laser_pointer,    0x68  Anycall,      0x69  TG_iMON
    //            0x6A  DIGN_iMON,  0x6C  LG,               0x6D  LLUON_iMON    0x6E  KLOSS_iMON
    //            0x6F  iMON_WT,    0x71  TG_Advent,        0x72  Philips,      0x74  iMON_2_4G
    //            0x75  Enlight,    0x76  AutoCAN,          0x77  MCE_Remote,   0x78  AutoCAN_II

    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_CheckDriverVersion")]
    private static extern int RC_CheckDriverVersion();

    /// <summary>
    /// Change the RC6 format used by the Remote Control Hardware
    /// </summary>
    /// <param name="rcMode">RC6 Protocol mode to use - Hardcoded to 1 for Antec hardware</param>
    /// <returns>Boolean - Initialization status</returns>
    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_ChangeRC6")]
    private static extern bool RC_ChangeRC6(int rcMode);


    // unverified functions

    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_GetDeletedPacket")]
    //      private static extern bool RC_GetDeletedPacket(int rcSet);

    /// <summary>
    /// Get the RC6 mode of the last RC6 Packet received
    /// only valid for HWType = 0x03 or 0x04
    /// </summary>
    /// <returns>Boolean - conversion status, name of button is in the packet buffer</returns>
    [DllImport("../../SG_RC.dll", EntryPoint = "iMONRC_GetLastRFMode")]
    private static extern int RC_GetLastFRMode();

    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_GetMoreStickPacket")]    // used for iMON PAD controller
    //      private static extern bool RC_GetMoreStickPacket(int rcSet);

    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_GetPacket")]
    //      private static extern bool RC_GetPacket(int* rcBuffer, int rcBufferSize);   // packet buffer = 212 bytes??

    /// <summary>
    /// Derive the name of a Remote button from the received packet
    /// </summary>
    /// <param name="rcBuffer">pointer to an packet buffer</param>
    /// <param name="rcBufferSize">size of the packet buffer</param>
    /// <returns>Boolean - conversion status, name of button is in the packet buffer</returns>
    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_GetRCBtnName")]
    //      private static extern bool RC_GetRCBtnName(int* rcBuffer, int rcBufferSize);  // buffer for received packet

    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_InitPlugin")]
    //      private static extern bool RC_InitPlugin(int rcSet);

    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_MakeGirderPacket")]
    //      private static extern bool RC_MakeGirderPacket(int* rcBuffer);

    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_SetRFID")]
    //      private static extern bool RC_SetRFID(int rcSet);

    /// <summary>
    /// Set maximum response time for a received command
    /// only valid for HWType = 0x03 or 0x04
    /// </summary>
    /// <param name="rcResponseTime">Response time (in milliseconds?)</param>
    /// <returns>Boolean - conversion status, name of button is in the packet buffer</returns>
    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_SetRFResponseTime")]
    //      private static extern bool RC_SetRFResponseTime(int rcResponseTime);

    //      [DllImport("SG_RC.dll", EntryPoint = "iMONRC_StickPacketCalib")]      // used for iMON PAD controller
    //      private static extern bool RC_StickPacketCalib(int rcSet);

    #endregion

    #region Unused Methods

    //private void SetEQ(byte[] EqDataArray)      // not currently used - included for complete coverage of SG_VFD.dll functions
    //{
    //  // 16 sized array for single bar.
    //  // EqDataArray[0] 	0 -> up bars
    //  // 					1 -> down bars from top
    //  // 					2 -> expand from middle
    //  //					7 -> from top and bottom (maybe another code also works?
    //  //					are there other options??
    //  // EqDataArray[i] = length of bar

    //  lock (DWriteMutex)
    //  {
    //    int DataControl = 0x40;

    //    for (int k = 0; k <= 3 * 7; k += 7)
    //    {
    //      Int64 EqData = DataControl;
    //      for (int i = 6; i >= 0; i--)
    //      {
    //        EqData <<= 8;
    //        if ((k + i) < EqDataArray.Length)
    //        {
    //          EqData += EqDataArray[k + i];
    //        }
    //      }
    //      if (DataControl <= 0x42)
    //      {
    //        SendData(EqData);
    //      }
    //      DataControl++;
    //    }
    //  }
    //}

    #endregion

  }
}
