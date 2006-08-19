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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIActionMenu : GUIWindow, IRenderLayer
  {

    #region Variables
    bool _running = false;
		bool _prevOverlay = false;
		int _parentWinID = -1;
		GUIWindow _parentWin = null;
		List<GUIControl> _list = null;
    #endregion

		public bool Running
		{ get { return _running; } }
    
    public GUIActionMenu()
    {
      GetID = (int)GUIWindow.Window.WINDOW_ACTIONMENU;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\ActionMenu.xml");
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void PreInit()
    {
    }
		
  	void AddControls(string xmlFilename)
		{
			if (_list != null) _list.Clear();
			_list = LoadControl(xmlFilename);
			for (int i = 0; i < _list.Count; i++)
			{
				GUIControl ctl = _list[i];
				Add(ref ctl);
			}
			LoadSkin();
			AllocResources();
			InitControls();
		}

		void RemoveControls()
		{
			FreeResources();
			DeInitControls();
			foreach (GUIControl listControl in _list)
			{
				foreach (GUIControl childControl in Children)
        {
					if (listControl == childControl)
					{
						Children.Remove(childControl);
						break;
					}
				}
			}
			_list.Clear();
		}

		
		protected void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        _running = false;
				_parentWinID = -1;
				_parentWin = null;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    #region Base Dialog Members
    public void DoModal(int dwParentId)
    {
			GUIWindow parentWindow = GUIWindowManager.GetWindow(dwParentId); ;
      if (parentWindow == null) return;

      bool wasRouted = GUIWindowManager.IsRouted;
      IRenderLayer prevLayer = GUILayerManager.GetLayer(GUILayerManager.LayerType.Dialog);

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, -1, 0, null);
      OnMessage(msg);

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      GUIWindowManager.IsSwitchingToNewWindow = false;
      _running = true;
      while (_running && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
      GUIGraphicsContext.Overlay = _prevOverlay;
      //FreeResources();
      //DeInitControls();
      GUILayerManager.UnRegisterLayer(this);
      if (wasRouted)
      {
        GUIWindowManager.RouteToWindow(dwParentId);
        GUILayerManager.RegisterLayer(prevLayer, GUILayerManager.LayerType.Dialog);
      }
    }
    #endregion


		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
			{
				Close();
				return;
			}

			if (action.wID == Action.ActionType.ACTION_SHOW_ACTIONMENU)
			{
				GUIActionMenu menu = (GUIActionMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ACTIONMENU);
				if (menu != null)
				{
					if (!menu.Running)
					{
						_parentWinID = (int)action.fAmount1;
						_parentWin = GUIWindowManager.GetWindow(_parentWinID);
						AddControls(action.m_SoundFileName);
						menu.DoModal(_parentWinID);
					}
				}
				return;
			}

			if (_running && _parentWin != null)
			{
				switch (action.wID)
				{
					case Action.ActionType.ACTION_SELECT_ITEM:
				    _parentWin.OnAction(action);
					  break;
			  }
			}

			base.OnAction(action);
		}

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            _running = false;
						RemoveControls();
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            _prevOverlay = GUIGraphicsContext.Overlay;
            base.OnMessage(message);
            int parentWindowId = GUIWindowManager.ActiveWindow;
            GUIWindow parentWindow = GUIWindowManager.GetWindow(parentWindowId);
            GUIGraphicsContext.Overlay = parentWindow.IsOverlayAllowed;
          }
          return true;
			
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					if (_running && _parentWin != null) _parentWin.OnMessage(message);
					break;
      }

      return base.OnMessage(message);
    }

    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
			//foreach (GUIControl control in _list) if (control != null) control.Render(timePassed);
    }
    #endregion


		public List<GUIControl> LoadControl(string xmlFilename)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(GUIGraphicsContext.Skin + "\\" + xmlFilename);
			List<GUIControl> listControls = new List<GUIControl>();


			if (doc.DocumentElement == null) return listControls;
			if (doc.DocumentElement.Name != "window") return listControls;

			// Load Definitions
			Hashtable table = new Hashtable();
			try
			{
				foreach (XmlNode node in doc.SelectNodes("/window/define"))
				{
					string[] tokens = node.InnerText.Split(':');
					if (tokens.Length < 2) continue;
					table[tokens[0]] = tokens[1];
				}
			}
			catch (Exception e)
			{
				_log.Info("LoadDefines: {0}", e.Message);
			}

			foreach (XmlNode controlNode in doc.DocumentElement.SelectNodes("/window/controls/control"))
			{
				try
				{
					GUIControl newControl = GUIControlFactory.Create(GetID, controlNode, table);
					if (newControl != null)
					{
						newControl.AllocResources();
						listControls.Add(newControl);
					}
				}
				catch (Exception ex)
				{
					_log.Error("Unable to load control: {0}", ex.ToString());
				}
			}
			return listControls;
		}

  }
}
