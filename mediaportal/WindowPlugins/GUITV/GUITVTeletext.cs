using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Pictures;


namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVTeletext : GUIWindow
	{
		enum Controls
		{
			LBL_MESSAGE=27,
			IMG_TELETEXT_PAGE=500,
			BTN_PAGE100=502,
			BTN_PAGE200,
			BTN_PAGE300,
			BTN_HIDDEN,
			BTN_SUBPAGE,
			BTN_LANG
		};

		DVBTeletext	m_teleText;
		Bitmap	m_pageBitmap;
		string	m_strInput="";
		int		m_actualPage=0x100;
		int		m_actualSubPage=0;
		bool	m_pageDirty=false;



		public  GUITVTeletext()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TELETEXT;
		}
    
		public override bool Init()
		{
			//
			try
			{
				System.IO.Directory.CreateDirectory("temp");
			}
			catch
			{
			}
			return Load (GUIGraphicsContext.Skin+@"\myteletext.xml");
		}
		
    
		#region Serialisation
		void LoadSettings()
		{
			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
			}
		}

		void SaveSettings()
		{
			using (AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
			{
			}
		}
		#endregion

		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_PREVIOUS_MENU:
				{
					GUIWindowManager.PreviousWindow();
					return;
				}
				case Action.ActionType.ACTION_KEY_PRESSED:
					if (action.m_key!=null)
						OnKeyCode((char)action.m_key.KeyChar);
					break;

				case Action.ActionType.ACTION_SELECT_ITEM:
					break;

			}
					base.OnAction(action);
			
		}
		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					base.OnMessage(message);
					return true;
				}

				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					base.OnMessage(message);
					ShowMessage(100,0);
					if(m_teleText==null)
					{
						Log.Write("dvb-teletext: no teletext object");
						GUIWindowManager.PreviousWindow();
						return false;
					}
					m_teleText.GetPage(0x100,0);
					GUIToggleButtonControl hiddenButton=(GUIToggleButtonControl)GetControl((int)Controls.BTN_HIDDEN);
					if(hiddenButton!=null && m_teleText!=null)
					{
						m_teleText.HiddenMode=true;
						hiddenButton.Selected=true;
						GetNewPage();
					}

					return true;
				}
					//break;

				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					int iControl=message.SenderControlId;
					if(iControl==(int)Controls.BTN_PAGE100)
					{
						m_actualPage=0x100;
						m_actualSubPage=0;
						GetNewPage();
					}
					if(iControl==(int)Controls.BTN_PAGE200)
					{
						m_actualPage=0x200;
						m_actualSubPage=0;
						GetNewPage();
					}
					if(iControl==(int)Controls.BTN_PAGE300)
					{
						m_actualPage=0x300;
						m_actualSubPage=0;
						GetNewPage();
					}
					if(iControl==(int)Controls.BTN_HIDDEN)
					{
						GUIToggleButtonControl button=(GUIToggleButtonControl)GetControl(iControl);
						
						if(m_teleText!=null && button!=null)
						{
							m_teleText.HiddenMode=button.Selected;
							GetNewPage();
						}
					}

					if(iControl==(int)Controls.BTN_LANG)
					{
						GUISelectButtonControl button=(GUISelectButtonControl)GetControl(iControl);
						
						if(m_teleText!=null && button!=null)
						{
							m_teleText.PageLanguage=button.SelectedItem;
							GetNewPage();
						}

					}
					if(iControl==(int)Controls.BTN_SUBPAGE)
					{
						GUISelectButtonControl button=(GUISelectButtonControl)GetControl(iControl);
						
						if(m_teleText!=null && button!=null)
						{
							m_actualSubPage=button.SelectedItem;
							GetNewPage();
						}

					}

					
