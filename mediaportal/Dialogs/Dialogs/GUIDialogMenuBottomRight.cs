/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;


namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogMenuBottomRight: GUIWindow
  {

    #region Base Dialog Variables
    bool m_bRunning=false;
    int m_dwParentWindowID=0;
    GUIWindow m_pParentWindow=null;
    #endregion
		[SkinControlAttribute(2)]			protected GUIButtonControl btnClose=null;
		[SkinControlAttribute(3)]			protected GUIListControl listView=null;
		[SkinControlAttribute(4)]			protected GUILabelControl lblHeading=null;
		[SkinControlAttribute(5)]			protected GUILabelControl lblHeading2=null;
		[SkinControlAttribute(6)]			protected GUILabelControl lblHeading3=null;

    int selectedItemIndex      = -1;
    int selectedId = -1;
		int timeoutInSeconds=-1;
		bool timedOut=false;
		bool    needRefresh=false;
		DateTime vmr7UpdateTimer=DateTime.Now;

		string selectedItemLabel=String.Empty;
    ArrayList listItems   = new ArrayList();
    bool    m_bPrevOverlay=false;

    public GUIDialogMenuBottomRight()
    {
      GetID=(int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT;
    }

    public override bool Init()
    { 
      return Load (GUIGraphicsContext.Skin+@"\DialogMenuBottomRight.xml");
    }
    
    public override bool SupportsDelayedLoad
    {
      get { return true;}
    }
    public override void PreInit()
    {
    }


    public override void OnAction(Action action)
		{
			needRefresh=true;
      int iSelection;
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        Close();
        return;
      }

      if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
      {
        if (action.m_key!=null)
        {
          if (action.m_key.KeyChar >'0' && action.m_key.KeyChar <='9')         
          {
            // Get offset to item
            iSelection = action.m_key.KeyChar-'1';
            if (iSelection < listItems.Count)
            {
              // Select dialog item
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, listView.GetID, 0, 0, null);
              OnMessage(msg);
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0, listView.GetID, iSelection ,0 ,null);
              OnMessage(msg);
              msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, listView.GetID, 0, 0, 0, null);
              OnMessage(msg);
            }
            else
            {
              // Exit dialog box
              Close();
            }
            return;
          }
        }
      }

      base.OnAction(action);
    }

    #region Base Dialog Members
		public void RenderDlg(float timePassed)
		{
			lock (this)
			{
				if (GUIGraphicsContext.IsFullScreenVideo)
				{
					if (VMR7Util.g_vmr7!=null)
					{
						TimeSpan ts = DateTime.Now-vmr7UpdateTimer;
						if (ts.TotalMilliseconds>=5000 || needRefresh)
						{
							needRefresh=false;
							using (Bitmap bmp = new Bitmap(GUIGraphicsContext.Width,GUIGraphicsContext.Height))
							{
								using (Graphics g = Graphics.FromImage(bmp))
								{
									GUIGraphicsContext.graphics=g;

									// render the parent window
									if (null!=m_pParentWindow) 
										m_pParentWindow.Render(timePassed);

									GUIFontManager.Present();
									// render this dialog box
									base.Render(timePassed);

									GUIGraphicsContext.graphics=null;
									VMR7Util.g_vmr7.SaveBitmap(bmp,true,true,1.0f);
									g.Dispose();
									bmp.Dispose();
								}
							}
							vmr7UpdateTimer=DateTime.Now;
						}
						return;
					}
				}
				// render the parent window
				if (null!=m_pParentWindow) 
					m_pParentWindow.Render(timePassed);

				GUIFontManager.Present();
				// render this dialog box
				base.Render(timePassed);
			}
		}

    void Close()
		{
			GUIWindowManager.IsSwitchingToNewWindow=true;
			lock (this)
			{
				GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT,GetID,0,0,0,0,null);
				OnMessage(msg);

				GUIWindowManager.UnRoute();
				m_pParentWindow=null;
				m_bRunning=false;
			}
			GUIWindowManager.IsSwitchingToNewWindow=false;
    }

    public void DoModal(int dwParentId)
    {
      if (listItems.Count == 0)
        Close();

      m_dwParentWindowID=dwParentId;
      m_pParentWindow=GUIWindowManager.GetWindow( m_dwParentWindowID);
      if (null==m_pParentWindow)
      {
        m_dwParentWindowID=0;
        return;
      }

			GUIWindowManager.IsSwitchingToNewWindow=true;
      GUIWindowManager.RouteToWindow( GetID );

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,-1,0,null);
      OnMessage(msg);

			GUIWindowManager.IsSwitchingToNewWindow=false;
      m_bRunning=true;
			DateTime startTime=DateTime.Now;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
				if (!GUIGraphicsContext.Vmr9Active)
					System.Threading.Thread.Sleep(100);
				TimeSpan ts = DateTime.Now - startTime;
				if (timeoutInSeconds> 0 && ts.TotalSeconds > timeoutInSeconds) 
				{
					Close();
					selectedItemIndex=-1;
					selectedId=-1;
					timedOut=true;
					return;
				}
      }
    }
    #endregion
	
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
      if (control==listView)
      {
				selectedItemIndex=listView.SelectedListItemIndex;
				selectedItemLabel=listView.SelectedListItem.Label;
				int pos=selectedItemLabel.IndexOf(" ");
				if (pos>0) selectedItemLabel=selectedItemLabel.Substring(pos+1);
        selectedId=listView.SelectedListItem.ItemId;
				Close();
      }
      if (control==btnClose)
      {
        Close();
      }
		}

    public override bool OnMessage(GUIMessage message)
    {
			needRefresh=true;
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					m_pParentWindow=null;
					m_bRunning=false;
          GUIGraphicsContext.Overlay=m_bPrevOverlay;
          FreeResources();
          DeInitControls();
		      return true;
        }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {        
          m_bPrevOverlay=GUIGraphicsContext.Overlay;
          base.OnMessage(message);
          GUIGraphicsContext.Overlay=false;
					listView.Clear();
          

          for (int i=0; i < listItems.Count; i++)
          {
            GUIListItem pItem=(GUIListItem)listItems[i];
            listView.Add(pItem);
          }

          if (selectedItemIndex>=0)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,listView.GetID,selectedItemIndex,0,null);
            OnMessage(msg);
          }
          selectedItemIndex=-1;
          selectedId=-1;
          string wszText=String.Format("{0} {1}", listItems.Count,GUILocalizeStrings.Get(127) );

        }
          return true;

      }

      return base.OnMessage(message);
    }

    public void Reset()
    {
			LoadSkin();
			AllocResources();
			InitControls();
			SetHeadingRow2(String.Empty);
			SetHeadingRow3(String.Empty);

			timedOut=true;
			timeoutInSeconds=-1;
      selectedItemIndex=-1;
      listItems.Clear();
    }

		public void Add(GUIListItem pItem)
		{
			int iItemIndex = listItems.Count+1;			
			if (iItemIndex < 10) 
				pItem.Label = iItemIndex.ToString()+" "+pItem.Label;
			else
				pItem.Label = pItem.Label;

			pItem.ItemId = iItemIndex;
			listItems.Add(pItem);
		}

    public void Add(string strLabel)
		{
			int iItemIndex = listItems.Count+1;
      GUIListItem pItem = new GUIListItem(iItemIndex.ToString()+" "+strLabel);
			pItem.ItemId =iItemIndex;
      listItems.Add(pItem);
    }

    public void AddLocalizedString(int iLocalizedString)
    {
      int iItemIndex = listItems.Count+1;
      GUIListItem pItem = new GUIListItem(iItemIndex.ToString()+" "+GUILocalizeStrings.Get(iLocalizedString));     
      pItem.ItemId = iLocalizedString;
      listItems.Add(pItem);      
    }

    public int SelectedLabel 
    {
      get { return selectedItemIndex;}
      set { selectedItemIndex=value;}
    }
    
    public int SelectedId 
    {
      get { return selectedId;}
      set { selectedId=value;}
    }

    public string SelectedLabelText
    {
      get { return selectedItemLabel;}
    }

    public void  SetHeading( string strLine)
    {
			LoadSkin();
			AllocResources();
			InitControls();

			lblHeading.Label=strLine;
		}
		public void  SetHeadingRow2( string strLine)
		{
			LoadSkin();
			AllocResources();
			InitControls();

			if (lblHeading2!=null)
				lblHeading2.Label=strLine;
		}
		public void  SetHeadingRow3( string strLine)
		{
			LoadSkin();
			AllocResources();
			InitControls();

			if (lblHeading3!=null)
				lblHeading3.Label=strLine;
		}

		public void SetHeadingRow2(int iString)
		{
			SetHeadingRow2 (GUILocalizeStrings.Get(iString) );
			selectedItemIndex=-1;
			listItems.Clear();
		}

		public void SetHeadingRow3(int iString)
		{
			SetHeadingRow3 (GUILocalizeStrings.Get(iString) );
			selectedItemIndex=-1;
			listItems.Clear();
		}

    public void SetHeading(int iString)
    {
			SetHeading (GUILocalizeStrings.Get(iString) );
			selectedItemIndex=-1;
			listItems.Clear();
    }

    public override void Render(float timePassed)
    {
			RenderDlg(timePassed);
    }

		public int TimeOut
		{
			get { return timeoutInSeconds;}
			set { timeoutInSeconds=value;}
		}
		public bool TimedOut
		{
			get { return timedOut;}
		}

	}
}
