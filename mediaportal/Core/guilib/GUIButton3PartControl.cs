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
using System.Drawing;
using System.Diagnostics;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class implementing a button which consists of 3 parts
	/// a left part, a middle part and a right part
	/// These are presented as [ Left Middle Right ]
	/// Each part has 2 images, 
	/// 1 for the normal state
	/// and 1 for the focused state
	/// Further the button can have an image (icon) which can be positioned 
	/// 
	/// </summary>
	public class GUIButton3PartControl : GUIControl
	{
		//TODO: make use of GUILabelControl to draw all text
		protected GUIImage               _imageFocusedLeft=null;
    protected GUIImage               _imageNonFocusedLeft=null;  
    protected GUIImage               _imageFocusedMid=null;
    protected GUIImage               _imageNonFocusedMid=null;  
    protected GUIImage               _imageFocusedRight=null;
    protected GUIImage               _imageNonFocusedRight=null;
    protected GUIImage               _imageIcon=null;  
    protected string								 _label1="";
    protected string								 _label2="";
    protected string                 fontName1=String.Empty;
    protected string    						 fontName2=String.Empty;
    protected long  								 _textColor1=(long)0xFFFFFFFF;
    protected long  								 _textColor2=(long)0xFFFFFFFF;
		protected long  								 _disabledColor=(long)0xFF606060;
		protected int                    _hyperLinkWindowId=-1;
    protected int                    _actionId=-1;
		protected string								 _scriptAction="";
    protected int                    _textOffsetX1=10;
    protected int                    _textOffsetY1=2;
    protected int                    _textOffsetX2=10;
    protected int                    _textOffsetY2=2;
    protected string                 _textLabel1;
    protected string                 _textLabel2;  
    protected string                 _application="";
    protected string                 _arguments="";
    protected int                    _iconOffsetX=-1;
    protected int                    _iconOffsetY=-1;
    protected int                    _iconWidth=-1;
    protected int                    _iconHeight=-1;
    protected bool                   _iconKeepAspectRatio=false;
    protected bool                   _iconCentered=false;
    protected bool                   _iconZoomed=false;
    GUILabelControl                  _labelControl1=null;
    GUILabelControl                  _labelControl2=null;
    bool                             containsProperty1=false;
    bool                             containsProperty2=false;
		bool														 renderLeftPart=true;
		bool														 renderRightPart=true;
    //Sprite                           sprite=null;
    
		/// <summary>
		/// The constructor of the GUIButton3PartControl class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strTextureFocus">The filename containing the texture of the butten, when the button has the focus.</param>
		/// <param name="strTextureNoFocus">The filename containing the texture of the butten, when the button does not have the focus.</param>
		public GUIButton3PartControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,  
                                  string strTextureFocusLeft, 
                                  string strTextureFocusMid, 
                                  string strTextureFocusRight, 
                                  string strTextureNoFocusLeft,
                                  string strTextureNoFocusMid,
                                  string strTextureNoFocusRight,
                                  string strTextureIcon)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
      _imageIcon        =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,0, 0, strTextureIcon,0);
			_imageFocusedLeft   =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureFocusLeft,0);
      _imageFocusedMid    =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureFocusMid,0);
      _imageFocusedRight  =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureFocusRight,0);
      _imageNonFocusedLeft =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureNoFocusLeft,0);
      _imageNonFocusedMid  =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureNoFocusMid,0);
      _imageNonFocusedRight=new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight, strTextureNoFocusRight,0);
      _isSelected=false;
      _labelControl1 = new GUILabelControl(dwParentID);
      _labelControl2 = new GUILabelControl(dwParentID);
      _imageIcon.ParentControl = this;
      _imageFocusedLeft.ParentControl = this;
      _imageFocusedMid.ParentControl = this;
      _imageFocusedRight.ParentControl = this;
      _imageNonFocusedLeft.ParentControl = this;
      _imageNonFocusedMid.ParentControl = this;
      _imageNonFocusedRight.ParentControl = this;
      _labelControl1.ParentControl = this;
      _labelControl2.ParentControl = this;
		}
  
		/// <summary>
		/// Renders the GUIButton3PartControl.
		/// </summary>
		public override void Render(float timePassed)
		{
			// Do not render if not visible.
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible ) return;
      }
      _textLabel1=_label1;
      _textLabel2=_label2;
      if (containsProperty1) _textLabel1=GUIPropertyManager.Parse(_label1);
      if (containsProperty2) _textLabel2=GUIPropertyManager.Parse(_label2);

