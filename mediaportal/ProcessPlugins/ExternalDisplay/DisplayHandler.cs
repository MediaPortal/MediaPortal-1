#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Drawing;
using ExternalDisplay.Setting;
using MediaPortal.GUI.Library;
using ProcessPlugins.ExternalDisplay.Setting;
using Image=ProcessPlugins.ExternalDisplay.Setting.Image;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This class is responsible for scrolling the texts on the display
  /// </summary>
  /// <author>JoeDalton</author>
  public class DisplayHandler
  {
    protected int heightInChars;
    protected int widthInChars;
    protected Line[] lines; //Keeps the lines of text to display on the display
    protected int[] pos; //Keeps track of the start positions in the display lines
    private List<Image> images;
    private IDisplay display; //Reference to the display we are controlling
    private Font font;
    private static readonly Brush graphicBrush = new SolidBrush(Color.White);
    private static readonly Brush textBrush = new SolidBrush(Color.Black);
    private int graphicTextHeight = -1;
    private readonly int pixelsToScroll;
    private readonly int widthInPixels;
    private readonly int heightInPixels;
    private readonly bool forceGraphicText;
    private readonly int charsToScroll;


    internal DisplayHandler(IDisplay _display)
    {
      display = _display;
      //buffer a number of Settings in local fields (=faster access)
      heightInChars = Settings.Instance.TextHeight;
      widthInChars = Settings.Instance.TextWidth;
      pixelsToScroll = Settings.Instance.PixelsToScroll;
      widthInPixels = Settings.Instance.GraphicWidth;
      heightInPixels = Settings.Instance.GraphicHeight;
      forceGraphicText = Settings.Instance.ForceGraphicText;
      charsToScroll = Settings.Instance.CharsToScroll;
      //
      lines = new Line[heightInChars];
      pos = new int[heightInChars];
      font = new Font(Settings.Instance.Font, Settings.Instance.FontSize);
      for (int i = 0; i < heightInChars; i++)
      {
        lines[i] = new Line();
        pos[i] = 0;
      }
    }

    public List<Image> Images
    {
      get { return images; }
      set { images = value; }
    }

    /// <summary>
    /// Initializes the display.
    /// </summary>
    /// <remarks>
    internal void Start()
    {
      display.Setup(Settings.Instance.Port, heightInChars, widthInChars,
                    Settings.Instance.TextComDelay, heightInPixels,
                    widthInPixels, Settings.Instance.GraphicComDelay,
                    Settings.Instance.BackLight, Settings.Instance.Contrast);
      display.Initialize();
      display.SetCustomCharacters(Settings.Instance.CustomCharacters);
    }

    /// <summary>
    /// Stops the display.
    /// </summary>
    internal void Stop()
    {
      display.CleanUp();
    }

    internal List<Line> Lines
    {
      set
      {
        int i = 0;
        while (i < value.Count && i < heightInChars)
        {
          lines[i] = value[i];
          i++;
        }
        while (i < heightInChars)
        {
          lines[i] = new Line();
          i++;
        }
      }
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    internal void Dispose()
    {
      Stop();
    }

    /// <summary>
    /// Updates the display
    /// </summary>
    internal void DisplayLines()
    {
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Sending lines to display.");
      }
      try
      {
        if (display.SupportsGraphics)
        {
          using (Bitmap buffer = new Bitmap(widthInPixels, heightInPixels))
          {
            using (Graphics graphics = Graphics.FromImage(buffer))
            {
              graphics.FillRectangle(graphicBrush, 0, 0, widthInPixels, heightInPixels);
              foreach (Image image in Images)
              {
                //we need to use the bitmap's physical dimensions, otherwise the image class 
                //will resize the image according the dpi of the monitor in Windows.
                graphics.DrawImage(image.Bitmap, new RectangleF(new PointF(image.X, image.Y), image.Bitmap.PhysicalDimension));
              }
              if (!display.SupportsText || forceGraphicText)
              {
                for (int i = 0; i < heightInChars; i++)
                {
                  ProcessG(graphics, i);
                }
              }
              display.DrawImage(0, 0, buffer);
            }
          }
        }
        if (display.SupportsText && !forceGraphicText)
        {
          for (int i = 0; i < heightInChars; i++)
          {
            display.SetLine(i, Process(i));
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("ExternalDisplay.DisplayLines: " + ex.Message);
      }
    }

    private void ProcessG(Graphics _graphics, int _line)
    {
      Line line = lines[_line];
      string text = line.Process();
      if (string.IsNullOrEmpty(text))
      {
        return;
      }
      SizeF size = _graphics.MeasureString(text, font);
      if (size.Height > graphicTextHeight)
      {
        graphicTextHeight = (int) (size.Height + 0.5f);
      }
      if (size.Width < widthInPixels)
      {
        //text is shorter than display width
        StringFormat fmt = new StringFormat();
        switch (line.Alignment)
        {
          case Alignment.Right:
            fmt.Alignment = StringAlignment.Far;
            break;
          case Alignment.Centered:
            fmt.Alignment = StringAlignment.Center;
            break;
          default:
            fmt.Alignment = StringAlignment.Near;
            break;
        }
        _graphics.DrawString(text, font, textBrush,
                             new RectangleF(new PointF(0, _line*graphicTextHeight),
                                            new SizeF(widthInPixels, size.Height)), fmt);
        return;
      }
      //Text is longer than display width
      if (pos[_line] > size.Width + pixelsToScroll)
      {
        pos[_line] = 0;
      }
      text += " - " + text;
      _graphics.DrawString(text, font, textBrush, new PointF(0 - pos[_line], _line*graphicTextHeight));
      pos[_line] += pixelsToScroll;
    }

    /// <summary>
    /// This method processes the text to send to the display so that it will fit.
    /// If the text is shorter than the display width it will use the message allignment.
    /// If the text is longer than the display width it will take a substring of it based on the 
    /// position to create a scrolling effect.
    /// </summary>
    /// <param name="_line">The line to process</param>
    /// <returns>The processed result</returns>
    protected string Process(int _line)
    {
      Line line = lines[_line];
      string tmp = line.Process();
      //No text to display, so empty the line
      if (tmp == null || tmp.Length == 0)
      {
        return new string(' ', widthInChars);
      }
      if (tmp.Length <= widthInChars)
      {
        //Text is shorter than display width
        switch (line.Alignment)
        {
          case Alignment.Right:
            {
              string format = "{0," + widthInChars + "}";
              return string.Format(format, tmp);
            }
          case Alignment.Centered:
            {
              int left = (widthInChars - tmp.Length)/2;
              return new string(' ', left) + tmp + new string(' ', widthInChars - tmp.Length - left);
            }
          default:
            {
              string format = "{0,-" + widthInChars + "}";
              return string.Format(format, tmp);
            }
        }
      }
      //Text is longer than display width
      if (pos[_line] > tmp.Length + charsToScroll + 1)
      {
        pos[_line] = 0;
      }
      tmp += " - " + tmp;
      tmp = tmp.Substring(pos[_line], widthInChars);
      pos[_line] += charsToScroll;
      return tmp;
    }
  }
}