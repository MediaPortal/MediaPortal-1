#region Copyright (C) 2005-2021 Team MediaPortal

// Copyright (C) 2005-2021 Team MediaPortal
// http://www.team-mediaportal.com
//
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
//
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Gentle.Framework;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using TvControl;
using TvDatabase;
using Action = MediaPortal.GUI.Library.Action;

namespace TvPlugin
{
  /// <summary>
  ///
  /// </summary>
  ///
  public class TvZapOsd : GUIInternalWindow
  {
    [SkinControl(35)] protected GUIControl lblCurrentChannel = null;
    [SkinControl(36)] protected GUIControl lblOnTvNow = null;
    [SkinControl(37)] protected GUIControl lblOnTvNext = null;
    [SkinControl(100)] protected GUIControl lblCurrentTime = null;
    [SkinControl(101)] protected GUIControl lblStartTime = null;
    [SkinControl(102)] protected GUIControl lblEndTime = null;
    [SkinControl(39)] protected GUIImage imgRecIcon = null;
    [SkinControl(10)] protected GUIImage imgTvChannelLogo = null;
    [SkinControl(38)] protected GUIControl lblZapToChannelNo = null;

    private bool m_bNeedRefresh = false;
    private DateTime m_dateTime = DateTime.Now;
    private string channelName = string.Empty;
    private string channelNr = string.Empty;
    private int idChannel;
    private bool _byIndex = false;