//      sprite.Begin(SpriteFlags.None);
			// if the GUIButton3PartControl has the focus
			if (Focus)
			{
				//render the focused images
				//if (_imageIcon!=null) GUIFontManager.Present();//TODO:not nice. but needed for the tvguide
				if (renderLeftPart) _imageFocusedLeft.Render(timePassed);
				_imageFocusedMid.Render(timePassed);
				if (renderRightPart) _imageFocusedRight.Render(timePassed);
				GUIPropertyManager.SetProperty("#highlightedbutton", _textLabel1);
			}
			else 
			{
				//else render the non-focus images
				//if (_imageIcon!=null) GUIFontManager.Present();//TODO:not nice. but needed for the tvguide
				if (renderLeftPart) _imageNonFocusedLeft.Render(timePassed);  		
				_imageNonFocusedMid.Render(timePassed);  		
				if (renderRightPart) _imageNonFocusedRight.Render(timePassed);
      }

			//render the icon
      if (_imageIcon!=null) 
      {
        _imageIcon.Render(timePassed);
      }
//      sprite.End();

			// render the 1st line of text on the button
      int iWidth=_imageNonFocusedMid.Width-10-_textOffsetX1;
			if (iWidth<=0) iWidth=1;
			if (_imageNonFocusedMid.IsVisible && _textLabel1.Length>0 )
			{
        int widthLeft =(int)((float)_imageFocusedLeft.TextureWidth * ((float)_height/(float)_imageFocusedLeft.TextureHeight));
        int xoff=_textOffsetX1+widthLeft ;

        if (Disabled )
          _labelControl1.TextColor=_disabledColor;
        else
          _labelControl1.TextColor=_textColor1;
        _labelControl1.SetPosition(xoff+_positionX,_textOffsetY1+_positionY);
        _labelControl1.TextAlignment=GUIControl.Alignment.ALIGN_LEFT;
        _labelControl1.FontName=fontName1;
        _labelControl1.Label=_textLabel1;
        _labelControl1.Width=iWidth;
        _labelControl1.Render(timePassed);
			}
     
			// render the 2nd line of text on the button
			if (_imageNonFocusedMid.IsVisible && _textLabel2.Length>0)
      {
        int widthLeft =(int)((float)_imageFocusedLeft.TextureWidth * ((float)_height/(float)_imageFocusedLeft.TextureHeight));
        int xoff=_textOffsetX2+widthLeft;

        if (Disabled )
          _labelControl2.TextColor=_disabledColor;
        else
          _labelControl2.TextColor=_textColor2;
        _labelControl2.SetPosition(xoff+_positionX,_textOffsetY2+_positionY);
        _labelControl2.TextAlignment=GUIControl.Alignment.ALIGN_LEFT;
        _labelControl2.FontName=fontName1;
        _labelControl2.Label=_textLabel2;
        _labelControl2.Width=iWidth-10;
        _labelControl2.Render(timePassed);
      }
		}

		/// <summary>
		/// OnAction() method. This method gets called when there's a new action like a 
		/// keypress or mousemove or... By overriding this method, the control can respond
		/// to any action
		/// </summary>
		/// <param name="action">action : contains the action</param>
		public override void OnAction( Action action) 
		{
			base.OnAction(action);
			GUIMessage message ;
      if (Focus)
      {
				//is the button clicked?
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
					// yes,
					//If this button contains scriptactions call the scriptactions.
          if (_application.Length!=0)
          {
							//button should start an external application, so start it
              Process proc = new Process();
              string strWorkingDir=System.IO.Path.GetFullPath(_application);
              string strFileName=System.IO.Path.GetFileName(_application);
              strWorkingDir=strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length+1) );
              proc.StartInfo.FileName=strFileName;
              proc.StartInfo.WorkingDirectory=strWorkingDir;
              proc.StartInfo.Arguments=_arguments;
              proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
	            proc.StartInfo.CreateNoWindow=true;
              proc.Start();
              //proc.WaitForExit();
          }

					// If this links to another window go to the window.
          if (_hyperLinkWindowId >=0)
          {
						//then switch to the other window
            GUIWindowManager.ActivateWindow((int)_hyperLinkWindowId);
            return;
          }

					// If this button corresponds to an action generate that action.
          if (ActionID >=0)
          {
            Action newaction = new Action((Action.ActionType)ActionID,0,0);
            GUIGraphicsContext.OnAction(newaction);
            return;
          }

          // button selected.
          // send a message to the parent window
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId,GetID, ParentID,0,0,null );
          GUIGraphicsContext.SendMessage(message);
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
			// Handle the GUI_MSG_LABEL_SET message
			if ( message.TargetControlId==GetID )
			{
				if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
				{
          if (message.Label!=null)
          {
            _label1=message.Label;
            containsProperty1=_containsProperty(_label1);
          }
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
			_imageFocusedLeft.PreAllocResources();
      _imageFocusedMid.PreAllocResources();
      _imageFocusedRight.PreAllocResources();
      _imageNonFocusedLeft.PreAllocResources();
      _imageNonFocusedMid.PreAllocResources();
      _imageNonFocusedRight.PreAllocResources();
      _imageIcon.PreAllocResources();
    }
		
		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
		public override void AllocResources()
		{
			base.AllocResources();
			_imageFocusedLeft.AllocResources();
      _imageFocusedMid.AllocResources();
      _imageFocusedRight.AllocResources();
      _imageNonFocusedLeft.AllocResources();
      _imageNonFocusedMid.AllocResources();
      _imageNonFocusedRight.AllocResources();
      _imageIcon.AllocResources();

      _labelControl1.AllocResources();
      _labelControl2.AllocResources();
//      sprite=new Sprite(GUIGraphicsContext.DX9Device);
		}

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
		public override void FreeResources()
		{
			base.FreeResources();
			_imageFocusedLeft.FreeResources();
      _imageFocusedMid.FreeResources();
      _imageFocusedRight.FreeResources();
      _imageNonFocusedLeft.FreeResources();
      _imageNonFocusedMid.FreeResources();
      _imageNonFocusedRight.FreeResources();
      _imageIcon.FreeResources();

      
      _labelControl1.FreeResources();
      _labelControl2.FreeResources();
      /*if (sprite!=null)
      {
        if (!sprite.Disposed) sprite.Dispose();
        sprite=null;
      }*/
    }

		/// <summary>
		/// Get/set the color of the text when the GUIButton3PartControl is disabled.
		/// </summary>
		public long DisabledColor
		{
			get { return _disabledColor;}
			set {_disabledColor=value;}
		}
		
		/// <summary>
		/// Get the filename of the texture when the GUIButton3PartControl does not have the focus.
		/// </summary>
		public string TexutureNoFocusLeftName
		{ 
      get { return _imageNonFocusedLeft.FileName;} 
      set 
      {
        if (value==null) return;
        _imageNonFocusedLeft.SetFileName(value);
      }
		}
    public string TexutureNoFocusMidName
    { 
      get { return _imageNonFocusedMid.FileName;} 
      set 
      {
        if (value==null) return;
        _imageNonFocusedMid.SetFileName(value);
      }
    }
    public string TexutureNoFocusRightName
    { 
      get { return _imageNonFocusedRight.FileName;} 
      set 
      {
        if (value==null) return;
        _imageNonFocusedRight.SetFileName(value);
      }
    }

		/// <summary>
		/// Get the filename of the texture when the GUIButton3PartControl has the focus.
		/// </summary>
		public string TexutureFocusLeftName
		{ 
      get {return _imageFocusedLeft.FileName;} 
      set 
      {
        if (value==null) return;
        _imageFocusedLeft.SetFileName(value);
      }
		}
    public string TexutureFocusMidName
    { 
      get {return _imageFocusedMid.FileName;} 
      set 
      {
        if (value==null) return;
        _imageFocusedMid.SetFileName(value);
      }
    }
    public string TexutureFocusRightName
    { 
      get 
      {
        return _imageFocusedRight.FileName;
      } 
      set { 
        if (value==null) return;
        _imageFocusedRight.SetFileName(value);
      }
    }

		/// <summary>
		/// Get/set the filename of the icon texture
		/// </summary>
    public string TexutureIcon
    {
      get { 
				if (_imageIcon==null) return String.Empty;
				return _imageIcon.FileName;
			}
      set
      {
        if (_imageIcon!=null)
        {
					_imageIcon.SetFileName(value);
					_imageIcon.Width=_iconWidth;
					_imageIcon.Height=_iconHeight;
					Update();
        }
      }
    }
		
		/// <summary>
		/// Set the color of the text on the GUIButton3PartControl. 
		/// </summary>
		public long	TextColor1
		{ 
			get { return _textColor1;}
			set { _textColor1=value;}
		}
    public long	TextColor2
    { 
      get { return _textColor2;}
      set { _textColor2=value;}
    }

		/// <summary>
		/// Get/set the name of the font of the text of the GUIButton3PartControl.
		/// </summary>
		public string FontName1
		{ 
			get { 
        return fontName1;
			}
			set { 
				if (value==null) return;
				fontName1=value;
			}
		}

    public string FontName2
    { 
      get { 
				return fontName2; 
			}
      set { 
				if (value==null) return;
				fontName2=value;
			}
    }

		/// <summary>
		/// Set the text of the GUIButton3PartControl. 
		/// </summary>
		/// <param name="strFontName">The font name.</param>
		/// <param name="strLabel">The text.</param>
		/// <param name="dwColor">The font color.</param>
		public void SetLabel1( string strFontName,string strLabel,long dwColor)
    {
      if (strFontName==null) return;
      if (strLabel==null) return;
			_label1=strLabel;
			_textColor1=dwColor;
      fontName1=strFontName;
      containsProperty1=_containsProperty(_label1);
		}
    public void SetLabel2( string strFontName,string strLabel,long dwColor)
    {
      if (strFontName==null) return;
      if (strLabel==null) return;
      _label2=strLabel;
      _textColor2=dwColor;
      fontName2=strFontName;
      containsProperty2=_containsProperty(_label2);
    }

		/// <summary>
		/// Get/set the text of the GUIButton3PartControl.
		/// </summary>
		public string Label1
		{ 
			get { return _label1; }
      set 
      { 
        if (value==null) return;
        _label1=value;
        containsProperty1=_containsProperty(_label1);
      }
    }
    public string Label2
    { 
      get { return _label2; }
      set 
      { 
        if (value==null) return;
        _label2=value;
        containsProperty2=_containsProperty(_label2);
      }
    }

		/// <summary>
		/// Get/set the window ID to which the GUIButton3PartControl links.
		/// </summary>
		public int HyperLink
		{ 
			get { return _hyperLinkWindowId;}
			set {_hyperLinkWindowId=value;}
		}

		/// <summary>
		/// Get/set the scriptaction that needs to be performed when the button is clicked.
		/// </summary>
		public string ScriptAction  
		{ 
			get { return _scriptAction; }
      set 
      { 
        if (value==null) return;
        _scriptAction=value; 
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
    /// Get/set the X-offset of the label.
    /// </summary>
    public int TextOffsetX1
    {
      get { return _textOffsetX1;}
      set 
      { 
        if (value<0) return;
        _textOffsetX1=value;
      }
    }
    public int TextOffsetX2
    {
      get { return _textOffsetX2;}
      set 
      { 
        if (value<0) return;
        _textOffsetX2=value;
      }
    }
    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY1
    {
      get { return _textOffsetY1;}
      set 
      { 
        if (value<0) return;
        _textOffsetY1=value;
      }
    }
    public int TextOffsetY2
    {
      get { return _textOffsetY2;}
      set { 
        if (value<0) return;
        _textOffsetY2=value;
      }
    }

		/// <summary>
		/// Perform an update after a change has occured. E.g. change to a new position.
		/// </summary>
		protected override void  Update() 
		{
			base.Update();
  
      _imageFocusedLeft.ColourDiffuse=ColourDiffuse;
      _imageFocusedMid.ColourDiffuse=ColourDiffuse;
      _imageFocusedRight.ColourDiffuse=ColourDiffuse;

      _imageNonFocusedLeft.ColourDiffuse=ColourDiffuse;
      _imageNonFocusedMid.ColourDiffuse=ColourDiffuse;
      _imageNonFocusedRight.ColourDiffuse=ColourDiffuse;      
      
			_imageFocusedLeft.Height =_height;
			_imageFocusedMid.Height  =_height;
			_imageFocusedRight.Height=_height;
      
      int width;

      int widthLeft =(int)((float)_imageFocusedLeft.TextureWidth * ((float)_height/(float)_imageFocusedLeft.TextureHeight));
      int widthRight=(int)((float)_imageFocusedRight.TextureWidth * ((float)_height/(float)_imageFocusedRight.TextureHeight));
      int widthMid = _width - widthLeft - widthRight;
      if (widthMid < 0) widthMid=0;

      while(true)
      {
				width=widthLeft+widthRight+widthMid;
        if (width > _width)
        {
          if (widthMid>0) widthMid--;
          else 
          {
            if (widthLeft>0) widthLeft--;
            if (widthRight>0) widthRight--;
          }
        }
        else break;
      } 

			_imageFocusedLeft.Width=widthLeft;
			_imageFocusedMid.Width=widthMid;
			_imageFocusedRight.Width=widthRight;
			if (widthLeft==0) _imageFocusedLeft.IsVisible=false;
			else _imageFocusedLeft.IsVisible=true;
			
			if (widthMid==0) _imageFocusedMid.IsVisible=false;
			else _imageFocusedMid.IsVisible=true;

			if (widthRight==0) _imageFocusedRight.IsVisible=false;
			else _imageFocusedRight.IsVisible=true;

			_imageNonFocusedLeft.Width=widthLeft;
			_imageNonFocusedMid.Width=widthMid;
			_imageNonFocusedRight.Width=widthRight;
			if (widthLeft==0) _imageNonFocusedLeft.IsVisible=false;
			else _imageNonFocusedLeft.IsVisible=true;
			
			if (widthMid==0) _imageNonFocusedMid.IsVisible=false;
			else _imageNonFocusedMid.IsVisible=true;

			if (widthRight==0) _imageNonFocusedRight.IsVisible=false;
			else _imageNonFocusedRight.IsVisible=true;

      _imageFocusedLeft.SetPosition (_positionX, _positionY);
      _imageFocusedMid.SetPosition  (_positionX + widthLeft, _positionY);
      _imageFocusedRight.SetPosition(_positionX + _width - widthRight, _positionY);


      _imageNonFocusedLeft.SetPosition (_positionX, _positionY);
      _imageNonFocusedMid.SetPosition  (_positionX + widthLeft, _positionY);
      _imageNonFocusedRight.SetPosition(_positionX + _width - widthRight, _positionY);



			if (_imageIcon!=null)
			{
        _imageIcon.KeepAspectRatio=_iconKeepAspectRatio;
        _imageIcon.Centered=_iconCentered;
        _imageIcon.Zoom=_iconZoomed;
				if (IconOffsetY<0 || IconOffsetX<0)
				{
          int iWidth=_imageIcon.TextureWidth;
          if (iWidth>=_width)
          {
            _imageIcon.Width=_width;
            iWidth=_width;
          }
          int offset=(iWidth + iWidth/2);
          if (offset > _width) offset=_width;
					_imageIcon.SetPosition(_positionX+(_width)  - offset,
																_positionY+(_height/2) - (_imageIcon.TextureHeight/2) );
				}
				else
				{
					_imageIcon.SetPosition(_positionX+IconOffsetX,_positionY+IconOffsetY );
				}
        
			}
    }

		public void Refresh()
		{
			Update();
		}

    /// <summary>
    /// Get/Set the icon to be zoomed into the dest. rectangle
    /// </summary>
    public bool IconZoom
    {
      get { return _iconZoomed; }
      set { _iconZoomed = value; }
    }
    /// <summary>
    /// Get/Set the icon to keep it's aspectratio in the dest. rectangle
    /// </summary>
    public bool IconKeepAspectRatio
    {
      get { return _iconKeepAspectRatio; }
      set { _iconKeepAspectRatio = value; }
    }
    /// <summary>
    /// Get/Set the icon centered in the dest. rectangle
    /// </summary>
    public bool IconCentered
    {
      get { return _iconCentered; }
      set { _iconCentered = value; }
    }
    /// <summary>
		/// Get/Set the the application filename
		/// which should be launched when this button gets clicked
		/// </summary>
	  public string Application
	  {
	    get { return _application; }
      set 
      { 
        if (_application==null) return;
        _application = value; 
      }
	  }

		/// <summary>
		/// Get/Set the arguments for the application
		/// which should be launched when this button gets clicked
		/// </summary>
	  public string Arguments
	  {
	    get { return _arguments; }
	    set { 
        if (_arguments==null) return;
        _arguments = value; 
      }
	  }
	
		/// <summary>
		/// Get/Set the x-position of the icon
		/// </summary>
    public int IconOffsetX
    {
      get { return _iconOffsetX; }
      set { _iconOffsetX = value; }
    }
	
		/// <summary>
		/// Get/Set the y-position of the icon
		/// </summary>
    public int IconOffsetY
    {
      get { return _iconOffsetY; }
      set { _iconOffsetY= value; }
    }
	
		/// <summary>
		/// Get/Set the width of the icon
		/// </summary>
    public int IconWidth
    {
      get { return _iconWidth; }
      set 
      { 
        _iconWidth= value; 
        if (_imageIcon!=null) _imageIcon.Width=_iconWidth;
      }
    }
		/// <summary>
		/// Get/Set the height of the icon
		/// </summary>
    public int IconHeight
    {
      get { return _iconHeight; }
      set { 
        if (value<0) return;
        _iconHeight= value; 
        if (_imageIcon!=null) _imageIcon.Height=_iconHeight;
      }
    }
    bool _containsProperty(string text)
    {
      if (text==null) return false;
      if (text.IndexOf("#")>=0) return true;
      return false;
    }
		public bool RenderLeft
		{
			get { return renderLeftPart;}
			set {renderLeftPart=value;}
		}
		public bool RenderRight
		{
			get { return renderRightPart;}
			set {renderRightPart=value;}
		}
  }
}
