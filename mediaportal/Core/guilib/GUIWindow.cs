using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// base class for every window. It contains all methods needed for basic window management like
	/// - initialization
	/// - deitialization
	/// - render itself onscreen
	/// - processing actions like keypresses, mouse clicks/movements
	/// - processing messages
	/// 
	/// Each window plugin should derive from this base class
	/// Pluginwindows should be copied in the plugins/windows folder
	/// </summary>
	public class GUIWindow 
	{
		//enum of all standard windows in MP
		public enum Window
		{
			WINDOW_INVALID = -1
			,WINDOW_HOME = 0 
			,WINDOW_TV = 1 
			,WINDOW_PICTURES = 2 
			,WINDOW_FILES = 3 
			,WINDOW_SETTINGS = 4 
			,WINDOW_MUSIC = 5 
			,WINDOW_VIDEOS = 6 
			,WINDOW_SYSTEM_INFORMATION = 7
			,WINDOW_SETTINGS_GENERAL = 8
			,WINDOW_SETTINGS_SCREEN = 9
			,WINDOW_UI_CALIBRATION = 10
			,WINDOW_MOVIE_CALIBRATION = 11
			,WINDOW_SETTINGS_SLIDESHOW = 12
			,WINDOW_SETTINGS_FILTER = 13
			,WINDOW_SETTINGS_MUSIC = 14
			,WINDOW_SETTINGS_SUBTITLES = 15
			,WINDOW_SETTINGS_SCREENSAVER = 16
			,WINDOW_WEATHER_SETTINGS = 17
			,WINDOW_SETTINGS_OSD = 18
			,WINDOW_SCRIPTS = 20
			,WINDOW_VIDEO_GENRE = 21
			,WINDOW_VIDEO_ACTOR = 22
			,WINDOW_VIDEO_YEAR = 23
			,WINDOW_SETTINGS_PROGRAMS = 24
			,WINDOW_VIDEO_TITLE = 25
			,WINDOW_SETTINGS_CACHE = 26
			,WINDOW_SETTINGS_AUTORUN = 27
			,WINDOW_VIDEO_PLAYLIST = 28
      ,WINDOW_SETTINGS_LCD = 29
      ,WINDOW_RADIO = 30
      ,WINDOW_SETTINGS_GUI = 31
      ,WINDOW_MSN = 32
      ,WINDOW_MSN_CHAT = 33
      ,WINDOW_MYPLUGINS = 34

			,WINDOW_DIALOG_YES_NO = 100
			,WINDOW_DIALOG_PROGRESS = 101
			,WINDOW_MUSIC_PLAYLIST = 500
			,WINDOW_MUSIC_FILES = 501
			,WINDOW_MUSIC_ALBUM = 502
			,WINDOW_MUSIC_ARTIST = 503
			,WINDOW_MUSIC_GENRE = 504
			,WINDOW_MUSIC_TOP100 = 505
			,WINDOW_MUSIC_FAVORITES = 506
			,WINDOW_TVGUIDE = 600
			,WINDOW_SCHEDULER = 601
			,WINDOW_TVFULLSCREEN = 602
			,WINDOW_RECORDEDTV = 603
      ,WINDOW_SEARCHTV = 604
      ,WINDOW_RECORDEDTVGENRE = 605
      ,WINDOW_RECORDEDTVCHANNEL = 606

      ,WINDOW_MY_RECIPIES = 750
      ,WINDOW_STATUS = 755
      ,WINDOW_STATUS_DETAILS = 756
      ,WINDOW_STATUS_PREFS

			,WINDOW_VIRTUAL_KEYBOARD = 1000
			,WINDOW_VIRTUAL_SEARCH_KEYBOARD = 1001 // by Agree
			,WINDOW_DIALOG_SELECT = 2000
			,WINDOW_MUSIC_INFO = 2001
			,WINDOW_DIALOG_OK = 2002
			,WINDOW_VIDEO_INFO = 2003
			,WINDOW_MUSIC_OVERLAY = 2004
			,WINDOW_FULLSCREEN_VIDEO = 2005
			,WINDOW_VISUALISATION = 2006
			,WINDOW_SLIDESHOW = 2007
			,WINDOW_DIALOG_FILESTACKING = 2008
			,WINDOW_DIALOG_SELECT2 = 2009
			,WINDOW_DIALOG_DATETIME = 2010
			,WINDOW_ARTIST_INFO = 2011
			,WINDOW_DIALOG_MENU = 2012
			,WINDOW_DIALOG_RATING = 2013
			,WINDOW_DIALOG_EXIF = 2014
			,WINDOW_WEATHER = 2600
			,WINDOW_SCREENSAVER = 2900
			,WINDOW_OSD = 2901
			,WINDOW_VIDEO_OVERLAY = 3000
			,WINDOW_DVD = 3001 // for keymapping
			,WINDOW_TV_OVERLAY = 3002
      ,WINDOW_TVOSD = 3003
      ,WINDOW_TOPBARHOME = 3004
      ,WINDOW_TOPBAR = 3005
		}

		private int m_dwWindowId = 0; 
		private int m_dwPreviousWindowId = 0;
		protected int m_dwDefaultFocusControlID = 0;
		protected ArrayList m_vecPositions = new ArrayList();
		protected ArrayList m_vecControls = new ArrayList();
		protected string m_strWindowXmlFile = "";
		protected bool m_bAllowOverlay = true;
		bool m_bSkinLoaded = false;

		/// <summary>
		/// The (emtpy) constructur of the GUIWindow
		/// </summary>
		public GUIWindow()
		{
    }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="strXMLFile">filename of xml skin file which belongs to this window</param>
    public GUIWindow(string strXMLFile)
    {
			if (strXMLFile==null) return;
      m_dwPreviousWindowId=-1;
      m_strWindowXmlFile=strXMLFile;

    }

    /// <summary>
    /// Clear() method. This method gets called when user switches skin. It removes any static vars
    /// the GUIWindow class has
    /// </summary>
    static public void Clear()
    {
    	GUIControlFactory.ClearReferences();
    }
    public virtual bool Focused
    {
      get { return false; }
      set {}
    }

		/// <summary>
		/// Render() method. This method draws the window by asking every control
		/// of the window to render itself
		/// </summary>
		public virtual void Render()
		{
      //lock (this)
      {
				try
				{
					if (!m_bSkinLoaded)
					{
						if (GUIGraphicsContext.IsFullScreenVideo) return;
						if (GetID == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO) return;
						if (GetID == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN) return;

						// Print an error message
						GUIFont font = GUIFontManager.GetFont(0);
						if (font != null)
						{
							float fW = 0f;
							float fH = 0f;
							string strLine = String.Format("Missing or invalid file:{0}", m_strWindowXmlFile);
							font.GetTextExtent(strLine, ref fW, ref fH);
							float x = (GUIGraphicsContext.Width - fW) / 2f;
							float y = (GUIGraphicsContext.Height - fH) / 2f;
							font.DrawText(x, y, 0xffffffff, strLine, GUIControl.Alignment.ALIGN_LEFT);
						}
					}
					for (int x = 0; x < m_vecControls.Count; ++x)
					{
						((GUIControl)m_vecControls[x]).Render();
					}
				}
				catch(Exception ex)
				{
					Log.Write("render exception:{0}", ex.ToString());
				}
      }
		}

		/// <summary>
		/// NeedRefresh() can be called to see if the windows needs 2 redraw itself or not
		/// some controls (for example the fadelabel) contain scrolling texts and need 2
		/// ne re-rendered constantly
		/// </summary>
		/// <returns>true or false</returns>
		public virtual bool NeedRefresh()
    {
			try
			{
				for (int x = 0; x < m_vecControls.Count; ++x)
				{
					if (((GUIControl)((GUIControl)m_vecControls[x])).NeedRefresh()) return true;
				}
			}
			catch(Exception ex)
			{
				Log.Write("NeedRefresh exception:{0}", ex.ToString());
			}
			return false;
		}

		/// <summary>
		/// OnAction() method. This method gets called when there's a new action like a 
		/// keypress or mousemove or... By overriding this method, the window can respond
		/// to any action
		/// </summary>
		/// <param name="action">action : contains the action</param>
		public virtual void OnAction(Action action)
		{
			if (action==null) return ;
      //lock (this)
      {
				try
				{
					GUIMessage msg;
					// mouse moved, check which control has the focus
					if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
					{
						for (int i=0; i < m_vecControls.Count;++i)
						{
							GUIControl control =(GUIControl )m_vecControls[i];
							bool bFocus;
							int controlID;
							if (control.HitTest((int)action.fAmount1, (int)action.fAmount2, out controlID, out bFocus))
							{	
								if (!bFocus)
								{
									msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, controlID, 0, 0, null);
									OnMessage(msg);
									control.HitTest((int)action.fAmount1, (int)action.fAmount2,out controlID, out bFocus);
								}
								control.OnAction(action);
								return;
							}
						}
						return;
					}
					// mouse clicked if there is a hit pass the action
					if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
					{
						for (int i=0; i < m_vecControls.Count;++i)
						{
							GUIControl control =(GUIControl )m_vecControls[i];
							bool bFocus;
							int controlID;
							if (control.HitTest((int)action.fAmount1, (int)action.fAmount2, out controlID, out bFocus))
							{	
								GUIControl cntl=GetControl(controlID);
								if (cntl!=null) cntl.OnAction(action);
								return;
							}
						}
						return;
					}
	  			

					// send the action to the control which has the focus
					GUIControl cntlFoc = GetControl(GetFocusControlId() );
					if (cntlFoc!=null)
					{
						cntlFoc.OnAction(action);
						return;
					}

					// no control has focus?
					// set focus to the default control then
					msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, m_dwDefaultFocusControlID, 0, 0, null);
					OnMessage(msg);

				}
				catch(Exception ex)
				{
					Log.Write("OnAction exception:{0}", ex.ToString());
				}
      }
		}

		/// <summary>
		/// Property which returns an arraylist containing all controls 
		/// of this window
		/// </summary>
    public ArrayList GUIControls
    {
      get { return m_vecControls;}
    }

		/// <summary>
		/// OnMessage() This method gets called when there's a new message. 
		/// Controls send messages to notify their parents about their state (changes)
		/// By overriding this method a window can respond to the messages of its controls
		/// </summary>
		/// <param name="message"></param>
		/// <returns>true if the message was handled, false if it wasnt</returns>
		public virtual bool OnMessage(GUIMessage message)
		{
			if (message==null) return true;
      //lock (this)
      {
				try
				{
					switch (message.Message)
					{
							// Initialize the window.
						case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
						{

							LoadSkin();
							AllocResources();
							InitControls();
							GUIGraphicsContext.Overlay = m_bAllowOverlay;
							if (message.Param1 != (int)GUIWindow.Window.WINDOW_INVALID)
							{
								if (message.Param1 != GetID)
									m_dwPreviousWindowId = message.Param1;
							}
							GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, m_dwDefaultFocusControlID, 0, 0, null);
							OnMessage(msg);

							GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(10000 + GetID));
              GUIGraphicsContext.form.Text="Media Portal - "+  GUILocalizeStrings.Get(10000 + GetID);
							Log.Write( "window:{0} init", this.ToString());
						}
							return true;
							// TODO BUG ! Check if this return needs to be in the case and if there needs to be a break statement after each case.
	      
							// Cleanup and free resources
						case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
						{
							Log.Write( "window:{0} deinit", this.ToString());
							FreeResources();
							DeInitControls();
							GUITextureManager.CleanupThumbs();
							GC.Collect();
							return true;
						}
	  				
							// Set the focus on the correct control
						case GUIMessage.MessageType.GUI_MSG_SETFOCUS : 
						{
							if (GetFocusControlId() == message.TargetControlId) return true;

							if (message.TargetControlId > 0)
							{
								GUIControl cntlFocused= GetControl(GetFocusControlId());					
								if (cntlFocused!=null) 
								{
									GUIMessage msgLostFocus = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LOSTFOCUS, GetID, cntlFocused.GetID, cntlFocused.GetID, 0, 0, null);
									cntlFocused.OnMessage(msgLostFocus);
								}
								GUIControl cntTarget=GetControl(message.TargetControlId);
								if (cntTarget!=null)
								{
									cntTarget.OnMessage(message);
								}
							}
							return true;
						}
					}
	      
					GUIControl cntlTarget=GetControl(message.TargetControlId);
					if (cntlTarget!=null)
					{
						return cntlTarget.OnMessage(message);
					}

				}
				catch(Exception ex)
				{
					Log.Write("OnMessage exception:{0}", ex.ToString());
				}
        return false;
      }
		}

		/// <summary>
		/// add a new control to this window
		/// </summary>
		/// <param name="control">new control to add</param>
		public void Add(ref GUIControl control)
		{
      if (control==null) return;
			control.WindowId = GetID;
			m_vecControls.Add(control);
		}

		/// <summary>
		/// remove a control by its id from this window
		/// </summary>
		/// <param name="dwId">ID of the control</param>
		public void Remove(int dwId)
		{
			int index = 0;
			foreach (GUIControl control in m_vecControls)
			{
        GUIGroup grp = control as GUIGroup;
        if (grp !=null)
        {
          grp.Remove(dwId);
        }
        else
        {
          if (control.GetID == dwId)
          {
						if (index >=0 && index < m_vecControls.Count)
							m_vecControls.RemoveAt(index);
            return;
          }
        }
				index++;
			}
		}

		/// <summary>
		/// This method will call the PreInit() on each control belonging to this window
		/// this gives the control a way to do some pre-initalisation stuff
		/// </summary>
    void InitControls()
    {
			try
			{
				for (int x = 0; x < m_vecControls.Count; ++x)
				{
					((GUIControl)m_vecControls[x]).OnInit();
				}
			}
			catch(Exception ex)
			{
				Log.Write("InitControls exception:{0}", ex.ToString());
			}
    }

		/// This method will call the OnDeInit() on each control belonging to this window
		/// this gives the control a way to do some de-initalisation stuff
    protected void DeInitControls()
    {
			try
			{
				for (int x = 0; x < m_vecControls.Count; ++x)
				{
					((GUIControl)m_vecControls[x]).OnDeInit();
				}
			}
			catch(Exception ex)
			{
				Log.Write("DeInitControls exception:{0}", ex.ToString());
			}
    }

		/// <summary>
		/// returns the ID of the control which has the focus
		/// </summary>
		/// <returns>id of control or -1 if no control has the focus</returns>
		public virtual int GetFocusControlId()
		{
      for (int x = 0; x < m_vecControls.Count; ++x)
      {
        GUIGroup grp = m_vecControls[x] as GUIGroup;
        if (grp!=null)
        {
          int iFocusedControlId=grp.GetFocusControlId();
          if (iFocusedControlId>=0) return iFocusedControlId;
        }
        else
        {
          if (((GUIControl)m_vecControls[x]).Focus) return ((GUIControl)m_vecControls[x]).GetID;
        }
			}
			return - 1;
		}
		
		/// <summary>
		/// This method will remove the focus from the currently focused control
		/// </summary>
    public virtual void LooseFocus()
    {
      GUIControl cntl= GetControl ( GetFocusControlId() );
      if (cntl!=null) cntl.Focus=false;
    }

		/// <summary>
		/// Return the id of this window
		/// </summary>
		public virtual int GetID
		{
			get { return m_dwWindowId; }
			set { m_dwWindowId = value; }
		}

		/// <summary>
		/// return the id of the previous active window
		/// </summary>
		public int	PreviousWindowID
		{
			get { return m_dwPreviousWindowId; }
		}

		/// <summary>
		/// get a control by the control ID
		/// </summary>
		/// <param name="iControlId">id of control</param>
		/// <returns>GUIControl or null if control is not found</returns>
		public virtual GUIControl	GetControl(int iControlId) 
		{
      for (int x = 0; x < m_vecControls.Count; ++x)
      {
         GUIControl cntl = (GUIControl)m_vecControls[x];
         GUIControl cntlFound =  cntl.GetControlById( iControlId  );
         if (cntlFound!=null) return cntlFound;

			}
			return null;
		}

		/// <summary>
		/// remove all controls from the window
		/// </summary>
		public void ClearAll()
		{
			FreeResources();
			m_vecControls = new ArrayList();
		}

		/// <summary>
		/// Gets called by the runtime just before the window gets shown. It
		/// will ask every control of the window to allocate its (directx) resources 
		/// </summary>
		// 
		public virtual void	AllocResources()
		{
			try
			{
				// tell every control we're gonna alloc the resources next
				
				for (int x = 0; x < m_vecControls.Count; ++x)
				{
					((GUIControl)m_vecControls[x]).PreAllocResources();
				}

				// ask every control to alloc its resources
				for (int x = 0; x < m_vecControls.Count; ++x)
				{
					((GUIControl)m_vecControls[x]).AllocResources();
				}
			}
			catch(Exception ex)
			{
				Log.Write("AllocResources exception:{0}", ex.ToString());
			}
		}

		/// <summary>
		/// Gets called by the runtime when the window is not longer shown. It will
		/// ask every control of the window 2 free its (directx) resources
		/// </summary>
		public virtual void	FreeResources()
		{
			try
			{
				// tell every control to free its resources
				for (int x = 0; x < m_vecControls.Count; ++x)
				{
					((GUIControl)m_vecControls[x]).FreeResources();
				}
			}
			catch(Exception ex)
			{
				Log.Write("FreeResources exception:{0}", ex.ToString());
			}
		}
		
		/// <summary>
		/// Resets all the controls to their original positions, width&height
		/// </summary>
		public virtual void	ResetAllControls()
		{
			try
			{
				for (int x = 0; x < m_vecControls.Count; ++x)
				{
					((GUIControl)m_vecControls[x]).DoUpdate();
				}
			}
			catch(Exception ex)
			{
				Log.Write("ResetAllControls exception:{0}", ex.ToString());
			}
		}
		
		/// <summary>
		/// Gets by the window manager when it has loaded the window
		/// default implementation stores the position of all controls
		/// in m_vecPositions
		/// </summary>
		protected virtual void OnWindowLoaded()
		{
			m_vecPositions = new ArrayList();
			for (int i = 0; i < m_vecControls.Count; ++i)
			{
				GUIControl control = (GUIControl)m_vecControls[i];
        control.StorePosition();
				CPosition pos = new CPosition(ref control, control.XPosition, control.YPosition);
				m_vecPositions.Add(pos);
			}
		}

		/// <summary>
		/// Gets called by the runtime when a new window has been created
		/// Every window window should override this method and load itself by calling
		/// the Load() method
		/// </summary>
		/// <returns>true if initialisation was succesfull 
		/// else false</returns>
		public virtual bool Init()
		{
			return false;
		}

		/// <summary>
		/// Gets called by the runtime when a  window will be destroyed
		/// Every window window should override this method and cleanup any resources
		/// </summary>
		/// <returns></returns>
    public virtual void DeInit()
    {
    }

		/// <summary>
		/// Property indicating if the window supports delay loading or not
		/// if a window returns true it means that its resources & XML will be loaded
		/// just before it gets activated
		/// for windows not supporting delayed loading, the xml is immediately loaded
		/// at startup of the application
		/// </summary>
		public virtual bool SupportsDelayedLoad
		{
			get { return true; }
		}

		/// <summary>
		/// Load the XML file for this window which 
		/// contains a definition of which controls the GUI has
		/// </summary>
		/// <param name="strFileName">filename of the .xml file</param>
		/// <returns></returns>
		public virtual bool Load(string strFileName)
		{
			if (strFileName==null) return true;
			m_bSkinLoaded = false;
			if (strFileName == "") return true;
			m_strWindowXmlFile = strFileName;
      
			// if windows supports delayed loading then do nothing
			if (SupportsDelayedLoad) return true;

			//else load xml file now
			return LoadSkin();
		}


		/// <summary>
		/// Loads the xml file for the window.
		/// </summary>
		/// <returns></returns>
		public bool LoadSkin()
		{
			// no filename is configured
			if (m_strWindowXmlFile == "") return false;
			// TODO what is the reason for this check
			if (m_vecControls.Count > 0) return false;
			m_dwDefaultFocusControlID = 0;
			// Load the reference controls
			int iPos = m_strWindowXmlFile.LastIndexOf('\\');
			string strReferenceFile = m_strWindowXmlFile.Substring(0, iPos);
			strReferenceFile += @"\references.xml";
			GUIControlFactory.LoadReferences(strReferenceFile);
			
			if (!System.IO.File.Exists(m_strWindowXmlFile))
			{
				Log.Write("SKIN: Missing {0}", m_strWindowXmlFile);
				return false;
			}
			try
			{
				// Load the XML file
				XmlDocument doc = new XmlDocument();
				doc.Load(m_strWindowXmlFile);
				if (doc.DocumentElement == null) return false;
				string strRoot = doc.DocumentElement.Name;
				// Check root element
				if (strRoot != "window") return false;
				// Load id value
				XmlNode nodeId = doc.DocumentElement.SelectSingleNode("/window/id");
				if (nodeId == null) return false;
				// Set the default control that has the focus after loading the window
				XmlNode nodeDefault = doc.DocumentElement.SelectSingleNode("/window/defaultcontrol");
				if (nodeDefault == null) return false;
				// Convert the id to an int
				try
				{
					m_dwWindowId = (int)System.Int32.Parse(nodeId.InnerText);
				}
				catch (Exception)
				{
					// TODO Add some error when conversion fails message here.
				}
				// Convert the id of the default control to an int
				try
				{
					m_dwDefaultFocusControlID = System.Int32.Parse(nodeDefault.InnerText);
				}
				catch (Exception)
				{
					// TODO Add some error when conversion fails message here.
				}
				// Configure the overlay settings
				XmlNode nodeOverlay = doc.DocumentElement.SelectSingleNode("/window/allowoverlay");
				if (nodeOverlay != null) 
				{
					if (nodeOverlay.InnerText != null)
					{
						string strAllow = nodeOverlay.InnerText.ToLower();
						if (strAllow == "yes" || strAllow == "true")
							m_bAllowOverlay = true;
						if (strAllow == "no" || strAllow == "false")
							m_bAllowOverlay = false;
					}
				}

				// Load the list of the Controls that are used in the window
				XmlNodeList nodeList = doc.DocumentElement.SelectNodes("/window/controls/control");
				foreach (XmlNode node in nodeList)
				{
					LoadControl(node, m_vecControls);
				}
				// initialize the controls
				OnWindowLoaded();
				m_bSkinLoaded = true;
				return true;
			}
			catch (Exception ex)
			{
				Log.Write("exception loading window {0} err:{1}", m_strWindowXmlFile, ex.Message);
				return false;
			}
		}
    
		/// <summary>
		/// This method will load a single control from the xml node
		/// </summary>
		/// <param name="node">XmlNode describing the control</param>
		/// <param name="controls">on return this will contain an arraylist of all controls loaded</param>
    protected void LoadControl(XmlNode node, ArrayList controls)
    {
			if (node==null) return;
			if (controls==null) return;
			try
			{
				GUIControl newControl = GUIControlFactory.Create(m_dwWindowId, node);
				newControl.WindowId = GetID;
				controls.Add(newControl);
			}
			catch(Exception ex)
			{
				Log.Write("Unable to load control. exception:{0}",ex.ToString());
			}
    }

		/// <summary>
		/// This function gets called once by the runtime when everything is up & running
		/// directX is now initialized, but before the first window is activated. 
		/// It gives the window the oppertunity to allocate any (directx) resources
		/// it may need
		/// </summary>
		public virtual void PreInit()
		{
		}

		/// <summary>
		/// Restores all the (x,y) positions of the XML file to their original values
		/// </summary>
		public virtual void Restore()
		{
			m_vecControls.Clear();
			m_vecPositions.Clear();
			Load(m_strWindowXmlFile);
		}

		/// <summary>
		///  Gets called when DirectX device has been restored. 
		/// </summary>
		public virtual void OnDeviceRestored()
		{
		}

		/// <summary>
		/// Gets called when DirectX device has been lost. Any texture/font is now invalid
		/// </summary>
		public virtual void OnDeviceLost()
		{
		}
		/// <summary>
		/// PostRender() gives the window the oppertunity to overlay itself ontop of
		/// the other window(s)
		/// It gets called at the end of every rendering cycle even 
    /// if the window is not activated
    /// <param name="iLayer">indicates which overlay layer is rendered (1-10)
    /// this gives the plugins the oppertunity to tell which overlay layer they are using
    /// For example the topbar is rendered on layer #1
    /// while the music overlay is rendered on layer #2 (and thus on top of the topbar)</param>
		/// </summary>
		public virtual void PostRender(int iLayer)
		{
		}

		/// <summary>
		/// Returns wither or not the window does postrendering.
		/// </summary>
		/// <returns>false</returns>
    public virtual bool DoesPostRender()
    {
      return false;
    }

    
		/// <summary>
		/// Returns whether the music/video/tv overlay is allowed on this screen
		/// </summary>
		public virtual bool OverlayAllowed
		{
			get { return m_bAllowOverlay; }
		}

		/// <summary>
		/// Returns whether the user can goto full screen video,tv,visualisation from this window
		/// </summary>
		public virtual bool FullScreenVideoAllowed
		{
			get { return true; }
		}

		/// <summary>
		/// Restores the position of the control to its default position.
		/// </summary>
		/// <param name="iControl">The identifier of the control that needs to be restored.</param>
    public void RestoreControlPosition(int iControl)
    {
      for (int x = 0; x < m_vecControls.Count; ++x)
      {
        GUIControl cntl = (GUIControl)m_vecControls[x];
        cntl.ReStorePosition();
      }
    }

    public virtual void Process()
    {
    }
	}
}

