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
using System.Text;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;

#endregion

namespace Mediaportal.TV.Server.SetupTV.Sections
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
            
      numEpgGrabber.Value = ValueSanityCheck(
        Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(UserFactory.EPG_TAGNAME, UserFactory.EPG_PRIORITY.ToString()).Value), 1, 100);

      numDefaultUser.Value = ValueSanityCheck(
        Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(UserFactory.USER_TAGNAME, UserFactory.USER_PRIORITY.ToString()).Value), 1, 100);

      numScheduler.Value = ValueSanityCheck(
        Convert.ToDecimal(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(UserFactory.SCHEDULER_TAGNAME, UserFactory.SCHEDULER_PRIORITY.ToString()).Value), 1, 100);

      Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(UserFactory.CUSTOM_TAGNAME, "");
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
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PriorityEPG", numEpgGrabber.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PriorityUser", numDefaultUser.Value.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PriorityScheduler", numScheduler.Value.ToString());
      

      
      var shares = new StringBuilder();
      foreach (DataGridViewRow row in gridUserPriorities.Rows)
      {
        shares.AppendFormat("{0},{1};", row.Cells[0].Value, row.Cells[1].Value);
      }

      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("PrioritiesCustom", shares.ToString());
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
        ServiceAgents.Instance.ControllerServiceAgent.Restart();
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