#region Copyright (C) 2005 Media Portal

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

#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Base class for GUIControls.
	/// </summary>
	public abstract class GUIControl : FrameworkElement
	{
		[XMLSkinElement("subtype")]			protected string  m_strSubType = "";
		[XMLSkinElement("onleft")]			protected int			m_dwControlLeft = 0;
		[XMLSkinElement("onright")]			protected int			m_dwControlRight = 0;
		[XMLSkinElement("onup")]			protected int			m_dwControlUp = 0;
		[XMLSkinElement("ondown")]			protected int			m_dwControlDown = 0;
		[XMLSkinElement("colordiffuse")]	protected long			m_colDiffuse = 0xFFFFFFFF;
		[XMLSkinElement("id")]				protected int			m_dwControlID = 0;
		[XMLSkinElement("type")]			protected string		m_strControlType = "";
		[XMLSkinElement("description")]		protected string		m_Description="";
		protected int			m_dwParentID = 0;
		protected bool			m_bSelected = false;
		protected bool			m_bCalibration = true;
		protected object		m_Data;
		protected int			m_iWindowID;
		protected int			m_SelectedItem=0;
		protected ArrayList m_SubItems = new ArrayList();
		protected System.Drawing.Rectangle m_originalRect;
		protected bool m_bAnimating=false;
		protected long m_lOriginalColorDiffuse;

		/// <summary>
		/// enum to specify the alignment of the control
		/// </summary>
		public enum Alignment
		{
			ALIGN_LEFT, 
			ALIGN_RIGHT, 
			ALIGN_CENTER,

			// added to support XAML parser
			Left = ALIGN_LEFT,
			Right = ALIGN_RIGHT,
			Center = ALIGN_CENTER,
		}
		

		public enum eOrientation
		{
			Horizontal,
			Vertical
		};
    
		/// <summary>
		/// empty constructor
		/// </summary>
		public GUIControl()
		{
		}

		/// <summary>
		/// The basic constructur of the GUIControl class.
		/// </summary>
		public GUIControl(int dwParentID)
		{
			m_dwParentID = dwParentID;
		}
		
	
		/// <summary>
		/// The constructor of the GUIControl class.
		/// </summary>
		/// <param name="dwParentID">The id of the parent control.</param>
		/// <param name="dwControlId">The id of this control.</param>
		/// <param name="dwPosX">The X position on the screen of this control.</param>
		/// <param name="dwPosY">The Y position on the screen of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		public GUIControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight)
		{
			m_dwParentID = dwParentID;
			m_dwControlID = dwControlId;
			m_dwPosX = dwPosX;
			m_dwPosY = dwPosY;

			base.Width = dwWidth;
			base.Height = dwHeight;
		}

		/// <summary> 
		/// This function is called after all of the XmlSkinnable fields have been filled
		/// with appropriate data.
		/// Use this to do any construction work other than simple data member assignments,
		/// for example, initializing new reference types, extra calculations, etc..
		/// </summary>
		public virtual void FinalizeConstruction()
		{
			//			if (m_dwControlUp == 0) m_dwControlUp		= m_dwControlID - 1; 
			//			if (m_dwControlDown == 0) m_dwControlDown	= m_dwControlID + 1; 
			//			if (m_dwControlLeft == 0) m_dwControlLeft	= m_dwControlID; 
			//			if (m_dwControlRight == 0) m_dwControlRight = m_dwControlID; 
		}
			
		/// <summary>
		/// Does any scaling on the inital size\position values to fit them to screen 
		/// resolution. 
		/// </summary>
		public virtual void ScaleToScreenResolution()
		{
			int x = m_dwPosX;
			int y = m_dwPosY;
			int w = base.Width;
			int h = base.Height;

			GUIGraphicsContext.ScaleRectToScreenResolution(ref x, ref y, ref w, ref h);

			m_dwPosX = x;
			m_dwPosY = y;
			base.Width = w;
			base.Height = h;
		}

		/// <summary>
		/// The default render method. This needs to be overwritten when inherited to give every control 
		/// its specific look and feel.
		/// </summary>
		public abstract void Render(float timePassed);

		/// <summary>
		/// Property to get/set the id of the window 
		/// to which this control belongs
		/// </summary>
		public virtual int WindowId
		{
			get { return m_iWindowID; }
			set { m_iWindowID = value; }
		}

		/// <summary>
		/// OnAction() method. This method gets called when there's a new action like a 
		/// keypress or mousemove or... By overriding this method, the control can respond
		/// to any action
		/// </summary>
		/// <param name="action">action : contains the action</param>
		public virtual void OnAction(Action action)
		{
			if(Focus == false)
				return;

			switch(action.wID)
			{
				case Action.ActionType.ACTION_MOVE_DOWN:
				case Action.ActionType.ACTION_MOVE_UP: 
				case Action.ActionType.ACTION_MOVE_LEFT: 
				case Action.ActionType.ACTION_MOVE_RIGHT: 
				{
					int controlId = 0;

					switch(action.wID)
					{
						case Action.ActionType.ACTION_MOVE_DOWN:
							controlId = m_dwControlDown;
							break;
						case Action.ActionType.ACTION_MOVE_UP:
							controlId = m_dwControlUp;
							break;
						case Action.ActionType.ACTION_MOVE_LEFT:
							controlId = m_dwControlLeft;
							break;
						case Action.ActionType.ACTION_MOVE_RIGHT:
							controlId = m_dwControlRight;
							break;
					}

					if(controlId == 0)
						controlId = Navigate((Direction)action.wID);

					if(controlId != -1 && controlId != GetID)
						FocusControl(WindowId, controlId, (Direction)action.wID);

					break;
				}
			}
		}

		int Navigate(Direction direction)
		{
			int currentX = this.XPosition;
			int currentY = this.YPosition;

			if(this is GUIListControl)
			{
				System.Drawing.Rectangle rect = ((GUIListControl)this).SelectedRectangle;

				currentX = rect.X;
				currentY = rect.Y;
			}

			int nearestIndex = -1;
			double distanceMin = 10000;
			double bearingMin = 10000;

			foreach(GUIControl control in FlattenHierarchy(GUIWindowManager.GetWindow(WindowId).Children))
			{
				if(control.GetID == GetID)
					continue;

				if(control.CanFocus() == false)
					continue;

				double bearing = CalcBearing(new Point(currentX, currentY), new Point(control.XPosition, control.YPosition));

				if(direction == Direction.Left && (bearing < 215 || bearing > 325))
					continue;

				if(direction == Direction.Right && (bearing < -145 || bearing > -35))
					continue;

				if(direction == Direction.Up && (bearing < -45 || bearing > 45))
					continue;

				if(direction == Direction.Down && !(bearing <= -135 || bearing >= 135))
					continue;
		
				double distance = CalcDistance(new Point(currentX, currentY), new Point(control.XPosition, control.YPosition));

				if(!(distance <= distanceMin && bearing <= bearingMin))
					continue;

				bearingMin = bearing;
				distanceMin = distance;
				nearestIndex = control.GetID;
			}

			return nearestIndex == -1 ? GetID : nearestIndex;
		}
		
		static double CalcBearing(Point p1, Point p2)
		{
			double horzDelta = p2.X - p1.X;
			double vertDelta = p2.Y - p1.Y;

			// arctan gives us the bearing, just need to convert -pi..+pi to 0..360 deg
			double bearing = Math.Round(90 - Math.Atan2(vertDelta, horzDelta) / Math.PI * 180 + 360) % 360;

			// normalize
			bearing = bearing > 180 ? ((bearing + 180) % 360) - 180 : bearing < -180 ? ((bearing - 180) % 360) + 180 : bearing;

			return bearing >= 0 ? bearing - 180 : 180 - bearing;
		}

		static double CalcDistance(Point p2, Point p1)
		{
			double horzDelta = p2.X - p1.X;
			double vertDelta = p2.Y - p1.Y;

			return Math.Round(Math.Sqrt((horzDelta * horzDelta) + (vertDelta * vertDelta)));
		}

		ArrayList FlattenHierarchy(UIElementCollection elements)
		{
			ArrayList targetList = new ArrayList();

			FlattenHierarchy(elements, targetList);

			return targetList;
		}

		void FlattenHierarchy(ICollection collection, ArrayList targetList)
		{
			foreach(GUIControl control in collection)
			{
				if(control.GetID == 1)
					continue;

				if(control is GUIGroup)
				{
					FlattenHierarchy(((GUIGroup)control).Children, targetList);
					continue;
				}

				if(control is GUIFacadeControl)
				{
					GUIFacadeControl facade = (GUIFacadeControl)control;

					switch(facade.View)
					{
						case GUIFacadeControl.ViewMode.AlbumView:
							targetList.Add(facade.AlbumListView);
							break;
						case GUIFacadeControl.ViewMode.Filmstrip:
							targetList.Add(facade.FilmstripView);
							break;
						case GUIFacadeControl.ViewMode.List:
							targetList.Add(facade.ListView);
							break;
						default:
							targetList.Add(facade.ThumbnailView);
							break;
					}

					continue;
				}

				targetList.Add(control);
			}
		}

		/// <summary>
		/// OnMessage() This method gets called when there's a new message. 
		/// Controls send messages to notify their parents about their state (changes)
		/// By overriding this method a control can respond to the messages of its controls
		/// </summary>
		/// <param name="message">message : contains the message</param>
		/// <returns>true if the message was handled, false if it wasnt</returns>
		public virtual bool OnMessage(GUIMessage message)
		{
			if (message.TargetControlId == GetID)
			{
				switch (message.Message)
				{
					case GUIMessage.MessageType.GUI_MSG_SETFOCUS : 

						// if control is disabled then move 2 the next control
						if (Disabled || !IsVisible || !CanFocus())
						{
							int controlId = 0;

							switch((Action.ActionType)message.Param1)
							{
								case Action.ActionType.ACTION_MOVE_DOWN:
									controlId = m_dwControlDown;
									break;
								case Action.ActionType.ACTION_MOVE_UP: 
									controlId = m_dwControlUp;
									break;
								case Action.ActionType.ACTION_MOVE_LEFT: 
									controlId = m_dwControlLeft;
									break;
								case Action.ActionType.ACTION_MOVE_RIGHT: 
									controlId = m_dwControlRight;
									break;
							}

							if(controlId == 0)
								controlId = Navigate((Direction)message.Param1);

							if(controlId != -1 && controlId != GetID)
								FocusControl(WindowId, controlId, (Direction)message.Param1);

							return true;
						}

						Focus = true;
						return true;
					
					case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS : 
					{
						Focus = false;
						return true;
					}
					
					case GUIMessage.MessageType.GUI_MSG_VISIBLE : 
						Visibility = System.Windows.Visibility.Visible;
						return true;
		      
					case GUIMessage.MessageType.GUI_MSG_HIDDEN : 
						Visibility = System.Windows.Visibility.Hidden;
						return true;

					case GUIMessage.MessageType.GUI_MSG_ENABLED : 
						IsEnabled = false;
						return true;
					
		      
					case GUIMessage.MessageType.GUI_MSG_DISABLED : 
						IsEnabled = true;
						return true;
					
					case GUIMessage.MessageType.GUI_MSG_SELECTED : 
						m_bSelected = true;
						return true;
					

					case GUIMessage.MessageType.GUI_MSG_DESELECTED : 
						m_bSelected = false;
						return true;
					
				}    
			}
			return false;

		}
		/// <summary>
		/// Gets the ID of the control.
		/// </summary>
		public virtual int GetID
		{
			get { return m_dwControlID; }
			set { m_dwControlID=value;}
		}

		/// <summary>
		/// Gets the ID of the parent control.
		/// </summary>
		public int ParentID
		{
			get { return m_dwParentID; }
			set { m_dwParentID=value; }
		}

		/// <summary>
		/// Sets and gets the status of the focus of the control.
		/// </summary>
		public new virtual bool Focus
		{
			get { return IsFocused; }
			set { SetValue(IsFocusedProperty, value); }
		}

		/// <summary>
		/// Preallocates the control its DirectX resources.
		/// </summary>
		public virtual void PreAllocResources()
		{
		}

		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
		public virtual void AllocResources()
		{
		}

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
		public virtual void FreeResources()
		{
		}

		/// <summary>
		/// NeedRefresh() can be called to see if the control needs 2 redraw itself or not
		/// some controls (for example the fadelabel) contain scrolling texts and need 2
		/// ne re-rendered constantly
		/// </summary>
		/// <returns>true or false</returns>
		public virtual bool NeedRefresh()
		{
			return false;
		}

		/// <summary>
		/// Checks if the control can focus.
		/// </summary>
		/// <returns>true or false</returns>
		public virtual bool CanFocus()
		{
			return Focusable && IsEnabled && IsVisible;
		}

		/// <summary>
		/// Gets and sets the Disabled property of the control.
		/// </summary>
		public virtual bool Disabled
		{
			get { return !IsEnabled; }
			set { IsEnabled = !value; }
		}

		/// <summary>
		/// Gets and sets the Selected property of the control.
		/// </summary>
		public virtual bool Selected
		{
			get { return m_bSelected; }
			set { m_bSelected = value; }
		}
		
		/// <summary>
		/// Sets the position of the control.
		/// </summary>
		/// <param name="dwPosX">The X position.</param>
		/// <param name="dwPosY">The Y position.</param>
		public virtual void SetPosition(int dwPosX, int dwPosY)
		{
			if(m_dwPosX == dwPosX && m_dwPosY == dwPosY) return;
			m_dwPosX = dwPosX;
			m_dwPosY = dwPosY;
			Update();
		}

		/// <summary>
		/// Changes the alpha transparency component of the colordiffuse.
		/// </summary>
		/// <param name="dwAlpha"></param>
		public virtual void SetAlpha(int dwAlpha)
		{
		}

		/// <summary>
		/// ColourDiffuse allows you to mix a color & a graphics texture.
		/// (E.g., if you have a graphics texture like the button which is blue you can mix it 
		///  with lets say a yellow color diffuse and the end result will b green).
		/// </summary>
		public virtual long ColourDiffuse
		{
			get { return m_colDiffuse; }
			set 
			{
				if (value != m_colDiffuse)
				{
					m_colDiffuse = value;
					Update();
				}
			}
		}

		/// <summary>
		/// Gets and sets the X position of the control.
		/// </summary>
		public virtual int XPosition
		{
			get { return m_dwPosX; }
			set { if(m_dwPosX != value) { m_dwPosX = Math.Max(0, value); Update(); } }
		}

		/// <summary>
		/// Gets and sets the Y position of the control.
		/// </summary>
		public virtual int YPosition
		{
			get { return m_dwPosY; }
			set { if(m_dwPosY != value) { m_dwPosY = Math.Max(0, value); Update(); } }
		}

		public bool Visible
		{
			get { return IsVisible; }
			set { IsVisible = value; }
		}

		/// <summary>
		/// Set the up/down/left/right control
		/// </summary>
		/// <param name="dwUp">The control above this control.</param>
		/// <param name="dwDown">The control under this control.</param>
		/// <param name="dwLeft">The control left to this control.</param>
		/// <param name="dwRight">The control right to this control.</param>
		public virtual void SetNavigation(int dwUp, int dwDown, int dwLeft, int dwRight)
		{
			m_dwControlLeft = dwLeft;
			m_dwControlRight = dwRight;
			m_dwControlUp = dwUp;
			m_dwControlDown = dwDown;
		}

		public virtual int NavigateUp
		{
			get { return m_dwControlUp; }
			set { m_dwControlUp = value; }
		}
		public virtual int NavigateDown
		{
			get { return m_dwControlDown; }
			set { m_dwControlDown = value; }
		}
		public virtual int NavigateLeft
		{
			get { return m_dwControlLeft; }
			set { m_dwControlLeft = value; }
		}
		public virtual int NavigateRight
		{
			get { return m_dwControlRight; }
			set { m_dwControlRight = value; }
		}

		/// <summary>
		/// Gets and sets if the control is in calibration mode
		/// </summary>
		public bool CalibrationEnabled
		{
			get { return m_bCalibration; }
			set { m_bCalibration = value; }
		}

		/// <summary>
		/// Gets and sets the type of the control. E.g. image, label, etc.
		/// </summary>
		public string Type
		{
			get { return m_strControlType; }
			set 
			{ 
				if (m_strControlType ==null) return;
				m_strControlType = value; 
			}
		}

		/// <summary>
		/// Gets and sets the data that is contained within the control. E.g. a TVProgram
		/// </summary>
		public object Data
		{
			get { return m_Data; }
			set { m_Data = value; }
		}

		/// <summary>
		/// Checks if the x and y coordinates correspond to the current control.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <returns>True if the control was hit.</returns>
		public virtual bool HitTest(int x, int y,out int controlID, out bool focused)
		{
			focused=Focus;
			controlID=GetID;
			if (!IsVisible) return false;
			if (Disabled) return false;
			if (CanFocus() == false) return false;
			return InControl(x,y, out controlID);
		}

    
		/// <summary>
		/// Perform an update after a change has occured. E.g. change to a new position.
		/// </summary>
		protected virtual void Update() {}

		/// <summary>
		/// Sends a GUI_MSG_HIDDEN message to a control (Hide a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void HideControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);
		}

		/// <summary>
		/// Sends a GUI_MSG_VISIBLE message to a control (Make a control visible).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void ShowControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}

    
		public static void RefreshControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REFRESH, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}

		/// <summary>
		/// Sends a GUI_MSG_LOSTFOCUS message to a control (Set the focus on a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void UnfocusControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LOSTFOCUS, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}
		/// <summary>
		/// Sends a GUI_MSG_SETFOCUS message to a control (Set the focus on a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void FocusControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}

		public static void FocusControl(int iWindowId, int iControlId, Direction direction)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, iWindowId, 0, iControlId, (int)direction, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);
		}

		/// <summary>
		/// Sends a GUI_MSG_LABEL_SET message to a control (Set the label of a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		/// <param name="strText">The text that needs to be set on the target label.</param>
		public static void SetControlLabel(int iWindowId, int iControlId, string strText)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, iWindowId, 0, iControlId, 0, 0, null);
			msg.Label = strText;
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}

		/// <summary>
		/// Sends a GUI_MSG_LABEL_ADD message to a control (Add a ListItem to a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		/// <param name="item">The item that needs to be added to the ListControl</param>
		public static void AddListItemControl(int iWindowId, int iControlId, GUIListItem item)
		{
			// TODO The AddListItemControl should use another message type for adding Items. (REQUIRES a check of every GUI_MSG_LABEL_ADD!).
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, iWindowId, 0, iControlId, 0, 0, item);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}

		/// <summary>
		/// Sends a GUI_MSG_LABEL_ADD message to a control (Add an ItemLabel to a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		/// <param name="strLabel">The text of the label that needs to be added.</param>
		public static void AddItemLabelControl(int iWindowId, int iControlId, string strLabel)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, iWindowId, 0, iControlId, 0, 0, null);
			msg.Label = strLabel;
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}
		
		/// <summary>
		/// Sends a GUI_MSG_LABEL_RESET message to a control (Clears a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void ClearControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}
		
		/// <summary>
		/// Sends a GUI_MSG_ITEM_SELECT message to a control (Select an item in a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		/// <param name="iItem">The id of the item that is selected on the control.</param>
		public static void SelectItemControl(int iWindowId, int iControlId, int iItem)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, iWindowId, 0, iControlId, iItem, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}
		
		/// <summary>
		/// Sends a GUI_MSG_ITEM_FOCUS message to a control (set item in control to selected state).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		/// <param name="iItem">The id of the item that should have the selected state.</param>
		public static void FocusItemControl(int iWindowId, int iControlId, int iItem)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, iWindowId, 0, iControlId, iItem, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}

		/// <summary>
		/// Sends a GUI_MSG_GET_ITEM message to a control (Gets a GUIListItem based on the lWindowId, iControlId, iItem parameters).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		/// <param name="iItem">The item id of the item that needs to be returned.</param>
		/// <returns>The GUIListItem that corresponds to the lWindowId, iControlId, iItem</returns>
		public static GUIListItem GetListItem(int lWindowId, int iControlId, int iItem)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_ITEM, lWindowId, 0, iControlId, iItem, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(lWindowId);
			if (window!=null) window.OnMessage(msg);

			return msg.Object as GUIListItem;
		}

		/// <summary>
		/// Sends a GUI_MSG_GET_SELECTED_ITEM message to a control (Gets the selected GUIListItem based on the lWindowId, iControlId).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		/// <returns>The GUIListItem that is selected.</returns>
		public static GUIListItem GetSelectedListItem(int lWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM, 0, lWindowId, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(lWindowId);
			if (window!=null) window.OnMessage(msg);

			return msg.Object as GUIListItem;
		}

		/// <summary>
		/// Sends a GUI_MSG_ITEMS message to a control (Gets the number of Items in a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		/// <returns>The number of items in a control.</returns>
		public static int GetItemCount(int lWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEMS, lWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(lWindowId);
			if (window!=null) window.OnMessage(msg);

			return msg.Param1;
		}

		/// <summary>
		/// Sends a GUI_MSG_SELECTED message to a control (Select a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void SelectControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}

		/// <summary>
		/// Sends a GUI_MSG_DESELECTED message to a control (Deselect a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void DeSelectControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}
		
		/// <summary>
		/// Sends a GUI_MSG_DISABLED message to a control (Disables a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void DisableControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLED, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

		}
		
		/// <summary>
		/// Sends a GUI_MSG_ENABLED message to a control (Enables a control).
		/// </summary>
		/// <param name="iWindowId">The SenderId.</param>
		/// <param name="iControlId">The target control.</param>
		public static void EnableControl(int iWindowId, int iControlId)
		{
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLED, iWindowId, 0, iControlId, 0, 0, null);
			GUIWindow window=GUIWindowManager.GetWindow(iWindowId);
			if (window!=null) window.OnMessage(msg);

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
		public virtual bool InControl(int x, int y, out int controlID)
		{
			controlID=-1;
			if (x >= XPosition && x < XPosition + Width)
			{
				if (y >= YPosition && y < YPosition + Height)
				{
					controlID=GetID;
					return true;
				}
			}
			return false;
		}

		public virtual void DoUpdate()
		{
			Update();
		}


		/// <summary>
		/// Add an subitem to a control
		/// </summary>
		/// <param name="obj">subitem</param>
		public void AddSubItem(object obj)
		{
			m_SubItems.Add(obj);
		}
		/// <summary>
		/// Remove an subitem from an control
		/// </summary>
		/// <param name="obj">subitem</param>
		public void RemoveSubItem(object obj)
		{
			m_SubItems.Remove(obj);
		}

		/// <summary>
		/// Remove an subitem from an control
		/// </summary>
		/// <param name="obj">index</param>
		public void RemoveSubItem(int index)
		{
			if (index<=0 || index>= m_SubItems.Count) return;
			m_SubItems.RemoveAt(index);
		}

		/// <summary>
		/// Property to get the # of subitems for the control
		/// </summary>
		public int SubItemCount
		{
			get { return m_SubItems.Count;}
		}

		/// <summary>
		/// Property to get a subitem
		/// </summary>
		/// <param name="index">index</param>
		/// <returns>subitem object</returns>
		public object GetSubItem(int index)
		{
			if (index<0 || index>= m_SubItems.Count) return null;
			return m_SubItems[index];
		}
		/// <summary>
		/// Property to set an subitem
		/// </summary>
		/// <param name="index">index</param>
		/// <param name="o">subitem</param>
		public void SetSubItem(int index, object o)
		{ 
			if (index < 0 || index >= m_SubItems.Count) return;
			m_SubItems[index]=o;
		}

		/// <summary>
		/// Property to get/set the current selected subitem
		/// </summary>
		public virtual int SelectedItem
		{
			get { return m_SelectedItem;}
			set { m_SelectedItem=value;}
		}

		/// <summary>
		/// Property to get the control for a specific control ID
		/// </summary>
		/// <param name="ID">Id of wanted control</param>
		/// <returns>null if not found or
		///          GUIControl if found
		/// </returns>
		public virtual GUIControl GetControlById(int ID)
		{
			if (ID==GetID) return this;
			return null;
		}

		/// <summary>
		/// Virtual method. This method gets called when the control is initialized
		/// and allows it to do any initalization
		/// </summary>
		public virtual void OnInit()
		{
		}
		
		/// <summary>
		/// Virtual method. This method gets called when the control is de-initialized
		/// and allows it to do any de-initalization
		/// </summary>
		public virtual void OnDeInit()
		{
		}
		
		/// <summary>
		/// Description (from xml skin file) for control
		/// </summary>
		public string Description
		{
			get { return m_Description;}
			set 
			{ 
				if (value==null) return;
				m_Description=value;
			}
		}

		/// <summary>
		/// Method to store(save) the current control rectangle
		/// </summary>
		public virtual void StorePosition()
		{
			m_bAnimating=false;
			m_originalRect=new System.Drawing.Rectangle(m_dwPosX, m_dwPosY, base.Width, base.Height);
			m_lOriginalColorDiffuse=m_colDiffuse;
		}

		/// <summary>
		/// Property to determine if control is animating
		/// </summary>
		public bool IsAnimating
		{
			get { return m_bAnimating;}
		}

		/// <summary>
		/// Method to restore the saved-current control rectangle
		/// </summary>
		public virtual void ReStorePosition()
		{
			m_dwPosX=m_originalRect.X;
			m_dwPosY=m_originalRect.Y;
			base.Width = m_originalRect.Width;
			base.Height = m_originalRect.Height;
			m_colDiffuse=m_lOriginalColorDiffuse;
			Update();
			m_bAnimating=false;
		}
		
		/// <summary>
		/// Method to get the rectangle of the current control 
		/// </summary>
		public virtual void GetRect(out int x, out int y, out int width, out int height)
		{
			x=m_dwPosX;
			y=m_dwPosY;
			width=base.Width;
			height=base.Height;
		}

		/// <summary>
		/// Method to get animate the current control
		/// </summary>
		public virtual void Animate(float timePassed,Animator animator)
		{
			m_bAnimating=true;
			int x=m_originalRect.X;
			int y=m_originalRect.Y;
			int w=m_originalRect.Width;
			int h=m_originalRect.Height;
			long color=m_colDiffuse;
			animator.Animate( timePassed, ref x, ref y, ref w, ref h, ref color);

			m_colDiffuse=color;
			m_dwPosX=x;
			m_dwPosY=y;
			base.Width=w;
			base.Height=h;
			DoUpdate();
		}

		public string SubType
		{
			get
			{
				return m_strSubType;
			}
		}

		[XMLSkinElement("width")]
		public int m_dwWidth
		{
			get { return base.Width; }
			set { base.Width = value; }
		}

		[XMLSkinElement("height")]
		public int m_dwHeight
		{
			get { return base.Height; }
			set { base.Height = value; }
		}

		[XMLSkinElement("posX")]
		public int m_dwPosX
		{
			get { return (int)base.Location.X; }
			set { base.Location = new Point(value, base.Location.Y); }
		}

		[XMLSkinElement("posY")]
		public int m_dwPosY
		{
			get { return (int)base.Location.Y; }
			set { base.Location = new Point(base.Location.X, value); }
		}

		/////////////////////////////////////////////

		#region Enums

		public enum Direction
		{
			None = 0,
			Up = Action.ActionType.ACTION_MOVE_UP,
			Down = Action.ActionType.ACTION_MOVE_DOWN,
			Left = Action.ActionType.ACTION_MOVE_LEFT,
			Right = Action.ActionType.ACTION_MOVE_RIGHT,
		}

		#endregion Enums

		#region Methods

		protected override Size ArrangeOverride(Rect finalRect)
		{
			Size size = base.ArrangeOverride(finalRect);

			Update();

			return size;
		}

		#endregion Methods

		#region Properties

		public override int Width
		{
			get { return base.Width; }
			set { if(base.Width != value) { base.Width = Math.Max(0, value); Update(); } }
		}

		public override int Height
		{
			get { return base.Height; }
			set { if(base.Height != value) { base.Height = Math.Max(0, value); Update(); } }
		}

		public override double Opacity
		{
			get { return 255.0 / System.Drawing.Color.FromArgb((int)m_colDiffuse).A; }
			set { m_colDiffuse = System.Drawing.Color.FromArgb((int)(255 * value), System.Drawing.Color.FromArgb((int)m_colDiffuse)).ToArgb(); }
		}

		public Size Size
		{
			get { return new Size(base.Width, base.Height); }
			set { base.Width = (int)value.Width; base.Height = (int)value.Height; Update(); }
		}

		#endregion Properties
	}
}