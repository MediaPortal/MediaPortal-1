using System;
using System.Drawing;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Teletext;

namespace MediaPortal.GUI.TV
{

  #region enum

  public enum TeletextButton
  {
    Red,
    Green,
    Yellow,
    Blue
  }

  #endregion

  /// <summary>
  /// Common class for both teletext windows
  /// </summary>
  public class GUITVTeletextBase : GUIWindow
  {
    #region gui controls

    [SkinControl(27)] protected GUILabelControl lblMessage = null;
    [SkinControl(500)] protected GUIImage imgTeletextFirst = null;
    [SkinControl(501)] protected GUIImage imgTeletextSecond = null;

    #endregion

    #region variables

    protected Bitmap bmpTeletextPage;
    protected string inputLine = string.Empty;
    protected int currentPageNumber = 100;
    protected int currentSubPageNumber = 1;
    private static Mutex _requestmutex = new Mutex();

    protected int[] _requestPage = new int[2];
    protected int[] _requestSubPage = new int[2];
    protected int _requestPtr = 0;
    protected int _renderPtr = 0;
    protected byte[] receivedPage;
    protected DateTime _startTime = DateTime.MinValue;
    protected bool _hiddenMode;
    protected bool _transparentMode;
    protected Thread _updateThread;
    protected bool _updateThreadStop;
    protected int _numberOfRequestedUpdates = 0;
    protected bool _rememberLastValues;
    protected bool _updating;
    protected int _percentageOfMaximumHeight;
    protected bool _updateFirst = true;

    #endregion

    #region Property

    public override bool IsTv
    {
      get { return true; }
    }

    #endregion

    #region Initialization method

    /// <summary>
    /// Initialize the window
    /// </summary>
    /// <param name="fullscreenMode">Indicate, if is fullscreen mode</param>
    protected void InitializeWindow(bool fullscreenMode)
    {
      LoadSettings();
      lblMessage.Label = "";
      lblMessage.Visible = false;
      // Activate teletext grabbing in the server
      TeletextGrabber.Grab = true;
      TeletextGrabber.TeletextCache.PageUpdatedEvent += new DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);


      // Remember the start time
      _startTime = DateTime.MinValue;

      // Initialize the render
      TeletextGrabber.TeletextCache.TransparentMode = _transparentMode;
      TeletextGrabber.TeletextCache.FullscreenMode = fullscreenMode;
      TeletextGrabber.TeletextCache.PageSelectText = "100";
      TeletextGrabber.TeletextCache.PercentageOfMaximumHeight = _percentageOfMaximumHeight;

      // Initialize the images
      if (imgTeletextFirst != null)
      {
        imgTeletextFirst.ColorKey = Color.HotPink.ToArgb();
        TeletextGrabber.TeletextCache.SetPageSize(imgTeletextFirst.Width, imgTeletextFirst.Height);
      }
      if (imgTeletextSecond != null)
      {
        imgTeletextSecond.ColorKey = Color.HotPink.ToArgb();
        TeletextGrabber.TeletextCache.SetPageSize(imgTeletextSecond.Width, imgTeletextSecond.Height);
      }

      _requestPtr = 0;
      _renderPtr = 0;
      _requestPage[0] = -1;
      _requestPage[1] = -1;
      // Set the current page to the index page
      currentPageNumber = 100;
      currentSubPageNumber = 1;

      // Request an update
      RequestPage();
      // Create an update thread and set it's priority to lowest
      _updateThreadStop = false;
      _updateThread = new Thread(UpdatePage);
      _updateThread.Name = "TeletextUpdater";
      _updateThread.Priority = ThreadPriority.Normal;
      _updateThread.IsBackground = true;
      _updateThread.Start();
    }

    #endregion

    #region OnAction

