#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogSetRating : GUIDialogWindow
  {
    public enum ResultCode
    {
      Close,
      Next,
      Previous
    } ;

    [SkinControl(2)] protected GUILabelControl lblHeading = null;
    [SkinControl(4)] protected GUILabelControl lblName = null;
    [SkinControl(10)] protected GUIButtonControl btnPlus = null;
    [SkinControl(11)] protected GUIButtonControl btnMin = null;
    [SkinControl(12)] protected GUIButtonControl btnOk = null;
    [SkinControl(13)] protected GUIButtonControl btnNextItem = null;
    [SkinControl(14)] protected GUIButtonControl btnPlay = null;
    [SkinControl(15)] protected GUIButtonControl btnPreviousItem = null;
    [SkinControl(100)] protected GUIImage imgStar1 = null;
    [SkinControl(101)] protected GUIImage imgStar2 = null;
    [SkinControl(102)] protected GUIImage imgStar3 = null;
    [SkinControl(103)] protected GUIImage imgStar4 = null;
    [SkinControl(104)] protected GUIImage imgStar5 = null;

    private int rating = 1;
    private string fileName;
    private ResultCode resultCode;

    public GUIDialogSetRating()
    {
      GetID = (int) Window.WINDOW_DIALOG_RATING;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogRating.xml");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnOk)
      {
        PageDestroy();
        resultCode = ResultCode.Close;
        return;
      }
      if (control == btnNextItem)
      {
        PageDestroy();
        resultCode = ResultCode.Next;
        return;
      }
      if (control == btnPreviousItem)
      {
        PageDestroy();
        resultCode = ResultCode.Previous;
        return;
      }
      if (control == btnPlay)
      {
        Log.Info("DialogSetRating:Play:{0}", FileName);
        g_Player.Play(FileName);
      }

      if (control == btnMin)
      {
        if (rating >= 1)
        {
          rating--;
        }
        UpdateRating();
        return;
      }
      if (control == btnPlus)
      {
        if (rating < 5)
        {
          rating++;
        }
        UpdateRating();
        return;
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            resultCode = ResultCode.Close;
            base.OnMessage(message);
            UpdateRating();
          }
          return true;
      }

      return base.OnMessage(message);
    }

    public void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      lblHeading.Label = strLine;
    }

    public void SetHeading(int iString)
    {
      if (iString == 0)
      {
        SetHeading(string.Empty);
      }
      else
      {
        SetHeading(GUILocalizeStrings.Get(iString));
      }
    }

    public void SetTitle(string title)
    {
      LoadSkin();
      AllocResources();
      InitControls();
      lblName.Label = title;
    }

    private void UpdateRating()
    {
      GUIImage[] imgStars = new GUIImage[5] {imgStar1, imgStar2, imgStar3, imgStar4, imgStar5};
      for (int i = 0; i < 5; ++i)
      {
        if ((i + 1) > (int) (Rating))
        {
          imgStars[i].IsVisible = false;
        }
        else
        {
          imgStars[i].IsVisible = true;
        }
      }
    }

    public int Rating
    {
      get { return rating; }
      set { rating = value; }
    }

    public string FileName
    {
      get { return fileName; }
      set { fileName = value; }
    }

    public ResultCode Result
    {
      get { return resultCode; }
    }
  }
}