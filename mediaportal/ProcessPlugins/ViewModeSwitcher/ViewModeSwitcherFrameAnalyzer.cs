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
using System.Drawing;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace ProcessPlugins.ViewModeSwitcher
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

    private int maxBrightnessTreshold = 32;
    private int minBrightnessTreshold = 16;

    private double topScanStartFraction = 0.3;
    private double topScanEndFraction = 0.7;
    private double bottomScanEndFraction = 0.7;
    private double bottomScanStartFraction = 0.3;

    private double leftScanStartFraction = 0.3;
    private double leftScanEndFraction = 0.7;
    private double rightScanStartFraction = 0.3;
    private double rightScanEndFraction = 0.7;

    private int[] histR = new int[256];
    private int[] histG = new int[256];
    private int[] histB = new int[256];

    private Bitmap frame = null;

    public FrameAnalyzer() 
    {
      maxBrightnessTreshold = (int)ViewModeSwitcher.currentSettings.LBMaxBlackLevel;
      minBrightnessTreshold = (int)ViewModeSwitcher.currentSettings.LBMinBlackLevel;
      
      topScanStartFraction    = (double)(100-ViewModeSwitcher.currentSettings.DetectWidthPercent)/200.0;
      topScanEndFraction      = 1.0 - topScanStartFraction;
      bottomScanStartFraction = topScanStartFraction;
      bottomScanEndFraction   = topScanEndFraction;
      
      leftScanStartFraction   = (double)(100-ViewModeSwitcher.currentSettings.DetectHeightPercent)/200.0;
      leftScanEndFraction     = 1.0 - topScanStartFraction;                                             
      rightScanStartFraction  = leftScanStartFraction;
      rightScanEndFraction    = leftScanEndFraction;
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
      if (ViewModeSwitcher.currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: FindBounds");
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

      topStart = (int)(topScanStartFraction * (double)frame.Width);
      topEnd = (int)(topScanEndFraction * (double)frame.Width);
      if (topEnd >= frame.Width)
      {
        topEnd--;
      }

      bottomStart = (int)(bottomScanStartFraction * (double)frame.Width);
      bottomEnd = (int)(bottomScanEndFraction * (double)frame.Width);
      if (bottomEnd >= frame.Width)
      {
        bottomEnd--;
      }

      leftStart = (int)(leftScanStartFraction * (double)frame.Height);
      leftEnd = (int)(leftScanEndFraction * (double)frame.Height);
      if (leftEnd >= frame.Height)
      {
        leftEnd--;
      }

      rightStart = (int)(rightScanStartFraction * (double)frame.Height);
      rightEnd = (int)(rightScanEndFraction * (double)frame.Height);
      if (rightEnd >= frame.Height)
      {
        rightEnd--;
      }

      if (ViewModeSwitcher.currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: Scanning top: {0} - {1}, bottom: {2} - {3}, left: {4} - {5}, right: {6} - {7}",
                  topStart, topEnd,
                  bottomStart, bottomEnd, leftStart, leftEnd, rightStart, rightEnd);
      }

      //DrawLine(frame.Height / 2, 0, frame.Width - 1, Color.Red, true);

      //Top black bar binary search scan
      int mid = 1;
      int low = 1;
      int high = (int)((double)frame.Height * 0.25);

      while (low <= high)
      {
        ScanLine(mid, topStart, topEnd, true);
        if (IsContent(topStart, topEnd))
        {
          high = mid - 1;
          topLine = mid;
          foundTop = true;
          if (ViewModeSwitcher.currentSettings.verboseLog)
          {
            Log.Debug("ViewModeSwitcher: Found top line: {0}", topLine);
            //DrawLine(topLine, topStart, topEnd, Color.Red, true);
          }
        }
        else
        {
          low = mid + 1;
        }
        mid = (low + high) / 2;
      }
      if (topLine < 1)
      {
        topLine = 1;
      }

      //Left black bar binary search scan
      mid = 1;
      low = 1;
      high = (int)((double)frame.Width * 0.25);

      while (low <= high)
      {
        ScanLine(mid, leftStart, leftEnd, false);
        if (IsContent(leftStart, leftEnd))
        {
          high = mid - 1;
          leftLine = mid;
          foundLeft = true;
          if (ViewModeSwitcher.currentSettings.verboseLog)
          {
            Log.Debug("ViewModeSwitcher: Found left line: {0}", leftLine);
            //DrawLine(leftLine, leftStart, leftEnd, Color.Red, false);
          }
        }
        else
        {
          low = mid + 1;
        }
        mid = (low + high) / 2;
      }
      if (leftLine < 1)
      {
        leftLine = 1;
      }

      if (!foundLeft && !foundTop)
      {
        bounds.Y = 0;
        bounds.X = 0;
        bounds.Height = frame.Height;
        bounds.Width = frame.Width;
        return true;
        //return false;
      }


      //Right black bar binary search scan
      low = (int)((double)frame.Width * 0.75);
      high = frame.Width - 1;
      mid = high;
      while (low <= high)
      {
        ScanLine(mid, rightStart, rightEnd, false);
        if (IsContent(rightStart, rightEnd))
        {
          low = mid + 1;
          rightLine = mid;
          foundRight = true;
          if (ViewModeSwitcher.currentSettings.verboseLog)
          {
            Log.Debug("ViewModeSwitcher: Found right line: {0}", rightLine);
            //DrawLine(rightLine, rightStart, rightEnd, Color.Coral, false);
          }
        }
        else
        {
          high = mid - 1;
        }
        mid = (low + high) / 2;
      }
      if (rightLine >= frame.Width)
      {
        rightLine = frame.Width - 1;
      }

      if (!foundLeft && !foundRight)
      {
        bounds.Y = 0;
        bounds.X = 0;
        bounds.Height = frame.Height;
        bounds.Width = frame.Width;
        return true;
        //return false;
      }

      //Bottom black bar binary search scan
      low = (int)((double)frame.Height * 0.75);
      high = frame.Height - 1;
      mid = high;
      while (low <= high)
      {
        ScanLine(mid, bottomStart, bottomEnd, true);
        if (IsContent(bottomStart, bottomEnd))
        {
          low = mid + 1;
          bottomLine = mid;
          foundBottom = true;
          if (ViewModeSwitcher.currentSettings.verboseLog)
          {
            Log.Debug("ViewModeSwitcher: Found bottom line: {0}", bottomLine);
            //DrawLine(topLine, topStart, topEnd, Color.Red, true);
          }
        }
        else
        {
          high = mid - 1;
        }
        mid = (low + high) / 2;
      }
      if (bottomLine >= frame.Height)
      {
        bottomLine = frame.Height - 1;
      }

      //frame.Save("C:\\analyzed_frame.bmp", ImageFormat.Bmp); // for debug purposes

      if (!foundBottom || (bottomLine - topLine) < (int)((double)frame.Height * 0.25) ||
          (rightLine - leftLine) < (int)((double)frame.Width * 0.25))
      {
        if (ViewModeSwitcher.currentSettings.verboseLog)
        {
          Log.Debug("ViewModeSwitcher: Sanity check failed, analysis failed, returning null to skip frame");
        }
        //DrawLine(frame.Height / 2, 0, frame.Width - 1, Color.White, true); // indicate give up
        return false;
      }

      //DrawLine(topLine, 0, frame.Width - 1, Color.Red, true);
      //DrawLine(bottomLine, 0, frame.Width - 1, Color.Yellow, true);

      bounds.Y = topLine;
      bounds.X = leftLine;
      bounds.Height = bottomLine - topLine;
      bounds.Width = rightLine - leftLine;
      return true;
    }

    /// <summary>
    /// Checks if the horizontal line in the center of the frame is black.
    /// This is the case for 3D videos in TAB format
    /// </summary>
    /// <param name="frame"></param>
    /// <returns>True if analysis succeeded(ie is trustworthy) and false otherwise</returns>
    public bool CheckFor3DTabBlackBar(Bitmap frame)
    {
        ScanLine(frame.Height / 2, 0, frame.Width - 1, true);        
        return !IsContent(0, frame.Width - 1);        
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
      if (ViewModeSwitcher.currentSettings.verboseLog)
      {
        Log.Debug("ViewModeSwitcher: DrawLine " + line);
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
        if (i >= minBrightnessTreshold)
        {
          sumR = sumR + histR[i];
          sumG = sumG + histG[i];
          sumB = sumB + histB[i];
        }
      }
      //if (ViewModeSwitcher.currentSettings.verboseLog)
      //  Log.Debug("ViewModeSwitcher: Max : R{0}, G{1}, B{2}", maxR, maxG, maxB);

      //At least one pixel with brightness level over 'LBMaxBlackLevel' is found
      if (maxR > maxBrightnessTreshold || maxG > maxBrightnessTreshold || maxB > maxBrightnessTreshold)
      {
        return true;
      }

      //if (ViewModeSwitcher.currentSettings.verboseLog)
      //  Log.Debug("ViewModeSwitcher: Number of pixel above treshold : R{0}, G{1}, B{2}", sumR, sumG, sumB);

      //Over 25% of the pixels in the line are above the minimum brightness treshold
      if (sumR > ((end - start) / 4) || sumG > ((end - start) / 4) || sumB > ((end - start) / 4))
      {
        return true;
      }

      //No content detected
      return false;
    }
  }
}