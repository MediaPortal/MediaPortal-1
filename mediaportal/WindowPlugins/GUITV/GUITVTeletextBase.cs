using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using MediaPortal.TV.Teletext;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;


namespace MediaPortal.GUI.TV {
  #region enum
  public enum TeletextButton {
    Red,
    Green,
    Yellow,
    Blue
  }
  #endregion

  /// <summary>
  /// Common class for both teletext windows
  /// </summary>
  public class GUITVTeletextBase : GUIWindow {
    #region gui controls
    [SkinControlAttribute(27)]
    protected GUILabelControl lblMessage = null;
    [SkinControlAttribute(500)]
    protected GUIImage imgTeletextForeground = null;
    [SkinControlAttribute(501)]
    protected GUIImage imgTeletextBackground = null;
    #endregion

    #region variables
    protected Bitmap bmpTeletextPage;
    protected string inputLine = String.Empty;
    protected int currentPageNumber = 100;
    protected int currentSubPageNumber = 1;
    protected int receivedPageNumber = 100;
    protected int receivedSubPageNumber = 1;
    protected byte[] receivedPage;
    protected bool _updatingForegroundImage;
    protected bool _updatingBackgroundImage;
    protected bool _waiting = false;
    protected DateTime _startTime = DateTime.MinValue;
    protected bool _hiddenMode;
    protected bool _transparentMode;
    protected Thread _updateThread;
    protected bool _updateThreadStop;
    protected int _numberOfRequestedUpdates = 0;
    protected bool _rememberLastValues;
    protected bool _updating;
    #endregion

    #region Property
    public override bool IsTv {
      get {
        return true;
      }
    }
    #endregion

    #region Initialization method
    /// <summary>
    /// Initialize the window
    /// </summary>
    /// <param name="fullscreenMode">Indicate, if is fullscreen mode</param>
    protected void InitializeWindow(bool fullscreenMode) {
      LoadSettings();
      _numberOfRequestedUpdates = 0;
      // Create an update thread and set it's priority to lowest
      _updateThreadStop = false;
      _updateThread = new Thread(UpdatePage);
      _updateThread.Priority = ThreadPriority.Lowest;
      _updateThread.IsBackground = true;
      _updateThread.Start();
      lblMessage.Label = "";
      lblMessage.Visible = false;
      // Activate teletext grabbing in the server
      TeletextGrabber.Grab = true;
      TeletextGrabber.TeletextCache.PageUpdatedEvent += new MediaPortal.TV.Teletext.DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);
      // Set the current page to the index page
      currentPageNumber = 100;
      currentSubPageNumber = 1;

      // Remember the start time
      _startTime = DateTime.MinValue;

      // Initialize the render
      TeletextGrabber.TeletextCache.TransparentMode = _transparentMode;
      TeletextGrabber.TeletextCache.FullscreenMode = fullscreenMode;
      TeletextGrabber.TeletextCache.PageSelectText = "100";

      // Initialize the images
      if (imgTeletextForeground != null) {
        imgTeletextForeground.ColorKey = System.Drawing.Color.HotPink.ToArgb();
        TeletextGrabber.TeletextCache.SetPageSize(imgTeletextForeground.Width, imgTeletextForeground.Height);
      }
      if (imgTeletextBackground != null) {
        imgTeletextBackground.ColorKey = System.Drawing.Color.HotPink.ToArgb();
        TeletextGrabber.TeletextCache.SetPageSize(imgTeletextBackground.Width, imgTeletextBackground.Height);
      }
      // Request an update
      _numberOfRequestedUpdates++;
    }
    #endregion

