#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace ProcessPlugins.AutoCropper
{
  /// <summary>
  /// The FrameAnalyzer class provides functionality to decide
  /// the bounding box of a given Bitmap object containing
  /// letterboxed video material.
  /// </summary>
  class FrameAnalyzer
  {
    private int topStart = 0;
    private int topEnd = 0;
    private int bottomStart = 0;
    private int bottomEnd = 0;

    private float topScanStartFraction = 0.35f;
    private float topScanEndFraction = 0.85f;

    private float bottomScanEndFraction = 1.0f;
    private float bottomScanStartFraction = 0.0f;
    private bool verboseLog = false;

    int[] histR = new int[256];
    int[] histG = new int[256];
    int[] histB = new int[256];

    private Bitmap frame = null;

    public FrameAnalyzer()
    {
      LoadSettings();
    }

    /// <summary>
    /// Loads settings for the frame analyzer from the settings file
    /// </summary>
    private void LoadSettings()
    {
      if (verboseLog) Log.Debug("AutoCropper: Loading settings");
      using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        verboseLog = reader.GetValueAsBool(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmVerboseLog, false);
        topScanStartFraction = reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmTopStartSetting, 35) / 100.0f;
        topScanEndFraction = reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmTopEndSetting, 80) / 100.0f;
        bottomScanStartFraction = reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmBottomStartSetting, 0) / 100.0f;
        bottomScanEndFraction = reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmBottomEndSetting, 100) / 100.0f;
        if (topScanEndFraction <= topScanStartFraction)
        {
          Log.Warn("AutoCropper: Top settings are wrong, end is lower than start!");
          topScanStartFraction = 0.0f;
          topScanEndFraction = 1.0f;
        }
        if (bottomScanEndFraction <= bottomScanStartFraction)
        {
          Log.Warn("AutoCropper: Bottom settings are wrong, end is lower than start!");
          bottomScanStartFraction = 0.0f;
          bottomScanEndFraction = 1.0f;
        }
      }
    }

    /// <summary>
    /// Find top and bottom bounds of a given bitmap. Performs a top down and bottom down scan
    /// to find the first top/bottom line that has content. Whether or not a line is 'content'
    /// is decided by the IsContent() method.
    /// </summary>
    /// <param name="frame"></param>
    /// <returns>True if analysis succeeded(ie is trustworthy) and false otherwise</returns>
    public bool FindBounds(Bitmap frame, ref Rectangle bounds)
    {
      if (verboseLog) Log.Debug("FindBounds");
      this.frame = frame;

      int topLine = 0;
      int bottomLine = frame.Height - 1;

      bool foundTop = false;
      bool foundBottom = false;

      topStart = (int)(topScanStartFraction * frame.Width);
      topEnd = (int)(topScanEndFraction * frame.Width);
      if (topEnd >= frame.Width) topEnd--;
      bottomEnd = (int)(bottomScanEndFraction * frame.Width);
      if (bottomEnd >= frame.Width) bottomEnd--;
      bottomStart = (int)(bottomScanStartFraction * frame.Width);

      if (verboseLog) Log.Debug("Scanning top: {0} - {1}, bottom {2} - {3}", topStart, topEnd, bottomStart, bottomEnd);

      //DrawLine(frame.Height / 2, 0, frame.Width - 1, Color.Red);

      // top down scan
      for (int line = 7; line < frame.Height / 2; line++)
      {
        ScanLine(line, topStart, topEnd);
        if (IsContent())
        {
          if (line <= 0.01f * frame.Height) line = 0;
          topLine = line;
          foundTop = true;
          if (verboseLog) Log.Debug("Found top line: {0}", topLine);
          DrawLine(topLine, topStart, topEnd, Color.Red);
          break;
        }
      }

      // bottom up scan
      for (int line = frame.Height - 1; line > frame.Height / 2; line--)
      {
        ScanLine(line, bottomStart, bottomEnd);
        if (IsContent())
        {
          foundBottom = true;
          bottomLine = line;
          if (verboseLog) Log.Debug("Found bottom line: {0}", bottomLine);
          DrawLine(bottomLine, bottomStart, bottomEnd, Color.Coral);
          break;
        }
      }

      //frame.Save("C:\\analyzed_frame.bmp", ImageFormat.Bmp); // for debug purposes

      if (!foundTop || !foundBottom || bottomLine - topLine + 1 < frame.Height * 0.25f)
      {
        if (verboseLog) Log.Debug("Sanity check failed, analysis failed, returning null to skip frame");
        //DrawLine(frame.Height / 2, 0, frame.Width - 1, Color.White); // indicate give up
        return false;
      }

      //DrawLine(topLine, 0, frame.Width - 1, Color.Red);
      //DrawLine(bottomLine, 0, frame.Width - 1, Color.Yellow);

      bounds.Y = topLine;
      bounds.X = 0;
      bounds.Height = bottomLine - topLine + 1;
      bounds.Width = frame.Width;
      return true;
    }

    /// <summary>
    /// Scans a line in the frame, producing R,G and B histograms
    /// </summary>
    /// <param name="line"> The line to scan</param>
    /// <param name="start"> How far into the line to start scan (to avoid logos etc)</param>
    /// <param name="end"> How far into the line to stop the scan (to avoid logos etc) </param>
    private void ScanLine(int line, int start, int end)
    {
      //Log.Debug("Scanning line " + line);
      ResetHistograms();
      for (int p = start; p <= end; p++)
      {
        Color c = frame.GetPixel(p, line);
        histG[0xFF & c.G]++;
        histR[0xFF & c.R]++;
        histB[0xFF & c.B]++;
      }
    }

    /// <summary>
    /// (For debugging) Draws a line in the frame being analyzed
    /// </summary>
    /// <param name="line"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="c"></param>
    public void DrawLine(int line, int start, int end, Color c)
    {
      if (verboseLog) Log.Debug("DrawLine " + line);
      for (int p = start; p <= end; p++)
      {
        frame.SetPixel(p, line, c);
      }
    }


    /// <summary>
    /// Resets the RGB histograms
    /// </summary>
    private void ResetHistograms()
    {
      for (int i = 0; i < histR.Length; i++)
      {
        histR[i] = 0;
        histG[i] = 0;
        histB[i] = 0;
      }
    }

    /// <summary>
    /// Determines if the last line scanned was content or not
    /// </summary>
    /// <returns></returns>
    private bool IsContent()
    {
      int maxR = 0;
      int maxG = 0;
      int maxB = 0;

      for (int i = 0; i < 255; i++)
      {
        if (histR[i] > 0 && i >= maxR) maxR = i;
        if (histG[i] > 0 && i >= maxG) maxG = i;
        if (histB[i] > 0 && i >= maxB) maxB = i;
      }
      //Log.Debug("Max : {0}, {1}, {2}", maxR, maxG, maxB);

      // for now, try to just rely on max value
      if (maxR > 30 || maxG > 30 || maxB > 30) return true;

      return false;
    }
  }
}
