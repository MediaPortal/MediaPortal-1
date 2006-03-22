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
  public class GUITVFullscreenTeletext : GUIWindow, IRenderLayer
  {
    [SkinControlAttribute(27)]
    protected GUILabelControl lblMessage = null;
    [SkinControlAttribute(500)]
    protected GUIImage imgTeletext = null;

    Bitmap bmpTeletextPage;
    string inputLine = String.Empty;
    int acutalPageNumber = 100;
    int actualSubPageNumber = 0;
    bool isPageDirty = false;

    public GUITVFullscreenTeletext()
    {
      GetID = (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myfsteletext.xml");
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
      }
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      TeletextGrabber.Grab = false;
      TeletextGrabber.TeletextCache.PageUpdatedEvent -= new MediaPortal.TV.Teletext.DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);

      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            Recorder.StopViewing();
          }
        }
      }
      GUILayerManager.UnRegisterLayer(this);
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      acutalPageNumber = 100;
      actualSubPageNumber = 0;
      TeletextGrabber.Grab = true;
      TeletextGrabber.TeletextCache.PageUpdatedEvent += new MediaPortal.TV.Teletext.DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);
      TeletextGrabber.TeletextCache.TransparentMode = true;

      ShowMessage(acutalPageNumber, actualSubPageNumber);



      if (imgTeletext != null)
      {
        imgTeletext.Width = GUIGraphicsContext.OverScanWidth;
        imgTeletext.Height = GUIGraphicsContext.OverScanHeight;
        imgTeletext.XPosition = GUIGraphicsContext.OverScanLeft;
        imgTeletext.YPosition = GUIGraphicsContext.OverScanTop;
        TeletextGrabber.TeletextCache.SetPageSize(imgTeletext.Width, imgTeletext.Height);
      }
      GetNewPage();
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);
    }

    void SubpageUp()
    {
      if (actualSubPageNumber < 128)
      {
        actualSubPageNumber++;
        GetNewPage();
      }
    }
    void SubpageDown()
    {
      if (actualSubPageNumber > 0)
      {
        actualSubPageNumber--;
        GetNewPage();
      }
    }

    void GetNewPage()
    {
      if (TeletextGrabber.TeletextCache != null)
      {
        bmpTeletextPage = TeletextGrabber.TeletextCache.GetPage(acutalPageNumber, actualSubPageNumber);
        Redraw();
        Log.Write("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(acutalPageNumber), Convert.ToString(actualSubPageNumber));
      }
    }


    void OnKeyPressed(char chKey)
    {
      if (chKey == 'w' || chKey == 'W')
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
      }
      if (chKey == 'c' || chKey == 'C')
      {
        if (TeletextGrabber.TeletextCache != null)
          TeletextGrabber.TeletextCache.PageSelectText = "";
        inputLine = "";
        GetNewPage();
        return;
      }
      // top text
      if (chKey == 'h' || chKey == 'j' || chKey == 'k' || chKey == 'l' ||
        chKey == 'H' || chKey == 'J' || chKey == 'K' || chKey == 'L')
      {

        if (TeletextGrabber.TeletextCache == null)
          return;

        int hexPage = 0;
        string topButton = new string(chKey, 1);
        switch (topButton.ToLower())
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

        int pageNr = mag * 100 + tens * 10 + hexPage;
        if (pageNr >= 100 && pageNr <= 899)
        {
          acutalPageNumber = pageNr;
          actualSubPageNumber = 0;
          bmpTeletextPage = TeletextGrabber.TeletextCache.GetPage(acutalPageNumber, actualSubPageNumber);
          Redraw();
          Log.Write("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(acutalPageNumber), Convert.ToString(actualSubPageNumber));
          inputLine = "";
          return;
        }
      }
      if ((chKey >= '0' && chKey <= '9') || (chKey == '+' || chKey == '-')) //navigation
      {
        if (chKey == '0' && inputLine.Length == 0) return;

        // page up
        if ((byte)chKey == 0x2B && acutalPageNumber < 899) // +
        {
          acutalPageNumber++;
          actualSubPageNumber = 0;
          if (TeletextGrabber.TeletextCache != null)
          {
            bmpTeletextPage = TeletextGrabber.TeletextCache.GetPage(acutalPageNumber, actualSubPageNumber);
            Redraw();
            Log.Write("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(acutalPageNumber), Convert.ToString(actualSubPageNumber));
            inputLine = String.Empty;
            return;
          }

        }
        // page down
        if ((byte)chKey == 0x2D && acutalPageNumber > 100) // -
        {
          acutalPageNumber--;
          actualSubPageNumber = 0;
          if (TeletextGrabber.TeletextCache != null)
          {
            bmpTeletextPage = TeletextGrabber.TeletextCache.GetPage(acutalPageNumber, actualSubPageNumber);
            Redraw();
            Log.Write("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(acutalPageNumber), Convert.ToString(actualSubPageNumber));
            inputLine = String.Empty;
            return;
          }

        }
        if (chKey >= '0' && chKey <= '9')
        {
          inputLine += chKey;
          if (TeletextGrabber.TeletextCache != null)
          {
            TeletextGrabber.TeletextCache.PageSelectText = inputLine;
            GetNewPage();
          }
        }

        if (inputLine.Length == 3)
        {
          // change channel
          acutalPageNumber = Convert.ToInt16(inputLine);
          actualSubPageNumber = 0;
          if (acutalPageNumber < 100)
            acutalPageNumber = 100;
          if (acutalPageNumber > 899)
            acutalPageNumber = 899;
          if (TeletextGrabber.TeletextCache != null)
          {
            TeletextGrabber.TeletextCache.PageSelectText = "";
            bmpTeletextPage = TeletextGrabber.TeletextCache.GetPage(acutalPageNumber, actualSubPageNumber);
            Redraw();
          }
          Log.Write("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(acutalPageNumber), Convert.ToString(actualSubPageNumber));
          inputLine = String.Empty;

        }
        //
        // get page
        //
      }
    }

    //
    //
    void ShowMessage(int page, int subpage)
    {
      if (lblMessage == null) return;
      lblMessage.Label = String.Format("Waiting for Page {0}/{1}...", page, subpage);
      lblMessage.IsVisible = true;

    }
    //
    //
    private void dvbTeletextParser_PageUpdatedEvent()
    {
      // make sure the callback returns as soon as possible!!
      // here is only a flag set to true, the bitmap is getting
      // in a timer-elapsed event!
      if (TeletextGrabber.TeletextCache == null)
        return;
      if (TeletextGrabber.TeletextCache.PageSelectText.IndexOf("-") != -1)// page select is running
        return;

      if (GUIWindowManager.ActiveWindow == GetID)
      {
        isPageDirty = true;
      }
    }

    public override void Process()
    {
      if (isPageDirty == true)
      {
        if (actualSubPageNumber < 100) actualSubPageNumber = 100;
        if (actualSubPageNumber > 899) actualSubPageNumber = 899;
        Log.Write("dvb-teletext page updated. {0:X}/{1}", acutalPageNumber, actualSubPageNumber);
        int NumberOfSubpages = TeletextGrabber.TeletextCache.NumberOfSubpages(acutalPageNumber);
        if (NumberOfSubpages > actualSubPageNumber)
        {
          actualSubPageNumber++;
        }
        else if (actualSubPageNumber >= NumberOfSubpages)
          actualSubPageNumber = 1;

        bmpTeletextPage = TeletextGrabber.TeletextCache.GetPage(acutalPageNumber, actualSubPageNumber);
        Redraw();
        isPageDirty = false;
      }
    }

    void Redraw()
    {
      Log.Write("dvb-teletext redraw()");
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
          GUITextureManager.ReleaseTexture("#useMemoryImage");
          Utils.FileDelete(@"teletext.png");
          //img.Save(@"teletext.png",System.Drawing.Imaging.ImageFormat.Png);
          imgTeletext.MemoryImage = img;
          imgTeletext.FileName = "#useMemoryImage";
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
        Log.Write(ex);
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