    private TVHome.ChannelErrorInfo m_lastError;

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _byIndex = xmlreader.GetValueAsBool("mytv", "byindex", true);
      }
    }

    #endregion

    public TVHome.ChannelErrorInfo LastError
    {
      get { return m_lastError; }
      set
      {
        m_lastError = value;
      }
    }

    public TvZapOsd()
    {
      GetID = (int)Window.WINDOW_TVZAPOSD;
    }

    public override bool IsTv
    {
      get { return true; }
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\tvZAPOSD.xml"));
      GetID = (int)Window.WINDOW_TVZAPOSD;
      LoadSettings();
      return bResult;
    }


    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void Render(float timePassed)
    {
      UpdateProgressBar();
      Get_TimeInfo(); // show the time elapsed/total playing time
      base.Render(timePassed); // render our controls to the screen
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_SHOW_OSD:
          {
            return;
          }

        case Action.ActionType.ACTION_NEXT_CHANNEL:
          {
            OnNextChannel();
            return;
          }

        case Action.ActionType.ACTION_PREV_CHANNEL:
          {
            OnPreviousChannel();
            return;
          }

        case Action.ActionType.ACTION_CONTEXT_MENU:
          {
            if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
            {
              TvFullScreen tvWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)Window.WINDOW_TVFULLSCREEN);
              tvWindow.OnAction(new Action(Action.ActionType.ACTION_SHOW_OSD, 0, 0));
              tvWindow.OnAction(action);
            }
            return;
          }
      }

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      Log.Debug("zaposd pagedestroy");
      Dispose();
      base.OnPageDestroy(newWindowId);

      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + newWindowId));
    }

    protected override void OnPageLoad()
    {
      //Log.Debug("zaposd pageload");
      // following line should stay. Problems with OSD not
      // appearing are already fixed elsewhere
      
      // SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof (Channel));
      // sb.AddConstraint(Operator.Equals, "istv", 1);
      // sb.AddOrderByField(true, "sortOrder");
      // SqlStatement stmt = sb.GetStatement(true);
      // ObjectFactory.GetCollection(typeof (Channel), stmt.Execute());

      AllocResources();
      // if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(false);
      ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
      m_bNeedRefresh = false;
      m_dateTime = DateTime.Now;
      channelNr = GetChannelNumber();
      channelName = GetChannelName();
      idChannel = GetIdChannel();
      SetCurrentChannelLogo();
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
      
      Log.Debug("TVZapOSD:OnPageLoad(), channelName:{0}, channelNr:{1}, idChannel:{2}, useIndex:{3}",
               channelName, channelNr, idChannel, _byIndex);
    }

    private void Get_TimeInfo()
    {
      string strTime = channelName;
      Program prog = TVHome.Navigator.GetChannel(idChannel, true).CurrentProgram;
      if (prog != null)
      {
        strTime = String.Format("{0}-{1}",
                                prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
      }
      SetLabel(lblCurrentTime, strTime);
    }

    public override void ResetAllControls()
    {
      //reset all
      bool bOffScreen = false;
      int iCalibrationY = GUIGraphicsContext.OSDOffset;
      int iTop = GUIGraphicsContext.OverScanTop;
      int iMin = 0;

      foreach (CPosition pos in _listPositions)
      {
        pos.control.SetPosition((int)pos.XPos, (int)pos.YPos + iCalibrationY);
      }
      foreach (CPosition pos in _listPositions)
      {
        GUIControl pControl = pos.control;

        int dwPosY = pControl.YPosition;
        if (pControl.IsVisible)
        {
          if (dwPosY < iTop)
          {
            int iSize = iTop - dwPosY;
            if (iSize > iMin)
            {
              iMin = iSize;
            }
            bOffScreen = true;
          }
        }
      }
      if (bOffScreen)
      {
        foreach (CPosition pos in _listPositions)
        {
          GUIControl pControl = pos.control;
          int dwPosX = pControl.XPosition;
          int dwPosY = pControl.YPosition;
          if (dwPosY < (int)100)
          {
            dwPosY += Math.Abs(iMin);
            pControl.SetPosition(dwPosX, dwPosY);
          }
        }
      }
      base.ResetAllControls();
    }


    public override bool NeedRefresh()
    {
      if (m_bNeedRefresh)
      {
        m_bNeedRefresh = false;
        return true;
      }
      return false;
    }

    private void OnPreviousChannel()
    {
      Log.Debug("GUITV OSD: OnPreviousChannel");
      if (!TVHome.Card.IsTimeShifting)
      {
        return;
      }
      TVHome.Navigator.ZapToPreviousChannel(true);
      channelNr = GetChannelNumber();
      channelName = GetChannelName();
      idChannel = GetIdChannel();
      SetCurrentChannelLogo();
      m_dateTime = DateTime.Now;
    }

    private void OnNextChannel()
    {
      Log.Debug("GUITV ZAPOSD: OnNextChannel");
      if (!TVHome.Card.IsTimeShifting)
      {
        return;
      }
      TVHome.Navigator.ZapToNextChannel(true);
      channelNr = GetChannelNumber();
      channelName = GetChannelName();
      idChannel = GetIdChannel();
      SetCurrentChannelLogo();
      m_dateTime = DateTime.Now;
    }

    public void UpdateChannelInfo()
    {
      if (LastError == null)
      {
        channelNr = GetChannelNumber();
      }
      channelName = GetChannelName();
      idChannel = GetIdChannel();
      SetCurrentChannelLogo();
    }


    private void SetCurrentChannelLogo()
    {
      if (imgTvChannelLogo != null)
      {
        string strLogo = null;
        if (LastError != null)
        {
          strLogo = TVUtil.GetChannelLogo(LastError.FailingChannel);
        }
        else
        {
          strLogo = TVUtil.GetChannelLogo(TVHome.Navigator.ZapChannel);
        }
  
        if (string.IsNullOrEmpty(strLogo))
        {
          imgTvChannelLogo.SetFileName(string.Empty);
          imgTvChannelLogo.IsVisible = false;
        }
        else
        {
          imgTvChannelLogo.SetFileName(strLogo);
          //img.SetPosition(GUIGraphicsContext.OverScanLeft, GUIGraphicsContext.OverScanTop);
          m_bNeedRefresh = true;
          //imgTvChannelLogo.IsVisible = true;
        }
      }
      
      ShowPrograms();
    }

    private string GetChannelName()
    {
      if (LastError != null)
      {
        return LastError.FailingChannel.DisplayName;
      }
      return TVHome.Navigator.ZapChannel.DisplayName;
    }

    private string GetChannelNumber()
    {
      int zapChannelNr;
      if (_byIndex)
      {
        zapChannelNr = TVHome.Navigator.ZapChannelIdx;
      }
      else
      {
        zapChannelNr = TVHome.Navigator.ZapChannelNr;
      }
      
      if (zapChannelNr<0)
      {
        return string.Empty;
      }
      return zapChannelNr.ToString();
    }

    private int GetIdChannel()
    {
      if (LastError != null)
      {
        return LastError.FailingChannel.IdChannel;
      }
      return TVHome.Navigator.ZapChannel.IdChannel;
    }

    private void ShowPrograms()
    {
      SetLabel(lblOnTvNow, string.Empty);
      SetLabel(lblOnTvNext, string.Empty);

      // Set recorder status
      if (imgRecIcon != null)
      {
        VirtualCard card;
        TvServer server = new TvServer();
        imgRecIcon.IsVisible = server.IsRecording(idChannel, out card);
      }

      if (lblZapToChannelNo != null)
      {
        if (string.IsNullOrEmpty(channelNr))
        {
          SetLabel(lblZapToChannelNo, string.Empty);
          lblZapToChannelNo.Visible = false;
        }
        else
        {
          SetLabel(lblZapToChannelNo, channelNr);
          lblZapToChannelNo.Visible = true;
        }
      }
      var chan = TVHome.Navigator.GetChannel(idChannel, true);
      if (chan != null)
      {
        var prog = chan.GetProgramAt(m_dateTime);
        if (LastError != null)
        {
          SetLabel(lblStartTime, string.Empty);
          SetLabel(lblEndTime, string.Empty);
          if (LastError.FailingChannel != null)
          {
            SetLabel(lblCurrentChannel, LastError.FailingChannel.DisplayName);
          }
          if (LastError.Messages.Count > 0)
          {
            SetLabel(lblOnTvNow, LastError.Messages[0]); // first line in "NOW"
            if (LastError.Messages.Count > 1)
            {
              SetLabel(lblOnTvNext, String.Join(", ", LastError.Messages.ToArray(), 1, LastError.Messages.Count - 1));
              // 2nd and later in "NEXT"
            }
          }
          m_lastError = null; // reset member only, not the failing channel info in Navigator
        }
        else
        {
          SetLabel(lblCurrentChannel, channelName);
          if (prog != null)
          {
            string strTime = String.Format("{0}-{1}",
              prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

            SetLabel(lblCurrentTime, strTime);
            SetLabel(lblOnTvNow, prog.Title);
            strTime = String.Format("{0}", prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            SetLabel(lblStartTime, strTime);
            strTime = String.Format("{0} ", prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            SetLabel(lblEndTime, strTime);

            // next program
            prog = chan.GetProgramAt(prog.EndTime.AddMinutes(1));
            //prog = TVHome.Navigator.GetChannel(channelName).GetProgramAt(prog.EndTime.AddMinutes(1));
            if (prog != null)
            {
              SetLabel(lblOnTvNext, prog.Title);
            }
          }
          else
          {
            SetLabel(lblOnTvNow, GUILocalizeStrings.Get(736)); // no epg for this channel
            SetLabel(lblStartTime, string.Empty);
            SetLabel(lblEndTime, string.Empty);
            SetLabel(lblCurrentTime, string.Empty);
          }
        }
      }
      UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
      double fPercent = 0;
      var program = TVHome.Navigator.GetChannel(idChannel, true).CurrentProgram;
      if (program != null)
      {
        var currentProgram = program;
        if (currentProgram != null)
        {
          Program prog = currentProgram;
          string strTime = String.Format("{0}-{1}",
            prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
            prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

          TimeSpan ts = prog.EndTime - prog.StartTime;
          double iTotalSecs = ts.TotalSeconds;
          ts = DateTime.Now - prog.StartTime;
          double iCurSecs = ts.TotalSeconds;
          fPercent = ((double)iCurSecs) / ((double)iTotalSecs);
        }
      }
      fPercent *= 100.0d;
      GUIPropertyManager.SetProperty("#TV.View.Percentage", fPercent.ToString());
    }

    private void SetLabel(GUIControl control, string value, bool translate = false)
    {
      if (control == null)
      {
        return;
      }

      if (translate && !string.IsNullOrEmpty(value))
      {
        value = value.ToValue() ?? value;
      }

      var cf = control as GUIFadeLabel;
      if (cf != null) cf.Label = value;
      var cl = control as GUILabelControl;
      if (cl != null) cl.Label = value;
      var ct = control as GUITextControl;
      if (ct != null)
      { 
        ct.EnableUpDown = false;
        ct.Label = value;
      }
    }
  }
}
