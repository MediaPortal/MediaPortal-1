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
					{
						if ((char)action.m_key.KeyChar < '0' || (char)action.m_key.KeyChar > '9')
							OnKeyPressed((char)action.m_key.KeyChar);
					}
				break;

				case Action.ActionType.REMOTE_0:
				OnKeyPressed((char)'0');
				break;
				case Action.ActionType.REMOTE_1:
				OnKeyPressed((char)'1');
				break;
				case Action.ActionType.REMOTE_2:
				OnKeyPressed((char)'2');
				break;
				case Action.ActionType.REMOTE_3:
				OnKeyPressed((char)'3');
				break;
				case Action.ActionType.REMOTE_4:
				OnKeyPressed((char)'4');
				break;
				case Action.ActionType.REMOTE_5:
				OnKeyPressed((char)'5');
				break;
				case Action.ActionType.REMOTE_6:
				OnKeyPressed((char)'6');
				break;
				case Action.ActionType.REMOTE_7:
				OnKeyPressed((char)'7');
				break;
				case Action.ActionType.REMOTE_8:
				OnKeyPressed((char)'8');
				break;
				case Action.ActionType.REMOTE_9:
				OnKeyPressed((char)'9');
				break;

				case Action.ActionType.ACTION_SELECT_ITEM:
					break;
				case Action.ActionType.ACTION_REMOTE_RED_BUTTON:
					OnKeyPressed((char)'h');
					break;
				case Action.ActionType.ACTION_REMOTE_GREEN_BUTTON:
					OnKeyPressed((char)'j');
					break;
				case Action.ActionType.ACTION_REMOTE_YELLOW_BUTTON:
					OnKeyPressed((char)'k');
					break;
				case Action.ActionType.ACTION_REMOTE_BLUE_BUTTON:
					OnKeyPressed((char)'l');
					break;
				case Action.ActionType.ACTION_REMOTE_SUBPAGE_UP:
					SubpageUp();
					break;
				case Action.ActionType.ACTION_REMOTE_SUBPAGE_DOWN:
					SubpageDown();
					break;
			}
			base.OnAction(action);
		}

		protected override void OnPageDestroy(int newWindowId)
		{
			if ( !GUITVHome.IsTVWindow(newWindowId) )
			{
				if (Recorder.IsViewing() && ! (Recorder.IsTimeShifting()||Recorder.IsRecording()) )
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
				dvbTeletextParser.PageSelectText="";
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

		void SubpageUp()
		{
			if(actualSubPageNumber<128)
			{
				actualSubPageNumber++;
				GetNewPage();
			}
		}
		void SubpageDown()
		{
			if(actualSubPageNumber>0)
			{
				actualSubPageNumber--;
				GetNewPage();
			}
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
			if(chKey=='c' || chKey=='C')
			{
				if(dvbTeletextParser!=null)
					dvbTeletextParser.PageSelectText="";
				inputLine="";
				GetNewPage();
				return;
			}
			// top text
			if(chKey=='h' || chKey=='j' || chKey=='k' || chKey=='l' ||
				chKey=='H' || chKey=='J' || chKey=='K' || chKey=='L')
			{

				if(dvbTeletextParser==null)
					return;
				
				string topButton=new string(chKey,1);
				switch(topButton.ToLower())
				{
					case "h":
						acutalPageNumber=dvbTeletextParser.PageRed;
						break;
					case "j":
						acutalPageNumber=dvbTeletextParser.PageGreen;
						break;
					case "k":
						acutalPageNumber=dvbTeletextParser.PageYellow;
						break;
					case "l":
						acutalPageNumber=dvbTeletextParser.PageBlue;
						break;
				}
				actualSubPageNumber=0;
				bmpTeletextPage=dvbTeletextParser.GetPage(acutalPageNumber,actualSubPageNumber);
				Redraw();
				Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(acutalPageNumber),Convert.ToString(actualSubPageNumber));
				inputLine="";
				return;
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
				{
					inputLine+= chKey;
					if(dvbTeletextParser!=null)
					{
						dvbTeletextParser.PageSelectText=inputLine;
						GetNewPage();
					}
				}

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
						dvbTeletextParser.PageSelectText="";
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
			if(dvbTeletextParser==null)
				return;
			if(dvbTeletextParser.PageSelectText.IndexOf("-")!=-1)// page select is running
				return;

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
				if (lblMessage!=null)
					lblMessage.IsVisible=false;

				
				lock (imgTeletext)
				{
					System.Drawing.Image img=(Image)bmpTeletextPage.Clone();
					imgTeletext.FileName="";
					imgTeletext.FreeResources();
					imgTeletext.IsVisible=false;
					//Utils.FileDelete(@"teletext.jpg");
					GUITextureManager.ReleaseTexture("#useMemoryImage");
					//bitmapTeletextPage.Save(@"teletext.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
					imgTeletext.FileName="#useMemoryImage";
					imgTeletext.MemoryImage=img;
					imgTeletext.AllocResources();
					imgTeletext.IsVisible=true;
					imgTeletext.Centered=false;
					imgTeletext.KeepAspectRatio=false;
					int left=GUIGraphicsContext.Width/20; // 5%
					int top=GUIGraphicsContext.Height/20; // 5%
					imgTeletext.SetPosition(left,top);
					imgTeletext.Width=GUIGraphicsContext.Width-(2*left);
					imgTeletext.Height=GUIGraphicsContext.Height-(2*top);
				}
			}
			catch (Exception ex)
			{
				Log.Write("ex:{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
			}
		}

		public override void Render(float timePassed)
		{
			lock (imgTeletext)
			{
				base.Render (timePassed);
			}
		}


	}// class
}// namespace