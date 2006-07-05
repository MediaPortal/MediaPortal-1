/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using MediaPortal.TV.Teletext;

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
    [SkinControlAttribute(507)]       protected GUIButtonControl btnFullscreen = null;

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
			using (MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
			}
		}

		void SaveSettings()
		{
			using (MediaPortal.Profile.Settings   xmlwriter=new MediaPortal.Profile.Settings("MediaPortal.xml"))
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
						if ((char)action.m_key.KeyChar >= '0' || (char)action.m_key.KeyChar <= '9')
							OnKeyPressed((char)action.m_key.KeyChar);
					}
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
			
			TeletextGrabber.Grab=false;
			TeletextGrabber.TeletextCache.PageUpdatedEvent-=new MediaPortal.TV.Teletext.DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);

			if ( !GUIGraphicsContext.IsTvWindow(newWindowId) )
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
			TeletextGrabber.Grab=true;
			btnSubPage.RestoreSelection=false;
      currentPageNumber = 100;
      currentSubPageNumber = 0;

      ShowMessage(currentPageNumber, currentSubPageNumber);
			TeletextGrabber.TeletextCache.PageSelectText="";
			if(imgTeletextPage!=null && TeletextGrabber.TeletextCache!=null)
			{
				TeletextGrabber.TeletextCache.SetPageSize(imgTeletextPage.Width,imgTeletextPage.Height);
			}
      TeletextGrabber.TeletextCache.GetPage(currentPageNumber, currentSubPageNumber);
			if(btnHidden!=null && TeletextGrabber.TeletextCache!=null)
			{
				TeletextGrabber.TeletextCache.HiddenMode=true;
				btnHidden.Selected=true;
				GetNewPage();
			}
			TeletextGrabber.TeletextCache.PageUpdatedEvent+=new MediaPortal.TV.Teletext.DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);
			TeletextGrabber.TeletextCache.TransparentMode=false;

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
				if(TeletextGrabber.TeletextCache!=null && btnHidden!=null)
				{
					TeletextGrabber.TeletextCache.HiddenMode=btnHidden.Selected;
					GetNewPage();
				}
			}
			if(control==btnSubPage)
			{
				if(TeletextGrabber.TeletextCache!=null && btnSubPage!=null)
				{
					currentSubPageNumber=btnSubPage.SelectedItem;
					GetNewPage();
				}
			}
      if (control == btnFullscreen)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
      }
		}

		void GetNewPage()
		{
			if(TeletextGrabber.TeletextCache!=null)
			{
				bitmapTeletextPage=TeletextGrabber.TeletextCache.GetPage(currentPageNumber,currentSubPageNumber);
				Redraw();
				_log.Info("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
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
				if(TeletextGrabber.TeletextCache!=null)
					TeletextGrabber.TeletextCache.PageSelectText="";
				inputLine="";
				GetNewPage();
				return;
			}
			// top text
			if(chKey=='h' || chKey=='j' || chKey=='k' || chKey=='l' ||
				chKey=='H' || chKey=='J' || chKey=='K' || chKey=='L')
			{

				if(TeletextGrabber.TeletextCache==null)
					return;

        int hexPage=0;
				string topButton=new string(chKey,1);
				switch(topButton.ToLower())
				{
					case "h":
            hexPage = TeletextGrabber.TeletextCache.PageRed;
						break;
					case "j":
            hexPage = TeletextGrabber.TeletextCache.PageGreen;
						break;
					case "k":
            hexPage = TeletextGrabber.TeletextCache.PageYellow;
						break;
					case "l":
            hexPage = TeletextGrabber.TeletextCache.PageBlue;
						break;
				}
        int mag = hexPage / 0x100;
        hexPage -= mag * 0x100;

        int tens = hexPage / 0x10;
        hexPage -= tens * 0x10;

        int pageNr = mag*100+tens*10+hexPage;
        if (pageNr >= 100 && pageNr <= 899)
        {
          currentPageNumber = pageNr;
          currentSubPageNumber = 0;
          bitmapTeletextPage = TeletextGrabber.TeletextCache.GetPage(currentPageNumber, currentSubPageNumber);
          Redraw();
          _log.Info("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
          inputLine = "";
          return;
        }
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
					if(TeletextGrabber.TeletextCache!=null)
					{
						bitmapTeletextPage=TeletextGrabber.TeletextCache.GetPage(currentPageNumber,currentSubPageNumber);
						Redraw();
						_log.Info("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
						inputLine="";
						return;
					}

				}
				// page down
				if((byte)chKey==0x2D && currentPageNumber>100) // -
				{
					currentPageNumber--;
					currentSubPageNumber=0;
					if(TeletextGrabber.TeletextCache!=null)
					{
						bitmapTeletextPage=TeletextGrabber.TeletextCache.GetPage(currentPageNumber,currentSubPageNumber);
						Redraw();
						_log.Info("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
						inputLine="";
						return;
					}

				}
				if(chKey>='0' && chKey<='9')
				{
					inputLine+= chKey;
					if(TeletextGrabber.TeletextCache!=null)
					{
						TeletextGrabber.TeletextCache.PageSelectText=inputLine;
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

					if(TeletextGrabber.TeletextCache!=null)
					{
						TeletextGrabber.TeletextCache.PageSelectText="";
						bitmapTeletextPage=TeletextGrabber.TeletextCache.GetPage(currentPageNumber,currentSubPageNumber);
						Redraw();
					}
					_log.Info("dvb-teletext: select page {0} / subpage {1}",Convert.ToString(currentPageNumber),Convert.ToString(currentSubPageNumber));
					inputLine="";
					
				}
				//
				// get page
				//
			}
		}


		public bool HasTeletext()
		{
			return (TeletextGrabber.TeletextCache!=null);
		}
		//
		//
		void ShowMessage(int page,int subpage)
		{
			if (lblMessage==null) return;
          lblMessage.Label = String.Format(GUILocalizeStrings.Get(596), page, subpage); // Waiting for Page {0}/{1}...
			lblMessage.IsVisible=true;
		}
		//
		//
		private void dvbTeletextParser_PageUpdatedEvent()
		{
			// make sure the callback returns as soon as possible!!
			// here is only a flag set to true, the bitmap is getting
			// in a timer-elapsed event!

			if(TeletextGrabber.TeletextCache==null)
				return;
			if(TeletextGrabber.TeletextCache.PageSelectText.IndexOf("-")!=-1)// page select is running
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
        if (currentPageNumber < 100) currentPageNumber = 100;
        if (currentPageNumber > 899) currentPageNumber = 899;
        TeletextGrabber.TeletextCache.PageSelectText = Convert.ToString(currentPageNumber);
				int NumberOfSubpages=TeletextGrabber.TeletextCache.NumberOfSubpages(currentPageNumber);
				if (NumberOfSubpages>currentSubPageNumber)
				{
					currentSubPageNumber++;
				}
				else if (currentSubPageNumber>=NumberOfSubpages)
					currentSubPageNumber=1;

				_log.Info("dvb-teletext page updated. {0}/{1} {2}",currentPageNumber,currentSubPageNumber,NumberOfSubpages);
				bitmapTeletextPage=TeletextGrabber.TeletextCache.GetPage(currentPageNumber,currentSubPageNumber);
				Redraw();
				isPageDirty=false;
			}
		}

		void Redraw()
		{
			_log.Info("dvb-teletext redraw()");
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
          System.Drawing.Image img = (Image)bitmapTeletextPage.Clone();
          imgTeletextPage.IsVisible = false;
					imgTeletextPage.FileName="";
					//Utils.FileDelete(@"teletext.jpg");
					GUITextureManager.ReleaseTexture("[teletextpage]");
          //bitmapTeletextPage.Save(@"teletext.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
          imgTeletextPage.MemoryImage = img;
          imgTeletextPage.FileName = "[teletextpage]";
					imgTeletextPage.IsVisible=true;
				}
			}
			catch (Exception ex)
			{
        _log.Error(ex);
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
