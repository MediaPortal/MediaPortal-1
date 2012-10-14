using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;

namespace Mediaportal.TV.Server.TVLibrary.EventDispatchers
{
  public class CiMenuManager : EventDispatcher, ICiMenuCallbacks
  {
    #region CI Menu Event handling

    /// <summary>
    /// Flag that is that to true when a users opens CI menu interactive.
    /// It is used to filter out unwanted, unrequested CI callbacks.
    /// </summary>
    private bool _isCiMenuInteractive;

    /// <summary>
    /// Remember the number of currently attached CI menu supporting card.
    /// </summary>
    private int _activeCiMenuCard = -1;

    #endregion

    private CiMenu _curMenu;

    /// <summary>
    /// Flag that is that to true when a users opens CI menu interactive.
    /// It is used to filter out unwanted, unrequested CI callbacks.
    /// </summary>
    public bool IsCiMenuInteractive
    {
      get { return _isCiMenuInteractive; }
      set { _isCiMenuInteractive = value; }
    }

    /// <summary>
    /// Remember the number of currently attached CI menu supporting card.
    /// </summary>
    public int ActiveCiMenuCard
    {
      get { return _activeCiMenuCard; }
      set { _activeCiMenuCard = value; }
    }

    /// <summary>
    /// Checks menu state; If it's ready, fire event to "push" it to client
    /// </summary>
    /// <param name="menu"></param>
    private void CheckForCallback()
    {
      if (_curMenu != null)
      {
        if (_curMenu.State == CiMenuState.Ready || _curMenu.State == CiMenuState.NoChoices ||
            _curMenu.State == CiMenuState.Request || _curMenu.State == CiMenuState.Close)
        {
          // special workaround for AstonCrypt2 cam type (according to database CamType)
          // avoid unwanted CI menu callbacks if user has not opened CI menu interactively

          if (ActiveCiMenuCard != -1 && ServiceManager.Instance.InternalControllerService.CardCollection[ActiveCiMenuCard].DataBaseCard.CamType == 1 && !IsCiMenuInteractive)
          {
            Log.Debug("AstonCrypt2: unrequested CI menu received, no action done. Menu Title: {0}", _curMenu.Title);
            return;
          }

          IDictionary<string, DateTime> usersCopy = GetUsersCopy();                    
          if (usersCopy.Count > 0)
          {
            foreach (var user in usersCopy.Keys)
            {
              EventService.CallbackCiMenuEvent(user, _curMenu);
            }
            return;
          }
          
          Log.Debug("CI menu received but no listeners available");
        }
      }
    }

    #region Implementation of ICiMenuCallbacks

    /// <summary>
    /// Callback on opening menu
    /// </summary>
    /// <param name="lpszTitle">Title</param>
    /// <param name="lpszSubTitle">Subtitle</param>
    /// <param name="lpszBottom">Bottom text</param>
    /// <param name="nNumChoices">number of choices</param>
    /// <returns></returns>
    public int OnCiMenu(string lpszTitle, string lpszSubTitle, string lpszBottom, int nNumChoices)
    {
      var curMenu = new CiMenu(lpszTitle, lpszSubTitle, lpszBottom, CiMenuState.Opened) { NumChoices = nNumChoices };
      if (nNumChoices == 0)
      {
        curMenu.State = CiMenuState.NoChoices;
      }

      CheckForCallback();
      return 0;
    }

    /// <summary>
    /// Callback for each menu entry
    /// </summary>
    /// <param name="nChoice">choice number</param>
    /// <param name="lpszText">choice text</param>
    /// <returns></returns>
    public int OnCiMenuChoice(int nChoice, string lpszText)
    {
      if (_curMenu == null)
      {
        Log.Debug("Error in OnCiMenuChoice: menu choice sent before menu started");
        return 0;
      }
      _curMenu.AddEntry(nChoice + 1, lpszText); // choices for display +1 
      if (nChoice + 1 == _curMenu.NumChoices)
      {
        _curMenu.State = CiMenuState.Ready;
        CheckForCallback();
      }
      return 0;
    }

    /// <summary>
    /// Callback on closing display
    /// </summary>
    /// <param name="nDelay">delay in seconds</param>
    /// <returns></returns>
    public int OnCiCloseDisplay(int nDelay)
    {
      // sometimes first a "Close" is sent, even no others callbacks were done before 
      if (_curMenu == null)
      {
        _curMenu = new CiMenu(String.Empty, String.Empty, String.Empty, CiMenuState.Close);
      }
      _curMenu.State = CiMenuState.Close;
      CheckForCallback();
      return 0;
    }

    /// <summary>
    /// Callback on requesting user input (PIN,...)
    /// </summary>
    /// <param name="bBlind">true if password</param>
    /// <param name="nAnswerLength">expected (max) answer length</param>
    /// <param name="lpszText">request text</param>
    /// <returns></returns>
    public int OnCiRequest(bool bBlind, uint nAnswerLength, string lpszText)
    {
      if (_curMenu == null)
      {
        _curMenu = new CiMenu(String.Empty, String.Empty, String.Empty, CiMenuState.Request);
      }
      _curMenu.State = CiMenuState.Request;
      _curMenu.Request(lpszText, (int)nAnswerLength, bBlind);
      CheckForCallback();
      return 0;
    }

    #endregion

    #region Overrides of EventDispatcher

    public override void Start()
    {
    }

    public override void Stop()
    {
    }

    #endregion
  }
}