//					if (iControl==(int)Controls.SPINCONTROL_TIME_INTERVAL)
//					{
//					}

					break;

			}
			return base.OnMessage(message);;
		}

		void GetNewPage()
		{
			if(m_teleText!=null)
			{
				m_pageBitmap=m_teleText.GetPage(m_actualPage,m_actualSubPage);
				Redraw();
				Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(m_actualPage,16),Convert.ToString(m_actualSubPage,16));
			}
		}


		void OnKeyCode(char chKey)
		{

			GUIImage gImg=(GUIImage)this.GetControl(500);
			if((chKey>='0'&& chKey <='9') || (chKey=='+' || chKey=='-')) //navigation
			{
				if (chKey=='0' && m_strInput.Length==0) return;

				// page up
				if((byte)chKey==0x2B && m_actualPage<0x898) // +
				{
					m_actualPage++;
					m_actualSubPage=0;
					if(m_teleText!=null)
					{
						m_pageBitmap=m_teleText.GetPage(m_actualPage,m_actualSubPage);
						Redraw();
						Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(m_actualPage,16),Convert.ToString(m_actualSubPage,16));
						m_strInput="";
						return;
					}

				}
				// page down
				if((byte)chKey==0x2D && m_actualPage>0x100) // -
				{
					m_actualPage--;
					m_actualSubPage=0;
					if(m_teleText!=null)
					{
						m_pageBitmap=m_teleText.GetPage(m_actualPage,m_actualSubPage);
						Redraw();
						Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(m_actualPage,16),Convert.ToString(m_actualSubPage,16));
						m_strInput="";
						return;
					}

				}
				if(chKey>='0' && chKey<='9')
					m_strInput+= chKey;

				if (m_strInput.Length==3)
				{
					// change channel
					m_actualPage=Convert.ToInt16("0x"+m_strInput,16);
					m_actualSubPage=0;
					if(m_actualPage<0x100)
						m_actualPage=0x100;
					if(m_teleText!=null)
					{
						m_pageBitmap=m_teleText.GetPage(m_actualPage,m_actualSubPage);
						Redraw();
					}
					Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(m_actualPage,16),Convert.ToString(m_actualSubPage,16));
					m_strInput="";
					
				}
				//
				// get page
				//
			}
		}

		public override void SetObject(object obj)
		{
			m_teleText=(DVBTeletext)obj;
			m_teleText.PageUpdatedEvent+=new MediaPortal.TV.Recording.DVBTeletext.PageUpdated(m_teleText_PageUpdatedEvent);
		}
		//
		//
		void ShowMessage(int page,int subpage)
		{

			string msg=String.Format("Waiting for Page {0:X}/{1}...",page,subpage);
			GUIControl.SetControlLabel(GetID,(int)Controls.LBL_MESSAGE,msg);
			GUIControl.ShowControl(GetID,(int)Controls.LBL_MESSAGE);
		}
		//
		//
		private void m_teleText_PageUpdatedEvent()
		{
			// make sure the callback returns as soon as possible!!
			// here is only a flag set to true, the bitmap is getting
			// in a timer-elapsed event!

			if(GUIWindowManager.ActiveWindow==GetID)
			{
				m_pageDirty=true;
			}
		}

		public override void Process()
		{
			if(m_pageDirty==true)
			{
				Log.Write("dvb-teletext page updated. {0:X}/{1}",m_actualPage,m_actualSubPage);
				m_pageBitmap=m_teleText.GetPage(m_actualPage,m_actualSubPage);
				Redraw();
				m_pageDirty=false;
			}
		}

		void Redraw()
		{
			Log.Write("dvb-teletext redraw()");
			try
			{

				GUIImage pictureBox = (GUIImage )GetControl( (int)Controls.IMG_TELETEXT_PAGE);
				if(m_pageBitmap==null)
				{
					ShowMessage(m_actualPage,m_actualSubPage);
					pictureBox.FreeResources();
					pictureBox.SetFileName("button_small_settings_nofocus.png");
					pictureBox.AllocResources();
					return;
				}
				GUIControl.HideControl(GetID,(int)Controls.LBL_MESSAGE);

				
				pictureBox.FileName="";
				pictureBox.FreeResources();
				pictureBox.IsVisible=false;
				Utils.FileDelete(@"temp\teletext.jpg");
				GUITextureManager.ReleaseTexture(@"temp\teletext.jpg");
				using (Bitmap img = new Bitmap (pictureBox.Width,pictureBox.Height))
				{
					using (Graphics g = Graphics.FromImage(img))
					{
						g.DrawImage(m_pageBitmap,0,0,pictureBox.Width,pictureBox.Height);
					}
					img.Save(@"temp\teletext.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
				}
				pictureBox.FileName=@"temp\teletext.jpg";
				pictureBox.AllocResources();
				pictureBox.IsVisible=true;
			}
			catch (Exception ex)
			{
				Log.Write("ex:{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
			}
		}
	}// class
}// namespace