    public override void OnAction(Action action)
    {
      // if we have a keypress or a remote button press then check if it is a number and add it to the inputLine
      char key = (char) 0;
      if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
      {
        if (action.m_key != null)
        {
          if (action.m_key.KeyChar >= '0' && action.m_key.KeyChar <= '9')
          {
            // Get offset to item
            key = (char) action.m_key.KeyChar;
          }
        }
        if (key == (char) 0)
        {
          return;
        }
        UpdateInputLine(key);
      }
      switch (action.wID)
      {
        case Action.ActionType.ACTION_REMOTE_RED_BUTTON:
          // Red teletext button
          showTeletextButtonPage(TeletextButton.Red);
          break;
        case Action.ActionType.ACTION_REMOTE_GREEN_BUTTON:
          // Green teletext button
          showTeletextButtonPage(TeletextButton.Green);
          break;
        case Action.ActionType.ACTION_REMOTE_YELLOW_BUTTON:
          // Yellow teletext button
          showTeletextButtonPage(TeletextButton.Yellow);
          break;
        case Action.ActionType.ACTION_REMOTE_BLUE_BUTTON:
          // Blue teletext button
          showTeletextButtonPage(TeletextButton.Blue);
          break;
        case Action.ActionType.ACTION_REMOTE_SUBPAGE_UP:
          // Subpage up
          SubpageUp();
          break;
        case Action.ActionType.ACTION_REMOTE_SUBPAGE_DOWN:
          // Subpage down
          SubpageDown();
          break;
        case Action.ActionType.ACTION_NEXT_TELETEXTPAGE:
          // Page up
          PageUp();
          break;
        case Action.ActionType.ACTION_PREV_TELETEXTPAGE:
          // Page down
          PageDown();
          break;
        case Action.ActionType.ACTION_CONTEXT_MENU:
          // Show previous window
          GUIWindowManager.ShowPreviousWindow();
          break;
        case Action.ActionType.ACTION_SWITCH_TELETEXT_HIDDEN:
          //Change Hidden Mode
          _hiddenMode = !_hiddenMode;
          TeletextGrabber.TeletextCache.HiddenMode = _hiddenMode;
          // Rerender the image
          RequestPage();
          break;
        case Action.ActionType.ACTION_SHOW_INDEXPAGE:
          // Index page
          showNewPage(100);
          break;
      }
      base.OnAction(action);
    }

    #endregion

    #region Navigation methods

    /// <summary>
    /// Selects the next subpage, if possible
    /// </summary>
    protected void SubpageUp()
    {
      if (currentSubPageNumber < 80)
      {
        if (currentSubPageNumber < TeletextGrabber.TeletextCache.NumberOfSubpages(currentPageNumber))
        {
          currentSubPageNumber++;
          RequestPage();
        }
      }
    }

    /// <summary>
    /// Selects the previous subpage, if possible
    /// </summary>
    protected void SubpageDown()
    {
      if (currentSubPageNumber > 1)
      {
        currentSubPageNumber--;
        RequestPage();
      }
    }

    /// <summary>
    /// Selects the next page, if possible
    /// </summary>
    protected void PageUp()
    {
      if (currentPageNumber < 899)
      {
        currentPageNumber++;
        TeletextGrabber.TeletextCache.PageSelectText = Convert.ToString(currentPageNumber);
        currentSubPageNumber = 1;
        RequestPage();
        inputLine = "";
        return;
      }
    }

    /// <summary>
    /// Selects the previous subpage, if possible
    /// </summary>
    protected void PageDown()
    {
      if (currentPageNumber > 100)
      {
        currentPageNumber--;
        TeletextGrabber.TeletextCache.PageSelectText = Convert.ToString(currentPageNumber);
        currentSubPageNumber = 1;
        RequestPage();
        inputLine = "";
        return;
      }
    }

