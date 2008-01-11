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

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class implementing a GUICheckMarkControl.
	/// </summary>
  public class GUICheckMarkControl: GUIControl
  {
		//TODO: make use of GUILabelControl for drawing text
		[XMLSkinElement("textureCheckmarkNoFocus")] protected string	_checkMarkNoFocusTextureName = string.Empty;
		[XMLSkinElement("textureCheckmark")]	protected string	_checkMarkFocusTextureName = string.Empty;
		[XMLSkinElement("MarkWidth")]			protected int		_checkMarkWidth;
		[XMLSkinElement("MarkHeight")]			protected int		_checkMarkHeight;
		[XMLSkinElement("font")]				protected string	_fontName;
		[XMLSkinElement("textcolor")]			protected long  	_textColor=0xFFFFFFFF;
    [XMLSkinElement("label")]				protected string	_label="";
    [XMLSkinElement("disabledcolor")]		protected long		_disabledColor=0xFF606060;
    [XMLSkinElement("align")]				protected Alignment _alignment=Alignment.ALIGN_RIGHT;  
    [XMLSkinElement("shadow")]				protected bool		_shadow=false;
		protected GUIAnimation	_imageCheckMarkFocused=null;
		protected GUIAnimation	_imageCheckMarkNonFocused=null;
		protected GUIFont   _font=null;
		protected Rectangle _rectangle=new Rectangle();
	  public GUICheckMarkControl (int dwParentID) : base(dwParentID)
	  {
	  }
	    /// <summary>
		/// The constructor of the GUICheckMarkControl class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strTextureCheckMark">The filename containing the checked texture.</param>
		/// <param name="strTextureCheckMarkNF">The filename containing the not checked texture.</param>
		/// <param name="dwCheckWidth">The width of the checkmark texture.</param>
		/// <param name="dwCheckHeight">The height of the checkmark texture.</param>
		/// <param name="dwAlign">The alignment of the control.</param>
    public GUICheckMarkControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strTextureCheckMark, string strTextureCheckMarkNF,int dwCheckWidth, int dwCheckHeight,GUIControl.Alignment dwAlign)
    :base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
    {
      _isSelected=false;
      _alignment=dwAlign;
			_checkMarkHeight = dwCheckHeight;
			_checkMarkWidth = dwCheckWidth;
			_checkMarkFocusTextureName = strTextureCheckMark;
			_checkMarkNoFocusTextureName = strTextureCheckMarkNF;
			FinalizeConstruction();
    }

		/// <summary>
		/// This method gets called when the control is created and all properties has been set
		/// It allows the control todo any initialization
		/// </summary>
	  public override void FinalizeConstruction()
	  {
		  base.FinalizeConstruction ();

			_imageCheckMarkFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY,
			   _checkMarkWidth, _checkMarkHeight, _checkMarkFocusTextureName);
      _imageCheckMarkFocused.ParentControl = this;
      _imageCheckMarkFocused.DimColor = DimColor;

			_imageCheckMarkNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY,
			   _checkMarkWidth, _checkMarkHeight, _checkMarkNoFocusTextureName);
      _imageCheckMarkNonFocused.ParentControl = this;
      _imageCheckMarkNonFocused.DimColor = DimColor;
		  
		  if (_fontName!="" && _fontName!="-")
			  _font=GUIFontManager.GetFont(_fontName);
		  
		  GUILocalizeStrings.LocalizeLabel(ref _label);
	  }

		public override bool Focus
		{
			get
			{
				return base.Focus;
			}
			set
			{
				if (value != IsFocused)
				{
					if (value == true)
					{
						if (_imageCheckMarkFocused != null) _imageCheckMarkFocused.Begin();
					}
					else
					{
						if (_imageCheckMarkNonFocused != null) _imageCheckMarkNonFocused.Begin();
					}
				}
				base.Focus = value;
			}

		}

		/// <summary>
		/// Renders the GUICheckMarkControl.
		/// </summary>
    public override void Render(float timePassed)
    {
			// Do not render if not visible.
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }
			if (Focus)
			{	
				GUIPropertyManager.SetProperty("#highlightedbutton", _label);
			}
      int dwTextPosX=_positionX;
      int dwCheckMarkPosX=_positionX;
			_rectangle.X=_positionY;
			_rectangle.Y=_positionY;
			_rectangle.Height=_imageCheckMarkFocused.Height;
      if (null!=_font) 
      {
        if (_alignment==GUIControl.Alignment.ALIGN_LEFT)
        {
					// calculate the position of the checkmark if the text appears at the left side of the checkmark
          float fTextHeight=0,fTextWidth=0;
          _font.GetTextExtent( _label, ref fTextWidth,ref fTextHeight);
          dwCheckMarkPosX += ( (int)(fTextWidth)+5);
					_rectangle.X=_positionX;
					_rectangle.Width=5+(int)fTextWidth+_imageCheckMarkFocused.Width;
					
        }
        else
        {
					// put text at the right side of the checkmark
					dwTextPosX = (dwCheckMarkPosX+_imageCheckMarkFocused.Width +5);

					float fTextHeight=0,fTextWidth=0;
					_font.GetTextExtent( _label, ref fTextWidth,ref fTextHeight);
					_rectangle.X=dwTextPosX;
					_rectangle.Width=(dwTextPosX+(int)fTextWidth+5)-dwTextPosX;
				}
        if (Disabled )
        {
					// If disabled, draw the text in the disabled color.
					_font.DrawText((float)dwTextPosX, (float)_positionY, _disabledColor, _label,GUIControl.Alignment.ALIGN_LEFT,-1);
        }
        else
        {
					// Draw focused text and shadow
          if (Focus)
          {
            if (_shadow)
              _font.DrawShadowText((float)dwTextPosX, (float)_positionY, _textColor, _label,GUIControl.Alignment.ALIGN_LEFT,5,5,0xff000000);
            else
              _font.DrawText((float)dwTextPosX, (float)_positionY, _textColor, _label,GUIControl.Alignment.ALIGN_LEFT,-1);
          }
					// Draw non-focused text and shadow
          else
          {
            if (_shadow)
              _font.DrawShadowText((float)dwTextPosX, (float)_positionY, _disabledColor, _label,GUIControl.Alignment.ALIGN_LEFT,5,5,0xff000000);
            else
              _font.DrawText((float)dwTextPosX, (float)_positionY, _disabledColor, _label,GUIControl.Alignment.ALIGN_LEFT,-1);
          }
        }
      }
			
			// Render the selected checkmark image
      if (_isSelected)
      {
        _imageCheckMarkFocused.SetPosition(dwCheckMarkPosX, _positionY);
        _imageCheckMarkFocused.Render(timePassed);
      }
      else
      {
				// Render the non-selected checkmark image
				_imageCheckMarkNonFocused.SetPosition(dwCheckMarkPosX, _positionY);
        _imageCheckMarkNonFocused.Render(timePassed);
      }
      base.Render(timePassed);
    }

		/// <summary>
		/// OnAction() method. This method gets called when there's a new action like a 
		/// keypress or mousemove or... By overriding this method, the control can respond
		/// to any action
		/// </summary>
		/// <param name="action">action : contains the action</param>
    public override void OnAction(Action action) 
    {
      base.OnAction(action);
      if (Focus)
      {
        if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
					// Send a message that the checkbox was clicked.
					_isSelected=!_isSelected;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID, (int)action.wID,0,null);
          GUIGraphicsContext.SendMessage(msg);
        }
      }
    }

		/// <summary>
		/// OnMessage() This method gets called when there's a new message. 
		/// Controls send messages to notify their parents about their state (changes)
		/// By overriding this method a control can respond to the messages of its controls
		/// </summary>
		/// <param name="message">message : contains the message</param>
		/// <returns>true if the message was handled, false if it wasnt</returns>
		public override bool OnMessage(GUIMessage message) 
    {
      if ( message.TargetControlId==GetID )
      {
				// Set the label.
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label!=null)
			      _label=message.Label;
          return true;
        }
      }
			// Let the base class handle the other messages
			if (base.OnMessage(message)) return true;
      return false;
    }

		/// <summary>
		/// Preallocates the control its DirectX resources.
		/// </summary>
    public override void PreAllocResources() 
    {
      base.PreAllocResources();
      _imageCheckMarkFocused.PreAllocResources();
      _imageCheckMarkNonFocused.PreAllocResources();
    }

		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
    public override void AllocResources() 
    {
      base.AllocResources();
      _imageCheckMarkFocused.AllocResources();
      _imageCheckMarkNonFocused.AllocResources();
      _font=GUIFontManager.GetFont(_fontName);
    }

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
		public override void FreeResources() 
    {
      base.FreeResources();
      _imageCheckMarkFocused.FreeResources();
      _imageCheckMarkNonFocused.FreeResources();
    }

		/// <summary>
		/// Get/set the color of the text when the control is disabled.
		/// </summary>
    public long DisabledColor
    {
      get { return _disabledColor;}
      set {_disabledColor=value;}
    }

		/// <summary>
		/// Set the text of the control. 
		/// </summary>
		/// <param name="strFontName">The font name.</param>
		/// <param name="strLabel">The text.</param>
		/// <param name="dwColor">The font color.</param>
    public void SetLabel(string strFontName,string strLabel,long dwColor)
    {
      if (strFontName==null || strLabel==null) return;
      _label=strLabel;
	    _textColor=dwColor;
      _fontName=strFontName;
	    _font=GUIFontManager.GetFont(_fontName);
    }

		/// <summary>
		/// Set the color of the text on the control. 
		/// </summary>
    public long TextColor 
    { 
      get { return _textColor;}
      set {_textColor=value;}
    }

		/// <summary>
		/// Set the alignment of the text on the control. 
		/// </summary>
    public GUIControl.Alignment TextAlignment 
    { 
      get { return _alignment;}
      set { _alignment=value;}
    }

		/// <summary>
		/// Get/set the name of the font of the text of the control.
		/// </summary>
    public string FontName 
    { 
      get { return _fontName; }
    }
   
		/// <summary>
		/// Get/set the text of the control.
		/// </summary> 
    public string Label 
    { 
      get { return _label; }
      set { 
        if (value==null) return;
        _label=value;
      }
    }

		/// <summary>
		/// Get the width of the texture of the control.
		/// </summary>
    public int CheckMarkWidth
    { 
      get { return _imageCheckMarkFocused.Width; }
    }

		/// <summary>
		/// Get the height of the texture of the control.
		/// </summary>
    public int CheckMarkHeight 
    { 
      get { return _imageCheckMarkFocused.Height ;}
    }
    
		/// <summary>
		/// Get the filename of the checked texture of the control.
		/// </summary>
    public string CheckMarkTextureName
    { 
      get { return _imageCheckMarkFocused.FileName; }
    }
    
		/// <summary>
		/// Get the filename of the not checked texture of the control.
		/// </summary>
    public string CheckMarkTextureNameNF
    { 
      get { return _imageCheckMarkNonFocused.FileName; }
    }

		/// <summary>
		/// Get/set if the control text needs to be rendered with shadow.
		/// </summary>
    public bool Shadow
    {
      get { return _shadow;}
      set { _shadow=value;}
    }

 

		/// <summary>
		/// Method which determines of the coordinate(x,y) is within the current control
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordiate </param>
		/// <param name="controlID">return id of control if coordinate is within control</param>
		/// <returns>true: point is in control
		///          false: point is not within control
		/// </returns>
		public override bool InControl(int x, int y, out int controlID)
		{
			controlID=-1;
			if (x >= _rectangle.Left && x < _rectangle.Right)
			{
				if (y >= _rectangle.Top && y < _rectangle.Bottom)
				{
					controlID=GetID;
					return true;
				}
			}
			return false;
		}

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageCheckMarkFocused != null) _imageCheckMarkFocused.DimColor = value;
        if (_imageCheckMarkNonFocused != null) _imageCheckMarkNonFocused.DimColor = value;
      }
    }


	}
}
