#region Copyright (C) 2005-2025 Team MediaPortal

// Copyright (C) 2005-2025 Team MediaPortal
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

using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.Dialogs
{
  public class GUIDialogUserRating : GUIDialogWindow
  {
    public GUIDialogUserRating()
    {
      GetID = (int)Window.WINDOW_DIALOG_USER_RATING;
    }

    public enum StarDisplay
    {
      FIVE_STARS = 5,
      TEN_STARS = 10
    }

    [SkinControlAttribute(6)]
    protected GUILabelControl lblText = null;
    [SkinControlAttribute(7)]
    protected GUILabelControl lblRating = null;
    [SkinControlAttribute(100)]
    protected GUICheckMarkControl btnStar1 = null;
    [SkinControlAttribute(101)]
    protected GUICheckMarkControl btnStar2 = null;
    [SkinControlAttribute(102)]
    protected GUICheckMarkControl btnStar3 = null;
    [SkinControlAttribute(103)]
    protected GUICheckMarkControl btnStar4 = null;
    [SkinControlAttribute(104)]
    protected GUICheckMarkControl btnStar5 = null;
    [SkinControlAttribute(105)]
    protected GUICheckMarkControl btnStar6 = null;
    [SkinControlAttribute(106)]
    protected GUICheckMarkControl btnStar7 = null;
    [SkinControlAttribute(107)]
    protected GUICheckMarkControl btnStar8 = null;
    [SkinControlAttribute(108)]
    protected GUICheckMarkControl btnStar9 = null;
    [SkinControlAttribute(109)]
    protected GUICheckMarkControl btnStar10 = null;

    public string Text
    {
      get
      {
        return lblText.Label;
      }

      set
      {
        lblText.Label = value;
      }
    }

    public StarDisplay DisplayStars
    {
      get
      {
        return _displayStars;
      }
      set
      {
        _displayStars = value;
      }
    }
    public StarDisplay _displayStars = StarDisplay.FIVE_STARS;

    public int Rating
    {
      get
      {
        return _rating;
      }
      set
      {
        if (DisplayStars == StarDisplay.FIVE_STARS)
        {
          _rating = System.Math.Min(5, System.Math.Max(value, 1));
        }
        else
        {
          _rating = System.Math.Min(10, System.Math.Max(value, 1));
        }
      }
    }
    public int _rating = 1;

    public bool IsSubmitted { get; set; }

    public override void Reset()
    {
      base.Reset();

      SetTitle(string.Empty);
      SetLine(1, string.Empty);
      SetLine(2, string.Empty);
      SetLine(3, string.Empty);
      SetLine(4, string.Empty);
    }

    public override void DoModal(int ParentID)
    {
      LoadSkin();
      AllocResources();
      InitControls();
      UpdateStarVisibility();

      base.DoModal(ParentID);
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\dialogUserRating.xml"));
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.REMOTE_1:
          _rating = 1;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_2:
          _rating = 2;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_3:
          _rating = 3;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_4:
          _rating = 4;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_5:
          _rating = 5;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_6:
          if (DisplayStars == StarDisplay.FIVE_STARS)
            break;
          _rating = 6;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_7:
          if (DisplayStars == StarDisplay.FIVE_STARS)
            break;
          _rating = 7;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_8:
          if (DisplayStars == StarDisplay.FIVE_STARS)
            break;
          _rating = 8;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_9:
          if (DisplayStars == StarDisplay.FIVE_STARS)
            break;
          _rating = 9;
          UpdateRating();
          break;
        case Action.ActionType.REMOTE_0:
          if (DisplayStars == StarDisplay.FIVE_STARS)
            break;
          _rating = 10;
          UpdateRating();
          break;
        case Action.ActionType.ACTION_SELECT_ITEM:
          IsSubmitted = true;
          PageDestroy();
          return;
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        case Action.ActionType.ACTION_CLOSE_DIALOG:
        case Action.ActionType.ACTION_CONTEXT_MENU:
          IsSubmitted = false;
          PageDestroy();
          return;
      }

      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnStar1)
      {
        _rating = 1;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar2)
      {
        _rating = 2;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar3)
      {
        _rating = 3;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar4)
      {
        _rating = 4;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar5)
      {
        _rating = 5;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar6)
      {
        _rating = 6;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar7)
      {
        _rating = 7;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar8)
      {
        _rating = 8;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar9)
      {
        _rating = 9;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
      else if (control == btnStar10)
      {
        _rating = 10;
        IsSubmitted = true;
        PageDestroy();
        return;
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          base.OnMessage(message);
          IsSubmitted = false;
          UpdateRating();
          return true;

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          if (message.TargetControlId < 100 || message.TargetControlId > (100 + (int)DisplayStars))
            break;

          _rating = message.TargetControlId - 99;
          UpdateRating();
          break;
      }
      return base.OnMessage(message);
    }

    private void UpdateRating()
    {
      GUICheckMarkControl[] btnStars;
      if (DisplayStars == StarDisplay.FIVE_STARS)
      {
        btnStars = new GUICheckMarkControl[5]
                {
                    btnStar1, btnStar2, btnStar3, btnStar4, btnStar5
                };
      }
      else
      {
        btnStars = new GUICheckMarkControl[10]
                {
                    btnStar1, btnStar2, btnStar3, btnStar4, btnStar5,
                    btnStar6, btnStar7, btnStar8, btnStar9, btnStar10
                };
      }

      for (int i = 0; i < (int)DisplayStars; i++)
      {
        btnStars[i].Label = string.Empty;
        btnStars[i].Selected = (_rating >= i + 1);
      }
      btnStars[_rating - 1].Focus = true;

      // Display Rating Description
      if (lblRating != null)
      {
        lblRating.Label = string.Format("({0}) {1} / {2}", GetRatingDescription(), Rating.ToString(), (int)DisplayStars);
      }
    }

    public void SetTitle(string HeadingLine)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1, 0, 0, null);
      msg.Label = HeadingLine;
      OnMessage(msg);
    }

    public void SetLine(int LineNr, string Line)
    {
      if (LineNr < 1)
      {
        return;
      }

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1 + LineNr, 0, 0, null);
      msg.Label = Line;
      if (string.IsNullOrEmpty(msg.Label))
      {
        msg.Label = "  ";
      }
      OnMessage(msg);
    }

    private void UpdateStarVisibility()
    {

      // Check skin supports 10 stars, if not fallback to 5 stars
      if (btnStar10 == null && DisplayStars == StarDisplay.TEN_STARS)
      {
        DisplayStars = StarDisplay.FIVE_STARS;
      }

      // Hide star controls 6-10
      if (DisplayStars == StarDisplay.FIVE_STARS)
      {
        if (btnStar6 != null)
          btnStar6.Visible = false;
        if (btnStar7 != null)
          btnStar7.Visible = false;
        if (btnStar8 != null)
          btnStar8.Visible = false;
        if (btnStar9 != null)
          btnStar9.Visible = false;
        if (btnStar10 != null)
          btnStar10.Visible = false;
      }
    }

    private string GetRatingDescription()
    {
      return string.Format("{0} {1}", GUILocalizeStrings.Get(173), _rating.ToString());
    }

  }
}
