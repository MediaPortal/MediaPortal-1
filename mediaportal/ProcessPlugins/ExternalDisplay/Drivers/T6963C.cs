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

using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// Driver for the Toshiba T6963C display controller
  /// </summary>
  /// <author>JoeDalton</author>
  public class T6963C : BaseDisplay, IDisplay
  {
    private const int GHOME_ADDR = 0x0000;
    private const int THOME_ADDR = 0x1700;
    private const int CGROM_ADDR = 0x1C00;
    private int dataPort;
    private int controlPort;
    private int NC_ROWS;
    private int NC_COLS;
    private int grows;
    private int gcols;
    private readonly BitmapConverter converter = new BitmapConverter(true);
    private readonly SHA256Managed sha256 = new SHA256Managed(); //instance of crypto engine used to calculate hashes

    private byte[] lastHash; //hash of the last bitmap that was sent to the display

    [DllImport("dlportio.dll", EntryPoint = "DlPortWritePortUchar")]
    private static extern void Output(int adress, byte value);

    [DllImport("dlportio.dll", EntryPoint = "DlPortReadPortUchar")]
    private static extern int Input(int adress);

    public void Setup(string _port, int _lines, int _cols, int _delay, int linesG, int colsG, int timeG,
                      bool backLight, int contrast)
    {
      NC_ROWS = _lines;
      NC_COLS = _cols;
      grows = linesG;
      gcols = colsG;
      dataPort = int.Parse(_port, NumberStyles.HexNumber);
      controlPort = dataPort + 2;
      int tmp;

      tmp = Input(dataPort + 0x402);
      tmp = tmp & 0x1F;
      tmp = tmp | 0x20; // Bidirektionaler Modus = PS/2
      Output(dataPort + 0x402, (byte) tmp);

      // Set Text Home Address to 0x0000
      disp_write_data2(THOME_ADDR); // Data1: LowAddress
      disp_write_command(0x40); // Command: 0x40 -> 01000000

      //Set width of 1 line to 40 bytes
      disp_write_data2(NC_COLS);
      disp_write_command(0x41);


      ///* SET GRAPHICS HOME ADDR */
      disp_write_data2(GHOME_ADDR);
      disp_write_command(0x42);

      ///* SET GRAPHICS AREA */
      disp_write_data2(NC_COLS);
      disp_write_command(0x43);

      ///* SET MODE */
      disp_write_command(0x81); /* CG ROM, LOGICAL XOR */

      disp_write_command(0xA7); // cursor is 8 lines high

      //Set Cursor position
      SetLCDCursor(1, 1);

      // DisplayMode
      disp_write_command(0x9D);
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
      if (customCharacters == null)
      {
        return;
      }
      disp_write_data2(CGROM_ADDR >> 11);
      disp_write_command(0x22);
      disp_write_data2(CGROM_ADDR); /* disp_set_addr(CGROM_ADDR); */
      disp_write_command(0x24);
      disp_write_command(0xb0); /* AUTO WRITE */
      // Send "Raw" data defined in ExternalDisplay.xml, no errorchecking
      foreach (int[] CustomCharacter in customCharacters)
      {
        foreach (int line in CustomCharacter)
        {
          SendData(line);
        }
      }
      disp_write_command(0xb2);
    }

    public void Initialize()
    {
      Clear();
    }

    public void CleanUp()
    {
      Clear();
    }

    private void Clear()
    {
      ClearLCDText();
      ClearLCDGraph();
    }

    public void Print(char _c)
    {
      //Custom characters in the T6963c start at position 128, instead of position 0
      if (_c < 32)
      {
        _c = (char) (_c + 160);
      }
      SendData(_c - 0x20);
      disp_write_command(0xC0);
    }

    private void SetPosition(byte _line, byte _column)
    {
      SetLCDXY(_column, _line);
    }

    private void WaitForDisplayReady()
    {
      int ret = 0;
      while (ret != 3)
      {
        Output(controlPort, 0x26);
        Output(controlPort, 0x2e);
        ret = Input(dataPort);
        Output(controlPort, 0x26);
        ret = ret & 3;
      }
    }

    public void SendData(int Data)
    {
      WaitForDisplayReady();
      Output(controlPort, 0); //(* C/D = L (Data) *) R
      Output(controlPort, 2); //(* CE *)             R
      Output(dataPort, (byte) Data);
      Output(controlPort, 3); //(* CE+WR *)
      Output(controlPort, 2); //(* CE *)             R
      Output(controlPort, 0);
    }


    public void disp_write_command(int Command)
    {
      WaitForDisplayReady();
      Output(controlPort, 4); //(* C/D = H (ctrl) *)  R
      Output(controlPort, 6); //(* CE *)              R
      Output(dataPort, (byte) Command);
      Output(controlPort, 7); //(* CE+WR *)
      Output(controlPort, 6); //(* CE *)              R
      Output(controlPort, 0);
    }


    public void SetLCDXY(int X, int Y)
    {
      int addr = THOME_ADDR + (Y*NC_COLS) + X;
      disp_set_addr(addr);
    }

    private void SetLCDCursor(int X, int Y)
    {
      SendData(X);
      SendData(Y);
      disp_write_command(0x21);
    }

    private void ClearLCDText()
    {
      int numPixels = NC_ROWS*NC_COLS;
      disp_set_addr(THOME_ADDR);
      disp_write_command(0xb0);
      for (int i = 0; i <= numPixels; i++)
      {
        SendData(0);
      }
      disp_write_command(0xb2);
    }

    private void ClearLCDGraph()
    {
      int numPixels = grows/8*gcols;
      disp_set_addr(GHOME_ADDR);
      disp_write_command(0xb0);
      for (int i = 0; i <= numPixels; i++)
      {
        SendData(0);
      }
      disp_write_command(0xb2);
    }


    public void disp_plot(int x, int y)
    {
      int _data;

      disp_set_addr(GHOME_ADDR + y*NC_COLS + (x/8));

      _data = 7 - (byte) (x%8); /* THE BIT NUMBER */
      _data |= 0xf8; /* COMMAND */
      disp_write_command(_data); /* EXECUTE THE COMMAND */
    }

    public void disp_clear(int x, int y)
    {
      int _data;

      disp_set_addr(GHOME_ADDR + y*NC_COLS + (x/8));

      _data = 7 - (byte) (x%8); /* THE BIT NUMBER */
      _data |= 0xf0; /* COMMAND */
      disp_write_command(_data); /* EXECUTE THE COMMAND */
    }

    /// <summary>
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="line">The line to thow the message on.</param>
    /// <param name="message">The message to show.</param>
    public void SetLine(int line, string message)
    {
      SetPosition((byte) line, 0);
      for (int i = 0; i < message.Length; i++)
      {
        Print(message[i]);
      }
    }

    /// <summary>
    /// Gets the short name of the display
    /// </summary>
    public string Name
    {
      get { return "T6963CJD"; }
    }

    /// <summary>
    /// Gets the description of the display
    /// </summary>
    public string Description
    {
      get { return "Joe Dalton's own T6963c driver"; }
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
    {}

    public bool IsDisabled
    {
      get { return false; }
    }

    public string ErrorMessage
    {
      get { return ""; }
    }

    public void Dispose()
    {}

    private void disp_write_data2(int _data)
    {
      SendData(_data & 0xFF);
      SendData(_data >> 8);
    }

    private void disp_set_addr(int address)
    {
      disp_write_data2(address);
      disp_write_command(0x24); // address pointer to GHOME_ADDR
    }

    private void disp_auto_write(int _data)
    {
      WaitForDisplayReady();
      SendData(_data);
    }

    public void DrawImage(Bitmap bitmap)
    {
      if (bitmap == null)
      {
        return;
      }
      byte[] data = converter.ToByteArray(bitmap);
      //Calculate its hash so we can compare it to the previous bitmap more efficiently
      byte[] hash = sha256.ComputeHash(data);
      //Compare the new hash with the previous one to determine whether the new image is
      //equal to the one that is already shown.  If they are equal, then we are done
      if (ByteArray.AreEqual(hash, lastHash))
      {
        return;
      }

      int size = gcols*grows;
      byte b = 0;
      disp_set_addr(GHOME_ADDR);
      disp_write_command(0xb0);
      for (int i = 0; i < size; i++)
      {
        int pixel = i*3;
        if (Color.FromArgb(data[pixel + 2],
                           data[pixel + 1],
                           data[pixel]).GetBrightness() < 0.5f)
        {
          b = (byte) (b | (byte) (1 << (7 - i%8)));
        }
        if (i%8 == 7)
        {
          disp_auto_write(b);
          b = 0;
        }
      }
      disp_write_command(0xb2);
      lastHash = hash;
    }


  }
}