    /// <summary>
    /// Updates the header and the selected page text
    /// </summary>
    /// <param name="key">Key</param>
    protected void UpdateInputLine(char key)
    {
      Log.Info("dvb-teletext: key received: " + key);
      if (inputLine.Length == 0 && (key == '0' || key == '9'))
      {
        return;
      }
      inputLine += key;
      TeletextGrabber.TeletextCache.PageSelectText = inputLine;
      if (inputLine.Length == 3)
      {
        // change channel
        currentPageNumber = Int32.Parse(inputLine);
        currentSubPageNumber = 1;
        if (currentPageNumber < 100)
        {
          currentPageNumber = 100;
        }
        if (currentPageNumber > 899)
        {
          currentPageNumber = 899;
        }
        RequestPage();
        inputLine = "";
      }
    }

    /// <summary>
    /// Selects a teletext page, based on the teletext button
    /// </summary>
    /// <param name="button"></param>
    protected void showTeletextButtonPage(TeletextButton button)
    {
      int hexPage = 0x100;
      switch (button)
      {
        case TeletextButton.Red:
          hexPage = TeletextGrabber.TeletextCache.PageRed;
          break;
        case TeletextButton.Green:
          hexPage = TeletextGrabber.TeletextCache.PageGreen;
          break;
        case TeletextButton.Yellow:
          hexPage = TeletextGrabber.TeletextCache.PageYellow;
          break;
        case TeletextButton.Blue:
          hexPage = TeletextGrabber.TeletextCache.PageBlue;
          break;
      }
      int mag = hexPage/0x100;
      hexPage -= mag*0x100;

      int tens = hexPage/0x10;
      hexPage -= tens*0x10;

      int pageNr = mag*100 + tens*10 + hexPage;
      if (pageNr >= 100 && pageNr <= 899)
      {
        showNewPage(pageNr);
      }
    }

    /// <summary>
    /// Displays a new page, with the give page number
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    protected void showNewPage(int pageNumber)
    {
      if (pageNumber >= 100 && pageNumber <= 899)
      {
        currentPageNumber = pageNumber;
        TeletextGrabber.TeletextCache.PageSelectText = Convert.ToString(currentPageNumber);
        currentSubPageNumber = 1;
        inputLine = "";
        RequestPage();
        return;
      }
    }

    #endregion

    #region Update, Process, Cache event callback and Redraw

    /// <summary>
    /// Callback for the update event
    /// </summary>
    protected void dvbTeletextParser_PageUpdatedEvent()
    {
      // make sure the callback returns as soon as possible!!
      // here is only a flag set to true, the bitmap is getting
      // in a timer-elapsed event!
      if (TeletextGrabber.TeletextCache == null)
      {
        return;
      }
      //if (TeletextGrabber.TeletextCache.PageSelectText.IndexOf("-") != -1)// page select is running
      //  return;
      if (GUIWindowManager.ActiveWindow == GetID)
      {
        if (currentPageNumber < 100)
        {
          currentPageNumber = 100;
        }
        if (currentPageNumber > 899)
        {
          currentPageNumber = 899;
        }
        int NumberOfSubpages = TeletextGrabber.TeletextCache.NumberOfSubpages(currentPageNumber);
        Log.Debug("dvb-teletext page updated. {0:X}/{1}", Convert.ToString(currentPageNumber),
                  Convert.ToString(currentSubPageNumber));
        RequestPage();
        if (NumberOfSubpages > currentSubPageNumber)
        {
          currentSubPageNumber++;
        }
        else if (currentSubPageNumber >= NumberOfSubpages)
        {
          currentSubPageNumber = 1;
        }
      }
    }

