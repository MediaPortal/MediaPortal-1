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
using System.Collections;
using System.Drawing;
using System.Diagnostics;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class implementing a GUIButton.
	/// </summary>
	public class GUIActionMenuButton : GUIControl
	{
		[XMLSkinElement("layoutfile")] 	   protected string _layoutFile = String.Empty;
		[XMLSkinElement("textureFocus")] 	 protected string _focusedTextureName = String.Empty;
		[XMLSkinElement("textureNoFocus")] protected string _nonFocusedTextureName = String.Empty;


		protected GUIAnimation _imageFocused = null;
		protected GUIAnimation _imageNonFocused = null;

		public GUIActionMenuButton(int dwParentID) : base(dwParentID)
    {
    }

		/// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction();
			_imageFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height, _focusedTextureName);
			_imageFocused.ParentControl = this;
			_imageFocused.Filtering = false;
			_imageFocused.DimColor = DimColor;
			_imageFocused.ColourDiffuse = ColourDiffuse;

			_imageNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height, _nonFocusedTextureName);
			_imageNonFocused.ParentControl = this;
			_imageNonFocused.Filtering = false;
			_imageNonFocused.DimColor = DimColor;
			_imageNonFocused.ColourDiffuse = ColourDiffuse;
		}

		/// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
		public override void AllocResources()
		{
			base.AllocResources();
			_imageFocused.AllocResources();
			_imageNonFocused.AllocResources();
		}

		/// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
		public override void FreeResources()
		{
			base.FreeResources();
			_imageFocused.FreeResources();
			_imageNonFocused.FreeResources();
		}

		public override void Render(float timePassed)
		{
			if (Focus) _imageFocused.Render(timePassed);
			else       _imageNonFocused.Render(timePassed);
		}

		public override bool Focus
		{
			get { return base.Focus; }
			set
			{
				if (base.Focus != value)
				{
					if (value == true)
					{
					  if (_imageFocused != null) _imageFocused.Begin();
						GUIWindow win = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ACTIONMENU);
					  if (win != null)
				   	{
					  	Action newAction = new Action(Action.ActionType.ACTION_SHOW_ACTIONMENU, ParentID, 0);
						  newAction.m_SoundFileName = _layoutFile;
						  win.OnAction(newAction);
					  }
					}
					else if (_imageNonFocused != null) _imageNonFocused.Begin();
				}
				base.Focus = value;
			}
		} 


	}
}
