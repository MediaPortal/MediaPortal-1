#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;


namespace MediaPortal.Dialogs
{

  public abstract class GUIDialogWindow : GUIWindow, IRenderLayer
  {
    #region Variables
    // Private Variables
    private Object thisLock = new Object();      // used in Close
    private int _selectedLabel = -1;
    // Protected Variables
    protected GUIWindow _parentWindow = null;
    protected int _parentWindowID = -1;
    protected bool _prevOverlay = false;
    protected bool _running = false;
    // Public Variables
    #endregion

    #region Properties
    // Public Properties
    public int SelectedLabel
    {
      get { return _selectedLabel; }
      set { _selectedLabel = value; }
    }
    #endregion

    #region Public Methods
    public virtual void Reset()
    {
      LoadSkin();
      AllocResources();
      InitControls();
      _selectedLabel = -1;
    }

    public void DoModal(int ParentID)
    {
      _parentWindowID = ParentID;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowID);
      if (_parentWindow == null)
      {
        _parentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);
      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);
      GUIWindowManager.IsSwitchingToNewWindow = false;

      _running = true;
      while (_running && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }

    #endregion

    #region Protected Methods
    protected void SetControlLabel(int WindowID, int ControlID, string LabelText)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, WindowID, 0, ControlID, 0, 0, null);
      msg.Label = LabelText;
      OnMessage(msg);
    }

    protected void HideControl(int WindowID, int ControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN, WindowID, 0, ControlID, 0, 0, null);
      OnMessage(msg);
    }

    protected void ShowControl(int WindowID, int ControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, WindowID, 0, ControlID, 0, 0, null);
      OnMessage(msg);
    }

    protected void DisableControl(int WindowID, int ControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLED, WindowID, 0, ControlID, 0, 0, null);
      OnMessage(msg);
    }

    protected void EnableControl(int WindowID, int ControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLED, WindowID, 0, ControlID, 0, 0, null);
      OnMessage(msg);
    }
    
    protected void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (thisLock)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);
        GUIWindowManager.UnRoute();
        _parentWindow = null;
        _running = false;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }


    #endregion

    #region <Base class> Overloads
    #region SupportsDelayedLoad
    public override bool SupportsDelayedLoad
    {
      get { return true; }
    }
    #endregion
    #region PreInit
    public override void PreInit()
    {
    }
    #endregion
    #region OnAction
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || 
          action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || 
          action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }
    #endregion
    #region OnMessage
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            _prevOverlay = GUIGraphicsContext.Overlay;
            base.OnMessage(message);
            GUIGraphicsContext.Overlay = base.IsOverlayAllowed;
            //Reset();
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            base.OnMessage(message);
            _parentWindow = null;
            _running = false;
            GUIGraphicsContext.Overlay = _prevOverlay;
            FreeResources();
            DeInitControls();
            GUILayerManager.UnRegisterLayer(this);
            return true;
          }
      }
      return base.OnMessage(message);
    }
    #endregion
    #endregion

    #region <Interface> Implementations
    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }
    #endregion
    #endregion
  }

  
  public class GUIDialogTVConflict : GUIDialogWindow
  {
    #region Enums
    enum Controls
    {
      LIST = 3,
      HEADING = 4,
      BUTTON_NEW_REC = 11,
      BUTTON_CONFLICT_REC = 12,
      BUTTON_KEEP_CONFLICT = 13
    };
    #endregion

    #region Variables
    // Private Variables
    // Protected Variables
    // Public Variables
    #endregion

    #region Constructors/Destructors
    public GUIDialogTVConflict()
    {
      GetID = (int)GUIWindow.Window.WINDOW_DIALOG_TVCONFLICT;
    }

    #endregion

    #region Public Methods
    public void SetHeading(string HeadingText)
    {
      SetControlLabel(GetID, (int)Controls.HEADING, HeadingText);
    }

    public void AddConflictRecordings(List<TVRecording> conflicts)
    {
      if ((conflicts == null) || (conflicts.Count < 1)) return;
      GUIListControl list = (GUIListControl)GetControl((int)Controls.LIST);

      if (list != null)
      {
        foreach (TVRecording conflict in conflicts)
        {
          GUIListItem item = new GUIListItem(conflict.Title);
          item.Label2 = GetRecordingDateTime(conflict);
          item.Label3 = conflict.Channel;
          item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(OnListItemSelected);
          string logo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, conflict.Channel);
          if (!System.IO.File.Exists(logo))
          {
            logo = "defaultVideoBig.png";
          }
          item.ThumbnailImage = logo;
          item.IconImageBig = logo;
          item.IconImage = logo;
          item.TVTag = conflict;
          list.Add(item);
        }
      }
    }
       
    #endregion

    #region Private Methods
    private void OnListItemSelected(GUIListItem item, GUIControl parent)
    {
      if ((item == null) || (item.TVTag == null)) return;
      // to be implemented
    }

    private string GetRecordingDateTime(TVRecording rec)
    {
      return String.Format("{0} {1} - {2}",
                MediaPortal.Util.Utils.GetShortDayString(rec.StartTime),
                rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));      
    }

    #endregion

    #region <Base class> Overloads
    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogTVConflict.xml");
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl = message.SenderControlId;
          if ((int)Controls.BUTTON_NEW_REC == iControl)
          {
            SelectedLabel = 0;
            Close();
          }
          else if ((int)Controls.BUTTON_CONFLICT_REC == iControl)
          {
            SelectedLabel = 1;
            Close();
          }
          else if ((int)Controls.BUTTON_KEEP_CONFLICT == iControl)
          {
            SelectedLabel = 2;
            Close();
          }
          break;
      }
      return base.OnMessage(message);
    }
   
    #endregion

  }

}
