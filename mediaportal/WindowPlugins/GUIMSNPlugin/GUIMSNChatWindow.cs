using System;
using System.IO;
using DotMSN;
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
      List=50
    }
		public GUIMSNChatWindow()
		{
      GetID = (int)GUIWindow.Window.WINDOW_MSN_CHAT;
    }
    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\my messenger chat.xml");
    }
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Conversation conversation=GUIMSNPlugin.CurrentConversation;
        if (conversation!=null) 
        {
          if (conversation.Connected) 
          {
            GUIMSNPlugin.CloseConversation();
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN);
            return;
          }
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
          GUIMSNPlugin.CurrentConversation.MessageReceived +=new DotMSN.Conversation.MessageReceivedHandler(MessageReceived);
          GUIControl.ClearControl(GetID,(int)Controls.List);
          for (int i=0; i < 30;++i)
          {
            AddToList("");
          }
          GUIListControl list= (GUIListControl)GetControl((int)Controls.List);
          list.ScrollToEnd();
          if (g_Player.Playing && !g_Player.Paused)
          {
            if (g_Player.IsVideo || g_Player.IsDVD) g_Player.Pause();
          }
          int activeWindow=(int)GUIWindowManager.ActiveWindow;
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
          if (GUIMSNPlugin.CurrentConversation!=null)
            GUIMSNPlugin.CurrentConversation.MessageReceived -=new DotMSN.Conversation.MessageReceivedHandler(MessageReceived);
          if (g_Player.Playing && g_Player.Paused)
          {
            if (g_Player.IsVideo || g_Player.IsDVD) g_Player.Pause();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
        break;

        case GUIMessage.MessageType.GUI_MSG_NEW_LINE_ENTERED:
          Conversation conversation=GUIMSNPlugin.CurrentConversation;
          if (conversation==null) return true;
          if (conversation.Connected==false) return true;
          conversation.SendMessage(message.Label);

          string text=String.Format(">{0}", message.Label);
          AddToList(text);
        break;
      }
      return base.OnMessage(message);
    }

    void Update()
    {
      Conversation conversation=GUIMSNPlugin.CurrentConversation;
      if (conversation!=null) return;
      if (conversation.Connected==false) return;

      if (GUIMSNPlugin.IsTyping)
      {
        string text=String.Format("{0} {1}", GUIMSNPlugin.ContactName, GUILocalizeStrings.Get(906) );
        GUIControl.SetControlLabel(GetID,(int)Controls.Status,text);
      }
      else 
        GUIControl.SetControlLabel(GetID,(int)Controls.Status,"");
    }

    public override void Process()
    {
      Conversation conversation=GUIMSNPlugin.CurrentConversation;
      if (conversation!=null)
      {
        if (conversation.Connected==false)
        {
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN);
          return;
        }
      }
      Update();
    }

    private void MessageReceived(Conversation sender, MessageEventArgs e)
    {
      string text=String.Format("{0}:{1}", e.Sender.Name, e.Message.Text);
      AddToList(text);
    }
    void AddToList(string text)
    {
      GUIListItem item =new GUIListItem(text);
      item.IsFolder=false;
      GUIControl.AddListItemControl(GetID,(int)Controls.List,item);
      GUIListControl list= (GUIListControl)GetControl((int)Controls.List);
      list.ScrollToEnd();
    }
  }
}
