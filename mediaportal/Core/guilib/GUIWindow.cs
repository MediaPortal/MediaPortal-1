using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using System.Reflection;
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
		#region window ids
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
			,WINDOW_SECOND_HOME = 35

			,WINDOW_DIALOG_YES_NO = 100
			,WINDOW_DIALOG_PROGRESS = 101
			,WINDOW_MUSIC_PLAYLIST = 500
			,WINDOW_MUSIC_FILES = 501
			,WINDOW_MUSIC_ALBUM = 502
			,WINDOW_MUSIC_ARTIST = 503
			,WINDOW_MUSIC_GENRE = 504
			,WINDOW_MUSIC_TOP100 = 505
			,WINDOW_MUSIC_FAVORITES = 506
			,WINDOW_MUSIC_YEARS = 507
			,WINDOW_TVGUIDE = 600
			,WINDOW_SCHEDULER = 601
			,WINDOW_TVFULLSCREEN = 602
			,WINDOW_RECORDEDTV = 603
      ,WINDOW_SEARCHTV = 604
      ,WINDOW_RECORDEDTVGENRE = 605
      ,WINDOW_RECORDEDTVCHANNEL = 606
			,WINDOW_TV_SCHEDULER_PRIORITIES = 607
			,WINDOW_TV_CONFLICTS = 608
			,WINDOW_TV_COMPRESS_MAIN = 609
			,WINDOW_TV_COMPRESS_SETTINGS = 610
			,WINDOW_TV_COMPRESS_AUTO = 611
			,WINDOW_TV_COMPRESS_COMPRESS = 612
			,WINDOW_TV_COMPRESS_COMPRESS_STATUS = 613
		  ,WINDOW_VIDEO_ARTIST_INFO = 614
			,WINDOW_WIZARD_WELCOME =615
			,WINDOW_WIZARD_CARDS_DETECTED =616
		  ,WINDOW_WIZARD_DVBT_COUNTRY =617
		  ,WINDOW_WIZARD_DVBT_SCAN=618
			,WINDOW_WIZARD_DVBC_COUNTRY =619
			,WINDOW_WIZARD_DVBC_SCAN=620
			,WINDOW_WIZARD_DVBS_SELECT_LNB=621
			,WINDOW_WIZARD_DVBS_SELECT_DETAILS=622

      ,WINDOW_MY_RECIPIES = 750
      ,WINDOW_STATUS = 755
      ,WINDOW_STATUS_DETAILS = 756
      ,WINDOW_STATUS_PREFS = 757
			,WINDOW_DIALOG_FILE = 758
		  ,WINDOW_MY_BURNER=760

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
			,WINDOW_DIALOG_MENU_BOTTOM_RIGHT=2015
			,WINDOW_DIALOG_NOTIFY=2016
			,WINDOW_WEATHER = 2600
			,WINDOW_SCREENSAVER = 2900
			,WINDOW_OSD = 2901
			,WINDOW_MSNOSD = 2902
			,WINDOW_VIDEO_OVERLAY = 3000
			,WINDOW_DVD = 3001 // for keymapping
			,WINDOW_TV_OVERLAY = 3002
      ,WINDOW_TVOSD = 3003
      ,WINDOW_TOPBARHOME = 3004
      ,WINDOW_TOPBAR = 3005
			,WINDOW_TVMSNOSD = 3006
			,WINDOW_TVZAPOSD=3007
			,WINDOW_TELETEXT = 7700
			,WINDOW_FULLSCREEN_TELETEXT = 7701
		}
