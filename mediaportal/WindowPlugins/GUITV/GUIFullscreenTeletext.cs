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
	public class GUITVFullscreenTeletext : GUIWindow
	{
		[SkinControlAttribute(27)]				protected GUILabelControl lblMessage=null;
		[SkinControlAttribute(500)]				protected GUIImage imgTeletext=null;

		DVBTeletext	dvbTeletextParser;
		Bitmap			bmpTeletextPage;
		string			inputLine=String.Empty;
		int					acutalPageNumber=100;
		int					actualSubPageNumber=0;
		bool				isPageDirty=false;

		public  GUITVFullscreenTeletext()
		{
			GetID=(int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\myfsteletext.xml");
		}
		
    
		#region Serialisation
		void LoadSettings()
		{
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
			}
		}

		void SaveSettings()
		{
			using (MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
			}
		}
		#endregion

		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_KEY_PRESSED:
					if (action.m_key!=null)
						OnKeyPressed((char)action.m_key.KeyChar);
					break;

				case Action.ActionType.ACTION_SELECT_ITEM:
					break;

			}
			base.OnAction(action);
			
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			if ( !GUITVHome.IsTVWindow(newWindowId) )
			{
				if (! g_Player.Playing)
				{
					if (GUIGraphicsContext.ShowBackground)
					{
						// stop timeshifting & viewing... 
	              
						Recorder.StopViewing();
					}
				}
			}
			base.OnPageDestroy (newWindowId);
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			ShowMessage(100,0);
					
			if(dvbTeletextParser==null)
			{
				Log.Write("dvb-teletext: no teletext object");
				GUIWindowManager.ShowPreviousWindow();
				return ;
			}
			if(imgTeletext!=null && dvbTeletextParser!=null)
			{
				imgTeletext.Width=GUIGraphicsContext.OverScanWidth;
				imgTeletext.Height=GUIGraphicsContext.OverScanHeight;
				imgTeletext.XPosition=GUIGraphicsContext.OverScanLeft;
				imgTeletext.YPosition=GUIGraphicsContext.OverScanTop;
				dvbTeletextParser.SetPageSize(imgTeletext.Width,imgTeletext.Height);
			}
			acutalPageNumber=100;
			actualSubPageNumber=0;
			GetNewPage();
		}


		void GetNewPage()
		{
			if(dvbTeletextParser!=null)
			{
				bmpTeletextPage=dvbTeletextParser.GetPage(acutalPageNumber,actualSubPageNumber);
				Redraw();
				Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(acutalPageNumber),Convert.ToString(actualSubPageNumber));
			}
		}


		void OnKeyPressed(char chKey)
		{
			if(chKey=='w' || chKey=='W')
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
			}
			if((chKey>='0'&& chKey <='9') || (chKey=='+' || chKey=='-')) //navigation
			{
				if (chKey=='0' && inputLine.Length==0) return;

				// page up
				if((byte)chKey==0x2B && acutalPageNumber<899) // +
				{
					acutalPageNumber++;
					actualSubPageNumber=0;
					if(dvbTeletextParser!=null)
					{
						bmpTeletextPage=dvbTeletextParser.GetPage(acutalPageNumber,actualSubPageNumber);
						Redraw();
						Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(acutalPageNumber),Convert.ToString(actualSubPageNumber));
						inputLine=String.Empty;
						return;
					}

				}
				// page down
				if((byte)chKey==0x2D && acutalPageNumber>100) // -
				{
					acutalPageNumber--;
					actualSubPageNumber=0;
					if(dvbTeletextParser!=null)
					{
						bmpTeletextPage=dvbTeletextParser.GetPage(acutalPageNumber,actualSubPageNumber);
						Redraw();
						Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(acutalPageNumber),Convert.ToString(actualSubPageNumber));
						inputLine=String.Empty;
						return;
					}

				}
				if(chKey>='0' && chKey<='9')
					inputLine+= chKey;

				if (inputLine.Length==3)
				{
					// change channel
					acutalPageNumber=Convert.ToInt16(inputLine);
					actualSubPageNumber=0;
					if(acutalPageNumber<100)
						acutalPageNumber=100;
					if(acutalPageNumber>899)
						acutalPageNumber=899;
					if(dvbTeletextParser!=null)
					{
						bmpTeletextPage=dvbTeletextParser.GetPage(acutalPageNumber,actualSubPageNumber);
						Redraw();
					}
					Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(acutalPageNumber),Convert.ToString(actualSubPageNumber));
					inputLine=String.Empty;
					
				}
				//
				// get page
				//
			}
		}

		public override void SetObject(object obj)
		{
			if(obj.GetType()==typeof(DVBTeletext))
			{
				dvbTeletextParser=(DVBTeletext)obj;
				if(dvbTeletextParser==null)
					return;
				dvbTeletextParser.PageUpdatedEvent+=new MediaPortal.TV.Recording.DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);
				dvbTeletextParser.TransparentMode=true;
			}
		}
		//
		//
		void ShowMessage(int page,int subpage)
		{
			if (lblMessage==null) return;
			lblMessage.Label=String.Format("Waiting for Page {0}/{1}...",page,subpage);
			lblMessage.IsVisible=true;

		}
		//
		//
		private void dvbTeletextParser_PageUpdatedEvent()
		{
			// make sure the callback returns as soon as possible!!
			// here is only a flag set to true, the bitmap is getting
			// in a timer-elapsed event!

			if(GUIWindowManager.ActiveWindow==GetID)
			{
				isPageDirty=true;
			}
		}

		public override void Process()
		{
			if(isPageDirty==true)
			{
				Log.Write("dvb-teletext page updated. {0:X}/{1}",acutalPageNumber,actualSubPageNumber);
				bmpTeletextPage=dvbTeletextParser.GetPage(acutalPageNumber,actualSubPageNumber);
				Redraw();
				isPageDirty=false;
			}
		}

		void Redraw()
		{
			Log.Write("dvb-teletext redraw()");
			try
			{

				if(bmpTeletextPage==null)
				{
					ShowMessage(acutalPageNumber,actualSubPageNumber);
					imgTeletext.FreeResources();
					imgTeletext.SetFileName("button_small_settings_nofocus.png");
					imgTeletext.AllocResources();
					return;
				}
				lblMessage.IsVisible=false;

				
				imgTeletext.FileName=String.Empty;
				imgTeletext.FreeResources();
				imgTeletext.IsVisible=false;
				Utils.FileDelete(@"teletext.jpg");
				GUITextureManager.ReleaseTexture(@"teletext.jpg");
				bmpTeletextPage.Save(@"teletext.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
				imgTeletext.FileName=@"teletext.jpg";
				imgTeletext.AllocResources();
				imgTeletext.IsVisible=true;
			}
			catch (Exception ex)
			{
				Log.Write("ex:{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
			}
		}

	}// class
}// namespace