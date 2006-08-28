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
using MediaPortal.GUI.Pictures;

using TvDatabase;
using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TVTeletext : GUIWindow
  {
    [SkinControlAttribute(27)]
    protected GUILabelControl lblMessage = null;
    [SkinControlAttribute(500)]
    protected GUIImage imgTeletextPage = null;
    [SkinControlAttribute(502)]
    protected GUIButtonControl btnPage100 = null;
    [SkinControlAttribute(503)]
    protected GUIButtonControl btnPage200 = null;
    [SkinControlAttribute(504)]
    protected GUIButtonControl btnPage300 = null;
    [SkinControlAttribute(505)]
    protected GUIToggleButtonControl btnHidden = null;
    [SkinControlAttribute(506)]
    protected GUISelectButtonControl btnSubPage = null;
    [SkinControlAttribute(507)]
    protected GUIButtonControl btnFullscreen = null;

    Bitmap bitmapTeletextPage;
    string inputLine = "";
    int currentPageNumber = 0x100;
    int currentSubPageNumber = 0;

    bool _waiting = false;
    DateTime _startTime = DateTime.MinValue;
    TvLibrary.Teletext.TeletextPageRenderer _renderer = new TvLibrary.Teletext.TeletextPageRenderer();


    public TVTeletext()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TELETEXT;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myteletext.xml");
    }
    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TELETEXT, this);
    }
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
      }
    }
    #endregion

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_KEY_PRESSED:
          if (action.m_key != null)
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

      TVHome.Card.GrabTeletext= false;
      //@TeletextGrabber.TeletextCache.PageUpdatedEvent-=new MediaPortal.TV.Teletext.DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);

      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (TVHome.Card.IsTimeShifting && !(TVHome.Card.IsTimeShifting || TVHome.Card.IsRecording))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            //@Recorder.StopViewing();
          }
        }
      }
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      TVHome.Card.GrabTeletext=true;
      btnSubPage.RestoreSelection = false;
      currentPageNumber = 0x100;
      currentSubPageNumber = 0;
      _startTime = DateTime.MinValue;
      ShowMessage(currentPageNumber, currentSubPageNumber);
      _renderer = new TvLibrary.Teletext.TeletextPageRenderer();
      _renderer.Height = imgTeletextPage.Height;
      _renderer.Width = imgTeletextPage.Width;

      //@
      _renderer.PageSelectText = "";
      //if(imgTeletextPage!=null && TeletextGrabber.TeletextCache!=null)
      //{
      //	TeletextGrabber.TeletextCache.SetPageSize(imgTeletextPage.Width,imgTeletextPage.Height);
      //}
      if (btnHidden != null)
      {
        _renderer.HiddenMode = true;
        btnHidden.Selected = true;
      }
      GetNewPage();
      Redraw();
      //@TeletextGrabber.TeletextCache.PageUpdatedEvent+=new MediaPortal.TV.Teletext.DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);
      _renderer.TransparentMode = false;


    }
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnPage100)
      {
        currentPageNumber = 0x100;
        currentSubPageNumber = 0;
        GetNewPage();
        Redraw();
      }
      if (control == btnPage200)
      {
        currentPageNumber = 0x200;
        currentSubPageNumber = 0;
        GetNewPage();
        Redraw();
      }
      if (control == btnPage300)
      {
        currentPageNumber = 0x300;
        currentSubPageNumber = 0;
        GetNewPage();
        Redraw();
      }
      if (control == btnHidden)
      {
        if (btnHidden != null)
        {
          _renderer.HiddenMode = btnHidden.Selected;
          GetNewPage();
          Redraw();
        }
      }
      if (control == btnSubPage)
      {
        if (btnSubPage != null)
        {
          currentSubPageNumber = btnSubPage.SelectedItem;
          GetNewPage();
          Redraw();
        }
      }
      if (control == btnFullscreen)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
      }
    }

    void GetNewPage()
    {

      
      byte[] page = TVHome.Card.GetTeletextPage(currentPageNumber, currentSubPageNumber);
      if (page != null && page.Length>1)
      {
        bitmapTeletextPage = _renderer.RenderPage(page, currentPageNumber, currentSubPageNumber);
        Redraw();
        _waiting = false;
        Log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", currentPageNumber, currentSubPageNumber);
      }
      else
      {
        _waiting = true;
      }

    }


    void OnKeyPressed(char chKey)
    {

      if (chKey == 'f' || chKey == 'F')
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
      }
      if (chKey == 'c' || chKey == 'C')
      {
        _renderer.PageSelectText = "";
        inputLine = "";
        GetNewPage();
        Redraw();
        return;
      }
      // top text
      if (chKey == 'h' || chKey == 'j' || chKey == 'k' || chKey == 'l' ||
        chKey == 'H' || chKey == 'J' || chKey == 'K' || chKey == 'L')
      {

        TvLibrary.Teletext.FastTextDecoder decoder = new TvLibrary.Teletext.FastTextDecoder();
        byte[] page = TVHome.Card.GetTeletextPage(currentPageNumber, currentSubPageNumber);
        decoder.Decode(page);
        int hexPage = 0;
        string topButton = new string(chKey, 1);
        switch (topButton.ToLower())
        {
          case "h":
            hexPage = decoder.Red;
            break;
          case "j":
            hexPage = decoder.Green;
            break;
          case "k":
            hexPage = decoder.Yellow;
            break;
          case "l":
            hexPage = decoder.Blue;
            break;
        }

        if (hexPage >= 0x100 && hexPage <= 0x899)
        {
          currentPageNumber = hexPage;
          currentSubPageNumber = 0;
          GetNewPage();
          Redraw();
          Log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", currentPageNumber, currentSubPageNumber);
          inputLine = "";
          return;
        }
      }

      //
      if ((chKey >= '0' && chKey <= '9') || (chKey == '+' || chKey == '-')) //navigation
      {
        if (chKey == '0' && inputLine.Length == 0) return;

        // page up
        if ((byte)chKey == 0x2B && currentPageNumber < 0x899) // +
        {
          currentPageNumber++;
          while ((currentPageNumber % 0xf) > 9) currentSubPageNumber++;
          currentSubPageNumber = 0;
          {
            GetNewPage();
            Redraw();
            Log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", currentPageNumber, currentSubPageNumber);
            inputLine = "";
            return;
          }

        }
        // page down
        if ((byte)chKey == 0x2D && currentPageNumber > 0x100) // -
        {
          currentPageNumber--;
          while ((currentPageNumber % 0xf) > 9) currentSubPageNumber--;
          currentSubPageNumber = 0;
          {
            GetNewPage();
            Redraw();
            Log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", currentPageNumber, currentSubPageNumber);
            inputLine = "";
            return;
          }

        }
        if (chKey >= '0' && chKey <= '9')
        {
          inputLine += chKey;
          {
            _renderer.PageSelectText = inputLine;
            GetNewPage();
            Redraw();
          }
        }

        if (inputLine.Length == 3)
        {
          // change channel
          currentPageNumber = Convert.ToInt16(inputLine, 16);
          currentSubPageNumber = 0;
          if (currentPageNumber < 0x100)
            currentPageNumber = 0x100;
          if (currentPageNumber > 0x899)
            currentPageNumber = 0x899;

          {
            GetNewPage();
            Redraw();
          }
          Log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", currentPageNumber, currentSubPageNumber);
          inputLine = "";

        }
        //
        // get page
        //
      }
    }


    public bool HasTeletext()
    {
      return (TVHome.Card.HasTeletext);
    }
    //
    //
    void ShowMessage(int page, int subpage)
    {
      string tmp = String.Format("{0:X}", page);
      int pageNr = Int32.Parse(tmp);
      tmp = String.Format("{0:X}", subpage);
      int subPageNr = Int32.Parse(tmp);
      if (lblMessage == null) return;
      lblMessage.Label = String.Format(GUILocalizeStrings.Get(596), pageNr, subPageNr); // Waiting for Page {0}/{1}...
      lblMessage.IsVisible = true;
    }


    public override void Process()
    {
      TimeSpan ts = DateTime.Now - _startTime;
      if (ts.TotalMilliseconds < 1000) return;
      if (_waiting)
      {
        GetNewPage();
        Redraw();
        _startTime = DateTime.Now;
        return;
      }
      TimeSpan tsRotation=TVHome.Card.TeletextRotation(currentPageNumber);
      if (ts.TotalMilliseconds < tsRotation.TotalMilliseconds) return;
      _startTime = DateTime.Now;

      if (currentPageNumber < 0x100) currentPageNumber = 0x100;
      if (currentPageNumber > 0x899) currentPageNumber = 0x899;
      _renderer.PageSelectText = String.Format("{0:X}", currentPageNumber);
      int NumberOfSubpages = TVHome.Card.SubPageCount(currentPageNumber);
      if (NumberOfSubpages > currentSubPageNumber)
      {
        currentSubPageNumber++;
        while ((currentSubPageNumber & 0xf) > 9) currentSubPageNumber++;
      }
      if (currentSubPageNumber >  NumberOfSubpages)
        currentSubPageNumber = 0;

      Log.Info("dvb-teletext page updated. {0:X}/{1:X} total:{2} rotspeed:{3}", currentPageNumber, currentSubPageNumber, NumberOfSubpages, tsRotation.TotalMilliseconds);
      GetNewPage();
      Redraw();
    }

    void Redraw()
    {
      Log.Info("dvb-teletext redraw()");
      try
      {

        if (bitmapTeletextPage == null)
        {
          ShowMessage(currentPageNumber, currentSubPageNumber);
          imgTeletextPage.FreeResources();
          imgTeletextPage.SetFileName("button_small_settings_nofocus.png");
          imgTeletextPage.AllocResources();
          return;
        }
        if (lblMessage != null)
          lblMessage.IsVisible = false;
        lock (imgTeletextPage)
        {
          System.Drawing.Image img = (Image)bitmapTeletextPage.Clone();
          imgTeletextPage.IsVisible = false;
          imgTeletextPage.FileName = "";
          //Utils.FileDelete(@"teletext.jpg");
          GUITextureManager.ReleaseTexture("[teletextpage]");
          //bitmapTeletextPage.Save(@"teletext.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
          imgTeletextPage.MemoryImage = img;
          imgTeletextPage.FileName = "[teletextpage]";
          imgTeletextPage.IsVisible = true;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }
    public override void Render(float timePassed)
    {
      lock (imgTeletextPage)
      {
        base.Render(timePassed);
      }
    }

  }// class
}// namespace