    /// <summary>
    /// Method of the update thread
    /// </summary>
    protected void UpdatePage()
    {
      Log.Debug("dvb-teletext: Thread render create");

      // While not stop the thread, continue
      while (!_updateThreadStop)
      {
        // Is there an update request, than update
        if (_requestPage[_requestPtr] != -1)
        {
          // Ok to check because thread updates _reqPtr
          Log.Debug("dvb-teletext: Thread render page {0} / subpage {1} /ptr {2}", currentPageNumber,
                    currentSubPageNumber, _requestPtr);
          _renderPtr = _requestPtr;
          _requestmutex.WaitOne();
          _requestPtr = (_requestPtr + 1)%2;
          _requestmutex.ReleaseMutex();

          GetNewPage(_requestPage[_renderPtr], _requestSubPage[_renderPtr]);
          _requestPage[_renderPtr] = -1;
          Log.Debug("dvb-teletext: Thread end - render page {0} / subpage {1} /ptr {2}", currentPageNumber,
                    currentSubPageNumber, _requestPtr);
        }
        else
        {
          // Otherwise sleep for 20ms
          Thread.Sleep(20);
        }
      }
      Log.Debug("dvb-teletext: Thread render exit");
    }

    /// <summary>
    /// Retrieve the new page from the teletext grabber
    /// </summary>
    protected void GetNewPage(int page, int subpage)
    {
      if (TeletextGrabber.TeletextCache != null)
      {
        Log.Debug("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(page), Convert.ToString(subpage));
        bmpTeletextPage = TeletextGrabber.TeletextCache.GetPage(page, subpage);

        try
        {
          Log.Debug("dvb-teletext redraw start");
          if (_updateFirst)
          {
            _updateFirst = !_updateFirst;
            // Update first image. Step 1 make it invisible
            imgTeletextFirst.IsVisible = false;
            // Clear the old image
            Image img = (Image) bmpTeletextPage.Clone();
            imgTeletextFirst.FileName = "";
            GUITextureManager.ReleaseTexture("[teletextpage]");
            // Set the new image and make the image visible again
            imgTeletextFirst.MemoryImage = img;
            imgTeletextFirst.FileName = "[teletextpage]";
            imgTeletextFirst.Centered = false;
            imgTeletextFirst.KeepAspectRatio = false;
            imgTeletextFirst.IsVisible = true;
          }
          else
          {
            _updateFirst = !_updateFirst;
            // Update second image
            imgTeletextSecond.IsVisible = false;
            // Clear the old image
            Image img2 = (Image) bmpTeletextPage.Clone();
            imgTeletextSecond.FileName = "";
            GUITextureManager.ReleaseTexture("[teletextpage2]");
            // Set the new image and make the image visible again
            imgTeletextSecond.MemoryImage = img2;
            imgTeletextSecond.FileName = "[teletextpage2]";
            imgTeletextSecond.Centered = false;
            imgTeletextSecond.KeepAspectRatio = false;
            imgTeletextSecond.IsVisible = true;
          }
          Log.Debug("dvb-teletext redraw End");
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
      }
    }

    protected void RequestPage()
    {
      _requestmutex.WaitOne();
      Log.Debug("dvb-teletext: RequestPage page {0} / subpage {1} /ptr {2}", currentPageNumber, currentSubPageNumber,
                _requestPtr);
      _requestPage[_requestPtr] = currentPageNumber;
      _requestSubPage[_requestPtr] = currentSubPageNumber;
      _requestmutex.ReleaseMutex();
    }

    #endregion

    #region Serialisation

    /// <summary>
    /// Load the settings
    /// </summary>
    protected void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _hiddenMode = xmlreader.GetValueAsBool("mytv", "teletextHidden", false);
        _transparentMode = xmlreader.GetValueAsBool("mytv", "teletextTransparent", false);
        _rememberLastValues = xmlreader.GetValueAsBool("mytv", "teletextRemember", true);
        _percentageOfMaximumHeight = xmlreader.GetValueAsInt("mytv", "teletextMaxFontSize", 100);
      }
    }

    /// <summary>
    /// Store the settings, if the user wants it
    /// </summary>
    protected void SaveSettings()
    {
      if (_rememberLastValues)
      {
        using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlreader.SetValueAsBool("mytv", "teletextHidden", _hiddenMode);
          xmlreader.SetValueAsBool("mytv", "teletextTransparent", _transparentMode);
        }
      }
    }

    #endregion
  }
}