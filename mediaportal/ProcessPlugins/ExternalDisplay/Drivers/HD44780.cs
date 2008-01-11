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

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>
  /// This class controls a HD44780 compatible display controller.
  /// </summary>
  /// <author>JoeDalton</author>
  public class HD44780 : BaseDisplay, IDisplay
  {
    private int cols;
    private int lines;
    private int delay;
    private const byte MASK = 0xB; //1011
    private const uint DELAY_SHORT = 40;
    private const uint DELAY_MEDIUM = 100;
    private const uint DELAY_LONG = 1600;
    private const uint DELAY_INIT = 4100;
    private const uint DELAY_BUS = 17;
    private int data = 0x378;
    private int control;
    private bool use2Controllers;
    private readonly bool alternateAddressing = false;
    private Controller controller = Controller.All;

    /// <summary>
    /// Initializes the display and makes it ready to accept data.
    /// </summary>
    public void Initialize()
    {
      //Initializing by instruction
      SetFunction(DataBits.Eight, Lines.Two, Font.Font5x8);
      Wait(DELAY_INIT);
      SetFunction(DataBits.Eight, Lines.Two, Font.Font5x8);
      Wait(DELAY_MEDIUM);
      SetFunction(DataBits.Eight, Lines.Two, Font.Font5x8);
      Wait(DELAY_SHORT);
      SetFunction(DataBits.Eight, Lines.Two, Font.Font5x8);
      Wait(DELAY_SHORT);
      DisplayControl(Display.Off, Cursor.Off, Blink.No);
      Wait(DELAY_SHORT);
      Clear();
      Wait(DELAY_LONG);
      SetEntryMode();
      Wait(DELAY_MEDIUM);
      DisplayControl(Display.On, Cursor.Off, Blink.No);
      Wait(DELAY_LONG);
      Home();
    }

    public void CleanUp()
    {
      Clear();
    }

    /// <summary>
    /// User defined Characters
    /// </summary>
    public void SetCustomCharacters(int[][] customCharacters)
    {
      if (customCharacters == null)
      {
        return;
      }
      // Begin ADDED/MODIFIED
      CGRam();

      // Send "Raw" data defined in ExternalDisplay.xml, no errorchecking
      foreach (int[] CustomCharacter in customCharacters)
      {
        foreach (int line in CustomCharacter)
        {
          SendData(line);
        }
      }
      DDRam();
    }

    public void DrawImage(Bitmap bitmap)
    {
      //Not supported
    }

    public void DrawText(int line, string text)
    {
      //Not supported
    }

    /// <summary>
    /// Moves cursor to home position
    /// </summary>
    protected void Home()
    {
      SendInstruction((int) Instruction.Home);
    }

    // Begin ADDED
    protected void CGRam()
    {
      SendInstruction((int) Instruction.SetCGRamAddress);
    }

    protected void DDRam()
    {
      SendInstruction((int) Instruction.SetDDRamAddress);
    }

    // End ADDED

    /// <summary>
    /// Moves the cursor to the indicated line and column by setting the DDRam address
    /// </summary>
    /// <param name="_line">The line to position the cursor on</param>
    /// <param name="_column">The column to position the cursor on</param>
    /// <remarks>
    /// The DDRam buffer is 128 characters.
    /// 0-63 are for line 1,
    /// 64-127 are for line 2
    /// 
    /// Displays having 4 lines, map lines 3 and 4 after line 1 and 2.
    /// So when having a 4x16 display, position 1 of line 3, 
    /// is mapped to position 17 of line 1 in the buffer.
    /// 
    /// Displays having more than 80 characters, have 2 controllers.
    /// Lines 1 & 2 are handled by controller 1, lines 3 & 4  are handled by controller 2.
    /// The C3 bit is then used as the enable line of controller 2
    ///
    /// At last a special case for some 1x16 displays.
    /// Some of them work internally as 2x8, so position 9 is in fact position 1 of line 2
    /// </remarks>
    private void SetPosition(int _line, int _column)
    {
      int line = _line;
      int col = _column;

      int pos;

      // special case for 1 chip 1x16 displays, acts like 2x8 display
      if (alternateAddressing)
      {
        if (col >= (cols/2))
        {
          col -= (cols/2);
          line += 1;
        }
      }
      //handle 2 controllers
      if (use2Controllers && (line >= (lines/2)))
      {
        line = line%(lines/2);
        controller = Controller.C2;
      }
      else
      {
        controller = Controller.C1;
      }

      pos = col + (line%2)*64;

      // line 3 logically follows line 1, (same for 4 and 2)
      if (line >= 2)
      {
        pos += cols;
      }

      SendInstruction((int) Instruction.SetDDRamAddress | pos);
    }

    /// <summary>
    /// Prints a character to the display
    /// </summary>
    /// <param name="_c"></param>
    private void Print(char _c)
    {
      SendData(_c);
    }

    /// <summary>
    /// Turns the display on or off, and sets the cursor mode.
    /// </summary>
    /// <param name="_on">A <see cref="Display"/> constant specifying whether the display should be turned ON or OFF</param>
    /// <param name="_cursor">A <see cref="Cursor"/> constant specifying whether the cursor is visible.</param>
    /// <param name="_blink">A <see cref="Blink"/> constant specifying whether the cursor should blink.</param>
    private void DisplayControl(Display _on, Cursor _cursor, Blink _blink)
    {
      controller = Controller.All;
      SendInstruction((byte) Instruction.DisplayControl | (byte) _on | (byte) _cursor | (byte) _blink);
    }

    /// <summary>
    /// Configures the display.
    /// </summary>
    /// <param name="_mode">A <see cref="DataBits"/> constant indicating whether data is send and received in Indicates whether communication with the dipsplay is done in 8bit (<b>true</b>) or 4bit(<b>false</b>) mode.</param>
    /// <param name="_lines">A <see cref="Lines"/> constant indicating the number of _lines in the display.</param>
    /// <param name="_font">A <see cref="Font"/> constant, specifying the font to use.</param>
    private void SetFunction(DataBits _mode, Lines _lines, Font _font)
    {
      controller = Controller.All;
      SendInstruction((byte) Instruction.SetFunction | (byte) _mode | (byte) _lines | (byte) _font);
    }

    private void SetEntryMode()
    {
      controller = Controller.All;
      SendInstruction((int) Instruction.SetEntryMode | 2);
    }

    private void SendInstruction(int _value)
    {
      //The parallel port control port has bits 0, 1 and 3 reversed,
      //this means that when we send 1, they are actually low.
      //That is why we are XOR-ing the value with 1011 to flip those bytes to their opposite state
      IO.Port[control] = ((int)Register.Instruction | (int)Mode.Write) ^ MASK;
      IO.Port[data] = _value;
      Wait(DELAY_BUS);
      IO.Port[control] =  (( (int)controller |  (int)Register.Instruction | (int)Mode.Write) ^ MASK);
      Wait(DELAY_BUS);
      IO.Port[control] = ((int)Register.Instruction | (int)Mode.Write) ^ MASK;
      Wait(DELAY_SHORT);
    }

    private void SendData(int _value)
    {
      //The parallel port control port has bits 0, 1 and 3 reversed,
      //this means that when we send 1, they are actually low.
      //That is why we are XOR-ing the value with 1011 to flip those bytes to their opposite state
      IO.Port[control] = ((byte) Register.Data | (byte) Mode.Write) ^ MASK;
      IO.Port[data] = _value;
      Wait(DELAY_BUS);
      IO.Port[control] = ((int) controller | (int) Register.Data | (int) Mode.Write) ^ MASK;
      Wait(DELAY_BUS);
      IO.Port[control] = ((byte) Register.Data | (byte) Mode.Write) ^ MASK;
      Wait(DELAY_SHORT);
    }


    private void Wait(long _microSecs)
    {
      long time = _microSecs*delay;
      if (time == 0)
      {
        return;
      }
      HighPerformanceCounter count = new HighPerformanceCounter();
      count.Start();
      do
        count.End(); while (count.MicroSeconds < time);
    }

    private enum Controller 
    {
      C1 = 0x1, //0001
      C2 = 0x8, //1000
      All = 0x9 //1001
    }

    private enum Font 
    {
      Font5x8 = 0x0,
      Font5x10 = 0x4
    }

    private enum Cursor 
    {
      On = 0x2,
      Off = 0x0
    }

    private enum Display
    {
      On = 0x4,
      Off = 0x0
    }

    private enum Blink
    {
      Yes = 0x1,
      No = 0x0
    }

    /// <summary>
    /// The number of data bits used to communciate with the display
    /// </summary>
    private enum DataBits
    {
      /// <summary>
      /// Use 4 bit wide data bus
      /// </summary>
      Four = 0x00,
      /// <summary>
      /// Use 8 bits wide data bus
      /// </summary>
      Eight = 0x10
    }

    /// <summary>
    /// The number of lines in the display
    /// </summary>
    private enum Lines 
    {
      /// <summary>
      /// The display has one line
      /// </summary>
      One = 0x0,
      /// <summary>
      /// The display has two lines
      /// </summary>
      Two = 0x8
    }

    private enum Register 
    {
      /// <summary>
      /// Use the Instruction register
      /// </summary>
      Instruction = 0x0,
      /// <summary>
      /// Use the Data register
      /// </summary>
      Data = 0x4
    }

    private enum Instruction
    {
      ClearDisplay = 0x01,
      Home = 0x02,
      SetEntryMode = 0x04,
      DisplayControl = 0x08,
      Shift = 0x10,
      SetFunction = 0x20,
      SetCGRamAddress = 0x40,
      SetDDRamAddress = 0x80
    }

    private enum Mode
    {
      Read = 0x2,
      Write = 0x0
    }

    #region IDisplay Members

    public void SetLine(int line, string message)
    {
      SetPosition(line, 0);
      for (int i = 0; i < message.Length; i++)
      {
        Print(message[i]);
      }
    }

    public string Name
    {
      get { return "HD44780JD"; }
    }

    public string Description
    {
      get { return "Joe Dalton's own HD44780 driver"; }
    }

    public bool SupportsText
    {
      get { return true; }
    }

    public bool SupportsGraphics
    {
      get { return false; }
    }

    public void Configure()
    {
    }

    public void Setup(string _port, int _lines, int _cols, int _delay, int linesG, int colsG, int timeG,
                      bool backLight, int contrast)
    {
      data = int.Parse(_port, NumberStyles.HexNumber);
      control = data + 2;
      lines = _lines;
      cols = _cols;
      delay = _delay;
      if (cols*lines > 80)
      {
        use2Controllers = true;
      }
      //alternateAddressing = _alternateAddressing;
      Initialize();
    }

    /// <summary>
    /// Clears the display (fill it with blanks), and moves to home position.
    /// </summary>
    public void Clear()
    {
      controller = Controller.All;
      SendInstruction((int) Instruction.ClearDisplay);
    }

    public bool IsDisabled
    {
      get { return false; }
    }

    public string ErrorMessage
    {
      get { return ""; }
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      Initialize();
    }

    #endregion
  }
}