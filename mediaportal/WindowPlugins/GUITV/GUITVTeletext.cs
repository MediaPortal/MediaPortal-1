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
	public class GUITVTeletext : GUIWindow
	{
		[SkinControlAttribute(27)]				protected GUILabelControl lblMessage=null;
		[SkinControlAttribute(500)]				protected GUIImage imgTeletextPage=null;
		[SkinControlAttribute(502)]				protected GUIButtonControl btnPage100=null;
		[SkinControlAttribute(503)]				protected GUIButtonControl btnPage200=null;
		[SkinControlAttribute(504)]				protected GUIButtonControl btnPage300=null;
		[SkinControlAttribute(505)]				protected GUIToggleButtonControl btnHidden=null;
		[SkinControlAttribute(506)]				protected GUISelectButtonControl btnSubPage=null;

		DVBTeletext	dvbTeletextParser;
		Bitmap	bitmapTeletextPage;
		string	inputLine="";
		int		currentPageNumber=100;
		int		currentSubPageNumber=0;
		bool	isPageDirty=false;



		public  GUITVTeletext()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TELETEXT;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\myteletext.xml");
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
			btnSubPage.RestoreSelection=false;

			ShowMessage(100,0);
			if(dvbTeletextParser==null)
			{
				Log.Write("dvb-teletext: no teletext object");
				GUIWindowManager.ShowPreviousWindow();
				return ;
			}
			dvbTeletextParser.PageSelectText="";
			if(imgTeletextPage!=null && dvbTeletextParser!=null)
			{
				dvbTeletextParser.SetPageSize(imgTeletextPage.Width,imgTeletextPage.Height);
			}
			dvbTeletextParser.GetPage(100,0);
			if(btnHidden!=null && dvbTeletextParser!=null)
			{
				dvbTeletextParser.HiddenMode=true;
				btnHidden.Selected=true;
				GetNewPage();
			}
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if(control==btnPage100)
			{
				currentPageNumber=100;
				currentSubPageNumber=0;
				GetNewPage();
			}
			if(control==btnPage200)
			{
				currentPageNumber=200;
				currentSubPageNumber=0;
				GetNewPage();
			}
			if(control==btnPage300)
			{
				currentPageNumber=300;
				currentSubPageNumber=0;
				GetNewPage();
			}
			if(control==btnHidden)
			{
				if(dvbTeletextParser!=null && btnHidden!=null)
				{
					dvbTeletextParser.HiddenMode=btnHidden.Selected;
					GetNewPage();
				}
			}
			if(control==btnSubPage)
			{
				if(dvbTeletextParser!=null && btnSubPage!=null)
				{
					currentSubPageNumber=btnSubPage.SelectedItem;
					GetNewPage();
				}
			}
		}

		void GetNewPage()
		{
			if(dvbTeletextParser!=null)
			{
				bitmapTeletextPage=dvbTeletextParser.GetPage(currentPageNumber,currentSubPageNumber);
				Redraw();
				Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
			}
		}


		void OnKeyPressed(char chKey)
		{

			if(chKey=='f' || chKey=='F')
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
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
						currentPageNumber=dvbTeletextParser.PageRed;
						break;
					case "j":
						currentPageNumber=dvbTeletextParser.PageGreen;
						break;
					case "k":
						currentPageNumber=dvbTeletextParser.PageYellow;
						break;
					case "l":
						currentPageNumber=dvbTeletextParser.PageBlue;
						break;
				}
				currentSubPageNumber=0;
				bitmapTeletextPage=dvbTeletextParser.GetPage(currentPageNumber,currentSubPageNumber);
				Redraw();
				Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
				inputLine="";
				return;
			}

			//
			if((chKey>='0'&& chKey <='9') || (chKey=='+' || chKey=='-')) //navigation
			{
				if (chKey=='0' && inputLine.Length==0) return;

				// page up
				if((byte)chKey==0x2B && currentPageNumber<899) // +
				{
					currentPageNumber++;
					currentSubPageNumber=0;
					if(dvbTeletextParser!=null)
					{
						bitmapTeletextPage=dvbTeletextParser.GetPage(currentPageNumber,currentSubPageNumber);
						Redraw();
						Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
						inputLine="";
						return;
					}

				}
				// page down
				if((byte)chKey==0x2D && currentPageNumber>100) // -
				{
					currentPageNumber--;
					currentSubPageNumber=0;
					if(dvbTeletextParser!=null)
					{
						bitmapTeletextPage=dvbTeletextParser.GetPage(currentPageNumber,currentSubPageNumber);
						Redraw();
						Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
						inputLine="";
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
					currentPageNumber=Convert.ToInt16(inputLine);
					currentSubPageNumber=0;
					if(currentPageNumber<100)
						currentPageNumber=100;
					if(currentPageNumber>899)
						currentPageNumber=899;

					if(dvbTeletextParser!=null)
					{
						dvbTeletextParser.PageSelectText="";
						bitmapTeletextPage=dvbTeletextParser.GetPage(currentPageNumber,currentSubPageNumber);
						Redraw();
					}
					Log.Write("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
					inputLine="";
					
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
			}
		}

		public bool HasTeletext()
		{
			return (dvbTeletextParser!=null);
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
				Log.Write("dvb-teletext page updated. {0:X}/{1}",currentPageNumber,currentSubPageNumber);
				dvbTeletextParser.PageSelectText=Convert.ToString(currentPageNumber);
				bitmapTeletextPage=dvbTeletextParser.GetPage(currentPageNumber,currentSubPageNumber);
				Redraw();
				isPageDirty=false;
			}
		}

		void Redraw()
		{
			Log.Write("dvb-teletext redraw()");
			try
			{

				if(bitmapTeletextPage==null)
				{
					ShowMessage(currentPageNumber,currentSubPageNumber);
					imgTeletextPage.FreeResources();
					imgTeletextPage.SetFileName("button_small_settings_nofocus.png");
					imgTeletextPage.AllocResources();
					return;
				}
				if (lblMessage!=null)
					lblMessage.IsVisible=false;
				lock (imgTeletextPage)
				{
					System.Drawing.Image img=(Image)bitmapTeletextPage.Clone();
					imgTeletextPage.FileName="";
					imgTeletextPage.FreeResources();
					imgTeletextPage.IsVisible=false;
					//Utils.FileDelete(@"teletext.jpg");
					GUITextureManager.ReleaseTexture("#useMemoryImage");
					//bitmapTeletextPage.Save(@"teletext.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
					imgTeletextPage.FileName="#useMemoryImage";
					imgTeletextPage.MemoryImage=img;
					imgTeletextPage.AllocResources();
					imgTeletextPage.IsVisible=true;
				}
			}
			catch (Exception ex)
			{
				Log.Write("ex:{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
			}
		}
		public override void Render(float timePassed)
		{
			lock (imgTeletextPage)
			{
				base.Render (timePassed);
			}
		}

	}// class
}// namespace