#endregion

		#region variables
		private int windowId = -1; 
		private int previousWindowId = -1;
		protected int defaultControlId = 0;
		protected ArrayList m_vecPositions = new ArrayList();
		protected ArrayList controlList = new ArrayList();
		protected string windowXmlFileName = "";
		protected bool overlayAllowed = true;
		
		//-1=default from topbar.xml 
		// 0=flase from skin.xml
		// 1=true  from skin.xml
    protected int m_iAutoHideTopbar = -1;
		protected bool m_bAutoHideTopbar = false;
		bool isSkinLoaded = false;
		#endregion

		#region ctor
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
    public GUIWindow(string skinFile)
    {
			if (skinFile==null) return;
      previousWindowId=-1;
      windowXmlFileName=skinFile;

    }

		#endregion

		#region methods
    /// <summary>
    /// Clear() method. This method gets called when user switches skin. It removes any static vars
    /// the GUIWindow class has
    /// </summary>
    static public void Clear()
    {
    	GUIControlFactory.ClearReferences();
    }
		/// <summary>
		/// Property which returns an arraylist containing all controls 
		/// of this window
		/// </summary>
    public ArrayList GUIControls
    {
      get { return controlList;}
    }

		/// <summary>
		/// add a new control to this window
		/// </summary>
		/// <param name="control">new control to add</param>
		public void Add(ref GUIControl control)
		{
      if (control==null) return;
			control.WindowId = GetID;
			controlList.Add(control);
		}

		/// <summary>
		/// remove a control by its id from this window
		/// </summary>
		/// <param name="dwId">ID of the control</param>
		public void Remove(int dwId)
		{
			int index = 0;
			foreach (GUIControl control in controlList)
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
						if (index >=0 && index < controlList.Count)
							controlList.RemoveAt(index);
            return;
          }
        }
				index++;
			}
		}

		/// <summary>
		/// This method will call the OnInit() on each control belonging to this window
		/// this gives the control a way to do some pre-initalisation stuff
		/// </summary>
    public void InitControls()
    {
			try
			{
				for (int x = 0; x < controlList.Count; ++x)
				{
					((GUIControl)controlList[x]).OnInit();
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"InitControls exception:{0}", ex.ToString());
			}
    }

		/// This method will call the OnDeInit() on each control belonging to this window
		/// this gives the control a way to do some de-initalisation stuff
    protected void DeInitControls()
    {
			try
			{
				for (int x = 0; x < controlList.Count; ++x)
				{
					((GUIControl)controlList[x]).OnDeInit();
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"DeInitControls exception:{0}", ex.ToString());
			}
    }


		/// <summary>
		/// return the id of the previous active window
		/// </summary>
		public int	PreviousWindowID
		{
			get { return previousWindowId; }
		}

		/// <summary>
		/// remove all controls from the window
		/// </summary>
		public void ClearAll()
		{
			FreeResources();
			controlList = new ArrayList();
		}


		/// <summary>
		/// Restores the position of the control to its default position.
		/// </summary>
		/// <param name="iControl">The identifier of the control that needs to be restored.</param>

		public void RestoreControlPosition(int iControl)
		{
			for (int x = 0; x < controlList.Count; ++x)
			{
				GUIControl cntl = (GUIControl)controlList[x];
				cntl.ReStorePosition();
			}
		}
		#endregion 

		#region load skin file
		/// <summary>
		/// Load the XML file for this window which 
		/// contains a definition of which controls the GUI has
		/// </summary>
		/// <param name="skinFileName">filename of the .xml file</param>
		/// <returns></returns>
		public virtual bool Load(string skinFileName)
		{
			if (skinFileName==null) return true;
			isSkinLoaded = false;
			if (skinFileName == "") return true;
			windowXmlFileName = skinFileName;
      
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
			if (windowXmlFileName == "") return false;
			// TODO what is the reason for this check
			if (controlList.Count > 0) return false;
			defaultControlId = 0;
			// Load the reference controls
			int iPos = windowXmlFileName.LastIndexOf('\\');
			string strReferenceFile = windowXmlFileName.Substring(0, iPos);
			strReferenceFile += @"\references.xml";
			GUIControlFactory.LoadReferences(strReferenceFile);
			
			if (!System.IO.File.Exists(windowXmlFileName))
			{
				Log.WriteFile(Log.LogType.Log,true,"SKIN: Missing {0}", windowXmlFileName);
				return false;
			}
			try
			{
				// Load the XML file
				XmlDocument doc = new XmlDocument();
				doc.Load(windowXmlFileName);
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
					windowId = (int)System.Int32.Parse(nodeId.InnerText);
				}
				catch (Exception)
				{
					// TODO Add some error when conversion fails message here.
				}
				// Convert the id of the default control to an int
				try
				{
					defaultControlId = System.Int32.Parse(nodeDefault.InnerText);
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
							overlayAllowed = true;
						if (strAllow == "no" || strAllow == "false")
							overlayAllowed = false;
					}
				}

				IDictionary defines = LoadDefines(doc);

				// Configure the autohide setting
				XmlNode nodeAutoHideTopbar = doc.DocumentElement.SelectSingleNode("/window/autohidetopbar");
				if (nodeAutoHideTopbar != null) 
				{
					if (nodeAutoHideTopbar.InnerText != null)
					{
						m_iAutoHideTopbar = -1;
						string strAllow = nodeAutoHideTopbar.InnerText.ToLower();
						if (strAllow == "yes" || strAllow == "true")
							m_iAutoHideTopbar = 1;
						if (strAllow == "no" || strAllow == "false")
							m_iAutoHideTopbar = 0;
					}
				} 

				XmlNodeList nodeList = doc.DocumentElement.SelectNodes("/window/controls/*");

				foreach(XmlNode node in nodeList)
				{
					if(node.Name == null)
						continue;

					switch(node.Name)
					{
						case "control":
							LoadControl(node, controlList, defines);
							break;
						case "include":
						case "import":
							LoadInclude(node, controlList, defines);
							break;
					}
				}

				// initialize the controls
				OnWindowLoaded();
				isSkinLoaded = true;
				return true;
			}
			catch (Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"exception loading window {0} err:{1}", windowXmlFileName, ex.Message);
				return false;
			}
		}
    
		/// <summary>
		/// This method will load a single control from the xml node
		/// </summary>
		/// <param name="node">XmlNode describing the control</param>
		/// <param name="controls">on return this will contain an arraylist of all controls loaded</param>
		protected void LoadControl(XmlNode node, ArrayList controls, IDictionary defines)
		{
			if (node==null) return;
			if (controls==null) return;
			try
			{
				GUIControl newControl = GUIControlFactory.Create(windowId, node, defines);
				newControl.WindowId = GetID;
				GUIImage img = newControl as GUIImage;
				if (img!=null)
				{
					if (img.Width==0 || img.Height==0)
					{
						Log.Write("xml:{0} image id:{1} width:{2} height:{3} gfx:{4}",
							windowXmlFileName, img.GetID, img.Width, img.Height, img.FileName);
					}
				}
				controls.Add(newControl);
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"Unable to load control. exception:{0}",ex.ToString());
			}
		}

		bool LoadInclude(XmlNode node, ArrayList controlList, IDictionary defines)
		{
			if(node == null || controlList == null)
				return false;

			if(System.IO.File.Exists(windowXmlFileName) == false)
			{
				Log.WriteFile(Log.LogType.Log, true,"SKIN: Missing {0}", windowXmlFileName);
				return false;
			}

			try
			{
				XmlDocument doc = new XmlDocument();

				doc.Load(GUIGraphicsContext.Skin + "\\" + node.InnerText);

				if(doc.DocumentElement == null)
					return false;

				if(doc.DocumentElement.Name != "window")
					return false;

				foreach(XmlNode controlNode in doc.DocumentElement.SelectNodes("/window/controls/control"))
					LoadControl(controlNode, controlList, defines);

				return true;
			}
			catch(Exception e)
			{
				Log.Write("GUIWIndow.LoadInclude: {0}", e.Message);
			}

			return false;
		}

		IDictionary LoadDefines(XmlDocument document)
		{
			Hashtable table = new Hashtable();

			try
			{
				foreach(XmlNode node in document.SelectNodes("/window/define"))
				{
					string[] tokens = node.InnerText.Split(':');

					if(tokens.Length < 2)
						continue;

					table[tokens[0]] = tokens[1];
				}
			}
			catch(Exception e)
			{
				Log.Write("GUIWindow.LoadDefines: {0}", e.Message);
			}

			return table;
		}
		
		#endregion

		#region virtual methods
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
			controlList.Clear();
			m_vecPositions.Clear();
			Load(windowXmlFileName);
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
		/// Returns whether the music/video/tv overlay is allowed on this screen
		/// </summary>
		public virtual bool OverlayAllowed
		{
			get { return overlayAllowed; }
		}
    
    /// <summary>
    /// Returns whether autohide the topbar is allowed on this screen
    /// </summary>
    public virtual bool AutoHideTopbar 
    {
      get
			{
				// set topbar autohide 
				switch (m_iAutoHideTopbar)
				{
					case 0: 
						return false;
					case 1:
						return true;
					default:
						return GUIGraphicsContext.DefaultTopBarHide;
				}
			}
    }

    public virtual void Process()
    {
    }

		public virtual void SetObject(object obj)
		{
		}
		protected virtual void OnPageLoad()
		{
		}
		protected virtual void OnPageDestroy(int newWindowId)
		{
		}
		protected virtual void OnShowContextMenu()
		{
		}
		protected virtual void OnPreviousWindow()
		{
			GUIWindowManager.ShowPreviousWindow();
		}
		protected virtual void OnClicked( int controlId, GUIControl control, Action.ActionType actionType) 
		{
		}
		protected virtual void OnClickedUp( int controlId, GUIControl control, Action.ActionType actionType) 
		{
		}
		protected virtual void OnClickedDown( int controlId, GUIControl control, Action.ActionType actionType) 
		{
		}

		/// <summary>
		/// Returns whether the user can goto full screen video,tv,visualisation from this window
		/// </summary>
		public virtual bool FullScreenVideoAllowed
		{
			get { return true; }
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
		/// Gets called by the runtime just before the window gets shown. It
		/// will ask every control of the window to allocate its (directx) resources 
		/// </summary>
		// 
		public virtual void	AllocResources()
		{
			try
			{
				// tell every control we're gonna alloc the resources next
				
				for (int x = 0; x < controlList.Count; ++x)
				{
					((GUIControl)controlList[x]).PreAllocResources();
				}

				// ask every control to alloc its resources
				for (int x = 0; x < controlList.Count; ++x)
				{
					((GUIControl)controlList[x]).AllocResources();
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"AllocResources exception:{0}", ex.ToString());
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
				for (int x = 0; x < controlList.Count; ++x)
				{
					((GUIControl)controlList[x]).FreeResources();
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"FreeResources exception:{0}", ex.ToString());
			}
		}
		
		/// <summary>
		/// Resets all the controls to their original positions, width&height
		/// </summary>
		public virtual void	ResetAllControls()
		{
			try
			{
				for (int x = 0; x < controlList.Count; ++x)
				{
					((GUIControl)controlList[x]).DoUpdate();
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"ResetAllControls exception:{0}", ex.ToString());
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
			for (int i = 0; i < controlList.Count; ++i)
			{
				GUIControl control = (GUIControl)controlList[i];
				control.StorePosition();
				CPosition pos = new CPosition(ref control, control.XPosition, control.YPosition);
				m_vecPositions.Add(pos);
			}

			FieldInfo[] allFields = this.GetType().GetFields(BindingFlags.Instance 
				|BindingFlags.NonPublic
				|BindingFlags.FlattenHierarchy
				|BindingFlags.Public);
			foreach (FieldInfo field in allFields)
			{
				if (field.IsDefined(typeof(SkinControlAttribute), false))
				{
					SkinControlAttribute atrb = (SkinControlAttribute)field.GetCustomAttributes(typeof(SkinControlAttribute), false)[0];
				
					GUIControl control = GetControl(atrb.ID);
					if (control!=null)
					{
						try
						{
							field.SetValue(this, control);
						}
						catch(Exception ex)
						{
							Log.WriteFile(Log.LogType.Log,true,"GUIWindow:OnWindowLoaded id:{0} ex:{1} {2}", atrb.ID,ex.Message,ex.StackTrace);
						}
					}
				}
			}

		}

		/// <summary>
		/// get a control by the control ID
		/// </summary>
		/// <param name="iControlId">id of control</param>
		/// <returns>GUIControl or null if control is not found</returns>
		public virtual GUIControl	GetControl(int iControlId) 
		{
			for (int x = 0; x < controlList.Count; ++x)
			{
				GUIControl cntl = (GUIControl)controlList[x];
				GUIControl cntlFound =  cntl.GetControlById( iControlId  );
				if (cntlFound!=null) return cntlFound;

			}
			return null;
		}


		/// <summary>
		/// returns the ID of the control which has the focus
		/// </summary>
		/// <returns>id of control or -1 if no control has the focus</returns>
		public virtual int GetFocusControlId()
		{
			for (int x = 0; x < controlList.Count; ++x)
			{
				GUIGroup grp = controlList[x] as GUIGroup;
				if (grp!=null)
				{
					int iFocusedControlId=grp.GetFocusControlId();
					if (iFocusedControlId>=0) return iFocusedControlId;
				}
				else
				{
					if (((GUIControl)controlList[x]).Focus) return ((GUIControl)controlList[x]).GetID;
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
			get { return windowId; }
			set { windowId = value; }
		}
		/// <summary>
		/// Render() method. This method draws the window by asking every control
		/// of the window to render itself
		/// </summary>
		public virtual void Render(float timePassed)
		{
      //lock (this)
      {
				try
				{
					if (!isSkinLoaded)
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
							string strLine = String.Format("Missing or invalid file:{0}", windowXmlFileName);
							font.GetTextExtent(strLine, ref fW, ref fH);
							float x = (GUIGraphicsContext.Width - fW) / 2f;
							float y = (GUIGraphicsContext.Height - fH) / 2f;
							font.DrawText(x, y, 0xffffffff, strLine, GUIControl.Alignment.ALIGN_LEFT,-1);
							strLine=null;
						}
						font=null;
					}
					for (int x = 0; x < controlList.Count; ++x)
					{
						((GUIControl)controlList[x]).Render(timePassed);
					}
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"render exception:{0}", ex.ToString());
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
				for (int x = 0; x < controlList.Count; ++x)
				{
					if (((GUIControl)((GUIControl)controlList[x])).NeedRefresh()) return true;
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"NeedRefresh exception:{0}", ex.ToString());
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
				if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
				{
					OnShowContextMenu();
					return;
				}
				if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
				{
					OnPreviousWindow();
					return;
				}

				try
				{
					GUIMessage msg;
					// mouse moved, check which control has the focus
					if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
					{
						OnMouseMove((int)action.fAmount1, (int)action.fAmount2,action);
						return;
					}
					// mouse clicked if there is a hit pass the action
					if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
					{
						OnMouseClick((int)action.fAmount1, (int)action.fAmount2,action);
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
					msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, defaultControlId, 0, 0, null);
					OnMessage(msg);
					msg=null;

				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"OnAction exception:{0}", ex.ToString());
				}
      }
		}

		public virtual bool Focused
		{
			get { return false; }
			set {}
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
						case GUIMessage.MessageType.GUI_MSG_CLICKED:
						{
							int iControlId = message.SenderControlId;
							if (iControlId != 0) 
								OnClicked( iControlId, GetControl(iControlId), (Action.ActionType)message.Param1) ;
						}
						break;
						case GUIMessage.MessageType.GUI_MSG_CLICKED_DOWN:
						{
							int iControlId = message.SenderControlId;
							if (iControlId != 0) 
								OnClickedDown( iControlId, GetControl(iControlId), (Action.ActionType)message.Param1) ;
						}
							break;

						case GUIMessage.MessageType.GUI_MSG_CLICKED_UP:
						{
							int iControlId = message.SenderControlId;
							if (iControlId != 0) 
								OnClickedUp( iControlId, GetControl(iControlId), (Action.ActionType)message.Param1) ;
						}
							break;


							// Initialize the window.
						case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
						{
							GUIPropertyManager.SetProperty("#itemcount",String.Empty);
							GUIPropertyManager.SetProperty("#selecteditem",String.Empty);
							GUIPropertyManager.SetProperty("#selecteditem2",String.Empty);
							GUIPropertyManager.SetProperty("#selectedthumb",String.Empty);
							LoadSkin();
							AllocResources();
							InitControls();
							GUIGraphicsContext.Overlay = overlayAllowed;

							// set topbar autohide 
							switch (m_iAutoHideTopbar)
							{
								case 0:
									m_bAutoHideTopbar = false;
									break;
								case 1:
									m_bAutoHideTopbar = true;
									break;
								default:
									m_bAutoHideTopbar = GUIGraphicsContext.DefaultTopBarHide;
									break;
							}
							GUIGraphicsContext.AutoHideTopBar = m_bAutoHideTopbar;
							GUIGraphicsContext.TopBarHidden = m_bAutoHideTopbar;

							if (message.Param1 != (int)GUIWindow.Window.WINDOW_INVALID)
							{
								if (message.Param1 != GetID)
									previousWindowId = message.Param1;
							}
							GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, defaultControlId, 0, 0, null);
							OnMessage(msg);

              if (message.Param1 != (int)GUIWindow.Window.WINDOW_INVALID)
              {
                GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
              }
							Log.Write( "window:{0} init", this.ToString());
						}
							OnPageLoad();
							return true;
							// TODO BUG ! Check if this return needs to be in the case and if there needs to be a break statement after each case.
	      
							// Cleanup and free resources
						case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
						{
							OnPageDestroy(message.Param1);
							if (previousWindowId != (int)GUIWindow.Window.WINDOW_INVALID)
              {
                GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + previousWindowId));
              }

							Log.Write( "window:{0} deinit", this.ToString());
							FreeResources();
							DeInitControls();
							GUITextureManager.CleanupThumbs();
							GC.Collect();
							GC.Collect();
							GC.Collect();
							long lTotalMemory=GC.GetTotalMemory(true);
							Log.Write("Total Memory allocated:{0}", MediaPortal.Util.Utils.GetSize(lTotalMemory) );
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
									msgLostFocus=null;
									cntlFocused=null;
								}
								GUIControl cntTarget=GetControl(message.TargetControlId);
								if (cntTarget!=null)
								{
									cntTarget.OnMessage(message);
								}
								cntTarget=null;
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
					Log.WriteFile(Log.LogType.Log,true,"OnMessage exception:{0}", ex.ToString());
				}
        return false;
      }
		}

		protected virtual void OnMouseMove(int cx, int cy, Action action)
		{
			for (int i=controlList.Count-1 ; i>=0 ; i--)
			{
				GUIControl control =(GUIControl )controlList[i];
				bool bFocus;
				int controlID;
				if (control.HitTest(cx, cy, out controlID, out bFocus))
				{	
					if (!bFocus)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, controlID, 0, 0, null);
						OnMessage(msg);
						control.HitTest(cx, cy,out controlID, out bFocus);
					}
					control.OnAction(action);
					return;
				}
				else
				{
					// no control selected
					LooseFocus();
				}
			}
		}
		protected virtual void OnMouseClick(int posX, int posY, Action action)
		{
			for (int i=controlList.Count-1 ; i>=0 ; i--)
			{
				GUIControl control =(GUIControl )controlList[i];
				bool bFocus;
				int controlID;
				if (control.HitTest(posX, posY, out controlID, out bFocus))
				{	
					GUIControl cntl=GetControl(controlID);
					if (cntl!=null) cntl.OnAction(action);
					return;
				}
			}
		}
		#endregion

	}
}

