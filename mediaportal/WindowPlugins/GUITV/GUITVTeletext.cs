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
		// form control
		System.Windows.Forms.PictureBox m_pictureBox;
		// timer
		System.Timers.Timer	m_grabPageTimer=new System.Timers.Timer(500);


		public  GUITVTeletext()
		{
			GetID=7700;
		}
    
		public override bool Init()
		{
			// adding the page box
			m_pictureBox=new PictureBox();
			m_pictureBox.Top=100;
			m_pictureBox.Left=250;
			m_pictureBox.Width=440;
			m_pictureBox.Height=455;
			m_pictureBox.Visible=false;
			m_pictureBox.Image=new Bitmap(440,460);
			GUIGraphicsContext.form.Controls.Add(m_pictureBox);
			//
			m_grabPageTimer.AutoReset=true;
			m_grabPageTimer.Elapsed+=new System.Timers.ElapsedEventHandler(m_grabPageTimer_Elapsed);
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
					if(m_pictureBox!=null)
						m_pictureBox.Visible=false;
					base.OnMessage(message);
					return true;
				}

				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					base.OnMessage(message);
					GUIImage gImg=(GUIImage)this.GetControl(500);
					if(m_teleText==null)
					{
						GUIWindowManager.PreviousWindow();
						return false;
					}
					m_teleText.GetPage(0x100,0);
					m_grabPageTimer.Start();
					if(gImg!=null)
					{	// setting the coordinates from the image-control
						m_pictureBox.Top=gImg.rect.Y;
						m_pictureBox.Left=gImg.rect.X;
						m_pictureBox.Width=gImg.rect.Width;
						m_pictureBox.Height=gImg.rect.Height;
						m_teleText.SetPageDimensions(gImg.rect.Width,gImg.rect.Height);
						m_pictureBox.Image.Dispose();
						m_pictureBox.Image=new Bitmap(gImg.rect.Width,gImg.rect.Height);
						Graphics g=Graphics.FromImage(m_pictureBox.Image);
						g.FillRectangle(new System.Drawing.SolidBrush(Color.Black),0,0,gImg.rect.Width,gImg.rect.Height);
						g.Dispose();
						if(m_pictureBox!=null)
							m_pictureBox.Visible=true;
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
				m_grabPageTimer.Start();
			}
			Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(m_actualPage,16),Convert.ToString(m_actualSubPage,16));
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
						m_grabPageTimer.Start();
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
						m_grabPageTimer.Start();
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
						m_grabPageTimer.Start();
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
		private void m_teleText_PageUpdatedEvent()
		{
			if(GUIWindowManager.ActiveWindow==7700)
			{
				m_pageDirty=true;
				
			}
		}

		private void m_grabPageTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(m_pageDirty==true)
			{
				m_pageBitmap=m_teleText.PageBitmap;
				Redraw();
				m_pageDirty=false;
			}
		}

		void Redraw()
		{
			try
			{
				Graphics g=System.Drawing.Graphics.FromImage(m_pictureBox.Image);
				g.DrawImage(m_pageBitmap,0,0);
				g.Dispose();
				m_pictureBox.Refresh();
			}
			catch
			{}
		}
	}// class
}// namespace