    #region OnAction
    public override void OnAction(Action action) {
      // if we have a keypress or a remote button press then check if it is a number and add it to the inputLine
      char key = (char)0;
      if (action.wID == Action.ActionType.ACTION_KEY_PRESSED) {
        if (action.m_key != null) {
          if (action.m_key.KeyChar >= '0' && action.m_key.KeyChar <= '9') {
            // Get offset to item
            key = (char)action.m_key.KeyChar;
          }
        }
        if (key == (char)0) return;
        UpdateInputLine(key);
      }
      switch (action.wID) {
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
          _numberOfRequestedUpdates++;
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
    protected void SubpageUp() {
      if (currentSubPageNumber < 80) {
        if (currentSubPageNumber < TeletextGrabber.TeletextCache.NumberOfSubpages(currentPageNumber)) {
          currentSubPageNumber++;
          _numberOfRequestedUpdates++;
          Log.Info("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
        }
      }
    }

    /// <summary>
    /// Selects the previous subpage, if possible
    /// </summary>
    protected void SubpageDown() {
      if (currentSubPageNumber > 1) {
        currentSubPageNumber--;
        _numberOfRequestedUpdates++;
        Log.Info("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
      }
    }

    /// <summary>
    /// Selects the next page, if possible
    /// </summary>
    protected void PageUp() {
      if (currentPageNumber < 899) {
        currentPageNumber++;
        TeletextGrabber.TeletextCache.PageSelectText = Convert.ToString(currentPageNumber);
        currentSubPageNumber = 1;
        _numberOfRequestedUpdates++;
        Log.Info("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
        inputLine = "";
        return;
      }
    }

    /// <summary>
    /// Selects the previous subpage, if possible
    /// </summary>
    protected void PageDown() {
      if (currentPageNumber > 100) {
        currentPageNumber--;
        TeletextGrabber.TeletextCache.PageSelectText = Convert.ToString(currentPageNumber);
        currentSubPageNumber = 1;
        _numberOfRequestedUpdates++;
        Log.Info("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
        inputLine = "";
        return;
      }
    }

    /// <summary>
    /// Updates the header and the selected page text
    /// </summary>
    /// <param name="key">Key</param>
    protected void UpdateInputLine(char key) {
      Log.Info("dvb-teletext: key received: " + key);
      if (inputLine.Length == 0 && (key == '0' || key == '9')) {
        return;
      }
      inputLine += key;
      TeletextGrabber.TeletextCache.PageSelectText = inputLine;
      _numberOfRequestedUpdates++;
      if (inputLine.Length == 3) {
        // change channel
        currentPageNumber = Int32.Parse(inputLine);
        currentSubPageNumber = 1;
        if (currentPageNumber < 100)
          currentPageNumber = 100;
        if (currentPageNumber > 899)
          currentPageNumber = 899;
        _numberOfRequestedUpdates++;
        Log.Info("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
        inputLine = "";
      }
    }

    /// <summary>
    /// Selects a teletext page, based on the teletext button
    /// </summary>
    /// <param name="button"></param>
    protected void showTeletextButtonPage(TeletextButton button) {
      int hexPage=0x100;
      switch (button) {
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
      int mag = hexPage / 0x100;
      hexPage -= mag * 0x100;

      int tens = hexPage / 0x10;
      hexPage -= tens * 0x10;

      int pageNr = mag * 100 + tens * 10 + hexPage;
      if (pageNr >= 100 && pageNr <= 899) {
        showNewPage(pageNr);
      }
    }

    /// <summary>
    /// Displays a new page, with the give page number
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    protected void showNewPage(int pageNumber) {
      if (pageNumber >= 100 && pageNumber <= 899) {
        currentPageNumber = pageNumber;
        TeletextGrabber.TeletextCache.PageSelectText = Convert.ToString(currentPageNumber);
        currentSubPageNumber = 1;
        inputLine = "";
        _numberOfRequestedUpdates++;
        Log.Info("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
        return;
      }
    }
    #endregion

    #region Update, Process, Cache event callback and Redraw
    /// <summary>
    /// Callback for the update event
    /// </summary>
    protected void dvbTeletextParser_PageUpdatedEvent() {
      // make sure the callback returns as soon as possible!!
      // here is only a flag set to true, the bitmap is getting
      // in a timer-elapsed event!
      if (TeletextGrabber.TeletextCache == null)
        return;
      if (TeletextGrabber.TeletextCache.PageSelectText.IndexOf("-") != -1)// page select is running
        return;
      if (GUIWindowManager.ActiveWindow == GetID) {
        //_isDirty = true;
        if (currentPageNumber < 100) currentPageNumber = 100;
        if (currentPageNumber > 899) currentPageNumber = 899;
        int NumberOfSubpages = TeletextGrabber.TeletextCache.NumberOfSubpages(currentPageNumber);
        Log.Info("dvb-teletext page updated. {0:X}/{1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
        if (NumberOfSubpages > currentSubPageNumber) {
          currentSubPageNumber++;
        } else if (currentSubPageNumber >= NumberOfSubpages) {
          currentSubPageNumber = 1;
        }
        if (_updating) {
          _numberOfRequestedUpdates++;
        } else {
          _updating = true;
          GetNewPage();
          _updating = false;
        }
      }
    }

    /// <summary>
    /// Method of the update thread
    /// </summary>
    protected void UpdatePage() {
      // While not stop the thread, continue
      while (!_updateThreadStop) {
        // Is there an update request, than update
        if (_numberOfRequestedUpdates > 0 && !_updateThreadStop &&!_updating) {
          _updating = true;
          GetNewPage();
          _numberOfRequestedUpdates--;
          _updating = false;
        } else {
          // Otherwise sleep for 300ms
          Thread.Sleep(300);
        }
      }
    }

    /// <summary>
    /// Redraws the images
    /// </summary>
    protected void Redraw() {
      Log.Info("dvb-teletext redraw()");
      try {
        // First update the foreground image. Step 1 make it invisible
        _updatingForegroundImage = true;
        imgTeletextForeground.IsVisible = false;
        // Clear the old image
        System.Drawing.Image img = (Image)bmpTeletextPage.Clone();
        imgTeletextForeground.FileName = "";
        GUITextureManager.ReleaseTexture("[teletextpage]");
        // Set the new image and make the image visible again
        imgTeletextForeground.MemoryImage = img;
        imgTeletextForeground.FileName = "[teletextpage]";
        imgTeletextForeground.Centered = false;
        imgTeletextForeground.KeepAspectRatio = false;
        imgTeletextForeground.IsVisible = true;
        _updatingForegroundImage = false;
        // Update the background image now. Therefor make image invisible
        _updatingBackgroundImage = true;
        imgTeletextBackground.IsVisible = false;
        // Clear the old image
        System.Drawing.Image img2 = (Image)bmpTeletextPage.Clone();
        imgTeletextBackground.FileName = "";
        GUITextureManager.ReleaseTexture("[teletextpage2]");
        // Set the new image and make the image visible again
        imgTeletextBackground.MemoryImage = img2;
        imgTeletextBackground.FileName = "[teletextpage2]";
        imgTeletextBackground.Centered = false;
        imgTeletextBackground.KeepAspectRatio = false;
        imgTeletextBackground.IsVisible = true;
        _updatingBackgroundImage = false;
      } catch (Exception ex) {
        Log.Error(ex);
      }
    }

    /// <summary>
    /// Retrieve the new page from the teletext grabber
    /// </summary>
    protected void GetNewPage() {
      if (TeletextGrabber.TeletextCache != null) {
        bmpTeletextPage = TeletextGrabber.TeletextCache.GetPage(currentPageNumber, currentSubPageNumber);
        Redraw();
        Log.Info("dvb-teletext: select page {0} / subpage {1}", Convert.ToString(currentPageNumber), Convert.ToString(currentSubPageNumber));
      }
    }
    #endregion

    #region Serialisation
    /// <summary>
    /// Load the settings
    /// </summary>
    protected void LoadSettings() {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"))) {
        _hiddenMode = xmlreader.GetValueAsBool("mytv", "teletextHidden", false);
        _transparentMode = xmlreader.GetValueAsBool("mytv", "teletextTransparent", false);
        _rememberLastValues = xmlreader.GetValueAsBool("mytv", "teletextRemember", true);
      }
    }

    /// <summary>
    /// Store the settings, if the user wants it
    /// </summary>
    protected void SaveSettings() {
      if (_rememberLastValues) {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"))) {
          xmlreader.SetValueAsBool("mytv", "teletextHidden", _hiddenMode);
          xmlreader.SetValueAsBool("mytv", "teletextTransparent", _transparentMode);
        }
      }
    }
    #endregion

  }
}
