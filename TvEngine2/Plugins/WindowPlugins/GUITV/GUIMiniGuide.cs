#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// GUIMiniGuide
  /// </summary>
  /// 
  public class GUIMiniGuide : GUIWindow, IRenderLayer
  {
    // Member variables                                  
    [SkinControl(34)] protected GUIButtonControl cmdExit = null;
    [SkinControl(35)] protected GUIListControl lstChannels = null;
    [SkinControl(36)] protected GUISpinControl spinGroup = null;

    private bool m_bRunning = false;
    private bool _altLayout = false;
    private int m_dwParentWindowID = 0;
    private GUIWindow m_pParentWindow = null;
    private List<TVChannel> tvChannelList = null;
    private List<TVGroup> tvGroupList = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public GUIMiniGuide()
    {
      GetID = (int) Window.WINDOW_MINI_GUIDE;
    }

    /// <summary>
    /// Init method
    /// </summary>
    /// <returns></returns>
    public override bool Init()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _altLayout = xmlreader.GetValueAsBool("mytve2", "altminiguide", true);
      }
      bool bResult = Load(GUIGraphicsContext.Skin + @"\TVMiniGuide_TVE2.xml");

      GetID = (int) Window.WINDOW_MINI_GUIDE;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      return bResult;
    }

    /// <summary>
    /// Delayed load
    /// </summary>
    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    /// <summary>
    /// Renderer
    /// </summary>
    /// <param name="timePassed"></param>
    public override void Render(float timePassed)
    {
      base.Render(timePassed); // render our controls to the screen
    }

    /// <summary>
    /// On close
    /// </summary>
    private void Close()
    {
      Log.Debug("miniguide:close()");
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
              if ((int) Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
              {
                // switching logic
                string selectedChan = (string) lstChannels.SelectedListItem.TVTag;
                if (GUITVHome.Navigator.CurrentChannel != selectedChan)
                {
                  GUITVHome.Navigator.ZapToChannel(selectedChan, false);
                  GUITVHome.Navigator.ZapNow();
                }
                Close();
              }
            }
            else if (message.SenderControlId == 36) // spincontrol
            {
              // switch group
              GUIPropertyManager.SetProperty("#TV.Guide.Group", spinGroup.GetLabel());
              GUITVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
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
          GUIPropertyManager.SetProperty("#TV.Guide.Group", spinGroup.GetLabel());
          GUITVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
          FillChannelList();
          return;
        case Action.ActionType.ACTION_MOVE_RIGHT:
          // switch group
          spinGroup.MoveDown();
          GUIPropertyManager.SetProperty("#TV.Guide.Group", spinGroup.GetLabel());
          GUITVHome.Navigator.SetCurrentGroup(spinGroup.GetLabel());
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
      Log.Debug("miniguide OnPageDestroy");
      base.OnPageDestroy(new_windowId);
      m_bRunning = false;
    }

    /// <summary>
    /// Page gets loaded
    /// </summary>
    protected override void OnPageLoad()
    {
      Log.Debug("miniguide onpageload");
      // following line should stay. Problems with OSD not
      // appearing are already fixed elsewhere
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      AllocResources();
      ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
      FillChannelList();
      FillGroupList();
      base.OnPageLoad();
    }

    /// <summary>
    /// Fill up the list with groups
    /// </summary>
    public void FillGroupList()
    {
      TVGroup current = null;
      tvGroupList = GUITVHome.Navigator.Groups;
      // empty list of channels currently in the 
      // spin control
      spinGroup.Reset();
      // start to fill them up again
      for (int i = 0; i < tvGroupList.Count; i++)
      {
        current = tvGroupList[i];
        spinGroup.AddLabel(current.GroupName, i);
        // set selected
        if (current.GroupName.CompareTo(GUITVHome.Navigator.CurrentGroup.GroupName) == 0)
        {
          spinGroup.Value = i;
        }
      }
    }

    /// <summary>
    /// Fill the list with channels
    /// </summary>
    public void FillChannelList()
    {
      tvChannelList = GUITVHome.Navigator.CurrentGroup.TvChannels;
      lstChannels.Clear();
      TVChannel current = null;
      GUIListItem item = null;
      TVProgram prog = null;
      TVProgram prognext = null;
      string logo = "";
      int selected = 0;

      for (int i = 0; i < tvChannelList.Count; i++)
      {
        current = tvChannelList[i];
        StringBuilder sb = new StringBuilder();

        if (current.VisibleInGuide)
        {
          item = new GUIListItem("");
          // store here as it is not needed right now - please beat me later..
          item.TVTag = current.Name;
          if (!_altLayout)
          {
            item.Label2 = current.Name;
          }

          logo = Util.Utils.GetCoverArt(Thumbs.TVChannel, current.Name);

          // if we are watching this channel mark it
          if (GUITVHome.Navigator.CurrentChannel.CompareTo(tvChannelList[i].Name) == 0)
          {
            item.IsRemote = true;
            selected = lstChannels.Count;
          }

          if (File.Exists(logo))
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

          if (prog == null || prog.Title == string.Empty)
          {
            item.Label2 = current.Name;
          }

          if (prog != null)
          {
            //                    item.Label3 = prog.Title + " [" + prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "-" + prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "]";
            if (_altLayout)
            {
              item.Label3 = GUILocalizeStrings.Get(789) + prog.Title;
              sb.Append(current.Name);
              sb.Append(" - ");
              sb.Append(CalculateProgress(prog).ToString());
              sb.Append("%");
              item.Label2 = sb.ToString();
            }
            else
            {
              item.Label3 = prog.Title + ": " + CalculateProgress(prog).ToString() + "%";
            }
          }
          prognext = GetNextProgram(current, prog);
          if (prognext != null)
          {
            //                    item.Label = prognext.Title + " [" + prognext.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "-" + prognext.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "]";
            if (!_altLayout)
            {
              item.Label = prognext.Title;
            }
            else
            {
              item.Label = GUILocalizeStrings.Get(790) + prognext.Title;
            }
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
    private double CalculateProgress(TVProgram prog)
    {
      TimeSpan length = prog.EndTime - prog.StartTime;
      TimeSpan passed = DateTime.Now - prog.StartTime;
      if (length.TotalMinutes > 0)
      {
        double fprogress = (passed.TotalMinutes/length.TotalMinutes)*100;
        fprogress = Math.Floor(fprogress);
        if (fprogress > 100.0f)
        {
          return 100.0f;
        }
        return fprogress;
      }
      else
      {
        return 0;
      }
    }

    /// <summary>
    /// Get current tv program
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    private TVProgram GetCurrentProgram(TVChannel channel)
    {
      //DateTime timeshiftStart = Recorder.TimeTimeshiftingStarted; 
      //DateTime livePoint = timeshiftStart.AddSeconds(g_Player.CurrentPosition);
      //livePoint = livePoint.AddSeconds(g_Player.ContentStart);
      TVProgram prog = channel.CurrentProgram;
      return prog;
    }

    /// <summary>
    /// Get next tv program
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="based"></param>
    /// <returns></returns>
    private TVProgram GetNextProgram(TVChannel channel, TVProgram based)
    {
      if (based == null)
      {
        return null;
      }
      TVProgram prognext = channel.GetProgramAt(based.EndTime.AddMinutes(1));
      return prognext;
    }

    /// <summary>
    /// Do this modal
    /// </summary>
    /// <param name="dwParentId"></param>
    public void DoModal(int dwParentId)
    {
      Log.Debug("miniguide domodal");
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        Log.Debug("parentwindow=0");
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
      }
      GUILayerManager.UnRegisterLayer(this);

      Log.Debug("miniguide closed");
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
      {
        Render(timePassed);
      }
    }

    #endregion
  }
}