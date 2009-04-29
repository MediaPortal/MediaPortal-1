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
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Shows a dialog box with an OK button  
  /// </summary>
  public class GUIDialogDateTime : GUIDialogWindow
  {
    private readonly int[] months = new int[] {0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};

    [SkinControl(1)] protected GUISpinControl spinChannel = null;
    [SkinControl(2)] protected GUISpinControl spinStartHour = null;
    [SkinControl(3)] protected GUISpinControl spinStartMinute = null;
    [SkinControl(4)] protected GUISpinControl spinStartDay = null;
    [SkinControl(5)] protected GUISpinControl spinStartMonth = null;
    [SkinControl(6)] protected GUISpinControl spinStartYear = null;
    [SkinControl(7)] protected GUISpinControl spinEndHour = null;
    [SkinControl(8)] protected GUISpinControl spinEndMinute = null;
    [SkinControl(9)] protected GUISpinControl spinEndDay = null;
    [SkinControl(10)] protected GUISpinControl spinEndMonth = null;
    [SkinControl(11)] protected GUISpinControl spinEndYear = null;
    [SkinControl(12)] protected GUIButtonControl btnOK = null;
    [SkinControl(13)] protected GUILabelControl lblHeading = null;

    private bool _confirmed = false;
    private string channel = string.Empty;
    private bool enableEditStartTime = true;
    private bool enableEditChannel = true;
    private DateTime startDateTime = DateTime.Now;
    private DateTime endDateTime = DateTime.Now;
    private ArrayList itemList = new ArrayList();

    public GUIDialogDateTime()
    {
      GetID = (int) Window.WINDOW_DIALOG_DATETIME;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogDateTime.xml");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      int iYear, iMonth;
      if (control == spinStartMonth)
      {
        iYear = spinStartYear.Value;
        iMonth = spinStartMonth.Value;
        if (iMonth == 2 && DateTime.IsLeapYear(iYear))
        {
          spinStartDay.SetRange(1, 29);
        }
        else
        {
          spinStartDay.SetRange(1, months[iMonth]);
        }
      }
      if (control == spinEndMonth)
      {
        iYear = spinEndYear.Value;
        iMonth = spinEndMonth.Value;
        if (iMonth == 2 && DateTime.IsLeapYear(iYear))
        {
          spinEndDay.SetRange(1, 29);
        }
        else
        {
          spinEndDay.SetRange(1, months[iMonth]);
        }
      }
      if (control == btnOK)
      {
        int iHour;
        iHour = spinStartHour.Value;
        int iMin;
        iMin = spinStartMinute.Value;
        int iDay;
        iDay = spinStartDay.Value;
        iMonth = spinStartMonth.Value;
        iYear = spinStartYear.Value;
        channel = spinChannel.GetLabel();

        startDateTime = new DateTime(iYear, iMonth, iDay, iHour, iMin, 0, 0);
        iHour = spinEndHour.Value;
        iMin = spinEndMinute.Value;
        iDay = spinEndDay.Value;
        iMonth = spinEndMonth.Value;
        iYear = spinEndYear.Value;
        endDateTime = new DateTime(iYear, iMonth, iDay, iHour, iMin, 0, 0);

        _confirmed = true;
        PageDestroy();
        return;
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            _confirmed = false;
            spinStartHour.SetRange(0, 23);
            spinStartHour.Value = startDateTime.Hour;

            spinStartMinute.SetRange(0, 59);
            spinStartMinute.Value = startDateTime.Minute;

            if (DateTime.IsLeapYear(startDateTime.Year) && startDateTime.Month == 2)
            {
              spinStartDay.SetRange(1, 29);
            }
            else
            {
              spinStartDay.SetRange(1, months[startDateTime.Month]);
            }
            spinStartDay.Value = startDateTime.Day;

            spinStartMonth.SetRange(1, 12);
            spinStartMonth.Value = startDateTime.Month;

            spinStartYear.Value = startDateTime.Year;
            spinStartYear.SetRange(2004, 2010);


            spinChannel.Reset();
            int i = 0, iSel = 0;
            foreach (string strLabel in itemList)
            {
              spinChannel.AddLabel(strLabel, 0);
              if (channel == strLabel)
              {
                iSel = i;
              }
              i++;
            }
            if (iSel >= 0)
            {
              SelectItemControl(GetID, spinChannel.GetID, iSel);
            }

            spinEndHour.SetRange(0, 23);
            spinEndHour.Value = endDateTime.Hour;

            spinEndMinute.SetRange(0, 59);
            spinEndMinute.Value = endDateTime.Minute;

            if (DateTime.IsLeapYear(endDateTime.Year) && endDateTime.Month == 2)
            {
              spinEndDay.SetRange(1, 29);
            }
            else
            {
              spinEndDay.SetRange(1, months[endDateTime.Month]);
            }
            spinEndDay.Value = endDateTime.Day;


            spinEndMonth.SetRange(1, 12);
            spinEndMonth.Value = endDateTime.Month;


            spinEndYear.Value = endDateTime.Year;
            spinEndYear.SetRange(2004, 2010);

            spinStartHour.Disabled = !enableEditStartTime;
            spinStartMinute.Disabled = !enableEditStartTime;
            spinStartDay.Disabled = !enableEditStartTime;
            spinStartMonth.Disabled = !enableEditStartTime;
            spinStartYear.Disabled = !enableEditStartTime;

            spinChannel.Disabled = !enableEditChannel;
          }
          return true;
      }
      return base.OnMessage(message);
    }


    public bool IsConfirmed
    {
      get { return _confirmed; }
    }

    private void SelectItemControl(int iWindowId, int iControlId, int iItem)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, iWindowId, 0, iControlId, iItem, 0,
                                      null);
      OnMessage(msg);
    }

    public ArrayList Items
    {
      get { return itemList; }
    }

    public DateTime StartDateTime
    {
      get { return startDateTime; }
      set { startDateTime = value; }
    }

    public DateTime EndDateTime
    {
      get { return endDateTime; }
      set { endDateTime = value; }
    }

    public void SetHeading(int iString)
    {
      SetHeading(GUILocalizeStrings.Get(iString));
    }

    public void SetHeading(string line)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      lblHeading.Label = line;
    }

    public string Channel
    {
      get { return channel; }
      set { channel = value; }
    }

    public bool EnableStartTime
    {
      get { return enableEditStartTime; }
      set { enableEditStartTime = value; }
    }

    public bool EnableChannel
    {
      get { return enableEditChannel; }
      set { enableEditChannel = value; }
    }
  }
}