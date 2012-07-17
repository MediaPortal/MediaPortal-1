#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TvDatabase;
using TvControl;
using TvService;

#endregion

namespace SetupTv.Sections
{
  public partial class UserPriorities : SectionSettings
  {
    #region CardInfo class

    public class CardInfo
    {
      public Card card;

      public CardInfo(Card newcard)
      {
        card = newcard;
      }

      public override string ToString()
      {
        return card.Name;
      }
    }

    #endregion

    #region Vars

    private bool _needRestart;

    #endregion

    #region Constructors

    public UserPriorities()
      : this("User Priorities") { }

    public UserPriorities(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Serialization

    public override void LoadSettings()
    {
      var layer = new TvBusinessLayer();      
      numEpgGrabber.Value = ValueSanityCheck(
        Convert.ToDecimal(layer.GetSetting(UserFactory.EPG_TAGNAME, UserFactory.EPG_PRIORITY.ToString()).Value), 1, 100);

      numDefaultUser.Value = ValueSanityCheck(
        Convert.ToDecimal(layer.GetSetting(UserFactory.USER_TAGNAME, UserFactory.USER_PRIORITY.ToString()).Value), 1, 100);

      numScheduler.Value = ValueSanityCheck(
        Convert.ToDecimal(layer.GetSetting(UserFactory.SCHEDULER_TAGNAME, UserFactory.SCHEDULER_PRIORITY.ToString()).Value), 1, 100);

      Setting setting = layer.GetSetting(UserFactory.CUSTOM_TAGNAME, "");
      gridUserPriorities.Rows.Clear();
      string[] users = setting.Value.Split(';');
      foreach (string user in users)
      {
        string[] shareItem = user.Split(',');
        if ((shareItem.Length.Equals(2)) &&
            ((shareItem[0].Trim().Length > 0) ||
             (shareItem[1].Trim().Length > 0)))
        {
          gridUserPriorities.Rows.Add(shareItem);
        }
      }
    }

    public override void SaveSettings()
    {
      var layer = new TvBusinessLayer();
      Setting s = layer.GetSetting("PriorityEPG", UserFactory.EPG_PRIORITY.ToString());
      s.Value = numEpgGrabber.Value.ToString();
      s.Persist();

      s = layer.GetSetting("PriorityUser", UserFactory.USER_PRIORITY.ToString());
      s.Value = numDefaultUser.Value.ToString();
      s.Persist();

      s = layer.GetSetting("PriorityScheduler", UserFactory.SCHEDULER_PRIORITY.ToString());
      s.Value = numScheduler.Value.ToString();
      s.Persist();

      Setting setting = layer.GetSetting("PrioritiesCustom", "");
      var shares = new StringBuilder();
      foreach (DataGridViewRow row in gridUserPriorities.Rows)
      {
        shares.AppendFormat("{0},{1};", row.Cells[0].Value, row.Cells[1].Value);
      }
      setting.Value = shares.ToString();
      setting.Persist();
    }

    #endregion

    #region GUI-Events

    public override void OnSectionActivated()
    {
      _needRestart = false;
      
      LoadSettings();
      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();

      SaveSettings();
      if (_needRestart)
      {
        RemoteControl.Instance.ClearCache();
        RemoteControl.Instance.Restart();
      }
    }
    
    private static decimal ValueSanityCheck(decimal Value, int Min, int Max)
    {
      if (Value < Min)
        return Min;
      if (Value > Max)
        return Max;
      return Value;
    }


    #endregion

    private void numEpgGrabber_ValueChanged(object sender, EventArgs e)
    {
      _needRestart = true;
    }

    private void numDefaultUser_ValueChanged(object sender, EventArgs e)
    {
      _needRestart = true;
    }

    private void numScheduler_ValueChanged(object sender, EventArgs e)
    {
      _needRestart = true;
    }

    private void gridUserPriorities_UserAddedRow(object sender, DataGridViewRowEventArgs e)
    {
      _needRestart = true;
    }

    private void gridUserPriorities_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
    {
      _needRestart = true;
    }

  }
}