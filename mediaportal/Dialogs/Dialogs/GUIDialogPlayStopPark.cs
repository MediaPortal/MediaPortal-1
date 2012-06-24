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

using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Dialog that react to STOP remote command and take it as a YES 
  /// </summary>
  public class GUIDialogPlayStopPark : GUIDialogPlayStop
  {    
    [SkinControl(12)] protected GUIButtonControl btnPark = null;
    private bool m_bParked = false;

    public GUIDialogPlayStopPark()
    {
      GetID = (int)Window.WINDOW_DIALOG_PLAY_STOP_PARK;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogPlayStopPark.xml");
    }
    
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {           
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {            
            int iControl = message.SenderControlId;            
            if (iControl == btnPark.GetID)
            {
              m_bParked = true;              
              PageDestroy();              
              return true;
            }
            m_bParked = false;
          }
          break;        
      }

      return base.OnMessage(message);
    }


    public bool IsParkConfirmed
    {
      get { return m_bParked; }
    }    
  }
}