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
using ExternalDisplay.Setting;
using ProcessPlugins.ExternalDisplay.Setting;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay
{
    /// <summary>
    /// This class is responsible for scrolling the texts on the display
    /// </summary>
    /// <author>JoeDalton</author>
    public class DisplayHandler
    {
        protected int height;
        protected int width;
        protected Line[] lines; //Keeps the lines of text to display on the display
        protected int[] pos; //Keeps track of the start positions in the display lines
        private List<Image> images;
        

        private IDisplay display; //Reference to the display we are controlling

        internal DisplayHandler(IDisplay _display)
        {
            display = _display;
            height = Settings.Instance.TextHeight;
            width = Settings.Instance.TextWidth;
            lines = new Line[height];
            pos = new int[height];
            for (int i = 0; i < height; i++)
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
            display.Setup(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth,
                                Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight,
                                Settings.Instance.GraphicWidth, Settings.Instance.GraphicComDelay,
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

        /// <summary>
        /// Shows the given message on the indicated line.
        /// </summary>
        /// <param name="_line">The line to thow the message on.</param>
        /// <param name="_message">The message to show.</param>
        internal void SetLine(int _line, Line _message)
        {
            lines[_line] = _message;
            //pos[_line-1]   = 0;  //reset scrolling
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
                for (byte i = 0; i < height; i++)
                {
                    display.SetLine(i, Process(i));
                }
                foreach (Image image in Images)
                {
                    display.DrawImage(image.X, image.Y, image.Bitmap);
                }
            }
            catch (Exception ex)
            {
                Log.Info("ExternalDisplay.DisplayLines: " + ex.Message);
            }
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
                return new string(' ', width);
            }
            if (tmp.Length <= width)
            {
                //Text is shorter than display width
                switch (line.Alignment)
                {
                    case Alignment.Right:
                        {
                            string format = "{0," + width + "}";
                            return string.Format(format, tmp);
                        }
                    case Alignment.Centered:
                        {
                            int left = (width - tmp.Length)/2;
                            return new string(' ', left) + tmp + new string(' ', width - tmp.Length - left);
                        }
                    default:
                        {
                            string format = "{0,-" + width + "}";
                            return string.Format(format, tmp);
                        }
                }
            }
            //Text is longer than display width
            if (pos[_line] > tmp.Length + 2)
            {
                pos[_line] = 0;
            }
            tmp += " - " + tmp;
            tmp = tmp.Substring(pos[_line]++, width);
            return tmp;
        }
    }
}
