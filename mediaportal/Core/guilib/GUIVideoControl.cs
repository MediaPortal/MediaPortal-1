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
using System;
using System.Drawing;
using Microsoft.DirectX;

using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;




namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// This class will draw a placeholder for the current video window
	/// if no video is playing it will present an empty rectangle
	/// </summary>
	public class GUIVideoControl : GUIControl
	{
		GUIImage image;
		[XMLSkinElement("textureFocus")]	protected string	_focusedTextureName="";
		[XMLSkinElement("action")]			protected int		_actionId=-1;
		protected GUIAnimation _imageFocusRectangle=null;
		
		protected Rectangle[] _videoWindows= new Rectangle[1];
	
		public GUIVideoControl(int dwParentID) : base(dwParentID)
		{
		}
		public GUIVideoControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string texturename)
			:base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
		{
			_focusedTextureName = texturename;
			FinalizeConstruction();
		}
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
			_imageFocusRectangle = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height, _focusedTextureName);
			image = new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height, "black.bmp" ,1);

      _imageFocusRectangle.ParentControl = this;
      image.ParentControl = this;
		}


    public override void AllocResources()
    {
      base.AllocResources ();
      _imageFocusRectangle.AllocResources();
			image.AllocResources();
    }
    public override void FreeResources()
    {
      base.FreeResources ();
      _imageFocusRectangle.FreeResources();
			image.FreeResources();
    }


    public override bool CanFocus()
    {
      if (_imageFocusRectangle.FileName==String.Empty) return false;
      return true;
    }


    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible) return;
      }
      
      float x=base.XPosition;
      float y=base.YPosition;
      GUIGraphicsContext.Correct(ref x,ref y);

      _videoWindows[0].X=(int)x;
      _videoWindows[0].Y=(int)y;
      _videoWindows[0].Width=base.Width;
      _videoWindows[0].Height=base.Height;
      if (!GUIGraphicsContext.Calibrating )
      {
        GUIGraphicsContext.VideoWindow=_videoWindows[0];
				if (GUIGraphicsContext.ShowBackground)
				{
          if (Focus)
          {
            int xoff=5; int yoff=5;
            int w=10;int h=10;
            GUIGraphicsContext.ScalePosToScreenResolution(ref xoff, ref yoff);
            GUIGraphicsContext.ScalePosToScreenResolution(ref w, ref h);
            xoff += GUIGraphicsContext.OffsetX;
            yoff += GUIGraphicsContext.OffsetY;
            _imageFocusRectangle.SetPosition((int)x-xoff,(int)y-yoff);
            _imageFocusRectangle.Width=base.Width+w;
            _imageFocusRectangle.Height=base.Height+h;
            _imageFocusRectangle.Render(timePassed);
          }

					if (GUIGraphicsContext.graphics!=null)
          {
						GUIGraphicsContext.graphics.FillRectangle(Brushes.Black , _videoWindows[0].X,_videoWindows[0].Y,base.Width,base.Height);
					}
					else
					{
						//image.SetPosition(_videoWindows[0].X,_videoWindows[0].Y);
						//image.Width=_videoWindows[0].Width;
						//image.Height=_videoWindows[0].Height;
						image.Render(timePassed);
						//GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target|ClearFlags.Target, Color.FromArgb(255,1,1,1), 1.0f, 0,_videoWindows);
					}
				}
				else
				{
					if (GUIGraphicsContext.graphics!=null)
					{
						GUIGraphicsContext.graphics.FillRectangle(Brushes.Black , _videoWindows[0].X,_videoWindows[0].Y,base.Width,base.Height);
					}
				}
      }
    }

    public override void OnAction( Action action) 
    {
      base.OnAction(action);
      GUIMessage message ;
      if (Focus)
      {
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          // If this button corresponds to an action generate that action.
          if (ActionID >=0)
          {
            Action newaction = new Action((Action.ActionType)ActionID,0,0);
            GUIGraphicsContext.OnAction(newaction);
            return;
          }
          
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID,0,0,null );
          GUIGraphicsContext.SendMessage(message);
        }
      }
    }

    /// <summary>
    /// Get/set the action ID that corresponds to this button.
    /// </summary>
    public int ActionID
    {
      get { return _actionId;}
      set { _actionId=value;}

    }

    /// <summary>
    /// Checks if the x and y coordinates correspond to the current control.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>True if the control was hit.</returns>
    public override bool HitTest(int x, int y,out int controlID, out bool focused)
    {
      focused=Focus;
      controlID=GetID;
      if (!IsVisible || Disabled || CanFocus() == false) 
      {
        return false;
      }
      if ( InControl(x,y, out controlID))
      {
        if (CanFocus())
        {
          return true; 
        }
      }
      focused=Focus=false;
      return false;
    }

    
	}
}
