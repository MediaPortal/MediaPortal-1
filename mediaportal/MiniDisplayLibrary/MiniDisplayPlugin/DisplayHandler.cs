#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using MediaPortal.GUI.Library;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting;
using Image = MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting.Image;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class DisplayHandler
  {
    private readonly int charsToScroll;
    private readonly IDisplay display;
    private Bitmap emptyBitmap;
    private static Font font;
    private readonly bool forceGraphicText;
    private readonly Brush graphicBrush = new SolidBrush(Color.White);
    private int graphicTextHeight = -1;
    protected int heightInChars;
    protected int heightInCharsSim;
    private int heightInPixels;
    private List<Image> images;
    protected Line[] lines;
    private readonly int pixelsToScroll;
    protected int[] pos;
    protected int[] posSkips;
    protected string[] prevLines;
    private readonly Brush textBrush = new SolidBrush(Color.Black);
    protected int widthInChars;
    private int widthInPixels;

    public DisplayHandler(IDisplay _display)
    {
      this.display = _display;
      this.heightInChars = Settings.Instance.TextHeight;
      if (this.heightInChars == 1)
      {
        this.heightInCharsSim = 2;
      }
      else
      {
        this.heightInCharsSim = this.heightInChars;
      }
      this.widthInChars = Settings.Instance.TextWidth;
      this.pixelsToScroll = Settings.Instance.PixelsToScroll;
      this.widthInPixels = Settings.Instance.GraphicWidth;
      this.heightInPixels = Settings.Instance.GraphicHeight;
      this.charsToScroll = Settings.Instance.CharsToScroll;
      this.forceGraphicText = Settings.Instance.ForceGraphicText;
      this.lines = new Line[this.heightInCharsSim];
      this.prevLines = new string[this.heightInChars];
      this.posSkips = new int[this.heightInChars];
      this.pos = new int[this.heightInChars];
      font = new Font(Settings.Instance.Font, (float)Settings.Instance.FontSize);
      for (int i = 0; i < this.heightInCharsSim; i++)
      {
        this.lines[i] = new Line();
      }
      for (int i = 0; i < this.heightInChars; i++)
      {
        this.pos[i] = 0;
      }
    }

    public void DisplayLines()
    {
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Info("MiniDisplayPlugin.DisplayHandler.DisplayLines(): Sending lines to display.");
      }
      try
      {
        if (this.display.SupportsGraphics)
        {
          this.SendGraphics();
        }
        if (this.display.SupportsText && !this.forceGraphicText)
        {
          this.SendText();
        }
      }
      catch (Exception exception)
      {
        Log.Error(
          "MiniDisplayPlugin.DisplayHandler.DisplayLines(): CAUGHT EXCEPTION {0}\n\n{1}\n\n" + exception.Message,
          new object[] {exception.StackTrace});
      }
    }

    public void Dispose()
    {
      this.Stop();
    }

    private void DrawImages(Graphics graphics)
    {
      if ((this.images.Count == 0) && Settings.Instance.ExtensiveLogging)
      {
        Log.Info("MiniDisplayPlugin.DisplayHandler.DrawImages(): No images to process");
      }
      foreach (Image image in this.Images)
      {
        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Info("MiniDisplayPlugin.DisplayHandler.DrawImages(): Drawing image to buffer");
        }
        Bitmap bitmap = image.Bitmap;
        if (bitmap != null)
        {
          graphics.DrawImage(bitmap,
                             new RectangleF(new PointF((float)image.X, (float)image.Y), bitmap.PhysicalDimension));
        }
      }
    }

    private void DrawText(Graphics graphics)
    {
      for (int i = 0; i < this.heightInChars; i++)
      {
        this.ProcessG(graphics, i);
      }
    }

    private Bitmap GetEmptyBitmap()
    {
      if (this.emptyBitmap == null)
      {
        this.emptyBitmap = new Bitmap(this.widthInPixels, this.heightInPixels, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(this.emptyBitmap))
        {
          graphics.FillRectangle(this.graphicBrush, 0, 0, this.widthInPixels, this.heightInPixels);
          goto Label_00E0;
        }
      }
      if ((this.emptyBitmap.Width != Settings.Instance.GraphicWidth) ||
          (this.emptyBitmap.Height != Settings.Instance.GraphicHeight))
      {
        this.emptyBitmap.Dispose();
        this.emptyBitmap = new Bitmap(this.widthInPixels, this.heightInPixels, PixelFormat.Format32bppArgb);
        using (Graphics graphics2 = Graphics.FromImage(this.emptyBitmap))
        {
          graphics2.FillRectangle(this.graphicBrush, 0, 0, this.widthInPixels, this.heightInPixels);
        }
      }
      Label_00E0:
      return (Bitmap)this.emptyBitmap.Clone();
    }

    protected string Process(int _line)
    {
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Info("MiniDisplayPlugin.DisplayHandler.Process(): Processing line #{0}", new object[] {_line});
      }
      Line line;
      Line line2;
      string str;
      if (this.heightInChars == 1)
      {
        line = this.lines[0];
        line2 = this.lines[1];
        str = line2.Process() + " - " + line.Process();
      }
      else
      {
        line = this.lines[_line];
        str = line.Process();
      }
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Info("MiniDisplayPlugin.DisplayHandler.Process(): translated line #{0} to \"{2}\"",
                 new object[] {_line, str});
      }
      if ((str == null) || (str.Length == 0))
      {
        return new string(' ', this.widthInChars);
      }
      if (this.prevLines[_line] == null)
      {
        this.prevLines[_line] = "";
      }
      try
      {
        if (this.prevLines[_line] != str)
        {
          this.pos[_line] = 0;
          this.posSkips[_line] = 0;
          this.prevLines[_line] = str;
        }
      }
      catch (Exception exception)
      {
        Log.Error("MiniDisplayPlugin.DisplayHandler.Process(): CAUGHT EXCEPTION - {0}\n\n{1}\n\n" + exception.Message,
                  new object[] {exception.StackTrace});
      }
      if (str.Length <= this.widthInChars)
      {
        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Info("MiniDisplayPlugin.DisplayHandler.Process(): final processing result: \"{0}\"", new object[] {str});
        }
        switch (line.Alignment)
        {
          case Alignment.Centered:
            {
              int count = (this.widthInChars - str.Length) / 2;
              return (new string(' ', count) + str + new string(' ', (this.widthInChars - str.Length) - count));
            }
          case Alignment.Right:
            return string.Format("{0," + this.widthInChars + "}", str);
        }
        return string.Format("{0,-" + this.widthInChars + "}", str);
      }
      if (this.pos[_line] > ((str.Length + this.charsToScroll) + 1))
      {
        this.pos[_line] = 0;
      }
      str = (str + " - " + str).Substring(this.pos[_line], this.widthInChars);
      if (this.posSkips[_line] > 2)
      {
        this.pos[_line] += this.charsToScroll;
      }
      else
      {
        this.posSkips[_line]++;
      }
      if (this.posSkips[_line] > 10)
      {
        this.posSkips[_line] = 10;
      }
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Info("MiniDisplayPlugin.DisplayHandler.Process(): final processing result: \"{0}\"", new object[] {str});
      }
      return str;
    }

    private void ProcessG(Graphics _graphics, int _line)
    {
      if (font.SizeInPoints != Settings.Instance.FontSize)
      {
        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Debug("MiniDisplayPlugin.DisplayHandler.ProcessG: Adjusting graphics Font size to {0}.",
                    new object[] {Settings.Instance.FontSize});
        }
        font.Dispose();
        font = new Font(Settings.Instance.Font, (float)Settings.Instance.FontSize);
      }
      Line line = this.lines[_line];
      string str = line.Process();
      if (!string.IsNullOrEmpty(str))
      {
        if (this.prevLines[_line] == null)
        {
          this.prevLines[_line] = "";
        }
        try
        {
          if (this.prevLines[_line] != str)
          {
            this.pos[_line] = 0;
            this.posSkips[_line] = 0;
            this.prevLines[_line] = str;
          }
        }
        catch (Exception exception)
        {
          Log.Error("MiniDisplayPlugin.DisplayHandler.ProcessG(): error - {0}" + exception.Message);
        }
        SizeF ef = _graphics.MeasureString(str, font);
        if (ef.Height > this.graphicTextHeight)
        {
          this.graphicTextHeight = (int)(ef.Height + 0.5f);
        }
        if (ef.Width >= this.widthInPixels)
        {
          if (this.pos[_line] > (ef.Width + this.pixelsToScroll))
          {
            this.pos[_line] = 0;
          }
          str = str + " - " + str;
          _graphics.DrawString(str, font, this.textBrush,
                               new PointF((float)-this.pos[_line], (float)(_line * this.graphicTextHeight)));
          if (this.posSkips[_line] > 2)
          {
            this.pos[_line] += this.pixelsToScroll;
          }
          else
          {
            this.posSkips[_line]++;
          }
          if (this.posSkips[_line] > 10)
          {
            this.posSkips[_line] = 10;
          }
        }
        else
        {
          StringFormat format = new StringFormat();
          switch (line.Alignment)
          {
            case Alignment.Centered:
              format.Alignment = StringAlignment.Center;
              break;

            case Alignment.Right:
              format.Alignment = StringAlignment.Far;
              break;

            default:
              format.Alignment = StringAlignment.Near;
              break;
          }
          _graphics.DrawString(str, font, this.textBrush,
                               new RectangleF(new PointF(0f, (float)(_line * this.graphicTextHeight)),
                                              new SizeF((float)this.widthInPixels, ef.Height)), format);
        }
      }
    }

    private void SendGraphics()
    {
      using (Bitmap bitmap = this.GetEmptyBitmap())
      {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Info("MiniDisplayPlugin.DisplayHandler.SendGraphics(): Processing graphics display.");
          }
          this.DrawImages(graphics);
          if (!this.display.SupportsText || this.forceGraphicText)
          {
            this.DrawText(graphics);
          }
          this.display.DrawImage(bitmap);
        }
      }
    }

    private void SendText()
    {
      for (int i = 0; i < this.heightInChars; i++)
      {
        this.display.SetLine(i, this.Process(i));
      }
    }

    public void Start()
    {
      Log.Info("MiniDisplayPlugin.DisplayHandler.Start(): Called");
      Log.Info("MiniDisplayPlugin.DisplayHandler.Start(): Calling driver Setup() function");
      this.display.Setup(Settings.Instance.Port, this.heightInChars, this.widthInChars, Settings.Instance.TextComDelay,
                         this.heightInPixels, this.widthInPixels, Settings.Instance.GraphicComDelay,
                         Settings.Instance.BackLightControl, Settings.Instance.Backlight,
                         Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
      if (font.SizeInPoints != Settings.Instance.FontSize)
      {
        font.Dispose();
        font = new Font(Settings.Instance.Font, (float)Settings.Instance.FontSize);
        Log.Info("MiniDisplayPlugin.DisplayHandler.Start(): Forcing font size to {0}",
                 new object[] {Settings.Instance.FontSize});
      }
      if ((this.heightInPixels != Settings.Instance.GraphicHeight) ||
          (this.widthInPixels != Settings.Instance.GraphicWidth))
      {
        this.heightInPixels = Settings.Instance.GraphicHeight;
        this.widthInPixels = Settings.Instance.GraphicWidth;
      }
      if (this.widthInChars != Settings.Instance.TextWidth)
      {
        this.widthInChars = Settings.Instance.TextWidth;
      }
      Log.Info("MiniDisplayPlugin.DisplayHandler.Start(): Calling driver Initialize() function");
      this.display.Initialize();
      this.display.SetCustomCharacters(Settings.Instance.CustomCharacters);
      Log.Info("MiniDisplayPlugin.DisplayHandler.Start(): Completed");
    }

    public void Stop()
    {
      Log.Info("MiniDisplay.DisplayHandler.Stop(): Called");
      Log.Info("MiniDisplay.DisplayHandler.Stop(): Calling driver CleanUp() function");
      this.display.CleanUp();
      Log.Info("MiniDisplay.DisplayHandler.Stop(): completed");
    }

    public List<Image> Images
    {
      get { return this.images; }
      set { this.images = value; }
    }

    public List<Line> Lines
    {
      set
      {
        int index = 0;
        while ((index < value.Count) && (index < this.heightInCharsSim))
        {
          this.lines[index] = value[index];
          index++;
        }
        while (index < this.heightInCharsSim)
        {
          this.lines[index] = new Line();
          index++;
        }
      }
    }
  }
}