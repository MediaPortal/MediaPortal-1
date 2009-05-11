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
using System.Collections;
using System.IO;
using System.Threading;
using System.Windows.Media.Animation;

using MediaPortal.Drawing;

namespace MediaPortal.GUI.Library
{

  public sealed class GUIWaitCursor : GUIControl
  {
    #region Constructors

    private GUIWaitCursor()
    {
    }

    #endregion Constructors

    #region Methods

    private static Thread guiWaitCursorThread = null;

    public static void Dispose()
    {
      if (_animation != null)
        _animation.FreeResources();

      _animation = null;
    }

    public static void Hide()
    {
      Interlocked.Decrement(ref _showCount);
      guiWaitCursorThread = null;
    }

    private static void GUIWaitCursorThread()
    {
      if (Interlocked.Increment(ref _showCount) == 1)
      {
        _animation.Begin();
      }
    }

    public static void Init()
    {
      if (_animation != null)
        return;
      _animation = new GUIAnimation();

      foreach (string filename in Directory.GetFiles(GUIGraphicsContext.Skin + @"\media\", "common.waiting.*.png"))
        _animation.Filenames.Add(Path.GetFileName(filename));

      // dirty hack because the files are 96x96 - unfortunately no property gives the correct size at runtime when init is called :S
      int scaleWidth = (GUIGraphicsContext.Width / 2) - 48;
      int scaleHeigth = (GUIGraphicsContext.Height / 2) - 48;

      _animation.SetPosition(scaleWidth, scaleHeigth);

      // broken!?
      _animation.HorizontalAlignment = HorizontalAlignment.Center;
      _animation.VerticalAlignment = VerticalAlignment.Center;

      Log.Debug("GUIWaitCursor: init at position {0}:{1}", scaleWidth, scaleHeigth);
      _animation.AllocResources();
      _animation.Duration = new Duration(800);
      _animation.RepeatBehavior = RepeatBehavior.Forever;
    }

    public override void Render(float timePassed)
    {
    }

    public static void Render()
    {
      if (_showCount <= 0)
        return;

      GUIGraphicsContext.SetScalingResolution(0, 0, false);
      _animation.Render(GUIGraphicsContext.TimePassed);
    }

    public static void Show()
    {

      if (guiWaitCursorThread == null)
      {
        guiWaitCursorThread = new Thread(GUIWaitCursorThread);
        guiWaitCursorThread.IsBackground = true;
        guiWaitCursorThread.Name = "Waitcursor";
        guiWaitCursorThread.Start();
      }
    }

    #endregion Methods

    #region Fields

    static GUIAnimation _animation;
    static int _showCount = 0;

    #endregion Fields
  }
}