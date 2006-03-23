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
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.Dialogs
{
	/// <summary>
	/// 
	/// </summary>
  public class GUIDialogSetRating : GUIWindow, IRenderLayer
	{
		public enum ResultCode
		{
			Close,
			Next,
			Previous
		};

		[SkinControlAttribute(2)]				protected GUILabelControl lblHeading=null;
		[SkinControlAttribute(4)]				protected GUILabelControl lblName=null;
		[SkinControlAttribute(10)]			protected GUIButtonControl btnPlus=null;
		[SkinControlAttribute(11)]			protected GUIButtonControl btnMin=null;
		[SkinControlAttribute(12)]			protected GUIButtonControl btnOk=null;
		[SkinControlAttribute(13)]			protected GUIButtonControl btnNextItem=null;
		[SkinControlAttribute(14)]			protected GUIButtonControl btnPlay=null;
		[SkinControlAttribute(15)]			protected GUIButtonControl btnPreviousItem=null;
		[SkinControlAttribute(100)]			protected GUIImage imgStar1=null;
		[SkinControlAttribute(101)]			protected GUIImage imgStar2=null;
		[SkinControlAttribute(102)]			protected GUIImage imgStar3=null;
		[SkinControlAttribute(103)]			protected GUIImage imgStar4=null;
		[SkinControlAttribute(104)]			protected GUIImage imgStar5=null;

		#region Base Dialog Variables
		bool m_bRunning=false;
		int m_dwParentWindowID=0;
		GUIWindow m_pParentWindow=null;
		#endregion
    
		bool m_bPrevOverlay=true;
		int  rating=1;
		string fileName;
		ResultCode resultCode;

		public GUIDialogSetRating()
		{
			GetID=(int)GUIWindow.Window.WINDOW_DIALOG_RATING;
		}

		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\dialogRating.xml");
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
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				Close();
				return;
			}
			base.OnAction(action);
		}

		#region Base Dialog Members
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
			GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT,GetID,0,0,0,0,null);
			OnMessage(msg);

			GUIWindowManager.IsSwitchingToNewWindow=false;
			m_bRunning=true;
			while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
			{
				GUIWindowManager.Process();

			}
		}
		#endregion
	
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnOk)
			{
				Close();
				resultCode=ResultCode.Close;
				return ;
			}
			if (control==btnNextItem)
			{
				Close();
				resultCode=ResultCode.Next;
				return ;
			}
			if (control==btnPreviousItem)
			{
				Close();
				resultCode=ResultCode.Previous;
				return ;
			}
			if (control==btnPlay)
			{
				Log.Write("DialogSetRating:Play:{0}",FileName);
				g_Player.Play(FileName);
			}

			if (control==btnMin)
			{
				if (rating >=1) rating--;
				UpdateRating();
				return ;
			}
			if (control==btnPlus)
			{
				if (rating<5) rating++;
				UpdateRating();
				return ;
			}
		}

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					m_pParentWindow=null;
					m_bRunning=false;
					GUIGraphicsContext.Overlay=m_bPrevOverlay;				
					FreeResources();
          DeInitControls();
          GUILayerManager.UnRegisterLayer(this);
					return true;
				}

				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					resultCode=ResultCode.Close;
					m_bPrevOverlay=GUIGraphicsContext.Overlay;
					base.OnMessage(message);
          GUIGraphicsContext.Overlay = base.IsOverlayAllowed;
          UpdateRating();
          GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
				}
					return true;
			}

			return base.OnMessage(message);
		}
		public void  SetHeading( string strLine)
		{
			LoadSkin();
			AllocResources();
			InitControls();

			lblHeading.Label=strLine; 
		}

		public void SetHeading(int iString)
		{
			if (iString==0) SetHeading (String.Empty);
			else SetHeading (GUILocalizeStrings.Get(iString) );
		}

		public void SetTitle(string title)
		{
			LoadSkin();
			AllocResources();
			InitControls();
			lblName.Label=title; 
		}

		void UpdateRating()
		{
			GUIImage[] imgStars = new GUIImage[5]{imgStar1, imgStar2, imgStar3, imgStar4, imgStar5};
			for (int i=0; i < 5; ++i)
			{
				if ( (i+1) > (int)(Rating) )
					imgStars[i].IsVisible=false;
				else
					imgStars[i].IsVisible=true;
			}
		}

		public int Rating
		{
			get { return rating;}
			set {rating=value;}
		}
		public string FileName
		{
			get { return fileName;}
			set {fileName=value;}
		}

		public ResultCode Result
		{
			get { return resultCode;}
		}

    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }
    #endregion

	}
}
