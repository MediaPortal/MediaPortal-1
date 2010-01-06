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
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplayPlugin.VFD_Control;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  /// <summary>
  /// DM-140GINK Demo VFD driver
  /// </summary>
  /// <author>stevie77</author>
  public class DM140GINK : BaseDisplay, IDisplay
  {
    #region Static Fields

    private static control vfd;

    #endregion

    #region Readonly Fields

    private const int lines = 2;
    private readonly bool isDisabled;
    private readonly string errorMessage = "";

    #endregion

    #region Constructor

    public DM140GINK()
    {
      try
      {
        vfd = new control(0x040b, 0x7001);
        vfd.initScreen();
      }
      catch (NotSupportedException ex)
      {
        isDisabled = true;
        errorMessage = ex.Message;
      }
    }

    #endregion

    #region IDisplay Members

    public void CleanUp()
    {
      vfd.clearLines();
    }

    /// <summary>
    /// Shows the advanced configuration screen
    /// </summary>
    public void Configure()
    {
      MessageBox.Show("No advanced configuration", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// Description of this display driver
    /// </summary>
    public string Description
    {
      get { return "DM-140GINK VFD driver for MSI Media Live & Hiper HMC-2K53A Barebones"; }
    }

    public void Dispose()
    {
      try
      {
        vfd.clearScreen();
        vfd.Shutdown();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void DrawImage(Bitmap bitmap) {}

    public string ErrorMessage
    {
      get { return errorMessage; }
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    public void Initialize()
    {
      vfd.clearScreen();
    }

    public bool IsDisabled
    {
      get { return isDisabled; }
    }

    /// <summary>
    /// Short name of this display driver
    /// </summary>
    public string Name
    {
      get { return "DM-140GINK"; }
    }

    public void SetCustomCharacters(int[][] customCharacters) {}

    /// <summary>
    /// Displays the message on the indicated line
    /// </summary>
    /// <param name="line">The line to display the message on</param>
    /// <param name="message">The message to display</param>
    public void SetLine(int line, string message)
    {
      if (line >= lines)
      {
        Log.Error("DM140GINK.SetLine: error bad line number" + line);
        return;
      }
      try
      {
        // Write message
        vfd.writeLine(line, RemoveDiacritics(message));

        //Volume calculation
        int vol = VolumeHandler.Instance.Volume / 2730;

        // Display symbols
        // LiveTV, LiveRadio
        vfd.updateSymbol(control.VFDSymbols.Antenna, false);

        // DVD
        if (g_Player.IsDVD)
        {
          vfd.updateSymbol(control.VFDSymbols.DVD, true);
        }
        else
        {
          vfd.updateSymbol(control.VFDSymbols.DVD, false);
        }

        // CDA
        if (g_Player.IsCDA)
        {
          vfd.updateSymbol(control.VFDSymbols.CD, true);
        }
        else
        {
          vfd.updateSymbol(control.VFDSymbols.CD, false);
        }

        // Video
        if ((g_Player.IsVideo || g_Player.IsTVRecording) & !(g_Player.IsDVD || g_Player.IsCDA))
        {
          vfd.updateSymbol(control.VFDSymbols.V, true);
        }
        else
        {
          vfd.updateSymbol(control.VFDSymbols.V, false);
        }

        // Play
        if (g_Player.Playing & !g_Player.Paused & (g_Player.Speed == 1) & !g_Player.IsDVDMenu)
        {
          vfd.updateSymbol(control.VFDSymbols.Play, true);
        }
        else
        {
          vfd.updateSymbol(control.VFDSymbols.Play, false);
        }

        // Pause
        if (g_Player.Paused)
        {
          vfd.updateSymbol(control.VFDSymbols.Pause, true);
        }
        else
        {
          vfd.updateSymbol(control.VFDSymbols.Pause, false);
        }

        // Forward
        if (g_Player.Speed > 1)
        {
          vfd.updateSymbol(control.VFDSymbols.Forward, true);
        }
        else
        {
          vfd.updateSymbol(control.VFDSymbols.Forward, false);
        }

        // Rewind
        if (g_Player.Speed < -1)
        {
          vfd.updateSymbol(control.VFDSymbols.Rewind, true);
        }
        else
        {
          vfd.updateSymbol(control.VFDSymbols.Rewind, false);
        }

        // Volume
        vfd.setVolume(vol);

        // Mute
        if (VolumeHandler.Instance.IsMuted)
        {
          vfd.updateSymbol(control.VFDSymbols.Mute, true);
        }
        else
        {
          vfd.updateSymbol(control.VFDSymbols.Mute, false);
        }

        // Recording in progress
        vfd.updateSymbol(control.VFDSymbols.REC, false);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    /// <summary>
    /// Initializes the display
    /// </summary>
    /// <param name="_port">The port the display is connected to</param>
    /// <param name="_lines">The number of lines in text mode</param>
    /// <param name="_cols">The number of columns in text mode</param>
    /// <param name="_delay">Communication delay in text mode</param>
    /// <param name="_linesG">The height in pixels in graphic mode</param>
    /// <param name="_colsG">The width in pixels in graphic mode</param>
    /// <param name="_delayG">Communication delay in graphic mode</param>
    /// <param name="_backLight">Backlight on?</param>
    /// <param name="_backLightLevel">Backlight level</param>
    /// <param name="_contrast">Contrast on?</param>
    /// <param name="_contrastLevel">Contrast level</param>
    /// <param name="_blankOnExit">Blank on exit?</param>
    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _backLightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
    {
      try
      {
        vfd.clearScreen();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    /// <summary>
    /// Does this driver support graphic mode?
    /// </summary>
    public bool SupportsGraphics
    {
      get { return false; }
    }

    /// <summary>
    /// Does this display support text mode?
    /// </summary>
    public bool SupportsText
    {
      get { return true; }
    }

    #endregion

    private static string RemoveDiacritics(String s)
    {
      // oddìlení znakù od modifikátorù (háèkù, èárek, atd.)
      s = s.Normalize(NormalizationForm.FormD);
      StringBuilder sb = new StringBuilder();

      for (int i = 0; i < s.Length; i++)
      {
        // do øetìzce pøidá všechny znaky kromì modifikátorù
        if (CharUnicodeInfo.GetUnicodeCategory(s[i]) != UnicodeCategory.NonSpacingMark)
        {
          sb.Append(s[i]);
        }
      }

      // vrátí øetìzec bez diakritiky
      return sb.ToString();
    }

    #region IDisposable Members

    #endregion
  }
}