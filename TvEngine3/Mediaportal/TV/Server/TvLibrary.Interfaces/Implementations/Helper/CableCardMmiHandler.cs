#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper
{
  #region events

  /// <summary>
  /// A delegate for the enter menu action.
  /// </summary>
  /// <returns><c>true</c> if the menu is entered successfully, otherwise <c>false</c></returns>
  public delegate bool EnterMenuDelegate();

  /// <summary>
  /// A delegate for the close dialog action.
  /// </summary>
  /// <returns><c>true</c> if the dialog is closed successfully, otherwise <c>false</c></returns>
  public delegate bool CloseDialogDelegate(byte dialogNumber);

  #endregion

  /// <summary>
  /// This class parses HTML which is compliant with the OpenCable CCIF 2.0 I22 baseline HTML
  /// profile, performing CA menu callbacks appropriately.
  /// </summary>
  public class CableCardMmiHandler
  {
    #region enums

    private enum MmiAction : byte
    {
      Close = 0,
      Open = 1
    }

    private enum MmiDisplayType : byte
    {
      FullScreen = 0,
      Overlay = 1,
      NewWindow = 2
      // 0x03..0xff reserved
    }

    #endregion

    #region constants

    private const string ROOT_URI = "root";

    #endregion

    #region variables

    private EnterMenuDelegate _enterMenuDelegate = null;
    private CloseDialogDelegate _closeDialogDelegate = null;

    private Uri _uriBase = null;
    private string _currentUri = null;
    private IList<string> _currentMenuUris = new List<string>();
    private Stack<string> _uriHistoryStack = new Stack<string>();
    private byte _dialogNumber = 0;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="CableCardMmiHandler"/> class.
    /// </summary>
    /// <param name="enterMenuDelegate">A delegate to invoke if/when the handler determines that it should enter or return to the root of the menu.</param>
    /// <param name="closeDialogDelegate">A delegate to invoke if/when the handler determines that the menu/dialog should be closed.</param>
    public CableCardMmiHandler(EnterMenuDelegate enterMenuDelegate, CloseDialogDelegate closeDialogDelegate)
    {
      _enterMenuDelegate = enterMenuDelegate;
      _closeDialogDelegate = closeDialogDelegate;
    }

    private void Reset()
    {
      _uriBase = null;
      _currentUri = null;
      _currentMenuUris.Clear();
      _uriHistoryStack.Clear();
      _dialogNumber = 0;
    }

    /// <summary>
    /// Present the root menu.
    /// </summary>
    /// <remarks>
    /// Initiated by the user.
    /// </remarks>
    /// <param name="title">The menu title.</param>
    /// <param name="subTitle">The menu sub-title.</param>
    /// <param name="footer">The menu footer.</param>
    /// <param name="applicationList">The raw/unparsed menu data.</param>
    /// <param name="callBacks">The call back delegate.</param>
    /// <returns><c>true</c> if the menu is presented successfully, otherwise <c>false</c></returns>
    public bool EnterMenu(string title, string subTitle, string footer, byte[] applicationList, IConditionalAccessMenuCallBacks callBacks)
    {
      this.LogDebug("CableCARD MMI: enter menu, title = {0}, sub-title = {1}, footer = {2}", title, subTitle, footer);
      Reset();

      IList<SmartCardApplication> applications = new List<SmartCardApplication>();
      try
      {
        // DRI specification, page 29-30 table 6.2-27.
        byte applicationCount = applicationList[0];
        this.LogDebug("CableCARD MMI: application count = {0}", applicationCount);
        int offset = 1;
        for (byte i = 0; i < applicationCount; i++)
        {
          this.LogDebug("  application #{0}", i + 1);
          SmartCardApplication application = new SmartCardApplication();
          application.ApplicationType = (ApplicationTypeType)applicationList[offset++];
          this.LogDebug("    type    = {0}", application.ApplicationType);
          application.ApplicationVersion = (short)((applicationList[offset] << 8) + applicationList[offset + 1]);
          this.LogDebug("    version = {0}", application.ApplicationVersion);
          offset += 2;

          byte applicationNameLength = applicationList[offset++];
          application.pbstrApplicationName = System.Text.Encoding.ASCII.GetString(applicationList, offset, applicationNameLength - 1);  // - 1 for NULL termination
          offset += applicationNameLength;
          this.LogDebug("    name    = {0}", application.pbstrApplicationName);

          ushort applicationUrlLength = (ushort)((applicationList[offset] << 8) + applicationList[offset + 1]);
          string url = CompleteUri(System.Text.Encoding.ASCII.GetString(applicationList, offset, applicationUrlLength));  // URLs don't seem to be NULL terminated
          offset += applicationUrlLength;

          application.pbstrApplicationURL = url;
          this.LogDebug("    URL     = {0}", url);

          applications.Add(application);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "CableCARD MMI: failed to parse application list");
        Dump.DumpBinary(applicationList);
        return false;
      }

      if (callBacks == null)
      {
        this.LogDebug("CableCARD MMI: menu call backs are not set");
        return true;
      }

      _currentUri = ROOT_URI;
      callBacks.OnCiMenu(title, subTitle, footer, applications.Count);
      for (int i = 0; i < applications.Count; i++)
      {
        SmartCardApplication application = applications[i];
        callBacks.OnCiMenuChoice(i, application.pbstrApplicationName);
        _currentMenuUris.Add(application.pbstrApplicationURL);
      }
      this.LogDebug("CableCARD MMI: result = true");
      return true;
    }

    /// <summary>
    /// Send a menu or dialog entry selection from the user to the CableCARD,
    /// and present the corresponding menu.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <param name="callBacks">The call back delegate.</param>
    /// <returns><c>true</c> if the selection is completed successfully, otherwise <c>false</c></returns>
    public bool SelectEntry(byte choice, IConditionalAccessMenuCallBacks callBacks)
    {
      this.LogDebug("CableCARD MMI: select entry, choice = {0}", choice);

      bool toReturn = true;
      string nextUri;
      if (choice == 0)    // Back up one level.
      {
        if (_uriHistoryStack.Count == 0)
        {
          if (callBacks == null)
          {
            this.LogDebug("CableCARD MMI: menu call backs are not set");
            return true;
          }

          bool closingRootMenu = false;
          if (_currentUri == null || _currentUri.Equals(ROOT_URI))
          {
            this.LogDebug("CableCARD MMI: closing root menu");
            closingRootMenu = true;
          }

          callBacks.OnCiCloseDisplay(0);

          if (closingRootMenu)
          {
            Reset();
          }
          else
          {
            if (_closeDialogDelegate == null)
            {
              this.LogWarn("CableCARD MMI: delegate not set, unable to close dialog {0}", _dialogNumber);
            }
            else
            {
              this.LogDebug("CableCARD MMI: closing dialog {0}", _dialogNumber);
              toReturn = _closeDialogDelegate(_dialogNumber);
              Reset();
            }
          }
          return toReturn;
        }

        nextUri = _uriHistoryStack.Peek();
        if (nextUri.Equals(ROOT_URI))
        {
          if (_enterMenuDelegate == null)
          {
            this.LogWarn("CableCARD MMI: delegate not set, unable to enter menu");
            return true;
          }

          this.LogDebug("CableCARD MMI: returning to root menu");
          Reset();
          return _enterMenuDelegate();
        }
      }
      else
      {
        nextUri = _currentMenuUris[choice - 1];
        if (string.IsNullOrEmpty(nextUri))
        {
          this.LogDebug("CableCARD MMI: selected non-link entry, nothing to do");
          return true;
        }
        // Check if the user selected an integrated back link.
        if (_uriHistoryStack.Count > 0)
        {
          if (nextUri.Equals(_uriHistoryStack.Peek()))
          {
            this.LogDebug("CableCARD MMI: selected back entry");
            choice = 0;
          }
        }
      }

      toReturn = GenerateMenuFromUri(nextUri, callBacks);
      if (toReturn)
      {
        if (choice == 0)
        {
          _uriHistoryStack.Pop();
        }
        else
        {
          _uriHistoryStack.Push(_currentUri);
        }
        _currentUri = nextUri;
      }
      return toReturn;
    }

    /// <summary>
    /// Handle a dialog.
    /// </summary>
    /// <remarks>
    /// Initiated by the CableCARD.
    /// </remarks>
    /// <param name="data">The raw/unparsed dialog data.</param>
    /// <param name="callBacks">The call back delegate.</param>
    /// <returns><c>true</c> if the dialog is handled successfully, otherwise <c>false</c></returns>
    public void HandleDialog(byte[] data, IConditionalAccessMenuCallBacks callBacks)
    {
      this.LogDebug("CableCARD MMI: handle dialog");

      string uri;
      try
      {
        // DRI specification, page 32 table 6.2-30.
        _dialogNumber = data[0];
        MmiDisplayType displayType = (MmiDisplayType)data[1];
        MmiAction action = (MmiAction)data[2];
        this.LogDebug("  dialog number = {0}", _dialogNumber);
        this.LogDebug("  display type  = {0}", displayType);
        this.LogDebug("  action        = {0}", action);

        if (action == MmiAction.Close)
        {
          if (callBacks == null)
          {
            this.LogDebug("CableCARD MMI: menu call backs are not set");
            return;
          }

          callBacks.OnCiCloseDisplay(0);
          if (_closeDialogDelegate == null)
          {
            this.LogWarn("CableCARD MMI: delegate not set, unable to close dialog");
          }
          else
          {
            this.LogDebug("CableCARD MMI: closing dialog");
            _closeDialogDelegate(_dialogNumber);
            Reset();
          }
          return;
        }
        else if (action != MmiAction.Open)
        {
          this.LogWarn("CableCARD MMI: unrecognised action {0}, ignoring", action);
          return;
        }

        int uriLength = (data[3] << 8) + data[4] - 1;   // URI seems to be NULL terminated
        uri = CompleteUri(System.Text.Encoding.ASCII.GetString(data, 5, uriLength));
      }
      catch (Exception ex)
      {
        this.LogError(ex, "CableCARD MMI: failed to parse dialog message");
        Dump.DumpBinary(data);
        return;
      }

      GenerateMenuFromUri(uri, callBacks);
    }

    private bool GenerateMenuFromUri(string uri, IConditionalAccessMenuCallBacks callBacks)
    {
      this.LogDebug("CableCARD MMI: retrieving menu from URI {0}", uri);

      // Request.
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
      request.Timeout = 5000;
      HttpWebResponse response = null;
      try
      {
        response = (HttpWebResponse)request.GetResponse();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "CableCARD MMI: failed to send HTTP menu request to URI {0}", uri);
        request.Abort();
        return false;
      }

      // Response.
      string content = string.Empty;
      try
      {
        using (Stream s = response.GetResponseStream())
        {
          using (TextReader textReader = new StreamReader(s))
          {
            content = textReader.ReadToEnd();
            textReader.Close();
          }
          s.Close();
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "CableCARD MMI: failed to receive HTTP menu response from URI {0}", uri);
        return false;
      }
      finally
      {
        if (response != null)
        {
          response.Close();
        }
      }

      // Reformat from pure HTML into title and menu items. This is quite
      // hacky, but we have no way to render HTML in MediaPortal.
      this.LogDebug("CableCARD MMI: retrieved raw menu HTML\r\n{0}", content);
      IList<string> entries = new List<string>();
      IList<string> entryUris = new List<string>();
      try
      {
        content = Regex.Replace(content, "(<\\/b>|<center>)", string.Empty, RegexOptions.IgnoreCase);
        content = Regex.Replace(content, "&nbsp;", " ", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, "\\s+", " ");
        content = Regex.Replace(content, @".*<body( [^>]*)?>\s*(.*?)\s*</body>.*", "$2", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        this.LogDebug("CableCARD MMI: pre-split menu HTML\r\n{0}", content);
        string[] sections = content.Split(new string[] { "<b>", "<B>", "<br>", "<BR>" }, StringSplitOptions.RemoveEmptyEntries);

        // The split options should remove empty entries... but it won't remove whitespace entries.
        foreach (string s in sections)
        {
          string trimmed = s.Trim();
          if (!string.IsNullOrEmpty(trimmed))
          {
            entries.Add(trimmed);
          }
        }

        this.LogDebug("  title   = {0}", entries[0]);
        for (int i = 1; i < entries.Count; i++)
        {
          string entry = entries[i];
          string entryUri = string.Empty;
          Match m = Regex.Match(entry, @"<a href=""\s*([^""]+)\s*"">\s*(.*?)\s*</a>", RegexOptions.IgnoreCase);
          if (m.Success)
          {
            entryUri = CompleteUri(m.Groups[1].Captures[0].Value);
            entry = m.Groups[2].Captures[0].Value;
            this.LogDebug("  entry {0} = {1} [{2}]", i, entry, entryUri);
          }
          else
          {
            this.LogDebug("  entry {0} = {1}", i, entry);
          }
          entries.Add(entry);
          entryUris.Add(entryUri);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "CableCARD MMI: failed to handle menu HTML\r\n{0}", content);
        return false;
      }

      // Do call backs.
      if (callBacks == null)
      {
        this.LogDebug("CableCARD MMI: menu call backs are not set");
        return false;
      }

      callBacks.OnCiMenu(entries[0], string.Empty, string.Empty, entries.Count - 1);
      for (int i = 1; i < entries.Count; i++)
      {
        callBacks.OnCiMenuChoice(i - 1, entries[i]);
      }
      _currentMenuUris = entryUris;
      return true;
    }

    private string CompleteUri(string uri)
    {
      if (uri.StartsWith("http"))
      {
        if (_uriBase == null)
        {
          _uriBase = new Uri(uri);
        }
      }
      else
      {
        if (_uriBase != null)
        {
          return new Uri(_uriBase, uri).AbsoluteUri;
        }
        this.LogWarn("CableCARD MMI: URI base not available, not able to complete URI");
      }
      return uri;
    }
  }
}