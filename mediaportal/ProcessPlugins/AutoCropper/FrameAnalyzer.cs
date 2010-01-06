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

using System.Drawing;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace ProcessPlugins.AutoCropper
{
  /// <summary>
  /// The FrameAnalyzer class provides functionality to decide
  /// the bounding box of a given Bitmap object containing
  /// letterboxed video material.
  /// </summary>
  internal class FrameAnalyzer
  {
    private int topStart = 0;
    private int topEnd = 0;
    private int bottomStart = 0;
    private int bottomEnd = 0;

    private int leftStart = 0;
    private int leftEnd = 0;
    private int rightStart = 0;
    private int rightEnd = 0;

    private int maxBrightnessTreshold = 40;
    private int minBrightnessTreshold = 4;

    private float topScanStartFraction = 0.35f;
    private float topScanEndFraction = 0.85f;
    private float bottomScanEndFraction = 1.0f;
    private float bottomScanStartFraction = 0.0f;

    private float leftScanStartFraction = 0.0f;
    private float leftScanEndFraction = 1.0f;
    private float rightScanStartFraction = 0.0f;
    private float rightScanEndFraction = 1.0f;

    private bool verboseLog = false;

    private int[] histR = new int[256];
    private int[] histG = new int[256];
    private int[] histB = new int[256];

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
      if (verboseLog)
      {
        Log.Debug("AutoCropper: Loading settings");
      }
      using (Settings reader = new MPSettings())
      {
        verboseLog = reader.GetValueAsBool(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmVerboseLog,
                                           false);
        topScanStartFraction =
          reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmTopStartSetting, 35) /
          100.0f;
        topScanEndFraction =
          reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmTopEndSetting, 80) / 100.0f;
        bottomScanStartFraction =
          reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmBottomStartSetting, 0) /
          100.0f;
        bottomScanEndFraction =
          reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmBottomEndSetting, 100) /
          100.0f;
        maxBrightnessTreshold = reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName,
                                                     AutoCropperConfig.parmMaxBrightnessTreshold, 40);
        minBrightnessTreshold =
          reader.GetValueAsInt(AutoCropperConfig.autoCropSectionName, AutoCropperConfig.parmMinBrightnessTreshold, 4) +
          16; //Add 16 to get level compared to video black level
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
      if (verboseLog)
      {
        Log.Debug("FindBounds");
      }
      this.frame = frame;

      int topLine = 0;
      int bottomLine = frame.Height - 1;
      int leftLine = 0;
      int rightLine = frame.Width - 1;

      bool foundTop = false;
      bool foundBottom = false;
      bool foundLeft = false;
      bool foundRight = false;

      topStart = (int)(topScanStartFraction * frame.Width);
      topEnd = (int)(topScanEndFraction * frame.Width);
      if (topEnd >= frame.Width)
      {
        topEnd--;
      }

      bottomStart = (int)(bottomScanStartFraction * frame.Width);
      bottomEnd = (int)(bottomScanEndFraction * frame.Width);
      if (bottomEnd >= frame.Width)
      {
        bottomEnd--;
      }

      leftStart = (int)(leftScanStartFraction * frame.Height);
      leftEnd = (int)(leftScanEndFraction * frame.Height);
      if (leftEnd >= frame.Height)
      {
        leftEnd--;
      }

      rightStart = (int)(rightScanStartFraction * frame.Height);
      rightEnd = (int)(rightScanEndFraction * frame.Height);
      if (rightEnd >= frame.Height)
      {
        rightEnd--;
      }

      if (verboseLog)
      {
        Log.Debug("Scanning top: {0} - {1}, bottom: {2} - {3}, left: {4} - {5}, right: {6} - {7}", topStart, topEnd,
                  bottomStart, bottomEnd, leftStart, leftEnd, rightStart, rightEnd);
      }

      //DrawLine(frame.Height / 2, 0, frame.Width - 1, Color.Red, true);

      //Top black bar binary search scan
      int mid = 0;
      int low = 0;
      int high = frame.Height / 2;

      while (low <= high)
      {
        mid = (low + high) / 2;
        ScanLine(mid, topStart, topEnd, true);
        if (IsContent(topStart, topEnd))
        {
          high = mid - 1;
          topLine = mid;
          foundTop = true;
          if (verboseLog)
          {
            Log.Debug("Found top line: {0}", topLine);
            //DrawLine(topLine, topStart, topEnd, Color.Red, true);
          }
        }
        else
        {
          low = mid + 1;
        }
      }

      //Bottom black bar binary search scan
      low = frame.Height / 2;
      high = frame.Height - 1;

      while (low <= high)
      {
        mid = (low + high) / 2;
        ScanLine(mid, bottomStart, bottomEnd, true);
        if (IsContent(bottomStart, bottomEnd))
        {
          low = mid + 1;
          bottomLine = mid;
          foundBottom = true;
          if (verboseLog)
          {
            Log.Debug("Found bottom line: {0}", bottomLine);
            //DrawLine(topLine, topStart, topEnd, Color.Red, true);
          }
        }
        else
        {
          high = mid - 1;
        }
      }

      // vertical scan of left half of screen
      for (int line = 0; line < frame.Width / 2; line++)
      {
        ScanLine(line, leftStart, leftEnd, false);
        if (IsContent(leftStart, leftEnd))
        {
          leftLine = line;
          foundLeft = true;
          if (verboseLog)
          {
            Log.Debug("Found left line: {0}", leftLine);
            //DrawLine(leftLine, leftStart, leftEnd, Color.Red, false);
          }
          break;
        }
      }

      // vertical scan of right half of screen
      for (int line = frame.Width - 1; line > frame.Width / 2; line--)
      {
        ScanLine(line, rightStart, rightEnd, false);
        if (IsContent(rightStart, rightEnd))
        {
          rightLine = line;
          foundRight = true;
          if (verboseLog)
          {
            Log.Debug("Found right line: {0}", rightLine);
            //DrawLine(rightLine, rightStart, rightEnd, Color.Coral, false);
          }
          break;
        }
      }

      //frame.Save("C:\\analyzed_frame.bmp", ImageFormat.Bmp); // for debug purposes

      if (!foundTop || !foundBottom || !foundLeft || !foundRight || bottomLine - topLine + 1 < frame.Height * 0.25f ||
          rightLine - leftLine + 1 < frame.Width * 0.25f)
      {
        if (verboseLog)
        {
          Log.Debug("Sanity check failed, analysis failed, returning null to skip frame");
        }
        //DrawLine(frame.Height / 2, 0, frame.Width - 1, Color.White, true); // indicate give up
        return false;
      }

      //DrawLine(topLine, 0, frame.Width - 1, Color.Red, true);
      //DrawLine(bottomLine, 0, frame.Width - 1, Color.Yellow, true);

      bounds.Y = topLine;
      bounds.X = leftLine;
      bounds.Height = bottomLine - topLine + 1;
      bounds.Width = rightLine - leftLine + 1;

      return true;
    }

    /// <summary>
    /// Scans a line in the frame, producing R,G and B histograms
    /// </summary>
    /// <param name="line"> The line to scan</param>
    /// <param name="start"> How far into the line to start scan (to avoid logos etc)</param>
    /// <param name="end"> How far into the line to stop the scan (to avoid logos etc) </param>
    /// <param name="horizontal"> Decides if this is a horizontal line scan (or vertical) </param>
    private void ScanLine(int line, int start, int end, bool horizontal)
    {
      //Log.Debug("Scanning line " + line);
      ResetHistograms();
      Color c = Color.Empty;

      for (int p = start; p <= end; p++)
      {
        if (horizontal) //horizontal line scan
        {
          c = frame.GetPixel(p, line);
        }
        else //vertical line scan
        {
          c = frame.GetPixel(line, p);
        }

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
    /// <param name="horizontal"></param>
    public void DrawLine(int line, int start, int end, Color c, bool horizontal)
    {
      if (verboseLog)
      {
        Log.Debug("DrawLine " + line);
      }
      for (int p = start; p <= end; p++)
      {
        if (horizontal) //horizontal line scan
        {
          frame.SetPixel(p, line, c);
        }
        else //vertical line scan
        {
          frame.SetPixel(line, p, c);
        }
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
    private bool IsContent(int start, int end)
    {
      int maxR = 0;
      int maxG = 0;
      int maxB = 0;
      int sumR = 0;
      int sumG = 0;
      int sumB = 0;

      //Check for brightest pixel value
      for (int i = 0; i < 255; i++)
      {
        if (histR[i] > 0 && i >= maxR)
        {
          maxR = i;
        }
        if (histG[i] > 0 && i >= maxG)
        {
          maxG = i;
        }
        if (histB[i] > 0 && i >= maxB)
        {
          maxB = i;
        }
      }
      //if (verboseLog) Log.Debug("Max : {0}, {1}, {2}", maxR, maxG, maxB);

      //At least one pixel with brightness level over 40 is found
      if (maxR > maxBrightnessTreshold || maxG > maxBrightnessTreshold || maxB > maxBrightnessTreshold)
      {
        return true;
      }

      //Check number of pixels above brightness treshold
      for (int j = minBrightnessTreshold; j < 255; j++)
      {
        sumR = sumR + histR[j];
        sumG = sumG + histG[j];
        sumB = sumB + histB[j];
      }
      //if (verboseLog) Log.Debug("Number of pixel above treshold : {0}, {1}, {2}", sumR, sumG, sumB);

      //Over half of the number of pixels are above the brightness treshold
      if (sumR > ((end - start) / 2) || sumG > ((end - start) / 2) || sumB > ((end - start) / 2))
      {
        return true;
      }

      //No content detected
      return false;
    }
  }
}