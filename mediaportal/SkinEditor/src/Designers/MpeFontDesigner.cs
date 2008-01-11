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
using Mpe.Controls;
using Mpe.Controls.Properties;

namespace Mpe.Designers
{
  public class FontDesigner : MpeResourceDesigner
  {
    #region Variables

    private MpeFont font;
    private MpeFontViewer viewer;

    #endregion

    #region Constructors

    public FontDesigner(MediaPortalEditor mpe, MpeFont font) : base(mpe)
    {
      this.font = font;
    }

    #endregion

    #region Methods - MpeDesigner

    public override void Initialize()
    {
      try
      {
        AllowDrop = false;
        PropertyManager.HideResourceList();
        if (font != null)
        {
          screen = (MpeScreen) Parser.GetControl(MpeControlType.Screen);
          viewer = new MpeFontViewer(font, screen.TextureBack);
          viewer.Location = new Point(Mask.NodeSize, Mask.NodeSize);
          viewer.SelectedIndexChanged += new MpeFontViewer.SelectedIndexChangedHandler(OnViewerIndexChanged);
          viewer.Modified = false;
          Controls.Add(viewer);
        }
        MpeLog.Info("Font designer initialized [" + ResourceName + "]");
      }
      catch (MpeParserException mpe)
      {
        MpeLog.Debug(mpe);
        throw new DesignerException(mpe.Message);
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        throw new DesignerException(e.Message);
      }
    }

    public override void Save()
    {
      try
      {
        Parser.SaveFont(font);
        viewer.Modified = false;
        MpeLog.Info("Font designer saved [" + ResourceName + "]");
      }
      catch (MpeParserException mpe)
      {
        throw new DesignerException(mpe.Message, mpe);
      }
    }

    public override void Cancel()
    {
      base.Cancel();
      MpeLog.Info("Font designer cancelled [" + ResourceName + "]");
    }

    public override void Destroy()
    {
      base.Destroy();
      if (screen != null)
      {
        screen.Destroy();
      }
      if (viewer != null)
      {
        viewer.Destroy();
      }
      MpeLog.Info("Font designer destroyed [" + ResourceName + "]");
    }

    public override void Pause()
    {
      base.Pause();
      MpeLog.Info("Font designer paused [" + ResourceName + "]");
    }

    public override void Resume()
    {
      base.Resume();
      PropertyManager.SelectedResource = viewer;
      PropertyManager.HideResourceList();
      MpeLog.Info("Font designer resumed [" + ResourceName + "]");
    }

    #endregion

    #region Designer Implementation Properties

    public override string ResourceName
    {
      get { return font.Name; }
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

    private void OnViewerIndexChanged(int oldIndex, int newIndex)
    {
      PropertyManager.Refresh();
    }
  }
}