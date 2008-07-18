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
using System.IO;
using Mpe.Controls;
using Mpe.Controls.Properties;

namespace Mpe.Designers
{
  /// <summary>
  ///
  /// </summary>
  public class MpeImageDesigner : MpeResourceDesigner
  {
    #region Variables

    private FileInfo imageFile;

    #endregion

    #region Constructors

    public MpeImageDesigner(MediaPortalEditor mpe, FileInfo image) : base(mpe)
    {
      imageFile = image;
    }

    #endregion

    #region Properties - Designer

    public override string ResourceName
    {
      get { return imageFile.Name; }
    }

    public override bool AllowAdditions
    {
      get { return false; }
    }

    public override bool AllowDeletions
    {
      get { return false; }
    }

    #endregion

    #region Methods - Designer

    public override void Resume()
    {
      base.Resume();
      MpeLog.Info("Image designer resumed [" + ResourceName + "]");
    }

    public override void Pause()
    {
      base.Pause();
      MpeLog.Info("Image designer paused [" + ResourceName + "]");
    }

    public override void Initialize()
    {
      screen = (MpeScreen) Parser.CreateControl(MpeControlType.Screen);
      screen.Location = new Point(Mask.NodeSize, Mask.NodeSize);

      Controls.Add(screen);

      MpeImageViewer image = new MpeImageViewer();
      image.Id = 1;
      image.Texture = imageFile;
      screen.Controls.Add(image);
      MpeLog.Info("Image designer initialized [" + ResourceName + "]");
    }

    public override void Save()
    {
      MpeLog.Info("Image designer saved [" + ResourceName + "]");
    }

    public override void Cancel()
    {
      MpeLog.Info("Image designer cancelled [" + ResourceName + "]");
    }

    public override void Destroy()
    {
      base.Destroy();
      if (screen != null)
      {
        screen.Dispose();
      }
      MpeLog.Info("Image designer destroyed [" + ResourceName + "]");
    }

    #endregion

    #region Event Handlers

    public override void OnControlStatusChanged(MpeControl sender, bool modified)
    {
      //
    }

    #endregion
  }
}