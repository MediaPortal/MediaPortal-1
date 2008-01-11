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
  public class MpeControlDesigner : MpeResourceDesigner
  {
    #region Variables

    protected MpeControl control;

    #endregion

    #region Constructors

    public MpeControlDesigner(MediaPortalEditor mpe, MpeControl control) : base(mpe)
    {
      this.control = control;
    }

    #endregion

    #region Properties - Designer

    public override string ResourceName
    {
      get { return control.Type.DisplayName; }
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

    public override void Initialize()
    {
      try
      {
        AllowDrop = false;
        screen = (MpeScreen) Parser.CreateControl(MpeControlType.Screen);
        screen.Location = new Point(Mask.NodeSize, Mask.NodeSize);
        Controls.Add(screen);
        if (control.Type != MpeControlType.Screen)
        {
          screen.Controls.Add(control);
        }
        else
        {
          control = screen;
        }
        control.IsReference = true;
        control.Modified = false;
        MpeLog.Info("Control designer intialized [" + control.Type.DisplayName + "]");
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
        Parser.SaveControl(control);
        control.Modified = false;
        MpeLog.Info("Control designer saved [" + control.Type.DisplayName + "]");
      }
      catch (MpeParserException mpe)
      {
        throw new DesignerException(mpe.Message, mpe);
      }
    }

    public override void Cancel()
    {
      base.Cancel();
      MpeLog.Info("Control designer cancelled [" + control.Type.DisplayName + "]");
    }

    public override void Destroy()
    {
      base.Destroy();
      MpeLog.Info("Control designer destroyed [" + control.Type.DisplayName + "]");
    }

    public override void Pause()
    {
      base.Pause();
      MpeLog.Info("Control designer paused [" + control.Type.DisplayName + "]");
    }

    public override void Resume()
    {
      base.Resume();
      MpeLog.Info("Control designer resumed [" + control.Type.DisplayName + "]");
    }

    #endregion

    #region Event Handlers

    public override void OnControlStatusChanged(MpeControl sender, bool modified)
    {
      if (sender == control)
      {
        base.OnControlStatusChanged(sender, modified);
      }
    }

    #endregion
  }
}