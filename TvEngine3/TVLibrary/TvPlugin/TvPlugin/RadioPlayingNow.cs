#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using TvDatabase;
using Action = MediaPortal.GUI.Library.Action;
using System.Globalization;

namespace TvPlugin
{
  public class RadioHelper
  {
    public static Channel CurrentChannel { get; set; } = null;
    public static RadioChannelGroup SelectedGroup { get; set; }

    public static void SetRadioProperties()
    {

      if (CurrentChannel.IsWebstream() || CurrentChannel.CurrentProgram == null || CurrentChannel.NextProgram == null ||
        string.IsNullOrEmpty(CurrentChannel.CurrentProgram.Title) || string.IsNullOrEmpty(CurrentChannel.NextProgram.Title) ||
        !(g_Player.IsRadio && g_Player.Playing))
      {
        GUIPropertyManager.SetProperty("#Radio.Listen.Title", String.Empty);
        GUIPropertyManager.SetProperty("#Radio.Listen.Channel", CurrentChannel.DisplayName);
        GUIPropertyManager.SetProperty("#Radio.Listen.Genre", String.Empty);
        GUIPropertyManager.SetProperty("#Radio.Listen.Group", SelectedGroup?.GroupName);
        GUIPropertyManager.SetProperty("#Radio.Listen.Played", GUIPropertyManager.GetProperty("#currentplaytime"));
        GUIPropertyManager.SetProperty("#Radio.Listen.Description", String.Empty);
        GUIPropertyManager.SetProperty("#Radio.Listen.Start", String.Empty);
        GUIPropertyManager.SetProperty("#Radio.Listen.Stop", String.Empty);
        GUIPropertyManager.SetProperty("#Radio.Listen.Remaining", String.Empty);

        GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentChannel.DisplayName); // No EPG
        GUIPropertyManager.SetProperty("#Play.Next.Title", string.Empty);
      }
      else
      {
        GUIPropertyManager.SetProperty("#Radio.Listen.Title", CurrentChannel.CurrentProgram.Title);
        GUIPropertyManager.SetProperty("#Radio.Listen.Channel", CurrentChannel.DisplayName);
        GUIPropertyManager.SetProperty("#Radio.Listen.Genre", CurrentChannel.CurrentProgram.Genre);
        GUIPropertyManager.SetProperty("#Radio.Listen.Group", SelectedGroup?.GroupName);
        GUIPropertyManager.SetProperty("#Radio.Listen.Description", CurrentChannel.CurrentProgram.Description);
        GUIPropertyManager.SetProperty("#Radio.Listen.Start", CurrentChannel.CurrentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat).ToString());
        GUIPropertyManager.SetProperty("#Radio.Listen.Stop", CurrentChannel.CurrentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat).ToString());
        var remaining = CurrentChannel.CurrentProgram.CalculateTimeRemaining();
        GUIPropertyManager.SetProperty("#Radio.Listen.Remaining", Utils.SecondsToHMSString(remaining));
        GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentChannel.CurrentProgram.Title);
        GUIPropertyManager.SetProperty("#Play.Current.Genre", CurrentChannel.CurrentProgram.Genre);
        GUIPropertyManager.SetProperty("#Play.Next.Title", CurrentChannel.NextProgram.Title);
        var played = (CurrentChannel.CurrentProgram.EndTime - CurrentChannel.CurrentProgram.StartTime) - remaining;
        GUIPropertyManager.SetProperty("#Radio.Listen.Played", Utils.SecondsToHMSString(played));
      }
    }

    static DateTime _updateTimer = DateTime.MinValue;
    public static void Process()
    {
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds < 700)
      {
        return;
      }
      try
      {
        SetRadioProperties();
      }
      finally
      {
        _updateTimer = DateTime.Now;
      }
    }

  }

  public class RadioPlayingNow : GUIInternalWindow
  {
    #region Base variables
    #endregion
    public RadioPlayingNow()
    {
      GetID = (int)Window.WINDOW_RADIO_PLAYING_NOW;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\RadioPlayingNow.xml"));
    }

    public override void Process()
    {
      RadioHelper.Process();
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_STOP)
      {
        GUIWindowManager.ShowPreviousWindow();
      }

      base.OnAction(action);
    }
  }
}