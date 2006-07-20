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
  public class TVTeletextFullScreen : GUIWindow, IRenderLayer
  {
    [SkinControlAttribute(27)]
    protected GUILabelControl lblMessage = null;
    [SkinControlAttribute(500)]
    protected GUIImage imgTeletext = null;

    Bitmap bmpTeletextPage;
    string inputLine = String.Empty;
    int acutalPageNumber = 0x100;
    int actualSubPageNumber = 0;
    bool isPageDirty = false;
    bool _isFullScreenVideo = false;

    bool _waiting = false;
    DateTime _startTime = DateTime.MinValue;
    TvLibrary.Teletext.TeletextPageRenderer _renderer = new TvLibrary.Teletext.TeletextPageRenderer();


    public TVTeletextFullScreen()
    {
      GetID = (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myfsteletext.xml");
    }
    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT, this);
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
        case Action.ActionType.ACTION_CONTEXT_MENU:
          GUIWindowManager.ShowPreviousWindow();
          break;
      }
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      TVHome.Card.GrabTeletext = false;


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
      GUILayerManager.UnRegisterLayer(this);
      GUIGraphicsContext.IsFullScreenVideo = _isFullScreenVideo;
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      _isFullScreenVideo = GUIGraphicsContext.IsFullScreenVideo;
      base.OnPageLoad();

      acutalPageNumber = 0x100;
      actualSubPageNumber = 0;

      TVHome.Card.GrabTeletext = true;
      _renderer = new TvLibrary.Teletext.TeletextPageRenderer();
      _renderer.TransparentMode = true;

      ShowMessage(acutalPageNumber, actualSubPageNumber);



      if (imgTeletext != null)
      {
        imgTeletext.Width = GUIGraphicsContext.OverScanWidth;
        imgTeletext.Height = GUIGraphicsContext.OverScanHeight;
        imgTeletext.XPosition = GUIGraphicsContext.OverScanLeft;
        imgTeletext.YPosition = GUIGraphicsContext.OverScanTop;
        _renderer.Width=imgTeletext.Width;
        _renderer.Height= imgTeletext.Height;
      }
      GetNewPage();
      Redraw();
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);
    }

    void SubpageUp()
    {
      if (actualSubPageNumber < 0x79)
      {
        actualSubPageNumber++;
        while ((actualSubPageNumber % 0xf) > 9) actualSubPageNumber++;
        GetNewPage();
        Redraw();
      }
    }
    void SubpageDown()
    {
      if (actualSubPageNumber > 0)
      {
        actualSubPageNumber--;
        while ((actualSubPageNumber % 0xf) > 9) actualSubPageNumber--;
        GetNewPage();
        Redraw();
      }
    }

    void GetNewPage()
    {

      byte[] page = TVHome.Card.GetTeletextPage(acutalPageNumber, actualSubPageNumber);
      if (page != null)
      {
        bmpTeletextPage = _renderer.RenderPage(page, acutalPageNumber, actualSubPageNumber);
        Redraw();
        _waiting = false;
        _log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", acutalPageNumber, actualSubPageNumber);
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
        byte[] page = TVHome.Card.GetTeletextPage(acutalPageNumber, actualSubPageNumber);
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
          acutalPageNumber = hexPage;
          actualSubPageNumber = 0;
          GetNewPage();
          Redraw();
          _log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", acutalPageNumber, actualSubPageNumber);
          inputLine = "";
          return;
        }
      }

      //
      if ((chKey >= '0' && chKey <= '9') || (chKey == '+' || chKey == '-')) //navigation
      {
        if (chKey == '0' && inputLine.Length == 0) return;

        // page up
        if ((byte)chKey == 0x2B && acutalPageNumber < 0x899) // +
        {
          acutalPageNumber++;
          while ((acutalPageNumber % 0xf) > 9) actualSubPageNumber++;
          actualSubPageNumber = 0;
          {
            GetNewPage();
            Redraw();
            _log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", acutalPageNumber, actualSubPageNumber);
            inputLine = "";
            return;
          }

        }
        // page down
        if ((byte)chKey == 0x2D && acutalPageNumber > 0x100) // -
        {
          acutalPageNumber--;
          while ((acutalPageNumber % 0xf) > 9) actualSubPageNumber--;
          actualSubPageNumber = 0;
          {
            GetNewPage();
            Redraw();
            _log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", acutalPageNumber, actualSubPageNumber);
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
          acutalPageNumber = Convert.ToInt16(inputLine, 16);
          actualSubPageNumber = 0;
          if (acutalPageNumber < 0x100)
            acutalPageNumber = 0x100;
          if (acutalPageNumber > 0x899)
            acutalPageNumber = 0x899;

          {
            GetNewPage();
            Redraw();
          }
          _log.Info("dvb-teletext: select page {0:X} / subpage {1:X}", acutalPageNumber, actualSubPageNumber);
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
      TimeSpan tsRotation = TVHome.Card.TeletextRotation( acutalPageNumber);
      if (ts.TotalMilliseconds < tsRotation.TotalMilliseconds) return;
      _startTime = DateTime.Now;

      if (acutalPageNumber < 0x100) acutalPageNumber = 0x100;
      if (acutalPageNumber > 0x899) acutalPageNumber = 0x899;
      _renderer.PageSelectText = String.Format("{0:X}", acutalPageNumber);
      int NumberOfSubpages = TVHome.Card.SubPageCount(acutalPageNumber);
      if (NumberOfSubpages > actualSubPageNumber)
      {
        actualSubPageNumber++;
        while ((actualSubPageNumber&0xf)>9) actualSubPageNumber++;
        
      }
      if (actualSubPageNumber >  NumberOfSubpages)
        actualSubPageNumber = 0;

      _log.Info("dvb-teletext page updated. {0:X}/{1:X} total:{2} rotspeed:{3}", acutalPageNumber, actualSubPageNumber, NumberOfSubpages, tsRotation.TotalMilliseconds);
      GetNewPage();
      Redraw();
    }
    void Redraw()
    {
      _log.Info("dvb-teletext redraw()");
      try
      {

        if (bmpTeletextPage == null)
        {
          ShowMessage(acutalPageNumber, actualSubPageNumber);
          imgTeletext.FreeResources();
          imgTeletext.SetFileName("button_small_settings_nofocus.png");
          imgTeletext.AllocResources();
          return;
        }
        if (lblMessage != null)
          lblMessage.IsVisible = false;


        lock (imgTeletext)
        {
          System.Drawing.Image img = (Image)bmpTeletextPage.Clone();
          imgTeletext.IsVisible = false;
          imgTeletext.FileName = "";
          GUITextureManager.ReleaseTexture("[teletextpage]");
          MediaPortal.Util.Utils.FileDelete(@"teletext.png");
          //img.Save(@"teletext.png",System.Drawing.Imaging.ImageFormat.Png);
          imgTeletext.MemoryImage = img;
          imgTeletext.FileName = "[teletextpage]";
          imgTeletext.ColorKey = System.Drawing.Color.HotPink.ToArgb();
          imgTeletext.IsVisible = true;
          imgTeletext.Centered = false;
          imgTeletext.KeepAspectRatio = false;
          int left = GUIGraphicsContext.Width / 20; // 5%
          int top = GUIGraphicsContext.Height / 20; // 5%
          imgTeletext.SetPosition(left, top);
          imgTeletext.Width = GUIGraphicsContext.Width - (2 * left);
          imgTeletext.Height = GUIGraphicsContext.Height - (2 * top);
        }
      }
      catch (Exception ex)
      {
        _log.Error(ex);
      }
    }

    public override void Render(float timePassed)
    {
      GUIGraphicsContext.IsFullScreenVideo = true;
      lock (imgTeletext)
      {
        imgTeletext.Render(timePassed);
      }
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

  }// class
}// namespace