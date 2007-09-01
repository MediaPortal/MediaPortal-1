#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
 * 
 *  Modified iMONLCD.cs Aug 8, 2007 by RalphY
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

//TODO Known problems
// should check the input pixel dimensions in the setup form for validity 
// IsOpen() behaviour is odd - Even if iMon should be closed, IsOpen returns true in the setup application
//  is it being opened by iMON.cs or iMONLCD, when VerifyLCD is called?
// 8x5 font bitmap may have some incorrect bitmaps
// Set LCD icons, use Graphic Eq
// check that input pixel array size is valid

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using MediaPortal.GUI.Library;

//using System.Globalization;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// SoundGraph iMONLCD USB Driver for Soundgraph OEM LCD and UltraBay
  /// With Graphics support
  /// <author>Ralph Youie</author>
  /// </summary>
  public class iMONLCDg : BaseDisplay, IDisplay
  {
    private const Int32 _VfdType = 0x18; // iMon version 5 initialised with 0x16
    // iMon version 6 uses 0x18 - no idea why the difference
    private const Int32 _reserved = 0x00008888; // both versions send this long int 
    private readonly string[] _lines = new string[2];
    private readonly bool _isDisabled = false;
    private readonly string _errorMessage = "";
    private readonly SHA256Managed sha256 = new SHA256Managed(); //instance of crypto engine used to calculate hashes
    private byte[] lastHash; //hash of the last bitmap that was sent to the display

    private bool _Backlight = false;
    private Int64 _Contrast = 0x0A;

    private int _grows = 16;
    private int _gcols = 96;
    private int _delay = 0; // milliseconds of delay between sending each data word to iMON Text mode
    private int _delayG = 0; // milliseconds of delay between sending each data word to iMON Graphics
    // really nothing distinguishing text or graphics.
    private byte[] bitmapData;


    public iMONLCDg()
    {
      try
      {
        Log.Debug("ExternalDisplay.iMONLCDg .ctr() called");
        if (IsOpen())
        {
          Log.Debug("ExternalDisplay.iMONLCDg .ctr(): Already open, Close Called");
          Close();
        }
        if (!Open(_VfdType, _reserved))
        {
          Log.Error("ExternalDisplay.iMONLCDg .ctr(): Open failed");
          _isDisabled = true;
          _errorMessage = "Could not find an iMON LCDg display";
        }
        //      
        //      	if (!IsOpen())
        //      	{
        //      		Log.Debug("ExternalDisplay.iMONLCDg .ctr(): LCD not open");
        //      		if	(!Open(VfdType, reserved))
        //  	        {
        //	          Log.Error("ExternalDisplay.iMONLCDg .ctr(): Open failed");
        //      		  isDisabled = true;
        //	          errorMessage = "Could not find an iMON LCDg display";
        //	        }
        //      	}
        //      	else
        //      	{
        //      		Log.Info("ExternalDisplay.iMONLCD Constructor: Already open");
        //      	}
      }
      catch (Exception ex)
      {
        _isDisabled = true;
        _errorMessage = ex.Message;
      }
    }

    public bool IsDisabled
    {
      get { return _isDisabled; }
    }

    public string ErrorMessage
    {
      get { return _errorMessage; }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    { }

    public void DrawImage(Bitmap bitmap)
    {
      Log.Debug("iMONLCDg.DrawImage");
      if (bitmap == null)
      {
        Log.Debug("iMONLCDg.DrawImage:  bitmap null");
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
      byte[] hash = sha256.ComputeHash(bitmapData);
      //Compare the new hash with the previous one to determine whether the new image is
      //equal to the one that is already shown.  If they are equal, then we are done
      if (ByteArray.AreEqual(hash, lastHash))
      {
        Log.Debug("iMONLCDg.DrawImage:  bitmap not changed");
        return;
      }

      //gcols=96; grows=16;
      byte[] PixelArray = new byte[_gcols * 2];
      for (int i = 0; i < _gcols - 1; i++)
      {
        PixelArray[i] = 0; // line1
        PixelArray[i + _gcols] = 0; // line2
        for (int j = 0; j < 8; j++)
        {
          int pixel = j * data.Stride + i * 4;
          if (Color.FromArgb(bitmapData[pixel + 2],
                             bitmapData[pixel + 1],
                             bitmapData[pixel]).GetBrightness() < 0.5f)
          {
            PixelArray[i] = (byte)(PixelArray[i] | (byte)(1 << (7 - j)));
          }
        }

        for (int j = 8; j < 16; j++)
        {
          int pixel = j * data.Stride + i * 4;
          if (Color.FromArgb(bitmapData[pixel + 2],
                             bitmapData[pixel + 1],
                             bitmapData[pixel]).GetBrightness() < 0.5f)
          {
            PixelArray[i + _gcols] = (byte)(PixelArray[i + _gcols] | (byte)(1 << (15 - j)));
          }
        }
        //       Log.Info("PixelArray i {0}: {1}{2}",i, PixelArray[i].ToString("X2"),PixelArray[i+gcols/2].ToString("X2"));
      }
      SendPixelArray(PixelArray);
      Log.Debug("Sending pixel array to iMON Handler");
      lastHash = hash;
    }

    /// <summary>
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="line">The line to thow the message on.</param>
    /// <param name="message">The message to show.</param>
    public void SetLine(int line, string message)
    {
      _lines[line] = message;
      if (line == 1)
      {
        DisplayLines();
      }
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
      get { return "SoundGraph iMON LCD USB Driver V1.1"; }
    }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    /// <summary>
    /// Does this display support graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get { return true; }
    }

    /// <summary>
    /// Shows the advanced configuration screen
    /// </summary>
    public void Configure()
    {
      //No advanced configuration needed
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
      Log.Debug("iMONLCDg Setup called");

      _Contrast = ((Int64)contrast >> 2);
      _Backlight = backLight;

      _grows = linesG; // TODO should hard code to 16 or test the setup form has valid input
      _gcols = colsG; // should hard code to 96	
      _delay = delay;
      _delayG = timeG;
      _delay = Math.Max(_delay, _delayG);
      OpenLcd();
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Initialize()
    {
      Log.Debug("iMONLCDg Initialize called");
      Clear();
    }

    public void CleanUp()
    {
      Log.Debug("iMONLCDg Cleanup called");
      Clear();
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Clear()
    {
      Log.Debug("iMONLCDg Clear called");
      ClearPixels();
      for (int i = 0; i < 2; i++)
      {
        _lines[i] = new string(' ', Settings.Instance.TextWidth);
      }
      //      DisplayLines();
    }

    /// <summary>
    /// Cleanup/Dispose
    /// </summary>
    public void Dispose()
    {
      Log.Debug("iMONLCDg Dispose called");
      CloseLcd();
    }

    /// <summary>
    /// Opens the display driver
    /// </summary>
    private void OpenLcd()
    {
      Log.Info("ExternalDisplay.iMONLCDg.OpenLCD called");
      if (!IsOpen())
      {
        Log.Debug("ExternalDisplay.iMONLCDg.OpenLCD: LCD not open");
        if (!Open(_VfdType, _reserved))
        {
          Log.Error("ExternalDisplay.iMONLCDg.OpenLCD: Could not open display");
        }
      }
      else
      {
        Log.Debug("ExternalDisplay.iMONLCDg.OpenLCD: LCD already open");
      }
      SendData(Command.DisplayOn);             // turn the display on
      SendData(Command.ClearAlarm);             // clear the alarm
      SendData((long)Command.SetContrast | _Contrast); // set contrast
      ClearDisplay();
      ClearPixels();
      Log.Info("iMON LCDg Started");
    }

    /// <summary>
    /// Closes the display driver
    /// </summary>
    private void CloseLcd()
    {
      if (IsOpen())
      {
        Log.Info("iMON LCD Close called");
        SendData(0x1000000000000000); // remove top and bottom lines
        SendData(0x1100000000000000);
        SendData(0x1200000000000000);
        if (_Backlight)
        {
          // shut down the display
          SendData(Command.Shutdown);
        }
        else
        {
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
        Close();
      }
    }

    /// <summary>
    /// Sends the text to the display
    /// </summary>
    private void DisplayLines()
    {
      //SetText(lines[0], lines[1]);
      SendText(_lines[0], _lines[1]);
    }

    #region  iMON LCD Specific methods

    private unsafe void SendData(Int64 data)
    {
      iMONLCD_SendData(&data);
      Thread.Sleep(_delay);
    }

    private unsafe void SendData(Command command)
    {
      SendData((long)command);
    }

    private void SendText(string Line1, string Line2)
    {
      int k = 0;
      byte[] pixel = new byte[192];
      for (int i = 0; i < Math.Min(16, Line1.Length); i++)
      {
        char ch = Line1[i];
        int j;
        for (j = 5; j >= 0; j--)
        {
          pixel[k + j] = BitReverse(Font8x5[ch, j]);
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
          pixel[k + j] = BitReverse(Font8x5[ch, j]);
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
        Log.Error("ERROR in iMONLCD SendPixelArray");
      }

      int DataControl = 0x20;

      lock (this) // must send all the data to LCD without being interrupted.?
      // if scrolling too quickly, calls may get banked up and grind to a halt???
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
    /// Displays the lines at the top and bottom, based on a bit map input
    /// </summary>
    /// <returns></returns>	
    private void SetLinePixels(UInt32 TopLine, UInt32 BotLine,
                               UInt32 TopProgress, UInt32 BotProgress)
    {
      Int64 Data;
      //Least sig. bit is on the right

      Data = ((Int64)TopProgress) << 8 * 4;
      Data += TopLine;
      Data &= 0x00FFFFFFFFFFFFFF;
      Data += 0x1000000000000000;
      SendData(Data);

      Data = ((Int64)TopProgress) >> 8 * 3;
      Data += ((Int64)BotProgress) << 8;
      Data += ((Int64)BotLine) << 8 * 5;
      Data &= 0x00FFFFFFFFFFFFFF;
      Data += 0x1100000000000000;
      SendData(Data);

      Data = ((Int64)BotLine) >> 8 * 2;
      Data += 0x1200000000000000;
      SendData(Data);
    }

    /// <summary>
    /// Displays the lines at the top and bottom, based on a line length
    /// </summary>
    /// <description
    /// Positive length display bar from the left, negative length displays from the right
    /// No input checking; data should range from -32 to + 32
    /// </description>
    /// <returns></returns>	
    private void SetLineLength(int TopLine, int BotLine,
                               int TopProgress, int BotProgress)
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

    private void SetEQ(byte[] EqDataArray)
    {
      // 16 sized array for single bar.
      // EqDataArray[0] 	0 -> up bars
      // 					1 -> down bars from top
      // 					2 -> expand from middle
      //					7 -> from top and bottom (maybe another code also works?
      //					are there other options??
      // EqDataArray[i] = length of bar

      int DataControl = 0x40;

      for (int k = 0; k <= 3 * 7; k += 7)
      {
        Int64 EqData = DataControl;
        for (int i = 6; i >= 0; i--)
        {
          EqData <<= 8;
          if ((k + i) < EqDataArray.Length)
          {
            EqData += EqDataArray[k + i];
          }
        }
        if (DataControl <= 0x42)
        {
          SendData(EqData);
        }
        DataControl++;
      }
    }

    /// <summary>
    /// Clear Display
    /// Send the same intialisation sequence debugged from an iMon run
    /// </summary>
    /// <returns></returns>	
    private void ClearDisplay()
    {
      SendData(0x0200000000000000);
      SendData(0x0100000000000000);
      SendData(0x10ffffff00000000);
      SendData(0x110000ffffffffff);
      SendData(0x1200000000000000);
    }

    private void ClearPixels()
    {
      Console.WriteLine("Clear Pixels");
      for (Int64 i = 0x20; i <= 0x3b; i++)
      {
        Int64 pixels = i << 56;
        SendData(pixels);
      }
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

    private UInt32 BitReverse(UInt32 Word)
    {
      UInt32 v; // 32 bit word to reverse bit order
      v = Word;
      // swap odd and even bits
      v = ((v >> 1) & 0x55555555) | ((v & 0x55555555) << 1);
      // swap consecutive pairs
      v = ((v >> 2) & 0x33333333) | ((v & 0x33333333) << 2);
      // swap nibbles ... 
      v = ((v >> 4) & 0x0F0F0F0F) | ((v & 0x0F0F0F0F) << 4);
      // swap bytes
      v = ((v >> 8) & 0x00FF00FF) | ((v & 0x00FF00FF) << 8);
      // swap 2-byte long pairs
      v = (v >> 16) | (v << 16);

      return v;
    }

    #endregion

    #region Display Commands
    private enum Command : long
    {
      DisplayOn = 0x5000000000000040,
      ClearAlarm = 0x5100000000000000,
      SetContrast = 0x0300000000000000,
      Shutdown = 0x5000000000000008
    }

    #endregion

    #region Display Icons Class

    /// <summary>
    /// Icons Class
    /// </summary>
    /// <returns>Int 64 with the icon bit set</returns>	
    private class Icons
    {
      private Int64 icon = 0x0100000000000000;
      //Byte 6
      private readonly Int64 DiskOff = 0x0F00FFFFFFFFFFFF;
      private readonly Int64 DiskOn = 0x0080FF0000000000;

      // Byte 5
      private readonly int WMA_2bit = 40;
      private readonly int WAVbit = 39;
      private readonly int REPbit = 38;
      private readonly int SFLbit = 37;
      private readonly int Alarmbit = 36;
      private readonly int Recbit = 35;
      private readonly int Volbit = 34;
      private readonly int Timebit = 33;
      // Byte 4
      private readonly int xVidbit = 32;
      private readonly int WMVbit = 31;
      private readonly int MPG_2bit = 30;
      private readonly int AC3bit = 29;
      private readonly int DTSbit = 28;
      private readonly int WMAbit = 27;
      private readonly int MP3bit = 26;
      private readonly int OGGbit = 25;

      //Byte 3
      private readonly int SRCbit = 24;
      private readonly int FITbit = 23;
      private readonly int TV_2bit = 22;
      private readonly int HDTVbit = 21;
      private readonly int SCR1bit = 20;
      private readonly int SCR2bit = 19;
      private readonly int MPGbit = 18;
      private readonly int DivXbit = 17;
      // Byte 2
      private readonly int Cbit = 16;
      private readonly int Rbit = 15;
      private readonly int SLbit = 14;
      private readonly int LFEbit = 13;
      private readonly int SRbit = 12;
      private readonly int RLbit = 11;
      private readonly int SPDIFbit = 10;
      private readonly int RRbit = 9;
      // Byte 1
      private readonly int Musicbit = 8;
      private readonly int Moviebit = 7;
      private readonly int Photobit = 6;
      private readonly int CD_DVDbit = 5;
      private readonly int TVbit = 4;
      private readonly int WebCastbit = 3;
      private readonly int Newsbit = 2;
      private readonly int Lbit = 1;

      public Int64 ClearAll()
      {
        return 0x0100000000000000;
      }

      public Int64 REP(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, REPbit);
        }
        else
        {
          icon = BitClear(icon, REPbit);
        }
        return (icon);
      }

      public Int64 Alarm(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Alarmbit);
        }
        else
        {
          icon = BitClear(icon, Alarmbit);
        }
        return (icon);
      }

      public Int64 Rec(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Recbit);
        }
        else
        {
          icon = BitClear(icon, Recbit);
        }
        return (icon);
      }

      public Int64 Vol(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Volbit);
        }
        else
        {
          icon = BitClear(icon, Volbit);
        }
        return (icon);
      }

      public Int64 Time(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Timebit);
        }
        else
        {
          icon = BitClear(icon, Timebit);
        }
        return (icon);
      }

      public Int64 MPG(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, MPGbit);
        }
        else
        {
          icon = BitClear(icon, MPGbit);
        }
        return (icon);
      }

      public Int64 DivX(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, DivXbit);
        }
        else
        {
          icon = BitClear(icon, DivXbit);
        }
        return (icon);
      }

      public Int64 xVid(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, xVidbit);
        }
        else
        {
          icon = BitClear(icon, xVidbit);
        }
        return (icon);
      }

      public Int64 WMV(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, WMVbit);
        }
        else
        {
          icon = BitClear(icon, WMVbit);
        }
        return (icon);
      }

      public Int64 MPG_2(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, MPG_2bit);
        }
        else
        {
          icon = BitClear(icon, MPG_2bit);
        }
        return (icon);
      }

      public Int64 AC3(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, AC3bit);
        }
        else
        {
          icon = BitClear(icon, AC3bit);
        }
        return (icon);
      }

      public Int64 DTS(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, DTSbit);
        }
        else
        {
          icon = BitClear(icon, DTSbit);
        }
        return (icon);
      }

      public Int64 WMA(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, WMAbit);
        }
        else
        {
          icon = BitClear(icon, WMAbit);
        }
        return (icon);
      }

      public Int64 MP3(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, MP3bit);
        }
        else
        {
          icon = BitClear(icon, MP3bit);
        }
        return (icon);
      }

      public Int64 OGG(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, OGGbit);
        }
        else
        {
          icon = BitClear(icon, OGGbit);
        }
        return (icon);
      }

      public Int64 WMA_2(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, WMA_2bit);
        }
        else
        {
          icon = BitClear(icon, WMA_2bit);
        }
        return (icon);
      }

      public Int64 WAV(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, WAVbit);
        }
        else
        {
          icon = BitClear(icon, WAVbit);
        }
        return (icon);
      }

      public Int64 Music(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Musicbit);
        }
        else
        {
          icon = BitClear(icon, Musicbit);
        }
        return (icon);
      }

      public Int64 Movie(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Moviebit);
        }
        else
        {
          icon = BitClear(icon, Moviebit);
        }
        return (icon);
      }

      public Int64 Photo(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Photobit);
        }
        else
        {
          icon = BitClear(icon, Photobit);
        }
        return (icon);
      }

      public Int64 CD_DVD(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, CD_DVDbit);
        }
        else
        {
          icon = BitClear(icon, CD_DVDbit);
        }
        return (icon);
      }

      public Int64 TV(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, TVbit);
        }
        else
        {
          icon = BitClear(icon, TVbit);
        }
        return (icon);
      }

      public Int64 WebCast(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, WebCastbit);
        }
        else
        {
          icon = BitClear(icon, WebCastbit);
        }
        return (icon);
      }

      public Int64 News(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Newsbit);
        }
        else
        {
          icon = BitClear(icon, Newsbit);
        }
        return (icon);
      }

      public Int64 Centre(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Cbit);
        }
        else
        {
          icon = BitClear(icon, Cbit);
        }
        return (icon);
      }

      public Int64 LFE(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, LFEbit);
        }
        else
        {
          icon = BitClear(icon, LFEbit);
        }
        return (icon);
      }

      public Int64 Left(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Lbit);
        }
        else
        {
          icon = BitClear(icon, Lbit);
        }
        return (icon);
      }

      public Int64 Right(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, Rbit);
        }
        else
        {
          icon = BitClear(icon, Rbit);
        }
        return (icon);
      }

      public Int64 RL(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, RLbit);
        }
        else
        {
          icon = BitClear(icon, RLbit);
        }
        return (icon);
      }

      public Int64 RR(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, RRbit);
        }
        else
        {
          icon = BitClear(icon, RRbit);
        }
        return (icon);
      }

      public Int64 SCR1(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, SCR1bit);
        }
        else
        {
          icon = BitClear(icon, SCR1bit);
        }
        return (icon);
      }

      public Int64 SCR2(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, SCR2bit);
        }
        else
        {
          icon = BitClear(icon, SCR2bit);
        }
        return (icon);
      }

      public Int64 HDTV(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, HDTVbit);
        }
        else
        {
          icon = BitClear(icon, HDTVbit);
        }
        return (icon);
      }

      public Int64 SFL(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, SFLbit);
        }
        else
        {
          icon = BitClear(icon, SFLbit);
        }
        return (icon);
      }

      public Int64 SRC(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, SRCbit);
        }
        else
        {
          icon = BitClear(icon, SRCbit);
        }
        return (icon);
      }

      public Int64 FIT(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, FITbit);
        }
        else
        {
          icon = BitClear(icon, FITbit);
        }
        return (icon);
      }

      public Int64 TV_2(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, TV_2bit);
        }
        else
        {
          icon = BitClear(icon, TV_2bit);
        }
        return (icon);
      }

      public Int64 SL(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, SLbit);
        }
        else
        {
          icon = BitClear(icon, SLbit);
        }
        return (icon);
      }

      public Int64 SR(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, SRbit);
        }
        else
        {
          icon = BitClear(icon, SRbit);
        }
        return (icon);
      }

      public Int64 SPDIF(bool on)
      {
        if (on)
        {
          icon = BitSet(icon, SPDIFbit);
        }
        else
        {
          icon = BitClear(icon, SPDIFbit);
        }
        return (icon);
      }

      public Int64 Disk(bool on)
      {
        if (on)
        {
          icon |= DiskOn;
        }
        else
        {
          icon &= DiskOff;
        }
        return (icon);
      }


      public Int64 DiskIcon(bool on)
      {
        //TODO
        //	if (on) icon	|= 0x000F880000000000;
        //	else icon 		&= 0xFF0000FFFFFFFFFF;
        return (icon);
      }


      public Int64 DiskSpin(int Position)
      {
        //TODO
        //	if (on) icon	|= 0x000F880000000000;
        //	else icon 		&= 0xFF0000FFFFFFFFFF;
        return (icon);
      }

    #endregion

      #region Bit Helpers

      /// <summary>
      /// Sets a bit
      /// </summary>
      /// <param name="Source">The source long.</param>
      /// <param name="bit">Which Bit to set</param>
      /// <returns></returns>
      private static Int64 BitSet(Int64 Source, int bit)
      {
        //	Int64 temp;
        //	temp = BitClear(Source,bit);

        Int64 mask = 1;
        mask = mask << bit - 1;
        //	Debug.WriteLine ("bit set {0}",(Source|mask).ToString("X16"));
        return Source | mask;
      }

      /// <summary>
      /// Clears a bit
      /// </summary>
      /// <param name="Source">The source long.</param>
      /// <param name="bit">Which Bit to clear</param>
      /// <returns></returns>
      private static Int64 BitClear(Int64 Source, int bit)
      {
        Int64 mask = 1;
        mask = mask << bit - 1;
        mask ^= 0x0FFFFFFFFFFFFFFF;
        //	Debug.WriteLine("Bit Clear {0}",(Source & mask).ToString("X16"));
        return Source & mask;
      }
    }

      #endregion

    #region Font Bit Map

    // bit map updated Aug 31 2007 to fix numerous incorrect characters
    private static readonly byte[,] Font8x5 = { 
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

    #region Interop declarations

    [DllImport("SG_VFD.dll", EntryPoint = "iMONVFD_Init")]
    private static extern bool Open(int vfdType, int resevered);

    [DllImport("SG_VFD.dll", EntryPoint = "iMONVFD_Uninit")]
    private static extern void Close();

    [DllImport("SG_VFD.dll", EntryPoint = "iMONVFD_IsInited")]
    private static extern bool IsOpen();

    [DllImport("SG_VFD.dll", EntryPoint = "iMONVFD_SetText")]
    private static extern bool SetText(string firstLine, string secondLine);

    [DllImport("sg_vfd.dll", EntryPoint = "iMONVFD_SetEQ")]
    public static extern bool SetEQ(int arEQValue);

    [DllImport("sg_vfd.dll", EntryPoint = "iMONLCD_SendData")]
    public static extern unsafe bool iMONLCD_SendData(ref Int64 bitMap);

    #endregion
  }
}
