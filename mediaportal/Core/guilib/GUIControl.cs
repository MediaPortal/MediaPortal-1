using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;

using MediaPortal.Layouts;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Base class for GUIControls.
	/// </summary>
	public abstract class GUIControl : ILayoutComponent
	{
		[XMLSkinElement("subtype")]			protected string  m_strSubType = "";
		[XMLSkinElement("onleft")]			protected int			m_dwControlLeft = 0;
		[XMLSkinElement("onright")]			protected int			m_dwControlRight = 0;
		[XMLSkinElement("onup")]			protected int			m_dwControlUp = 0;
		[XMLSkinElement("ondown")]			protected int			m_dwControlDown = 0;
		[XMLSkinElement("posX")]			protected int			m_dwPosX = 0;
		[XMLSkinElement("posY")]			protected int			m_dwPosY = 0;
		[XMLSkinElement("height")]			protected int			m_dwHeight = 0;
		[XMLSkinElement("width")]			protected int			m_dwWidth = 0;
		[XMLSkinElement("colordiffuse")]	protected long			m_colDiffuse = 0xFFFFFFFF;
		[XMLSkinElement("id")]				protected int			m_dwControlID = 0;
		[XMLSkinElement("type")]			protected string		m_strControlType = "";
		[XMLSkinElement("description")]		protected string		m_Description="";
		[XMLSkinElement("visible")]			protected bool			m_bVisible = true;
		protected int			m_dwParentID = 0;
		protected bool			m_bHasFocus = false;
		protected bool			m_bDisabled = false;
		protected bool			m_bSelected = false;
		protected bool			m_bCalibration = true;
		protected object		m_Data;
		protected int			m_iWindowID;
		protected int			m_SelectedItem=0;
		protected ArrayList m_SubItems = new ArrayList();
		protected Rectangle m_originalRect;
		protected bool m_bAnimating=false;
		protected long m_lOriginalColorDiffuse;

		/// <summary>
		/// enum to specify the alignment of the control
		/// </summary>
		public enum Alignment
		{
			ALIGN_LEFT, 
			ALIGN_RIGHT, 
			ALIGN_CENTER
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
			m_dwWidth = dwWidth;
			m_dwHeight = dwHeight;
		}

		/// <summary> 
		/// This function is called after all of the XmlSkinnable fields have been filled
		/// with appropriate data.
		/// Use this to do any construction work other than simple data member assignments,
		/// for example, initializing new reference types, extra calculations, etc..
		/// </summary>
		public virtual void FinalizeConstruction()
		{
			if (m_dwControlUp == 0) m_dwControlUp		= m_dwControlID - 1; 
			if (m_dwControlDown == 0) m_dwControlDown	= m_dwControlID + 1; 
			if (m_dwControlLeft == 0) m_dwControlLeft	= m_dwControlID; 
			if (m_dwControlRight == 0) m_dwControlRight = m_dwControlID; 
		}
			
		/// <summary>
		/// Does any scaling on the inital size\position values to fit them to screen 
		/// resolution. 
		/// </summary>
		public virtual void ScaleToScreenResolution()
		{
			GUIGraphicsContext.ScaleRectToScreenResolution(ref m_dwPosX, ref m_dwPosY, ref m_dwWidth, ref m_dwHeight);
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
			switch (action.wID)
			{
					// Move down action
				case Action.ActionType.ACTION_MOVE_DOWN : 
				{
					// Set the focus on the down control.
					if (Focus)
					{
						Focus = false;
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, m_dwControlDown, (int)action.wID, 0, null);
						GUIWindow window=GUIWindowManager.GetWindow(WindowId);
						if (window!=null) window.OnMessage(msg);
					}
				}
					break;

					// Set the focus on the up control.
				case Action.ActionType.ACTION_MOVE_UP : 
				{
					if (Focus)
					{
						Focus = false;
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, m_dwControlUp, (int)action.wID, 0, null);
						GUIWindow window=GUIWindowManager.GetWindow(WindowId);
						if (window!=null) window.OnMessage(msg);
					}
				}
					break;
		    
					// Set the focus on the left control.
				case Action.ActionType.ACTION_MOVE_LEFT : 
				{
					if (Focus)
					{
						Focus = false;
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, m_dwControlLeft, (int)action.wID, 0, null);
						GUIWindow window=GUIWindowManager.GetWindow(WindowId);
						if (window!=null) window.OnMessage(msg);
					}
				}
					break;

					// Set the focus on the right control.
				case Action.ActionType.ACTION_MOVE_RIGHT : 
				{
					if (Focus)
					{
						Focus = false;
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, m_dwControlRight, (int)action.wID, 0, null);
						GUIWindow window=GUIWindowManager.GetWindow(WindowId);
						if (window!=null) window.OnMessage(msg);
					}
				}
					break;
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
							int dwControl = 0;
							if (message.Param1 == (int)Action.ActionType.ACTION_MOVE_DOWN) dwControl = m_dwControlDown;
							if (message.Param1 == (int)Action.ActionType.ACTION_MOVE_UP) dwControl = m_dwControlUp;
							if (message.Param1 == (int)Action.ActionType.ACTION_MOVE_LEFT) dwControl = m_dwControlLeft;
							if (message.Param1 == (int)Action.ActionType.ACTION_MOVE_RIGHT) dwControl = m_dwControlRight;
							if (dwControl!=GetID)
							{
								GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, WindowId, GetID, dwControl, message.Param1, 0, null);
								GUIWindow window=GUIWindowManager.GetWindow(WindowId);
								if (window!=null) window.OnMessage(msg);
							}
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
						if (IsVisible) return false;
						IsVisible = true;
						return true;
					
		      
					case GUIMessage.MessageType.GUI_MSG_HIDDEN : 
						IsVisible = false;
						return true;
					

					case GUIMessage.MessageType.GUI_MSG_ENABLED : 
						m_bDisabled = false;
						return true;
					
		      
					case GUIMessage.MessageType.GUI_MSG_DISABLED : 
						m_bDisabled = true;
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
		public virtual bool Focus
		{
			get { return m_bHasFocus; }
			set 
			{ 
				m_bHasFocus = value; 
			}
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
			
			if (!IsVisible) return false;
			if (Disabled) return false;
			return true;
		}

		/// <summary>
		/// Gets and sets the visible state of the control.
		/// </summary>
		public virtual bool IsVisible
		{
			get { return m_bVisible; }
			set 
			{
				m_bVisible = value;
			}
		}
			
		/// <summary>
		/// Gets and sets the Disabled property of the control.
		/// </summary>
		public virtual bool Disabled
		{
			get { return m_bDisabled; }
			set { m_bDisabled = value; }
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
			if (m_dwPosX == dwPosX && m_dwPosY == dwPosY) return;
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
			set 
			{ 
				if (m_dwPosX==value) return;
				m_dwPosX = value; 
				if (m_dwPosX<0) m_dwPosX=0;
				Update();
			}
		}

		/// <summary>
		/// Gets and sets the Y position of the control.
		/// </summary>
		public virtual int YPosition
		{
			get { return m_dwPosY; }
			set 
			{ 
				if (m_dwPosY==value) return;
				m_dwPosY = value; 
				if (m_dwPosY<0) m_dwPosY=0;
				Update();
			}
		}

		/// <summary>
		/// Gets and sets the width of the control.
		/// </summary>
		public virtual int Width
		{
			get { return m_dwWidth; }
			set 
			{ 
				if (m_dwWidth==value) return;
				m_dwWidth = value;
				if (m_dwWidth<0) m_dwWidth=0;
				Update();
			}
		}

		/// <summary>
		/// Gets and sets the height of the control.
		/// </summary>
		public virtual int Height
		{
			get { return m_dwHeight; }
			set 
			{ 
				if (m_dwHeight==value) return;
				m_dwHeight = value; 
				if (m_dwHeight<0) m_dwHeight=0;
				Update();
			}
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
			m_originalRect=new Rectangle(m_dwPosX, m_dwPosY, m_dwWidth, m_dwHeight);
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
			m_dwWidth=m_originalRect.Width;
			m_dwHeight=m_originalRect.Height;
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
			width=m_dwWidth;
			height=m_dwHeight;
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
			m_dwWidth=w;
			m_dwHeight=h;
			DoUpdate();
		}

		public string SubType
		{
			get
			{
				return m_strSubType;
			}
		}

		/////////////////////////////////////////////
	  
		#region Methods

		void ILayoutComponent.Measure(Size availableSize)
		{
		}

		void ILayoutComponent.Arrange(Rectangle finalRectangle)
		{
			m_dwPosX = finalRectangle.X;
			m_dwPosY = finalRectangle.Y;
			m_dwWidth = finalRectangle.Width;
			m_dwHeight = finalRectangle.Height;
		}

		#endregion Methods

		#region Properties

		ICollection ILayoutComponent.Children
		{
			get { return EmptyCollection.Instance; }
		}

		Size ILayoutComponent.Size
		{
			get { return new Size(m_dwWidth, m_dwHeight); }
		}

		Rectangle ILayoutComponent.Margins
		{
			get { return Rectangle.Empty; }
		}

		Point ILayoutComponent.Location
		{
			get { return new Point(m_dwPosX, m_dwPosY); }
		}

		#endregion Properties
	}
}
