/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System.Windows.Controls;


namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class implementing a GUIButton.
	/// </summary>
	public class GUIButtonControl : GUIControl
	{
		[XMLSkinElement("textureFocus")]	protected string	m_strImgFocusTexture="";
		[XMLSkinElement("textureNoFocus")]	protected string	m_strImgNoFocusTexture="";
		[XMLSkinElement("font")]			protected string	m_strFontName;
		[XMLSkinElement("label")]			protected string	m_strLabel="";
		[XMLSkinElement("textcolor")]		protected long  	m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("disabledcolor")]	protected long  m_dwDisabledColor=0xFF606060;
		[XMLSkinElement("hyperlink")]		protected int       m_lHyperLinkWindowID=-1;
		[XMLSkinElement("action")]			protected int       m_iAction=-1;
		[XMLSkinElement("script")]			protected string	m_strScriptAction="";
		[XMLSkinElement("textXOff")]		protected int       m_iTextOffsetX=0;
		[XMLSkinElement("textYOff")]		protected int       m_iTextOffsetY=0;
		[XMLSkinElement("textalign")]		protected GUIControl.Alignment       _textAlignment=GUIControl.Alignment.ALIGN_LEFT;
		[XMLSkinElement("application")]		protected string    m_strApplication="";
		[XMLSkinElement("arguments")]		protected string    m_strArguments="";

		[XMLSkinElement("hover")]			
		protected string					_hoverFilename = string.Empty;

		[XMLSkinElement("hoverX")]			
		protected int						_hoverX;

		[XMLSkinElement("hoverY")]
		protected int						_hoverY;

		[XMLSkinElement("hoverWidth")]
		protected int						_hoverWidth;

		[XMLSkinElement("hoverHeight")]
		protected int						_hoverHeight;

		protected GUIImage					_hoverImage; 

		protected int       m_dwFrameCounter=0;
		protected GUIImage	m_imgFocus=null;
		protected GUIImage  m_imgNoFocus=null; 
		protected GUILabelControl     m_label=null;

		public GUIButtonControl(int dwParentID) : base(dwParentID)
		{
		}
		
		/// <summary>
		/// The constructor of the GUIButtonControl class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strTextureFocus">The filename containing the texture of the butten, when the button has the focus.</param>
		/// <param name="strTextureNoFocus">The filename containing the texture of the butten, when the button does not have the focus.</param>
		public GUIButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,  string strTextureFocus, string strTextureNoFocus)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
			m_strImgFocusTexture = strTextureFocus;
			m_strImgNoFocusTexture = strTextureNoFocus;
			FinalizeConstruction();
		}

		/// <summary>
		/// This method gets called when the control is created and all properties has been set
		/// It allows the control todo any initialization
		/// </summary>
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction();
			m_imgFocus  = new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,
				m_dwWidth, m_dwHeight, m_strImgFocusTexture,0);
			
			m_imgNoFocus= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,
				m_dwWidth, m_dwHeight, m_strImgNoFocusTexture,0);

			if(_hoverFilename != string.Empty)
			{
				_hoverImage = new GUIImage(m_dwParentID, m_dwControlID, _hoverX, _hoverY, _hoverWidth, _hoverHeight, _hoverFilename, 0);
				_hoverImage.Filtering = false;	
			}
			
			m_imgFocus.Filtering=false;
			m_imgNoFocus.Filtering=false;
			GUILocalizeStrings.LocalizeLabel(ref m_strLabel);
			m_label = new GUILabelControl(m_dwParentID,0,m_dwPosX,m_dwPosY,m_dwWidth, m_dwHeight,m_strFontName,m_strLabel,m_dwTextColor,GUIControl.Alignment.ALIGN_LEFT,false);
			m_label.TextAlignment = _textAlignment;
		}

		/// <summary>
		/// This method gets called when the control is created and all properties has been set
		/// It allows the control to scale itself to the current screen resolution
		/// </summary>
		public override void ScaleToScreenResolution()
		{
			base.ScaleToScreenResolution();
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iTextOffsetX, ref m_iTextOffsetY);
		}

		public override bool Focus
		{
			get
			{
				return IsFocused;
			}
			set
			{
				if (value != base.Focus && value)
				{
					GUIPropertyManager.SetProperty("#highlightedbutton", Label);
				}

				base.Focus = value;
			}
		}

		/// <summary>
		/// Renders the GUIButtonControl.
		/// </summary>
		public override void Render(float timePassed)
		{
			// Do not render if not visible.
			if (GUIGraphicsContext.EditMode==false)
			{
				if (!IsVisible ) return;
			}

			// The GUIButtonControl has the focus
			if (Focus)
			{
				//render the focused image
				m_imgFocus.Render(timePassed);

				if(_hoverImage != null)
					_hoverImage.Render(timePassed);
			}
			else 
			{
				//render the non-focused image
				m_imgNoFocus.Render(timePassed);  		
			}

			m_label.TextAlignment = _textAlignment;
			m_label.Label=m_strLabel;
			m_label.TextColor= Disabled ? m_dwDisabledColor : m_dwTextColor;

			// render the text on the button
			int x = 0;

			switch(_textAlignment)
			{
				case Alignment.ALIGN_LEFT:
					x = m_iTextOffsetX + m_dwPosX;
					break;

				case Alignment.ALIGN_RIGHT:
					x = m_dwPosX + m_dwWidth - m_iTextOffsetY;
					break;
			}

			m_label.SetPosition(x, m_iTextOffsetY + m_dwPosY);
			m_label.Render(timePassed);
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
				if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
				{
//					if(base.ContextMenu != null)
//						DoContextMenu();

					// If this button contains scriptactions call the scriptactions.
					if (m_strApplication.Length!=0)
					{
						//button should start an external application, so start it
						Process proc = new Process();

						string strWorkingDir=System.IO.Path.GetFullPath(m_strApplication);
						string strFileName=System.IO.Path.GetFileName(m_strApplication);
						strWorkingDir=strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length+1) );
						proc.StartInfo.FileName=strFileName;
						proc.StartInfo.WorkingDirectory=strWorkingDir;
						proc.StartInfo.Arguments=m_strArguments;
						proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
						proc.StartInfo.CreateNoWindow=true;
						proc.Start();
						//proc.WaitForExit();
					}

					// If this links to another window go to the window.
					if (m_lHyperLinkWindowID >=0)
					{
						GUIWindowManager.ActivateWindow((int)m_lHyperLinkWindowID);
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
					if (SubItemCount>0)
					{
						// if we got subitems, then change the label of the control to the next
						//subitem
						SelectedItem++;
						if (SelectedItem >= SubItemCount) SelectedItem=0;
						Label=(string)GetSubItem(SelectedItem);
					}

					// send a message to anyone interested 
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
						Label=message.Label;
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
			m_imgFocus.PreAllocResources();
			m_imgNoFocus.PreAllocResources();

			if(_hoverImage != null)
				_hoverImage.PreAllocResources();
      
		}
		
		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
		public override void AllocResources()
		{
			base.AllocResources();
			m_dwFrameCounter=0;
			m_imgFocus.AllocResources();
			m_imgNoFocus.AllocResources();

			if(_hoverImage != null)
				_hoverImage.AllocResources();

			m_dwWidth=m_imgFocus.Width;
			m_dwHeight=m_imgFocus.Height;
      
			if (SubItemCount>0)
			{
				Label=(string)GetSubItem(SelectedItem);
			}
			m_label.Width=m_dwWidth;
			m_label.Height=m_dwHeight;
			m_label.AllocResources();
		}

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
		public override void FreeResources()
		{
			base.FreeResources();
			m_imgFocus.FreeResources();
			m_imgNoFocus.FreeResources();
			m_label.FreeResources();

			if(_hoverImage != null)
				_hoverImage.FreeResources();
		}

		/// <summary>
		/// Sets the position of the control.
		/// </summary>
		/// <param name="dwPosX">The X position.</param>
		/// <param name="dwPosY">The Y position.</param>		
		public override void SetPosition(int dwPosX, int dwPosY)
		{
			base.SetPosition(dwPosX, dwPosY);
			m_imgFocus.SetPosition(dwPosX, dwPosY);
			m_imgNoFocus.SetPosition(dwPosX, dwPosY);
		}

		/// <summary>
		/// Changes the alpha transparency component of the colordiffuse.
		/// </summary>
		/// <param name="dwAlpha">The new value of the colordiffuse.</param>
		public override void SetAlpha(int dwAlpha)
		{
			base.SetAlpha(dwAlpha);
			m_imgFocus.SetAlpha(dwAlpha);
			m_imgNoFocus.SetAlpha(dwAlpha);

			if(_hoverImage != null)
				_hoverImage.SetAlpha(dwAlpha);
		}

		/// <summary>
		/// Get/set the color of the text when the GUIButtonControl is disabled.
		/// </summary>
		public long DisabledColor
		{
			get { return m_dwDisabledColor;}
			set {m_dwDisabledColor=value;}
		}
		
		/// <summary>
		/// Get the filename of the texture when the GUIButtonControl does not have the focus.
		/// </summary>
		public string TexutureNoFocusName
		{ 
			get { return m_imgNoFocus.FileName;} 
		}

		/// <summary>
		/// Get the filename of the texture when the GUIButtonControl has the focus.
		/// </summary>
		public string TexutureFocusName
		{ 
			get {return m_imgFocus.FileName;} 
		}
		
		/// <summary>
		/// Set the color of the text on the GUIButtonControl. 
		/// </summary>
		public long	TextColor 
		{ 
			get { return m_dwTextColor;}
			set { m_dwTextColor=value;}
		}

		/// <summary>
		/// Get/set the name of the font of the text of the GUIButtonControl.
		/// </summary>
		public string FontName
		{ 
			get { return m_strFontName; }
			set 
			{ 
				if (value==null) return;
				m_strFontName=value;
				m_label.FontName=m_strFontName;
			}
		}

		/// <summary>
		/// Set the text of the GUIButtonControl. 
		/// </summary>
		/// <param name="strFontName">The font name.</param>
		/// <param name="strLabel">The text.</param>
		/// <param name="dwColor">The font color.</param>
		public void SetLabel( string strFontName,string strLabel,long dwColor)
		{
			if (strFontName==null) return;
			if (strLabel==null) return;
			Label=strLabel;
			m_dwTextColor=dwColor;
			m_strFontName=strFontName;
      
			m_label.FontName=m_strFontName;
			m_label.TextColor=dwColor;
			m_label.Label=strLabel;
		}

		/// <summary>
		/// Get/set the text of the GUIButtonControl.
		/// </summary>
		public string Label
		{ 
			get { return m_strLabel; }
			set 
			{
				if (value==null) return;
             
				m_strLabel=value;
				m_label.Label=m_strLabel;
			}
		}

		/// <summary>
		/// Get/set the window ID to which the GUIButtonControl links.
		/// </summary>
		public int HyperLink
		{ 
			get { return m_lHyperLinkWindowID;}
			set {m_lHyperLinkWindowID=value;}
		}

		/// <summary>
		/// Get/set the scriptaction that needs to be performed when the button is clicked.
		/// </summary>
		public string ScriptAction  
		{ 
			get { return m_strScriptAction; }
			set 
			{ 
				if (value==null) return;
				m_strScriptAction=value; 
			}
		}

		/// <summary>
		/// Get/set the action ID that corresponds to this button.
		/// </summary>
		public int ActionID
		{
			get { return m_iAction;}
			set { m_iAction=value;}

		}

		/// <summary>
		/// Get/set the X-offset of the label.
		/// </summary>
		public int TextOffsetX
		{
			get { return m_iTextOffsetX;}
			set 
			{ 
				if (value<0) return;
				m_iTextOffsetX=value;
			}
		}
		/// <summary>
		/// Get/set the Y-offset of the label.
		/// </summary>
		public int TextOffsetY
		{
			get { return m_iTextOffsetY;}
			set 
			{ 
				if (value<0) return;
				m_iTextOffsetY=value;
			}
		}

		public GUIControl.Alignment TextAlignment
		{
			get { return _textAlignment; }
			set { _textAlignment = value; }
		}

		/// <summary>
		/// Perform an update after a change has occured. E.g. change to a new position.
		/// </summary>
		protected override void  Update() 
		{
			base.Update();
  
			m_imgFocus.ColourDiffuse=ColourDiffuse;
			m_imgFocus.Width=m_dwWidth;
			m_imgFocus.Height=m_dwHeight;

			m_imgNoFocus.ColourDiffuse=ColourDiffuse;
			m_imgNoFocus.Width=m_dwWidth;
			m_imgNoFocus.Height=m_dwHeight;
      
			m_imgFocus.SetPosition(m_dwPosX, m_dwPosY);
			m_imgNoFocus.SetPosition(m_dwPosX, m_dwPosY);
		}

		public void Refresh()
		{
			Update();
		}

		/// <summary>
		/// Get/Set the the application filename
		/// which should be launched when this button gets clicked
		/// </summary>
		public string Application
		{
			get { return m_strApplication; }
			set 
			{ 
				if (value==null) return;
				m_strApplication = value; 
			}
		}

		/// <summary>
		/// Get/Set the arguments for the application
		/// which should be launched when this button gets clicked
		/// </summary>
		public string Arguments
		{
			get { return m_strArguments; }
			set 
			{ 
				if (value==null) return;
				m_strArguments = value; 
			}
		}

		/// <summary>
		/// get/set the current selected item
		/// A button can have 1 or more subitems
		/// each subitem has its own text to render on the button
		/// When the user presses the button, the next item will be selected
		/// and shown on the button
		/// </summary>
		public override int SelectedItem
		{
			get { return m_SelectedItem;}
			set 
			{
				if (value<0) return;
				if (SubItemCount>0)
				{
					m_SelectedItem=value;
					if (m_SelectedItem<0 || m_SelectedItem >= SubItemCount) m_SelectedItem=0;
					Label=(string)GetSubItem(m_SelectedItem);
				}
				else m_SelectedItem=0;
			}
		}

/*		void DoContextMenu()
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);

			if(dlg==null)
				return;

			dlg.Reset();
			dlg.SetHeading(924); // menu

			foreach(object item in ContextMenu.Items)
			{
				if(item is MenuItem)
					dlg.AddLocalizedString(((MenuItem)item).Header);
			}

			dlg.DoModal(GetID);

			if(dlg.SelectedId==-1)
				return;

			switch(dlg.SelectedId)
			{
			case 117: // Delete
					OnDelete();
					break;

				case 735: // Rotate					
					DoRotate();
					break;
				
				case 108: // Start slideshow
					StartSlideShow();
					break;

				case 940: // Properties
					OnShowInfo();
					break;

				case 970:
					ShowPreviousWindow();
					break;
			}
		}	
*/	}
}
