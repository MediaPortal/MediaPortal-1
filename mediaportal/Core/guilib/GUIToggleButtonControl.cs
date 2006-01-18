/* 
 *	Copyright (C) 2005 Team MediaPortal
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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// 
  /// </summary>
	public class GUIToggleButtonControl : GUIControl
	{
		[XMLSkinElement("textureFocus")]	protected string	_focusedTextureName="";
		[XMLSkinElement("textureNoFocus")]	protected string	_nonFocusedTextureName="";
		[XMLSkinElement("AltTextureFocus")]	protected string	m_strImgAltFocusTexture="";
		[XMLSkinElement("AltTextureNoFocus")]	
											protected string m_strImgAltNoFocusTexture="";
		protected GUIImage                _imageFocused=null;
		protected GUIImage                _imageNonFocused=null;  
		protected GUIImage                m_imgAltFocus=null;
		protected GUIImage                m_imgAltNoFocus=null;  
		protected int                    _frameCounter=0;
		[XMLSkinElement("font")]			protected string	_fontName;
		[XMLSkinElement("label")]			protected string	_label="";
		protected GUIFont 								_font=null;
		[XMLSkinElement("textcolor")]		protected long  	_textColor=0xFFFFFFFF;
		[XMLSkinElement("disabledcolor")]	protected long		_disabledColor=0xFF606060;
		[XMLSkinElement("hyperlink")]		protected int       _hyperLinkWindowId=-1;
		
		protected string										_scriptAction="";
		[XMLSkinElement("textXOff")]		protected int       _textOffsetX=0;
		[XMLSkinElement("textYOff")]		protected int       _textOffsetY=0;
	
		public GUIToggleButtonControl(int dwParentID) : base(dwParentID)
		{
		}
		public GUIToggleButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,  string strTextureFocus, string strTextureNoFocus,  string strAltTextureFocus, string strAltTextureNoFocus)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
			_focusedTextureName = strTextureFocus;
			_nonFocusedTextureName = strTextureNoFocus;
			m_strImgAltFocusTexture = strAltTextureFocus;
			m_strImgAltNoFocusTexture = strAltTextureNoFocus;
			_isSelected=false;
			FinalizeConstruction();
		}
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
			
			_imageFocused     =new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height, _focusedTextureName ,0);
      _imageFocused.ParentControl = this;

			_imageNonFocused   =new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height, _nonFocusedTextureName,0);
      _imageNonFocused.ParentControl = this;

			m_imgAltFocus  =new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height, m_strImgAltFocusTexture,0);
      m_imgAltFocus.ParentControl = this;

			m_imgAltNoFocus=new GUIImage(_parentControlId, _controlId, _positionX, _positionY,_width, _height, m_strImgAltNoFocusTexture,0);
      m_imgAltNoFocus.ParentControl = this;
			if (_fontName!="" && _fontName!="-")
				_font=GUIFontManager.GetFont(_fontName);
			GUILocalizeStrings.LocalizeLabel(ref _label);
		}

		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution ();
			GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX, ref _textOffsetY);
		}

    public override void Render(float timePassed)
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible ) return;
      }

      if (Focus)
      {
        int dwAlphaCounter = _frameCounter+2;
        int dwAlphaChannel;
        if ((dwAlphaCounter%128)>=64)
          dwAlphaChannel = dwAlphaCounter%64;
        else
          dwAlphaChannel = 63-(dwAlphaCounter%64);

        dwAlphaChannel += 192;
        SetAlpha(dwAlphaChannel );
        if (_isSelected)
          _imageFocused.Render(timePassed);
        else
          m_imgAltFocus.Render(timePassed);
        _frameCounter++;
      }
      else 
      {
        SetAlpha(0xff);
        if (_isSelected)
          _imageNonFocused.Render(timePassed);
        else
          m_imgAltNoFocus.Render(timePassed);  
      }

      if (_label.Length > 0 && _font!=null)
      {
        if (Disabled )
          _font.DrawText((float)_textOffsetX+_positionX, (float)_textOffsetY+_positionY,_disabledColor,_label,GUIControl.Alignment.ALIGN_LEFT,-1);
        else
          _font.DrawText((float)_textOffsetX+_positionX, (float)_textOffsetY+_positionY,_textColor,_label,GUIControl.Alignment.ALIGN_LEFT,-1);
      }
    }

    public override void OnAction( Action action) 
    {
      base.OnAction(action);
      GUIMessage message;
      if (Focus)
      {
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          _isSelected=!_isSelected;
          if (_hyperLinkWindowId >=0)
          {
            GUIWindowManager.ActivateWindow(_hyperLinkWindowId);
            return;
          }
          // button selected.
          // send a message
          int iParam=1;
          if (!_isSelected) iParam=0;
          message=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID,iParam,0,null );
          GUIGraphicsContext.SendMessage(message);
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      if ( message.TargetControlId==GetID )
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          _label=message.Label ;

          return true;
        }
      }
      if (base.OnMessage(message)) return true;
      return false;
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageFocused.PreAllocResources();
      _imageNonFocused.PreAllocResources();
      m_imgAltFocus.PreAllocResources();
      m_imgAltNoFocus.PreAllocResources();
    }
    public override void AllocResources()
    {
      base.AllocResources();
      _font=GUIFontManager.GetFont(_fontName);
      _frameCounter=0;
      _imageFocused.AllocResources();
      _imageNonFocused.AllocResources();
      m_imgAltFocus.AllocResources();
      m_imgAltNoFocus.AllocResources();
      _width=_imageFocused.Width;
      _height=_imageFocused.Height;
    }
    public override void FreeResources()
    {
      base.FreeResources();
      _imageFocused.FreeResources();
      _imageNonFocused.FreeResources();
      m_imgAltFocus.FreeResources();
      m_imgAltNoFocus.FreeResources();
    }
    public override void SetPosition(int dwPosX, int dwPosY)
    {
      base.SetPosition(dwPosX, dwPosY);
      _imageFocused.SetPosition(dwPosX, dwPosY);
      _imageNonFocused.SetPosition(dwPosX, dwPosY);
      m_imgAltFocus.SetPosition(dwPosX, dwPosY);
      m_imgAltNoFocus.SetPosition(dwPosX, dwPosY);
    }
    public override void SetAlpha(int dwAlpha)
    {
      base.SetAlpha(dwAlpha);
      _imageFocused.SetAlpha(dwAlpha);
      _imageNonFocused.SetAlpha(dwAlpha);
      m_imgAltFocus.SetAlpha(dwAlpha);
      m_imgAltNoFocus.SetAlpha(dwAlpha);
    }

    public long DisabledColor
    {
      get { return _disabledColor;}
      set {_disabledColor=value;}
    }
    public string TexutureNoFocusName
    { 
      get { return _imageNonFocused.FileName;} 
    }

    public string TexutureFocusName
    { 
      get {return _imageFocused.FileName;} 
    }
    public string AltTexutureNoFocusName
    { 
      get { return m_imgAltNoFocus.FileName;} 
    }

    public string AltTexutureFocusName
    { 
      get {return m_imgAltFocus.FileName;} 
    }
		
    public long	TextColor 
    { 
      get { return _textColor;}
      set { _textColor=value;}
    }

    public string FontName
    { 
      get { return _fontName; }
      set { 
        if (value==null) return;
        _fontName=value;
        _font=GUIFontManager.GetFont(_fontName);
      }
    }

    public void SetLabel( string strFontName,string strLabel,long dwColor)
    {
      if (strFontName==null) return;
      if (strLabel==null) return;
      _label=strLabel;
      _textColor=dwColor;
      if (strFontName!="" && strFontName!="-")
      {
        _fontName=strFontName;
        _font=GUIFontManager.GetFont(_fontName);
      }
    }

    public string Label
    { 
      get { return _label; }
      set { _label=value;}
    }

    public int HyperLink
    { 
      get { return _hyperLinkWindowId;}
      set {_hyperLinkWindowId=value;}
    }
    public string ScriptAction  
    { 
      get { return _scriptAction; }
      set { _scriptAction=value; }
    }

    protected override void  Update() 
    {
      base.Update();
  
      _imageFocused.Width=_width;
      _imageFocused.Height=_height;

      _imageNonFocused.Width=_width;
      _imageNonFocused.Height=_height;
      
      m_imgAltFocus.Width=_width;
      m_imgAltFocus.Height=_height;

      m_imgAltNoFocus.Width=_width;
      m_imgAltNoFocus.Height=_height;

      _imageFocused.SetPosition(_positionX, _positionY);
      _imageNonFocused.SetPosition(_positionX, _positionY);
      m_imgAltFocus.SetPosition(_positionX, _positionY);
      m_imgAltNoFocus.SetPosition(_positionX, _positionY);

    }
		/// <summary>
		/// Get/set the X-offset of the label.
		/// </summary>
		public int TextOffsetX
		{
			get { return _textOffsetX;}
			set { _textOffsetX=value;}
		}
		/// <summary>
		/// Get/set the Y-offset of the label.
		/// </summary>
		public int TextOffsetY
		{
			get { return _textOffsetY;}
			set { _textOffsetY=value;}
		}


  }
}
