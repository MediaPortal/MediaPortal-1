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

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplayPlugin.VFD_Control;
using MediaPortal.TV.Recording;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  /// <summary>
  /// FIC Spectra Futaba VFD driver
  /// </summary>
  /// <author>Andrea Tincani</author>
  public class FICSpectra : BaseDisplay, IDisplay
  {
    #region Static Fields

    private static control vfd;

    #endregion

    #region Readonly Fields

    private const int lines = 1;
    private readonly bool isDisabled;
    private readonly string errorMessage = "";

    #endregion

    #region Constructor

    public FICSpectra()
    {
      try
      {
        vfd = new control(0x0547, 0x7000);
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
      vfd.clearScreen();
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
      get { return "FIC Spectra Futaba VFD driver by Andrea Tincani (andreavb)"; }
    }

    public void Dispose()
    {
      try
      {
        vfd.clearScreen();
        vfd.writeMainScreen("SPECTRA");
        vfd.Shutdown();
        vfd = null;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void DrawImage(Bitmap bitmap)
    {
    }

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
      get { return "FICSpectra"; }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
    }

    /// <summary>
    /// Displays the message on the indicated line
    /// </summary>
    /// <param name="line">The line to display the message on</param>
    /// <param name="message">The message to display</param>
    public void SetLine(int line, string message)
    {
      try
      {
        if (line >= lines)
        {
          Log.Error("FICSpectra.SetLine: error bad line number" + line);
        }
        else
        {
          // Write message
          vfd.writeLine(line, RemoveDiacritics(message));
        }

        //Volume calculation
        int vol = VolumeHandler.Instance.Volume/6553;

        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_TV ||
            GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_TVFULLSCREEN)
        {
          Log.Debug("FICSpectra.SetLine: TV ON");
          vfd.updateFICSymbol(control.FICSymbols.TV, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: TV OFF");
          vfd.updateFICSymbol(control.FICSymbols.TV, false);
        }
        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_RADIO)
        {
          Log.Debug("FICSpectra.SetLine: RADIO ON");
          vfd.updateFICSymbol(control.FICSymbols.Radio, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: RADIO OFF");
          vfd.updateFICSymbol(control.FICSymbols.Radio, false);
        }
        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_PICTURES)
        {
          Log.Debug("FICSpectra.SetLine: PHOTO ON");
          vfd.updateFICSymbol(control.FICSymbols.Photo, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: Photo OFF");
          vfd.updateFICSymbol(control.FICSymbols.Photo, false);
        }
        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_MUSIC)
        {
          Log.Debug("FICSpectra.SetLine: Music ON");
          vfd.updateFICSymbol(control.FICSymbols.Music, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: Music OFF");
          vfd.updateFICSymbol(control.FICSymbols.Music, false);
        }
        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_TVGUIDE)
        {
          Log.Debug("FICSpectra.SetLine: GUIDE ON");
          vfd.updateFICSymbol(control.FICSymbols.Guide1, true);
          vfd.updateFICSymbol(control.FICSymbols.Guide2, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: GUIDE OFF");
          vfd.updateFICSymbol(control.FICSymbols.Guide1, false);
          vfd.updateFICSymbol(control.FICSymbols.Guide2, false);
        }
        // DVD
        if (g_Player.IsDVD)
        {
          Log.Debug("FICSpectra.SetLine: DVD ON");
          vfd.updateFICSymbol(control.FICSymbols.DVD, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: DVD OFF");
          vfd.updateFICSymbol(control.FICSymbols.DVD, false);
        }

        // CDA
        if (g_Player.IsCDA)
        {
          Log.Debug("FICSpectra.SetLine: CD ON");
          vfd.updateFICSymbol(control.FICSymbols.CD, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: CD OFF");
          vfd.updateFICSymbol(control.FICSymbols.CD, false);
        }


        // Play
        if (g_Player.Playing & !g_Player.Paused & (g_Player.Speed == 1) & !g_Player.IsDVDMenu)
        {
          Log.Debug("FICSpectra.SetLine: Play ON");
          vfd.updateFICSymbol(control.FICSymbols.Play, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: Play OFF");
          vfd.updateFICSymbol(control.FICSymbols.Play, false);
        }

        // Pause
        if (g_Player.Paused)
        {
          Log.Debug("FICSpectra.SetLine: Pause ON");
          vfd.updateFICSymbol(control.FICSymbols.Pause, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: Pause OFF");
          vfd.updateFICSymbol(control.FICSymbols.Pause, false);
        }

        // Forward
        if (g_Player.Speed > 1)
        {
          Log.Debug("FICSpectra.SetLine: Forward ON");
          vfd.updateFICSymbol(control.FICSymbols.Fwd, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: Forward OFF");
          vfd.updateFICSymbol(control.FICSymbols.Fwd, false);
        }

        // Rewind
        if (g_Player.Speed < -1)
        {
          Log.Debug("FICSpectra.SetLine: Rewind ON");
          vfd.updateFICSymbol(control.FICSymbols.Rew, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: Rewind OFF");
          vfd.updateFICSymbol(control.FICSymbols.Rew, false);
        }

        // Volume
        vfd.setVolume(vol);

        // Mute
        if (VolumeHandler.Instance.IsMuted)
        {
          Log.Debug("FICSpectra.SetLine: Mute ON");
          vfd.updateFICSymbol(control.FICSymbols.Mute, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: Mute OFF");
          vfd.updateFICSymbol(control.FICSymbols.Mute, false);
        }

        // Recording in progress
        if (Recorder.IsAnyCardRecording())
        {
          Log.Debug("FICSpectra.SetLine: REC ON");
          vfd.updateFICSymbol(control.FICSymbols.REC, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: REC OFF");
          vfd.updateFICSymbol(control.FICSymbols.REC, false);
        }
        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_HOME)
        {
          Log.Debug("FICSpectra.SetLine: HOME ON");
          vfd.updateFICSymbol(control.FICSymbols.Home, true);
        }
        else
        {
          Log.Debug("FICSpectra.SetLine: HOME OFF");
          vfd.updateFICSymbol(control.FICSymbols.Home, false);
        }
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
      // only upper case characters for FIC Spectra display
      s = s.ToUpper();
      StringBuilder sb = new StringBuilder();

      for (int i = 0; i < s.Length; i++)
      {
        if ((s[i] >= 'A' && s[i] <= 'Z') || (s[i] >= '0' && s[i] <= '9') || (s[i] == ' '))
        {
          sb.Append(s[i]);
        }
        else
        {
          sb.Append(' ');
        }
      }

      // vrátí øetìzec bez diakritiky
      return sb.ToString();
    }

    #region IDisposable Members

    #endregion
  }
}