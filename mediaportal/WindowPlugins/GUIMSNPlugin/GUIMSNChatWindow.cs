/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.IO;
using XihSolutions.DotMSN;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.MSN
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIMSNChatWindow: GUIWindow
	{
    enum Controls:int
    {
      Status=2,
      List=50,
			Input=51
    }

    static int _messageIndex=0;
    static string[] _messageList;

		public GUIMSNChatWindow()
		{
      GetID = (int)GUIWindow.Window.WINDOW_MSN_CHAT;
    }
   
    public override bool Init()
    {
      _messageList = new string[30];

      return Load(GUIGraphicsContext.Skin + @"\my messenger chat.xml");
    }
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {      
				GUIMessage msg= new GUIMessage (GUIMessage.MessageType.GUI_MSG_MSN_CLOSECONVERSATION, (int)GUIWindow.Window.WINDOW_MSN, GetID, 0,0,0,null );
				msg.SendToTargetWindow = true;
				GUIGraphicsContext.SendMessage(msg);
				
				// GUIMSNPlugin.CloseConversation();

				GUIWindowManager.ShowPreviousWindow();
        return;
      }

			if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
			{
				// Check focus on sms input control
				if (GetFocusControlId() != (int)Controls.Input)
				{
					// set focus to the default control then
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, (int)Controls.Input, 0, 0, null);
					OnMessage(msg);
				}
      }
      else
      {
        // translate all other actions from regular keypresses back to keypresses
        if (action.m_key != null && action.m_key.KeyChar >= 32)
        {
          action.wID = Action.ActionType.ACTION_KEY_PRESSED;
        }
      } 
      
      base.OnAction(action);
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
          base.OnMessage(message);

          GUIListControl list= (GUIListControl)GetControl((int)Controls.List);
          list.WordWrap=true;         
          
          GUIControl.ClearControl(GetID,(int)Controls.List);
          int j=_messageIndex-30;
          if (j<0) 
            j=0;
          for (int i=0; i < 30;++i)
          {
            AddToList(_messageList[j]);
            j++;
            if (j>_messageList.Length)
              j=0;
          }
          list.ScrollToEnd();
/*          if (g_Player.Playing && !g_Player.Paused)
          {
            if (g_Player.IsVideo || g_Player.IsDVD) g_Player.Pause();
          }
*/          
					          
					// route keys
					GUIWindowManager.RouteToWindow( GetID );
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
/*          if (g_Player.Playing && g_Player.Paused)
          {
            if (g_Player.IsVideo || g_Player.IsDVD) g_Player.Pause();
          }
*/
					GUIWindowManager.UnRoute();
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
        break;

				case GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE:
					if ((GUIWindowManager.ActiveWindow != GetID) && !GUIGraphicsContext.IsFullScreenVideo)
					{
						GUIWindowManager.ActivateWindow(GetID);
					}
					AddMessageToList(message.Label);
					break;
				
				case GUIMessage.MessageType.GUI_MSG_NEW_LINE_ENTERED:
          Conversation conversation=GUIMSNPlugin.CurrentConversation;
          if (conversation==null) return true;
          conversation.Switchboard.SendTextMessage(new TextMessage(message.Label));

          string text=String.Format(">{0}", message.Label);
          AddToList(text);

          // Store
          _messageList[_messageIndex] = text;
          _messageIndex++;
          if (_messageIndex >= _messageList.Length) 
            _messageIndex = 0;
        break;
      }
      return base.OnMessage(message);
    }

    void Update()
    {
      Conversation conversation=GUIMSNPlugin.CurrentConversation;
      if (conversation==null) return;

/*      if (GUIMSNPlugin.IsTyping)
      {
        string text=String.Format("{0} {1}", GUIMSNPlugin.ContactName, GUILocalizeStrings.Get(908) );
        GUIControl.SetControlLabel(GetID,(int)Controls.Status,text);
      }
      else 
        GUIControl.SetControlLabel(GetID,(int)Controls.Status,"");
*/
    }

    public override void Process()
    {
      Conversation conversation=GUIMSNPlugin.CurrentConversation;
      if (conversation==null)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN);
        return;
      }
      Update();
    }

    private void AddMessageToList(string FormattedText)
    {
      AddToList(FormattedText);

      // Store
      _messageList[_messageIndex] = FormattedText;
      _messageIndex++;
      if (_messageIndex >= _messageList.Length) 
        _messageIndex = 0;
    }
    void AddToList(string text)
    {
      //TODO: add wordwrapping
      GUIListItem item =new GUIListItem(text);
      item.IsFolder=false;
      GUIControl.AddListItemControl(GetID,(int)Controls.List,item);
      GUIListControl list= (GUIListControl)GetControl((int)Controls.List);
			if (list!=null)
			{
				list.ScrollToEnd();
				list.Disabled=true;
			}
    }
  }
}
