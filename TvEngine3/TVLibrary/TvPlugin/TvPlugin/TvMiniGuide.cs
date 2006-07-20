#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;

using TvDatabase;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

namespace TvPlugin
{
  /// <summary>
  /// GUIMiniGuide
  /// </summary>
  /// 
  public class TvMiniGuide : GUIWindow, IRenderLayer
  {
    // Member variables                                  
    [SkinControlAttribute(34)]
    protected GUIButtonControl cmdExit = null;
    [SkinControlAttribute(35)]
    protected GUIListControl lstChannels = null;
    [SkinControlAttribute(36)]
    protected GUISpinControl spinGroup = null;

    bool m_bRunning = false;
    int m_dwParentWindowID = 0;
    GUIWindow m_pParentWindow = null;
    List<Channel> tvChannelList = null;
    List<ChannelGroup> ChannelGroupList = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public TvMiniGuide()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MINI_GUIDE;
    }
    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_MINI_GUIDE, this);
    }
    public override bool SupportsDelayedLoad
    {
      get
      {
        return false;
      }
    }

    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Init method
    /// </summary>
    /// <returns></returns>
    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\TVMiniGuide.xml");
      GetID = (int)GUIWindow.Window.WINDOW_MINI_GUIDE;
      
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      return bResult;
    }


    /// <summary>
    /// Renderer
    /// </summary>
    /// <param name="timePassed"></param>
    public override void Render(float timePassed)
    {
      base.Render(timePassed);		// render our controls to the screen
    }

    /// <summary>
    /// On close
    /// </summary>
    void Close()
    {
      Log.Write("miniguide:close()");
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        m_bRunning = false;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    /// <summary>
    /// On Message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            if (message.SenderControlId == 35) // listbox
            {
              if ((int)Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
              {
                // switching logic
                TVHome.Navigator.ZapToChannel(lstChannels.SelectedListItem.Label2, false);
                TVHome.Navigator.ZapNow();
                Close();
              }
            }
            else if (message.SenderControlId == 36) // spincontrol
            {
              // switch group
              TVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
              FillChannelList();
            }
            else if (message.SenderControlId == 34) // exit button
            {
              // exit
              Close();
            }
            break;
          }
      }
      return base.OnMessage(message);
    }

    /// <summary>
    /// On action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_CONTEXT_MENU:
          //m_bRunning = false;
          Close();
          return;
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          //m_bRunning = false;
          Close();
          return;
        case Action.ActionType.ACTION_MOVE_LEFT:
          // switch group
          spinGroup.MoveUp();
          TVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
          FillChannelList();
          return;
        case Action.ActionType.ACTION_MOVE_RIGHT:
          // switch group
          spinGroup.MoveDown();
          TVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
          FillChannelList();
          return;
      }
      base.OnAction(action);
    }

    /// <summary>
    /// Page gets destroyed
    /// </summary>
    /// <param name="new_windowId"></param>
    protected override void OnPageDestroy(int new_windowId)
    {
      Log.Write("miniguide OnPageDestroy");
      base.OnPageDestroy(new_windowId);
      m_bRunning = false;
    }

    /// <summary>
    /// Page gets loaded
    /// </summary>
    protected override void OnPageLoad()
    {
      Log.Write("miniguide onpageload");
      // following line should stay. Problems with OSD not
      // appearing are already fixed elsewhere
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      AllocResources();
      ResetAllControls();							// make sure the controls are positioned relevant to the OSD Y offset
      FillChannelList();
      FillGroupList();
      base.OnPageLoad();
    }

    /// <summary>
    /// Fill up the list with groups
    /// </summary>
    public void FillGroupList()
    {
      ChannelGroup current = null;
      ChannelGroupList = TVHome.Navigator.Groups;
      // empty list of channels currently in the 
      // spin control
      spinGroup.Reset();
      // start to fill them up again
      for (int i = 0; i < ChannelGroupList.Count; i++)
      {
        current = ChannelGroupList[i];
        spinGroup.AddLabel(current.GroupName, i);
        // set selected
        if (current.GroupName.CompareTo(TVHome.Navigator.CurrentGroup.GroupName) == 0)
          spinGroup.Value = i;
      }
    }

    /// <summary>
    /// Fill the list with channels
    /// </summary>
    public void FillChannelList()
    {
      tvChannelList = new List<Channel>();
      foreach (GroupMap map in TVHome.Navigator.CurrentGroup.GroupMaps)
      {
        tvChannelList.Add(map.Channel);
      }
      lstChannels.Clear();
      Channel current = null;
      GUIListItem item = null;
      Program prog = null;
      Program prognext = null;
      string logo = "";
      int selected = 0;

      for (int i = 0; i < tvChannelList.Count; i++)
      {
        current = tvChannelList[i];
        if (current.VisibleInGuide)
        {
          item = new GUIListItem("");
          item.Label2 = current.Name;
          logo = Utils.GetCoverArt(Thumbs.TVChannel, current.Name);

          // if we are watching this channel mark it
          if (TVHome.Navigator.CurrentChannel.CompareTo(tvChannelList[i].Name) == 0)
          {
            item.IsRemote = true;
            selected = lstChannels.Count;
          }

          if (System.IO.File.Exists(logo))
          {
            item.IconImageBig = logo;
            item.IconImage = logo;
          }
          else
          {
            item.IconImageBig = string.Empty;
            item.IconImage = string.Empty;
          }
          prog = GetCurrentProgram(current);
          if (prog != null)
          {
            //                    item.Label3 = prog.Title + " [" + prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "-" + prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "]";
            item.Label3 = prog.Title + ": " + CalculateProgress(prog).ToString() + "%";
          }
          prognext = GetNextProgram(current, prog);
          if (prognext != null)
          {
            //                    item.Label = prognext.Title + " [" + prognext.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "-" + prognext.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "]";
            item.Label = prognext.Title;
          }
          lstChannels.Add(item);
          lstChannels.SelectedListItemIndex = selected;
        }
      }
    }

    /// <summary>
    /// Get current tv program
    /// </summary>
    /// <param name="prog"></param>
    /// <returns></returns>
    private double CalculateProgress(Program prog)
    {
      TimeSpan length = prog.EndTime - prog.StartTime;
      TimeSpan passed = DateTime.Now - prog.StartTime;
      if (length.TotalMinutes > 0)
      {
        double fprogress = (passed.TotalMinutes / length.TotalMinutes) * 100;
        fprogress = Math.Floor(fprogress);
        if (fprogress > 100.0f) return 100.0f;
        return fprogress;
      }
      else
        return 0;
    }

    /// <summary>
    /// Get current tv program
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    private Program GetCurrentProgram(Channel channel)
    {
      //DateTime timeshiftStart = Recorder.TimeTimeshiftingStarted; 
      //DateTime livePoint = timeshiftStart.AddSeconds(g_Player.CurrentPosition);
      //livePoint = livePoint.AddSeconds(g_Player.ContentStart);
      Program prog = channel.CurrentProgram;
      return prog;
    }

    /// <summary>
    /// Get next tv program
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="based"></param>
    /// <returns></returns>
    private Program GetNextProgram(Channel channel, Program based)
    {
      if (based == null) return null;
      Program prognext = channel.GetProgramAt(based.EndTime.AddMinutes(1));
      return prognext;
    }

    /// <summary>
    /// Do this modal
    /// </summary>
    /// <param name="dwParentId"></param>
    public void DoModal(int dwParentId)
    {
      Log.Write("miniguide domodal");
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        Log.Write("parentwindow=0");
        m_dwParentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // activate this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, -1, 0, null);
      OnMessage(msg);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      m_bRunning = true;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
        if (!GUIGraphicsContext.Vmr9Active)
          System.Threading.Thread.Sleep(50);
      }
      GUILayerManager.UnRegisterLayer(this);

      Log.Write("miniguide closed");
    }

    // Overlay IRenderLayer members
    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      if (m_bRunning)
        Render(timePassed);
    }
    #endregion
  }
}